using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace Saturn9;

internal class TutorialMenuScreen : MenuScreen
{
	private ContentManager content;

	private VideoPlayer m_VideoPlayer;

	private Video m_IntroMovie;

	private Texture2D texture;

	public TutorialMenuScreen()
		: base("")
	{
		MenuEntry menuEntry = new MenuEntry("");
		menuEntry.Selected += OnAutoChoose;
		base.MenuEntries.Add(menuEntry);
		m_VideoPlayer = new VideoPlayer();
	}

	private void OnAutoChoose(object sender, PlayerIndexEventArgs e)
	{
		g.m_App.screenManager.AddScreen(new JoinTeamMenuScreen(g.m_App.m_NetworkSession), base.ControllingPlayer);
		ExitScreen();
	}

	public override void LoadContent()
	{
		if (content == null)
		{
			content = new ContentManager(base.ScreenManager.Game.Services, "Content");
		}
		m_IntroMovie = content.Load<Video>("Video\\intro");
		MediaPlayer.Stop();
		if (m_VideoPlayer.State == MediaState.Stopped)
		{
			m_VideoPlayer.Play(m_IntroMovie);
			m_VideoPlayer.IsLooped = false;
			m_VideoPlayer.Volume = 1f;
		}
	}

	public override void UnloadContent()
	{
		m_VideoPlayer.Dispose();
		content.Unload();
		m_VideoPlayer = null;
		m_IntroMovie = null;
		GC.Collect();
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		if (m_VideoPlayer.State == MediaState.Stopped)
		{
			g.m_App.screenManager.AddScreen(new JoinTeamMenuScreen(g.m_App.m_NetworkSession), base.ControllingPlayer);
			base.ScreenManager.RemoveScreen(this);
		}
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen: false);
	}

	public override void Draw(GameTime gameTime)
	{
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
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
