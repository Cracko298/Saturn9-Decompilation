using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;

namespace Saturn9;

internal class AvailableSessionMenuEntry : MenuEntry
{
	private AvailableNetworkSession availableSession;

	private bool gotQualityOfService;

	public AvailableNetworkSession AvailableSession => availableSession;

	public AvailableSessionMenuEntry(AvailableNetworkSession availableSession)
		: base(GetMenuItemText(availableSession))
	{
		this.availableSession = availableSession;
	}

	private static string GetMenuItemText(AvailableNetworkSession session)
	{
		int num = session.CurrentGamerCount + session.OpenPublicGamerSlots;
		return $"{session.HostGamertag} ({session.CurrentGamerCount}/{num})";
	}

	public override void Update(MenuScreen screen, bool isSelected, GameTime gameTime)
	{
		base.Update(screen, isSelected, gameTime);
		if (screen.IsActive && !gotQualityOfService)
		{
			QualityOfService qualityOfService = availableSession.QualityOfService;
			if (qualityOfService.IsAvailable)
			{
				TimeSpan averageRoundtripTime = qualityOfService.AverageRoundtripTime;
				base.Text += $" - {averageRoundtripTime.TotalMilliseconds:0} ms";
				gotQualityOfService = true;
			}
		}
	}
}
