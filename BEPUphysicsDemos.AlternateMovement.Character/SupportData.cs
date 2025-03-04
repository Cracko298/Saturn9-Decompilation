using BEPUphysics.BroadPhaseEntries;
using Microsoft.Xna.Framework;

namespace BEPUphysicsDemos.AlternateMovement.Character;

public struct SupportData
{
	public Vector3 Position;

	public Vector3 Normal;

	public bool HasTraction;

	public float Depth;

	public Collidable SupportObject;
}
