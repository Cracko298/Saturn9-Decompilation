using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Saturn9;

internal class MessageBoxScreen : GameScreen
{
	private string message;

	private Texture2D gradientTexture;

	public event EventHandler<PlayerIndexEventArgs> Accepted;

	public event EventHandler<PlayerIndexEventArgs> Cancelled;

	public MessageBoxScreen(string message)
		: this(message, includeUsageText: true)
	{
	}

	public MessageBoxScreen(string message, bool includeUsageText)
	{
		if (includeUsageText)
		{
			this.message = message + Resources.MessageBoxUsage;
		}
		else
		{
			this.message = message;
		}
		base.IsPopup = true;
		base.TransitionOnTime = TimeSpan.FromSeconds(0.2);
		base.TransitionOffTime = TimeSpan.FromSeconds(0.2);
	}

	public override void LoadContent()
	{
		ContentManager content = base.ScreenManager.Game.Content;
		gradientTexture = content.Load<Texture2D>("sprites\\back");
	}

	public override void HandleInput(InputState input)
	{
		if (input.IsMenuSelect(base.ControllingPlayer, out var playerIndex))
		{
			if (this.Accepted != null)
			{
				this.Accepted(this, new PlayerIndexEventArgs(playerIndex));
			}
			g.m_SoundManager.Play(8);
			ExitScreen();
		}
		else if (input.IsMenuCancel(base.ControllingPlayer, out playerIndex))
		{
			if (this.Cancelled != null)
			{
				this.Cancelled(this, new PlayerIndexEventArgs(playerIndex));
			}
			g.m_SoundManager.Play(9);
			ExitScreen();
		}
	}

	public override void Draw(GameTime gameTime)
	{
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
		SpriteFont font = base.ScreenManager.Font;
		Viewport viewport = base.ScreenManager.GraphicsDevice.Viewport;
		Vector2 vector = new Vector2(viewport.Width, viewport.Height);
		Vector2 vector2 = font.MeasureString(message);
		Vector2 position = (vector - vector2) / 2f;
		Rectangle destinationRectangle = new Rectangle((int)position.X - 32, (int)position.Y - 16, (int)vector2.X + 64, (int)vector2.Y + 32);
		Color color = Color.White * base.TransitionAlpha;
		spriteBatch.Begin();
		spriteBatch.Draw(gradientTexture, destinationRectangle, color);
		spriteBatch.DrawString(font, message, position, color);
		spriteBatch.End();
	}
}
