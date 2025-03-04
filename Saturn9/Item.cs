using System.Collections.Generic;
using BEPUphysics;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.Entities;
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

namespace Saturn9;

public class Item
{
	public enum LOCKERSTATE
	{
		CLOSED,
		OPENING,
		OPEN
	}

	public enum SAFESTATE
	{
		CLOSED,
		OPENING,
		OPEN
	}

	public enum CRATESTATE
	{
		CLOSED,
		OPENING,
		OPEN
	}

	public enum DOORSTATE
	{
		CLOSED,
		OPENING,
		CLOSING,
		OPEN
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
		END
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
		Reloading
	}

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

	public LOCKERSTATE m_LockerState;

	private static float ARM_FIRE_RATE = 150f;

	private Matrix m_SafeStartMtx = Matrix.Identity;

	private float m_SafeCurrentAng;

	public SAFESTATE m_SafeState;

	private Matrix m_StartMtx = Matrix.Identity;

	private float m_CurrentAng;

	public CRATESTATE m_CrateState;

	public CrateSmokeParticleSystem m_CrateSmoke;

	private static float MAC10_FIRE_RATE = 150f;

	public bool m_CanSawArm;

	private static float SAW_FIRE_RATE = 150f;

	private Matrix m_DoorStartMtx = Matrix.Identity;

	public PointLight m_DoorLight;

	public SoundEffectInstance m_DoorSFX;

	public DOORSTATE m_DoorState;

	public CrateSmokeParticleSystem m_DoorSmoke;

	public Entity m_Collision;

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

