using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Saturn9;

internal class MenuEntry
{
	private string text;

	private float selectionFade;

	private Vector2 position;

	private float displayProgress;

	private Vector2 r = Vector2.Zero;

	private float ralpha = 0.2f;

	private float rscale = 2f;

	public string Text
	{
		get
		{
			return text;
		}
		set
		{
			text = value;
		}
	}

	public Vector2 Position
	{
		get
		{
			return position;
		}
		set
		{
			position = value;
		}
	}

	public event EventHandler<PlayerIndexEventArgs> Selected;

	protected internal virtual void OnSelectEntry(PlayerIndex playerIndex)
	{
		if (this.Selected != null)
		{
			g.m_SoundManager.Play(8);
			this.Selected(this, new PlayerIndexEventArgs(playerIndex));
		}
	}

	public MenuEntry(string text)
	{
		this.text = text;
		displayProgress = 0f;
	}

	public virtual void Update(MenuScreen screen, bool isSelected, GameTime gameTime)
	{
		float num = (float)gameTime.ElapsedGameTime.TotalSeconds * 4f;
		if (isSelected)
		{
			selectionFade = Math.Min(selectionFade + num, 1f);
		}
		else
		{
			selectionFade = Math.Max(selectionFade - num, 0f);
		}
	}

	public virtual void Draw(MenuScreen screen, bool isSelected, GameTime gameTime)
	{
		Color color = (isSelected ? (new Color(197, 194, 190) * 0.75f) : (new Color(143, 130, 109) * 0.75f));
		_ = gameTime.TotalGameTime.TotalSeconds;
		if (displayProgress < 1f)
		{
			displayProgress += (float)gameTime.ElapsedGameTime.TotalSeconds * 2f;
		}
		int length = text.Length;
		int num = (int)((float)length * displayProgress);
		if (num > length)
		{
			num = length;
		}
		float num2 = 1f;
		float num3 = 1f + num2 * 0.05f * selectionFade;
		color *= screen.TransitionAlpha;
		ScreenManager screenManager = screen.ScreenManager;
		SpriteBatch spriteBatch = screenManager.SpriteBatch;
		SpriteFont font = screenManager.Font;
		Vector2 vector = font.MeasureString(text);
		Vector2 origin = new Vector2(vector.X * 0.5f, font.LineSpacing / 2);
		spriteBatch.DrawString(font, text.Substring(0, num), position + new Vector2(vector.X * 0.5f, 0f), color, 0f, origin, num3, SpriteEffects.None, 0f);
		if (isSelected)
		{
			if ((double)(float)g.m_App.m_Rand.NextDouble() < 0.9)
			{
				spriteBatch.DrawString(font, text.Substring(0, num), position + new Vector2(vector.X * 0.5f, 0f) + r, color * ralpha, 0f, origin, num3 * rscale, SpriteEffects.None, 0f);
			}
			else
			{
				rscale = 2f + (float)g.m_App.m_Rand.NextDouble() * 0.5f;
			}
		}
	}

	public virtual int GetHeight(MenuScreen screen)
	{
		return screen.ScreenManager.Font.LineSpacing;
	}

	public virtual int GetWidth(MenuScreen screen)
	{
		return (int)screen.ScreenManager.Font.MeasureString(Text).X;
	}
}
