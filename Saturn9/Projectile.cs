using System.Collections.Generic;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SynapseGaming.LightingSystem.Core;
using SynapseGaming.LightingSystem.Lights;
using SynapseGaming.LightingSystem.Rendering;
using SynapseGaming.LightingSystem.Shadows;

namespace Saturn9;

public class Projectile
{
	public enum PROJECTILE_STATE
	{
		NONE,
		INFLIGHT,
		EXPLODING
	}

	public enum OBJ
	{
		RPG_PROJECTILE,
		END
	}

	public int m_Id;

	public short m_NetId;

	public Model m_Model;

	public SceneObject m_SceneObject;

	public int m_Type;

	public Player m_Player;

	public int m_VfxSystemIdx;

	public Entity m_Collision;

	public AnimatedQuadParticleSystem m_Explosion;

	public RPGShellTrailParticleSystem m_Trail;

	public PROJECTILE_STATE m_State;

	public float m_ExplodeTimer;

	public PointLight m_ProjectileLight;

	public PointLight m_ExplodeFlash;

	public int m_ExplodeFlashCount;

	private List<Player> m_PlayersToDamage;

	private List<byte> m_PlayerDamageValue;

	public Vector3 m_Velocity;

	public Projectile()
	{
		m_Type = -1;
		m_Id = -1;
		m_NetId = 255;
		m_Model = null;
		m_SceneObject = null;
		m_Player = null;
		m_VfxSystemIdx = 0;
		m_Collision = null;
		m_State = PROJECTILE_STATE.NONE;
		m_PlayersToDamage = new List<Player>();
		m_PlayerDamageValue = new List<byte>();
	}

