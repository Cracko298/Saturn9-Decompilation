using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SynapseGaming.LightingSystem.Collision;
using SynapseGaming.LightingSystem.Core;
using SynapseGaming.LightingSystem.Lights;
using SynapseGaming.LightingSystem.Rendering;

namespace Saturn9;

public class ItemManager
{
	public class CrateContents
	{
		public enum CRATECONTENTS
		{
			NONE,
			ARTIFACT,
			ALIEN,
			RPG,
			HEALTH,
			AMMO,
			LOCATOR
		}

		public int m_ItemId;

		public int m_CrateId;

		public CRATECONTENTS m_Contents;

		public bool m_bUsed;

		public CrateContents()
		{
			m_ItemId = -1;
			m_Contents = CRATECONTENTS.NONE;
			m_bUsed = false;
		}
	}

	public const int MAX_ITEMS = 125;

	private const int NUM_DOORS = 9;

	private const int NUM_FOG = 18;

	public const int NUM_CRATES = 22;

	public Item[] m_Item;

	public Model[] m_Model;

	private int m_NextId;

	public Texture2D m_ShotgunCrosshair;

	public Texture2D m_Mac10Crosshair;

	public Texture2D m_CrossbowCrosshair;

	public Texture2D m_RPGCrosshair;

	private byte m_ItemNetId;

	public int[] m_DoorItemId = new int[9];

	public Vector3[] m_FogPos = new Vector3[18];

	public int m_FogItemId;

	private int m_FogBankId = -1;

	public int m_Locker1Id = -1;

	public int m_Locker2Id = -1;

	public int m_Locker3Id = -1;

	public int m_Locker4Id = -1;

	private ExplosionFireSmokeParticleSystem[] m_PrecacheWeaponSmoke;

	private ExplosionFlyingSparksParticleSystem[] m_PrecacheWeaponSparks;

	private BuckshotQuadSprayParticleSystem[] m_PrecacheWeaponBuckshot;

	private RPGShellTrailParticleSystem[] m_PrecacheRPGTrail;

	private AnimatedQuadParticleSystem[] m_PrecacheRPGExplosion;

	private int m_SmokePrecacheId = -1;

	private int m_SparksPrecacheId = -1;

	private int m_BuckshotPrecacheId = -1;

	private int m_RPGTrailPrecacheId = -1;

	private int m_RPGExplosionPrecacheId = -1;

	private int MAX_SMOKE_PRECACHE = 24;

	private int MAX_SPARKS_PRECACHE = 64;

	private int MAX_BUCKSHOT_PRECACHE = 64;

	private int MAX_RPGTRAIL_PRECACHE = 16;

	private int MAX_RPGEXPLOSION_PRECACHE = 24;

	private CrateContents[] m_CrateContents;

	private int m_ArtifactCrateId;

