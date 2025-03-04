using System;
using BEPUphysics;
using BEPUphysics.CollisionTests;
using BEPUphysics.DataStructures;
using BEPUphysics.MathExtensions;
using BEPUphysics.Settings;
using Microsoft.Xna.Framework;

namespace BEPUphysicsDemos.AlternateMovement.Character;

public class StepManager
{
	private CharacterController character;

	private float maximumStepHeight = 0.1f;

	private float minimumDownStepHeight = 0.1f;

	private float minimumUpStepHeight;

	private RawList<ContactData> stepContacts = new RawList<ContactData>();

	private float upStepMargin = 0.1f;

	public float MaximumStepHeight
	{
		get
		{
			return maximumStepHeight;
		}
		set
		{
			if (maximumStepHeight < 0f)
			{
				throw new Exception("Value must be nonnegative.");
			}
			maximumStepHeight = value;
		}
	}

	public float MinimumDownStepHeight
	{
		get
		{
			return minimumDownStepHeight;
		}
		set
		{
			if (minimumDownStepHeight < 0f)
			{
				throw new Exception("Value must be nonnegative.");
			}
			minimumDownStepHeight = value;
		}
	}

	public StepManager(CharacterController character)
	{
		this.character = character;
		minimumUpStepHeight = CollisionDetectionSettings.AllowedPenetration * 1.1f;
	}

