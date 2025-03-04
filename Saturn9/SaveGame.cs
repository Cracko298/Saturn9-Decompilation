using System.IO;

namespace Saturn9;

public class SaveGame
{
	private const int XOR = 65521;

	public void Save(BinaryWriter writer)
	{
		writer.Write(g.m_App.m_CheckpointId ^ 0xFFF1);
	}

	public void Restore(BinaryReader reader)
	{
		g.m_App.m_CheckpointId = reader.ReadInt32() ^ 0xFFF1;
	}
}