	public void PrecacheVFX()
	{
		m_PrecacheWeaponSmoke = new ExplosionFireSmokeParticleSystem[MAX_SMOKE_PRECACHE];
		m_PrecacheWeaponSparks = new ExplosionFlyingSparksParticleSystem[MAX_SPARKS_PRECACHE];
		m_PrecacheWeaponBuckshot = new BuckshotQuadSprayParticleSystem[MAX_BUCKSHOT_PRECACHE];
		m_PrecacheRPGTrail = new RPGShellTrailParticleSystem[MAX_RPGTRAIL_PRECACHE];
		m_PrecacheRPGExplosion = new AnimatedQuadParticleSystem[MAX_RPGEXPLOSION_PRECACHE];
		for (int i = 0; i < MAX_SMOKE_PRECACHE; i++)
		{
			m_PrecacheWeaponSmoke[i] = new ExplosionFireSmokeParticleSystem(g.m_App);
			m_PrecacheWeaponSmoke[i].AutoInitialize(g.m_App.GraphicsDevice, g.m_App.Content, g.m_App.screenManager.SpriteBatch);
			m_PrecacheWeaponSmoke[i].Enabled = false;
		}
		for (int j = 0; j < MAX_SPARKS_PRECACHE; j++)
		{
			m_PrecacheWeaponSparks[j] = new ExplosionFlyingSparksParticleSystem(g.m_App);
			m_PrecacheWeaponSparks[j].AutoInitialize(g.m_App.GraphicsDevice, g.m_App.Content, g.m_App.screenManager.SpriteBatch);
			m_PrecacheWeaponSparks[j].Enabled = false;
		}
		for (int k = 0; k < MAX_BUCKSHOT_PRECACHE; k++)
		{
			m_PrecacheWeaponBuckshot[k] = new BuckshotQuadSprayParticleSystem(g.m_App);
			m_PrecacheWeaponBuckshot[k].AutoInitialize(g.m_App.GraphicsDevice, g.m_App.Content, g.m_App.screenManager.SpriteBatch);
			m_PrecacheWeaponBuckshot[k].Enabled = false;
		}
		for (int l = 0; l < MAX_RPGTRAIL_PRECACHE; l++)
		{
			m_PrecacheRPGTrail[l] = new RPGShellTrailParticleSystem(g.m_App);
			m_PrecacheRPGTrail[l].AutoInitialize(g.m_App.GraphicsDevice, g.m_App.Content, g.m_App.screenManager.SpriteBatch);
			m_PrecacheRPGTrail[l].Enabled = false;
		}
		for (int m = 0; m < MAX_RPGEXPLOSION_PRECACHE; m++)
		{
			m_PrecacheRPGExplosion[m] = new AnimatedQuadParticleSystem(g.m_App);
			m_PrecacheRPGExplosion[m].AutoInitialize(g.m_App.GraphicsDevice, g.m_App.Content, g.m_App.screenManager.SpriteBatch);
			m_PrecacheRPGExplosion[m].Enabled = false;
		}
	}

	public ExplosionFireSmokeParticleSystem GetCachedWeaponSmoke()
	{
		m_SmokePrecacheId++;
		if (m_SmokePrecacheId >= MAX_SMOKE_PRECACHE)
		{
			return null;
		}
		return m_PrecacheWeaponSmoke[m_SmokePrecacheId];
	}

	public ExplosionFlyingSparksParticleSystem GetCachedWeaponSparks()
	{
		m_SparksPrecacheId++;
		if (m_SparksPrecacheId >= MAX_SPARKS_PRECACHE)
		{
			return null;
		}
		return m_PrecacheWeaponSparks[m_SparksPrecacheId];
	}

	public BuckshotQuadSprayParticleSystem GetCachedWeaponBuckshot()
	{
		m_BuckshotPrecacheId++;
		if (m_BuckshotPrecacheId >= MAX_BUCKSHOT_PRECACHE)
		{
			return null;
		}
		return m_PrecacheWeaponBuckshot[m_BuckshotPrecacheId];
	}

	public RPGShellTrailParticleSystem GetCachedRPGTrail()
	{
		m_RPGTrailPrecacheId++;
		if (m_RPGTrailPrecacheId >= MAX_RPGTRAIL_PRECACHE)
		{
			return null;
		}
		return m_PrecacheRPGTrail[m_RPGTrailPrecacheId];
	}

	public AnimatedQuadParticleSystem GetCachedRPGExplosion()
	{
		m_RPGExplosionPrecacheId++;
		if (m_RPGExplosionPrecacheId >= MAX_RPGEXPLOSION_PRECACHE)
		{
			return null;
		}
		return m_PrecacheRPGExplosion[m_RPGExplosionPrecacheId];
	}

	public ItemManager()
	{
		m_Item = new Item[125];
		for (int i = 0; i < 125; i++)
		{
			m_Item[i] = new Item();
		}
		m_Model = new Model[32];
	}

