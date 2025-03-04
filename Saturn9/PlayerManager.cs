using System;
using System.Collections.ObjectModel;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysicsDemos.AlternateMovement.Character;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Net;
using SgMotion;

namespace Saturn9;

public class PlayerManager
{
	public const int MAX_PLAYERS = 16;

	public const float CHAR_HEIGHT = 4.318f;

	public const float CHAR_RADIUS = 1.016f;

	public const float CHAR_MASS = 10f;

	public const float CHAR_CROUCH_HEIGHT = 1.778f;

	public const float CHAR_JUMP_SPEED = 9f;

	public const float VAMPIRE_JUMP_SPEED = 20f;

	private const int NUM_TEAM_SPAWN_POINTS = 12;

	private const float SPAWN_RADIUS_SQ = 6.4515996f;

	private const float SHOW_TEAMNAME_DIST = 254f;

	private const float SHOW_NAME_DIST = 25.4f;

	private const float SHOW_NAME_MIN_X = 50f;

	private const float SHOW_NAME_MAX_X = 974f;

	private const float SHOW_NAME_MIN_Y = 50f;

	private const float SHOW_NAME_MAX_Y = 526f;

	public Player[] m_Player;

	public Player[] m_SortedPlayer;

	public Vector3 m_SpawnPos;

	public byte m_NextBotId;

	public CollisionGroup m_CylinderGroup;

	public CollisionGroup m_RagdollGroup;

	public SkinnedModel m_FullModel_VampM;

	public SkinnedModel m_FullModel_Alien;

	public SkinnedModel m_ViewModel;

	public bool RAGDOLLS_COLLIDE_WITH_PLAYERS;

	public Vector3 CHAR_STARTPOS = new Vector3(0f, 4.5f, 0f);

	public int m_Frame;

	public float m_CountdownTimer;

	public int m_SpacemanId = -1;

	public int m_MedBayAlienId = -1;

	public bool m_MedBayAlienShown;

	public int m_OxygenAlienId = -1;

	public bool m_OxygenAlienShown;

	public int m_CargoBayAlienId = -1;

	public bool m_CargoBayAlienShown;

	public float m_PhoneTimer;

	public float m_GameCompleteTimer;

	public bool m_bShowLifeformMessage;

	public bool m_bShowAirlockMessage;

	public int m_NumDeaths;

	public PlayerManager()
	{
		m_Player = new Player[16];
		m_SortedPlayer = new Player[16];
		for (int i = 0; i < 16; i++)
		{
			m_Player[i] = new Player();
		}
		for (int j = 0; j < 16; j++)
		{
			m_Player[j].m_Id = -1;
		}
		m_SpawnPos = Vector3.Zero;
		m_CylinderGroup = new CollisionGroup();
		m_RagdollGroup = new CollisionGroup();
		CollisionGroupPair key = new CollisionGroupPair(m_CylinderGroup, m_RagdollGroup);
		CollisionRules.CollisionGroupRules.Add(key, CollisionRule.NoBroadPhase);
		m_NextBotId = 0;
	}

	public void LoadModels()
	{
		m_FullModel_VampM = g.m_App.Content.Load<SkinnedModel>("Models/stagger");
		m_FullModel_Alien = g.m_App.Content.Load<SkinnedModel>("Models/alien");
		m_ViewModel = g.m_App.Content.Load<SkinnedModel>("Models/view");
	}

