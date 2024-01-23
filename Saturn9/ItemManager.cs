using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SynapseGaming.LightingSystem.Collision;
using SynapseGaming.LightingSystem.Core;
using SynapseGaming.LightingSystem.Lights;
using SynapseGaming.LightingSystem.Rendering;
using System;

namespace Saturn9
{
  public class ItemManager
  {
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
    private ItemManager.CrateContents[] m_CrateContents;
    private int m_ArtifactCrateId;

    public void PrecacheVFX()
    {
      this.m_PrecacheWeaponSmoke = new ExplosionFireSmokeParticleSystem[this.MAX_SMOKE_PRECACHE];
      this.m_PrecacheWeaponSparks = new ExplosionFlyingSparksParticleSystem[this.MAX_SPARKS_PRECACHE];
      this.m_PrecacheWeaponBuckshot = new BuckshotQuadSprayParticleSystem[this.MAX_BUCKSHOT_PRECACHE];
      this.m_PrecacheRPGTrail = new RPGShellTrailParticleSystem[this.MAX_RPGTRAIL_PRECACHE];
      this.m_PrecacheRPGExplosion = new AnimatedQuadParticleSystem[this.MAX_RPGEXPLOSION_PRECACHE];
      for (int index = 0; index < this.MAX_SMOKE_PRECACHE; ++index)
      {
        this.m_PrecacheWeaponSmoke[index] = new ExplosionFireSmokeParticleSystem((Game) g.m_App);
        this.m_PrecacheWeaponSmoke[index].AutoInitialize(g.m_App.GraphicsDevice, g.m_App.Content, g.m_App.screenManager.SpriteBatch);
        this.m_PrecacheWeaponSmoke[index].Enabled = false;
      }
      for (int index = 0; index < this.MAX_SPARKS_PRECACHE; ++index)
      {
        this.m_PrecacheWeaponSparks[index] = new ExplosionFlyingSparksParticleSystem((Game) g.m_App);
        this.m_PrecacheWeaponSparks[index].AutoInitialize(g.m_App.GraphicsDevice, g.m_App.Content, g.m_App.screenManager.SpriteBatch);
        this.m_PrecacheWeaponSparks[index].Enabled = false;
      }
      for (int index = 0; index < this.MAX_BUCKSHOT_PRECACHE; ++index)
      {
        this.m_PrecacheWeaponBuckshot[index] = new BuckshotQuadSprayParticleSystem((Game) g.m_App);
        this.m_PrecacheWeaponBuckshot[index].AutoInitialize(g.m_App.GraphicsDevice, g.m_App.Content, g.m_App.screenManager.SpriteBatch);
        this.m_PrecacheWeaponBuckshot[index].Enabled = false;
      }
      for (int index = 0; index < this.MAX_RPGTRAIL_PRECACHE; ++index)
      {
        this.m_PrecacheRPGTrail[index] = new RPGShellTrailParticleSystem((Game) g.m_App);
        this.m_PrecacheRPGTrail[index].AutoInitialize(g.m_App.GraphicsDevice, g.m_App.Content, g.m_App.screenManager.SpriteBatch);
        this.m_PrecacheRPGTrail[index].Enabled = false;
      }
      for (int index = 0; index < this.MAX_RPGEXPLOSION_PRECACHE; ++index)
      {
        this.m_PrecacheRPGExplosion[index] = new AnimatedQuadParticleSystem((Game) g.m_App);
        this.m_PrecacheRPGExplosion[index].AutoInitialize(g.m_App.GraphicsDevice, g.m_App.Content, g.m_App.screenManager.SpriteBatch);
        this.m_PrecacheRPGExplosion[index].Enabled = false;
      }
    }

    public ExplosionFireSmokeParticleSystem GetCachedWeaponSmoke()
    {
      ++this.m_SmokePrecacheId;
      return this.m_SmokePrecacheId >= this.MAX_SMOKE_PRECACHE ? (ExplosionFireSmokeParticleSystem) null : this.m_PrecacheWeaponSmoke[this.m_SmokePrecacheId];
    }

    public ExplosionFlyingSparksParticleSystem GetCachedWeaponSparks()
    {
      ++this.m_SparksPrecacheId;
      return this.m_SparksPrecacheId >= this.MAX_SPARKS_PRECACHE ? (ExplosionFlyingSparksParticleSystem) null : this.m_PrecacheWeaponSparks[this.m_SparksPrecacheId];
    }

