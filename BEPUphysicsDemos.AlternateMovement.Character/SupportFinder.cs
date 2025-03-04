using System;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.CollisionTests;
using BEPUphysics.DataStructures;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using Microsoft.Xna.Framework;

namespace BEPUphysicsDemos.AlternateMovement.Character;

public class SupportFinder
{
	internal static float SideContactThreshold = 0.01f;

	internal RawList<SupportContact> supports = new RawList<SupportContact>();

	internal RawList<OtherContact> sideContacts = new RawList<OtherContact>();

	internal RawList<OtherContact> headContacts = new RawList<OtherContact>();

	private float bottomHeight;

	private CharacterController character;

	internal float sinMaximumSlope = (float)Math.Sin(MathHelper.ToRadians(60f) + 0.01f);

	internal float cosMaximumSlope = (float)Math.Cos(MathHelper.ToRadians(60f) + 0.01f);

	public float RayLengthToBottom => bottomHeight;

	public SupportData? SupportData
	{
		get
		{
			if (supports.Count > 0)
			{
				SupportData supportData = default(SupportData);
				supportData.Position = supports.Elements[0].Contact.Position;
				supportData.Normal = supports.Elements[0].Contact.Normal;
				SupportData value = supportData;
				for (int i = 1; i < supports.Count; i++)
				{
					Vector3.Add(ref value.Position, ref supports.Elements[i].Contact.Position, out value.Position);
					Vector3.Add(ref value.Normal, ref supports.Elements[i].Contact.Normal, out value.Normal);
				}
				if (supports.Count > 1)
				{
					Vector3.Multiply(ref value.Position, 1f / (float)supports.Count, out value.Position);
					float num = value.Normal.LengthSquared();
					if (num < 1E-07f)
					{
						value.Normal = supports.Elements[0].Contact.Normal;
					}
					else
					{
						Vector3.Multiply(ref value.Normal, 1f / (float)Math.Sqrt(num), out value.Normal);
					}
				}
				float num2 = float.MinValue;
				Collidable supportObject = null;
				for (int j = 0; j < supports.Count; j++)
				{
					Vector3.Dot(ref supports.Elements[j].Contact.Normal, ref value.Normal, out var result);
					result *= supports.Elements[j].Contact.PenetrationDepth;
					if (result > num2)
					{
						num2 = result;
						supportObject = supports.Elements[j].Support;
					}
				}
				value.Depth = num2;
				value.SupportObject = supportObject;
				return value;
			}
			if (SupportRayData.HasValue)
			{
				SupportData value2 = default(SupportData);
				value2.Position = SupportRayData.Value.HitData.Location;
				value2.Normal = SupportRayData.Value.HitData.Normal;
				value2.HasTraction = SupportRayData.Value.HasTraction;
				value2.Depth = Vector3.Dot(character.Body.OrientationMatrix.Down, SupportRayData.Value.HitData.Normal) * (bottomHeight - SupportRayData.Value.HitData.T);
				value2.SupportObject = SupportRayData.Value.HitObject;
				return value2;
			}
			return null;
		}
	}

	public SupportData? TractionData
	{
		get
		{
			if (supports.Count > 0)
			{
				SupportData value = default(SupportData);
				int num = 0;
				for (int i = 0; i < supports.Count; i++)
				{
					if (supports.Elements[i].HasTraction)
					{
						num++;
						Vector3.Add(ref value.Position, ref supports.Elements[i].Contact.Position, out value.Position);
						Vector3.Add(ref value.Normal, ref supports.Elements[i].Contact.Normal, out value.Normal);
					}
				}
				if (num > 1)
				{
					Vector3.Multiply(ref value.Position, 1f / (float)num, out value.Position);
					float num2 = value.Normal.LengthSquared();
					if (num2 < 1E-05f)
					{
						value.Normal = supports.Elements[0].Contact.Normal;
					}
					else
					{
						Vector3.Multiply(ref value.Normal, 1f / (float)Math.Sqrt(num2), out value.Normal);
					}
				}
				if (num > 0)
				{
					float num3 = float.MinValue;
					Collidable supportObject = null;
					for (int j = 0; j < supports.Count; j++)
					{
						if (supports.Elements[j].HasTraction)
						{
							Vector3.Dot(ref supports.Elements[j].Contact.Normal, ref value.Normal, out var result);
							result *= supports.Elements[j].Contact.PenetrationDepth;
							if (result > num3)
							{
								num3 = result;
								supportObject = supports.Elements[j].Support;
							}
						}
					}
					value.Depth = num3;
					value.SupportObject = supportObject;
					value.HasTraction = true;
					return value;
				}
			}
			if (SupportRayData.HasValue && SupportRayData.Value.HasTraction)
			{
				SupportData value2 = default(SupportData);
				value2.Position = SupportRayData.Value.HitData.Location;
				value2.Normal = SupportRayData.Value.HitData.Normal;
				value2.HasTraction = true;
				value2.Depth = Vector3.Dot(character.Body.OrientationMatrix.Down, SupportRayData.Value.HitData.Normal) * (bottomHeight - SupportRayData.Value.HitData.T);
				value2.SupportObject = SupportRayData.Value.HitObject;
				return value2;
			}
			return null;
		}
	}

