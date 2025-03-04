using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Saturn9;

internal class NetworkBusyScreen : GameScreen
{
	private IAsyncResult asyncResult;

	private Texture2D gradientTexture;

	private Texture2D catTexture;

	private float m_RotateTime;

	private float m_Rotation;

	public event EventHandler<OperationCompletedEventArgs> OperationCompleted;

	public NetworkBusyScreen(IAsyncResult asyncResult)
	{
		this.asyncResult = asyncResult;
		base.IsPopup = true;
		base.TransitionOnTime = TimeSpan.FromSeconds(0.1);
		base.TransitionOffTime = TimeSpan.FromSeconds(0.2);
	}

	public override void LoadContent()
	{
		ContentManager content = base.ScreenManager.Game.Content;
		gradientTexture = content.Load<Texture2D>("sprites\\back");
		catTexture = content.Load<Texture2D>("sprites\\busyicon");
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		if (asyncResult != null && asyncResult.IsCompleted)
		{
			if (this.OperationCompleted != null)
			{
				this.OperationCompleted(this, new OperationCompletedEventArgs(asyncResult));
			}
			ExitScreen();
			asyncResult = null;
		}
	}

	public override void Draw(GameTime gameTime)
	{
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
		SpriteFont font = base.ScreenManager.Font;
		string networkBusy = Resources.NetworkBusy;
		Viewport viewport = base.ScreenManager.GraphicsDevice.Viewport;
		Vector2 vector = new Vector2(viewport.Width, viewport.Height);
		Vector2 vector2 = font.MeasureString(networkBusy);
		Vector2 vector3 = new Vector2(catTexture.Width);
		vector2.X = Math.Max(vector2.X, vector3.X);
		vector2.Y += vector3.Y + 16f;
		Vector2 position = (vector - vector2) / 2f;
		Rectangle destinationRectangle = new Rectangle((int)position.X - 32, (int)position.Y - 16, (int)vector2.X + 64, (int)vector2.Y + 32);
		Color color = Color.White * base.TransitionAlpha;
		spriteBatch.Begin();
		spriteBatch.Draw(gradientTexture, destinationRectangle, color);
		spriteBatch.DrawString(font, networkBusy, position, color);
		if (m_RotateTime < (float)gameTime.TotalGameTime.TotalSeconds)
		{
			m_Rotation += MathHelper.ToRadians(30f);
			m_RotateTime = (float)gameTime.TotalGameTime.TotalSeconds + 0.1f;
		}
		spriteBatch.Draw(position: new Vector2(position.X + vector2.X / 2f, position.Y + vector2.Y - vector3.Y / 2f), texture: catTexture, sourceRectangle: null, color: color, rotation: m_Rotation, origin: vector3 / 2f, scale: 1f, effects: SpriteEffects.None, layerDepth: 0f);
		spriteBatch.End();
	}
}