    public BuckshotQuadSprayParticleSystem GetCachedWeaponBuckshot()
    {
      ++this.m_BuckshotPrecacheId;
      return this.m_BuckshotPrecacheId >= this.MAX_BUCKSHOT_PRECACHE ? (BuckshotQuadSprayParticleSystem) null : this.m_PrecacheWeaponBuckshot[this.m_BuckshotPrecacheId];
    }

    public RPGShellTrailParticleSystem GetCachedRPGTrail()
    {
      ++this.m_RPGTrailPrecacheId;
      return this.m_RPGTrailPrecacheId >= this.MAX_RPGTRAIL_PRECACHE ? (RPGShellTrailParticleSystem) null : this.m_PrecacheRPGTrail[this.m_RPGTrailPrecacheId];
    }

    public AnimatedQuadParticleSystem GetCachedRPGExplosion()
    {
      ++this.m_RPGExplosionPrecacheId;
      return this.m_RPGExplosionPrecacheId >= this.MAX_RPGEXPLOSION_PRECACHE ? (AnimatedQuadParticleSystem) null : this.m_PrecacheRPGExplosion[this.m_RPGExplosionPrecacheId];
    }

    public ItemManager()
    {
      this.m_Item = new Item[125];
      for (int index = 0; index < 125; ++index)
        this.m_Item[index] = new Item();
      this.m_Model = new Model[32];
    }

    public int Create(int type, byte netId, Vector3 pos, float rot, Player player)
    {
      bool flag = false;
      int id = -1;
      if (this.m_NextId >= 125)
        this.m_NextId = 0;
      if (type == 1 || type == 14)
      {
        int num = this.ReusePreCachedItemId();
        if (num != -1)
        {
          id = num;
          flag = true;
        }
      }
      if (!flag)
      {
        for (int nextId = this.m_NextId; nextId < 125; ++nextId)
        {
          if (this.m_Item[nextId].m_Id == -1)
          {
            id = nextId;
            flag = true;
            this.m_NextId = nextId + 1;
            break;
          }
        }
      }
      if (!flag)
      {
        for (int index = 0; index < 125; ++index)
        {
          if (this.m_Item[index].m_Id == -1)
          {
            id = index;
            flag = true;
            this.m_NextId = index + 1;
            break;
          }
        }
      }
      if (id == -1)
        return -1;
      this.m_Item[id].m_Type = type;
      this.m_Item[id].m_Id = id;
      this.m_Item[id].m_NetId = netId;
      this.m_Item[id].m_Player = player;
      this.m_Item[id].m_VfxSystemIdx = 0;
      this.m_Item[id].m_WeaponAmmoToGive = 0;
      this.m_Item[id].m_ReloadState = Item.RELOAD.None;
      if (this.m_Item[id].m_Type != 6)
      {
        if (this.m_Model[type] == null)
        {
          this.Delete(id);
          return -1;
        }
        this.m_Item[id].m_Model = this.m_Model[type];
        Item obj = this.m_Item[id];
        SceneObject sceneObject1 = new SceneObject(this.m_Item[id].m_Model);
        sceneObject1.UpdateType = UpdateType.Automatic;
        sceneObject1.Visibility = ObjectVisibility.None;
        sceneObject1.StaticLightingType = StaticLightingType.Composite;
        sceneObject1.CollisionType = CollisionType.None;
        sceneObject1.AffectedByGravity = false;
        sceneObject1.Name = string.Format("Item{0}", (object) id);
        sceneObject1.World = Matrix.op_Multiply(Matrix.CreateRotationY(rot), Matrix.CreateTranslation(pos));
        SceneObject sceneObject2 = sceneObject1;
        obj.m_SceneObject = sceneObject2;
        ((ISubmit<SceneEntity>) g.m_App.sceneInterface.ObjectManager).Submit((SceneEntity) this.m_Item[id].m_SceneObject);
      }
      switch (this.m_Item[id].m_Type)
      {
        case 1:
          this.m_Item[id].InitTorch();
          break;
        case 7:
          this.m_Item[id].m_SceneObject.Visibility = ObjectVisibility.Rendered;
          break;
        case 10:
          this.m_Item[id].m_SceneObject.Visibility = ObjectVisibility.Rendered;
          break;
        case 16:
        case 17:
        case 20:
        case 26:
          this.m_Item[id].InitDoor();
          break;
        case 18:
          this.m_Item[id].InitSafe();
          break;
        case 19:
          this.m_Item[id].InitArm();
          break;
        case 21:
          this.m_Item[id].InitFog();
          break;
        case 22:
          this.m_Item[id].InitSaw();
          break;
        case 23:
        case 24:
          this.m_Item[id].InitLocker();
          break;
        case 25:
          this.m_Item[id].InitScrewdriver();
          break;
        case 27:
        case 28:
        case 29:
        case 30:
        case 31:
          this.m_Item[id].m_SceneObject.Visibility = ObjectVisibility.Rendered;
          break;
      }
      return flag ? id : -1;
    }

