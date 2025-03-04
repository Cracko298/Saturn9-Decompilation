using System.IO;
using Microsoft.Xna.Framework;

namespace Saturn9;

public class BotPath
{
	public enum BotNodeType
	{
		FOLLOW,
		END
	}

	public const int MAX_BOTNODES = 1024;

	public BotNode[] m_BotNode;

	public int m_PrevBotNodeID = -1;

	public int m_NumNodes = -1;

	public bool m_bDoneLineOfSightRaycast;

	public int m_Id;

	public BotPath()
	{
		m_BotNode = new BotNode[1024];
		for (int i = 0; i < 1024; i++)
		{
			m_BotNode[i] = default(BotNode);
		}
		DeleteAll();
	}

	public int Create(int type, Vector3 pos)
	{
		bool flag = false;
		int num = -1;
		for (int i = 0; i < 1024; i++)
		{
			if (m_BotNode[i].m_Type == -1)
			{
				flag = true;
				num = i;
				break;
			}
		}
		if (flag)
		{
			m_BotNode[num].m_Type = type;
			m_BotNode[num].m_Position = pos;
			return num;
		}
		return -1;
	}

	public void Delete(int Id)
	{
		m_BotNode[Id].m_Type = -1;
		m_BotNode[Id].m_Position = Vector3.Zero;
	}

	public void DeleteAll()
	{
		for (int i = 0; i < 1024; i++)
		{
			if (m_BotNode[i].m_Type != -1)
			{
				Delete(i);
			}
		}
	}

	public void Update()
	{
		Vector3 position = g.m_PlayerManager.GetLocalPlayer().m_Position;
		if (m_PrevBotNodeID != -1)
		{
			float num = (position - m_BotNode[m_PrevBotNodeID].m_Position).LengthSquared();
			if (num > 25f)
			{
				m_PrevBotNodeID = Create(0, position);
			}
		}
		else
		{
			m_PrevBotNodeID = Create(0, position);
		}
	}

	public void Render()
	{
	}

	public void Save()
	{
		int num = 0;
		for (int i = 0; i < 1024; i++)
		{
			if (m_BotNode[i].m_Type != -1)
			{
				num++;
			}
		}
		string path = $"botdata{g.m_App.m_Level}_{m_Id}.dat";
		path = Path.Combine("C:\\Dev\\XNA\\AlienHorror\\AlienHorrorContent\\Paths", path);
		FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Write);
		BinaryWriter binaryWriter = new BinaryWriter(fileStream);
		binaryWriter.Write(num);
		_ = Vector2.Zero;
		for (int j = 0; j < 1024; j++)
		{
			if (m_BotNode[j].m_Type != -1)
			{
				binaryWriter.Write(m_BotNode[j].m_Type);
				binaryWriter.Write((int)(m_BotNode[j].m_Position.X * 1000f));
				binaryWriter.Write((int)(m_BotNode[j].m_Position.Y * 1000f));
				binaryWriter.Write((int)(m_BotNode[j].m_Position.Z * 1000f));
			}
		}
		fileStream.Close();
		m_PrevBotNodeID = -1;
	}

	public void LoadBotPath()
	{
		if (m_BotNode[0].m_Type != -1)
		{
			return;
		}
		string path = $"Content\\Paths\\botdata{g.m_App.m_Level}_{m_Id}.dat";
		if (!File.Exists(path))
		{
			return;
		}
		FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
		if (fileStream.Length != 0)
		{
			BinaryReader binaryReader = new BinaryReader(fileStream);
			m_NumNodes = binaryReader.ReadInt32();
			Vector3 zero = Vector3.Zero;
			for (int i = 0; i < m_NumNodes; i++)
			{
				int type = binaryReader.ReadInt32();
				int num = binaryReader.ReadInt32();
				zero.X = (float)num * 0.001f;
				int num2 = binaryReader.ReadInt32();
				zero.Y = (float)num2 * 0.001f;
				int num3 = binaryReader.ReadInt32();
				zero.Z = (float)num3 * 0.001f;
				Create(type, zero);
			}
			fileStream.Close();
		}
	}

	public int FindNearestNode(Vector3 pos)
	{
		float num = 9999999f;
		int result = -1;
		float num2 = 0f;
		for (int i = 0; i < 1024; i++)
		{
			if (m_BotNode[i].m_Type != -1)
			{
				num2 = (m_BotNode[i].m_Position - pos).LengthSquared();
				if (num2 < num)
				{
					num = num2;
					result = i;
				}
			}
		}
		return result;
	}

	public int FindNearestNodeInRange(Vector3 pos, float radiusSq)
	{
		float num = 9999999f;
		int result = -1;
		float num2 = 0f;
		for (int i = 0; i < 1024; i++)
		{
			if (m_BotNode[i].m_Type != -1)
			{
				num2 = (m_BotNode[i].m_Position - pos).LengthSquared();
				if (num2 < num && num2 < radiusSq)
				{
					num = num2;
					result = i;
				}
			}
		}
		return result;
	}
}
