using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.DataStructures;
using BEPUphysics.MathExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using SynapseGaming.LightingSystem.Core;
using SynapseGaming.LightingSystem.Lights;
using SynapseGaming.LightingSystem.Rendering;

namespace Saturn9;

public class GameplayScreen : GameScreen
{
	public enum ARTIFACTSTATE
	{
		NONE,
		INCRATE,
		ATTACHED,
		DROPPED
	}

	private const float LEVEL_TIME = 1000000f;

	private const int updatesBetweenWorldDataSend = 30;

	private const int updatesBetweenStatusPackets = 4;

	private const int updatesBetweenBotPackets = 4;

	private const int NUM_ALARM_LIGHTS = 1;

	public const float UADOCKTIME = 15f;

	private NetworkSession networkSession;

	private PacketWriter packetWriter = new PacketWriter();

	private PacketReader packetReader = new PacketReader();

	private int updatesSinceStatusPacket;

	private int updatesSinceBotPacket = 2;

	private FlickerLight m_FLickerLight1;

	private FlickerLight m_FLickerLight2;

	private FlickerLight m_FLickerLight3;

	private FlickerLight m_FLickerLight4;

	private FlickerLight m_FLickerLight5;

	private FlickerLight m_FLickerLight6;

	private FlickerLight m_FLickerLight7;

	private FlickerLight m_FLickerLight8;

	private FlickerLight m_FLickerLight9;

	public SpotLight[] m_AlarmLight;

	public SpotLight m_LCDRedLight;

	public AmbientLight m_AmbientLight;

	private ContentManager content;

	private Vector2 playerPosition = new Vector2(100f, 100f);

	private Vector2 enemyPosition = new Vector2(100f, 100f);

	private Random random = new Random();

	private bool jump;

	private bool leap;

	private Vector2 movement;

	private float turn;

	private float pauseAlpha;

	private GamePadState m_GamepadState;

	private GamePadState m_OldGamepadState;

	private KeyboardState m_KeyboardState;

	private KeyboardState m_OldKeyboardState;

	private Stopwatch m_StopWatchUpdate;

	private Stopwatch m_StopWatchRender;

	private Stopwatch m_StopWatchPhysics;

	private Stopwatch m_StopWatchPlayers;

	private Stopwatch m_StopWatchItems;

	private Stopwatch m_StopWatchNetwork;

	private Stopwatch m_StopWatchScene;

	private Stopwatch m_StopWatchUpdateParticles;

	private float m_DebugStatUpdateTime;

	private float m_ParticleTime;

	private float m_SceneTime;

	private float m_NetworkTime;

	private float m_ItemTime;

	private float m_PhysicsTime;

	private float m_PlayerTime;

	private float m_UpdateTime;

	private float m_RenderTime;

	private double FPSlastTime;

	private double FPStotalSinceLast;

	private double FPStoDisplay;

	private int FPStotalFramesSinceLast;

	private Model skybox;

	private Matrix skyboxWorld;

	private float m_LevelTime;

	public StaticMesh m_CollisionMesh;

	private float m_BalanceBotTime;

	private bool m_HostSendUseCrateAllow;

	private short m_HostSendUseCrateId = -1;

	private short m_HostSendUsePlayerNetId = -1;

	private bool m_HostSendCrateItemToPeer;

	private byte m_HostSendCrateItemType = byte.MaxValue;

	private short m_HostSendCrateItemNetId = -1;

	private bool m_HostSendDetachItem;

	private short m_HostSendDetachPlayerNetId = -1;

	private Vector3 m_HostSendDetachItemPos = Vector3.Zero;

	private bool m_HostSendArtifactReturnedMessage;

	private bool m_HostSendDeleteNetworkItem;

	private byte m_HostSendDeleteNetworkItemId = byte.MaxValue;

	private float m_HostEscapeCountDownTimer;

	private bool m_HostEscapeCountDownStarted;

	private bool m_HostSendEscapeCountdownMessage;

	private bool m_HostSendCancelCountdownMessage;

	private bool m_HostSendGiveScore;

	private short m_HostSendGiveScorePlayerNetId = -1;

	private byte m_HostSendGiveScoreAmount;

	private bool m_HostSendArtifactState;

	private bool m_HostSendCrateSeed;

	public int m_HostCrateSeedValue;

	public float m_NextUnauthorisedDocking = 15f;

	private AirlockSmokeParticleSystem m_AirlockSmoke;

	private float m_StartAirlockSmokeTime;

	private Vector3 m_AirlockSmokeDir = Vector3.Zero;

	private Vector3 m_AirlockSmokePos = Vector3.Zero;

	public ARTIFACTSTATE m_HostArtifactState;

	private float m_HostDroppedArtifactTimeout;

	private bool IsNetActive
	{
		get
		{
			if (networkSession == null)
			{
				return base.IsActive;
			}
			return !base.IsExiting;
		}
	}

