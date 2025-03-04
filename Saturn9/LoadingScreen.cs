using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Net;
using SynapseGaming.LightingSystem.Rendering;

namespace Saturn9;

internal class LoadingScreen : GameScreen
{
	private bool loadingIsSlow;

	private bool otherScreensAreGone;

	private GameScreen[] screensToLoad;

	private Thread backgroundThread;

	private EventWaitHandle backgroundThreadExit;

	private GraphicsDevice graphicsDevice;

	private NetworkSession networkSession;

	private IMessageDisplay messageDisplay;

	private GameTime loadStartTime;

	private TimeSpan loadAnimationTimer;

	private int m_PrevScreen = -1;

	private int m_CurrentTip = -1;

	private float m_ChangeTipTime;

	private float TIP_TIME = 6f;

	private float m_NextFrameTime;

	private int m_AnimFrame;

	private LoadingScreen(ScreenManager screenManager, bool loadingIsSlow, GameScreen[] screensToLoad)
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected O, but got Unknown
		this.loadingIsSlow = loadingIsSlow;
		this.screensToLoad = screensToLoad;
		base.TransitionOnTime = TimeSpan.FromSeconds(0.5);
		base.TransitionOffTime = TimeSpan.FromSeconds(1.5);
		if (loadingIsSlow)
		{
			backgroundThread = new Thread(BackgroundWorkerThread);
			backgroundThreadExit = new ManualResetEvent(initialState: false);
			graphicsDevice = screenManager.GraphicsDevice;
			IServiceProvider services = screenManager.Game.Services;
			networkSession = (NetworkSession)services.GetService(typeof(NetworkSession));
			messageDisplay = (IMessageDisplay)services.GetService(typeof(IMessageDisplay));
		}
	}

	public static void Load(ScreenManager screenManager, bool loadingIsSlow, PlayerIndex? controllingPlayer, int prevScreen, params GameScreen[] screensToLoad)
	{
		GameScreen[] screens = screenManager.GetScreens();
		foreach (GameScreen gameScreen in screens)
		{
			gameScreen.ExitScreen();
		}
		LoadingScreen loadingScreen = new LoadingScreen(screenManager, loadingIsSlow, screensToLoad);
		loadingScreen.m_PrevScreen = prevScreen;
		screenManager.AddScreen(loadingScreen, controllingPlayer);
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		if (!otherScreensAreGone)
		{
			return;
		}
		if (backgroundThread != null)
		{
			loadStartTime = gameTime;
			backgroundThread.Start();
		}
		base.ScreenManager.RemoveScreen(this);
		GameScreen[] array = screensToLoad;
		foreach (GameScreen gameScreen in array)
		{
			if (gameScreen != null)
			{
				base.ScreenManager.AddScreen(gameScreen, base.ControllingPlayer);
			}
		}
		if (backgroundThread != null)
		{
			backgroundThreadExit.Set();
			backgroundThread.Join();
		}
		base.ScreenManager.Game.ResetElapsedTime();
	}

	public override void Draw(GameTime gameTime)
	{
		if (!SplashScreenGameComponent.DisplayComplete)
		{
			base.Draw(gameTime);
			return;
		}
		if (base.ScreenState == ScreenState.Active && base.ScreenManager.GetScreens().Length == 1)
		{
			otherScreensAreGone = true;
		}
		base.ScreenManager.GraphicsDevice.Clear(Color.Black);
		if (!loadingIsSlow)
		{
			return;
		}
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
		_ = base.ScreenManager.Font;
		spriteBatch.Begin();
		m_NextFrameTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
		if (m_NextFrameTime > 0.1f)
		{
			m_NextFrameTime = 0f;
			m_AnimFrame++;
			if (m_AnimFrame > 17)
			{
				m_AnimFrame = 0;
			}
		}
		Rectangle value = new Rectangle(128 * m_AnimFrame, 0, 128, 128);
		Vector2 vector = new Vector2(1080f, 520f);
		spriteBatch.Draw(g.m_App.m_LoadingIcon, new Rectangle((int)vector.X, (int)vector.Y, 128, 128), value, Color.White * base.TransitionAlpha);
		spriteBatch.End();
	}

	private void BackgroundWorkerThread()
	{
		long lastTime = Stopwatch.GetTimestamp();
		while (!backgroundThreadExit.WaitOne(33))
		{
			GameTime gameTime = GetGameTime(ref lastTime);
			DrawLoadAnimation(gameTime);
			UpdateNetworkSession();
		}
	}

	private GameTime GetGameTime(ref long lastTime)
	{
		long timestamp = Stopwatch.GetTimestamp();
		long num = timestamp - lastTime;
		lastTime = timestamp;
		TimeSpan timeSpan = TimeSpan.FromTicks(num * 10000000 / Stopwatch.Frequency);
		return new GameTime(loadStartTime.TotalGameTime + timeSpan, timeSpan);
	}

	private void DrawLoadAnimation(GameTime gameTime)
	{
		if (graphicsDevice == null || graphicsDevice.IsDisposed)
		{
			return;
		}
		try
		{
			graphicsDevice.Clear(Color.Black);
			Draw(gameTime);
			if (messageDisplay != null)
			{
				messageDisplay.Update(gameTime);
				messageDisplay.Draw(gameTime);
			}
			graphicsDevice.Present();
		}
		catch
		{
			graphicsDevice = null;
		}
	}

	private void UpdateNetworkSession()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		if (networkSession == null || (int)networkSession.SessionState == 2)
		{
			return;
		}
		try
		{
			networkSession.Update();
		}
		catch
		{
			networkSession = null;
			g.m_App.m_NetworkSession = null;
		}
	}
}
