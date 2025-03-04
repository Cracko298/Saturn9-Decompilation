using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Saturn9;

internal class TitleScreen : GameScreen
{
	private ContentManager content;

	private Texture2D titleTexture;

	public GamePadState m_TitleGamepadState;

	public KeyboardState m_TitleKeyboardState;

	private bool done;

	public TitleScreen()
	{
		base.TransitionOnTime = TimeSpan.FromSeconds(5.0);
		base.TransitionOffTime = TimeSpan.FromSeconds(0.5);
	}

	public override void LoadContent()
	{
		if (content == null)
		{
			content = new ContentManager(base.ScreenManager.Game.Services, "Content");
		}
		titleTexture = content.Load<Texture2D>("sprites\\title");
		g.m_App.PreCacheSunburn();
		if (g.m_App.SOUNDON)
		{
			MediaPlayer.Play(g.m_App.m_MenuMusic);
			MediaPlayer.IsRepeating = true;
			MediaPlayer.Volume = 1f;
		}
	}

	public override void UnloadContent()
	{
		content.Unload();
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		if (m_TitleKeyboardState.IsKeyDown(Keys.Space) && !done)
		{
			if (g.m_App.m_PlayerOnePadId == (PlayerIndex)(-1))
			{
				g.m_App.m_PlayerOnePadId = PlayerIndex.One;
			}
			StartGame();
			ExitScreen();
		}
		for (int i = 0; i < 4; i++)
		{
			m_TitleGamepadState = GamePad.GetState((PlayerIndex)i);
			m_TitleKeyboardState = Keyboard.GetState((PlayerIndex)i);
			if ((m_TitleGamepadState.IsButtonDown(Buttons.A) || m_TitleGamepadState.IsButtonDown(Buttons.Start)) && !done && GamePad.GetCapabilities((PlayerIndex)i).GamePadType == GamePadType.GamePad)
			{
				g.m_App.m_PlayerOnePadId = (PlayerIndex)i;
				StartGame();
			}
		}
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen: false);
	}

	private void StartGame()
	{
		g.m_LoadSaveManager.LoadOptions();
		g.m_LoadSaveManager.LoadGame();
		base.ScreenManager.AddScreen(new MainMenuScreen(0), g.m_App.m_PlayerOnePadId);
		g.m_SoundManager.Play(8);
		done = true;
		ExitScreen();
	}

	public override void Draw(GameTime gameTime)
	{
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
		Viewport viewport = base.ScreenManager.GraphicsDevice.Viewport;
		new Rectangle(0, 0, viewport.Width, viewport.Height);
		spriteBatch.Begin();
		Color color = Color.White * base.TransitionAlpha;
		spriteBatch.Draw(titleTexture, new Vector2(0f, 200f), color);
		SpriteFont font = base.ScreenManager.Font;
		Vector2 vector = new Vector2(viewport.Width, viewport.Height);
		Vector2 vector2 = font.MeasureString("PRESS START");
		Vector2 vector3 = (vector - vector2) / 2f;
		spriteBatch.DrawString(font, "PRESS START", vector3 + new Vector2(0f, 250f), g.HIGHLIGHT_COL * base.TransitionAlpha * 0.75f);
		spriteBatch.End();
	}
}
