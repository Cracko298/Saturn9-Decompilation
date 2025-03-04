using System;
using DPSF;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Saturn9;

public class CrateSmokeParticleSystem : DefaultSprite3DBillboardTextureCoordinatesParticleSystem
{
	private Rectangle _flameSmoke1TextureCoordinates = new Rectangle(0, 0, 128, 128);

	private Rectangle _flameSmoke2TextureCoordinates = new Rectangle(128, 0, 128, 128);

	private Rectangle _flameSmoke3TextureCoordinates = new Rectangle(0, 128, 128, 128);

	private Rectangle _flameSmoke4TextureCoordinates = new Rectangle(128, 128, 128, 128);

	public Color ExplosionColor { get; set; }

	public int ExplosionParticleSize { get; set; }

	public int ExplosionIntensity { get; set; }

	public CrateSmokeParticleSystem(Game game)
		: base(game)
	{
	}

	protected override void InitializeRenderProperties()
	{
		base.InitializeRenderProperties();
		base.RenderProperties.BlendState = BlendState.AlphaBlend;
	}

	public override void AutoInitialize(GraphicsDevice graphicsDevice, ContentManager contentManager, SpriteBatch spriteBatch)
	{
		InitializeSpriteParticleSystem(graphicsDevice, contentManager, 100, 100, "Sprites/Smoke");
		base.Name = "Crate Smoke";
		LoadEvents();
	}

	public void LoadEvents()
	{
		base.ParticleInitializationFunction = InitializeParticleExplosion;
		base.ParticleEvents.RemoveAllEvents();
		base.ParticleEvents.AddEveryTimeEvent(base.UpdateParticleVelocityUsingExternalForce);
		base.ParticleEvents.AddEveryTimeEvent(base.UpdateParticlePositionAndVelocityUsingAcceleration);
		base.ParticleEvents.AddEveryTimeEvent(base.UpdateParticleRotationAndRotationalVelocityUsingRotationalAcceleration);
		base.ParticleEvents.AddEveryTimeEvent(base.UpdateParticleTransparencyWithQuickFadeInAndSlowFadeOut, 100);
		base.ParticleEvents.AddEveryTimeEvent(UpdateParticleFireSmokeSize);
		base.Emitter.PositionData.Position = new Vector3(0f, 0f, 0f);
		base.Emitter.ParticlesPerSecond = 100f;
		base.Emitter.EmitParticlesAutomatically = false;
		ExplosionColor = new Color(255, 120, 0);
	}

	public void InitializeParticleExplosion(DefaultSprite3DBillboardTextureCoordinatesParticle particle)
	{
		particle.Lifetime = base.RandomNumber.Between(0.3f, 0.5f);
		particle.Color = (particle.StartColor = Color.White);
		particle.EndColor = Color.Black;
		particle.Position = base.Emitter.PositionData.Position;
		particle.Velocity = DPSFHelper.RandomNormalizedVector() * base.RandomNumber.Next(1, 50) * 0.5f;
		particle.Velocity.Y *= 0.1f;
		particle.ExternalForce = new Vector3(0f, 0f, 0f);
		float size = (particle.StartSize = 2f);
		particle.Size = size;
		particle.EndSize = 5f;
		particle.Rotation = base.RandomNumber.Between(0f, MathF.PI * 2f);
		particle.RotationalVelocity = base.RandomNumber.Between(-MathF.PI / 2f, MathF.PI / 2f) * 0.3f;
		particle.SetTextureCoordinates(new Rectangle(0, 0, 64, 64));
	}

	protected void UpdateParticleFireSmokeSize(DefaultSprite3DBillboardTextureCoordinatesParticle particle, float elapsedTimeInSeconds)
	{
		particle.Size = MathHelper.Lerp(particle.StartWidth, particle.EndWidth, particle.NormalizedElapsedTime);
	}

	public void ChangeExplosionColor()
	{
		ExplosionColor = DPSFHelper.RandomColor();
	}

	public void ChangeExplosionColor(Color color)
	{
		ExplosionColor = color;
	}
}
