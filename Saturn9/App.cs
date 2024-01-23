using BEPUphysics;
using BEPUphysics.CollisionTests.CollisionAlgorithms;
using BEPUphysics.Constraints;
using DPSF;
using EasyStorage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using SynapseGaming.LightingSystem.Core;
using SynapseGaming.LightingSystem.Effects.Deferred;
using SynapseGaming.LightingSystem.Rendering;
using SynapseGaming.LightingSystem.Rendering.Deferred;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;

namespace Saturn9
{
  public class App : Game
  {
    public const int WIDTH = 1024;
    public const int HEIGHT = 576;
    private const float moveScale = 100f;
    public const int MAX_ARTIFACTS_SP = 5;
    public SunBurnCoreSystem sunBurnCoreSystem;
    public FrameBuffers frameBuffers;
    public SplashScreenGameComponent splashScreenGameComponent;
    public SceneState sceneState;
    public SceneInterface sceneInterface;
    public ContentRepository contentRepository;
    public SceneEnvironment environment;
    public Scene scene;
    public GraphicsDeviceManager graphics;
    public ScreenManager screenManager;
    private static readonly string[] preloadAssets = new string[2]
    {
      "sprites\\back",
      "sprites\\busyicon"
    };
    public Space m_Space;
    public GameTime m_GameTime;
    public SpriteFont font;
    public SpriteFont hudFont;
    public SpriteFont lcdFont;
    public SpriteFont graveFont;
    public NetworkSession m_NetworkSession;
    public Random m_Rand;
    public ParticleSystemManager m_ParticleSystemManager;
    public BEPUDebugDrawer m_BEPUDebugDrawer;
    public bool m_Paused;
    public bool m_SingleStep;
    public bool DEBUG_DRAW_CYLINDER;
    public bool DEBUG_DRAW_HITZONES;
    public bool DEBUG_DRAW_RAGDOLL;
    public bool DEBUG_DRAW_VIEW_MODEL_BB;
    public bool DEBUG_DRAW_FULL_MODEL_BB;
    public bool DEBUG_DRAW_COLLISION;
    public bool DEBUG_DRAW_PROJECTILES;
    public bool DEBUG_DRAW_ITEMCOL;
    public IAsyncSaveDevice saveDevice;
    public bool m_bSaveExists;
    public int m_Level = 1;
    private Texture2D gradientTexture;
    public Texture2D compassHTexture;
    private Texture2D compassVTexture;
    private Texture2D m_HealthBar;
    public Texture2D markerTexture;
    public Texture2D m_LoadingIcon;
    public Texture2D m_SOSTexture;
    public Texture2D m_SOS1Texture;
    public Texture2D m_SOS2Texture;
    private HelmetMessage[] m_HelmetMessage = new HelmetMessage[48];
    private string[] m_SysMessage = new string[5];
    private float[] m_SysMessageTime = new float[5];
    private Color[] m_SysMessageColour = new Color[5];
    public PlayerIndex m_PlayerOnePadId;
    private int m_DoneSunburnStartupRender = 10;
    public Song m_Level1Music;
    public Song m_HallucinateMusic;
    public Song m_MenuMusic;
    public Song m_IntermissionMusic;
    public Song m_CompleteMusic;
    public bool m_ShowScoreboard;
    public float m_OptionsHoriz = 2.5f;
    public float m_OptionsVert = 2.5f;
    public bool m_OptionsInvertY;
    public bool m_OptionsVibration = true;
    public int m_OptionsBotsSP = 5;
    public int m_OptionsBotsMP = 1;
    public int m_OptionsMapTime = 15;
    public bool m_OptionsBlood = true;
    public float m_OptionsVol = 1f;
    public float m_OptionsMusicVol = 1f;
    public int m_OptionsMaxPlayers = 6;
    public bool m_OptionsLights = true;
    public int m_OptionsBrightness;
    public bool m_RequestIntermission;
    public bool m_RequestIntermissionEnd;
    public bool m_Intermission;
    public float m_IntermissionTime;
    public Scene m_Scene1;
    public Scene m_Scene2;
    public SceneEnvironment m_Environment1;
    public SceneEnvironment m_Environment2;
    public Model m_CollisionModel1;
    public Model m_CollisionModel2;
    public Model m_Skybox;
    public short m_RequestDeleteBotId = (short) byte.MaxValue;
    public int m_RumbleFrames;
    public float m_ShowPermissionTime;
    public float m_ShowNotSignedInToLiveTime;
    public float m_ShowNoOnlineSessionPrivilidgeTime;
    public RenderTarget2D m_HudRenderTarget;
    public RenderTarget2D m_LCD1RenderTarget;
    public RenderTarget2D m_GraveRenderTarget;
    public RenderTarget2D m_TabletRenderTarget;
    public SoundEffectInstance m_AlarmSFX;
    public SoundEffectInstance m_CountdownSFX;
    public SoundEffectInstance m_CargoLoopSFX;
    public int m_ArtifactsFoundSP;
    public bool m_bShowCompletetionScreen;
    public bool m_bTutorialDone;
    public int m_Rank;
    public int m_XP;
    public bool m_SurvivalMode;
    public int m_SurvivalScoreBest;
    public bool m_ForceJoin;
    public SceneObject m_LCD1;
    public SceneObject m_LCD2;
    public SceneObject m_GRAVE;
    public GameplayScreen m_GameplayScreen;
    public float m_HudDistort1Time;
    public float m_HudDistort1Alpha = 1f;
    private Texture2D m_HudNoise1;
    public bool SOUNDON = true;
    public int m_CheckpointId = -1;
    public Color HUD_GREEN = new Color(45, 207, (int) byte.MaxValue);
    public Color HUD_RED = new Color((int) byte.MaxValue, 0, 0, (int) byte.MaxValue);
    private VisualizationData visData;
    private Vector3 viewPosition = new Vector3(86.5f, 11.2f, 57f);
    private Vector3 viewRotation = new Vector3(-2.2f, 0.16f, 0.0f);
    private Matrix view = Matrix.Identity;
    private Matrix projection = Matrix.Identity;

