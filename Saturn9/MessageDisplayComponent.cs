using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Saturn9;

internal class MessageDisplayComponent : DrawableGameComponent, IMessageDisplay, IDrawable, IUpdateable
{
	private class NotificationMessage
	{
		public string Text;

		public float Position;

		public TimeSpan Age;

		public NotificationMessage(string text, float position)
		{
			Text = text;
			Position = position;
			Age = TimeSpan.Zero;
		}
	}

	private SpriteBatch spriteBatch;

	private SpriteFont font;

	private List<NotificationMessage> messages = new List<NotificationMessage>();

	private object syncObject = new object();

	private static readonly TimeSpan fadeInTime = TimeSpan.FromSeconds(0.25);

	private static readonly TimeSpan showTime = TimeSpan.FromSeconds(5.0);

	private static readonly TimeSpan fadeOutTime = TimeSpan.FromSeconds(0.5);

	public MessageDisplayComponent(Game game)
		: base(game)
	{
		game.Services.AddService(typeof(IMessageDisplay), this);
	}

	protected override void LoadContent()
	{
		spriteBatch = new SpriteBatch(base.GraphicsDevice);
		font = base.Game.Content.Load<SpriteFont>("fonts\\orbitron26");
	}

	public override void Update(GameTime gameTime)
	{
		lock (syncObject)
		{
			int num = 0;
			float num2 = 0f;
			while (num < messages.Count)
			{
				NotificationMessage notificationMessage = messages[num];
				float num3 = num2 - notificationMessage.Position;
				float val = (float)gameTime.ElapsedGameTime.TotalSeconds * 2f;
				notificationMessage.Position += num3 * Math.Min(val, 1f);
				notificationMessage.Age += gameTime.ElapsedGameTime;
				if (notificationMessage.Age < showTime + fadeOutTime)
				{
					num++;
					if (notificationMessage.Age < showTime)
					{
						num2 += 1f;
					}
				}
				else
				{
					messages.RemoveAt(num);
				}
			}
		}
	}

	public override void Draw(GameTime gameTime)
	{
		lock (syncObject)
		{
			if (messages.Count == 0)
			{
				return;
			}
			Vector2 vector = new Vector2(base.GraphicsDevice.Viewport.Width - 100, 0f);
			spriteBatch.Begin();
			foreach (NotificationMessage message in messages)
			{
				float num = 1f;
				if (message.Age < fadeInTime)
				{
					double totalSeconds = message.Age.TotalSeconds;
					TimeSpan timeSpan = fadeInTime;
					num = (float)(totalSeconds / timeSpan.TotalSeconds);
				}
				else if (message.Age > showTime)
				{
					double totalSeconds2 = (showTime + fadeOutTime - message.Age).TotalSeconds;
					TimeSpan timeSpan2 = fadeOutTime;
					num = (float)(totalSeconds2 / timeSpan2.TotalSeconds);
				}
				vector.Y = 80f + message.Position * (float)font.LineSpacing * 0.75f;
				Vector2 origin = font.MeasureString(message.Text);
				origin.Y = 0f;
				spriteBatch.DrawString(font, message.Text, vector + Vector2.One, Color.Black * num, 0f, origin, 0.75f, SpriteEffects.None, 0f);
				spriteBatch.DrawString(font, message.Text, vector, Color.White * num, 0f, origin, 0.75f, SpriteEffects.None, 0f);
			}
			spriteBatch.End();
		}
	}

	public void ShowMessage(string message, params object[] parameters)
	{
		string text = string.Format(message, parameters);
		lock (syncObject)
		{
			float position = messages.Count;
			messages.Add(new NotificationMessage(text, position));
		}
	}
}
