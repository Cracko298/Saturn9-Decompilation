using System;
using BEPUphysics.CollisionTests;
using BEPUphysics.DataStructures;
using BEPUphysics.Settings;
using Microsoft.Xna.Framework;

namespace BEPUphysicsDemos.AlternateMovement.Character;

public class StanceManager
{
	private float standingHeight;

	private float crouchingHeight;

	private CharacterController character;

	public float StandingHeight
	{
		get
		{
			return standingHeight;
		}
		set
		{
			if (value <= 0f || value < CrouchingHeight)
			{
				throw new Exception("Standing height must be positive and greater than the crouching height.");
			}
			standingHeight = value;
			character.QueryManager.UpdateQueryShapes();
			if (CurrentStance == Stance.Standing)
			{
				character.Body.CollisionInformation.Shape.Height = standingHeight;
			}
		}
	}

	public float CrouchingHeight
	{
		get
		{
			return crouchingHeight;
		}
		set
		{
			if (value <= 0f || value > StandingHeight)
			{
				throw new Exception("Crouching height must be positive and less than the standing height.");
			}
			crouchingHeight = value;
			character.QueryManager.UpdateQueryShapes();
			if (CurrentStance == Stance.Crouching)
			{
				character.Body.CollisionInformation.Shape.Height = crouchingHeight;
			}
		}
	}

	public Stance CurrentStance { get; private set; }

	public Stance DesiredStance { get; set; }

	public StanceManager(CharacterController character, float crouchingHeight)
	{
		this.character = character;
		standingHeight = character.Body.Height;
		if (crouchingHeight < standingHeight)
		{
			this.crouchingHeight = StandingHeight * 0.7f;
			return;
		}
		throw new Exception("Crouching height must be less than standing height.");
	}

	public bool UpdateStance(out Vector3 newPosition)
	{
		newPosition = default(Vector3);
		if (CurrentStance != DesiredStance)
		{
			if (CurrentStance == Stance.Standing && DesiredStance == Stance.Crouching)
			{
				if (character.SupportFinder.HasSupport)
				{
					newPosition = character.Body.Position + character.Body.OrientationMatrix.Down * ((StandingHeight - CrouchingHeight) * 0.5f);
					character.Body.Height = CrouchingHeight;
					CurrentStance = Stance.Crouching;
				}
				else
				{
					newPosition = character.Body.Position;
					character.Body.Height = CrouchingHeight;
					CurrentStance = Stance.Crouching;
				}
				return true;
			}
			if (CurrentStance == Stance.Crouching && DesiredStance == Stance.Standing)
			{
				if (character.SupportFinder.HasSupport)
				{
					newPosition = character.Body.Position - character.Body.OrientationMatrix.Down * ((StandingHeight - CrouchingHeight) * 0.5f);
					character.QueryManager.QueryContacts(newPosition, Stance.Standing);
					character.Body.Height = StandingHeight;
					CurrentStance = Stance.Standing;
					return true;
				}
				float num = 0f;
				float num2 = (StandingHeight - CrouchingHeight) * 0.5f;
				float num3 = num2;
				float num4 = num2;
				int num5 = 0;
				Vector3 down = character.Body.OrientationMatrix.Down;
				while (num5++ < 5 && num2 - num > 1E-05f)
				{
					Vector3 position = character.Body.Position + num3 * down;
					float hintOffset;
					switch (TrySupportLocation(ref position, out hintOffset))
					{
					case PositionState.Accepted:
						num3 += hintOffset;
						if (num3 > 0f && num3 < num4)
						{
							newPosition = character.Body.Position + num3 * down;
							character.Body.Height = StandingHeight;
							CurrentStance = Stance.Standing;
							return true;
						}
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
				newPosition = character.Body.Position;
				character.Body.Height = StandingHeight;
				CurrentStance = Stance.Standing;
				return true;
			}
		}
		return false;
	}

	private bool IsObstructed(RawList<ContactData> sideContacts, RawList<ContactData> headContacts)
	{
		if (headContacts.Count > 0)
		{
			return true;
		}
		for (int i = 0; i < sideContacts.Count; i++)
		{
			if (IsObstructive(ref sideContacts.Elements[i]))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsObstructive(ref ContactData contact)
	{
		if (character.SupportFinder.SideContacts.Count == 0 && contact.PenetrationDepth > CollisionDetectionSettings.AllowedPenetration)
		{
			return true;
		}
		foreach (OtherContact sideContact in character.SupportFinder.SideContacts)
		{
			float num = Vector3.Dot(contact.Normal, sideContact.Contact.Normal);
			float num2 = num * sideContact.Contact.PenetrationDepth;
			if (num2 > Math.Max(sideContact.Contact.PenetrationDepth, CollisionDetectionSettings.AllowedPenetration))
			{
				return true;
			}
		}
		return false;
	}

	private PositionState TrySupportLocation(ref Vector3 position, out float hintOffset)
	{
		hintOffset = 0f;
		character.QueryManager.QueryContacts(position, Stance.Standing);
		bool flag = IsObstructed(character.QueryManager.SideContacts, character.QueryManager.HeadContacts);
		if (character.QueryManager.HasSupports(out var _, out var state, out var supportContact) && !flag)
		{
			switch (state)
			{
			case PositionState.Accepted:
				hintOffset = Math.Min(0f, Vector3.Dot(supportContact.Normal, character.Body.OrientationMatrix.Down) * (CollisionDetectionSettings.AllowedPenetration * 0.5f - supportContact.PenetrationDepth));
				return PositionState.Accepted;
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
}