    private int ReusePreCachedItemId()
    {
      for (int index = 0; index < 125; ++index)
      {
        if (this.m_Item[index].m_Id == -1 && this.m_Item[index].m_WeaponSmoke != null)
          return index;
      }
      return -1;
    }

    public void DeleteAll()
    {
      this.m_NextId = 0;
      for (int index = 0; index < 125; ++index)
      {
        if (this.m_Item[index].m_Id != -1)
          this.m_Item[index].Delete();
      }
      this.m_ItemNetId = (byte) 0;
    }

    public void Delete(int id) => this.m_Item[id].Delete();

    public void Update()
    {
      this.UpdateFogBank();
      for (int index = 0; index < 125; ++index)
      {
        if (this.m_Item[index].m_Id != -1)
          this.m_Item[index].Update();
      }
    }

    public Item FindObjectByType(int type)
    {
      for (int index = 0; index < 125; ++index)
      {
        if (this.m_Item[index].m_Id != -1 && this.m_Item[index].m_Type == type)
          return this.m_Item[index];
      }
      return (Item) null;
    }

    public void Copy(Item[] src, Item[] dest)
    {
      for (int index = 0; index < 125; ++index)
      {
        dest[index].m_Id = src[index].m_Id;
        dest[index].m_Type = src[index].m_Type;
      }
    }

    public void LoadContent(ContentManager Content)
    {
      this.m_Model[1] = Content.Load<Model>("Models\\torch");
      this.m_Model[7] = Content.Load<Model>("Models\\helmet");
      this.m_Model[16] = Content.Load<Model>("Models\\door1");
      this.m_Model[17] = Content.Load<Model>("Models\\door2");
      this.m_Model[20] = Content.Load<Model>("Models\\door3");
      this.m_Model[26] = Content.Load<Model>("Models\\door4");
      this.m_Model[21] = Content.Load<Model>("Models\\fog");
      this.m_Model[18] = Content.Load<Model>("Models\\safe");
      this.m_Model[22] = Content.Load<Model>("Models\\saw");
      this.m_Model[19] = Content.Load<Model>("Models\\arm");
      this.m_Model[23] = Content.Load<Model>("Models\\locker");
      this.m_Model[24] = Content.Load<Model>("Models\\locker2");
      this.m_Model[25] = Content.Load<Model>("Models\\screwdriver");
      this.m_Model[27] = Content.Load<Model>("Models\\tablet");
      this.m_Model[28] = Content.Load<Model>("Models\\tablet");
      this.m_Model[29] = Content.Load<Model>("Models\\tablet");
      this.m_Model[30] = Content.Load<Model>("Models\\tablet");
      this.m_Model[31] = Content.Load<Model>("Models\\tablet");
    }

    public float GetWeaponRecoil(int id)
    {
      switch (this.m_Item[id].m_Type)
      {
        case 1:
          return 0.02f;
        case 6:
          return 0.0f;
        case 11:
          return 0.2f;
        case 14:
          return 0.04f;
        default:
          return 0.0f;
      }
    }

    public int GetWeaponFireAnim(int id)
    {
      switch (this.m_Item[id].m_Type)
      {
        case 1:
          return 1112;
        case 6:
          return 1;
        case 11:
          return 14;
        case 14:
          return 17;
        case 19:
          return 1112;
        case 22:
          return 3;
        case 25:
          return 6;
        default:
          return 1112;
      }
    }

    public bool GetWeaponAnimShouldLoop(int id) => this.m_Item[id].m_Type == 4;

    public bool GetWeaponShouldShowAmmo(int id)
    {
      switch (this.m_Item[id].m_Type)
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
      switch (this.m_Item[id].m_Type)
      {
        case 1:
          this.m_Item[id].m_WeaponAmmo = 25;
          this.m_Item[id].m_WeaponAmmoInClip = 25;
          break;
        case 6:
          this.m_Item[id].m_WeaponAmmo = 0;
          this.m_Item[id].m_WeaponAmmoInClip = 0;
          break;
        case 11:
          this.m_Item[id].m_WeaponAmmo = 3;
          this.m_Item[id].m_WeaponAmmoInClip = 1;
          break;
        case 14:
          this.m_Item[id].m_WeaponAmmo = 35;
          this.m_Item[id].m_WeaponAmmoInClip = 15;
          break;
      }
    }