	public Player Create(short netId, bool bot, Player.TEAM botTeam)
	{
		for (int i = 0; i < 16; i++)
		{
			if (m_Player[i].m_Id != -1)
			{
				continue;
			}
			m_Player[i].m_Id = i;
			m_Player[i].m_NetId = netId;
			m_Player[i].m_Position = new Vector3(0f, 1000f, 0f);
			m_Player[i].m_Rotation.Y = 0f;
			m_Player[i].m_LAX = 0f;
			m_Player[i].m_LAY = 0f;
			m_Player[i].m_Health = 100;
			m_Player[i].m_bRequestDied = false;
			m_Player[i].m_bFired = false;
			m_Player[i].m_Bot = bot;
			m_Player[i].m_bTorchChanged = false;
			m_Player[i].m_bWeaponChanged = false;
			m_Player[i].m_Anim = byte.MaxValue;
			m_Player[i].m_ViewAnim = -1;
			m_Player[i].m_bRequestSendDamage = false;
			m_Player[i].m_RequestedDamageAmount = 0;
			m_Player[i].m_RequestedPlayerToDamageNetID = 255;
			m_Player[i].m_RequestedHitZone = byte.MaxValue;
			m_Player[i].m_RequestedAttacker = 255;
			m_Player[i].m_LastAttackerNetId = 255;
			m_Player[i].m_State = Player.STATE.JoinTeam;
			m_Player[i].m_Team = Player.TEAM.None;
			m_Player[i].m_Class = Player.CLASS.None;
			m_Player[i].m_NetworkPosition = m_Player[i].m_Position;
			m_Player[i].m_NetworkRotation = m_Player[i].m_Rotation.Y;
			m_Player[i].m_RespawnTimer = 0f;
			m_Player[i].m_Crouch = false;
			m_Player[i].m_RequestSendCrouch = false;
			m_Player[i].m_SpinePitch = 0f;
			m_Player[i].m_Kills = 0;
			m_Player[i].m_Deaths = 0;
			m_Player[i].m_Score = 0;
			m_Player[i].m_XP = 0;
			m_Player[i].m_Rank = 0;
			m_Player[i].m_XPForNextRank = 0;
			m_Player[i].m_RequestSendTeam = false;
			m_Player[i].m_RequestSendClass = false;
			m_Player[i].m_WeaponItemIndex = -1;
			m_Player[i].m_AnimChanged = false;
			m_Player[i].m_bStaked = false;
			m_Player[i].m_HasAmmoToGive = false;
			m_Player[i].m_RequestDelete = false;
			m_Player[i].m_RequestRankUp = false;
			m_Player[i].m_ScanningProgress = 0f;
			m_Player[i].m_ScanPressed = false;
			m_Player[i].m_SpawnId = -1;
			m_Player[i].m_AttachedItemId = -1;
			m_Player[i].m_ArtifactEscapeAirlockId = byte.MaxValue;
			m_Player[i].m_RequestCreateProjectile = false;
			m_Player[i].m_RequestCreateProjectileNetId = 255;
			m_Player[i].m_RequestCreateProjectileType = byte.MaxValue;
			m_Player[i].m_NextRecoverTime = 0f;
			m_Player[i].m_RequestSendImpactPos = Vector3.Zero;
			m_Player[i].m_RequestSendImpact = false;
			m_Player[i].m_RequestSendImpactNormal = Vector3.Zero;
			m_Player[i].m_PeerPitch = 0f;
			m_Player[i].m_HasLocatorDevice = false;
			m_Player[i].m_NextLocatorTime = 0f;
			m_Player[i].m_DrawLocator = false;
			m_Player[i].m_TrialCratesOpened = 0;
			m_Player[i].ClearUsedCrates();
			m_Player[i].m_CrouchY = 0f;
			m_Player[i].m_bRagdoll = false;
			m_Player[i].m_Hallucinate = false;
			m_Player[i].m_NumSaws = 0;
			m_Player[i].m_Sprint = 150;
			m_Player[i].m_bSprinting = false;
			bool flag = false;
			if (m_Player[i].IsLocalPlayer())
			{
				flag = true;
			}
			if (flag)
			{
				if (!bot)
				{
					m_Player[i].m_Rank = (short)g.m_App.m_Rank;
				}
				m_Player[i].m_XP = g.m_App.m_XP;
				m_Player[i].SetXPForNextLevel();
				m_Player[i].m_RequestSendScore = true;
			}
			if (!flag || bot || m_Player[i].UPDATEFULLMODEL_DEBUG)
			{
				m_Player[i].SetState(Player.STATE.InGame);
			}
			if (flag || (bot && GetLocalPlayer().IsHost()))
			{
				if (bot && m_Player[i].m_Team != Player.TEAM.CargoBay)
				{
					m_Player[i].m_CharacterController = new CharacterController(CHAR_STARTPOS, 4.318f, 1.778f, 0.254f, 10f);
				}
				else
				{
					m_Player[i].m_CharacterController = new CharacterController(CHAR_STARTPOS, 4.318f, 1.778f, 1.016f, 10f);
				}
				m_Player[i].m_CharacterController.Body.Tag = m_Player[i];
				m_Player[i].m_CharacterController.Body.Position = m_Player[i].m_Position;
				m_Player[i].m_CharacterController.JumpSpeed = 9f;
				m_Player[i].m_CharacterController.m_PlayerIdx = i;
				g.m_App.m_Space.Add(m_Player[i].m_CharacterController);
				if (!RAGDOLLS_COLLIDE_WITH_PLAYERS)
				{
					m_Player[i].m_CharacterController.Body.CollisionInformation.CollisionRules.Group = m_CylinderGroup;
				}
			}
			if (flag && !bot)
			{
				m_Player[i].DisableCollisionAndGravity();
			}
			if (!bot)
			{
				m_Player[i].InitTorchLight();
			}
			m_Player[i].InitParticleSystems();
			if (bot && GetLocalPlayer().IsHost())
			{
				m_Player[i].BotSetClassAndTeam(botTeam);
			}
			if (bot)
			{
				m_Player[i].InitBot();
			}
			m_Player[i].InitPlayerGameData();
			return m_Player[i];
		}
		return null;
	}

