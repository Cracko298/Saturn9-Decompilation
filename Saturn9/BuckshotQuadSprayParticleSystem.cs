using DPSF;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Saturn9;

public class BuckshotQuadSprayParticleSystem : DefaultTexturedQuadTextureCoordinatesParticleSystem
{
	public Vector3 Normal;

	public BuckshotQuadSprayParticleSystem(Game cGame)
		: base(cGame)
	{
	}

	public override void AutoInitialize(GraphicsDevice cGraphicsDevice, ContentManager cContentManager, SpriteBatch cSpriteBatch)
	{
		InitializeTexturedQuadParticleSystem(cGraphicsDevice, cContentManager, 100, 100, UpdateVertexProperties, "Sprites/White");
		base.Name = "Quad Spray";
		LoadSprayEvents();
	}

	public void LoadSprayEvents()
	{
		base.ParticleInitializationFunction = InitializeParticleSpray;
		base.Emitter.ParticlesPerSecond = 10000f;
		base.Emitter.EmitParticlesAutomatically = false;
		base.ParticleEvents.RemoveAllEventsInGroup(0);
		base.ParticleEvents.AddEveryTimeEvent(base.UpdateParticlePositionAndVelocityUsingAcceleration, 100);
		base.ParticleEvents.AddEveryTimeEvent(base.UpdateParticleToFaceTheCamera, 200);
		base.ParticleEvents.AddEveryTimeEvent(base.UpdateParticleTransparencyWithQuickFadeInAndSlowFadeOut, 100);
		base.ParticleEvents.AddEveryTimeEvent(base.UpdateParticleVelocityUsingExternalForce, 0, 1);
	}

	public void InitializeParticleSpray(DefaultTextureQuadTextureCoordinatesParticle cParticle)
	{
		cParticle.Lifetime = base.RandomNumber.Between(0.1f, 0.3f);
		cParticle.Position = base.Emitter.PositionData.Position;
		Vector3 axis = DPSFHelper.RandomNormalizedVector();
		axis = Vector3.Transform(Normal, Quaternion.CreateFromAxisAngle(axis, (float)((base.RandomNumber.NextDouble() - 0.5) * (double)MathHelper.ToRadians(30f))));
		cParticle.Velocity = axis * base.RandomNumber.Next(100, 225) * 0.08f;
		cParticle.Size = 0.08f;
		cParticle.ExternalForce = new Vector3(0f, -30f, 0f);
		cParticle.Color = new Color(0, 0, 0);
	}
}
