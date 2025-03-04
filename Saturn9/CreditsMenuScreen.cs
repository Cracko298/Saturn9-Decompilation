using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Saturn9;

internal class CreditsMenuScreen : MenuScreen
{
	private ContentManager content;

	private float m_ScrollY;

	private int m_NextScroll = 2;

	private bool m_GameComplete;

	public CreditsMenuScreen(bool gameComplete)
		: base("")
	{
		MenuEntry menuEntry = new MenuEntry("");
		menuEntry.Selected += OnChooseBack;
		base.MenuEntries.Add(menuEntry);
		m_GameComplete = gameComplete;
		m_ScrollY = 350f;
	}

	private void OnChooseBack(object sender, PlayerIndexEventArgs e)
	{
		if (m_GameComplete)
		{
			LoadingScreen.Load(base.ScreenManager, false, g.m_App.m_PlayerOnePadId, 0, new BackgroundScreen(), new MainMenuScreen(1));
		}
		ExitScreen();
	}

	protected override void OnCancel(PlayerIndex playerIndex)
	{
		if (m_GameComplete)
		{
			LoadingScreen.Load(base.ScreenManager, false, g.m_App.m_PlayerOnePadId, 0, new BackgroundScreen(), new MainMenuScreen(1));
		}
		ExitScreen();
	}

	public override void LoadContent()
	{
		if (content == null)
		{
			content = new ContentManager(base.ScreenManager.Game.Services, "Content");
		}
	}

	public override void UnloadContent()
	{
		content.Unload();
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		if (m_ScrollY < -700f)
		{
			if (m_GameComplete)
			{
				LoadingScreen.Load(base.ScreenManager, false, g.m_App.m_PlayerOnePadId, 0, new BackgroundScreen(), new MainMenuScreen(1));
			}
			ExitScreen();
		}
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
	}