	public void Update()
	{
		switch (m_State)
		{
		case PROJECTILE_STATE.INFLIGHT:
		{
			Vector3 position = m_Collision.Position;
			Matrix world = m_SceneObject.World;
			world.Translation = position;
			m_SceneObject.World = world;
			m_ProjectileLight.World = world;
			m_Trail.Emitter.PositionData.Position = position;
			CheckPlayerCollision();
			break;
		}
		case PROJECTILE_STATE.EXPLODING:
			if (m_ExplodeFlash.Enabled)
			{
				if (m_ExplodeFlashCount > 0)
				{
					m_ExplodeFlashCount--;
				}
				else
				{
					m_ExplodeFlash.Enabled = false;
				}
			}
			if (m_PlayersToDamage.Count > 0)
			{
				m_Player.RequestDamageOther(4, m_PlayerDamageValue[0], m_PlayersToDamage[0], m_NetId);
				m_PlayersToDamage.RemoveAt(0);
				m_PlayerDamageValue.RemoveAt(0);
			}
			if (m_ExplodeTimer < (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds)
			{
				Remove();
			}
			break;
		}
	}

	public void Explode(Vector3 position)
	{
		if (m_ExplodeTimer > (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds)
		{
			return;
		}
		m_State = PROJECTILE_STATE.EXPLODING;
		if (m_Player != null && (m_Player == g.m_PlayerManager.GetLocalPlayer() || (m_Player.IsHost() && m_Player.m_Bot)))
		{
			m_PlayersToDamage.Clear();
			for (int i = 0; i < 16; i++)
			{
				if (g.m_PlayerManager.m_Player[i].m_Id != -1 && !g.m_PlayerManager.m_Player[i].IsDead() && g.m_PlayerManager.m_Player[i].IsValid())
				{
					float num = (m_SceneObject.World.Translation - g.m_PlayerManager.m_Player[i].m_Position).LengthSquared();
					float value = 1f - num / 100f;
					value = MathHelper.Clamp(value, 0f, 1f);
					byte b = (byte)(value * 110f);
					if (b != 0)
					{
						m_PlayersToDamage.Add(g.m_PlayerManager.m_Player[i]);
						m_PlayerDamageValue.Add(b);
					}
				}
			}
		}
		m_ExplodeTimer = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 2f;
		if (m_SceneObject != null)
		{
			m_SceneObject.Visibility = ObjectVisibility.None;
		}
		if (m_Collision != null && m_Collision.Space != null)
		{
			g.m_App.m_Space.Remove(m_Collision);
		}
		m_Explosion.Enabled = true;
		m_Explosion.Visible = true;
		m_Explosion.Emitter.BurstTime = 0.5f;
		m_Explosion.Emitter.PositionData.Position = position;
		m_Explosion.LerpEmittersPositionAndOrientationOnNextUpdate = false;
		m_Trail.Enabled = false;
		m_Trail.Visible = false;
		if (m_ProjectileLight != null)
		{
			m_ProjectileLight.Enabled = false;
		}
		StartExplodeFlash();
	}

	public void Remove()
	{
		m_Id = -1;
		m_NetId = 255;
		DisableVFX();
		if (m_SceneObject != null)
		{
			m_SceneObject.Visibility = ObjectVisibility.None;
		}
		if (m_Collision != null && m_Collision.Space != null)
		{
			g.m_App.m_Space.Remove(m_Collision);
		}
		if (m_ProjectileLight != null)
		{
			m_ProjectileLight.Enabled = false;
		}
	}

	public void Delete()
	{
		m_Id = -1;
		m_NetId = 255;
		DisableVFX();
		if (m_SceneObject != null)
		{
			g.m_App.sceneInterface.ObjectManager.Remove(m_SceneObject);
			m_SceneObject = null;
		}
		if (m_Collision != null && m_Collision.Space != null)
		{
			g.m_App.m_Space.Remove(m_Collision);
			m_Collision = null;
		}
		if (m_ProjectileLight != null)
		{
			g.m_App.sceneInterface.LightManager.Remove(m_ProjectileLight);
			m_ProjectileLight = null;
		}
	}

	public void StartExplodeFlash()
	{
		m_ExplodeFlash.Position = m_SceneObject.World.Translation;
		m_ExplodeFlash.Enabled = true;
		m_ExplodeFlashCount = 6;
	}

	public void InitProjectileLight()
	{
		m_ProjectileLight = new PointLight();
		m_ProjectileLight.LightingType = LightingType.RealTime;
		m_ProjectileLight.DiffuseColor = new Vector3(1f, 1f, 0.7f);
		m_ProjectileLight.Intensity = 3f;
		m_ProjectileLight.Radius = 5f;
		m_ProjectileLight.FalloffStrength = 1f;
		m_ProjectileLight.Enabled = true;
		m_ProjectileLight.ShadowType = ShadowType.None;
		m_ProjectileLight.UpdateType = UpdateType.Automatic;
		g.m_App.sceneInterface.LightManager.Submit(m_ProjectileLight);
	}

	public void InitExplodeFlash()
	{
		m_ExplodeFlash = new PointLight();
		m_ExplodeFlash.LightingType = LightingType.RealTime;
		m_ExplodeFlash.DiffuseColor = new Vector3(1f, 1f, 0.7f);
		m_ExplodeFlash.Intensity = 10f;
		m_ExplodeFlash.Radius = 30f;
		m_ExplodeFlash.FalloffStrength = 1f;
		m_ExplodeFlash.Enabled = false;
		m_ExplodeFlash.ShadowType = ShadowType.None;
		m_ExplodeFlash.UpdateType = UpdateType.Automatic;
		g.m_App.sceneInterface.LightManager.Submit(m_ExplodeFlash);
		m_ExplodeFlashCount = 0;
	}

	public void InitTrail()
	{
		m_Trail.Enabled = true;
		m_Trail.Visible = true;
		m_Trail.Emitter.PositionData.Position = m_SceneObject.World.Translation;
		m_Trail.Emitter.PositionData.Velocity = new Vector3(0f, 0f, 0f);
		m_Trail.LerpEmittersPositionAndOrientationOnNextUpdate = false;
	}

	public void Show()
	{
		if (m_SceneObject != null)
		{
			m_SceneObject.Visibility = ObjectVisibility.Rendered;
		}
	}

	public void Hide()
	{
		if (m_SceneObject != null)
		{
			m_SceneObject.Visibility = ObjectVisibility.None;
		}
		DisableVFX();
	}

	private void DisableVFX()
	{
		if (m_Explosion != null)
		{
			m_Explosion.Enabled = false;
			m_Explosion.Visible = false;
		}
		if (m_Trail != null)
		{
			m_Trail.Enabled = false;
			m_Trail.Visible = false;
		}
	}

	private void CheckPlayerCollision()
	{
		int num = g.m_PlayerManager.PlayerNearestToPos(m_SceneObject.World.Translation, 4f);
		if (num != -1 && g.m_PlayerManager.m_Player[num].m_Id != -1 && g.m_PlayerManager.m_Player[num].IsValid() && !g.m_PlayerManager.m_Player[num].IsDead())
		{
			Explode(m_SceneObject.World.Translation);
		}
	}
}