	public bool HasSupport { get; private set; }

	public bool HasTraction { get; private set; }

	public SupportRayData? SupportRayData { get; private set; }

	public ReadOnlyList<SupportContact> Supports => new ReadOnlyList<SupportContact>(supports);

	public ReadOnlyList<OtherContact> SideContacts => new ReadOnlyList<OtherContact>(sideContacts);

	public ReadOnlyList<OtherContact> HeadContacts => new ReadOnlyList<OtherContact>(headContacts);

	public TractionSupportCollection TractionSupports => new TractionSupportCollection(supports);

	public float MaximumSlope
	{
		get
		{
			return (float)Math.Acos(MathHelper.Clamp(cosMaximumSlope, -1f, 1f));
		}
		set
		{
			cosMaximumSlope = (float)Math.Cos(value);
			sinMaximumSlope = (float)Math.Sin(value);
		}
	}

	public bool GetTractionInDirection(ref Vector3 movementDirection, out SupportData supportData)
	{
		if (HasTraction)
		{
			int num = -1;
			float num2 = float.MinValue;
			for (int i = 0; i < supports.Count; i++)
			{
				if (supports.Elements[i].HasTraction)
				{
					Vector3.Dot(ref movementDirection, ref supports.Elements[i].Contact.Normal, out var result);
					if (result > num2)
					{
						num2 = result;
						num = i;
					}
				}
			}
			if (num != -1)
			{
				supportData.Position = supports.Elements[num].Contact.Position;
				supportData.Normal = supports.Elements[num].Contact.Normal;
				supportData.SupportObject = supports.Elements[num].Support;
				supportData.HasTraction = true;
				float num3 = float.MinValue;
				for (int j = 0; j < supports.Count; j++)
				{
					if (supports.Elements[j].HasTraction)
					{
						Vector3.Dot(ref supports.Elements[j].Contact.Normal, ref supportData.Normal, out var result2);
						result2 *= supports.Elements[j].Contact.PenetrationDepth;
						if (result2 > num3)
						{
							num3 = result2;
						}
					}
				}
				supportData.Depth = num3;
				return true;
			}
			if (SupportRayData.HasValue && SupportRayData.Value.HasTraction)
			{
				supportData.Position = SupportRayData.Value.HitData.Location;
				supportData.Normal = SupportRayData.Value.HitData.Normal;
				supportData.Depth = Vector3.Dot(character.Body.OrientationMatrix.Down, SupportRayData.Value.HitData.Normal) * (bottomHeight - SupportRayData.Value.HitData.T);
				supportData.SupportObject = SupportRayData.Value.HitObject;
				supportData.HasTraction = true;
				return true;
			}
			supportData = default(SupportData);
			return false;
		}
		supportData = default(SupportData);
		return false;
	}

	public SupportFinder(CharacterController character)
	{
		this.character = character;
	}