	public int Create(int type, byte netId, Vector3 pos, float rot, Player player)
	{
		bool flag = false;
		int num = -1;
		if (m_NextId >= 125)
		{
			m_NextId = 0;
		}
		if (type == 1 || type == 14)
		{
			int num2 = ReusePreCachedItemId();
			if (num2 != -1)
			{
				num = num2;
				flag = true;
			}
		}
		if (!flag)
		{
			for (int i = m_NextId; i < 125; i++)
			{
				if (m_Item[i].m_Id == -1)
				{
					num = i;
					flag = true;
					m_NextId = i + 1;
					break;
				}
			}
		}
		if (!flag)
		{
			for (int j = 0; j < 125; j++)
			{
				if (m_Item[j].m_Id == -1)
				{
					num = j;
					flag = true;
					m_NextId = j + 1;
					break;
				}
			}
		}
		if (num == -1)
		{
			return -1;
		}
		m_Item[num].m_Type = type;
		m_Item[num].m_Id = num;
		m_Item[num].m_NetId = netId;
		m_Item[num].m_Player = player;
		m_Item[num].m_VfxSystemIdx = 0;
		m_Item[num].m_WeaponAmmoToGive = 0;
		m_Item[num].m_ReloadState = Item.RELOAD.None;
		if (m_Item[num].m_Type != 6)
		{
			if (m_Model[type] == null)
			{
				Delete(num);
				return -1;
			}
			m_Item[num].m_Model = m_Model[type];
			m_Item[num].m_SceneObject = new SceneObject(m_Item[num].m_Model)
			{
				UpdateType = UpdateType.Automatic,
				Visibility = ObjectVisibility.None,
				StaticLightingType = StaticLightingType.Composite,
				CollisionType = CollisionType.None,
				AffectedByGravity = false,
				Name = $"Item{num}",
				World = Matrix.CreateRotationY(rot) * Matrix.CreateTranslation(pos)
			};
			g.m_App.sceneInterface.ObjectManager.Submit(m_Item[num].m_SceneObject);
		}
		switch (m_Item[num].m_Type)
		{
		case 7:
			m_Item[num].m_SceneObject.Visibility = ObjectVisibility.Rendered;
			break;
		case 10:
			m_Item[num].m_SceneObject.Visibility = ObjectVisibility.Rendered;
			break;
		case 27:
		case 28:
		case 29:
		case 30:
		case 31:
			m_Item[num].m_SceneObject.Visibility = ObjectVisibility.Rendered;
			break;
		case 1:
			m_Item[num].InitTorch();
			break;
		case 22:
			m_Item[num].InitSaw();
			break;
		case 19:
			m_Item[num].InitArm();
			break;
		case 25:
			m_Item[num].InitScrewdriver();
			break;
		case 16:
		case 17:
		case 20:
		case 26:
			m_Item[num].InitDoor();
			break;
		case 18:
			m_Item[num].InitSafe();
			break;
		case 23:
		case 24:
			m_Item[num].InitLocker();
			break;
		case 21:
			m_Item[num].InitFog();
			break;
		}
		if (flag)
		{
			return num;
		}
		return -1;
	}

	private int ReusePreCachedItemId()
	{
		for (int i = 0; i < 125; i++)
		{
			if (m_Item[i].m_Id == -1 && m_Item[i].m_WeaponSmoke != null)
			{
				return i;
			}
		}
		return -1;
	}

	public void DeleteAll()
	{
		m_NextId = 0;
		for (int i = 0; i < 125; i++)
		{
			if (m_Item[i].m_Id != -1)
			{
				m_Item[i].Delete();
			}
		}
		m_ItemNetId = 0;
	}

	public void Delete(int id)
	{
		m_Item[id].Delete();
	}

	public void Update()
	{
		UpdateFogBank();
		for (int i = 0; i < 125; i++)
		{
			if (m_Item[i].m_Id != -1)
			{
				m_Item[i].Update();
			}
		}
	}

	public Item FindObjectByType(int type)
	{
		for (int i = 0; i < 125; i++)
		{
			if (m_Item[i].m_Id != -1 && m_Item[i].m_Type == type)
			{
				return m_Item[i];
			}
		}
		return null;
	}

