using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Saturn9;

internal class GameCompleteScreen : MenuScreen
{
	private ContentManager content;

	private VideoPlayer m_VideoPlayer;

	private Video m_OutroMovie;

	private float m_RumbleTime;

	private bool m_RumbleDone;

	private Texture2D texture;

	public GameCompleteScreen()
		: base("")
	{
		MenuEntry menuEntry = new MenuEntry("");
		menuEntry.Selected += OnAutoChoose;
		base.MenuEntries.Add(menuEntry);
		m_VideoPlayer = new VideoPlayer();
		m_RumbleTime = 0f;
		m_RumbleDone = false;
	}

	public override void LoadContent()
	{
		if (content == null)
		{
			content = new ContentManager(base.ScreenManager.Game.Services, "Content");
		}
		m_OutroMovie = content.Load<Video>("Video\\outro");
		if (m_VideoPlayer.State == MediaState.Stopped)
		{
			m_VideoPlayer.Play(m_OutroMovie);
			m_VideoPlayer.IsLooped = false;
			m_VideoPlayer.Volume = 1f;
			m_RumbleTime = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 4.8f;
			m_RumbleDone = false;
		}
	}

	public override void UnloadContent()
	{
		m_VideoPlayer.Dispose();
		content.Unload();
		m_VideoPlayer = null;
		m_OutroMovie = null;
		GC.Collect();
	}

	private void OnAutoChoose(object sender, PlayerIndexEventArgs e)
	{
		g.m_App.screenManager.AddScreen(new CreditsMenuScreen(gameComplete: true), base.ControllingPlayer);
		ExitScreen();
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		if (m_VideoPlayer.State == MediaState.Stopped)
		{
			g.m_App.screenManager.AddScreen(new CreditsMenuScreen(gameComplete: true), base.ControllingPlayer);
			base.ScreenManager.RemoveScreen(this);
		}
		if (!m_RumbleDone && m_RumbleTime < (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds)
		{
			g.m_App.m_RumbleFrames = 30;
			m_RumbleDone = true;
		}
		UpdateRumble(g.m_App.m_PlayerOnePadId);
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen: false);
	}

	public void UpdateRumble(PlayerIndex playerIndex)
	{
		if (g.m_App.m_RumbleFrames > 0 && g.m_App.m_OptionsVibration)
		{
			g.m_App.m_RumbleFrames--;
			GamePad.SetVibration(playerIndex, 0.5f, 0.5f);
		}
		else
		{
			GamePad.SetVibration(playerIndex, 0f, 0f);
		}
	}

	public override void Draw(GameTime gameTime)
	{
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
		base.ScreenManager.GraphicsDevice.Clear(Color.Black);
		spriteBatch.Begin();
		if (m_VideoPlayer.State != 0)
		{
			texture = m_VideoPlayer.GetTexture();
			if (texture != null)
			{
				spriteBatch.Draw(texture, new Rectangle(0, 0, 1280, 720), Color.White);
			}
		}
		spriteBatch.End();
		base.Draw(gameTime);
	}
}
