using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using BEPUphysics;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.CollisionTests;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.Materials;
using BEPUphysics.MathExtensions;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.PositionUpdating;
using BEPUphysics.UpdateableSystems;
using Microsoft.Xna.Framework;
using Saturn9;

namespace BEPUphysicsDemos.AlternateMovement.Character;

public class CharacterController : Updateable, IBeforeSolverUpdateable, ISpaceUpdateable, ISpaceObject
{
	private class Comparer : IComparer<ICharacterTag>
	{
		public int Compare(ICharacterTag x, ICharacterTag y)
		{
			if (x.InstanceId < y.InstanceId)
			{
				return -1;
			}
			if (x.InstanceId > y.InstanceId)
			{
				return 1;
			}
			return 0;
		}
	}

	public int m_PlayerIdx;

	private float jumpSpeed = 4.5f;

	private float leapUpSpeed = 5f;

	private float leapFwdSpeed = 5f;

	private float slidingJumpSpeed = 3f;

	private float jumpForceFactor = 1f;

	private List<ICharacterTag> involvedCharacters = new List<ICharacterTag>();

	private static Comparer comparer = new Comparer();

	private SupportData supportData;

	private bool tryToJump;

	private bool tryToLeap;

	public Cylinder Body { get; private set; }

	public StepManager StepManager { get; private set; }

	public StanceManager StanceManager { get; private set; }

	public QueryManager QueryManager { get; private set; }

	public HorizontalMotionConstraint HorizontalMotionConstraint { get; private set; }

	public VerticalMotionConstraint VerticalMotionConstraint { get; private set; }

	public float JumpSpeed
	{
		get
		{
			return jumpSpeed;
		}
		set
		{
			if (value < 0f)
			{
				throw new Exception("Value must be nonnegative.");
			}
			jumpSpeed = value;
		}
	}

	public float SlidingJumpSpeed
	{
		get
		{
			return slidingJumpSpeed;
		}
		set
		{
			if (value < 0f)
			{
				throw new Exception("Value must be nonnegative.");
			}
			slidingJumpSpeed = value;
		}
	}

	public float JumpForceFactor
	{
		get
		{
			return jumpForceFactor;
		}
		set
		{
			if (value < 0f)
			{
				throw new Exception("Value must be nonnegative.");
			}
			jumpForceFactor = value;
		}
	}

	public float BodyRadius
	{
		get
		{
			return Body.CollisionInformation.Shape.Radius;
		}
		set
		{
			if (value <= 0f)
			{
				throw new Exception("Radius must be positive.");
			}
			Body.CollisionInformation.Shape.Radius = value;
			QueryManager.UpdateQueryShapes();
		}
	}

	public SupportFinder SupportFinder { get; private set; }

	public CharacterController()
		: this(default(Vector3), 1.7f, 1.19f, 0.6f, 10f)
	{
	}

	public CharacterController(Vector3 position, float height, float crouchingHeight, float radius, float mass)
	{
		Body = new Cylinder(position, height, radius, mass);
		Body.IgnoreShapeChanges = true;
		Body.CollisionInformation.Shape.CollisionMargin = 0.1f;
		Body.PositionUpdateMode = PositionUpdateMode.Continuous;
		Body.LocalInertiaTensorInverse = default(Matrix3X3);
		Body.CollisionInformation.Events.DetectingInitialCollision += RemoveFriction;
		Body.LinearDamping = 0f;
		SupportFinder = new SupportFinder(this);
		HorizontalMotionConstraint = new HorizontalMotionConstraint(this);
		VerticalMotionConstraint = new VerticalMotionConstraint(this);
		StepManager = new StepManager(this);
		StanceManager = new StanceManager(this, crouchingHeight);
		QueryManager = new QueryManager(this);
		base.IsUpdatedSequentially = false;
		Body.CollisionInformation.Tag = new CharacterSynchronizer(Body);
	}

