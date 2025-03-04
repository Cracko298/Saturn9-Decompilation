using System;
using BEPUphysics;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.Constraints;
using BEPUphysics.DataStructures;
using BEPUphysics.Entities;
using BEPUphysics.MathExtensions;
using Microsoft.Xna.Framework;

namespace BEPUphysicsDemos.AlternateMovement.Character;

public class HorizontalMotionConstraint : EntitySolverUpdateable
{
	private CharacterController character;

	private SupportData supportData;

	private Vector2 movementDirection;

	private float speed = 8f;

	private float crouchingSpeed = 3f;

	private float slidingSpeed = 6f;

	private float airSpeed = 8f;

	private float maximumForce = 1000f;

	private float maximumSlidingForce = 50f;

	private float maximumAirForce = 250f;

	private float supportForceFactor = 1f;

	private float maxSpeed;

	private float maxForce;

	private Matrix2X2 massMatrix;

	private Entity supportEntity;

	private Vector3 linearJacobianA1;

	private Vector3 linearJacobianA2;

	private Vector3 linearJacobianB1;

	private Vector3 linearJacobianB2;

	private Vector3 angularJacobianB1;

	private Vector3 angularJacobianB2;

	private Vector2 accumulatedImpulse;

	private Vector2 targetVelocity;

	private Vector2 positionCorrectionBias;

	private Vector3 positionLocalOffset;

	private bool wasTryingToMove;

	private bool hadTraction;

	private Entity previousSupportEntity;

	private float timeSinceTransition;

	public SupportData SupportData
	{
		get
		{
			return supportData;
		}
		set
		{
			Collidable supportObject = supportData.SupportObject;
			supportData = value;
			if (supportObject != supportData.SupportObject)
			{
				OnInvolvedEntitiesChanged();
				if (supportData.SupportObject is EntityCollidable entityCollidable)
				{
					supportEntity = entityCollidable.Entity;
				}
				else
				{
					supportEntity = null;
				}
			}
		}
	}

	public Vector2 MovementDirection
	{
		get
		{
			return movementDirection;
		}
		set
		{
			float num = value.LengthSquared();
			if (num > 1E-07f)
			{
				character.Body.ActivityInformation.Activate();
				Vector2.Divide(ref value, (float)Math.Sqrt(num), out movementDirection);
			}
			else
			{
				character.Body.ActivityInformation.Activate();
				movementDirection = default(Vector2);
			}
		}
	}

	public float Speed
	{
		get
		{
			return speed;
		}
		set
		{
			if (value < 0f)
			{
				throw new Exception("Value must be nonnegative.");
			}
			speed = value;
		}
	}

	public float CrouchingSpeed
	{
		get
		{
			return crouchingSpeed;
		}
		set
		{
			if (value < 0f)
			{
				throw new Exception("Value must be nonnegative.");
			}
			crouchingSpeed = value;
		}
	}

	public float SlidingSpeed
	{
		get
		{
			return slidingSpeed;
		}
		set
		{
			if (value < 0f)
			{
				throw new Exception("Value must be nonnegative.");
			}
			slidingSpeed = value;
		}
	}

	public float AirSpeed
	{
		get
		{
			return airSpeed;
		}
		set
		{
			if (value < 0f)
			{
				throw new Exception("Value must be nonnegative.");
			}
			airSpeed = value;
		}
	}

	public float MaximumForce
	{
		get
		{
			return maximumForce;
		}
		set
		{
			if (value < 0f)
			{
				throw new Exception("Value must be nonnegative.");
			}
			maximumForce = value;
		}
	}

	public float MaximumSlidingForce
	{
		get
		{
			return maximumSlidingForce;
		}
		set
		{
			if (value < 0f)
			{
				throw new Exception("Value must be nonnegative.");
			}
			maximumSlidingForce = value;
		}
	}

	public float MaximumAirForce
	{
		get
		{
			return maximumAirForce;
		}
		set
		{
			if (value < 0f)
			{
				throw new Exception("Value must be nonnegative.");
			}
			maximumAirForce = value;
		}
	}