    public int FindNearbyItem(int type, Vector3 pos, Vector3 facing, float rangeSq)
    {
      int index1 = -1;
      float num1 = 1E+11f;
      for (int index2 = 0; index2 < 125; ++index2)
      {
        if (this.m_Item[index2].m_Id != -1 && this.m_Item[index2].m_Type == type)
        {
          Matrix world = this.m_Item[index2].m_SceneObject.World;
          Vector3 vector3 = Vector3.op_Subtraction(((Matrix) ref world).Translation, pos);
          vector3.Y *= 2f;
          float num2 = ((Vector3) ref vector3).LengthSquared();
          if ((double) num2 < (double) num1 && (double) num2 < (double) rangeSq)
          {
            num1 = num2;
            index1 = index2;
          }
        }
      }
      if (index1 == -1)
        return -1;
      Vector3 vector3_1 = pos;
      Matrix world1 = this.m_Item[index1].m_SceneObject.World;
      Vector3 translation = ((Matrix) ref world1).Translation;
      Vector3 vector3_2 = Vector3.op_Subtraction(vector3_1, translation);
      vector3_2.Y = 0.0f;
      ((Vector3) ref vector3_2).Normalize();
      return (double) Vector3.Dot(vector3_2, facing) < -0.97000002861022949 ? index1 : -1;
    }

    public void SetUpTriggeredItems()
    {
      TriggerEntity triggerEntity = (TriggerEntity) null;
      for (int index = 0; index < 9; ++index)
      {
        string name = string.Format("Door{0}", (object) (index + 1));
        if (g.m_App.sceneInterface.ObjectManager.Find<TriggerEntity>(name, false, out triggerEntity))
        {
          Matrix world1 = triggerEntity.World;
          Vector3 translation = ((Matrix) ref world1).Translation;
          Matrix world2 = triggerEntity.World;
          Vector3 forward = ((Matrix) ref world2).Forward;
          float rot = (float) Math.Atan2((double) forward.X, (double) forward.Z);
          switch (index)
          {
            case 3:
              this.m_DoorItemId[index] = this.Create(17, byte.MaxValue, translation, rot, (Player) null);
              continue;
            case 4:
              this.m_DoorItemId[index] = this.Create(20, byte.MaxValue, translation, rot, (Player) null);
              continue;
            case 7:
              this.m_DoorItemId[index] = this.Create(26, byte.MaxValue, translation, rot, (Player) null);
              continue;
            default:
              this.m_DoorItemId[index] = this.Create(16, byte.MaxValue, translation, rot, (Player) null);
              continue;
          }
        }
      }
      string name1 = string.Format("Safe");
      if (g.m_App.sceneInterface.ObjectManager.Find<TriggerEntity>(name1, false, out triggerEntity))
      {
        Matrix world3 = triggerEntity.World;
        Vector3 translation = ((Matrix) ref world3).Translation;
        Matrix world4 = triggerEntity.World;
        Vector3 forward = ((Matrix) ref world4).Forward;
        float rot = (float) Math.Atan2((double) forward.X, (double) forward.Z);
        this.Create(18, byte.MaxValue, translation, rot, (Player) null);
        this.Create(22, byte.MaxValue, Vector3.op_Addition(translation, new Vector3(-0.5f, 0.71f, 0.0f)), MathHelper.ToRadians(-90f), g.m_PlayerManager.GetLocalPlayer());
      }
      string name2 = string.Format("Arm");
      if (g.m_App.sceneInterface.ObjectManager.Find<TriggerEntity>(name2, false, out triggerEntity))
      {
        Matrix world5 = triggerEntity.World;
        Vector3 translation = ((Matrix) ref world5).Translation;
        Matrix world6 = triggerEntity.World;
        Vector3 forward = ((Matrix) ref world6).Forward;
        float rot = (float) Math.Atan2((double) forward.X, (double) forward.Z);
        this.Create(19, byte.MaxValue, Vector3.op_Addition(translation, new Vector3(0.0f, 0.0f, 0.0f)), rot, g.m_PlayerManager.GetLocalPlayer());
      }
      string name3 = string.Format("Screwdriver");
      if (g.m_App.sceneInterface.ObjectManager.Find<TriggerEntity>(name3, false, out triggerEntity))
      {
        Matrix world7 = triggerEntity.World;
        Vector3 translation = ((Matrix) ref world7).Translation;
        Matrix world8 = triggerEntity.World;
        Vector3 forward = ((Matrix) ref world8).Forward;
        float rot = (float) Math.Atan2((double) forward.X, (double) forward.Z);
        this.Create(25, byte.MaxValue, Vector3.op_Addition(translation, new Vector3(0.0f, 0.0f, 0.0f)), rot, g.m_PlayerManager.GetLocalPlayer());
      }
      string name4 = string.Format("Locker1");
      if (g.m_App.sceneInterface.ObjectManager.Find<TriggerEntity>(name4, false, out triggerEntity))
      {
        Matrix world9 = triggerEntity.World;
        Vector3 translation = ((Matrix) ref world9).Translation;
        Matrix world10 = triggerEntity.World;
        Vector3 forward = ((Matrix) ref world10).Forward;
        float rot = (float) Math.Atan2((double) forward.X, (double) forward.Z);
        this.m_Locker1Id = this.Create(23, byte.MaxValue, translation, rot, (Player) null);
      }
      string name5 = string.Format("Locker2");
      if (g.m_App.sceneInterface.ObjectManager.Find<TriggerEntity>(name5, false, out triggerEntity))
      {
        Matrix world11 = triggerEntity.World;
        Vector3 translation = ((Matrix) ref world11).Translation;
        Matrix world12 = triggerEntity.World;
        Vector3 forward = ((Matrix) ref world12).Forward;
        float rot = (float) Math.Atan2((double) forward.X, (double) forward.Z);
        this.m_Locker2Id = this.Create(24, byte.MaxValue, translation, rot, (Player) null);
      }
      string name6 = string.Format("Locker3");
      if (g.m_App.sceneInterface.ObjectManager.Find<TriggerEntity>(name6, false, out triggerEntity))
      {
        Matrix world13 = triggerEntity.World;
        Vector3 translation = ((Matrix) ref world13).Translation;
        Matrix world14 = triggerEntity.World;
        Vector3 forward = ((Matrix) ref world14).Forward;
        float rot = (float) Math.Atan2((double) forward.X, (double) forward.Z);
        this.m_Locker3Id = this.Create(23, byte.MaxValue, translation, rot, (Player) null);
      }
      string name7 = string.Format("Locker4");
      if (g.m_App.sceneInterface.ObjectManager.Find<TriggerEntity>(name7, false, out triggerEntity))
      {
        Matrix world15 = triggerEntity.World;
        Vector3 translation = ((Matrix) ref world15).Translation;
        Matrix world16 = triggerEntity.World;
        Vector3 forward = ((Matrix) ref world16).Forward;
        float rot = (float) Math.Atan2((double) forward.X, (double) forward.Z);
        this.m_Locker4Id = this.Create(23, byte.MaxValue, translation, rot, (Player) null);
      }
      this.SetupTablets();
    }