	private bool IsDownStepObstructed(RawList<ContactData> sideContacts)
	{
		for (int i = 0; i < sideContacts.Count; i++)
		{
			if (IsObstructiveToDownStepping(ref sideContacts.Elements[i]))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsObstructiveToDownStepping(ref ContactData contact)
	{
		if (character.SupportFinder.SideContacts.Count == 0 && contact.PenetrationDepth > CollisionDetectionSettings.AllowedPenetration)
		{
			return true;
		}
		foreach (OtherContact sideContact in character.SupportFinder.SideContacts)
		{
			float num = Vector3.Dot(contact.Normal, sideContact.Contact.Normal);
			float num2 = num * sideContact.Contact.PenetrationDepth;
			if (num2 > sideContact.Contact.PenetrationDepth)
			{
				return true;
			}
		}
		return false;
	}

	public bool TryToStepDown(out Vector3 newPosition)
	{
		if (character.SupportFinder.supports.Count == 0 && character.SupportFinder.SupportRayData.HasValue && character.SupportFinder.SupportRayData.Value.HasTraction && character.SupportFinder.SupportRayData.Value.HitData.T - character.SupportFinder.RayLengthToBottom > minimumDownStepHeight)
		{
			Vector3 normal = character.SupportFinder.SupportRayData.Value.HitData.Normal;
			Vector3 down = character.Body.OrientationMatrix.Down;
			RigidTransform shapeTransform = character.Body.CollisionInformation.WorldTransform;
			Ray ray = default(Ray);
			character.Body.CollisionInformation.Shape.GetExtremePoint(normal, ref shapeTransform, out ray.Position);
			ray.Direction = down;
			Plane p = new Plane(normal, Vector3.Dot(character.SupportFinder.SupportRayData.Value.HitData.Location, normal));
			float num = 0f;
			float num2 = character.Body.CollisionInformation.Shape.CollisionMargin + character.SupportFinder.SupportRayData.Value.HitData.T - character.SupportFinder.RayLengthToBottom;
			float num3 = num2;
			float hintOffset;
			if (Toolbox.GetRayPlaneIntersection(ref ray, ref p, out var t, out var _))
			{
				num3 = t + CollisionDetectionSettings.AllowedPenetration;
				Vector3 position = character.Body.Position + down * num3;
				switch (TryDownStepPosition(ref position, out hintOffset))
				{
				case PositionState.Accepted:
					num3 += hintOffset;
					if (num3 > minimumDownStepHeight && num3 < maximumStepHeight)
					{
						newPosition = character.Body.Position + num3 * down;
						return true;
					}
					newPosition = default(Vector3);
					return false;
				case PositionState.NoHit:
					num = num3 + hintOffset;
					num3 = (num2 + num3) * 0.5f;
					break;
				case PositionState.Obstructed:
					num2 = num3;
					num3 = (num + num3) * 0.5f;
					break;
				case PositionState.TooDeep:
					num3 += hintOffset;
					num2 = num3;
					break;
				}
			}
			int num4 = 0;
			while (num4++ < 5 && num2 - num > 1E-05f)
			{
				Vector3 position = character.Body.Position + num3 * down;
				switch (TryDownStepPosition(ref position, out hintOffset))
				{
				case PositionState.Accepted:
					num3 += hintOffset;
					if (num3 > minimumDownStepHeight && num3 < maximumStepHeight)
					{
						newPosition = character.Body.Position + num3 * down;
						return true;
					}
					newPosition = default(Vector3);
					return false;
				case PositionState.NoHit:
					num = num3 + hintOffset;
					num3 = (num2 + num) * 0.5f;
					break;
				case PositionState.Obstructed:
					num2 = num3;
					num3 = (num + num2) * 0.5f;
					break;
				case PositionState.TooDeep:
					num3 += hintOffset;
					num2 = num3;
					break;
				}
			}
			newPosition = default(Vector3);
			return false;
		}
		newPosition = default(Vector3);
		return false;
	}

	private PositionState TryDownStepPosition(ref Vector3 position, out float hintOffset)
	{
		hintOffset = 0f;
		character.QueryManager.QueryContacts(position);
		bool flag = IsDownStepObstructed(character.QueryManager.SideContacts);
		if (character.QueryManager.HasSupports(out var _, out var state, out var supportContact) && !flag)
		{
			switch (state)
			{
			case PositionState.Accepted:
				hintOffset = (0f - Vector3.Dot(supportContact.Normal, character.Body.OrientationMatrix.Down)) * supportContact.PenetrationDepth;
				return PositionState.Accepted;
			case PositionState.TooDeep:
				hintOffset = Math.Min(0f, 0.001f - Vector3.Dot(supportContact.Normal, character.Body.OrientationMatrix.Down) * supportContact.PenetrationDepth);
				return PositionState.TooDeep;
			default:
				hintOffset = -0.001f - Vector3.Dot(supportContact.Normal, character.Body.OrientationMatrix.Down) * supportContact.PenetrationDepth;
				return PositionState.NoHit;
			}
		}
		if (flag)
		{
			return PositionState.Obstructed;
		}
		return PositionState.NoHit;
	}

	public bool TryToStepUp(out Vector3 newPosition)
	{
		if (character.SupportFinder.HasTraction)
		{
			stepContacts.Clear();
			FindUpStepCandidates(stepContacts);
			for (int i = 0; i < stepContacts.Count; i++)
			{
				if (TryToStepUsingContact(ref stepContacts.Elements[i], out newPosition))
				{
					return true;
				}
			}
		}
		newPosition = default(Vector3);
		return false;
	}

	private void FindUpStepCandidates(RawList<ContactData> outputStepCandidates)
	{
		foreach (OtherContact sideContact in character.SupportFinder.sideContacts)
		{
			Vector3 vector = default(Vector3);
			vector.X = character.HorizontalMotionConstraint.MovementDirection.X;
			vector.Z = character.HorizontalMotionConstraint.MovementDirection.Y;
			Vector3 vector2 = vector;
			ContactData contact = sideContact.Contact;
			Vector3.Dot(ref contact.Normal, ref vector2, out var result);
			if (!(result > 0f))
			{
				continue;
			}
			result = Vector3.Dot(character.Body.OrientationMatrix.Down, sideContact.Contact.Position - character.Body.Position);
			if (!(result < character.Body.Height * 0.5f) || !(result > character.Body.Height * 0.5f - maximumStepHeight - upStepMargin))
			{
				continue;
			}
			bool flag = true;
			for (int i = 0; i < outputStepCandidates.Count; i++)
			{
				Vector3.Dot(ref outputStepCandidates.Elements[i].Normal, ref contact.Normal, out result);
				if (result > 0.99f)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				outputStepCandidates.Add(contact);
			}
		}
	}

	private bool TryToStepUsingContact(ref ContactData contact, out Vector3 newPosition)
	{
		Vector3 vector = character.Body.OrientationMatrix.Down;
		Vector3 value = character.Body.Position;
		Vector3 vector2 = contact.Normal;
		Vector3.Dot(ref vector2, ref vector, out var result);
		Vector3.Multiply(ref vector, result, out var result2);
		Vector3.Subtract(ref vector2, ref result2, out vector2);
		vector2.Normalize();
		float height = character.Body.Height;
		Ray ray = default(Ray);
		Vector3.Multiply(ref vector, character.Body.Height * 0.5f - height, out ray.Position);
		Vector3.Add(ref ray.Position, ref value, out ray.Position);
		ray.Direction = vector2;
		float collisionMargin = character.Body.CollisionInformation.Shape.CollisionMargin;
		float num = character.Body.Radius + collisionMargin;
		if (character.QueryManager.RayCastHitAnything(ray, num))
		{
			newPosition = default(Vector3);
			return false;
		}
		Vector3.Multiply(ref vector2, num, out var result3);
		Vector3.Add(ref ray.Position, ref result3, out ray.Position);
		ray.Direction = vector;
		RayHit earliestHit = default(RayHit);
		if (!character.QueryManager.RayCast(ray, height, out earliestHit) || earliestHit.T <= 0f || earliestHit.T - height > 0f - minimumUpStepHeight || earliestHit.T - height < 0f - maximumStepHeight - upStepMargin)
		{
			newPosition = default(Vector3);
			return false;
		}
		Vector3.Normalize(ref earliestHit.Normal, out var result4);
		Vector3.Dot(ref result4, ref vector, out result);
		if (result < 0f)
		{
			Vector3.Negate(ref result4, out result4);
			result = 0f - result;
		}
		if (result < character.SupportFinder.cosMaximumSlope)
		{
			newPosition = default(Vector3);
			return false;
		}
		Vector3.Negate(ref vector, out ray.Direction);
		float length = character.Body.Height - earliestHit.T;
		if (character.QueryManager.RayCastHitAnything(ray, length))
		{
			newPosition = default(Vector3);
			return false;
		}
		RigidTransform shapeTransform = character.Body.CollisionInformation.WorldTransform;
		Vector3.Multiply(ref vector2, collisionMargin, out result3);
		Vector3.Add(ref shapeTransform.Position, ref result3, out shapeTransform.Position);
		Vector3.Multiply(ref vector, 0f - height, out var result5);
		Vector3.Add(ref shapeTransform.Position, ref result5, out shapeTransform.Position);
		Ray ray2 = default(Ray);
		character.Body.CollisionInformation.Shape.GetExtremePoint(result4, ref shapeTransform, out ray2.Position);
		ray2.Direction = vector;
		Vector3.Dot(ref earliestHit.Location, ref result4, out result);
		Plane p = new Plane(result4, result);
		float num2 = 0f - maximumStepHeight;
		float num3 = character.Body.CollisionInformation.Shape.CollisionMargin - height + earliestHit.T;
		float num4 = num3;
		float hintOffset;
		if (Toolbox.GetRayPlaneIntersection(ref ray2, ref p, out var t, out var _))
		{
			t = 0f - height + t + CollisionDetectionSettings.AllowedPenetration;
			if (t < num2)
			{
				t = num2;
			}
			num4 = t;
			if (num4 > num3)
			{
				num3 = num4;
			}
			Vector3 position = character.Body.Position + vector * num4 + result3;
			switch (TryUpStepPosition(ref vector2, ref position, out hintOffset))
			{
			case PositionState.Accepted:
				num4 += hintOffset;
				if (num4 < 0f && num4 > 0f - maximumStepHeight - CollisionDetectionSettings.AllowedPenetration)
				{
					newPosition = character.Body.Position + Math.Max(0f - maximumStepHeight, num4) * vector + result3;
					return true;
				}
				newPosition = default(Vector3);
				return false;
			case PositionState.Rejected:
				newPosition = default(Vector3);
				return false;
			case PositionState.NoHit:
				num2 = num4 + hintOffset;
				num4 = (num3 + num4) * 0.5f;
				break;
			case PositionState.Obstructed:
				num3 = num4;
				num4 = (num2 + num4) * 0.5f;
				break;
			case PositionState.HeadObstructed:
				num2 = num4 + hintOffset;
				num4 = (num3 + num4) * 0.5f;
				break;
			case PositionState.TooDeep:
				num4 += hintOffset;
				num3 = num4;
				break;
			}
		}
		int num5 = 0;
		while (num5++ < 5 && num3 - num2 > 1E-05f)
		{
			Vector3 position = character.Body.Position + num4 * vector + result3;
			switch (TryUpStepPosition(ref vector2, ref position, out hintOffset))
			{
			case PositionState.Accepted:
				num4 += hintOffset;
				if (num4 < 0f && num4 > 0f - maximumStepHeight - CollisionDetectionSettings.AllowedPenetration)
				{
					newPosition = character.Body.Position + Math.Max(0f - maximumStepHeight, num4) * vector + result3;
					return true;
				}
				newPosition = default(Vector3);
				return false;
			case PositionState.Rejected:
				newPosition = default(Vector3);
				return false;
			case PositionState.NoHit:
				num2 = num4 + hintOffset;
				num4 = (num3 + num2) * 0.5f;
				break;
			case PositionState.Obstructed:
				num3 = num4;
				num4 = (num2 + num3) * 0.5f;
				break;
			case PositionState.HeadObstructed:
				num2 = num4 + hintOffset;
				num4 = (num3 + num4) * 0.5f;
				break;
			case PositionState.TooDeep:
				num4 += hintOffset;
				num3 = num4;
				break;
			}
		}
		newPosition = default(Vector3);
		return false;
	}

	private PositionState TryUpStepPosition(ref Vector3 sideNormal, ref Vector3 position, out float hintOffset)
	{
		hintOffset = 0f;
		character.QueryManager.QueryContacts(position);
		if (character.QueryManager.HeadContacts.Count > 0)
		{
			Vector3 vector = character.Body.OrientationMatrix.Up;
			Vector3.Dot(ref vector, ref character.QueryManager.HeadContacts.Elements[0].Normal, out var result);
			hintOffset = result * character.QueryManager.HeadContacts.Elements[0].PenetrationDepth;
			for (int i = 1; i < character.QueryManager.HeadContacts.Count; i++)
			{
				Vector3.Dot(ref vector, ref character.QueryManager.HeadContacts.Elements[i].Normal, out result);
				result *= character.QueryManager.HeadContacts.Elements[i].PenetrationDepth;
				if (result > hintOffset)
				{
					hintOffset = result;
				}
			}
			return PositionState.HeadObstructed;
		}
		bool flag = IsUpStepObstructed(ref sideNormal, character.QueryManager.SideContacts, character.QueryManager.HeadContacts);
		if (character.QueryManager.HasSupports(out var hasTraction, out var state, out var supportContact) && !flag)
		{
			switch (state)
			{
			case PositionState.Accepted:
			{
				if (hasTraction)
				{
					hintOffset = Math.Min(0f, Vector3.Dot(supportContact.Normal, character.Body.OrientationMatrix.Down) * (CollisionDetectionSettings.AllowedPenetration * 0.5f - supportContact.PenetrationDepth));
					return PositionState.Accepted;
				}
				Vector3 vector2 = character.Body.OrientationMatrix.Down;
				Ray ray = default(Ray);
				ray.Position = supportContact.Position + sideNormal * 0.001f;
				float num = Vector3.Dot(ray.Position - position, vector2);
				num = character.Body.Height * 0.5f + num;
				ray.Position -= num * vector2;
				ray.Direction = vector2;
				Ray ray2 = default(Ray);
				ray2.Position = position + character.Body.OrientationMatrix.Up * (character.Body.Height * 0.5f);
				ray2.Direction = ray.Position - ray2.Position;
				if (!character.QueryManager.RayCastHitAnything(ray2, 1f) && character.QueryManager.RayCast(ray, character.Body.Height, out var earliestHit) && character.Body.Height - maximumStepHeight < earliestHit.T)
				{
					earliestHit.Normal.Normalize();
					Vector3.Dot(ref earliestHit.Normal, ref vector2, out var result2);
					if (Math.Abs(result2) > character.SupportFinder.cosMaximumSlope)
					{
						hintOffset = Math.Min(0f, Vector3.Dot(supportContact.Normal, character.Body.OrientationMatrix.Down) * (CollisionDetectionSettings.AllowedPenetration * 0.5f - supportContact.PenetrationDepth));
						ray.Position = position;
						if (character.QueryManager.RayCast(ray, character.Body.Height * 0.5f + maximumStepHeight, out earliestHit))
						{
							earliestHit.Normal.Normalize();
							Vector3.Dot(ref earliestHit.Normal, ref vector2, out result2);
							if (Math.Abs(result2) > character.SupportFinder.cosMaximumSlope)
							{
								return PositionState.Accepted;
							}
						}
					}
				}
				return PositionState.Rejected;
			}
			case PositionState.TooDeep:
				hintOffset = Math.Min(0f, Vector3.Dot(supportContact.Normal, character.Body.OrientationMatrix.Down) * (CollisionDetectionSettings.AllowedPenetration * 0.5f - supportContact.PenetrationDepth));
				return PositionState.TooDeep;
			default:
				hintOffset = -0.001f - Vector3.Dot(supportContact.Normal, character.Body.OrientationMatrix.Down) * supportContact.PenetrationDepth;
				return PositionState.NoHit;
			}
		}
		if (flag)
		{
			return PositionState.Obstructed;
		}
		return PositionState.NoHit;
	}

	private bool IsUpStepObstructed(ref Vector3 sideNormal, RawList<ContactData> sideContacts, RawList<ContactData> headContacts)
	{
		for (int i = 0; i < sideContacts.Count; i++)
		{
			if (IsObstructiveToUpStepping(ref sideNormal, ref sideContacts.Elements[i]))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsObstructiveToUpStepping(ref Vector3 sideNormal, ref ContactData contact)
	{
		Vector3.Dot(ref contact.Normal, ref sideNormal, out var result);
		if (result * contact.PenetrationDepth > CollisionDetectionSettings.AllowedPenetration)
		{
			return true;
		}
		foreach (OtherContact sideContact in character.SupportFinder.SideContacts)
		{
			result = Vector3.Dot(contact.Normal, sideContact.Contact.Normal);
			float num = result * sideContact.Contact.PenetrationDepth;
			if (num > Math.Max(sideContact.Contact.PenetrationDepth, CollisionDetectionSettings.AllowedPenetration))
			{
				return true;
			}
		}
		return false;
	}
}
