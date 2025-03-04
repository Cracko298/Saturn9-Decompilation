using System;
using System.Diagnostics;
using System.Threading;
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
using XnaToFna;

namespace Saturn9;

public class App : XnaToFnaGame
{
	private static class Program
	{
		[STAThread]
		private static void Main(string[] args)
		{
			XnaToFnaHelper.MainHook(args);
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
					return;
				}
			}
			try
			{
				using App app2 = new App(args);
				g.m_App = app2;
				app2.Run();
			}
			catch (Exception exception)
			{
				using CrashDebugGame crashDebugGame = new CrashDebugGame(exception);
				crashDebugGame.Run();
			}
		}
	}

	public class CrashDebugGame : XnaToFnaGame
	{
		private SpriteBatch spriteBatch;

		private SpriteFont font_debug;

		private readonly Exception exception;

		public CrashDebugGame(Exception exception)
		{
			this.exception = exception;
			new GraphicsDeviceManager(this);
			base.Content.RootDirectory = "Content";
		}

		protected override void LoadContent()
		{
			font_debug = base.Content.Load<SpriteFont>("Fonts/SpriteFont1");
			spriteBatch = new SpriteBatch(base.GraphicsDevice);
		}

		protected override void Update(GameTime gameTime)
		{
			XnaToFnaHelper.PreUpdate(gameTime);
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
			{
				Exit();
			}
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			base.GraphicsDevice.Clear(Color.Black);
			spriteBatch.Begin();
			spriteBatch.DrawString(font_debug, "**** CRASH LOG ****", new Vector2(40f, 50f), Color.White);
			spriteBatch.DrawString(font_debug, "Press Back to Exit", new Vector2(40f, 70f), Color.White);
			spriteBatch.DrawString(font_debug, $"Exception: {exception.Message}", new Vector2(40f, 90f), Color.White);
			spriteBatch.DrawString(font_debug, $"Stack Trace:\n{exception.StackTrace}", new Vector2(40f, 110f), Color.White);
			spriteBatch.End();
			base.Draw(gameTime);
		}
	}

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

	private static readonly string[] preloadAssets = new string[2] { "sprites\\back", "sprites\\busyicon" };

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

	public short m_RequestDeleteBotId = 255;

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

	public Color HUD_GREEN = new Color(45, 207, 255);

	public Color HUD_RED = new Color(255, 0, 0, 255);

	private VisualizationData visData;

	private Vector3 viewPosition = new Vector3(86.5f, 11.2f, 57f);

	private Vector3 viewRotation = new Vector3(-2.2f, 0.16f, 0f);

	private Matrix view = Matrix.Identity;

	private Matrix projection = Matrix.Identity;

	public App(string[] args)
	{
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Expected O, but got Unknown
		InitBEPU();
		graphics = new GraphicsDeviceManager(this);
		base.Content.RootDirectory = "Content";
		graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
		graphics.PreferredBackBufferWidth = 1280;
		graphics.PreferredBackBufferHeight = 720;
		graphics.PreferMultiSampling = true;
		base.IsFixedTimeStep = true;
		base.TargetElapsedTime = new TimeSpan(333333L);
		graphics.SynchronizeWithVerticalRetrace = true;
		splashScreenGameComponent = new SplashScreenGameComponent(this);
		base.Components.Add(splashScreenGameComponent);
		sunBurnCoreSystem = new SunBurnCoreSystem(base.Services, base.Content);
		sceneState = new SceneState();
		sceneInterface = new SceneInterface();
		sceneInterface.CreateDefaultManagers(RenderingSystemType.Deferred, includeautoloadedplugins: false);
		frameBuffers = new FrameBuffers(1024, 576, DetailPreference.Medium, DetailPreference.Medium);
		sceneInterface.ResourceManager.AssignOwnership(frameBuffers);
		sceneInterface.PostProcessManager.AddPostProcessor(new HighDynamicRangePostProcessor());
		(sceneInterface.RenderManager as DeferredRenderManager).DepthFillOptimizationEnabled = false;
		(sceneInterface.RenderManager as DeferredRenderManager).OcclusionQueryEnabled = false;
		screenManager = new ScreenManager(this);
		base.Components.Add(screenManager);
		base.Components.Add(new MessageDisplayComponent(this));
		base.Components.Add((IGameComponent)new GamerServicesComponent((Game)this));
		m_Rand = new Random();
		m_ParticleSystemManager = new ParticleSystemManager();
		m_ParticleSystemManager.SetPerformanceProfilingIsEnabledForAllParticleSystems(performanceProfilingIsEnabled: false);
		m_ParticleSystemManager.SetUpdatesPerSecondForAllParticleSystems(30);
		DPSFDefaultSettings.UpdatesPerSecond = 30;
		DPSFDefaultSettings.PerformanceProfilingIsEnabled = false;
		DPSFDefaultSettings.AutoMemoryManagementSettings.MemoryManagementMode = AutoMemoryManagerModes.Disabled;
		m_Paused = false;
		m_SingleStep = false;
		m_ShowScoreboard = false;
		for (int i = 0; i < 48; i++)
		{
			m_HelmetMessage[i] = new HelmetMessage();
		}
	}

	public bool IsTrialMode()
	{
		return Guide.IsTrialMode;
	}

	public bool CanPurchaseContent(PlayerIndex player)
	{
		if (Gamer.SignedInGamers[player] != null)
		{
			SignedInGamer val = Gamer.SignedInGamers[player];
			if (val != null)
			{
				return val.Privileges.AllowPurchaseContent;
			}
			return false;
		}
		return false;
	}

	private void InitBEPU()
	{
		m_Space = new Space();
		SolverSettings.DefaultMinimumIterations = 0;
		m_Space.Solver.IterationLimit = 4;
		GeneralConvexPairTester.UseSimplexCaching = true;
		SolverSettings.DefaultMinimumImpulse = 0.01f;
		m_Space.ThreadManager.AddThread(delegate
		{
			Thread.CurrentThread.SetProcessorAffinity(3);
		}, null);
		m_Space.ThreadManager.AddThread(delegate
		{
			Thread.CurrentThread.SetProcessorAffinity(4);
		}, null);
		m_Space.ThreadManager.AddThread(delegate
		{
			Thread.CurrentThread.SetProcessorAffinity(5);
		}, null);
		m_Space.ForceUpdater.Gravity = new Vector3(0f, -29.43f, 0f);
	}

	protected override void Initialize()
	{
		InitEasyStorage();
		m_HudRenderTarget = new RenderTarget2D(base.GraphicsDevice, 512, 512);
		m_LCD1RenderTarget = new RenderTarget2D(base.GraphicsDevice, 512, 512);
		m_GraveRenderTarget = new RenderTarget2D(base.GraphicsDevice, 256, 256);
		m_TabletRenderTarget = new RenderTarget2D(base.GraphicsDevice, 256, 256);
		base.Initialize();
	}

	public void PreCacheSunburn()
	{
		g.m_PlayerManager.LoadModels();
		LoadMusic();
		LoadSFX();
		LoadMisc();
		contentRepository = g.m_App.Content.Load<ContentRepository>("Content");
		m_Scene1 = g.m_App.Content.Load<Scene>("Scenes/Scene");
		m_Environment1 = g.m_App.Content.Load<SceneEnvironment>("Environment/Environment");
		m_CollisionModel1 = g.m_App.Content.Load<Model>("Models\\ShipCol");
		g.m_BotPathManager.LoadBotPath();
	}

	protected override void LoadContent()
	{
		LoadingScreen.Load(screenManager, true, null, 0, new BackgroundScreen(), new TitleScreen());
		m_BEPUDebugDrawer = new BEPUDebugDrawer(base.GraphicsDevice);
	}

	public void LoadMisc()
	{
		gradientTexture = base.Content.Load<Texture2D>("sprites\\back");
		compassHTexture = base.Content.Load<Texture2D>("sprites\\compass_horiz");
		compassVTexture = base.Content.Load<Texture2D>("sprites\\compass_vert");
		m_HealthBar = base.Content.Load<Texture2D>("sprites\\healthbar");
		markerTexture = base.Content.Load<Texture2D>("sprites\\compass_marker");
		m_SOSTexture = base.Content.Load<Texture2D>("sprites\\sos");
		m_SOS1Texture = base.Content.Load<Texture2D>("sprites\\sos_glitch1");
		m_SOS2Texture = base.Content.Load<Texture2D>("sprites\\sos_glitch2");
		m_HudNoise1 = base.Content.Load<Texture2D>("sprites\\noise");
		string[] array = preloadAssets;
		foreach (string assetName in array)
		{
			base.Content.Load<object>(assetName);
		}
		font = screenManager.Font;
		hudFont = base.Content.Load<SpriteFont>("fonts\\Orbitron12Offset4");
		lcdFont = base.Content.Load<SpriteFont>("fonts\\bitstream14reg");
		graveFont = base.Content.Load<SpriteFont>("fonts\\palatino30");
		g.m_ItemManager.LoadContent(base.Content);
		g.m_ProjectileManager.LoadContent(base.Content);
	}

	public void LoadMusic()
	{
		m_Level1Music = base.Content.Load<Song>("Music/Horror_Ambience");
		m_HallucinateMusic = base.Content.Load<Song>("Music/Hallucinate");
		m_MenuMusic = base.Content.Load<Song>("Music/title");
	}

	public void LoadSFX()
	{
		g.m_SoundManager.Add(2, base.Content.Load<SoundEffect>("Sounds/footsteps_right1"));
		g.m_SoundManager.Add(3, base.Content.Load<SoundEffect>("Sounds/footsteps_right2"));
		g.m_SoundManager.Add(4, base.Content.Load<SoundEffect>("Sounds/footsteps_right3"));
		g.m_SoundManager.Add(5, base.Content.Load<SoundEffect>("Sounds/footsteps_left1"));
		g.m_SoundManager.Add(6, base.Content.Load<SoundEffect>("Sounds/footsteps_left2"));
		g.m_SoundManager.Add(7, base.Content.Load<SoundEffect>("Sounds/footsteps_left3"));
		g.m_SoundManager.Add(8, base.Content.Load<SoundEffect>("Sounds/select"));
		g.m_SoundManager.Add(9, base.Content.Load<SoundEffect>("Sounds/back"));
		g.m_SoundManager.Add(10, base.Content.Load<SoundEffect>("Sounds/switch5up"));
		g.m_SoundManager.Add(11, base.Content.Load<SoundEffect>("Sounds/switch5down"));
		g.m_SoundManager.Add(16, base.Content.Load<SoundEffect>("Sounds/CrateOpen"));
		g.m_SoundManager.Add(18, base.Content.Load<SoundEffect>("Sounds/TextPrompt"));
		g.m_SoundManager.Add(19, base.Content.Load<SoundEffect>("Sounds/LineBeep"));
		g.m_SoundManager.Add(21, base.Content.Load<SoundEffect>("Sounds/helmetwarning"));
		g.m_SoundManager.Add(22, base.Content.Load<SoundEffect>("Sounds/flatline"));
		g.m_SoundManager.Add(23, base.Content.Load<SoundEffect>("Sounds/Alarm"));
		g.m_SoundManager.Add(24, base.Content.Load<SoundEffect>("Sounds/breathe1"));
		g.m_SoundManager.Add(25, base.Content.Load<SoundEffect>("Sounds/locator"));
		g.m_SoundManager.Add(26, base.Content.Load<SoundEffect>("Sounds/ambience"));
		g.m_SoundManager.Add(27, base.Content.Load<SoundEffect>("Sounds/horror_enterroom"));
		g.m_SoundManager.Add(28, base.Content.Load<SoundEffect>("Sounds/horror_lookatblood"));
		g.m_SoundManager.Add(29, base.Content.Load<SoundEffect>("Sounds/door"));
		g.m_SoundManager.Add(30, base.Content.Load<SoundEffect>("Sounds/clue"));
		g.m_SoundManager.Add(31, base.Content.Load<SoundEffect>("Sounds/clatter"));
		g.m_SoundManager.Add(30, base.Content.Load<SoundEffect>("Sounds/clue"));
		g.m_SoundManager.Add(32, base.Content.Load<SoundEffect>("Sounds/ghosts"));
		g.m_SoundManager.Add(33, base.Content.Load<SoundEffect>("Sounds/cell"));
		g.m_SoundManager.Add(34, base.Content.Load<SoundEffect>("Sounds/keypad"));
		g.m_SoundManager.Add(35, base.Content.Load<SoundEffect>("Sounds/switch"));
		g.m_SoundManager.Add(36, base.Content.Load<SoundEffect>("Sounds/saw"));
		g.m_SoundManager.Add(37, base.Content.Load<SoundEffect>("Sounds/scream1"));
		g.m_SoundManager.Add(38, base.Content.Load<SoundEffect>("Sounds/heartbeat"));
		g.m_SoundManager.Add(39, base.Content.Load<SoundEffect>("Sounds/ticktock"));
		g.m_SoundManager.Add(40, base.Content.Load<SoundEffect>("Sounds/bigbong"));
		g.m_SoundManager.Add(41, base.Content.Load<SoundEffect>("Sounds/ocean"));
		g.m_SoundManager.Add(42, base.Content.Load<SoundEffect>("Sounds/knock"));
		g.m_SoundManager.Add(43, base.Content.Load<SoundEffect>("Sounds/locker"));
		g.m_SoundManager.Add(44, base.Content.Load<SoundEffect>("Sounds/growl1"));
		g.m_SoundManager.Add(45, base.Content.Load<SoundEffect>("Sounds/growl2"));
		g.m_SoundManager.Add(46, base.Content.Load<SoundEffect>("Sounds/growl3"));
		g.m_SoundManager.Add(47, base.Content.Load<SoundEffect>("Sounds/ugh"));
		g.m_SoundManager.Add(48, base.Content.Load<SoundEffect>("Sounds/cargo"));
	}

	protected override void UnloadContent()
	{
		sceneInterface.Unload();
		sunBurnCoreSystem.Unload();
		environment = null;
	}

	public void InitEasyStorage()
	{
		SharedSaveDevice sharedSaveDevice = new SharedSaveDevice();
		base.Components.Add(sharedSaveDevice);
		saveDevice = sharedSaveDevice;
		sharedSaveDevice.DeviceSelectorCanceled += delegate(object s, SaveDeviceEventArgs e)
		{
			e.Response = SaveDeviceEventResponse.Force;
		};
		sharedSaveDevice.DeviceDisconnected += delegate(object s, SaveDeviceEventArgs e)
		{
			e.Response = SaveDeviceEventResponse.Force;
		};
		sharedSaveDevice.PromptForDevice();
		saveDevice.SaveCompleted += saveDevice_SaveCompleted;
	}

	private void saveDevice_SaveCompleted(object sender, FileActionCompletedEventArgs args)
	{
	}

	public void Reset()
	{
		sceneInterface.Clear();
		sceneInterface.Remove(g.m_App.scene);
		scene = null;
		g.m_App.m_RequestIntermissionEnd = false;
	}

	protected override void Update(GameTime gameTime)
	{
		XnaToFnaHelper.PreUpdate(gameTime);
		m_GameTime = gameTime;
		if (!sceneInterface.Editor.EditorAttached && Keyboard.GetState().IsKeyDown(Keys.Escape))
		{
			Exit();
		}
		base.Update(gameTime);
	}

	protected override void Draw(GameTime gameTime)
	{
		if (!SplashScreenGameComponent.DisplayComplete)
		{
			base.Draw(gameTime);
			return;
		}
		m_GameTime = gameTime;
		if (m_DoneSunburnStartupRender > 0)
		{
			m_DoneSunburnStartupRender--;
			sceneState.BeginFrameRendering(g.m_CameraManager.m_ViewMatrix, g.m_CameraManager.m_ProjectionMatrix, gameTime, environment, frameBuffers, renderingtoscreen: true);
			sceneInterface.BeginFrameRendering(sceneState);
			sceneInterface.RenderManager.Render();
			sceneInterface.EndFrameRendering();
			sceneState.EndFrameRendering();
		}
		base.Draw(gameTime);
	}

	public float GetHudCentreX(string s, SpriteFont font)
	{
		return (477f - font.MeasureString(s).X) / 2f;
	}

	public void DrawHelmetHud()
	{
		Player localPlayer = g.m_PlayerManager.GetLocalPlayer();
		if (localPlayer != null && localPlayer.m_ViewSceneObject != null)
		{
			for (int i = 0; i < 8; i++)
			{
				g.m_App.GraphicsDevice.SamplerStates[i] = SamplerState.PointClamp;
			}
			base.GraphicsDevice.SetRenderTarget(g.m_App.m_HudRenderTarget);
			screenManager.SpriteBatch.Begin();
			base.GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 1f, 0);
			if (m_HudDistort1Time > (float)m_GameTime.TotalGameTime.TotalSeconds && m_Rand.NextDouble() > 0.25)
			{
				Vector2 position = new Vector2(m_Rand.Next(0, 6) - 3, m_Rand.Next(0, 6) - 3);
				screenManager.SpriteBatch.Draw(m_HudNoise1, position, new Color(1f, 1f, 1f, 1f) * m_HudDistort1Alpha);
			}
			Color hUD_GREEN = HUD_GREEN;
			if (localPlayer.m_Health < 30 && localPlayer.m_Health > 0)
			{
				hUD_GREEN = HUD_RED;
				string s = "DANGER LOW OXYGEN";
				AddHelmetMessage(HelmetMessage.TYPE.LOW_HEALTH, "DANGER LOW OXYGEN", new Vector2(GetHudCentreX(s, hudFont), 300f), 0.25f, HUD_RED, SoundManager.SFX.HelmetWarning, hudFont, hideSysMsg: false);
				screenManager.SpriteBatch.DrawString(hudFont, "O", new Vector2(60f, 395f), hUD_GREEN);
				screenManager.SpriteBatch.DrawString(hudFont, "2", new Vector2(72f, 388f), hUD_GREEN, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);
				Rectangle value = new Rectangle(0, 0, localPlayer.m_Health * 2, 20);
				screenManager.SpriteBatch.Draw(m_HealthBar, new Rectangle(90, 395, (int)((float)localPlayer.m_Health * 2f), 20), value, hUD_GREEN);
			}
			DrawHelmetMessages();
			if (!m_Intermission)
			{
				localPlayer.DrawCompassToAirlockMarker();
				localPlayer.DrawCompassLocator();
			}
			if (localPlayer.m_State == Player.STATE.LocalDeath && !m_Intermission && m_Rand.NextDouble() > 0.25)
			{
				Vector2 vector = new Vector2(m_Rand.Next(0, 6) - 3, m_Rand.Next(0, 6) - 3);
				string text = "Dead";
				screenManager.SpriteBatch.DrawString(hudFont, text, new Vector2(GetHudCentreX(text, g.m_App.hudFont), 200f) + vector, HUD_RED);
			}
			if (g.m_PlayerManager.GetLocalPlayer().m_ShowAudioDisplay)
			{
				DrawHelmetAudio();
			}
			if (localPlayer.m_State == Player.STATE.UsingLCD2 && localPlayer.m_LCDMenu == Player.LCDMenu.SECURITYQUESTION)
			{
				localPlayer.DrawHelmetKeyboardCursor();
			}
			if (localPlayer.m_State == Player.STATE.UsingSafe)
			{
				localPlayer.DrawHelmetKeypad();
			}
			if (localPlayer.m_State == Player.STATE.UsingLCD4 && localPlayer.m_LCDMenu == Player.LCDMenu.NETWORKMENU)
			{
				localPlayer.DrawHelmetKeyboardCursor();
			}
			if (localPlayer.m_State == Player.STATE.UsingLCD4 && localPlayer.m_LCDMenu == Player.LCDMenu.BRINGONLINE)
			{
				localPlayer.DrawHelmetKeypad();
			}
			screenManager.SpriteBatch.End();
			base.GraphicsDevice.SetRenderTarget(null);
			int helmet = g.m_CameraManager.m_Helmet;
			if (helmet != -1 && g.m_ItemManager.m_Item[helmet].m_SceneObject != null && g.m_ItemManager.m_Item[helmet].m_Type == 7)
			{
				DeferredObjectEffect deferredObjectEffect = g.m_ItemManager.m_Item[helmet].m_SceneObject.RenderableMeshes[2].Effect as DeferredObjectEffect;
				deferredObjectEffect.DiffuseMapTexture = m_HudRenderTarget;
				deferredObjectEffect.EmissiveColor = new Vector3(0.5f, 0.5f, 0.5f);
			}
		}
	}

	public void DrawHelmetAudio()
	{
		if (MediaPlayer.State != MediaState.Playing)
		{
			return;
		}
		if (visData == null)
		{
			visData = new VisualizationData();
		}
		MediaPlayer.GetVisualizationData(visData);
		for (int i = 0; i < visData.Frequencies.Count; i++)
		{
			Rectangle value = new Rectangle(0, 0, 4, 4);
			if (visData.Frequencies[i] > 0.0001f)
			{
				g.m_App.screenManager.SpriteBatch.Draw(g.m_App.m_HealthBar, new Vector2(200 + i / 2, 135f - visData.Frequencies[i] * 40f), value, HUD_GREEN);
			}
		}
	}

	public void DrawHelmetMessages()
	{
		float num = (float)m_GameTime.TotalGameTime.TotalSeconds;
		for (int i = 0; i < 48; i++)
		{
			if (m_HelmetMessage[i].m_Time > num && m_HelmetMessage[i].m_Type != 0)
			{
				if (m_HelmetMessage[i].m_DisplayProgress < 1f)
				{
					m_HelmetMessage[i].m_DisplayProgress += (float)m_GameTime.ElapsedGameTime.TotalSeconds * 4f;
				}
				int length = m_HelmetMessage[i].m_Text.Length;
				int num2 = (int)((float)length * m_HelmetMessage[i].m_DisplayProgress);
				if (num2 > length)
				{
					num2 = length;
				}
				screenManager.SpriteBatch.DrawString(m_HelmetMessage[i].m_Font, m_HelmetMessage[i].m_Text.Substring(0, num2), m_HelmetMessage[i].m_Position, m_HelmetMessage[i].m_Colour);
			}
			else
			{
				m_HelmetMessage[i].m_Type = HelmetMessage.TYPE.NONE;
			}
		}
	}

	public void DrawHud()
	{
		if (!m_ShowScoreboard && !m_Intermission)
		{
			Player localPlayer = g.m_PlayerManager.GetLocalPlayer();
			if ((localPlayer == null || localPlayer.m_State != Player.STATE.LocalDeath) && !m_ShowScoreboard && localPlayer != null && localPlayer.m_WeaponItemIndex != -1)
			{
				_ = g.m_ItemManager.m_Item[localPlayer.m_WeaponItemIndex].m_Id;
				_ = -1;
			}
		}
	}

	public bool AddHelmetMessage(HelmetMessage.TYPE type, string s, Vector2 position, float time, Color c, SoundManager.SFX sfxId, SpriteFont font, bool hideSysMsg)
	{
		int num = -1;
		bool flag = false;
		for (int i = 0; i < 48; i++)
		{
			if (m_HelmetMessage[i].m_Type == type)
			{
				num = i;
				flag = true;
				break;
			}
		}
		float num2 = (float)m_GameTime.TotalGameTime.TotalSeconds;
		if (num == -1)
		{
			for (int j = 0; j < 48; j++)
			{
				if (m_HelmetMessage[j].m_Time < num2)
				{
					num = j;
					break;
				}
			}
		}
		if (num != -1)
		{
			m_HelmetMessage[num].m_Type = type;
			m_HelmetMessage[num].m_Text = s;
			m_HelmetMessage[num].m_Time = num2 + time;
			m_HelmetMessage[num].m_Colour = c;
			m_HelmetMessage[num].m_Position = position;
			if (font == null)
			{
				m_HelmetMessage[num].m_Font = hudFont;
			}
			else
			{
				m_HelmetMessage[num].m_Font = font;
			}
			m_HelmetMessage[num].m_HideSysMsg = hideSysMsg;
			if (!flag)
			{
				m_HelmetMessage[num].m_DisplayProgress = 0f;
				if (time < 0.2f)
				{
					m_HelmetMessage[num].m_DisplayProgress = 1f;
				}
				if (sfxId != SoundManager.SFX.END)
				{
					g.m_SoundManager.Play((int)sfxId);
				}
			}
		}
		return true;
	}

	public void RemoveHelmetMessage(HelmetMessage.TYPE type)
	{
		for (int i = 0; i < 48; i++)
		{
			if (m_HelmetMessage[i].m_Type == type)
			{
				m_HelmetMessage[i].m_Type = HelmetMessage.TYPE.NONE;
				m_HelmetMessage[i].m_Time = 0f;
				m_HelmetMessage[i].m_Text = "";
			}
		}
	}

	public void ClearHelmetMessages()
	{
		for (int i = 0; i < 48; i++)
		{
			m_HelmetMessage[i].m_Time = 0f;
			m_HelmetMessage[i].m_Text = "";
			m_HelmetMessage[i].m_Type = HelmetMessage.TYPE.NONE;
		}
	}

	public bool IsHelmetMessageActive()
	{
		for (int i = 0; i < 48; i++)
		{
			if (m_HelmetMessage[i].m_Type != 0 && m_HelmetMessage[i].m_HideSysMsg)
			{
				return true;
			}
		}
		return false;
	}

	public void DebugDraw()
	{
	}

	private Matrix GetViewMatrix()
	{
		Matrix matrix = Matrix.CreateFromYawPitchRoll(viewRotation.X, viewRotation.Y, viewRotation.Z);
		Vector3 cameraTarget = viewPosition + Vector3.Transform(Vector3.Backward, matrix);
		return Matrix.CreateLookAt(viewPosition, cameraTarget, Vector3.Up);
	}

	public void ResetTriggers()
	{
		if (scene == null)
		{
			return;
		}
		for (int i = 0; i < scene.EntityGroups.Count; i++)
		{
			for (int j = 0; j < scene.EntityGroups[i].Entities.Count; j++)
			{
				if ((object)g.m_App.scene.EntityGroups[i].Entities[j].GetType() == typeof(MiscTriggerEntity))
				{
					MiscTriggerEntity miscTriggerEntity = g.m_App.scene.EntityGroups[i].Entities[j] as MiscTriggerEntity;
					miscTriggerEntity.Reset();
				}
			}
		}
	}
}