	public void UpdateSupports()
	{
		bool hasTraction = HasTraction;
		HasTraction = false;
		HasSupport = false;
		Cylinder body = character.Body;
		Vector3 vector = character.Body.OrientationMatrix.Down;
		supports.Clear();
		sideContacts.Clear();
		headContacts.Clear();
		Vector3 value = character.Body.Position;
		OtherContact item2 = default(OtherContact);
		OtherContact item3 = default(OtherContact);
		foreach (CollidablePairHandler pair in character.Body.CollisionInformation.Pairs)
		{
			if (pair.CollisionRule != CollisionRule.Normal)
			{
				continue;
			}
			foreach (ContactInformation contact in pair.Contacts)
			{
				if (contact.Pair.CollisionRule != CollisionRule.Normal)
				{
					continue;
				}
				Vector3.Subtract(ref contact.Contact.Position, ref value, out var result);
				Vector3.Dot(ref result, ref contact.Contact.Normal, out var result2);
				Vector3 value2 = contact.Contact.Normal;
				if (result2 < 0f)
				{
					Vector3.Negate(ref value2, out value2);
					result2 = 0f - result2;
				}
				Vector3.Dot(ref value2, ref vector, out result2);
				if (result2 > SideContactThreshold)
				{
					HasSupport = true;
					SupportContact supportContact = default(SupportContact);
					supportContact.Contact = new ContactData
					{
						Position = contact.Contact.Position,
						Normal = value2,
						PenetrationDepth = contact.Contact.PenetrationDepth,
						Id = contact.Contact.Id
					};
					supportContact.Support = ((pair.BroadPhaseOverlap.EntryA != body.CollisionInformation) ? ((Collidable)pair.BroadPhaseOverlap.EntryA) : ((Collidable)pair.BroadPhaseOverlap.EntryB));
					SupportContact item = supportContact;
					if (result2 > cosMaximumSlope)
					{
						item.HasTraction = true;
						HasTraction = true;
					}
					else
					{
						sideContacts.Add(new OtherContact
						{
							Collidable = item.Support,
							Contact = item.Contact
						});
					}
					supports.Add(item);
				}
				else if (result2 < 0f - SideContactThreshold)
				{
					item2.Collidable = ((pair.BroadPhaseOverlap.EntryA != body.CollisionInformation) ? ((Collidable)pair.BroadPhaseOverlap.EntryA) : ((Collidable)pair.BroadPhaseOverlap.EntryB));
					item2.Contact.Position = contact.Contact.Position;
					item2.Contact.Normal = value2;
					item2.Contact.PenetrationDepth = contact.Contact.PenetrationDepth;
					item2.Contact.Id = contact.Contact.Id;
					headContacts.Add(item2);
				}
				else
				{
					item3.Collidable = ((pair.BroadPhaseOverlap.EntryA != body.CollisionInformation) ? ((Collidable)pair.BroadPhaseOverlap.EntryA) : ((Collidable)pair.BroadPhaseOverlap.EntryB));
					item3.Contact.Position = contact.Contact.Position;
					item3.Contact.Normal = value2;
					item3.Contact.PenetrationDepth = contact.Contact.PenetrationDepth;
					item3.Contact.Id = contact.Contact.Id;
					sideContacts.Add(item3);
				}
			}
		}
		SupportRayData = null;
		bottomHeight = body.Height * 0.25f;
		if (!HasTraction && hasTraction)
		{
			float length = (hasTraction ? (bottomHeight + character.StepManager.MaximumStepHeight) : bottomHeight);
			Ray ray = new Ray(body.Position + vector * body.Height * 0.25f, vector);
			if (TryDownCast(ref ray, length, out var _, out var supportRayData))
			{
				SupportRayData = supportRayData;
				HasTraction = supportRayData.HasTraction;
				HasSupport = true;
			}
		}
		bool flag = character.HorizontalMotionConstraint.MovementDirection.LengthSquared() > 0f;
		if (!HasTraction && hasTraction && flag)
		{
			Ray ray2 = new Ray(body.Position + new Vector3(character.HorizontalMotionConstraint.MovementDirection.X, 0f, character.HorizontalMotionConstraint.MovementDirection.Y) * (character.Body.Radius - character.Body.CollisionInformation.Shape.CollisionMargin) + vector * body.Height * 0.25f, vector);
			Ray ray3 = default(Ray);
			ray3.Position = body.Position + vector * body.Height * 0.25f;
			ray3.Direction = ray2.Position - ray3.Position;
			if (!character.QueryManager.RayCastHitAnything(ray3, 1f))
			{
				float length2 = (hasTraction ? (bottomHeight + character.StepManager.MaximumStepHeight) : bottomHeight);
				if (TryDownCast(ref ray2, length2, out var hasTraction3, out var supportRayData2) && (!SupportRayData.HasValue || supportRayData2.HitData.T < SupportRayData.Value.HitData.T))
				{
					if (hasTraction3)
					{
						SupportRayData = supportRayData2;
						HasTraction = true;
					}
					else if (!SupportRayData.HasValue)
					{
						SupportRayData = supportRayData2;
					}
					HasSupport = true;
				}
			}
		}
		if (!HasTraction && hasTraction && flag)
		{
			Vector3 vector2 = new Vector3(character.HorizontalMotionConstraint.MovementDirection.X, 0f, character.HorizontalMotionConstraint.MovementDirection.Y);
			Vector3.Cross(ref vector2, ref vector, out vector2);
			Vector3.Multiply(ref vector2, character.Body.Radius - character.Body.CollisionInformation.Shape.CollisionMargin, out vector2);
			Ray ray4 = new Ray(body.Position + vector2 + vector * body.Height * 0.25f, vector);
			Ray ray5 = default(Ray);
			ray5.Position = body.Position + vector * body.Height * 0.25f;
			ray5.Direction = ray4.Position - ray5.Position;
			if (!character.QueryManager.RayCastHitAnything(ray5, 1f))
			{
				float length3 = (hasTraction ? (bottomHeight + character.StepManager.MaximumStepHeight) : bottomHeight);
				if (TryDownCast(ref ray4, length3, out var hasTraction4, out var supportRayData3) && (!SupportRayData.HasValue || supportRayData3.HitData.T < SupportRayData.Value.HitData.T))
				{
					if (hasTraction4)
					{
						SupportRayData = supportRayData3;
						HasTraction = true;
					}
					else if (!SupportRayData.HasValue)
					{
						SupportRayData = supportRayData3;
					}
					HasSupport = true;
				}
			}
		}
		if (HasTraction || !hasTraction || !flag)
		{
			return;
		}
		Vector3 vector3 = new Vector3(character.HorizontalMotionConstraint.MovementDirection.X, 0f, character.HorizontalMotionConstraint.MovementDirection.Y);
		Vector3.Cross(ref vector, ref vector3, out vector3);
		Vector3.Multiply(ref vector3, character.Body.Radius - character.Body.CollisionInformation.Shape.CollisionMargin, out vector3);
		Ray ray6 = new Ray(body.Position + vector3 + vector * body.Height * 0.25f, vector);
		Ray ray7 = default(Ray);
		ray7.Position = body.Position + vector * body.Height * 0.25f;
		ray7.Direction = ray6.Position - ray7.Position;
		if (character.QueryManager.RayCastHitAnything(ray7, 1f))
		{
			return;
		}
		float length4 = (hasTraction ? (bottomHeight + character.StepManager.MaximumStepHeight) : bottomHeight);
		if (TryDownCast(ref ray6, length4, out var hasTraction5, out var supportRayData4) && (!SupportRayData.HasValue || supportRayData4.HitData.T < SupportRayData.Value.HitData.T))
		{
			if (hasTraction5)
			{
				SupportRayData = supportRayData4;
				HasTraction = true;
			}
			else if (!SupportRayData.HasValue)
			{
				SupportRayData = supportRayData4;
			}
			HasSupport = true;
		}
	}

