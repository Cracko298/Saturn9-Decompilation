using BEPUphysics.Entities;
using BEPUphysics.Threading;

namespace BEPUphysicsDemos.AlternateMovement;

public class CharacterSynchronizer : ICharacterTag
{
	public static SpinLock ConstraintAccessLocker = new SpinLock();

	private Entity body;

	public long InstanceId => body.InstanceId;

	public CharacterSynchronizer(Entity body)
	{
		this.body = body;
	}
}
