using System;
using BEPUphysics;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.CollisionTests;
using BEPUphysics.DataStructures;
using BEPUphysics.MathExtensions;
using BEPUphysics.NarrowPhaseSystems;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.Settings;
using Microsoft.Xna.Framework;

namespace BEPUphysicsDemos.AlternateMovement.Character;

public class QueryManager
{
	private RawList<ContactData> contacts = new RawList<ContactData>();

	private RawList<ContactData> supportContacts = new RawList<ContactData>();

	private RawList<ContactData> tractionContacts = new RawList<ContactData>();

	private RawList<ContactData> sideContacts = new RawList<ContactData>();

	private RawList<ContactData> headContacts = new RawList<ContactData>();

	private ConvexCollidable<CylinderShape> standingQueryObject;

	private ConvexCollidable<CylinderShape> crouchingQueryObject;

	private ConvexCollidable<CylinderShape> currentQueryObject;

	private CharacterController character;

	private Func<BroadPhaseEntry, bool> SupportRayFilter;

	public RawList<ContactData> Contacts => contacts;

	public RawList<ContactData> SupportContacts => supportContacts;

	public RawList<ContactData> TractionContacts => tractionContacts;

	public RawList<ContactData> SideContacts => sideContacts;

	public RawList<ContactData> HeadContacts => headContacts;

	public QueryManager(CharacterController character)
	{
		this.character = character;
		currentQueryObject = new ConvexCollidable<CylinderShape>(character.Body.CollisionInformation.Shape);
		standingQueryObject = new ConvexCollidable<CylinderShape>(new CylinderShape(character.StanceManager.StandingHeight, character.Body.Radius));
		crouchingQueryObject = new ConvexCollidable<CylinderShape>(new CylinderShape(character.StanceManager.CrouchingHeight, character.Body.Radius));
		currentQueryObject.CollisionRules = character.Body.CollisionInformation.CollisionRules;
		standingQueryObject.CollisionRules = character.Body.CollisionInformation.CollisionRules;
		crouchingQueryObject.CollisionRules = character.Body.CollisionInformation.CollisionRules;
		SupportRayFilter = SupportRayFilterFunction;
	}

	internal void UpdateQueryShapes()
	{
		standingQueryObject.Shape.Radius = character.Body.CollisionInformation.Shape.Radius;
		standingQueryObject.Shape.Height = character.StanceManager.StandingHeight;
		crouchingQueryObject.Shape.Radius = character.Body.CollisionInformation.Shape.Radius;
		crouchingQueryObject.Shape.Height = character.StanceManager.CrouchingHeight;
	}

	private bool SupportRayFilterFunction(BroadPhaseEntry entry)
	{
		return CollisionRules.CollisionRuleCalculator(entry, character.Body.CollisionInformation) == CollisionRule.Normal;
	}

	public bool RayCast(Ray ray, float length, out RayHit earliestHit)
	{
		earliestHit = default(RayHit);
		earliestHit.T = float.MaxValue;
		foreach (Collidable overlappedCollidable in character.Body.CollisionInformation.OverlappedCollidables)
		{
			float? num = ray.Intersects(overlappedCollidable.BoundingBox);
			if (num.HasValue)
			{
				float? num2 = num;
				if (num2.GetValueOrDefault() < length && num2.HasValue && overlappedCollidable.RayCast(ray, length, SupportRayFilter, out var rayHit) && rayHit.T < earliestHit.T)
				{
					earliestHit = rayHit;
				}
			}
		}
		if (earliestHit.T == float.MaxValue)
		{
			return false;
		}
		return true;
	}

	public bool RayCast(Ray ray, float length, out RayHit earliestHit, out Collidable hitObject)
	{
		earliestHit = default(RayHit);
		earliestHit.T = float.MaxValue;
		hitObject = null;
		foreach (Collidable overlappedCollidable in character.Body.CollisionInformation.OverlappedCollidables)
		{
			float? num = ray.Intersects(overlappedCollidable.BoundingBox);
			if (num.HasValue)
			{
				float? num2 = num;
				if (num2.GetValueOrDefault() < length && num2.HasValue && overlappedCollidable.RayCast(ray, length, SupportRayFilter, out var rayHit) && rayHit.T < earliestHit.T)
				{
					earliestHit = rayHit;
					hitObject = overlappedCollidable;
				}
			}
		}
		if (earliestHit.T == float.MaxValue)
		{
			return false;
		}
		return true;
	}

