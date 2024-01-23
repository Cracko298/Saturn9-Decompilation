using BEPUphysics;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.DataStructures;
using BEPUphysics.MathExtensions;
using DPSF;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using SynapseGaming.LightingSystem.Core;
using SynapseGaming.LightingSystem.Lights;
using SynapseGaming.LightingSystem.Rendering;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace Saturn9
{
  public class GameplayScreen : GameScreen
  {
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
    public GameplayScreen.ARTIFACTSTATE m_HostArtifactState;
    private float m_HostDroppedArtifactTimeout;

    private bool IsNetActive => this.networkSession == null ? this.IsActive : !this.IsExiting;

    public GameplayScreen(NetworkSession networkSession)
    {
      this.networkSession = networkSession;
      this.TransitionOnTime = TimeSpan.FromSeconds(0.05);
      this.TransitionOffTime = TimeSpan.FromSeconds(0.05);
      if (Gamer.SignedInGamers[g.m_App.m_PlayerOnePadId] != null)
        Gamer.SignedInGamers[g.m_App.m_PlayerOnePadId].Presence.PresenceMode = networkSession == null ? (GamerPresenceMode) 1 : (GamerPresenceMode) 2;
      this.m_StopWatchUpdate = new Stopwatch();
      this.m_StopWatchRender = new Stopwatch();
      this.m_StopWatchPhysics = new Stopwatch();
      this.m_StopWatchPlayers = new Stopwatch();
      this.m_StopWatchItems = new Stopwatch();
      this.m_StopWatchNetwork = new Stopwatch();
      this.m_StopWatchScene = new Stopwatch();
      this.m_StopWatchUpdateParticles = new Stopwatch();
      this.m_AlarmLight = new SpotLight[1];
      if (this.m_AirlockSmoke == null)
      {
        this.m_AirlockSmoke = new AirlockSmokeParticleSystem((Game) g.m_App);
        this.m_AirlockSmoke.AutoInitialize(g.m_App.GraphicsDevice, g.m_App.Content, g.m_App.screenManager.SpriteBatch);
        g.m_App.m_ParticleSystemManager.AddParticleSystem((IDPSFParticleSystem) this.m_AirlockSmoke);
      }
      this.m_AirlockSmoke.Enabled = false;
      this.m_AirlockSmoke.Visible = false;
      if (g.m_App.m_SurvivalMode)
        this.m_NextUnauthorisedDocking = 3f;
      else
        this.m_NextUnauthorisedDocking = 15f;
    }

    public override void LoadContent()
    {
      if (this.content == null)
        this.content = new ContentManager((IServiceProvider) ((GameComponent) this.ScreenManager).Game.Services, "Content");
      if (g.m_PlayerManager.GetLocalPlayer().IsHost())
      {
        this.HostReset();
        this.LoadSunburnScene();
      }
      ((GameComponent) this.ScreenManager).Game.ResetElapsedTime();
      g.m_CameraManager.Init();
      g.m_App.m_GameplayScreen = this;
    }

    private void HostReset()
    {
      this.m_HostSendCrateItemNetId = (short) -1;
      this.m_HostSendCrateItemToPeer = false;
      this.m_HostSendCrateItemType = byte.MaxValue;
      this.m_HostSendDetachItem = false;
      this.m_HostSendDetachItemPos = Vector3.Zero;
      this.m_HostSendDetachPlayerNetId = (short) -1;
      this.m_HostSendUseCrateAllow = false;
      this.m_HostSendUseCrateId = (short) -1;
      this.m_HostSendUsePlayerNetId = (short) -1;
      this.m_HostDroppedArtifactTimeout = 0.0f;
      this.SetHostArtifactState(GameplayScreen.ARTIFACTSTATE.NONE);
      this.m_HostSendDeleteNetworkItem = false;
      this.m_HostSendDeleteNetworkItemId = byte.MaxValue;
      this.m_HostEscapeCountDownTimer = 0.0f;
      this.m_HostSendEscapeCountdownMessage = false;
      this.m_HostEscapeCountDownStarted = false;
      this.m_HostSendArtifactReturnedMessage = false;
      this.m_HostSendCancelCountdownMessage = false;
      this.m_HostSendArtifactState = false;
      this.m_HostSendCrateSeed = false;
      this.m_HostCrateSeedValue = 0;
    }

    public void LoadSunburnScene()
    {
      g.m_App.scene = g.m_App.m_Scene1;
      g.m_App.ResetTriggers();
      g.m_App.sceneInterface.Submit((IScene) g.m_App.scene);
      g.m_App.environment = g.m_App.m_Environment1;
      Matrix world = g.m_App.sceneInterface.ObjectManager.Find<SceneObject>("Box01", false).World;
      Vector3 translation = ((Matrix) ref world).Translation;
      g.m_App.m_LCD1 = g.m_App.sceneInterface.ObjectManager.Find<SceneObject>("LCD1", false);
      g.m_App.m_LCD2 = g.m_App.sceneInterface.ObjectManager.Find<SceneObject>("LCD2", false);
      g.m_App.m_GRAVE = g.m_App.sceneInterface.ObjectManager.Find<SceneObject>("GRAVE", false);
      Vector3[] vertices;
      int[] indices;
      TriangleMesh.GetVerticesAndIndicesFromModel(g.m_App.m_CollisionModel1, out vertices, out indices);
      this.m_CollisionMesh = new StaticMesh(vertices, indices, new AffineTransform(translation));
      g.m_App.m_Space.Add((ISpaceObject) this.m_CollisionMesh);
      if (g.m_CameraManager.m_Helmet == -1)
        g.m_CameraManager.m_Helmet = g.m_ItemManager.Create(7, byte.MaxValue, Vector3.Zero, 0.0f, g.m_PlayerManager.GetLocalPlayer());
      g.m_CameraManager.Init();
      g.m_CameraManager.Update();
      g.m_App.m_RumbleFrames = 0;
      this.SetUpFlickerLights();
      this.SetupAlarmLights();
      if (this.networkSession != null && !this.networkSession.IsHost)
        return;
      g.m_ItemManager.SetUpTriggeredItems();
      g.m_ItemManager.SetUpFog();
    }

    public void SetUpFlickerLights()
    {
    }

    public void UpdateFlickerLights()
    {
    }

    public void SetupAlarmLights()
    {
      for (int index = 0; index < 1; ++index)
      {
        g.m_App.sceneInterface.LightManager.Find<SpotLight>(string.Format("AlarmLight{0}", (object) (index + 1)), false, out this.m_AlarmLight[index]);
        if (this.m_AlarmLight[index] != null)
          this.m_AlarmLight[index].Enabled = false;
      }
      g.m_App.sceneInterface.LightManager.Find<SpotLight>("AlarmPoint1", false, out this.m_LCDRedLight);
      g.m_App.sceneInterface.LightManager.Find<AmbientLight>("Ambient", false, out this.m_AmbientLight);
      this.SetBrightness(g.m_App.m_OptionsBrightness);
    }

    public void SetBrightness(int brightness)
    {
      if (this.m_AmbientLight != null)
        this.m_AmbientLight.Intensity = (float) (0.15000000596046448 + 0.039999999105930328 * (double) brightness);
      if (g.m_PlayerManager.GetLocalPlayer() == null || g.m_PlayerManager.GetLocalPlayer().m_TorchLight == null)
        return;
      g.m_PlayerManager.GetLocalPlayer().m_TorchLight.Intensity = (float) (2.5 + (double) brightness * 0.10000000149011612);
    }

    public void UpdateAlarmLights()
    {
      if (g.m_App == null || g.m_App.scene == null || this.m_AlarmLight == null || !g.m_App.m_OptionsLights)
        return;
      for (int index = 0; index < 1; ++index)
      {
        if (this.m_AlarmLight[index] != null && this.m_AlarmLight[index].Enabled)
        {
          Vector3 vector3 = Vector3.Transform(this.m_AlarmLight[index].Direction, Matrix.CreateRotationY((float) g.m_App.m_GameTime.ElapsedGameTime.TotalSeconds * MathHelper.ToRadians(960.12f)));
          this.m_AlarmLight[index].Direction = vector3;
          this.m_AlarmLight[index].Enabled = true;
        }
      }
    }

    public override void UnloadContent()
    {
      if (this.m_CollisionMesh != null)
      {
        g.m_App.m_Space.Remove((ISpaceObject) this.m_CollisionMesh);
        this.m_CollisionMesh = (StaticMesh) null;
      }
      if (g.m_App.m_AlarmSFX != null)
      {
        g.m_App.m_AlarmSFX.Stop();
        g.m_App.m_AlarmSFX = (SoundEffectInstance) null;
      }
      if (g.m_App.m_CargoLoopSFX != null)
      {
        g.m_App.m_CargoLoopSFX.Stop();
        g.m_App.m_CargoLoopSFX = (SoundEffectInstance) null;
      }
      if (g.m_PlayerManager.GetLocalPlayer() != null && g.m_PlayerManager.GetLocalPlayer().m_BreatheSFX != null)
      {
        g.m_PlayerManager.GetLocalPlayer().m_BreatheSFX.Stop();
        g.m_PlayerManager.GetLocalPlayer().m_BreatheSFX = (SoundEffectInstance) null;
      }
      if (g.m_PlayerManager.GetLocalPlayer() != null && g.m_PlayerManager.GetLocalPlayer().m_PhoneSFX != null)
      {
        g.m_PlayerManager.GetLocalPlayer().m_PhoneSFX.Stop();
        g.m_PlayerManager.GetLocalPlayer().m_PhoneSFX = (SoundEffectInstance) null;
      }
      this.content.Unload();
    }

    public override void Update(
      GameTime gameTime,
      bool otherScreenHasFocus,
      bool coveredByOtherScreen)
    {
      base.Update(gameTime, otherScreenHasFocus, false);
      this.pauseAlpha = !coveredByOtherScreen ? Math.Max(this.pauseAlpha - 1f / 32f, 0.0f) : Math.Min(this.pauseAlpha + 1f / 32f, 1f);
      this.m_StopWatchUpdate.Start();
      if (g.m_App.m_Paused && !g.m_App.m_SingleStep)
        return;
      g.m_App.m_SingleStep = false;
      if (this.IsNetActive && g.m_App.m_Intermission)
        this.UpdateNetworking((float) gameTime.ElapsedGameTime.TotalSeconds);
      if (this.IsNetActive && !g.m_App.m_Intermission)
      {
        g.m_App.sceneInterface.Update(gameTime);
        g.m_App.m_ParticleSystemManager.UpdateAllParticleSystems((float) gameTime.ElapsedGameTime.TotalSeconds);
        this.UpdateNetworking((float) gameTime.ElapsedGameTime.TotalSeconds);
        g.m_PlayerManager.ClearDamageRequests();
        g.m_PlayerManager.Update();
        g.m_ItemManager.Update();
        g.m_ProjectileManager.Update();
        g.m_CameraManager.Update();
        g.m_BotPathManager.Update();
        g.m_PlayerManager.UpdatePrevousPositions();
        this.m_StopWatchPhysics.Start();
        g.m_App.m_Space.Update();
        this.m_StopWatchPhysics.Stop();
        this.UpdateFlickerLights();
        this.UpdateAlarmLights();
      }
      if ((double) g.m_PlayerManager.m_GameCompleteTimer != 0.0 && (double) g.m_PlayerManager.m_GameCompleteTimer < g.m_App.m_GameTime.TotalGameTime.TotalSeconds)
        LoadingScreen.Load(this.ScreenManager, true, new PlayerIndex?(), 0, (GameScreen) new GameCompleteScreen());
      if (this.networkSession != null && !this.IsExiting && this.networkSession.SessionState == null)
        LoadingScreen.Load(this.ScreenManager, true, new PlayerIndex?(), 0, (GameScreen) new BackgroundScreen(), (GameScreen) new LobbyScreen(this.networkSession));
      if (this.networkSession == null || this.networkSession.IsHost)
      {
        int num1 = g.m_App.m_Intermission ? 1 : 0;
      }
      this.PeerCheckAirlockSmoke();
      if ((this.networkSession == null || this.networkSession.IsHost) && g.m_App.m_Intermission && (double) g.m_App.m_IntermissionTime < g.m_App.m_GameTime.TotalGameTime.TotalSeconds)
        this.EndIntermission();
      this.m_StopWatchUpdate.Stop();
      if (this.networkSession == null)
        return;
      int num2 = this.networkSession.IsHost ? 1 : 0;
    }

    private void CheckArtifactDrop()
    {
      for (int index = 0; index < 16; ++index)
      {
        if (g.m_PlayerManager.m_Player[index].m_Id != -1 && g.m_PlayerManager.m_Player[index].m_AttachedItemId != -1 && g.m_PlayerManager.m_Player[index].IsDead())
        {
          this.HostDropArtifactForPlayer(g.m_PlayerManager.m_Player[index]);
          g.m_PlayerManager.m_Player[index].PeerDetach();
        }
      }
    }

    public void HostDropArtifactForPlayer(Player playerData)
    {
      this.m_HostSendDetachItem = true;
      this.m_HostSendDetachPlayerNetId = playerData.m_NetId;
      this.m_HostSendDetachItemPos = playerData.m_PrevPosition;
      this.m_HostDroppedArtifactTimeout = (float) g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 30f;
      this.SetHostArtifactState(GameplayScreen.ARTIFACTSTATE.DROPPED);
      g.m_ItemManager.m_Item[playerData.m_AttachedItemId].NetItemSetPosition(playerData.m_Position);
    }

    private void CheckGameOver()
    {
      if (!g.m_PlayerManager.IsAnyPlayerDead())
        return;
      if ((int) g.m_PlayerManager.GetLocalPlayer().m_Score > g.m_App.m_SurvivalScoreBest)
        g.m_App.m_SurvivalScoreBest = (int) g.m_PlayerManager.GetLocalPlayer().m_Score;
      this.GoToIntermission();
    }

    private float DistancePointLine(Vector3 Point, Vector3 LineStart, Vector3 LineEnd)
    {
      Vector3 zero = Vector3.Zero;
      Vector3 vector3_1 = Vector3.op_Subtraction(LineEnd, LineStart);
      float num1 = ((Vector3) ref vector3_1).Length();
      float num2 = (float) ((((double) Point.X - (double) LineStart.X) * ((double) LineEnd.X - (double) LineStart.X) + ((double) Point.Y - (double) LineStart.Y) * ((double) LineEnd.Y - (double) LineStart.Y) + ((double) Point.Z - (double) LineStart.Z) * ((double) LineEnd.Z - (double) LineStart.Z)) / ((double) num1 * (double) num1));
      if ((double) num2 < 0.0 || (double) num2 > 1.0)
        return 1000000f;
      zero.X = LineStart.X + num2 * (LineEnd.X - LineStart.X);
      zero.Y = LineStart.Y + num2 * (LineEnd.Y - LineStart.Y);
      zero.Z = LineStart.Z + num2 * (LineEnd.Z - LineStart.Z);
      Vector3 vector3_2 = Vector3.op_Subtraction(Point, zero);
      return ((Vector3) ref vector3_2).Length();
    }

    private void PeerCheckAirlockSmoke()
    {
      if ((double) this.m_StartAirlockSmokeTime >= g.m_App.m_GameTime.TotalGameTime.TotalSeconds || this.m_AirlockSmoke.Enabled || !this.m_HostEscapeCountDownStarted)
        return;
      this.m_AirlockSmoke.Emitter.PositionData.Position = this.m_AirlockSmokePos;
      this.m_AirlockSmoke.m_ExplosionDir = this.m_AirlockSmokeDir;
      this.m_AirlockSmoke.Emitter.PositionData.Velocity = new Vector3(0.0f, 0.0f, 0.0f);
      this.m_AirlockSmoke.LerpEmittersPositionAndOrientationOnNextUpdate = false;
      this.m_AirlockSmoke.Emitter.BurstTime = 3f;
      this.m_AirlockSmoke.Enabled = true;
      this.m_AirlockSmoke.Visible = true;
    }

    private void SetHostArtifactState(GameplayScreen.ARTIFACTSTATE state)
    {
      this.m_HostArtifactState = state;
      this.m_HostSendArtifactState = true;
    }

    private void PeerSetHostArtifactState(GameplayScreen.ARTIFACTSTATE state) => this.m_HostArtifactState = state;

    private void GoToIntermission()
    {
      float totalSeconds = (float) g.m_App.m_GameTime.TotalGameTime.TotalSeconds;
      g.m_App.m_RequestIntermission = true;
      g.m_App.m_Intermission = true;
      g.m_App.m_IntermissionTime = totalSeconds + 3f;
      this.m_BalanceBotTime = totalSeconds + 30f;
      this.HostReset();
      if (g.m_App.m_AlarmSFX != null)
      {
        g.m_App.m_AlarmSFX.Stop();
        g.m_App.m_AlarmSFX = (SoundEffectInstance) null;
      }
      if (g.m_App.m_CargoLoopSFX != null)
      {
        g.m_App.m_CargoLoopSFX.Stop();
        g.m_App.m_CargoLoopSFX = (SoundEffectInstance) null;
      }
      if (g.m_PlayerManager.GetLocalPlayer().m_BreatheSFX != null)
      {
        g.m_PlayerManager.GetLocalPlayer().m_BreatheSFX.Stop();
        g.m_PlayerManager.GetLocalPlayer().m_BreatheSFX = (SoundEffectInstance) null;
      }
      if (g.m_PlayerManager.GetLocalPlayer().m_PhoneSFX != null)
      {
        g.m_PlayerManager.GetLocalPlayer().m_PhoneSFX.Stop();
        g.m_PlayerManager.GetLocalPlayer().m_PhoneSFX = (SoundEffectInstance) null;
      }
      this.m_HostEscapeCountDownStarted = false;
      this.m_HostEscapeCountDownTimer = 0.0f;
      this.m_AirlockSmoke.Enabled = false;
      this.m_AirlockSmoke.Visible = false;
    }

    public void EndIntermission()
    {
      g.m_App.m_RequestIntermissionEnd = true;
      this.PeerEndIntermission();
    }

    private void PeerEndIntermission()
    {
      g.m_App.m_Space.Remove((ISpaceObject) this.m_CollisionMesh);
      this.m_CollisionMesh = (StaticMesh) null;
      g.m_App.sceneInterface.Clear();
      g.m_ItemManager.DeleteAll();
      if (g.m_CameraManager.m_Helmet != -1)
      {
        g.m_ItemManager.Delete(g.m_CameraManager.m_Helmet);
        g.m_CameraManager.m_Helmet = -1;
      }
      g.m_PlayerManager.ReInitPlayers();
      this.LoadSunburnScene();
      g.m_BotPathManager.LoadBotPath();
      ((GameComponent) this.ScreenManager).Game.ResetElapsedTime();
      g.m_CameraManager.Init();
      g.m_PlayerManager.ReInitTorches();
      if (g.m_PlayerManager.GetLocalPlayer().IsHost())
        g.m_PlayerManager.ReInitBots();
      GameScreen[] screens = g.m_App.screenManager.GetScreens();
      for (int index = 0; index < screens.Length; ++index)
      {
        if (screens[index] is JoinTeamMenuScreen)
          screens[index].ScreenManager.RemoveScreen(screens[index]);
      }
      g.m_PlayerManager.GetLocalPlayer().m_State = Player.STATE.JoinTeam;
      if (g.m_App.m_AlarmSFX != null)
      {
        g.m_App.m_AlarmSFX.Stop();
        g.m_App.m_AlarmSFX = (SoundEffectInstance) null;
      }
      if (g.m_App.m_CargoLoopSFX != null)
      {
        g.m_App.m_CargoLoopSFX.Stop();
        g.m_App.m_CargoLoopSFX = (SoundEffectInstance) null;
      }
      g.m_App.m_Intermission = false;
      g.m_App.m_ForceJoin = true;
      GC.Collect();
    }

    private void PeerGoToIntermission()
    {
      g.m_App.m_Intermission = true;
      g.m_App.m_IntermissionTime = (float) (g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 3.0);
      if (g.m_App.m_AlarmSFX != null)
      {
        g.m_App.m_AlarmSFX.Stop();
        g.m_App.m_AlarmSFX = (SoundEffectInstance) null;
      }
      if (g.m_App.m_CargoLoopSFX != null)
      {
        g.m_App.m_CargoLoopSFX.Stop();
        g.m_App.m_CargoLoopSFX = (SoundEffectInstance) null;
      }
      if (g.m_PlayerManager.GetLocalPlayer().m_BreatheSFX != null)
      {
        g.m_PlayerManager.GetLocalPlayer().m_BreatheSFX.Stop();
        g.m_PlayerManager.GetLocalPlayer().m_BreatheSFX = (SoundEffectInstance) null;
      }
      if (g.m_PlayerManager.GetLocalPlayer().m_PhoneSFX != null)
      {
        g.m_PlayerManager.GetLocalPlayer().m_PhoneSFX.Stop();
        g.m_PlayerManager.GetLocalPlayer().m_PhoneSFX = (SoundEffectInstance) null;
      }
      this.m_HostEscapeCountDownStarted = false;
      this.m_HostEscapeCountDownTimer = 0.0f;
      this.m_AirlockSmoke.Enabled = false;
      this.m_AirlockSmoke.Visible = false;
    }

    private void UpdateBotSpawner()
    {
      if (g.m_PlayerManager.NumBots() > 5 && !g.m_App.m_SurvivalMode)
        g.m_PlayerManager.RemoveBot();
      bool flag = false;
      float num1 = 15f;
      int num2 = 5;
      if (g.m_App.m_SurvivalMode)
      {
        flag = true;
        num1 = 3f;
        num2 = 8;
      }
      if (g.m_PlayerManager.GetLocalPlayer() == null || g.m_PlayerManager.GetLocalPlayer().m_State != Player.STATE.InGame || g.m_PlayerManager.GetLocalPlayer().m_Team == Player.TEAM.None)
        return;
      if (g.m_App.m_NetworkSession != null)
      {
        if (g.m_App.m_OptionsBotsMP <= 0 && !flag)
          return;
        int num3 = g.m_App.m_OptionsBotsMP - (g.m_PlayerManager.NumPlayersOnTeams() - 1);
        if (flag)
          num3 = num2;
        if (g.m_PlayerManager.NumBots() < num3)
        {
          g.m_BotPathManager.LoadBotPath();
          this.SendCreateBot(Player.TEAM.Hunter);
        }
        else
        {
          if (g.m_PlayerManager.NumBots() <= num3)
            return;
          g.m_PlayerManager.RemoveBot();
        }
      }
      else
      {
        if ((double) this.m_NextUnauthorisedDocking < g.m_App.m_GameTime.TotalGameTime.TotalSeconds && (g.m_PlayerManager.NumBots() < g.m_App.m_ArtifactsFoundSP + 2 || flag) && g.m_PlayerManager.NumBots() < num2)
        {
          g.m_BotPathManager.LoadBotPath();
          g.m_PlayerManager.Create(g.m_PlayerManager.GetNextBotId(), true, Player.TEAM.Hunter);
          this.m_NextUnauthorisedDocking = (float) g.m_App.m_GameTime.TotalGameTime.TotalSeconds + num1;
        }
        int num4 = 2;
        if (g.m_App.m_SurvivalMode)
          num4 = num2;
        if (!Guide.IsTrialMode || (double) this.m_NextUnauthorisedDocking >= g.m_App.m_GameTime.TotalGameTime.TotalSeconds || g.m_PlayerManager.NumBots() >= num4)
          return;
        g.m_BotPathManager.LoadBotPath();
        g.m_PlayerManager.Create(g.m_PlayerManager.GetNextBotId(), true, Player.TEAM.Hunter);
        this.m_NextUnauthorisedDocking = (float) g.m_App.m_GameTime.TotalGameTime.TotalSeconds + num1;
      }
    }

    public override void HandleInput(InputState input)
    {
      if (input == null)
        throw new ArgumentNullException(nameof (input));
      if (this.ControllingPlayer.HasValue)
      {
        this.HandlePlayerInput(input, this.ControllingPlayer.Value);
      }
      else
      {
        if (this.networkSession == null)
          return;
        foreach (LocalNetworkGamer localGamer in this.networkSession.LocalGamers)
        {
          if (!this.HandlePlayerInput(input, localGamer.SignedInGamer.PlayerIndex))
            break;
        }
      }
    }

    private bool HandlePlayerInput(InputState input, PlayerIndex playerIndex)
    {
      this.m_KeyboardState = input.CurrentKeyboardStates[playerIndex];
      this.m_GamepadState = input.CurrentGamePadStates[playerIndex];
      bool flag = !((GamePadState) ref this.m_GamepadState).IsConnected && input.GamePadWasConnected[playerIndex];
      if (input.IsPauseGame(new PlayerIndex?(playerIndex)) || flag)
      {
        this.ScreenManager.AddScreen((GameScreen) new PauseMenuScreen(this.networkSession), new PlayerIndex?(playerIndex));
        return false;
      }
      this.movement = Vector2.Zero;
      this.jump = false;
      this.leap = false;
      this.turn = 0.0f;
      this.UpdateGamepadControl();
      if ((double) ((Vector2) ref this.movement).Length() > 1.0)
        ((Vector2) ref this.movement).Normalize();
      g.m_PlayerManager.GetLocalPlayer().m_Movement = this.movement;
      g.m_PlayerManager.GetLocalPlayer().m_Turn = this.turn;
      g.m_PlayerManager.GetLocalPlayer().m_Jump = this.jump;
      g.m_PlayerManager.GetLocalPlayer().m_Leap = this.leap;
      this.m_OldGamepadState = this.m_GamepadState;
      this.m_OldKeyboardState = this.m_KeyboardState;
      g.m_PlayerManager.GetLocalPlayer().UpdateRumble(playerIndex);
      return true;
    }

    private void UpdateGamepadControl()
    {
      if (g.m_PlayerManager.GetLocalPlayer().m_State == Player.STATE.InGame || g.m_PlayerManager.GetLocalPlayer().m_State == Player.STATE.LocalDeath)
      {
        GamePadButtons buttons = ((GamePadState) ref this.m_GamepadState).Buttons;
        g.m_App.m_ShowScoreboard = ((GamePadButtons) ref buttons).Back == 1;
      }
      if (g.m_PlayerManager.GetLocalPlayer().m_State == Player.STATE.LocalDeath || g.m_PlayerManager.GetLocalPlayer().m_State == Player.STATE.ChooseCharacter || g.m_PlayerManager.GetLocalPlayer().m_State == Player.STATE.Intermission || g.m_PlayerManager.GetLocalPlayer().m_State == Player.STATE.JoinTeam || g.m_App.m_Intermission)
        return;
      float num1 = 1f;
      GamePadThumbSticks thumbSticks1 = ((GamePadState) ref this.m_GamepadState).ThumbSticks;
      if ((double) Math.Abs(((GamePadThumbSticks) ref thumbSticks1).Left.Y) > 0.10000000149011612 && g.m_CameraManager.m_LookAtPlayerId == -1)
      {
        ref Vector2 local = ref this.movement;
        double y = (double) local.Y;
        GamePadThumbSticks thumbSticks2 = ((GamePadState) ref this.m_GamepadState).ThumbSticks;
        double num2 = (double) ((GamePadThumbSticks) ref thumbSticks2).Left.Y * (double) num1;
        local.Y = (float) (y - num2);
      }
      GamePadThumbSticks thumbSticks3 = ((GamePadState) ref this.m_GamepadState).ThumbSticks;
      if ((double) Math.Abs(((GamePadThumbSticks) ref thumbSticks3).Left.X) > 0.10000000149011612 && g.m_CameraManager.m_LookAtPlayerId == -1)
      {
        ref Vector2 local = ref this.movement;
        double x = (double) local.X;
        GamePadThumbSticks thumbSticks4 = ((GamePadState) ref this.m_GamepadState).ThumbSticks;
        double num3 = (double) ((GamePadThumbSticks) ref thumbSticks4).Left.X * (double) num1;
        local.X = (float) (x + num3);
      }
      GamePadDPad dpad1 = ((GamePadState) ref this.m_GamepadState).DPad;
      if (((GamePadDPad) ref dpad1).Up == 1 && g.m_CameraManager.m_LookAtPlayerId == -1)
        this.movement.Y = -1f * num1;
      GamePadDPad dpad2 = ((GamePadState) ref this.m_GamepadState).DPad;
      if (((GamePadDPad) ref dpad2).Down == 1 && g.m_CameraManager.m_LookAtPlayerId == -1)
        this.movement.Y = 1f * num1;
      GamePadDPad dpad3 = ((GamePadState) ref this.m_GamepadState).DPad;
      if (((GamePadDPad) ref dpad3).Left == 1 && g.m_CameraManager.m_LookAtPlayerId == -1)
        this.movement.X = -1f * num1;
      GamePadDPad dpad4 = ((GamePadState) ref this.m_GamepadState).DPad;
      if (((GamePadDPad) ref dpad4).Right == 1 && g.m_CameraManager.m_LookAtPlayerId == -1)
        this.movement.X = 1f * num1;
      GamePadThumbSticks thumbSticks5 = ((GamePadState) ref this.m_GamepadState).ThumbSticks;
      if ((double) Math.Abs(((GamePadThumbSticks) ref thumbSticks5).Right.X) > 0.10000000149011612 && g.m_CameraManager.m_LookAtPlayerId == -1)
      {
        GamePadThumbSticks thumbSticks6 = ((GamePadState) ref this.m_GamepadState).ThumbSticks;
        double x1 = (double) ((GamePadThumbSticks) ref thumbSticks6).Right.X;
        GamePadThumbSticks thumbSticks7 = ((GamePadState) ref this.m_GamepadState).ThumbSticks;
        double x2 = (double) ((GamePadThumbSticks) ref thumbSticks7).Right.X;
        double num4 = x1 * x2;
        GamePadThumbSticks thumbSticks8 = ((GamePadState) ref this.m_GamepadState).ThumbSticks;
        double x3 = (double) ((GamePadThumbSticks) ref thumbSticks8).Right.X;
        this.turn = (float) (num4 * x3) * num1;
      }
      float num5 = g.m_App.m_OptionsInvertY ? -1f : 1f;
      GamePadThumbSticks thumbSticks9 = ((GamePadState) ref this.m_GamepadState).ThumbSticks;
      if ((double) Math.Abs(((GamePadThumbSticks) ref thumbSticks9).Right.Y) > 0.10000000149011612 && g.m_CameraManager.m_LookAtPlayerId == -1)
      {
        CameraManager cameraManager = g.m_CameraManager;
        double pitch = (double) cameraManager.m_Pitch;
        GamePadThumbSticks thumbSticks10 = ((GamePadState) ref this.m_GamepadState).ThumbSticks;
        double y1 = (double) ((GamePadThumbSticks) ref thumbSticks10).Right.Y;
        GamePadThumbSticks thumbSticks11 = ((GamePadState) ref this.m_GamepadState).ThumbSticks;
        double y2 = (double) ((GamePadThumbSticks) ref thumbSticks11).Right.Y;
        double num6 = y1 * y2;
        GamePadThumbSticks thumbSticks12 = ((GamePadState) ref this.m_GamepadState).ThumbSticks;
        double y3 = (double) ((GamePadThumbSticks) ref thumbSticks12).Right.Y;
        double num7 = num6 * y3 * 0.014999999664723873 * (double) num1 * (60.0 * g.m_App.m_GameTime.ElapsedGameTime.TotalSeconds) * ((double) g.m_App.m_OptionsVert * 0.40000000596046448) * (double) num5;
        cameraManager.m_Pitch = (float) (pitch + num7);
      }
      if ((double) g.m_CameraManager.m_Pitch < -1.1000000238418579)
        g.m_CameraManager.m_Pitch = -1.1f;
      if ((double) g.m_CameraManager.m_Pitch > 1.2000000476837158)
        g.m_CameraManager.m_Pitch = 1.2f;
      GamePadTriggers triggers1 = ((GamePadState) ref this.m_GamepadState).Triggers;
      if ((double) ((GamePadTriggers) ref triggers1).Right > 0.10000000149011612)
      {
        g.m_PlayerManager.GetLocalPlayer().m_bFired = true;
        bool bDebounced = false;
        GamePadTriggers triggers2 = ((GamePadState) ref this.m_OldGamepadState).Triggers;
        if ((double) ((GamePadTriggers) ref triggers2).Right < 0.10000000149011612)
          bDebounced = true;
        g.m_PlayerManager.GetLocalPlayer().FireWeapon(bDebounced);
      }
      g.m_PlayerManager.GetLocalPlayer().m_bSprinting = false;
      GamePadTriggers triggers3 = ((GamePadState) ref this.m_GamepadState).Triggers;
      if ((double) ((GamePadTriggers) ref triggers3).Left > 0.10000000149011612 && Vector2.op_Inequality(this.movement, Vector2.Zero) && g.m_PlayerManager.GetLocalPlayer().m_Sprint > 0)
      {
        --g.m_PlayerManager.GetLocalPlayer().m_Sprint;
        g.m_PlayerManager.GetLocalPlayer().m_bSprinting = true;
      }
      if (this.Debounce((Buttons) 512) && g.m_PlayerManager.GetLocalPlayer().m_Team == Player.TEAM.Hunter)
      {
        g.m_PlayerManager.GetLocalPlayer().ToggleTorchLight();
        g.m_PlayerManager.GetLocalPlayer().m_bTorchChanged = true;
      }
      if (this.Debounce((Buttons) 4096))
        g.m_PlayerManager.GetLocalPlayer().Interact();
      if (this.Debounce((Buttons) 8192))
        g.m_PlayerManager.GetLocalPlayer().InteractBack();
      if (this.Debounce((Buttons) 16384))
        g.m_PlayerManager.GetLocalPlayer().InteractX();
      if (!this.Debounce((Buttons) 32768) || g.m_PlayerManager.GetLocalPlayer().InteractY() || g.m_PlayerManager.GetLocalPlayer().NumWeapons() <= 1)
        return;
      g.m_PlayerManager.GetLocalPlayer().HolsterWeapon();
      g.m_PlayerManager.GetLocalPlayer().m_bWeaponChanged = true;
    }

    private void UpdateKeyboardControl()
    {
    }

    public bool Debounce(Buttons b)
    {
      Buttons buttons1 = b;
      if (buttons1 <= 512)
      {
        if (buttons1 <= 32)
        {
          if (buttons1 != 16)
          {
            if (buttons1 == 32)
            {
              GamePadButtons buttons2 = ((GamePadState) ref this.m_GamepadState).Buttons;
              if (((GamePadButtons) ref buttons2).Back == 1)
              {
                GamePadButtons buttons3 = ((GamePadState) ref this.m_OldGamepadState).Buttons;
                if (((GamePadButtons) ref buttons3).Back == null)
                  return true;
              }
            }
          }
          else
          {
            GamePadButtons buttons4 = ((GamePadState) ref this.m_GamepadState).Buttons;
            if (((GamePadButtons) ref buttons4).Start == 1)
            {
              GamePadButtons buttons5 = ((GamePadState) ref this.m_OldGamepadState).Buttons;
              if (((GamePadButtons) ref buttons5).Start == null)
                return true;
            }
          }
        }
        else if (buttons1 != 256)
        {
          if (buttons1 == 512)
          {
            GamePadButtons buttons6 = ((GamePadState) ref this.m_GamepadState).Buttons;
            if (((GamePadButtons) ref buttons6).RightShoulder == 1)
            {
              GamePadButtons buttons7 = ((GamePadState) ref this.m_OldGamepadState).Buttons;
              if (((GamePadButtons) ref buttons7).RightShoulder == null)
                return true;
            }
          }
        }
        else
        {
          GamePadButtons buttons8 = ((GamePadState) ref this.m_GamepadState).Buttons;
          if (((GamePadButtons) ref buttons8).LeftShoulder == 1)
          {
            GamePadButtons buttons9 = ((GamePadState) ref this.m_OldGamepadState).Buttons;
            if (((GamePadButtons) ref buttons9).LeftShoulder == null)
              return true;
          }
        }
      }
      else if (buttons1 <= 8192)
      {
        if (buttons1 != 4096)
        {
          if (buttons1 == 8192)
          {
            GamePadButtons buttons10 = ((GamePadState) ref this.m_GamepadState).Buttons;
            if (((GamePadButtons) ref buttons10).B == 1)
            {
              GamePadButtons buttons11 = ((GamePadState) ref this.m_OldGamepadState).Buttons;
              if (((GamePadButtons) ref buttons11).B == null)
                return true;
            }
          }
        }
        else
        {
          GamePadButtons buttons12 = ((GamePadState) ref this.m_GamepadState).Buttons;
          if (((GamePadButtons) ref buttons12).A == 1)
          {
            GamePadButtons buttons13 = ((GamePadState) ref this.m_OldGamepadState).Buttons;
            if (((GamePadButtons) ref buttons13).A == null)
              return true;
          }
        }
      }
      else if (buttons1 != 16384)
      {
        if (buttons1 == 32768)
        {
          GamePadButtons buttons14 = ((GamePadState) ref this.m_GamepadState).Buttons;
          if (((GamePadButtons) ref buttons14).Y == 1)
          {
            GamePadButtons buttons15 = ((GamePadState) ref this.m_OldGamepadState).Buttons;
            if (((GamePadButtons) ref buttons15).Y == null)
              return true;
          }
        }
      }
      else
      {
        GamePadButtons buttons16 = ((GamePadState) ref this.m_GamepadState).Buttons;
        if (((GamePadButtons) ref buttons16).X == 1)
        {
          GamePadButtons buttons17 = ((GamePadState) ref this.m_OldGamepadState).Buttons;
          if (((GamePadButtons) ref buttons17).X == null)
            return true;
        }
      }
      return false;
    }

    public override void Draw(GameTime gameTime)
    {
      if (this.ScreenState == ScreenState.TransitionOff || this.ScreenState == ScreenState.TransitionOn)
      {
        this.ScreenManager.GraphicsDevice.Clear(Color.Black);
      }
      else
      {
        g.m_App.DrawHelmetHud();
        g.m_PlayerManager.GetLocalPlayer().DrawLCD();
        g.m_PlayerManager.GetLocalPlayer().DrawGrave();
        g.m_PlayerManager.GetLocalPlayer().DrawTablet();
        g.m_App.sceneState.BeginFrameRendering(g.m_CameraManager.m_ViewMatrix, g.m_CameraManager.m_ProjectionMatrix, gameTime, (ISceneEnvironment) g.m_App.environment, g.m_App.frameBuffers, true);
        g.m_App.sceneInterface.BeginFrameRendering((ISceneState) g.m_App.sceneState);
        g.m_App.sceneInterface.RenderManager.Render();
        g.m_App.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.None;
        g.m_App.m_ParticleSystemManager.SetCameraPositionForAllParticleSystems(g.m_CameraManager.m_Position);
        g.m_App.m_ParticleSystemManager.SetWorldViewProjectionMatricesForAllParticleSystems(Matrix.Identity, g.m_CameraManager.m_ViewMatrix, g.m_CameraManager.m_ProjectionMatrix);
        g.m_App.m_ParticleSystemManager.DrawAllParticleSystems();
        g.m_App.screenManager.SpriteBatch.Begin();
        g.m_App.DrawHud();
        if ((double) g.m_App.m_ShowPermissionTime > gameTime.TotalGameTime.TotalSeconds)
          g.m_App.screenManager.SpriteBatch.DrawString(g.m_App.hudFont, "No permissions to buy with this Profile", new Vector2(220f, 400f), Color.Red);
        g.m_App.screenManager.SpriteBatch.End();
        g.m_App.sceneInterface.EndFrameRendering();
        g.m_App.sceneState.EndFrameRendering();
        this.m_StopWatchRender.Stop();
        this.UpdateDebugStats();
      }
    }

    protected void RenderSky(ISceneState scenestate)
    {
      if (this.skybox == null)
      {
        g.m_App.GraphicsDevice.Clear((ClearOptions) 7, Color.Black, 1f, 0);
      }
      else
      {
        g.m_App.GraphicsDevice.Clear((ClearOptions) 6, Color.Black, 1f, 0);
        g.m_App.GraphicsDevice.BlendState = BlendState.Opaque;
        g.m_App.GraphicsDevice.DepthStencilState = DepthStencilState.None;
        g.m_App.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        g.m_App.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;
        for (int index = 1; index < 8; ++index)
          g.m_App.GraphicsDevice.SamplerStates[index] = SamplerState.PointWrap;
        Matrix view = scenestate.View;
        ((Matrix) ref view).Translation = Vector3.Zero;
        foreach (ModelMesh mesh in this.skybox.Meshes)
        {
          foreach (Effect effect in mesh.Effects)
          {
            if (effect is BasicEffect)
            {
              BasicEffect basicEffect = effect as BasicEffect;
              basicEffect.LightingEnabled = false;
              basicEffect.DiffuseColor = new Vector3(1f, 1f, 1f);
              basicEffect.View = view;
              basicEffect.World = this.skyboxWorld;
              basicEffect.Projection = scenestate.Projection;
            }
          }
          mesh.Draw();
        }
        g.m_App.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
      }
    }

    private void UpdateDebugStats()
    {
      if ((double) this.m_DebugStatUpdateTime < g.m_App.m_GameTime.TotalGameTime.TotalSeconds)
      {
        this.m_ParticleTime = (float) this.m_StopWatchUpdateParticles.Elapsed.TotalMilliseconds;
        this.m_SceneTime = (float) this.m_StopWatchScene.Elapsed.TotalMilliseconds;
        this.m_NetworkTime = (float) this.m_StopWatchNetwork.Elapsed.TotalMilliseconds;
        this.m_ItemTime = (float) this.m_StopWatchItems.Elapsed.TotalMilliseconds;
        this.m_PhysicsTime = (float) this.m_StopWatchPhysics.Elapsed.TotalMilliseconds;
        this.m_PlayerTime = (float) this.m_StopWatchPlayers.Elapsed.TotalMilliseconds;
        this.m_UpdateTime = (float) this.m_StopWatchUpdate.Elapsed.TotalMilliseconds;
        this.m_RenderTime = (float) this.m_StopWatchRender.Elapsed.TotalMilliseconds;
        this.m_DebugStatUpdateTime = (float) g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 0.1f;
      }
      this.m_StopWatchUpdate.Reset();
      this.m_StopWatchRender.Reset();
      this.m_StopWatchPhysics.Reset();
      this.m_StopWatchPlayers.Reset();
      this.m_StopWatchItems.Reset();
      this.m_StopWatchNetwork.Reset();
      this.m_StopWatchScene.Reset();
      this.m_StopWatchUpdateParticles.Reset();
    }

    private void DrawDebugStats(GameTime gameTime)
    {
      Viewport viewport1 = g.m_App.GraphicsDevice.Viewport;
      int height = ((Viewport) ref viewport1).Bounds.Height;
      Viewport viewport2 = g.m_App.GraphicsDevice.Viewport;
      int width = ((Viewport) ref viewport2).Bounds.Width;
      this.FPStotalSinceLast += gameTime.ElapsedGameTime.TotalSeconds;
      ++this.FPStotalFramesSinceLast;
      if (gameTime.TotalGameTime.TotalSeconds - this.FPSlastTime > 0.0 && gameTime.ElapsedGameTime.TotalSeconds > 0.0)
      {
        double num = this.FPStotalSinceLast / (double) this.FPStotalFramesSinceLast;
        this.FPSlastTime = gameTime.TotalGameTime.TotalSeconds;
        this.FPStoDisplay = Math.Round(1.0 / num, 1);
        this.FPStotalSinceLast = 0.0;
        this.FPStotalFramesSinceLast = 0;
      }
      g.m_App.screenManager.SpriteBatch.DrawString(g.m_App.lcdFont, string.Format("FPS: {0}", (object) this.FPStoDisplay), new Vector2(300f, 470f), Color.op_Multiply(Color.White, 0.25f));
    }

    private void UpdateNetworking(float elapsedTime)
    {
      this.ProcessPackets();
      if (this.networkSession == null || this.networkSession.SessionState != 1)
        return;
      if (this.networkSession.IsHost)
      {
        NetworkGamer host = this.networkSession.Host;
        if (g.m_App.m_RequestIntermissionEnd)
        {
          ((BinaryWriter) this.packetWriter).Write((byte) 43);
          ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3);
          g.m_App.m_RequestIntermissionEnd = false;
        }
        bool flag = false;
        for (int index = 0; index < 16; ++index)
        {
          if (g.m_PlayerManager.m_Player[index].m_Id != -1 && g.m_PlayerManager.m_Player[index].m_Bot)
          {
            if (g.m_PlayerManager.m_Player[index].m_RequestSendTeam)
              this.SendBotTeamChangedMessage(g.m_PlayerManager.m_Player[index].m_NetId);
            if (g.m_PlayerManager.m_Player[index].m_RequestSendClass)
              this.SendBotClassChangedMessage(g.m_PlayerManager.m_Player[index].m_NetId);
            if (g.m_PlayerManager.m_Player[index].m_bRequestDied)
              this.SendBotDeath(g.m_PlayerManager.m_Player[index].m_NetId);
            if (g.m_PlayerManager.m_Player[index].m_bFired)
              this.SendBotFireMessage(g.m_PlayerManager.m_Player[index].m_NetId);
            if (g.m_PlayerManager.m_Player[index].m_RequestSendImpact)
              this.SendBotImpactMessage(g.m_PlayerManager.m_Player[index].m_NetId);
            if (g.m_PlayerManager.m_Player[index].m_bTorchChanged)
              this.SendBotTorchChangedMessage(g.m_PlayerManager.m_Player[index].m_NetId);
            if (g.m_PlayerManager.m_Player[index].m_bRequestSendDamage)
              this.SendBotDamageMessage(g.m_PlayerManager.m_Player[index].m_NetId);
            if (g.m_PlayerManager.m_Player[index].m_bWeaponChanged)
              this.SendBotWeaponChangedMessage(g.m_PlayerManager.m_Player[index].m_NetId);
            if (g.m_PlayerManager.m_Player[index].m_bRequestSendSpawn)
              this.SendBotSpawnMessage(g.m_PlayerManager.m_Player[index].m_NetId);
            if (g.m_PlayerManager.m_Player[index].m_RequestSendCrouch)
              this.SendBotCrouchChangedMessage(g.m_PlayerManager.m_Player[index].m_NetId);
            if (g.m_PlayerManager.m_Player[index].m_AnimChanged)
              this.SendBotAnimChangedMessage(g.m_PlayerManager.m_Player[index].m_NetId);
            if (g.m_PlayerManager.m_Player[index].m_AnimUpperChanged)
              this.SendBotAnimUpperChangedMessage(g.m_PlayerManager.m_Player[index].m_NetId);
            if (g.m_PlayerManager.m_Player[index].m_RequestCreateProjectile)
              this.SendBotCreateProjectileMessage(g.m_PlayerManager.m_Player[index].m_NetId);
            if (this.updatesSinceBotPacket >= 4)
              this.SendBotData(g.m_PlayerManager.m_Player[index].m_NetId);
            flag = true;
          }
        }
        if (flag)
        {
          if (this.updatesSinceBotPacket >= 4)
            this.updatesSinceBotPacket = 0;
          else
            ++this.updatesSinceBotPacket;
        }
        if (this.m_HostSendGiveScore)
        {
          ((BinaryWriter) this.packetWriter).Write((byte) 37);
          ((BinaryWriter) this.packetWriter).Write(this.m_HostSendGiveScorePlayerNetId);
          ((BinaryWriter) this.packetWriter).Write(this.m_HostSendGiveScoreAmount);
          ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3);
          this.m_HostSendGiveScore = false;
        }
        if (g.m_App.m_RequestIntermission)
        {
          ((BinaryWriter) this.packetWriter).Write((byte) 21);
          ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3);
          g.m_App.m_RequestIntermission = false;
        }
        if (g.m_App.m_RequestDeleteBotId != (short) byte.MaxValue)
        {
          ((BinaryWriter) this.packetWriter).Write((byte) 23);
          ((BinaryWriter) this.packetWriter).Write(g.m_App.m_RequestDeleteBotId);
          ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3);
          g.m_App.m_RequestDeleteBotId = (short) byte.MaxValue;
        }
        if (this.m_HostSendUseCrateAllow)
        {
          ((BinaryWriter) this.packetWriter).Write((byte) 27);
          ((BinaryWriter) this.packetWriter).Write(this.m_HostSendUseCrateId);
          ((BinaryWriter) this.packetWriter).Write(this.m_HostSendUsePlayerNetId);
          ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3);
          this.m_HostSendUseCrateAllow = false;
        }
        if (this.m_HostSendCrateItemToPeer)
        {
          ((BinaryWriter) this.packetWriter).Write((byte) 29);
          ((BinaryWriter) this.packetWriter).Write(this.m_HostSendCrateItemType);
          ((BinaryWriter) this.packetWriter).Write(this.m_HostSendCrateItemNetId);
          ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3);
          this.m_HostSendCrateItemToPeer = false;
        }
        if (this.m_HostSendDetachItem)
        {
          ((BinaryWriter) this.packetWriter).Write((byte) 32);
          ((BinaryWriter) this.packetWriter).Write(this.m_HostSendDetachPlayerNetId);
          this.packetWriter.Write(this.m_HostSendDetachItemPos);
          ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3);
          this.m_HostSendDetachItem = false;
        }
        if (this.m_HostSendDeleteNetworkItem)
        {
          this.SendDeleteNetworkItem(this.m_HostSendDeleteNetworkItemId);
          this.m_HostSendDeleteNetworkItem = false;
        }
        if (this.m_HostSendArtifactReturnedMessage)
        {
          ((BinaryWriter) this.packetWriter).Write((byte) 34);
          ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3);
          this.m_HostSendArtifactReturnedMessage = false;
        }
        if (this.m_HostSendEscapeCountdownMessage)
        {
          ((BinaryWriter) this.packetWriter).Write((byte) 35);
          this.packetWriter.Write(this.m_AirlockSmokeDir);
          this.packetWriter.Write(this.m_AirlockSmokePos);
          ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3);
          this.m_HostSendEscapeCountdownMessage = false;
        }
        if (this.m_HostSendCancelCountdownMessage)
        {
          ((BinaryWriter) this.packetWriter).Write((byte) 36);
          ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3);
          this.m_HostSendCancelCountdownMessage = false;
        }
        if (this.m_HostSendArtifactState)
        {
          ((BinaryWriter) this.packetWriter).Write((byte) 38);
          ((BinaryWriter) this.packetWriter).Write((byte) this.m_HostArtifactState);
          ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3);
          this.m_HostSendArtifactState = false;
        }
        if (this.m_HostSendCrateSeed)
        {
          ((BinaryWriter) this.packetWriter).Write((byte) 41);
          ((BinaryWriter) this.packetWriter).Write(this.m_HostCrateSeedValue);
          ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3);
          this.m_HostSendCrateSeed = false;
        }
      }
      foreach (NetworkGamer allGamer in this.networkSession.AllGamers)
      {
        if (((Gamer) allGamer).Tag is Player tag && tag.m_Id != -1 && allGamer.IsLocal)
        {
          if (tag.m_RequestCleanItems)
            this.SendCleanItemsMessage();
          if (tag.m_RequestSendTeam)
            this.SendTeamChangedMessage();
          if (tag.m_RequestSendClass)
            this.SendClassChangedMessage();
          if (tag.m_bRequestDied)
            this.SendLocalShipDeath();
          if (tag.m_bFired)
            this.SendFireMessage();
          if (tag.m_RequestSendImpact)
            this.SendImpactMessage();
          if (tag.m_bTorchChanged)
            this.SendTorchChangedMessage();
          if (tag.m_bRequestSendDamage)
            this.SendDamageMessage();
          if (tag.m_bWeaponChanged)
            this.SendWeaponChangedMessage();
          if (tag.m_bRequestSendSpawn)
            this.SendSpawnMessage();
          if (tag.m_RequestSendCrouch)
            this.SendCrouchChangedMessage();
          if (tag.m_RequestSendScore)
            this.SendScoreChangedMessage();
          if (tag.m_AnimChanged)
            this.SendAnimChangedMessage();
          if (tag.m_AnimUpperChanged)
            this.SendAnimUpperChangedMessage();
          if (tag.m_RequestRankUp)
            this.SendRankUpMessage();
          if (tag.m_RequestUseCrate)
            this.SendRequestUseCrateMessage();
          if (tag.m_RequestCreateProjectile)
            this.SendRequestCreateProjectileMessage();
        }
      }
      if (this.updatesSinceStatusPacket >= 4)
      {
        this.updatesSinceStatusPacket = 0;
        this.SendLocalShipData();
      }
      else
        ++this.updatesSinceStatusPacket;
    }

    private Player SendCreateBot(Player.TEAM team)
    {
      Player bot;
      if (this.networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count > 0 && this.networkSession.IsHost)
      {
        bot = g.m_PlayerManager.Create(g.m_PlayerManager.GetNextBotId(), true, team);
        ((BinaryWriter) this.packetWriter).Write((byte) 44);
        this.packetWriter.Write(Vector3.Zero);
        ((BinaryWriter) this.packetWriter).Write(0.0f);
        ((BinaryWriter) this.packetWriter).Write(bot.m_NetId);
        ((BinaryWriter) this.packetWriter).Write((byte) bot.GetTeam());
        ((BinaryWriter) this.packetWriter).Write((byte) bot.GetClass());
        ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3);
      }
      else
        bot = g.m_PlayerManager.Create(g.m_PlayerManager.GetNextBotId(), true, team);
      return bot;
    }

    public int SendCreateNetworkItem(Item.OBJ type, byte netId, Vector3 pos)
    {
      int networkItem;
      if (this.networkSession != null && ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count > 0 && this.networkSession.IsHost)
      {
        networkItem = g.m_ItemManager.Create(10, netId, pos, 0.0f, (Player) null);
        ((BinaryWriter) this.packetWriter).Write((byte) 30);
        ((BinaryWriter) this.packetWriter).Write((byte) type);
        ((BinaryWriter) this.packetWriter).Write(netId);
        this.packetWriter.Write(pos);
        ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3);
      }
      else
        networkItem = g.m_ItemManager.Create(10, netId, pos, 0.0f, (Player) null);
      return networkItem;
    }

    public void SendDeleteNetworkItem(byte netId)
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0 || !this.networkSession.IsHost)
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 33);
      ((BinaryWriter) this.packetWriter).Write(netId);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3);
    }

    public void SendAttachItem(byte netItemId, short netPlayerId, byte artifactEscapeAirlockId)
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0 || !this.networkSession.IsHost)
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 31);
      ((BinaryWriter) this.packetWriter).Write(netItemId);
      ((BinaryWriter) this.packetWriter).Write(netPlayerId);
      ((BinaryWriter) this.packetWriter).Write(artifactEscapeAirlockId);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3);
    }

    private void SendLocalShipData()
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0 || !(((Gamer) ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0]).Tag is Player tag) || tag.m_Id == -1)
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 2);
      this.packetWriter.Write(tag.m_Position);
      ((BinaryWriter) this.packetWriter).Write(tag.m_Rotation.Y);
      float num = g.m_CameraManager.m_Pitch;
      if ((double) num > 0.5)
        num = 0.5f;
      if ((double) num < -0.5)
        num = -0.5f;
      ((BinaryWriter) this.packetWriter).Write((sbyte) ((double) num * 252.0));
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 0);
    }

    private void SendBotData(short i)
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0)
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 45);
      ((BinaryWriter) this.packetWriter).Write(i);
      this.packetWriter.Write(g.m_PlayerManager.GetBot(i).m_Position);
      ((BinaryWriter) this.packetWriter).Write(g.m_PlayerManager.GetBot(i).m_Rotation.Y);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 0);
    }

    private void SendLocalShipDeath()
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0)
        return;
      LocalNetworkGamer localGamer = ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0];
      if (!(((Gamer) localGamer).Tag is Player tag) || tag.m_Id == -1)
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 7);
      short lastAttackerNetId = tag.m_LastAttackerNetId;
      short lastProjectileNetId = tag.m_LastProjectileNetId;
      ((BinaryWriter) this.packetWriter).Write(lastAttackerNetId);
      ((BinaryWriter) this.packetWriter).Write(lastProjectileNetId);
      localGamer.SendData(this.packetWriter, (SendDataOptions) 3);
      tag.m_bRequestDied = false;
    }

    private void SendBotDeath(short botId)
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0)
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 49);
      ((BinaryWriter) this.packetWriter).Write(botId);
      ((BinaryWriter) this.packetWriter).Write(g.m_PlayerManager.GetBot(botId).m_LastAttackerNetId);
      ((BinaryWriter) this.packetWriter).Write(g.m_PlayerManager.GetBot(botId).m_LastProjectileNetId);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3);
      g.m_PlayerManager.GetBot(botId).m_bRequestDied = false;
    }

    private void SendFireMessage()
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0 || !(((Gamer) ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0]).Tag is Player tag))
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 10);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 1);
      tag.m_bFired = false;
    }

    private void SendBotFireMessage(short botId)
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0)
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 46);
      ((BinaryWriter) this.packetWriter).Write(botId);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 1);
      g.m_PlayerManager.GetBot(botId).m_bFired = false;
    }

    private void SendImpactMessage()
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0 || !(((Gamer) ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0]).Tag is Player tag))
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 40);
      ((BinaryWriter) this.packetWriter).Write((short) ((double) tag.m_RequestSendImpactPos.X * 10.0));
      ((BinaryWriter) this.packetWriter).Write((short) ((double) tag.m_RequestSendImpactPos.Y * 10.0));
      ((BinaryWriter) this.packetWriter).Write((short) ((double) tag.m_RequestSendImpactPos.Z * 10.0));
      ((BinaryWriter) this.packetWriter).Write((sbyte) ((double) tag.m_RequestSendImpactNormal.X * (double) sbyte.MaxValue));
      ((BinaryWriter) this.packetWriter).Write((sbyte) ((double) tag.m_RequestSendImpactNormal.Y * (double) sbyte.MaxValue));
      ((BinaryWriter) this.packetWriter).Write((sbyte) ((double) tag.m_RequestSendImpactNormal.Z * (double) sbyte.MaxValue));
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 0);
      tag.m_RequestSendImpact = false;
    }

    private void SendBotImpactMessage(short botId)
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0)
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 59);
      ((BinaryWriter) this.packetWriter).Write(botId);
      ((BinaryWriter) this.packetWriter).Write((short) ((double) g.m_PlayerManager.GetBot(botId).m_RequestSendImpactPos.X * 10.0));
      ((BinaryWriter) this.packetWriter).Write((short) ((double) g.m_PlayerManager.GetBot(botId).m_RequestSendImpactPos.Y * 10.0));
      ((BinaryWriter) this.packetWriter).Write((short) ((double) g.m_PlayerManager.GetBot(botId).m_RequestSendImpactPos.Z * 10.0));
      ((BinaryWriter) this.packetWriter).Write((sbyte) ((double) g.m_PlayerManager.GetBot(botId).m_RequestSendImpactNormal.X * (double) sbyte.MaxValue));
      ((BinaryWriter) this.packetWriter).Write((sbyte) ((double) g.m_PlayerManager.GetBot(botId).m_RequestSendImpactNormal.Y * (double) sbyte.MaxValue));
      ((BinaryWriter) this.packetWriter).Write((sbyte) ((double) g.m_PlayerManager.GetBot(botId).m_RequestSendImpactNormal.Z * (double) sbyte.MaxValue));
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 0);
      g.m_PlayerManager.GetBot(botId).m_RequestSendImpact = false;
    }

    private void SendTorchChangedMessage()
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0 || !(((Gamer) ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0]).Tag is Player tag))
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 11);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 1);
      tag.m_bTorchChanged = false;
    }

    private void SendBotTorchChangedMessage(short botId)
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0)
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 47);
      ((BinaryWriter) this.packetWriter).Write(botId);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 1);
      g.m_PlayerManager.GetBot(botId).m_bTorchChanged = false;
    }

    private void SendDamageMessage()
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0 || !(((Gamer) ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0]).Tag is Player tag))
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 12);
      ((BinaryWriter) this.packetWriter).Write(tag.m_RequestedDamageAmount);
      ((BinaryWriter) this.packetWriter).Write(tag.m_RequestedPlayerToDamageNetID);
      ((BinaryWriter) this.packetWriter).Write(tag.m_RequestedHitZone);
      ((BinaryWriter) this.packetWriter).Write(tag.m_RequestedAttacker);
      ((BinaryWriter) this.packetWriter).Write(tag.m_RequestedProjectileNetId);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3);
      tag.m_bRequestSendDamage = false;
    }

    private void SendBotDamageMessage(short botId)
    {
      ((BinaryWriter) this.packetWriter).Write((byte) 48);
      ((BinaryWriter) this.packetWriter).Write(botId);
      ((BinaryWriter) this.packetWriter).Write(g.m_PlayerManager.GetBot(botId).m_RequestedDamageAmount);
      ((BinaryWriter) this.packetWriter).Write(g.m_PlayerManager.GetBot(botId).m_RequestedPlayerToDamageNetID);
      ((BinaryWriter) this.packetWriter).Write(g.m_PlayerManager.GetBot(botId).m_RequestedHitZone);
      ((BinaryWriter) this.packetWriter).Write(g.m_PlayerManager.GetBot(botId).m_RequestedAttacker);
      ((BinaryWriter) this.packetWriter).Write(g.m_PlayerManager.GetBot(botId).m_RequestedProjectileNetId);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3);
      g.m_PlayerManager.GetBot(botId).m_bRequestSendDamage = false;
    }

    private void SendWeaponChangedMessage()
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0 || !(((Gamer) ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0]).Tag is Player tag))
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 13);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3);
      tag.m_bWeaponChanged = false;
    }

    private void SendBotWeaponChangedMessage(short botId)
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0)
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 50);
      ((BinaryWriter) this.packetWriter).Write(botId);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3);
      g.m_PlayerManager.GetBot(botId).m_bWeaponChanged = false;
    }

    private void SendSpawnMessage()
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0 || !(((Gamer) ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0]).Tag is Player tag))
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 14);
      this.packetWriter.Write(tag.m_Position);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3);
      tag.m_bRequestSendSpawn = false;
    }

    private void SendBotSpawnMessage(short botId)
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0)
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 51);
      ((BinaryWriter) this.packetWriter).Write(botId);
      this.packetWriter.Write(g.m_PlayerManager.GetBot(botId).m_Position);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3);
      g.m_PlayerManager.GetBot(botId).m_bRequestSendSpawn = false;
    }

    private void SendBotCrouchChangedMessage(short botId)
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0)
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 60);
      ((BinaryWriter) this.packetWriter).Write(botId);
      ((BinaryWriter) this.packetWriter).Write(g.m_PlayerManager.GetBot(botId).m_Crouch);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 1);
      g.m_PlayerManager.GetBot(botId).m_RequestSendCrouch = false;
    }

    private void SendCrouchChangedMessage()
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0 || !(((Gamer) ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0]).Tag is Player tag))
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 15);
      ((BinaryWriter) this.packetWriter).Write(tag.m_Crouch);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 1);
      tag.m_RequestSendCrouch = false;
    }

    private void SendTeamChangedMessage()
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0 || !(((Gamer) ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0]).Tag is Player tag))
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 18);
      ((BinaryWriter) this.packetWriter).Write((byte) tag.m_Team);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3);
      tag.m_RequestSendTeam = false;
    }

    private void SendBotTeamChangedMessage(short botId)
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0)
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 54);
      ((BinaryWriter) this.packetWriter).Write(botId);
      ((BinaryWriter) this.packetWriter).Write((byte) g.m_PlayerManager.GetBot(botId).m_Team);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3);
      g.m_PlayerManager.GetBot(botId).m_RequestSendTeam = false;
    }

    private void SendClassChangedMessage()
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0 || !(((Gamer) ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0]).Tag is Player tag))
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 19);
      ((BinaryWriter) this.packetWriter).Write((byte) tag.m_Class);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3);
      tag.m_RequestSendClass = false;
    }

    private void SendBotClassChangedMessage(short botId)
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0)
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 55);
      ((BinaryWriter) this.packetWriter).Write(botId);
      ((BinaryWriter) this.packetWriter).Write((byte) g.m_PlayerManager.GetBot(botId).m_Class);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3);
      g.m_PlayerManager.GetBot(botId).m_RequestSendClass = false;
    }

    private void SendScoreChangedMessage()
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0 || !(((Gamer) ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0]).Tag is Player tag))
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 16);
      ((BinaryWriter) this.packetWriter).Write(tag.m_Score);
      ((BinaryWriter) this.packetWriter).Write(tag.m_Kills);
      ((BinaryWriter) this.packetWriter).Write(tag.m_Deaths);
      ((BinaryWriter) this.packetWriter).Write(tag.m_Rank);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 1);
      tag.m_RequestSendScore = false;
    }

    private void SendAnimChangedMessage()
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0 || !(((Gamer) ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0]).Tag is Player tag))
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 20);
      ((BinaryWriter) this.packetWriter).Write(tag.m_Anim);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 1);
      tag.m_AnimChanged = false;
    }

    private void SendAnimUpperChangedMessage()
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0 || !(((Gamer) ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0]).Tag is Player tag))
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 28);
      ((BinaryWriter) this.packetWriter).Write(tag.m_AnimUpper);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 1);
      tag.m_AnimUpperChanged = false;
    }

    private void SendRankUpMessage()
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0 || !(((Gamer) ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0]).Tag is Player tag))
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 24);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 0);
      tag.m_RequestRankUp = false;
    }

    private void SendRequestUseCrateMessage()
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0 || !(((Gamer) ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0]).Tag is Player tag) || this.networkSession.IsHost)
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 26);
      ((BinaryWriter) this.packetWriter).Write(tag.m_RequestUseCrateId);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3, this.networkSession.Host);
      tag.m_RequestUseCrate = false;
    }

    private void SendRequestCreateProjectileMessage()
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0 || !(((Gamer) ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0]).Tag is Player tag))
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 39);
      ((BinaryWriter) this.packetWriter).Write(tag.m_RequestCreateProjectileNetId);
      ((BinaryWriter) this.packetWriter).Write(tag.m_RequestCreateProjectileType);
      this.packetWriter.Write(tag.m_RequestCreateProjectilePosition);
      this.packetWriter.Write(tag.m_RequestCreateProjectileQuaterion);
      this.packetWriter.Write(tag.m_RequestCreateProjectileVeclocity);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3);
      tag.m_RequestCreateProjectile = false;
    }

    private void SendCleanItemsMessage()
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0 || !(((Gamer) ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0]).Tag is Player tag))
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 25);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 3);
      tag.m_RequestCleanItems = false;
    }

    private void SendBotAnimChangedMessage(short botId)
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0)
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 56);
      ((BinaryWriter) this.packetWriter).Write(botId);
      ((BinaryWriter) this.packetWriter).Write(g.m_PlayerManager.GetBot(botId).m_Anim);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 1);
      g.m_PlayerManager.GetBot(botId).m_AnimChanged = false;
    }

    private void SendBotAnimUpperChangedMessage(short botId)
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0)
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 57);
      ((BinaryWriter) this.packetWriter).Write(botId);
      ((BinaryWriter) this.packetWriter).Write(g.m_PlayerManager.GetBot(botId).m_AnimUpper);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 1);
      g.m_PlayerManager.GetBot(botId).m_AnimUpperChanged = false;
    }

    private void SendBotCreateProjectileMessage(short botId)
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0)
        return;
      ((BinaryWriter) this.packetWriter).Write((byte) 58);
      ((BinaryWriter) this.packetWriter).Write(botId);
      ((BinaryWriter) this.packetWriter).Write(g.m_PlayerManager.GetBot(botId).m_RequestCreateProjectileNetId);
      ((BinaryWriter) this.packetWriter).Write(g.m_PlayerManager.GetBot(botId).m_RequestCreateProjectileType);
      this.packetWriter.Write(g.m_PlayerManager.GetBot(botId).m_RequestCreateProjectilePosition);
      this.packetWriter.Write(g.m_PlayerManager.GetBot(botId).m_RequestCreateProjectileQuaterion);
      this.packetWriter.Write(g.m_PlayerManager.GetBot(botId).m_RequestCreateProjectileVeclocity);
      ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].SendData(this.packetWriter, (SendDataOptions) 1);
      g.m_PlayerManager.GetBot(botId).m_RequestCreateProjectile = false;
    }

    private void ProcessPackets()
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0)
        return;
      while (((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].IsDataAvailable)
      {
        NetworkGamer sender;
        ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers)[0].ReceiveData(this.packetReader, ref sender);
        switch (((BinaryReader) this.packetReader).ReadByte())
        {
          case 1:
            this.UpdatePlayerData(sender);
            continue;
          case 2:
            if (sender != null && !sender.IsLocal)
            {
              this.UpdateShipData(sender);
              continue;
            }
            continue;
          case 7:
            this.KillShip(sender);
            continue;
          case 10:
            if (sender != null && !sender.IsLocal && ((Gamer) sender).Tag is Player tag1 && tag1.m_Id != -1)
            {
              tag1.SimulateFireWeapon();
              continue;
            }
            continue;
          case 11:
            if (sender != null && !sender.IsLocal && ((Gamer) sender).Tag is Player tag2 && tag2.m_Id != -1)
            {
              tag2.ToggleTorchLight();
              continue;
            }
            continue;
          case 12:
            if (sender != null && !sender.IsLocal && ((Gamer) sender).Tag is Player tag3 && tag3.m_Id != -1)
            {
              sbyte damage = ((BinaryReader) this.packetReader).ReadSByte();
              short netId = ((BinaryReader) this.packetReader).ReadInt16();
              byte hitZone = ((BinaryReader) this.packetReader).ReadByte();
              short attackerNetId = ((BinaryReader) this.packetReader).ReadInt16();
              short projectileNetId = ((BinaryReader) this.packetReader).ReadInt16();
              int playerExistsWithNetId = g.m_PlayerManager.GetPlayerExistsWithNetId(netId);
              if (playerExistsWithNetId != -1 && g.m_PlayerManager.m_Player[playerExistsWithNetId].m_Id != -1)
              {
                g.m_PlayerManager.m_Player[playerExistsWithNetId].DoDamage(damage, hitZone, attackerNetId, projectileNetId);
                continue;
              }
              continue;
            }
            continue;
          case 13:
            if (sender != null && !sender.IsLocal && ((Gamer) sender).Tag is Player tag4 && tag4.m_Id != -1)
            {
              tag4.PeerChangeWeapon();
              continue;
            }
            continue;
          case 14:
            if (sender != null && !sender.IsLocal && ((Gamer) sender).Tag is Player tag5 && tag5.m_Id != -1)
            {
              Vector3 pos = this.packetReader.ReadVector3();
              tag5.PeerSpawned(pos);
              continue;
            }
            continue;
          case 15:
            if (sender != null && !sender.IsLocal && ((Gamer) sender).Tag is Player tag6 && tag6.m_Id != -1)
            {
              bool crouch = ((BinaryReader) this.packetReader).ReadBoolean();
              tag6.PeerCrouch(crouch);
              continue;
            }
            continue;
          case 16:
            if (sender != null && !sender.IsLocal && ((Gamer) sender).Tag is Player tag7 && tag7.m_Id != -1)
            {
              tag7.m_Score = ((BinaryReader) this.packetReader).ReadInt16();
              tag7.m_Kills = ((BinaryReader) this.packetReader).ReadInt16();
              tag7.m_Deaths = ((BinaryReader) this.packetReader).ReadInt16();
              tag7.m_Rank = ((BinaryReader) this.packetReader).ReadInt16();
              continue;
            }
            continue;
          case 17:
            if (sender != null && !sender.IsLocal && ((Gamer) sender).Tag is Player tag8 && tag8.m_Id != -1)
            {
              sbyte weaponType = ((BinaryReader) this.packetReader).ReadSByte();
              tag8.SetWeaponByType((int) weaponType);
              continue;
            }
            continue;
          case 18:
            if (sender != null && !sender.IsLocal && ((Gamer) sender).Tag is Player tag9 && tag9.m_Id != -1)
            {
              byte t = ((BinaryReader) this.packetReader).ReadByte();
              tag9.PeerSetTeam((Player.TEAM) t);
              continue;
            }
            continue;
          case 19:
            if (sender != null && !sender.IsLocal && ((Gamer) sender).Tag is Player tag10 && tag10.m_Id != -1)
            {
              byte c = ((BinaryReader) this.packetReader).ReadByte();
              tag10.PeerSetClass((Player.CLASS) c);
              continue;
            }
            continue;
          case 20:
            if (sender != null && !sender.IsLocal && ((Gamer) sender).Tag is Player tag11 && tag11.m_Id != -1)
            {
              byte anim = ((BinaryReader) this.packetReader).ReadByte();
              tag11.PeerSetAnim(anim);
              continue;
            }
            continue;
          case 21:
            if (sender != null && !sender.IsLocal)
            {
              this.PeerGoToIntermission();
              continue;
            }
            continue;
          case 22:
            if (sender != null && !sender.IsLocal)
            {
              g.m_App.m_Level = (int) ((BinaryReader) this.packetReader).ReadByte();
              this.LoadSunburnScene();
              continue;
            }
            continue;
          case 23:
            if (sender != null && !sender.IsLocal)
            {
              short botId = ((BinaryReader) this.packetReader).ReadInt16();
              if (g.m_PlayerManager.BotExists(botId))
              {
                g.m_PlayerManager.GetBot(botId).RequestDelete();
                continue;
              }
              continue;
            }
            continue;
          case 24:
            if (sender != null && !sender.IsLocal && ((Gamer) sender).Tag is Player tag12 && tag12.m_Id != -1)
            {
              tag12.PeerRankUp();
              continue;
            }
            continue;
          case 25:
            if (sender != null && !sender.IsLocal && ((Gamer) sender).Tag is Player tag13 && tag13.m_Id != -1)
            {
              tag13.CleanItems();
              continue;
            }
            continue;
          case 27:
            if (sender != null && !sender.IsLocal && ((Gamer) sender).Tag is Player tag14 && tag14.m_Id != -1)
            {
              short crateIdx = ((BinaryReader) this.packetReader).ReadInt16();
              short playerNetId = ((BinaryReader) this.packetReader).ReadInt16();
              int indexByCrateIndex = g.m_ItemManager.GetItemIndexByCrateIndex(crateIdx);
              g.m_ItemManager.m_Item[indexByCrateIndex].PeerUseCrate((int) crateIdx, playerNetId);
              continue;
            }
            continue;
          case 28:
            if (sender != null && !sender.IsLocal && ((Gamer) sender).Tag is Player tag15 && tag15.m_Id != -1)
            {
              byte anim = ((BinaryReader) this.packetReader).ReadByte();
              tag15.PeerSetAnimUpper((int) anim);
              continue;
            }
            continue;
          case 30:
            if (sender != null && !sender.IsLocal)
            {
              byte type = ((BinaryReader) this.packetReader).ReadByte();
              byte netId = ((BinaryReader) this.packetReader).ReadByte();
              Vector3 pos = this.packetReader.ReadVector3();
              g.m_ItemManager.Create((int) type, netId, pos, 0.0f, (Player) null);
              continue;
            }
            continue;
          case 31:
            if (sender != null && !sender.IsLocal)
            {
              byte netItemId = ((BinaryReader) this.packetReader).ReadByte();
              short netId = ((BinaryReader) this.packetReader).ReadInt16();
              byte escapeAirlockId = ((BinaryReader) this.packetReader).ReadByte();
              int playerExistsWithNetId = g.m_PlayerManager.GetPlayerExistsWithNetId(netId);
              if (playerExistsWithNetId != -1)
              {
                g.m_PlayerManager.m_Player[playerExistsWithNetId].PeerAttach(netItemId, escapeAirlockId);
                continue;
              }
              continue;
            }
            continue;
          case 32:
            if (sender != null && !sender.IsLocal)
            {
              short netId = ((BinaryReader) this.packetReader).ReadInt16();
              int playerExistsWithNetId = g.m_PlayerManager.GetPlayerExistsWithNetId(netId);
              if (playerExistsWithNetId != -1)
              {
                g.m_PlayerManager.m_Player[playerExistsWithNetId].PeerDetach();
                continue;
              }
              continue;
            }
            continue;
          case 33:
            if (sender != null && !sender.IsLocal)
            {
              byte netItemId = ((BinaryReader) this.packetReader).ReadByte();
              int itemIdByNetItemId = g.m_ItemManager.GetItemIdByNetItemId(netItemId);
              if (itemIdByNetItemId != -1)
              {
                g.m_ItemManager.Delete(itemIdByNetItemId);
                continue;
              }
              continue;
            }
            continue;
          case 34:
            if (sender != null && !sender.IsLocal)
            {
              g.m_ItemManager.ShowArtifactReturnedMessage();
              continue;
            }
            continue;
          case 37:
            if (sender != null && !sender.IsLocal)
            {
              short netId = ((BinaryReader) this.packetReader).ReadInt16();
              byte score = ((BinaryReader) this.packetReader).ReadByte();
              int playerExistsWithNetId = g.m_PlayerManager.GetPlayerExistsWithNetId(netId);
              if (playerExistsWithNetId != -1 && g.m_PlayerManager.m_Player[playerExistsWithNetId].m_Id != -1)
              {
                g.m_PlayerManager.m_Player[playerExistsWithNetId].PeerGiveScore(score);
                continue;
              }
              continue;
            }
            continue;
          case 38:
            if (sender != null && !sender.IsLocal)
            {
              this.PeerSetHostArtifactState((GameplayScreen.ARTIFACTSTATE) ((BinaryReader) this.packetReader).ReadByte());
              continue;
            }
            continue;
          case 39:
            if (sender != null && !sender.IsLocal)
            {
              Player tag16 = ((Gamer) sender).Tag as Player;
              short projectileNetId = ((BinaryReader) this.packetReader).ReadInt16();
              byte projectileType = ((BinaryReader) this.packetReader).ReadByte();
              Vector3 pos = this.packetReader.ReadVector3();
              Quaternion q = this.packetReader.ReadQuaternion();
              Vector3 vel = this.packetReader.ReadVector3();
              if (tag16 != null && tag16.m_Id != -1)
              {
                tag16.PeerCreateProjectile(projectileNetId, projectileType, pos, q, vel);
                continue;
              }
              continue;
            }
            continue;
          case 40:
            if (sender != null && !sender.IsLocal)
            {
              Player tag17 = ((Gamer) sender).Tag as Player;
              Vector3 zero = Vector3.Zero;
              zero.X = (float) ((BinaryReader) this.packetReader).ReadInt16() * 0.1f;
              zero.Y = (float) ((BinaryReader) this.packetReader).ReadInt16() * 0.1f;
              zero.Z = (float) ((BinaryReader) this.packetReader).ReadInt16() * 0.1f;
              sbyte num1 = ((BinaryReader) this.packetReader).ReadSByte();
              sbyte num2 = ((BinaryReader) this.packetReader).ReadSByte();
              sbyte num3 = ((BinaryReader) this.packetReader).ReadSByte();
              Vector3 impactNormal;
              // ISSUE: explicit constructor call
              ((Vector3) ref impactNormal).\u002Ector((float) num1 / (float) sbyte.MaxValue, (float) num2 / (float) sbyte.MaxValue, (float) num3 / (float) sbyte.MaxValue);
              if (tag17 != null && tag17.m_Id != -1)
              {
                tag17.SimulateWeaponImpact(zero, impactNormal);
                continue;
              }
              continue;
            }
            continue;
          case 41:
            if (sender != null && !sender.IsLocal)
            {
              int seed = ((BinaryReader) this.packetReader).ReadInt32();
              g.m_ItemManager.SetUpCrates(seed);
              this.m_HostCrateSeedValue = seed;
              continue;
            }
            continue;
          case 42:
            if (sender != null && !sender.IsLocal)
            {
              byte[] buffer = ((BinaryReader) this.packetReader).ReadBytes(22);
              g.m_ItemManager.SetCrateStatesFromBuffer(buffer);
              continue;
            }
            continue;
          case 43:
            if (sender != null && !sender.IsLocal)
            {
              this.PeerEndIntermission();
              continue;
            }
            continue;
          case 44:
            if (sender != null && !sender.IsLocal)
            {
              g.m_BotPathManager.LoadBotPath();
              Vector3 vector3 = this.packetReader.ReadVector3();
              float num4 = ((BinaryReader) this.packetReader).ReadSingle();
              short netId = ((BinaryReader) this.packetReader).ReadInt16();
              byte num5 = ((BinaryReader) this.packetReader).ReadByte();
              byte c = ((BinaryReader) this.packetReader).ReadByte();
              Player player = g.m_PlayerManager.Create(netId, true, (Player.TEAM) num5);
              player.PeerSetTeam((Player.TEAM) num5);
              player.PeerSetClass((Player.CLASS) c);
              player.m_Position = vector3;
              player.m_Rotation.Y = num4;
              continue;
            }
            continue;
          case 45:
            if (sender != null && !sender.IsLocal)
            {
              this.UpdateBotData(sender);
              continue;
            }
            continue;
          case 46:
            if (sender != null && !sender.IsLocal)
            {
              short botId = ((BinaryReader) this.packetReader).ReadInt16();
              if (g.m_PlayerManager.BotExists(botId))
              {
                g.m_PlayerManager.GetBot(botId).SimulateFireWeapon();
                continue;
              }
              continue;
            }
            continue;
          case 47:
            if (sender != null && !sender.IsLocal)
            {
              short botId = ((BinaryReader) this.packetReader).ReadInt16();
              if (g.m_PlayerManager.BotExists(botId))
              {
                g.m_PlayerManager.GetBot(botId).ToggleTorchLight();
                continue;
              }
              continue;
            }
            continue;
          case 48:
            if (sender != null && !sender.IsLocal && ((Gamer) sender).Tag is Player tag18 && tag18.m_Id != -1)
            {
              int num = (int) ((BinaryReader) this.packetReader).ReadInt16();
              sbyte damage = ((BinaryReader) this.packetReader).ReadSByte();
              short netId = ((BinaryReader) this.packetReader).ReadInt16();
              byte hitZone = ((BinaryReader) this.packetReader).ReadByte();
              short attackerNetId = ((BinaryReader) this.packetReader).ReadInt16();
              short projectileNetId = ((BinaryReader) this.packetReader).ReadInt16();
              int playerExistsWithNetId = g.m_PlayerManager.GetPlayerExistsWithNetId(netId);
              if (playerExistsWithNetId != -1 && g.m_PlayerManager.m_Player[playerExistsWithNetId].m_Id != -1)
              {
                g.m_PlayerManager.m_Player[playerExistsWithNetId].DoDamage(damage, hitZone, attackerNetId, projectileNetId);
                continue;
              }
              continue;
            }
            continue;
          case 49:
            this.KillBot(sender);
            continue;
          case 50:
            if (sender != null && !sender.IsLocal)
            {
              short botId = ((BinaryReader) this.packetReader).ReadInt16();
              if (g.m_PlayerManager.BotExists(botId))
              {
                g.m_PlayerManager.GetBot(botId).PeerChangeWeapon();
                continue;
              }
              continue;
            }
            continue;
          case 51:
            if (sender != null && !sender.IsLocal)
            {
              short botId = ((BinaryReader) this.packetReader).ReadInt16();
              Vector3 pos = this.packetReader.ReadVector3();
              if (g.m_PlayerManager.BotExists(botId))
              {
                g.m_PlayerManager.GetBot(botId).PeerSpawned(pos);
                continue;
              }
              continue;
            }
            continue;
          case 52:
            if (sender != null && !sender.IsLocal)
            {
              short botId = ((BinaryReader) this.packetReader).ReadInt16();
              short num6 = ((BinaryReader) this.packetReader).ReadInt16();
              short num7 = ((BinaryReader) this.packetReader).ReadInt16();
              short num8 = ((BinaryReader) this.packetReader).ReadInt16();
              short num9 = ((BinaryReader) this.packetReader).ReadInt16();
              if (g.m_PlayerManager.BotExists(botId))
              {
                g.m_PlayerManager.GetBot(botId).m_Score = num6;
                g.m_PlayerManager.GetBot(botId).m_Kills = num7;
                g.m_PlayerManager.GetBot(botId).m_Deaths = num8;
                g.m_PlayerManager.GetBot(botId).m_Rank = num9;
                continue;
              }
              continue;
            }
            continue;
          case 53:
            if (sender != null && !sender.IsLocal)
            {
              short botId = ((BinaryReader) this.packetReader).ReadInt16();
              int weaponType = ((BinaryReader) this.packetReader).ReadInt32();
              if (g.m_PlayerManager.BotExists(botId))
              {
                g.m_PlayerManager.GetBot(botId).SetWeaponByType(weaponType);
                continue;
              }
              continue;
            }
            continue;
          case 54:
            if (sender != null && !sender.IsLocal)
            {
              short botId = ((BinaryReader) this.packetReader).ReadInt16();
              byte t = ((BinaryReader) this.packetReader).ReadByte();
              if (g.m_PlayerManager.BotExists(botId))
              {
                g.m_PlayerManager.GetBot(botId).PeerSetTeam((Player.TEAM) t);
                continue;
              }
              continue;
            }
            continue;
          case 55:
            if (sender != null && !sender.IsLocal)
            {
              short botId = ((BinaryReader) this.packetReader).ReadInt16();
              byte c = ((BinaryReader) this.packetReader).ReadByte();
              if (g.m_PlayerManager.BotExists(botId))
              {
                g.m_PlayerManager.GetBot(botId).PeerSetClass((Player.CLASS) c);
                continue;
              }
              continue;
            }
            continue;
          case 56:
            if (sender != null && !sender.IsLocal)
            {
              short botId = ((BinaryReader) this.packetReader).ReadInt16();
              byte anim = ((BinaryReader) this.packetReader).ReadByte();
              if (g.m_PlayerManager.BotExists(botId))
              {
                g.m_PlayerManager.GetBot(botId).PeerSetAnim(anim);
                continue;
              }
              continue;
            }
            continue;
          case 57:
            if (sender != null && !sender.IsLocal)
            {
              short botId = ((BinaryReader) this.packetReader).ReadInt16();
              byte anim = ((BinaryReader) this.packetReader).ReadByte();
              if (g.m_PlayerManager.BotExists(botId))
              {
                g.m_PlayerManager.GetBot(botId).PeerSetAnimUpper((int) anim);
                continue;
              }
              continue;
            }
            continue;
          case 58:
            if (sender != null && !sender.IsLocal)
            {
              short botId = ((BinaryReader) this.packetReader).ReadInt16();
              short projectileNetId = ((BinaryReader) this.packetReader).ReadInt16();
              byte projectileType = ((BinaryReader) this.packetReader).ReadByte();
              Vector3 pos = this.packetReader.ReadVector3();
              Quaternion q = this.packetReader.ReadQuaternion();
              Vector3 vel = this.packetReader.ReadVector3();
              if (g.m_PlayerManager.BotExists(botId))
              {
                g.m_PlayerManager.GetBot(botId).PeerCreateProjectile(projectileNetId, projectileType, pos, q, vel);
                continue;
              }
              continue;
            }
            continue;
          case 59:
            if (sender != null && !sender.IsLocal)
            {
              short botId = ((BinaryReader) this.packetReader).ReadInt16();
              Vector3 zero = Vector3.Zero;
              zero.X = (float) ((BinaryReader) this.packetReader).ReadInt16() * 0.1f;
              zero.Y = (float) ((BinaryReader) this.packetReader).ReadInt16() * 0.1f;
              zero.Z = (float) ((BinaryReader) this.packetReader).ReadInt16() * 0.1f;
              sbyte num10 = ((BinaryReader) this.packetReader).ReadSByte();
              sbyte num11 = ((BinaryReader) this.packetReader).ReadSByte();
              sbyte num12 = ((BinaryReader) this.packetReader).ReadSByte();
              Vector3 impactNormal;
              // ISSUE: explicit constructor call
              ((Vector3) ref impactNormal).\u002Ector((float) num10 / (float) sbyte.MaxValue, (float) num11 / (float) sbyte.MaxValue, (float) num12 / (float) sbyte.MaxValue);
              if (g.m_PlayerManager.BotExists(botId))
              {
                g.m_PlayerManager.GetBot(botId).SimulateWeaponImpact(zero, impactNormal);
                continue;
              }
              continue;
            }
            continue;
          case 60:
            if (sender != null && !sender.IsLocal)
            {
              short botId = ((BinaryReader) this.packetReader).ReadInt16();
              bool crouch = ((BinaryReader) this.packetReader).ReadBoolean();
              if (g.m_PlayerManager.BotExists(botId))
              {
                g.m_PlayerManager.GetBot(botId).PeerCrouch(crouch);
                continue;
              }
              continue;
            }
            continue;
          default:
            continue;
        }
      }
    }

    private void KillShip(NetworkGamer sender)
    {
      if (sender == null || sender.IsLocal || !(((Gamer) sender).Tag is Player tag) || tag.m_Id == -1)
        return;
      short lastAttackerNetId = ((BinaryReader) this.packetReader).ReadInt16();
      short projectileNetId = ((BinaryReader) this.packetReader).ReadInt16();
      tag.Kill(lastAttackerNetId, projectileNetId);
    }

    private void KillBot(NetworkGamer sender)
    {
      if (sender == null || sender.IsLocal || !(((Gamer) sender).Tag is Player tag) || tag.m_Id == -1)
        return;
      short botId = ((BinaryReader) this.packetReader).ReadInt16();
      short lastAttackerNetId = ((BinaryReader) this.packetReader).ReadInt16();
      short projectileNetId = ((BinaryReader) this.packetReader).ReadInt16();
      if (!g.m_PlayerManager.BotExists(botId))
        return;
      g.m_PlayerManager.GetBot(botId).Kill(lastAttackerNetId, projectileNetId);
    }

    private void UpdatePlayerData(NetworkGamer sender)
    {
      if (this.networkSession == null || ((ReadOnlyCollection<LocalNetworkGamer>) this.networkSession.LocalGamers).Count <= 0 || sender == null || !(((Gamer) sender).Tag is Player tag) || tag.m_Id == -1)
        return;
      tag.m_Position = this.packetReader.ReadVector3();
      tag.m_Rotation.Y = ((BinaryReader) this.packetReader).ReadSingle();
      tag.m_NetworkPosition = tag.m_Position;
      tag.m_NetworkRotation = tag.m_Rotation.Y;
    }

    private void UpdateShipData(NetworkGamer sender)
    {
      if (sender == null || !(((Gamer) sender).Tag is Player tag) || tag.m_Id == -1)
        return;
      tag.m_NetworkPosition = this.packetReader.ReadVector3();
      tag.m_NetworkRotation = ((BinaryReader) this.packetReader).ReadSingle();
      sbyte num = ((BinaryReader) this.packetReader).ReadSByte();
      tag.m_PeerPitch = MathHelper.Lerp(tag.m_PeerPitch, (float) num / 252f, 0.5f);
    }

    private void UpdateBotData(NetworkGamer sender)
    {
      if (sender == null)
        return;
      short botId = ((BinaryReader) this.packetReader).ReadInt16();
      Vector3 vector3 = this.packetReader.ReadVector3();
      float num = ((BinaryReader) this.packetReader).ReadSingle();
      if (!g.m_PlayerManager.BotExists(botId))
        return;
      g.m_PlayerManager.GetBot(botId).m_NetworkPosition = vector3;
      g.m_PlayerManager.GetBot(botId).m_NetworkRotation = num;
    }

    public enum ARTIFACTSTATE
    {
      NONE,
      INCRATE,
      ATTACHED,
      DROPPED,
    }
  }
}
