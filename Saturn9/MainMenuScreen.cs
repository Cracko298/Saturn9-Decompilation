using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace Saturn9;

internal class MainMenuScreen : MenuScreen
{
	private MenuEntry buyMenuEntry;

	private MenuEntry continueMenuEntry;

	private int m_PrevScreen = -1;

	private Texture2D m_Artifact;

	private Texture2D m_Star;

	public MainMenuScreen(int prevScreen)
		: base(Resources.MainMenu)
	{
		m_PrevScreen = prevScreen;
		MenuEntry menuEntry = new MenuEntry("New Game");
		if (g.m_App.m_bSaveExists && g.m_App.m_CheckpointId != -1)
		{
			continueMenuEntry = new MenuEntry("Continue Game");
			continueMenuEntry.Selected += ContinueMenuEntrySelected;
		}
		MenuEntry menuEntry2 = new MenuEntry(Resources.HelpOptions);
		MenuEntry menuEntry3 = new MenuEntry(Resources.Exit);
		if (g.m_App.IsTrialMode())
		{
			buyMenuEntry = new MenuEntry(Resources.Buy);
			buyMenuEntry.Selected += BuyMenuEntrySelected;
		}
		menuEntry.Selected += SinglePlayerMenuEntrySelected;
		menuEntry3.Selected += base.OnCancel;
		menuEntry2.Selected += HelpAndOptionsMenuEntrySelected;
		base.MenuEntries.Add(menuEntry);
		if (g.m_App.m_bSaveExists && g.m_App.m_CheckpointId != -1)
		{
			base.MenuEntries.Add(continueMenuEntry);
		}
		base.MenuEntries.Add(menuEntry2);
		if (g.m_App.IsTrialMode())
		{
			base.MenuEntries.Add(buyMenuEntry);
		}
		base.MenuEntries.Add(menuEntry3);
		if (Gamer.SignedInGamers[g.m_App.m_PlayerOnePadId] != null)
		{
			Gamer.SignedInGamers[g.m_App.m_PlayerOnePadId].Presence.PresenceMode = (GamerPresenceMode)46;
		}
		ResetGame();
	}

	private void ResetGame()
	{
		g.m_CameraManager.Init();
		g.m_ItemManager.DeleteAll();
		g.m_PlayerManager.DeleteAll();
		g.m_CameraManager.m_Helmet = -1;
		g.m_App.Reset();
		GC.Collect();
	}

	private void SinglePlayerMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		if (g.m_App.m_bSaveExists && g.m_App.m_CheckpointId != -1)
		{
			MessageBoxScreen messageBoxScreen = new MessageBoxScreen("Overwrite Existing Game?");
			messageBoxScreen.Accepted += ConfirmOverwriteMessageBoxAccepted;
			base.ScreenManager.AddScreen(messageBoxScreen, base.ControllingPlayer);
		}
		else
		{
			StartSinglePlayerGame();
		}
	}

	private void StartSinglePlayerGame()
	{
		g.m_App.m_SurvivalMode = false;
		ResetGame();
		g.m_App.m_CheckpointId = -1;
		g.m_App.m_bTutorialDone = false;
		g.m_PlayerManager.Create(255, bot: false, Player.TEAM.None);
		LoadingScreen.Load(base.ScreenManager, true, g.m_App.m_PlayerOnePadId, 1, new GameplayScreen(null));
	}

	private void ConfirmOverwriteMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
	{
		StartSinglePlayerGame();
	}

	private void ContinueMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		g.m_App.m_SurvivalMode = false;
		ResetGame();
		g.m_App.m_bTutorialDone = true;
		g.m_PlayerManager.Create(255, bot: false, Player.TEAM.None);
		LoadingScreen.Load(base.ScreenManager, true, e.PlayerIndex, 1, new GameplayScreen(null));
	}

	protected override void OnCancel(PlayerIndex playerIndex)
	{
		MessageBoxScreen messageBoxScreen = new MessageBoxScreen(Resources.ConfirmExitSample);
		messageBoxScreen.Accepted += ConfirmExitMessageBoxAccepted;
		base.ScreenManager.AddScreen(messageBoxScreen, playerIndex);
	}

	private void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
	{
		base.ScreenManager.Game.Exit();
	}

	private void HelpAndOptionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		GameScreen screen = new HelpAndOptionsMenuScreen();
		base.ScreenManager.AddScreen(screen, e.PlayerIndex);
	}

	private void BuyMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		if (!Guide.IsVisible)
		{
			if (g.m_App.CanPurchaseContent(g.m_App.m_PlayerOnePadId))
			{
				Guide.ShowMarketplace(g.m_App.m_PlayerOnePadId);
			}
			else if (Gamer.SignedInGamers[g.m_App.m_PlayerOnePadId] != null)
			{
				g.m_App.m_ShowPermissionTime = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 3f;
			}
			else
			{
				Guide.ShowSignIn(1, true);
			}
		}
	}

	public override void LoadContent()
	{
		_ = base.ScreenManager.Game.Content;
		if (m_PrevScreen == 1 && g.m_App.SOUNDON)
		{
			MediaPlayer.Play(g.m_App.m_MenuMusic);
			MediaPlayer.IsRepeating = true;
			MediaPlayer.Volume = g.m_App.m_OptionsMusicVol;
		}
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		if (!g.m_App.IsTrialMode() && buyMenuEntry != null)
		{
			base.MenuEntries.Remove(buyMenuEntry);
		}
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
	}

	public override void Draw(GameTime gameTime)
	{
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
		SpriteFont font = base.ScreenManager.Font;
		if ((double)g.m_App.m_ShowPermissionTime > gameTime.TotalGameTime.TotalSeconds)
		{
			spriteBatch.Begin();
			spriteBatch.DrawString(font, "No permissions to buy with this Profile", new Vector2(220f, 430f), Color.Red);
			spriteBatch.End();
		}
		base.Draw(gameTime);
	}
}
