using System;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Net;

namespace Saturn9;

internal class JoinSessionScreen : MenuScreen
{
	private const int MaxSearchResults = 8;

	private AvailableNetworkSessionCollection availableSessions;

	public JoinSessionScreen(AvailableNetworkSessionCollection availableSessions)
		: base(Resources.JoinSession)
	{
		this.availableSessions = availableSessions;
		foreach (AvailableNetworkSession item in (ReadOnlyCollection<AvailableNetworkSession>)(object)availableSessions)
		{
			MenuEntry menuEntry = new AvailableSessionMenuEntry(item);
			menuEntry.Selected += AvailableSessionMenuEntrySelected;
			base.MenuEntries.Add(menuEntry);
			if (base.MenuEntries.Count >= 8)
			{
				break;
			}
		}
		MenuEntry menuEntry2 = new MenuEntry(Resources.Back);
		menuEntry2.Selected += BackMenuEntrySelected;
		base.MenuEntries.Add(menuEntry2);
	}

	private void AvailableSessionMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		AvailableSessionMenuEntry availableSessionMenuEntry = (AvailableSessionMenuEntry)sender;
		AvailableNetworkSession availableSession = availableSessionMenuEntry.AvailableSession;
		try
		{
			IAsyncResult asyncResult = NetworkSession.BeginJoin(availableSession, (AsyncCallback)null, (object)null);
			NetworkBusyScreen networkBusyScreen = new NetworkBusyScreen(asyncResult);
			networkBusyScreen.OperationCompleted += JoinSessionOperationCompleted;
			base.ScreenManager.AddScreen(networkBusyScreen, base.ControllingPlayer);
		}
		catch (Exception exception)
		{
			NetworkErrorScreen screen = new NetworkErrorScreen(exception);
			base.ScreenManager.AddScreen(screen, base.ControllingPlayer);
		}
	}

	private void JoinSessionOperationCompleted(object sender, OperationCompletedEventArgs e)
	{
		try
		{
			NetworkSession networkSession = NetworkSession.EndJoin(e.AsyncResult);
			NetworkSessionComponent.Create(base.ScreenManager, networkSession);
			base.ScreenManager.AddScreen(new LobbyScreen(networkSession), null);
			availableSessions.Dispose();
		}
		catch (Exception exception)
		{
			NetworkErrorScreen screen = new NetworkErrorScreen(exception);
			base.ScreenManager.AddScreen(screen, base.ControllingPlayer);
		}
	}

	private void BackMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		availableSessions.Dispose();
		ExitScreen();
	}
}