    public App(string[] args)
    {
      this.InitBEPU();
      this.graphics = new GraphicsDeviceManager((Game) this);
      this.Content.RootDirectory = "Content";
      this.graphics.PreferredDepthStencilFormat = (DepthFormat) 3;
      this.graphics.PreferredBackBufferWidth = 1280;
      this.graphics.PreferredBackBufferHeight = 720;
      this.graphics.PreferMultiSampling = true;
      this.IsFixedTimeStep = true;
      this.TargetElapsedTime = new TimeSpan(333333L);
      this.graphics.SynchronizeWithVerticalRetrace = true;
      this.splashScreenGameComponent = new SplashScreenGameComponent((Game) this);
      ((Collection<IGameComponent>) this.Components).Add((IGameComponent) this.splashScreenGameComponent);
      this.sunBurnCoreSystem = new SunBurnCoreSystem((IServiceProvider) this.Services, this.Content);
      this.sceneState = new SceneState();
      this.sceneInterface = new SceneInterface();
      this.sceneInterface.CreateDefaultManagers(RenderingSystemType.Deferred, false);
      this.frameBuffers = new FrameBuffers(1024, 576, DetailPreference.Medium, DetailPreference.Medium);
      this.sceneInterface.ResourceManager.AssignOwnership((IUnloadable) this.frameBuffers);
      this.sceneInterface.PostProcessManager.AddPostProcessor((IPostProcessor) new HighDynamicRangePostProcessor());
      (this.sceneInterface.RenderManager as DeferredRenderManager).DepthFillOptimizationEnabled = false;
      (this.sceneInterface.RenderManager as DeferredRenderManager).OcclusionQueryEnabled = false;
      this.screenManager = new ScreenManager((Game) this);
      ((Collection<IGameComponent>) this.Components).Add((IGameComponent) this.screenManager);
      ((Collection<IGameComponent>) this.Components).Add((IGameComponent) new MessageDisplayComponent((Game) this));
      ((Collection<IGameComponent>) this.Components).Add((IGameComponent) new GamerServicesComponent((Game) this));
      this.m_Rand = new Random();
      this.m_ParticleSystemManager = new ParticleSystemManager();
      this.m_ParticleSystemManager.SetPerformanceProfilingIsEnabledForAllParticleSystems(false);
      this.m_ParticleSystemManager.SetUpdatesPerSecondForAllParticleSystems(30);
      DPSFDefaultSettings.UpdatesPerSecond = 30;
      DPSFDefaultSettings.PerformanceProfilingIsEnabled = false;
      DPSFDefaultSettings.AutoMemoryManagementSettings.MemoryManagementMode = AutoMemoryManagerModes.Disabled;
      this.m_Paused = false;
      this.m_SingleStep = false;
      this.m_ShowScoreboard = false;
      for (int index = 0; index < 48; ++index)
        this.m_HelmetMessage[index] = new HelmetMessage();
    }

