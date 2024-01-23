using BEPUphysics;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.MathExtensions;
using DPSF;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using SynapseGaming.LightingSystem.Core;
using SynapseGaming.LightingSystem.Effects.Deferred;
using SynapseGaming.LightingSystem.Lights;
using SynapseGaming.LightingSystem.Rendering;
using SynapseGaming.LightingSystem.Shadows;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Saturn9
{
  public class Item
  {
    private const float ZOOM_MAC10 = 30f;
    public const int MAX_ITEMS_VFX = 3;
    public const int MAC10_MAX_AMMO = 100;
    public const int MAC10_CLIP_SIZE = 25;
    public const int PISTOL_MAX_AMMO = 100;
    public const int PISTOL_CLIP_SIZE = 15;
    public const int RPG_MAX_AMMO = 3;
    public const int RPG_CLIP_SIZE = 1;
    public const int CLAWS_MAX_AMMO = 0;
    public const int CLAWS_CLIP_SIZE = 0;
    private Matrix m_LockerStartMtx = Matrix.Identity;
    private float m_LockerCurrentAng;
    public Item.LOCKERSTATE m_LockerState;
    private static float ARM_FIRE_RATE = 150f;
    private Matrix m_SafeStartMtx = Matrix.Identity;
    private float m_SafeCurrentAng;
    public Item.SAFESTATE m_SafeState;
    private Matrix m_StartMtx = Matrix.Identity;
    private float m_CurrentAng;
    public Item.CRATESTATE m_CrateState;
    public CrateSmokeParticleSystem m_CrateSmoke;
    private static float MAC10_FIRE_RATE = 150f;
    public bool m_CanSawArm;
    private static float SAW_FIRE_RATE = 150f;
    private Matrix m_DoorStartMtx = Matrix.Identity;
    public PointLight m_DoorLight;
    public SoundEffectInstance m_DoorSFX;
    public Item.DOORSTATE m_DoorState;
    public CrateSmokeParticleSystem m_DoorSmoke;
    public BEPUphysics.Entities.Entity m_Collision;
    private float m_DoorStartY;
    private float m_DoorLightStartY;
    private static float SCREWDRIVER_FIRE_RATE = 150f;
    public int m_Id;
    public byte m_NetId;
    public Model m_Model;
    public SceneObject m_SceneObject;
    public int m_Type;
    public Player m_Player;
    public int m_VfxSystemIdx;
    public ExplosionFireSmokeParticleSystem m_WeaponSmoke;
    public ExplosionFlyingSparksParticleSystem[] m_WeaponSparks;
    public BuckshotQuadSprayParticleSystem[] m_WeaponBuckshot;
    public float m_WeaponTimer;
    public PointLight m_WeaponFlash;
    public int m_WeaponFlashCount;
    public int m_WeaponAmmo;
    public int m_WeaponAmmoInClip;
    public int m_WeaponAmmoToGive;
    public Item.RELOAD m_ReloadState;
    private bool m_bShouldRicochetSFX = true;
    private bool m_bShouldRicochetVFX = true;
    private bool m_bZoomed;
    private int m_MuzzleIdx = -1;
    public SceneObject m_MagazineSceneObject;
    private Vector2 centre = new Vector2(512f, 288f);
    private Vector2 spriteHalfSize = new Vector2(32f, 32f);
    private Matrix m_FogStartMtx = Matrix.Identity;
    public float m_MaxFogAlpha;
    public float m_FogAlpha;
    public static float NEAR_DIST_END_SQ = 100f;
    public static float FAR_DIST_END_SQ = 625f;

    public void InitLocker()
    {
      this.Show();
      this.m_LockerStartMtx = this.m_SceneObject.RenderableMeshes[1].MeshToObject;
      this.m_LockerState = Item.LOCKERSTATE.CLOSED;
      foreach (RenderableMesh renderableMesh in (ReadOnlyCollection<RenderableMesh>) this.m_SceneObject.RenderableMeshes)
      {
        BoundingBox meshBoundingBox = renderableMesh.MeshBoundingBox;
        renderableMesh.MeshBoundingBox = new BoundingBox(new Vector3(meshBoundingBox.Min.Z * 10f, meshBoundingBox.Min.Y * 10f, meshBoundingBox.Min.X * 10f), new Vector3(meshBoundingBox.Max.Z * 10f, meshBoundingBox.Max.Y * 10f, meshBoundingBox.Max.X * 10f));
      }
      this.m_SceneObject.CalculateBounds();
      if (this.m_Collision == null)
      {
        Matrix world = this.m_SceneObject.World;
        this.m_Collision = (BEPUphysics.Entities.Entity) new Box(Vector3.op_Addition(((Matrix) ref world).Translation, new Vector3(0.0f, 3f, 0.0f)), 1.4f, 5f, 1.4f);
        this.m_Collision.OrientationMatrix = Matrix3X3.CreateFromMatrix(this.m_SceneObject.World);
        this.m_Collision.Tag = (object) this;
        g.m_App.m_Space.Add((ISpaceObject) this.m_Collision);
      }
      else
      {
        BEPUphysics.Entities.Entity collision = this.m_Collision;
        Matrix world = this.m_SceneObject.World;
        Vector3 vector3 = Vector3.op_Addition(((Matrix) ref world).Translation, new Vector3(0.0f, 3f, 0.0f));
        collision.Position = vector3;
        this.m_Collision.OrientationMatrix = Matrix3X3.CreateFromMatrix(this.m_SceneObject.World);
      }
      this.m_Collision.IsAffectedByGravity = false;
      this.m_Collision.LinearVelocity = Vector3.Zero;
      this.m_Collision.AngularVelocity = Vector3.Zero;
    }

    public void UpdateLocker()
    {
      switch (this.m_LockerState)
      {
        case Item.LOCKERSTATE.CLOSED:
          Matrix lockerStartMtx = this.m_LockerStartMtx;
          ((Matrix) ref lockerStartMtx).Translation = Vector3.op_Addition(((Matrix) ref this.m_LockerStartMtx).Translation, Vector3.op_Multiply(((Matrix) ref this.m_LockerStartMtx).Left, 20f));
          this.m_SceneObject.RenderableMeshes[1].MeshToObject = lockerStartMtx;
          this.m_SceneObject.RenderableMeshes[2].MeshToObject = lockerStartMtx;
          this.m_SceneObject.RenderableMeshes[3].MeshToObject = lockerStartMtx;
          this.m_SceneObject.RenderableMeshes[4].MeshToObject = lockerStartMtx;
          this.m_SceneObject.RenderableMeshes[5].MeshToObject = lockerStartMtx;
          if (this.m_SceneObject.RenderableMeshes.Count > 6)
            this.m_SceneObject.RenderableMeshes[6].MeshToObject = lockerStartMtx;
          this.m_LockerCurrentAng = 0.0f;
          break;
        case Item.LOCKERSTATE.OPENING:
          this.m_LockerCurrentAng = MathHelper.Lerp(this.m_LockerCurrentAng, 1.9f, 0.05f);
          Matrix matrix = Matrix.op_Multiply(this.m_LockerStartMtx, Matrix.CreateRotationY(this.m_LockerCurrentAng));
          ((Matrix) ref matrix).Translation = Vector3.op_Addition(((Matrix) ref this.m_LockerStartMtx).Translation, Vector3.op_Multiply(((Matrix) ref this.m_LockerStartMtx).Left, 20f));
          this.m_SceneObject.RenderableMeshes[1].MeshToObject = matrix;
          this.m_SceneObject.RenderableMeshes[2].MeshToObject = matrix;
          this.m_SceneObject.RenderableMeshes[3].MeshToObject = matrix;
          this.m_SceneObject.RenderableMeshes[4].MeshToObject = matrix;
          this.m_SceneObject.RenderableMeshes[5].MeshToObject = matrix;
          if (this.m_SceneObject.RenderableMeshes.Count <= 6)
            break;
          this.m_SceneObject.RenderableMeshes[6].MeshToObject = matrix;
          break;
      }
    }

    private void PlayLockerUseSFX()
    {
      SoundManager soundManager = g.m_SoundManager;
      Matrix world = this.m_SceneObject.World;
      Vector3 translation = ((Matrix) ref world).Translation;
      soundManager.Play3D(16, translation);
    }

    public bool PeerUseLocker()
    {
      this.m_LockerState = Item.LOCKERSTATE.OPENING;
      this.m_WeaponTimer = !g.m_App.m_SurvivalMode ? (float) g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 3f : (float) g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 10f;
      return true;
    }

    private void HideLocker()
    {
    }

    public void InitArm()
    {
      this.m_WeaponTimer = 0.0f;
      this.InitWeaponFlash();
      this.m_SceneObject.Visibility = ObjectVisibility.RenderedAndCastShadows;
      this.m_WeaponAmmo = 25;
      this.m_WeaponAmmoInClip = 25;
      this.m_bShouldRicochetVFX = true;
      this.m_bShouldRicochetSFX = true;
    }

    public void UpdateArm() => this.UpdateWeaponFlash();

    private bool FireArm()
    {
      if ((double) this.m_WeaponTimer > g.m_App.m_GameTime.TotalGameTime.TotalMilliseconds)
        return false;
      if (this.m_WeaponAmmoInClip == 0)
      {
        if (this.m_ReloadState == Item.RELOAD.None)
          this.m_WeaponTimer = (float) g.m_App.m_GameTime.TotalGameTime.TotalMilliseconds + Item.ARM_FIRE_RATE;
        return false;
      }
      this.m_WeaponTimer = (float) g.m_App.m_GameTime.TotalGameTime.TotalMilliseconds + 500f;
      return true;
    }

    private void PlayArmFireSFX()
    {
    }

    private bool SimulateFireArm()
    {
      this.StartSmokePuffAndFlash(this.m_Player.GetAimVector());
      this.PlayArmFireSFX();
      return true;
    }

    private void HideArm()
    {
    }

    private void ZoomArm(bool bZoom)
    {
    }

    public void InitSafe()
    {
      this.Show();
      this.m_SafeStartMtx = this.m_SceneObject.RenderableMeshes[1].MeshToObject;
      this.m_SafeState = Item.SAFESTATE.CLOSED;
      foreach (RenderableMesh renderableMesh in (ReadOnlyCollection<RenderableMesh>) this.m_SceneObject.RenderableMeshes)
      {
        BoundingBox meshBoundingBox = renderableMesh.MeshBoundingBox;
        renderableMesh.MeshBoundingBox = new BoundingBox(new Vector3(meshBoundingBox.Min.Z * 100f, meshBoundingBox.Min.Y + 2.159f, meshBoundingBox.Min.X * 100f), new Vector3(meshBoundingBox.Max.Z * 100f, meshBoundingBox.Max.Y + 2.159f, meshBoundingBox.Max.X * 100f));
      }
      this.m_SceneObject.CalculateBounds();
    }

    public void UpdateSafe()
    {
      switch (this.m_SafeState)
      {
        case Item.SAFESTATE.CLOSED:
          Matrix safeStartMtx = this.m_SafeStartMtx;
          ((Matrix) ref safeStartMtx).Translation = Vector3.op_Addition(Vector3.op_Addition(Vector3.op_Addition(((Matrix) ref this.m_SafeStartMtx).Translation, Vector3.op_Multiply(((Matrix) ref this.m_SafeStartMtx).Left, -1.52f)), Vector3.op_Multiply(((Matrix) ref this.m_SafeStartMtx).Forward, 0.0f)), Vector3.op_Multiply(((Matrix) ref this.m_SafeStartMtx).Up, -0.88f));
          this.m_SceneObject.RenderableMeshes[1].MeshToObject = safeStartMtx;
          this.m_SceneObject.RenderableMeshes[2].MeshToObject = safeStartMtx;
          this.m_SafeCurrentAng = 0.0f;
          break;
        case Item.SAFESTATE.OPENING:
          this.m_SafeCurrentAng = MathHelper.Lerp(this.m_SafeCurrentAng, -2.3f, 0.05f);
          Matrix matrix = Matrix.op_Multiply(this.m_SafeStartMtx, Matrix.CreateRotationY(this.m_SafeCurrentAng));
          ((Matrix) ref matrix).Translation = Vector3.op_Addition(Vector3.op_Addition(Vector3.op_Addition(((Matrix) ref this.m_SafeStartMtx).Translation, Vector3.op_Multiply(((Matrix) ref this.m_SafeStartMtx).Left, -1.52f)), Vector3.op_Multiply(((Matrix) ref this.m_SafeStartMtx).Forward, 0.0f)), Vector3.op_Multiply(((Matrix) ref this.m_SafeStartMtx).Up, -0.88f));
          this.m_SceneObject.RenderableMeshes[1].MeshToObject = matrix;
          this.m_SceneObject.RenderableMeshes[2].MeshToObject = matrix;
          break;
      }
    }

    private void PlaySafeUseSFX()
    {
      SoundManager soundManager = g.m_SoundManager;
      Matrix world = this.m_SceneObject.World;
      Vector3 translation = ((Matrix) ref world).Translation;
      soundManager.Play3D(16, translation);
    }

    public bool PeerUseSafe(int crateIdx, short playerNetId)
    {
      this.m_SafeState = Item.SAFESTATE.OPENING;
      this.m_WeaponTimer = !g.m_App.m_SurvivalMode ? (float) g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 3f : (float) g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 10f;
      return true;
    }

    private void HideSafe()
    {
    }

    public void InitCrate()
    {
      this.Show();
      this.m_StartMtx = this.m_SceneObject.RenderableMeshes[1].MeshToObject;
      if (this.m_CrateSmoke == null)
      {
        this.m_CrateSmoke = new CrateSmokeParticleSystem((Game) g.m_App);
        this.m_CrateSmoke.Enabled = false;
        this.m_CrateSmoke.AutoInitialize(g.m_App.GraphicsDevice, g.m_App.Content, g.m_App.screenManager.SpriteBatch);
        g.m_App.m_ParticleSystemManager.AddParticleSystem((IDPSFParticleSystem) this.m_CrateSmoke);
      }
      this.m_CrateState = Item.CRATESTATE.CLOSED;
    }

    public void UpdateCrate()
    {
      switch (this.m_CrateState)
      {
        case Item.CRATESTATE.CLOSED:
          Matrix startMtx = this.m_StartMtx;
          ((Matrix) ref startMtx).Translation = Vector3.op_Addition(Vector3.op_Addition(((Matrix) ref this.m_StartMtx).Translation, Vector3.op_Multiply(((Matrix) ref this.m_StartMtx).Forward, -0.96f)), Vector3.op_Multiply(((Matrix) ref this.m_StartMtx).Up, 0.9f));
          this.m_SceneObject.RenderableMeshes[1].MeshToObject = startMtx;
          this.m_CrateSmoke.Enabled = false;
          this.m_CrateSmoke.Visible = false;
          this.m_CurrentAng = 0.0f;
          break;
        case Item.CRATESTATE.OPENING:
          this.m_CurrentAng = MathHelper.Lerp(this.m_CurrentAng, 0.3f, 0.1f);
          Matrix matrix = Matrix.op_Multiply(this.m_StartMtx, Matrix.CreateRotationX(this.m_CurrentAng));
          ((Matrix) ref matrix).Translation = Vector3.op_Addition(Vector3.op_Addition(((Matrix) ref this.m_StartMtx).Translation, Vector3.op_Multiply(((Matrix) ref this.m_StartMtx).Forward, -1f)), Vector3.op_Multiply(((Matrix) ref this.m_StartMtx).Up, 0.9f));
          this.m_SceneObject.RenderableMeshes[1].MeshToObject = matrix;
          if ((double) this.m_WeaponTimer >= g.m_App.m_GameTime.TotalGameTime.TotalSeconds)
            break;
          this.m_CrateState = Item.CRATESTATE.CLOSED;
          SoundManager soundManager = g.m_SoundManager;
          Matrix world = this.m_SceneObject.World;
          Vector3 translation = ((Matrix) ref world).Translation;
          soundManager.Play3D(17, translation);
          break;
      }
    }

    private void PlayCrateUseSFX()
    {
      SoundManager soundManager = g.m_SoundManager;
      Matrix world1 = this.m_SceneObject.World;
      Vector3 translation = ((Matrix) ref world1).Translation;
      soundManager.Play3D(16, translation);
      this.m_CrateSmoke.Enabled = true;
      this.m_CrateSmoke.Visible = true;
      this.m_CrateSmoke.Emitter.BurstTime = 1f;
      Position3D positionData = this.m_CrateSmoke.Emitter.PositionData;
      Matrix world2 = this.m_SceneObject.World;
      Vector3 vector3 = Vector3.op_Addition(((Matrix) ref world2).Translation, new Vector3(0.0f, 1.2f, 0.0f));
      positionData.Position = vector3;
      this.m_CrateSmoke.Emitter.PositionData.Velocity = new Vector3(0.0f, 0.0f, 0.0f);
      this.m_CrateSmoke.LerpEmittersPositionAndOrientationOnNextUpdate = false;
    }

    public bool PeerUseCrate(int crateIdx, short playerNetId)
    {
      this.PlayCrateUseSFX();
      this.m_CrateState = Item.CRATESTATE.OPENING;
      this.m_WeaponTimer = !g.m_App.m_SurvivalMode ? (float) g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 3f : (float) g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 10f;
      if (!g.m_App.m_SurvivalMode)
      {
        g.m_ItemManager.EmptyCrate(crateIdx);
        if ((int) playerNetId == (int) g.m_PlayerManager.GetLocalPlayer().m_NetId)
          g.m_PlayerManager.GetLocalPlayer().SetUsedCrate(crateIdx);
      }
      return true;
    }

    private void HideCrate()
    {
    }

    public void InitTorch()
    {
      this.m_WeaponTimer = 0.0f;
      this.InitWeaponFlash();
      this.m_WeaponAmmo = 25;
      this.m_WeaponAmmoInClip = 25;
      this.m_bShouldRicochetVFX = true;
      this.m_bShouldRicochetSFX = true;
    }

    public void UpdateTorch() => this.UpdateWeaponFlash();

    private bool FireTorch()
    {
      if ((double) this.m_WeaponTimer > g.m_App.m_GameTime.TotalGameTime.TotalMilliseconds)
        return false;
      if (this.m_WeaponAmmoInClip == 0)
      {
        if (this.m_ReloadState == Item.RELOAD.None)
          this.m_WeaponTimer = (float) g.m_App.m_GameTime.TotalGameTime.TotalMilliseconds + Item.MAC10_FIRE_RATE;
        return false;
      }
      Vector3 aimPosition = this.m_Player.GetAimPosition();
      Vector3 aimVector = this.m_Player.GetAimVector();
      this.StartSmokePuffAndFlash(aimVector);
      Vector3 vector3 = DPSFHelper.RandomNormalizedVector();
      float num = 6f;
      if (this.m_bZoomed)
        num = 4f;
      Vector3 direction = Vector3.Transform(aimVector, Quaternion.CreateFromAxisAngle(vector3, (float) (g.m_App.m_Rand.NextDouble() - 0.5) * MathHelper.ToRadians(num)));
      this.DoWeaponRayCast(aimPosition, direction, 101.6f, 15);
      this.PlayTorchFireSFX();
      --this.m_WeaponAmmoInClip;
      this.m_WeaponTimer = (float) g.m_App.m_GameTime.TotalGameTime.TotalMilliseconds + Item.MAC10_FIRE_RATE;
      if (!this.m_Player.m_Bot)
        g.m_App.m_RumbleFrames = 3;
      return true;
    }

    private void PlayTorchFireSFX()
    {
    }

    private bool SimulateFireTorch()
    {
      this.StartSmokePuffAndFlash(this.m_Player.GetAimVector());
      this.PlayTorchFireSFX();
      return true;
    }

    private void HideTorch()
    {
    }

    private void ZoomTorch(bool bZoom)
    {
      if (bZoom)
        g.m_CameraManager.SetTargetFov(30f);
      else
        g.m_CameraManager.SetDefaultFov();
    }

    public void InitSaw()
    {
      this.m_WeaponTimer = 0.0f;
      this.InitWeaponFlash();
      this.m_WeaponAmmo = 25;
      this.m_WeaponAmmoInClip = 25;
      this.m_CanSawArm = false;
      this.m_bShouldRicochetVFX = true;
      this.m_bShouldRicochetSFX = true;
    }

    public void UpdateSaw() => this.UpdateWeaponFlash();

    private bool FireSaw()
    {
      if ((double) this.m_WeaponTimer > g.m_App.m_GameTime.TotalGameTime.TotalMilliseconds)
        return false;
      if (this.m_WeaponAmmoInClip == 0)
      {
        if (this.m_ReloadState == Item.RELOAD.None)
          this.m_WeaponTimer = (float) g.m_App.m_GameTime.TotalGameTime.TotalMilliseconds + Item.SAW_FIRE_RATE;
        return false;
      }
      if (this.m_CanSawArm)
        this.m_Player.SawArm();
      this.m_WeaponTimer = (float) g.m_App.m_GameTime.TotalGameTime.TotalMilliseconds + 500f;
      return true;
    }

    private void PlaySawFireSFX()
    {
    }

    private bool SimulateFireSaw()
    {
      this.StartSmokePuffAndFlash(this.m_Player.GetAimVector());
      this.PlaySawFireSFX();
      return true;
    }

    private void HideSaw()
    {
    }

    private void ZoomSaw(bool bZoom)
    {
    }

    public void InitDoor()
    {
      this.Show();
      this.m_DoorStartMtx = this.m_SceneObject.RenderableMeshes[0].MeshToObject;
      Matrix world1 = this.m_SceneObject.World;
      this.m_DoorStartY = ((Matrix) ref world1).Translation.Y;
      if (this.m_DoorSmoke == null)
      {
        this.m_DoorSmoke = new CrateSmokeParticleSystem((Game) g.m_App);
        this.m_DoorSmoke.Enabled = false;
        this.m_DoorSmoke.AutoInitialize(g.m_App.GraphicsDevice, g.m_App.Content, g.m_App.screenManager.SpriteBatch);
        g.m_App.m_ParticleSystemManager.AddParticleSystem((IDPSFParticleSystem) this.m_DoorSmoke);
      }
      this.m_DoorState = Item.DOORSTATE.CLOSED;
      if (this.m_Collision == null)
      {
        Matrix world2 = this.m_SceneObject.World;
        this.m_Collision = (BEPUphysics.Entities.Entity) new Box(Vector3.op_Addition(((Matrix) ref world2).Translation, new Vector3(0.0f, 3f, 0.0f)), 5f, 5f, 1f);
        this.m_Collision.OrientationMatrix = Matrix3X3.CreateFromMatrix(this.m_SceneObject.World);
        this.m_Collision.Tag = (object) this;
      }
      else
      {
        BEPUphysics.Entities.Entity collision = this.m_Collision;
        Matrix world3 = this.m_SceneObject.World;
        Vector3 vector3 = Vector3.op_Addition(((Matrix) ref world3).Translation, new Vector3(0.0f, 3f, 0.0f));
        collision.Position = vector3;
        this.m_Collision.OrientationMatrix = Matrix3X3.CreateFromMatrix(this.m_SceneObject.World);
      }
      g.m_App.m_Space.Add((ISpaceObject) this.m_Collision);
      this.m_Collision.IsAffectedByGravity = false;
      this.m_Collision.LinearVelocity = Vector3.Zero;
      this.m_Collision.AngularVelocity = Vector3.Zero;
      if (this.m_DoorLight == null)
        return;
      Matrix world4 = this.m_DoorLight.World;
      Vector3 translation = ((Matrix) ref world4).Translation;
      translation.Y = this.m_DoorLightStartY;
      ((Matrix) ref world4).Translation = translation;
      this.m_DoorLight.World = world4;
      this.m_DoorLight = (PointLight) null;
      this.m_DoorLightStartY = 0.0f;
    }

    public void UpdateDoor()
    {
      switch (this.m_DoorState)
      {
        case Item.DOORSTATE.CLOSED:
          this.m_DoorSmoke.Enabled = false;
          this.m_DoorSmoke.Visible = false;
          break;
        case Item.DOORSTATE.OPENING:
          Matrix world1 = this.m_SceneObject.World;
          Matrix world2 = this.m_SceneObject.World;
          Vector3 translation1 = ((Matrix) ref world2).Translation;
          float num1 = (float) (0.019999999552965164 * (30.0 * g.m_App.m_GameTime.ElapsedGameTime.TotalSeconds));
          if ((double) translation1.Y < 1.3999999761581421)
          {
            translation1.Y += num1;
            if (this.m_DoorSFX != null)
            {
              SoundManager soundManager = g.m_SoundManager;
              SoundEffectInstance doorSfx = this.m_DoorSFX;
              Matrix world3 = this.m_SceneObject.World;
              Vector3 translation2 = ((Matrix) ref world3).Translation;
              soundManager.UpdateTrackedSound(doorSfx, translation2);
            }
            if (this.m_DoorLight != null)
            {
              if ((double) this.m_DoorLightStartY == 0.0)
              {
                Matrix world4 = this.m_DoorLight.World;
                this.m_DoorLightStartY = ((Matrix) ref world4).Translation.Y;
              }
              Matrix world5 = this.m_DoorLight.World;
              Matrix world6 = this.m_DoorLight.World;
              Vector3 translation3 = ((Matrix) ref world6).Translation;
              translation3.Y += num1;
              ((Matrix) ref world5).Translation = translation3;
              this.m_DoorLight.World = world5;
            }
          }
          ((Matrix) ref world1).Translation = translation1;
          this.m_SceneObject.World = world1;
          this.m_Collision.Position = Vector3.op_Addition(translation1, new Vector3(0.0f, 3f, 0.0f));
          break;
        case Item.DOORSTATE.CLOSING:
          Matrix world7 = this.m_SceneObject.World;
          Matrix world8 = this.m_SceneObject.World;
          Vector3 translation4 = ((Matrix) ref world8).Translation;
          float num2 = (float) (0.019999999552965164 * (30.0 * g.m_App.m_GameTime.ElapsedGameTime.TotalSeconds));
          if ((double) translation4.Y > (double) this.m_DoorStartY)
          {
            translation4.Y -= num2;
            if (this.m_DoorSFX != null)
            {
              SoundManager soundManager = g.m_SoundManager;
              SoundEffectInstance doorSfx = this.m_DoorSFX;
              Matrix world9 = this.m_SceneObject.World;
              Vector3 translation5 = ((Matrix) ref world9).Translation;
              soundManager.UpdateTrackedSound(doorSfx, translation5);
            }
          }
          else
            this.m_DoorState = Item.DOORSTATE.CLOSED;
          ((Matrix) ref world7).Translation = translation4;
          this.m_SceneObject.World = world7;
          this.m_Collision.Position = Vector3.op_Addition(translation4, new Vector3(0.0f, 3f, 0.0f));
          break;
      }
    }

    private void PlayDoorUseSFX()
    {
      SoundManager soundManager1 = g.m_SoundManager;
      Matrix world1 = this.m_SceneObject.World;
      Vector3 translation1 = ((Matrix) ref world1).Translation;
      soundManager1.Play3D(16, translation1);
      SoundManager soundManager2 = g.m_SoundManager;
      Matrix world2 = this.m_SceneObject.World;
      Vector3 translation2 = ((Matrix) ref world2).Translation;
      this.m_DoorSFX = soundManager2.Play3D(29, translation2);
      this.m_DoorSmoke.Enabled = true;
      this.m_DoorSmoke.Visible = true;
      this.m_DoorSmoke.Emitter.BurstTime = 1f;
      Position3D positionData = this.m_DoorSmoke.Emitter.PositionData;
      Matrix world3 = this.m_SceneObject.World;
      Vector3 vector3 = Vector3.op_Addition(((Matrix) ref world3).Translation, new Vector3(0.0f, 1.2f, 0.0f));
      positionData.Position = vector3;
      this.m_DoorSmoke.Emitter.PositionData.Velocity = new Vector3(0.0f, 0.0f, 0.0f);
      this.m_DoorSmoke.LerpEmittersPositionAndOrientationOnNextUpdate = false;
    }

    public bool PeerUseDoor(short playerNetId)
    {
      this.PlayDoorUseSFX();
      this.m_DoorState = Item.DOORSTATE.OPENING;
      return true;
    }

    public void PeerCloseDoor()
    {
      if (this.m_DoorState == Item.DOORSTATE.CLOSED)
        return;
      this.m_DoorState = Item.DOORSTATE.CLOSING;
      SoundManager soundManager = g.m_SoundManager;
      Matrix world = this.m_SceneObject.World;
      Vector3 translation = ((Matrix) ref world).Translation;
      this.m_DoorSFX = soundManager.Play3D(29, translation);
    }

    private void HideDoor()
    {
    }

    public void InitScrewdriver()
    {
      this.m_WeaponTimer = 0.0f;
      this.InitWeaponFlash();
      this.m_SceneObject.Visibility = ObjectVisibility.RenderedAndCastShadows;
      this.m_WeaponAmmo = 25;
      this.m_WeaponAmmoInClip = 25;
      this.m_bShouldRicochetVFX = true;
      this.m_bShouldRicochetSFX = true;
    }

    public void UpdateScrewdriver() => this.UpdateWeaponFlash();

    private bool FireScrewdriver()
    {
      if ((double) this.m_WeaponTimer > g.m_App.m_GameTime.TotalGameTime.TotalMilliseconds)
        return false;
      if (this.m_WeaponAmmoInClip == 0)
      {
        if (this.m_ReloadState == Item.RELOAD.None)
          this.m_WeaponTimer = (float) g.m_App.m_GameTime.TotalGameTime.TotalMilliseconds + Item.SCREWDRIVER_FIRE_RATE;
        return false;
      }
      this.m_Player.OpenLocker();
      this.m_WeaponTimer = (float) g.m_App.m_GameTime.TotalGameTime.TotalMilliseconds + 1200f;
      return true;
    }

    private void PlayScrewdriverFireSFX()
    {
    }

    private bool SimulateFireScrewdriver()
    {
      this.StartSmokePuffAndFlash(this.m_Player.GetAimVector());
      this.PlayScrewdriverFireSFX();
      return true;
    }

    private void HideScrewdriver()
    {
    }

    private void ZoomScrewdriver(bool bZoom)
    {
    }

    public Item()
    {
      this.m_Type = -1;
      this.m_Id = -1;
      this.m_NetId = byte.MaxValue;
      this.m_Model = (Model) null;
      this.m_SceneObject = (SceneObject) null;
      this.m_Player = (Player) null;
      this.m_VfxSystemIdx = 0;
      this.m_WeaponBuckshot = new BuckshotQuadSprayParticleSystem[3];
      this.m_WeaponSparks = new ExplosionFlyingSparksParticleSystem[3];
      this.m_WeaponSmoke = (ExplosionFireSmokeParticleSystem) null;
      for (int index = 0; index < 3; ++index)
      {
        this.m_WeaponBuckshot[index] = (BuckshotQuadSprayParticleSystem) null;
        this.m_WeaponSparks[index] = (ExplosionFlyingSparksParticleSystem) null;
      }
    }

    public void Update()
    {
      switch (this.m_Type)
      {
        case 1:
          if (!this.ActiveWeapon())
            break;
          this.UpdateTorch();
          break;
        case 16:
        case 17:
        case 20:
        case 26:
          this.UpdateDoor();
          break;
        case 18:
          this.UpdateSafe();
          break;
        case 19:
          if (!this.ActiveWeapon())
            break;
          this.UpdateArm();
          break;
        case 21:
          this.UpdateFog();
          break;
        case 22:
          if (!this.ActiveWeapon())
            break;
          this.UpdateSaw();
          break;
        case 23:
        case 24:
          this.UpdateLocker();
          break;
        case 25:
          if (!this.ActiveWeapon())
            break;
          this.UpdateScrewdriver();
          break;
      }
    }

    private bool ActiveWeapon() => this.m_Player != null && this.m_Player.m_Health > (sbyte) 0 && this.m_Player.m_WeaponItemIndex == this.m_Id;

    public void Delete()
    {
      this.m_Id = -1;
      this.m_NetId = byte.MaxValue;
      this.DisableWeaponVFX();
      if (this.m_SceneObject != null)
      {
        ((ISubmit<SceneEntity>) g.m_App.sceneInterface.ObjectManager).Remove((SceneEntity) this.m_SceneObject);
        this.m_SceneObject = (SceneObject) null;
      }
      if (this.m_MagazineSceneObject != null)
      {
        ((ISubmit<SceneEntity>) g.m_App.sceneInterface.ObjectManager).Remove((SceneEntity) this.m_MagazineSceneObject);
        this.m_MagazineSceneObject = (SceneObject) null;
      }
      if (this.m_WeaponFlash != null)
      {
        ((ISubmit<BaseLight>) g.m_App.sceneInterface.LightManager).Remove((BaseLight) this.m_WeaponFlash);
        this.m_WeaponFlash = (PointLight) null;
        this.m_WeaponFlashCount = 0;
      }
      if (this.m_MuzzleIdx != -1 && g.m_ItemManager.m_Item[this.m_MuzzleIdx].m_Id != -1)
      {
        g.m_ItemManager.Delete(this.m_MuzzleIdx);
        this.m_MuzzleIdx = -1;
      }
      if (this.m_Collision == null)
        return;
      g.m_App.m_Space.Remove((ISpaceObject) this.m_Collision);
      this.m_Collision = (BEPUphysics.Entities.Entity) null;
    }

    public bool Fire(bool bDebounced)
    {
      if (this.m_Player.m_CurrentViewAnim == 2)
        return false;
      bool flag = false;
      switch (this.m_Type)
      {
        case 22:
          if (!bDebounced)
            return false;
          flag = this.FireSaw();
          break;
        case 25:
          if (!bDebounced)
            return false;
          flag = this.FireScrewdriver();
          break;
      }
      return flag;
    }

    public bool SimulateFire()
    {
      bool flag = false;
      switch (this.m_Type)
      {
        case 1:
          flag = this.SimulateFireTorch();
          break;
      }
      return flag;
    }

    public void DoWeaponRayCast(Vector3 position, Vector3 direction, float range, int damage)
    {
      List<RayCastResult> outputRayCastResults = new List<RayCastResult>();
      Ray ray;
      // ISSUE: explicit constructor call
      ((Ray) ref ray).\u002Ector(position, direction);
      if (!g.m_App.m_Space.RayCast(ray, range, (IList<RayCastResult>) outputRayCastResults))
        return;
      outputRayCastResults.Sort();
      Player playerToDamage = (Player) null;
      int hitZone = (int) byte.MaxValue;
      for (int index = 0; index < outputRayCastResults.Count; ++index)
      {
        if (!(outputRayCastResults[index].HitObject is EntityCollidable hitObject))
        {
          Vector3 location = outputRayCastResults[index].HitData.Location;
          Vector3 normal = outputRayCastResults[index].HitData.Normal;
          ((Vector3) ref normal).Normalize();
          this.DoImpactVFXSFX(location, normal);
          this.m_Player.m_RequestSendImpact = true;
          this.m_Player.m_RequestSendImpactPos = location;
          this.m_Player.m_RequestSendImpactNormal = normal;
          break;
        }
        if (hitObject.Entity.Tag is HitTag tag)
        {
          if (g.m_PlayerManager.m_Player[tag.m_PlayerId].m_Id != -1 && tag.m_PlayerId != this.m_Player.m_Id)
            playerToDamage = g.m_PlayerManager.m_Player[tag.m_PlayerId];
          if (playerToDamage != null)
          {
            if (tag.m_HitZone == 1)
              hitZone = 1;
            else if (tag.m_HitZone == 2)
              hitZone = 2;
            else if (tag.m_HitZone == 3)
              hitZone = 3;
          }
        }
        if (playerToDamage != null && hitZone != (int) byte.MaxValue)
        {
          this.m_Player.RequestDamageOther(hitZone, damage, playerToDamage, (short) byte.MaxValue);
          break;
        }
      }
    }

    public void DoImpactVFXSFX(Vector3 pos, Vector3 normal)
    {
    }

    public void DrawCrosshair(SpriteBatch spriteBatch)
    {
      switch (this.m_Type)
      {
        case 1:
          spriteBatch.Draw(g.m_ItemManager.m_Mac10Crosshair, Vector2.op_Subtraction(this.centre, this.spriteHalfSize), Color.White);
          break;
        case 11:
          spriteBatch.Draw(g.m_ItemManager.m_RPGCrosshair, Vector2.op_Subtraction(this.centre, new Vector2(64f, 64f)), Color.White);
          break;
        case 14:
          spriteBatch.Draw(g.m_ItemManager.m_ShotgunCrosshair, Vector2.op_Subtraction(this.centre, this.spriteHalfSize), Color.White);
          break;
      }
    }

    public void StartSmokePuffAndFlash(Vector3 direction)
    {
    }

    public bool IsZoomed() => this.m_bZoomed;

    public void InitWeaponFlash()
    {
      this.m_WeaponFlash = new PointLight();
      this.m_WeaponFlash.LightingType = LightingType.RealTime;
      this.m_WeaponFlash.DiffuseColor = new Vector3(10f, 10f, 8f);
      this.m_WeaponFlash.Intensity = 0.1f;
      this.m_WeaponFlash.Radius = 20f;
      this.m_WeaponFlash.FalloffStrength = 1f;
      this.m_WeaponFlash.Enabled = false;
      this.m_WeaponFlash.ShadowType = ShadowType.None;
      this.m_WeaponFlash.UpdateType = UpdateType.Automatic;
      ((ISubmit<BaseLight>) g.m_App.sceneInterface.LightManager).Submit((BaseLight) this.m_WeaponFlash);
      this.m_WeaponFlashCount = 0;
    }

    public void UpdateWeaponFlash()
    {
      if (!this.m_WeaponFlash.Enabled)
        return;
      if (this.m_WeaponFlashCount > 0)
      {
        --this.m_WeaponFlashCount;
        PointLight weaponFlash = this.m_WeaponFlash;
        Matrix world1 = this.m_SceneObject.World;
        Vector3 translation1 = ((Matrix) ref world1).Translation;
        Matrix world2 = this.m_SceneObject.World;
        Vector3 vector3_1 = Vector3.op_Multiply(((Matrix) ref world2).Forward, 0.5f);
        Vector3 vector3_2 = Vector3.op_Addition(translation1, vector3_1);
        weaponFlash.Position = vector3_2;
        if (this.m_MuzzleIdx == -1)
          return;
        g.m_ItemManager.m_Item[this.m_MuzzleIdx].m_SceneObject.Visibility = ObjectVisibility.Rendered;
        Matrix world3 = this.m_SceneObject.World;
        Matrix matrix = Matrix.op_Multiply(Matrix.CreateRotationZ((float) g.m_App.m_Rand.NextDouble() * 3.14f), world3);
        float num1 = 1.4f;
        float num2 = 0.1f;
        if (this.m_Type == 14)
        {
          num1 = 0.5f;
          num2 = 0.2f;
        }
        ref Matrix local = ref matrix;
        Vector3 translation2 = ((Matrix) ref local).Translation;
        Matrix world4 = this.m_SceneObject.World;
        Vector3 vector3_3 = Vector3.op_Multiply(((Matrix) ref world4).Forward, num1);
        Matrix world5 = this.m_SceneObject.World;
        Vector3 vector3_4 = Vector3.op_Multiply(((Matrix) ref world5).Up, num2);
        Vector3 vector3_5 = Vector3.op_Addition(vector3_3, vector3_4);
        ((Matrix) ref local).Translation = Vector3.op_Addition(translation2, vector3_5);
        g.m_ItemManager.m_Item[this.m_MuzzleIdx].m_SceneObject.World = matrix;
      }
      else
      {
        this.m_WeaponFlash.Enabled = false;
        if (this.m_MuzzleIdx == -1)
          return;
        g.m_ItemManager.m_Item[this.m_MuzzleIdx].m_SceneObject.Visibility = ObjectVisibility.None;
      }
    }

    public void Show()
    {
      if (this.m_SceneObject != null)
        this.m_SceneObject.Visibility = ObjectVisibility.Rendered;
      if (this.m_MagazineSceneObject != null && this.m_Player != null && this.m_Player.IsLocalPlayer() && !this.m_Player.m_Bot)
        this.m_MagazineSceneObject.Visibility = ObjectVisibility.Rendered;
      this.m_WeaponTimer = (float) g.m_App.m_GameTime.TotalGameTime.TotalMilliseconds + 300f;
    }

    public void Hide()
    {
      if (this.m_SceneObject != null)
        this.m_SceneObject.Visibility = ObjectVisibility.None;
      if (this.m_MagazineSceneObject != null)
        this.m_MagazineSceneObject.Visibility = ObjectVisibility.None;
      if (this.m_MuzzleIdx != -1 && g.m_ItemManager.m_Item[this.m_MuzzleIdx].m_SceneObject != null)
        g.m_ItemManager.m_Item[this.m_MuzzleIdx].m_SceneObject.Visibility = ObjectVisibility.None;
      this.m_ReloadState = Item.RELOAD.None;
      this.m_WeaponAmmoToGive = 0;
      if (this.m_WeaponFlash != null)
      {
        this.m_WeaponFlash.Enabled = false;
        this.m_WeaponFlashCount = 0;
      }
      this.DisableWeaponVFX();
      switch (this.m_Type)
      {
        case 1:
          this.HideTorch();
          break;
        case 19:
          this.HideArm();
          break;
        case 22:
          this.HideSaw();
          break;
        case 25:
          this.HideScrewdriver();
          break;
      }
    }

    private void EnableWeaponVFX()
    {
      if (this.m_WeaponSmoke != null)
        this.m_WeaponSmoke.Enabled = true;
      for (int index = 0; index < 3; ++index)
      {
        if (this.m_WeaponBuckshot[index] != null)
          this.m_WeaponBuckshot[index].Enabled = true;
        if (this.m_WeaponSparks[index] != null)
          this.m_WeaponSparks[index].Enabled = true;
      }
    }

    private void DisableWeaponVFX()
    {
      if (this.m_WeaponSmoke != null)
      {
        this.m_WeaponSmoke.Enabled = false;
        this.m_WeaponSmoke.Visible = false;
      }
      for (int index = 0; index < 3; ++index)
      {
        if (this.m_WeaponBuckshot[index] != null)
        {
          this.m_WeaponBuckshot[index].Enabled = false;
          this.m_WeaponBuckshot[index].Visible = false;
        }
        if (this.m_WeaponSparks[index] != null)
        {
          this.m_WeaponSparks[index].Enabled = false;
          this.m_WeaponSparks[index].Visible = false;
        }
      }
    }

    public void AddAmmo(int ammo)
    {
      this.m_WeaponAmmo += ammo;
      int num = 0;
      switch (this.m_Type)
      {
        case 1:
          num = 100;
          break;
        case 11:
          num = 3;
          break;
        case 14:
          num = 100;
          break;
      }
      if (this.m_WeaponAmmo <= num)
        return;
      this.m_WeaponAmmo = num;
    }

    public void NetItemSetPosition(Vector3 pos)
    {
      Matrix world = this.m_SceneObject.World;
      ((Matrix) ref world).Translation = pos;
      this.m_SceneObject.World = world;
    }

    public int ClipSize()
    {
      switch (this.m_Type)
      {
        case 1:
          return 25;
        case 11:
          return 1;
        case 14:
          return 15;
        default:
          return 0;
      }
    }

    public void InitFog()
    {
      this.Show();
      this.m_DoorStartMtx = this.m_SceneObject.RenderableMeshes[0].MeshToObject;
      this.m_SceneObject.Visibility = ObjectVisibility.Rendered;
      this.m_SceneObject.UpdateType = UpdateType.Automatic;
    }

    public void ResetFog() => this.m_MaxFogAlpha = 1E-05f;

    public bool CanChangeFogBank() => (double) this.m_FogAlpha == 0.0;

    public void UpdateFog()
    {
      Matrix world = this.m_SceneObject.World;
      Vector3 translation = ((Matrix) ref world).Translation;
      translation.Y = -1f;
      ((Matrix) ref world).Translation = translation;
      this.m_SceneObject.World = world;
      this.m_CurrentAng += 1f / 800f;
      this.m_SceneObject.RenderableMeshes[0].MeshToObject = Matrix.op_Multiply(this.m_StartMtx, Matrix.CreateRotationY(this.m_CurrentAng));
      this.m_SceneObject.RenderableMeshes[1].MeshToObject = Matrix.op_Multiply(this.m_StartMtx, Matrix.CreateRotationY(-this.m_CurrentAng));
      Vector3 vector3 = Vector3.op_Subtraction(translation, g.m_PlayerManager.GetLocalPlayer().m_Position);
      vector3.Y = 0.0f;
      float num1 = ((Vector3) ref vector3).LengthSquared();
      float num2 = 1f;
      float num3 = 225f;
      float num4 = 256f;
      this.m_MaxFogAlpha = MathHelper.Lerp(this.m_MaxFogAlpha, num2, 0.05f);
      this.m_FogAlpha = 1f * this.m_MaxFogAlpha;
      if ((double) num1 < (double) Item.NEAR_DIST_END_SQ)
        this.m_FogAlpha = 0.0f;
      if ((double) num1 < (double) num3 && (double) num1 > (double) Item.NEAR_DIST_END_SQ)
        this.m_FogAlpha = (float) (((double) num1 - (double) Item.NEAR_DIST_END_SQ) / ((double) num3 - (double) Item.NEAR_DIST_END_SQ)) * this.m_MaxFogAlpha;
      else if ((double) num1 > (double) num4 && (double) num1 < (double) Item.FAR_DIST_END_SQ)
        this.m_FogAlpha = (float) (((double) Item.FAR_DIST_END_SQ - (double) num1) / ((double) Item.FAR_DIST_END_SQ - (double) num4)) * this.m_MaxFogAlpha;
      else if ((double) num1 > (double) Item.FAR_DIST_END_SQ)
        this.m_FogAlpha = 0.0f;
      (this.m_SceneObject.RenderableMeshes[0].Effect as DeferredObjectEffect).TransparencyAmount = this.m_FogAlpha;
      (this.m_SceneObject.RenderableMeshes[1].Effect as DeferredObjectEffect).TransparencyAmount = this.m_FogAlpha;
    }

    private void HideFog()
    {
    }

    public enum LOCKERSTATE
    {
      CLOSED,
      OPENING,
      OPEN,
    }

    public enum SAFESTATE
    {
      CLOSED,
      OPENING,
      OPEN,
    }

    public enum CRATESTATE
    {
      CLOSED,
      OPENING,
      OPEN,
    }

    public enum DOORSTATE
    {
      CLOSED,
      OPENING,
      CLOSING,
      OPEN,
    }

    public enum OBJ
    {
      SHOTGUN,
      TORCH,
      CROSSBOW,
      CROSSBOW_EMPTY,
      CRUCIFIX,
      STAKE,
      CLAWS,
      HELMET,
      MUZZLE1,
      CRATE,
      ARTIFACT,
      RPG,
      RIFLE_MAG,
      RPG_SHELL,
      PISTOL,
      PISTOL_MAG,
      DOOR,
      DOORBIO,
      SAFE,
      ARM,
      DOORSKULL,
      FOG,
      SAW,
      LOCKER,
      LOCKER_PW,
      SCREWDRIVER,
      DOORAIRLOCK,
      TABLET1,
      TABLET2,
      TABLET3,
      TABLET4,
      TABLET5,
      END,
    }

    public enum RELOAD
    {
      None,
      Start,
      ShellLoad,
      Shell,
      Ending,
      End,
      Empty,
      Reloading,
    }
  }
}
