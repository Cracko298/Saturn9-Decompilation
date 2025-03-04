using BEPUphysics;
using BEPUphysics.BroadPhaseEntries;

namespace BEPUphysicsDemos.AlternateMovement.Character;

public struct SupportRayData
{
	public RayHit HitData;

	public Collidable HitObject;

	public bool HasTraction;
}
