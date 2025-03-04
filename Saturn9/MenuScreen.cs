using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Saturn9;

internal abstract class MenuScreen : GameScreen
{
	private List<MenuEntry> menuEntries = new List<MenuEntry>();

	public int selectedEntry;

	private string menuTitle;

	protected IList<MenuEntry> MenuEntries => menuEntries;

	public MenuScreen(string menuTitle)
	{
		this.menuTitle = menuTitle;
		base.TransitionOnTime = TimeSpan.FromSeconds(0.5);
		base.TransitionOffTime = TimeSpan.FromSeconds(0.5);
	}

	public override void HandleInput(InputState input)
	{
		if (input.IsMenuUp(base.ControllingPlayer))
		{
			selectedEntry--;
			if (selectedEntry < 0)
			{
				selectedEntry = menuEntries.Count - 1;
			}
			if (menuEntries.Count > 1)
			{
				g.m_SoundManager.Play(11);
			}
		}
		if (input.IsMenuDown(base.ControllingPlayer))
		{
			selectedEntry++;
			if (selectedEntry >= menuEntries.Count)
			{
				selectedEntry = 0;
			}
			if (menuEntries.Count > 1)
			{
				g.m_SoundManager.Play(10);
			}
		}
		if (input.IsMenuSelect(base.ControllingPlayer, out var playerIndex))
		{
			OnSelectEntry(selectedEntry, playerIndex);
		}
		else if (input.IsMenuCancel(base.ControllingPlayer, out playerIndex))
		{
			OnCancel(playerIndex);
			g.m_SoundManager.Play(9);
		}
	}

	protected virtual void OnSelectEntry(int entryIndex, PlayerIndex playerIndex)
	{
		menuEntries[entryIndex].OnSelectEntry(playerIndex);
	}

	protected virtual void OnCancel(PlayerIndex playerIndex)
	{
		ExitScreen();
	}

	protected void OnCancel(object sender, PlayerIndexEventArgs e)
	{
		OnCancel(e.PlayerIndex);
	}

	protected virtual void UpdateMenuEntryLocations()
	{
		Vector2 position = new Vector2(0f, 250f);
		for (int i = 0; i < menuEntries.Count; i++)
		{
			MenuEntry menuEntry = menuEntries[i];
			position.X = base.ScreenManager.GraphicsDevice.Viewport.Width / 2 - menuEntry.GetWidth(this) / 2;
			menuEntry.Position = position;
			position.Y += menuEntry.GetHeight(this) + 10;
		}
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		for (int i = 0; i < menuEntries.Count; i++)
		{
			bool isSelected = base.IsActive && i == selectedEntry;
			menuEntries[i].Update(this, isSelected, gameTime);
		}
	}

	public override void Draw(GameTime gameTime)
	{
		UpdateMenuEntryLocations();
		GraphicsDevice graphicsDevice = base.ScreenManager.GraphicsDevice;
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
		SpriteFont font = base.ScreenManager.Font;
		spriteBatch.Begin();
		for (int i = 0; i < menuEntries.Count; i++)
		{
			MenuEntry menuEntry = menuEntries[i];
			bool isSelected = base.IsActive && i == selectedEntry;
			menuEntry.Draw(this, isSelected, gameTime);
		}
		Vector2 position = new Vector2(graphicsDevice.Viewport.Width / 2, 80f);
		Vector2 origin = font.MeasureString(menuTitle) / 2f;
		Color color = g.TITLE_COL * base.TransitionAlpha * 0.75f;
		float scale = 1.25f;
		spriteBatch.DrawString(font, menuTitle, position, color, 0f, origin, scale, SpriteEffects.None, 0f);
		spriteBatch.End();
	}
}