	public float SupportForceFactor
	{
		get
		{
			return supportForceFactor;
		}
		set
		{
			if (value < 0f)
			{
				throw new Exception("Value must be nonnegative.");
			}
			supportForceFactor = value;
		}
	}

	public MovementMode MovementMode { get; private set; }

	public Vector2 RelativeVelocity
	{
		get
		{
			Vector2 result = default(Vector2);
			Vector3 vector = character.Body.LinearVelocity;
			Vector3.Dot(ref linearJacobianA1, ref vector, out result.X);
			Vector3.Dot(ref linearJacobianA2, ref vector, out result.Y);
			if (supportEntity != null)
			{
				Vector3 vector2 = supportEntity.LinearVelocity;
				Vector3 vector3 = supportEntity.AngularVelocity;
				Vector3.Dot(ref linearJacobianB1, ref vector2, out var result2);
				Vector3.Dot(ref linearJacobianB2, ref vector2, out var result3);
				result.X += result2;
				result.Y += result3;
				Vector3.Dot(ref angularJacobianB1, ref vector3, out result2);
				Vector3.Dot(ref angularJacobianB2, ref vector3, out result3);
				result.X += result2;
				result.Y += result3;
			}
			return result;
		}
	}

	public Vector3 RelativeWorldVelocity
	{
		get
		{
			Vector3 linearVelocity = character.Body.LinearVelocity;
			if (supportEntity != null)
			{
				return linearVelocity - Toolbox.GetVelocityOfPoint(supportData.Position, supportEntity);
			}
			return linearVelocity;
		}
	}

	public HorizontalMotionConstraint(CharacterController characterController)
	{
		character = characterController;
		CollectInvolvedEntities();
	}

	protected override void CollectInvolvedEntities(RawList<Entity> outputInvolvedEntities)
	{
		if (supportData.SupportObject is EntityCollidable entityCollidable)
		{
			outputInvolvedEntities.Add(entityCollidable.Entity);
		}
		outputInvolvedEntities.Add(character.Body);
	}

