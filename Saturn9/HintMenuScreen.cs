using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;

namespace Saturn9;

internal class HintMenuScreen : MenuScreen
{
	private Texture2D gradientTexture;

	private bool[] m_bShowHint = new bool[12];

	private string[] m_Hint = new string[12];

	private float Y_START = 130f;

	public HintMenuScreen()
		: base("HINTS")
	{
		if (g.m_PlayerManager.GetLocalPlayer() != null)
		{
			MenuEntry menuEntry = new MenuEntry("SOS Password #1");
			menuEntry.Selected += ShowHint1;
			base.MenuEntries.Add(menuEntry);
			MenuEntry menuEntry2 = new MenuEntry("SOS Password #2");
			menuEntry2.Selected += ShowHint2;
			base.MenuEntries.Add(menuEntry2);
			MenuEntry menuEntry3 = new MenuEntry("SOS Password #3");
			menuEntry3.Selected += ShowHint3;
			base.MenuEntries.Add(menuEntry3);
			if (!Guide.IsTrialMode && g.m_PlayerManager.GetLocalPlayer().m_Door0Unlocked)
			{
				MenuEntry menuEntry4 = new MenuEntry("Forgotten Password #1");
				menuEntry4.Selected += ShowHint4;
				base.MenuEntries.Add(menuEntry4);
				MenuEntry menuEntry5 = new MenuEntry("Forgotten Password #2");
				menuEntry5.Selected += ShowHint5;
				base.MenuEntries.Add(menuEntry5);
				MenuEntry menuEntry6 = new MenuEntry("Forgotten Password #3");
				menuEntry6.Selected += ShowHint6;
				base.MenuEntries.Add(menuEntry6);
				if (g.m_PlayerManager.GetLocalPlayer().m_Door1Unlocked)
				{
					MenuEntry menuEntry7 = new MenuEntry("Sharps PIN number #1");
					menuEntry7.Selected += ShowHint7;
					base.MenuEntries.Add(menuEntry7);
					MenuEntry menuEntry8 = new MenuEntry("Sharps PIN number #2");
					menuEntry8.Selected += ShowHint8;
					base.MenuEntries.Add(menuEntry8);
					MenuEntry menuEntry9 = new MenuEntry("Sharps PIN number #3");
					menuEntry9.Selected += ShowHint9;
					base.MenuEntries.Add(menuEntry9);
					if (g.m_PlayerManager.GetLocalPlayer().m_DoorMedbayUnlocked)
					{
						MenuEntry menuEntry10 = new MenuEntry("Network Access #1");
						menuEntry10.Selected += ShowHint10;
						base.MenuEntries.Add(menuEntry10);
						MenuEntry menuEntry11 = new MenuEntry("Network Access #2");
						menuEntry11.Selected += ShowHint11;
						base.MenuEntries.Add(menuEntry11);
						MenuEntry menuEntry12 = new MenuEntry("Network Access #3");
						menuEntry12.Selected += ShowHint12;
						base.MenuEntries.Add(menuEntry12);
					}
				}
			}
		}
		MenuEntry menuEntry13 = new MenuEntry("Back");
		menuEntry13.Selected += OnChooseBack;
		base.MenuEntries.Add(menuEntry13);
		m_Hint[0] = "Find the password left on a desk";
		m_Hint[1] = "A yellow note might have the password on it";
		m_Hint[2] = "The password is YXXY";
		m_Hint[3] = "There might be some food left on a desk";
		m_Hint[4] = "The food may be in the first area";
		m_Hint[5] = "The answer is NOODLES";
		m_Hint[6] = "The pin number may be in the Medical Notes menu";
		m_Hint[7] = "Read 'Symbiosis' for the pin number";
		m_Hint[8] = "The pin number is 1877";
		m_Hint[9] = "The network password is Marilyn";
		m_Hint[10] = "Study the sequences of the last two numbers for each door";
		m_Hint[11] = "The answer is 75.9";
	}

	private void ShowHint1(object sender, PlayerIndexEventArgs e)
	{
		m_bShowHint[0] = !m_bShowHint[0];
	}

	private void ShowHint2(object sender, PlayerIndexEventArgs e)
	{
		m_bShowHint[1] = !m_bShowHint[1];
	}

	private void ShowHint3(object sender, PlayerIndexEventArgs e)
	{
		m_bShowHint[2] = !m_bShowHint[2];
	}

	private void ShowHint4(object sender, PlayerIndexEventArgs e)
	{
		m_bShowHint[3] = !m_bShowHint[3];
	}

	private void ShowHint5(object sender, PlayerIndexEventArgs e)
	{
		m_bShowHint[4] = !m_bShowHint[4];
	}

	private void ShowHint6(object sender, PlayerIndexEventArgs e)
	{
		m_bShowHint[5] = !m_bShowHint[5];
	}

	private void ShowHint7(object sender, PlayerIndexEventArgs e)
	{
		m_bShowHint[6] = !m_bShowHint[6];
	}

	private void ShowHint8(object sender, PlayerIndexEventArgs e)
	{
		m_bShowHint[7] = !m_bShowHint[7];
	}

	private void ShowHint9(object sender, PlayerIndexEventArgs e)
	{
		m_bShowHint[8] = !m_bShowHint[8];
	}

	private void ShowHint10(object sender, PlayerIndexEventArgs e)
	{
		m_bShowHint[9] = !m_bShowHint[9];
	}

	private void ShowHint11(object sender, PlayerIndexEventArgs e)
	{
		m_bShowHint[10] = !m_bShowHint[10];
	}

	private void ShowHint12(object sender, PlayerIndexEventArgs e)
	{
		m_bShowHint[11] = !m_bShowHint[11];
	}

	private void OnChooseBack(object sender, PlayerIndexEventArgs e)
	{
		ExitScreen();
	}

	public override void LoadContent()
	{
		ContentManager content = base.ScreenManager.Game.Content;
		gradientTexture = content.Load<Texture2D>("sprites\\back");
	}

	protected override void UpdateMenuEntryLocations()
	{
		Vector2 position = new Vector2(0f, Y_START);
		for (int i = 0; i < base.MenuEntries.Count; i++)
		{
			MenuEntry menuEntry = base.MenuEntries[i];
			position.X = 90f;
			menuEntry.Position = position;
			position.Y += menuEntry.GetHeight(this) + 10;
		}
	}

	public override void Draw(GameTime gameTime)
	{
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
		_ = base.ScreenManager.Font;
		spriteBatch.Begin();
		if (g.m_PlayerManager.GetLocalPlayer() == null)
		{
			spriteBatch.DrawString(g.m_App.lcdFont, "Hints are available from the Pause Menu during gameplay", new Vector2(350f, 100f), g.HIGHLIGHT_COL * base.TransitionAlpha);
		}
		else
		{
			spriteBatch.DrawString(g.m_App.lcdFont, "We recommend you only use these hints when you are really stuck! #3 will spoil the answers!", new Vector2(200f, 100f), g.HIGHLIGHT_COL * base.TransitionAlpha);
			Vector2 vector = new Vector2(640f, Y_START);
			for (int i = 0; i < 12; i++)
			{
				if (m_bShowHint[i])
				{
					spriteBatch.DrawString(g.m_App.lcdFont, m_Hint[i], vector + new Vector2(0f, i * 45), g.HIGHLIGHT_COL * base.TransitionAlpha);
				}
			}
		}
		spriteBatch.End();
		base.Draw(gameTime);
	}
}
