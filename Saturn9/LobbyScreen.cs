using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;

namespace Saturn9;

internal class LobbyScreen : GameScreen
{
	private NetworkSession networkSession;

	public LobbyScreen(NetworkSession networkSession)
	{
		this.networkSession = networkSession;
		base.TransitionOnTime = TimeSpan.FromSeconds(0.0);
		base.TransitionOffTime = TimeSpan.FromSeconds(0.0);
	}

	public override void LoadContent()
	{
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		if (!base.IsExiting)
		{
			if ((int)networkSession.SessionState == 1)
			{
				LoadingScreen.Load(base.ScreenManager, true, null, 1, new GameplayScreen(networkSession));
			}
			if (networkSession.IsHost && (int)networkSession.SessionState == 0)
			{
				networkSession.StartGame();
			}
		}
	}

	public override void HandleInput(InputState input)
	{
	}

	private void HandleMenuSelect(LocalNetworkGamer gamer)
	{
		if (!((NetworkGamer)gamer).IsReady)
		{
			((NetworkGamer)gamer).IsReady = true;
		}
		else if (((NetworkGamer)gamer).IsHost)
		{
			MessageBoxScreen messageBoxScreen = new MessageBoxScreen(Resources.ConfirmForceStartGame);
			messageBoxScreen.Accepted += ConfirmStartGameMessageBoxAccepted;
			base.ScreenManager.AddScreen(messageBoxScreen, gamer.SignedInGamer.PlayerIndex);
		}
	}

	private void ConfirmStartGameMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if ((int)networkSession.SessionState == 0)
		{
			networkSession.StartGame();
		}
	}

	private void HandleMenuCancel(LocalNetworkGamer gamer)
	{
		if (((NetworkGamer)gamer).IsReady)
		{
			((NetworkGamer)gamer).IsReady = false;
			return;
		}
		PlayerIndex playerIndex = gamer.SignedInGamer.PlayerIndex;
		NetworkSessionComponent.LeaveSession(base.ScreenManager, playerIndex);
	}

	public override void Draw(GameTime gameTime)
	{
		base.ScreenManager.GraphicsDevice.Clear(Color.Black);
	}
}
