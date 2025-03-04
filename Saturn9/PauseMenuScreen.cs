using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using XnaToFna.StubXDK.Net;

namespace Saturn9;

internal class PauseMenuScreen : MenuScreen
{
	private NetworkSession networkSession;

	public PauseMenuScreen(NetworkSession networkSession)
		: base(Resources.Paused)
	{
		this.networkSession = networkSession;
		if (g.m_PlayerManager.GetLocalPlayer().m_Score > g.m_App.m_SurvivalScoreBest)
		{
			g.m_App.m_SurvivalScoreBest = g.m_PlayerManager.GetLocalPlayer().m_Score;
		}
		if (Gamer.SignedInGamers[g.m_App.m_PlayerOnePadId] != null && g.m_App.m_PlayerOnePadId != (PlayerIndex)(-1))
		{
			GamePad.SetVibration(g.m_App.m_PlayerOnePadId, 0f, 0f);
		}
		if (g.m_PlayerManager.GetLocalPlayer() != null)
		{
			g.m_PlayerManager.GetLocalPlayer().ClearMovement();
		}
		MenuEntry menuEntry = new MenuEntry(Resources.ResumeGame);
		menuEntry.Selected += base.OnCancel;
		base.MenuEntries.Add(menuEntry);
		MenuEntry menuEntry2 = new MenuEntry(Resources.Options);
		menuEntry2.Selected += OnChooseOptions;
		base.MenuEntries.Add(menuEntry2);
		MenuEntry menuEntry3 = new MenuEntry("Hints");
		menuEntry3.Selected += OnChooseHint;
		base.MenuEntries.Add(menuEntry3);
		MenuEntry menuEntry4 = new MenuEntry(Resources.Help);
		menuEntry4.Selected += OnChooseHelp;
		base.MenuEntries.Add(menuEntry4);
		if (Guide.IsTrialMode)
		{
			MenuEntry menuEntry5 = new MenuEntry("UNLOCK FULL GAME");
			menuEntry5.Selected += OnChooseBuy;
			base.MenuEntries.Add(menuEntry5);
		}
		if (networkSession == null)
		{
			MenuEntry menuEntry6 = new MenuEntry(Resources.QuitGame);
			menuEntry6.Selected += QuitGameMenuEntrySelected;
			base.MenuEntries.Add(menuEntry6);
		}
	}

	private void OnChooseOptions(object sender, PlayerIndexEventArgs e)
	{
		GameScreen screen = new OptionsMenuScreen(fadeBG: true);
		base.ScreenManager.AddScreen(screen, e.PlayerIndex);
	}

	private void OnChooseHint(object sender, PlayerIndexEventArgs e)
	{
		GameScreen screen = new HintMenuScreen();
		base.ScreenManager.AddScreen(screen, e.PlayerIndex);
	}

	private void OnChooseHelp(object sender, PlayerIndexEventArgs e)
	{
		GameScreen screen = new HelpMenuScreen();
		base.ScreenManager.AddScreen(screen, e.PlayerIndex);
	}

	private void QuitGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		MessageBoxScreen messageBoxScreen = new MessageBoxScreen(Resources.ConfirmQuitGame);
		messageBoxScreen.Accepted += ConfirmQuitMessageBoxAccepted;
		base.ScreenManager.AddScreen(messageBoxScreen, base.ControllingPlayer);
	}

	private void ConfirmQuitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
	{
		LoadingScreen.Load(base.ScreenManager, false, g.m_App.m_PlayerOnePadId, 0, new BackgroundScreen(), new MainMenuScreen(1));
	}

	private void LeaveSessionMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		NetworkSessionComponent.LeaveSession(base.ScreenManager, e.PlayerIndex);
	}

	private void OnChooseBuy(object sender, PlayerIndexEventArgs e)
	{
		TryPurchase();
	}

	private void TryPurchase()
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

	public void InvitePlayer(object sender, PlayerIndexEventArgs e)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (Guide.IsTrialMode)
		{
			TryPurchase();
		}
		else
		{
			if (Guide.IsVisible)
			{
				return;
			}
			GamerCollectionEnumerator<SignedInGamer> enumerator = ((GamerCollection<SignedInGamer>)(object)Gamer.SignedInGamers).GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SignedInGamer current = enumerator.Current;
					if (current.PlayerIndex == g.m_App.m_PlayerOnePadId && (int)current.Privileges.AllowCommunication != 0)
					{
						try
						{
							Guide.ShowGameInvite(g.m_App.m_PlayerOnePadId, (IEnumerable<Gamer>)null);
						}
						catch (Exception)
						{
						}
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to .constrained prefix*/).Dispose();
			}
		}
	}

	public void InviteParty(object sender, PlayerIndexEventArgs e)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (Guide.IsTrialMode)
		{
			TryPurchase();
		}
		else
		{
			if (Guide.IsVisible)
			{
				return;
			}
			GamerCollectionEnumerator<SignedInGamer> enumerator = ((GamerCollection<SignedInGamer>)(object)Gamer.SignedInGamers).GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SignedInGamer current = enumerator.Current;
					if (current.PlayerIndex != g.m_App.m_PlayerOnePadId || (int)current.Privileges.AllowCommunication == 0)
					{
						continue;
					}
					try
					{
						if (current.PartySize <= 0)
						{
							continue;
						}
						GamerCollectionEnumerator<LocalNetworkGamer> enumerator2 = g.m_App.m_NetworkSession.LocalGamers.GetEnumerator();
						try
						{
							while (enumerator2.MoveNext())
							{
								LocalNetworkGamer current2 = enumerator2.Current;
								__LocalNetworkGamer__.SendPartyInvites(current2);
							}
						}
						finally
						{
							((IDisposable)enumerator2/*cast due to .constrained prefix*/).Dispose();
						}
					}
					catch (Exception)
					{
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to .constrained prefix*/).Dispose();
			}
		}
	}
}