    public bool IsTrialMode() => Guide.IsTrialMode;

    public bool CanPurchaseContent(PlayerIndex player)
    {
      if (Gamer.SignedInGamers[player] == null)
        return false;
      SignedInGamer signedInGamer = Gamer.SignedInGamers[player];
      return signedInGamer != null && signedInGamer.Privileges.AllowPurchaseContent;
    }

    private void InitBEPU()
    {
      this.m_Space = new Space();
      SolverSettings.DefaultMinimumIterations = 0;
      this.m_Space.Solver.IterationLimit = 4;
      GeneralConvexPairTester.UseSimplexCaching = true;
      SolverSettings.DefaultMinimumImpulse = 0.01f;
      this.m_Space.ThreadManager.AddThread((Action<object>) delegate
      {
        Thread.CurrentThread.SetProcessorAffinity(new int[1]
        {
          3
        });
      }, (object) null);
      this.m_Space.ThreadManager.AddThread((Action<object>) delegate
      {
        Thread.CurrentThread.SetProcessorAffinity(new int[1]
        {
          4
        });
      }, (object) null);
      this.m_Space.ThreadManager.AddThread((Action<object>) delegate
      {
        Thread.CurrentThread.SetProcessorAffinity(new int[1]
        {
          5
        });
      }, (object) null);
      this.m_Space.ForceUpdater.Gravity = new Vector3(0.0f, -29.43f, 0.0f);
    }

    protected virtual void Initialize()
    {
      this.InitEasyStorage();
      this.m_HudRenderTarget = new RenderTarget2D(this.GraphicsDevice, 512, 512);
      this.m_LCD1RenderTarget = new RenderTarget2D(this.GraphicsDevice, 512, 512);
      this.m_GraveRenderTarget = new RenderTarget2D(this.GraphicsDevice, 256, 256);
      this.m_TabletRenderTarget = new RenderTarget2D(this.GraphicsDevice, 256, 256);
      base.Initialize();
    }

    public void PreCacheSunburn()
    {
      g.m_PlayerManager.LoadModels();
      this.LoadMusic();
      this.LoadSFX();
      this.LoadMisc();
      this.contentRepository = g.m_App.Content.Load<ContentRepository>("Content");
      this.m_Scene1 = g.m_App.Content.Load<Scene>("Scenes/Scene");
      this.m_Environment1 = g.m_App.Content.Load<SceneEnvironment>("Environment/Environment");
      this.m_CollisionModel1 = g.m_App.Content.Load<Model>("Models\\ShipCol");
      g.m_BotPathManager.LoadBotPath();
    }

    protected virtual void LoadContent()
    {
      LoadingScreen.Load(this.screenManager, true, new PlayerIndex?(), 0, (GameScreen) new BackgroundScreen(), (GameScreen) new TitleScreen());
      this.m_BEPUDebugDrawer = new BEPUDebugDrawer(this.GraphicsDevice);
    }

    public void LoadMisc()
    {
      this.gradientTexture = this.Content.Load<Texture2D>("sprites\\back");
      this.compassHTexture = this.Content.Load<Texture2D>("sprites\\compass_horiz");
      this.compassVTexture = this.Content.Load<Texture2D>("sprites\\compass_vert");
      this.m_HealthBar = this.Content.Load<Texture2D>("sprites\\healthbar");
      this.markerTexture = this.Content.Load<Texture2D>("sprites\\compass_marker");
      this.m_SOSTexture = this.Content.Load<Texture2D>("sprites\\sos");
      this.m_SOS1Texture = this.Content.Load<Texture2D>("sprites\\sos_glitch1");
      this.m_SOS2Texture = this.Content.Load<Texture2D>("sprites\\sos_glitch2");
      this.m_HudNoise1 = this.Content.Load<Texture2D>("sprites\\noise");
      foreach (string preloadAsset in App.preloadAssets)
        this.Content.Load<object>(preloadAsset);
      this.font = this.screenManager.Font;
      this.hudFont = this.Content.Load<SpriteFont>("fonts\\Orbitron12Offset4");
      this.lcdFont = this.Content.Load<SpriteFont>("fonts\\bitstream14reg");
      this.graveFont = this.Content.Load<SpriteFont>("fonts\\palatino30");
      g.m_ItemManager.LoadContent(this.Content);
      g.m_ProjectileManager.LoadContent(this.Content);
    }