	public void LockCharacterPairs()
	{
		foreach (CollidablePairHandler pair in Body.CollisionInformation.Pairs)
		{
			BroadPhaseEntry broadPhaseEntry = ((pair.BroadPhaseOverlap.EntryA == Body.CollisionInformation) ? pair.BroadPhaseOverlap.EntryB : pair.BroadPhaseOverlap.EntryA);
			if (broadPhaseEntry.Tag is ICharacterTag item)
			{
				involvedCharacters.Add(item);
			}
		}
		if (involvedCharacters.Count > 0)
		{
			involvedCharacters.Add((ICharacterTag)Body.CollisionInformation.Tag);
			involvedCharacters.Sort(comparer);
			for (int i = 0; i < involvedCharacters.Count; i++)
			{
				Monitor.Enter(involvedCharacters[i]);
			}
		}
	}

	public void UnlockCharacterPairs()
	{
		for (int num = involvedCharacters.Count - 1; num >= 0; num--)
		{
			Monitor.Exit(involvedCharacters[num]);
		}
		involvedCharacters.Clear();
	}

	private void RemoveFriction(EntityCollidable sender, BroadPhaseEntry other, NarrowPhasePair pair)
	{
		if (pair is CollidablePairHandler collidablePairHandler)
		{
			collidablePairHandler.UpdateMaterialProperties(default(InteractionProperties));
		}
	}

	private void ExpandBoundingBox()
	{
		if (Body.ActivityInformation.IsActive)
		{
			float num = Body.CollisionInformation.Shape.CollisionMargin * 1.1f;
			Vector3 value = default(Vector3);
			value.X = num;
			value.Y = StepManager.MaximumStepHeight;
			value.Z = num;
			BoundingBox boundingBox = Body.CollisionInformation.BoundingBox;
			Vector3.Add(ref boundingBox.Max, ref value, out boundingBox.Max);
			Vector3.Subtract(ref boundingBox.Min, ref value, out boundingBox.Min);
			Body.CollisionInformation.BoundingBox = boundingBox;
		}
	}

	private void CollectSupportData()
	{
		SupportFinder.UpdateSupports();
		if (SupportFinder.HasSupport)
		{
			if (SupportFinder.HasTraction)
			{
				supportData = SupportFinder.TractionData.Value;
			}
			else
			{
				supportData = SupportFinder.SupportData.Value;
			}
		}
		else
		{
			supportData = default(SupportData);
		}
	}

