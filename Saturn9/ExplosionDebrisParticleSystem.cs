using System;
using DPSF;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Saturn9;

public class ExplosionDebrisParticleSystem : DefaultSprite3DBillboardTextureCoordinatesParticleSystem
{
	private Rectangle _debris1TextureCoordinates = new Rectangle(0, 0, 137, 291);

	private Rectangle _debris2TextureCoordinates = new Rectangle(138, 0, 128, 219);

	private Rectangle _debris3TextureCoordinates = new Rectangle(268, 0, 242, 190);

	private Rectangle _debris4TextureCoordinates = new Rectangle(0, 293, 165, 146);

	private Rectangle _debris5TextureCoordinates = new Rectangle(166, 220, 237, 146);

	private Rectangle _debris6TextureCoordinates = new Rectangle(405, 191, 106, 127);

	private Rectangle _debris7TextureCoordinates = new Rectangle(165, 366, 239, 145);

	private Rectangle _debris8TextureCoordinates = new Rectangle(404, 337, 107, 103);

	public Color ExplosionColor { get; set; }

	public int ExplosionParticleSize { get; set; }

	public int ExplosionIntensity { get; set; }

	public ExplosionDebrisParticleSystem(Game game)
		: base(game)
	{
	}

	public override void SetCameraPosition(Vector3 cameraPosition)
	{
		base.CameraPosition = cameraPosition;
	}

	public override void AutoInitialize(GraphicsDevice graphicsDevice, ContentManager contentManager, SpriteBatch spriteBatch)
	{
		InitializeSpriteParticleSystem(graphicsDevice, contentManager, 100, 100, "Sprites/Gibs");
		base.Name = "Explosion - Debris";
		LoadEvents();
	}

	public void LoadEvents()
	{
		base.ParticleInitializationFunction = InitializeParticleExplosion;
		base.ParticleEvents.RemoveAllEvents();
		base.ParticleEvents.AddEveryTimeEvent(base.UpdateParticleVelocityUsingExternalForce);
		base.ParticleEvents.AddEveryTimeEvent(base.UpdateParticlePositionAndVelocityUsingAcceleration);
		base.ParticleEvents.AddEveryTimeEvent(base.UpdateParticleRotationAndRotationalVelocityUsingRotationalAcceleration);
		base.ParticleEvents.AddEveryTimeEvent(base.UpdateParticleTransparencyWithQuickFadeInAndQuickFadeOut, 100);
		base.ParticleEvents.AddEveryTimeEvent(base.UpdateParticleColorUsingLerp);
		base.Emitter.PositionData.Position = new Vector3(0f, 0f, 0f);
		base.Emitter.ParticlesPerSecond = 10000f;
		base.Emitter.EmitParticlesAutomatically = false;
		ExplosionColor = new Color(128, 128, 128);
		ExplosionParticleSize = 1;
		ExplosionIntensity = 100;
	}

	public void SetupToAutoExplodeEveryInterval(float intervalInSeconds)
	{
		base.ParticleSystemEvents.RemoveAllEventsInGroup(1);
		base.ParticleSystemEvents.LifetimeData.EndOfLifeOption = CParticleSystemEvents.EParticleSystemEndOfLifeOptions.Repeat;
		base.ParticleSystemEvents.LifetimeData.Lifetime = intervalInSeconds;
		base.ParticleSystemEvents.AddTimedEvent(0f, UpdateParticleSystemToExplode, 0, 1);
	}

	public void InitializeParticleExplosion(DefaultSprite3DBillboardTextureCoordinatesParticle particle)
	{
		particle.Lifetime = base.RandomNumber.Between(1f, 1.2f);
		particle.Color = (particle.StartColor = ExplosionColor);
		particle.EndColor = Color.Black;
		particle.Position = base.Emitter.PositionData.Position;
		particle.ExternalForce = new Vector3(0f, -40f, 0f);
		particle.RotationalVelocity = base.RandomNumber.Between(-MathF.PI / 2f, MathF.PI / 2f);
		particle.Velocity = DPSFHelper.RandomNormalizedVector();
		particle.Velocity.Y *= 2f;
		if (particle.Velocity.Y < 0f)
		{
			particle.Velocity.Y *= -1f;
		}
		if (particle.Velocity.Y < 0.3f)
		{
			particle.Velocity.Y += 0.3f;
		}
		if (base.RandomNumber.Between(0f, 5f) == 0f)
		{
			int num = 10;
			new Vector3(base.CameraPosition.X + (float)base.RandomNumber.Next(-num, num), base.CameraPosition.Y + (float)base.RandomNumber.Next(-num, num), base.CameraPosition.Z + (float)base.RandomNumber.Next(-num, num));
			particle.Velocity = particle.Position;
			particle.Velocity.Normalize();
		}
		particle.Velocity *= (float)base.RandomNumber.Next(10, 15);
		Rectangle textureCoordinates = base.RandomNumber.Next(0, 8) switch
		{
			1 => _debris2TextureCoordinates, 
			2 => _debris3TextureCoordinates, 
			3 => _debris4TextureCoordinates, 
			4 => _debris5TextureCoordinates, 
			5 => _debris6TextureCoordinates, 
			6 => _debris7TextureCoordinates, 
			7 => _debris8TextureCoordinates, 
			_ => _debris1TextureCoordinates, 
		};
		particle.SetTextureCoordinates(textureCoordinates);
		particle.Width = textureCoordinates.Width;
		particle.Height = textureCoordinates.Height;
		particle.Size = 0.5f;
		particle.ScaleToWidth((float)ExplosionParticleSize * base.RandomNumber.Between(0.75f, 1.25f));
	}

	protected void UpdateParticleSystemToExplode(float elapsedTimeInSeconds)
	{
		Explode();
	}

	public void Explode()
	{
		base.Emitter.BurstParticles = ExplosionIntensity;
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