	public override void Update(float dt)
	{
		bool flag = movementDirection.LengthSquared() > 0f;
		if (supportData.SupportObject != null)
		{
			if (supportData.HasTraction)
			{
				MovementMode = MovementMode.Traction;
				if (character.StanceManager.CurrentStance == Stance.Standing)
				{
					maxSpeed = speed;
				}
				else
				{
					maxSpeed = crouchingSpeed;
				}
				maxForce = maximumForce;
			}
			else
			{
				MovementMode = MovementMode.Sliding;
				maxSpeed = slidingSpeed;
				maxForce = maximumSlidingForce;
			}
		}
		else
		{
			MovementMode = MovementMode.Floating;
			maxSpeed = airSpeed;
			maxForce = maximumAirForce;
			supportEntity = null;
		}
		if (!flag)
		{
			maxSpeed = 0f;
		}
		maxForce *= dt;
		Vector3 value = character.Body.OrientationMatrix.Down;
		if (MovementMode != MovementMode.Floating)
		{
			if (flag)
			{
				Vector3 value2 = new Vector3(movementDirection.X, 0f, movementDirection.Y);
				Vector3.Add(ref value2, ref value, out var result);
				Plane p = new Plane(supportData.Normal, 0f);
				Toolbox.GetLinePlaneIntersection(ref value2, ref result, ref p, out var _, out var q);
				q.Normalize();
				Vector3.Cross(ref q, ref supportData.Normal, out var result2);
				linearJacobianA1 = q;
				linearJacobianA2 = result2;
				linearJacobianB1 = -q;
				linearJacobianB2 = -result2;
			}
			else
			{
				Vector3.Dot(ref linearJacobianA1, ref supportData.Normal, out var result3);
				Vector3.Multiply(ref supportData.Normal, result3, out var result4);
				Vector3.Subtract(ref linearJacobianA1, ref result4, out linearJacobianA1);
				float num = linearJacobianA1.LengthSquared();
				if (num < 1E-07f)
				{
					Vector3.Cross(ref Toolbox.RightVector, ref supportData.Normal, out linearJacobianA1);
					num = linearJacobianA1.LengthSquared();
					if (num < 1E-07f)
					{
						Vector3.Cross(ref Toolbox.ForwardVector, ref supportData.Normal, out linearJacobianA1);
						num = linearJacobianA1.LengthSquared();
					}
				}
				Vector3.Divide(ref linearJacobianA1, (float)Math.Sqrt(num), out linearJacobianA1);
				Vector3.Cross(ref linearJacobianA1, ref supportData.Normal, out linearJacobianA2);
				linearJacobianB1 = -linearJacobianA1;
				linearJacobianB2 = -linearJacobianA2;
			}
			if (supportEntity != null)
			{
				Vector3 vector = supportData.Position - supportEntity.Position;
				Vector3.Cross(ref linearJacobianA1, ref vector, out angularJacobianB1);
				Vector3.Cross(ref linearJacobianA2, ref vector, out angularJacobianB2);
			}
			else
			{
				angularJacobianB1 = default(Vector3);
				angularJacobianB2 = default(Vector3);
			}
		}
		else
		{
			linearJacobianA1 = new Vector3(movementDirection.X, 0f, movementDirection.Y);
			linearJacobianA2 = new Vector3(movementDirection.Y, 0f, 0f - movementDirection.X);
		}
		targetVelocity.X = maxSpeed;
		targetVelocity.Y = 0f;
		if (supportEntity != null && supportEntity.IsDynamic)
		{
			float num2 = 0f;
			float inverseMass = character.Body.InverseMass;
			float num3 = inverseMass;
			float num4 = inverseMass;
			Matrix3X3 matrix = supportEntity.InertiaTensorInverse;
			Matrix3X3.Multiply(ref matrix, supportForceFactor, out matrix);
			inverseMass = supportForceFactor * supportEntity.InverseMass;
			Matrix3X3.Transform(ref angularJacobianB1, ref matrix, out var result5);
			Vector3.Dot(ref result5, ref angularJacobianB1, out var result6);
			num3 += inverseMass + result6;
			Vector3.Dot(ref result5, ref angularJacobianB2, out result6);
			num2 += result6;
			Matrix3X3.Transform(ref angularJacobianB2, ref matrix, out result5);
			Vector3.Dot(ref result5, ref angularJacobianB2, out result6);
			num4 += inverseMass + result6;
			massMatrix.M11 = num3;
			massMatrix.M12 = num2;
			massMatrix.M21 = num2;
			massMatrix.M22 = num4;
			Matrix2X2.Invert(ref massMatrix, out massMatrix);
		}
		else
		{
			Matrix2X2.CreateScale(character.Body.Mass, out massMatrix);
		}
		if (supportEntity != null && ((wasTryingToMove && !flag) || (!hadTraction && supportData.HasTraction) || supportEntity != previousSupportEntity))
		{
			timeSinceTransition = 0f;
		}
		if (!flag && supportData.HasTraction && supportEntity != null)
		{
			float num5 = speed / (maximumForce * character.Body.InverseMass);
			if (timeSinceTransition >= 0f && timeSinceTransition < num5)
			{
				timeSinceTransition += dt;
			}
			if (timeSinceTransition >= num5)
			{
				Vector3.Multiply(ref value, character.Body.Height * 0.5f, out positionLocalOffset);
				positionLocalOffset = positionLocalOffset + character.Body.Position - supportEntity.Position;
				positionLocalOffset = Matrix3X3.TransformTranspose(positionLocalOffset, supportEntity.OrientationMatrix);
				timeSinceTransition = -1f;
			}
			if (timeSinceTransition < 0f)
			{
				Vector3.Multiply(ref value, character.Body.Height * 0.5f, out var result7);
				result7 += character.Body.Position;
				Vector3 value3 = Matrix3X3.Transform(positionLocalOffset, supportEntity.OrientationMatrix) + supportEntity.Position;
				Vector3.Subtract(ref result7, ref value3, out var result8);
				if (result8.LengthSquared() > 0.0225f)
				{
					Vector3.Multiply(ref value, character.Body.Height * 0.5f, out positionLocalOffset);
					positionLocalOffset = positionLocalOffset + character.Body.Position - supportEntity.Position;
					positionLocalOffset = Matrix3X3.TransformTranspose(positionLocalOffset, supportEntity.OrientationMatrix);
					positionCorrectionBias = default(Vector2);
				}
				else
				{
					Vector3.Dot(ref result8, ref linearJacobianA1, out positionCorrectionBias.X);
					Vector3.Dot(ref result8, ref linearJacobianA2, out positionCorrectionBias.Y);
					Vector2.Multiply(ref positionCorrectionBias, 0.2f / dt, out positionCorrectionBias);
				}
			}
		}
		else
		{
			timeSinceTransition = 0f;
			positionCorrectionBias = default(Vector2);
		}
		wasTryingToMove = flag;
		hadTraction = supportData.HasTraction;
		previousSupportEntity = supportEntity;
	}