    public void LoadMusic()
    {
      this.m_Level1Music = this.Content.Load<Song>("Music/Horror_Ambience");
      this.m_HallucinateMusic = this.Content.Load<Song>("Music/Hallucinate");
      this.m_MenuMusic = this.Content.Load<Song>("Music/title");
    }

    public void LoadSFX()
    {
      g.m_SoundManager.Add(2, this.Content.Load<SoundEffect>("Sounds/footsteps_right1"));
      g.m_SoundManager.Add(3, this.Content.Load<SoundEffect>("Sounds/footsteps_right2"));
      g.m_SoundManager.Add(4, this.Content.Load<SoundEffect>("Sounds/footsteps_right3"));
      g.m_SoundManager.Add(5, this.Content.Load<SoundEffect>("Sounds/footsteps_left1"));
      g.m_SoundManager.Add(6, this.Content.Load<SoundEffect>("Sounds/footsteps_left2"));
      g.m_SoundManager.Add(7, this.Content.Load<SoundEffect>("Sounds/footsteps_left3"));
      g.m_SoundManager.Add(8, this.Content.Load<SoundEffect>("Sounds/select"));
      g.m_SoundManager.Add(9, this.Content.Load<SoundEffect>("Sounds/back"));
      g.m_SoundManager.Add(10, this.Content.Load<SoundEffect>("Sounds/switch5up"));
      g.m_SoundManager.Add(11, this.Content.Load<SoundEffect>("Sounds/switch5down"));
      g.m_SoundManager.Add(16, this.Content.Load<SoundEffect>("Sounds/CrateOpen"));
      g.m_SoundManager.Add(18, this.Content.Load<SoundEffect>("Sounds/TextPrompt"));
      g.m_SoundManager.Add(19, this.Content.Load<SoundEffect>("Sounds/LineBeep"));
      g.m_SoundManager.Add(21, this.Content.Load<SoundEffect>("Sounds/helmetwarning"));
      g.m_SoundManager.Add(22, this.Content.Load<SoundEffect>("Sounds/flatline"));
      g.m_SoundManager.Add(23, this.Content.Load<SoundEffect>("Sounds/Alarm"));
      g.m_SoundManager.Add(24, this.Content.Load<SoundEffect>("Sounds/breathe1"));
      g.m_SoundManager.Add(25, this.Content.Load<SoundEffect>("Sounds/locator"));
      g.m_SoundManager.Add(26, this.Content.Load<SoundEffect>("Sounds/ambience"));
      g.m_SoundManager.Add(27, this.Content.Load<SoundEffect>("Sounds/horror_enterroom"));
      g.m_SoundManager.Add(28, this.Content.Load<SoundEffect>("Sounds/horror_lookatblood"));
      g.m_SoundManager.Add(29, this.Content.Load<SoundEffect>("Sounds/door"));
      g.m_SoundManager.Add(30, this.Content.Load<SoundEffect>("Sounds/clue"));
      g.m_SoundManager.Add(31, this.Content.Load<SoundEffect>("Sounds/clatter"));
      g.m_SoundManager.Add(30, this.Content.Load<SoundEffect>("Sounds/clue"));
      g.m_SoundManager.Add(32, this.Content.Load<SoundEffect>("Sounds/ghosts"));
      g.m_SoundManager.Add(33, this.Content.Load<SoundEffect>("Sounds/cell"));
      g.m_SoundManager.Add(34, this.Content.Load<SoundEffect>("Sounds/keypad"));
      g.m_SoundManager.Add(35, this.Content.Load<SoundEffect>("Sounds/switch"));
      g.m_SoundManager.Add(36, this.Content.Load<SoundEffect>("Sounds/saw"));
      g.m_SoundManager.Add(37, this.Content.Load<SoundEffect>("Sounds/scream1"));
      g.m_SoundManager.Add(38, this.Content.Load<SoundEffect>("Sounds/heartbeat"));
      g.m_SoundManager.Add(39, this.Content.Load<SoundEffect>("Sounds/ticktock"));
      g.m_SoundManager.Add(40, this.Content.Load<SoundEffect>("Sounds/bigbong"));
      g.m_SoundManager.Add(41, this.Content.Load<SoundEffect>("Sounds/ocean"));
      g.m_SoundManager.Add(42, this.Content.Load<SoundEffect>("Sounds/knock"));
      g.m_SoundManager.Add(43, this.Content.Load<SoundEffect>("Sounds/locker"));
      g.m_SoundManager.Add(44, this.Content.Load<SoundEffect>("Sounds/growl1"));
      g.m_SoundManager.Add(45, this.Content.Load<SoundEffect>("Sounds/growl2"));
      g.m_SoundManager.Add(46, this.Content.Load<SoundEffect>("Sounds/growl3"));
      g.m_SoundManager.Add(47, this.Content.Load<SoundEffect>("Sounds/ugh"));
      g.m_SoundManager.Add(48, this.Content.Load<SoundEffect>("Sounds/cargo"));
    }