	public RELOAD m_ReloadState;

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
		Show();
		m_LockerStartMtx = m_SceneObject.RenderableMeshes[1].MeshToObject;
		m_LockerState = LOCKERSTATE.CLOSED;
		foreach (RenderableMesh renderableMesh in m_SceneObject.RenderableMeshes)
		{
			BoundingBox meshBoundingBox = renderableMesh.MeshBoundingBox;
			renderableMesh.MeshBoundingBox = new BoundingBox(new Vector3(meshBoundingBox.Min.Z * 10f, meshBoundingBox.Min.Y * 10f, meshBoundingBox.Min.X * 10f), new Vector3(meshBoundingBox.Max.Z * 10f, meshBoundingBox.Max.Y * 10f, meshBoundingBox.Max.X * 10f));
		}
		m_SceneObject.CalculateBounds();
		if (m_Collision == null)
		{
			m_Collision = new Box(m_SceneObject.World.Translation + new Vector3(0f, 3f, 0f), 1.4f, 5f, 1.4f);
			m_Collision.OrientationMatrix = Matrix3X3.CreateFromMatrix(m_SceneObject.World);
			m_Collision.Tag = this;
			g.m_App.m_Space.Add(m_Collision);
		}
		else
		{
			m_Collision.Position = m_SceneObject.World.Translation + new Vector3(0f, 3f, 0f);
			m_Collision.OrientationMatrix = Matrix3X3.CreateFromMatrix(m_SceneObject.World);
		}
		m_Collision.IsAffectedByGravity = false;
		m_Collision.LinearVelocity = Vector3.Zero;
		m_Collision.AngularVelocity = Vector3.Zero;
	}

	public void UpdateLocker()
	{
		switch (m_LockerState)
		{
		case LOCKERSTATE.CLOSED:
		{
			Matrix lockerStartMtx = m_LockerStartMtx;
			lockerStartMtx.Translation = m_LockerStartMtx.Translation + m_LockerStartMtx.Left * 20f;
			m_SceneObject.RenderableMeshes[1].MeshToObject = lockerStartMtx;
			m_SceneObject.RenderableMeshes[2].MeshToObject = lockerStartMtx;
			m_SceneObject.RenderableMeshes[3].MeshToObject = lockerStartMtx;
			m_SceneObject.RenderableMeshes[4].MeshToObject = lockerStartMtx;
			m_SceneObject.RenderableMeshes[5].MeshToObject = lockerStartMtx;
			if (m_SceneObject.RenderableMeshes.Count > 6)
			{
				m_SceneObject.RenderableMeshes[6].MeshToObject = lockerStartMtx;
			}
			m_LockerCurrentAng = 0f;
			break;
		}
		case LOCKERSTATE.OPENING:
		{
			m_LockerCurrentAng = MathHelper.Lerp(m_LockerCurrentAng, 1.9f, 0.05f);
			Matrix matrix = Matrix.CreateRotationY(m_LockerCurrentAng);
			Matrix meshToObject = m_LockerStartMtx * matrix;
			meshToObject.Translation = m_LockerStartMtx.Translation + m_LockerStartMtx.Left * 20f;
			m_SceneObject.RenderableMeshes[1].MeshToObject = meshToObject;
			m_SceneObject.RenderableMeshes[2].MeshToObject = meshToObject;
			m_SceneObject.RenderableMeshes[3].MeshToObject = meshToObject;
			m_SceneObject.RenderableMeshes[4].MeshToObject = meshToObject;
			m_SceneObject.RenderableMeshes[5].MeshToObject = meshToObject;
			if (m_SceneObject.RenderableMeshes.Count > 6)
			{
				m_SceneObject.RenderableMeshes[6].MeshToObject = meshToObject;
			}
			break;
		}
		}
	}

	private void PlayLockerUseSFX()
	{
		g.m_SoundManager.Play3D(16, m_SceneObject.World.Translation);
	}

	public bool PeerUseLocker()
	{
		m_LockerState = LOCKERSTATE.OPENING;
		if (g.m_App.m_SurvivalMode)
		{
			m_WeaponTimer = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 10f;
		}
		else
		{
			m_WeaponTimer = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 3f;
		}
		return true;
	}

	private void HideLocker()
	{
	}

	public void InitArm()
	{
		m_WeaponTimer = 0f;
		InitWeaponFlash();
		m_SceneObject.Visibility = ObjectVisibility.RenderedAndCastShadows;
		m_WeaponAmmo = 25;
		m_WeaponAmmoInClip = 25;
		m_bShouldRicochetVFX = true;
		m_bShouldRicochetSFX = true;
	}

	public void UpdateArm()
	{
		UpdateWeaponFlash();
	}

	private bool FireArm()
	{
		if (m_WeaponTimer > (float)g.m_App.m_GameTime.TotalGameTime.TotalMilliseconds)
		{
			return false;
		}
		if (m_WeaponAmmoInClip == 0)
		{
			if (m_ReloadState == RELOAD.None)
			{
				m_WeaponTimer = (float)g.m_App.m_GameTime.TotalGameTime.TotalMilliseconds + ARM_FIRE_RATE;
			}
			return false;
		}
		m_WeaponTimer = (float)g.m_App.m_GameTime.TotalGameTime.TotalMilliseconds + 500f;
		return true;
	}

	private void PlayArmFireSFX()
	{
	}

	private bool SimulateFireArm()
	{
		Vector3 aimVector = m_Player.GetAimVector();
		StartSmokePuffAndFlash(aimVector);
		PlayArmFireSFX();
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
		Show();
		m_SafeStartMtx = m_SceneObject.RenderableMeshes[1].MeshToObject;
		m_SafeState = SAFESTATE.CLOSED;
		foreach (RenderableMesh renderableMesh in m_SceneObject.RenderableMeshes)
		{
			BoundingBox meshBoundingBox = renderableMesh.MeshBoundingBox;
			renderableMesh.MeshBoundingBox = new BoundingBox(new Vector3(meshBoundingBox.Min.Z * 100f, meshBoundingBox.Min.Y + 2.159f, meshBoundingBox.Min.X * 100f), new Vector3(meshBoundingBox.Max.Z * 100f, meshBoundingBox.Max.Y + 2.159f, meshBoundingBox.Max.X * 100f));
		}
		m_SceneObject.CalculateBounds();
	}

	public void UpdateSafe()
	{
		switch (m_SafeState)
		{
		case SAFESTATE.CLOSED:
		{
			Matrix safeStartMtx = m_SafeStartMtx;
			safeStartMtx.Translation = m_SafeStartMtx.Translation + m_SafeStartMtx.Left * -1.52f + m_SafeStartMtx.Forward * 0f + m_SafeStartMtx.Up * -0.88f;
			m_SceneObject.RenderableMeshes[1].MeshToObject = safeStartMtx;
			m_SceneObject.RenderableMeshes[2].MeshToObject = safeStartMtx;
			m_SafeCurrentAng = 0f;
			break;
		}
		case SAFESTATE.OPENING:
		{
			m_SafeCurrentAng = MathHelper.Lerp(m_SafeCurrentAng, -2.3f, 0.05f);
			Matrix matrix = Matrix.CreateRotationY(m_SafeCurrentAng);
			Matrix meshToObject = m_SafeStartMtx * matrix;
			meshToObject.Translation = m_SafeStartMtx.Translation + m_SafeStartMtx.Left * -1.52f + m_SafeStartMtx.Forward * 0f + m_SafeStartMtx.Up * -0.88f;
			m_SceneObject.RenderableMeshes[1].MeshToObject = meshToObject;
			m_SceneObject.RenderableMeshes[2].MeshToObject = meshToObject;
			break;
		}
		}
	}

	private void PlaySafeUseSFX()
	{
		g.m_SoundManager.Play3D(16, m_SceneObject.World.Translation);
	}

	public bool PeerUseSafe(int crateIdx, short playerNetId)
	{
		m_SafeState = SAFESTATE.OPENING;
		if (g.m_App.m_SurvivalMode)
		{
			m_WeaponTimer = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 10f;
		}
		else
		{
			m_WeaponTimer = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 3f;
		}
		return true;
	}

	private void HideSafe()
	{
	}

	public void InitCrate()
	{
		Show();
		m_StartMtx = m_SceneObject.RenderableMeshes[1].MeshToObject;
		if (m_CrateSmoke == null)
		{
			m_CrateSmoke = new CrateSmokeParticleSystem(g.m_App);
			m_CrateSmoke.Enabled = false;
			m_CrateSmoke.AutoInitialize(g.m_App.GraphicsDevice, g.m_App.Content, g.m_App.screenManager.SpriteBatch);
			g.m_App.m_ParticleSystemManager.AddParticleSystem(m_CrateSmoke);
		}
		m_CrateState = CRATESTATE.CLOSED;
	}

	public void UpdateCrate()
	{
		switch (m_CrateState)
		{
		case CRATESTATE.CLOSED:
		{
			Matrix startMtx = m_StartMtx;
			startMtx.Translation = m_StartMtx.Translation + m_StartMtx.Forward * -0.96f + m_StartMtx.Up * 0.9f;
			m_SceneObject.RenderableMeshes[1].MeshToObject = startMtx;
			m_CrateSmoke.Enabled = false;
			m_CrateSmoke.Visible = false;
			m_CurrentAng = 0f;
			break;
		}
		case CRATESTATE.OPENING:
		{
			m_CurrentAng = MathHelper.Lerp(m_CurrentAng, 0.3f, 0.1f);
			Matrix matrix = Matrix.CreateRotationX(m_CurrentAng);
			Matrix meshToObject = m_StartMtx * matrix;
			meshToObject.Translation = m_StartMtx.Translation + m_StartMtx.Forward * -1f + m_StartMtx.Up * 0.9f;
			m_SceneObject.RenderableMeshes[1].MeshToObject = meshToObject;
			if (m_WeaponTimer < (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds)
			{
				m_CrateState = CRATESTATE.CLOSED;
				g.m_SoundManager.Play3D(17, m_SceneObject.World.Translation);
			}
			break;
		}
		}
	}

	private void PlayCrateUseSFX()
	{
		g.m_SoundManager.Play3D(16, m_SceneObject.World.Translation);
		m_CrateSmoke.Enabled = true;
		m_CrateSmoke.Visible = true;
		m_CrateSmoke.Emitter.BurstTime = 1f;
		m_CrateSmoke.Emitter.PositionData.Position = m_SceneObject.World.Translation + new Vector3(0f, 1.2f, 0f);
		m_CrateSmoke.Emitter.PositionData.Velocity = new Vector3(0f, 0f, 0f);
		m_CrateSmoke.LerpEmittersPositionAndOrientationOnNextUpdate = false;
	}

	public bool PeerUseCrate(int crateIdx, short playerNetId)
	{
		PlayCrateUseSFX();
		m_CrateState = CRATESTATE.OPENING;
		if (g.m_App.m_SurvivalMode)
		{
			m_WeaponTimer = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 10f;
		}
		else
		{
			m_WeaponTimer = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 3f;
		}
		if (!g.m_App.m_SurvivalMode)
		{
			g.m_ItemManager.EmptyCrate(crateIdx);
			if (playerNetId == g.m_PlayerManager.GetLocalPlayer().m_NetId)
			{
				g.m_PlayerManager.GetLocalPlayer().SetUsedCrate(crateIdx);
			}
		}
		return true;
	}

	private void HideCrate()
	{
	}

	public void InitTorch()
	{
		m_WeaponTimer = 0f;
		InitWeaponFlash();
		m_WeaponAmmo = 25;
		m_WeaponAmmoInClip = 25;
		m_bShouldRicochetVFX = true;
		m_bShouldRicochetSFX = true;
	}

	public void UpdateTorch()
	{
		UpdateWeaponFlash();
	}

	private bool FireTorch()
	{
		if (m_WeaponTimer > (float)g.m_App.m_GameTime.TotalGameTime.TotalMilliseconds)
		{
			return false;
		}
		if (m_WeaponAmmoInClip == 0)
		{
			if (m_ReloadState == RELOAD.None)
			{
				m_WeaponTimer = (float)g.m_App.m_GameTime.TotalGameTime.TotalMilliseconds + MAC10_FIRE_RATE;
			}
			return false;
		}
		Vector3 aimPosition = m_Player.GetAimPosition();
		Vector3 aimVector = m_Player.GetAimVector();
		StartSmokePuffAndFlash(aimVector);
		Vector3 axis = DPSFHelper.RandomNormalizedVector();
		float degrees = 6f;
		if (m_bZoomed)
		{
			degrees = 4f;
		}
		axis = Vector3.Transform(aimVector, Quaternion.CreateFromAxisAngle(axis, (float)((g.m_App.m_Rand.NextDouble() - 0.5) * (double)MathHelper.ToRadians(degrees))));
		DoWeaponRayCast(aimPosition, axis, 101.6f, 15);
		PlayTorchFireSFX();
		m_WeaponAmmoInClip--;
		m_WeaponTimer = (float)g.m_App.m_GameTime.TotalGameTime.TotalMilliseconds + MAC10_FIRE_RATE;
		if (!m_Player.m_Bot)
		{
			g.m_App.m_RumbleFrames = 3;
		}
		return true;
	}

	private void PlayTorchFireSFX()
	{
	}

	private bool SimulateFireTorch()
	{
		Vector3 aimVector = m_Player.GetAimVector();
		StartSmokePuffAndFlash(aimVector);
		PlayTorchFireSFX();
		return true;
	}

	private void HideTorch()
	{
	}

	private void ZoomTorch(bool bZoom)
	{
		if (bZoom)
		{
			g.m_CameraManager.SetTargetFov(30f);
		}
		else
		{
			g.m_CameraManager.SetDefaultFov();
		}
	}

	public void InitSaw()
	{
		m_WeaponTimer = 0f;
		InitWeaponFlash();
		m_WeaponAmmo = 25;
		m_WeaponAmmoInClip = 25;
		m_CanSawArm = false;
		m_bShouldRicochetVFX = true;
		m_bShouldRicochetSFX = true;
	}

	public void UpdateSaw()
	{
		UpdateWeaponFlash();
	}

	private bool FireSaw()
	{
		if (m_WeaponTimer > (float)g.m_App.m_GameTime.TotalGameTime.TotalMilliseconds)
		{
			return false;
		}
		if (m_WeaponAmmoInClip == 0)
		{
			if (m_ReloadState == RELOAD.None)
			{
				m_WeaponTimer = (float)g.m_App.m_GameTime.TotalGameTime.TotalMilliseconds + SAW_FIRE_RATE;
			}
			return false;
		}
		if (m_CanSawArm)
		{
			m_Player.SawArm();
		}
		m_WeaponTimer = (float)g.m_App.m_GameTime.TotalGameTime.TotalMilliseconds + 500f;
		return true;
	}

	private void PlaySawFireSFX()
	{
	}

	private bool SimulateFireSaw()
	{
		Vector3 aimVector = m_Player.GetAimVector();
		StartSmokePuffAndFlash(aimVector);
		PlaySawFireSFX();
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
		Show();
		m_DoorStartMtx = m_SceneObject.RenderableMeshes[0].MeshToObject;
		m_DoorStartY = m_SceneObject.World.Translation.Y;
		if (m_DoorSmoke == null)
		{
			m_DoorSmoke = new CrateSmokeParticleSystem(g.m_App);
			m_DoorSmoke.Enabled = false;
			m_DoorSmoke.AutoInitialize(g.m_App.GraphicsDevice, g.m_App.Content, g.m_App.screenManager.SpriteBatch);
			g.m_App.m_ParticleSystemManager.AddParticleSystem(m_DoorSmoke);
		}
		m_DoorState = DOORSTATE.CLOSED;
		if (m_Collision == null)
		{
			m_Collision = new Box(m_SceneObject.World.Translation + new Vector3(0f, 3f, 0f), 5f, 5f, 1f);
			m_Collision.OrientationMatrix = Matrix3X3.CreateFromMatrix(m_SceneObject.World);
			m_Collision.Tag = this;
		}
		else
		{
			m_Collision.Position = m_SceneObject.World.Translation + new Vector3(0f, 3f, 0f);
			m_Collision.OrientationMatrix = Matrix3X3.CreateFromMatrix(m_SceneObject.World);
		}
		g.m_App.m_Space.Add(m_Collision);
		m_Collision.IsAffectedByGravity = false;
		m_Collision.LinearVelocity = Vector3.Zero;
		m_Collision.AngularVelocity = Vector3.Zero;
		if (m_DoorLight != null)
		{
			Matrix world = m_DoorLight.World;
			Vector3 translation = world.Translation;
			translation.Y = m_DoorLightStartY;
			world.Translation = translation;
			m_DoorLight.World = world;
			m_DoorLight = null;
			m_DoorLightStartY = 0f;
		}
	}

	public void UpdateDoor()
	{
		switch (m_DoorState)
		{
		case DOORSTATE.CLOSED:
			m_DoorSmoke.Enabled = false;
			m_DoorSmoke.Visible = false;
			break;
		case DOORSTATE.OPENING:
		{
			Matrix world2 = m_SceneObject.World;
			Vector3 translation2 = m_SceneObject.World.Translation;
			float num2 = 0.02f * (30f * (float)g.m_App.m_GameTime.ElapsedGameTime.TotalSeconds);
			if (translation2.Y < 1.4f)
			{
				translation2.Y += num2;
				if (m_DoorSFX != null)
				{
					g.m_SoundManager.UpdateTrackedSound(m_DoorSFX, m_SceneObject.World.Translation);
				}
				if (m_DoorLight != null)
				{
					if (m_DoorLightStartY == 0f)
					{
						m_DoorLightStartY = m_DoorLight.World.Translation.Y;
					}
					Matrix world3 = m_DoorLight.World;
					Vector3 translation3 = m_DoorLight.World.Translation;
					translation3.Y += num2;
					world3.Translation = translation3;
					m_DoorLight.World = world3;
				}
			}
			world2.Translation = translation2;
			m_SceneObject.World = world2;
			m_Collision.Position = translation2 + new Vector3(0f, 3f, 0f);
			break;
		}
		case DOORSTATE.CLOSING:
		{
			Matrix world = m_SceneObject.World;
			Vector3 translation = m_SceneObject.World.Translation;
			float num = 0.02f * (30f * (float)g.m_App.m_GameTime.ElapsedGameTime.TotalSeconds);
			if (translation.Y > m_DoorStartY)
			{
				translation.Y -= num;
				if (m_DoorSFX != null)
				{
					g.m_SoundManager.UpdateTrackedSound(m_DoorSFX, m_SceneObject.World.Translation);
				}
			}
			else
			{
				m_DoorState = DOORSTATE.CLOSED;
			}
			world.Translation = translation;
			m_SceneObject.World = world;
			m_Collision.Position = translation + new Vector3(0f, 3f, 0f);
			break;
		}
		}
	}

	private void PlayDoorUseSFX()
	{
		g.m_SoundManager.Play3D(16, m_SceneObject.World.Translation);
		m_DoorSFX = g.m_SoundManager.Play3D(29, m_SceneObject.World.Translation);
		m_DoorSmoke.Enabled = true;
		m_DoorSmoke.Visible = true;
		m_DoorSmoke.Emitter.BurstTime = 1f;
		m_DoorSmoke.Emitter.PositionData.Position = m_SceneObject.World.Translation + new Vector3(0f, 1.2f, 0f);
		m_DoorSmoke.Emitter.PositionData.Velocity = new Vector3(0f, 0f, 0f);
		m_DoorSmoke.LerpEmittersPositionAndOrientationOnNextUpdate = false;
	}

	public bool PeerUseDoor(short playerNetId)
	{
		PlayDoorUseSFX();
		m_DoorState = DOORSTATE.OPENING;
		return true;
	}

	public void PeerCloseDoor()
	{
		if (m_DoorState != 0)
		{
			m_DoorState = DOORSTATE.CLOSING;
			m_DoorSFX = g.m_SoundManager.Play3D(29, m_SceneObject.World.Translation);
		}
	}

	private void HideDoor()
	{
	}

	public void InitScrewdriver()
	{
		m_WeaponTimer = 0f;
		InitWeaponFlash();
		m_SceneObject.Visibility = ObjectVisibility.RenderedAndCastShadows;
		m_WeaponAmmo = 25;
		m_WeaponAmmoInClip = 25;
		m_bShouldRicochetVFX = true;
		m_bShouldRicochetSFX = true;
	}

	public void UpdateScrewdriver()
	{
		UpdateWeaponFlash();
	}

	private bool FireScrewdriver()
	{
		if (m_WeaponTimer > (float)g.m_App.m_GameTime.TotalGameTime.TotalMilliseconds)
		{
			return false;
		}
		if (m_WeaponAmmoInClip == 0)
		{
			if (m_ReloadState == RELOAD.None)
			{
				m_WeaponTimer = (float)g.m_App.m_GameTime.TotalGameTime.TotalMilliseconds + SCREWDRIVER_FIRE_RATE;
			}
			return false;
		}
		m_Player.OpenLocker();
		m_WeaponTimer = (float)g.m_App.m_GameTime.TotalGameTime.TotalMilliseconds + 1200f;
		return true;
	}

	private void PlayScrewdriverFireSFX()
	{
	}

	private bool SimulateFireScrewdriver()
	{
		Vector3 aimVector = m_Player.GetAimVector();
		StartSmokePuffAndFlash(aimVector);
		PlayScrewdriverFireSFX();
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
		m_Type = -1;
		m_Id = -1;
		m_NetId = byte.MaxValue;
		m_Model = null;
		m_SceneObject = null;
		m_Player = null;
		m_VfxSystemIdx = 0;
		m_WeaponBuckshot = new BuckshotQuadSprayParticleSystem[3];
		m_WeaponSparks = new ExplosionFlyingSparksParticleSystem[3];
		m_WeaponSmoke = null;
		for (int i = 0; i < 3; i++)
		{
			m_WeaponBuckshot[i] = null;
			m_WeaponSparks[i] = null;
		}
	}

	public void Update()
	{
		switch (m_Type)
		{
		case 1:
			if (ActiveWeapon())
			{
				UpdateTorch();
			}
			break;
		case 22:
			if (ActiveWeapon())
			{
				UpdateSaw();
			}
			break;
		case 19:
			if (ActiveWeapon())
			{
				UpdateArm();
			}
			break;
		case 25:
			if (ActiveWeapon())
			{
				UpdateScrewdriver();
			}
			break;
		case 16:
		case 17:
		case 20:
		case 26:
			UpdateDoor();
			break;
		case 18:
			UpdateSafe();
			break;
		case 23:
		case 24:
			UpdateLocker();
			break;
		case 21:
			UpdateFog();
			break;
		}
	}

	private bool ActiveWeapon()
	{
		if (m_Player == null)
		{
			return false;
		}
		if (m_Player.m_Health <= 0)
		{
			return false;
		}
		if (m_Player.m_WeaponItemIndex == m_Id)
		{
			return true;
		}
		return false;
	}

	public void Delete()
	{
		m_Id = -1;
		m_NetId = byte.MaxValue;
		DisableWeaponVFX();
		if (m_SceneObject != null)
		{
			g.m_App.sceneInterface.ObjectManager.Remove(m_SceneObject);
			m_SceneObject = null;
		}
		if (m_MagazineSceneObject != null)
		{
			g.m_App.sceneInterface.ObjectManager.Remove(m_MagazineSceneObject);
			m_MagazineSceneObject = null;
		}
		if (m_WeaponFlash != null)
		{
			g.m_App.sceneInterface.LightManager.Remove(m_WeaponFlash);
			m_WeaponFlash = null;
			m_WeaponFlashCount = 0;
		}
		if (m_MuzzleIdx != -1 && g.m_ItemManager.m_Item[m_MuzzleIdx].m_Id != -1)
		{
			g.m_ItemManager.Delete(m_MuzzleIdx);
			m_MuzzleIdx = -1;
		}
		if (m_Collision != null)
		{
			g.m_App.m_Space.Remove(m_Collision);
			m_Collision = null;
		}
	}

	public bool Fire(bool bDebounced)
	{
		if (m_Player.m_CurrentViewAnim == 2)
		{
			return false;
		}
		bool result = false;
		switch (m_Type)
		{
		case 22:
			if (!bDebounced)
			{
				return false;
			}
			result = FireSaw();
			break;
		case 25:
			if (!bDebounced)
			{
				return false;
			}
			result = FireScrewdriver();
			break;
		}
		return result;
	}

	public bool SimulateFire()
	{
		bool result = false;
		switch (m_Type)
		{
		case 1:
			result = SimulateFireTorch();
			break;
		}
		return result;
	}

	public void DoWeaponRayCast(Vector3 position, Vector3 direction, float range, int damage)
	{
		List<RayCastResult> list = new List<RayCastResult>();
		Ray ray = new Ray(position, direction);
		if (!g.m_App.m_Space.RayCast(ray, range, list))
		{
			return;
		}
		list.Sort();
		Player player = null;
		int num = 255;
		for (int i = 0; i < list.Count; i++)
		{
			if (!(list[i].HitObject is EntityCollidable entityCollidable))
			{
				Vector3 location = list[i].HitData.Location;
				Vector3 normal = list[i].HitData.Normal;
				normal.Normalize();
				DoImpactVFXSFX(location, normal);
				m_Player.m_RequestSendImpact = true;
				m_Player.m_RequestSendImpactPos = location;
				m_Player.m_RequestSendImpactNormal = normal;
				break;
			}
			if (entityCollidable.Entity.Tag is HitTag hitTag)
			{
				if (g.m_PlayerManager.m_Player[hitTag.m_PlayerId].m_Id != -1 && hitTag.m_PlayerId != m_Player.m_Id)
				{
					player = g.m_PlayerManager.m_Player[hitTag.m_PlayerId];
				}
				if (player != null)
				{
					if (hitTag.m_HitZone == 1)
					{
						num = 1;
					}
					else if (hitTag.m_HitZone == 2)
					{
						num = 2;
					}
					else if (hitTag.m_HitZone == 3)
					{
						num = 3;
					}
				}
			}
			if (player != null && num != 255)
			{
				m_Player.RequestDamageOther(num, damage, player, 255);
				break;
			}
		}
	}

	public void DoImpactVFXSFX(Vector3 pos, Vector3 normal)
	{
	}

	public void DrawCrosshair(SpriteBatch spriteBatch)
	{
		switch (m_Type)
		{
		case 1:
			spriteBatch.Draw(g.m_ItemManager.m_Mac10Crosshair, centre - spriteHalfSize, Color.White);
			break;
		case 11:
			spriteBatch.Draw(g.m_ItemManager.m_RPGCrosshair, centre - new Vector2(64f, 64f), Color.White);
			break;
		case 14:
			spriteBatch.Draw(g.m_ItemManager.m_ShotgunCrosshair, centre - spriteHalfSize, Color.White);
			break;
		}
	}

	public void StartSmokePuffAndFlash(Vector3 direction)
	{
	}

	public bool IsZoomed()
	{
		return m_bZoomed;
	}

	public void InitWeaponFlash()
	{
		m_WeaponFlash = new PointLight();
		m_WeaponFlash.LightingType = LightingType.RealTime;
		m_WeaponFlash.DiffuseColor = new Vector3(10f, 10f, 8f);
		m_WeaponFlash.Intensity = 0.1f;
		m_WeaponFlash.Radius = 20f;
		m_WeaponFlash.FalloffStrength = 1f;
		m_WeaponFlash.Enabled = false;
		m_WeaponFlash.ShadowType = ShadowType.None;
		m_WeaponFlash.UpdateType = UpdateType.Automatic;
		g.m_App.sceneInterface.LightManager.Submit(m_WeaponFlash);
		m_WeaponFlashCount = 0;
	}

	public void UpdateWeaponFlash()
	{
		if (!m_WeaponFlash.Enabled)
		{
			return;
		}
		if (m_WeaponFlashCount > 0)
		{
			m_WeaponFlashCount--;
			m_WeaponFlash.Position = m_SceneObject.World.Translation + m_SceneObject.World.Forward * 0.5f;
			if (m_MuzzleIdx != -1)
			{
				g.m_ItemManager.m_Item[m_MuzzleIdx].m_SceneObject.Visibility = ObjectVisibility.Rendered;
				Matrix world = m_SceneObject.World;
				Matrix matrix = Matrix.CreateRotationZ((float)g.m_App.m_Rand.NextDouble() * 3.14f);
				world = matrix * world;
				float num = 1.4f;
				float num2 = 0.1f;
				int type = m_Type;
				if (type == 14)
				{
					num = 0.5f;
					num2 = 0.2f;
				}
				world.Translation += m_SceneObject.World.Forward * num + m_SceneObject.World.Up * num2;
				g.m_ItemManager.m_Item[m_MuzzleIdx].m_SceneObject.World = world;
			}
		}
		else
		{
			m_WeaponFlash.Enabled = false;
			if (m_MuzzleIdx != -1)
			{
				g.m_ItemManager.m_Item[m_MuzzleIdx].m_SceneObject.Visibility = ObjectVisibility.None;
			}
		}
	}

	public void Show()
	{
		if (m_SceneObject != null)
		{
			m_SceneObject.Visibility = ObjectVisibility.Rendered;
		}
		if (m_MagazineSceneObject != null && m_Player != null && m_Player.IsLocalPlayer() && !m_Player.m_Bot)
		{
			m_MagazineSceneObject.Visibility = ObjectVisibility.Rendered;
		}
		m_WeaponTimer = (float)g.m_App.m_GameTime.TotalGameTime.TotalMilliseconds + 300f;
	}

	public void Hide()
	{
		if (m_SceneObject != null)
		{
			m_SceneObject.Visibility = ObjectVisibility.None;
		}
		if (m_MagazineSceneObject != null)
		{
			m_MagazineSceneObject.Visibility = ObjectVisibility.None;
		}
		if (m_MuzzleIdx != -1 && g.m_ItemManager.m_Item[m_MuzzleIdx].m_SceneObject != null)
		{
			g.m_ItemManager.m_Item[m_MuzzleIdx].m_SceneObject.Visibility = ObjectVisibility.None;
		}
		m_ReloadState = RELOAD.None;
		m_WeaponAmmoToGive = 0;
		if (m_WeaponFlash != null)
		{
			m_WeaponFlash.Enabled = false;
			m_WeaponFlashCount = 0;
		}
		DisableWeaponVFX();
		switch (m_Type)
		{
		case 1:
			HideTorch();
			break;
		case 22:
			HideSaw();
			break;
		case 19:
			HideArm();
			break;
		case 25:
			HideScrewdriver();
			break;
		}
	}

	private void EnableWeaponVFX()
	{
		if (m_WeaponSmoke != null)
		{
			m_WeaponSmoke.Enabled = true;
		}
		for (int i = 0; i < 3; i++)
		{
			if (m_WeaponBuckshot[i] != null)
			{
				m_WeaponBuckshot[i].Enabled = true;
			}
			if (m_WeaponSparks[i] != null)
			{
				m_WeaponSparks[i].Enabled = true;
			}
		}
	}

	private void DisableWeaponVFX()
	{
		if (m_WeaponSmoke != null)
		{
			m_WeaponSmoke.Enabled = false;
			m_WeaponSmoke.Visible = false;
		}
		for (int i = 0; i < 3; i++)
		{
			if (m_WeaponBuckshot[i] != null)
			{
				m_WeaponBuckshot[i].Enabled = false;
				m_WeaponBuckshot[i].Visible = false;
			}
			if (m_WeaponSparks[i] != null)
			{
				m_WeaponSparks[i].Enabled = false;
				m_WeaponSparks[i].Visible = false;
			}
		}
	}

	public void AddAmmo(int ammo)
	{
		m_WeaponAmmo += ammo;
		int num = 0;
		switch (m_Type)
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
		if (m_WeaponAmmo > num)
		{
			m_WeaponAmmo = num;
		}
	}

	public void NetItemSetPosition(Vector3 pos)
	{
		Matrix world = m_SceneObject.World;
		world.Translation = pos;
		m_SceneObject.World = world;
	}

	public int ClipSize()
	{
		return m_Type switch
		{
			1 => 25, 
			14 => 15, 
			11 => 1, 
			_ => 0, 
		};
	}

	public void InitFog()
	{
		Show();
		m_DoorStartMtx = m_SceneObject.RenderableMeshes[0].MeshToObject;
		m_SceneObject.Visibility = ObjectVisibility.Rendered;
		m_SceneObject.UpdateType = UpdateType.Automatic;
	}

	public void ResetFog()
	{
		m_MaxFogAlpha = 1E-05f;
	}

	public bool CanChangeFogBank()
	{
		if (m_FogAlpha == 0f)
		{
			return true;
		}
		return false;
	}

	public void UpdateFog()
	{
		Matrix world = m_SceneObject.World;
		Vector3 translation = world.Translation;
		translation.Y = -1f;
		world.Translation = translation;
		m_SceneObject.World = world;
		m_CurrentAng += 0.00125f;
		Matrix matrix = Matrix.CreateRotationY(m_CurrentAng);
		Matrix meshToObject = m_StartMtx * matrix;
		m_SceneObject.RenderableMeshes[0].MeshToObject = meshToObject;
		matrix = Matrix.CreateRotationY(0f - m_CurrentAng);
		meshToObject = m_StartMtx * matrix;
		m_SceneObject.RenderableMeshes[1].MeshToObject = meshToObject;
		Vector3 vector = translation - g.m_PlayerManager.GetLocalPlayer().m_Position;
		vector.Y = 0f;
		float num = vector.LengthSquared();
		float value = 1f;
		float num2 = 225f;
		float num3 = 256f;
		m_MaxFogAlpha = MathHelper.Lerp(m_MaxFogAlpha, value, 0.05f);
		m_FogAlpha = 1f * m_MaxFogAlpha;
		if (num < NEAR_DIST_END_SQ)
		{
			m_FogAlpha = 0f;
		}
		if (num < num2 && num > NEAR_DIST_END_SQ)
		{
			m_FogAlpha = (num - NEAR_DIST_END_SQ) / (num2 - NEAR_DIST_END_SQ) * m_MaxFogAlpha;
		}
		else if (num > num3 && num < FAR_DIST_END_SQ)
		{
			m_FogAlpha = (FAR_DIST_END_SQ - num) / (FAR_DIST_END_SQ - num3) * m_MaxFogAlpha;
		}
		else if (num > FAR_DIST_END_SQ)
		{
			m_FogAlpha = 0f;
		}
		DeferredObjectEffect deferredObjectEffect = m_SceneObject.RenderableMeshes[0].Effect as DeferredObjectEffect;
		deferredObjectEffect.TransparencyAmount = m_FogAlpha;
		deferredObjectEffect = m_SceneObject.RenderableMeshes[1].Effect as DeferredObjectEffect;
		deferredObjectEffect.TransparencyAmount = m_FogAlpha;
	}

	private void HideFog()
	{
	}
}