    public void SetupTablets()
    {
      Item objectByType1 = g.m_ItemManager.FindObjectByType(27);
      if (objectByType1 != null)
        g.m_ItemManager.Delete(objectByType1.m_Id);
      Item objectByType2 = g.m_ItemManager.FindObjectByType(28);
      if (objectByType2 != null)
        g.m_ItemManager.Delete(objectByType2.m_Id);
      Item objectByType3 = g.m_ItemManager.FindObjectByType(29);
      if (objectByType3 != null)
        g.m_ItemManager.Delete(objectByType3.m_Id);
      Item objectByType4 = g.m_ItemManager.FindObjectByType(30);
      if (objectByType4 != null)
        g.m_ItemManager.Delete(objectByType4.m_Id);
      Item objectByType5 = g.m_ItemManager.FindObjectByType(31);
      if (objectByType5 != null)
        g.m_ItemManager.Delete(objectByType5.m_Id);
      int[] numArray = new int[5];
      for (int index = 0; index < 5; ++index)
        numArray[index] = -1;
      for (int index1 = 0; index1 < 5; ++index1)
      {
        bool flag1 = false;
        int num = -1;
        while (!flag1)
        {
          num = g.m_App.m_Rand.Next(1, 11);
          bool flag2 = false;
          for (int index2 = 0; index2 < 5; ++index2)
          {
            if (numArray[index2] == num)
              flag2 = true;
          }
          if (!flag2)
            flag1 = true;
        }
        numArray[index1] = num;
      }
      TriggerEntity triggerEntity = (TriggerEntity) null;
      for (int index = 0; index < 5; ++index)
      {
        string name1 = string.Format("Tablet{0}", (object) numArray[index]);
        Vector3 vector3 = Vector3.Zero;
        if (g.m_App.sceneInterface.ObjectManager.Find<TriggerEntity>(name1, false, out triggerEntity))
        {
          Matrix world1 = triggerEntity.World;
          vector3 = ((Matrix) ref world1).Translation;
          Matrix world2 = triggerEntity.World;
          Vector3 forward = ((Matrix) ref world2).Forward;
          float rot = (float) Math.Atan2((double) forward.X, (double) forward.Z);
          this.Create(27 + index, byte.MaxValue, Vector3.op_Addition(vector3, new Vector3(0.0f, 0.0f, 0.0f)), rot, g.m_PlayerManager.GetLocalPlayer());
        }
        string name2 = string.Format("TriggerResearch{0}", (object) (index + 1));
        MiscTriggerEntity miscTriggerEntity = (MiscTriggerEntity) null;
        if (g.m_App.sceneInterface.ObjectManager.Find<MiscTriggerEntity>(name2, false, out miscTriggerEntity))
        {
          Matrix world = miscTriggerEntity.World;
          ((Matrix) ref world).Translation = vector3;
          miscTriggerEntity.World = world;
          miscTriggerEntity.m_Complete = false;
        }
      }
    }

