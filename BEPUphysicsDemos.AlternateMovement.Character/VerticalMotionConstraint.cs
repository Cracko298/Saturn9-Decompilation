using System;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.Constraints;
using BEPUphysics.DataStructures;
using BEPUphysics.Entities;
using BEPUphysics.MathExtensions;
using BEPUphysics.Settings;
using Microsoft.Xna.Framework;

namespace BEPUphysicsDemos.AlternateMovement.Character;

public class VerticalMotionConstraint : EntitySolverUpdateable
{
	private CharacterController character;

	private SupportData supportData;

	private float maximumGlueForce = 5000f;

	private float maximumForce;

	private float supportForceFactor = 1f;

	private float effectiveMass;

	private Entity supportEntity;

	private Vector3 linearJacobianA;

	private Vector3 linearJacobianB;

	private Vector3 angularJacobianB;

	private float accumulatedImpulse;

	private float permittedVelocity;

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
			}
		}
	}

	public float MaximumGlueForce
	{
		get
		{
			return maximumGlueForce;
		}
		set
		{
			if (maximumGlueForce < 0f)
			{
				throw new Exception("Value must be nonnegative.");
			}
			maximumGlueForce = value;
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

	public float EffectiveMass => effectiveMass;

	public float RelativeVelocity
	{
		get
		{
			Vector3 vector = character.Body.LinearVelocity;
			Vector3.Dot(ref linearJacobianA, ref vector, out var result);
			if (supportEntity != null)
			{
				Vector3 vector2 = supportEntity.LinearVelocity;
				Vector3 vector3 = supportEntity.AngularVelocity;
				Vector3.Dot(ref linearJacobianB, ref vector2, out var result2);
				result += result2;
				Vector3.Dot(ref angularJacobianB, ref vector3, out result2);
				return result + result2;
			}
			return result;
		}
	}

	public VerticalMotionConstraint(CharacterController characterController)
	{
		character = characterController;
	}

	protected override void CollectInvolvedEntities(RawList<Entity> outputInvolvedEntities)
	{
		if (supportData.SupportObject is EntityCollidable entityCollidable)
		{
			outputInvolvedEntities.Add(entityCollidable.Entity);
		}
		outputInvolvedEntities.Add(character.Body);
	}

	public override void UpdateSolverActivity()
	{
		if (supportData.HasTraction)
		{
			base.UpdateSolverActivity();
		}
		else
		{
			isActiveInSolver = false;
		}
	}

	public override void Update(float dt)
	{
		if (supportData.SupportObject != null)
		{
			if (supportData.SupportObject is EntityCollidable entityCollidable)
			{
				supportEntity = entityCollidable.Entity;
			}
			else
			{
				supportEntity = null;
			}
		}
		else
		{
			supportEntity = null;
		}
		maximumForce = maximumGlueForce * dt;
		if (supportData.Depth > 0f)
		{
			permittedVelocity = CollisionResponseSettings.MaximumPenetrationCorrectionSpeed;
		}
		else
		{
			permittedVelocity = 0f;
		}
		linearJacobianA = supportData.Normal;
		Vector3.Negate(ref linearJacobianA, out linearJacobianB);
		effectiveMass = character.Body.InverseMass;
		if (supportEntity != null)
		{
			Vector3 vector = supportData.Position - supportEntity.Position;
			Vector3.Cross(ref vector, ref linearJacobianB, out angularJacobianB);
			if (supportEntity.IsDynamic)
			{
				Matrix3X3 matrix = supportEntity.InertiaTensorInverse;
				Matrix3X3.Transform(ref angularJacobianB, ref matrix, out var result);
				Vector3.Dot(ref result, ref angularJacobianB, out var result2);
				effectiveMass += supportForceFactor * (result2 + supportEntity.InverseMass);
			}
		}
		effectiveMass = 1f / effectiveMass;
	}

	public override void ExclusiveUpdate()
	{
		Vector3 result = default(Vector3);
		Vector3 result2 = default(Vector3);
		Vector3.Multiply(ref linearJacobianA, accumulatedImpulse, out result);
		character.Body.ApplyLinearImpulse(ref result);
		if (supportEntity != null && supportEntity.IsDynamic)
		{
			Vector3.Multiply(ref result, 0f - supportForceFactor, out result);
			Vector3.Multiply(ref angularJacobianB, accumulatedImpulse * supportForceFactor, out result2);
			supportEntity.ApplyLinearImpulse(ref result);
			supportEntity.ApplyAngularImpulse(ref result2);
		}
	}

	public override float SolveIteration()
	{
		float num = RelativeVelocity + permittedVelocity;
		float num2 = (0f - num) * effectiveMass;
		float num3 = accumulatedImpulse;
		accumulatedImpulse = MathHelper.Clamp(accumulatedImpulse + num2, 0f, maximumForce);
		num2 = accumulatedImpulse - num3;
		Vector3 result = default(Vector3);
		Vector3 result2 = default(Vector3);
		Vector3.Multiply(ref linearJacobianA, num2, out result);
		character.Body.ApplyLinearImpulse(ref result);
		if (supportEntity != null && supportEntity.IsDynamic)
		{
			Vector3.Multiply(ref result, 0f - supportForceFactor, out result);
			Vector3.Multiply(ref angularJacobianB, num2 * supportForceFactor, out result2);
			supportEntity.ApplyLinearImpulse(ref result);
			supportEntity.ApplyAngularImpulse(ref result2);
		}
		return Math.Abs(num2);
	}
}
