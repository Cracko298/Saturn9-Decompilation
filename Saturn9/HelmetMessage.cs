using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Saturn9;

public class HelmetMessage
{
	public enum TYPE
	{
		NONE,
		CRATE_NEAR,
		SCANNING,
		ALIEN,
		LOW_HEALTH,
		LOW_AMMO,
		LOW_AMMO_TOTAL,
		EMPTY_AMMO,
		RIVAL,
		AIRLOCK,
		WARNING,
		RECEIVED_AMMO,
		RECEIVED_HEALTH,
		RECEIVED_RPG,
		RECEIVED_ARTIFACT,
		ARTIFACT_TAKEN,
		ARTIFACT_DROPPED,
		ARTIFACT_IN_CONTAINER,
		ESCAPE_AIRLOCK,
		COUNTDOWN_STARTED,
		COUNTDOWN_CANCELLED,
		CROUCHED,
		LCD_INTERACT,
		LCD2_INTERACT,
		LCD3_INTERACT,
		LCD4_INTERACT,
		DOOR_LOCKED,
		DOOR2_LOCKED,
		DOOR3_LOCKED,
		DOOR4_LOCKED,
		DOOR5_LOCKED,
		DOOR6_LOCKED,
		DOOR7_LOCKED,
		DOOR8_LOCKED,
		DOOR9,
		USING_LCD11,
		USING_LCD12,
		USING_LCD13,
		USING_LCD14,
		USING_LCD15,
		USING_LCD21,
		USING_LCD22,
		USING_LCD23,
		USING_LCD24,
		USING_LCD25,
		USING_LCD26,
		USING_LCD27,
		USING_LCD28,
		USING_LCD29,
		USING_LCD30,
		USING_LCD31,
		USING_LCD32,
		USING_LCD33,
		USING_LCD34,
		USING_LCD35,
		USING_LCD36,
		USING_LCD37,
		USING_LCD38,
		USING_LCD39,
		USING_LCD40,
		USING_LCD41,
		USING_LCD42,
		USING_LCD43,
		USING_LCD44,
		USING_LCD45,
		USING_LCD46,
		USING_LCD47,
		O2_EMPTY,
		O2_FULL1,
		O2_EMPTY2,
		O2_FULL2,
		O2_EMPTY3,
		SAFE_INTERACT,
		PICKUP_SAW,
		PICKUP_ARM,
		USE_BIOSCAN,
		BIOSCAN_RESULT,
		PICKUP_SCREWDRIVER,
		USE_SCREWDRIVER,
		ENTERCARGO,
		RESEARCH,
		RESEARCHFOUND,
		RESEARCH2,
		RESEARCH3,
		TABLET,
		SAVING
	}

	public const int MAX_MSG = 48;

	public TYPE m_Type;

	public string m_Text;

	public float m_Time;

	public Color m_Colour;

	public Vector2 m_Position;

	public SoundManager.SFX m_SfxId;

	public SpriteFont m_Font;

	public float m_DisplayProgress;

	public bool m_HideSysMsg;

	public HelmetMessage()
	{
		m_Type = TYPE.NONE;
		m_Text = "";
		m_Time = 0f;
		m_Colour = Color.Green;
		m_Position = Vector2.Zero;
		m_SfxId = SoundManager.SFX.Down;
		m_Font = null;
		m_DisplayProgress = 0f;
	}
}
