using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using BEPUphysics;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.CollisionTests;
using BEPUphysics.Constraints.SolverGroups;
using BEPUphysics.Constraints.TwoEntity.JointLimits;
using BEPUphysics.Constraints.TwoEntity.Joints;
using BEPUphysics.Constraints.TwoEntity.Motors;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.MathExtensions;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.SolverSystems;
using BEPUphysicsDemos.AlternateMovement.Character;
using Maths;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using SgMotion;
using SgMotion.Controllers;
using SynapseGaming.LightingSystem.Collision;
using SynapseGaming.LightingSystem.Core;
using SynapseGaming.LightingSystem.Effects.Deferred;
using SynapseGaming.LightingSystem.Lights;
using SynapseGaming.LightingSystem.Rendering;
using SynapseGaming.LightingSystem.Shadows;

namespace Saturn9;

public class Player : Actor
{
	public enum TRIGGERS
	{
		NONE,
		LCD1,
		DOOR1,
		DOOR2,
		LCD2,
		OXYGEN1,
		DOOR3,
		LCD3,
		SAFE,
		SAW,
		ARM,
		BIOSCAN,
		DOOR4,
		DOOR5,
		DOOR6,
		OXYGEN2,
		SCREWDRIVER,
		LOCKER1,
		LOCKER2,
		LOCKER3,
		LOCKER4,
		DOOR7,
		LCD4,
		DOOR8,
		RESEARCH1,
		RESEARCH2,
		RESEARCH3,
		RESEARCH4,
		RESEARCH5,
		DOOR9
	}

	public enum LCDMenu
	{
		OFF,
		SHOW_SOS,
		SHOW_SOS_GLITCH1,
		SHOW_SOS_GLITCH2,
		SHOW_SOS_GLITCH1_IN,
		SCREENSAVER,
		MAIN_MENU1,
		CAPTAIN1,
		HOLTZ1,
		SIMMONS1,
		SOSSYSTEM,
		MAIN_MENU2,
		CAPTAIN2,
		HOLTZ2,
		SIMMONS2,
		DOORSYSTEM,
		DOORPASSWORD,
		SECURITYQUESTION,
		MAIN_MENU3,
		CAPTAIN3,
		HOLTZ3,
		SIMMONS3,
		MEDICALNOTES,
		MEDICALNOTE1,
		MEDICALNOTE2,
		MEDICALNOTE3,
		MAIN_MENU4,
		CAPTAIN4,
		HOLTZ4,
		SIMMONS4,
		NETWORKMENU,
		PING,
		BRINGONLINE,
		PINGDOOR1,
		PINGDOOR2,
		PINGDOOR3,
		PINGDOOR4,
		PINGDOOR5
	}

	public enum STATE
	{
		JoinTeam,
		ChooseCharacter,
		InGame,
		LocalDeath,
		Intermission,
		UsingLCD1,
		UsingLCD2,
		UsingLCD3,
		UsingSafe,
		UsingLCD4,
		Grabbed
	}

	public enum TEAM
	{
		None,
		Vampire,
		Hunter,
		MedBay,
		OxygenTanks,
		CargoBay
	}

	public enum CLASS
	{
		None,
		FatherD,
		Molly,
		Edgar,
		Nina,
		MedBay,
		OxygenTanks,
		CargoBay
	}

	public enum BOTACTION
	{
		Idle,
		Searching,
		Attacking,
		VampireAttacking,
		Dying,
		Dead,
		Staking,
		FindPath,
		Caught
	}

	public const int HITZONE_NONE = 255;

	public const int HITZONE_TORSO = 1;

	public const int HITZONE_HEAD = 2;

	public const int HITZONE_LOWERBODY = 3;

	public const int HITZONE_EXPLOSION = 4;

	public const byte ANIM_NONE = byte.MaxValue;

	public const byte ANIM_IDLE_RIFLE = 0;

	public const byte ANIM_RUN_RIFLE = 1;

	public const byte ANIM_CROUCH_RIFLE = 2;

	public const byte ANIM_RELOAD_RIFLE = 6;

	public const byte ANIM_RUNBACK_RIFLE = 7;

	public const byte ANIM_STRAFELEFT_RIFLE = 8;

	public const byte ANIM_STRAFERIGHT_RIFLE = 9;

	public const byte ANIM_RUN_PISTOL = 11;

	public const byte ANIM_IDLE_PISTOL = 12;

	public const byte ANIM_RELOAD_PISTOL = 13;

	public const byte ANIM_RUNBACK_PISTOL = 14;

	public const byte ANIM_STRAFELEFT_PISTOL = 15;

	public const byte ANIM_STRAFERIGHT_PISTOL = 16;

	public const byte ANIM_CROUCH_PISTOL = 17;

	public const byte ANIM_RUN_RPG = 18;

	public const byte ANIM_IDLE_RPG = 19;

	public const byte ANIM_RELOAD_RPG = 20;

	public const byte ANIM_RUNBACK_RPG = 21;

	public const byte ANIM_STRAFELEFT_RPG = 22;

	public const byte ANIM_STRAFERIGHT_RPG = 23;

	public const byte ANIM_CROUCH_RPG = 24;

	public const byte ANIM_DEATH = 6;

	public const int VIEW_ANIM_NONE = -1;

	public const int VIEW_ANIM_IDLE = 0;

	public const int VIEW_ANIM_WALK = 1;

	public const int VIEW_ANIM_HOLSTER = 2;

	public const int VIEW_ANIM_USE_SAW = 3;

	public const int VIEW_ANIM_USE_BIOSCAN = 4;

	public const int VIEW_ANIM_USE_BIOSCAN_NOHAND = 5;

	public const int VIEW_ANIM_USE_SCREWDRIVER = 6;

	public const int ANIM_WALK_OLD = 0;

	public const int ANIM_IDLE = 1;

	public const int ANIM_MEDBAY_DOOR = 2;

	public const int ANIM_OXYGEN_DOOR = 3;

	public const int ANIM_WALK = 4;

	public const int ANIM_CAUGHT = 5;

	public const int VIEW_ANIM_FIRE = 1;

	public const int VIEW_ANIM_FIRE_SHORT = 1112;

	public const int VIEW_ANIM_RELOAD_SHORT = 1113;

	public const int VIEW_ANIM_RELOAD_START = 1114;

	public const int VIEW_ANIM_RELOAD_SHELL = 1115;

	public const int VIEW_ANIM_RELOAD_END = 6;

	public const int VIEW_ANIM_RELOAD_CROSSBOW = 7;

	public const int VIEW_ANIM_IDLE_CRUCIFIX = 8;

	public const int VIEW_ANIM_USE_CRUCIFIX = 9;

	public const int VIEW_ANIM_IDLE_STAKE = 10;

	public const int VIEW_ANIM_USE_STAKE = 11;

	public const int VIEW_ANIM_IDLE_RPG = 13;

	public const int VIEW_ANIM_FIRE_RPG = 14;

	public const int VIEW_ANIM_RELOAD_RPG = 15;

	public const int VIEW_ANIM_IDLE_PISTOL = 16;

	public const int VIEW_ANIM_FIRE_PISTOL = 17;

	public const int VIEW_ANIM_RELOAD_PISTOL = 18;

	public const int MAX_WEAPONS = 6;

	private const float SPAWN_INVINVIBILITY_TIME = 3f;

	private const float RESURRECT_INVINVIBILITY_TIME = 3f;

	private const float ITEM_RANGE_SQ = 42.25f;

	public const int MAX_SPRINT = 150;

	public const int NUM_TRIAL_CRATES = 3;

	public const int R_PROP = 1;

	public const int R_HAND = 24;

	public const int R_UPPERARM = 27;

	public const int R_PROP_VIEW = 34;

	public const int R_HAND_VIEW = 23;

	public const float BASE_TORCH_INTENSITY = 2.5f;

	private const float LEAP_THESHOLD_SQ = 225f;

	private const int STICK_FWD = 0;

	private const int STICK_BACK = 1;

	private const int STICK_LEFT = 2;

	private const int STICK_RIGHT = 3;

	private const float DEFAULT_BLEND_TIME = 0.2f;

	private const float RESPAWN_AFTER_STAKED_TIME = 5f;

	private const float RESPAWN_HUNTER_TIME = 1f;

	private const float RESURRECT_VAMPIRE_TIME = 1f;

	private const int MAX_TEMPBOTNODES = 256;

	private const float MAX_BOT_SPEED = 5f;

	private const float BOT_ACTION_TIME = 9f;

	private const float BOT_ATTACK_RANGE = 127f;

	private const float BOT_BEHIND_ATTACK_RANGE = 20.32f;

	private const float BOT_STAKE_RANGE = 127f;

	private const int NUM_BOT_NAMES = 7;

	private bool bFirst = true;

	public Entity m_HitZone_Head;

	public Entity m_HitZone_Torso;

	public Entity m_HitZone_LowerBody;

	public Entity pelvis;

	public Entity torsoBottom;

	public Entity torsoTop;

	public Entity neck;

	public Entity head;

	public Entity leftUpperArm;

	public Entity leftForearm;

	public Entity leftHand;

	public Entity rightUpperArm;

	public Entity rightForearm;

	public Entity rightHand;

	public Entity leftThigh;

	public Entity leftShin;

	public Entity leftFoot;

	public Entity rightThigh;

	public Entity rightShin;

	public Entity rightFoot;

	public BallSocketJoint pelvisToTorsoBottomBallSocketJoint;

	public TwistLimit pelvisToTorsoBottomTwistLimit;

	public SwingLimit pelvisToTorsoBottomSwingLimit;

	public AngularMotor pelvisToTorsoBottomMotor;

	public BallSocketJoint torsoBottomToTorsoTopBallSocketJoint;

	public TwistLimit torsoBottomToTorsoTopTwistLimit;

	public SwingLimit torsoBottomToTorsoTopSwingLimit;

	public AngularMotor torsoBottomToTorsoTopMotor;

	public BallSocketJoint torsoTopToNeckBallSocketJoint;

	public TwistLimit torsoTopToNeckTwistLimit;

	public SwingLimit torsoTopToNeckSwingLimit;

	public AngularMotor torsoTopToNeckMotor;

	public BallSocketJoint neckToHeadBallSocketJoint;

	public TwistLimit neckToHeadTwistLimit;

	public SwingLimit neckToHeadSwingLimit;

	public AngularMotor neckToHeadMotor;

	public BallSocketJoint torsoTopToLeftArmBallSocketJoint;

	public EllipseSwingLimit torsoTopToLeftArmEllipseLimit;

	public TwistLimit torsoTopToLeftArmTwistLimit;

	public AngularMotor torsoTopToLeftArmMotor;

	public SwivelHingeJoint leftUpperArmToLeftForearmSwivelHingeJoint;

	public AngularMotor leftUpperArmToLeftForearmMotor;

	public BallSocketJoint leftForearmToLeftHandBallSocketJoint;

	public EllipseSwingLimit leftForearmToLeftHandEllipseSwingLimit;

	public TwistLimit leftForearmToLeftHandTwistLimit;

	public AngularMotor leftForearmToLeftHandMotor;

	public BallSocketJoint torsoTopToRightArmBallSocketJoint;

	public EllipseSwingLimit torsoTopToRightArmEllipseLimit;

	public TwistLimit torsoTopToRightArmTwistLimit;

	public AngularMotor torsoTopToRightArmMotor;

	public SwivelHingeJoint rightUpperArmToRightForearmSwivelHingeJoint;

	public AngularMotor rightUpperArmToRightForearmMotor;

	public BallSocketJoint rightForearmToRightHandBallSocketJoint;

	public EllipseSwingLimit rightForearmToRightHandEllipseSwingLimit;

	public TwistLimit rightForearmToRightHandTwistLimit;

	public AngularMotor rightForearmToRightHandMotor;

	public BallSocketJoint pelvisToLeftThighBallSocketJoint;

	public EllipseSwingLimit pelvisToLeftThighEllipseSwingLimit;

	public TwistLimit pelvisToLeftThighTwistLimit;

	public AngularMotor pelvisToLeftThighMotor;

	public RevoluteJoint leftThighToLeftShinRevoluteJoint;

	public BallSocketJoint leftShinToLeftFootBallSocketJoint;

	public SwingLimit leftShinToLeftFootSwingLimit;

	public TwistLimit leftShinToLeftFootTwistLimit;

	public AngularMotor leftShinToLeftFootMotor;

	public BallSocketJoint pelvisToRightThighBallSocketJoint;

	public EllipseSwingLimit pelvisToRightThighEllipseSwingLimit;

	public TwistLimit pelvisToRightThighTwistLimit;

	public AngularMotor pelvisToRightThighMotor;

	public RevoluteJoint rightThighToRightShinRevoluteJoint;

	public BallSocketJoint rightShinToRightFootBallSocketJoint;

	public SwingLimit rightShinToRightFootSwingLimit;

	public TwistLimit rightShinToRightFootTwistLimit;

	public AngularMotor rightShinToRightFootMotor;

	private Vector3 m_RagdollCreatePosition;

	private float m_BodyThumpTime;

	private List<Entity> bones = new List<Entity>();

	private List<SolverUpdateable> joints = new List<SolverUpdateable>();

	private TRIGGERS m_NearTrigger;

	private TRIGGERS m_UsingTrigger;

	public LCDMenu m_LCDMenu = LCDMenu.SHOW_SOS;

	private float m_LCDTimer;

	private int m_LCDCursorRow;

	private int m_LCDCursorBlink;

	private bool m_LCDCursorOn;

	private float m_LCDCursorMoveTimer;

	public bool m_ShowAudioDisplay;

	private SoundEffectInstance m_AmbienceSFX;

	private Song m_AudioLog;

	private int[] m_Password = new int[4];

	private int m_PasswordIndex;

	private byte[] m_SecurityQuestion = new byte[7];

	private int m_SecurityQuestionIndex;

	private bool m_SecurityQuestionAnswered;

	private bool m_SecurityQuestionWrong;

	private byte[] m_SafePassword = new byte[4];

	private int m_SafePasswordIndex;

	private bool m_SafeOpened;

	private byte[] m_NetworkPassword = new byte[7];

	private int m_NetworkPasswordIndex;

	private bool m_NetworkPasswordAnswered;

	private byte[] m_IPAddress = new byte[3];

	private int m_IPAddressIndex;

	public bool m_Door0Unlocked;

	public bool m_Door1Unlocked;

	public bool m_DoorMedbayUnlocked;

	public bool m_DoorCrewExitUnlocked;

	private Vector2[] m_LinePos = new Vector2[16];

	private float[] m_LineRot = new float[16];

	private Vector2 m_LineDir = new Vector2(1f, 1f);

	private int m_KeyboardRow;

	private int m_KeyboardCol;

	private Color LCDGreen = new Color(39, 116, 39);

	private int m_PingFrame;

	public int m_TabletsCollected;

	public bool m_AllowNoodlesClue;

	private Vector2 HELMET_KEYBOARD_POS = new Vector2(70f, 240f);

	private float HELMET_KEYBOARD_X = 20f;

	private float HELMET_KEYBOARD_Y = 24f;

	public float m_LAX;

	public float m_LAY;

	public float thismove;

	public float thisrotate;

	public CharacterController m_CharacterController;

	public AnimationSet m_AnimationSet;

	public AnimationSet m_ViewAnimationSet;

	public byte m_Anim = byte.MaxValue;

	public int m_ViewAnim = -1;

	public byte m_AnimUpper = byte.MaxValue;

	public short m_NetId;

	public sbyte m_Health = 100;

	public int m_Sprint = 150;

	public bool m_bSprinting;

	private float m_SprintRegenTime;

	private float m_SprintArmAngle;

	public bool m_bStaked;

	public bool m_bRequestDied;

	public Vector2 m_Movement;

	public float m_Turn;

	public bool m_Jump;

	public bool m_Leap;

	public int m_WeaponItemIndex;

	public int m_StartWeaponItemIndex;

	public int[] m_Weapons = new int[6];

	public SpotLight m_TorchLight;

	public PointLight m_TorchPointLight;

	public PointLight m_TorchHelmetLight;

	public SkinnedModel m_ViewModel;

	public SceneObject m_ViewSceneObject;

	public Vector3 m_FrameMove;

	public float m_PunchAngle;

	public bool UPDATEFULLMODEL_DEBUG;

	public bool m_bFired;

	public bool m_RequestSendImpact;

	public Vector3 m_RequestSendImpactPos = Vector3.Zero;

	public Vector3 m_RequestSendImpactNormal = Vector3.Zero;

	public bool m_bTorchChanged;

	public bool m_bWeaponChanged;

	public bool m_Bot;

	public bool m_bRagdoll;

	public BloodQuadSprayParticleSystem m_BloodSpray;

	public bool m_bRequestSendDamage;

	public sbyte m_RequestedDamageAmount;

	public short m_RequestedPlayerToDamageNetID = 255;

	public byte m_RequestedHitZone = byte.MaxValue;

	public short m_RequestedAttacker = 255;

	public short m_RequestedProjectileNetId = 255;

	public short m_LastAttackerNetId = 255;

	public short m_LastProjectileNetId = 255;

	public bool m_DEBUG_Invincible;

	public float m_InvincibilityTime;

	public STATE m_State;

	public TEAM m_Team;

	public CLASS m_Class;

	public float m_RespawnTimer;

	public float m_FootstepTime;

	public int m_CurrentViewAnim = -1;

	public bool m_bRequestSendSpawn;

	public bool m_Crouch;

	public bool m_RequestSendCrouch;

	public float m_SpinePitch;

	public Matrix m_SpinePitchMatrix;

	public short m_Score;

	public short m_Kills;

	public short m_Deaths;

	public short m_Rank;

	public int m_XP;

	public int m_XPForNextRank;

	public bool m_RequestSendTeam;

	public bool m_RequestSendClass;

	public bool m_RequestSendScore;

	public bool m_AnimChanged;

	public bool m_AnimUpperChanged;

	public bool m_HasAmmoToGive;

	public bool m_RequestDelete;

	public bool m_RequestRankUp;

	public bool m_RequestCleanItems;

	public float m_ChangeTeamTime;

	public bool m_RequestUseCrate;

	public short m_RequestUseCrateId = -1;

	public float m_ScanningProgress;

	public bool m_ScanPressed;

	private int m_PrevScanMod;

	private bool m_ShowAlienDetected;

	private float m_ShowAlienDetectedTime;

	public int m_SpawnId = -1;

	public int m_AttachedItemId = -1;

	public SoundEffectInstance m_BreatheSFX;

	public SoundEffectInstance m_PhoneSFX;

	public SoundEffectInstance m_HeartBeatSFX;

	public SoundEffectInstance m_TickTockSFX;

	public SoundEffectInstance m_OceanSFX;

	public byte m_ArtifactEscapeAirlockId = byte.MaxValue;

	public bool m_RequestCreateProjectile;

	public short m_RequestCreateProjectileNetId = 255;

	public byte m_RequestCreateProjectileType = byte.MaxValue;

	public Vector3 m_RequestCreateProjectilePosition;

	public Quaternion m_RequestCreateProjectileQuaterion;

	public Vector3 m_RequestCreateProjectileVeclocity;

	public float m_NextRecoverTime;

	public float m_PeerPitch;

	public bool m_HasLocatorDevice;

	public float m_NextLocatorTime;

	public bool m_DrawLocator;

	private bool[] m_UsedCrate;

	public int m_TrialCratesOpened;

	public float m_CrouchY;

	public bool m_Hallucinate;

	public int m_NumSaws;

	private Vector3 m_ArmRot = Vector3.Zero;

	private BOTACTION m_PrevBotAction;

	private BOTACTION m_BotAction;

	private int m_TargetNode;

	private bool m_TargetDirectionForward;

	private Vector3 m_BotVecTarget;

	private Vector3 m_BotVecMove;

	private float m_BotTargetRotY;

	private float m_BotSpeed;

	private float m_NextActionTime;

	private float m_AttackTimeout;

	private int m_EnemyId;

	private bool m_BotAllowFire;

	private bool m_BotAllowMove;

	private byte m_BotNameIdx;

	public float m_LookForEnemyTime;

	private float m_BerserkTime;

	private Vector3 m_BotAimVector;

	private BotNode[] m_TempBotNode;

	private int m_TempPrevBotNodeID = -1;

	private int m_TempTargetNode = -1;

	private int m_TempPathTime;

	private int m_CurrentPathId;

	private bool m_bTryJoinMainPath;

	private float m_BotAccuracy = 0.2f;

	private float m_AlienTimeout = 6f;

	public static string[] BotMaleVampireNames = new string[7] { "ALIEN LIFEFORM", "ALIEN LIFEFORM", "ALIEN LIFEFORM", "ALIEN LIFEFORM", "ALIEN LIFEFORM", "ALIEN LIFEFORM", "ALIEN LIFEFORM" };

	public static string[] BotMaleSlayerNames = new string[7] { "Reynolds", "Spiegel", "O'Neil", "Hachirota", "Pinback", "Sinclair", "Bishop" };

	public void CreateHitZones()
	{
		m_HitZone_Torso = new Sphere(new Vector3(0f, 5f, 0f), 0.9f);
		m_HitZone_Torso.CollisionInformation.CollisionRules.Personal = CollisionRule.NoBroadPhase;
		m_HitZone_Torso.Tag = new HitTag(m_Id, 1);
		g.m_App.m_Space.Add(m_HitZone_Torso);
		m_HitZone_Head = new Sphere(m_HitZone_Torso.Position + new Vector3(0f, 2f, 0f), 0.6f);
		m_HitZone_Head.CollisionInformation.CollisionRules.Personal = CollisionRule.NoBroadPhase;
		m_HitZone_Head.Tag = new HitTag(m_Id, 2);
		g.m_App.m_Space.Add(m_HitZone_Head);
		m_HitZone_LowerBody = new Sphere(m_HitZone_Torso.Position + new Vector3(0f, -2f, 0f), 1f);
		m_HitZone_LowerBody.CollisionInformation.CollisionRules.Personal = CollisionRule.NoBroadPhase;
		m_HitZone_LowerBody.Tag = new HitTag(m_Id, 3);
		g.m_App.m_Space.Add(m_HitZone_LowerBody);
	}

	public void DestroyHitZones()
	{
		if (m_HitZone_Torso != null)
		{
			g.m_App.m_Space.Remove(m_HitZone_Torso);
			m_HitZone_Torso = null;
		}
		if (m_HitZone_Head != null)
		{
			g.m_App.m_Space.Remove(m_HitZone_Head);
			m_HitZone_Head = null;
		}
		if (m_HitZone_LowerBody != null)
		{
			g.m_App.m_Space.Remove(m_HitZone_LowerBody);
			m_HitZone_LowerBody = null;
		}
	}

	public void UpdateHitZones()
	{
		if (m_HitZone_Torso != null)
		{
			Matrix matrix = Matrix.CreateRotationY(m_Rotation.Y);
			if (m_SceneObject != null)
			{
				m_HitZone_Head.Position = Vector3.Transform(m_AnimationSet.GetBoneAbsoluteTransform("Bip01_Head").Translation, matrix) + m_SceneObject.World.Translation + new Vector3(0f, 0.2f, 0f);
				m_HitZone_Torso.Position = m_AnimationSet.GetBoneAbsoluteTransform("Bip01_Spine1").Translation + m_SceneObject.World.Translation + new Vector3(0f, -0.1f, 0f);
				m_HitZone_LowerBody.Position = m_AnimationSet.GetBoneAbsoluteTransform("Bip01").Translation + m_SceneObject.World.Translation + new Vector3(0f, -1f, 0f);
			}
			else
			{
				m_HitZone_Head.Position = Vector3.Transform(m_ViewAnimationSet.GetBoneAbsoluteTransform("Bip01_Head").Translation, matrix) + m_ViewSceneObject.World.Translation + new Vector3(0f, 0.2f, 0f);
				m_HitZone_Torso.Position = m_ViewAnimationSet.GetBoneAbsoluteTransform("Bip01_Spine1").Translation + m_ViewSceneObject.World.Translation + new Vector3(0f, -0.1f, 0f);
				m_HitZone_LowerBody.Position = m_ViewAnimationSet.GetBoneAbsoluteTransform("Bip01").Translation + m_ViewSceneObject.World.Translation + new Vector3(0f, -1f, 0f);
			}
		}
	}

	public void EnableRagdoll()
	{
		m_bRagdoll = true;
		if (m_Team == TEAM.Vampire)
		{
			if (m_CharacterController != null)
			{
				DisableCollisionAndGravity();
			}
			return;
		}
		if (m_Crouch)
		{
			Crouch();
		}
		if (m_CharacterController != null)
		{
			m_CharacterController.Body.CollisionInformation.CollisionRules.Personal = CollisionRule.NoBroadPhase;
		}
		m_RagdollCreatePosition = m_Position;
		_ = m_NetworkPosition.Y;
		_ = -10f;
		m_NetworkPosition = m_Position;
		m_HasAmmoToGive = true;
		CreateRagdoll();
		m_Anim = 0;
		if (m_WeaponItemIndex != -1)
		{
			g.m_ItemManager.m_Item[m_WeaponItemIndex].Hide();
		}
	}

	public void DisableRagdoll()
	{
		m_bRagdoll = false;
		if (m_Team == TEAM.Vampire)
		{
			EnableCollisionAndGravity();
			return;
		}
		DestroyRagdoll();
		if (m_CharacterController != null)
		{
			m_CharacterController.Body.CollisionInformation.CollisionRules.Personal = CollisionRule.Defer;
		}
		m_HasAmmoToGive = false;
		RagdollEnd();
	}

	public void CreateRagdoll()
	{
		if (m_SceneObject == null)
		{
			return;
		}
		if (m_AnimationSet != null)
		{
			m_AnimationSet.StopClip();
		}
		float num = 1.75f;
		float num2 = 0.05f;
		_ = Matrix.Identity;
		Matrix matrix = Matrix.CreateRotationY(m_Rotation.Y);
		if (pelvis == null)
		{
			pelvis = new Box(Vector3.Zero, 0.5f * num, 0.28f * num, 0.33f * num, 20f);
		}
		pelvis.Position = Vector3.Zero;
		pelvis.OrientationMatrix = Matrix3X3.CreateFromMatrix(matrix);
		pelvis.CollisionInformation.CollisionRules.Group = g.m_PlayerManager.m_RagdollGroup;
		if (torsoBottom == null)
		{
			torsoBottom = new Box(pelvis.Position + new Vector3(0f, 0.3f * num, 0f), 0.42f * num, 0.48f * num, 0.4f * num, 15f);
		}
		torsoBottom.Position = pelvis.Position + Vector3.Transform(new Vector3(0f, 0.3f * num, 0f), matrix);
		torsoBottom.OrientationMatrix = Matrix3X3.CreateFromMatrix(matrix);
		torsoBottom.CollisionInformation.CollisionRules.Group = g.m_PlayerManager.m_RagdollGroup;
		if (torsoTop == null)
		{
			torsoTop = new Box(torsoBottom.Position + new Vector3(0f, 0.3f * num, 0f), 0.5f * num, 0.38f * num, 0.42f * num, 20f);
		}
		torsoTop.Position = torsoBottom.Position + Vector3.Transform(new Vector3(0f, 0.3f * num, 0f), matrix);
		torsoTop.OrientationMatrix = Matrix3X3.CreateFromMatrix(matrix);
		torsoTop.CollisionInformation.CollisionRules.Group = g.m_PlayerManager.m_RagdollGroup;
		torsoTop.CollisionInformation.Events.ContactCreated += Events_ContactCreated;
		if (neck == null)
		{
			neck = new Box(torsoTop.Position + new Vector3(0f, 0.2f * num, 0.04f * num), 0.19f * num, 0.19f * num, 0.2f * num, 5f);
		}
		neck.Position = torsoTop.Position + Vector3.Transform(new Vector3(0f, 0.2f * num, 0.04f * num), matrix);
		neck.OrientationMatrix = Matrix3X3.CreateFromMatrix(matrix);
		neck.CollisionInformation.CollisionRules.Group = g.m_PlayerManager.m_RagdollGroup;
		if (head == null)
		{
			head = new Sphere(neck.Position + new Vector3(0f, 0.22f * num, -0.04f * num), 0.25f * num, 7f);
		}
		head.Position = neck.Position + Vector3.Transform(new Vector3(0f, 0.22f * num, -0.04f * num), matrix);
		head.OrientationMatrix = Matrix3X3.CreateFromMatrix(matrix);
		head.CollisionInformation.CollisionRules.Group = g.m_PlayerManager.m_RagdollGroup;
		if (leftUpperArm == null)
		{
			leftUpperArm = new Box(torsoTop.Position + new Vector3(-0.46f * num, 0.1f * num, 0f), 0.52f * num, 0.19f * num, 0.19f * num, 6f);
		}
		leftUpperArm.Position = torsoTop.Position + Vector3.Transform(new Vector3(-0.46f * num, 0.1f * num, 0f), matrix);
		leftUpperArm.OrientationMatrix = Matrix3X3.CreateFromMatrix(matrix);
		leftUpperArm.CollisionInformation.CollisionRules.Group = g.m_PlayerManager.m_RagdollGroup;
		if (leftForearm == null)
		{
			leftForearm = new Box(leftUpperArm.Position + new Vector3(-0.5f * num, 0f, 0f), 0.52f * num, 0.18f * num, 0.18f * num, 5f);
		}
		leftForearm.Position = leftUpperArm.Position + Vector3.Transform(new Vector3(-0.5f * num, 0f, 0f), matrix);
		leftForearm.OrientationMatrix = Matrix3X3.CreateFromMatrix(matrix);
		leftForearm.CollisionInformation.CollisionRules.Group = g.m_PlayerManager.m_RagdollGroup;
		if (leftHand == null)
		{
			leftHand = new Box(leftForearm.Position + new Vector3(-0.35f * num, 0f, 0f), 0.28f * num, 0.13f * num, 0.22f * num, 4f);
		}
		leftHand.Position = leftForearm.Position + Vector3.Transform(new Vector3(-0.35f * num, 0f, 0f), matrix);
		leftHand.OrientationMatrix = Matrix3X3.CreateFromMatrix(matrix);
		leftHand.CollisionInformation.CollisionRules.Group = g.m_PlayerManager.m_RagdollGroup;
		if (rightUpperArm == null)
		{
			rightUpperArm = new Box(torsoTop.Position + new Vector3(0.46f * num, 0.1f * num, 0f), 0.52f * num, 0.19f * num, 0.19f * num, 6f);
		}
		rightUpperArm.Position = torsoTop.Position + Vector3.Transform(new Vector3(0.46f * num, 0.1f * num, 0f), matrix);
		rightUpperArm.OrientationMatrix = Matrix3X3.CreateFromMatrix(matrix);
		rightUpperArm.CollisionInformation.CollisionRules.Group = g.m_PlayerManager.m_RagdollGroup;
		if (rightForearm == null)
		{
			rightForearm = new Box(rightUpperArm.Position + new Vector3(0.5f * num, 0f, 0f), 0.52f * num, 0.18f * num, 0.18f * num, 5f);
		}
		rightForearm.Position = rightUpperArm.Position + Vector3.Transform(new Vector3(0.5f * num, 0f, 0f), matrix);
		rightForearm.OrientationMatrix = Matrix3X3.CreateFromMatrix(matrix);
		rightForearm.CollisionInformation.CollisionRules.Group = g.m_PlayerManager.m_RagdollGroup;
		if (rightHand == null)
		{
			rightHand = new Box(rightForearm.Position + new Vector3(0.35f * num, 0f, 0f), 0.28f * num, 0.13f * num, 0.22f * num, 4f);
		}
		rightHand.Position = rightForearm.Position + Vector3.Transform(new Vector3(0.35f * num, 0f, 0f), matrix);
		rightHand.OrientationMatrix = Matrix3X3.CreateFromMatrix(matrix);
		rightHand.CollisionInformation.CollisionRules.Group = g.m_PlayerManager.m_RagdollGroup;
		if (leftThigh == null)
		{
			leftThigh = new Box(pelvis.Position + new Vector3(-0.15f * num, -0.4f * num, 0f), 0.23f * num, 0.63f * num, 0.23f * num, 10f);
		}
		leftThigh.Position = pelvis.Position + Vector3.Transform(new Vector3(-0.15f * num, -0.4f * num, 0f), matrix);
		leftThigh.OrientationMatrix = Matrix3X3.CreateFromMatrix(matrix);
		leftThigh.CollisionInformation.CollisionRules.Group = g.m_PlayerManager.m_RagdollGroup;
		if (leftShin == null)
		{
			leftShin = new Box(leftThigh.Position + new Vector3(0f, -0.6f * num, 0f), 0.21f * num, 0.63f * num, 0.21f * num, 7f);
		}
		leftShin.Position = leftThigh.Position + Vector3.Transform(new Vector3(0f, -0.6f * num, 0f), matrix);
		leftShin.OrientationMatrix = Matrix3X3.CreateFromMatrix(matrix);
		leftShin.CollisionInformation.CollisionRules.Group = g.m_PlayerManager.m_RagdollGroup;
		if (leftFoot == null)
		{
			leftFoot = new Box(leftShin.Position + new Vector3(0f, -0.35f * num, -0.1f), 0.23f * num, 0.15f * num, 0.43f * num, 5f);
		}
		leftFoot.Position = leftShin.Position + Vector3.Transform(new Vector3(0f, -0.35f * num, -0.1f), matrix);
		leftFoot.OrientationMatrix = Matrix3X3.CreateFromMatrix(matrix);
		leftFoot.CollisionInformation.CollisionRules.Group = g.m_PlayerManager.m_RagdollGroup;
		if (rightThigh == null)
		{
			rightThigh = new Box(pelvis.Position + new Vector3(0.15f * num, -0.4f * num, 0f), 0.23f * num, 0.63f * num, 0.23f * num, 10f);
		}
		rightThigh.Position = pelvis.Position + Vector3.Transform(new Vector3(0.15f * num, -0.4f * num, 0f), matrix);
		rightThigh.OrientationMatrix = Matrix3X3.CreateFromMatrix(matrix);
		rightThigh.CollisionInformation.CollisionRules.Group = g.m_PlayerManager.m_RagdollGroup;
		if (rightShin == null)
		{
			rightShin = new Box(rightThigh.Position + new Vector3(0f, -0.6f * num, 0f), 0.21f * num, 0.63f * num, 0.21f * num, 7f);
		}
		rightShin.Position = rightThigh.Position + Vector3.Transform(new Vector3(0f, -0.6f * num, 0f), matrix);
		rightShin.OrientationMatrix = Matrix3X3.CreateFromMatrix(matrix);
		rightShin.CollisionInformation.CollisionRules.Group = g.m_PlayerManager.m_RagdollGroup;
		if (rightFoot == null)
		{
			rightFoot = new Box(rightShin.Position + new Vector3(0f, -0.35f * num, -0.1f), 0.23f * num, 0.15f * num, 0.43f * num, 5f);
		}
		rightFoot.Position = rightShin.Position + Vector3.Transform(new Vector3(0f, -0.35f * num, -0.1f), matrix);
		rightFoot.OrientationMatrix = Matrix3X3.CreateFromMatrix(matrix);
		rightFoot.CollisionInformation.CollisionRules.Group = g.m_PlayerManager.m_RagdollGroup;
		if (bFirst)
		{
			bones.Add(pelvis);
			bones.Add(torsoBottom);
			bones.Add(torsoTop);
			bones.Add(neck);
			bones.Add(head);
			bones.Add(leftUpperArm);
			bones.Add(leftForearm);
			bones.Add(leftHand);
			bones.Add(rightUpperArm);
			bones.Add(rightForearm);
			bones.Add(rightHand);
			bones.Add(leftThigh);
			bones.Add(leftShin);
			bones.Add(leftFoot);
			bones.Add(rightThigh);
			bones.Add(rightShin);
			bones.Add(rightFoot);
			CollisionRules.AddRule(pelvis, torsoBottom, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(torsoBottom, torsoTop, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(torsoTop, neck, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(neck, head, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(head, torsoTop, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(torsoTop, leftUpperArm, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(leftUpperArm, leftForearm, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(leftForearm, leftHand, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(torsoTop, rightUpperArm, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(rightUpperArm, rightForearm, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(rightForearm, rightHand, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(pelvis, leftThigh, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(leftThigh, leftShin, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(leftThigh, torsoBottom, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(leftShin, leftFoot, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(pelvis, rightThigh, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(rightThigh, rightShin, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(rightThigh, torsoBottom, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(rightShin, rightFoot, CollisionRule.NoBroadPhase);
		}
		if (pelvisToTorsoBottomBallSocketJoint == null)
		{
			pelvisToTorsoBottomBallSocketJoint = new BallSocketJoint(pelvis, torsoBottom, pelvis.Position + Vector3.Transform(new Vector3(0f, 0.1f * num, 0f), matrix));
		}
		if (pelvisToTorsoBottomTwistLimit == null)
		{
			pelvisToTorsoBottomTwistLimit = new TwistLimit(pelvis, torsoBottom, Vector3.Up, Vector3.Up, -MathF.PI / 6f, MathF.PI / 6f);
		}
		if (pelvisToTorsoBottomSwingLimit == null)
		{
			pelvisToTorsoBottomSwingLimit = new SwingLimit(pelvis, torsoBottom, Vector3.Up, Vector3.Up, MathF.PI / 6f);
		}
		if (pelvisToTorsoBottomMotor == null)
		{
			pelvisToTorsoBottomMotor = new AngularMotor(pelvis, torsoBottom);
		}
		pelvisToTorsoBottomMotor.Settings.VelocityMotor.Softness = 0.05f * num2;
		if (torsoBottomToTorsoTopBallSocketJoint == null)
		{
			torsoBottomToTorsoTopBallSocketJoint = new BallSocketJoint(torsoBottom, torsoTop, torsoBottom.Position + Vector3.Transform(new Vector3(0f, 0.25f * num, 0f), matrix));
		}
		if (torsoBottomToTorsoTopSwingLimit == null)
		{
			torsoBottomToTorsoTopSwingLimit = new SwingLimit(torsoBottom, torsoTop, Vector3.Up, Vector3.Up, MathF.PI / 6f);
		}
		if (torsoBottomToTorsoTopTwistLimit == null)
		{
			torsoBottomToTorsoTopTwistLimit = new TwistLimit(torsoBottom, torsoTop, Vector3.Up, Vector3.Up, -MathF.PI / 6f, MathF.PI / 6f);
		}
		if (torsoBottomToTorsoTopMotor == null)
		{
			torsoBottomToTorsoTopMotor = new AngularMotor(torsoBottom, torsoTop);
		}
		torsoBottomToTorsoTopMotor.Settings.VelocityMotor.Softness = 0.05f * num2;
		if (torsoTopToNeckBallSocketJoint == null)
		{
			torsoTopToNeckBallSocketJoint = new BallSocketJoint(torsoTop, neck, torsoTop.Position + Vector3.Transform(new Vector3(0f, 0.15f * num, 0.05f * num), matrix));
		}
		if (torsoTopToNeckSwingLimit == null)
		{
			torsoTopToNeckSwingLimit = new SwingLimit(torsoTop, neck, Vector3.Up, Vector3.Up, MathF.PI / 6f);
		}
		if (torsoTopToNeckTwistLimit == null)
		{
			torsoTopToNeckTwistLimit = new TwistLimit(torsoTop, neck, Vector3.Up, Vector3.Up, -MathF.PI / 8f, MathF.PI / 8f);
		}
		if (torsoTopToNeckMotor == null)
		{
			torsoTopToNeckMotor = new AngularMotor(torsoTop, neck);
		}
		torsoTopToNeckMotor.Settings.VelocityMotor.Softness = 0.1f * num2;
		if (neckToHeadBallSocketJoint == null)
		{
			neckToHeadBallSocketJoint = new BallSocketJoint(neck, head, neck.Position + Vector3.Transform(new Vector3(0f, 0.1f * num, 0.05f * num), matrix));
		}
		if (neckToHeadTwistLimit == null)
		{
			neckToHeadTwistLimit = new TwistLimit(neck, head, Vector3.Up, Vector3.Up, -MathF.PI / 8f, MathF.PI / 8f);
		}
		if (neckToHeadSwingLimit == null)
		{
			neckToHeadSwingLimit = new SwingLimit(neck, head, Vector3.Up, Vector3.Up, MathF.PI / 6f);
		}
		if (neckToHeadMotor == null)
		{
			neckToHeadMotor = new AngularMotor(neck, head);
		}
		neckToHeadMotor.Settings.VelocityMotor.Softness = 0.1f * num2;
		if (torsoTopToLeftArmBallSocketJoint == null)
		{
			torsoTopToLeftArmBallSocketJoint = new BallSocketJoint(torsoTop, leftUpperArm, torsoTop.Position + Vector3.Transform(new Vector3(-0.3f * num, 0.1f * num, 0f), matrix));
		}
		if (torsoTopToLeftArmEllipseLimit == null)
		{
			torsoTopToLeftArmEllipseLimit = new EllipseSwingLimit(torsoTop, leftUpperArm, Vector3.Left, MathF.PI * 3f / 4f, MathF.PI / 2f);
		}
		if (torsoTopToLeftArmTwistLimit == null)
		{
			torsoTopToLeftArmTwistLimit = new TwistLimit(torsoTop, leftUpperArm, Vector3.Left, Vector3.Left, -MathF.PI / 2f, MathF.PI / 2f);
		}
		if (torsoTopToLeftArmMotor == null)
		{
			torsoTopToLeftArmMotor = new AngularMotor(torsoTop, leftUpperArm);
		}
		torsoTopToLeftArmMotor.Settings.VelocityMotor.Softness = 0.2f * num2;
		if (leftUpperArmToLeftForearmSwivelHingeJoint == null)
		{
			leftUpperArmToLeftForearmSwivelHingeJoint = new SwivelHingeJoint(leftUpperArm, leftForearm, leftUpperArm.Position + Vector3.Transform(new Vector3(-0.28f * num, 0f, 0f), matrix), Vector3.Up);
		}
		leftUpperArmToLeftForearmSwivelHingeJoint.HingeLimit.IsActive = true;
		leftUpperArmToLeftForearmSwivelHingeJoint.TwistLimit.IsActive = true;
		leftUpperArmToLeftForearmSwivelHingeJoint.TwistLimit.MinimumAngle = -MathF.PI / 8f;
		leftUpperArmToLeftForearmSwivelHingeJoint.TwistLimit.MaximumAngle = MathF.PI / 8f;
		leftUpperArmToLeftForearmSwivelHingeJoint.HingeLimit.MinimumAngle = MathF.PI * -4f / 5f;
		leftUpperArmToLeftForearmSwivelHingeJoint.HingeLimit.MaximumAngle = 0f;
		if (leftUpperArmToLeftForearmMotor == null)
		{
			leftUpperArmToLeftForearmMotor = new AngularMotor(leftUpperArm, leftForearm);
		}
		leftUpperArmToLeftForearmMotor.Settings.VelocityMotor.Softness = 0.3f * num2;
		if (leftForearmToLeftHandBallSocketJoint == null)
		{
			leftForearmToLeftHandBallSocketJoint = new BallSocketJoint(leftForearm, leftHand, leftForearm.Position + Vector3.Transform(new Vector3(-0.2f * num, 0f, 0f), matrix));
		}
		if (leftForearmToLeftHandEllipseSwingLimit == null)
		{
			leftForearmToLeftHandEllipseSwingLimit = new EllipseSwingLimit(leftForearm, leftHand, Vector3.Left, MathF.PI / 2f, MathF.PI / 6f);
		}
		if (leftForearmToLeftHandTwistLimit == null)
		{
			leftForearmToLeftHandTwistLimit = new TwistLimit(leftForearm, leftHand, Vector3.Left, Vector3.Left, -MathF.PI / 6f, MathF.PI / 6f);
		}
		if (leftForearmToLeftHandMotor == null)
		{
			leftForearmToLeftHandMotor = new AngularMotor(leftForearm, leftHand);
		}
		leftForearmToLeftHandMotor.Settings.VelocityMotor.Softness = 0.4f * num2;
		if (torsoTopToRightArmBallSocketJoint == null)
		{
			torsoTopToRightArmBallSocketJoint = new BallSocketJoint(torsoTop, rightUpperArm, torsoTop.Position + Vector3.Transform(new Vector3(0.3f * num, 0.1f * num, 0f), matrix));
		}
		if (torsoTopToRightArmEllipseLimit == null)
		{
			torsoTopToRightArmEllipseLimit = new EllipseSwingLimit(torsoTop, rightUpperArm, Vector3.Right, MathF.PI * 3f / 4f, MathF.PI / 2f);
		}
		if (torsoTopToRightArmTwistLimit == null)
		{
			torsoTopToRightArmTwistLimit = new TwistLimit(torsoTop, rightUpperArm, Vector3.Right, Vector3.Right, -MathF.PI / 2f, MathF.PI / 2f);
		}
		if (torsoTopToRightArmMotor == null)
		{
			torsoTopToRightArmMotor = new AngularMotor(torsoTop, rightUpperArm);
		}
		torsoTopToRightArmMotor.Settings.VelocityMotor.Softness = 0.2f * num2;
		if (rightUpperArmToRightForearmSwivelHingeJoint == null)
		{
			rightUpperArmToRightForearmSwivelHingeJoint = new SwivelHingeJoint(rightUpperArm, rightForearm, rightUpperArm.Position + Vector3.Transform(new Vector3(0.28f * num, 0f, 0f), matrix), Vector3.Up);
		}
		rightUpperArmToRightForearmSwivelHingeJoint.HingeLimit.IsActive = true;
		rightUpperArmToRightForearmSwivelHingeJoint.TwistLimit.IsActive = true;
		rightUpperArmToRightForearmSwivelHingeJoint.TwistLimit.MinimumAngle = -MathF.PI / 8f;
		rightUpperArmToRightForearmSwivelHingeJoint.TwistLimit.MaximumAngle = MathF.PI / 8f;
		rightUpperArmToRightForearmSwivelHingeJoint.HingeLimit.MinimumAngle = 0f;
		rightUpperArmToRightForearmSwivelHingeJoint.HingeLimit.MaximumAngle = MathF.PI * 4f / 5f;
		if (rightUpperArmToRightForearmMotor == null)
		{
			rightUpperArmToRightForearmMotor = new AngularMotor(rightUpperArm, rightForearm);
		}
		rightUpperArmToRightForearmMotor.Settings.VelocityMotor.Softness = 0.3f * num2;
		if (rightForearmToRightHandBallSocketJoint == null)
		{
			rightForearmToRightHandBallSocketJoint = new BallSocketJoint(rightForearm, rightHand, rightForearm.Position + Vector3.Transform(new Vector3(0.2f * num, 0f, 0f), matrix));
		}
		if (rightForearmToRightHandEllipseSwingLimit == null)
		{
			rightForearmToRightHandEllipseSwingLimit = new EllipseSwingLimit(rightForearm, rightHand, Vector3.Right, MathF.PI / 2f, MathF.PI / 6f);
		}
		if (rightForearmToRightHandTwistLimit == null)
		{
			rightForearmToRightHandTwistLimit = new TwistLimit(rightForearm, rightHand, Vector3.Right, Vector3.Right, -MathF.PI / 6f, MathF.PI / 6f);
		}
		if (rightForearmToRightHandMotor == null)
		{
			rightForearmToRightHandMotor = new AngularMotor(rightForearm, rightHand);
		}
		rightForearmToRightHandMotor.Settings.VelocityMotor.Softness = 0.4f * num2;
		if (pelvisToLeftThighBallSocketJoint == null)
		{
			pelvisToLeftThighBallSocketJoint = new BallSocketJoint(pelvis, leftThigh, pelvis.Position + Vector3.Transform(new Vector3(-0.15f * num, -0.1f * num, 0f), matrix));
		}
		if (pelvisToLeftThighEllipseSwingLimit == null)
		{
			pelvisToLeftThighEllipseSwingLimit = new EllipseSwingLimit(pelvis, leftThigh, Vector3.Normalize(new Vector3(-0.2f, -1f, -0.6f)), 2.1991148f, MathF.PI / 4f);
		}
		pelvisToLeftThighEllipseSwingLimit.LocalTwistAxisB = Vector3.Down;
		if (pelvisToLeftThighTwistLimit == null)
		{
			pelvisToLeftThighTwistLimit = new TwistLimit(pelvis, leftThigh, Vector3.Down, Vector3.Down, -MathF.PI / 6f, MathF.PI / 6f);
		}
		if (pelvisToLeftThighMotor == null)
		{
			pelvisToLeftThighMotor = new AngularMotor(pelvis, leftThigh);
		}
		pelvisToLeftThighMotor.Settings.VelocityMotor.Softness = 0.1f * num2;
		if (leftThighToLeftShinRevoluteJoint == null)
		{
			leftThighToLeftShinRevoluteJoint = new RevoluteJoint(leftThigh, leftShin, leftThigh.Position + Vector3.Transform(new Vector3(0f, -0.3f * num, 0f), matrix), Vector3.Right);
		}
		leftThighToLeftShinRevoluteJoint.Limit.IsActive = true;
		leftThighToLeftShinRevoluteJoint.Limit.MinimumAngle = MathF.PI * -4f / 5f;
		leftThighToLeftShinRevoluteJoint.Limit.MaximumAngle = 0f;
		leftThighToLeftShinRevoluteJoint.Motor.IsActive = true;
		leftThighToLeftShinRevoluteJoint.Motor.Settings.VelocityMotor.Softness = 0.2f * num2;
		if (leftShinToLeftFootBallSocketJoint == null)
		{
			leftShinToLeftFootBallSocketJoint = new BallSocketJoint(leftShin, leftFoot, leftShin.Position + Vector3.Transform(new Vector3(0f, -0.3f * num, 0f), matrix));
		}
		if (leftShinToLeftFootSwingLimit == null)
		{
			leftShinToLeftFootSwingLimit = new SwingLimit(leftShin, leftFoot, Vector3.Forward, Vector3.Forward, MathF.PI / 8f);
		}
		if (leftShinToLeftFootTwistLimit == null)
		{
			leftShinToLeftFootTwistLimit = new TwistLimit(leftShin, leftFoot, Vector3.Down, Vector3.Forward, -MathF.PI / 8f, MathF.PI / 8f);
		}
		if (leftShinToLeftFootMotor == null)
		{
			leftShinToLeftFootMotor = new AngularMotor(leftShin, leftFoot);
		}
		leftShinToLeftFootMotor.Settings.VelocityMotor.Softness = 0.2f * num2;
		leftShinToLeftFootTwistLimit.SpringSettings.StiffnessConstant = 100f;
		leftShinToLeftFootTwistLimit.SpringSettings.DampingConstant = 500f;
		if (pelvisToRightThighBallSocketJoint == null)
		{
			pelvisToRightThighBallSocketJoint = new BallSocketJoint(pelvis, rightThigh, pelvis.Position + Vector3.Transform(new Vector3(0.15f * num, -0.1f * num, 0f), matrix));
		}
		if (pelvisToRightThighEllipseSwingLimit == null)
		{
			pelvisToRightThighEllipseSwingLimit = new EllipseSwingLimit(pelvis, rightThigh, Vector3.Normalize(new Vector3(0.2f, -1f, -0.6f)), 2.1991148f, MathF.PI / 4f);
		}
		pelvisToRightThighEllipseSwingLimit.LocalTwistAxisB = Vector3.Down;
		if (pelvisToRightThighTwistLimit == null)
		{
			pelvisToRightThighTwistLimit = new TwistLimit(pelvis, rightThigh, Vector3.Down, Vector3.Down, -MathF.PI / 6f, MathF.PI / 6f);
		}
		if (pelvisToRightThighMotor == null)
		{
			pelvisToRightThighMotor = new AngularMotor(pelvis, rightThigh);
		}
		pelvisToRightThighMotor.Settings.VelocityMotor.Softness = 0.1f * num2;
		if (rightThighToRightShinRevoluteJoint == null)
		{
			rightThighToRightShinRevoluteJoint = new RevoluteJoint(rightThigh, rightShin, rightThigh.Position + Vector3.Transform(new Vector3(0f, -0.3f * num, 0f), matrix), Vector3.Right);
		}
		rightThighToRightShinRevoluteJoint.Limit.IsActive = true;
		rightThighToRightShinRevoluteJoint.Limit.MinimumAngle = MathF.PI * -4f / 5f;
		rightThighToRightShinRevoluteJoint.Limit.MaximumAngle = 0f;
		rightThighToRightShinRevoluteJoint.Motor.IsActive = true;
		rightThighToRightShinRevoluteJoint.Motor.Settings.VelocityMotor.Softness = 0.2f * num2;
		if (rightShinToRightFootBallSocketJoint == null)
		{
			rightShinToRightFootBallSocketJoint = new BallSocketJoint(rightShin, rightFoot, rightShin.Position + Vector3.Transform(new Vector3(0f, -0.3f * num, 0f), matrix));
		}
		if (rightShinToRightFootSwingLimit == null)
		{
			rightShinToRightFootSwingLimit = new SwingLimit(rightShin, rightFoot, Vector3.Forward, Vector3.Forward, MathF.PI / 8f);
		}
		if (rightShinToRightFootTwistLimit == null)
		{
			rightShinToRightFootTwistLimit = new TwistLimit(rightShin, rightFoot, Vector3.Down, Vector3.Forward, -MathF.PI / 8f, MathF.PI / 8f);
		}
		if (rightShinToRightFootMotor == null)
		{
			rightShinToRightFootMotor = new AngularMotor(rightShin, rightFoot);
		}
		rightShinToRightFootMotor.Settings.VelocityMotor.Softness = 0.2f * num2;
		rightShinToRightFootTwistLimit.SpringSettings.StiffnessConstant = 100f;
		rightShinToRightFootTwistLimit.SpringSettings.DampingConstant = 500f;
		if (bFirst)
		{
			joints.Add(pelvisToTorsoBottomBallSocketJoint);
			joints.Add(pelvisToTorsoBottomTwistLimit);
			joints.Add(pelvisToTorsoBottomSwingLimit);
			joints.Add(pelvisToTorsoBottomMotor);
			joints.Add(torsoBottomToTorsoTopBallSocketJoint);
			joints.Add(torsoBottomToTorsoTopTwistLimit);
			joints.Add(torsoBottomToTorsoTopSwingLimit);
			joints.Add(torsoBottomToTorsoTopMotor);
			joints.Add(torsoTopToNeckBallSocketJoint);
			joints.Add(torsoTopToNeckTwistLimit);
			joints.Add(torsoTopToNeckSwingLimit);
			joints.Add(torsoTopToNeckMotor);
			joints.Add(neckToHeadBallSocketJoint);
			joints.Add(neckToHeadTwistLimit);
			joints.Add(neckToHeadSwingLimit);
			joints.Add(neckToHeadMotor);
			joints.Add(torsoTopToLeftArmBallSocketJoint);
			joints.Add(torsoTopToLeftArmEllipseLimit);
			joints.Add(torsoTopToLeftArmTwistLimit);
			joints.Add(torsoTopToLeftArmMotor);
			joints.Add(leftUpperArmToLeftForearmSwivelHingeJoint);
			joints.Add(leftUpperArmToLeftForearmMotor);
			joints.Add(leftForearmToLeftHandBallSocketJoint);
			joints.Add(leftForearmToLeftHandEllipseSwingLimit);
			joints.Add(leftForearmToLeftHandTwistLimit);
			joints.Add(leftForearmToLeftHandMotor);
			joints.Add(torsoTopToRightArmBallSocketJoint);
			joints.Add(torsoTopToRightArmEllipseLimit);
			joints.Add(torsoTopToRightArmTwistLimit);
			joints.Add(torsoTopToRightArmMotor);
			joints.Add(rightUpperArmToRightForearmSwivelHingeJoint);
			joints.Add(rightUpperArmToRightForearmMotor);
			joints.Add(rightForearmToRightHandBallSocketJoint);
			joints.Add(rightForearmToRightHandEllipseSwingLimit);
			joints.Add(rightForearmToRightHandTwistLimit);
			joints.Add(rightForearmToRightHandMotor);
			joints.Add(pelvisToLeftThighBallSocketJoint);
			joints.Add(pelvisToLeftThighEllipseSwingLimit);
			joints.Add(pelvisToLeftThighTwistLimit);
			joints.Add(pelvisToLeftThighMotor);
			joints.Add(leftThighToLeftShinRevoluteJoint);
			joints.Add(leftShinToLeftFootBallSocketJoint);
			joints.Add(leftShinToLeftFootSwingLimit);
			joints.Add(leftShinToLeftFootTwistLimit);
			joints.Add(leftShinToLeftFootMotor);
			joints.Add(pelvisToRightThighBallSocketJoint);
			joints.Add(pelvisToRightThighEllipseSwingLimit);
			joints.Add(pelvisToRightThighTwistLimit);
			joints.Add(pelvisToRightThighMotor);
			joints.Add(rightThighToRightShinRevoluteJoint);
			joints.Add(rightShinToRightFootBallSocketJoint);
			joints.Add(rightShinToRightFootSwingLimit);
			joints.Add(rightShinToRightFootTwistLimit);
			joints.Add(rightShinToRightFootMotor);
		}
		foreach (Entity bone in bones)
		{
			bone.WorldTransform *= m_SceneObject.World;
			bone.Position += new Vector3(0f, 2.75f, 0f);
			g.m_App.m_Space.Add(bone);
		}
		foreach (SolverUpdateable joint in joints)
		{
			g.m_App.m_Space.Add(joint);
		}
		bFirst = false;
		UpdateRagdoll();
	}

	private void Events_ContactCreated(EntityCollidable sender, Collidable other, CollidablePairHandler pair, ContactData contact)
	{
		if (m_BodyThumpTime < (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds && !(other.Shape is BoxShape) && contact.PenetrationDepth > 0.02f)
		{
			int num = g.m_App.m_Rand.Next(0, 4);
			g.m_SoundManager.Play3D(12 + num, m_Position);
			m_BodyThumpTime = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 0.5f;
		}
	}

	public void DestroyRagdoll()
	{
		g.m_App.m_Space.Remove(pelvis);
		g.m_App.m_Space.Remove(torsoBottom);
		g.m_App.m_Space.Remove(torsoTop);
		g.m_App.m_Space.Remove(neck);
		g.m_App.m_Space.Remove(head);
		g.m_App.m_Space.Remove(leftUpperArm);
		g.m_App.m_Space.Remove(leftForearm);
		g.m_App.m_Space.Remove(leftHand);
		g.m_App.m_Space.Remove(rightUpperArm);
		g.m_App.m_Space.Remove(rightForearm);
		g.m_App.m_Space.Remove(rightHand);
		g.m_App.m_Space.Remove(leftThigh);
		g.m_App.m_Space.Remove(leftShin);
		g.m_App.m_Space.Remove(leftFoot);
		g.m_App.m_Space.Remove(rightThigh);
		g.m_App.m_Space.Remove(rightShin);
		g.m_App.m_Space.Remove(rightFoot);
		g.m_App.m_Space.Remove(pelvisToTorsoBottomBallSocketJoint);
		g.m_App.m_Space.Remove(pelvisToTorsoBottomTwistLimit);
		g.m_App.m_Space.Remove(pelvisToTorsoBottomSwingLimit);
		g.m_App.m_Space.Remove(pelvisToTorsoBottomMotor);
		g.m_App.m_Space.Remove(torsoBottomToTorsoTopBallSocketJoint);
		g.m_App.m_Space.Remove(torsoBottomToTorsoTopTwistLimit);
		g.m_App.m_Space.Remove(torsoBottomToTorsoTopSwingLimit);
		g.m_App.m_Space.Remove(torsoBottomToTorsoTopMotor);
		g.m_App.m_Space.Remove(torsoTopToNeckBallSocketJoint);
		g.m_App.m_Space.Remove(torsoTopToNeckTwistLimit);
		g.m_App.m_Space.Remove(torsoTopToNeckSwingLimit);
		g.m_App.m_Space.Remove(torsoTopToNeckMotor);
		g.m_App.m_Space.Remove(neckToHeadBallSocketJoint);
		g.m_App.m_Space.Remove(neckToHeadTwistLimit);
		g.m_App.m_Space.Remove(neckToHeadSwingLimit);
		g.m_App.m_Space.Remove(neckToHeadMotor);
		g.m_App.m_Space.Remove(torsoTopToLeftArmBallSocketJoint);
		g.m_App.m_Space.Remove(torsoTopToLeftArmEllipseLimit);
		g.m_App.m_Space.Remove(torsoTopToLeftArmTwistLimit);
		g.m_App.m_Space.Remove(torsoTopToLeftArmMotor);
		g.m_App.m_Space.Remove(leftUpperArmToLeftForearmSwivelHingeJoint);
		g.m_App.m_Space.Remove(leftUpperArmToLeftForearmMotor);
		g.m_App.m_Space.Remove(leftForearmToLeftHandBallSocketJoint);
		g.m_App.m_Space.Remove(leftForearmToLeftHandEllipseSwingLimit);
		g.m_App.m_Space.Remove(leftForearmToLeftHandTwistLimit);
		g.m_App.m_Space.Remove(leftForearmToLeftHandMotor);
		g.m_App.m_Space.Remove(torsoTopToRightArmBallSocketJoint);
		g.m_App.m_Space.Remove(torsoTopToRightArmEllipseLimit);
		g.m_App.m_Space.Remove(torsoTopToRightArmTwistLimit);
		g.m_App.m_Space.Remove(torsoTopToRightArmMotor);
		g.m_App.m_Space.Remove(rightUpperArmToRightForearmSwivelHingeJoint);
		g.m_App.m_Space.Remove(rightUpperArmToRightForearmMotor);
		g.m_App.m_Space.Remove(rightForearmToRightHandBallSocketJoint);
		g.m_App.m_Space.Remove(rightForearmToRightHandEllipseSwingLimit);
		g.m_App.m_Space.Remove(rightForearmToRightHandTwistLimit);
		g.m_App.m_Space.Remove(rightForearmToRightHandMotor);
		g.m_App.m_Space.Remove(pelvisToLeftThighBallSocketJoint);
		g.m_App.m_Space.Remove(pelvisToLeftThighEllipseSwingLimit);
		g.m_App.m_Space.Remove(pelvisToLeftThighTwistLimit);
		g.m_App.m_Space.Remove(pelvisToLeftThighMotor);
		g.m_App.m_Space.Remove(leftThighToLeftShinRevoluteJoint);
		g.m_App.m_Space.Remove(leftShinToLeftFootBallSocketJoint);
		g.m_App.m_Space.Remove(leftShinToLeftFootSwingLimit);
		g.m_App.m_Space.Remove(leftShinToLeftFootTwistLimit);
		g.m_App.m_Space.Remove(leftShinToLeftFootMotor);
		g.m_App.m_Space.Remove(pelvisToRightThighBallSocketJoint);
		g.m_App.m_Space.Remove(pelvisToRightThighEllipseSwingLimit);
		g.m_App.m_Space.Remove(pelvisToRightThighTwistLimit);
		g.m_App.m_Space.Remove(pelvisToRightThighMotor);
		g.m_App.m_Space.Remove(rightThighToRightShinRevoluteJoint);
		g.m_App.m_Space.Remove(rightShinToRightFootBallSocketJoint);
		g.m_App.m_Space.Remove(rightShinToRightFootSwingLimit);
		g.m_App.m_Space.Remove(rightShinToRightFootTwistLimit);
		g.m_App.m_Space.Remove(rightShinToRightFootMotor);
	}

	public void UpdateRagdoll()
	{
		if (m_Team != TEAM.Vampire)
		{
			Matrix worldTransform = pelvis.WorldTransform;
			worldTransform.Translation += new Vector3(0f, 2f, 0f);
			Vector3 position = new Vector3(0f, 0.14999998f, -0.1f);
			m_Position = Vector3.Transform(position, worldTransform);
			m_NetworkPosition = m_Position;
			worldTransform.Translation -= new Vector3(0f, 2f, 0f);
			Matrix matrix = Matrix.CreateRotationY(0f - m_Rotation.Y);
			Matrix angles = Matrix.CreateRotationY(MathHelper.ToRadians(-90f)) * (pelvis.WorldTransform * matrix);
			m_AnimationSet.SetBoneController("Bip01", ref angles);
			angles = Matrix.CreateRotationY(MathHelper.ToRadians(180f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(90f)) * (torsoBottom.WorldTransform * matrix);
			m_AnimationSet.SetBoneController("Bip01_Spine", ref angles);
			angles = Matrix.CreateRotationY(MathHelper.ToRadians(180f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(90f)) * (torsoTop.WorldTransform * matrix);
			m_AnimationSet.SetBoneController("Bip01_Spine1", ref angles);
			angles = Matrix.CreateRotationY(MathHelper.ToRadians(180f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(90f)) * (neck.WorldTransform * matrix);
			m_AnimationSet.SetBoneController("Bip01_Neck", ref angles);
			angles = Matrix.CreateRotationY(MathHelper.ToRadians(180f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(90f)) * (head.WorldTransform * matrix);
			m_AnimationSet.SetBoneController("Bip01_Head", ref angles);
			angles = Matrix.CreateRotationY(MathHelper.ToRadians(180f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(-90f)) * (leftThigh.WorldTransform * matrix);
			m_AnimationSet.SetBoneController("Bip01_L_Thigh", ref angles);
			angles = Matrix.CreateRotationY(MathHelper.ToRadians(180f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(-90f)) * (leftShin.WorldTransform * matrix);
			m_AnimationSet.SetBoneController("Bip01_L_Calf", ref angles);
			angles = Matrix.CreateRotationY(MathHelper.ToRadians(180f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(-90f)) * (leftFoot.WorldTransform * matrix);
			m_AnimationSet.SetBoneController("Bip01_L_Foot", ref angles);
			angles = Matrix.CreateRotationY(MathHelper.ToRadians(180f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(-90f)) * (rightThigh.WorldTransform * matrix);
			m_AnimationSet.SetBoneController("Bip01_R_Thigh", ref angles);
			angles = Matrix.CreateRotationY(MathHelper.ToRadians(180f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(-90f)) * (rightShin.WorldTransform * matrix);
			m_AnimationSet.SetBoneController("Bip01_R_Calf", ref angles);
			angles = Matrix.CreateRotationY(MathHelper.ToRadians(180f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(-90f)) * (rightFoot.WorldTransform * matrix);
			m_AnimationSet.SetBoneController("Bip01_R_Foot", ref angles);
			angles = Matrix.CreateRotationZ(MathHelper.ToRadians(-10f)) * (leftUpperArm.WorldTransform * matrix);
			m_AnimationSet.SetBoneController("Bip01_L_UpperArm", ref angles);
			angles = Matrix.CreateRotationZ(MathHelper.ToRadians(-10f)) * (leftForearm.WorldTransform * matrix);
			m_AnimationSet.SetBoneController("Bip01_L_Forearm", ref angles);
			angles = Matrix.CreateRotationZ(MathHelper.ToRadians(-10f)) * (leftHand.WorldTransform * matrix);
			m_AnimationSet.SetBoneController("Bip01_L_Hand", ref angles);
			angles = Matrix.CreateRotationY(MathHelper.ToRadians(180f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(10f)) * (rightUpperArm.WorldTransform * matrix);
			m_AnimationSet.SetBoneController("Bip01_R_UpperArm", ref angles);
			angles = Matrix.CreateRotationY(MathHelper.ToRadians(180f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(10f)) * (rightForearm.WorldTransform * matrix);
			m_AnimationSet.SetBoneController("Bip01_R_Forearm", ref angles);
			angles = Matrix.CreateRotationY(MathHelper.ToRadians(180f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(10f)) * (rightHand.WorldTransform * matrix);
			m_AnimationSet.SetBoneController("Bip01_R_Hand", ref angles);
		}
	}

	public void InitPlayerGameData()
	{
		ResetPassword();
		ResetSecurityQuestion();
		ResetNetworkPassword();
		ResetIPAddress();
		m_ShowAudioDisplay = false;
		m_LCDMenu = LCDMenu.SHOW_SOS;
		m_LCDCursorRow = 0;
		m_KeyboardRow = 0;
		m_KeyboardCol = 0;
		m_Door0Unlocked = false;
		m_Door1Unlocked = false;
		m_SecurityQuestionAnswered = false;
		m_SecurityQuestionWrong = false;
		m_SafeOpened = false;
		m_DoorMedbayUnlocked = false;
		m_DoorCrewExitUnlocked = false;
		m_NetworkPasswordAnswered = false;
		m_TabletsCollected = 0;
		m_AllowNoodlesClue = false;
	}

	public bool FindFacingTrigger(Vector3 triggerPos, Vector3 facing, float rangeSq, float dotTol)
	{
		float num = 1E+11f;
		bool flag = false;
		float num2 = 1E+11f;
		Vector3 vector = triggerPos - m_Position;
		vector.Y = 0f;
		num = vector.LengthSquared();
		if (num < num2 && num < rangeSq)
		{
			num2 = num;
			flag = true;
		}
		if (!flag)
		{
			return false;
		}
		Vector3 vector2 = g.m_CameraManager.m_Position - triggerPos;
		vector2.Normalize();
		float num3 = Vector3.Dot(vector2, facing);
		if (num3 < dotTol)
		{
			return true;
		}
		return false;
	}

	private void UpdatePlayerUsingLCD()
	{
		float num = 60f * (float)g.m_App.m_GameTime.ElapsedGameTime.TotalSeconds;
		m_Rotation.Y += (0f - m_Turn) * 0.04f * num * (g.m_App.m_OptionsHoriz * 0.4f);
		Matrix matrix = Matrix.CreateRotationY(m_Rotation.Y);
		Matrix matrix2 = Matrix.CreateTranslation(m_ViewSceneObject.World.Translation);
		m_ViewSceneObject.World = matrix * matrix2;
		UpdateViewAnimation();
		Matrix angles = Matrix.CreateRotationY(3.14f) * Matrix.CreateRotationZ(1.57f) * Matrix.CreateRotationX(g.m_CameraManager.m_Pitch + MathHelper.ToRadians(-14f));
		m_ViewAnimationSet.SetBoneController("Bip01_Neck", ref angles);
		Matrix identity = Matrix.Identity;
		m_ViewAnimationSet.Update(g.m_App.m_GameTime.ElapsedGameTime, identity);
		m_ViewSceneObject.SkinBones = m_ViewAnimationSet.SkinnedBoneTransforms;
	}

	public void Interact()
	{
		MiscTriggerEntity obj;
		switch (m_NearTrigger)
		{
		case TRIGGERS.LCD1:
			InteractLCD1();
			break;
		case TRIGGERS.DOOR1:
		{
			if (m_State != STATE.InGame || !m_Door0Unlocked)
			{
				break;
			}
			if (Guide.IsTrialMode)
			{
				TryPurchase();
				break;
			}
			g.m_ItemManager.m_Item[g.m_ItemManager.m_DoorItemId[0]].PeerUseDoor(m_NetId);
			string name = "TriggerDoor1";
			if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
			{
				obj.m_Complete = true;
			}
			break;
		}
		case TRIGGERS.LCD2:
			InteractLCD2();
			break;
		case TRIGGERS.OXYGEN1:
		{
			string name = "TriggerO2Full1";
			if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && !obj.m_Complete)
			{
				HallucinateOff(rumble: true);
				obj.m_Complete = true;
			}
			break;
		}
		case TRIGGERS.DOOR2:
			if (m_State == STATE.InGame && m_Door1Unlocked)
			{
				g.m_ItemManager.m_Item[g.m_ItemManager.m_DoorItemId[1]].PeerUseDoor(m_NetId);
				string name = "TriggerDoor2";
				if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
				{
					obj.m_Complete = true;
				}
			}
			break;
		case TRIGGERS.DOOR3:
			if (m_State == STATE.InGame)
			{
				g.m_ItemManager.m_Item[g.m_ItemManager.m_DoorItemId[2]].PeerUseDoor(m_NetId);
				string name = "TriggerDoor3";
				if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
				{
					obj.m_Complete = true;
				}
			}
			break;
		case TRIGGERS.LCD3:
			InteractLCD3();
			break;
		case TRIGGERS.SAFE:
			InteractSafe();
			break;
		case TRIGGERS.SAW:
			InteractSaw();
			break;
		case TRIGGERS.ARM:
			InteractArm();
			break;
		case TRIGGERS.BIOSCAN:
			InteractBioScan();
			break;
		case TRIGGERS.DOOR4:
		{
			if (m_State != STATE.InGame || !m_DoorMedbayUnlocked)
			{
				break;
			}
			if (!g.m_PlayerManager.m_MedBayAlienShown)
			{
				g.m_PlayerManager.m_MedBayAlienShown = true;
				g.m_PlayerManager.m_Player[g.m_PlayerManager.m_MedBayAlienId].DoMedbayScare();
				g.m_CameraManager.m_LookAtPlayerId = g.m_PlayerManager.m_MedBayAlienId;
				break;
			}
			g.m_ItemManager.m_Item[g.m_ItemManager.m_DoorItemId[3]].PeerUseDoor(m_NetId);
			PointLight obj2 = null;
			g.m_App.sceneInterface.LightManager.Find<PointLight>("BioScanLight", onlysearchdynamicobjects: false, out obj2);
			g.m_ItemManager.m_Item[g.m_ItemManager.m_DoorItemId[3]].m_DoorLight = obj2;
			string name = "TriggerDoor4";
			if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
			{
				obj.m_Complete = true;
			}
			break;
		}
		case TRIGGERS.DOOR5:
		{
			g.m_ItemManager.m_Item[g.m_ItemManager.m_DoorItemId[4]].PeerUseDoor(m_NetId);
			string name = "TriggerDoor5";
			if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
			{
				if (m_OceanSFX != null)
				{
					m_OceanSFX.Stop();
					m_OceanSFX = null;
				}
				obj.m_Complete = true;
			}
			break;
		}
		case TRIGGERS.OXYGEN2:
		{
			string name = "TriggerO2Full2";
			if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && !obj.m_Complete)
			{
				HallucinateOff(rumble: true);
				obj.m_Complete = true;
				g.m_ItemManager.m_Item[g.m_ItemManager.m_DoorItemId[4]].PeerCloseDoor();
			}
			break;
		}
		case TRIGGERS.DOOR6:
		{
			g.m_ItemManager.m_Item[g.m_ItemManager.m_DoorItemId[5]].PeerUseDoor(m_NetId);
			string name = "TriggerDoor6";
			if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
			{
				obj.m_Complete = true;
			}
			break;
		}
		case TRIGGERS.SCREWDRIVER:
			InteractScrewdriver();
			break;
		case TRIGGERS.DOOR7:
			if (m_State == STATE.InGame && m_DoorCrewExitUnlocked)
			{
				g.m_ItemManager.m_Item[g.m_ItemManager.m_DoorItemId[6]].PeerUseDoor(m_NetId);
				string name = "TriggerDoor7";
				if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
				{
					obj.m_Complete = true;
				}
				m_TabletsCollected = 0;
			}
			break;
		case TRIGGERS.LCD4:
			InteractLCD4();
			break;
		case TRIGGERS.DOOR8:
			if (m_State == STATE.InGame && m_TabletsCollected == 5)
			{
				g.m_ItemManager.m_Item[g.m_ItemManager.m_DoorItemId[7]].PeerUseDoor(m_NetId);
				string name = "TriggerDoor8";
				if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
				{
					obj.m_Complete = true;
				}
			}
			break;
		case TRIGGERS.RESEARCH1:
		{
			Item item5 = g.m_ItemManager.FindObjectByType(27);
			g.m_ItemManager.Delete(item5.m_Id);
			m_TabletsCollected++;
			string s5 = $"Found {m_TabletsCollected} of 5";
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.RESEARCHFOUND, s5, new Vector2(g.m_App.GetHudCentreX(s5, g.m_App.hudFont), 400f), 15f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			string name = "TriggerResearch1";
			if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
			{
				obj.m_Complete = true;
			}
			break;
		}
		case TRIGGERS.RESEARCH2:
		{
			Item item4 = g.m_ItemManager.FindObjectByType(28);
			g.m_ItemManager.Delete(item4.m_Id);
			m_TabletsCollected++;
			string s4 = $"Found {m_TabletsCollected} of 5";
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.RESEARCHFOUND, s4, new Vector2(g.m_App.GetHudCentreX(s4, g.m_App.hudFont), 400f), 15f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			string name = "TriggerResearch2";
			if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
			{
				obj.m_Complete = true;
			}
			break;
		}
		case TRIGGERS.RESEARCH3:
		{
			Item item3 = g.m_ItemManager.FindObjectByType(29);
			g.m_ItemManager.Delete(item3.m_Id);
			m_TabletsCollected++;
			string s3 = $"Found {m_TabletsCollected} of 5";
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.RESEARCHFOUND, s3, new Vector2(g.m_App.GetHudCentreX(s3, g.m_App.hudFont), 400f), 15f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			string name = "TriggerResearch3";
			if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
			{
				obj.m_Complete = true;
			}
			break;
		}
		case TRIGGERS.RESEARCH4:
		{
			Item item2 = g.m_ItemManager.FindObjectByType(30);
			g.m_ItemManager.Delete(item2.m_Id);
			m_TabletsCollected++;
			string s2 = $"Found {m_TabletsCollected} of 5";
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.RESEARCHFOUND, s2, new Vector2(g.m_App.GetHudCentreX(s2, g.m_App.hudFont), 400f), 15f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			string name = "TriggerResearch4";
			if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
			{
				obj.m_Complete = true;
			}
			break;
		}
		case TRIGGERS.RESEARCH5:
		{
			Item item = g.m_ItemManager.FindObjectByType(31);
			g.m_ItemManager.Delete(item.m_Id);
			m_TabletsCollected++;
			string s = $"Found {m_TabletsCollected} of 5";
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.RESEARCHFOUND, s, new Vector2(g.m_App.GetHudCentreX(s, g.m_App.hudFont), 400f), 15f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			string name = "TriggerResearch5";
			if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
			{
				obj.m_Complete = true;
			}
			break;
		}
		case TRIGGERS.DOOR9:
		{
			g.m_ItemManager.m_Item[g.m_ItemManager.m_DoorItemId[8]].PeerUseDoor(m_NetId);
			string name = "TriggerDoor9";
			if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
			{
				obj.m_Complete = true;
			}
			break;
		}
		case TRIGGERS.NONE:
		case TRIGGERS.LOCKER1:
		case TRIGGERS.LOCKER2:
		case TRIGGERS.LOCKER3:
		case TRIGGERS.LOCKER4:
			break;
		}
	}

	private void InteractLCD1()
	{
		if (m_State == STATE.InGame)
		{
			m_State = STATE.UsingLCD1;
			SetLCDMenu(LCDMenu.MAIN_MENU1);
			ClearMovement();
			m_LCDCursorRow = 0;
			return;
		}
		switch (m_LCDMenu)
		{
		case LCDMenu.MAIN_MENU1:
			if (m_LCDCursorRow == 0)
			{
				SetLCDMenu(LCDMenu.CAPTAIN1);
			}
			else if (m_LCDCursorRow == 1)
			{
				SetLCDMenu(LCDMenu.HOLTZ1);
			}
			else if (m_LCDCursorRow == 2)
			{
				SetLCDMenu(LCDMenu.SIMMONS1);
			}
			else if (m_LCDCursorRow == 3)
			{
				SetLCDMenu(LCDMenu.SOSSYSTEM);
			}
			g.m_SoundManager.Play(19);
			break;
		case LCDMenu.SOSSYSTEM:
			m_Password[m_PasswordIndex] = 1;
			if (m_PasswordIndex < 3)
			{
				m_PasswordIndex++;
			}
			else
			{
				CheckPassword();
			}
			g.m_SoundManager.Play(19);
			break;
		}
	}

	private void InteractLCD2()
	{
		if (m_State == STATE.InGame)
		{
			m_State = STATE.UsingLCD2;
			SetLCDMenu(LCDMenu.MAIN_MENU2);
			ClearMovement();
			m_LCDCursorRow = 0;
			return;
		}
		switch (m_LCDMenu)
		{
		case LCDMenu.MAIN_MENU2:
			if (m_LCDCursorRow == 0)
			{
				SetLCDMenu(LCDMenu.CAPTAIN2);
			}
			else if (m_LCDCursorRow == 1)
			{
				SetLCDMenu(LCDMenu.HOLTZ2);
			}
			else if (m_LCDCursorRow == 2)
			{
				SetLCDMenu(LCDMenu.SIMMONS2);
			}
			else if (m_LCDCursorRow == 3)
			{
				SetLCDMenu(LCDMenu.DOORSYSTEM);
				m_LCDCursorRow = 0;
			}
			g.m_SoundManager.Play(19);
			break;
		case LCDMenu.DOORSYSTEM:
			if (m_LCDCursorRow == 0)
			{
				SetLCDMenu(LCDMenu.DOORPASSWORD);
			}
			else if (m_LCDCursorRow == 1)
			{
				SetLCDMenu(LCDMenu.SECURITYQUESTION);
			}
			g.m_SoundManager.Play(19);
			break;
		case LCDMenu.DOORPASSWORD:
			m_Password[m_PasswordIndex] = 1;
			if (m_PasswordIndex < 3)
			{
				m_PasswordIndex++;
			}
			else
			{
				ResetPassword();
			}
			g.m_SoundManager.Play(19);
			break;
		case LCDMenu.SECURITYQUESTION:
			if (!m_SecurityQuestionAnswered)
			{
				string text = "QWERTYUIOPASDFGHJKLZXCVBNM";
				int num = 0;
				if (m_KeyboardRow == 1)
				{
					num = 10;
				}
				if (m_KeyboardRow == 2)
				{
					num = 19;
				}
				m_SecurityQuestion[m_SecurityQuestionIndex] = (byte)text[num + m_KeyboardCol];
				m_SecurityQuestionWrong = false;
				if (m_SecurityQuestionIndex < 6)
				{
					m_SecurityQuestionIndex++;
				}
				else
				{
					CheckSecurityQuestion();
				}
				g.m_SoundManager.Play(34);
			}
			break;
		case LCDMenu.CAPTAIN2:
		case LCDMenu.HOLTZ2:
		case LCDMenu.SIMMONS2:
			break;
		}
	}

	private void InteractLCD3()
	{
		if (m_State == STATE.InGame)
		{
			m_State = STATE.UsingLCD3;
			SetLCDMenu(LCDMenu.MAIN_MENU3);
			ClearMovement();
			m_LCDCursorRow = 0;
			return;
		}
		g.m_ItemManager.m_Item[g.m_ItemManager.m_DoorItemId[2]].PeerCloseDoor();
		switch (m_LCDMenu)
		{
		case LCDMenu.MAIN_MENU3:
			if (m_LCDCursorRow == 0)
			{
				SetLCDMenu(LCDMenu.CAPTAIN3);
			}
			else if (m_LCDCursorRow == 1)
			{
				SetLCDMenu(LCDMenu.HOLTZ3);
			}
			else if (m_LCDCursorRow == 2)
			{
				SetLCDMenu(LCDMenu.SIMMONS3);
			}
			else if (m_LCDCursorRow == 3)
			{
				SetLCDMenu(LCDMenu.MEDICALNOTES);
				m_LCDCursorRow = 0;
			}
			g.m_SoundManager.Play(19);
			break;
		case LCDMenu.MEDICALNOTES:
			if (m_LCDCursorRow == 0)
			{
				SetLCDMenu(LCDMenu.MEDICALNOTE1);
			}
			else if (m_LCDCursorRow == 1)
			{
				SetLCDMenu(LCDMenu.MEDICALNOTE2);
			}
			else if (m_LCDCursorRow == 2)
			{
				SetLCDMenu(LCDMenu.MEDICALNOTE3);
			}
			g.m_SoundManager.Play(19);
			break;
		}
	}

	public void InteractSafe()
	{
		if (m_State == STATE.InGame)
		{
			m_State = STATE.UsingSafe;
			ClearMovement();
			m_KeyboardRow = 1;
			m_KeyboardCol = 1;
		}
		else if (!m_SafeOpened)
		{
			string text = "789456123";
			int num = 0;
			if (m_KeyboardRow == 1)
			{
				num = 3;
			}
			if (m_KeyboardRow == 2)
			{
				num = 6;
			}
			m_SafePassword[m_SafePasswordIndex] = (byte)text[num + m_KeyboardCol];
			if (m_SafePasswordIndex < 3)
			{
				m_SafePasswordIndex++;
			}
			else
			{
				CheckSafePassword();
			}
			g.m_SoundManager.Play(34);
		}
	}

	public void InteractSaw()
	{
		Item item = g.m_ItemManager.FindObjectByType(22);
		if (item != null)
		{
			item.Hide();
			GiveWeapon(item.m_Id);
			string name = "TriggerSafe";
			MiscTriggerEntity obj = null;
			if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
			{
				obj.m_Enabled = false;
			}
			string s = "PRESS [Y] TO CHANGE TOOLS";
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.PICKUP_SAW, s, new Vector2(g.m_App.GetHudCentreX(s, g.m_App.hudFont), 120f), 7f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
		}
	}

	public void InteractArm()
	{
		Item item = g.m_ItemManager.FindObjectByType(19);
		if (item != null)
		{
			item.Hide();
			GiveWeapon(item.m_Id);
			string name = "TriggerArm";
			MiscTriggerEntity obj = null;
			if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
			{
				obj.m_Enabled = false;
			}
			string s = "PRESS [Y] TO CHANGE TOOLS";
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.PICKUP_SAW, s, new Vector2(g.m_App.GetHudCentreX(s, g.m_App.hudFont), 120f), 7f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
		}
	}

	public void InteractBioScan()
	{
		if (!m_DoorMedbayUnlocked)
		{
			if (m_WeaponItemIndex != -1 && g.m_ItemManager.m_Item[m_WeaponItemIndex].m_Type == 19)
			{
				PlayViewAnim(4, loop: false, 0.2f);
			}
			else
			{
				PlayViewAnim(5, loop: false, 0.2f);
			}
		}
	}

	public void UseBioScan()
	{
		if (m_NearTrigger == TRIGGERS.BIOSCAN && m_WeaponItemIndex != -1 && g.m_ItemManager.m_Item[m_WeaponItemIndex].m_Type == 19)
		{
			m_DoorMedbayUnlocked = true;
			string s = "BIOSCAN SUCCESS";
			g.m_App.RemoveHelmetMessage(HelmetMessage.TYPE.BIOSCAN_RESULT);
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.BIOSCAN_RESULT, s, new Vector2(g.m_App.GetHudCentreX(s, g.m_App.hudFont), 120f), 3f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			g.m_SoundManager.Play(30);
			string name = "TriggerBioScan";
			MiscTriggerEntity obj = null;
			if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
			{
				obj.m_Complete = true;
			}
			name = "TriggerDoor4";
			obj = null;
			if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
			{
				obj.m_NextUseTime = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 4f;
			}
		}
		else
		{
			string s2 = "BIOSCAN FAILED";
			g.m_App.RemoveHelmetMessage(HelmetMessage.TYPE.BIOSCAN_RESULT);
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.BIOSCAN_RESULT, s2, new Vector2(g.m_App.GetHudCentreX(s2, g.m_App.hudFont), 120f), 3f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			string name2 = "TriggerBioScan";
			MiscTriggerEntity obj2 = null;
			if (g.m_App.sceneInterface.ObjectManager.Find(name2, onlysearchdynamicobjects: false, out obj2))
			{
				obj2.m_NextUseTime = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 4f;
			}
		}
	}

	public void InteractScrewdriver()
	{
		Item item = g.m_ItemManager.FindObjectByType(25);
		if (item != null)
		{
			item.Hide();
			GiveWeapon(item.m_Id);
			string name = "TriggerScrewdriver";
			MiscTriggerEntity obj = null;
			if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
			{
				obj.m_Enabled = false;
			}
			string s = "PRESS [Y] TO CHANGE TOOLS";
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.PICKUP_SAW, s, new Vector2(g.m_App.GetHudCentreX(s, g.m_App.hudFont), 120f), 7f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
		}
	}

	private void UseScrewdriver()
	{
		switch (m_UsingTrigger)
		{
		case TRIGGERS.LOCKER1:
			g.m_ItemManager.m_Item[g.m_ItemManager.m_Locker1Id].PeerUseLocker();
			break;
		case TRIGGERS.LOCKER2:
			g.m_ItemManager.m_Item[g.m_ItemManager.m_Locker2Id].PeerUseLocker();
			g.m_SoundManager.Play(30);
			break;
		case TRIGGERS.LOCKER3:
			g.m_ItemManager.m_Item[g.m_ItemManager.m_Locker3Id].PeerUseLocker();
			break;
		case TRIGGERS.LOCKER4:
			g.m_ItemManager.m_Item[g.m_ItemManager.m_Locker4Id].PeerUseLocker();
			break;
		}
		m_UsingTrigger = TRIGGERS.NONE;
	}

	private void InteractLCD4()
	{
		if (m_State == STATE.InGame)
		{
			m_State = STATE.UsingLCD4;
			SetLCDMenu(LCDMenu.MAIN_MENU4);
			ClearMovement();
			m_LCDCursorRow = 0;
			return;
		}
		g.m_ItemManager.m_Item[g.m_ItemManager.m_DoorItemId[5]].PeerCloseDoor();
		switch (m_LCDMenu)
		{
		case LCDMenu.MAIN_MENU4:
			if (m_LCDCursorRow == 0)
			{
				SetLCDMenu(LCDMenu.CAPTAIN4);
			}
			else if (m_LCDCursorRow == 1)
			{
				SetLCDMenu(LCDMenu.HOLTZ4);
			}
			else if (m_LCDCursorRow == 2)
			{
				SetLCDMenu(LCDMenu.SIMMONS4);
			}
			else if (m_LCDCursorRow == 3)
			{
				SetLCDMenu(LCDMenu.NETWORKMENU);
				m_LCDCursorRow = 0;
			}
			g.m_SoundManager.Play(19);
			break;
		case LCDMenu.NETWORKMENU:
			if (!m_NetworkPasswordAnswered)
			{
				string text2 = "QWERTYUIOPASDFGHJKLZXCVBNM";
				int num2 = 0;
				if (m_KeyboardRow == 1)
				{
					num2 = 10;
				}
				if (m_KeyboardRow == 2)
				{
					num2 = 19;
				}
				m_NetworkPassword[m_NetworkPasswordIndex] = (byte)text2[num2 + m_KeyboardCol];
				if (m_NetworkPasswordIndex < 6)
				{
					m_NetworkPasswordIndex++;
				}
				else
				{
					CheckNetworkPassword();
				}
				g.m_SoundManager.Play(34);
			}
			else
			{
				if (m_LCDCursorRow == 0)
				{
					SetLCDMenu(LCDMenu.PING);
				}
				else if (m_LCDCursorRow == 1)
				{
					SetLCDMenu(LCDMenu.BRINGONLINE);
				}
				g.m_SoundManager.Play(19);
			}
			break;
		case LCDMenu.PING:
			switch (m_LCDCursorRow)
			{
			case 0:
				SetLCDMenu(LCDMenu.PINGDOOR1);
				break;
			case 1:
				SetLCDMenu(LCDMenu.PINGDOOR2);
				break;
			case 2:
				SetLCDMenu(LCDMenu.PINGDOOR3);
				break;
			case 3:
				SetLCDMenu(LCDMenu.PINGDOOR4);
				break;
			case 4:
				SetLCDMenu(LCDMenu.PINGDOOR5);
				break;
			}
			break;
		case LCDMenu.BRINGONLINE:
		{
			string text = "789456123";
			int num = 0;
			if (m_KeyboardRow == 1)
			{
				num = 3;
			}
			if (m_KeyboardRow == 2)
			{
				num = 6;
			}
			m_IPAddress[m_IPAddressIndex] = (byte)text[num + m_KeyboardCol];
			if (m_IPAddressIndex < 2)
			{
				m_IPAddressIndex++;
			}
			else
			{
				CheckIPAddress();
			}
			g.m_SoundManager.Play(34);
			break;
		}
		case LCDMenu.CAPTAIN4:
		case LCDMenu.HOLTZ4:
		case LCDMenu.SIMMONS4:
			break;
		}
	}

	public void InteractX()
	{
		switch (m_NearTrigger)
		{
		case TRIGGERS.LCD1:
			if (m_LCDMenu == LCDMenu.SOSSYSTEM)
			{
				m_Password[m_PasswordIndex] = 2;
				if (m_PasswordIndex < 3)
				{
					m_PasswordIndex++;
				}
				else
				{
					CheckPassword();
				}
				g.m_SoundManager.Play(19);
			}
			break;
		case TRIGGERS.LCD2:
			if (m_LCDMenu == LCDMenu.DOORPASSWORD)
			{
				m_Password[m_PasswordIndex] = 2;
				if (m_PasswordIndex < 3)
				{
					m_PasswordIndex++;
				}
				else if (m_SecurityQuestionAnswered)
				{
					CheckPassword2();
				}
				else
				{
					ResetPassword();
				}
				g.m_SoundManager.Play(19);
			}
			else if (m_LCDMenu == LCDMenu.SECURITYQUESTION && m_SecurityQuestionIndex > 0)
			{
				m_SecurityQuestionIndex--;
				m_SecurityQuestion[m_SecurityQuestionIndex] = 0;
			}
			break;
		case TRIGGERS.SAFE:
			if (m_SafePasswordIndex > 0)
			{
				m_SafePasswordIndex--;
				m_SafePassword[m_SafePasswordIndex] = 0;
			}
			break;
		case TRIGGERS.LCD4:
			if (m_LCDMenu == LCDMenu.NETWORKMENU)
			{
				if (m_NetworkPasswordIndex > 0)
				{
					m_NetworkPasswordIndex--;
					m_NetworkPassword[m_NetworkPasswordIndex] = 0;
				}
			}
			else if (m_LCDMenu == LCDMenu.BRINGONLINE && m_IPAddressIndex > 0)
			{
				m_IPAddressIndex--;
				m_IPAddress[m_IPAddressIndex] = 0;
			}
			break;
		}
	}

	public bool InteractY()
	{
		switch (m_NearTrigger)
		{
		case TRIGGERS.LCD1:
			if (m_LCDMenu == LCDMenu.SOSSYSTEM)
			{
				m_Password[m_PasswordIndex] = 3;
				if (m_PasswordIndex < 3)
				{
					m_PasswordIndex++;
				}
				else
				{
					CheckPassword();
				}
				g.m_SoundManager.Play(19);
			}
			return true;
		case TRIGGERS.LCD2:
			if (m_LCDMenu == LCDMenu.DOORPASSWORD)
			{
				m_Password[m_PasswordIndex] = 3;
				if (m_PasswordIndex < 3)
				{
					m_PasswordIndex++;
				}
				else
				{
					ResetPassword();
				}
				g.m_SoundManager.Play(19);
			}
			return true;
		default:
			return false;
		}
	}

	public void InteractBack()
	{
		switch (m_NearTrigger)
		{
		case TRIGGERS.LCD1:
			InteractBackLCD1();
			break;
		case TRIGGERS.LCD2:
			InteractBackLCD2();
			break;
		case TRIGGERS.LCD3:
			InteractBackLCD3();
			break;
		case TRIGGERS.SAFE:
			InteractBackSafe();
			break;
		case TRIGGERS.LCD4:
			InteractBackLCD4();
			break;
		}
	}

	private void InteractBackLCD1()
	{
		g.m_SoundManager.Play(19);
		switch (m_LCDMenu)
		{
		case LCDMenu.MAIN_MENU1:
			if (!m_Door0Unlocked)
			{
				SetLCDMenu(LCDMenu.SHOW_SOS);
			}
			else
			{
				SetLCDMenu(LCDMenu.SCREENSAVER);
			}
			m_State = STATE.InGame;
			m_NearTrigger = TRIGGERS.NONE;
			if (g.m_App.SOUNDON)
			{
				MediaPlayer.Play(g.m_App.m_Level1Music);
				MediaPlayer.IsRepeating = true;
				MediaPlayer.Volume = g.m_App.m_OptionsMusicVol;
			}
			if (m_AmbienceSFX != null)
			{
				m_AmbienceSFX.Stop();
				m_AmbienceSFX = null;
			}
			break;
		case LCDMenu.CAPTAIN1:
		case LCDMenu.HOLTZ1:
		case LCDMenu.SIMMONS1:
			SetLCDMenu(LCDMenu.MAIN_MENU1);
			MediaPlayer.Stop();
			break;
		case LCDMenu.SOSSYSTEM:
			SetLCDMenu(LCDMenu.MAIN_MENU1);
			break;
		}
	}

	private void InteractBackLCD2()
	{
		g.m_SoundManager.Play(19);
		switch (m_LCDMenu)
		{
		case LCDMenu.MAIN_MENU2:
			m_State = STATE.InGame;
			m_NearTrigger = TRIGGERS.NONE;
			SetLCDMenu(LCDMenu.SCREENSAVER);
			if (g.m_App.SOUNDON)
			{
				MediaPlayer.Play(g.m_App.m_Level1Music);
				MediaPlayer.IsRepeating = true;
				MediaPlayer.Volume = g.m_App.m_OptionsMusicVol;
			}
			if (m_AmbienceSFX != null)
			{
				m_AmbienceSFX.Stop();
				m_AmbienceSFX = null;
			}
			break;
		case LCDMenu.CAPTAIN2:
		case LCDMenu.HOLTZ2:
		case LCDMenu.SIMMONS2:
			SetLCDMenu(LCDMenu.MAIN_MENU2);
			MediaPlayer.Stop();
			break;
		case LCDMenu.DOORSYSTEM:
			SetLCDMenu(LCDMenu.MAIN_MENU2);
			break;
		case LCDMenu.DOORPASSWORD:
		case LCDMenu.SECURITYQUESTION:
			SetLCDMenu(LCDMenu.DOORSYSTEM);
			break;
		}
	}

	private void InteractBackLCD3()
	{
		g.m_SoundManager.Play(19);
		switch (m_LCDMenu)
		{
		case LCDMenu.MAIN_MENU3:
			m_State = STATE.InGame;
			m_NearTrigger = TRIGGERS.NONE;
			SetLCDMenu(LCDMenu.SCREENSAVER);
			if (g.m_App.SOUNDON)
			{
				MediaPlayer.Play(g.m_App.m_Level1Music);
				MediaPlayer.IsRepeating = true;
				MediaPlayer.Volume = g.m_App.m_OptionsMusicVol;
			}
			if (m_AmbienceSFX != null)
			{
				m_AmbienceSFX.Stop();
				m_AmbienceSFX = null;
			}
			break;
		case LCDMenu.CAPTAIN3:
		case LCDMenu.HOLTZ3:
		case LCDMenu.SIMMONS3:
			SetLCDMenu(LCDMenu.MAIN_MENU3);
			MediaPlayer.Stop();
			break;
		case LCDMenu.MEDICALNOTES:
			SetLCDMenu(LCDMenu.MAIN_MENU3);
			break;
		case LCDMenu.MEDICALNOTE1:
		case LCDMenu.MEDICALNOTE2:
		case LCDMenu.MEDICALNOTE3:
			SetLCDMenu(LCDMenu.MEDICALNOTES);
			break;
		}
	}

	private void InteractBackSafe()
	{
		g.m_SoundManager.Play(19);
		m_State = STATE.InGame;
		m_NearTrigger = TRIGGERS.NONE;
	}

	private void InteractBackLCD4()
	{
		g.m_SoundManager.Play(19);
		switch (m_LCDMenu)
		{
		case LCDMenu.MAIN_MENU4:
			m_State = STATE.InGame;
			m_NearTrigger = TRIGGERS.NONE;
			SetLCDMenu(LCDMenu.SCREENSAVER);
			if (g.m_App.SOUNDON)
			{
				MediaPlayer.Play(g.m_App.m_Level1Music);
				MediaPlayer.IsRepeating = true;
				MediaPlayer.Volume = g.m_App.m_OptionsMusicVol;
			}
			if (m_AmbienceSFX != null)
			{
				m_AmbienceSFX.Stop();
				m_AmbienceSFX = null;
			}
			break;
		case LCDMenu.CAPTAIN4:
		case LCDMenu.HOLTZ4:
		case LCDMenu.SIMMONS4:
			SetLCDMenu(LCDMenu.MAIN_MENU4);
			MediaPlayer.Stop();
			break;
		case LCDMenu.NETWORKMENU:
			SetLCDMenu(LCDMenu.MAIN_MENU4);
			break;
		case LCDMenu.PING:
		case LCDMenu.BRINGONLINE:
			SetLCDMenu(LCDMenu.NETWORKMENU);
			m_LCDCursorRow = 0;
			break;
		case LCDMenu.PINGDOOR1:
		case LCDMenu.PINGDOOR2:
		case LCDMenu.PINGDOOR3:
		case LCDMenu.PINGDOOR4:
		case LCDMenu.PINGDOOR5:
			SetLCDMenu(LCDMenu.PING);
			break;
		}
	}

	public void SetLCDMenu(LCDMenu newMenu)
	{
		if (m_LCDMenu == newMenu)
		{
			return;
		}
		switch (m_LCDMenu)
		{
		case LCDMenu.SHOW_SOS:
		case LCDMenu.SHOW_SOS_GLITCH1:
		case LCDMenu.SHOW_SOS_GLITCH2:
		case LCDMenu.SHOW_SOS_GLITCH1_IN:
			if (g.m_App.m_GameplayScreen != null && g.m_App.m_GameplayScreen.m_LCDRedLight != null)
			{
				g.m_App.m_GameplayScreen.m_LCDRedLight.Enabled = false;
			}
			break;
		case LCDMenu.CAPTAIN1:
		case LCDMenu.HOLTZ1:
		case LCDMenu.SIMMONS1:
		case LCDMenu.CAPTAIN2:
		case LCDMenu.HOLTZ2:
		case LCDMenu.SIMMONS2:
		case LCDMenu.CAPTAIN3:
		case LCDMenu.HOLTZ3:
		case LCDMenu.CAPTAIN4:
		case LCDMenu.HOLTZ4:
			m_ShowAudioDisplay = false;
			MediaPlayer.IsVisualizationEnabled = false;
			break;
		}
		m_LCDMenu = newMenu;
		switch (m_LCDMenu)
		{
		case LCDMenu.CAPTAIN1:
			m_AudioLog = g.m_App.Content.Load<Song>("Music/captain_log1");
			MediaPlayer.Play(m_AudioLog);
			MediaPlayer.Volume = g.m_App.m_OptionsMusicVol;
			HandleAmbienceAndDisplay();
			break;
		case LCDMenu.HOLTZ1:
			m_AudioLog = g.m_App.Content.Load<Song>("Music/holtz_log1");
			MediaPlayer.Play(m_AudioLog);
			MediaPlayer.Volume = g.m_App.m_OptionsMusicVol;
			HandleAmbienceAndDisplay();
			break;
		case LCDMenu.SIMMONS1:
			m_AudioLog = g.m_App.Content.Load<Song>("Music/engineer_log1");
			MediaPlayer.Play(m_AudioLog);
			MediaPlayer.Volume = g.m_App.m_OptionsMusicVol;
			HandleAmbienceAndDisplay();
			break;
		case LCDMenu.SOSSYSTEM:
			ResetPassword();
			break;
		case LCDMenu.CAPTAIN2:
			m_AudioLog = g.m_App.Content.Load<Song>("Music/captain_log2");
			MediaPlayer.Play(m_AudioLog);
			MediaPlayer.Volume = g.m_App.m_OptionsMusicVol;
			HandleAmbienceAndDisplay();
			break;
		case LCDMenu.HOLTZ2:
			m_AudioLog = g.m_App.Content.Load<Song>("Music/holtz_log2");
			MediaPlayer.Play(m_AudioLog);
			MediaPlayer.Volume = g.m_App.m_OptionsMusicVol;
			HandleAmbienceAndDisplay();
			break;
		case LCDMenu.DOORPASSWORD:
			ResetPassword();
			break;
		case LCDMenu.CAPTAIN3:
			m_AudioLog = g.m_App.Content.Load<Song>("Music/captain_log3");
			MediaPlayer.Play(m_AudioLog);
			MediaPlayer.Volume = g.m_App.m_OptionsMusicVol;
			HandleAmbienceAndDisplay();
			break;
		case LCDMenu.HOLTZ3:
			m_AudioLog = g.m_App.Content.Load<Song>("Music/holtz_log3");
			MediaPlayer.Play(m_AudioLog);
			MediaPlayer.Volume = g.m_App.m_OptionsMusicVol;
			HandleAmbienceAndDisplay();
			break;
		case LCDMenu.CAPTAIN4:
			m_AudioLog = g.m_App.Content.Load<Song>("Music/captain_log4");
			MediaPlayer.Play(m_AudioLog);
			MediaPlayer.Volume = g.m_App.m_OptionsMusicVol;
			HandleAmbienceAndDisplay();
			break;
		case LCDMenu.HOLTZ4:
			m_AudioLog = g.m_App.Content.Load<Song>("Music/holtz_log4");
			MediaPlayer.Play(m_AudioLog);
			MediaPlayer.Volume = g.m_App.m_OptionsMusicVol;
			HandleAmbienceAndDisplay();
			break;
		case LCDMenu.NETWORKMENU:
			ResetNetworkPassword();
			break;
		case LCDMenu.MAIN_MENU1:
		case LCDMenu.MAIN_MENU2:
		case LCDMenu.SIMMONS2:
		case LCDMenu.DOORSYSTEM:
		case LCDMenu.SECURITYQUESTION:
		case LCDMenu.MAIN_MENU3:
		case LCDMenu.SIMMONS3:
		case LCDMenu.MEDICALNOTES:
		case LCDMenu.MEDICALNOTE1:
		case LCDMenu.MEDICALNOTE2:
		case LCDMenu.MEDICALNOTE3:
		case LCDMenu.MAIN_MENU4:
		case LCDMenu.SIMMONS4:
			break;
		}
	}

	private void HandleAmbienceAndDisplay()
	{
		MediaPlayer.IsRepeating = false;
		m_ShowAudioDisplay = true;
		MediaPlayer.IsVisualizationEnabled = true;
		if (m_AmbienceSFX == null)
		{
			m_AmbienceSFX = g.m_SoundManager.PlayLooped(26);
		}
	}

	public void UpdateUsingLCD1()
	{
		if (m_LCDMenu == LCDMenu.SOSSYSTEM)
		{
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD11, "[A][X][Y]-ENTER PASSWORD", new Vector2(30f, 335f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD12, "[B]-EXIT", new Vector2(30f, 355f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
		}
		else
		{
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD11, "[A]-SELECT", new Vector2(30f, 335f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD12, "[B]-EXIT", new Vector2(30f, 355f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD13, "LEFT STICK-UP/DOWN", new Vector2(30f, 375f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
		}
		string s = "COMPUTER LINK ACTIVE";
		g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD14, s, new Vector2(g.m_App.GetHudCentreX(s, g.m_App.hudFont), 90f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.TextPrompt, g.m_App.hudFont, hideSysMsg: true);
		UpdatePlayerUsingLCD();
		float num = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds;
		LCDMenu lCDMenu = m_LCDMenu;
		if (lCDMenu == LCDMenu.MAIN_MENU1 && m_LCDCursorMoveTimer < num)
		{
			if (m_Movement.Y < -0.5f && m_LCDCursorRow > 0)
			{
				m_LCDCursorRow--;
				m_LCDCursorMoveTimer = num + 0.25f;
				m_LCDCursorOn = true;
				m_LCDCursorBlink = 0;
				g.m_SoundManager.Play(19);
			}
			if (m_Movement.Y > 0.5f && m_LCDCursorRow < 3)
			{
				m_LCDCursorRow++;
				m_LCDCursorMoveTimer = num + 0.25f;
				m_LCDCursorOn = true;
				m_LCDCursorBlink = 0;
				g.m_SoundManager.Play(19);
			}
		}
	}

	public void DrawLCD()
	{
		m_LCDCursorBlink++;
		if (m_LCDCursorBlink > 15)
		{
			m_LCDCursorOn = !m_LCDCursorOn;
			m_LCDCursorBlink = 0;
		}
		switch (m_State)
		{
		case STATE.UsingLCD1:
			DrawLCD1();
			return;
		case STATE.UsingLCD2:
			DrawLCD2();
			return;
		case STATE.UsingLCD3:
			DrawLCD3();
			return;
		case STATE.UsingLCD4:
			DrawLCD4();
			return;
		}
		if (!m_Door0Unlocked)
		{
			DrawLCD1();
		}
		else
		{
			DrawScreenSaver();
		}
	}

	public void DrawLCD1()
	{
		float num = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds;
		for (int i = 0; i < 8; i++)
		{
			g.m_App.GraphicsDevice.SamplerStates[i] = SamplerState.PointClamp;
		}
		g.m_App.GraphicsDevice.SetRenderTarget(g.m_App.m_LCD1RenderTarget);
		g.m_App.screenManager.SpriteBatch.Begin();
		g.m_App.GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 1f, 0);
		Vector2 position = new Vector2(0f, 0f);
		Color lCDGreen = LCDGreen;
		int num2 = 40;
		int num3 = 32;
		SpriteFont lcdFont = g.m_App.lcdFont;
		switch (m_LCDMenu)
		{
		case LCDMenu.MAIN_MENU1:
		{
			Vector2 vector = new Vector2(10f, 70f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "MAIN SYSTEM MENU", vector + new Vector2(90f, 0f), lCDGreen);
			vector.Y += 80f;
			if (m_LCDCursorOn)
			{
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, ">", vector + new Vector2(0f, m_LCDCursorRow * num2), Color.White);
			}
			vector.X += 20f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Captain Meyer Log Entry #1", vector, lCDGreen);
			vector.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Dr Holtz Log Entry #1", vector, lCDGreen);
			vector.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Engineer Simmons Log Entry #1", vector, lCDGreen);
			vector.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "SOS System Control", vector, lCDGreen);
			break;
		}
		case LCDMenu.CAPTAIN1:
		{
			Vector2 vector2 = new Vector2(10f, 10f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "CAPTAINS LOG #1", vector2 + new Vector2(100f, 0f), lCDGreen);
			vector2.Y += 50f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "We arrived at the research site on schedule", vector2, lCDGreen);
			vector2.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "and without incident.", vector2, lCDGreen);
			vector2.Y += num3;
			vector2.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Dr Amanda Holtz and her assistant Samuel", vector2, lCDGreen);
			vector2.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Curtis are already analyzing their first", vector2, lCDGreen);
			vector2.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "batch of samples.", vector2, lCDGreen);
			vector2.Y += num3;
			vector2.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Engineer Frank Simmons has reported the", vector2, lCDGreen);
			vector2.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "successful installation of the new O2", vector2, lCDGreen);
			vector2.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "system.", vector2, lCDGreen);
			vector2.Y += num3;
			vector2.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "I've requested an A.B. from the Company.", vector2, lCDGreen);
			vector2.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "2 to 3 weeks should cover it.", vector2, lCDGreen);
			break;
		}
		case LCDMenu.HOLTZ1:
		{
			Vector2 vector5 = new Vector2(10f, 10f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "DR HOLTZ LOG #1", vector5 + new Vector2(100f, 0f), lCDGreen);
			vector5.Y += 50f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "We have arrived at the excavation site and ", vector5, lCDGreen);
			vector5.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "collected as many samples as daylight would", vector5, lCDGreen);
			vector5.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "allow. Samuel is cataloguing the first finds", vector5, lCDGreen);
			vector5.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "and I'm putting them through the mass-spec. ", vector5, lCDGreen);
			vector5.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Initial results are promising. I'll write", vector5, lCDGreen);
			vector5.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "up my prelim notes tonight.", vector5, lCDGreen);
			vector5.Y += num3;
			vector5.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "The rest of the crew seem ok - Captain's a bit", vector5, lCDGreen);
			vector5.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "stand-offish when I complained about the", vector5, lCDGreen);
			vector5.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "funny smell. The engineer fellow was a bit", vector5, lCDGreen);
			vector5.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "more helpful - said not to worry as was the", vector5, lCDGreen);
			vector5.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "new O2 system settling in.", vector5, lCDGreen);
			break;
		}
		case LCDMenu.SIMMONS1:
		{
			Vector2 vector4 = new Vector2(10f, 10f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "ENGINEER SIMMONS LOG #1", vector4 + new Vector2(100f, 0f), lCDGreen);
			vector4.Y += 50f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "I replaced the old Antisept Aeration System", vector4, lCDGreen);
			vector4.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "with brand new BiO2 aerators... those are a", vector4, lCDGreen);
			vector4.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "million times better.", vector4, lCDGreen);
			vector4.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "O2 levels now constant and I've installed", vector4, lCDGreen);
			vector4.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "surplus tanks on all decks as back-up.", vector4, lCDGreen);
			vector4.Y += num3;
			vector4.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "The Combi-plex Unit in Lenndrive 1 is reading", vector4, lCDGreen);
			vector4.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "a 0.003 differential from yesterday. Could", vector4, lCDGreen);
			vector4.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "be a possible crack - worst case scenario -", vector4, lCDGreen);
			vector4.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "so I'll do hourly checks.", vector4, lCDGreen);
			vector4.Y += num3;
			vector4.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "I've taken comms offline to rewire the", vector4, lCDGreen);
			vector4.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Anti-Stat. Will do a test message at 18:00 hours", vector4, lCDGreen);
			break;
		}
		case LCDMenu.SOSSYSTEM:
		{
			Vector2 vector3 = new Vector2(10f, 10f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "SOS SECURITY SYSTEM", vector3 + new Vector2(100f, 0f), lCDGreen);
			vector3.Y += 100f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Security Door Status:", vector3, lCDGreen);
			if (m_Door0Unlocked)
			{
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, " UNLOCKED", vector3 + new Vector2(220f, 0f), Color.White);
			}
			else
			{
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "LOCKED", vector3 + new Vector2(220f, 0f), Color.Red);
			}
			vector3.Y += num3;
			vector3.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Enter password below to disable the", vector3, lCDGreen);
			vector3.Y += num3;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "security system.", vector3, lCDGreen);
			vector3.Y += 100f;
			string text = "";
			for (int j = 0; j < 4; j++)
			{
				if (m_Password[j] == 0)
				{
					text += "- ";
				}
				if (m_Password[j] == 1)
				{
					text += "A ";
				}
				if (m_Password[j] == 2)
				{
					text += "X ";
				}
				if (m_Password[j] == 3)
				{
					text += "Y ";
				}
			}
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, text, vector3, lCDGreen);
			break;
		}
		case LCDMenu.SHOW_SOS:
			g.m_App.screenManager.SpriteBatch.Draw(g.m_App.m_SOSTexture, position, Color.White);
			if (m_LCDTimer < num)
			{
				m_LCDMenu = LCDMenu.SHOW_SOS_GLITCH1;
				m_LCDTimer = num + 0.1f;
			}
			break;
		case LCDMenu.SHOW_SOS_GLITCH1:
			g.m_App.screenManager.SpriteBatch.Draw(g.m_App.m_SOS1Texture, position, Color.White);
			if (m_LCDTimer < num)
			{
				m_LCDMenu = LCDMenu.SHOW_SOS_GLITCH2;
				m_LCDTimer = num + 0.1f;
			}
			break;
		case LCDMenu.SHOW_SOS_GLITCH2:
			g.m_App.screenManager.SpriteBatch.Draw(g.m_App.m_SOS2Texture, position, Color.White);
			if (m_LCDTimer < num)
			{
				m_LCDMenu = LCDMenu.OFF;
				m_LCDTimer = num + 1f;
				if (g.m_App.m_GameplayScreen != null && g.m_App.m_GameplayScreen.m_LCDRedLight != null)
				{
					g.m_App.m_GameplayScreen.m_LCDRedLight.Enabled = false;
				}
			}
			break;
		case LCDMenu.OFF:
			if (m_LCDTimer < num)
			{
				m_LCDMenu = LCDMenu.SHOW_SOS_GLITCH1_IN;
				m_LCDTimer = num + 0.1f;
				if (g.m_App.m_GameplayScreen != null && g.m_App.m_GameplayScreen.m_LCDRedLight != null)
				{
					g.m_App.m_GameplayScreen.m_LCDRedLight.Enabled = true;
				}
			}
			break;
		case LCDMenu.SHOW_SOS_GLITCH1_IN:
			g.m_App.screenManager.SpriteBatch.Draw(g.m_App.m_SOS1Texture, position, Color.White);
			if (m_LCDTimer < num)
			{
				m_LCDMenu = LCDMenu.SHOW_SOS;
				m_LCDTimer = num + 1f;
			}
			break;
		}
		g.m_App.screenManager.SpriteBatch.End();
		g.m_App.GraphicsDevice.SetRenderTarget(null);
		if (g.m_App.m_LCD1 != null)
		{
			DeferredObjectEffect deferredObjectEffect = g.m_App.m_LCD1.RenderableMeshes[0].Effect as DeferredObjectEffect;
			deferredObjectEffect.DiffuseMapTexture = g.m_App.m_LCD1RenderTarget;
			deferredObjectEffect.SpecularAmount = 0.4f;
			deferredObjectEffect.SpecularPower = 10f;
			deferredObjectEffect.EmissiveColor = new Vector3(1f, 1f, 1f);
			deferredObjectEffect.TransparencyMode = TransparencyMode.None;
		}
	}

	public void DrawScreenSaver()
	{
		_ = g.m_App.m_GameTime.TotalGameTime.TotalSeconds;
		for (int i = 0; i < 8; i++)
		{
			g.m_App.GraphicsDevice.SamplerStates[i] = SamplerState.PointClamp;
		}
		g.m_App.GraphicsDevice.SetRenderTarget(g.m_App.m_LCD1RenderTarget);
		g.m_App.screenManager.SpriteBatch.Begin();
		g.m_App.GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 1f, 0);
		for (int num = 15; num > 0; num--)
		{
			ref Vector2 reference = ref m_LinePos[num];
			reference = m_LinePos[num - 1];
			m_LineRot[num] = m_LineRot[num - 1];
		}
		m_LinePos[0].X += m_LineDir.X * 5f;
		if (m_LinePos[0].X > 512f)
		{
			m_LineDir.X = -1f;
		}
		if (m_LinePos[0].X < 0f)
		{
			m_LineDir.X = 1f;
		}
		m_LinePos[0].Y += m_LineDir.Y * 6f;
		if (m_LinePos[0].Y > 512f)
		{
			m_LineDir.Y = -1f;
		}
		if (m_LinePos[0].Y < 0f)
		{
			m_LineDir.Y = 1f;
		}
		m_LineRot[0] += 0.1f;
		for (int j = 0; j < 16; j++)
		{
			g.m_App.screenManager.SpriteBatch.Draw(g.m_App.compassHTexture, new Rectangle((int)m_LinePos[j].X, (int)m_LinePos[j].Y, 100, 3), new Rectangle(0, 0, 100, 2), Color.Green * (1f - (float)j / 16f), m_LineRot[j], Vector2.Zero, SpriteEffects.None, 0f);
			g.m_App.screenManager.SpriteBatch.Draw(g.m_App.compassHTexture, new Rectangle(512 - (int)m_LinePos[j].X, 512 - (int)m_LinePos[j].Y, 100, 3), new Rectangle(0, 0, 100, 2), Color.LightBlue * (1f - (float)j / 16f), m_LineRot[j], Vector2.Zero, SpriteEffects.None, 0f);
		}
		g.m_App.screenManager.SpriteBatch.End();
		g.m_App.GraphicsDevice.SetRenderTarget(null);
		if (g.m_App.m_LCD2 != null)
		{
			DeferredObjectEffect deferredObjectEffect = g.m_App.m_LCD2.RenderableMeshes[0].Effect as DeferredObjectEffect;
			deferredObjectEffect.DiffuseMapTexture = g.m_App.m_LCD1RenderTarget;
			deferredObjectEffect.SpecularAmount = 0.4f;
			deferredObjectEffect.SpecularPower = 10f;
			deferredObjectEffect.EmissiveColor = new Vector3(1f, 1f, 1f);
			deferredObjectEffect.TransparencyMode = TransparencyMode.None;
		}
	}

	private void CheckPassword()
	{
		if (!m_Door0Unlocked)
		{
			if (m_Password[0] == 3 && m_Password[1] == 2 && m_Password[2] == 2 && m_Password[3] == 3)
			{
				m_Door0Unlocked = true;
				g.m_SoundManager.Play(30);
			}
			else
			{
				ResetPassword();
			}
		}
	}

	private void ResetPassword()
	{
		for (int i = 0; i < 4; i++)
		{
			m_Password[i] = 0;
		}
		m_PasswordIndex = 0;
	}

	public void UpdateUsingLCD2()
	{
		HELMET_KEYBOARD_POS.X = 60f;
		string text = "QWERTYUIOP";
		string text2 = "ASDFGHJKL";
		string text3 = "ZXCVBNM";
		if (m_LCDMenu == LCDMenu.SECURITYQUESTION)
		{
			if (!m_SecurityQuestionAnswered)
			{
				g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD11, "[A]-SELECT LETTER", new Vector2(30f, 335f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
				g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD12, "[B]-EXIT   [X]-DEL", new Vector2(30f, 355f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
				g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD13, "LEFT STICK-MOVE CURSOR", new Vector2(30f, 375f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
				for (int i = 0; i < 10; i++)
				{
					g.m_App.AddHelmetMessage((HelmetMessage.TYPE)(40 + i), text.Substring(i, 1), new Vector2(HELMET_KEYBOARD_POS.X + (float)(38 * i), HELMET_KEYBOARD_POS.Y), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
				}
				for (int j = 0; j < 9; j++)
				{
					g.m_App.AddHelmetMessage((HelmetMessage.TYPE)(50 + j), text2.Substring(j, 1), new Vector2(HELMET_KEYBOARD_POS.X + (float)(38 * j), HELMET_KEYBOARD_POS.Y + HELMET_KEYBOARD_Y), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
				}
				for (int k = 0; k < 7; k++)
				{
					g.m_App.AddHelmetMessage((HelmetMessage.TYPE)(59 + k), text3.Substring(k, 1), new Vector2(HELMET_KEYBOARD_POS.X + (float)(38 * k), HELMET_KEYBOARD_POS.Y + HELMET_KEYBOARD_Y * 2f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
				}
			}
			else
			{
				g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD12, "[B]-EXIT", new Vector2(30f, 360f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
			}
		}
		else if (m_LCDMenu == LCDMenu.DOORPASSWORD)
		{
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD11, "[A][X][Y]-ENTER PASSWORD", new Vector2(30f, 335f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD12, "[B]-EXIT", new Vector2(30f, 355f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
		}
		else
		{
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD11, "[A]-SELECT", new Vector2(30f, 335f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD12, "[B]-EXIT", new Vector2(30f, 355f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD13, "LEFT STICK-UP/DOWN", new Vector2(30f, 375f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
		}
		string s = "COMPUTER LINK ACTIVE";
		g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD14, s, new Vector2(g.m_App.GetHudCentreX(s, g.m_App.hudFont), 90f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.TextPrompt, g.m_App.hudFont, hideSysMsg: true);
		UpdatePlayerUsingLCD();
		float num = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds;
		switch (m_LCDMenu)
		{
		case LCDMenu.MAIN_MENU2:
			if (m_LCDCursorMoveTimer < num)
			{
				if (m_Movement.Y < -0.5f && m_LCDCursorRow > 0)
				{
					m_LCDCursorRow--;
					m_LCDCursorMoveTimer = num + 0.25f;
					m_LCDCursorOn = true;
					m_LCDCursorBlink = 0;
					g.m_SoundManager.Play(19);
				}
				if (m_Movement.Y > 0.5f && m_LCDCursorRow < 3)
				{
					m_LCDCursorRow++;
					m_LCDCursorMoveTimer = num + 0.25f;
					m_LCDCursorOn = true;
					m_LCDCursorBlink = 0;
					g.m_SoundManager.Play(19);
				}
			}
			break;
		case LCDMenu.DOORSYSTEM:
			if (m_LCDCursorMoveTimer < num)
			{
				if (m_Movement.Y < -0.5f && m_LCDCursorRow > 0)
				{
					m_LCDCursorRow--;
					m_LCDCursorMoveTimer = num + 0.25f;
					m_LCDCursorOn = true;
					m_LCDCursorBlink = 0;
					g.m_SoundManager.Play(19);
				}
				if (m_Movement.Y > 0.5f && m_LCDCursorRow < 1)
				{
					m_LCDCursorRow++;
					m_LCDCursorMoveTimer = num + 0.25f;
					m_LCDCursorOn = true;
					m_LCDCursorBlink = 0;
					g.m_SoundManager.Play(19);
				}
			}
			break;
		case LCDMenu.SECURITYQUESTION:
			if (!m_SecurityQuestionAnswered && m_LCDCursorMoveTimer < num)
			{
				if (m_Movement.Y < -0.5f && m_KeyboardRow > 0)
				{
					m_KeyboardRow--;
					m_LCDCursorMoveTimer = num + 0.25f;
					g.m_SoundManager.Play(19);
				}
				if (m_Movement.Y > 0.5f && m_KeyboardRow < 2)
				{
					m_KeyboardRow++;
					m_LCDCursorMoveTimer = num + 0.25f;
					g.m_SoundManager.Play(19);
				}
				int num2 = 9;
				if (m_KeyboardRow == 1)
				{
					num2 = 8;
				}
				else if (m_KeyboardRow == 2)
				{
					num2 = 6;
				}
				if (m_Movement.X < -0.5f && m_KeyboardCol > 0)
				{
					m_KeyboardCol--;
					m_LCDCursorMoveTimer = num + 0.25f;
					g.m_SoundManager.Play(19);
				}
				if (m_Movement.X > 0.5f && m_KeyboardCol < num2)
				{
					m_KeyboardCol++;
					m_LCDCursorMoveTimer = num + 0.25f;
					g.m_SoundManager.Play(19);
				}
				if (m_KeyboardCol > num2)
				{
					m_KeyboardCol = num2;
				}
			}
			break;
		}
	}

	public void DrawLCD2()
	{
		_ = g.m_App.m_GameTime.TotalGameTime.TotalSeconds;
		for (int i = 0; i < 8; i++)
		{
			g.m_App.GraphicsDevice.SamplerStates[i] = SamplerState.PointClamp;
		}
		g.m_App.GraphicsDevice.SetRenderTarget(g.m_App.m_LCD1RenderTarget);
		g.m_App.screenManager.SpriteBatch.Begin();
		g.m_App.GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 1f, 0);
		new Vector2(0f, 0f);
		Color lCDGreen = LCDGreen;
		int num = 40;
		int num2 = 32;
		SpriteFont lcdFont = g.m_App.lcdFont;
		switch (m_LCDMenu)
		{
		case LCDMenu.MAIN_MENU2:
		{
			Vector2 vector2 = new Vector2(10f, 70f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "SECURITY TERMINAL", vector2 + new Vector2(90f, 0f), lCDGreen);
			vector2.Y += 80f;
			if (m_LCDCursorOn)
			{
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, ">", vector2 + new Vector2(0f, m_LCDCursorRow * num), Color.White);
			}
			vector2.X += 20f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Captain Meyer Log Entry #2", vector2, lCDGreen);
			vector2.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Dr Holtz Log Entry #2", vector2, lCDGreen);
			vector2.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Engineer Simmons Log Entry #2", vector2, lCDGreen);
			vector2.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Door Security", vector2, lCDGreen);
			break;
		}
		case LCDMenu.CAPTAIN2:
		{
			Vector2 vector3 = new Vector2(10f, 10f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "CAPTAINS LOG #2", vector3 + new Vector2(100f, 0f), lCDGreen);
			vector3.Y += 50f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Dr Holtz has requested another visit to the", vector3, lCDGreen);
			vector3.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "research site. Not more samples surely. The", vector3, lCDGreen);
			vector3.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "hold is bursting at the seams as it is.", vector3, lCDGreen);
			vector3.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "No reply from the company yet about my A.B.", vector3, lCDGreen);
			vector3.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "request. Did they get my message?", vector3, lCDGreen);
			vector3.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Simmons has reported feeling unwell so", vector3, lCDGreen);
			vector3.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "I've put him on light duties.", vector3, lCDGreen);
			vector3.Y += num2;
			vector3.Y += num2;
			break;
		}
		case LCDMenu.HOLTZ2:
		{
			Vector2 vector6 = new Vector2(10f, 10f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "DR HOLTZ LOG #2", vector6 + new Vector2(100f, 0f), lCDGreen);
			vector6.Y += 50f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Yes! The mass-spec results prove my theory.", vector6, lCDGreen);
			vector6.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "I need more samples - the first batch have", vector6, lCDGreen);
			vector6.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "already deteriorated and the DNA analyser", vector6, lCDGreen);
			vector6.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "seems to be playing up.", vector6, lCDGreen);
			vector6.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "I've asked the captain if we can go down and", vector6, lCDGreen);
			vector6.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "do another survey.", vector6, lCDGreen);
			vector6.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "In the meantime, Samuel and I will do the", vector6, lCDGreen);
			vector6.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "dissections as soon as the specimens thaw.", vector6, lCDGreen);
			vector6.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "The engineer (Simmons was it?) was hanging", vector6, lCDGreen);
			vector6.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "around the lab and acting a bit strange.", vector6, lCDGreen);
			vector6.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Looked ill to me - but I'm no doctor.", vector6, lCDGreen);
			break;
		}
		case LCDMenu.SIMMONS2:
		{
			Vector2 vector5 = new Vector2(10f, 10f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "ENGINEER SIMMONS LOG #2", vector5 + new Vector2(100f, 0f), lCDGreen);
			vector5.Y += 50f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "<EOF>", vector5, lCDGreen);
			break;
		}
		case LCDMenu.DOORSYSTEM:
		{
			Vector2 vector4 = new Vector2(10f, 70f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "DOOR SECURITY MENU", vector4 + new Vector2(130f, 0f), lCDGreen);
			vector4.Y += 80f;
			if (m_LCDCursorOn)
			{
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, ">", vector4 + new Vector2(0f, m_LCDCursorRow * num), Color.White);
			}
			vector4.X += 20f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Enter Password", vector4, lCDGreen);
			vector4.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Forgotten your password?", vector4, lCDGreen);
			break;
		}
		case LCDMenu.DOORPASSWORD:
		{
			Vector2 vector7 = new Vector2(10f, 10f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "DOOR SECURITY SYSTEM", vector7 + new Vector2(150f, 0f), lCDGreen);
			vector7.Y += 100f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Door Status:", vector7, lCDGreen);
			if (m_Door1Unlocked)
			{
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, " UNLOCKED", vector7 + new Vector2(220f, 0f), Color.White);
			}
			else
			{
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "LOCKED", vector7 + new Vector2(220f, 0f), Color.Red);
			}
			vector7.Y += num2;
			vector7.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Enter password below to unlock the", vector7, lCDGreen);
			vector7.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "door system.", vector7, lCDGreen);
			vector7.Y += 100f;
			string text2 = "";
			for (int k = 0; k < 4; k++)
			{
				if (m_Password[k] == 0)
				{
					text2 += "- ";
				}
				if (m_Password[k] == 1)
				{
					text2 += "A ";
				}
				if (m_Password[k] == 2)
				{
					text2 += "X ";
				}
				if (m_Password[k] == 3)
				{
					text2 += "Y ";
				}
			}
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, text2, vector7, lCDGreen);
			break;
		}
		case LCDMenu.SECURITYQUESTION:
		{
			Vector2 vector = new Vector2(10f, 10f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "RESET PASSWORD", vector + new Vector2(150f, 0f), lCDGreen);
			vector.Y += 100f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Please answer the security question below. ", vector, lCDGreen);
			vector.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "This question helps us verify your identity.", vector, lCDGreen);
			vector.Y += num2;
			vector.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "What is your favorite food?", vector, lCDGreen);
			m_AllowNoodlesClue = true;
			string text = "";
			for (int j = 0; j < 7; j++)
			{
				text = ((m_SecurityQuestion[j] != 0) ? (text + Encoding.UTF8.GetString(m_SecurityQuestion, j, 1)) : (text + "- "));
			}
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, text, vector + new Vector2(300f, 0f), lCDGreen);
			if (m_SecurityQuestionAnswered)
			{
				vector.Y += num2;
				vector.Y += num2;
				vector.Y += num2;
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Thanks for helping us verify your account.", vector, lCDGreen);
				vector.Y += num2;
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Your password has been reset to:", vector, lCDGreen);
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "X X X X", vector + new Vector2(340f, 0f), Color.White);
				vector.Y += num2;
				vector.Y += num2;
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "You should now be able to login and change it.", vector, lCDGreen);
				vector.Y += num2;
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Note: Never give your password to anyone.", vector, lCDGreen);
			}
			else if (m_SecurityQuestionWrong)
			{
				vector.Y += num2;
				vector.Y += num2;
				vector.Y += num2;
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "We apologize, but we were unable to verify your", vector, lCDGreen);
				vector.Y += num2;
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "account information with the answer you", vector, lCDGreen);
				vector.Y += num2;
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "provided to our security question.", vector, lCDGreen);
			}
			break;
		}
		}
		g.m_App.screenManager.SpriteBatch.End();
		g.m_App.GraphicsDevice.SetRenderTarget(null);
		if (g.m_App.m_LCD2 != null)
		{
			DeferredObjectEffect deferredObjectEffect = g.m_App.m_LCD2.RenderableMeshes[0].Effect as DeferredObjectEffect;
			deferredObjectEffect.DiffuseMapTexture = g.m_App.m_LCD1RenderTarget;
			deferredObjectEffect.SpecularAmount = 0.4f;
			deferredObjectEffect.SpecularPower = 10f;
			deferredObjectEffect.EmissiveColor = new Vector3(1f, 1f, 1f);
			deferredObjectEffect.TransparencyMode = TransparencyMode.None;
		}
	}

	public void UpdateUsingLCD3()
	{
		g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD11, "[A]-SELECT", new Vector2(30f, 335f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
		g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD12, "[B]-EXIT", new Vector2(30f, 355f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
		g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD13, "LEFT STICK-UP/DOWN", new Vector2(30f, 375f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
		string s = "COMPUTER LINK ACTIVE";
		g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD14, s, new Vector2(g.m_App.GetHudCentreX(s, g.m_App.hudFont), 90f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.TextPrompt, g.m_App.hudFont, hideSysMsg: true);
		UpdatePlayerUsingLCD();
		float num = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds;
		switch (m_LCDMenu)
		{
		case LCDMenu.MAIN_MENU3:
			if (m_LCDCursorMoveTimer < num)
			{
				if (m_Movement.Y < -0.5f && m_LCDCursorRow > 0)
				{
					m_LCDCursorRow--;
					m_LCDCursorMoveTimer = num + 0.25f;
					m_LCDCursorOn = true;
					m_LCDCursorBlink = 0;
					g.m_SoundManager.Play(19);
				}
				if (m_Movement.Y > 0.5f && m_LCDCursorRow < 3)
				{
					m_LCDCursorRow++;
					m_LCDCursorMoveTimer = num + 0.25f;
					m_LCDCursorOn = true;
					m_LCDCursorBlink = 0;
					g.m_SoundManager.Play(19);
				}
			}
			break;
		case LCDMenu.MEDICALNOTES:
			if (m_LCDCursorMoveTimer < num)
			{
				if (m_Movement.Y < -0.5f && m_LCDCursorRow > 0)
				{
					m_LCDCursorRow--;
					m_LCDCursorMoveTimer = num + 0.25f;
					m_LCDCursorOn = true;
					m_LCDCursorBlink = 0;
					g.m_SoundManager.Play(19);
				}
				if (m_Movement.Y > 0.5f && m_LCDCursorRow < 2)
				{
					m_LCDCursorRow++;
					m_LCDCursorMoveTimer = num + 0.25f;
					m_LCDCursorOn = true;
					m_LCDCursorBlink = 0;
					g.m_SoundManager.Play(19);
				}
			}
			break;
		}
	}

	public void DrawLCD3()
	{
		_ = g.m_App.m_GameTime.TotalGameTime.TotalSeconds;
		for (int i = 0; i < 8; i++)
		{
			g.m_App.GraphicsDevice.SamplerStates[i] = SamplerState.PointClamp;
		}
		g.m_App.GraphicsDevice.SetRenderTarget(g.m_App.m_LCD1RenderTarget);
		g.m_App.screenManager.SpriteBatch.Begin();
		g.m_App.GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 1f, 0);
		new Vector2(0f, 0f);
		Color lCDGreen = LCDGreen;
		int num = 40;
		int num2 = 32;
		SpriteFont lcdFont = g.m_App.lcdFont;
		switch (m_LCDMenu)
		{
		case LCDMenu.MAIN_MENU3:
		{
			Vector2 vector5 = new Vector2(10f, 70f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "RESEARCH TERMINAL", vector5 + new Vector2(90f, 0f), lCDGreen);
			vector5.Y += 80f;
			if (m_LCDCursorOn)
			{
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, ">", vector5 + new Vector2(0f, m_LCDCursorRow * num), Color.White);
			}
			vector5.X += 20f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Captain Meyer Log Entry #3", vector5, lCDGreen);
			vector5.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Dr Holtz Log Entry #3", vector5, lCDGreen);
			vector5.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Engineer Simmons Log Entry #3", vector5, lCDGreen);
			vector5.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Medical Notes", vector5, lCDGreen);
			break;
		}
		case LCDMenu.CAPTAIN3:
		{
			Vector2 vector6 = new Vector2(10f, 10f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "CAPTAINS LOG #3", vector6 + new Vector2(100f, 0f), lCDGreen);
			vector6.Y += 50f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "There's something seriously wrong with Simmons.", vector6, lCDGreen);
			vector6.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "I think he needs urgent medical help.", vector6, lCDGreen);
			vector6.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "The engine's not responding though. Problem", vector6, lCDGreen);
			vector6.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "seems to be in the Lenndrive 1.", vector6, lCDGreen);
			vector6.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Still no response from company - could", vector6, lCDGreen);
			vector6.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "be interference or something.", vector6, lCDGreen);
			vector6.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "I'd better set a distress beacon.", vector6, lCDGreen);
			vector6.Y += num2;
			vector6.Y += num2;
			break;
		}
		case LCDMenu.HOLTZ3:
		{
			Vector2 vector8 = new Vector2(10f, 10f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "DR HOLTZ LOG #3", vector8 + new Vector2(100f, 0f), lCDGreen);
			vector8.Y += 50f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "The captain has informed us, on no uncertain", vector8, lCDGreen);
			vector8.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "terms, that we need to return to HQ but there", vector8, lCDGreen);
			vector8.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "seems to be some sort of technical hitch with", vector8, lCDGreen);
			vector8.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "the ships engines, apparently.", vector8, lCDGreen);
			vector8.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "We'll have to make do with the samples we've", vector8, lCDGreen);
			vector8.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "already collected. I guess I'd better pack up", vector8, lCDGreen);
			vector8.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "all our stuff. Sammuel said he saw Simmons", vector8, lCDGreen);
			vector8.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "today. Said he looked ghastly and tried to", vector8, lCDGreen);
			vector8.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "tell him something - could only point - before", vector8, lCDGreen);
			vector8.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "he collapsed. Oh I've double checked the DNA", vector8, lCDGreen);
			vector8.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "analyser and it's working fine. I think it's ", vector8, lCDGreen);
			vector8.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "picking up some sort of background contamination.", vector8, lCDGreen);
			vector8.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "It could be in the air. I'm gonna get myself", vector8, lCDGreen);
			vector8.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "a respirator.", vector8, lCDGreen);
			break;
		}
		case LCDMenu.SIMMONS3:
		{
			Vector2 vector7 = new Vector2(10f, 10f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "ENGINEER SIMMONS LOG #3", vector7 + new Vector2(100f, 0f), lCDGreen);
			vector7.Y += 50f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "<EOF>", vector7, lCDGreen);
			break;
		}
		case LCDMenu.MEDICALNOTES:
		{
			Vector2 vector4 = new Vector2(10f, 70f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "MEDICAL NOTES", vector4 + new Vector2(90f, 0f), lCDGreen);
			vector4.Y += 80f;
			if (m_LCDCursorOn)
			{
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, ">", vector4 + new Vector2(0f, m_LCDCursorRow * num), Color.White);
			}
			vector4.X += 20f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Cerebral Hypoxia", vector4, lCDGreen);
			vector4.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Parasitoidal microbial diseases", vector4, lCDGreen);
			vector4.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Symbiosis", vector4, lCDGreen);
			break;
		}
		case LCDMenu.MEDICALNOTE1:
		{
			Vector2 vector3 = new Vector2(10f, 70f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Cerebral Hypoxia", vector3 + new Vector2(90f, 0f), lCDGreen);
			vector3.Y += 80f;
			vector3.X += 20f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Cerebral hypoxia is a form of hypoxia", vector3, lCDGreen);
			vector3.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "(reduced supply of oxygen) specifically", vector3, lCDGreen);
			vector3.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "involving the brain.", vector3, lCDGreen);
			vector3.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Without sufficient oxygen to sustain life, ", vector3, lCDGreen);
			vector3.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "individuals will feel dizzy and are prone", vector3, lCDGreen);
			vector3.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "to hallucinations.", vector3, lCDGreen);
			break;
		}
		case LCDMenu.MEDICALNOTE2:
		{
			Vector2 vector2 = new Vector2(10f, 70f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Parasitoidal Microbial Diseases", vector2 + new Vector2(90f, 0f), lCDGreen);
			vector2.Y += 80f;
			vector2.X += 20f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "There are many species of parasitoid ", vector2, lCDGreen);
			vector2.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "that frequently or even routinely kill", vector2, lCDGreen);
			vector2.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "their prey without consuming much of it.", vector2, lCDGreen);
			vector2.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "This apparently wasteful strategy sometimes", vector2, lCDGreen);
			vector2.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "might have the effect of reducing the risk", vector2, lCDGreen);
			vector2.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "that the prey could escape or offer resistance.", vector2, lCDGreen);
			break;
		}
		case LCDMenu.MEDICALNOTE3:
		{
			Vector2 vector = new Vector2(10f, 70f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Symbiosis", vector + new Vector2(90f, 0f), lCDGreen);
			vector.Y += 80f;
			vector.X += 20f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Symbiosis is a close and often long-term", vector, lCDGreen);
			vector.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "interaction between two or more different", vector, lCDGreen);
			vector.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "biological species.", vector, lCDGreen);
			vector.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Bennet used the word symbiosis to", vector, lCDGreen);
			vector.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "sharply define the mutualistic", vector, lCDGreen);
			vector.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "relationship in lichens in 1877.", vector, lCDGreen);
			break;
		}
		}
		g.m_App.screenManager.SpriteBatch.End();
		g.m_App.GraphicsDevice.SetRenderTarget(null);
		if (g.m_App.m_LCD2 != null)
		{
			DeferredObjectEffect deferredObjectEffect = g.m_App.m_LCD2.RenderableMeshes[0].Effect as DeferredObjectEffect;
			deferredObjectEffect.DiffuseMapTexture = g.m_App.m_LCD1RenderTarget;
			deferredObjectEffect.SpecularAmount = 0.4f;
			deferredObjectEffect.SpecularPower = 10f;
			deferredObjectEffect.EmissiveColor = new Vector3(1f, 1f, 1f);
			deferredObjectEffect.TransparencyMode = TransparencyMode.None;
		}
	}

	public void UpdateUsingLCD4()
	{
		HELMET_KEYBOARD_POS.X = 60f;
		string text = "QWERTYUIOP";
		string text2 = "ASDFGHJKL";
		string text3 = "ZXCVBNM";
		string s = "COMPUTER LINK ACTIVE";
		g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD14, s, new Vector2(g.m_App.GetHudCentreX(s, g.m_App.hudFont), 90f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.TextPrompt, g.m_App.hudFont, hideSysMsg: true);
		if (m_LCDMenu == LCDMenu.NETWORKMENU)
		{
			if (!m_NetworkPasswordAnswered)
			{
				g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD11, "[A]-SELECT LETTER", new Vector2(30f, 335f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
				g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD12, "[B]-EXIT   [X]-DEL", new Vector2(30f, 355f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
				g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD13, "LEFT STICK-MOVE CURSOR", new Vector2(30f, 375f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
				for (int i = 0; i < 10; i++)
				{
					g.m_App.AddHelmetMessage((HelmetMessage.TYPE)(40 + i), text.Substring(i, 1), new Vector2(HELMET_KEYBOARD_POS.X + (float)(38 * i), HELMET_KEYBOARD_POS.Y), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
				}
				for (int j = 0; j < 9; j++)
				{
					g.m_App.AddHelmetMessage((HelmetMessage.TYPE)(50 + j), text2.Substring(j, 1), new Vector2(HELMET_KEYBOARD_POS.X + (float)(38 * j), HELMET_KEYBOARD_POS.Y + HELMET_KEYBOARD_Y), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
				}
				for (int k = 0; k < 7; k++)
				{
					g.m_App.AddHelmetMessage((HelmetMessage.TYPE)(59 + k), text3.Substring(k, 1), new Vector2(HELMET_KEYBOARD_POS.X + (float)(38 * k), HELMET_KEYBOARD_POS.Y + HELMET_KEYBOARD_Y * 2f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
				}
			}
			else
			{
				g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD12, "[B]-EXIT", new Vector2(30f, 360f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
			}
		}
		else if (m_LCDMenu == LCDMenu.BRINGONLINE)
		{
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD11, "[A]-SELECT NUMBER", new Vector2(30f, 335f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD12, "[B]-EXIT   [X]-DEL", new Vector2(30f, 355f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD13, "LEFT STICK-MOVE CURSOR", new Vector2(30f, 375f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
			float num = 37f;
			float num2 = 25f;
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD21, "7", new Vector2(HELMET_KEYBOARD_POS.X, HELMET_KEYBOARD_POS.Y), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD22, "8", new Vector2(HELMET_KEYBOARD_POS.X + num, HELMET_KEYBOARD_POS.Y), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD23, "9", new Vector2(HELMET_KEYBOARD_POS.X + num + num, HELMET_KEYBOARD_POS.Y), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD24, "4", new Vector2(HELMET_KEYBOARD_POS.X, HELMET_KEYBOARD_POS.Y + num2), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD25, "5", new Vector2(HELMET_KEYBOARD_POS.X + num, HELMET_KEYBOARD_POS.Y + num2), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD26, "6", new Vector2(HELMET_KEYBOARD_POS.X + num + num, HELMET_KEYBOARD_POS.Y + num2), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD27, "1", new Vector2(HELMET_KEYBOARD_POS.X, HELMET_KEYBOARD_POS.Y + num2 + num2), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD28, "2", new Vector2(HELMET_KEYBOARD_POS.X + num, HELMET_KEYBOARD_POS.Y + num2 + num2), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD29, "3", new Vector2(HELMET_KEYBOARD_POS.X + num + num, HELMET_KEYBOARD_POS.Y + num2 + num2), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
		}
		else
		{
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD11, "[A]-SELECT", new Vector2(30f, 335f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD12, "[B]-EXIT", new Vector2(30f, 355f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD13, "LEFT STICK-UP/DOWN", new Vector2(30f, 375f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
		}
		UpdatePlayerUsingLCD();
		float num3 = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds;
		switch (m_LCDMenu)
		{
		case LCDMenu.MAIN_MENU4:
			if (m_LCDCursorMoveTimer < num3)
			{
				if (m_Movement.Y < -0.5f && m_LCDCursorRow > 0)
				{
					m_LCDCursorRow--;
					m_LCDCursorMoveTimer = num3 + 0.25f;
					m_LCDCursorOn = true;
					m_LCDCursorBlink = 0;
					g.m_SoundManager.Play(19);
				}
				if (m_Movement.Y > 0.5f && m_LCDCursorRow < 3)
				{
					m_LCDCursorRow++;
					m_LCDCursorMoveTimer = num3 + 0.25f;
					m_LCDCursorOn = true;
					m_LCDCursorBlink = 0;
					g.m_SoundManager.Play(19);
				}
			}
			break;
		case LCDMenu.NETWORKMENU:
			if (!m_NetworkPasswordAnswered)
			{
				if (m_LCDCursorMoveTimer < num3)
				{
					if (m_Movement.Y < -0.5f && m_KeyboardRow > 0)
					{
						m_KeyboardRow--;
						m_LCDCursorMoveTimer = num3 + 0.25f;
						g.m_SoundManager.Play(19);
					}
					if (m_Movement.Y > 0.5f && m_KeyboardRow < 2)
					{
						m_KeyboardRow++;
						m_LCDCursorMoveTimer = num3 + 0.25f;
						g.m_SoundManager.Play(19);
					}
					int num5 = 9;
					if (m_KeyboardRow == 1)
					{
						num5 = 8;
					}
					else if (m_KeyboardRow == 2)
					{
						num5 = 6;
					}
					if (m_Movement.X < -0.5f && m_KeyboardCol > 0)
					{
						m_KeyboardCol--;
						m_LCDCursorMoveTimer = num3 + 0.25f;
						g.m_SoundManager.Play(19);
					}
					if (m_Movement.X > 0.5f && m_KeyboardCol < num5)
					{
						m_KeyboardCol++;
						m_LCDCursorMoveTimer = num3 + 0.25f;
						g.m_SoundManager.Play(19);
					}
					if (m_KeyboardCol > num5)
					{
						m_KeyboardCol = num5;
					}
				}
			}
			else if (m_LCDCursorMoveTimer < num3)
			{
				if (m_Movement.Y < -0.5f && m_LCDCursorRow > 0)
				{
					m_LCDCursorRow--;
					m_LCDCursorMoveTimer = num3 + 0.25f;
					m_LCDCursorOn = true;
					m_LCDCursorBlink = 0;
					g.m_SoundManager.Play(19);
				}
				if (m_Movement.Y > 0.5f && m_LCDCursorRow < 1)
				{
					m_LCDCursorRow++;
					m_LCDCursorMoveTimer = num3 + 0.25f;
					m_LCDCursorOn = true;
					m_LCDCursorBlink = 0;
					g.m_SoundManager.Play(19);
				}
			}
			break;
		case LCDMenu.PING:
			if (m_LCDCursorMoveTimer < num3)
			{
				if (m_Movement.Y < -0.5f && m_LCDCursorRow > 0)
				{
					m_LCDCursorRow--;
					m_LCDCursorMoveTimer = num3 + 0.25f;
					m_LCDCursorOn = true;
					m_LCDCursorBlink = 0;
					g.m_SoundManager.Play(19);
				}
				if (m_Movement.Y > 0.5f && m_LCDCursorRow < 4)
				{
					m_LCDCursorRow++;
					m_LCDCursorMoveTimer = num3 + 0.25f;
					m_LCDCursorOn = true;
					m_LCDCursorBlink = 0;
					g.m_SoundManager.Play(19);
				}
			}
			break;
		case LCDMenu.BRINGONLINE:
			if (m_LCDCursorMoveTimer < num3)
			{
				if (m_Movement.Y < -0.5f && m_KeyboardRow > 0)
				{
					m_KeyboardRow--;
					m_LCDCursorMoveTimer = num3 + 0.25f;
					g.m_SoundManager.Play(19);
				}
				if (m_Movement.Y > 0.5f && m_KeyboardRow < 2)
				{
					m_KeyboardRow++;
					m_LCDCursorMoveTimer = num3 + 0.25f;
					g.m_SoundManager.Play(19);
				}
				int num4 = 2;
				if (m_Movement.X < -0.5f && m_KeyboardCol > 0)
				{
					m_KeyboardCol--;
					m_LCDCursorMoveTimer = num3 + 0.25f;
					g.m_SoundManager.Play(19);
				}
				if (m_Movement.X > 0.5f && m_KeyboardCol < num4)
				{
					m_KeyboardCol++;
					m_LCDCursorMoveTimer = num3 + 0.25f;
					g.m_SoundManager.Play(19);
				}
				if (m_KeyboardCol > num4)
				{
					m_KeyboardCol = num4;
				}
			}
			break;
		case LCDMenu.CAPTAIN4:
		case LCDMenu.HOLTZ4:
		case LCDMenu.SIMMONS4:
			break;
		}
	}

	public void DrawLCD4()
	{
		_ = g.m_App.m_GameTime.TotalGameTime.TotalSeconds;
		for (int i = 0; i < 8; i++)
		{
			g.m_App.GraphicsDevice.SamplerStates[i] = SamplerState.PointClamp;
		}
		g.m_App.GraphicsDevice.SetRenderTarget(g.m_App.m_LCD1RenderTarget);
		g.m_App.screenManager.SpriteBatch.Begin();
		g.m_App.GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 1f, 0);
		new Vector2(0f, 0f);
		Color lCDGreen = LCDGreen;
		int num = 40;
		int num2 = 32;
		SpriteFont lcdFont = g.m_App.lcdFont;
		switch (m_LCDMenu)
		{
		case LCDMenu.MAIN_MENU4:
		{
			Vector2 vector6 = new Vector2(10f, 70f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "NETWORK TERMINAL", vector6 + new Vector2(90f, 0f), lCDGreen);
			vector6.Y += 80f;
			if (m_LCDCursorOn)
			{
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, ">", vector6 + new Vector2(0f, m_LCDCursorRow * num), Color.White);
			}
			vector6.X += 20f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Captain Meyer Log Entry #4", vector6, lCDGreen);
			vector6.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Dr Holtz Log Entry #4", vector6, lCDGreen);
			vector6.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Engineer Simmons Log Entry #4", vector6, lCDGreen);
			vector6.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Network Access", vector6, lCDGreen);
			break;
		}
		case LCDMenu.CAPTAIN4:
		{
			Vector2 vector12 = new Vector2(10f, 10f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "CAPTAINS LOG #4", vector12 + new Vector2(100f, 0f), lCDGreen);
			vector12.Y += 50f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Simmons has attacked and killed Dr Holtz and", vector12, lCDGreen);
			vector12.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "and Curtis! He seems to have contracted", vector12, lCDGreen);
			vector12.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "something that has changed him beyond all", vector12, lCDGreen);
			vector12.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "recognition. He is currently hiding out in", vector12, lCDGreen);
			vector12.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "the cargo hold.", vector12, lCDGreen);
			vector12.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "No response yet to our SOS. So I guess I'll", vector12, lCDGreen);
			vector12.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "have to get past him to repair the Lenndrive", vector12, lCDGreen);
			vector12.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "myself, if I've got any hope of getting out of", vector12, lCDGreen);
			vector12.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "here alive.", vector12, lCDGreen);
			break;
		}
		case LCDMenu.HOLTZ4:
		{
			Vector2 vector2 = new Vector2(10f, 10f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "DR HOLTZ LOG #4", vector2 + new Vector2(100f, 0f), lCDGreen);
			vector2.Y += 50f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "I've holed myself up in the lab. Simmons has", vector2, lCDGreen);
			vector2.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "gone crazy and - oh my god! - he killed", vector2, lCDGreen);
			vector2.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "poor Sammuel.", vector2, lCDGreen);
			vector2.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Captain's no use at all.", vector2, lCDGreen);
			vector2.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "My only hope is - what's that noise?!", vector2, lCDGreen);
			vector2.Y += num2;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "<EOF>", vector2, lCDGreen);
			break;
		}
		case LCDMenu.SIMMONS4:
		{
			Vector2 vector7 = new Vector2(10f, 10f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "ENGINEER SIMMONS LOG #4", vector7 + new Vector2(100f, 0f), lCDGreen);
			vector7.Y += 50f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "<EOF>", vector7, lCDGreen);
			break;
		}
		case LCDMenu.NETWORKMENU:
		{
			if (!m_NetworkPasswordAnswered)
			{
				Vector2 vector3 = new Vector2(10f, 10f);
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "NETWORK ACCESS", vector3 + new Vector2(150f, 0f), lCDGreen);
				vector3.Y += 150f;
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Enter password: ", vector3, lCDGreen);
				string text2 = "";
				for (int k = 0; k < 7; k++)
				{
					text2 = ((m_NetworkPassword[k] != 0) ? (text2 + Encoding.UTF8.GetString(m_NetworkPassword, k, 1)) : (text2 + "- "));
				}
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, text2, vector3 + new Vector2(200f, 0f), lCDGreen);
				break;
			}
			Vector2 vector4 = new Vector2(10f, 10f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "NETWORK ACCESS", vector4 + new Vector2(150f, 0f), lCDGreen);
			vector4.Y += 150f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Network Commands:", vector4, lCDGreen);
			vector4.Y += num;
			if (m_LCDCursorOn)
			{
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, ">", vector4 + new Vector2(0f, m_LCDCursorRow * num), Color.White);
			}
			vector4.X += 20f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "1. Ping Door", vector4, lCDGreen);
			vector4.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "2. Bring Door Online", vector4, lCDGreen);
			break;
		}
		case LCDMenu.PING:
		{
			Vector2 vector13 = new Vector2(10f, 10f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Send a Ping to a door:", vector13 + new Vector2(150f, 0f), lCDGreen);
			vector13.Y += 150f;
			if (m_LCDCursorOn)
			{
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, ">", vector13 + new Vector2(0f, m_LCDCursorRow * num), Color.White);
			}
			vector13.X += 20f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Door #1", vector13, lCDGreen);
			vector13.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Door #2", vector13, lCDGreen);
			vector13.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Door #3", vector13, lCDGreen);
			vector13.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Door #4", vector13, lCDGreen);
			vector13.Y += num;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Door #5", vector13, lCDGreen);
			m_PingFrame = 0;
			break;
		}
		case LCDMenu.PINGDOOR1:
		{
			m_PingFrame++;
			Vector2 vector8 = new Vector2(10f, 10f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Send a Ping to a door:", vector8 + new Vector2(150f, 0f), lCDGreen);
			vector8.Y += 150f;
			vector8.X += 20f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Pinging Door 1 with 32 bytes of data.", vector8, lCDGreen);
			if (m_PingFrame > 10)
			{
				vector8.Y += num2;
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Reply from 192.168.15.1 bytes = 32 time < 1 ms", vector8, lCDGreen);
			}
			if (m_PingFrame > 20)
			{
				vector8.Y += num2;
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Reply from 192.168.15.1 bytes = 32 time < 1 ms", vector8, lCDGreen);
			}
			if (m_PingFrame > 30)
			{
				vector8.Y += num2;
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Reply from 192.168.15.1 bytes = 32 time < 1 ms", vector8, lCDGreen);
			}
			if (m_PingFrame > 40)
			{
				vector8.Y += num2;
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "IP Address = 192.168.15.1", vector8, Color.Gray);
			}
			break;
		}
		case LCDMenu.PINGDOOR2:
		{
			m_PingFrame++;
			Vector2 vector5 = new Vector2(10f, 10f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Send a Ping to a door:", vector5 + new Vector2(150f, 0f), lCDGreen);
			vector5.Y += 150f;
			vector5.X += 20f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Ping Door 2 with 32 bytes of data.", vector5, lCDGreen);
			if (m_PingFrame > 10)
			{
				vector5.Y += num2;
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Reply from 192.168.30.3 bytes = 32 time < 1 ms", vector5, lCDGreen);
			}
			if (m_PingFrame > 20)
			{
				vector5.Y += num2;
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Reply from 192.168.30.3 bytes = 32 time < 1 ms", vector5, lCDGreen);
			}
			if (m_PingFrame > 30)
			{
				vector5.Y += num2;
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Reply from 192.168.30.3 bytes = 32 time < 1 ms", vector5, lCDGreen);
			}
			if (m_PingFrame > 40)
			{
				vector5.Y += num2;
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "IP Address = 192.168.30.3", vector5, Color.Gray);
			}
			break;
		}
		case LCDMenu.PINGDOOR3:
		{
			m_PingFrame++;
			Vector2 vector9 = new Vector2(10f, 10f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Send a Ping to a door:", vector9 + new Vector2(150f, 0f), lCDGreen);
			vector9.Y += 150f;
			vector9.X += 20f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Ping Door 3 with 32 bytes of data.", vector9, lCDGreen);
			if (m_PingFrame > 10)
			{
				vector9.Y += num2;
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Reply from 192.168.45.5 bytes = 32 time < 1 ms", vector9, lCDGreen);
			}
			if (m_PingFrame > 20)
			{
				vector9.Y += num2;
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Reply from 192.168.45.5 bytes = 32 time < 1 ms", vector9, lCDGreen);
			}
			if (m_PingFrame > 30)
			{
				vector9.Y += num2;
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Reply from 192.168.45.5 bytes = 32 time < 1 ms", vector9, lCDGreen);
			}
			if (m_PingFrame > 40)
			{
				vector9.Y += num2;
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "IP Address = 192.168.45.5", vector9, Color.Gray);
			}
			break;
		}
		case LCDMenu.PINGDOOR4:
		{
			m_PingFrame++;
			Vector2 vector11 = new Vector2(10f, 10f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Send a Ping to a door:", vector11 + new Vector2(150f, 0f), lCDGreen);
			vector11.Y += 150f;
			vector11.X += 20f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Ping Door 4 with 32 bytes of data.", vector11, lCDGreen);
			if (m_PingFrame > 10)
			{
				vector11.Y += num2;
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Reply from 192.168.60.7 bytes = 32 time < 1 ms", vector11, lCDGreen);
			}
			if (m_PingFrame > 20)
			{
				vector11.Y += num2;
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Reply from 192.168.60.7 bytes = 32 time < 1 ms", vector11, lCDGreen);
			}
			if (m_PingFrame > 30)
			{
				vector11.Y += num2;
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Reply from 192.168.60.7 bytes = 32 time < 1 ms", vector11, lCDGreen);
			}
			if (m_PingFrame > 40)
			{
				vector11.Y += num2;
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "IP Address = 192.168.60.7", vector11, Color.Gray);
			}
			break;
		}
		case LCDMenu.PINGDOOR5:
		{
			m_PingFrame++;
			Vector2 vector10 = new Vector2(10f, 10f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Send a Ping to a door:", vector10 + new Vector2(150f, 0f), lCDGreen);
			vector10.Y += 150f;
			vector10.X += 20f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Ping Door 5 with 32 bytes of data.", vector10, lCDGreen);
			if (m_DoorCrewExitUnlocked)
			{
				if (m_PingFrame > 10)
				{
					vector10.Y += num2;
					g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Reply from 192.168.75.9 bytes = 32 time < 1 ms", vector10, lCDGreen);
				}
				if (m_PingFrame > 20)
				{
					vector10.Y += num2;
					g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Reply from 192.168.75.9 bytes = 32 time < 1 ms", vector10, lCDGreen);
				}
				if (m_PingFrame > 30)
				{
					vector10.Y += num2;
					g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Reply from 192.168.75.9 bytes = 32 time < 1 ms", vector10, lCDGreen);
				}
				if (m_PingFrame > 40)
				{
					vector10.Y += num2;
					g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "IP Address = 192.168.75.9", vector10, Color.Gray);
				}
			}
			else if (m_PingFrame > 30)
			{
				vector10.Y += num;
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Request timed out. Not online.", vector10, lCDGreen);
			}
			break;
		}
		case LCDMenu.BRINGONLINE:
		{
			Vector2 vector = new Vector2(10f, 10f);
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Bring A Network Door Online", vector + new Vector2(150f, 0f), lCDGreen);
			vector.Y += 150f;
			if (m_LCDCursorOn)
			{
				g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, ">", vector + new Vector2(0f, m_LCDCursorRow * num), Color.White);
			}
			vector.X += 20f;
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "Enter IP Address of Door to bring online:", vector, lCDGreen);
			vector.Y += num;
			string text = "";
			for (int j = 0; j < 2; j++)
			{
				text = ((m_IPAddress[j] != 0) ? (text + Encoding.UTF8.GetString(m_IPAddress, j, 1)) : (text + "*"));
			}
			text += ".";
			text = ((m_IPAddress[2] != 0) ? (text + Encoding.UTF8.GetString(m_IPAddress, 2, 1)) : (text + "*"));
			g.m_App.screenManager.SpriteBatch.DrawString(lcdFont, "192.168." + text, vector, lCDGreen);
			break;
		}
		}
		g.m_App.screenManager.SpriteBatch.End();
		g.m_App.GraphicsDevice.SetRenderTarget(null);
		if (g.m_App.m_LCD2 != null)
		{
			DeferredObjectEffect deferredObjectEffect = g.m_App.m_LCD2.RenderableMeshes[0].Effect as DeferredObjectEffect;
			deferredObjectEffect.DiffuseMapTexture = g.m_App.m_LCD1RenderTarget;
			deferredObjectEffect.SpecularAmount = 0.4f;
			deferredObjectEffect.SpecularPower = 10f;
			deferredObjectEffect.EmissiveColor = new Vector3(1f, 1f, 1f);
			deferredObjectEffect.TransparencyMode = TransparencyMode.None;
		}
	}

	public void UpdateUsingSafe()
	{
		HELMET_KEYBOARD_POS.X = 60f;
		g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD11, "[A]-SELECT NUMBER", new Vector2(30f, 335f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
		g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD12, "[B]-EXIT   [X]-DEL", new Vector2(30f, 355f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
		g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD13, "LEFT STICK-MOVE CURSOR", new Vector2(30f, 375f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
		string s = "KEYPAD LINK ACTIVE";
		g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD14, s, new Vector2(g.m_App.GetHudCentreX(s, g.m_App.hudFont), 90f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.TextPrompt, g.m_App.hudFont, hideSysMsg: true);
		s = "";
		for (int i = 0; i < 4; i++)
		{
			s = ((m_SafePassword[i] != 0) ? (s + Encoding.UTF8.GetString(m_SafePassword, i, 1)) : (s + "- "));
		}
		g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD15, s, new Vector2(g.m_App.GetHudCentreX(s, g.m_App.hudFont), 110f), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
		float num = 37f;
		float num2 = 25f;
		g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD21, "7", new Vector2(HELMET_KEYBOARD_POS.X, HELMET_KEYBOARD_POS.Y), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
		g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD22, "8", new Vector2(HELMET_KEYBOARD_POS.X + num, HELMET_KEYBOARD_POS.Y), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
		g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD23, "9", new Vector2(HELMET_KEYBOARD_POS.X + num + num, HELMET_KEYBOARD_POS.Y), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
		g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD24, "4", new Vector2(HELMET_KEYBOARD_POS.X, HELMET_KEYBOARD_POS.Y + num2), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
		g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD25, "5", new Vector2(HELMET_KEYBOARD_POS.X + num, HELMET_KEYBOARD_POS.Y + num2), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
		g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD26, "6", new Vector2(HELMET_KEYBOARD_POS.X + num + num, HELMET_KEYBOARD_POS.Y + num2), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
		g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD27, "1", new Vector2(HELMET_KEYBOARD_POS.X, HELMET_KEYBOARD_POS.Y + num2 + num2), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
		g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD28, "2", new Vector2(HELMET_KEYBOARD_POS.X + num, HELMET_KEYBOARD_POS.Y + num2 + num2), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
		g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USING_LCD29, "3", new Vector2(HELMET_KEYBOARD_POS.X + num + num, HELMET_KEYBOARD_POS.Y + num2 + num2), 1f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.hudFont, hideSysMsg: true);
		UpdatePlayerUsingLCD();
		float num3 = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds;
		if (m_LCDCursorMoveTimer < num3)
		{
			if (m_Movement.Y < -0.5f && m_KeyboardRow > 0)
			{
				m_KeyboardRow--;
				m_LCDCursorMoveTimer = num3 + 0.25f;
				g.m_SoundManager.Play(19);
			}
			if (m_Movement.Y > 0.5f && m_KeyboardRow < 2)
			{
				m_KeyboardRow++;
				m_LCDCursorMoveTimer = num3 + 0.25f;
				g.m_SoundManager.Play(19);
			}
			int num4 = 2;
			if (m_Movement.X < -0.5f && m_KeyboardCol > 0)
			{
				m_KeyboardCol--;
				m_LCDCursorMoveTimer = num3 + 0.25f;
				g.m_SoundManager.Play(19);
			}
			if (m_Movement.X > 0.5f && m_KeyboardCol < num4)
			{
				m_KeyboardCol++;
				m_LCDCursorMoveTimer = num3 + 0.25f;
				g.m_SoundManager.Play(19);
			}
			if (m_KeyboardCol > num4)
			{
				m_KeyboardCol = num4;
			}
		}
	}

	public void DrawHelmetKeypad()
	{
		Vector2 vector = HELMET_KEYBOARD_POS + new Vector2((float)m_KeyboardCol * 38f, (float)m_KeyboardRow * HELMET_KEYBOARD_Y);
		if (g.m_App.m_Rand.NextDouble() < 0.5)
		{
			g.m_App.screenManager.SpriteBatch.DrawString(g.m_App.hudFont, "_", vector + new Vector2(0f, 3f), Color.White);
		}
	}

	public void DrawHelmetKeyboardCursor()
	{
		if ((m_LCDMenu != LCDMenu.SECURITYQUESTION || !m_SecurityQuestionAnswered) && (m_LCDMenu != LCDMenu.NETWORKMENU || !m_NetworkPasswordAnswered))
		{
			Vector2 vector = HELMET_KEYBOARD_POS + new Vector2((float)m_KeyboardCol * 38f, (float)m_KeyboardRow * HELMET_KEYBOARD_Y);
			if (g.m_App.m_Rand.NextDouble() < 0.5)
			{
				g.m_App.screenManager.SpriteBatch.DrawString(g.m_App.hudFont, "_", vector + new Vector2(0f, 3f), Color.White);
			}
		}
	}

	private void CheckSecurityQuestion()
	{
		if (!m_SecurityQuestionAnswered)
		{
			if (m_SecurityQuestion[0] == 78 && m_SecurityQuestion[1] == 79 && m_SecurityQuestion[2] == 79 && m_SecurityQuestion[3] == 68 && m_SecurityQuestion[4] == 76 && m_SecurityQuestion[5] == 69 && m_SecurityQuestion[6] == 83)
			{
				m_SecurityQuestionAnswered = true;
				g.m_SoundManager.Play(30);
			}
			else
			{
				m_SecurityQuestionWrong = true;
				ResetSecurityQuestion();
			}
		}
	}

	private void ResetSecurityQuestion()
	{
		for (int i = 0; i < 7; i++)
		{
			m_SecurityQuestion[i] = 0;
		}
		m_SecurityQuestionIndex = 0;
	}

	private void CheckPassword2()
	{
		if (!m_Door1Unlocked)
		{
			if (m_Password[0] == 2 && m_Password[1] == 2 && m_Password[2] == 2 && m_Password[3] == 2)
			{
				m_Door1Unlocked = true;
				g.m_SoundManager.Play(30);
			}
			else
			{
				ResetPassword();
			}
		}
	}

	private void ResetSafe()
	{
		for (int i = 0; i < 4; i++)
		{
			m_SafePassword[i] = 0;
		}
		m_SafePasswordIndex = 0;
	}

	private void CheckSafePassword()
	{
		if (m_SafeOpened)
		{
			return;
		}
		if (m_SafePassword[0] == 49 && m_SafePassword[1] == 56 && m_SafePassword[2] == 55 && m_SafePassword[3] == 55)
		{
			m_SafeOpened = true;
			g.m_ItemManager.FindObjectByType(18)?.PeerUseSafe(0, 255);
			g.m_SoundManager.Play(30);
			m_State = STATE.InGame;
			m_NearTrigger = TRIGGERS.NONE;
			string name = "TriggerSafe";
			MiscTriggerEntity obj = null;
			if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
			{
				obj.m_Complete = true;
			}
			Item item = g.m_ItemManager.FindObjectByType(22);
			if (item != null)
			{
				item.m_SceneObject.Visibility = ObjectVisibility.RenderedAndCastShadows;
			}
		}
		else
		{
			ResetSafe();
		}
	}

	private void CheckNetworkPassword()
	{
		if (!m_NetworkPasswordAnswered)
		{
			if (m_NetworkPassword[0] == 77 && m_NetworkPassword[1] == 65 && m_NetworkPassword[2] == 82 && m_NetworkPassword[3] == 73 && m_NetworkPassword[4] == 76 && m_NetworkPassword[5] == 89 && m_NetworkPassword[6] == 78)
			{
				m_NetworkPasswordAnswered = true;
				g.m_SoundManager.Play(30);
			}
			else
			{
				ResetNetworkPassword();
			}
		}
	}

	private void ResetNetworkPassword()
	{
		for (int i = 0; i < 7; i++)
		{
			m_NetworkPassword[i] = 0;
		}
		m_NetworkPasswordIndex = 0;
	}

	private void ResetIPAddress()
	{
		for (int i = 0; i < 3; i++)
		{
			m_IPAddress[i] = 0;
		}
		m_IPAddressIndex = 0;
	}

	private void CheckIPAddress()
	{
		if (!m_DoorCrewExitUnlocked)
		{
			if (m_IPAddress[0] == 55 && m_IPAddress[1] == 53 && m_IPAddress[2] == 57)
			{
				m_DoorCrewExitUnlocked = true;
				g.m_SoundManager.Play(30);
				m_State = STATE.InGame;
				m_NearTrigger = TRIGGERS.NONE;
			}
			else
			{
				ResetIPAddress();
			}
		}
	}

	public void DrawGrave()
	{
		_ = g.m_App.m_GameTime.TotalGameTime.TotalSeconds;
		for (int i = 0; i < 8; i++)
		{
			g.m_App.GraphicsDevice.SamplerStates[i] = SamplerState.PointClamp;
		}
		g.m_App.GraphicsDevice.SetRenderTarget(g.m_App.m_GraveRenderTarget);
		g.m_App.screenManager.SpriteBatch.Begin();
		g.m_App.GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 1f, 0);
		float num = 43f;
		Color color = new Color(0f, 0f, 0f, 0.75f);
		SpriteFont graveFont = g.m_App.graveFont;
		Vector2 position = new Vector2(0f, 0f);
		string text = "Here Lies";
		float num2 = 255f;
		position.X = (num2 - graveFont.MeasureString(text).X) / 2f;
		g.m_App.screenManager.SpriteBatch.DrawString(graveFont, text, position, color);
		position.Y += num;
		text = GetName();
		position.X = (num2 - graveFont.MeasureString(text).X) / 2f;
		g.m_App.screenManager.SpriteBatch.DrawString(graveFont, text, position, color);
		position.Y += num;
		text = "Died";
		position.X = (num2 - graveFont.MeasureString(text).X) / 2f;
		g.m_App.screenManager.SpriteBatch.DrawString(graveFont, text, position, color);
		position.Y += num;
		text = DateTime.Today.ToString("MMMM dd");
		position.X = (num2 - graveFont.MeasureString(text).X) / 2f;
		g.m_App.screenManager.SpriteBatch.DrawString(graveFont, text, position, color);
		position.Y += num - 3f;
		text = DateTime.Today.ToString("yyyy");
		position.X = (num2 - graveFont.MeasureString(text).X) / 2f;
		g.m_App.screenManager.SpriteBatch.DrawString(graveFont, text, position, color);
		position.Y += num;
		text = "R.I.P.";
		position.X = (num2 - graveFont.MeasureString(text).X) / 2f;
		g.m_App.screenManager.SpriteBatch.DrawString(graveFont, text, position, color);
		g.m_App.screenManager.SpriteBatch.End();
		g.m_App.GraphicsDevice.SetRenderTarget(null);
		if (g.m_App.m_GRAVE != null)
		{
			DeferredObjectEffect deferredObjectEffect = g.m_App.m_GRAVE.RenderableMeshes[0].Effect as DeferredObjectEffect;
			deferredObjectEffect.DiffuseMapTexture = g.m_App.m_GraveRenderTarget;
			deferredObjectEffect.TransparencyMode = TransparencyMode.Blend;
		}
	}

	public void DrawTablet()
	{
		_ = g.m_App.m_GameTime.TotalGameTime.TotalSeconds;
		for (int i = 0; i < 8; i++)
		{
			g.m_App.GraphicsDevice.SamplerStates[i] = SamplerState.PointClamp;
		}
		g.m_App.GraphicsDevice.SetRenderTarget(g.m_App.m_TabletRenderTarget);
		g.m_App.screenManager.SpriteBatch.Begin();
		g.m_App.GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 1f, 0);
		SpriteFont hudFont = g.m_App.hudFont;
		Vector2 position = new Vector2(0f, 10f);
		string text = "Research Data";
		float num = 255f;
		position.X = (num - hudFont.MeasureString(text).X) / 2f;
		g.m_App.screenManager.SpriteBatch.DrawString(hudFont, text, position, g.m_App.HUD_GREEN);
		float num2 = 25f;
		position.X = 10f;
		position.Y += num2;
		position.Y += num2;
		g.m_App.screenManager.SpriteBatch.DrawString(hudFont, $"{g.m_App.m_Rand.Next(0, 999999)},{g.m_App.m_Rand.Next(0, 999999)}", position, g.m_App.HUD_GREEN);
		position.Y += num2;
		g.m_App.screenManager.SpriteBatch.DrawString(hudFont, $"{g.m_App.m_Rand.Next(0, 999999)},{g.m_App.m_Rand.Next(0, 999999)}", position, g.m_App.HUD_GREEN);
		position.Y += num2;
		g.m_App.screenManager.SpriteBatch.DrawString(hudFont, $"{g.m_App.m_Rand.Next(0, 999999)},{g.m_App.m_Rand.Next(0, 999999)}", position, g.m_App.HUD_GREEN);
		position.Y += num2;
		g.m_App.screenManager.SpriteBatch.DrawString(hudFont, $"{g.m_App.m_Rand.Next(0, 999999)},{g.m_App.m_Rand.Next(0, 999999)}", position, g.m_App.HUD_GREEN);
		position.Y += num2;
		g.m_App.screenManager.SpriteBatch.DrawString(hudFont, $"{g.m_App.m_Rand.Next(0, 999999)},{g.m_App.m_Rand.Next(0, 999999)}", position, g.m_App.HUD_GREEN);
		position.Y += num2;
		g.m_App.screenManager.SpriteBatch.DrawString(hudFont, $"{g.m_App.m_Rand.Next(0, 999999)},{g.m_App.m_Rand.Next(0, 999999)}", position, g.m_App.HUD_GREEN);
		position.Y += num2;
		g.m_App.screenManager.SpriteBatch.DrawString(hudFont, $"{g.m_App.m_Rand.Next(0, 999999)},{g.m_App.m_Rand.Next(0, 999999)}", position, g.m_App.HUD_GREEN);
		position.Y += num2;
		g.m_App.screenManager.SpriteBatch.DrawString(hudFont, $"{g.m_App.m_Rand.Next(0, 999999)},{g.m_App.m_Rand.Next(0, 999999)}", position, g.m_App.HUD_GREEN);
		g.m_App.screenManager.SpriteBatch.End();
		g.m_App.GraphicsDevice.SetRenderTarget(null);
		Item item = g.m_ItemManager.FindObjectByType(27);
		if (item != null)
		{
			DeferredObjectEffect deferredObjectEffect = item.m_SceneObject.RenderableMeshes[1].Effect as DeferredObjectEffect;
			deferredObjectEffect.DiffuseMapTexture = g.m_App.m_TabletRenderTarget;
			if (g.m_App.m_Rand.NextDouble() > 0.10000000149011612)
			{
				deferredObjectEffect.EmissiveColor = Vector3.One;
			}
			else
			{
				deferredObjectEffect.EmissiveColor = Vector3.Zero;
			}
		}
	}

	public Player()
	{
		m_LAX = 0f;
		m_LAY = 0f;
		m_Id = -1;
		m_NetId = 255;
		m_Health = 100;
		m_bRequestDied = false;
		m_WeaponItemIndex = -1;
		m_ViewSceneObject = null;
		m_FrameMove = Vector3.Zero;
		m_PunchAngle = 0f;
		m_bFired = false;
		m_RequestSendImpact = false;
		m_RequestSendImpactPos = Vector3.Zero;
		m_RequestSendImpactNormal = Vector3.Zero;
		m_Bot = false;
		m_bTorchChanged = false;
		m_bWeaponChanged = false;
		m_bRequestSendDamage = false;
		m_RequestedDamageAmount = 0;
		m_RequestedPlayerToDamageNetID = 255;
		m_RequestedHitZone = byte.MaxValue;
		m_RequestedAttacker = 255;
		m_RequestedProjectileNetId = 255;
		m_LastAttackerNetId = 255;
		m_LastProjectileNetId = 255;
		m_RespawnTimer = 0f;
		m_Crouch = false;
		m_RequestSendCrouch = false;
		m_CrouchY = 0f;
		m_SpinePitch = 0f;
		m_Kills = 0;
		m_Deaths = 0;
		m_XP = 0;
		m_Rank = 1;
		m_XPForNextRank = 0;
		m_Score = 0;
		m_RequestSendScore = false;
		m_AnimChanged = false;
		m_HasAmmoToGive = false;
		m_RequestRankUp = false;
		m_RequestCleanItems = false;
		m_ChangeTeamTime = 0f;
		m_ScanningProgress = 0f;
		m_ScanPressed = false;
		m_AnimUpperChanged = false;
		m_AttachedItemId = -1;
		m_ArtifactEscapeAirlockId = byte.MaxValue;
		m_UsedCrate = new bool[22];
		m_TempBotNode = new BotNode[1024];
		for (int i = 0; i < 1024; i++)
		{
			m_TempBotNode[i] = default(BotNode);
			m_TempBotNode[i].m_Type = -1;
		}
	}

	public void SetViewModel()
	{
		switch (m_Class)
		{
		case CLASS.FatherD:
		case CLASS.Molly:
			m_ViewModel = g.m_PlayerManager.m_ViewModel;
			break;
		}
		m_ViewAnimationSet = new AnimationSet(1, m_ViewModel.SkeletonBones);
		PlayViewAnim(0, loop: true, 0f);
		m_ViewSceneObject = new SceneObject(m_ViewModel.Model)
		{
			UpdateType = UpdateType.Automatic,
			Visibility = ObjectVisibility.Rendered,
			StaticLightingType = StaticLightingType.Composite,
			CollisionType = CollisionType.None,
			AffectedByGravity = false,
			Name = $"Player{m_Id}",
			World = Matrix.Identity
		};
		g.m_App.sceneInterface.ObjectManager.Submit(m_ViewSceneObject);
		m_ViewAnimationSet.TranslationInterpolation = InterpolationMode.Linear;
		m_ViewAnimationSet.OrientationInterpolation = InterpolationMode.Linear;
		m_ViewAnimationSet.ScaleInterpolation = InterpolationMode.None;
		foreach (RenderableMesh renderableMesh in m_ViewSceneObject.RenderableMeshes)
		{
			BoundingBox meshBoundingBox = renderableMesh.MeshBoundingBox;
			renderableMesh.MeshBoundingBox = new BoundingBox(new Vector3(meshBoundingBox.Min.Z, meshBoundingBox.Min.Y + 2.159f - 1f, meshBoundingBox.Min.X), new Vector3(meshBoundingBox.Max.Z, meshBoundingBox.Max.Y + 2.159f + 1f, meshBoundingBox.Max.X));
		}
		m_ViewSceneObject.CalculateBounds();
		SetWeapon(m_StartWeaponItemIndex);
	}

	public void SpawnLocal()
	{
		SetViewModel();
		Spawn();
	}

	public void Spawn()
	{
		SetState(STATE.InGame);
		Vector3 pos = Vector3.Zero;
		float rotY = 0f;
		int num = -1;
		if (m_Team == TEAM.Hunter)
		{
			num = 0;
		}
		else if (m_Team == TEAM.Vampire)
		{
			num = 1;
		}
		else if (m_Team == TEAM.MedBay)
		{
			num = 2;
		}
		else if (m_Team == TEAM.OxygenTanks)
		{
			num = 3;
		}
		else if (m_Team == TEAM.CargoBay)
		{
			num = 5;
		}
		if (num != -1)
		{
			if (g.m_App.m_CheckpointId != -1 && !m_Bot)
			{
				g.m_PlayerManager.FindCheckpointPos(g.m_App.m_CheckpointId, out pos, out rotY);
				m_Door0Unlocked = true;
				if (g.m_App.m_CheckpointId == 1)
				{
					ResetCheckpoint1Triggers();
				}
				if (g.m_App.m_CheckpointId == 3)
				{
					ResetCheckpoint3Triggers();
				}
			}
			else if (num == 5)
			{
				g.m_PlayerManager.FindCargoSpawnPoint(out pos, out rotY);
			}
			else
			{
				m_SpawnId = g.m_PlayerManager.FindSpawnPos(num, out pos, out rotY);
			}
			if (m_bRagdoll)
			{
				DisableRagdoll();
			}
			m_Position = pos;
			m_NetworkPosition = pos;
			m_Rotation.Y = rotY;
			m_BotTargetRotY = rotY;
			m_NetworkRotation = rotY;
			m_PrevPosition = pos;
			m_CharacterController.Body.Position = m_Position;
			if (IsHost() && m_Bot)
			{
				if (m_Team != TEAM.Vampire)
				{
					UseSpindlePath();
				}
				else
				{
					m_CurrentPathId = 0;
				}
			}
		}
		EnableCollisionAndGravity();
		m_InvincibilityTime = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 3f;
		ResetAllAmmo();
		SetWeapon(m_StartWeaponItemIndex);
		m_bRequestSendSpawn = true;
		if (m_Crouch)
		{
			Crouch();
		}
		m_BloodSpray.Enabled = false;
		m_BloodSpray.Visible = false;
		if (m_Team == TEAM.Vampire)
		{
			m_CharacterController.JumpSpeed = 20f;
			m_CharacterController.SlidingJumpSpeed = 20f;
		}
		else
		{
			m_CharacterController.JumpSpeed = 9f;
			m_CharacterController.SlidingJumpSpeed = 9f;
		}
		m_bStaked = false;
		m_TargetNode = -1;
		if (IsLocalPlayer() && !m_Bot)
		{
			if (g.m_App.m_CheckpointId == -1)
			{
				g.m_App.AddHelmetMessage(HelmetMessage.TYPE.AIRLOCK, string.Format("Saturn 9", m_SpawnId), new Vector2(285f, 100f), 5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: false);
			}
			if (g.m_App.SOUNDON)
			{
				m_BreatheSFX = g.m_SoundManager.PlayLooped(24);
				m_BreatheSFX.Volume = 0.75f;
			}
		}
		if (!m_Bot)
		{
			g.m_CameraManager.m_Pitch = 0f;
			g.m_CameraManager.m_ZTilt = 0f;
		}
		m_Anim = byte.MaxValue;
		if (!m_Bot)
		{
			if (g.m_App.m_CheckpointId == 2)
			{
				g.m_CameraManager.m_ShakeyCam = false;
				PlayViewAnim(0, loop: true, 0.2f);
				HallucinateOn();
			}
			else
			{
				HallucinateOff(rumble: false);
			}
		}
		m_Sprint = 150;
		m_bSprinting = false;
	}

	public void PeerSpawned(Vector3 pos)
	{
		if (m_State != STATE.Intermission)
		{
			if (m_bRagdoll)
			{
				DisableRagdoll();
			}
			m_Position = pos;
			m_NetworkPosition = pos;
			if (m_SceneObject != null)
			{
				m_SceneObject.Visibility = ObjectVisibility.Rendered;
			}
			SetWeapon(m_Weapons[0]);
			m_bStaked = false;
			m_Anim = 0;
			m_AnimationSet.CrossFade(m_Model.AnimationClips.Values[m_Anim], TimeSpan.FromSeconds(0.20000000298023224));
		}
	}

	public bool IsLocalPlayer()
	{
		if (g.m_App.m_NetworkSession == null)
		{
			return true;
		}
		if (((ReadOnlyCollection<LocalNetworkGamer>)(object)g.m_App.m_NetworkSession.LocalGamers).Count > 0 && (m_NetId & 0xFF) == ((NetworkGamer)((ReadOnlyCollection<LocalNetworkGamer>)(object)g.m_App.m_NetworkSession.LocalGamers)[0]).Id)
		{
			return true;
		}
		return false;
	}

	public void Delete()
	{
		if (m_Id != -1)
		{
			CheckDetachItem();
		}
		m_Id = -1;
		m_NetId = -1;
		m_Bot = false;
		m_RequestDelete = false;
		if (m_bRagdoll)
		{
			DisableRagdoll();
		}
		for (int i = 0; i < 6; i++)
		{
			if (m_Weapons[i] != -1)
			{
				g.m_ItemManager.Delete(m_Weapons[i]);
				m_Weapons[i] = -1;
			}
		}
		m_WeaponItemIndex = -1;
		if (m_CharacterController != null)
		{
			m_CharacterController.m_PlayerIdx = -1;
			g.m_App.m_Space.Remove(m_CharacterController.Body);
			m_CharacterController = null;
		}
		if (m_ViewSceneObject != null)
		{
			g.m_App.sceneInterface.ObjectManager.Remove(m_ViewSceneObject);
			m_ViewSceneObject = null;
		}
		if (m_SceneObject != null)
		{
			g.m_App.sceneInterface.ObjectManager.Remove(m_SceneObject);
			m_SceneObject = null;
		}
		if (m_TorchLight != null)
		{
			g.m_App.sceneInterface.LightManager.Remove(m_TorchLight);
			m_TorchLight = null;
		}
		if (m_TorchPointLight != null)
		{
			g.m_App.sceneInterface.LightManager.Remove(m_TorchPointLight);
			m_TorchPointLight = null;
		}
		if (m_TorchHelmetLight != null)
		{
			g.m_App.sceneInterface.LightManager.Remove(m_TorchHelmetLight);
			m_TorchHelmetLight = null;
		}
		DestroyHitZones();
		if (m_BreatheSFX != null)
		{
			m_BreatheSFX.Stop();
			m_BreatheSFX = null;
		}
		if (m_PhoneSFX != null)
		{
			m_PhoneSFX.Stop();
			m_PhoneSFX = null;
		}
		if (m_HeartBeatSFX != null)
		{
			m_HeartBeatSFX.Stop();
			m_HeartBeatSFX = null;
		}
		if (m_TickTockSFX != null)
		{
			m_TickTockSFX.Stop();
			m_TickTockSFX = null;
		}
		if (m_OceanSFX != null)
		{
			m_OceanSFX.Stop();
			m_OceanSFX = null;
		}
	}

	private void CheckDetachItem()
	{
		if (g.m_App.m_NetworkSession == null || !g.m_App.m_NetworkSession.IsHost || m_AttachedItemId == -1)
		{
			return;
		}
		GameScreen[] screens = g.m_App.screenManager.GetScreens();
		for (int i = 0; i < screens.Length; i++)
		{
			if (screens[i] is GameplayScreen && screens[i] is GameplayScreen gameplayScreen)
			{
				gameplayScreen.HostDropArtifactForPlayer(this);
			}
		}
	}

	public void RequestDelete()
	{
		m_RequestDelete = true;
	}

	public void CleanItems()
	{
		if (m_bRagdoll)
		{
			DisableRagdoll();
		}
		for (int i = 0; i < 6; i++)
		{
			if (m_Weapons[i] != -1)
			{
				g.m_ItemManager.Delete(m_Weapons[i]);
				m_Weapons[i] = -1;
			}
		}
		m_WeaponItemIndex = -1;
		if (m_ViewSceneObject != null)
		{
			g.m_App.sceneInterface.ObjectManager.Remove(m_ViewSceneObject);
			m_ViewSceneObject = null;
		}
		if (m_SceneObject != null)
		{
			g.m_App.sceneInterface.ObjectManager.Remove(m_SceneObject);
			m_SceneObject = null;
		}
		DestroyHitZones();
		m_Leap = false;
		m_Jump = false;
		if (m_Crouch)
		{
			Crouch();
		}
		m_FrameMove = Vector3.Zero;
		m_PunchAngle = 0f;
		m_bFired = false;
		m_RequestSendImpact = false;
		m_RequestSendImpactPos = Vector3.Zero;
		m_RequestSendImpactNormal = Vector3.Zero;
		m_bTorchChanged = false;
		m_bWeaponChanged = false;
		m_bRequestDied = false;
		m_bRequestSendDamage = false;
		m_RequestedDamageAmount = 0;
		m_RequestedPlayerToDamageNetID = 255;
		m_RequestedHitZone = byte.MaxValue;
		m_RequestedAttacker = 255;
		m_RequestedProjectileNetId = 255;
		m_LastAttackerNetId = 255;
		m_LastProjectileNetId = 255;
		m_RespawnTimer = 0f;
		m_RequestSendCrouch = false;
		m_SpinePitch = 0f;
		m_Kills = 0;
		m_Deaths = 0;
		m_Score = 0;
		m_RequestSendScore = false;
		m_AnimChanged = false;
		m_Anim = byte.MaxValue;
		m_AnimUpperChanged = false;
		m_AnimUpper = byte.MaxValue;
		m_State = STATE.Intermission;
		m_Team = TEAM.None;
		m_Class = CLASS.None;
		m_bRequestSendSpawn = false;
		m_RequestRankUp = false;
		m_RequestUseCrate = false;
		m_RequestUseCrateId = -1;
		m_ScanningProgress = 0f;
		m_ScanPressed = false;
		m_AttachedItemId = -1;
		if (m_BreatheSFX != null)
		{
			m_BreatheSFX.Stop();
			m_BreatheSFX = null;
		}
		if (m_PhoneSFX != null)
		{
			m_PhoneSFX.Stop();
			m_PhoneSFX = null;
		}
		if (m_HeartBeatSFX != null)
		{
			m_HeartBeatSFX.Stop();
			m_HeartBeatSFX = null;
		}
		if (m_TickTockSFX != null)
		{
			m_TickTockSFX.Stop();
			m_TickTockSFX = null;
		}
		if (m_OceanSFX != null)
		{
			m_OceanSFX.Stop();
			m_OceanSFX = null;
		}
		m_RequestCreateProjectile = false;
		m_RequestCreateProjectileNetId = 255;
		m_RequestCreateProjectileType = byte.MaxValue;
		m_NextRecoverTime = 0f;
		m_PeerPitch = 0f;
		m_HasLocatorDevice = false;
		m_NextLocatorTime = 0f;
		m_DrawLocator = false;
		ClearUsedCrates();
		m_Hallucinate = false;
		m_NumSaws = 0;
		g.m_App.m_CheckpointId = -1;
		m_Sprint = 150;
		m_bSprinting = false;
	}

	public void Render()
	{
	}

	public void Update()
	{
		switch (m_State)
		{
		case STATE.Intermission:
			return;
		case STATE.JoinTeam:
		{
			bool flag = false;
			bool flag2 = false;
			GameScreen[] screens = g.m_App.screenManager.GetScreens();
			if (g.m_App.m_ForceJoin)
			{
				AutoChooseTeam();
				if (m_Team != 0)
				{
					SetClass(CLASS.FatherD);
					if (GetClass() != 0)
					{
						SpawnLocal();
						g.m_SoundManager.Play(16);
					}
				}
				g.m_App.m_ForceJoin = false;
				break;
			}
			for (int i = 0; i < screens.Length; i++)
			{
				if (screens[i] is JoinTeamMenuScreen)
				{
					flag = true;
				}
			}
			for (int j = 0; j < screens.Length; j++)
			{
				if (screens[j] is TutorialMenuScreen)
				{
					flag2 = true;
				}
			}
			if (g.m_App.m_NetworkSession == null)
			{
				if (!flag2 && !g.m_App.m_bTutorialDone)
				{
					g.m_App.screenManager.AddScreen(new TutorialMenuScreen(), screens[0].ControllingPlayer);
				}
				else if (!flag)
				{
					g.m_App.screenManager.AddScreen(new JoinTeamMenuScreen(g.m_App.m_NetworkSession), screens[0].ControllingPlayer);
				}
			}
			else if (!flag)
			{
				g.m_App.screenManager.AddScreen(new JoinTeamMenuScreen(g.m_App.m_NetworkSession), screens[0].ControllingPlayer);
			}
			break;
		}
		case STATE.InGame:
			UpdateInGame();
			UpdateHostBot();
			UpdatePeer();
			UpdateWeaponToBone();
			UpdateTorchLight();
			UpdateAttachedItem();
			UpdateSprintArm();
			break;
		case STATE.Grabbed:
			UpdateHostBot();
			UpdatePeer();
			UpdateWeaponToBone();
			UpdateTorchLight();
			UpdateSprintArm();
			break;
		case STATE.LocalDeath:
			UpdateLocalDeath();
			UpdateHostBot();
			break;
		case STATE.UsingLCD1:
			UpdateUsingLCD1();
			UpdateWeaponToBone();
			UpdateTorchLight();
			break;
		case STATE.UsingLCD2:
			UpdateUsingLCD2();
			UpdateWeaponToBone();
			UpdateTorchLight();
			break;
		case STATE.UsingLCD3:
			UpdateUsingLCD3();
			UpdateWeaponToBone();
			UpdateTorchLight();
			break;
		case STATE.UsingSafe:
			UpdateUsingSafe();
			UpdateWeaponToBone();
			UpdateTorchLight();
			break;
		case STATE.UsingLCD4:
			UpdateUsingLCD4();
			UpdateWeaponToBone();
			UpdateTorchLight();
			break;
		}
		m_Rotation.Y = MathHelper.WrapAngle(m_Rotation.Y);
		m_FrameMove = m_Position - m_PrevPosition;
		if (m_Position.Y < -100f && m_CharacterController != null)
		{
			Spawn();
		}
		if (m_RequestDelete)
		{
			Delete();
		}
	}

	public void UpdateInGame()
	{
		if (IsLocalPlayer() && !m_Bot)
		{
			float num = 60f * (float)g.m_App.m_GameTime.ElapsedGameTime.TotalSeconds;
			m_Position.X = m_CharacterController.Body.Position.X;
			m_Position.Z = m_CharacterController.Body.Position.Z;
			m_Position.Y = MathHelper.Lerp(m_Position.Y, m_CharacterController.Body.Position.Y, 0.3f);
			m_Rotation.Y += (0f - m_Turn) * 0.04f * num * (g.m_App.m_OptionsHoriz * 0.4f);
			Matrix matrix = Matrix.CreateRotationY(m_Rotation.Y);
			_ = matrix.Forward;
			m_ArmRot.Y = Fn.Clerp(m_ArmRot.Y, m_Rotation.Y, 0.4f);
			Matrix matrix2 = Matrix.CreateRotationY(m_ArmRot.Y);
			Vector3 vector = new Vector3(m_Movement.X, 0f, m_Movement.Y);
			Vector3 vector2 = Vector3.Transform(vector * 0.05f, matrix);
			vector2 *= num;
			Matrix matrix3 = Matrix.CreateTranslation(m_Position + vector2);
			Matrix matrix4 = Matrix.CreateTranslation(m_Position);
			m_Position = (matrix * matrix3).Translation;
			float num2 = m_Movement.Length();
			if (num2 > 0.5f && m_Crouch)
			{
				Crouch();
			}
			if (!m_bSprinting && m_Sprint < 150 && m_SprintRegenTime < (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds)
			{
				m_Sprint++;
				m_SprintRegenTime = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 1f / 15f;
			}
			if (m_Hallucinate)
			{
				m_CharacterController.HorizontalMotionConstraint.Speed = 2f * num2 * num;
			}
			else if (m_bSprinting && m_Sprint > 2)
			{
				m_CharacterController.HorizontalMotionConstraint.Speed = 7f * num2 * num;
			}
			else
			{
				m_CharacterController.HorizontalMotionConstraint.Speed = 4f * num2 * num;
			}
			m_CharacterController.HorizontalMotionConstraint.MovementDirection = new Vector2(vector2.X, vector2.Z);
			m_CharacterController.Body.OrientationMatrix = Matrix3X3.CreateFromMatrix(matrix);
			if (m_Jump)
			{
				m_CharacterController.Jump();
			}
			if (m_Leap && m_Team == TEAM.Vampire)
			{
				m_CharacterController.Leap();
			}
			matrix4.Translation += new Vector3(0f, -2.159f, 0f);
			m_ViewSceneObject.World = matrix2 * matrix4;
			UpdateViewAnimation();
			m_ArmRot.X = Fn.Clerp(m_ArmRot.X, g.m_CameraManager.m_Pitch, 0.4f);
			Matrix angles = Matrix.CreateRotationY(3.14f) * Matrix.CreateRotationZ(1.57f) * Matrix.CreateRotationX(m_ArmRot.X + MathHelper.ToRadians(-14f));
			m_ViewAnimationSet.SetBoneController("Bip01_Neck", ref angles);
			if (g.m_CameraManager.m_ShakeyCam && m_CurrentViewAnim == 1)
			{
				Vector3 vector3 = m_Position - m_PrevPosition;
				vector3.Y = 0f;
				float val = vector3.Length() * 10f;
				val = Math.Min(Math.Max(0f, val), 1f);
				m_ViewAnimationSet.Speed = MathHelper.Lerp(m_ViewAnimationSet.Speed, val, 0.1f);
			}
			else
			{
				m_ViewAnimationSet.Speed = 1f;
			}
			Matrix identity = Matrix.Identity;
			m_ViewAnimationSet.Update(g.m_App.m_GameTime.ElapsedGameTime, identity);
			m_ViewSceneObject.SkinBones = m_ViewAnimationSet.SkinnedBoneTransforms;
			m_PunchAngle = MathHelper.Lerp(m_PunchAngle, 0f, 0.2f);
			if (IsHost())
			{
				UpdateHitZones();
			}
			UpdateLocalAnimForPeers(m_Position - m_PrevPosition);
			CheckPeerCollision();
			CheckMiscTriggers();
			UpdatePromptsRequired();
		}
	}

	public void PlayLeapSFX()
	{
	}

	public void PlayJumpSFX()
	{
	}

	private void UpdateHostBot()
	{
		bool flag = false;
		if (m_Bot && IsHost())
		{
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		if (m_Team == TEAM.Vampire && m_bRagdoll)
		{
			UpdateAlienDeath();
			return;
		}
		if (m_bStaked)
		{
			m_CharacterController.HorizontalMotionConstraint.Speed = 0f;
			m_CharacterController.HorizontalMotionConstraint.CrouchingSpeed = 0f;
			return;
		}
		m_Position = m_CharacterController.Body.Position;
		if (m_Class != CLASS.Edgar && m_Class != CLASS.MedBay && m_Class != CLASS.OxygenTanks)
		{
			UpdateBot();
		}
		if (m_BotSpeed > 0f && m_Crouch)
		{
			Crouch();
		}
		if (m_Crouch)
		{
			m_CharacterController.HorizontalMotionConstraint.CrouchingSpeed = 0f;
			RecoverHealthFromCrouching();
		}
		else if (m_BotSpeed > 0f)
		{
			m_CharacterController.HorizontalMotionConstraint.Speed = m_BotSpeed;
			m_CharacterController.HorizontalMotionConstraint.MovementDirection = new Vector2(m_BotVecMove.X, m_BotVecMove.Z);
		}
		else
		{
			_ = m_BotSpeed;
			_ = 0f;
		}
		Matrix matrix = Matrix.CreateTranslation(m_Position);
		if (m_BotSpeed != 0f)
		{
			m_Position += m_BotVecMove;
		}
		m_Rotation.Y = Fn.Clerp(m_Rotation.Y, m_BotTargetRotY, 0.2f);
		m_Rotation.Y = MathHelper.WrapAngle(m_Rotation.Y);
		if (m_Crouch)
		{
			m_CrouchY = MathHelper.Lerp(m_CrouchY, 0.68f, 0.2f);
		}
		else
		{
			m_CrouchY = MathHelper.Lerp(m_CrouchY, 0f, 0.2f);
		}
		matrix.Translation += new Vector3(0f, -2.159f, 0f);
		Matrix matrix2 = Matrix.CreateRotationY(m_Rotation.Y);
		m_SceneObject.World = matrix2 * matrix;
		m_CharacterController.Body.OrientationMatrix = Matrix3X3.CreateFromMatrix(matrix2);
		if (m_SceneObject.Visibility != 0)
		{
			m_AnimationSet.Update(g.m_App.m_GameTime.ElapsedGameTime, Matrix.Identity);
			m_AnimationSet.CopyCombinedBoneTransforms(m_SceneObject);
		}
		if (m_Leap)
		{
			m_CharacterController.Leap();
		}
		if (m_Class != CLASS.Edgar && m_Class != CLASS.MedBay && m_Class != CLASS.OxygenTanks)
		{
			UpdateBotAnimation(m_Position - m_PrevPosition);
		}
		if (m_Class == CLASS.Edgar)
		{
			UpdateBotSpaceman();
		}
		if (m_Class == CLASS.MedBay)
		{
			UpdateBotMedbay();
		}
		if (m_Class == CLASS.OxygenTanks)
		{
			UpdateBotOxygenTanks();
		}
		UpdateSpinePitch();
		UpdateHitZones();
	}

	private void UpdatePeer()
	{
		bool flag = false;
		if (m_Bot && IsHost())
		{
			flag = true;
		}
		if (IsLocalPlayer() || flag || m_SceneObject == null)
		{
			return;
		}
		if (m_bRagdoll)
		{
			UpdateRagdoll();
		}
		m_Position.X = MathHelper.Lerp(m_Position.X, m_NetworkPosition.X, 0.25f);
		m_Position.Y = MathHelper.Lerp(m_Position.Y, m_NetworkPosition.Y, 0.25f);
		m_Position.Z = MathHelper.Lerp(m_Position.Z, m_NetworkPosition.Z, 0.25f);
		m_Rotation.Y = Fn.Clerp(m_Rotation.Y, m_NetworkRotation, 0.3f);
		m_Rotation.Y = MathHelper.WrapAngle(m_Rotation.Y);
		if (m_Crouch)
		{
			m_CrouchY = MathHelper.Lerp(m_CrouchY, 0.68f, 0.2f);
		}
		else
		{
			m_CrouchY = MathHelper.Lerp(m_CrouchY, 0f, 0.2f);
		}
		Matrix matrix = Matrix.CreateTranslation(m_Position);
		matrix.Translation += new Vector3(0f, -2.159f + m_CrouchY, 0f);
		Matrix matrix2 = Matrix.CreateRotationY(m_Rotation.Y);
		m_SceneObject.World = matrix2 * matrix;
		if (m_SceneObject.Visibility != 0)
		{
			if (m_Anim == RunAnimByWeapon() || m_Anim == RunBackAnimByWeapon() || m_Anim == RunBackAnimByWeapon() || m_Anim == StrafeLeftAnimByWeapon() || m_Anim == StrafeRightAnimByWeapon())
			{
				Vector3 vector = m_Position - m_PrevPosition;
				vector.Y = 0f;
				float val = vector.Length() * 20f;
				val = Math.Min(Math.Max(0f, val), 1f);
				if (val < 1f)
				{
					val *= 0.5f;
				}
				m_AnimationSet.Speed = MathHelper.Lerp(val, m_AnimationSet.Speed, 0.5f);
			}
			else
			{
				m_AnimationSet.Speed = 1f;
			}
			m_AnimationSet.Update(g.m_App.m_GameTime.ElapsedGameTime, Matrix.Identity);
			m_AnimationSet.CopyCombinedBoneTransforms(m_SceneObject);
		}
		UpdateHitZones();
		UpdatePeerSpinePitch();
		UpdateSpinePitch();
		UpdatePeerFootsteps();
	}

	private void UpdateAlienDeath()
	{
		if (m_Anim != 6)
		{
			m_AnimationSet.CrossFade(m_Model.AnimationClips.Values[6], TimeSpan.FromSeconds(0.20000000298023224));
			m_AnimationSet.LoopEnabled = false;
			m_Anim = 6;
			m_AnimChanged = true;
		}
		m_CharacterController.Body.LinearVelocity = Vector3.Zero;
		m_CharacterController.HorizontalMotionConstraint.MovementDirection = Vector2.Zero;
		m_CharacterController.HorizontalMotionConstraint.Speed = 0f;
		m_CharacterController.HorizontalMotionConstraint.CrouchingSpeed = 0f;
		if (m_SceneObject.Visibility != 0)
		{
			m_AnimationSet.Update(g.m_App.m_GameTime.ElapsedGameTime, Matrix.Identity);
			m_AnimationSet.CopyCombinedBoneTransforms(m_SceneObject);
		}
	}

	public void ClearMovement()
	{
		m_Movement = Vector2.Zero;
		m_Turn = 0f;
		if (m_CharacterController != null && m_CharacterController.Body != null)
		{
			m_CharacterController.Body.LinearVelocity = Vector3.Zero;
			m_CharacterController.HorizontalMotionConstraint.MovementDirection = Vector2.Zero;
			m_CharacterController.HorizontalMotionConstraint.Speed = 0f;
			m_CharacterController.HorizontalMotionConstraint.CrouchingSpeed = 0f;
		}
	}

	private void StartSpinePitch(float pitch)
	{
		if (m_AnimationSet != null)
		{
			m_SpinePitchMatrix = m_AnimationSet.GetBoneAbsoluteTransform("Bip01_Spine1") * Matrix.CreateRotationX(0f - m_SpinePitch);
			m_SpinePitch = pitch - m_SpinePitch;
		}
	}

	private void UpdatePeerSpinePitch()
	{
		if (m_Team != TEAM.Vampire)
		{
			if (!m_bRagdoll)
			{
				Matrix angles = Matrix.CreateRotationY(0f - m_PeerPitch);
				m_AnimationSet.SetBoneControllerAdditive("Bip01_Spine1", ref angles);
			}
			else
			{
				m_AnimationSet.ClearBoneController("Bip01_Spine1");
			}
		}
	}

	private void UpdateSpinePitch()
	{
		if (m_Team == TEAM.Vampire)
		{
			return;
		}
		if (!m_bRagdoll)
		{
			m_SpinePitch = MathHelper.Lerp(m_SpinePitch, 0f, 0.2f);
			if (m_SpinePitch > 0.01f)
			{
				Matrix angles = Matrix.CreateRotationY(0f - (m_SpinePitch + m_PeerPitch));
				m_AnimationSet.SetBoneControllerAdditive("Bip01_Spine1", ref angles);
			}
		}
		else
		{
			m_AnimationSet.ClearBoneController("Bip01_Spine1");
		}
	}

	public void FireWeapon(bool bDebounced)
	{
		if (m_WeaponItemIndex != -1)
		{
			if (!g.m_ItemManager.m_Item[m_WeaponItemIndex].Fire(bDebounced))
			{
				m_bFired = false;
			}
			else if (this == g.m_PlayerManager.GetLocalPlayer())
			{
				int weaponFireAnim = g.m_ItemManager.GetWeaponFireAnim(m_WeaponItemIndex);
				bool weaponAnimShouldLoop = g.m_ItemManager.GetWeaponAnimShouldLoop(m_WeaponItemIndex);
				PlayViewAnim(weaponFireAnim, weaponAnimShouldLoop, 0f);
				m_PunchAngle += g.m_ItemManager.GetWeaponRecoil(m_WeaponItemIndex);
			}
		}
	}

	public void SimulateFireWeapon()
	{
		if (m_WeaponItemIndex != -1 && m_State != STATE.Intermission)
		{
			g.m_ItemManager.m_Item[m_WeaponItemIndex].SimulateFire();
			StartSpinePitch(MathHelper.ToRadians(20f));
		}
	}

	public void SimulateWeaponImpact(Vector3 impactPos, Vector3 impactNormal)
	{
		if (m_WeaponItemIndex != -1 && m_State != STATE.Intermission)
		{
			g.m_ItemManager.m_Item[m_WeaponItemIndex].DoImpactVFXSFX(impactPos, impactNormal);
		}
	}

	public void UpdatePromptsRequired()
	{
		int num = g.m_ItemManager.FindNearbyItem(9, m_Position, m_ViewSceneObject.World.Forward, 42.25f);
		if (num != -1 && g.m_ItemManager.m_Item[num].m_Id != -1 && g.m_ItemManager.m_Item[num].m_CrateState == Item.CRATESTATE.CLOSED && m_ScanningProgress == 0f)
		{
			int crateIndexByItemIndex = g.m_ItemManager.GetCrateIndexByItemIndex(num);
			if (m_UsedCrate[crateIndexByItemIndex])
			{
				g.m_App.AddHelmetMessage(HelmetMessage.TYPE.CRATE_NEAR, "EMPTY", new Vector2(200f, 100f), 10f, g.m_App.HUD_GREEN, SoundManager.SFX.TextPrompt, null, hideSysMsg: false);
			}
			else
			{
				g.m_App.AddHelmetMessage(HelmetMessage.TYPE.CRATE_NEAR, "HOLD (A) TO OPEN", new Vector2(120f, 100f), 10f, g.m_App.HUD_GREEN, SoundManager.SFX.TextPrompt, null, hideSysMsg: true);
			}
		}
		else
		{
			g.m_App.RemoveHelmetMessage(HelmetMessage.TYPE.CRATE_NEAR);
		}
		if (!m_ScanPressed)
		{
			m_ScanningProgress = 0f;
			g.m_App.RemoveHelmetMessage(HelmetMessage.TYPE.SCANNING);
		}
		m_ScanPressed = false;
	}

	public void UpdateWeaponToBone()
	{
		if (m_WeaponItemIndex == -1 || g.m_ItemManager.m_Item[m_WeaponItemIndex].m_SceneObject == null)
		{
			return;
		}
		if (this == g.m_PlayerManager.GetLocalPlayer() && !m_Bot && !UPDATEFULLMODEL_DEBUG && m_ViewAnimationSet != null)
		{
			Matrix boneAbsoluteTransform = m_ViewAnimationSet.GetBoneAbsoluteTransform("Bone01");
			g.m_ItemManager.m_Item[m_WeaponItemIndex].m_SceneObject.World = boneAbsoluteTransform * m_ViewSceneObject.World;
			if (g.m_ItemManager.m_Item[m_WeaponItemIndex].m_MagazineSceneObject != null)
			{
				Matrix boneAbsoluteTransform2 = m_ViewAnimationSet.GetBoneAbsoluteTransform("Bone05");
				g.m_ItemManager.m_Item[m_WeaponItemIndex].m_MagazineSceneObject.World = boneAbsoluteTransform2 * m_ViewSceneObject.World;
			}
		}
		else if (m_AnimationSet != null)
		{
			Matrix boneAbsoluteTransform3 = m_AnimationSet.GetBoneAbsoluteTransform("Bone01");
			g.m_ItemManager.m_Item[m_WeaponItemIndex].m_SceneObject.World = boneAbsoluteTransform3 * m_SceneObject.World;
		}
	}

	public void UpdateAttachedItem()
	{
		if (!IsValid() || m_AttachedItemId == -1 || g.m_ItemManager.m_Item[m_AttachedItemId].m_SceneObject == null)
		{
			return;
		}
		if (this == g.m_PlayerManager.GetLocalPlayer() && !m_Bot)
		{
			Matrix boneAbsoluteTransform = m_ViewAnimationSet.GetBoneAbsoluteTransform("Bip01_Spine");
			boneAbsoluteTransform.Translation += new Vector3(-0.5f, 0f, 0f);
			boneAbsoluteTransform = Matrix.CreateRotationZ(MathHelper.ToRadians(90f)) * boneAbsoluteTransform;
			g.m_ItemManager.m_Item[m_AttachedItemId].m_SceneObject.World = boneAbsoluteTransform * m_ViewSceneObject.World;
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.RECEIVED_ARTIFACT, "YOU HAVE THE ARTIFACT", new Vector2(80f, 130f), 0.25f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.ESCAPE_AIRLOCK, $"ESCAPE TO AIRLOCK {m_ArtifactEscapeAirlockId}", new Vector2(95f, 150f), 0.25f, g.m_App.HUD_GREEN, SoundManager.SFX.END, null, hideSysMsg: true);
			if (IsLocalPlayer() && !m_Bot && g.m_PlayerManager.m_CountdownTimer > (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds)
			{
				g.m_App.AddHelmetMessage(HelmetMessage.TYPE.COUNTDOWN_STARTED, "WAIT HERE TO ESCAPE", new Vector2(90f, 230f), 0.25f, g.m_App.HUD_RED, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			}
		}
		else if (m_AnimationSet != null)
		{
			if (m_Team == TEAM.Vampire)
			{
				Matrix boneAbsoluteTransform2 = m_AnimationSet.GetBoneAbsoluteTransform("Bip01_Spine");
				boneAbsoluteTransform2.Translation += new Vector3(0f, 0.5f, 0f);
				boneAbsoluteTransform2 = Matrix.CreateRotationZ(MathHelper.ToRadians(90f)) * boneAbsoluteTransform2;
				g.m_ItemManager.m_Item[m_AttachedItemId].m_SceneObject.World = boneAbsoluteTransform2 * m_SceneObject.World;
			}
			else
			{
				Matrix boneAbsoluteTransform3 = m_AnimationSet.GetBoneAbsoluteTransform("Bip01_Spine");
				boneAbsoluteTransform3.Translation += new Vector3(-0.5f, 0f, 0f);
				boneAbsoluteTransform3 = Matrix.CreateRotationZ(MathHelper.ToRadians(90f)) * boneAbsoluteTransform3;
				g.m_ItemManager.m_Item[m_AttachedItemId].m_SceneObject.World = boneAbsoluteTransform3 * m_SceneObject.World;
			}
		}
	}

	public void DrawCompassToAirlockMarker()
	{
		if (m_ArtifactEscapeAirlockId == byte.MaxValue || m_AttachedItemId == -1)
		{
			return;
		}
		float num = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds;
		if (m_NextLocatorTime < num)
		{
			m_DrawLocator = !m_DrawLocator;
			if (!m_DrawLocator)
			{
				m_NextLocatorTime = num + 1f;
			}
			else
			{
				m_NextLocatorTime = num + 1f;
				g.m_SoundManager.Play(25);
			}
		}
		if (!m_DrawLocator)
		{
			return;
		}
		float value = m_NextLocatorTime - num;
		value = MathHelper.Clamp(value, 0f, 1f);
		TriggerEntity obj = null;
		string name = $"Team{1}_Spawn{m_ArtifactEscapeAirlockId}";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
		{
			Vector3 forward = m_ViewSceneObject.World.Forward;
			float num2 = (float)Math.Atan2(0f - forward.X, forward.Z);
			Vector3 vector = obj.World.Translation - m_Position;
			float num3 = (float)Math.Atan2(0f - vector.X, vector.Z);
			num2 -= num3;
			float num4 = MathHelper.ToDegrees(num2);
			if (num4 > 360f)
			{
				num4 -= 360f;
			}
			if (num4 < 0f)
			{
				num4 += 360f;
			}
			num4 *= 45f / 64f;
			num4 += 256f;
			if (num4 > 384f)
			{
				num4 -= 256f;
			}
			Rectangle value2 = new Rectangle((int)(num4 - 128f), 0, 256, 64);
			Rectangle destinationRectangle = new Rectangle(28, 60, 412, 64);
			if (vector.Y < -2f)
			{
				g.m_App.screenManager.SpriteBatch.Draw(g.m_App.markerTexture, destinationRectangle, value2, g.m_App.HUD_RED * value, 0f, Vector2.Zero, SpriteEffects.FlipVertically, 0f);
			}
			else if (vector.Y > 2f)
			{
				g.m_App.screenManager.SpriteBatch.Draw(g.m_App.markerTexture, destinationRectangle, value2, g.m_App.HUD_RED * value);
			}
			else
			{
				g.m_App.screenManager.SpriteBatch.Draw(g.m_App.markerTexture, destinationRectangle, value2, g.m_App.HUD_GREEN * value);
			}
			destinationRectangle = new Rectangle(28, 420, 412, 64);
			if (vector.Y < -2f)
			{
				g.m_App.screenManager.SpriteBatch.Draw(g.m_App.markerTexture, destinationRectangle, value2, g.m_App.HUD_RED * value, 0f, Vector2.Zero, SpriteEffects.FlipVertically, 0f);
			}
			else if (vector.Y > 2f)
			{
				g.m_App.screenManager.SpriteBatch.Draw(g.m_App.markerTexture, destinationRectangle, value2, g.m_App.HUD_RED * value);
			}
			else
			{
				g.m_App.screenManager.SpriteBatch.Draw(g.m_App.markerTexture, destinationRectangle, value2, g.m_App.HUD_GREEN * value);
			}
		}
	}

	public void DrawCompassLocator()
	{
		if (m_AttachedItemId != -1 || !m_HasLocatorDevice || m_ViewSceneObject == null)
		{
			return;
		}
		int artifactItemId = g.m_ItemManager.GetArtifactItemId();
		Vector3 zero = Vector3.Zero;
		if (artifactItemId == -1)
		{
			return;
		}
		zero = g.m_ItemManager.m_Item[artifactItemId].m_SceneObject.World.Translation;
		float num = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds;
		if (m_NextLocatorTime < num)
		{
			m_DrawLocator = !m_DrawLocator;
			if (!m_DrawLocator)
			{
				m_NextLocatorTime = num + 1f;
			}
			else
			{
				m_NextLocatorTime = num + 1f;
				g.m_SoundManager.Play(25);
			}
		}
		if (m_DrawLocator)
		{
			float value = m_NextLocatorTime - num;
			value = MathHelper.Clamp(value, 0f, 1f);
			Vector3 forward = m_ViewSceneObject.World.Forward;
			float num2 = (float)Math.Atan2(0f - forward.X, forward.Z);
			Vector3 vector = zero - m_Position;
			float num3 = (float)Math.Atan2(0f - vector.X, vector.Z);
			num2 -= num3;
			float num4 = MathHelper.ToDegrees(num2);
			if (num4 > 360f)
			{
				num4 -= 360f;
			}
			if (num4 < 0f)
			{
				num4 += 360f;
			}
			num4 *= 45f / 64f;
			num4 += 256f;
			if (num4 > 384f)
			{
				num4 -= 256f;
			}
			Rectangle value2 = new Rectangle((int)(num4 - 128f), 0, 256, 64);
			Rectangle destinationRectangle = new Rectangle(28, 60, 412, 64);
			if (vector.Y < -3f)
			{
				g.m_App.screenManager.SpriteBatch.Draw(g.m_App.markerTexture, destinationRectangle, value2, g.m_App.HUD_RED * value, 0f, Vector2.Zero, SpriteEffects.FlipVertically, 0f);
			}
			else if (vector.Y > 3f)
			{
				g.m_App.screenManager.SpriteBatch.Draw(g.m_App.markerTexture, destinationRectangle, value2, g.m_App.HUD_RED * value);
			}
			else
			{
				g.m_App.screenManager.SpriteBatch.Draw(g.m_App.markerTexture, destinationRectangle, value2, g.m_App.HUD_GREEN * value);
			}
			destinationRectangle = new Rectangle(28, 420, 412, 64);
			if (vector.Y < -3f)
			{
				g.m_App.screenManager.SpriteBatch.Draw(g.m_App.markerTexture, destinationRectangle, value2, g.m_App.HUD_RED * value, 0f, Vector2.Zero, SpriteEffects.FlipVertically, 0f);
			}
			else if (vector.Y > 3f)
			{
				g.m_App.screenManager.SpriteBatch.Draw(g.m_App.markerTexture, destinationRectangle, value2, g.m_App.HUD_RED * value);
			}
			else
			{
				g.m_App.screenManager.SpriteBatch.Draw(g.m_App.markerTexture, destinationRectangle, value2, g.m_App.HUD_GREEN * value);
			}
		}
	}

	public void InitTorchLight()
	{
		m_TorchLight = new SpotLight();
		m_TorchLight.LightingType = LightingType.RealTime;
		m_TorchLight.DiffuseColor = new Vector3(0.6f, 0.6f, 1f);
		m_TorchLight.Intensity = 2.5f + (float)g.m_App.m_OptionsBrightness * 0.2f;
		m_TorchLight.Radius = 40f;
		m_TorchLight.Angle = 60f;
		m_TorchLight.FalloffStrength = 1f;
		m_TorchLight.Enabled = true;
		m_TorchLight.ShadowType = ShadowType.AllObjects;
		m_TorchLight.UpdateType = UpdateType.Automatic;
		g.m_App.sceneInterface.LightManager.Submit(m_TorchLight);
		m_TorchPointLight = new PointLight();
		m_TorchPointLight.LightingType = LightingType.RealTime;
		m_TorchPointLight.DiffuseColor = new Vector3(0.6f, 0.6f, 1f);
		m_TorchPointLight.Intensity = 0.25f;
		m_TorchPointLight.Radius = 2f;
		m_TorchPointLight.Enabled = true;
		m_TorchPointLight.ShadowType = ShadowType.None;
		m_TorchPointLight.UpdateType = UpdateType.Automatic;
		g.m_App.sceneInterface.LightManager.Submit(m_TorchPointLight);
		m_TorchHelmetLight = new PointLight();
		m_TorchHelmetLight.LightingType = LightingType.RealTime;
		m_TorchHelmetLight.DiffuseColor = new Vector3(0.6f, 0.6f, 1f);
		m_TorchHelmetLight.Intensity = 0.25f;
		m_TorchHelmetLight.Radius = 2f;
		m_TorchHelmetLight.Enabled = true;
		m_TorchHelmetLight.ShadowType = ShadowType.None;
		m_TorchHelmetLight.UpdateType = UpdateType.Automatic;
		g.m_App.sceneInterface.LightManager.Submit(m_TorchHelmetLight);
	}

	public void UpdateTorchLight()
	{
		if (m_TorchLight == null || !m_TorchLight.Enabled)
		{
			return;
		}
		if (m_Hallucinate || m_State == STATE.Grabbed)
		{
			m_TorchLight.DiffuseColor = new Vector3(1f, 0.6f, 0.6f);
			m_TorchPointLight.DiffuseColor = new Vector3(1f, 0.6f, 0.6f);
			m_TorchHelmetLight.DiffuseColor = new Vector3(1f, 0.6f, 0.6f);
		}
		else
		{
			m_TorchLight.DiffuseColor = new Vector3(0.6f, 0.6f, 1f);
			m_TorchPointLight.DiffuseColor = new Vector3(0.6f, 0.6f, 1f);
			m_TorchHelmetLight.DiffuseColor = new Vector3(0.6f, 0.6f, 1f);
		}
		if (m_WeaponItemIndex != -1 && g.m_ItemManager.m_Item[m_WeaponItemIndex].m_SceneObject != null)
		{
			Matrix identity = Matrix.Identity;
			identity = g.m_ItemManager.m_Item[m_WeaponItemIndex].m_SceneObject.World;
			m_TorchLight.Position = identity.Translation + new Vector3(0f, 0.1f, 0f);
			Vector3 vector = identity.Translation + identity.Up * 0.3f + identity.Right * 0f + identity.Forward * 0.4f;
			Vector3 zero = Vector3.Zero;
			zero.X = MathHelper.Lerp(m_TorchPointLight.Position.X, vector.X, 0.5f);
			zero.Y = MathHelper.Lerp(m_TorchPointLight.Position.Y, vector.Y, 0.5f);
			zero.Z = MathHelper.Lerp(m_TorchPointLight.Position.Z, vector.Z, 0.5f);
			m_TorchPointLight.Position = zero;
			m_TorchHelmetLight.Position = m_Position + new Vector3(0f, 1.55f, 0f);
			Vector3 direction = identity.Translation + identity.Forward * 40f - m_TorchLight.Position;
			direction.Normalize();
			m_TorchLight.Direction = direction;
			if (m_State == STATE.Grabbed)
			{
				m_TorchLight.Position = g.m_CameraManager.m_Position;
				m_TorchLight.Direction = Vector3.Normalize(g.m_CameraManager.m_LookAt - m_TorchLight.Position);
			}
		}
	}

	public void ToggleTorchLight()
	{
		if (m_State != STATE.Intermission && m_Team == TEAM.Hunter && m_WeaponItemIndex != -1 && g.m_ItemManager.m_Item[m_WeaponItemIndex].m_Id != -1 && (g.m_ItemManager.m_Item[m_WeaponItemIndex].m_Type == 14 || g.m_ItemManager.m_Item[m_WeaponItemIndex].m_Type == 11 || g.m_ItemManager.m_Item[m_WeaponItemIndex].m_Type == 1))
		{
			m_TorchLight.Enabled = !m_TorchLight.Enabled;
			m_TorchPointLight.Enabled = !m_TorchPointLight.Enabled;
			g.m_SoundManager.Play(35);
		}
	}

	public void UpdateSprintArm()
	{
		if (!m_Bot && m_Id != -1 && m_ViewAnimationSet != null)
		{
			if (m_bSprinting && m_Sprint > 2 && !m_Hallucinate)
			{
				m_SprintArmAngle = MathHelper.Lerp(m_SprintArmAngle, 0.8f, 0.1f);
				Matrix angles = Matrix.CreateRotationX(m_SprintArmAngle);
				m_ViewAnimationSet.SetBoneControllerAdditive("Bip01_R_UpperArm", ref angles);
			}
			else
			{
				m_SprintArmAngle = MathHelper.Lerp(m_SprintArmAngle, 0f, 0.1f);
				Matrix angles2 = Matrix.CreateRotationX(m_SprintArmAngle);
				m_ViewAnimationSet.SetBoneControllerAdditive("Bip01_R_UpperArm", ref angles2);
			}
		}
	}

	public void UpdateState()
	{
	}

	public void Reset()
	{
		m_Position.X = 0f;
		m_Position.Y = 0f;
	}

	public void UpdateLocalAnimForPeers(Vector3 move)
	{
		if (move.LengthSquared() > 1E-06f)
		{
			int num = -1;
			num = ((Math.Abs(m_Movement.X) > Math.Abs(m_Movement.Y)) ? ((!(m_Movement.X > 0f)) ? 2 : 3) : ((m_Movement.Y > 0f) ? 1 : 0));
			Vector3 linearVelocity = m_CharacterController.Body.LinearVelocity;
			linearVelocity.Y = 0f;
			linearVelocity.LengthSquared();
			if (num == 0 && m_Anim != RunAnimByWeapon() && m_CharacterController.SupportFinder.HasTraction)
			{
				m_Anim = RunAnimByWeapon();
				m_AnimChanged = true;
			}
			else if (num == 1 && m_Anim != RunBackAnimByWeapon() && m_CharacterController.SupportFinder.HasTraction)
			{
				m_Anim = RunBackAnimByWeapon();
				m_AnimChanged = true;
			}
			else if (num == 2 && m_Anim != StrafeLeftAnimByWeapon() && m_CharacterController.SupportFinder.HasTraction)
			{
				m_Anim = StrafeLeftAnimByWeapon();
				m_AnimChanged = true;
			}
			else if (num == 3 && m_Anim != StrafeRightAnimByWeapon() && m_CharacterController.SupportFinder.HasTraction)
			{
				m_Anim = StrafeRightAnimByWeapon();
				m_AnimChanged = true;
			}
		}
		else if (m_Crouch)
		{
			if (m_Anim != CrouchAnimByWeapon())
			{
				m_Anim = CrouchAnimByWeapon();
				m_AnimChanged = true;
			}
		}
		else if (m_Anim != IdleAnimByWeapon())
		{
			m_Anim = IdleAnimByWeapon();
			m_AnimChanged = true;
		}
	}

	private byte RunAnimByWeapon()
	{
		return (Item.OBJ)g.m_ItemManager.m_Item[m_WeaponItemIndex].m_Type switch
		{
			Item.OBJ.TORCH => 1, 
			Item.OBJ.SAW => 1, 
			Item.OBJ.ARM => 1, 
			Item.OBJ.PISTOL => 11, 
			Item.OBJ.RPG => 18, 
			Item.OBJ.CLAWS => 1, 
			_ => 1, 
		};
	}

	private byte RunBackAnimByWeapon()
	{
		return (Item.OBJ)g.m_ItemManager.m_Item[m_WeaponItemIndex].m_Type switch
		{
			Item.OBJ.TORCH => 7, 
			Item.OBJ.SAW => 7, 
			Item.OBJ.ARM => 7, 
			Item.OBJ.PISTOL => 14, 
			Item.OBJ.RPG => 21, 
			Item.OBJ.CLAWS => 1, 
			_ => 7, 
		};
	}

	private byte CrouchAnimByWeapon()
	{
		return (Item.OBJ)g.m_ItemManager.m_Item[m_WeaponItemIndex].m_Type switch
		{
			Item.OBJ.TORCH => 2, 
			Item.OBJ.SAW => 2, 
			Item.OBJ.ARM => 2, 
			Item.OBJ.PISTOL => 17, 
			Item.OBJ.RPG => 24, 
			Item.OBJ.CLAWS => 1, 
			_ => 2, 
		};
	}

	private byte IdleAnimByWeapon()
	{
		return (Item.OBJ)g.m_ItemManager.m_Item[m_WeaponItemIndex].m_Type switch
		{
			Item.OBJ.TORCH => 0, 
			Item.OBJ.SAW => 0, 
			Item.OBJ.ARM => 0, 
			Item.OBJ.CLAWS => 0, 
			Item.OBJ.PISTOL => 12, 
			Item.OBJ.RPG => 19, 
			_ => 0, 
		};
	}

	private byte StrafeLeftAnimByWeapon()
	{
		return (Item.OBJ)g.m_ItemManager.m_Item[m_WeaponItemIndex].m_Type switch
		{
			Item.OBJ.TORCH => 8, 
			Item.OBJ.SAW => 8, 
			Item.OBJ.ARM => 8, 
			Item.OBJ.PISTOL => 15, 
			Item.OBJ.RPG => 22, 
			Item.OBJ.CLAWS => 1, 
			_ => 8, 
		};
	}

	private byte StrafeRightAnimByWeapon()
	{
		return (Item.OBJ)g.m_ItemManager.m_Item[m_WeaponItemIndex].m_Type switch
		{
			Item.OBJ.TORCH => 9, 
			Item.OBJ.SAW => 9, 
			Item.OBJ.ARM => 9, 
			Item.OBJ.PISTOL => 16, 
			Item.OBJ.RPG => 23, 
			Item.OBJ.CLAWS => 1, 
			_ => 9, 
		};
	}

	private byte ReloadAnimByWeapon()
	{
		return (Item.OBJ)g.m_ItemManager.m_Item[m_WeaponItemIndex].m_Type switch
		{
			Item.OBJ.TORCH => 6, 
			Item.OBJ.SAW => 6, 
			Item.OBJ.ARM => 6, 
			Item.OBJ.PISTOL => 13, 
			Item.OBJ.RPG => 20, 
			Item.OBJ.CLAWS => 1, 
			_ => 6, 
		};
	}

	public void UpdateLocalAnimUpperForPeers(byte anim)
	{
		m_AnimUpper = anim;
		m_AnimUpperChanged = true;
	}

	public void UpdateBotAnimation(Vector3 move)
	{
		if (m_bRagdoll)
		{
			return;
		}
		m_AnimationSet.LoopEnabled = true;
		move.LengthSquared();
		if (m_BotAction == BOTACTION.Caught)
		{
			if (m_Anim != 5)
			{
				m_AnimationSet.CrossFade(0, m_Model.AnimationClips.Values[5], TimeSpan.FromSeconds(0.20000000298023224));
				m_Anim = 5;
			}
			return;
		}
		if (m_BotAction != 0)
		{
			Vector3 linearVelocity = m_CharacterController.Body.LinearVelocity;
			linearVelocity.Y = 0f;
			linearVelocity.LengthSquared();
			if (m_Anim != 4 && m_CharacterController.SupportFinder.HasTraction)
			{
				float num = 0.2f;
				m_AnimationSet.CrossFade(0, m_Model.AnimationClips.Values[4], TimeSpan.FromSeconds(num));
				m_Anim = 4;
				m_AnimChanged = true;
			}
		}
		else if (m_Anim != IdleAnimByWeapon())
		{
			if (m_Crouch)
			{
				m_AnimationSet.CrossFade(0, m_Model.AnimationClips.Values[CrouchAnimByWeapon()], TimeSpan.FromSeconds(0.20000000298023224));
			}
			else
			{
				m_AnimationSet.CrossFade(0, m_Model.AnimationClips.Values[IdleAnimByWeapon()], TimeSpan.FromSeconds(0.20000000298023224));
			}
			m_Anim = 1;
			m_AnimChanged = true;
		}
		if (m_BotAction != 0 && m_CharacterController.SupportFinder.HasTraction)
		{
			PlayFootsteps();
		}
	}

	public void PlayUpperBodyReloadAnimation()
	{
		_ = g.m_ItemManager.m_Item[m_WeaponItemIndex].m_Type;
		m_AnimationSet.PlayUpperBodyAnim(m_Model.AnimationClips.Values[ReloadAnimByWeapon()], loop: false, TimeSpan.FromSeconds(0.10000000149011612));
		UpdateLocalAnimUpperForPeers(ReloadAnimByWeapon());
	}

	public void PeerSetAnimUpper(int anim)
	{
		if (m_State != STATE.Intermission)
		{
			m_AnimationSet.PlayUpperBodyAnim(m_Model.AnimationClips.Values[anim], loop: false, TimeSpan.FromSeconds(0.10000000149011612));
		}
	}

	public void UpdatePeerFootsteps()
	{
		if (m_Team == TEAM.Hunter && (m_Anim == RunAnimByWeapon() || m_Anim == RunBackAnimByWeapon() || m_Anim == StrafeLeftAnimByWeapon() || m_Anim == StrafeRightAnimByWeapon()))
		{
			PlayFootsteps();
		}
	}

	private void PlayFootsteps()
	{
		float num = (float)m_AnimationSet.Time.TotalSeconds;
		float num2 = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds;
		float num3 = 0.4f;
		float num4 = 0.08f;
		if (((num > num3 * 1f && num < num3 * 1f + num4) || (num > num3 * 2f && num < num3 * 2f + num4)) && m_FootstepTime < num2)
		{
			int num5 = g.m_App.m_Rand.Next(6);
			g.m_SoundManager.Play3D(2 + num5, m_Position, GetMoveVol());
			m_FootstepTime = num2 + 0.1f;
		}
	}

	public void PeerSetAnim(byte anim)
	{
		if (m_State != STATE.Intermission && anim != byte.MaxValue && m_AnimationSet != null && m_Model != null)
		{
			m_Anim = anim;
			if (m_Crouch && m_Anim != CrouchAnimByWeapon())
			{
				m_AnimationSet.CrossFade(m_Model.AnimationClips.Values[CrouchAnimByWeapon()], TimeSpan.FromSeconds(0.20000000298023224));
			}
			else
			{
				m_AnimationSet.CrossFade(m_Model.AnimationClips.Values[anim], TimeSpan.FromSeconds(0.20000000298023224));
			}
		}
	}

	public float GetMoveVol()
	{
		Vector3 vector = m_Position - m_PrevPosition;
		vector.Y = 0f;
		float value = vector.LengthSquared() * 100f;
		return MathHelper.Clamp(value, 0.5f, 1f);
	}

	public void UpdateViewAnimation()
	{
		if (!m_ViewAnimationSet.IsPlaying)
		{
			switch (m_CurrentViewAnim)
			{
			case 1:
			case 3:
			case 1112:
			case 1113:
				PlayViewAnim(0, loop: true, 0.04f);
				break;
			case 4:
			case 5:
				PlayViewAnim(0, loop: true, 0.04f);
				UseBioScan();
				break;
			case 6:
				PlayViewAnim(0, loop: true, 0.04f);
				UseScrewdriver();
				break;
			case 14:
			case 15:
				PlayViewAnim(13, loop: true, 0f);
				break;
			case 17:
			case 18:
				PlayViewAnim(16, loop: true, 0f);
				break;
			case 2:
				ChangeWeapon();
				break;
			}
		}
	}

	public bool IsReloadAnimPlaying()
	{
		if (m_ViewAnimationSet.IsPlaying)
		{
			int currentViewAnim = m_CurrentViewAnim;
			if (currentViewAnim == 15 || currentViewAnim == 18 || currentViewAnim == 1113)
			{
				return true;
			}
		}
		return false;
	}

	public void PlayViewAnim(int animId, bool loop, float blend)
	{
		if (m_ViewAnimationSet != null)
		{
			if (blend == 0f || m_CurrentViewAnim == -1)
			{
				m_ViewAnimationSet.StartClip(m_ViewModel.AnimationClips.Values[animId]);
			}
			else
			{
				m_ViewAnimationSet.CrossFade(m_ViewModel.AnimationClips.Values[animId], TimeSpan.FromSeconds(blend));
			}
			m_ViewAnimationSet.LoopEnabled = loop;
			m_CurrentViewAnim = animId;
		}
	}

	public void PlayViewIdleAnim(float blend)
	{
		switch (g.m_ItemManager.m_Item[m_WeaponItemIndex].m_Type)
		{
		case 4:
			PlayViewAnim(8, loop: false, blend);
			break;
		case 5:
			PlayViewAnim(10, loop: false, blend);
			break;
		case 11:
			PlayViewAnim(13, loop: true, blend);
			break;
		case 14:
			PlayViewAnim(16, loop: true, blend);
			break;
		default:
			PlayViewAnim(0, loop: true, blend);
			break;
		}
	}

	public void Serialize(PacketWriter packetWriter)
	{
		if (packetWriter == null)
		{
			throw new ArgumentNullException("packetWriter");
		}
		packetWriter.Write(m_Position);
		((BinaryWriter)(object)packetWriter).Write(m_Rotation.Y);
	}

	public void RequestDamageOther(int hitZone, int damage, Player playerToDamage, short projectileNetId)
	{
		if ((g.m_App.m_SurvivalMode && m_Bot == playerToDamage.m_Bot) || playerToDamage.m_bStaked || playerToDamage.m_State == STATE.Intermission)
		{
			return;
		}
		int num = -1;
		if (m_WeaponItemIndex != -1 && g.m_ItemManager.m_Item[m_WeaponItemIndex].m_Id != -1)
		{
			num = g.m_ItemManager.m_Item[m_WeaponItemIndex].m_Type;
			if (!m_Bot)
			{
				if ((num == 1 || num == 14) && hitZone == 2)
				{
					damage += 10;
				}
				if (num == 14 && g.m_ItemManager.m_Item[m_WeaponItemIndex].IsZoomed())
				{
					damage = ((hitZone != 2) ? (damage + 5) : (damage + 10));
				}
			}
			if (damage > 127)
			{
				damage = 127;
			}
		}
		int num2 = m_RequestedDamageAmount + damage;
		if (num2 > 127)
		{
			num2 = 127;
		}
		if (!playerToDamage.m_bRagdoll && playerToDamage.m_Health > 0)
		{
			m_bRequestSendDamage = true;
			m_RequestedDamageAmount = (sbyte)num2;
			m_RequestedPlayerToDamageNetID = playerToDamage.m_NetId;
			m_RequestedHitZone = (byte)hitZone;
			m_RequestedAttacker = m_NetId;
			m_RequestedProjectileNetId = projectileNetId;
			playerToDamage.DoDamage((sbyte)damage, (byte)hitZone, m_NetId, projectileNetId);
		}
	}

	public void DoDamage(sbyte damage, byte hitZone, short attackerNetId, short projectileNetId)
	{
		if (m_InvincibilityTime > (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds || m_State == STATE.Intermission || m_HitZone_Head == null || m_HitZone_Torso == null || m_HitZone_LowerBody == null || m_Health <= 0)
		{
			return;
		}
		if (m_Team == TEAM.Vampire)
		{
			damage /= 5;
		}
		m_RequestedHitZone = hitZone;
		Vector3 vector = new Vector3(0f, 0f, 1f);
		int playerExistsWithNetId = g.m_PlayerManager.GetPlayerExistsWithNetId(attackerNetId);
		if (projectileNetId != 255)
		{
			Projectile projectileByNetId = g.m_ProjectileManager.GetProjectileByNetId(projectileNetId);
			if (projectileByNetId != null)
			{
				vector = m_Position - projectileByNetId.m_SceneObject.World.Translation;
				vector.Normalize();
			}
		}
		else if (playerExistsWithNetId != -1 && g.m_PlayerManager.m_Player[playerExistsWithNetId].m_Id != -1)
		{
			vector = m_Position - g.m_PlayerManager.m_Player[playerExistsWithNetId].m_Position;
			vector.Normalize();
		}
		Vector3 zero = Vector3.Zero;
		zero = hitZone switch
		{
			2 => m_HitZone_Head.Position, 
			3 => m_HitZone_LowerBody.Position, 
			_ => m_HitZone_Torso.Position, 
		};
		if (g.m_App.m_OptionsBlood)
		{
			m_BloodSpray.Enabled = true;
			m_BloodSpray.Visible = true;
			m_BloodSpray.Emitter.BurstParticles = damage / 2;
			m_BloodSpray.Emitter.PositionData.Position = zero;
			m_BloodSpray.Emitter.PositionData.Velocity = Vector3.Zero;
			m_BloodSpray.LerpEmittersPositionAndOrientationOnNextUpdate = false;
			m_BloodSpray.Normal = -vector * 10f;
			if (m_Team == TEAM.Vampire)
			{
				m_BloodSpray.m_Colour = new Color(201, 227, 79);
			}
			else
			{
				m_BloodSpray.m_Colour = new Color(128, 0, 0);
			}
		}
		g.m_App.m_Rand.Next(2);
		bool flag = false;
		if (!m_Bot && g.m_PlayerManager.GetLocalPlayer().m_NetId == m_NetId)
		{
			flag = true;
		}
		StartSpinePitch(MathHelper.ToRadians(20f));
		if (m_Bot && IsHost())
		{
			BotTookDamage();
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		if (!m_DEBUG_Invincible)
		{
			m_Health -= damage;
		}
		m_LastAttackerNetId = attackerNetId;
		m_LastProjectileNetId = projectileNetId;
		if (m_Health > 0)
		{
			return;
		}
		m_bRequestDied = true;
		m_Health = 0;
		if (m_Bot)
		{
			StartLocalBotDeath();
			Kill(m_LastAttackerNetId, m_LastProjectileNetId);
			return;
		}
		StartLocalDeath(vector);
		g.m_SoundManager.Play3D(22, m_Position);
		if (m_Team == TEAM.Hunter)
		{
			m_Deaths++;
			if (playerExistsWithNetId != -1 && g.m_PlayerManager.m_Player[playerExistsWithNetId].m_Id != -1 && m_Id != g.m_PlayerManager.m_Player[playerExistsWithNetId].m_Id)
			{
				g.m_PlayerManager.m_Player[playerExistsWithNetId].m_Kills++;
				g.m_PlayerManager.m_Player[playerExistsWithNetId].m_Score++;
			}
		}
		else
		{
			m_Deaths++;
			if (playerExistsWithNetId != -1 && g.m_PlayerManager.m_Player[playerExistsWithNetId].m_Id != -1)
			{
				g.m_PlayerManager.m_Player[playerExistsWithNetId].m_Kills++;
				g.m_PlayerManager.m_Player[playerExistsWithNetId].m_Score += 10;
			}
		}
		g.m_App.m_RumbleFrames = 10;
	}

	public void StartLocalDeath(Vector3 vctDir)
	{
		if (m_ViewSceneObject != null)
		{
			SetState(STATE.LocalDeath);
			m_ViewSceneObject.Visibility = ObjectVisibility.None;
			if (m_WeaponItemIndex != -1)
			{
				g.m_ItemManager.m_Item[m_WeaponItemIndex].Hide();
			}
			m_RespawnTimer = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 1f;
			m_RagdollCreatePosition = m_Position;
			m_CharacterController.Body.LinearVelocity = vctDir * 30f;
			if (m_BreatheSFX != null)
			{
				m_BreatheSFX.Stop();
				m_BreatheSFX = null;
			}
			if (m_PhoneSFX != null)
			{
				m_PhoneSFX.Stop();
				m_PhoneSFX = null;
			}
			if (m_HeartBeatSFX != null)
			{
				m_HeartBeatSFX.Stop();
				m_HeartBeatSFX = null;
			}
			if (m_TickTockSFX != null)
			{
				m_TickTockSFX.Stop();
				m_TickTockSFX = null;
			}
			if (m_OceanSFX != null)
			{
				m_OceanSFX.Stop();
				m_OceanSFX = null;
			}
			RemoveWeaponByType(11);
			g.m_PlayerManager.m_NumDeaths++;
		}
	}

	public void StartLocalBotDeath()
	{
		SetState(STATE.LocalDeath);
		if (m_TorchLight != null)
		{
			m_TorchLight.Enabled = false;
		}
		if (m_TorchPointLight != null)
		{
			m_TorchPointLight.Enabled = false;
		}
		if (m_WeaponItemIndex != -1)
		{
			g.m_ItemManager.m_Item[m_WeaponItemIndex].Hide();
		}
		if (m_Team == TEAM.Hunter)
		{
			m_RespawnTimer = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 1f;
		}
		else
		{
			m_RespawnTimer = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 1f;
		}
		DisableCollisionAndGravity();
		RemoveWeaponByType(11);
	}

	private void UpdateLocalDeath()
	{
		m_Position.X = m_CharacterController.Body.Position.X;
		m_Position.Y = m_CharacterController.Body.Position.Y;
		m_Position.Z = m_CharacterController.Body.Position.Z;
		if (!m_Bot)
		{
			m_RagdollCreatePosition = m_Position;
		}
		m_CharacterController.HorizontalMotionConstraint.Speed = 0f;
		m_CharacterController.HorizontalMotionConstraint.CrouchingSpeed = 0f;
		if (m_RespawnTimer > (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds)
		{
			return;
		}
		m_Health = 100;
		if (m_ViewSceneObject != null)
		{
			m_ViewSceneObject.Visibility = ObjectVisibility.Rendered;
		}
		if (m_SceneObject != null)
		{
			m_SceneObject.Visibility = ObjectVisibility.Rendered;
			if (m_bRagdoll)
			{
				DisableRagdoll();
			}
		}
		if (m_bStaked || m_Team == TEAM.Hunter)
		{
			Spawn();
		}
		else
		{
			DeleteBot();
		}
		m_bStaked = false;
	}

	public void Kill(short lastAttackerNetId, short projectileNetId)
	{
		if (m_State == STATE.Intermission)
		{
			return;
		}
		EnableRagdoll();
		m_Leap = false;
		RemoveWeaponByType(11);
		int playerExistsWithNetId = g.m_PlayerManager.GetPlayerExistsWithNetId(lastAttackerNetId);
		if (pelvis != null)
		{
			Vector3 vector = new Vector3(0f, 0f, 1f);
			if (projectileNetId != 255)
			{
				Projectile projectileByNetId = g.m_ProjectileManager.GetProjectileByNetId(projectileNetId);
				if (projectileByNetId != null)
				{
					vector = m_Position - projectileByNetId.m_SceneObject.World.Translation;
					vector.Normalize();
				}
			}
			else if (playerExistsWithNetId != -1 && g.m_PlayerManager.m_Player[playerExistsWithNetId].m_Id != -1)
			{
				vector = m_Position - g.m_PlayerManager.m_Player[playerExistsWithNetId].m_Position;
				vector.Normalize();
			}
			vector.Y = 0f;
			if (m_RequestedHitZone != byte.MaxValue)
			{
				switch (m_RequestedHitZone)
				{
				case 2:
					head.ApplyImpulse(head.Position, vector * 1200f);
					break;
				case 1:
					torsoBottom.ApplyImpulse(torsoBottom.Position, vector * 1200f);
					break;
				case 3:
					rightThigh.ApplyImpulse(rightThigh.Position, vector * 1200f);
					break;
				case 4:
					if (lastAttackerNetId != m_NetId)
					{
						vector = g.m_PlayerManager.m_Player[playerExistsWithNetId].m_Position - m_Position;
						vector.Normalize();
						vector.Y = 0f;
						torsoTop.ApplyImpulse(torsoBottom.Position, vector * 4000f);
					}
					else
					{
						vector.Y = 0f;
						torsoBottom.ApplyImpulse(torsoBottom.Position, vector * 4000f);
					}
					break;
				default:
					pelvis.ApplyImpulse(pelvis.Position, vector * 1200f);
					break;
				}
			}
			else
			{
				pelvis.ApplyImpulse(pelvis.Position, vector * 1200f);
			}
		}
		m_RequestedHitZone = byte.MaxValue;
		ShowKillMessage(lastAttackerNetId);
		g.m_SoundManager.Play3D(22, m_Position);
		if (m_Team == TEAM.Hunter)
		{
			m_Deaths++;
			if (playerExistsWithNetId != -1)
			{
				g.m_PlayerManager.m_Player[playerExistsWithNetId].m_Kills++;
				g.m_PlayerManager.m_Player[playerExistsWithNetId].PeerGiveScore(1);
			}
		}
		else
		{
			m_Deaths++;
			if (playerExistsWithNetId != -1)
			{
				g.m_PlayerManager.m_Player[playerExistsWithNetId].m_Kills++;
				g.m_PlayerManager.m_Player[playerExistsWithNetId].PeerGiveScore(10);
			}
		}
	}

	public void ShowKillMessage(short attackerNetId)
	{
		int playerExistsWithNetId = g.m_PlayerManager.GetPlayerExistsWithNetId(attackerNetId);
		if (playerExistsWithNetId != -1)
		{
			_ = g.m_App.HUD_GREEN;
			if (playerExistsWithNetId == g.m_PlayerManager.GetLocalPlayer().m_Id || g.m_PlayerManager.GetLocalPlayer().m_Id == m_Id)
			{
				_ = Color.Goldenrod;
			}
			_ = $"{g.m_PlayerManager.m_Player[playerExistsWithNetId].GetName()} killed {GetName()}";
		}
	}

	public void InitParticleSystems()
	{
		m_BloodSpray = new BloodQuadSprayParticleSystem(g.m_App);
		m_BloodSpray.Enabled = false;
		m_BloodSpray.AutoInitialize(g.m_App.GraphicsDevice, g.m_App.Content, g.m_App.screenManager.SpriteBatch);
		g.m_App.m_ParticleSystemManager.AddParticleSystem(m_BloodSpray);
		m_BloodSpray.Enabled = false;
	}

	public bool IsHost()
	{
		bool result = true;
		if (g.m_App.m_NetworkSession != null && !g.m_App.m_NetworkSession.IsHost)
		{
			result = false;
		}
		return result;
	}

	public void RagdollEnd()
	{
		m_AnimationSet.ClearBoneControllers();
		m_Position = m_RagdollCreatePosition;
		if (m_CharacterController != null && m_CharacterController.Body != null)
		{
			m_Position = m_RagdollCreatePosition;
			m_NetworkPosition = m_RagdollCreatePosition;
			m_CharacterController.Body.Position = m_RagdollCreatePosition;
			m_CharacterController.Body.LinearVelocity = Vector3.Zero;
			m_CharacterController.Body.AngularVelocity = Vector3.Zero;
		}
	}

	public void SetState(STATE s)
	{
		m_State = s;
	}

	public void AutoChooseTeam()
	{
		SetTeam(TEAM.Hunter);
	}

	public void SetTeam(TEAM t)
	{
		m_Team = t;
		m_RequestSendTeam = true;
	}

	public void PeerSetTeam(TEAM t)
	{
		if (t != 0)
		{
			m_Team = t;
		}
	}

	public TEAM GetTeam()
	{
		return m_Team;
	}

	public void AutoChooseClass()
	{
		bool flag = g.m_App.m_Rand.NextDouble() > 0.5;
		if (m_Team == TEAM.Hunter)
		{
			if (flag)
			{
				SetClass(CLASS.FatherD);
			}
			else
			{
				SetClass(CLASS.Molly);
			}
		}
		else if (flag)
		{
			SetClass(CLASS.Edgar);
		}
		else
		{
			SetClass(CLASS.Nina);
		}
	}

	public void SetClass(CLASS c)
	{
		m_RequestSendClass = true;
		PeerSetClass(c);
	}

	public void PeerSetClass(CLASS c)
	{
		if (c == CLASS.None)
		{
			return;
		}
		m_Class = c;
		m_Health = 100;
		GiveWeapons();
		CreateHitZones();
		if (IsLocalPlayer() && !m_Bot)
		{
			return;
		}
		switch (m_Class)
		{
		case CLASS.Edgar:
			SetModel(g.m_PlayerManager.m_FullModel_VampM);
			break;
		case CLASS.MedBay:
		case CLASS.OxygenTanks:
		case CLASS.CargoBay:
			SetModel(g.m_PlayerManager.m_FullModel_Alien);
			break;
		}
		m_Model.Model.CopyAbsoluteBoneTransformsTo(m_Transforms);
		if (m_Team == TEAM.Hunter)
		{
			m_AnimationSet = new AnimationSet(2, m_Model.SkeletonBones);
			m_AnimationSet.TranslationInterpolation = InterpolationMode.None;
			m_AnimationSet.OrientationInterpolation = InterpolationMode.None;
			m_AnimationSet.ScaleInterpolation = InterpolationMode.None;
			m_AnimationSet.ClearBoneMasks(1);
			m_AnimationSet.SetBoneMask(1, "Bip01_Pelvis");
			m_AnimationSet.SetBoneMask(1, "Bip01_Spine");
			m_AnimationSet.SetBoneMask(1, "Bip01_Spine1");
			m_AnimationSet.SetBoneMask(1, "Bip01_Neck");
			m_AnimationSet.SetBoneMask(1, "Bip01_Head");
			m_AnimationSet.SetBoneMask(1, "Bip01_L_Clavicle");
			m_AnimationSet.SetBoneMask(1, "Bip01_L_UpperArm");
			m_AnimationSet.SetBoneMask(1, "Bip01_L_Forearm");
			m_AnimationSet.SetBoneMask(1, "Bip01_L_Hand");
			m_AnimationSet.SetBoneMask(1, "Bip01_R_Clavicle");
			m_AnimationSet.SetBoneMask(1, "Bip01_R_UpperArm");
			m_AnimationSet.SetBoneMask(1, "Bip01_R_Forearm");
			m_AnimationSet.SetBoneMask(1, "Bip01_R_Hand");
			m_AnimationSet.SetBoneMask(1, "Bone01");
			m_AnimationSet.SetBoneMask(1, "Bone02");
			m_AnimationSet.SetBoneMask(1, "Bip01_L_Finger0");
			m_AnimationSet.SetBoneMask(1, "Bip01_L_Finger01");
			m_AnimationSet.SetBoneMask(1, "Bip01_L_Finger1");
			m_AnimationSet.SetBoneMask(1, "Bip01_L_Finger11");
			m_AnimationSet.SetBoneMask(1, "Bip01_R_Finger0");
			m_AnimationSet.SetBoneMask(1, "Bip01_R_Finger01");
			m_AnimationSet.SetBoneMask(1, "Bip01_R_Finger1");
			m_AnimationSet.SetBoneMask(1, "Bip01_R_Finger11");
			m_AnimationSet.StartClip(0, m_Model.AnimationClips[m_Model.AnimationClips.Keys[0]]);
		}
		else
		{
			m_AnimationSet = new AnimationSet(1, m_Model.SkeletonBones);
			m_AnimationSet.TranslationInterpolation = InterpolationMode.None;
			m_AnimationSet.OrientationInterpolation = InterpolationMode.None;
			m_AnimationSet.ScaleInterpolation = InterpolationMode.None;
			m_AnimationSet.StartClip(m_Model.AnimationClips[m_Model.AnimationClips.Keys[0]]);
			if (m_Team == TEAM.CargoBay)
			{
				m_AnimationSet.UseMotionExtraction = true;
			}
		}
		if (m_SceneObject == null)
		{
			m_SceneObject = new SceneObject(m_Model.Model)
			{
				UpdateType = UpdateType.Automatic,
				Visibility = ObjectVisibility.RenderedAndCastShadows,
				StaticLightingType = StaticLightingType.Composite,
				CollisionType = CollisionType.None,
				AffectedByGravity = false,
				Name = $"Player{m_NetId}",
				World = Matrix.Identity
			};
			g.m_App.sceneInterface.ObjectManager.Submit(m_SceneObject);
			foreach (RenderableMesh renderableMesh in m_SceneObject.RenderableMeshes)
			{
				BoundingBox meshBoundingBox = renderableMesh.MeshBoundingBox;
				renderableMesh.MeshBoundingBox = new BoundingBox(new Vector3(meshBoundingBox.Min.Z, meshBoundingBox.Min.Y + 2.159f, meshBoundingBox.Min.X), new Vector3(meshBoundingBox.Max.Z, meshBoundingBox.Max.Y + 2.159f, meshBoundingBox.Max.X));
			}
			if (m_Class == CLASS.Edgar)
			{
				foreach (RenderableMesh renderableMesh2 in m_SceneObject.RenderableMeshes)
				{
					BoundingBox meshBoundingBox2 = renderableMesh2.MeshBoundingBox;
					renderableMesh2.MeshBoundingBox = new BoundingBox(new Vector3(meshBoundingBox2.Min.Z * 100f, meshBoundingBox2.Min.Y + 2.159f, meshBoundingBox2.Min.X * 100f), new Vector3(meshBoundingBox2.Max.Z * 100f, meshBoundingBox2.Max.Y + 2.159f, meshBoundingBox2.Max.X * 100f));
				}
			}
			m_SceneObject.CalculateBounds();
		}
		SetWeapon(m_StartWeaponItemIndex);
		SetState(STATE.InGame);
	}

	public CLASS GetClass()
	{
		return m_Class;
	}

	public void EnableCollisionAndGravity()
	{
		if (m_CharacterController != null && m_CharacterController.Body != null)
		{
			m_CharacterController.Body.CollisionInformation.CollisionRules.Personal = CollisionRule.Defer;
			m_CharacterController.Body.IsAffectedByGravity = true;
			m_CharacterController.Body.Position = m_Position;
			m_CharacterController.Body.LinearVelocity = Vector3.Zero;
			m_CharacterController.Body.AngularVelocity = Vector3.Zero;
		}
	}

	public void DisableCollisionAndGravity()
	{
		m_CharacterController.Body.CollisionInformation.CollisionRules.Personal = CollisionRule.NoBroadPhase;
		m_CharacterController.Body.IsAffectedByGravity = false;
		m_CharacterController.Body.Position = m_Position;
		m_CharacterController.Body.LinearVelocity = Vector3.Zero;
		m_CharacterController.Body.AngularVelocity = Vector3.Zero;
	}

	public Vector3 GetAimPosition()
	{
		if (!m_Bot && IsLocalPlayer())
		{
			return g.m_CameraManager.m_Position;
		}
		return m_Position + new Vector3(0f, 1.65f, 0f);
	}

	public Vector3 GetAimVector()
	{
		if (!m_Bot)
		{
			if (m_ViewSceneObject != null)
			{
				return (Matrix.CreateRotationX(g.m_CameraManager.m_Pitch) * m_ViewSceneObject.World).Forward;
			}
			return (Matrix.CreateRotationX(g.m_CameraManager.m_Pitch) * m_SceneObject.World).Forward;
		}
		return m_BotAimVector;
	}

	public string GetName()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (m_Bot)
		{
			if (m_Team == TEAM.Hunter)
			{
				return BotMaleSlayerNames[m_BotNameIdx];
			}
			return BotMaleVampireNames[m_BotNameIdx];
		}
		if (g.m_App.m_NetworkSession != null)
		{
			GamerCollectionEnumerator<NetworkGamer> enumerator = g.m_App.m_NetworkSession.AllGamers.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					NetworkGamer current = enumerator.Current;
					if (current.Id == m_NetId)
					{
						return ((Gamer)current).Gamertag;
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to .constrained prefix*/).Dispose();
			}
		}
		else if (Gamer.SignedInGamers[g.m_App.m_PlayerOnePadId] != null)
		{
			return ((Gamer)Gamer.SignedInGamers[g.m_App.m_PlayerOnePadId]).Gamertag;
		}
		return "Player";
	}

	public void RemoveWeapons()
	{
		for (int i = 0; i < 6; i++)
		{
			m_Weapons[i] = -1;
		}
	}

	public void RemoveWeaponByType(int type)
	{
		for (int i = 0; i < 6; i++)
		{
			if (m_Weapons[i] != -1 && g.m_ItemManager.m_Item[m_Weapons[i]].m_Type == type)
			{
				m_Weapons[i] = -1;
			}
		}
	}

	public void GiveWeapon(int itemId)
	{
		for (int i = 0; i < 6; i++)
		{
			if (m_Weapons[i] == -1)
			{
				m_Weapons[i] = itemId;
				break;
			}
		}
	}

	public void SetWeapon(int itemIndex)
	{
		if (itemIndex != -1)
		{
			m_WeaponItemIndex = itemIndex;
			UpdateWeaponToBone();
			g.m_ItemManager.m_Item[m_WeaponItemIndex].Show();
			PlayViewIdleAnim(0.1f);
		}
	}

	public void SetWeaponByType(int weaponType)
	{
		if (weaponType < 0 || weaponType > 32 || m_State == STATE.Intermission)
		{
			return;
		}
		for (int i = 0; i < 6; i++)
		{
			if (m_Weapons[i] != -1 && m_Weapons[i] <= 125 && m_Weapons[i] != -1 && g.m_ItemManager.m_Item[m_Weapons[i]].m_Type == weaponType)
			{
				SetWeapon(m_Weapons[i]);
				break;
			}
		}
	}

	public void HolsterWeapon()
	{
		if (m_CurrentViewAnim != 2)
		{
			PlayViewAnim(2, loop: false, 0f);
		}
	}

	public void ChangeWeapon()
	{
		m_bWeaponChanged = true;
		PeerChangeWeapon();
	}

	public void PeerChangeWeapon()
	{
		if (m_State == STATE.Intermission)
		{
			return;
		}
		int num = -1;
		for (int i = 0; i < 6; i++)
		{
			if (m_Weapons[i] == m_WeaponItemIndex)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			return;
		}
		if (m_TorchLight != null && m_TorchLight.Enabled)
		{
			ToggleTorchLight();
		}
		g.m_ItemManager.m_Item[m_Weapons[num]].Hide();
		num++;
		int num2 = 0;
		while (m_Weapons[num] == -1 && num2 < 100)
		{
			num++;
			if (num >= 6)
			{
				num = 0;
			}
		}
		SetWeapon(m_Weapons[num]);
		if (g.m_ItemManager.m_Item[m_Weapons[num]].m_Type == 1 && m_TorchLight != null && !m_TorchLight.Enabled)
		{
			ToggleTorchLight();
		}
	}

	public void GiveWeapons()
	{
		RemoveWeapons();
		int num = -1;
		switch (m_Class)
		{
		case CLASS.FatherD:
		case CLASS.Molly:
		case CLASS.Nina:
			num = g.m_ItemManager.Create(1, byte.MaxValue, new Vector3(0f, 0f, 0f), 0f, this);
			GiveWeapon(num);
			m_StartWeaponItemIndex = num;
			break;
		case CLASS.None:
		case CLASS.Edgar:
			break;
		}
	}

	private void ResetAllAmmo()
	{
		int num = -1;
		for (int i = 0; i < 6; i++)
		{
			num = m_Weapons[i];
			if (num != -1)
			{
				g.m_ItemManager.ResetAmmo(num);
			}
		}
	}

	public int NumWeapons()
	{
		int num = 0;
		for (int i = 0; i < 6; i++)
		{
			if (m_Weapons[i] != -1)
			{
				num++;
			}
		}
		return num;
	}

	public int FindWeapon(int wepType)
	{
		for (int i = 0; i < 6; i++)
		{
			if (m_Weapons[i] != -1 && g.m_ItemManager.m_Item[m_Weapons[i]].m_Type == wepType)
			{
				return m_Weapons[i];
			}
		}
		return -1;
	}

	public void Crouch()
	{
		m_Crouch = !m_Crouch;
		m_RequestSendCrouch = true;
		m_Anim = byte.MaxValue;
		if (m_CharacterController != null)
		{
			if (m_Crouch)
			{
				m_CharacterController.StanceManager.DesiredStance = Stance.Crouching;
			}
			else
			{
				m_CharacterController.StanceManager.DesiredStance = Stance.Standing;
			}
		}
	}

	public void PeerCrouch(bool crouch)
	{
		if (m_State != STATE.Intermission)
		{
			m_Crouch = crouch;
		}
	}

	public void SetXPForNextLevel()
	{
		m_XPForNextRank = 50 * m_Rank;
	}

	public void CheckRankUp()
	{
		if (m_XP > m_XPForNextRank && m_Rank < 5)
		{
			m_Rank++;
			m_XP = 0;
			SetXPForNextLevel();
			m_RequestRankUp = true;
			_ = g.m_App.HUD_GREEN;
			if (!m_Bot)
			{
				_ = Color.Goldenrod;
			}
		}
	}

	public void PeerRankUp()
	{
	}

	public void PeerGiveScore(byte score)
	{
		m_Score += score;
		if (g.m_App.m_NetworkSession != null && g.m_PlayerManager.NumPlayersOnTeams() > 1)
		{
			m_XP += score;
			CheckRankUp();
		}
	}

	public string GetNameForRank()
	{
		if (m_Rank > 5)
		{
			m_Rank = 5;
		}
		return m_Rank switch
		{
			1 => "*", 
			2 => "**", 
			3 => "***", 
			4 => "****", 
			5 => "*****", 
			_ => "", 
		};
	}

	private void CheckPeerCollision()
	{
		if (!m_CharacterController.SupportFinder.HasTraction)
		{
			return;
		}
		Player playerNearMe = g.m_PlayerManager.GetPlayerNearMe(m_Id, 1.6128999f);
		if (playerNearMe != null && !playerNearMe.m_bRagdoll)
		{
			Vector3 vector = m_Position - playerNearMe.m_Position;
			if (!(Math.Abs(vector.X) < 0.0001f) || !(Math.Abs(vector.Z) < 0.0001f))
			{
				vector.Y = 0f;
				vector.Normalize();
				m_CharacterController.Body.ApplyImpulse(m_Position, vector * 25f);
			}
		}
	}

	public bool IsValid()
	{
		if (m_ViewSceneObject == null && m_SceneObject == null)
		{
			return false;
		}
		if (m_Team == TEAM.None)
		{
			return false;
		}
		return true;
	}

	public bool IsDead()
	{
		if ((m_bRagdoll || m_Health <= 0) && IsValid())
		{
			return true;
		}
		return false;
	}

	public void UpdateRumble(PlayerIndex playerIndex)
	{
		if (g.m_App.m_RumbleFrames > 0 && g.m_App.m_OptionsVibration)
		{
			g.m_App.m_RumbleFrames--;
			GamePad.SetVibration(playerIndex, 0.5f, 0.5f);
		}
		else
		{
			GamePad.SetVibration(playerIndex, 0f, 0f);
		}
	}

	public void PeerAttach(byte netItemId, byte escapeAirlockId)
	{
		if (m_State != STATE.Intermission)
		{
			m_AttachedItemId = g.m_ItemManager.GetItemIdByNetItemId(netItemId);
			m_ArtifactEscapeAirlockId = escapeAirlockId;
			g.m_App.RemoveHelmetMessage(HelmetMessage.TYPE.ARTIFACT_DROPPED);
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.ARTIFACT_TAKEN, "ARTIFACT TAKEN", new Vector2(120f, 250f), 3f, g.m_App.HUD_GREEN, SoundManager.SFX.WarningBeep, null, hideSysMsg: true);
			if (g.m_App.m_AlarmSFX == null)
			{
				g.m_App.m_AlarmSFX = g.m_SoundManager.PlayLooped(23);
			}
		}
	}

	public void PeerDetach()
	{
		if (m_State != STATE.Intermission)
		{
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.ARTIFACT_DROPPED, "ARTIFACT DROPPED", new Vector2(120f, 250f), 3f, g.m_App.HUD_GREEN, SoundManager.SFX.WarningBeep, null, hideSysMsg: true);
			m_AttachedItemId = -1;
		}
	}

	public void PeerCreateProjectile(short projectileNetId, byte projectileType, Vector3 pos, Quaternion q, Vector3 vel)
	{
		if (projectileType == 0 && m_WeaponItemIndex != -1 && g.m_ItemManager.m_Item[m_WeaponItemIndex].m_SceneObject != null)
		{
			Matrix world = Matrix.CreateFromQuaternion(q);
			world.Translation = pos;
			g.m_ProjectileManager.Create(0, world, vel, this, projectileNetId);
		}
	}

	private void RecoverHealthFromCrouching()
	{
		if (m_NextRecoverTime < (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds)
		{
			if (m_Health < 100)
			{
				m_Health++;
			}
			m_NextRecoverTime = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 1f;
		}
	}

	public void SetUsedCrate(int crateIdx)
	{
		m_UsedCrate[crateIdx] = true;
	}

	public void ClearUsedCrate(int crateIdx)
	{
		m_UsedCrate[crateIdx] = false;
	}

	public void ClearUsedCrates()
	{
		for (int i = 0; i < 22; i++)
		{
			m_UsedCrate[i] = false;
		}
	}

	public void TryPurchase()
	{
		if (!Guide.IsVisible)
		{
			if (g.m_App.CanPurchaseContent(g.m_App.m_PlayerOnePadId))
			{
				Guide.ShowMarketplace(g.m_App.m_PlayerOnePadId);
			}
			else if (Gamer.SignedInGamers[g.m_App.m_PlayerOnePadId] != null)
			{
				g.m_App.m_ShowPermissionTime = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 3f;
			}
			else
			{
				Guide.ShowSignIn(1, true);
			}
		}
	}

	public void CheckMiscTriggers()
	{
		float num = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds;
		m_NearTrigger = TRIGGERS.NONE;
		float num2 = 120f;
		int rumbleFrames = 10;
		Vector3 facing = g.m_CameraManager.m_LookAt - g.m_CameraManager.m_Position;
		facing.Normalize();
		string name = "TriggerLCD1";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out MiscTriggerEntity obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled)
		{
			if (FindFacingTrigger(obj.World.Translation, facing, 16f, -0.96f))
			{
				string s = "PRESS [A] TO USE COMPUTER";
				g.m_App.AddHelmetMessage(HelmetMessage.TYPE.LCD_INTERACT, s, new Vector2(g.m_App.GetHudCentreX(s, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
				m_NearTrigger = TRIGGERS.LCD1;
			}
			else
			{
				g.m_App.RemoveHelmetMessage(HelmetMessage.TYPE.LCD_INTERACT);
			}
		}
		name = "TriggerDoor1";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled)
		{
			if (FindFacingTrigger(obj.World.Translation, facing, 16f, -0.85f))
			{
				if (m_Door0Unlocked)
				{
					string s2 = "PRESS [A] TO OPEN DOOR";
					g.m_App.AddHelmetMessage(HelmetMessage.TYPE.DOOR_LOCKED, s2, new Vector2(g.m_App.GetHudCentreX(s2, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
				}
				else
				{
					string s3 = "DOOR LOCKED BY S O S SYSTEM";
					g.m_App.AddHelmetMessage(HelmetMessage.TYPE.DOOR_LOCKED, s3, new Vector2(g.m_App.GetHudCentreX(s3, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
				}
				m_NearTrigger = TRIGGERS.DOOR1;
			}
			else
			{
				g.m_App.RemoveHelmetMessage(HelmetMessage.TYPE.DOOR_LOCKED);
			}
		}
		name = "TriggerEnterRoomSFX1";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 16f, 0f))
		{
			g.m_SoundManager.Play(27);
			obj.m_Complete = true;
		}
		name = "TriggerEnterRoomSFX2";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 16f, 0f))
		{
			g.m_SoundManager.Play(27);
			obj.m_Complete = true;
		}
		name = "TriggerBloodSFX";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 16f, -0.96f))
		{
			g.m_SoundManager.Play(28);
			g.m_App.m_HudDistort1Time = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 1f;
			g.m_App.m_HudDistort1Alpha = 1f;
			g.m_App.m_RumbleFrames = rumbleFrames;
			obj.m_Complete = true;
		}
		name = "Password";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 4.84f, -0.96f))
		{
			g.m_SoundManager.Play(30);
			obj.m_NextUseTime = num + 30f;
		}
		name = "TriggerNearSuitSFX3";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 16f, 0f))
		{
			g.m_SoundManager.Play(27);
			obj.m_Complete = true;
		}
		name = "TriggerLCD2";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled)
		{
			if (FindFacingTrigger(obj.World.Translation, facing, 16f, -0.96f))
			{
				string s4 = "PRESS [A] TO USE COMPUTER";
				g.m_App.AddHelmetMessage(HelmetMessage.TYPE.LCD2_INTERACT, s4, new Vector2(g.m_App.GetHudCentreX(s4, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
				m_NearTrigger = TRIGGERS.LCD2;
			}
			else
			{
				g.m_App.RemoveHelmetMessage(HelmetMessage.TYPE.LCD2_INTERACT);
			}
		}
		name = "TriggerNoodles";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && m_AllowNoodlesClue && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 16f, -0.96f))
		{
			g.m_SoundManager.Play(30);
			obj.m_Complete = true;
		}
		name = "TriggerDoor2Clatter";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 16f, 0f))
		{
			g.m_SoundManager.Play(31);
			obj.m_Complete = true;
		}
		name = "TriggerDoor2";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled)
		{
			if (FindFacingTrigger(obj.World.Translation, facing, 16f, -0.85f))
			{
				if (m_Door1Unlocked)
				{
					string s5 = "PRESS [A] TO OPEN DOOR";
					g.m_App.AddHelmetMessage(HelmetMessage.TYPE.DOOR2_LOCKED, s5, new Vector2(g.m_App.GetHudCentreX(s5, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
				}
				else
				{
					string s6 = "DOOR LOCKED";
					g.m_App.AddHelmetMessage(HelmetMessage.TYPE.DOOR2_LOCKED, s6, new Vector2(g.m_App.GetHudCentreX(s6, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
				}
				m_NearTrigger = TRIGGERS.DOOR2;
			}
			else
			{
				g.m_App.RemoveHelmetMessage(HelmetMessage.TYPE.DOOR2_LOCKED);
			}
		}
		name = "TriggerO2Empty";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled)
		{
			if (FindFacingTrigger(obj.World.Translation, facing, 16f, -0.85f))
			{
				string s7 = "EMPTY";
				g.m_App.AddHelmetMessage(HelmetMessage.TYPE.O2_EMPTY, s7, new Vector2(g.m_App.GetHudCentreX(s7, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			}
			else
			{
				g.m_App.RemoveHelmetMessage(HelmetMessage.TYPE.O2_EMPTY);
			}
		}
		name = "Checkpoint1";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 25f, 0f))
		{
			g.m_App.m_CheckpointId = 1;
			obj.m_Complete = true;
			g.m_LoadSaveManager.SaveGame();
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.SAVING, "Saving Progress", new Vector2(g.m_App.GetHudCentreX("Saving Progress", g.m_App.lcdFont), 430f), 5f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.lcdFont, hideSysMsg: true);
		}
		name = "TriggerHallucinate";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 25f, 0f))
		{
			HallucinateOn();
			g.m_App.m_RumbleFrames = rumbleFrames;
			obj.m_Complete = true;
		}
		name = "TriggerSpaceman";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 25f, 0f))
		{
			g.m_PlayerManager.m_SpacemanId = g.m_PlayerManager.Create(255, bot: true, TEAM.Vampire).m_Id;
			g.m_PlayerManager.m_Player[g.m_PlayerManager.m_SpacemanId].m_AnimationSet.CrossFade(g.m_PlayerManager.m_Player[g.m_PlayerManager.m_SpacemanId].m_Model.AnimationClips.Values[0], TimeSpan.FromSeconds(0.0));
			g.m_PlayerManager.m_Player[g.m_PlayerManager.m_SpacemanId].m_AnimationSet.LoopEnabled = false;
			g.m_App.m_HudDistort1Time = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 8f;
			g.m_App.m_HudDistort1Alpha = 1f;
			g.m_SoundManager.Play(32);
			g.m_App.m_RumbleFrames = rumbleFrames;
			obj.m_Complete = true;
		}
		name = "TriggerPhone";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 25f, 0f))
		{
			g.m_PlayerManager.m_PhoneTimer = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 19f;
			m_PhoneSFX = g.m_SoundManager.PlayLooped(33);
			m_PhoneSFX.Pan = 1f;
			obj.m_Complete = true;
		}
		name = "TriggerShakeyCam";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 25f, 0f))
		{
			g.m_CameraManager.m_ShakeyCam = true;
			PlayViewAnim(1, loop: true, 0.2f);
			m_Health = 5;
			g.m_App.m_RumbleFrames = rumbleFrames;
			m_HeartBeatSFX = g.m_SoundManager.PlayLooped(38);
			obj.m_Complete = true;
		}
		name = "TriggerO2Full1";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && obj.m_Enabled)
		{
			if (FindFacingTrigger(obj.World.Translation, facing, 16f, -0.96f))
			{
				if (!obj.m_Complete)
				{
					string s8 = "PRESS [A] TO USE OXYGEN";
					g.m_App.AddHelmetMessage(HelmetMessage.TYPE.O2_FULL1, s8, new Vector2(g.m_App.GetHudCentreX(s8, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
				}
				else
				{
					string s9 = "EMPTY";
					g.m_App.AddHelmetMessage(HelmetMessage.TYPE.O2_FULL1, s9, new Vector2(g.m_App.GetHudCentreX(s9, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
				}
				m_NearTrigger = TRIGGERS.OXYGEN1;
			}
			else
			{
				g.m_App.RemoveHelmetMessage(HelmetMessage.TYPE.O2_FULL1);
			}
		}
		name = "TriggerDoor3";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 16f, -0.85f))
		{
			if (m_Health < 10)
			{
				DoDamage(100, 1, -1, -1);
				return;
			}
			string s10 = "PRESS [A] TO OPEN DOOR";
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.DOOR3_LOCKED, s10, new Vector2(g.m_App.GetHudCentreX(s10, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			m_NearTrigger = TRIGGERS.DOOR3;
		}
		name = "TriggerLCD3";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled)
		{
			if (FindFacingTrigger(obj.World.Translation, facing, 16f, -0.96f))
			{
				string s11 = "PRESS [A] TO USE COMPUTER";
				g.m_App.AddHelmetMessage(HelmetMessage.TYPE.LCD3_INTERACT, s11, new Vector2(g.m_App.GetHudCentreX(s11, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
				m_NearTrigger = TRIGGERS.LCD3;
			}
			else
			{
				g.m_App.RemoveHelmetMessage(HelmetMessage.TYPE.LCD3_INTERACT);
			}
		}
		name = "TriggerO2Empty2";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled)
		{
			if (FindFacingTrigger(obj.World.Translation, facing, 16f, -0.85f))
			{
				string s12 = "EMPTY";
				g.m_App.AddHelmetMessage(HelmetMessage.TYPE.O2_EMPTY2, s12, new Vector2(g.m_App.GetHudCentreX(s12, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			}
			else
			{
				g.m_App.RemoveHelmetMessage(HelmetMessage.TYPE.O2_EMPTY2);
			}
		}
		name = "TriggerSafe";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
		{
			if (!obj.m_Complete && obj.m_Enabled)
			{
				if (FindFacingTrigger(obj.World.Translation, facing, 16f, -0.94f))
				{
					string s13 = "PRESS [A] TO USE KEYPAD";
					g.m_App.AddHelmetMessage(HelmetMessage.TYPE.SAFE_INTERACT, s13, new Vector2(g.m_App.GetHudCentreX(s13, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
					m_NearTrigger = TRIGGERS.SAFE;
				}
				else
				{
					g.m_App.RemoveHelmetMessage(HelmetMessage.TYPE.SAFE_INTERACT);
				}
			}
			else if (obj.m_Complete && obj.m_Enabled)
			{
				if (FindFacingTrigger(obj.World.Translation, facing, 16f, -0.94f))
				{
					string s14 = "PRESS [A] TO PICKUP BONE SAW";
					g.m_App.AddHelmetMessage(HelmetMessage.TYPE.PICKUP_SAW, s14, new Vector2(g.m_App.GetHudCentreX(s14, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
					m_NearTrigger = TRIGGERS.SAW;
				}
				else
				{
					g.m_App.RemoveHelmetMessage(HelmetMessage.TYPE.PICKUP_SAW);
				}
			}
		}
		name = "TriggerArm";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
		{
			if (g.m_ItemManager.m_Item[m_WeaponItemIndex].m_Type == 22)
			{
				g.m_ItemManager.m_Item[m_WeaponItemIndex].m_CanSawArm = false;
			}
			if (obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled)
			{
				if (g.m_ItemManager.m_Item[m_WeaponItemIndex].m_Type == 22)
				{
					g.m_ItemManager.m_Item[m_WeaponItemIndex].m_CanSawArm = false;
					Vector3 vector = g.m_ItemManager.m_Item[m_WeaponItemIndex].m_SceneObject.World.Translation + g.m_ItemManager.m_Item[m_WeaponItemIndex].m_SceneObject.World.Forward;
					float num3 = (obj.World.Translation - vector).LengthSquared();
					if (num3 < 0.1f)
					{
						if (m_NumSaws == 0)
						{
							string s15 = "PRESS [RT] TO SAW";
							g.m_App.AddHelmetMessage(HelmetMessage.TYPE.PICKUP_SAW, s15, new Vector2(g.m_App.GetHudCentreX(s15, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
						}
						g.m_ItemManager.m_Item[m_WeaponItemIndex].m_CanSawArm = true;
					}
				}
			}
			else if (obj.m_Complete && obj.m_Enabled)
			{
				if (FindFacingTrigger(obj.World.Translation, facing, 16f, -0.94f))
				{
					string s16 = "PRESS [A] TO PICKUP ARM";
					g.m_App.AddHelmetMessage(HelmetMessage.TYPE.PICKUP_ARM, s16, new Vector2(g.m_App.GetHudCentreX(s16, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
					m_NearTrigger = TRIGGERS.ARM;
				}
				else
				{
					g.m_App.RemoveHelmetMessage(HelmetMessage.TYPE.PICKUP_ARM);
				}
			}
		}
		name = "TriggerBioScan";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled)
		{
			if (FindFacingTrigger(obj.World.Translation, facing, 16f, -0.94f))
			{
				string s17 = "PRESS [A] TO USE BIOSCAN";
				g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USE_BIOSCAN, s17, new Vector2(g.m_App.GetHudCentreX(s17, g.m_App.hudFont), num2), 0.25f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
				m_NearTrigger = TRIGGERS.BIOSCAN;
			}
			else
			{
				g.m_App.RemoveHelmetMessage(HelmetMessage.TYPE.USE_BIOSCAN);
			}
		}
		name = "TriggerDoor4";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled)
		{
			if (g.m_PlayerManager.m_MedBayAlienId == -1 && FindFacingTrigger(obj.World.Translation, facing, 64f, 0f))
			{
				g.m_PlayerManager.m_MedBayAlienId = g.m_PlayerManager.Create(255, bot: true, TEAM.MedBay).m_Id;
				g.m_PlayerManager.m_Player[g.m_PlayerManager.m_MedBayAlienId].m_SceneObject.Visibility = ObjectVisibility.None;
			}
			if (!g.m_PlayerManager.m_MedBayAlienShown && FindFacingTrigger(obj.World.Translation, facing, 9.61f, -0.96f))
			{
				g.m_PlayerManager.m_MedBayAlienShown = true;
				g.m_PlayerManager.m_Player[g.m_PlayerManager.m_MedBayAlienId].DoMedbayScare();
				g.m_CameraManager.m_LookAtPlayerId = g.m_PlayerManager.m_MedBayAlienId;
			}
			if (FindFacingTrigger(obj.World.Translation, facing, 9f, -0.85f))
			{
				if (m_DoorMedbayUnlocked)
				{
					string s18 = "PRESS [A] TO OPEN DOOR";
					g.m_App.AddHelmetMessage(HelmetMessage.TYPE.DOOR4_LOCKED, s18, new Vector2(g.m_App.GetHudCentreX(s18, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
					m_NearTrigger = TRIGGERS.DOOR4;
				}
			}
			else
			{
				g.m_App.RemoveHelmetMessage(HelmetMessage.TYPE.DOOR4_LOCKED);
			}
		}
		name = "TriggerEnterMedbaySFX";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 16f, 0f))
		{
			g.m_SoundManager.Play(27);
			obj.m_Complete = true;
		}
		name = "TriggerDoor5";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 16f, -0.85f))
		{
			string s19 = "PRESS [A] TO EXIT GAME";
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.DOOR5_LOCKED, s19, new Vector2(g.m_App.GetHudCentreX(s19, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_RED, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			m_NearTrigger = TRIGGERS.DOOR5;
		}
		name = "TriggerDoor6";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 16f, -0.85f))
		{
			if (m_Health < 10)
			{
				DoDamage(100, 1, -1, -1);
				return;
			}
			string s20 = "PRESS [A] TO OPEN DOOR";
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.DOOR6_LOCKED, s20, new Vector2(g.m_App.GetHudCentreX(s20, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			m_NearTrigger = TRIGGERS.DOOR6;
		}
		name = "TriggerHallucinate2";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 25f, 0f))
		{
			HallucinateOn();
			g.m_App.m_RumbleFrames = rumbleFrames;
			m_TickTockSFX = g.m_SoundManager.PlayLooped(39);
			g.m_ItemManager.m_Item[g.m_ItemManager.m_DoorItemId[3]].PeerCloseDoor();
			obj.m_Complete = true;
		}
		name = "TriggerClock";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 25f, 0f))
		{
			g.m_App.m_RumbleFrames = rumbleFrames;
			if (m_TickTockSFX != null)
			{
				m_TickTockSFX.Stop();
				m_TickTockSFX = null;
			}
			g.m_SoundManager.Play(40);
			obj.m_Complete = true;
		}
		name = "TriggerLinerSFX";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 16f, 0f))
		{
			g.m_SoundManager.Play(27);
			obj.m_Complete = true;
		}
		name = "TriggerOcean";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 16f, 0f))
		{
			m_OceanSFX = g.m_SoundManager.PlayLooped(41);
			obj.m_Complete = true;
		}
		name = "TriggerShakeyCam2";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 25f, 0f))
		{
			g.m_CameraManager.m_ShakeyCam = true;
			PlayViewAnim(1, loop: true, 0.2f);
			m_Health = 5;
			g.m_App.m_RumbleFrames = rumbleFrames;
			m_HeartBeatSFX = g.m_SoundManager.PlayLooped(38);
			obj.m_Complete = true;
		}
		name = "TriggerO2Full2";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && obj.m_Enabled)
		{
			if (FindFacingTrigger(obj.World.Translation, facing, 16f, -0.96f))
			{
				if (g.m_PlayerManager.m_OxygenAlienId == -1)
				{
					g.m_PlayerManager.m_OxygenAlienId = g.m_PlayerManager.Create(255, bot: true, TEAM.OxygenTanks).m_Id;
				}
				if (!obj.m_Complete)
				{
					string s21 = "PRESS [A] TO USE OXYGEN";
					g.m_App.AddHelmetMessage(HelmetMessage.TYPE.O2_FULL2, s21, new Vector2(g.m_App.GetHudCentreX(s21, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
				}
				else
				{
					string s22 = "EMPTY";
					g.m_App.AddHelmetMessage(HelmetMessage.TYPE.O2_FULL2, s22, new Vector2(g.m_App.GetHudCentreX(s22, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
				}
				m_NearTrigger = TRIGGERS.OXYGEN2;
			}
			else
			{
				g.m_App.RemoveHelmetMessage(HelmetMessage.TYPE.O2_FULL2);
			}
		}
		name = "TriggerErrorMessage";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 25f, 0f) && Gamer.SignedInGamers[g.m_App.m_PlayerOnePadId] != null)
		{
			List<string> list = new List<string>();
			list.Add("OK");
			Guide.BeginShowMessageBox(g.m_App.m_PlayerOnePadId, "Error Code 9", "You are running out of oxygen and will die", (IEnumerable<string>)list, 0, (MessageBoxIcon)1, (AsyncCallback)GetMBResult, (object)null);
			obj.m_Complete = true;
			MediaPlayer.Stop();
			if (m_TickTockSFX != null)
			{
				m_TickTockSFX.Stop();
			}
			if (m_BreatheSFX != null)
			{
				m_BreatheSFX.Stop();
			}
		}
		name = "Team4_Spawn1";
		TriggerEntity obj2 = null;
		if (g.m_PlayerManager.m_OxygenAlienId != -1 && !g.m_PlayerManager.m_OxygenAlienShown && g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj2) && FindFacingTrigger(obj2.World.Translation, facing, 400f, -0.85f))
		{
			g.m_PlayerManager.m_Player[g.m_PlayerManager.m_OxygenAlienId].DoOxygenScare();
			g.m_PlayerManager.m_OxygenAlienShown = true;
			g.m_CameraManager.m_LookAtPlayerId = g.m_PlayerManager.m_OxygenAlienId;
		}
		name = "Checkpoint2";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 25f, 0f))
		{
			g.m_App.m_CheckpointId = 2;
			obj.m_Complete = true;
			g.m_LoadSaveManager.SaveGame();
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.SAVING, "Saving Progress", new Vector2(g.m_App.GetHudCentreX("Saving Progress", g.m_App.lcdFont), 430f), 5f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.lcdFont, hideSysMsg: true);
		}
		name = "TriggerGraveSFX";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 49f, -0.9848f))
		{
			g.m_SoundManager.Play(28);
			g.m_App.m_HudDistort1Time = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 1f;
			g.m_App.m_HudDistort1Alpha = 1f;
			g.m_App.m_RumbleFrames = rumbleFrames;
			obj.m_Complete = true;
		}
		name = "TriggerKnockSFX";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 25f, 0f))
		{
			SoundEffectInstance soundEffectInstance = g.m_SoundManager.Play(42);
			soundEffectInstance.Pan = -1f;
			obj.m_Complete = true;
		}
		name = "TriggerScrewdriver";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled)
		{
			if (FindFacingTrigger(obj.World.Translation, facing, 16f, -0.94f))
			{
				string s23 = "[A] TO PICKUP SCREWDRIVER";
				g.m_App.AddHelmetMessage(HelmetMessage.TYPE.PICKUP_SCREWDRIVER, s23, new Vector2(g.m_App.GetHudCentreX(s23, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
				m_NearTrigger = TRIGGERS.SCREWDRIVER;
			}
			else
			{
				g.m_App.RemoveHelmetMessage(HelmetMessage.TYPE.PICKUP_SCREWDRIVER);
			}
		}
		float num4 = 0.5f;
		float num5 = 0.1f;
		name = "TriggerLocker1";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled)
		{
			if (g.m_ItemManager.m_Item[m_WeaponItemIndex].m_Type == 25)
			{
				Vector3 vector2 = g.m_ItemManager.m_Item[m_WeaponItemIndex].m_SceneObject.World.Translation + g.m_ItemManager.m_Item[m_WeaponItemIndex].m_SceneObject.World.Forward * num4;
				float num6 = (obj.World.Translation - vector2).LengthSquared();
				if (num6 < num5)
				{
					string s24 = "PRESS [RT] TO BREAK LOCK";
					g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USE_SCREWDRIVER, s24, new Vector2(g.m_App.GetHudCentreX(s24, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
					m_UsingTrigger = TRIGGERS.LOCKER1;
				}
			}
			else if (g.m_ItemManager.m_Item[g.m_ItemManager.m_Locker1Id].m_LockerState == Item.LOCKERSTATE.CLOSED && FindFacingTrigger(obj.World.Translation, facing, 16f, -0.94f))
			{
				string s25 = "LOCKED";
				g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USE_SCREWDRIVER, s25, new Vector2(g.m_App.GetHudCentreX(s25, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			}
		}
		name = "TriggerLocker2";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled)
		{
			if (g.m_ItemManager.m_Item[m_WeaponItemIndex].m_Type == 25)
			{
				Vector3 vector3 = g.m_ItemManager.m_Item[m_WeaponItemIndex].m_SceneObject.World.Translation + g.m_ItemManager.m_Item[m_WeaponItemIndex].m_SceneObject.World.Forward * num4;
				float num7 = (obj.World.Translation - vector3).LengthSquared();
				if (num7 < num5)
				{
					string s26 = "PRESS [RT] TO BREAK LOCK";
					g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USE_SCREWDRIVER, s26, new Vector2(g.m_App.GetHudCentreX(s26, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
					m_UsingTrigger = TRIGGERS.LOCKER2;
				}
			}
			else if (g.m_ItemManager.m_Item[g.m_ItemManager.m_Locker2Id].m_LockerState == Item.LOCKERSTATE.CLOSED && FindFacingTrigger(obj.World.Translation, facing, 16f, -0.94f))
			{
				string s27 = "LOCKED";
				g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USE_SCREWDRIVER, s27, new Vector2(g.m_App.GetHudCentreX(s27, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			}
		}
		name = "TriggerLocker3";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled)
		{
			if (g.m_ItemManager.m_Item[m_WeaponItemIndex].m_Type == 25)
			{
				Vector3 vector4 = g.m_ItemManager.m_Item[m_WeaponItemIndex].m_SceneObject.World.Translation + g.m_ItemManager.m_Item[m_WeaponItemIndex].m_SceneObject.World.Forward * num4;
				float num8 = (obj.World.Translation - vector4).LengthSquared();
				if (num8 < num5)
				{
					string s28 = "PRESS [RT] TO BREAK LOCK";
					g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USE_SCREWDRIVER, s28, new Vector2(g.m_App.GetHudCentreX(s28, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
					m_UsingTrigger = TRIGGERS.LOCKER3;
				}
			}
			else if (g.m_ItemManager.m_Item[g.m_ItemManager.m_Locker3Id].m_LockerState == Item.LOCKERSTATE.CLOSED && FindFacingTrigger(obj.World.Translation, facing, 16f, -0.94f))
			{
				string s29 = "LOCKED";
				g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USE_SCREWDRIVER, s29, new Vector2(g.m_App.GetHudCentreX(s29, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			}
		}
		name = "TriggerLocker4";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled)
		{
			if (g.m_ItemManager.m_Item[m_WeaponItemIndex].m_Type == 25)
			{
				Vector3 vector5 = g.m_ItemManager.m_Item[m_WeaponItemIndex].m_SceneObject.World.Translation + g.m_ItemManager.m_Item[m_WeaponItemIndex].m_SceneObject.World.Forward * num4;
				float num9 = (obj.World.Translation - vector5).LengthSquared();
				if (num9 < num5)
				{
					string s30 = "PRESS [RT] TO BREAK LOCK";
					g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USE_SCREWDRIVER, s30, new Vector2(g.m_App.GetHudCentreX(s30, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
					m_UsingTrigger = TRIGGERS.LOCKER4;
				}
			}
			else if (g.m_ItemManager.m_Item[g.m_ItemManager.m_Locker4Id].m_LockerState == Item.LOCKERSTATE.CLOSED && FindFacingTrigger(obj.World.Translation, facing, 16f, -0.94f))
			{
				string s31 = "LOCKED";
				g.m_App.AddHelmetMessage(HelmetMessage.TYPE.USE_SCREWDRIVER, s31, new Vector2(g.m_App.GetHudCentreX(s31, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			}
		}
		name = "TriggerO2Empty3";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled)
		{
			if (FindFacingTrigger(obj.World.Translation, facing, 16f, -0.85f))
			{
				string s32 = "EMPTY";
				g.m_App.AddHelmetMessage(HelmetMessage.TYPE.O2_EMPTY3, s32, new Vector2(g.m_App.GetHudCentreX(s32, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			}
			else
			{
				g.m_App.RemoveHelmetMessage(HelmetMessage.TYPE.O2_EMPTY3);
			}
		}
		name = "TriggerDoor7";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 16f, -0.85f))
		{
			if (m_DoorCrewExitUnlocked)
			{
				string s33 = "PRESS [A] TO OPEN DOOR";
				g.m_App.AddHelmetMessage(HelmetMessage.TYPE.DOOR7_LOCKED, s33, new Vector2(g.m_App.GetHudCentreX(s33, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			}
			else
			{
				string s34 = "DOOR 5 - OFFLINE";
				g.m_App.AddHelmetMessage(HelmetMessage.TYPE.DOOR7_LOCKED, s34, new Vector2(g.m_App.GetHudCentreX(s34, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			}
			m_NearTrigger = TRIGGERS.DOOR7;
		}
		name = "TriggerLCD4";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled)
		{
			if (FindFacingTrigger(obj.World.Translation, facing, 16f, -0.96f))
			{
				SceneEntity obj3 = null;
				if (g.m_App.sceneInterface.ObjectManager.Find("LCD4", onlysearchdynamicobjects: false, out obj3))
				{
					float num10 = Vector3.Dot(m_ViewSceneObject.World.Forward, obj3.World.Forward);
					if (num10 > 0f)
					{
						string s35 = "PRESS [A] TO USE COMPUTER";
						g.m_App.AddHelmetMessage(HelmetMessage.TYPE.LCD4_INTERACT, s35, new Vector2(g.m_App.GetHudCentreX(s35, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
						m_NearTrigger = TRIGGERS.LCD4;
					}
				}
			}
			else
			{
				g.m_App.RemoveHelmetMessage(HelmetMessage.TYPE.LCD4_INTERACT);
			}
		}
		name = "TriggerDoor9";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 16f, -0.85f))
		{
			string s36 = "[A] - ENTER CARGO BAY";
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.DOOR9, s36, new Vector2(g.m_App.GetHudCentreX(s36, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			m_NearTrigger = TRIGGERS.DOOR9;
		}
		name = "TriggerDoor8";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled)
		{
			if (FindFacingTrigger(obj.World.Translation, facing, 16f, -0.85f))
			{
				if (m_TabletsCollected == 5)
				{
					string s37 = "PRESS [A] TO OPEN DOOR";
					g.m_App.AddHelmetMessage(HelmetMessage.TYPE.DOOR8_LOCKED, s37, new Vector2(g.m_App.GetHudCentreX(s37, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
				}
				else
				{
					string s38 = "LOCKED - NEED ALL RESEARCH";
					g.m_App.AddHelmetMessage(HelmetMessage.TYPE.DOOR8_LOCKED, s38, new Vector2(g.m_App.GetHudCentreX(s38, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
					s38 = $"Found {m_TabletsCollected} of 5";
					g.m_App.AddHelmetMessage(HelmetMessage.TYPE.RESEARCHFOUND, s38, new Vector2(g.m_App.GetHudCentreX(s38, g.m_App.hudFont), 400f), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.END, null, hideSysMsg: true);
				}
				m_NearTrigger = TRIGGERS.DOOR8;
			}
			else
			{
				g.m_App.RemoveHelmetMessage(HelmetMessage.TYPE.DOOR8_LOCKED);
			}
		}
		name = "TriggerEnterCargo";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 16f, 0f))
		{
			string s39 = "FIND ALL RESEARCH DATA";
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.RESEARCH2, s39, new Vector2(g.m_App.GetHudCentreX(s39, g.m_App.hudFont), num2 + 30f), 8f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			s39 = "THEN ESCAPE THE SHIP";
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.RESEARCH3, s39, new Vector2(g.m_App.GetHudCentreX(s39, g.m_App.hudFont), num2 + 55f), 8f, g.m_App.HUD_GREEN, SoundManager.SFX.END, null, hideSysMsg: true);
			obj.m_Complete = true;
			g.m_SoundManager.Play(27);
			g.m_ItemManager.m_Item[g.m_ItemManager.m_DoorItemId[8]].PeerCloseDoor();
		}
		float rangeSq = 9f;
		name = "TriggerResearch1";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, rangeSq, -0.96f))
		{
			string s40 = "[A] - PICK UP RESEARCH DATA";
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.RESEARCH, s40, new Vector2(g.m_App.GetHudCentreX(s40, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			m_NearTrigger = TRIGGERS.RESEARCH1;
		}
		name = "TriggerResearch2";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, rangeSq, -0.96f))
		{
			string s41 = "[A] - PICK UP RESEARCH DATA";
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.RESEARCH, s41, new Vector2(g.m_App.GetHudCentreX(s41, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			m_NearTrigger = TRIGGERS.RESEARCH2;
		}
		name = "TriggerResearch3";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, rangeSq, -0.96f))
		{
			string s42 = "[A] - PICK UP RESEARCH DATA";
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.RESEARCH, s42, new Vector2(g.m_App.GetHudCentreX(s42, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			m_NearTrigger = TRIGGERS.RESEARCH3;
		}
		name = "TriggerResearch4";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, rangeSq, -0.96f))
		{
			string s43 = "[A] - PICK UP RESEARCH DATA";
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.RESEARCH, s43, new Vector2(g.m_App.GetHudCentreX(s43, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			m_NearTrigger = TRIGGERS.RESEARCH4;
		}
		name = "TriggerResearch5";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, rangeSq, -0.96f))
		{
			string s44 = "[A] - PICK UP RESEARCH DATA";
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.RESEARCH, s44, new Vector2(g.m_App.GetHudCentreX(s44, g.m_App.hudFont), num2), 0.5f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			m_NearTrigger = TRIGGERS.RESEARCH5;
		}
		name = "Checkpoint3";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 25f, 0f))
		{
			g.m_App.m_CheckpointId = 3;
			obj.m_Complete = true;
			g.m_LoadSaveManager.SaveGame();
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.SAVING, "Saving Progress", new Vector2(g.m_App.GetHudCentreX("Saving Progress", g.m_App.lcdFont), 430f), 5f, g.m_App.HUD_GREEN, SoundManager.SFX.END, g.m_App.lcdFont, hideSysMsg: true);
		}
		name = "CargoAirlock";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 16f, 0f))
		{
			string s45 = "MISSION COMPLETE";
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.RESEARCH, s45, new Vector2(g.m_App.GetHudCentreX(s45, g.m_App.hudFont), num2), 20f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			obj.m_Complete = true;
			g.m_ItemManager.m_Item[g.m_ItemManager.m_DoorItemId[7]].PeerCloseDoor();
			MediaPlayer.Stop();
			g.m_App.m_CompleteMusic = g.m_App.Content.Load<Song>("Music/HeavyBrigade");
			MediaPlayer.Play(g.m_App.m_CompleteMusic);
			MediaPlayer.IsRepeating = true;
			if (g.m_PlayerManager.m_CargoBayAlienId != -1)
			{
				g.m_PlayerManager.Delete(g.m_PlayerManager.m_CargoBayAlienId);
				g.m_PlayerManager.m_CargoBayAlienId = -1;
				g.m_PlayerManager.m_CargoBayAlienShown = false;
			}
			if (g.m_App.m_CargoLoopSFX != null)
			{
				g.m_App.m_CargoLoopSFX.Stop();
				g.m_App.m_CargoLoopSFX = null;
			}
		}
		name = "CargoAirlock2";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 16f, 0f))
		{
			string s46 = "INITIATING SELF DESTRUCT";
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.ENTERCARGO, s46, new Vector2(g.m_App.GetHudCentreX(s46, g.m_App.hudFont), num2 + 40f), 7f, g.m_App.HUD_RED, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			s46 = "HEADING TO SHIP";
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.AIRLOCK, s46, new Vector2(g.m_App.GetHudCentreX(s46, g.m_App.hudFont), num2 + 80f), 7f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			obj.m_Complete = true;
			g.m_PlayerManager.m_GameCompleteTimer = num + 7f;
			if (g.m_App.m_AlarmSFX == null)
			{
				g.m_App.m_AlarmSFX = g.m_SoundManager.PlayLooped(23);
			}
			if (g.m_App.m_GameplayScreen.m_AlarmLight[0] != null)
			{
				g.m_App.m_GameplayScreen.m_AlarmLight[0].Enabled = true;
			}
		}
		name = "TriggerCargoCorpse";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj) && obj.m_NextUseTime < num && !obj.m_Complete && obj.m_Enabled && FindFacingTrigger(obj.World.Translation, facing, 400f, -0.85f))
		{
			obj.m_Complete = true;
			g.m_SoundManager.Play(27);
		}
		if (m_TabletsCollected == 1 && g.m_PlayerManager.m_CargoBayAlienId == -1)
		{
			g.m_PlayerManager.m_CargoBayAlienId = g.m_PlayerManager.Create(255, bot: true, TEAM.CargoBay).m_Id;
			g.m_App.m_CargoLoopSFX = g.m_SoundManager.PlayLooped(48);
		}
		if (m_TabletsCollected == 5 && !g.m_PlayerManager.m_bShowAirlockMessage)
		{
			string s47 = "GO TO THE AIRLOCK";
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.RESEARCH, s47, new Vector2(g.m_App.GetHudCentreX(s47, g.m_App.hudFont), num2 + 25f), 15f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
			g.m_PlayerManager.m_bShowAirlockMessage = true;
		}
	}

	protected void GetMBResult(IAsyncResult r)
	{
		Guide.EndShowMessageBox(r);
		MediaPlayer.Play(g.m_App.m_HallucinateMusic);
		MediaPlayer.IsRepeating = true;
		MediaPlayer.Volume = g.m_App.m_OptionsMusicVol;
		if (m_BreatheSFX != null)
		{
			g.m_SoundManager.PlayLooped(24);
		}
		if (m_TickTockSFX != null)
		{
			g.m_SoundManager.PlayLooped(39);
		}
	}

	public void ResetCheckpoint1Triggers()
	{
		string name = "TriggerHallucinate";
		MiscTriggerEntity obj = null;
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
		{
			obj.m_Complete = false;
		}
		name = "TriggerSpaceman";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
		{
			obj.m_Complete = false;
		}
		name = "TriggerPhone";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
		{
			obj.m_Complete = false;
		}
		name = "TriggerShakeyCam";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
		{
			obj.m_Complete = false;
		}
		g.m_App.RemoveHelmetMessage(HelmetMessage.TYPE.RESEARCH);
	}

	public void ResetCheckpoint3Triggers()
	{
		m_TabletsCollected = 0;
		if (g.m_PlayerManager.m_CargoBayAlienId != -1)
		{
			g.m_PlayerManager.Delete(g.m_PlayerManager.m_CargoBayAlienId);
			g.m_PlayerManager.m_CargoBayAlienId = -1;
			g.m_PlayerManager.m_CargoBayAlienShown = false;
		}
		g.m_ItemManager.SetupTablets();
		string name = "TriggerEnterCargo";
		MiscTriggerEntity obj = null;
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
		{
			obj.m_Complete = false;
		}
		name = "TriggerDoor8";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
		{
			obj.m_Complete = false;
		}
		name = "TriggerCargoCorpse";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
		{
			obj.m_Complete = false;
		}
		g.m_ItemManager.m_Item[g.m_ItemManager.m_DoorItemId[7]].PeerCloseDoor();
		g.m_CameraManager.m_LookAtPlayerId = -1;
		string s = $"Found {m_TabletsCollected} of 5";
		g.m_App.AddHelmetMessage(HelmetMessage.TYPE.RESEARCHFOUND, s, new Vector2(g.m_App.GetHudCentreX(s, g.m_App.hudFont), 400f), 15f, g.m_App.HUD_GREEN, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
		g.m_PlayerManager.m_GameCompleteTimer = 0f;
		g.m_PlayerManager.m_bShowLifeformMessage = false;
		g.m_PlayerManager.m_bShowAirlockMessage = false;
		if (g.m_App.m_GameplayScreen.m_AlarmLight[0] != null)
		{
			g.m_App.m_GameplayScreen.m_AlarmLight[0].Enabled = false;
		}
		if (g.m_App.m_CargoLoopSFX != null)
		{
			g.m_App.m_CargoLoopSFX.Stop();
			g.m_App.m_CargoLoopSFX = null;
		}
	}

	public void HallucinateOn()
	{
		if (!g.m_App.SOUNDON)
		{
			return;
		}
		m_Hallucinate = true;
		if (g.m_App.SOUNDON)
		{
			MediaPlayer.Play(g.m_App.m_HallucinateMusic);
			MediaPlayer.IsRepeating = true;
			MediaPlayer.Volume = g.m_App.m_OptionsMusicVol;
			g.m_SoundManager.Play(28);
			if (m_BreatheSFX != null)
			{
				m_BreatheSFX.Volume = 1f;
			}
		}
		m_Health = 25;
	}

	public void HallucinateOff(bool rumble)
	{
		m_Hallucinate = false;
		if (g.m_App.SOUNDON)
		{
			MediaPlayer.Play(g.m_App.m_Level1Music);
			MediaPlayer.IsRepeating = true;
			MediaPlayer.Volume = g.m_App.m_OptionsMusicVol;
			if (m_BreatheSFX != null)
			{
				m_BreatheSFX.Volume = 0.75f;
			}
		}
		if (m_HeartBeatSFX != null)
		{
			m_HeartBeatSFX.Stop();
		}
		if (rumble)
		{
			g.m_SoundManager.Play(16);
			g.m_App.m_RumbleFrames = 10;
		}
		m_Health = 100;
		g.m_CameraManager.m_ShakeyCam = false;
		PlayViewAnim(0, loop: true, 0.2f);
	}

	public void SawArm()
	{
		MiscTriggerEntity obj = null;
		string name = "TriggerArm";
		g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj);
		if (!obj.m_Complete)
		{
			if (m_NumSaws == 0)
			{
				g.m_SoundManager.Play(47);
			}
			m_NumSaws++;
			g.m_SoundManager.Play(36);
			g.m_App.m_RumbleFrames = 5;
			m_BloodSpray.Enabled = true;
			m_BloodSpray.Visible = true;
			m_BloodSpray.Emitter.BurstParticles = 50;
			m_BloodSpray.Emitter.PositionData.Position = obj.World.Translation;
			m_BloodSpray.Emitter.PositionData.Velocity = Vector3.Zero;
			m_BloodSpray.LerpEmittersPositionAndOrientationOnNextUpdate = false;
			m_BloodSpray.Normal = new Vector3(0f, -1f, 0f) * 2f;
			if (m_NumSaws > 10)
			{
				obj.m_Complete = true;
				g.m_SoundManager.Play(30);
			}
		}
	}

	public void OpenLocker()
	{
		int num = 0;
		switch (m_UsingTrigger)
		{
		case TRIGGERS.LOCKER1:
			num = 1;
			break;
		case TRIGGERS.LOCKER2:
			num = 2;
			break;
		case TRIGGERS.LOCKER3:
			num = 3;
			break;
		case TRIGGERS.LOCKER4:
			num = 4;
			break;
		}
		if (num != 0)
		{
			MiscTriggerEntity obj = null;
			string name = $"TriggerLocker{num}";
			g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj);
			if (obj != null && !obj.m_Complete)
			{
				obj.m_Complete = true;
				g.m_SoundManager.Play(43);
				g.m_App.m_RumbleFrames = 5;
			}
		}
	}

	private void DoMedbayScare()
	{
		m_SceneObject.Visibility = ObjectVisibility.RenderedAndCastShadows;
		m_AnimationSet.CrossFade(m_Model.AnimationClips.Values[2], TimeSpan.FromSeconds(0.0));
		m_AnimationSet.LoopEnabled = false;
		m_Anim = 2;
		g.m_SoundManager.Play(37);
	}

	private void DoOxygenScare()
	{
		m_SceneObject.Visibility = ObjectVisibility.RenderedAndCastShadows;
		m_AnimationSet.CrossFade(m_Model.AnimationClips.Values[3], TimeSpan.FromSeconds(0.0));
		m_AnimationSet.LoopEnabled = false;
		m_Anim = 3;
		g.m_SoundManager.Play(37);
	}

	public void InitBot()
	{
		m_Anim = byte.MaxValue;
		m_BotAction = BOTACTION.Idle;
		m_TargetNode = -1;
		m_BotVecTarget = Vector3.Zero;
		m_BotVecMove = Vector3.Zero;
		m_BotTargetRotY = 0f;
		m_BotSpeed = 0f;
		m_NextActionTime = 0f;
		m_EnemyId = -1;
		m_LookForEnemyTime = 0f;
		m_BerserkTime = 0f;
		m_BotNameIdx = (byte)((m_NetId - 1) & 7);
		if (m_BotNameIdx < 0)
		{
			m_BotNameIdx = 0;
		}
		if (m_BotNameIdx > 6)
		{
			m_BotNameIdx = 6;
		}
		m_BotAllowFire = true;
		m_BotAllowMove = true;
		if (IsHost())
		{
			Spawn();
		}
		if (m_Team == TEAM.Hunter)
		{
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.WARNING, string.Format("WARNING: UNAUTHORISED", GetName()), new Vector2(72f, 170f), 6f, g.m_App.HUD_RED, SoundManager.SFX.END, null, hideSysMsg: true);
			g.m_App.AddHelmetMessage(HelmetMessage.TYPE.RIVAL, string.Format("DOCKING DETECTED", GetName()), new Vector2(112f, 190f), 6f, g.m_App.HUD_RED, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
		}
		m_TempPrevBotNodeID = -1;
		m_TempTargetNode = -1;
		m_TempPathTime = 0;
		if (g.m_App.m_SurvivalMode)
		{
			m_BotAccuracy = 0.7f;
		}
		else
		{
			m_BotAccuracy = 0.3f;
		}
		m_AlienTimeout = 6f;
	}

	public void BotSetClassAndTeam(TEAM botTeam)
	{
		switch (botTeam)
		{
		case TEAM.Hunter:
			PeerSetTeam(TEAM.Hunter);
			PeerSetClass(CLASS.FatherD);
			break;
		case TEAM.Vampire:
			PeerSetTeam(TEAM.Vampire);
			PeerSetClass(CLASS.Edgar);
			break;
		case TEAM.MedBay:
			PeerSetTeam(TEAM.MedBay);
			PeerSetClass(CLASS.MedBay);
			break;
		case TEAM.OxygenTanks:
			PeerSetTeam(TEAM.OxygenTanks);
			PeerSetClass(CLASS.OxygenTanks);
			break;
		case TEAM.CargoBay:
			PeerSetTeam(TEAM.CargoBay);
			PeerSetClass(CLASS.CargoBay);
			break;
		}
	}

	private void UpdateBot()
	{
		float num = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds;
		CheckSurvivalAccuracy();
		switch (m_BotAction)
		{
		case BOTACTION.Idle:
			(g.m_PlayerManager.GetLocalPlayer().m_Position - m_Position).LengthSquared();
			ClearTempBotNodes();
			if (m_bRagdoll)
			{
				m_BotAction = BOTACTION.Dying;
				break;
			}
			m_BotVecTarget = g.m_PlayerManager.GetLocalPlayer().m_Position - m_Position;
			m_BotVecTarget.Y = 0f;
			m_BotVecTarget.Normalize();
			m_BotTargetRotY = (float)Math.Atan2(0f - m_BotVecTarget.X, 0f - m_BotVecTarget.Z);
			m_BotSpeed = 0f;
			if (m_LookForEnemyTime < num)
			{
				m_EnemyId = LookForEnemy();
				m_LookForEnemyTime = 0f;
			}
			if (m_EnemyId != -1 && CanSeeTargetEnemy())
			{
				m_BotAction = BOTACTION.VampireAttacking;
				m_AttackTimeout = num + 9f + (float)g.m_App.m_Rand.Next(0, 3);
				m_NextActionTime = num + m_AlienTimeout + (float)g.m_PlayerManager.m_NumDeaths;
				int num4 = g.m_App.m_Rand.Next(0, 3);
				g.m_SoundManager.Play3D(44 + num4, m_Position);
				if (!g.m_PlayerManager.m_bShowLifeformMessage)
				{
					string s = "Life Form Detected";
					g.m_App.AddHelmetMessage(HelmetMessage.TYPE.ENTERCARGO, s, new Vector2(g.m_App.GetHudCentreX(s, g.m_App.hudFont), 150f), 7f, g.m_App.HUD_RED, SoundManager.SFX.HelmetWarning, null, hideSysMsg: true);
					g.m_PlayerManager.m_bShowLifeformMessage = true;
				}
			}
			break;
		case BOTACTION.Searching:
			if (m_TargetNode == -1)
			{
				m_BotAction = BOTACTION.Idle;
				break;
			}
			m_Leap = false;
			if (m_bRagdoll)
			{
				m_BotAction = BOTACTION.Dying;
				break;
			}
			if (NearTargetNode())
			{
				if (m_bTryJoinMainPath && TryJoinMainPath())
				{
					break;
				}
				if (m_TargetDirectionForward)
				{
					m_TargetNode++;
					if (m_TargetNode >= g.m_BotPathManager.m_BotPath[m_CurrentPathId].m_NumNodes)
					{
						m_TargetNode = 0;
					}
				}
				else
				{
					m_TargetNode--;
					if (m_TargetNode < 0)
					{
						m_TargetNode = g.m_BotPathManager.m_BotPath[m_CurrentPathId].m_NumNodes - 1;
					}
				}
			}
			m_BotVecTarget = g.m_BotPathManager.m_BotPath[m_CurrentPathId].m_BotNode[m_TargetNode].m_Position - m_Position;
			m_BotVecTarget.Y = 0f;
			m_BotVecTarget.Normalize();
			m_BotVecMove = m_BotVecTarget * 0.1f;
			m_BotTargetRotY = (float)Math.Atan2(0f - m_BotVecTarget.X, 0f - m_BotVecTarget.Z);
			m_BotSpeed = 5f;
			if (m_LookForEnemyTime < num)
			{
				m_EnemyId = LookForEnemy();
				m_LookForEnemyTime = num + 0.2f + (float)g.m_App.m_Rand.NextDouble() * 0.1f;
			}
			if (m_EnemyId != -1 && m_BotAllowFire && m_AttackTimeout < num)
			{
				if (!CanSeeTargetEnemy())
				{
					m_AttackTimeout = num + 0.4f + (float)g.m_App.m_Rand.NextDouble() * 0.2f;
					break;
				}
				if (m_Team == TEAM.Vampire)
				{
					m_BotAction = BOTACTION.VampireAttacking;
					m_AttackTimeout = num + 9f + (float)g.m_App.m_Rand.Next(0, 3);
				}
				else
				{
					m_BotAction = BOTACTION.Attacking;
					m_AttackTimeout = num + 2.5f + (float)g.m_App.m_Rand.NextDouble();
					m_BotSpeed = 0f;
				}
				m_NextActionTime = num + 0.1f;
			}
			else if (m_NextActionTime < num)
			{
				if (g.m_App.m_Rand.NextDouble() > 0.5 && !m_bTryJoinMainPath)
				{
					m_TargetDirectionForward = !m_TargetDirectionForward;
				}
				m_BotAction = BOTACTION.Idle;
				m_BotSpeed = 0f;
				m_NextActionTime = num + 2.5f + (float)g.m_App.m_Rand.NextDouble();
			}
			break;
		case BOTACTION.Dying:
			if (!m_bRagdoll)
			{
				if (TempBotNodeCount() > 1)
				{
					m_TempPathTime = 0;
					m_BotAction = BOTACTION.FindPath;
				}
				else
				{
					m_BotAction = BOTACTION.Idle;
				}
				break;
			}
			UpdateRagdoll();
			m_BotSpeed = 0f;
			m_Rotation.Y = 0f;
			m_TorchLight.Enabled = false;
			m_AttackTimeout = 0f;
			m_EnemyId = -1;
			m_LookForEnemyTime = 0f;
			m_BerserkTime = 0f;
			m_Leap = false;
			break;
		case BOTACTION.VampireAttacking:
		{
			AddTempNode();
			if (CheckCaught())
			{
				m_BotAction = BOTACTION.Caught;
				m_NextActionTime = num + 3f;
				g.m_SoundManager.Play(45);
				g.m_SoundManager.Play(28);
				g.m_App.m_RumbleFrames = 20;
				g.m_PlayerManager.GetLocalPlayer().m_State = STATE.Grabbed;
				break;
			}
			float num2 = 0f;
			Vector3 zero = Vector3.Zero;
			zero = g.m_PlayerManager.m_Player[m_EnemyId].m_Position;
			num2 = (zero - m_Position).LengthSquared();
			if (m_NextActionTime < num)
			{
				m_NextActionTime = num + m_AlienTimeout + (float)g.m_PlayerManager.m_NumDeaths;
				if (!CanSeeTargetEnemy())
				{
					Vector3 pos = Vector3.Zero;
					float rotY = 0f;
					g.m_PlayerManager.FindCargoSpawnPoint(out pos, out rotY);
					m_Position = pos;
					m_NetworkPosition = pos;
					m_Rotation.Y = rotY;
					m_BotTargetRotY = rotY;
					m_NetworkRotation = rotY;
					m_PrevPosition = pos;
					m_CharacterController.Body.Position = m_Position;
					m_BotAction = BOTACTION.Idle;
					m_BotSpeed = 0f;
					m_BotVecMove = Vector3.Zero;
					ClearMovement();
					break;
				}
			}
			m_BotVecTarget = zero - m_Position;
			m_BotVecTarget.Y = 0f;
			m_BotVecTarget.Normalize();
			m_BotVecMove = m_BotVecTarget;
			m_BotSpeed = (0f - m_AnimationSet.DeltaZ) * 60f;
			Vector3 vector = zero - m_Position;
			m_BotTargetRotY = (float)Math.Atan2(0f - vector.X, 0f - vector.Z);
			g.m_App.m_HudDistort1Time = num + 0.25f;
			float value = 1f - num2 / 3600f;
			value = MathHelper.Clamp(value, 0f, 1f);
			g.m_App.m_HudDistort1Alpha = value;
			break;
		}
		case BOTACTION.FindPath:
			m_TempPathTime++;
			if (m_TempPathTime > 1000)
			{
				m_BotAction = BOTACTION.Idle;
				m_TargetNode = -1;
			}
			else if (TempBotNodeCount() > 1)
			{
				if (m_TempTargetNode == -1)
				{
					m_TargetNode = -1;
					m_BotAction = BOTACTION.Idle;
					break;
				}
				m_Leap = false;
				if (m_bRagdoll)
				{
					m_BotAction = BOTACTION.Dying;
					break;
				}
				int num3 = VeryNearAnyPathNode();
				if (num3 != -1)
				{
					m_TargetNode = num3;
					m_BotAction = BOTACTION.Idle;
					break;
				}
				if (NearTempBotNode())
				{
					m_TempTargetNode--;
					if (m_TempTargetNode < 0)
					{
						m_TargetNode = -1;
						m_BotAction = BOTACTION.Idle;
						break;
					}
				}
				m_BotVecTarget = m_TempBotNode[m_TempTargetNode].m_Position - m_Position;
				m_BotVecTarget.Y = 0f;
				m_BotVecTarget.Normalize();
				m_BotTargetRotY = (float)Math.Atan2(0f - m_BotVecTarget.X, 0f - m_BotVecTarget.Z);
				m_BotVecMove = m_BotVecTarget;
				m_BotSpeed = (0f - m_AnimationSet.DeltaZ) * 60f;
			}
			else
			{
				m_BotAction = BOTACTION.Idle;
			}
			break;
		case BOTACTION.Caught:
			g.m_CameraManager.m_LookAtPlayerId = m_Id;
			ClearMovement();
			m_BotSpeed = 0f;
			g.m_App.m_HudDistort1Time = num + 0.1f;
			g.m_App.m_HudDistort1Alpha = 1f;
			if (m_NextActionTime < num)
			{
				RequestDamageOther(0, 100, g.m_PlayerManager.GetLocalPlayer(), -1);
				m_BotAction = BOTACTION.Idle;
			}
			break;
		}
		_ = m_BotAction;
		_ = m_PrevBotAction;
		m_PrevBotAction = m_BotAction;
	}

	private bool CheckCaught()
	{
		float num = (g.m_PlayerManager.GetLocalPlayer().m_Position - m_Position).LengthSquared();
		if (num < 16f)
		{
			return true;
		}
		return false;
	}

	private bool TryJoinMainPath()
	{
		int num = g.m_BotPathManager.m_BotPath[0].FindNearestNodeInRange(m_Position, 25f);
		if (num != -1)
		{
			m_CurrentPathId = 0;
			m_TargetNode = num;
			m_bTryJoinMainPath = false;
			return true;
		}
		if (m_TargetNode >= g.m_BotPathManager.m_BotPath[m_CurrentPathId].m_NumNodes)
		{
			m_CurrentPathId = 0;
			m_TargetNode = -1;
			m_bTryJoinMainPath = false;
		}
		return false;
	}

	public void UseSpindlePath()
	{
		m_CurrentPathId = m_SpawnId;
		m_bTryJoinMainPath = true;
		m_TargetDirectionForward = true;
		m_TargetNode = -1;
		if (m_CurrentPathId == -1)
		{
			m_CurrentPathId = 0;
		}
	}

	private void AddTempNode()
	{
		if (m_TempPrevBotNodeID != -1)
		{
			float num = (m_Position - m_TempBotNode[m_TempPrevBotNodeID].m_Position).LengthSquared();
			if (num > 25f)
			{
				m_TempPrevBotNodeID = CreateTempNode(0, m_Position);
			}
		}
		else
		{
			m_TempPrevBotNodeID = CreateTempNode(0, m_Position);
		}
		m_TempTargetNode = m_TempPrevBotNodeID;
	}

	public int CreateTempNode(int type, Vector3 pos)
	{
		bool flag = false;
		int num = -1;
		for (int i = 0; i < 1024; i++)
		{
			if (m_TempBotNode[i].m_Type == -1)
			{
				flag = true;
				num = i;
				break;
			}
		}
		if (flag)
		{
			m_TempBotNode[num].m_Type = type;
			m_TempBotNode[num].m_Position = pos;
			return num;
		}
		return -1;
	}

	private int TempBotNodeCount()
	{
		for (int i = 0; i < 256; i++)
		{
			if (m_TempBotNode[i].m_Type == -1)
			{
				return i;
			}
		}
		return 0;
	}

	private void ClearTempBotNodes()
	{
		for (int i = 0; i < 256; i++)
		{
			m_TempBotNode[i].m_Type = -1;
		}
		m_TempTargetNode = -1;
		m_TempPrevBotNodeID = -1;
	}

	private bool NearTempBotNode()
	{
		if (m_TempTargetNode == -1)
		{
			return false;
		}
		Vector3 vector = m_Position - m_TempBotNode[m_TempTargetNode].m_Position;
		vector.Y = 0f;
		float num = vector.LengthSquared();
		if ((double)num < 4.0)
		{
			return true;
		}
		return false;
	}

	public void RenderTempNodes()
	{
	}

	private bool NearTargetNode()
	{
		if (m_TargetNode == -1)
		{
			return false;
		}
		Vector3 vector = m_Position - g.m_BotPathManager.m_BotPath[m_CurrentPathId].m_BotNode[m_TargetNode].m_Position;
		vector.Y = 0f;
		float num = vector.LengthSquared();
		if ((double)num < 4.0)
		{
			return true;
		}
		return false;
	}

	private int VeryNearAnyPathNode()
	{
		int num = g.m_BotPathManager.m_BotPath[m_CurrentPathId].FindNearestNode(m_Position);
		if (num == -1)
		{
			return -1;
		}
		Vector3 vector = m_Position - g.m_BotPathManager.m_BotPath[m_CurrentPathId].m_BotNode[num].m_Position;
		vector.Y = 0f;
		float num2 = vector.LengthSquared();
		if (num2 < 1f)
		{
			return num;
		}
		return -1;
	}

	private int LookForEnemy()
	{
		Vector3 botVecTarget = m_BotVecTarget;
		Vector3 zero = Vector3.Zero;
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		int num4 = -1;
		float num5 = 9999999f;
		if (m_BerserkTime > (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds)
		{
			num3 = 1600f;
		}
		for (int i = 0; i < 16; i++)
		{
			bool flag = true;
			if (g.m_PlayerManager.m_Player[i].m_SceneObject == null)
			{
				if (g.m_PlayerManager.m_Player[i].m_Health <= 0)
				{
					flag = false;
				}
			}
			else if (g.m_PlayerManager.m_Player[i].m_SceneObject.Visibility == ObjectVisibility.None || g.m_PlayerManager.m_Player[i].m_bRagdoll)
			{
				flag = false;
			}
			if (g.m_App.m_SurvivalMode && g.m_PlayerManager.m_Player[i].m_Bot)
			{
				flag = false;
			}
			if (g.m_PlayerManager.m_Player[i].m_Id == -1 || g.m_PlayerManager.m_Player[i].m_Id == m_Id || !flag)
			{
				continue;
			}
			zero = g.m_PlayerManager.m_Player[i].m_Position - m_Position;
			if (!(Math.Abs(zero.Y) > 6f))
			{
				num2 = zero.LengthSquared();
				zero.Normalize();
				num = Vector3.Dot(zero, botVecTarget);
				if (num > 0.8f && num2 < num5)
				{
					num5 = num2;
					num4 = i;
				}
				if (num2 < 412.90237f + num3 && num2 < num5)
				{
					num5 = num2;
					num4 = i;
				}
			}
		}
		if (num4 == -1)
		{
			return -1;
		}
		return g.m_PlayerManager.m_Player[num4].m_Id;
	}

	private int LookForEnemyToStake()
	{
		Vector3 botVecTarget = m_BotVecTarget;
		Vector3 zero = Vector3.Zero;
		float num = 0f;
		float num2 = 0f;
		if (m_Team == TEAM.Vampire)
		{
			return -1;
		}
		for (int i = 0; i < 16; i++)
		{
			if (g.m_PlayerManager.m_Player[i].m_Id != -1 && g.m_PlayerManager.m_Player[i].m_Id != m_Id && (g.m_PlayerManager.m_Player[i].m_bRagdoll || g.m_PlayerManager.m_Player[i].m_Health <= 0) && !g.m_PlayerManager.m_Player[i].m_bStaked && g.m_PlayerManager.m_Player[i].m_Team == TEAM.Vampire)
			{
				zero = g.m_PlayerManager.m_Player[i].m_Position - m_Position;
				num2 = zero.LengthSquared();
				zero.Normalize();
				num = Vector3.Dot(zero, botVecTarget);
				if (num > 0.8f && num2 < 16129f)
				{
					return g.m_PlayerManager.m_Player[i].m_Id;
				}
			}
		}
		return -1;
	}

	private bool CanSeeTargetEnemy()
	{
		if (g.m_BotPathManager.m_bDoneLineOfSightRaycast)
		{
			return false;
		}
		Vector3 aimPosition = GetAimPosition();
		Vector3 zero = Vector3.Zero;
		zero = ((!g.m_PlayerManager.m_Player[m_EnemyId].m_bRagdoll) ? g.m_PlayerManager.m_Player[m_EnemyId].GetAimPosition() : g.m_PlayerManager.m_Player[m_EnemyId].m_Position);
		Vector3 value = zero - aimPosition;
		value = Vector3.Normalize(value);
		return CanSeeTargetEnemyRaycast(aimPosition, value, 127f);
	}

	private bool CanSeeTargetEnemyRaycast(Vector3 position, Vector3 direction, float range)
	{
		List<RayCastResult> list = new List<RayCastResult>();
		Ray ray = new Ray(position, direction);
		if (g.m_App.m_Space.RayCast(ray, range, list))
		{
			list.Sort();
			for (int i = 0; i < list.Count; i++)
			{
				if (!(list[i].HitObject is EntityCollidable entityCollidable))
				{
					return false;
				}
				if (entityCollidable.Entity.Tag is HitTag hitTag && hitTag.m_PlayerId != m_Id && hitTag.m_PlayerId == m_EnemyId && hitTag.m_HitZone != 255)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void SetAimAtRagdoll(int enemyId)
	{
		m_BotAimVector = new Vector3(0f, -1f, 0f);
	}

	public void DeleteBot()
	{
		g.m_App.m_RequestDeleteBotId = m_NetId;
		Delete();
	}

	public void BotTookDamage()
	{
		m_BerserkTime = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 5f;
	}

	private void CheckSurvivalAccuracy()
	{
		if (g.m_App.m_SurvivalMode)
		{
			int totalPlayerScore = g.m_PlayerManager.GetTotalPlayerScore();
			if (totalPlayerScore > 60)
			{
				m_BotAccuracy = 0.1f;
			}
			else if (totalPlayerScore > 50)
			{
				m_BotAccuracy = 0.2f;
			}
			else if (totalPlayerScore > 40)
			{
				m_BotAccuracy = 0.3f;
			}
			else if (totalPlayerScore > 30)
			{
				m_BotAccuracy = 0.4f;
			}
			else if (totalPlayerScore > 20)
			{
				m_BotAccuracy = 0.5f;
			}
			else if (totalPlayerScore > 10)
			{
				m_BotAccuracy = 0.6f;
			}
			else
			{
				m_BotAccuracy = 0.7f;
			}
		}
	}

	public void UpdateBotSpaceman()
	{
		if (!m_AnimationSet.IsPlaying)
		{
			Delete();
		}
	}

	public void UpdateBotMedbay()
	{
		if (m_Anim == 2 && !m_AnimationSet.IsPlaying)
		{
			Delete();
			g.m_CameraManager.m_LookAtPlayerId = -1;
		}
	}

	public void UpdateBotOxygenTanks()
	{
		if (m_Anim == 3 && !m_AnimationSet.IsPlaying)
		{
			Delete();
			g.m_CameraManager.m_LookAtPlayerId = -1;
		}
	}
}
