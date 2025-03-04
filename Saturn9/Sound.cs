using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Saturn9;

public struct Sound
{
	public const int MAX_INSTANCES_PER_SOUND = 2;

	public const float SOUND_RANGE_SQ = 1225f;

	private const float PITCH_AMNT = 0.2f;

	public SoundEffectInstance[] m_Instance;

	public int m_Index;

	public void Add(SoundEffect s)
	{
		for (int i = 0; i < 2; i++)
		{
			m_Instance[i] = s.CreateInstance();
		}
	}

	public SoundEffectInstance Play()
	{
		m_Index++;
		if (m_Index >= 2)
		{
			m_Index = 0;
		}
		m_Instance[m_Index].Volume = 1f;
		m_Instance[m_Index].Play();
		return m_Instance[m_Index];
	}

	public SoundEffectInstance Play(float vol)
	{
		m_Index++;
		if (m_Index >= 2)
		{
			m_Index = 0;
		}
		m_Instance[m_Index].Volume = vol;
		m_Instance[m_Index].Play();
		return m_Instance[m_Index];
	}

	public SoundEffectInstance Play(float vol, float pitch)
	{
		m_Index++;
		if (m_Index >= 2)
		{
			m_Index = 0;
		}
		m_Instance[m_Index].Volume = vol;
		m_Instance[m_Index].Pitch = pitch;
		m_Instance[m_Index].Play();
		return m_Instance[m_Index];
	}

	public SoundEffectInstance Play3D(Vector3 pos)
	{
		m_Index++;
		if (m_Index >= 2)
		{
			m_Index = 0;
		}
		float num = (pos - g.m_CameraManager.m_Position).LengthSquared();
		float value = 1f - num / 1225f;
		value = MathHelper.Clamp(value, 0f, 1f);
		value *= value;
		if (value < 0.01f)
		{
			return null;
		}
		float angle = (float)Math.Atan2(g.m_CameraManager.m_Position.Z - pos.Z, g.m_CameraManager.m_Position.X - pos.X) + g.m_PlayerManager.GetLocalPlayer().m_Rotation.Y + MathF.PI / 2f;
		angle = MathHelper.WrapAngle(angle);
		angle = 0f - (float)Math.Sin(angle);
		angle = MathHelper.Clamp(angle, -1f, 1f);
		m_Instance[m_Index].Play();
		m_Instance[m_Index].Pan = angle;
		m_Instance[m_Index].Volume = value;
		return m_Instance[m_Index];
	}

	public SoundEffectInstance Play3D(Vector3 pos, float vol)
	{
		m_Index++;
		if (m_Index >= 2)
		{
			m_Index = 0;
		}
		float num = (pos - g.m_CameraManager.m_Position).LengthSquared();
		float value = 1f - num / 1225f;
		value = MathHelper.Clamp(value, 0f, 1f);
		value *= value;
		if (value < 0.01f)
		{
			return null;
		}
		float angle = (float)Math.Atan2(g.m_CameraManager.m_Position.Z - pos.Z, g.m_CameraManager.m_Position.X - pos.X) + g.m_PlayerManager.GetLocalPlayer().m_Rotation.Y + MathF.PI / 2f;
		angle = MathHelper.WrapAngle(angle);
		angle = 0f - (float)Math.Sin(angle);
		angle = MathHelper.Clamp(angle, -1f, 1f);
		m_Instance[m_Index].Play();
		m_Instance[m_Index].Pan = angle;
		m_Instance[m_Index].Volume = value * vol;
		return m_Instance[m_Index];
	}

	private float RandPitch()
	{
		return (float)g.m_App.m_Rand.NextDouble() * 0.2f - 0.1f;
	}

	public SoundEffectInstance PlayLooped()
	{
		m_Index = 0;
		if (!m_Instance[m_Index].IsLooped)
		{
			m_Instance[m_Index].IsLooped = true;
		}
		m_Instance[m_Index].Resume();
		return m_Instance[m_Index];
	}

	public void StopLooped(SoundEffectInstance s)
	{
		for (int i = 0; i < 2; i++)
		{
			if (m_Instance[i] == s)
			{
				m_Instance[i].Stop();
			}
		}
	}

	public void Stop()
	{
		for (int i = 0; i < 2; i++)
		{
			m_Instance[i].Stop();
		}
	}
}
