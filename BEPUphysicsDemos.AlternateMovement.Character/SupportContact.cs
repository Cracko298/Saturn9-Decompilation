using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.CollisionTests;

namespace BEPUphysicsDemos.AlternateMovement.Character;

public struct SupportContact
{
	public ContactData Contact;

	public Collidable Support;

	public bool HasTraction;
}