    protected virtual void UnloadContent()
    {
      this.sceneInterface.Unload();
      this.sunBurnCoreSystem.Unload();
      this.environment = (SceneEnvironment) null;
    }

    public void InitEasyStorage()
    {
      SharedSaveDevice sharedSaveDevice = new SharedSaveDevice();
      ((Collection<IGameComponent>) this.Components).Add((IGameComponent) sharedSaveDevice);
      this.saveDevice = (IAsyncSaveDevice) sharedSaveDevice;
      sharedSaveDevice.DeviceSelectorCanceled += (EventHandler<SaveDeviceEventArgs>) ((s, e) => e.Response = SaveDeviceEventResponse.Force);
      sharedSaveDevice.DeviceDisconnected += (EventHandler<SaveDeviceEventArgs>) ((s, e) => e.Response = SaveDeviceEventResponse.Force);
      sharedSaveDevice.PromptForDevice();
      this.saveDevice.SaveCompleted += new SaveCompletedEventHandler(this.saveDevice_SaveCompleted);
    }

    private void saveDevice_SaveCompleted(object sender, FileActionCompletedEventArgs args)
    {
    }

    public void Reset()
    {
      this.sceneInterface.Clear();
      this.sceneInterface.Remove((IScene) g.m_App.scene);
      this.scene = (Scene) null;
      g.m_App.m_RequestIntermissionEnd = false;
    }

    protected virtual void Update(GameTime gameTime)
    {
      this.m_GameTime = gameTime;
      if (!this.sceneInterface.Editor.EditorAttached)
      {
        KeyboardState state = Keyboard.GetState();
        if (((KeyboardState) ref state).IsKeyDown((Keys) 27))
          this.Exit();
      }
      base.Update(gameTime);
    }

    protected virtual void Draw(GameTime gameTime)
    {
      if (!SplashScreenGameComponent.DisplayComplete)
      {
        base.Draw(gameTime);
      }
      else
      {
        this.m_GameTime = gameTime;
        if (this.m_DoneSunburnStartupRender > 0)
        {
          --this.m_DoneSunburnStartupRender;
          this.sceneState.BeginFrameRendering(g.m_CameraManager.m_ViewMatrix, g.m_CameraManager.m_ProjectionMatrix, gameTime, (ISceneEnvironment) this.environment, this.frameBuffers, true);
          this.sceneInterface.BeginFrameRendering((ISceneState) this.sceneState);
          this.sceneInterface.RenderManager.Render();
          this.sceneInterface.EndFrameRendering();
          this.sceneState.EndFrameRendering();
        }
        base.Draw(gameTime);
      }
    }

    public float GetHudCentreX(string s, SpriteFont font) => (float) ((477.0 - (double) font.MeasureString(s).X) / 2.0);

