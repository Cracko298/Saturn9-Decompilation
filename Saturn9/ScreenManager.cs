using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace Saturn9;

public class ScreenManager : DrawableGameComponent
{
	private List<GameScreen> screens = new List<GameScreen>();

	private List<GameScreen> screensToUpdate = new List<GameScreen>();

	private InputState input = new InputState();

	private SpriteBatch spriteBatch;

	private SpriteFont font;

	public Texture2D blankTexture;

	private bool isInitialized;

	private bool traceEnabled;

	public SpriteBatch SpriteBatch => spriteBatch;

	public SpriteFont Font
	{
		get
		{
			return font;
		}
		set
		{
			font = value;
		}
	}

	public bool TraceEnabled
	{
		get
		{
			return traceEnabled;
		}
		set
		{
			traceEnabled = value;
		}
	}

	public ScreenManager(Game game)
		: base(game)
	{
		TouchPanel.EnabledGestures = GestureType.None;
	}

	public override void Initialize()
	{
		base.Initialize();
		isInitialized = true;
	}

	protected override void LoadContent()
	{
		g.m_App.m_LoadingIcon = g.m_App.Content.Load<Texture2D>("Sprites/icon");
		ContentManager content = base.Game.Content;
		spriteBatch = new SpriteBatch(base.GraphicsDevice);
		font = base.Game.Content.Load<SpriteFont>("fonts\\orbitron26");
		blankTexture = content.Load<Texture2D>("sprites\\blank");
		foreach (GameScreen screen in screens)
		{
			screen.LoadContent();
		}
	}

	protected override void UnloadContent()
	{
		foreach (GameScreen screen in screens)
		{
			screen.UnloadContent();
		}
	}

	public override void Update(GameTime gameTime)
	{
		input.Update();
		screensToUpdate.Clear();
		foreach (GameScreen screen in screens)
		{
			screensToUpdate.Add(screen);
		}
		bool flag = !base.Game.IsActive;
		bool coveredByOtherScreen = false;
		while (screensToUpdate.Count > 0)
		{
			GameScreen gameScreen = screensToUpdate[screensToUpdate.Count - 1];
			screensToUpdate.RemoveAt(screensToUpdate.Count - 1);
			gameScreen.Update(gameTime, flag, coveredByOtherScreen);
			if (gameScreen.ScreenState == ScreenState.TransitionOn || gameScreen.ScreenState == ScreenState.Active)
			{
				if (!flag)
				{
					gameScreen.HandleInput(input);
					flag = true;
				}
				if (!gameScreen.IsPopup)
				{
					coveredByOtherScreen = true;
				}
			}
		}
		if (traceEnabled)
		{
			TraceScreens();
		}
	}

	private void TraceScreens()
	{
		List<string> list = new List<string>();
		foreach (GameScreen screen in screens)
		{
			list.Add(screen.GetType().Name);
		}
	}

	public override void Draw(GameTime gameTime)
	{
		foreach (GameScreen screen in screens)
		{
			if (screen.ScreenState != ScreenState.Hidden)
			{
				screen.Draw(gameTime);
			}
		}
	}

	public void AddScreen(GameScreen screen, PlayerIndex? controllingPlayer)
	{
		screen.ControllingPlayer = controllingPlayer;
		screen.ScreenManager = this;
		screen.IsExiting = false;
		if (isInitialized)
		{
			screen.LoadContent();
		}
		screens.Add(screen);
		TouchPanel.EnabledGestures = screen.EnabledGestures;
	}

	public void RemoveScreen(GameScreen screen)
	{
		if (isInitialized)
		{
			screen.UnloadContent();
		}
		screens.Remove(screen);
		screensToUpdate.Remove(screen);
		if (screens.Count > 0)
		{
			TouchPanel.EnabledGestures = screens[screens.Count - 1].EnabledGestures;
		}
	}

	public GameScreen[] GetScreens()
	{
		return screens.ToArray();
	}

	public void FadeBackBufferToBlack(float alpha)
	{
		Viewport viewport = base.GraphicsDevice.Viewport;
		spriteBatch.Begin();
		spriteBatch.Draw(blankTexture, new Rectangle(0, 0, viewport.Width, viewport.Height), Color.Black * alpha);
		spriteBatch.End();
	}
}
