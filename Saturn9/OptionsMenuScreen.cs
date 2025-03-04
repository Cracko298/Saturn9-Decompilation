using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Saturn9;

internal class OptionsMenuScreen : MenuScreen
{
	private enum DESC
	{
		HORIZ,
		VERT,
		INVERT,
		VIBRATION,
		SFXVOL,
		MUSICVOL,
		BRIGHTNESS,
		RESET,
		ACCEPT
	}

	private const int NUM_MENU_ITEMS = 13;

	private Texture2D gradientTexture;

	private Desc[] m_Desc;

	private float m_DelayTime;

	private bool m_FadeBG;

	private void SetDescValues()
	{
		m_Desc[0].m_Text = Resources.Horizontal;
		m_Desc[0].m_UseYesNo = false;
		m_Desc[0].m_Min = 1;
		m_Desc[0].m_Max = 50;
		m_Desc[0].m_Value = 25;
		m_Desc[1].m_Text = Resources.Vertical;
		m_Desc[1].m_UseYesNo = false;
		m_Desc[1].m_Min = 1;
		m_Desc[1].m_Max = 50;
		m_Desc[1].m_Value = 25;
		m_Desc[2].m_Text = Resources.InvertY;
		m_Desc[2].m_UseYesNo = true;
		m_Desc[2].m_Value = 0;
		m_Desc[3].m_Text = Resources.Vibration;
		m_Desc[3].m_UseYesNo = true;
		m_Desc[3].m_Value = 1;
		m_Desc[4].m_Text = Resources.Vol;
		m_Desc[4].m_UseYesNo = false;
		m_Desc[4].m_Min = 0;
		m_Desc[4].m_Max = 10;
		m_Desc[4].m_Value = 10;
		m_Desc[5].m_Text = "Music Volume";
		m_Desc[5].m_UseYesNo = false;
		m_Desc[5].m_Min = 0;
		m_Desc[5].m_Max = 10;
		m_Desc[5].m_Value = 10;
		m_Desc[6].m_Text = "Brightness";
		m_Desc[6].m_UseYesNo = false;
		m_Desc[6].m_Min = 0;
		m_Desc[6].m_Max = 3;
		m_Desc[6].m_Value = 0;
		m_Desc[7].m_Text = Resources.Reset;
		m_Desc[7].m_HasValue = false;
		m_Desc[8].m_Text = Resources.Accept;
		m_Desc[8].m_HasValue = false;
	}

	public OptionsMenuScreen(bool fadeBG)
		: base("OPTIONS")
	{
		m_FadeBG = fadeBG;
		m_Desc = new Desc[13];
		for (int i = 0; i < 13; i++)
		{
			m_Desc[i] = new Desc();
		}
		SetDescValues();
		m_Desc[0].m_Value = (int)(g.m_App.m_OptionsHoriz * 10f);
		m_Desc[1].m_Value = (int)(g.m_App.m_OptionsVert * 10f);
		m_Desc[2].m_Value = (g.m_App.m_OptionsInvertY ? 1 : 0);
		m_Desc[3].m_Value = (g.m_App.m_OptionsVibration ? 1 : 0);
		m_Desc[4].m_Value = (int)(g.m_App.m_OptionsVol * 10f);
		m_Desc[5].m_Value = (int)(g.m_App.m_OptionsMusicVol * 10f);
		m_Desc[6].m_Value = g.m_App.m_OptionsBrightness;
		MenuEntry item = new MenuEntry(Resources.Horizontal);
		base.MenuEntries.Add(item);
		MenuEntry item2 = new MenuEntry(Resources.Vertical);
		base.MenuEntries.Add(item2);
		MenuEntry item3 = new MenuEntry(Resources.InvertY);
		base.MenuEntries.Add(item3);
		MenuEntry item4 = new MenuEntry(Resources.Vibration);
		base.MenuEntries.Add(item4);
		MenuEntry item5 = new MenuEntry(Resources.Vol);
		base.MenuEntries.Add(item5);
		MenuEntry item6 = new MenuEntry("Music Volume");
		base.MenuEntries.Add(item6);
		MenuEntry item7 = new MenuEntry("Brightness");
		base.MenuEntries.Add(item7);
		MenuEntry menuEntry = new MenuEntry(Resources.Reset);
		menuEntry.Selected += OnChooseReset;
		base.MenuEntries.Add(menuEntry);
		MenuEntry menuEntry2 = new MenuEntry(Resources.Accept);
		menuEntry2.Selected += OnChooseAccept;
		base.MenuEntries.Add(menuEntry2);
	}

	private void OnChooseReset(object sender, PlayerIndexEventArgs e)
	{
		SetDescValues();
	}