    public void DrawHelmetHud()
    {
      Player localPlayer = g.m_PlayerManager.GetLocalPlayer();
      if (localPlayer == null || localPlayer.m_ViewSceneObject == null)
        return;
      for (int index = 0; index < 8; ++index)
        g.m_App.GraphicsDevice.SamplerStates[index] = SamplerState.PointClamp;
      this.GraphicsDevice.SetRenderTarget(g.m_App.m_HudRenderTarget);
      this.screenManager.SpriteBatch.Begin();
      this.GraphicsDevice.Clear((ClearOptions) 1, Color.Transparent, 1f, 0);
      if ((double) this.m_HudDistort1Time > this.m_GameTime.TotalGameTime.TotalSeconds && this.m_Rand.NextDouble() > 0.25)
      {
        Vector2 vector2;
        // ISSUE: explicit constructor call
        ((Vector2) ref vector2).\u002Ector((float) (this.m_Rand.Next(0, 6) - 3), (float) (this.m_Rand.Next(0, 6) - 3));
        this.screenManager.SpriteBatch.Draw(this.m_HudNoise1, vector2, Color.op_Multiply(new Color(1f, 1f, 1f, 1f), this.m_HudDistort1Alpha));
      }
      Color hudGreen = this.HUD_GREEN;
      if (localPlayer.m_Health < (sbyte) 30 && localPlayer.m_Health > (sbyte) 0)
      {
        Color hudRed = this.HUD_RED;
        this.AddHelmetMessage(HelmetMessage.TYPE.LOW_HEALTH, "DANGER LOW OXYGEN", new Vector2(this.GetHudCentreX("DANGER LOW OXYGEN", this.hudFont), 300f), 0.25f, this.HUD_RED, SoundManager.SFX.HelmetWarning, this.hudFont, false);
        this.screenManager.SpriteBatch.DrawString(this.hudFont, "O", new Vector2(60f, 395f), hudRed);
        this.screenManager.SpriteBatch.DrawString(this.hudFont, "2", new Vector2(72f, 388f), hudRed, 0.0f, Vector2.Zero, 0.75f, (SpriteEffects) 0, 0.0f);
        Rectangle rectangle;
        // ISSUE: explicit constructor call
        ((Rectangle) ref rectangle).\u002Ector(0, 0, (int) localPlayer.m_Health * 2, 20);
        this.screenManager.SpriteBatch.Draw(this.m_HealthBar, new Rectangle(90, 395, (int) ((double) localPlayer.m_Health * 2.0), 20), new Rectangle?(rectangle), hudRed);
      }
      this.DrawHelmetMessages();
      if (!this.m_Intermission)
      {
        localPlayer.DrawCompassToAirlockMarker();
        localPlayer.DrawCompassLocator();
      }
      if (localPlayer.m_State == Player.STATE.LocalDeath && !this.m_Intermission && this.m_Rand.NextDouble() > 0.25)
      {
        Vector2 vector2;
        // ISSUE: explicit constructor call
        ((Vector2) ref vector2).\u002Ector((float) (this.m_Rand.Next(0, 6) - 3), (float) (this.m_Rand.Next(0, 6) - 3));
        string s = "Dead";
        this.screenManager.SpriteBatch.DrawString(this.hudFont, s, Vector2.op_Addition(new Vector2(this.GetHudCentreX(s, g.m_App.hudFont), 200f), vector2), this.HUD_RED);
      }
      if (g.m_PlayerManager.GetLocalPlayer().m_ShowAudioDisplay)
        this.DrawHelmetAudio();
      if (localPlayer.m_State == Player.STATE.UsingLCD2 && localPlayer.m_LCDMenu == Player.LCDMenu.SECURITYQUESTION)
        localPlayer.DrawHelmetKeyboardCursor();
      if (localPlayer.m_State == Player.STATE.UsingSafe)
        localPlayer.DrawHelmetKeypad();
      if (localPlayer.m_State == Player.STATE.UsingLCD4 && localPlayer.m_LCDMenu == Player.LCDMenu.NETWORKMENU)
        localPlayer.DrawHelmetKeyboardCursor();
      if (localPlayer.m_State == Player.STATE.UsingLCD4 && localPlayer.m_LCDMenu == Player.LCDMenu.BRINGONLINE)
        localPlayer.DrawHelmetKeypad();
      this.screenManager.SpriteBatch.End();
      this.GraphicsDevice.SetRenderTarget((RenderTarget2D) null);
      int helmet = g.m_CameraManager.m_Helmet;
      if (helmet == -1 || g.m_ItemManager.m_Item[helmet].m_SceneObject == null || g.m_ItemManager.m_Item[helmet].m_Type != 7)
        return;
      DeferredObjectEffect effect = g.m_ItemManager.m_Item[helmet].m_SceneObject.RenderableMeshes[2].Effect as DeferredObjectEffect;
      effect.DiffuseMapTexture = (Texture2D) this.m_HudRenderTarget;
      effect.EmissiveColor = new Vector3(0.5f, 0.5f, 0.5f);
    }

