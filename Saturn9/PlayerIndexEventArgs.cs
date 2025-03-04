using System;
using Microsoft.Xna.Framework;

namespace Saturn9;

internal class PlayerIndexEventArgs : EventArgs
{
	private PlayerIndex playerIndex;

	public PlayerIndex PlayerIndex => playerIndex;

	public PlayerIndexEventArgs(PlayerIndex playerIndex)
	{
		this.playerIndex = playerIndex;
	}
}