	private void OnChooseAccept(object sender, PlayerIndexEventArgs e)
	{
		g.m_App.m_OptionsHoriz = (float)m_Desc[0].m_Value / 10f;
		g.m_App.m_OptionsVert = (float)m_Desc[1].m_Value / 10f;
		g.m_App.m_OptionsInvertY = m_Desc[2].m_Value != 0;
		g.m_App.m_OptionsVibration = m_Desc[3].m_Value != 0;
		g.m_App.m_OptionsVol = (float)m_Desc[4].m_Value / 10f;
		g.m_App.m_OptionsMusicVol = (float)m_Desc[5].m_Value / 10f;
		g.m_App.m_OptionsBrightness = m_Desc[6].m_Value;
		g.m_LoadSaveManager.SaveOptions();
		SoundEffect.MasterVolume = g.m_App.m_OptionsVol;
		MediaPlayer.Volume = g.m_App.m_OptionsMusicVol;
		ExitScreen();
	}

	private void OnChooseCancel(object sender, PlayerIndexEventArgs e)
	{
		SoundEffect.MasterVolume = g.m_App.m_OptionsVol;
		MediaPlayer.Volume = g.m_App.m_OptionsMusicVol;
		ExitScreen();
	}

	public override void LoadContent()
	{
		ContentManager content = base.ScreenManager.Game.Content;
		gradientTexture = content.Load<Texture2D>("sprites\\back");
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		for (int i = 0; i < base.MenuEntries.Count; i++)
		{
			if (!m_Desc[i].m_HasValue)
			{
				continue;
			}
			if (m_Desc[i].m_UseYesNo)
			{
				if (m_Desc[i].m_Value == 0)
				{
					base.MenuEntries[i].Text = $"{m_Desc[i].m_Text} : No";
				}
				else
				{
					base.MenuEntries[i].Text = $"{m_Desc[i].m_Text} : Yes";
				}
			}
			else
			{
				base.MenuEntries[i].Text = $"{m_Desc[i].m_Text} : {m_Desc[i].m_Value}";
			}
		}
		base.Update(gameTime, otherScreenHasFocus: false, coveredByOtherScreen: false);
	}

	public override void HandleInput(InputState input)
	{
		base.HandleInput(input);
		KeyboardState keyboardState = input.CurrentKeyboardStates[(int)base.ControllingPlayer.Value];
		GamePadState gamePadState = input.CurrentGamePadStates[(int)base.ControllingPlayer.Value];
		float num = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds;
		if (num < m_DelayTime)
		{
			return;
		}
		for (int i = 0; i < base.MenuEntries.Count; i++)
		{
			bool flag = base.IsActive && i == selectedEntry;
			if (i == 4 && flag)
			{
				SoundEffect.MasterVolume = (float)m_Desc[i].m_Value / 10f;
			}
			if (i == 5 && flag)
			{
				MediaPlayer.Volume = (float)m_Desc[i].m_Value / 10f;
			}
			if (i == 6 && flag && g.m_App.m_GameplayScreen != null)
			{
				g.m_App.m_GameplayScreen.SetBrightness(m_Desc[i].m_Value);
			}
			if (flag)
			{
				if (keyboardState.IsKeyDown(Keys.Left) && m_Desc[i].m_Value > m_Desc[i].m_Min)
				{
					m_Desc[i].m_Value--;
					m_DelayTime = num + 0.1f;
					g.m_SoundManager.Play(11);
				}
				if (keyboardState.IsKeyDown(Keys.Right) && m_Desc[i].m_Value < m_Desc[i].m_Max)
				{
					m_Desc[i].m_Value++;
					m_DelayTime = num + 0.1f;
					g.m_SoundManager.Play(10);
				}
				if ((gamePadState.ThumbSticks.Left.X < -0.5f || gamePadState.DPad.Left == ButtonState.Pressed) && m_Desc[i].m_Value > m_Desc[i].m_Min)
				{
					m_Desc[i].m_Value--;
					m_DelayTime = num + 0.1f;
					g.m_SoundManager.Play(11);
				}
				if ((gamePadState.ThumbSticks.Left.X > 0.5f || gamePadState.DPad.Right == ButtonState.Pressed) && m_Desc[i].m_Value < m_Desc[i].m_Max)
				{
					m_Desc[i].m_Value++;
					m_DelayTime = num + 0.1f;
					g.m_SoundManager.Play(10);
				}
			}
		}
	}

	protected override void UpdateMenuEntryLocations()
	{
		Vector2 position = new Vector2(0f, 175f);
		for (int i = 0; i < base.MenuEntries.Count; i++)
		{
			MenuEntry menuEntry = base.MenuEntries[i];
			position.X = base.ScreenManager.GraphicsDevice.Viewport.Width / 2 - menuEntry.GetWidth(this) / 2;
			menuEntry.Position = position;
			position.Y += menuEntry.GetHeight(this) + 10;
		}
	}

	public override void Draw(GameTime gameTime)
	{
		base.Draw(gameTime);
	}
}
