namespace Saturn9;

public class BotPathManager
{
	public const int MAX_BOTPATHS = 32;

	public BotPath[] m_BotPath;

	public bool m_bDoneLineOfSightRaycast;

	public int m_CreatePathId = -1;

	public bool m_bCreatePath;

	public BotPathManager()
	{
		m_bDoneLineOfSightRaycast = false;
		m_CreatePathId = -1;
		m_bCreatePath = false;
		m_BotPath = new BotPath[32];
		for (int i = 0; i < 32; i++)
		{
			m_BotPath[i] = new BotPath();
		}
		DeleteAll();
	}

	public void Update()
	{
		if (m_bCreatePath)
		{
			m_BotPath[m_CreatePathId].Update();
		}
		m_bDoneLineOfSightRaycast = false;
	}

	public void Render()
	{
		for (int i = 0; i < 32; i++)
		{
			m_BotPath[i].Render();
		}
	}

	public void DeleteAll()
	{
		for (int i = 0; i < 32; i++)
		{
			m_BotPath[i].DeleteAll();
		}
	}

	public void LoadBotPath()
	{
		for (int i = 0; i < 32; i++)
		{
			m_BotPath[i].m_Id = i;
			m_BotPath[i].LoadBotPath();
		}
	}

	public void Save()
	{
		if (m_bCreatePath)
		{
			m_BotPath[m_CreatePathId].m_Id = m_CreatePathId;
			m_BotPath[m_CreatePathId].Save();
		}
	}

	public void ToggleCreatePath()
	{
		if (!m_bCreatePath)
		{
			m_CreatePathId++;
			m_BotPath[m_CreatePathId].DeleteAll();
			m_bCreatePath = true;
		}
		else
		{
			m_bCreatePath = false;
		}
	}
}
