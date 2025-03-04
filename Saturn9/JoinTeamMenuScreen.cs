using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;

namespace Saturn9;

internal class JoinTeamMenuScreen : MenuScreen
{
	private NetworkSession networkSession;

	private Texture2D gradientTexture;

	private Texture2D m_Artifact;

	private Texture2D m_Star;

	public JoinTeamMenuScreen(NetworkSession networkSession)
		: base("DOCKING COMPLETE")
	{
		this.networkSession = networkSession;
	}

	protected override void OnCancel(PlayerIndex playerIndex)
	{
	}

	public void OnAutoChoose()
	{
		g.m_PlayerManager.GetLocalPlayer().AutoChooseTeam();
		if (g.m_PlayerManager.GetLocalPlayer().m_Team == Player.TEAM.None)
		{
			return;
		}
		g.m_PlayerManager.GetLocalPlayer().SetClass(Player.CLASS.FatherD);
		if (g.m_PlayerManager.GetLocalPlayer().GetClass() == Player.CLASS.None)
		{
			return;
		}
		GameScreen[] screens = g.m_App.screenManager.GetScreens();
		for (int i = 0; i < screens.Length; i++)
		{
			if (screens[i] is GameplayScreen && screens[i] is GameplayScreen gameplayScreen)
			{
				if (g.m_App.m_SurvivalMode)
				{
					gameplayScreen.m_NextUnauthorisedDocking = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 3f;
				}
				else
				{
					gameplayScreen.m_NextUnauthorisedDocking = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 15f;
				}
			}
		}
		g.m_PlayerManager.GetLocalPlayer().SpawnLocal();
		if (g.m_App.SOUNDON)
		{
			MediaPlayer.Play(g.m_App.m_Level1Music);
			MediaPlayer.IsRepeating = true;
			MediaPlayer.Volume = g.m_App.m_OptionsMusicVol;
		}
		g.m_App.screenManager.RemoveScreen(this);
	}

	private void QuitGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		MessageBoxScreen messageBoxScreen = new MessageBoxScreen(Resources.ConfirmQuitGame);
		messageBoxScreen.Accepted += ConfirmQuitMessageBoxAccepted;
		base.ScreenManager.AddScreen(messageBoxScreen, base.ControllingPlayer);
	}

	private void ConfirmQuitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
	{
		LoadingScreen.Load(base.ScreenManager, false, g.m_App.m_PlayerOnePadId, 0, new BackgroundScreen(), new MainMenuScreen(1));
	}

	private void LeaveSessionMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		NetworkSessionComponent.LeaveSession(base.ScreenManager, e.PlayerIndex);
	}

	public override void LoadContent()
	{
		ContentManager content = base.ScreenManager.Game.Content;
		gradientTexture = content.Load<Texture2D>("sprites\\back");
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		OnAutoChoose();
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen: false);
	}

	public override void Draw(GameTime gameTime)
	{
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
		_ = base.ScreenManager.Font;
		Viewport viewport = base.ScreenManager.GraphicsDevice.Viewport;
		Vector2 vector = new Vector2(viewport.Width, viewport.Height);
		Vector2 vector2 = new Vector2(800f, 400f);
		Vector2 vector3 = (vector - vector2) / 2f;
		vector3.Y = 30f;
		Rectangle destinationRectangle = new Rectangle((int)vector3.X - 32, (int)vector3.Y - 16, (int)vector2.X + 64, (int)vector2.Y + 32);
		Color color = Color.White * base.TransitionAlpha;
		spriteBatch.Begin();
		spriteBatch.Draw(gradientTexture, destinationRectangle, color);
		spriteBatch.End();
		base.Draw(gameTime);
	}
}