	public void Copy(Item[] src, Item[] dest)
	{
		for (int i = 0; i < 125; i++)
		{
			dest[i].m_Id = src[i].m_Id;
			dest[i].m_Type = src[i].m_Type;
		}
	}

	public void LoadContent(ContentManager Content)
	{
		m_Model[1] = Content.Load<Model>("Models\\torch");
		m_Model[7] = Content.Load<Model>("Models\\helmet");
		m_Model[16] = Content.Load<Model>("Models\\door1");
		m_Model[17] = Content.Load<Model>("Models\\door2");
		m_Model[20] = Content.Load<Model>("Models\\door3");
		m_Model[26] = Content.Load<Model>("Models\\door4");
		m_Model[21] = Content.Load<Model>("Models\\fog");
		m_Model[18] = Content.Load<Model>("Models\\safe");
		m_Model[22] = Content.Load<Model>("Models\\saw");
		m_Model[19] = Content.Load<Model>("Models\\arm");
		m_Model[23] = Content.Load<Model>("Models\\locker");
		m_Model[24] = Content.Load<Model>("Models\\locker2");
		m_Model[25] = Content.Load<Model>("Models\\screwdriver");
		m_Model[27] = Content.Load<Model>("Models\\tablet");
		m_Model[28] = Content.Load<Model>("Models\\tablet");
		m_Model[29] = Content.Load<Model>("Models\\tablet");
		m_Model[30] = Content.Load<Model>("Models\\tablet");
		m_Model[31] = Content.Load<Model>("Models\\tablet");
	}

	public float GetWeaponRecoil(int id)
	{
		return m_Item[id].m_Type switch
		{
			14 => 0.04f, 
			1 => 0.02f, 
			11 => 0.2f, 
			6 => 0f, 
			_ => 0f, 
		};
	}

	public int GetWeaponFireAnim(int id)
	{
		return m_Item[id].m_Type switch
		{
			1 => 1112, 
			22 => 3, 
			19 => 1112, 
			11 => 14, 
			14 => 17, 
			25 => 6, 
			6 => 1, 
			_ => 1112, 
		};
	}

	public bool GetWeaponAnimShouldLoop(int id)
	{
		int type = m_Item[id].m_Type;
		if (type == 4)
		{
			return true;
		}
		return false;
	}

	public bool GetWeaponShouldShowAmmo(int id)
	{
		switch (m_Item[id].m_Type)
		{
		case 4:
		case 5:
		case 6:
			return false;
		default:
			return true;
		}
	}

	public void ResetAmmo(int id)
	{
		switch (m_Item[id].m_Type)
		{
		case 1:
			m_Item[id].m_WeaponAmmo = 25;
			m_Item[id].m_WeaponAmmoInClip = 25;
			break;
		case 14:
			m_Item[id].m_WeaponAmmo = 35;
			m_Item[id].m_WeaponAmmoInClip = 15;
			break;
		case 11:
			m_Item[id].m_WeaponAmmo = 3;
			m_Item[id].m_WeaponAmmoInClip = 1;
			break;
		case 6:
			m_Item[id].m_WeaponAmmo = 0;
			m_Item[id].m_WeaponAmmoInClip = 0;
			break;
		}
	}

	public int FindNearbyItem(int type, Vector3 pos, Vector3 facing, float rangeSq)
	{
		float num = 1E+11f;
		int num2 = -1;
		float num3 = 1E+11f;
		for (int i = 0; i < 125; i++)
		{
			if (m_Item[i].m_Id != -1 && m_Item[i].m_Type == type)
			{
				Vector3 vector = m_Item[i].m_SceneObject.World.Translation - pos;
				vector.Y *= 2f;
				num = vector.LengthSquared();
				if (num < num3 && num < rangeSq)
				{
					num3 = num;
					num2 = i;
				}
			}
		}
		if (num2 == -1)
		{
			return -1;
		}
		Vector3 vector2 = pos - m_Item[num2].m_SceneObject.World.Translation;
		vector2.Y = 0f;
		vector2.Normalize();
		float num4 = Vector3.Dot(vector2, facing);
		if (num4 < -0.97f)
		{
			return num2;
		}
		return -1;
	}