    public void SetUpFog()
    {
      TriggerEntity triggerEntity = (TriggerEntity) null;
      for (int index = 0; index < 18; ++index)
      {
        string name = string.Format("Fog{0}", (object) (index + 1));
        if (g.m_App.sceneInterface.ObjectManager.Find<TriggerEntity>(name, false, out triggerEntity))
        {
          Matrix world = triggerEntity.World;
          Vector3 translation = ((Matrix) ref world).Translation;
          this.m_FogPos[index] = translation;
        }
      }
      this.m_FogItemId = this.Create(21, byte.MaxValue, Vector3.Zero, 0.0f, (Player) null);
    }

    public void UpdateFogBank()
    {
      if (g.m_PlayerManager.GetLocalPlayer().m_ViewSceneObject == null)
        return;
      int index1 = -1;
      float num1 = -1f;
      Vector3 zero = Vector3.Zero;
      float num2 = 0.0f;
      Vector3 vector3_1;
      if (this.m_FogBankId != -1)
      {
        vector3_1 = Vector3.op_Subtraction(this.m_FogPos[this.m_FogBankId], g.m_PlayerManager.GetLocalPlayer().m_Position);
        vector3_1.Y = 0.0f;
        num2 = ((Vector3) ref vector3_1).LengthSquared();
        ((Vector3) ref vector3_1).Normalize();
        Vector3 vector3_2 = vector3_1;
        Matrix world = g.m_PlayerManager.GetLocalPlayer().m_ViewSceneObject.World;
        Vector3 forward = ((Matrix) ref world).Forward;
        if ((double) Vector3.Dot(vector3_2, forward) < 0.0)
          this.m_Item[this.m_FogItemId].m_FogAlpha = 0.0f;
      }
      for (int index2 = 0; index2 < 18; ++index2)
      {
        vector3_1 = Vector3.op_Subtraction(this.m_FogPos[index2], g.m_PlayerManager.GetLocalPlayer().m_Position);
        vector3_1.Y = 0.0f;
        float num3 = ((Vector3) ref vector3_1).LengthSquared();
        ((Vector3) ref vector3_1).Normalize();
        Vector3 vector3_3 = vector3_1;
        Matrix world = g.m_PlayerManager.GetLocalPlayer().m_ViewSceneObject.World;
        Vector3 forward = ((Matrix) ref world).Forward;
        float num4 = Vector3.Dot(vector3_3, forward);
        if ((double) num3 > (double) Item.NEAR_DIST_END_SQ && (double) num3 < (double) Item.FAR_DIST_END_SQ && (double) num4 > (double) num1)
        {
          num1 = num4;
          index1 = index2;
        }
      }
      if (index1 == -1 || index1 == this.m_FogBankId || !this.m_Item[this.m_FogItemId].CanChangeFogBank())
        return;
      this.m_Item[this.m_FogItemId].ResetFog();
      this.m_FogBankId = index1;
      Matrix world1 = g.m_ItemManager.m_Item[this.m_FogItemId].m_SceneObject.World;
      Vector3 translation = ((Matrix) ref world1).Translation;
      Vector3 fogPo = this.m_FogPos[index1];
      ((Matrix) ref world1).Translation = fogPo;
      g.m_ItemManager.m_Item[this.m_FogItemId].m_SceneObject.World = world1;
    }

