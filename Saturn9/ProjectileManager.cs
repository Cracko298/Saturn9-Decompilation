using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.MathExtensions;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SynapseGaming.LightingSystem.Collision;
using SynapseGaming.LightingSystem.Core;
using SynapseGaming.LightingSystem.Lights;
using SynapseGaming.LightingSystem.Rendering;

namespace Saturn9;

public class ProjectileManager
{
	public const int MAX_PROJECTILES = 8;

	public Projectile[] m_Projectile;

	public Model[] m_Model;

	private int m_NextId;

	private byte m_ProjectileNetId;

	public ProjectileManager()
	{
		m_Projectile = new Projectile[8];
		for (int i = 0; i < 8; i++)
		{
			m_Projectile[i] = new Projectile();
		}
		m_Model = new Model[1];
	}

	public int Create(int type, Matrix world, Vector3 vel, Player player, short netId)
	{
		bool flag = false;
		int num = -1;
		if (m_NextId >= 8)
		{
			m_NextId = 0;
		}
		for (int i = m_NextId; i < 8; i++)
		{
			if (m_Projectile[i].m_Id == -1)
			{
				num = i;
				flag = true;
				m_NextId = i + 1;
				break;
			}
		}
		if (!flag)
		{
			for (int j = 0; j < 8; j++)
			{
				if (m_Projectile[j].m_Id == -1)
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
		m_Projectile[num].m_Type = type;
		m_Projectile[num].m_Id = num;
		m_Projectile[num].m_Player = player;
		m_Projectile[num].m_VfxSystemIdx = 0;
		m_Projectile[num].m_State = Projectile.PROJECTILE_STATE.INFLIGHT;
		m_Projectile[num].m_ExplodeTimer = 0f;
		m_Projectile[num].m_Velocity = vel;
		if (m_Projectile[num].m_Collision == null)
		{
			m_Projectile[num].m_Collision = new Capsule(world.Translation, 2f, 0.25f, 1f);
			m_Projectile[num].m_Collision.OrientationMatrix = Matrix3X3.CreateFromMatrix(Matrix.CreateRotationX(MathHelper.ToRadians(90f)) * world);
			m_Projectile[num].m_Collision.Tag = m_Projectile[num];
		}
		else
		{
			m_Projectile[num].m_Collision.Position = world.Translation;
			m_Projectile[num].m_Collision.OrientationMatrix = Matrix3X3.CreateFromMatrix(Matrix.CreateRotationX(MathHelper.ToRadians(90f)) * world);
		}
		m_Projectile[num].m_Collision.CollisionInformation.Events.InitialCollisionDetected += Events_InitialCollision;
		g.m_App.m_Space.Add(m_Projectile[num].m_Collision);
		m_Projectile[num].m_Collision.IsAffectedByGravity = false;
		m_Projectile[num].m_Collision.LinearVelocity = vel;
		m_Projectile[num].m_Collision.AngularVelocity = Vector3.Zero;
		m_Projectile[num].m_Collision.CollisionInformation.CollisionRules.Personal = CollisionRule.NoSolver;
		if (m_Model[type] == null)
		{
			Delete(num);
			return -1;
		}
		m_Projectile[num].m_Model = m_Model[type];
		m_Projectile[num].m_SceneObject = new SceneObject(m_Projectile[num].m_Model)
		{
			UpdateType = UpdateType.Automatic,
			Visibility = ObjectVisibility.Rendered,
			StaticLightingType = StaticLightingType.Composite,
			CollisionType = CollisionType.None,
			AffectedByGravity = false,
			Name = $"Projectile{num}",
			World = world
		};
		g.m_App.sceneInterface.ObjectManager.Submit(m_Projectile[num].m_SceneObject);
		if (m_Projectile[num].m_Type == 0)
		{
			if (m_Projectile[num].m_Explosion == null)
			{
				m_Projectile[num].m_Trail = g.m_ItemManager.GetCachedRPGTrail();
				if (m_Projectile[num].m_Trail == null)
				{
					m_Projectile[num].m_Trail = new RPGShellTrailParticleSystem(g.m_App);
					m_Projectile[num].m_Trail.Enabled = false;
					m_Projectile[num].m_Trail.AutoInitialize(g.m_App.GraphicsDevice, g.m_App.Content, g.m_App.screenManager.SpriteBatch);
				}
				g.m_App.m_ParticleSystemManager.AddParticleSystem(m_Projectile[num].m_Trail);
			}
			if (m_Projectile[num].m_Explosion == null)
			{
				m_Projectile[num].m_Explosion = g.m_ItemManager.GetCachedRPGExplosion();
				if (m_Projectile[num].m_Explosion == null)
				{
					m_Projectile[num].m_Explosion = new AnimatedQuadParticleSystem(g.m_App);
					m_Projectile[num].m_Explosion.Enabled = false;
					m_Projectile[num].m_Explosion.AutoInitialize(g.m_App.GraphicsDevice, g.m_App.Content, g.m_App.screenManager.SpriteBatch);
				}
				g.m_App.m_ParticleSystemManager.AddParticleSystem(m_Projectile[num].m_Explosion);
			}
			m_Projectile[num].InitProjectileLight();
			m_Projectile[num].InitExplodeFlash();
			m_Projectile[num].InitTrail();
		}
		if (netId == 255)
		{
			m_Projectile[num].m_NetId = GetNextNetId(num);
			m_Projectile[num].m_Player.m_RequestCreateProjectile = true;
			m_Projectile[num].m_Player.m_RequestCreateProjectileNetId = m_Projectile[num].m_NetId;
			m_Projectile[num].m_Player.m_RequestCreateProjectileType = (byte)type;
			m_Projectile[num].m_Player.m_RequestCreateProjectilePosition = world.Translation;
			Quaternion requestCreateProjectileQuaterion = Quaternion.CreateFromRotationMatrix(world);
			m_Projectile[num].m_Player.m_RequestCreateProjectileQuaterion = requestCreateProjectileQuaterion;
			m_Projectile[num].m_Player.m_RequestCreateProjectileVeclocity = m_Projectile[num].m_Velocity;
		}
		else
		{
			m_Projectile[num].m_NetId = netId;
		}
		if (flag)
		{
			return num;
		}
		return -1;
	}

	private short GetNextNetId(int id)
	{
		m_ProjectileNetId++;
		if (m_ProjectileNetId > 120)
		{
			m_ProjectileNetId = 0;
		}
		return (short)((m_ProjectileNetId << 8) | (m_Projectile[id].m_Player.m_NetId & 0xFF));
	}

	public Projectile GetProjectileByNetId(short projectileNetId)
	{
		for (int i = 0; i < 8; i++)
		{
			if (m_Projectile[i].m_NetId == projectileNetId)
			{
				return m_Projectile[i];
			}
		}
		return null;
	}

	private void Events_InitialCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
	{
		Projectile projectile = sender.Entity.Tag as Projectile;
		if (projectile.m_Id != -1)
		{
			projectile.Explode(projectile.m_SceneObject.World.Translation);
		}
	}

	public void DeleteAll()
	{
		m_NextId = 0;
		for (int i = 0; i < 8; i++)
		{
			if (m_Projectile[i].m_Id != -1)
			{
				m_Projectile[i].Delete();
			}
		}
		m_ProjectileNetId = 0;
	}

	public void Delete(int id)
	{
		m_Projectile[id].Delete();
	}

	public void Update()
	{
		for (int i = 0; i < 8; i++)
		{
			if (m_Projectile[i].m_Id != -1)
			{
				m_Projectile[i].Update();
			}
		}
	}

	public void LoadContent(ContentManager Content)
	{
	}
}
