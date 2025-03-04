using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Saturn9;

internal class BackgroundScreen : GameScreen
{
	private ContentManager content;

	private Texture2D backgroundTexture;

	private static float m_Rotation;

	private static float m_Rotation2;

	private Texture2D m_AlphaCloud;

	public BackgroundScreen()
	{
		base.TransitionOnTime = TimeSpan.FromSeconds(0.5);
		base.TransitionOffTime = TimeSpan.FromSeconds(0.5);
	}

	public override void LoadContent()
	{
		if (content == null)
		{
			content = new ContentManager(base.ScreenManager.Game.Services, "Content");
		}
		backgroundTexture = content.Load<Texture2D>("backgrounds\\background");
		m_AlphaCloud = content.Load<Texture2D>("backgrounds\\smoke");
		m_Rotation = (float)g.m_App.m_Rand.NextDouble();
		m_Rotation2 = (float)g.m_App.m_Rand.NextDouble();
	}

	public override void UnloadContent()
	{
		content.Unload();
		GC.Collect();
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen: false);
	}

	public override void Draw(GameTime gameTime)
	{
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
		Viewport viewport = base.ScreenManager.GraphicsDevice.Viewport;
		Rectangle destinationRectangle = new Rectangle(0, 0, viewport.Width, viewport.Height);
		base.ScreenManager.GraphicsDevice.Clear(Color.Black);
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
		spriteBatch.Draw(backgroundTexture, destinationRectangle, new Color(base.TransitionAlpha, base.TransitionAlpha, base.TransitionAlpha));
		Color color = Color.DarkRed * 0.75f * base.TransitionAlpha;
		new Rectangle(0, 0, 1024, 1024);
		Rectangle destinationRectangle2 = new Rectangle(640, 360, 1468, 1468);
		spriteBatch.Draw(m_AlphaCloud, destinationRectangle2, null, color, m_Rotation, new Vector2(512f, 512f), SpriteEffects.None, 0f);
		spriteBatch.Draw(m_AlphaCloud, destinationRectangle2, null, color, m_Rotation2, new Vector2(512f, 512f), SpriteEffects.None, 0f);
		m_Rotation += (float)g.m_App.m_GameTime.ElapsedGameTime.TotalSeconds * 0.01f;
		m_Rotation2 -= (float)g.m_App.m_GameTime.ElapsedGameTime.TotalSeconds * 0.01f;
		spriteBatch.End();
		base.Draw(gameTime);
	}
}
