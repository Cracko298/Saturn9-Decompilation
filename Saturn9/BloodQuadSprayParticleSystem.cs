using System;
using DPSF;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Saturn9;

public class BloodQuadSprayParticleSystem : DefaultSprite3DBillboardParticleSystem
{
	public Vector3 Normal;

	public Color m_Colour = new Color(8, 0, 0);

	public BloodQuadSprayParticleSystem(Game cGame)
		: base(cGame)
	{
	}

	public override void AutoInitialize(GraphicsDevice cGraphicsDevice, ContentManager cContentManager, SpriteBatch cSpriteBatch)
	{
		InitializeSpriteParticleSystem(cGraphicsDevice, cContentManager, 100, 100, "Sprites/Smoke");
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
		base.ParticleEvents.AddEveryTimeEvent(base.UpdateParticleTransparencyWithQuickFadeInAndSlowFadeOut, 100);
		base.ParticleEvents.AddEveryTimeEvent(base.UpdateParticleVelocityUsingExternalForce, 0, 1);
		base.ParticleEvents.AddEveryTimeEvent(base.UpdateParticleRotationAndRotationalVelocityUsingRotationalAcceleration);
	}

	public void InitializeParticleSpray(DefaultSprite3DBillboardParticle cParticle)
	{
		cParticle.Lifetime = base.RandomNumber.Between(1.1f, 1.3f);
		cParticle.Position = base.Emitter.PositionData.Position;
		Vector3 axis = DPSFHelper.RandomNormalizedVector();
		axis = Vector3.Transform(Normal, Quaternion.CreateFromAxisAngle(axis, (float)((base.RandomNumber.NextDouble() - 0.5) * (double)MathHelper.ToRadians(90f))));
		cParticle.Velocity = axis * 0.3f + new Vector3(0f, 1.5f, 0f);
		cParticle.Size = 0.1f;
		cParticle.ExternalForce = new Vector3(0f, -2.5f, 0f);
		cParticle.Color = new Color(48, 0, 0);
		cParticle.Rotation = base.RandomNumber.Between(0f, MathF.PI * 2f);
		cParticle.RotationalVelocity = base.RandomNumber.Between(-MathF.PI / 2f, MathF.PI / 2f);
	}
}
