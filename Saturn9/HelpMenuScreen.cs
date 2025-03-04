using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Saturn9;

internal class HelpMenuScreen : MenuScreen
{
	private Texture2D gradientTexture;

	private Texture2D helpTexture;

	public HelpMenuScreen()
		: base("")
	{
		MenuEntry menuEntry = new MenuEntry("");
		menuEntry.Selected += OnChooseBack;
		base.MenuEntries.Add(menuEntry);
	}

	private void OnChooseBack(object sender, PlayerIndexEventArgs e)
	{
		ExitScreen();
	}

	public override void LoadContent()
	{
		ContentManager content = base.ScreenManager.Game.Content;
		gradientTexture = content.Load<Texture2D>("sprites\\back");
		helpTexture = content.Load<Texture2D>("Backgrounds\\help");
	}

	public override void Draw(GameTime gameTime)
	{
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
		_ = base.ScreenManager.Font;
		spriteBatch.Begin();
		new Vector2(90f, 220f);
		spriteBatch.Draw(helpTexture, new Rectangle(0, 0, 1280, 720), Color.White * base.TransitionAlpha);
		spriteBatch.End();
		base.Draw(gameTime);
	}
}