	public bool RayCastHitAnything(Ray ray, float length)
	{
		foreach (Collidable overlappedCollidable in character.Body.CollisionInformation.OverlappedCollidables)
		{
			float? num = ray.Intersects(overlappedCollidable.BoundingBox);
			if (num.HasValue)
			{
				float? num2 = num;
				if (num2.GetValueOrDefault() < length && num2.HasValue && overlappedCollidable.RayCast(ray, length, SupportRayFilter, out var _))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void ClearContacts()
	{
		contacts.Clear();
		supportContacts.Clear();
		tractionContacts.Clear();
		sideContacts.Clear();
		headContacts.Clear();
	}

	public void QueryContacts(Vector3 position)
	{
		QueryContacts(position, currentQueryObject);
	}

	public void QueryContacts(Vector3 position, Stance stance)
	{
		QueryContacts(position, (stance == Stance.Standing) ? standingQueryObject : crouchingQueryObject);
	}

	private void QueryContacts(Vector3 position, EntityCollidable queryObject)
	{
		ClearContacts();
		RigidTransform transform = default(RigidTransform);
		transform.Position = position;
		transform.Orientation = character.Body.Orientation;
		queryObject.UpdateBoundingBoxForTransform(ref transform, 0f);
		ContactData item = default(ContactData);
		foreach (Collidable overlappedCollidable in character.Body.CollisionInformation.OverlappedCollidables)
		{
			if (!overlappedCollidable.BoundingBox.Intersects(queryObject.BoundingBox))
			{
				continue;
			}
			CollidablePair pair = new CollidablePair(overlappedCollidable, queryObject);
			CollidablePairHandler pairHandler = NarrowPhaseHelper.GetPairHandler(ref pair);
			if (pairHandler.CollisionRule == CollisionRule.Normal)
			{
				pairHandler.UpdateCollision(0f);
				foreach (ContactInformation contact in pairHandler.Contacts)
				{
					if (contact.Pair.CollisionRule == CollisionRule.Normal)
					{
						item.Position = contact.Contact.Position;
						item.Normal = contact.Contact.Normal;
						item.Id = contact.Contact.Id;
						item.PenetrationDepth = contact.Contact.PenetrationDepth;
						contacts.Add(item);
					}
				}
			}
			pairHandler.CleanUp();
			pairHandler.Factory.GiveBack(pairHandler);
		}
		CategorizeContacts(ref position);
	}

	private void CategorizeContacts(ref Vector3 position)
	{
		Vector3 vector = character.Body.OrientationMatrix.Down;
		for (int i = 0; i < contacts.Count; i++)
		{
			Vector3.Subtract(ref contacts.Elements[i].Position, ref position, out var result);
			Vector3.Dot(ref contacts.Elements[i].Normal, ref result, out var result2);
			ContactData item = contacts.Elements[i];
			if (result2 < 0f)
			{
				result2 = 0f - result2;
				Vector3.Negate(ref item.Normal, out item.Normal);
			}
			Vector3.Dot(ref item.Normal, ref vector, out result2);
			if (result2 > SupportFinder.SideContactThreshold)
			{
				supportContacts.Add(item);
				if (result2 > character.SupportFinder.cosMaximumSlope)
				{
					tractionContacts.Add(item);
				}
				else
				{
					sideContacts.Add(item);
				}
			}
			else if (result2 < 0f - SupportFinder.SideContactThreshold)
			{
				headContacts.Add(item);
			}
			else
			{
				sideContacts.Add(item);
			}
		}
	}

	internal bool HasSupports(out bool hasTraction, out PositionState state, out ContactData supportContact)
	{
		float num = float.MinValue;
		int num2 = -1;
		if (tractionContacts.Count > 0)
		{
			for (int i = 0; i < tractionContacts.Count; i++)
			{
				if (tractionContacts.Elements[i].PenetrationDepth > num)
				{
					num = tractionContacts.Elements[i].PenetrationDepth;
					num2 = i;
				}
			}
			hasTraction = true;
			supportContact = tractionContacts.Elements[num2];
		}
		else
		{
			if (supportContacts.Count <= 0)
			{
				hasTraction = false;
				state = PositionState.NoHit;
				supportContact = default(ContactData);
				return false;
			}
			for (int j = 0; j < supportContacts.Count; j++)
			{
				if (supportContacts.Elements[j].PenetrationDepth > num)
				{
					num = supportContacts.Elements[j].PenetrationDepth;
					num2 = j;
				}
			}
			hasTraction = false;
			supportContact = supportContacts.Elements[num2];
		}
		if (num > CollisionDetectionSettings.AllowedPenetration)
		{
			state = PositionState.TooDeep;
		}
		else if (num < 0f)
		{
			state = PositionState.NoHit;
		}
		else
		{
			state = PositionState.Accepted;
		}
		return true;
	}
}
