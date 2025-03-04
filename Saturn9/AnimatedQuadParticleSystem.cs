using DPSF;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Saturn9;

public class AnimatedQuadParticleSystem : DefaultAnimatedTexturedQuadParticleSystem
{
	private Animations mcAnimation;

	private Vector3 msBoxMin = new Vector3(-100f, 0f, -100f);

	private Vector3 msBoxMax = new Vector3(100f, 100f, 100f);

	public AnimatedQuadParticleSystem(Game cGame)
		: base(cGame)
	{
	}

	protected override void AfterInitialize()
	{
		base.AfterInitialize();
		mcAnimation = new Animations();
		int[] array = new int[48];
		for (int i = 0; i < 48; i++)
		{
			array[i] = i;
		}
		mcAnimation.CreatePicturesFromTileSet(48, 8, new Rectangle(0, 0, 128, 170));
		int currentAnimationID = mcAnimation.CreateAnimation(array, 0.03f, 1);
		mcAnimation.CurrentAnimationID = currentAnimationID;
	}

	protected override void AfterDestroy()
	{
		base.AfterDestroy();
		mcAnimation = null;
	}

	protected override void InitializeRenderProperties()
	{
		base.InitializeRenderProperties();
		base.RenderProperties.RasterizerState.CullMode = CullMode.None;
		base.RenderProperties.DepthStencilState.DepthBufferWriteEnable = true;
	}

	public override void AutoInitialize(GraphicsDevice cGraphicsDevice, ContentManager cContentManager, SpriteBatch cSpriteBatch)
	{
		InitializeTexturedQuadParticleSystem(cGraphicsDevice, cContentManager, 100, 500, UpdateVertexProperties, "Sprites/Explosion1Spritesheet");
		base.Name = "Animated Textured Quads";
		LoadEvents();
	}

	public void LoadEvents()
	{
		base.ParticleInitializationFunction = InitializeParticleAnimated;
		base.ParticleEvents.RemoveAllEvents();
		base.ParticleEvents.AddEveryTimeEvent(base.UpdateParticlePositionAndVelocityUsingAcceleration);
		base.ParticleEvents.AddEveryTimeEvent(base.UpdateParticleRotationAndRotationalVelocityUsingRotationalAcceleration);
		base.ParticleEvents.AddEveryTimeEvent(base.UpdateParticleAnimationAndTextureCoordinates);
		base.ParticleEvents.AddEveryTimeEvent(base.UpdateParticleToFaceTheCamera, 200);
		base.Emitter.ParticlesPerSecond = 48f;
		base.Emitter.EmitParticlesAutomatically = false;
		base.Emitter.PositionData.Position = new Vector3(0f, 50f, 0f);
		base.InitialProperties.LifetimeMin = mcAnimation.TimeRequiredToPlayCurrentAnimation;
		base.InitialProperties.LifetimeMax = mcAnimation.TimeRequiredToPlayCurrentAnimation;
		base.InitialProperties.PositionMin = Vector3.Zero;
		base.InitialProperties.PositionMax = Vector3.Zero;
		base.InitialProperties.VelocityMin = Vector3.Zero;
		base.InitialProperties.VelocityMax = Vector3.Zero;
		base.InitialProperties.AccelerationMin = Vector3.Zero;
		base.InitialProperties.AccelerationMax = Vector3.Zero;
		base.InitialProperties.FrictionMin = 0f;
		base.InitialProperties.FrictionMax = 0f;
		base.InitialProperties.ExternalForceMin = Vector3.Zero;
		base.InitialProperties.ExternalForceMax = Vector3.Zero;
		base.InitialProperties.StartColorMin = Color.White;
		base.InitialProperties.StartColorMax = Color.White;
		base.InitialProperties.EndColorMin = Color.White;
		base.InitialProperties.EndColorMax = Color.White;
		base.InitialProperties.InterpolateBetweenMinAndMaxPosition = false;
		base.InitialProperties.InterpolateBetweenMinAndMaxVelocity = false;
		base.InitialProperties.InterpolateBetweenMinAndMaxAcceleration = false;
		base.InitialProperties.InterpolateBetweenMinAndMaxExternalForce = false;
		base.InitialProperties.InterpolateBetweenMinAndMaxColors = false;
		base.InitialProperties.RotationMin = Vector3.Zero;
		base.InitialProperties.RotationMax = Vector3.Zero;
		base.InitialProperties.RotationalVelocityMin = Vector3.Zero;
		base.InitialProperties.RotationalVelocityMax = Vector3.Zero;
		base.InitialProperties.RotationalAccelerationMin = Vector3.Zero;
		base.InitialProperties.RotationalAccelerationMax = Vector3.Zero;
		base.InitialProperties.StartWidthMin = 20f;
		base.InitialProperties.StartWidthMax = 20f;
		base.InitialProperties.StartHeightMin = 20f;
		base.InitialProperties.StartHeightMax = 20f;
		base.InitialProperties.EndWidthMin = 20f;
		base.InitialProperties.EndWidthMax = 20f;
		base.InitialProperties.EndHeightMin = 20f;
		base.InitialProperties.EndHeightMax = 20f;
		base.InitialProperties.InterpolateBetweenMinAndMaxRotation = false;
		base.InitialProperties.InterpolateBetweenMinAndMaxRotationalVelocity = false;
		base.InitialProperties.InterpolateBetweenMinAndMaxRotationalAcceleration = false;
	}

	public void InitializeParticleAnimated(DefaultAnimatedTexturedQuadParticle cParticle)
	{
		InitializeParticleUsingInitialProperties(cParticle);
		cParticle.Animation.CopyFrom(mcAnimation);
		cParticle.Velocity = DPSFHelper.RandomNormalizedVector() * base.RandomNumber.Next(1, 50) * 0.1f;
	}
}