	public void SetUpTriggeredItems()
	{
		TriggerEntity obj = null;
		string name;
		for (int i = 0; i < 9; i++)
		{
			name = $"Door{i + 1}";
			if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
			{
				Vector3 translation = obj.World.Translation;
				Vector3 forward = obj.World.Forward;
				float rot = (float)Math.Atan2(forward.X, forward.Z);
				switch (i)
				{
				case 3:
					m_DoorItemId[i] = Create(17, byte.MaxValue, translation, rot, null);
					break;
				case 4:
					m_DoorItemId[i] = Create(20, byte.MaxValue, translation, rot, null);
					break;
				case 7:
					m_DoorItemId[i] = Create(26, byte.MaxValue, translation, rot, null);
					break;
				default:
					m_DoorItemId[i] = Create(16, byte.MaxValue, translation, rot, null);
					break;
				}
			}
		}
		name = $"Safe";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
		{
			Vector3 translation2 = obj.World.Translation;
			Vector3 forward2 = obj.World.Forward;
			float rot2 = (float)Math.Atan2(forward2.X, forward2.Z);
			Create(18, byte.MaxValue, translation2, rot2, null);
			Create(22, byte.MaxValue, translation2 + new Vector3(-0.5f, 0.71f, 0f), MathHelper.ToRadians(-90f), g.m_PlayerManager.GetLocalPlayer());
		}
		name = $"Arm";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
		{
			Vector3 translation3 = obj.World.Translation;
			Vector3 forward3 = obj.World.Forward;
			float rot3 = (float)Math.Atan2(forward3.X, forward3.Z);
			Create(19, byte.MaxValue, translation3 + new Vector3(0f, 0f, 0f), rot3, g.m_PlayerManager.GetLocalPlayer());
		}
		name = $"Screwdriver";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
		{
			Vector3 translation4 = obj.World.Translation;
			Vector3 forward4 = obj.World.Forward;
			float rot4 = (float)Math.Atan2(forward4.X, forward4.Z);
			Create(25, byte.MaxValue, translation4 + new Vector3(0f, 0f, 0f), rot4, g.m_PlayerManager.GetLocalPlayer());
		}
		name = $"Locker1";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
		{
			Vector3 translation5 = obj.World.Translation;
			Vector3 forward5 = obj.World.Forward;
			float rot5 = (float)Math.Atan2(forward5.X, forward5.Z);
			m_Locker1Id = Create(23, byte.MaxValue, translation5, rot5, null);
		}
		name = $"Locker2";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
		{
			Vector3 translation6 = obj.World.Translation;
			Vector3 forward6 = obj.World.Forward;
			float rot6 = (float)Math.Atan2(forward6.X, forward6.Z);
			m_Locker2Id = Create(24, byte.MaxValue, translation6, rot6, null);
		}
		name = $"Locker3";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
		{
			Vector3 translation7 = obj.World.Translation;
			Vector3 forward7 = obj.World.Forward;
			float rot7 = (float)Math.Atan2(forward7.X, forward7.Z);
			m_Locker3Id = Create(23, byte.MaxValue, translation7, rot7, null);
		}
		name = $"Locker4";
		if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
		{
			Vector3 translation8 = obj.World.Translation;
			Vector3 forward8 = obj.World.Forward;
			float rot8 = (float)Math.Atan2(forward8.X, forward8.Z);
			m_Locker4Id = Create(23, byte.MaxValue, translation8, rot8, null);
		}
		SetupTablets();
	}