	public override void ExclusiveUpdate()
	{
		Vector3 impulse = default(Vector3);
		Vector3 impulse2 = default(Vector3);
		float x = accumulatedImpulse.X;
		float y = accumulatedImpulse.Y;
		impulse.X = linearJacobianA1.X * x + linearJacobianA2.X * y;
		impulse.Y = linearJacobianA1.Y * x + linearJacobianA2.Y * y;
		impulse.Z = linearJacobianA1.Z * x + linearJacobianA2.Z * y;
		character.Body.ApplyLinearImpulse(ref impulse);
		if (supportEntity != null && supportEntity.IsDynamic)
		{
			Vector3.Multiply(ref impulse, 0f - supportForceFactor, out impulse);
			x *= supportForceFactor;
			y *= supportForceFactor;
			impulse2.X = x * angularJacobianB1.X + y * angularJacobianB2.X;
			impulse2.Y = x * angularJacobianB1.Y + y * angularJacobianB2.Y;
			impulse2.Z = x * angularJacobianB1.Z + y * angularJacobianB2.Z;
			supportEntity.ApplyLinearImpulse(ref impulse);
			supportEntity.ApplyAngularImpulse(ref impulse2);
		}
	}

	public override float SolveIteration()
	{
		Vector2 value = RelativeVelocity;
		Vector2.Add(ref value, ref positionCorrectionBias, out value);
		Vector2.Subtract(ref targetVelocity, ref value, out var result);
		Matrix2X2.Transform(ref result, ref massMatrix, out result);
		Vector2 value2 = accumulatedImpulse;
		if (MovementMode == MovementMode.Floating)
		{
			accumulatedImpulse.X = MathHelper.Clamp(accumulatedImpulse.X + result.X, 0f, maxForce);
			accumulatedImpulse.Y = 0f;
		}
		else
		{
			Vector2.Add(ref result, ref accumulatedImpulse, out accumulatedImpulse);
			float num = accumulatedImpulse.LengthSquared();
			if (num > maxForce * maxForce)
			{
				Vector2.Multiply(ref accumulatedImpulse, maxForce / (float)Math.Sqrt(num), out accumulatedImpulse);
			}
		}
		Vector2.Subtract(ref accumulatedImpulse, ref value2, out result);
		Vector3 impulse = default(Vector3);
		Vector3 impulse2 = default(Vector3);
		float x = result.X;
		float y = result.Y;
		impulse.X = linearJacobianA1.X * x + linearJacobianA2.X * y;
		impulse.Y = linearJacobianA1.Y * x + linearJacobianA2.Y * y;
		impulse.Z = linearJacobianA1.Z * x + linearJacobianA2.Z * y;
		character.Body.ApplyLinearImpulse(ref impulse);
		if (supportEntity != null && supportEntity.IsDynamic)
		{
			Vector3.Multiply(ref impulse, 0f - supportForceFactor, out impulse);
			x *= supportForceFactor;
			y *= supportForceFactor;
			impulse2.X = x * angularJacobianB1.X + y * angularJacobianB2.X;
			impulse2.Y = x * angularJacobianB1.Y + y * angularJacobianB2.Y;
			impulse2.Z = x * angularJacobianB1.Z + y * angularJacobianB2.Z;
			supportEntity.ApplyLinearImpulse(ref impulse);
			supportEntity.ApplyAngularImpulse(ref impulse2);
		}
		return Math.Abs(result.X) + Math.Abs(result.Y);
	}
}