    public void SetUpCrates(int seed)
    {
      TriggerEntity triggerEntity = (TriggerEntity) null;
      if (this.m_CrateContents == null)
      {
        this.m_CrateContents = new ItemManager.CrateContents[22];
        for (int index = 0; index < 22; ++index)
          this.m_CrateContents[index] = new ItemManager.CrateContents();
      }
      for (int index = 0; index < 22; ++index)
      {
        string name = string.Format("Crate{0}", (object) index);
        if (g.m_App.sceneInterface.ObjectManager.Find<TriggerEntity>(name, false, out triggerEntity))
        {
          Matrix world1 = triggerEntity.World;
          Vector3 translation = ((Matrix) ref world1).Translation;
          Matrix world2 = triggerEntity.World;
          Vector3 forward = ((Matrix) ref world2).Forward;
          float rot = (float) Math.Atan2((double) forward.X, (double) forward.Z);
          int num = this.Create(9, byte.MaxValue, translation, rot, (Player) null);
          this.m_CrateContents[index].m_ItemId = num;
          this.m_CrateContents[index].m_CrateId = index;
          this.m_CrateContents[index].m_Contents = ItemManager.CrateContents.CRATECONTENTS.NONE;
          this.m_CrateContents[index].m_bUsed = false;
        }
      }
      Random NeworkRand = new Random(seed);
      if (g.m_App.m_SurvivalMode)
      {
        this.PopulateCratesForSurvival(NeworkRand);
      }
      else
      {
        this.m_ArtifactCrateId = NeworkRand.Next(0, 22);
        this.m_CrateContents[this.m_ArtifactCrateId].m_Contents = ItemManager.CrateContents.CRATECONTENTS.ARTIFACT;
        int num1 = 2;
        while (num1 > 0)
        {
          int index = NeworkRand.Next(0, 22);
          if (this.m_CrateContents[index].m_Contents == ItemManager.CrateContents.CRATECONTENTS.NONE)
          {
            this.m_CrateContents[index].m_Contents = ItemManager.CrateContents.CRATECONTENTS.ALIEN;
            --num1;
          }
        }
        int num2 = 2;
        while (num2 > 0)
        {
          int index = NeworkRand.Next(0, 22);
          if (this.m_CrateContents[index].m_Contents == ItemManager.CrateContents.CRATECONTENTS.NONE)
          {
            this.m_CrateContents[index].m_Contents = ItemManager.CrateContents.CRATECONTENTS.LOCATOR;
            --num2;
          }
        }
        int num3 = 5;
        while (num3 > 0)
        {
          int index = NeworkRand.Next(0, 22);
          if (this.m_CrateContents[index].m_Contents == ItemManager.CrateContents.CRATECONTENTS.NONE)
          {
            this.m_CrateContents[index].m_Contents = ItemManager.CrateContents.CRATECONTENTS.RPG;
            --num3;
          }
        }
        int num4 = 6;
        while (num4 > 0)
        {
          int index = NeworkRand.Next(0, 22);
          if (this.m_CrateContents[index].m_Contents == ItemManager.CrateContents.CRATECONTENTS.NONE)
          {
            this.m_CrateContents[index].m_Contents = ItemManager.CrateContents.CRATECONTENTS.HEALTH;
            --num4;
          }
        }
        int num5 = 0;
        for (int index = 0; index < 22; ++index)
        {
          if (this.m_CrateContents[index].m_Contents == ItemManager.CrateContents.CRATECONTENTS.NONE)
          {
            this.m_CrateContents[index].m_Contents = ItemManager.CrateContents.CRATECONTENTS.AMMO;
            ++num5;
          }
        }
      }
    }

    private void PopulateCratesForSurvival(Random NeworkRand)
    {
      int num1 = 10;
      while (num1 > 0)
      {
        int index = NeworkRand.Next(0, 22);
        if (this.m_CrateContents[index].m_Contents == ItemManager.CrateContents.CRATECONTENTS.NONE)
        {
          this.m_CrateContents[index].m_Contents = ItemManager.CrateContents.CRATECONTENTS.RPG;
          --num1;
        }
      }
      int num2 = 6;
      while (num2 > 0)
      {
        int index = NeworkRand.Next(0, 22);
        if (this.m_CrateContents[index].m_Contents == ItemManager.CrateContents.CRATECONTENTS.NONE)
        {
          this.m_CrateContents[index].m_Contents = ItemManager.CrateContents.CRATECONTENTS.HEALTH;
          --num2;
        }
      }
      int num3 = 0;
      for (int index = 0; index < 22; ++index)
      {
        if (this.m_CrateContents[index].m_Contents == ItemManager.CrateContents.CRATECONTENTS.NONE)
        {
          this.m_CrateContents[index].m_Contents = ItemManager.CrateContents.CRATECONTENTS.AMMO;
          ++num3;
        }
      }
    }

