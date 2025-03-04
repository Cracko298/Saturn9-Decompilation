using SynapseGaming.LightingSystem.Lights;

namespace Saturn9;

public struct FlickerLight
{
	public PointLight m_Light;

	public float m_Time;

	public void Flicker(float t)
	{
		if (m_Time < t)
		{
			if (m_Light.Enabled)
			{
				m_Time = t + (float)g.m_App.m_Rand.NextDouble() * 0.3f;
				m_Light.Enabled = false;
			}
			else
			{
				m_Time = t + (float)g.m_App.m_Rand.NextDouble() * 0.5f + 0.04f;
				m_Light.Enabled = true;
			}
		}
	}
}
