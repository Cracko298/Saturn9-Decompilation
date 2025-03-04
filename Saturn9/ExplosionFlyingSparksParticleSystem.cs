using DPSF;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Saturn9;

public class ExplosionFlyingSparksParticleSystem : DefaultTexturedQuadTextureCoordinatesParticleSystem
{
	public Vector3 Normal;

	private Rectangle _sparkTextureCoordinates = new Rectangle(0, 0, 16, 16);

	public Color ExplosionColor { get; set; }

	public int ExplosionParticleSize { get; set; }

	public int ExplosionIntensity { get; set; }

	public ExplosionFlyingSparksParticleSystem(Game game)
		: base(game)
	{
	}

	protected override void InitializeRenderProperties()
	{
		base.InitializeRenderProperties();
		base.RenderProperties.RasterizerState.CullMode = CullMode.None;
		base.RenderProperties.BlendState = BlendState.Additive;
	}

	public override void AutoInitialize(GraphicsDevice graphicsDevice, ContentManager contentManager, SpriteBatch spriteBatch)
	{
		InitializeTexturedQuadParticleSystem(graphicsDevice, contentManager, 100, 100, UpdateVertexProperties, "Sprites/White");
		base.Name = "Explosion - Flying Sparks";
		LoadEvents();
	}

	public void LoadEvents()
	{
		base.ParticleInitializationFunction = InitializeParticleExplosion;
		base.ParticleEvents.RemoveAllEvents();
		base.ParticleEvents.AddEveryTimeEvent(base.UpdateParticlePositionAndVelocityUsingAcceleration);
		base.ParticleEvents.AddEveryTimeEvent(base.UpdateParticleTransparencyWithQuickFadeInAndSlowFadeOut, 100);
		base.Emitter.PositionData.Position = new Vector3(0f, 0f, 0f);
		base.Emitter.ParticlesPerSecond = 10000f;
		base.Emitter.EmitParticlesAutomatically = false;
		ExplosionColor = new Color(255, 255, 0);
		ExplosionParticleSize = 1;
	}

	public void InitializeParticleExplosion(DefaultTextureQuadTextureCoordinatesParticle particle)
	{
		particle.Lifetime = base.RandomNumber.Between(0.1f, 0.2f);
		particle.Color = ExplosionColor;
		particle.Position = base.Emitter.PositionData.Position;
		Vector3 axis = DPSFHelper.RandomNormalizedVector();
		axis = Vector3.Transform(Normal, Quaternion.CreateFromAxisAngle(axis, (float)((base.RandomNumber.NextDouble() - 0.5) * (double)MathHelper.ToRadians(120f))));
		particle.Velocity = axis * base.RandomNumber.Next(100, 225) * 0.1f;
		particle.Right = -particle.Velocity;
		particle.Width = ExplosionParticleSize;
		particle.Height = (float)ExplosionParticleSize * 0.01f;
	}
}