	public GameplayScreen(NetworkSession networkSession)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		this.networkSession = networkSession;
		base.TransitionOnTime = TimeSpan.FromSeconds(0.05);
		base.TransitionOffTime = TimeSpan.FromSeconds(0.05);
		if (Gamer.SignedInGamers[g.m_App.m_PlayerOnePadId] != null)
		{
			if (networkSession != null)
			{
				Gamer.SignedInGamers[g.m_App.m_PlayerOnePadId].Presence.PresenceMode = (GamerPresenceMode)2;
			}
			else
			{
				Gamer.SignedInGamers[g.m_App.m_PlayerOnePadId].Presence.PresenceMode = (GamerPresenceMode)1;
			}
		}
		m_StopWatchUpdate = new Stopwatch();
		m_StopWatchRender = new Stopwatch();
		m_StopWatchPhysics = new Stopwatch();
		m_StopWatchPlayers = new Stopwatch();
		m_StopWatchItems = new Stopwatch();
		m_StopWatchNetwork = new Stopwatch();
		m_StopWatchScene = new Stopwatch();
		m_StopWatchUpdateParticles = new Stopwatch();
		m_AlarmLight = new SpotLight[1];
		if (m_AirlockSmoke == null)
		{
			m_AirlockSmoke = new AirlockSmokeParticleSystem(g.m_App);
			m_AirlockSmoke.AutoInitialize(g.m_App.GraphicsDevice, g.m_App.Content, g.m_App.screenManager.SpriteBatch);
			g.m_App.m_ParticleSystemManager.AddParticleSystem(m_AirlockSmoke);
		}
		m_AirlockSmoke.Enabled = false;
		m_AirlockSmoke.Visible = false;
		if (g.m_App.m_SurvivalMode)
		{
			m_NextUnauthorisedDocking = 3f;
		}
		else
		{
			m_NextUnauthorisedDocking = 15f;
		}
	}

	public override void LoadContent()
	{
		if (content == null)
		{
			content = new ContentManager(base.ScreenManager.Game.Services, "Content");
		}
		if (g.m_PlayerManager.GetLocalPlayer().IsHost())
		{
			HostReset();
			LoadSunburnScene();
		}
		base.ScreenManager.Game.ResetElapsedTime();
		g.m_CameraManager.Init();
		g.m_App.m_GameplayScreen = this;
	}

	private void HostReset()
	{
		m_HostSendCrateItemNetId = -1;
		m_HostSendCrateItemToPeer = false;
		m_HostSendCrateItemType = byte.MaxValue;
		m_HostSendDetachItem = false;
		m_HostSendDetachItemPos = Vector3.Zero;
		m_HostSendDetachPlayerNetId = -1;
		m_HostSendUseCrateAllow = false;
		m_HostSendUseCrateId = -1;
		m_HostSendUsePlayerNetId = -1;
		m_HostDroppedArtifactTimeout = 0f;
		SetHostArtifactState(ARTIFACTSTATE.NONE);
		m_HostSendDeleteNetworkItem = false;
		m_HostSendDeleteNetworkItemId = byte.MaxValue;
		m_HostEscapeCountDownTimer = 0f;
		m_HostSendEscapeCountdownMessage = false;
		m_HostEscapeCountDownStarted = false;
		m_HostSendArtifactReturnedMessage = false;
		m_HostSendCancelCountdownMessage = false;
		m_HostSendArtifactState = false;
		m_HostSendCrateSeed = false;
		m_HostCrateSeedValue = 0;
	}

	public void LoadSunburnScene()
	{
		g.m_App.scene = g.m_App.m_Scene1;
		g.m_App.ResetTriggers();
		g.m_App.sceneInterface.Submit(g.m_App.scene);
		g.m_App.environment = g.m_App.m_Environment1;
		SceneObject sceneObject = g.m_App.sceneInterface.ObjectManager.Find<SceneObject>("Box01", onlysearchdynamicobjects: false);
		Vector3 translation = sceneObject.World.Translation;
		g.m_App.m_LCD1 = g.m_App.sceneInterface.ObjectManager.Find<SceneObject>("LCD1", onlysearchdynamicobjects: false);
		g.m_App.m_LCD2 = g.m_App.sceneInterface.ObjectManager.Find<SceneObject>("LCD2", onlysearchdynamicobjects: false);
		g.m_App.m_GRAVE = g.m_App.sceneInterface.ObjectManager.Find<SceneObject>("GRAVE", onlysearchdynamicobjects: false);
		TriangleMesh.GetVerticesAndIndicesFromModel(g.m_App.m_CollisionModel1, out var vertices, out var indices);
		m_CollisionMesh = new StaticMesh(vertices, indices, new AffineTransform(translation));
		g.m_App.m_Space.Add(m_CollisionMesh);
		if (g.m_CameraManager.m_Helmet == -1)
		{
			g.m_CameraManager.m_Helmet = g.m_ItemManager.Create(7, byte.MaxValue, Vector3.Zero, 0f, g.m_PlayerManager.GetLocalPlayer());
		}
		g.m_CameraManager.Init();
		g.m_CameraManager.Update();
		g.m_App.m_RumbleFrames = 0;
		SetUpFlickerLights();
		SetupAlarmLights();
		if (networkSession == null || networkSession.IsHost)
		{
			g.m_ItemManager.SetUpTriggeredItems();
			g.m_ItemManager.SetUpFog();
		}
	}

	public void SetUpFlickerLights()
	{
	}

	public void UpdateFlickerLights()
	{
	}

	public void SetupAlarmLights()
	{
		for (int i = 0; i < 1; i++)
		{
			g.m_App.sceneInterface.LightManager.Find<SpotLight>($"AlarmLight{i + 1}", onlysearchdynamicobjects: false, out m_AlarmLight[i]);
			if (m_AlarmLight[i] != null)
			{
				m_AlarmLight[i].Enabled = false;
			}
		}
		g.m_App.sceneInterface.LightManager.Find<SpotLight>("AlarmPoint1", onlysearchdynamicobjects: false, out m_LCDRedLight);
		g.m_App.sceneInterface.LightManager.Find<AmbientLight>("Ambient", onlysearchdynamicobjects: false, out m_AmbientLight);
		SetBrightness(g.m_App.m_OptionsBrightness);
	}

	public void SetBrightness(int brightness)
	{
		if (m_AmbientLight != null)
		{
			m_AmbientLight.Intensity = 0.15f + 0.04f * (float)brightness;
		}
		if (g.m_PlayerManager.GetLocalPlayer() != null && g.m_PlayerManager.GetLocalPlayer().m_TorchLight != null)
		{
			g.m_PlayerManager.GetLocalPlayer().m_TorchLight.Intensity = 2.5f + (float)brightness * 0.1f;
		}
	}

	public void UpdateAlarmLights()
	{
		if (g.m_App == null || g.m_App.scene == null || m_AlarmLight == null || !g.m_App.m_OptionsLights)
		{
			return;
		}
		for (int i = 0; i < 1; i++)
		{
			if (m_AlarmLight[i] != null && m_AlarmLight[i].Enabled)
			{
				Vector3 direction = m_AlarmLight[i].Direction;
				Matrix matrix = Matrix.CreateRotationY((float)g.m_App.m_GameTime.ElapsedGameTime.TotalSeconds * MathHelper.ToRadians(960.12f));
				direction = Vector3.Transform(direction, matrix);
				m_AlarmLight[i].Direction = direction;
				m_AlarmLight[i].Enabled = true;
			}
		}
	}

	public override void UnloadContent()
	{
		if (m_CollisionMesh != null)
		{
			g.m_App.m_Space.Remove(m_CollisionMesh);
			m_CollisionMesh = null;
		}
		if (g.m_App.m_AlarmSFX != null)
		{
			g.m_App.m_AlarmSFX.Stop();
			g.m_App.m_AlarmSFX = null;
		}
		if (g.m_App.m_CargoLoopSFX != null)
		{
			g.m_App.m_CargoLoopSFX.Stop();
			g.m_App.m_CargoLoopSFX = null;
		}
		if (g.m_PlayerManager.GetLocalPlayer() != null && g.m_PlayerManager.GetLocalPlayer().m_BreatheSFX != null)
		{
			g.m_PlayerManager.GetLocalPlayer().m_BreatheSFX.Stop();
			g.m_PlayerManager.GetLocalPlayer().m_BreatheSFX = null;
		}
		if (g.m_PlayerManager.GetLocalPlayer() != null && g.m_PlayerManager.GetLocalPlayer().m_PhoneSFX != null)
		{
			g.m_PlayerManager.GetLocalPlayer().m_PhoneSFX.Stop();
			g.m_PlayerManager.GetLocalPlayer().m_PhoneSFX = null;
		}
		content.Unload();
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen: false);
		if (coveredByOtherScreen)
		{
			pauseAlpha = Math.Min(pauseAlpha + 1f / 32f, 1f);
		}
		else
		{
			pauseAlpha = Math.Max(pauseAlpha - 1f / 32f, 0f);
		}
		m_StopWatchUpdate.Start();
		if (!g.m_App.m_Paused || g.m_App.m_SingleStep)
		{
			g.m_App.m_SingleStep = false;
			if (IsNetActive && g.m_App.m_Intermission)
			{
				UpdateNetworking((float)gameTime.ElapsedGameTime.TotalSeconds);
			}
			if (IsNetActive && !g.m_App.m_Intermission)
			{
				g.m_App.sceneInterface.Update(gameTime);
				g.m_App.m_ParticleSystemManager.UpdateAllParticleSystems((float)gameTime.ElapsedGameTime.TotalSeconds);
				UpdateNetworking((float)gameTime.ElapsedGameTime.TotalSeconds);
				g.m_PlayerManager.ClearDamageRequests();
				g.m_PlayerManager.Update();
				g.m_ItemManager.Update();
				g.m_ProjectileManager.Update();
				g.m_CameraManager.Update();
				g.m_BotPathManager.Update();
				g.m_PlayerManager.UpdatePrevousPositions();
				m_StopWatchPhysics.Start();
				g.m_App.m_Space.Update();
				m_StopWatchPhysics.Stop();
				UpdateFlickerLights();
				UpdateAlarmLights();
			}
			if (g.m_PlayerManager.m_GameCompleteTimer != 0f && g.m_PlayerManager.m_GameCompleteTimer < (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds)
			{
				LoadingScreen.Load(base.ScreenManager, true, null, 0, new GameCompleteScreen());
			}
			if (networkSession != null && !base.IsExiting && (int)networkSession.SessionState == 0)
			{
				LoadingScreen.Load(base.ScreenManager, true, null, 0, new BackgroundScreen(), new LobbyScreen(networkSession));
			}
			if (networkSession == null || networkSession.IsHost)
			{
				_ = g.m_App.m_Intermission;
			}
			PeerCheckAirlockSmoke();
			if ((networkSession == null || networkSession.IsHost) && g.m_App.m_Intermission && g.m_App.m_IntermissionTime < (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds)
			{
				EndIntermission();
			}
			m_StopWatchUpdate.Stop();
			if (networkSession != null)
			{
				_ = networkSession.IsHost;
			}
		}
	}

	private void CheckArtifactDrop()
	{
		for (int i = 0; i < 16; i++)
		{
			if (g.m_PlayerManager.m_Player[i].m_Id != -1 && g.m_PlayerManager.m_Player[i].m_AttachedItemId != -1 && g.m_PlayerManager.m_Player[i].IsDead())
			{
				HostDropArtifactForPlayer(g.m_PlayerManager.m_Player[i]);
				g.m_PlayerManager.m_Player[i].PeerDetach();
			}
		}
	}

	public void HostDropArtifactForPlayer(Player playerData)
	{
		m_HostSendDetachItem = true;
		m_HostSendDetachPlayerNetId = playerData.m_NetId;
		m_HostSendDetachItemPos = playerData.m_PrevPosition;
		m_HostDroppedArtifactTimeout = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 30f;
		SetHostArtifactState(ARTIFACTSTATE.DROPPED);
		g.m_ItemManager.m_Item[playerData.m_AttachedItemId].NetItemSetPosition(playerData.m_Position);
	}

	private void CheckGameOver()
	{
		if (g.m_PlayerManager.IsAnyPlayerDead())
		{
			if (g.m_PlayerManager.GetLocalPlayer().m_Score > g.m_App.m_SurvivalScoreBest)
			{
				g.m_App.m_SurvivalScoreBest = g.m_PlayerManager.GetLocalPlayer().m_Score;
			}
			GoToIntermission();
		}
	}

	private float DistancePointLine(Vector3 Point, Vector3 LineStart, Vector3 LineEnd)
	{
		Vector3 zero = Vector3.Zero;
		float num = (LineEnd - LineStart).Length();
		float num2 = ((Point.X - LineStart.X) * (LineEnd.X - LineStart.X) + (Point.Y - LineStart.Y) * (LineEnd.Y - LineStart.Y) + (Point.Z - LineStart.Z) * (LineEnd.Z - LineStart.Z)) / (num * num);
		if (num2 < 0f || num2 > 1f)
		{
			return 1000000f;
		}
		zero.X = LineStart.X + num2 * (LineEnd.X - LineStart.X);
		zero.Y = LineStart.Y + num2 * (LineEnd.Y - LineStart.Y);
		zero.Z = LineStart.Z + num2 * (LineEnd.Z - LineStart.Z);
		return (Point - zero).Length();
	}

	private void PeerCheckAirlockSmoke()
	{
		if (m_StartAirlockSmokeTime < (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds && !m_AirlockSmoke.Enabled && m_HostEscapeCountDownStarted)
		{
			m_AirlockSmoke.Emitter.PositionData.Position = m_AirlockSmokePos;
			m_AirlockSmoke.m_ExplosionDir = m_AirlockSmokeDir;
			m_AirlockSmoke.Emitter.PositionData.Velocity = new Vector3(0f, 0f, 0f);
			m_AirlockSmoke.LerpEmittersPositionAndOrientationOnNextUpdate = false;
			m_AirlockSmoke.Emitter.BurstTime = 3f;
			m_AirlockSmoke.Enabled = true;
			m_AirlockSmoke.Visible = true;
		}
	}

	private void SetHostArtifactState(ARTIFACTSTATE state)
	{
		m_HostArtifactState = state;
		m_HostSendArtifactState = true;
	}

	private void PeerSetHostArtifactState(ARTIFACTSTATE state)
	{
		m_HostArtifactState = state;
	}

	private void GoToIntermission()
	{
		float num = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds;
		g.m_App.m_RequestIntermission = true;
		g.m_App.m_Intermission = true;
		g.m_App.m_IntermissionTime = num + 3f;
		m_BalanceBotTime = num + 30f;
		HostReset();
		if (g.m_App.m_AlarmSFX != null)
		{
			g.m_App.m_AlarmSFX.Stop();
			g.m_App.m_AlarmSFX = null;
		}
		if (g.m_App.m_CargoLoopSFX != null)
		{
			g.m_App.m_CargoLoopSFX.Stop();
			g.m_App.m_CargoLoopSFX = null;
		}
		if (g.m_PlayerManager.GetLocalPlayer().m_BreatheSFX != null)
		{
			g.m_PlayerManager.GetLocalPlayer().m_BreatheSFX.Stop();
			g.m_PlayerManager.GetLocalPlayer().m_BreatheSFX = null;
		}
		if (g.m_PlayerManager.GetLocalPlayer().m_PhoneSFX != null)
		{
			g.m_PlayerManager.GetLocalPlayer().m_PhoneSFX.Stop();
			g.m_PlayerManager.GetLocalPlayer().m_PhoneSFX = null;
		}
		m_HostEscapeCountDownStarted = false;
		m_HostEscapeCountDownTimer = 0f;
		m_AirlockSmoke.Enabled = false;
		m_AirlockSmoke.Visible = false;
	}

	public void EndIntermission()
	{
		g.m_App.m_RequestIntermissionEnd = true;
		PeerEndIntermission();
	}

	private void PeerEndIntermission()
	{
		g.m_App.m_Space.Remove(m_CollisionMesh);
		m_CollisionMesh = null;
		g.m_App.sceneInterface.Clear();
		g.m_ItemManager.DeleteAll();
		if (g.m_CameraManager.m_Helmet != -1)
		{
			g.m_ItemManager.Delete(g.m_CameraManager.m_Helmet);
			g.m_CameraManager.m_Helmet = -1;
		}
		g.m_PlayerManager.ReInitPlayers();
		LoadSunburnScene();
		g.m_BotPathManager.LoadBotPath();
		base.ScreenManager.Game.ResetElapsedTime();
		g.m_CameraManager.Init();
		g.m_PlayerManager.ReInitTorches();
		if (g.m_PlayerManager.GetLocalPlayer().IsHost())
		{
			g.m_PlayerManager.ReInitBots();
		}
		GameScreen[] screens = g.m_App.screenManager.GetScreens();
		for (int i = 0; i < screens.Length; i++)
		{
			if (screens[i] is JoinTeamMenuScreen)
			{
				screens[i].ScreenManager.RemoveScreen(screens[i]);
			}
		}
		g.m_PlayerManager.GetLocalPlayer().m_State = Player.STATE.JoinTeam;
		if (g.m_App.m_AlarmSFX != null)
		{
			g.m_App.m_AlarmSFX.Stop();
			g.m_App.m_AlarmSFX = null;
		}
		if (g.m_App.m_CargoLoopSFX != null)
		{
			g.m_App.m_CargoLoopSFX.Stop();
			g.m_App.m_CargoLoopSFX = null;
		}
		g.m_App.m_Intermission = false;
		g.m_App.m_ForceJoin = true;
		GC.Collect();
	}

	private void PeerGoToIntermission()
	{
		g.m_App.m_Intermission = true;
		g.m_App.m_IntermissionTime = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 3f;
		if (g.m_App.m_AlarmSFX != null)
		{
			g.m_App.m_AlarmSFX.Stop();
			g.m_App.m_AlarmSFX = null;
		}
		if (g.m_App.m_CargoLoopSFX != null)
		{
			g.m_App.m_CargoLoopSFX.Stop();
			g.m_App.m_CargoLoopSFX = null;
		}
		if (g.m_PlayerManager.GetLocalPlayer().m_BreatheSFX != null)
		{
			g.m_PlayerManager.GetLocalPlayer().m_BreatheSFX.Stop();
			g.m_PlayerManager.GetLocalPlayer().m_BreatheSFX = null;
		}
		if (g.m_PlayerManager.GetLocalPlayer().m_PhoneSFX != null)
		{
			g.m_PlayerManager.GetLocalPlayer().m_PhoneSFX.Stop();
			g.m_PlayerManager.GetLocalPlayer().m_PhoneSFX = null;
		}
		m_HostEscapeCountDownStarted = false;
		m_HostEscapeCountDownTimer = 0f;
		m_AirlockSmoke.Enabled = false;
		m_AirlockSmoke.Visible = false;
	}

	private void UpdateBotSpawner()
	{
		if (g.m_PlayerManager.NumBots() > 5 && !g.m_App.m_SurvivalMode)
		{
			g.m_PlayerManager.RemoveBot();
		}
		bool flag = false;
		float num = 15f;
		int num2 = 5;
		if (g.m_App.m_SurvivalMode)
		{
			flag = true;
			num = 3f;
			num2 = 8;
		}
		if (g.m_PlayerManager.GetLocalPlayer() == null || g.m_PlayerManager.GetLocalPlayer().m_State != Player.STATE.InGame || g.m_PlayerManager.GetLocalPlayer().m_Team == Player.TEAM.None)
		{
			return;
		}
		if (g.m_App.m_NetworkSession != null)
		{
			if (g.m_App.m_OptionsBotsMP > 0 || flag)
			{
				int num3 = g.m_App.m_OptionsBotsMP - (g.m_PlayerManager.NumPlayersOnTeams() - 1);
				if (flag)
				{
					num3 = num2;
				}
				if (g.m_PlayerManager.NumBots() < num3)
				{
					g.m_BotPathManager.LoadBotPath();
					SendCreateBot(Player.TEAM.Hunter);
				}
				else if (g.m_PlayerManager.NumBots() > num3)
				{
					g.m_PlayerManager.RemoveBot();
				}
			}
			return;
		}
		if (m_NextUnauthorisedDocking < (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds && (g.m_PlayerManager.NumBots() < g.m_App.m_ArtifactsFoundSP + 2 || flag) && g.m_PlayerManager.NumBots() < num2)
		{
			g.m_BotPathManager.LoadBotPath();
			g.m_PlayerManager.Create(g.m_PlayerManager.GetNextBotId(), bot: true, Player.TEAM.Hunter);
			m_NextUnauthorisedDocking = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + num;
		}
		int num4 = 2;
		if (g.m_App.m_SurvivalMode)
		{
			num4 = num2;
		}
		if (Guide.IsTrialMode && m_NextUnauthorisedDocking < (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds && g.m_PlayerManager.NumBots() < num4)
		{
			g.m_BotPathManager.LoadBotPath();
			g.m_PlayerManager.Create(g.m_PlayerManager.GetNextBotId(), bot: true, Player.TEAM.Hunter);
			m_NextUnauthorisedDocking = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + num;
		}
	}

	public override void HandleInput(InputState input)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		if (base.ControllingPlayer.HasValue)
		{
			HandlePlayerInput(input, base.ControllingPlayer.Value);
		}
		else
		{
			if (networkSession == null)
			{
				return;
			}
			GamerCollectionEnumerator<LocalNetworkGamer> enumerator = networkSession.LocalGamers.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					LocalNetworkGamer current = enumerator.Current;
					if (!HandlePlayerInput(input, current.SignedInGamer.PlayerIndex))
					{
						break;
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to .constrained prefix*/).Dispose();
			}
		}
	}

	private bool HandlePlayerInput(InputState input, PlayerIndex playerIndex)
	{
		m_KeyboardState = input.CurrentKeyboardStates[(int)playerIndex];
		m_GamepadState = input.CurrentGamePadStates[(int)playerIndex];
		bool flag = !m_GamepadState.IsConnected && input.GamePadWasConnected[(int)playerIndex];
		if (input.IsPauseGame(playerIndex) || flag)
		{
			base.ScreenManager.AddScreen(new PauseMenuScreen(networkSession), playerIndex);
			return false;
		}
		movement = Vector2.Zero;
		jump = false;
		leap = false;
		turn = 0f;
		UpdateGamepadControl();
		if (movement.Length() > 1f)
		{
			movement.Normalize();
		}
		g.m_PlayerManager.GetLocalPlayer().m_Movement = movement;
		g.m_PlayerManager.GetLocalPlayer().m_Turn = turn;
		g.m_PlayerManager.GetLocalPlayer().m_Jump = jump;
		g.m_PlayerManager.GetLocalPlayer().m_Leap = leap;
		m_OldGamepadState = m_GamepadState;
		m_OldKeyboardState = m_KeyboardState;
		g.m_PlayerManager.GetLocalPlayer().UpdateRumble(playerIndex);
		return true;
	}

	private void UpdateGamepadControl()
	{
		if (g.m_PlayerManager.GetLocalPlayer().m_State == Player.STATE.InGame || g.m_PlayerManager.GetLocalPlayer().m_State == Player.STATE.LocalDeath)
		{
			if (m_GamepadState.Buttons.Back == ButtonState.Pressed)
			{
				g.m_App.m_ShowScoreboard = true;
			}
			else
			{
				g.m_App.m_ShowScoreboard = false;
			}
		}
		if (g.m_PlayerManager.GetLocalPlayer().m_State == Player.STATE.LocalDeath || g.m_PlayerManager.GetLocalPlayer().m_State == Player.STATE.ChooseCharacter || g.m_PlayerManager.GetLocalPlayer().m_State == Player.STATE.Intermission || g.m_PlayerManager.GetLocalPlayer().m_State == Player.STATE.JoinTeam || g.m_App.m_Intermission)
		{
			return;
		}
		float num = 1f;
		if (Math.Abs(m_GamepadState.ThumbSticks.Left.Y) > 0.1f && g.m_CameraManager.m_LookAtPlayerId == -1)
		{
			movement.Y -= m_GamepadState.ThumbSticks.Left.Y * num;
		}
		if (Math.Abs(m_GamepadState.ThumbSticks.Left.X) > 0.1f && g.m_CameraManager.m_LookAtPlayerId == -1)
		{
			movement.X += m_GamepadState.ThumbSticks.Left.X * num;
		}
		if (m_GamepadState.DPad.Up == ButtonState.Pressed && g.m_CameraManager.m_LookAtPlayerId == -1)
		{
			movement.Y = -1f * num;
		}
		if (m_GamepadState.DPad.Down == ButtonState.Pressed && g.m_CameraManager.m_LookAtPlayerId == -1)
		{
			movement.Y = 1f * num;
		}
		if (m_GamepadState.DPad.Left == ButtonState.Pressed && g.m_CameraManager.m_LookAtPlayerId == -1)
		{
			movement.X = -1f * num;
		}
		if (m_GamepadState.DPad.Right == ButtonState.Pressed && g.m_CameraManager.m_LookAtPlayerId == -1)
		{
			movement.X = 1f * num;
		}
		if (Math.Abs(m_GamepadState.ThumbSticks.Right.X) > 0.1f && g.m_CameraManager.m_LookAtPlayerId == -1)
		{
			turn = m_GamepadState.ThumbSticks.Right.X * m_GamepadState.ThumbSticks.Right.X * m_GamepadState.ThumbSticks.Right.X * num;
		}
		float num2 = (g.m_App.m_OptionsInvertY ? (-1f) : 1f);
		if (Math.Abs(m_GamepadState.ThumbSticks.Right.Y) > 0.1f && g.m_CameraManager.m_LookAtPlayerId == -1)
		{
			g.m_CameraManager.m_Pitch += m_GamepadState.ThumbSticks.Right.Y * m_GamepadState.ThumbSticks.Right.Y * m_GamepadState.ThumbSticks.Right.Y * 0.015f * num * (60f * (float)g.m_App.m_GameTime.ElapsedGameTime.TotalSeconds) * (g.m_App.m_OptionsVert * 0.4f) * num2;
		}
		if (g.m_CameraManager.m_Pitch < -1.1f)
		{
			g.m_CameraManager.m_Pitch = -1.1f;
		}
		if (g.m_CameraManager.m_Pitch > 1.2f)
		{
			g.m_CameraManager.m_Pitch = 1.2f;
		}
		if (m_GamepadState.Triggers.Right > 0.1f)
		{
			g.m_PlayerManager.GetLocalPlayer().m_bFired = true;
			bool bDebounced = false;
			if (m_OldGamepadState.Triggers.Right < 0.1f)
			{
				bDebounced = true;
			}
			g.m_PlayerManager.GetLocalPlayer().FireWeapon(bDebounced);
		}
		g.m_PlayerManager.GetLocalPlayer().m_bSprinting = false;
		if (m_GamepadState.Triggers.Left > 0.1f && movement != Vector2.Zero && g.m_PlayerManager.GetLocalPlayer().m_Sprint > 0)
		{
			g.m_PlayerManager.GetLocalPlayer().m_Sprint--;
			g.m_PlayerManager.GetLocalPlayer().m_bSprinting = true;
		}
		if (Debounce(Buttons.RightShoulder) && g.m_PlayerManager.GetLocalPlayer().m_Team == Player.TEAM.Hunter)
		{
			g.m_PlayerManager.GetLocalPlayer().ToggleTorchLight();
			g.m_PlayerManager.GetLocalPlayer().m_bTorchChanged = true;
		}
		if (Debounce(Buttons.A))
		{
			g.m_PlayerManager.GetLocalPlayer().Interact();
		}
		if (Debounce(Buttons.B))
		{
			g.m_PlayerManager.GetLocalPlayer().InteractBack();
		}
		if (Debounce(Buttons.X))
		{
			g.m_PlayerManager.GetLocalPlayer().InteractX();
		}
		if (Debounce(Buttons.Y) && !g.m_PlayerManager.GetLocalPlayer().InteractY() && g.m_PlayerManager.GetLocalPlayer().NumWeapons() > 1)
		{
			g.m_PlayerManager.GetLocalPlayer().HolsterWeapon();
			g.m_PlayerManager.GetLocalPlayer().m_bWeaponChanged = true;
		}
	}

	private void UpdateKeyboardControl()
	{
	}

	public bool Debounce(Buttons b)
	{
		switch (b)
		{
		case Buttons.A:
			if (m_GamepadState.Buttons.A == ButtonState.Pressed && m_OldGamepadState.Buttons.A == ButtonState.Released)
			{
				return true;
			}
			break;
		case Buttons.B:
			if (m_GamepadState.Buttons.B == ButtonState.Pressed && m_OldGamepadState.Buttons.B == ButtonState.Released)
			{
				return true;
			}
			break;
		case Buttons.Y:
			if (m_GamepadState.Buttons.Y == ButtonState.Pressed && m_OldGamepadState.Buttons.Y == ButtonState.Released)
			{
				return true;
			}
			break;
		case Buttons.X:
			if (m_GamepadState.Buttons.X == ButtonState.Pressed && m_OldGamepadState.Buttons.X == ButtonState.Released)
			{
				return true;
			}
			break;
		case Buttons.Back:
			if (m_GamepadState.Buttons.Back == ButtonState.Pressed && m_OldGamepadState.Buttons.Back == ButtonState.Released)
			{
				return true;
			}
			break;
		case Buttons.Start:
			if (m_GamepadState.Buttons.Start == ButtonState.Pressed && m_OldGamepadState.Buttons.Start == ButtonState.Released)
			{
				return true;
			}
			break;
		case Buttons.LeftShoulder:
			if (m_GamepadState.Buttons.LeftShoulder == ButtonState.Pressed && m_OldGamepadState.Buttons.LeftShoulder == ButtonState.Released)
			{
				return true;
			}
			break;
		case Buttons.RightShoulder:
			if (m_GamepadState.Buttons.RightShoulder == ButtonState.Pressed && m_OldGamepadState.Buttons.RightShoulder == ButtonState.Released)
			{
				return true;
			}
			break;
		}
		return false;
	}

	public override void Draw(GameTime gameTime)
	{
		if (base.ScreenState == ScreenState.TransitionOff || base.ScreenState == ScreenState.TransitionOn)
		{
			base.ScreenManager.GraphicsDevice.Clear(Color.Black);
			return;
		}
		g.m_App.DrawHelmetHud();
		g.m_PlayerManager.GetLocalPlayer().DrawLCD();
		g.m_PlayerManager.GetLocalPlayer().DrawGrave();
		g.m_PlayerManager.GetLocalPlayer().DrawTablet();
		g.m_App.sceneState.BeginFrameRendering(g.m_CameraManager.m_ViewMatrix, g.m_CameraManager.m_ProjectionMatrix, gameTime, g.m_App.environment, g.m_App.frameBuffers, renderingtoscreen: true);
		g.m_App.sceneInterface.BeginFrameRendering(g.m_App.sceneState);
		g.m_App.sceneInterface.RenderManager.Render();
		g.m_App.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.None;
		g.m_App.m_ParticleSystemManager.SetCameraPositionForAllParticleSystems(g.m_CameraManager.m_Position);
		g.m_App.m_ParticleSystemManager.SetWorldViewProjectionMatricesForAllParticleSystems(Matrix.Identity, g.m_CameraManager.m_ViewMatrix, g.m_CameraManager.m_ProjectionMatrix);
		g.m_App.m_ParticleSystemManager.DrawAllParticleSystems();
		g.m_App.screenManager.SpriteBatch.Begin();
		g.m_App.DrawHud();
		if ((double)g.m_App.m_ShowPermissionTime > gameTime.TotalGameTime.TotalSeconds)
		{
			g.m_App.screenManager.SpriteBatch.DrawString(g.m_App.hudFont, "No permissions to buy with this Profile", new Vector2(220f, 400f), Color.Red);
		}
		g.m_App.screenManager.SpriteBatch.End();
		g.m_App.sceneInterface.EndFrameRendering();
		g.m_App.sceneState.EndFrameRendering();
		m_StopWatchRender.Stop();
		UpdateDebugStats();
	}

	protected void RenderSky(ISceneState scenestate)
	{
		if (skybox == null)
		{
			g.m_App.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Black, 1f, 0);
			return;
		}
		g.m_App.GraphicsDevice.Clear(ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Black, 1f, 0);
		g.m_App.GraphicsDevice.BlendState = BlendState.Opaque;
		g.m_App.GraphicsDevice.DepthStencilState = DepthStencilState.None;
		g.m_App.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
		g.m_App.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;
		for (int i = 1; i < 8; i++)
		{
			g.m_App.GraphicsDevice.SamplerStates[i] = SamplerState.PointWrap;
		}
		Matrix view = scenestate.View;
		view.Translation = Vector3.Zero;
		foreach (ModelMesh mesh in skybox.Meshes)
		{
			foreach (Effect effect in mesh.Effects)
			{
				if (effect is BasicEffect)
				{
					BasicEffect basicEffect = effect as BasicEffect;
					basicEffect.LightingEnabled = false;
					basicEffect.DiffuseColor = new Vector3(1f, 1f, 1f);
					basicEffect.View = view;
					basicEffect.World = skyboxWorld;
					basicEffect.Projection = scenestate.Projection;
				}
			}
			mesh.Draw();
		}
		g.m_App.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
	}

	private void UpdateDebugStats()
	{
		if (m_DebugStatUpdateTime < (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds)
		{
			m_ParticleTime = (float)m_StopWatchUpdateParticles.Elapsed.TotalMilliseconds;
			m_SceneTime = (float)m_StopWatchScene.Elapsed.TotalMilliseconds;
			m_NetworkTime = (float)m_StopWatchNetwork.Elapsed.TotalMilliseconds;
			m_ItemTime = (float)m_StopWatchItems.Elapsed.TotalMilliseconds;
			m_PhysicsTime = (float)m_StopWatchPhysics.Elapsed.TotalMilliseconds;
			m_PlayerTime = (float)m_StopWatchPlayers.Elapsed.TotalMilliseconds;
			m_UpdateTime = (float)m_StopWatchUpdate.Elapsed.TotalMilliseconds;
			m_RenderTime = (float)m_StopWatchRender.Elapsed.TotalMilliseconds;
			m_DebugStatUpdateTime = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 0.1f;
		}
		m_StopWatchUpdate.Reset();
		m_StopWatchRender.Reset();
		m_StopWatchPhysics.Reset();
		m_StopWatchPlayers.Reset();
		m_StopWatchItems.Reset();
		m_StopWatchNetwork.Reset();
		m_StopWatchScene.Reset();
		m_StopWatchUpdateParticles.Reset();
	}

	private void DrawDebugStats(GameTime gameTime)
	{
		_ = g.m_App.GraphicsDevice.Viewport.Bounds.Height;
		_ = g.m_App.GraphicsDevice.Viewport.Bounds.Width;
		FPStotalSinceLast += gameTime.ElapsedGameTime.TotalSeconds;
		FPStotalFramesSinceLast++;
		if (gameTime.TotalGameTime.TotalSeconds - FPSlastTime > 0.0 && gameTime.ElapsedGameTime.TotalSeconds > 0.0)
		{
			double num = FPStotalSinceLast / (double)FPStotalFramesSinceLast;
			FPSlastTime = gameTime.TotalGameTime.TotalSeconds;
			FPStoDisplay = Math.Round(1.0 / num, 1);
			FPStotalSinceLast = 0.0;
			FPStotalFramesSinceLast = 0;
		}
		g.m_App.screenManager.SpriteBatch.DrawString(g.m_App.lcdFont, $"FPS: {FPStoDisplay}", new Vector2(300f, 470f), Color.White * 0.25f);
	}

	private void UpdateNetworking(float elapsedTime)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		//IL_06bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c1: Unknown result type (might be due to invalid IL or missing references)
		ProcessPackets();
		if (networkSession == null || (int)networkSession.SessionState != 1)
		{
			return;
		}
		if (networkSession.IsHost)
		{
			_ = networkSession.Host;
			if (g.m_App.m_RequestIntermissionEnd)
			{
				((BinaryWriter)(object)packetWriter).Write((byte)43);
				((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3);
				g.m_App.m_RequestIntermissionEnd = false;
			}
			bool flag = false;
			for (int i = 0; i < 16; i++)
			{
				if (g.m_PlayerManager.m_Player[i].m_Id != -1 && g.m_PlayerManager.m_Player[i].m_Bot)
				{
					if (g.m_PlayerManager.m_Player[i].m_RequestSendTeam)
					{
						SendBotTeamChangedMessage(g.m_PlayerManager.m_Player[i].m_NetId);
					}
					if (g.m_PlayerManager.m_Player[i].m_RequestSendClass)
					{
						SendBotClassChangedMessage(g.m_PlayerManager.m_Player[i].m_NetId);
					}
					if (g.m_PlayerManager.m_Player[i].m_bRequestDied)
					{
						SendBotDeath(g.m_PlayerManager.m_Player[i].m_NetId);
					}
					if (g.m_PlayerManager.m_Player[i].m_bFired)
					{
						SendBotFireMessage(g.m_PlayerManager.m_Player[i].m_NetId);
					}
					if (g.m_PlayerManager.m_Player[i].m_RequestSendImpact)
					{
						SendBotImpactMessage(g.m_PlayerManager.m_Player[i].m_NetId);
					}
					if (g.m_PlayerManager.m_Player[i].m_bTorchChanged)
					{
						SendBotTorchChangedMessage(g.m_PlayerManager.m_Player[i].m_NetId);
					}
					if (g.m_PlayerManager.m_Player[i].m_bRequestSendDamage)
					{
						SendBotDamageMessage(g.m_PlayerManager.m_Player[i].m_NetId);
					}
					if (g.m_PlayerManager.m_Player[i].m_bWeaponChanged)
					{
						SendBotWeaponChangedMessage(g.m_PlayerManager.m_Player[i].m_NetId);
					}
					if (g.m_PlayerManager.m_Player[i].m_bRequestSendSpawn)
					{
						SendBotSpawnMessage(g.m_PlayerManager.m_Player[i].m_NetId);
					}
					if (g.m_PlayerManager.m_Player[i].m_RequestSendCrouch)
					{
						SendBotCrouchChangedMessage(g.m_PlayerManager.m_Player[i].m_NetId);
					}
					if (g.m_PlayerManager.m_Player[i].m_AnimChanged)
					{
						SendBotAnimChangedMessage(g.m_PlayerManager.m_Player[i].m_NetId);
					}
					if (g.m_PlayerManager.m_Player[i].m_AnimUpperChanged)
					{
						SendBotAnimUpperChangedMessage(g.m_PlayerManager.m_Player[i].m_NetId);
					}
					if (g.m_PlayerManager.m_Player[i].m_RequestCreateProjectile)
					{
						SendBotCreateProjectileMessage(g.m_PlayerManager.m_Player[i].m_NetId);
					}
					if (updatesSinceBotPacket >= 4)
					{
						SendBotData(g.m_PlayerManager.m_Player[i].m_NetId);
					}
					flag = true;
				}
			}
			if (flag)
			{
				if (updatesSinceBotPacket >= 4)
				{
					updatesSinceBotPacket = 0;
				}
				else
				{
					updatesSinceBotPacket++;
				}
			}
			if (m_HostSendGiveScore)
			{
				((BinaryWriter)(object)packetWriter).Write((byte)37);
				((BinaryWriter)(object)packetWriter).Write(m_HostSendGiveScorePlayerNetId);
				((BinaryWriter)(object)packetWriter).Write(m_HostSendGiveScoreAmount);
				((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3);
				m_HostSendGiveScore = false;
			}
			if (g.m_App.m_RequestIntermission)
			{
				((BinaryWriter)(object)packetWriter).Write((byte)21);
				((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3);
				g.m_App.m_RequestIntermission = false;
			}
			if (g.m_App.m_RequestDeleteBotId != 255)
			{
				((BinaryWriter)(object)packetWriter).Write((byte)23);
				((BinaryWriter)(object)packetWriter).Write(g.m_App.m_RequestDeleteBotId);
				((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3);
				g.m_App.m_RequestDeleteBotId = 255;
			}
			if (m_HostSendUseCrateAllow)
			{
				((BinaryWriter)(object)packetWriter).Write((byte)27);
				((BinaryWriter)(object)packetWriter).Write(m_HostSendUseCrateId);
				((BinaryWriter)(object)packetWriter).Write(m_HostSendUsePlayerNetId);
				((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3);
				m_HostSendUseCrateAllow = false;
			}
			if (m_HostSendCrateItemToPeer)
			{
				((BinaryWriter)(object)packetWriter).Write((byte)29);
				((BinaryWriter)(object)packetWriter).Write(m_HostSendCrateItemType);
				((BinaryWriter)(object)packetWriter).Write(m_HostSendCrateItemNetId);
				((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3);
				m_HostSendCrateItemToPeer = false;
			}
			if (m_HostSendDetachItem)
			{
				((BinaryWriter)(object)packetWriter).Write((byte)32);
				((BinaryWriter)(object)packetWriter).Write(m_HostSendDetachPlayerNetId);
				packetWriter.Write(m_HostSendDetachItemPos);
				((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3);
				m_HostSendDetachItem = false;
			}
			if (m_HostSendDeleteNetworkItem)
			{
				SendDeleteNetworkItem(m_HostSendDeleteNetworkItemId);
				m_HostSendDeleteNetworkItem = false;
			}
			if (m_HostSendArtifactReturnedMessage)
			{
				((BinaryWriter)(object)packetWriter).Write((byte)34);
				((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3);
				m_HostSendArtifactReturnedMessage = false;
			}
			if (m_HostSendEscapeCountdownMessage)
			{
				((BinaryWriter)(object)packetWriter).Write((byte)35);
				packetWriter.Write(m_AirlockSmokeDir);
				packetWriter.Write(m_AirlockSmokePos);
				((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3);
				m_HostSendEscapeCountdownMessage = false;
			}
			if (m_HostSendCancelCountdownMessage)
			{
				((BinaryWriter)(object)packetWriter).Write((byte)36);
				((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3);
				m_HostSendCancelCountdownMessage = false;
			}
			if (m_HostSendArtifactState)
			{
				((BinaryWriter)(object)packetWriter).Write((byte)38);
				((BinaryWriter)(object)packetWriter).Write((byte)m_HostArtifactState);
				((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3);
				m_HostSendArtifactState = false;
			}
			if (m_HostSendCrateSeed)
			{
				((BinaryWriter)(object)packetWriter).Write((byte)41);
				((BinaryWriter)(object)packetWriter).Write(m_HostCrateSeedValue);
				((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3);
				m_HostSendCrateSeed = false;
			}
		}
		GamerCollectionEnumerator<NetworkGamer> enumerator = networkSession.AllGamers.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				NetworkGamer current = enumerator.Current;
				if (((Gamer)current).Tag is Player { m_Id: not -1 } player && current.IsLocal)
				{
					if (player.m_RequestCleanItems)
					{
						SendCleanItemsMessage();
					}
					if (player.m_RequestSendTeam)
					{
						SendTeamChangedMessage();
					}
					if (player.m_RequestSendClass)
					{
						SendClassChangedMessage();
					}
					if (player.m_bRequestDied)
					{
						SendLocalShipDeath();
					}
					if (player.m_bFired)
					{
						SendFireMessage();
					}
					if (player.m_RequestSendImpact)
					{
						SendImpactMessage();
					}
					if (player.m_bTorchChanged)
					{
						SendTorchChangedMessage();
					}
					if (player.m_bRequestSendDamage)
					{
						SendDamageMessage();
					}
					if (player.m_bWeaponChanged)
					{
						SendWeaponChangedMessage();
					}
					if (player.m_bRequestSendSpawn)
					{
						SendSpawnMessage();
					}
					if (player.m_RequestSendCrouch)
					{
						SendCrouchChangedMessage();
					}
					if (player.m_RequestSendScore)
					{
						SendScoreChangedMessage();
					}
					if (player.m_AnimChanged)
					{
						SendAnimChangedMessage();
					}
					if (player.m_AnimUpperChanged)
					{
						SendAnimUpperChangedMessage();
					}
					if (player.m_RequestRankUp)
					{
						SendRankUpMessage();
					}
					if (player.m_RequestUseCrate)
					{
						SendRequestUseCrateMessage();
					}
					if (player.m_RequestCreateProjectile)
					{
						SendRequestCreateProjectileMessage();
					}
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to .constrained prefix*/).Dispose();
		}
		if (updatesSinceStatusPacket >= 4)
		{
			updatesSinceStatusPacket = 0;
			SendLocalShipData();
		}
		else
		{
			updatesSinceStatusPacket++;
		}
	}

	private Player SendCreateBot(Player.TEAM team)
	{
		Player player = null;
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0 && networkSession.IsHost)
		{
			player = g.m_PlayerManager.Create(g.m_PlayerManager.GetNextBotId(), bot: true, team);
			((BinaryWriter)(object)packetWriter).Write((byte)44);
			packetWriter.Write(Vector3.Zero);
			((BinaryWriter)(object)packetWriter).Write(0f);
			((BinaryWriter)(object)packetWriter).Write(player.m_NetId);
			((BinaryWriter)(object)packetWriter).Write((byte)player.GetTeam());
			((BinaryWriter)(object)packetWriter).Write((byte)player.GetClass());
			((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3);
		}
		else
		{
			player = g.m_PlayerManager.Create(g.m_PlayerManager.GetNextBotId(), bot: true, team);
		}
		return player;
	}

	public int SendCreateNetworkItem(Item.OBJ type, byte netId, Vector3 pos)
	{
		int num = -1;
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0 && networkSession.IsHost)
		{
			num = g.m_ItemManager.Create(10, netId, pos, 0f, null);
			((BinaryWriter)(object)packetWriter).Write((byte)30);
			((BinaryWriter)(object)packetWriter).Write((byte)type);
			((BinaryWriter)(object)packetWriter).Write(netId);
			packetWriter.Write(pos);
			((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3);
		}
		else
		{
			num = g.m_ItemManager.Create(10, netId, pos, 0f, null);
		}
		return num;
	}

	public void SendDeleteNetworkItem(byte netId)
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0 && networkSession.IsHost)
		{
			((BinaryWriter)(object)packetWriter).Write((byte)33);
			((BinaryWriter)(object)packetWriter).Write(netId);
			((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3);
		}
	}

	public void SendAttachItem(byte netItemId, short netPlayerId, byte artifactEscapeAirlockId)
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0 && networkSession.IsHost)
		{
			((BinaryWriter)(object)packetWriter).Write((byte)31);
			((BinaryWriter)(object)packetWriter).Write(netItemId);
			((BinaryWriter)(object)packetWriter).Write(netPlayerId);
			((BinaryWriter)(object)packetWriter).Write(artifactEscapeAirlockId);
			((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3);
		}
	}

	private void SendLocalShipData()
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0 && ((Gamer)((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0]).Tag is Player { m_Id: not -1 } player)
		{
			((BinaryWriter)(object)packetWriter).Write((byte)2);
			packetWriter.Write(player.m_Position);
			((BinaryWriter)(object)packetWriter).Write(player.m_Rotation.Y);
			float num = g.m_CameraManager.m_Pitch;
			if (num > 0.5f)
			{
				num = 0.5f;
			}
			if (num < -0.5f)
			{
				num = -0.5f;
			}
			((BinaryWriter)(object)packetWriter).Write((sbyte)(num * 252f));
			((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)0);
		}
	}

	private void SendBotData(short i)
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			((BinaryWriter)(object)packetWriter).Write((byte)45);
			((BinaryWriter)(object)packetWriter).Write(i);
			packetWriter.Write(g.m_PlayerManager.GetBot(i).m_Position);
			((BinaryWriter)(object)packetWriter).Write(g.m_PlayerManager.GetBot(i).m_Rotation.Y);
			((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)0);
		}
	}

	private void SendLocalShipDeath()
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			LocalNetworkGamer val = ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0];
			if (((Gamer)val).Tag is Player { m_Id: not -1 } player)
			{
				((BinaryWriter)(object)packetWriter).Write((byte)7);
				short lastAttackerNetId = player.m_LastAttackerNetId;
				short lastProjectileNetId = player.m_LastProjectileNetId;
				((BinaryWriter)(object)packetWriter).Write(lastAttackerNetId);
				((BinaryWriter)(object)packetWriter).Write(lastProjectileNetId);
				val.SendData(packetWriter, (SendDataOptions)3);
				player.m_bRequestDied = false;
			}
		}
	}

	private void SendBotDeath(short botId)
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			((BinaryWriter)(object)packetWriter).Write((byte)49);
			((BinaryWriter)(object)packetWriter).Write(botId);
			((BinaryWriter)(object)packetWriter).Write(g.m_PlayerManager.GetBot(botId).m_LastAttackerNetId);
			((BinaryWriter)(object)packetWriter).Write(g.m_PlayerManager.GetBot(botId).m_LastProjectileNetId);
			((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3);
			g.m_PlayerManager.GetBot(botId).m_bRequestDied = false;
		}
	}

	private void SendFireMessage()
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			LocalNetworkGamer val = ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0];
			if (((Gamer)val).Tag is Player player)
			{
				((BinaryWriter)(object)packetWriter).Write((byte)10);
				((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)1);
				player.m_bFired = false;
			}
		}
	}

	private void SendBotFireMessage(short botId)
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			((BinaryWriter)(object)packetWriter).Write((byte)46);
			((BinaryWriter)(object)packetWriter).Write(botId);
			((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)1);
			g.m_PlayerManager.GetBot(botId).m_bFired = false;
		}
	}

	private void SendImpactMessage()
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			LocalNetworkGamer val = ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0];
			if (((Gamer)val).Tag is Player player)
			{
				((BinaryWriter)(object)packetWriter).Write((byte)40);
				((BinaryWriter)(object)packetWriter).Write((short)(player.m_RequestSendImpactPos.X * 10f));
				((BinaryWriter)(object)packetWriter).Write((short)(player.m_RequestSendImpactPos.Y * 10f));
				((BinaryWriter)(object)packetWriter).Write((short)(player.m_RequestSendImpactPos.Z * 10f));
				((BinaryWriter)(object)packetWriter).Write((sbyte)(player.m_RequestSendImpactNormal.X * 127f));
				((BinaryWriter)(object)packetWriter).Write((sbyte)(player.m_RequestSendImpactNormal.Y * 127f));
				((BinaryWriter)(object)packetWriter).Write((sbyte)(player.m_RequestSendImpactNormal.Z * 127f));
				((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)0);
				player.m_RequestSendImpact = false;
			}
		}
	}

	private void SendBotImpactMessage(short botId)
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			((BinaryWriter)(object)packetWriter).Write((byte)59);
			((BinaryWriter)(object)packetWriter).Write(botId);
			((BinaryWriter)(object)packetWriter).Write((short)(g.m_PlayerManager.GetBot(botId).m_RequestSendImpactPos.X * 10f));
			((BinaryWriter)(object)packetWriter).Write((short)(g.m_PlayerManager.GetBot(botId).m_RequestSendImpactPos.Y * 10f));
			((BinaryWriter)(object)packetWriter).Write((short)(g.m_PlayerManager.GetBot(botId).m_RequestSendImpactPos.Z * 10f));
			((BinaryWriter)(object)packetWriter).Write((sbyte)(g.m_PlayerManager.GetBot(botId).m_RequestSendImpactNormal.X * 127f));
			((BinaryWriter)(object)packetWriter).Write((sbyte)(g.m_PlayerManager.GetBot(botId).m_RequestSendImpactNormal.Y * 127f));
			((BinaryWriter)(object)packetWriter).Write((sbyte)(g.m_PlayerManager.GetBot(botId).m_RequestSendImpactNormal.Z * 127f));
			((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)0);
			g.m_PlayerManager.GetBot(botId).m_RequestSendImpact = false;
		}
	}

	private void SendTorchChangedMessage()
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			LocalNetworkGamer val = ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0];
			if (((Gamer)val).Tag is Player player)
			{
				((BinaryWriter)(object)packetWriter).Write((byte)11);
				((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)1);
				player.m_bTorchChanged = false;
			}
		}
	}

	private void SendBotTorchChangedMessage(short botId)
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			((BinaryWriter)(object)packetWriter).Write((byte)47);
			((BinaryWriter)(object)packetWriter).Write(botId);
			((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)1);
			g.m_PlayerManager.GetBot(botId).m_bTorchChanged = false;
		}
	}

	private void SendDamageMessage()
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			LocalNetworkGamer val = ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0];
			if (((Gamer)val).Tag is Player player)
			{
				((BinaryWriter)(object)packetWriter).Write((byte)12);
				((BinaryWriter)(object)packetWriter).Write(player.m_RequestedDamageAmount);
				((BinaryWriter)(object)packetWriter).Write(player.m_RequestedPlayerToDamageNetID);
				((BinaryWriter)(object)packetWriter).Write(player.m_RequestedHitZone);
				((BinaryWriter)(object)packetWriter).Write(player.m_RequestedAttacker);
				((BinaryWriter)(object)packetWriter).Write(player.m_RequestedProjectileNetId);
				((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3);
				player.m_bRequestSendDamage = false;
			}
		}
	}

	private void SendBotDamageMessage(short botId)
	{
		((BinaryWriter)(object)packetWriter).Write((byte)48);
		((BinaryWriter)(object)packetWriter).Write(botId);
		((BinaryWriter)(object)packetWriter).Write(g.m_PlayerManager.GetBot(botId).m_RequestedDamageAmount);
		((BinaryWriter)(object)packetWriter).Write(g.m_PlayerManager.GetBot(botId).m_RequestedPlayerToDamageNetID);
		((BinaryWriter)(object)packetWriter).Write(g.m_PlayerManager.GetBot(botId).m_RequestedHitZone);
		((BinaryWriter)(object)packetWriter).Write(g.m_PlayerManager.GetBot(botId).m_RequestedAttacker);
		((BinaryWriter)(object)packetWriter).Write(g.m_PlayerManager.GetBot(botId).m_RequestedProjectileNetId);
		((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3);
		g.m_PlayerManager.GetBot(botId).m_bRequestSendDamage = false;
	}

	private void SendWeaponChangedMessage()
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			LocalNetworkGamer val = ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0];
			if (((Gamer)val).Tag is Player player)
			{
				((BinaryWriter)(object)packetWriter).Write((byte)13);
				((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3);
				player.m_bWeaponChanged = false;
			}
		}
	}

	private void SendBotWeaponChangedMessage(short botId)
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			((BinaryWriter)(object)packetWriter).Write((byte)50);
			((BinaryWriter)(object)packetWriter).Write(botId);
			((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3);
			g.m_PlayerManager.GetBot(botId).m_bWeaponChanged = false;
		}
	}

	private void SendSpawnMessage()
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			LocalNetworkGamer val = ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0];
			if (((Gamer)val).Tag is Player player)
			{
				((BinaryWriter)(object)packetWriter).Write((byte)14);
				packetWriter.Write(player.m_Position);
				((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3);
				player.m_bRequestSendSpawn = false;
			}
		}
	}

	private void SendBotSpawnMessage(short botId)
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			((BinaryWriter)(object)packetWriter).Write((byte)51);
			((BinaryWriter)(object)packetWriter).Write(botId);
			packetWriter.Write(g.m_PlayerManager.GetBot(botId).m_Position);
			((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3);
			g.m_PlayerManager.GetBot(botId).m_bRequestSendSpawn = false;
		}
	}

	private void SendBotCrouchChangedMessage(short botId)
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			((BinaryWriter)(object)packetWriter).Write((byte)60);
			((BinaryWriter)(object)packetWriter).Write(botId);
			((BinaryWriter)(object)packetWriter).Write(g.m_PlayerManager.GetBot(botId).m_Crouch);
			((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)1);
			g.m_PlayerManager.GetBot(botId).m_RequestSendCrouch = false;
		}
	}

	private void SendCrouchChangedMessage()
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			LocalNetworkGamer val = ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0];
			if (((Gamer)val).Tag is Player player)
			{
				((BinaryWriter)(object)packetWriter).Write((byte)15);
				((BinaryWriter)(object)packetWriter).Write(player.m_Crouch);
				((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)1);
				player.m_RequestSendCrouch = false;
			}
		}
	}

	private void SendTeamChangedMessage()
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			LocalNetworkGamer val = ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0];
			if (((Gamer)val).Tag is Player player)
			{
				((BinaryWriter)(object)packetWriter).Write((byte)18);
				((BinaryWriter)(object)packetWriter).Write((byte)player.m_Team);
				((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3);
				player.m_RequestSendTeam = false;
			}
		}
	}

	private void SendBotTeamChangedMessage(short botId)
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			((BinaryWriter)(object)packetWriter).Write((byte)54);
			((BinaryWriter)(object)packetWriter).Write(botId);
			((BinaryWriter)(object)packetWriter).Write((byte)g.m_PlayerManager.GetBot(botId).m_Team);
			((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3);
			g.m_PlayerManager.GetBot(botId).m_RequestSendTeam = false;
		}
	}

	private void SendClassChangedMessage()
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			LocalNetworkGamer val = ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0];
			if (((Gamer)val).Tag is Player player)
			{
				((BinaryWriter)(object)packetWriter).Write((byte)19);
				((BinaryWriter)(object)packetWriter).Write((byte)player.m_Class);
				((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3);
				player.m_RequestSendClass = false;
			}
		}
	}

	private void SendBotClassChangedMessage(short botId)
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			((BinaryWriter)(object)packetWriter).Write((byte)55);
			((BinaryWriter)(object)packetWriter).Write(botId);
			((BinaryWriter)(object)packetWriter).Write((byte)g.m_PlayerManager.GetBot(botId).m_Class);
			((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3);
			g.m_PlayerManager.GetBot(botId).m_RequestSendClass = false;
		}
	}

	private void SendScoreChangedMessage()
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			LocalNetworkGamer val = ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0];
			if (((Gamer)val).Tag is Player player)
			{
				((BinaryWriter)(object)packetWriter).Write((byte)16);
				((BinaryWriter)(object)packetWriter).Write(player.m_Score);
				((BinaryWriter)(object)packetWriter).Write(player.m_Kills);
				((BinaryWriter)(object)packetWriter).Write(player.m_Deaths);
				((BinaryWriter)(object)packetWriter).Write(player.m_Rank);
				((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)1);
				player.m_RequestSendScore = false;
			}
		}
	}

	private void SendAnimChangedMessage()
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			LocalNetworkGamer val = ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0];
			if (((Gamer)val).Tag is Player player)
			{
				((BinaryWriter)(object)packetWriter).Write((byte)20);
				((BinaryWriter)(object)packetWriter).Write(player.m_Anim);
				((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)1);
				player.m_AnimChanged = false;
			}
		}
	}

	private void SendAnimUpperChangedMessage()
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			LocalNetworkGamer val = ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0];
			if (((Gamer)val).Tag is Player player)
			{
				((BinaryWriter)(object)packetWriter).Write((byte)28);
				((BinaryWriter)(object)packetWriter).Write(player.m_AnimUpper);
				((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)1);
				player.m_AnimUpperChanged = false;
			}
		}
	}

	private void SendRankUpMessage()
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			LocalNetworkGamer val = ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0];
			if (((Gamer)val).Tag is Player player)
			{
				((BinaryWriter)(object)packetWriter).Write((byte)24);
				((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)0);
				player.m_RequestRankUp = false;
			}
		}
	}

	private void SendRequestUseCrateMessage()
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			LocalNetworkGamer val = ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0];
			if (((Gamer)val).Tag is Player player && !networkSession.IsHost)
			{
				((BinaryWriter)(object)packetWriter).Write((byte)26);
				((BinaryWriter)(object)packetWriter).Write(player.m_RequestUseCrateId);
				((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3, networkSession.Host);
				player.m_RequestUseCrate = false;
			}
		}
	}

	private void SendRequestCreateProjectileMessage()
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			LocalNetworkGamer val = ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0];
			if (((Gamer)val).Tag is Player player)
			{
				((BinaryWriter)(object)packetWriter).Write((byte)39);
				((BinaryWriter)(object)packetWriter).Write(player.m_RequestCreateProjectileNetId);
				((BinaryWriter)(object)packetWriter).Write(player.m_RequestCreateProjectileType);
				packetWriter.Write(player.m_RequestCreateProjectilePosition);
				packetWriter.Write(player.m_RequestCreateProjectileQuaterion);
				packetWriter.Write(player.m_RequestCreateProjectileVeclocity);
				((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3);
				player.m_RequestCreateProjectile = false;
			}
		}
	}

	private void SendCleanItemsMessage()
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			LocalNetworkGamer val = ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0];
			if (((Gamer)val).Tag is Player player)
			{
				((BinaryWriter)(object)packetWriter).Write((byte)25);
				((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)3);
				player.m_RequestCleanItems = false;
			}
		}
	}

	private void SendBotAnimChangedMessage(short botId)
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			((BinaryWriter)(object)packetWriter).Write((byte)56);
			((BinaryWriter)(object)packetWriter).Write(botId);
			((BinaryWriter)(object)packetWriter).Write(g.m_PlayerManager.GetBot(botId).m_Anim);
			((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)1);
			g.m_PlayerManager.GetBot(botId).m_AnimChanged = false;
		}
	}

	private void SendBotAnimUpperChangedMessage(short botId)
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			((BinaryWriter)(object)packetWriter).Write((byte)57);
			((BinaryWriter)(object)packetWriter).Write(botId);
			((BinaryWriter)(object)packetWriter).Write(g.m_PlayerManager.GetBot(botId).m_AnimUpper);
			((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)1);
			g.m_PlayerManager.GetBot(botId).m_AnimUpperChanged = false;
		}
	}

	private void SendBotCreateProjectileMessage(short botId)
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0)
		{
			((BinaryWriter)(object)packetWriter).Write((byte)58);
			((BinaryWriter)(object)packetWriter).Write(botId);
			((BinaryWriter)(object)packetWriter).Write(g.m_PlayerManager.GetBot(botId).m_RequestCreateProjectileNetId);
			((BinaryWriter)(object)packetWriter).Write(g.m_PlayerManager.GetBot(botId).m_RequestCreateProjectileType);
			packetWriter.Write(g.m_PlayerManager.GetBot(botId).m_RequestCreateProjectilePosition);
			packetWriter.Write(g.m_PlayerManager.GetBot(botId).m_RequestCreateProjectileQuaterion);
			packetWriter.Write(g.m_PlayerManager.GetBot(botId).m_RequestCreateProjectileVeclocity);
			((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].SendData(packetWriter, (SendDataOptions)1);
			g.m_PlayerManager.GetBot(botId).m_RequestCreateProjectile = false;
		}
	}

	private void ProcessPackets()
	{
		if (networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count <= 0)
		{
			return;
		}
		NetworkGamer val = default(NetworkGamer);
		while (((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].IsDataAvailable)
		{
			((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers)[0].ReceiveData(packetReader, ref val);
			switch ((NetworkSessionComponent.PacketTypes)((BinaryReader)(object)packetReader).ReadByte())
			{
			case NetworkSessionComponent.PacketTypes.PlayerData:
				UpdatePlayerData(val);
				break;
			case NetworkSessionComponent.PacketTypes.Level:
				if (val != null && !val.IsLocal)
				{
					g.m_App.m_Level = ((BinaryReader)(object)packetReader).ReadByte();
					LoadSunburnScene();
				}
				break;
			case NetworkSessionComponent.PacketTypes.CreateBot:
				if (val != null && !val.IsLocal)
				{
					g.m_BotPathManager.LoadBotPath();
					Vector3 position = packetReader.ReadVector3();
					float y = ((BinaryReader)(object)packetReader).ReadSingle();
					short netId2 = ((BinaryReader)(object)packetReader).ReadInt16();
					byte b = ((BinaryReader)(object)packetReader).ReadByte();
					byte c = ((BinaryReader)(object)packetReader).ReadByte();
					Player player6 = g.m_PlayerManager.Create(netId2, bot: true, (Player.TEAM)b);
					player6.PeerSetTeam((Player.TEAM)b);
					player6.PeerSetClass((Player.CLASS)c);
					player6.m_Position = position;
					player6.m_Rotation.Y = y;
				}
				break;
			case NetworkSessionComponent.PacketTypes.CreateItem:
				if (val != null && !val.IsLocal)
				{
					byte type = ((BinaryReader)(object)packetReader).ReadByte();
					byte netId5 = ((BinaryReader)(object)packetReader).ReadByte();
					Vector3 pos4 = packetReader.ReadVector3();
					g.m_ItemManager.Create(type, netId5, pos4, 0f, null);
				}
				break;
			case NetworkSessionComponent.PacketTypes.AttachItem:
				if (val != null && !val.IsLocal)
				{
					byte netItemId2 = ((BinaryReader)(object)packetReader).ReadByte();
					short netId6 = ((BinaryReader)(object)packetReader).ReadInt16();
					byte escapeAirlockId = ((BinaryReader)(object)packetReader).ReadByte();
					int playerExistsWithNetId4 = g.m_PlayerManager.GetPlayerExistsWithNetId(netId6);
					if (playerExistsWithNetId4 != -1)
					{
						g.m_PlayerManager.m_Player[playerExistsWithNetId4].PeerAttach(netItemId2, escapeAirlockId);
					}
				}
				break;
			case NetworkSessionComponent.PacketTypes.DetachItem:
				if (val != null && !val.IsLocal)
				{
					short netId7 = ((BinaryReader)(object)packetReader).ReadInt16();
					int playerExistsWithNetId5 = g.m_PlayerManager.GetPlayerExistsWithNetId(netId7);
					if (playerExistsWithNetId5 != -1)
					{
						g.m_PlayerManager.m_Player[playerExistsWithNetId5].PeerDetach();
					}
				}
				break;
			case NetworkSessionComponent.PacketTypes.DeleteItem:
				if (val != null && !val.IsLocal)
				{
					byte netItemId = ((BinaryReader)(object)packetReader).ReadByte();
					int itemIdByNetItemId = g.m_ItemManager.GetItemIdByNetItemId(netItemId);
					if (itemIdByNetItemId != -1)
					{
						g.m_ItemManager.Delete(itemIdByNetItemId);
					}
				}
				break;
			case NetworkSessionComponent.PacketTypes.ArtifactReturned:
				if (val != null && !val.IsLocal)
				{
					g.m_ItemManager.ShowArtifactReturnedMessage();
				}
				break;
			case NetworkSessionComponent.PacketTypes.HostGiveScore:
				if (val != null && !val.IsLocal)
				{
					short netId4 = ((BinaryReader)(object)packetReader).ReadInt16();
					byte score = ((BinaryReader)(object)packetReader).ReadByte();
					int playerExistsWithNetId3 = g.m_PlayerManager.GetPlayerExistsWithNetId(netId4);
					if (playerExistsWithNetId3 != -1 && g.m_PlayerManager.m_Player[playerExistsWithNetId3].m_Id != -1)
					{
						g.m_PlayerManager.m_Player[playerExistsWithNetId3].PeerGiveScore(score);
					}
				}
				break;
			case NetworkSessionComponent.PacketTypes.HostArtifactState:
				if (val != null && !val.IsLocal)
				{
					ARTIFACTSTATE state = (ARTIFACTSTATE)((BinaryReader)(object)packetReader).ReadByte();
					PeerSetHostArtifactState(state);
				}
				break;
			case NetworkSessionComponent.PacketTypes.HostCrateSeed:
				if (val != null && !val.IsLocal)
				{
					int num = ((BinaryReader)(object)packetReader).ReadInt32();
					g.m_ItemManager.SetUpCrates(num);
					m_HostCrateSeedValue = num;
				}
				break;
			case NetworkSessionComponent.PacketTypes.HostCrateStates:
				if (val != null && !val.IsLocal)
				{
					byte[] crateStatesFromBuffer = ((BinaryReader)(object)packetReader).ReadBytes(22);
					g.m_ItemManager.SetCrateStatesFromBuffer(crateStatesFromBuffer);
				}
				break;
			case NetworkSessionComponent.PacketTypes.ShipData:
				if (val != null && !val.IsLocal)
				{
					UpdateShipData(val);
				}
				break;
			case NetworkSessionComponent.PacketTypes.BotData:
				if (val != null && !val.IsLocal)
				{
					UpdateBotData(val);
				}
				break;
			case NetworkSessionComponent.PacketTypes.FireWeapon:
				if (val != null && !val.IsLocal && ((Gamer)val).Tag is Player { m_Id: not -1 } player19)
				{
					player19.SimulateFireWeapon();
				}
				break;
			case NetworkSessionComponent.PacketTypes.Impact:
				if (val != null && !val.IsLocal)
				{
					Player player13 = ((Gamer)val).Tag as Player;
					Vector3 zero2 = Vector3.Zero;
					zero2.X = (float)((BinaryReader)(object)packetReader).ReadInt16() * 0.1f;
					zero2.Y = (float)((BinaryReader)(object)packetReader).ReadInt16() * 0.1f;
					zero2.Z = (float)((BinaryReader)(object)packetReader).ReadInt16() * 0.1f;
					sbyte b5 = ((BinaryReader)(object)packetReader).ReadSByte();
					sbyte b6 = ((BinaryReader)(object)packetReader).ReadSByte();
					sbyte b7 = ((BinaryReader)(object)packetReader).ReadSByte();
					Vector3 impactNormal2 = new Vector3((float)b5 / 127f, (float)b6 / 127f, (float)b7 / 127f);
					if (player13 != null && player13.m_Id != -1)
					{
						player13.SimulateWeaponImpact(zero2, impactNormal2);
					}
				}
				break;
			case NetworkSessionComponent.PacketTypes.DoDamage:
				if (val != null && !val.IsLocal && ((Gamer)val).Tag is Player { m_Id: not -1 })
				{
					sbyte damage2 = ((BinaryReader)(object)packetReader).ReadSByte();
					short netId3 = ((BinaryReader)(object)packetReader).ReadInt16();
					byte hitZone2 = ((BinaryReader)(object)packetReader).ReadByte();
					short attackerNetId2 = ((BinaryReader)(object)packetReader).ReadInt16();
					short projectileNetId3 = ((BinaryReader)(object)packetReader).ReadInt16();
					int playerExistsWithNetId2 = g.m_PlayerManager.GetPlayerExistsWithNetId(netId3);
					if (playerExistsWithNetId2 != -1 && g.m_PlayerManager.m_Player[playerExistsWithNetId2].m_Id != -1)
					{
						g.m_PlayerManager.m_Player[playerExistsWithNetId2].DoDamage(damage2, hitZone2, attackerNetId2, projectileNetId3);
					}
				}
				break;
			case NetworkSessionComponent.PacketTypes.TorchChanged:
				if (val != null && !val.IsLocal && ((Gamer)val).Tag is Player { m_Id: not -1 } player3)
				{
					player3.ToggleTorchLight();
				}
				break;
			case NetworkSessionComponent.PacketTypes.WeaponChanged:
				if (val != null && !val.IsLocal && ((Gamer)val).Tag is Player { m_Id: not -1 } player16)
				{
					player16.PeerChangeWeapon();
				}
				break;
			case NetworkSessionComponent.PacketTypes.Spawned:
				if (val != null && !val.IsLocal && ((Gamer)val).Tag is Player { m_Id: not -1 } player12)
				{
					Vector3 pos3 = packetReader.ReadVector3();
					player12.PeerSpawned(pos3);
				}
				break;
			case NetworkSessionComponent.PacketTypes.Crouch:
				if (val != null && !val.IsLocal && ((Gamer)val).Tag is Player { m_Id: not -1 } player8)
				{
					bool crouch = ((BinaryReader)(object)packetReader).ReadBoolean();
					player8.PeerCrouch(crouch);
				}
				break;
			case NetworkSessionComponent.PacketTypes.Score:
				if (val != null && !val.IsLocal && ((Gamer)val).Tag is Player { m_Id: not -1 } player4)
				{
					player4.m_Score = ((BinaryReader)(object)packetReader).ReadInt16();
					player4.m_Kills = ((BinaryReader)(object)packetReader).ReadInt16();
					player4.m_Deaths = ((BinaryReader)(object)packetReader).ReadInt16();
					player4.m_Rank = ((BinaryReader)(object)packetReader).ReadInt16();
				}
				break;
			case NetworkSessionComponent.PacketTypes.WeaponType:
				if (val != null && !val.IsLocal && ((Gamer)val).Tag is Player { m_Id: not -1 } player18)
				{
					sbyte weaponByType2 = ((BinaryReader)(object)packetReader).ReadSByte();
					player18.SetWeaponByType(weaponByType2);
				}
				break;
			case NetworkSessionComponent.PacketTypes.Team:
				if (val != null && !val.IsLocal && ((Gamer)val).Tag is Player { m_Id: not -1 } player15)
				{
					byte t2 = ((BinaryReader)(object)packetReader).ReadByte();
					player15.PeerSetTeam((Player.TEAM)t2);
				}
				break;
			case NetworkSessionComponent.PacketTypes.Class:
				if (val != null && !val.IsLocal && ((Gamer)val).Tag is Player { m_Id: not -1 } player10)
				{
					byte c3 = ((BinaryReader)(object)packetReader).ReadByte();
					player10.PeerSetClass((Player.CLASS)c3);
				}
				break;
			case NetworkSessionComponent.PacketTypes.Anim:
				if (val != null && !val.IsLocal && ((Gamer)val).Tag is Player { m_Id: not -1 } player9)
				{
					byte anim4 = ((BinaryReader)(object)packetReader).ReadByte();
					player9.PeerSetAnim(anim4);
				}
				break;
			case NetworkSessionComponent.PacketTypes.AnimUpper:
				if (val != null && !val.IsLocal && ((Gamer)val).Tag is Player { m_Id: not -1 } player5)
				{
					byte anim = ((BinaryReader)(object)packetReader).ReadByte();
					player5.PeerSetAnimUpper(anim);
				}
				break;
			case NetworkSessionComponent.PacketTypes.RankUp:
				if (val != null && !val.IsLocal && ((Gamer)val).Tag is Player { m_Id: not -1 } player)
				{
					player.PeerRankUp();
				}
				break;
			case NetworkSessionComponent.PacketTypes.CleanItems:
				if (val != null && !val.IsLocal && ((Gamer)val).Tag is Player { m_Id: not -1 } player17)
				{
					player17.CleanItems();
				}
				break;
			case NetworkSessionComponent.PacketTypes.UseCrate:
				if (val != null && !val.IsLocal && ((Gamer)val).Tag is Player { m_Id: not -1 })
				{
					short crateIdx = ((BinaryReader)(object)packetReader).ReadInt16();
					short playerNetId = ((BinaryReader)(object)packetReader).ReadInt16();
					int itemIndexByCrateIndex = g.m_ItemManager.GetItemIndexByCrateIndex(crateIdx);
					g.m_ItemManager.m_Item[itemIndexByCrateIndex].PeerUseCrate(crateIdx, playerNetId);
				}
				break;
			case NetworkSessionComponent.PacketTypes.CreateProjectile:
				if (val != null && !val.IsLocal)
				{
					Player player11 = ((Gamer)val).Tag as Player;
					short projectileNetId4 = ((BinaryReader)(object)packetReader).ReadInt16();
					byte projectileType2 = ((BinaryReader)(object)packetReader).ReadByte();
					Vector3 pos2 = packetReader.ReadVector3();
					Quaternion q2 = packetReader.ReadQuaternion();
					Vector3 vel2 = packetReader.ReadVector3();
					if (player11 != null && player11.m_Id != -1)
					{
						player11.PeerCreateProjectile(projectileNetId4, projectileType2, pos2, q2, vel2);
					}
				}
				break;
			case NetworkSessionComponent.PacketTypes.BotFireWeapon:
				if (val != null && !val.IsLocal)
				{
					short botId8 = ((BinaryReader)(object)packetReader).ReadInt16();
					if (g.m_PlayerManager.BotExists(botId8))
					{
						g.m_PlayerManager.GetBot(botId8).SimulateFireWeapon();
					}
				}
				break;
			case NetworkSessionComponent.PacketTypes.BotImpact:
				if (val != null && !val.IsLocal)
				{
					short botId6 = ((BinaryReader)(object)packetReader).ReadInt16();
					Vector3 zero = Vector3.Zero;
					zero.X = (float)((BinaryReader)(object)packetReader).ReadInt16() * 0.1f;
					zero.Y = (float)((BinaryReader)(object)packetReader).ReadInt16() * 0.1f;
					zero.Z = (float)((BinaryReader)(object)packetReader).ReadInt16() * 0.1f;
					sbyte b2 = ((BinaryReader)(object)packetReader).ReadSByte();
					sbyte b3 = ((BinaryReader)(object)packetReader).ReadSByte();
					sbyte b4 = ((BinaryReader)(object)packetReader).ReadSByte();
					Vector3 impactNormal = new Vector3((float)b2 / 127f, (float)b3 / 127f, (float)b4 / 127f);
					if (g.m_PlayerManager.BotExists(botId6))
					{
						g.m_PlayerManager.GetBot(botId6).SimulateWeaponImpact(zero, impactNormal);
					}
				}
				break;
			case NetworkSessionComponent.PacketTypes.BotTorchChanged:
				if (val != null && !val.IsLocal)
				{
					short botId3 = ((BinaryReader)(object)packetReader).ReadInt16();
					if (g.m_PlayerManager.BotExists(botId3))
					{
						g.m_PlayerManager.GetBot(botId3).ToggleTorchLight();
					}
				}
				break;
			case NetworkSessionComponent.PacketTypes.BotDoDamage:
				if (val != null && !val.IsLocal && ((Gamer)val).Tag is Player { m_Id: not -1 })
				{
					((BinaryReader)(object)packetReader).ReadInt16();
					sbyte damage = ((BinaryReader)(object)packetReader).ReadSByte();
					short netId = ((BinaryReader)(object)packetReader).ReadInt16();
					byte hitZone = ((BinaryReader)(object)packetReader).ReadByte();
					short attackerNetId = ((BinaryReader)(object)packetReader).ReadInt16();
					short projectileNetId = ((BinaryReader)(object)packetReader).ReadInt16();
					int playerExistsWithNetId = g.m_PlayerManager.GetPlayerExistsWithNetId(netId);
					if (playerExistsWithNetId != -1 && g.m_PlayerManager.m_Player[playerExistsWithNetId].m_Id != -1)
					{
						g.m_PlayerManager.m_Player[playerExistsWithNetId].DoDamage(damage, hitZone, attackerNetId, projectileNetId);
					}
				}
				break;
			case NetworkSessionComponent.PacketTypes.BotDeath:
				KillBot(val);
				break;
			case NetworkSessionComponent.PacketTypes.BotWeaponChanged:
				if (val != null && !val.IsLocal)
				{
					short botId14 = ((BinaryReader)(object)packetReader).ReadInt16();
					if (g.m_PlayerManager.BotExists(botId14))
					{
						g.m_PlayerManager.GetBot(botId14).PeerChangeWeapon();
					}
				}
				break;
			case NetworkSessionComponent.PacketTypes.BotSpawned:
				if (val != null && !val.IsLocal)
				{
					short botId13 = ((BinaryReader)(object)packetReader).ReadInt16();
					Vector3 pos5 = packetReader.ReadVector3();
					if (g.m_PlayerManager.BotExists(botId13))
					{
						g.m_PlayerManager.GetBot(botId13).PeerSpawned(pos5);
					}
				}
				break;
			case NetworkSessionComponent.PacketTypes.BotCrouch:
				if (val != null && !val.IsLocal)
				{
					short botId12 = ((BinaryReader)(object)packetReader).ReadInt16();
					bool crouch2 = ((BinaryReader)(object)packetReader).ReadBoolean();
					if (g.m_PlayerManager.BotExists(botId12))
					{
						g.m_PlayerManager.GetBot(botId12).PeerCrouch(crouch2);
					}
				}
				break;
			case NetworkSessionComponent.PacketTypes.BotScore:
				if (val != null && !val.IsLocal)
				{
					short botId11 = ((BinaryReader)(object)packetReader).ReadInt16();
					short score2 = ((BinaryReader)(object)packetReader).ReadInt16();
					short kills = ((BinaryReader)(object)packetReader).ReadInt16();
					short deaths = ((BinaryReader)(object)packetReader).ReadInt16();
					short rank = ((BinaryReader)(object)packetReader).ReadInt16();
					if (g.m_PlayerManager.BotExists(botId11))
					{
						g.m_PlayerManager.GetBot(botId11).m_Score = score2;
						g.m_PlayerManager.GetBot(botId11).m_Kills = kills;
						g.m_PlayerManager.GetBot(botId11).m_Deaths = deaths;
						g.m_PlayerManager.GetBot(botId11).m_Rank = rank;
					}
				}
				break;
			case NetworkSessionComponent.PacketTypes.BotWeaponType:
				if (val != null && !val.IsLocal)
				{
					short botId10 = ((BinaryReader)(object)packetReader).ReadInt16();
					int weaponByType = ((BinaryReader)(object)packetReader).ReadInt32();
					if (g.m_PlayerManager.BotExists(botId10))
					{
						g.m_PlayerManager.GetBot(botId10).SetWeaponByType(weaponByType);
					}
				}
				break;
			case NetworkSessionComponent.PacketTypes.BotTeam:
				if (val != null && !val.IsLocal)
				{
					short botId9 = ((BinaryReader)(object)packetReader).ReadInt16();
					byte t = ((BinaryReader)(object)packetReader).ReadByte();
					if (g.m_PlayerManager.BotExists(botId9))
					{
						g.m_PlayerManager.GetBot(botId9).PeerSetTeam((Player.TEAM)t);
					}
				}
				break;
			case NetworkSessionComponent.PacketTypes.BotClass:
				if (val != null && !val.IsLocal)
				{
					short botId7 = ((BinaryReader)(object)packetReader).ReadInt16();
					byte c2 = ((BinaryReader)(object)packetReader).ReadByte();
					if (g.m_PlayerManager.BotExists(botId7))
					{
						g.m_PlayerManager.GetBot(botId7).PeerSetClass((Player.CLASS)c2);
					}
				}
				break;
			case NetworkSessionComponent.PacketTypes.BotAnim:
				if (val != null && !val.IsLocal)
				{
					short botId5 = ((BinaryReader)(object)packetReader).ReadInt16();
					byte anim3 = ((BinaryReader)(object)packetReader).ReadByte();
					if (g.m_PlayerManager.BotExists(botId5))
					{
						g.m_PlayerManager.GetBot(botId5).PeerSetAnim(anim3);
					}
				}
				break;
			case NetworkSessionComponent.PacketTypes.BotAnimUpper:
				if (val != null && !val.IsLocal)
				{
					short botId4 = ((BinaryReader)(object)packetReader).ReadInt16();
					byte anim2 = ((BinaryReader)(object)packetReader).ReadByte();
					if (g.m_PlayerManager.BotExists(botId4))
					{
						g.m_PlayerManager.GetBot(botId4).PeerSetAnimUpper(anim2);
					}
				}
				break;
			case NetworkSessionComponent.PacketTypes.BotCreateProjectile:
				if (val != null && !val.IsLocal)
				{
					short botId2 = ((BinaryReader)(object)packetReader).ReadInt16();
					short projectileNetId2 = ((BinaryReader)(object)packetReader).ReadInt16();
					byte projectileType = ((BinaryReader)(object)packetReader).ReadByte();
					Vector3 pos = packetReader.ReadVector3();
					Quaternion q = packetReader.ReadQuaternion();
					Vector3 vel = packetReader.ReadVector3();
					if (g.m_PlayerManager.BotExists(botId2))
					{
						g.m_PlayerManager.GetBot(botId2).PeerCreateProjectile(projectileNetId2, projectileType, pos, q, vel);
					}
				}
				break;
			case NetworkSessionComponent.PacketTypes.ShipDeath:
				KillShip(val);
				break;
			case NetworkSessionComponent.PacketTypes.Intermission:
				if (val != null && !val.IsLocal)
				{
					PeerGoToIntermission();
				}
				break;
			case NetworkSessionComponent.PacketTypes.IntermissionEnd:
				if (val != null && !val.IsLocal)
				{
					PeerEndIntermission();
				}
				break;
			case NetworkSessionComponent.PacketTypes.DeleteBot:
				if (val != null && !val.IsLocal)
				{
					short botId = ((BinaryReader)(object)packetReader).ReadInt16();
					if (g.m_PlayerManager.BotExists(botId))
					{
						g.m_PlayerManager.GetBot(botId).RequestDelete();
					}
				}
				break;
			}
		}
	}

	private void KillShip(NetworkGamer sender)
	{
		if (sender != null && !sender.IsLocal && ((Gamer)sender).Tag is Player { m_Id: not -1 } player)
		{
			short lastAttackerNetId = ((BinaryReader)(object)packetReader).ReadInt16();
			short projectileNetId = ((BinaryReader)(object)packetReader).ReadInt16();
			player.Kill(lastAttackerNetId, projectileNetId);
		}
	}

	private void KillBot(NetworkGamer sender)
	{
		if (sender != null && !sender.IsLocal && ((Gamer)sender).Tag is Player { m_Id: not -1 })
		{
			short botId = ((BinaryReader)(object)packetReader).ReadInt16();
			short lastAttackerNetId = ((BinaryReader)(object)packetReader).ReadInt16();
			short projectileNetId = ((BinaryReader)(object)packetReader).ReadInt16();
			if (g.m_PlayerManager.BotExists(botId))
			{
				g.m_PlayerManager.GetBot(botId).Kill(lastAttackerNetId, projectileNetId);
			}
		}
	}

	private void UpdatePlayerData(NetworkGamer sender)
	{
		if (networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>)(object)networkSession.LocalGamers).Count > 0 && sender != null && ((Gamer)sender).Tag is Player { m_Id: not -1 } player)
		{
			player.m_Position = packetReader.ReadVector3();
			player.m_Rotation.Y = ((BinaryReader)(object)packetReader).ReadSingle();
			player.m_NetworkPosition = player.m_Position;
			player.m_NetworkRotation = player.m_Rotation.Y;
		}
	}

	private void UpdateShipData(NetworkGamer sender)
	{
		if (sender != null && ((Gamer)sender).Tag is Player { m_Id: not -1 } player)
		{
			player.m_NetworkPosition = packetReader.ReadVector3();
			player.m_NetworkRotation = ((BinaryReader)(object)packetReader).ReadSingle();
			sbyte b = ((BinaryReader)(object)packetReader).ReadSByte();
			player.m_PeerPitch = MathHelper.Lerp(player.m_PeerPitch, (float)b / 252f, 0.5f);
		}
	}

	private void UpdateBotData(NetworkGamer sender)
	{
		if (sender != null)
		{
			short botId = ((BinaryReader)(object)packetReader).ReadInt16();
			Vector3 networkPosition = packetReader.ReadVector3();
			float networkRotation = ((BinaryReader)(object)packetReader).ReadSingle();
			if (g.m_PlayerManager.BotExists(botId))
			{
				g.m_PlayerManager.GetBot(botId).m_NetworkPosition = networkPosition;
				g.m_PlayerManager.GetBot(botId).m_NetworkRotation = networkRotation;
			}
		}
	}
}