	void IBeforeSolverUpdateable.Update(float dt)
	{
		LockCharacterPairs();
		try
		{
			CorrectContacts();
			bool hasTraction = SupportFinder.HasTraction;
			CollectSupportData();
			ComputeRelativeVelocity(ref this.supportData, out var relativeVelocity);
			float num = Vector3.Dot(this.supportData.Normal, relativeVelocity);
			if (SupportFinder.HasTraction && !hasTraction && num < 0f)
			{
				SupportFinder.ClearSupportData();
				this.supportData = default(SupportData);
			}
			if (tryToJump && StanceManager.CurrentStance != Stance.Crouching)
			{
				if (SupportFinder.HasTraction)
				{
					float num2 = Vector3.Dot(Body.OrientationMatrix.Up, relativeVelocity);
					float num3 = Math.Max(jumpSpeed - num2, 0f);
					ApplyJumpVelocity(ref this.supportData, Body.OrientationMatrix.Up * num3, ref relativeVelocity);
					foreach (CollidablePairHandler pair in Body.CollisionInformation.Pairs)
					{
						pair.ClearContacts();
					}
					SupportFinder.ClearSupportData();
					this.supportData = default(SupportData);
					if (m_PlayerIdx != -1 && g.m_PlayerManager.m_Player[m_PlayerIdx].m_Id != -1)
					{
						g.m_PlayerManager.m_Player[m_PlayerIdx].PlayJumpSFX();
					}
				}
				else if (SupportFinder.HasSupport)
				{
					float num4 = Vector3.Dot(this.supportData.Normal, relativeVelocity);
					float num5 = Math.Max(slidingJumpSpeed - num4, 0f);
					ApplyJumpVelocity(ref this.supportData, this.supportData.Normal * (0f - num5), ref relativeVelocity);
					foreach (CollidablePairHandler pair2 in Body.CollisionInformation.Pairs)
					{
						pair2.ClearContacts();
					}
					SupportFinder.ClearSupportData();
					this.supportData = default(SupportData);
					if (m_PlayerIdx != -1 && g.m_PlayerManager.m_Player[m_PlayerIdx].m_Id != -1)
					{
						g.m_PlayerManager.m_Player[m_PlayerIdx].PlayJumpSFX();
					}
				}
			}
			tryToJump = false;
			if (tryToLeap)
			{
				if (SupportFinder.HasTraction)
				{
					Body.LinearVelocity = Body.OrientationMatrix.Up * leapUpSpeed + Body.OrientationMatrix.Forward * leapFwdSpeed;
					relativeVelocity = Body.LinearVelocity;
					foreach (CollidablePairHandler pair3 in Body.CollisionInformation.Pairs)
					{
						pair3.ClearContacts();
					}
					SupportFinder.ClearSupportData();
					this.supportData = default(SupportData);
					if (m_PlayerIdx != -1 && g.m_PlayerManager.m_Player[m_PlayerIdx].m_Id != -1)
					{
						g.m_PlayerManager.m_Player[m_PlayerIdx].PlayLeapSFX();
					}
				}
				else if (SupportFinder.HasSupport)
				{
					Body.LinearVelocity = Body.OrientationMatrix.Up * leapUpSpeed + Body.OrientationMatrix.Forward * leapFwdSpeed;
					relativeVelocity = Body.LinearVelocity;
					foreach (CollidablePairHandler pair4 in Body.CollisionInformation.Pairs)
					{
						pair4.ClearContacts();
					}
					SupportFinder.ClearSupportData();
					this.supportData = default(SupportData);
					if (m_PlayerIdx != -1 && g.m_PlayerManager.m_Player[m_PlayerIdx].m_Id != -1)
					{
						g.m_PlayerManager.m_Player[m_PlayerIdx].PlayLeapSFX();
					}
				}
			}
			tryToLeap = false;
			bool flag = false;
			Stopwatch.GetTimestamp();
			if (StepManager.TryToStepUp(out var newPosition))
			{
				TeleportToPosition(newPosition, dt);
			}
			if (StanceManager.UpdateStance(out var newPosition2))
			{
				TeleportToPosition(newPosition2, dt);
			}
		}
		finally
		{
			UnlockCharacterPairs();
		}
		Vector3 movementDirection = new Vector3(HorizontalMotionConstraint.MovementDirection.X, 0f, HorizontalMotionConstraint.MovementDirection.Y);
		SupportFinder.GetTractionInDirection(ref movementDirection, out var supportData);
		bool flag2 = HorizontalMotionConstraint.SupportData.SupportObject != this.supportData.SupportObject || VerticalMotionConstraint.SupportData.SupportObject != supportData.SupportObject;
		if (flag2)
		{
			CharacterSynchronizer.ConstraintAccessLocker.Enter();
		}
		HorizontalMotionConstraint.SupportData = this.supportData;
		VerticalMotionConstraint.SupportData = supportData;
		if (flag2)
		{
			CharacterSynchronizer.ConstraintAccessLocker.Exit();
		}
	}

	private void TeleportToPosition(Vector3 newPosition, float dt)
	{
		Body.Position = newPosition;
		Quaternion orientation = Body.Orientation;
		Body.CollisionInformation.UpdateWorldTransform(ref newPosition, ref orientation);
		foreach (CollidablePairHandler pair in Body.CollisionInformation.Pairs)
		{
			pair.ClearContacts();
			pair.UpdateCollision(dt);
		}
		CollectSupportData();
	}