	public override void Draw(GameTime gameTime)
	{
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
		SpriteFont font = base.ScreenManager.Font;
		if (m_GameComplete)
		{
			base.ScreenManager.GraphicsDevice.Clear(Color.Black);
		}
		spriteBatch.Begin();
		Vector2 vector = new Vector2(90f, m_ScrollY);
		m_NextScroll--;
		if (m_NextScroll <= 0)
		{
			m_ScrollY -= 1f;
			m_NextScroll = 2;
		}
		float num = 60f;
		float num2 = 40f;
		float num3 = 120f;
		float x = 200f;
		float x2 = 550f;
		string text = "CREDITS";
		if (m_GameComplete)
		{
			text = "CONGRATULATIONS";
		}
		Viewport viewport = base.ScreenManager.GraphicsDevice.Viewport;
		Vector2 vector2 = new Vector2(viewport.Width, viewport.Height);
		Vector2 vector3 = font.MeasureString(text);
		vector.X = (vector2.X - vector3.X) / 2f;
		vector.Y += m_ScrollY;
		spriteBatch.DrawString(font, text, vector, g.TITLE_COL * 0.75f * base.TransitionAlpha);
		text = "'Saturn 9' developed by Raoghard";
		vector3 = font.MeasureString(text);
		vector.X = (vector2.X - vector3.X) / 2f;
		vector.Y += num3;
		spriteBatch.DrawString(font, text, vector, g.TITLE_COL * 0.75f * base.TransitionAlpha);
		text = "Voice Acting";
		vector3 = font.MeasureString(text);
		vector.X = (vector2.X - vector3.X) / 2f;
		vector.Y += num3;
		spriteBatch.DrawString(font, text, vector, g.TITLE_COL * 0.75f * base.TransitionAlpha);
		text = "Captain Meyer";
		string text2 = "River Kanoff";
		vector.X = x;
		vector.Y += num;
		spriteBatch.DrawString(font, text, vector, g.TITLE_COL * 0.75f * base.TransitionAlpha);
		spriteBatch.DrawString(font, text2, vector + new Vector2(x2, 0f), g.TITLE_COL * 0.75f * base.TransitionAlpha);
		text = "Dr Holtz";
		text2 = "Lily Gornall";
		vector.Y += num;
		spriteBatch.DrawString(font, text, vector, g.TITLE_COL * 0.75f * base.TransitionAlpha);
		spriteBatch.DrawString(font, text2, vector + new Vector2(x2, 0f), g.TITLE_COL * 0.75f * base.TransitionAlpha);
		text = "Engineer Simmons";
		text2 = "Mike Guthrie";
		vector.Y += num;
		spriteBatch.DrawString(font, text, vector, g.TITLE_COL * 0.75f * base.TransitionAlpha);
		spriteBatch.DrawString(font, text2, vector + new Vector2(x2, 0f), g.TITLE_COL * 0.75f * base.TransitionAlpha);
		text = "Development";
		vector3 = font.MeasureString(text);
		vector.X = (vector2.X - vector3.X) / 2f;
		vector.Y += num3;
		spriteBatch.DrawString(font, text, vector, g.TITLE_COL * 0.75f * base.TransitionAlpha);
		text = "Design, Programming,\nArt, Animation and Audio";
		text2 = "Raoghard";
		vector.X = x;
		vector.Y += num;
		spriteBatch.DrawString(font, text, vector, g.TITLE_COL * 0.75f * base.TransitionAlpha);
		spriteBatch.DrawString(font, text2, vector + new Vector2(x2, 0f), g.TITLE_COL * 0.75f * base.TransitionAlpha);
		text = "Alien Crewmember Model";
		text2 = "Madina Chionidi and\nCoss Mousikides";
		vector.Y += num + num2;
		spriteBatch.DrawString(font, text, vector, g.TITLE_COL * 0.75f * base.TransitionAlpha);
		spriteBatch.DrawString(font, text2, vector + new Vector2(x2, 0f), g.TITLE_COL * 0.75f * base.TransitionAlpha);
		text = "Cover Illustration";
		text2 = "Coss Mousikides";
		vector.Y += num + num2;
		spriteBatch.DrawString(font, text, vector, g.TITLE_COL * 0.75f * base.TransitionAlpha);
		spriteBatch.DrawString(font, text2, vector + new Vector2(x2, 0f), g.TITLE_COL * 0.75f * base.TransitionAlpha);
		text = "Middleware";
		vector3 = font.MeasureString(text);
		vector.X = (vector2.X - vector3.X) / 2f;
		vector.Y += num3;
		spriteBatch.DrawString(font, text, vector, g.TITLE_COL * 0.75f * base.TransitionAlpha);
		text = "Rendering Engine";
		text2 = "Sunburn\n(synapsegaming.com)";
		vector3 = font.MeasureString(text);
		vector.X = x;
		vector.Y += num;
		spriteBatch.DrawString(font, text, vector, g.TITLE_COL * 0.75f * base.TransitionAlpha);
		spriteBatch.DrawString(font, text2, vector + new Vector2(x2, 0f), g.TITLE_COL * 0.75f * base.TransitionAlpha);
		text = "Physics Engine";
		text2 = "BEPU (bepuphysics.com)";
		vector3 = font.MeasureString(text);
		vector.Y += num + num2;
		spriteBatch.DrawString(font, text, vector, g.TITLE_COL * 0.75f * base.TransitionAlpha);
		spriteBatch.DrawString(font, text2, vector + new Vector2(x2, 0f), g.TITLE_COL * 0.75f * base.TransitionAlpha);
		text = "Particle Engine";
		text2 = "DPSF (xnaparticles.com)";
		vector3 = font.MeasureString(text);
		vector.Y += num;
		spriteBatch.DrawString(font, text, vector, g.TITLE_COL * 0.75f * base.TransitionAlpha);
		spriteBatch.DrawString(font, text2, vector + new Vector2(x2, 0f), g.TITLE_COL * 0.75f * base.TransitionAlpha);
		text = "End Theme Music";
		text2 = "www.purple-planet.com";
		vector3 = font.MeasureString(text);
		vector.Y += num;
		spriteBatch.DrawString(font, text, vector, g.TITLE_COL * 0.75f * base.TransitionAlpha);
		spriteBatch.DrawString(font, text2, vector + new Vector2(x2, 0f), g.TITLE_COL * 0.75f * base.TransitionAlpha);
		text = "Special Thanks";
		vector3 = font.MeasureString(text);
		vector.X = (vector2.X - vector3.X) / 2f;
		vector.Y += num3;
		spriteBatch.DrawString(font, text, vector, g.TITLE_COL * 0.75f * base.TransitionAlpha);
		text = "XNA Team, Madina, Coss, Lily, Louis, Eddy and Violet";
		vector3 = font.MeasureString(text);
		vector.X = (vector2.X - vector3.X) / 2f;
		vector.Y += num;
		spriteBatch.DrawString(font, text, vector, g.TITLE_COL * 0.75f * base.TransitionAlpha);
		text = "Contact";
		vector3 = font.MeasureString(text);
		vector.X = (vector2.X - vector3.X) / 2f;
		vector.Y += num3;
		spriteBatch.DrawString(font, text, vector, g.TITLE_COL * 0.75f * base.TransitionAlpha);
		text = "twitter: @raoghard\nemail: raoghard@live.com\n";
		vector3 = font.MeasureString(text);
		vector.X = (vector2.X - vector3.X) / 2f;
		vector.Y += num;
		spriteBatch.DrawString(font, text, vector, g.TITLE_COL * 0.75f * base.TransitionAlpha);
		spriteBatch.End();
		base.Draw(gameTime);
	}
}
