using System;
using SynapseGaming.LightingSystem.Editor;
using SynapseGaming.LightingSystem.Rendering;

namespace Saturn9;

[Serializable]
[EditorCreatedObject]
public class MiscTriggerEntity : SceneEntity
{
	public float m_NextUseTime;

	public bool m_Complete;

	public int m_State;

	public string m_TriggerLink = "";

	public bool m_Enabled = true;

	public void Reset()
	{
		m_NextUseTime = 0f;
		m_Complete = false;
		m_State = 0;
		m_TriggerLink = "";
		m_Enabled = true;
	}
}