    public int GetCrateIndexByItemIndex(int itemIdx)
    {
      for (int index = 0; index < 22; ++index)
      {
        if (this.m_CrateContents[index].m_ItemId == itemIdx)
          return this.m_CrateContents[index].m_CrateId;
      }
      return -1;
    }

    public int GetItemIndexByCrateIndex(short crateIdx)
    {
      for (int index = 0; index < 22; ++index)
      {
        if (this.m_CrateContents[index].m_CrateId == (int) crateIdx)
          return this.m_CrateContents[index].m_ItemId;
      }
      return -1;
    }

    public ItemManager.CrateContents.CRATECONTENTS GetCrateContents(int crateIndex) => this.m_CrateContents[crateIndex].m_Contents;

    public void EmptyCrate(int crateIndex) => this.m_CrateContents[crateIndex].m_Contents = ItemManager.CrateContents.CRATECONTENTS.NONE;

    public byte[] GetCrateStateBuffer()
    {
      byte[] crateStateBuffer = new byte[22];
      for (int index = 0; index < 22; ++index)
        crateStateBuffer[index] = (byte) this.m_CrateContents[index].m_Contents;
      return crateStateBuffer;
    }

    public void SetCrateStatesFromBuffer(byte[] buffer)
    {
      for (int index = 0; index < 22; ++index)
        this.m_CrateContents[index].m_Contents = (ItemManager.CrateContents.CRATECONTENTS) buffer[index];
    }

    public void ReturnArtifactToCrate() => this.m_CrateContents[this.m_ArtifactCrateId].m_Contents = ItemManager.CrateContents.CRATECONTENTS.ARTIFACT;

    public void ShowArtifactReturnedMessage()
    {
      g.m_App.AddHelmetMessage(HelmetMessage.TYPE.ARTIFACT_IN_CONTAINER, "ARTIFACT RETURNED", new Vector2(120f, 190f), 5f, g.m_App.HUD_GREEN, SoundManager.SFX.WarningBeep, (SpriteFont) null, true);
      if (g.m_App.m_AlarmSFX != null)
      {
        g.m_App.m_AlarmSFX.Stop();
        g.m_App.m_AlarmSFX = (SoundEffectInstance) null;
      }
      if (g.m_PlayerManager.GetLocalPlayer() == null)
        return;
      g.m_PlayerManager.GetLocalPlayer().ClearUsedCrate(this.m_ArtifactCrateId);
    }

    public byte GetArtifactNetId()
    {
      for (int index = 0; index < 125; ++index)
      {
        if (this.m_Item[index].m_Type == 10 && this.m_Item[index].m_Id != -1 && this.m_Item[index].m_NetId != byte.MaxValue)
          return this.m_Item[index].m_NetId;
      }
      return byte.MaxValue;
    }

    public int GetArtifactItemId()
    {
      for (int index = 0; index < 125; ++index)
      {
        if (this.m_Item[index].m_Type == 10 && this.m_Item[index].m_Id != -1 && this.m_Item[index].m_NetId != byte.MaxValue)
          return this.m_Item[index].m_Id;
      }
      return -1;
    }

    public Vector3 GetArtifactCratePos()
    {
      Matrix world = this.m_Item[this.GetItemIndexByCrateIndex((short) this.m_ArtifactCrateId)].m_SceneObject.World;
      return ((Matrix) ref world).Translation;
    }

    public byte GetNextItemNetId()
    {
      ++this.m_ItemNetId;
      if (this.m_ItemNetId > (byte) 254)
        this.m_ItemNetId = (byte) 0;
      return this.m_ItemNetId;
    }

    public int GetItemIdByNetItemId(byte netItemId)
    {
      for (int itemIdByNetItemId = 0; itemIdByNetItemId < 125; ++itemIdByNetItemId)
      {
        if ((int) this.m_Item[itemIdByNetItemId].m_NetId == (int) netItemId)
          return itemIdByNetItemId;
      }
      return -1;
    }

    public class CrateContents
    {
      public int m_ItemId;
      public int m_CrateId;
      public ItemManager.CrateContents.CRATECONTENTS m_Contents;
      public bool m_bUsed;

      public CrateContents()
      {
        this.m_ItemId = -1;
        this.m_Contents = ItemManager.CrateContents.CRATECONTENTS.NONE;
        this.m_bUsed = false;
      }

      public enum CRATECONTENTS
      {
        NONE,
        ARTIFACT,
        ALIEN,
        RPG,
        HEALTH,
        AMMO,
        LOCATOR,
      }
    }
  }
}
