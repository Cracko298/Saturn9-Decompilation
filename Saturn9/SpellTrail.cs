using System;
using DPSF;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Saturn9;

public class SpellTrail : DefaultSprite3DBillboardTextureCoordinatesParticleSystem
{
	private Rectangle _flameSmoke11TextureCoordinates = new Rectangle(0, 0, 128, 128);

	private Rectangle _flameSmoke12TextureCoordinates = new Rectangle(128, 0, 128, 128);

	private Rectangle _flameSmoke13TextureCoordinates = new Rectangle(256, 0, 128, 128);

	private Rectangle _flameSmoke14TextureCoordinates = new Rectangle(384, 0, 128, 128);

	private Rectangle _flameSmoke21TextureCoordinates = new Rectangle(0, 128, 128, 128);

	private Rectangle _flameSmoke22TextureCoordinates = new Rectangle(128, 128, 128, 128);

	private Rectangle _flameSmoke23TextureCoordinates = new Rectangle(256, 128, 128, 128);

	private Rectangle _flameSmoke24TextureCoordinates = new Rectangle(384, 128, 128, 128);

	private Rectangle _flameSmoke31TextureCoordinates = new Rectangle(0, 256, 128, 128);

	private Rectangle _flameSmoke32TextureCoordinates = new Rectangle(128, 256, 128, 128);

	private Rectangle _flameSmoke33TextureCoordinates = new Rectangle(256, 256, 128, 128);

	private Rectangle _flameSmoke34TextureCoordinates = new Rectangle(384, 256, 128, 128);

	private Rectangle _flameSmoke41TextureCoordinates = new Rectangle(0, 384, 128, 128);

	private Rectangle _flameSmoke42TextureCoordinates = new Rectangle(128, 384, 128, 128);

	private Rectangle _flameSmoke43TextureCoordinates = new Rectangle(256, 384, 128, 128);

	private Rectangle _flameSmoke44TextureCoordinates = new Rectangle(384, 384, 128, 128);

	public Color m_Colour = Color.Yellow;

	public Color ExplosionColor { get; set; }

	public int ExplosionParticleSize { get; set; }

	public int ExplosionIntensity { get; set; }

	public SpellTrail(Game game)
		: base(game)
	{
	}

	public void SetColour(int colourId)
	{
		switch (colourId)
		{
		case -1:
			m_Colour = Color.Yellow;
			break;
		case 100:
			m_Colour = Color.DarkRed;
			break;
		case 0:
			m_Colour = Color.Pink;
			break;
		case 1:
			m_Colour = Color.Green;
			break;
		case 2:
			m_Colour = Color.Purple;
			break;
		case 3:
			m_Colour = Color.Violet;
			break;
		case 4:
			m_Colour = Color.RoyalBlue;
			break;
		case 5:
			m_Colour = Color.DarkTurquoise;
			break;
		case 6:
			m_Colour = Color.DarkOrange;
			break;
		case 7:
			m_Colour = Color.White;
			break;
		}
	}

	protected override void InitializeRenderProperties()
	{
		base.InitializeRenderProperties();
		base.RenderProperties.BlendState = BlendState.AlphaBlend;
	}

	public override void AutoInitialize(GraphicsDevice graphicsDevice, ContentManager contentManager, SpriteBatch spriteBatch)
	{
		InitializeSpriteParticleSystem(graphicsDevice, contentManager, 100, 100, "Sprites/Smoke_all_4x4");
		base.Name = "Crate Smoke";
		LoadEvents();
	}

	public void LoadEvents()
	{
		base.ParticleInitializationFunction = InitializeParticleExplosion;
		base.ParticleEvents.RemoveAllEvents();
		base.ParticleEvents.AddEveryTimeEvent(base.UpdateParticlePositionAndVelocityUsingAcceleration);
		base.ParticleEvents.AddEveryTimeEvent(base.UpdateParticleRotationAndRotationalVelocityUsingRotationalAcceleration);
		base.ParticleEvents.AddEveryTimeEvent(base.UpdateParticleTransparencyWithQuickFadeInAndSlowFadeOut, 100);
		base.ParticleEvents.AddEveryTimeEvent(UpdateParticleFireSmokeSize);
		base.Emitter.PositionData.Position = new Vector3(0f, 0f, 0f);
		base.Emitter.ParticlesPerSecond = 40f;
		base.Emitter.EmitParticlesAutomatically = true;
	}

	public void InitializeParticleExplosion(DefaultSprite3DBillboardTextureCoordinatesParticle particle)
	{
		particle.Lifetime = base.RandomNumber.Between(0.2f, 0.4f);
		int num = base.RandomNumber.Next(0, 2);
		if (num == 1)
		{
			particle.Color = (particle.StartColor = m_Colour);
		}
		else
		{
			particle.Color = (particle.StartColor = new Color(166, 220, 255));
		}
		particle.EndColor = Color.Black;
		particle.Position = base.Emitter.PositionData.Position;
		particle.Velocity = DPSFHelper.RandomNormalizedVector() * base.RandomNumber.Next(1, 50) * 0.025f;
		particle.Velocity.Y = 5f;
		float size = (particle.StartSize = 2f);
		particle.Size = size;
		particle.EndSize = 3f;
		particle.Rotation = base.RandomNumber.Between(0f, MathF.PI * 2f);
		particle.RotationalVelocity = base.RandomNumber.Between(-MathF.PI / 2f, MathF.PI / 2f) * 0.3f;
		particle.SetTextureCoordinates(base.RandomNumber.Next(0, 16) switch
		{
			1 => _flameSmoke12TextureCoordinates, 
			2 => _flameSmoke13TextureCoordinates, 
			3 => _flameSmoke14TextureCoordinates, 
			4 => _flameSmoke21TextureCoordinates, 
			5 => _flameSmoke22TextureCoordinates, 
			6 => _flameSmoke23TextureCoordinates, 
			7 => _flameSmoke24TextureCoordinates, 
			8 => _flameSmoke31TextureCoordinates, 
			9 => _flameSmoke32TextureCoordinates, 
			10 => _flameSmoke33TextureCoordinates, 
			11 => _flameSmoke34TextureCoordinates, 
			12 => _flameSmoke41TextureCoordinates, 
			13 => _flameSmoke42TextureCoordinates, 
			14 => _flameSmoke43TextureCoordinates, 
			15 => _flameSmoke44TextureCoordinates, 
			_ => _flameSmoke11TextureCoordinates, 
		});
	}

	protected void UpdateParticleFireSmokeSize(DefaultSprite3DBillboardTextureCoordinatesParticle particle, float elapsedTimeInSeconds)
	{
		particle.Size = MathHelper.Lerp(particle.StartWidth, particle.EndWidth, particle.NormalizedElapsedTime);
	}
}
