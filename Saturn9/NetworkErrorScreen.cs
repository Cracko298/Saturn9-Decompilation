using System;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net;

namespace Saturn9;

internal class NetworkErrorScreen : MessageBoxScreen
{
	public NetworkErrorScreen(Exception exception)
		: base(GetErrorMessage(exception), includeUsageText: false)
	{
	}

	private static string GetErrorMessage(Exception exception)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected I4, but got Unknown
		if (exception is GamerPrivilegeException)
		{
			if (Guide.IsTrialMode)
			{
				return Resources.ErrorTrialMode;
			}
			return Resources.ErrorGamerPrivilege;
		}
		NetworkSessionJoinException val = (NetworkSessionJoinException)(object)((exception is NetworkSessionJoinException) ? exception : null);
		if (val != null)
		{
			NetworkSessionJoinError joinError = val.JoinError;
			switch ((int)joinError)
			{
			case 2:
				return Resources.ErrorSessionFull;
			case 0:
				return Resources.ErrorSessionNotFound;
			case 1:
				return Resources.ErrorSessionNotJoinable;
			}
		}
		if (exception is NetworkNotAvailableException)
		{
			return Resources.ErrorNetworkNotAvailable;
		}
		if (exception is NetworkException)
		{
			return Resources.ErrorNetwork;
		}
		return Resources.ErrorUnknown;
	}
}