	private bool TryDownCast(ref Ray ray, float length, out bool hasTraction, out SupportRayData supportRayData)
	{
		supportRayData = default(SupportRayData);
		hasTraction = false;
		if (character.QueryManager.RayCast(ray, length, out var earliestHit, out var hitObject))
		{
			float num = earliestHit.Normal.LengthSquared();
			if (num < 1E-07f)
			{
				return false;
			}
			Vector3.Divide(ref earliestHit.Normal, (float)Math.Sqrt(num), out earliestHit.Normal);
			earliestHit.Normal.Normalize();
			Vector3.Dot(ref ray.Direction, ref earliestHit.Normal, out var result);
			if (result < 0f)
			{
				Vector3.Negate(ref earliestHit.Normal, out earliestHit.Normal);
				result = 0f - result;
			}
			if (result > cosMaximumSlope)
			{
				hasTraction = true;
				supportRayData = new SupportRayData
				{
					HitData = earliestHit,
					HitObject = hitObject,
					HasTraction = true
				};
			}
			else
			{
				if (!(result > SideContactThreshold))
				{
					return false;
				}
				supportRayData = new SupportRayData
				{
					HitData = earliestHit,
					HitObject = hitObject
				};
			}
			return true;
		}
		return false;
	}

	internal void ClearSupportData()
	{
		HasSupport = false;
		HasTraction = false;
		supports.Clear();
		SupportRayData = null;
	}
}
