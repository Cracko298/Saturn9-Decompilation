using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Saturn9;

public class SoundManager
{
	public enum SFX
	{
		Hit1,
		Hit2,
		step_right1,
		step_right2,
		step_right3,
		step_left1,
		step_left2,
		step_left3,
		Select,
		Back,
		Up,
		Down,
		Body2,
		Body3,
		Body4,
		Body5,
		CrateOpen,
		CrateClose,
		TextPrompt,
		SystemBeep,
		WarningBeep,
		HelmetWarning,
		Flatline,
		Alarm,
		Breathe,
		Locator,
		Ambience,
		Horror_enterroom,
		Horror_lookatblood,
		Door,
		Clue,
		Clatter,
		Ghosts,
		Phone,
		Keypad,
		Switch,
		Saw,
		ScreamMedbay,
		HeartBeat,
		TickTock,
		ClockChime,
		Ocean,
		Knock,
		Locker,
		Growl1,
		Growl2,
		Growl3,
		Ugh,
		CargoLoop,
		END
	}

	private float m_FootStepTime;

	public Sound[] m_Sound;

	public SoundManager()
	{
		m_Sound = new Sound[49];
		for (int i = 0; i < 49; i++)
		{
			m_Sound[i].m_Instance = new SoundEffectInstance[2];
		}
	}

	public SoundEffectInstance Play(int id)
	{
		return m_Sound[id].Play();
	}

	public SoundEffectInstance Play(int id, float vol)
	{
		return m_Sound[id].Play(vol);
	}

	public SoundEffectInstance Play(int id, float vol, float pitch)
	{
		return m_Sound[id].Play(vol, pitch);
	}

	public SoundEffectInstance Play3D(int id, Vector3 pos)
	{
		return m_Sound[id].Play3D(pos);
	}

	public SoundEffectInstance Play3D(int id, Vector3 pos, float vol)
	{
		return m_Sound[id].Play3D(pos, vol);
	}

	public SoundEffectInstance PlayLooped(int id)
	{
		return m_Sound[id].PlayLooped();
	}

	public void Add(int id, SoundEffect s)
	{
		m_Sound[id].Add(s);
	}

	public bool IsPlaying(int id)
	{
		for (int i = 0; i < 2; i++)
		{
			if (m_Sound[id].m_Instance[i].State == SoundState.Playing)
			{
				return true;
			}
		}
		return false;
	}

	public void Stop(int id)
	{
		m_Sound[id].Stop();
	}

	public void PlayLocalFootsteps(float bobY)
	{
		Vector3 vector = Vector3.Zero;
		if (g.m_PlayerManager.GetLocalPlayer().m_SceneObject != null)
		{
			vector = g.m_PlayerManager.GetLocalPlayer().m_SceneObject.World.Forward;
		}
		Vector3 pos = g.m_PlayerManager.GetLocalPlayer().m_Position + vector;
		float moveVol = g.m_PlayerManager.GetLocalPlayer().GetMoveVol();
		float num = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds;
		if (bobY > 0.95f && m_FootStepTime < num)
		{
			m_FootStepTime = num + 0.4f;
			int num2 = g.m_App.m_Rand.Next(6);
			Play3D(2 + num2, pos, moveVol);
		}
	}

	public void UpdateTrackedSound(SoundEffectInstance s, Vector3 pos)
	{
		float num = (pos - g.m_CameraManager.m_Position).LengthSquared();
		float value = 1f - num / 1225f;
		value = MathHelper.Clamp(value, 0f, 1f);
		value *= value;
		float angle = (float)Math.Atan2(g.m_CameraManager.m_Position.Z - pos.Z, g.m_CameraManager.m_Position.X - pos.X) + g.m_PlayerManager.GetLocalPlayer().m_Rotation.Y + MathF.PI / 2f;
		angle = MathHelper.WrapAngle(angle);
		angle = 0f - (float)Math.Sin(angle);
		angle = MathHelper.Clamp(angle, -1f, 1f);
		s.Pan = angle;
		s.Volume = value;
	}
}
