using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Saturn9;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (object.ReferenceEquals(resourceMan, null))
			{
				ResourceManager resourceManager = new ResourceManager("Saturn9.Resources", typeof(Resources).Assembly);
				resourceMan = resourceManager;
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static string Accept => ResourceManager.GetString("Accept", resourceCulture);

	internal static string AutoChoose => ResourceManager.GetString("AutoChoose", resourceCulture);

	internal static string Back => ResourceManager.GetString("Back", resourceCulture);

	internal static string Blood => ResourceManager.GetString("Blood", resourceCulture);

	internal static string BotsMP => ResourceManager.GetString("BotsMP", resourceCulture);

	internal static string BotsSP => ResourceManager.GetString("BotsSP", resourceCulture);

	internal static string Buy => ResourceManager.GetString("Buy", resourceCulture);

	internal static string Cancel => ResourceManager.GetString("Cancel", resourceCulture);

	internal static string ChooseCharacter => ResourceManager.GetString("ChooseCharacter", resourceCulture);

	internal static string ConfirmEndSession => ResourceManager.GetString("ConfirmEndSession", resourceCulture);

	internal static string ConfirmExitSample => ResourceManager.GetString("ConfirmExitSample", resourceCulture);

	internal static string ConfirmForceStartGame => ResourceManager.GetString("ConfirmForceStartGame", resourceCulture);

	internal static string ConfirmLeaveSession => ResourceManager.GetString("ConfirmLeaveSession", resourceCulture);

	internal static string ConfirmMarketplace => ResourceManager.GetString("ConfirmMarketplace", resourceCulture);

	internal static string ConfirmQuitGame => ResourceManager.GetString("ConfirmQuitGame", resourceCulture);

	internal static string CreateSession => ResourceManager.GetString("CreateSession", resourceCulture);

	internal static string Credits => ResourceManager.GetString("Credits", resourceCulture);

	internal static string Edgar => ResourceManager.GetString("Edgar", resourceCulture);

	internal static string EndSession => ResourceManager.GetString("EndSession", resourceCulture);

	internal static string ErrorDisconnected => ResourceManager.GetString("ErrorDisconnected", resourceCulture);

	internal static string ErrorGamerPrivilege => ResourceManager.GetString("ErrorGamerPrivilege", resourceCulture);

	internal static string ErrorHostEndedSession => ResourceManager.GetString("ErrorHostEndedSession", resourceCulture);

	internal static string ErrorNetwork => ResourceManager.GetString("ErrorNetwork", resourceCulture);

	internal static string ErrorNetworkNotAvailable => ResourceManager.GetString("ErrorNetworkNotAvailable", resourceCulture);

	internal static string ErrorRemovedByHost => ResourceManager.GetString("ErrorRemovedByHost", resourceCulture);

	internal static string ErrorSessionFull => ResourceManager.GetString("ErrorSessionFull", resourceCulture);

	internal static string ErrorSessionNotFound => ResourceManager.GetString("ErrorSessionNotFound", resourceCulture);

	internal static string ErrorSessionNotJoinable => ResourceManager.GetString("ErrorSessionNotJoinable", resourceCulture);

	internal static string ErrorTrialMode => ResourceManager.GetString("ErrorTrialMode", resourceCulture);

	internal static string ErrorUnknown => ResourceManager.GetString("ErrorUnknown", resourceCulture);

	internal static string Exit => ResourceManager.GetString("Exit", resourceCulture);

	internal static string FatherD => ResourceManager.GetString("FatherD", resourceCulture);

	internal static string FindSessions => ResourceManager.GetString("FindSessions", resourceCulture);

	internal static string Help => ResourceManager.GetString("Help", resourceCulture);

	internal static string HelpOptions => ResourceManager.GetString("HelpOptions", resourceCulture);

	internal static string Horizontal => ResourceManager.GetString("Horizontal", resourceCulture);

	internal static string HostSuffix => ResourceManager.GetString("HostSuffix", resourceCulture);

	internal static string HunterTeam => ResourceManager.GetString("HunterTeam", resourceCulture);

	internal static string InvertY => ResourceManager.GetString("InvertY", resourceCulture);

	internal static string JoinSession => ResourceManager.GetString("JoinSession", resourceCulture);

	internal static string JoinTeam => ResourceManager.GetString("JoinTeam", resourceCulture);

	internal static string LeaveSession => ResourceManager.GetString("LeaveSession", resourceCulture);

	internal static string Loading => ResourceManager.GetString("Loading", resourceCulture);

	internal static string Lobby => ResourceManager.GetString("Lobby", resourceCulture);

	internal static string MainMenu => ResourceManager.GetString("MainMenu", resourceCulture);

	internal static string MapTime => ResourceManager.GetString("MapTime", resourceCulture);

	internal static string MessageBoxUsage => ResourceManager.GetString("MessageBoxUsage", resourceCulture);

	internal static string MessageGamerJoined => ResourceManager.GetString("MessageGamerJoined", resourceCulture);

	internal static string MessageGamerLeft => ResourceManager.GetString("MessageGamerLeft", resourceCulture);

	internal static string Molly => ResourceManager.GetString("Molly", resourceCulture);

	internal static string NetworkBusy => ResourceManager.GetString("NetworkBusy", resourceCulture);

	internal static string Nina => ResourceManager.GetString("Nina", resourceCulture);

	internal static string NoSessionsFound => ResourceManager.GetString("NoSessionsFound", resourceCulture);

	internal static string Options => ResourceManager.GetString("Options", resourceCulture);

	internal static string Paused => ResourceManager.GetString("Paused", resourceCulture);

	internal static string PlayerMatch => ResourceManager.GetString("PlayerMatch", resourceCulture);

	internal static string QuitGame => ResourceManager.GetString("QuitGame", resourceCulture);

	internal static string Reset => ResourceManager.GetString("Reset", resourceCulture);

	internal static string ResumeGame => ResourceManager.GetString("ResumeGame", resourceCulture);

	internal static string ReturnToLobby => ResourceManager.GetString("ReturnToLobby", resourceCulture);

	internal static string SinglePlayer => ResourceManager.GetString("SinglePlayer", resourceCulture);

	internal static string SystemLink => ResourceManager.GetString("SystemLink", resourceCulture);

	internal static string VampireTeam => ResourceManager.GetString("VampireTeam", resourceCulture);

	internal static string Vertical => ResourceManager.GetString("Vertical", resourceCulture);

	internal static string Vibration => ResourceManager.GetString("Vibration", resourceCulture);

	internal static string Vol => ResourceManager.GetString("Vol", resourceCulture);

	internal Resources()
	{
	}
}