    public void DrawHelmetAudio()
    {
      if (MediaPlayer.State != 1)
        return;
      if (this.visData == null)
        this.visData = new VisualizationData();
      MediaPlayer.GetVisualizationData(this.visData);
      for (int index = 0; index < this.visData.Frequencies.Count; ++index)
      {
        Rectangle rectangle;
        // ISSUE: explicit constructor call
        ((Rectangle) ref rectangle).\u002Ector(0, 0, 4, 4);
        if ((double) this.visData.Frequencies[index] > 9.9999997473787516E-05)
          g.m_App.screenManager.SpriteBatch.Draw(g.m_App.m_HealthBar, new Vector2((float) (200 + index / 2), 135f - this.visData.Frequencies[index] * 40f), new Rectangle?(rectangle), this.HUD_GREEN);
      }
    }

    public void DrawHelmetMessages()
    {
      float totalSeconds = (float) this.m_GameTime.TotalGameTime.TotalSeconds;
      for (int index = 0; index < 48; ++index)
      {
        if ((double) this.m_HelmetMessage[index].m_Time > (double) totalSeconds && this.m_HelmetMessage[index].m_Type != HelmetMessage.TYPE.NONE)
        {
          if ((double) this.m_HelmetMessage[index].m_DisplayProgress < 1.0)
            this.m_HelmetMessage[index].m_DisplayProgress += (float) (this.m_GameTime.ElapsedGameTime.TotalSeconds * 4.0);
          int length1 = this.m_HelmetMessage[index].m_Text.Length;
          int length2 = (int) ((double) length1 * (double) this.m_HelmetMessage[index].m_DisplayProgress);
          if (length2 > length1)
            length2 = length1;
          this.screenManager.SpriteBatch.DrawString(this.m_HelmetMessage[index].m_Font, this.m_HelmetMessage[index].m_Text.Substring(0, length2), this.m_HelmetMessage[index].m_Position, this.m_HelmetMessage[index].m_Colour);
        }
        else
          this.m_HelmetMessage[index].m_Type = HelmetMessage.TYPE.NONE;
      }
    }

    public void DrawHud()
    {
      if (this.m_ShowScoreboard || this.m_Intermission)
        return;
      Player localPlayer = g.m_PlayerManager.GetLocalPlayer();
      if (localPlayer != null && localPlayer.m_State == Player.STATE.LocalDeath || this.m_ShowScoreboard || localPlayer == null || localPlayer.m_WeaponItemIndex == -1)
        return;
      int id = g.m_ItemManager.m_Item[localPlayer.m_WeaponItemIndex].m_Id;
    }

    public bool AddHelmetMessage(
      HelmetMessage.TYPE type,
      string s,
      Vector2 position,
      float time,
      Color c,
      SoundManager.SFX sfxId,
      SpriteFont font,
      bool hideSysMsg)
    {
      int index1 = -1;
      bool flag = false;
      for (int index2 = 0; index2 < 48; ++index2)
      {
        if (this.m_HelmetMessage[index2].m_Type == type)
        {
          index1 = index2;
          flag = true;
          break;
        }
      }
      float totalSeconds = (float) this.m_GameTime.TotalGameTime.TotalSeconds;
      if (index1 == -1)
      {
        for (int index3 = 0; index3 < 48; ++index3)
        {
          if ((double) this.m_HelmetMessage[index3].m_Time < (double) totalSeconds)
          {
            index1 = index3;
            break;
          }
        }
      }
      if (index1 != -1)
      {
        this.m_HelmetMessage[index1].m_Type = type;
        this.m_HelmetMessage[index1].m_Text = s;
        this.m_HelmetMessage[index1].m_Time = totalSeconds + time;
        this.m_HelmetMessage[index1].m_Colour = c;
        this.m_HelmetMessage[index1].m_Position = position;
        this.m_HelmetMessage[index1].m_Font = font != null ? font : this.hudFont;
        this.m_HelmetMessage[index1].m_HideSysMsg = hideSysMsg;
        if (!flag)
        {
          this.m_HelmetMessage[index1].m_DisplayProgress = 0.0f;
          if ((double) time < 0.20000000298023224)
            this.m_HelmetMessage[index1].m_DisplayProgress = 1f;
          if (sfxId != SoundManager.SFX.END)
            g.m_SoundManager.Play((int) sfxId);
        }
      }
      return true;
    }

    public void RemoveHelmetMessage(HelmetMessage.TYPE type)
    {
      for (int index = 0; index < 48; ++index)
      {
        if (this.m_HelmetMessage[index].m_Type == type)
        {
          this.m_HelmetMessage[index].m_Type = HelmetMessage.TYPE.NONE;
          this.m_HelmetMessage[index].m_Time = 0.0f;
          this.m_HelmetMessage[index].m_Text = "";
        }
      }
    }