	public void SetupTablets()
	{
		Item item = g.m_ItemManager.FindObjectByType(27);
		if (item != null)
		{
			g.m_ItemManager.Delete(item.m_Id);
		}
		item = g.m_ItemManager.FindObjectByType(28);
		if (item != null)
		{
			g.m_ItemManager.Delete(item.m_Id);
		}
		item = g.m_ItemManager.FindObjectByType(29);
		if (item != null)
		{
			g.m_ItemManager.Delete(item.m_Id);
		}
		item = g.m_ItemManager.FindObjectByType(30);
		if (item != null)
		{
			g.m_ItemManager.Delete(item.m_Id);
		}
		item = g.m_ItemManager.FindObjectByType(31);
		if (item != null)
		{
			g.m_ItemManager.Delete(item.m_Id);
		}
		int[] array = new int[5];
		for (int i = 0; i < 5; i++)
		{
			array[i] = -1;
		}
		for (int j = 0; j < 5; j++)
		{
			bool flag = false;
			int num = -1;
			while (!flag)
			{
				num = g.m_App.m_Rand.Next(1, 11);
				bool flag2 = false;
				for (int k = 0; k < 5; k++)
				{
					if (array[k] == num)
					{
						flag2 = true;
					}
				}
				if (!flag2)
				{
					flag = true;
				}
			}
			array[j] = num;
		}
		TriggerEntity obj = null;
		for (int l = 0; l < 5; l++)
		{
			string name = $"Tablet{array[l]}";
			Vector3 vector = Vector3.Zero;
			if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
			{
				vector = obj.World.Translation;
				Vector3 forward = obj.World.Forward;
				float rot = (float)Math.Atan2(forward.X, forward.Z);
				Create(27 + l, byte.MaxValue, vector + new Vector3(0f, 0f, 0f), rot, g.m_PlayerManager.GetLocalPlayer());
			}
			name = $"TriggerResearch{l + 1}";
			MiscTriggerEntity obj2 = null;
			if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj2))
			{
				Matrix world = obj2.World;
				world.Translation = vector;
				obj2.World = world;
				obj2.m_Complete = false;
			}
		}
	}

	public void SetUpFog()
	{
		TriggerEntity obj = null;
		for (int i = 0; i < 18; i++)
		{
			string name = $"Fog{i + 1}";
			if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
			{
				Vector3 translation = obj.World.Translation;
				m_FogPos[i] = translation;
			}
		}
		m_FogItemId = Create(21, byte.MaxValue, Vector3.Zero, 0f, null);
	}

	public void UpdateFogBank()
	{
		if (g.m_PlayerManager.GetLocalPlayer().m_ViewSceneObject == null)
		{
			return;
		}
		int num = -1;
		float num2 = -1f;
		Vector3 zero = Vector3.Zero;
		float num3 = 0f;
		float num4 = 0f;
		if (m_FogBankId != -1)
		{
			zero = m_FogPos[m_FogBankId] - g.m_PlayerManager.GetLocalPlayer().m_Position;
			zero.Y = 0f;
			num3 = zero.LengthSquared();
			zero.Normalize();
			num4 = Vector3.Dot(zero, g.m_PlayerManager.GetLocalPlayer().m_ViewSceneObject.World.Forward);
			if (num4 < 0f)
			{
				m_Item[m_FogItemId].m_FogAlpha = 0f;
			}
		}
		for (int i = 0; i < 18; i++)
		{
			zero = m_FogPos[i] - g.m_PlayerManager.GetLocalPlayer().m_Position;
			zero.Y = 0f;
			num3 = zero.LengthSquared();
			zero.Normalize();
			num4 = Vector3.Dot(zero, g.m_PlayerManager.GetLocalPlayer().m_ViewSceneObject.World.Forward);
			if (num3 > Item.NEAR_DIST_END_SQ && num3 < Item.FAR_DIST_END_SQ && num4 > num2)
			{
				num2 = num4;
				num = i;
			}
		}
		if (num != -1 && num != m_FogBankId && m_Item[m_FogItemId].CanChangeFogBank())
		{
			m_Item[m_FogItemId].ResetFog();
			m_FogBankId = num;
			Matrix world = g.m_ItemManager.m_Item[m_FogItemId].m_SceneObject.World;
			Vector3 translation = world.Translation;
			translation = m_FogPos[num];
			world.Translation = translation;
			g.m_ItemManager.m_Item[m_FogItemId].m_SceneObject.World = world;
		}
	}

	public void SetUpCrates(int seed)
	{
		TriggerEntity obj = null;
		if (m_CrateContents == null)
		{
			m_CrateContents = new CrateContents[22];
			for (int i = 0; i < 22; i++)
			{
				m_CrateContents[i] = new CrateContents();
			}
		}
		for (int j = 0; j < 22; j++)
		{
			string name = $"Crate{j}";
			if (g.m_App.sceneInterface.ObjectManager.Find(name, onlysearchdynamicobjects: false, out obj))
			{
				Vector3 translation = obj.World.Translation;
				Vector3 forward = obj.World.Forward;
				float rot = (float)Math.Atan2(forward.X, forward.Z);
				int itemId = Create(9, byte.MaxValue, translation, rot, null);
				m_CrateContents[j].m_ItemId = itemId;
				m_CrateContents[j].m_CrateId = j;
				m_CrateContents[j].m_Contents = CrateContents.CRATECONTENTS.NONE;
				m_CrateContents[j].m_bUsed = false;
			}
		}
		Random random = new Random(seed);
		if (g.m_App.m_SurvivalMode)
		{
			PopulateCratesForSurvival(random);
			return;
		}
		m_ArtifactCrateId = random.Next(0, 22);
		m_CrateContents[m_ArtifactCrateId].m_Contents = CrateContents.CRATECONTENTS.ARTIFACT;
		int num = 2;
		while (num > 0)
		{
			int num2 = random.Next(0, 22);
			if (m_CrateContents[num2].m_Contents == CrateContents.CRATECONTENTS.NONE)
			{
				m_CrateContents[num2].m_Contents = CrateContents.CRATECONTENTS.ALIEN;
				num--;
			}
		}
		num = 2;
		while (num > 0)
		{
			int num3 = random.Next(0, 22);
			if (m_CrateContents[num3].m_Contents == CrateContents.CRATECONTENTS.NONE)
			{
				m_CrateContents[num3].m_Contents = CrateContents.CRATECONTENTS.LOCATOR;
				num--;
			}
		}
		num = 5;
		while (num > 0)
		{
			int num4 = random.Next(0, 22);
			if (m_CrateContents[num4].m_Contents == CrateContents.CRATECONTENTS.NONE)
			{
				m_CrateContents[num4].m_Contents = CrateContents.CRATECONTENTS.RPG;
				num--;
			}
		}
		num = 6;
		while (num > 0)
		{
			int num5 = random.Next(0, 22);
			if (m_CrateContents[num5].m_Contents == CrateContents.CRATECONTENTS.NONE)
			{
				m_CrateContents[num5].m_Contents = CrateContents.CRATECONTENTS.HEALTH;
				num--;
			}
		}
		int num6 = 0;
		for (int k = 0; k < 22; k++)
		{
			if (m_CrateContents[k].m_Contents == CrateContents.CRATECONTENTS.NONE)
			{
				m_CrateContents[k].m_Contents = CrateContents.CRATECONTENTS.AMMO;
				num6++;
			}
		}
	}

	private void PopulateCratesForSurvival(Random NeworkRand)
	{
		int num = 10;
		while (num > 0)
		{
			int num2 = NeworkRand.Next(0, 22);
			if (m_CrateContents[num2].m_Contents == CrateContents.CRATECONTENTS.NONE)
			{
				m_CrateContents[num2].m_Contents = CrateContents.CRATECONTENTS.RPG;
				num--;
			}
		}
		num = 6;
		while (num > 0)
		{
			int num3 = NeworkRand.Next(0, 22);
			if (m_CrateContents[num3].m_Contents == CrateContents.CRATECONTENTS.NONE)
			{
				m_CrateContents[num3].m_Contents = CrateContents.CRATECONTENTS.HEALTH;
				num--;
			}
		}
		int num4 = 0;
		for (int i = 0; i < 22; i++)
		{
			if (m_CrateContents[i].m_Contents == CrateContents.CRATECONTENTS.NONE)
			{
				m_CrateContents[i].m_Contents = CrateContents.CRATECONTENTS.AMMO;
				num4++;
			}
		}
	}

	public int GetCrateIndexByItemIndex(int itemIdx)
	{
		for (int i = 0; i < 22; i++)
		{
			if (m_CrateContents[i].m_ItemId == itemIdx)
			{
				return m_CrateContents[i].m_CrateId;
			}
		}
		return -1;
	}

	public int GetItemIndexByCrateIndex(short crateIdx)
	{
		for (int i = 0; i < 22; i++)
		{
			if (m_CrateContents[i].m_CrateId == crateIdx)
			{
				return m_CrateContents[i].m_ItemId;
			}
		}
		return -1;
	}

	public CrateContents.CRATECONTENTS GetCrateContents(int crateIndex)
	{
		return m_CrateContents[crateIndex].m_Contents;
	}

	public void EmptyCrate(int crateIndex)
	{
		m_CrateContents[crateIndex].m_Contents = CrateContents.CRATECONTENTS.NONE;
	}

	public byte[] GetCrateStateBuffer()
	{
		byte[] array = new byte[22];
		for (int i = 0; i < 22; i++)
		{
			array[i] = (byte)m_CrateContents[i].m_Contents;
		}
		return array;
	}

	public void SetCrateStatesFromBuffer(byte[] buffer)
	{
		for (int i = 0; i < 22; i++)
		{
			m_CrateContents[i].m_Contents = (CrateContents.CRATECONTENTS)buffer[i];
		}
	}

	public void ReturnArtifactToCrate()
	{
		m_CrateContents[m_ArtifactCrateId].m_Contents = CrateContents.CRATECONTENTS.ARTIFACT;
	}

	public void ShowArtifactReturnedMessage()
	{
		g.m_App.AddHelmetMessage(HelmetMessage.TYPE.ARTIFACT_IN_CONTAINER, "ARTIFACT RETURNED", new Vector2(120f, 190f), 5f, g.m_App.HUD_GREEN, SoundManager.SFX.WarningBeep, null, hideSysMsg: true);
		if (g.m_App.m_AlarmSFX != null)
		{
			g.m_App.m_AlarmSFX.Stop();
			g.m_App.m_AlarmSFX = null;
		}
		if (g.m_PlayerManager.GetLocalPlayer() != null)
		{
			g.m_PlayerManager.GetLocalPlayer().ClearUsedCrate(m_ArtifactCrateId);
		}
	}

	public byte GetArtifactNetId()
	{
		for (int i = 0; i < 125; i++)
		{
			if (m_Item[i].m_Type == 10 && m_Item[i].m_Id != -1 && m_Item[i].m_NetId != byte.MaxValue)
			{
				return m_Item[i].m_NetId;
			}
		}
		return byte.MaxValue;
	}

	public int GetArtifactItemId()
	{
		for (int i = 0; i < 125; i++)
		{
			if (m_Item[i].m_Type == 10 && m_Item[i].m_Id != -1 && m_Item[i].m_NetId != byte.MaxValue)
			{
				return m_Item[i].m_Id;
			}
		}
		return -1;
	}

	public Vector3 GetArtifactCratePos()
	{
		int itemIndexByCrateIndex = GetItemIndexByCrateIndex((short)m_ArtifactCrateId);
		return m_Item[itemIndexByCrateIndex].m_SceneObject.World.Translation;
	}

	public byte GetNextItemNetId()
	{
		m_ItemNetId++;
		if (m_ItemNetId > 254)
		{
			m_ItemNetId = 0;
		}
		return m_ItemNetId;
	}

	public int GetItemIdByNetItemId(byte netItemId)
	{
		for (int i = 0; i < 125; i++)
		{
			if (m_Item[i].m_NetId == netItemId)
			{
				return i;
			}
		}
		return -1;
	}
}
