using Microsoft.Xna.Framework;

namespace Saturn9;

internal class HelpAndOptionsMenuScreen : MenuScreen
{
	public HelpAndOptionsMenuScreen()
		: base("HELP AND OPTIONS")
	{
		MenuEntry menuEntry = new MenuEntry(Resources.Options);
		menuEntry.Selected += OnChooseOptions;
		base.MenuEntries.Add(menuEntry);
		MenuEntry menuEntry2 = new MenuEntry("Hints");
		menuEntry2.Selected += OnChooseHint;
		base.MenuEntries.Add(menuEntry2);
		MenuEntry menuEntry3 = new MenuEntry(Resources.Help);
		menuEntry3.Selected += OnChooseHelp;
		base.MenuEntries.Add(menuEntry3);
		MenuEntry menuEntry4 = new MenuEntry(Resources.Credits);
		menuEntry4.Selected += CreditsMenuEntrySelected;
		base.MenuEntries.Add(menuEntry4);
		MenuEntry menuEntry5 = new MenuEntry(Resources.Back);
		menuEntry5.Selected += OnChooseBack;
		base.MenuEntries.Add(menuEntry5);
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

	private void OnChooseOptions(object sender, PlayerIndexEventArgs e)
	{
		GameScreen screen = new OptionsMenuScreen(fadeBG: false);
		base.ScreenManager.AddScreen(screen, e.PlayerIndex);
	}

	private void CreditsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		GameScreen screen = new CreditsMenuScreen(gameComplete: false);
		base.ScreenManager.AddScreen(screen, e.PlayerIndex);
	}

	private void OnChooseBack(object sender, PlayerIndexEventArgs e)
	{
		ExitScreen();
	}

	public override void LoadContent()
	{
		_ = base.ScreenManager.Game.Content;
	}

	public override void Draw(GameTime gameTime)
	{
		_ = base.ScreenManager.SpriteBatch;
		_ = base.ScreenManager.Font;
		base.Draw(gameTime);
	}
}