	private void CorrectContacts()
	{
		Vector3 vector = Body.OrientationMatrix.Down;
		Vector3 value = Body.Position;
		float collisionMargin = Body.CollisionInformation.Shape.CollisionMargin;
		float num = Body.Height * 0.5f - collisionMargin;
		float num2 = Body.Radius - collisionMargin;
		float num3 = num2 * num2;
		foreach (CollidablePairHandler pair in Body.CollisionInformation.Pairs)
		{
			foreach (ContactInformation contact2 in pair.Contacts)
			{
				Contact contact = contact2.Contact;
				Vector3 vector2 = contact.Position - Body.Position;
				Vector3.Dot(ref vector2, ref vector, out var result);
				if (!(result > num))
				{
					continue;
				}
				Vector3.Dot(ref vector2, ref vector, out result);
				Vector3.Multiply(ref vector, result, out var result2);
				Vector3.Subtract(ref vector2, ref result2, out result2);
				float num4 = result2.LengthSquared();
				if (num4 > num3)
				{
					Vector3.Multiply(ref result2, num2 / (float)Math.Sqrt(num4), out result2);
				}
				Vector3.Multiply(ref vector, num, out var result3);
				Vector3.Add(ref result3, ref result2, out result3);
				Vector3.Add(ref result3, ref value, out result3);
				Vector3.Subtract(ref contact.Position, ref result3, out var result4);
				num4 = result4.LengthSquared();
				if (!(num4 > 1E-07f))
				{
					continue;
				}
				Vector3.Divide(ref result4, (float)Math.Sqrt(num4), out result4);
				Vector3.Dot(ref result4, ref vector, out result);
				Vector3.Dot(ref contact.Normal, ref vector, out var result5);
				if (Math.Abs(result) > Math.Abs(result5))
				{
					Vector3.Dot(ref result4, ref contact.Normal, out result);
					if (result < 0f)
					{
						Vector3.Negate(ref result4, out result4);
						result = 0f - result;
					}
					contact.PenetrationDepth *= result;
					contact.Normal = result4;
				}
			}
		}
	}

	private void ComputeRelativeVelocity(ref SupportData supportData, out Vector3 relativeVelocity)
	{
		relativeVelocity = Body.LinearVelocity;
		if (!SupportFinder.HasSupport || !(supportData.SupportObject is EntityCollidable entityCollidable))
		{
			return;
		}
		bool isDynamic;
		if (isDynamic = entityCollidable.Entity.IsDynamic)
		{
			entityCollidable.Entity.Locker.Enter();
		}
		Vector3 value;
		try
		{
			value = Toolbox.GetVelocityOfPoint(supportData.Position, entityCollidable.Entity);
		}
		finally
		{
			if (isDynamic)
			{
				entityCollidable.Entity.Locker.Exit();
			}
		}
		Vector3.Subtract(ref relativeVelocity, ref value, out relativeVelocity);
	}

	private void ApplyJumpVelocity(ref SupportData supportData, Vector3 velocityChange, ref Vector3 relativeVelocity)
	{
		Body.LinearVelocity += velocityChange;
		if (supportData.SupportObject is EntityCollidable entityCollidable && entityCollidable.Entity.IsDynamic)
		{
			Vector3 vector = velocityChange * jumpForceFactor;
			entityCollidable.Entity.Locker.Enter();
			try
			{
				entityCollidable.Entity.LinearMomentum += vector * (0f - Body.Mass);
			}
			finally
			{
				entityCollidable.Entity.Locker.Exit();
			}
			velocityChange += vector;
		}
		Vector3.Add(ref relativeVelocity, ref velocityChange, out relativeVelocity);
	}

	private void ChangeVelocityUnilaterally(Vector3 velocityChange, ref Vector3 relativeVelocity)
	{
		Body.LinearVelocity += velocityChange;
		Vector3.Add(ref relativeVelocity, ref velocityChange, out relativeVelocity);
	}

	public void Jump()
	{
		tryToJump = true;
	}

	public void Leap()
	{
		tryToLeap = true;
	}

	public override void OnAdditionToSpace(ISpace newSpace)
	{
		newSpace.Add(Body);
		newSpace.Add(HorizontalMotionConstraint);
		newSpace.Add(VerticalMotionConstraint);
		((Space)newSpace).BoundingBoxUpdater.Finishing += ExpandBoundingBox;
		Body.AngularVelocity = default(Vector3);
		Body.LinearVelocity = default(Vector3);
	}

	public override void OnRemovalFromSpace(ISpace oldSpace)
	{
		oldSpace.Remove(Body);
		oldSpace.Remove(HorizontalMotionConstraint);
		oldSpace.Remove(VerticalMotionConstraint);
		((Space)oldSpace).BoundingBoxUpdater.Finishing -= ExpandBoundingBox;
		SupportFinder.ClearSupportData();
		Body.AngularVelocity = default(Vector3);
		Body.LinearVelocity = default(Vector3);
	}
}