	public void Delete(int id)
	{
		m_Player[id].Reset();
		m_Player[id].Delete();
	}

	public void DeleteAll()
	{
		for (int i = 0; i < 16; i++)
		{
			m_Player[i].Reset();
			m_Player[i].Delete();
		}
		m_SpacemanId = -1;
		m_MedBayAlienId = -1;
		m_MedBayAlienShown = false;
		m_OxygenAlienId = -1;
		m_OxygenAlienShown = false;
		m_CargoBayAlienId = -1;
		m_CargoBayAlienShown = false;
		m_GameCompleteTimer = 0f;
		m_bShowLifeformMessage = false;
		m_bShowAirlockMessage = false;
		m_PhoneTimer = 0f;
		m_NumDeaths = 0;
	}

	public void DeleteAllBots()
	{
		for (int i = 0; i < 16; i++)
		{
			if (m_Player[i].m_Id != -1 && m_Player[i].m_Bot)
			{
				m_Player[i].Reset();
				m_Player[i].Delete();
			}
		}
	}

	public void Render()
	{
		for (int i = 0; i < 16; i++)
		{
			if (m_Player[i].m_Id != -1)
			{
				m_Player[i].Render();
			}
		}
	}

	public void Update()
	{
		m_Frame++;
		for (int i = 0; i < 16; i++)
		{
			if (m_Player[i].m_Id != -1)
			{
				m_Player[i].Update();
			}
		}
		if (m_PhoneTimer < (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds && GetLocalPlayer().m_PhoneSFX != null)
		{
			GetLocalPlayer().m_PhoneSFX.Stop();
		}
	}

	public void UpdatePrevousPositions()
	{
		for (int i = 0; i < 16; i++)
		{
			if (m_Player[i].m_Id != -1)
			{
				m_Player[i].m_PrevPosition = m_Player[i].m_Position;
			}
		}
	}

	public void ClearDamageRequests()
	{
		for (int i = 0; i < 16; i++)
		{
			if (m_Player[i].m_Id != -1)
			{
				m_Player[i].m_RequestedDamageAmount = 0;
			}
		}
	}

	public int GetPlayerExistsWithNetId(short netId)
	{
		for (int i = 0; i < 16; i++)
		{
			if (m_Player[i].m_Id != -1 && m_Player[i].m_NetId == netId)
			{
				return m_Player[i].m_Id;
			}
		}
		return -1;
	}

	public int NumPlayers()
	{
		int num = 0;
		for (int i = 0; i < 16; i++)
		{
			if (m_Player[i].m_Id != -1)
			{
				num++;
			}
		}
		return num;
	}

	public Player GetLocalPlayer()
	{
		if (g.m_App.m_NetworkSession != null)
		{
			if (((ReadOnlyCollection<LocalNetworkGamer>)(object)g.m_App.m_NetworkSession.LocalGamers).Count > 0)
			{
				for (int i = 0; i < 16; i++)
				{
					if (m_Player[i].m_Id != -1 && m_Player[i].m_NetId == ((NetworkGamer)((ReadOnlyCollection<LocalNetworkGamer>)(object)g.m_App.m_NetworkSession.LocalGamers)[0]).Id)
					{
						return m_Player[i];
					}
				}
			}
			return null;
		}
		if (m_Player[0].m_Id != -1)
		{
			return m_Player[0];
		}
		return null;
	}

	public short GetNextBotId()
	{
		m_NextBotId++;
		if (m_NextBotId > 250)
		{
			m_NextBotId = 0;
		}
		return (short)(m_NextBotId | 0x100);
	}

	public Player GetBot(short botId)
	{
		for (int i = 0; i < 16; i++)
		{
			if (m_Player[i].m_Bot && m_Player[i].m_NetId == botId && m_Player[i].m_Id != -1)
			{
				return m_Player[i];
			}
		}
		return null;
	}

	public bool BotExists(short botId)
	{
		for (int i = 0; i < 16; i++)
		{
			if (m_Player[i].m_Bot && m_Player[i].m_NetId == botId && m_Player[i].m_Id != -1)
			{
				return true;
			}
		}
		return false;
	}

	public int FindSpawnPos(int team, out Vector3 pos, out float rotY)
	{
		TriggerEntity obj = null;
		int num = g.m_App.m_Rand.Next(12);
		for (int i = num; i < 12; i++)
		{
			string name = $"Team{team + 1}_Spawn{i + 1}";
			if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && !PlayerNearPosFilterY(obj.World.Translation, 6.4515996f, 2f))
			{
				pos = obj.World.Translation;
				pos.Y += 0.85f;
				Vector3 forward = obj.World.Forward;
				rotY = (float)Math.Atan2(forward.X, forward.Z);
				return i + 1;
			}
		}
		for (int j = 0; j < num; j++)
		{
			string name = $"Team{team + 1}_Spawn{j + 1}";
			if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && !PlayerNearPosFilterY(obj.World.Translation, 6.4515996f, 2f))
			{
				pos = obj.World.Translation;
				pos.Y += 0.85f;
				Vector3 forward2 = obj.World.Forward;
				rotY = (float)Math.Atan2(forward2.X, forward2.Z);
				return j + 1;
			}
		}
		pos = new Vector3(0f, 2f, 0f);
		rotY = 0f;
		return -1;
	}

	public void FindCheckpointPos(int checkpointId, out Vector3 pos, out float rotY)
	{
		MiscTriggerEntity obj = null;
		string name = $"Checkpoint{checkpointId}";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
		{
			pos = obj.World.Translation;
			pos.Y += 0.85f;
			Vector3 forward = obj.World.Forward;
			rotY = (float)Math.Atan2(forward.X, forward.Z);
		}
		else
		{
			pos = Vector3.Zero;
			rotY = 0f;
		}
	}

	public void FindCargoSpawnPoint(out Vector3 pos, out float rotY)
	{
		TriggerEntity obj = null;
		_ = Vector3.Zero;
		float num = 1E+10f;
		float num2 = (1f + (float)m_NumDeaths) * (1f + (float)m_NumDeaths);
		bool flag = false;
		pos = Vector3.Zero;
		rotY = 0f;
		string name;
		for (int i = 0; i < 12; i++)
		{
			name = $"Team5_Spawn{i + 1}";
			if (!g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
			{
				continue;
			}
			float num3 = (GetLocalPlayer().m_Position - obj.World.Translation).LengthSquared();
			if (num3 > num2 && num3 < num)
			{
				Vector3 vector = Vector3.Normalize(GetLocalPlayer().m_Position - obj.World.Translation);
				float num4 = Vector3.Dot(GetLocalPlayer().m_ViewSceneObject.World.Forward, vector);
				if (num4 > -0.707f)
				{
					num = num3;
					pos = obj.World.Translation;
					flag = true;
				}
			}
		}
		if (flag)
		{
			pos.Y += 0.85f;
			Vector3 forward = obj.World.Forward;
			rotY = (float)Math.Atan2(forward.X, forward.Z);
			return;
		}
		name = $"Team5_Spawn1";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
		{
			pos = obj.World.Translation;
			pos.Y += 0.85f;
			Vector3 forward2 = obj.World.Forward;
			rotY = (float)Math.Atan2(forward2.X, forward2.Z);
		}
	}

	public bool PlayerNearPos(Vector3 pos, float radiusSq)
	{
		for (int i = 0; i < 16; i++)
		{
			if (m_Player[i].m_Id != -1)
			{
				float num = (pos - m_Player[i].m_Position).LengthSquared();
				if (num < radiusSq)
				{
					return true;
				}
			}
		}
		return false;
	}

	public int PlayerNearestToPos(Vector3 pos, float radiusSq)
	{
		int result = -1;
		float num = 100000000f;
		for (int i = 0; i < 16; i++)
		{
			if (m_Player[i].m_Id != -1)
			{
				float num2 = (pos - m_Player[i].m_Position).LengthSquared();
				if (num2 < radiusSq && num2 < num)
				{
					num = num2;
					result = i;
				}
			}
		}
		return result;
	}

	public int PlayerNearestToPosFilterY(Vector3 pos, float radiusSq, float YTol)
	{
		int result = -1;
		float num = 100000000f;
		for (int i = 0; i < 16; i++)
		{
			if (m_Player[i].m_Id != -1 && !(m_Player[i].m_Position.Y > pos.Y + YTol) && !(m_Player[i].m_Position.Y < pos.Y - YTol))
			{
				float num2 = (pos - m_Player[i].m_Position).LengthSquared();
				if (num2 < radiusSq && num2 < num)
				{
					num = num2;
					result = i;
				}
			}
		}
		return result;
	}

	public bool PlayerNearPosFilterY(Vector3 pos, float radiusSq, float YTol)
	{
		for (int i = 0; i < 16; i++)
		{
			if (m_Player[i].m_Id != -1 && !(m_Player[i].m_Position.Y > pos.Y + YTol) && !(m_Player[i].m_Position.Y < pos.Y - YTol))
			{
				float num = (pos - m_Player[i].m_Position).LengthSquared();
				if (num < radiusSq)
				{
					return true;
				}
			}
		}
		return false;
	}

	public Player GetPlayerNearMe(int id, float radiusSq)
	{
		Vector3 position = g.m_PlayerManager.m_Player[id].m_Position;
		for (int i = 0; i < 16; i++)
		{
			if (m_Player[i].m_Id != -1 && m_Player[i].m_Id != id)
			{
				float num = (position - m_Player[i].m_Position).LengthSquared();
				if (num < radiusSq)
				{
					return m_Player[i];
				}
			}
		}
		return null;
	}

	public void InitParticleSystems()
	{
		for (int i = 0; i < 16; i++)
		{
			m_Player[i].InitParticleSystems();
		}
	}

	public void DrawTeamMatesNames(SpriteBatch spriteBatch)
	{
		Vector3 zero = Vector3.Zero;
		float num = 0f;
		Vector3 zero2 = Vector3.Zero;
		if (g.m_PlayerManager.GetLocalPlayer() == null || g.m_PlayerManager.GetLocalPlayer().m_ViewSceneObject == null)
		{
			return;
		}
		for (int i = 0; i < 16; i++)
		{
			if (g.m_PlayerManager.m_Player[i].m_Id == -1 || g.m_PlayerManager.m_Player[i].m_Id == g.m_PlayerManager.GetLocalPlayer().m_Id || g.m_PlayerManager.m_Player[i].m_Bot)
			{
				continue;
			}
			zero = g.m_PlayerManager.m_Player[i].m_Position - g.m_PlayerManager.GetLocalPlayer().m_Position;
			float num2 = Vector3.Dot(g.m_PlayerManager.GetLocalPlayer().m_ViewSceneObject.World.Forward, zero);
			if (num2 < 0f)
			{
				continue;
			}
			num = zero.LengthSquared();
			if (num < 64516f)
			{
				zero2 = g.m_CameraManager.WorldToScreen(g.m_PlayerManager.m_Player[i].m_Position + new Vector3(0f, 0f, 0f));
				if (zero2.X > 50f && zero2.X < 974f && zero2.Y > 50f && zero2.Y < 526f)
				{
					spriteBatch.DrawString(g.m_App.hudFont, g.m_PlayerManager.m_Player[i].GetName(), new Vector2(zero2.X, zero2.Y), g.m_App.HUD_GREEN);
				}
			}
		}
	}

	public void DrawDeadPlayerNames(SpriteBatch spriteBatch)
	{
		Vector3 zero = Vector3.Zero;
		float num = 0f;
		Vector3 zero2 = Vector3.Zero;
		if (g.m_PlayerManager.GetLocalPlayer() == null || g.m_PlayerManager.GetLocalPlayer().m_ViewSceneObject == null)
		{
			return;
		}
		for (int i = 0; i < 16; i++)
		{
			if (g.m_PlayerManager.m_Player[i].m_Id == -1 || g.m_PlayerManager.m_Player[i].m_Id == g.m_PlayerManager.GetLocalPlayer().m_Id || !g.m_PlayerManager.m_Player[i].IsDead())
			{
				continue;
			}
			zero = g.m_PlayerManager.m_Player[i].m_Position - g.m_PlayerManager.GetLocalPlayer().m_Position;
			float num2 = Vector3.Dot(g.m_PlayerManager.GetLocalPlayer().m_ViewSceneObject.World.Forward, zero);
			if (num2 < 0f)
			{
				continue;
			}
			num = zero.LengthSquared();
			if (num < 645.16f)
			{
				zero2 = g.m_CameraManager.WorldToScreen(g.m_PlayerManager.m_Player[i].m_Position + new Vector3(0f, 0f, 0f));
				if (zero2.X > 50f && zero2.X < 974f && zero2.Y > 50f && zero2.Y < 526f)
				{
					spriteBatch.DrawString(g.m_App.hudFont, g.m_PlayerManager.m_Player[i].GetName(), new Vector2(zero2.X, zero2.Y), g.m_App.HUD_GREEN);
				}
			}
		}
	}

	public void SortPlayers()
	{
		for (int i = 0; i < 16; i++)
		{
			m_SortedPlayer[i] = m_Player[i];
		}
		bool flag = false;
		do
		{
			flag = false;
			for (int j = 1; j < 16; j++)
			{
				if (m_SortedPlayer[j - 1].m_Score < m_SortedPlayer[j].m_Score)
				{
					Player player = m_SortedPlayer[j - 1];
					m_SortedPlayer[j - 1] = m_SortedPlayer[j];
					m_SortedPlayer[j] = player;
					flag = true;
				}
				if (m_SortedPlayer[j - 1].m_Score == m_SortedPlayer[j].m_Score)
				{
					if (m_SortedPlayer[j - 1].m_Kills < m_SortedPlayer[j].m_Kills)
					{
						Player player2 = m_SortedPlayer[j - 1];
						m_SortedPlayer[j - 1] = m_SortedPlayer[j];
						m_SortedPlayer[j] = player2;
						flag = true;
					}
					if (m_SortedPlayer[j - 1].m_Kills == m_SortedPlayer[j].m_Kills && m_SortedPlayer[j - 1].m_Deaths > m_SortedPlayer[j].m_Deaths)
					{
						Player player3 = m_SortedPlayer[j - 1];
						m_SortedPlayer[j - 1] = m_SortedPlayer[j];
						m_SortedPlayer[j] = player3;
						flag = true;
					}
				}
			}
		}
		while (flag);
	}

	public void ReInitTorches()
	{
		for (int i = 0; i < 16; i++)
		{
			if (m_Player[i].m_Id != -1)
			{
				m_Player[i].InitTorchLight();
			}
		}
	}

	public void ReInitBots()
	{
		for (int i = 0; i < 16; i++)
		{
			if (m_Player[i].m_Id != -1 && m_Player[i].m_Bot)
			{
				m_Player[i].BotSetClassAndTeam(Player.TEAM.Hunter);
				m_Player[i].m_RequestSendTeam = true;
				m_Player[i].m_RequestSendClass = true;
				m_Player[i].InitBot();
			}
		}
	}

	public void ReInitPlayers()
	{
		for (int i = 0; i < 16; i++)
		{
			if (m_Player[i].m_Id != -1)
			{
				m_Player[i].CleanItems();
			}
		}
	}

	public int NumBots()
	{
		int num = 0;
		for (int i = 0; i < 16; i++)
		{
			if (m_Player[i].m_Id != -1 && m_Player[i].m_Bot && m_Player[i].m_Team != Player.TEAM.Vampire)
			{
				num++;
			}
		}
		return num;
	}

	public int NumPlayersOnTeams()
	{
		int num = 0;
		for (int i = 0; i < 16; i++)
		{
			if (m_Player[i].m_Id != -1 && !m_Player[i].m_Bot)
			{
				num++;
			}
		}
		return num;
	}

	public int NumSlayers()
	{
		int num = 0;
		for (int i = 0; i < 16; i++)
		{
			if (m_Player[i].m_Id != -1 && m_Player[i].m_Team == Player.TEAM.Hunter)
			{
				num++;
			}
		}
		return num;
	}

	public int NumVampires()
	{
		int num = 0;
		for (int i = 0; i < 16; i++)
		{
			if (m_Player[i].m_Id != -1 && m_Player[i].m_Team == Player.TEAM.Vampire)
			{
				num++;
			}
		}
		return num;
	}

	public void RemoveBot()
	{
		Player.TEAM tEAM = Player.TEAM.Hunter;
		for (int i = 0; i < 16; i++)
		{
			if (m_Player[i].m_Id != -1 && m_Player[i].m_Bot && m_Player[i].m_Team == tEAM)
			{
				m_Player[i].DeleteBot();
				break;
			}
		}
	}

	public bool AlienExists(Vector3 pos, out float distSq)
	{
		float num = 1E+10f;
		bool result = false;
		for (int i = 0; i < 16; i++)
		{
			if (m_Player[i].m_Id != -1 && m_Player[i].m_Bot && m_Player[i].m_Team == Player.TEAM.Vampire && !m_Player[i].m_bRagdoll)
			{
				Vector3 vector = m_Player[i].m_Position - pos;
				vector.Y *= 5f;
				distSq = vector.LengthSquared();
				if (distSq < num)
				{
					result = true;
					num = distSq;
				}
			}
		}
		distSq = num;
		return result;
	}

	public int FindPlayerWithArtifact()
	{
		for (int i = 0; i < 16; i++)
		{
			if (m_Player[i].m_Id != -1 && m_Player[i].m_AttachedItemId != -1 && m_Player[i].IsValid() && !m_Player[i].IsDead() && g.m_ItemManager.m_Item[m_Player[i].m_AttachedItemId].m_Id != -1 && g.m_ItemManager.m_Item[m_Player[i].m_AttachedItemId].m_SceneObject != null)
			{
				return i;
			}
		}
		return -1;
	}

	public bool IsAnyPlayerDead()
	{
		for (int i = 0; i < 16; i++)
		{
			if (m_Player[i].m_Id != -1 && !m_Player[i].m_Bot && m_Player[i].IsDead())
			{
				return true;
			}
		}
		return false;
	}

	public int GetTotalPlayerScore()
	{
		int num = 0;
		for (int i = 0; i < 16; i++)
		{
			if (m_Player[i].m_Id != -1 && !m_Player[i].m_Bot)
			{
				num += m_Player[i].m_Score;
			}
		}
		return num;
	}
}