    public void ClearHelmetMessages()
    {
      for (int index = 0; index < 48; ++index)
      {
        this.m_HelmetMessage[index].m_Time = 0.0f;
        this.m_HelmetMessage[index].m_Text = "";
        this.m_HelmetMessage[index].m_Type = HelmetMessage.TYPE.NONE;
      }
    }

    public bool IsHelmetMessageActive()
    {
      for (int index = 0; index < 48; ++index)
      {
        if (this.m_HelmetMessage[index].m_Type != HelmetMessage.TYPE.NONE && this.m_HelmetMessage[index].m_HideSysMsg)
          return true;
      }
      return false;
    }

    public void DebugDraw()
    {
    }

    private Matrix GetViewMatrix() => Matrix.CreateLookAt(this.viewPosition, Vector3.op_Addition(this.viewPosition, Vector3.Transform(Vector3.Backward, Matrix.CreateFromYawPitchRoll(this.viewRotation.X, this.viewRotation.Y, this.viewRotation.Z))), Vector3.Up);

    public void ResetTriggers()
    {
      if (this.scene == null)
        return;
      for (int index1 = 0; index1 < this.scene.EntityGroups.Count; ++index1)
      {
        for (int index2 = 0; index2 < this.scene.EntityGroups[index1].Entities.Count; ++index2)
        {
          if ((object) g.m_App.scene.EntityGroups[index1].Entities[index2].GetType() == (object) typeof (MiscTriggerEntity))
            (g.m_App.scene.EntityGroups[index1].Entities[index2] as MiscTriggerEntity).Reset();
        }
      }
    }

    private static class Program
    {
      [STAThread]
      private static void Main(string[] args)
      {
        g.m_PlayerManager = new PlayerManager();
        g.m_CameraManager = new CameraManager();
        g.m_ItemManager = new ItemManager();
        g.m_BotPathManager = new BotPathManager();
        g.m_SoundManager = new SoundManager();
        g.m_LoadSaveManager = new LoadSaveManager();
        g.m_ProjectileManager = new ProjectileManager();
        if (Debugger.IsAttached)
        {
          using (App app = new App(args))
          {
            g.m_App = app;
            app.Run();
          }
        }
        else
        {
          try
          {
            using (App app = new App(args))
            {
              g.m_App = app;
              app.Run();
            }
          }
          catch (Exception ex)
          {
            using (App.CrashDebugGame crashDebugGame = new App.CrashDebugGame(ex))
              crashDebugGame.Run();
          }
        }
      }
    }

    public class CrashDebugGame : Game
    {
      private SpriteBatch spriteBatch;
      private SpriteFont font_debug;
      private readonly Exception exception;

      public CrashDebugGame(Exception exception)
      {
        this.exception = exception;
        GraphicsDeviceManager graphicsDeviceManager = new GraphicsDeviceManager((Game) this);
        this.Content.RootDirectory = "Content";
      }

      protected virtual void LoadContent()
      {
        this.font_debug = this.Content.Load<SpriteFont>("Fonts/SpriteFont1");
        this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
      }

      protected virtual void Update(GameTime gameTime)
      {
        GamePadState state = GamePad.GetState((PlayerIndex) 0);
        GamePadButtons buttons = ((GamePadState) ref state).Buttons;
        if (((GamePadButtons) ref buttons).Back == 1)
          this.Exit();
        base.Update(gameTime);
      }

      protected virtual void Draw(GameTime gameTime)
      {
        this.GraphicsDevice.Clear(Color.Black);
        this.spriteBatch.Begin();
        this.spriteBatch.DrawString(this.font_debug, "**** CRASH LOG ****", new Vector2(40f, 50f), Color.White);
        this.spriteBatch.DrawString(this.font_debug, "Press Back to Exit", new Vector2(40f, 70f), Color.White);
        this.spriteBatch.DrawString(this.font_debug, string.Format("Exception: {0}", (object) this.exception.Message), new Vector2(40f, 90f), Color.White);
        this.spriteBatch.DrawString(this.font_debug, string.Format("Stack Trace:\n{0}", (object) this.exception.StackTrace), new Vector2(40f, 110f), Color.White);
        this.spriteBatch.End();
        base.Draw(gameTime);
      }
    }
  }
}
