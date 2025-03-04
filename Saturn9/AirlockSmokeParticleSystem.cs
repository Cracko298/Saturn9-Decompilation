using System;
using DPSF;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Saturn9;

public class AirlockSmokeParticleSystem : DefaultSprite3DBillboardTextureCoordinatesParticleSystem
{
	private Rectangle _flameSmoke1TextureCoordinates = new Rectangle(0, 0, 128, 128);

	private Rectangle _flameSmoke2TextureCoordinates = new Rectangle(128, 0, 128, 128);

	private Rectangle _flameSmoke3TextureCoordinates = new Rectangle(0, 128, 128, 128);

	private Rectangle _flameSmoke4TextureCoordinates = new Rectangle(128, 128, 128, 128);

	public Vector3 m_ExplosionDir = new Vector3(0f, 0f, 1f);

	public Color ExplosionColor { get; set; }

	public int ExplosionParticleSize { get; set; }

	public int ExplosionIntensity { get; set; }

	public AirlockSmokeParticleSystem(Game game)
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
		InitializeSpriteParticleSystem(graphicsDevice, contentManager, 500, 500, "Sprites/Smoke");
		base.Name = "Airlock Smoke";
		LoadEvents();
	}

	public void LoadEvents()
	{
		base.ParticleInitializationFunction = InitializeParticleExplosion;
		base.ParticleEvents.RemoveAllEvents();
		base.ParticleEvents.AddEveryTimeEvent(base.UpdateParticlePositionAndVelocityUsingAcceleration);
		base.ParticleEvents.AddEveryTimeEvent(base.UpdateParticleRotationAndRotationalVelocityUsingRotationalAcceleration);
		base.ParticleEvents.AddEveryTimeEvent(base.UpdateParticleTransparencyWithQuickFadeInAndSlowFadeOut, 100);
		base.Emitter.PositionData.Position = new Vector3(0f, 0f, 0f);
		base.Emitter.ParticlesPerSecond = 100f;
		base.Emitter.EmitParticlesAutomatically = false;
		ExplosionColor = new Color(255, 120, 0);
	}

	public void InitializeParticleExplosion(DefaultSprite3DBillboardTextureCoordinatesParticle particle)
	{
		particle.Lifetime = base.RandomNumber.Between(3f, 5f);
		particle.Color = (particle.StartColor = Color.White);
		particle.EndColor = Color.Black;
		particle.Position = base.Emitter.PositionData.Position;
		Vector3 axis = DPSFHelper.RandomNormalizedVector();
		axis = Vector3.Transform(m_ExplosionDir, Quaternion.CreateFromAxisAngle(axis, (float)((base.RandomNumber.NextDouble() - 0.5) * (double)MathHelper.ToRadians(45f))));
		particle.Velocity = axis * base.RandomNumber.Next(100, 225) * 0.1f;
		float size = (particle.StartSize = 4f);
		particle.Size = size;
		particle.EndSize = 40f;
		particle.Rotation = base.RandomNumber.Between(0f, MathF.PI * 2f);
		particle.RotationalVelocity = base.RandomNumber.Between(-MathF.PI / 2f, MathF.PI / 2f) * 0.3f;
		particle.SetTextureCoordinates(new Rectangle(0, 0, 64, 64));
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
