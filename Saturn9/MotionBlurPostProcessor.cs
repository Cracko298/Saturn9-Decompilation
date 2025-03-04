using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SynapseGaming.LightingSystem.Core;
using SynapseGaming.LightingSystem.Rendering;

namespace Saturn9;

public class MotionBlurPostProcessor : BaseRenderTargetPostProcessor
{
	public struct EntityMeshWorld
	{
		public Matrix World;

		public Matrix PreviousWorld;

		public Matrix PreviousW;

		public EntityMeshWorld(Matrix world, Matrix previousWorld)
		{
			World = world;
			PreviousWorld = previousWorld;
			PreviousW = Matrix.Identity;
		}
	}

	public class Entity
	{
		public SceneObject SceneObject;

		public List<ModelMesh> ModelMeshes = new List<ModelMesh>();

		public Dictionary<ModelMesh, EntityMeshWorld> ModelMeshTransforms = new Dictionary<ModelMesh, EntityMeshWorld>();

		public Effect[][] modelEffects;

		public object Tag;

		public float CurrentMaskAmount = 0.5f;

		public bool UseOverrideBlurScale;

		public float OverrideBlurScale;

		public void UpdateWorld(Matrix world)
		{
			foreach (ModelMesh modelMesh in ModelMeshes)
			{
				EntityMeshWorld value = ModelMeshTransforms[modelMesh];
				value.World = world;
				ModelMeshTransforms[modelMesh] = value;
			}
		}

		public void UpdateMeshWorld(ModelMesh mesh, Matrix world)
		{
			EntityMeshWorld value = ModelMeshTransforms[mesh];
			value.World = world;
			ModelMeshTransforms[mesh] = value;
		}
	}

	private ContentManager content;

	private Effect motionBlurEffect;

	private Effect motionBlurMaskEffect;

	private Effect motionBlurBlendEffect;

	private Dictionary<Point, RenderTarget2D> sceneTargets = new Dictionary<Point, RenderTarget2D>();

	private float blurScale = 1f;

	private Dictionary<SceneObject, Entity> sceneObjectEntities = new Dictionary<SceneObject, Entity>();

	private Dictionary<Model, Entity> modelEntities = new Dictionary<Model, Entity>();

	private List<Entity> entities = new List<Entity>();

	private bool enabled = true;

	private Dictionary<string, Dictionary<Point, RenderTarget2D>> renderTargets = new Dictionary<string, Dictionary<Point, RenderTarget2D>>();

	private SpriteBatch batch;

	private bool blurViewMovement = true;

	private bool debug;

	private RenderTarget2D debugRenderTarget;

	private Matrix previousView;

	private Matrix previousProjection;

	public bool Enabled
	{
		get
		{
			return enabled;
		}
		set
		{
			enabled = value;
		}
	}

	public bool Debug
	{
		get
		{
			return debug;
		}
		set
		{
			debug = value;
		}
	}

	public bool BlurViewMovement
	{
		get
		{
			return blurViewMovement;
		}
		set
		{
			blurViewMovement = value;
		}
	}

	public float BlurScale
	{
		get
		{
			return blurScale;
		}
		set
		{
			blurScale = value;
		}
	}

	public Dictionary<SceneObject, Entity> SceneObjectEntities => sceneObjectEntities;

	public Dictionary<Model, Entity> ModelEntities => modelEntities;

	public List<Entity> Entities => entities;

	public MotionBlurPostProcessor(GraphicsDeviceManager deviceManager, ContentManager content)
	{
		this.content = content;
	}

	public Entity AddSceneObject(SceneObject sceneObject)
	{
		List<ModelMesh> list = new List<ModelMesh>();
		if (sceneObject.ModelAsset == null)
		{
			return null;
		}
		Model asset = sceneObject.ModelAsset.Asset;
		if (sceneObject.Name == null)
		{
			foreach (ModelMesh mesh in asset.Meshes)
			{
				list.Add(mesh);
			}
		}
		else
		{
			foreach (ModelMesh mesh2 in asset.Meshes)
			{
				if (mesh2.Name == sceneObject.Name)
				{
					list.Add(mesh2);
					break;
				}
			}
		}
		return AddSceneObject(sceneObject, list.ToArray());
	}

	public Entity AddSceneObject(SceneObject sceneObject, params ModelMesh[] meshes)
	{
		Entity entity = new Entity();
		entity.SceneObject = sceneObject;
		foreach (ModelMesh modelMesh in meshes)
		{
			entity.ModelMeshes.Add(modelMesh);
			entity.ModelMeshTransforms[modelMesh] = default(EntityMeshWorld);
		}
		sceneObjectEntities.Add(sceneObject, entity);
		entities.Add(entity);
		return entity;
	}

	public Entity AddModel(Model model, Matrix world)
	{
		Entity entity = new Entity();
		foreach (ModelMesh mesh in model.Meshes)
		{
			entity.ModelMeshes.Add(mesh);
			entity.ModelMeshTransforms[mesh] = new EntityMeshWorld(world, world);
		}
		modelEntities.Add(model, entity);
		entities.Add(entity);
		return entity;
	}

	public void RemoveSceneObject(SceneObject sceneObject)
	{
		Entity item = sceneObjectEntities[sceneObject];
		sceneObjectEntities.Remove(sceneObject);
		entities.Remove(item);
	}

	public void RemoveModel(Model model)
	{
		Entity item = modelEntities[model];
		modelEntities.Remove(model);
		entities.Remove(item);
	}

	public void ClearEntities()
	{
		sceneObjectEntities.Clear();
		entities.Clear();
	}

	public override void ApplyPreferences(ISystemPreferences preferences)
	{
	}

	protected void CacheEffects(Entity entity)
	{
		entity.modelEffects = new Effect[entity.ModelMeshes.Count][];
		Effect[][] modelEffects = entity.modelEffects;
		int num = -1;
		foreach (ModelMesh modelMesh in entity.ModelMeshes)
		{
			modelEffects[++num] = new Effect[modelMesh.MeshParts.Count];
			int num2 = -1;
			foreach (ModelMeshPart meshPart in modelMesh.MeshParts)
			{
				modelEffects[num][++num2] = meshPart.Effect;
			}
		}
	}

	protected void SetEntityEffect(Entity entity, Effect newEffect)
	{
		foreach (ModelMesh modelMesh in entity.ModelMeshes)
		{
			foreach (ModelMeshPart meshPart in modelMesh.MeshParts)
			{
				meshPart.Effect = newEffect;
			}
		}
	}

	protected void RestoreEntityEffect(Entity entity)
	{
		int num = -1;
		foreach (ModelMesh modelMesh in entity.ModelMeshes)
		{
			num++;
			int num2 = -1;
			foreach (ModelMeshPart meshPart in modelMesh.MeshParts)
			{
				meshPart.Effect = entity.modelEffects[num][++num2];
			}
		}
	}

	protected RenderTarget2D GetRenderTarget(string key, Point size)
	{
		if (renderTargets.ContainsKey(key) && renderTargets[key].ContainsKey(size))
		{
			return renderTargets[key][size];
		}
		PresentationParameters presentationParameters = base.GraphicsDeviceManager.GraphicsDevice.PresentationParameters;
		RenderTarget2D renderTarget2D = new RenderTarget2D(base.GraphicsDeviceManager.GraphicsDevice, size.X, size.Y, mipMap: false, presentationParameters.BackBufferFormat, DepthFormat.Depth24, presentationParameters.MultiSampleCount, presentationParameters.RenderTargetUsage);
		if (!renderTargets.ContainsKey(key))
		{
			renderTargets.Add(key, new Dictionary<Point, RenderTarget2D>());
		}
		renderTargets[key].Add(size, renderTarget2D);
		return renderTarget2D;
	}

	public override void EndFrameRendering()
	{
		if (batch == null)
		{
			batch = new SpriteBatch(base.GraphicsDeviceManager.GraphicsDevice);
			motionBlurEffect = content.Load<Effect>("Effects/MotionBlur");
			motionBlurMaskEffect = content.Load<Effect>("Effects/MotionBlurMask");
			motionBlurBlendEffect = content.Load<Effect>("Effects/MotionBlurBlend");
		}
		base.EndFrameRendering();
		RenderTarget2D processorRenderTarget = base.ProcessorRenderTarget;
		if (!enabled)
		{
			batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
			batch.Draw(processorRenderTarget, new Rectangle(0, 0, processorRenderTarget.Width, processorRenderTarget.Height), Color.White);
			batch.End();
			return;
		}
		bool flag = false;
		foreach (Entity entity in entities)
		{
			if (entity.CurrentMaskAmount > 0f)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
			batch.Draw(processorRenderTarget, new Rectangle(0, 0, processorRenderTarget.Width, processorRenderTarget.Height), Color.White);
			batch.End();
			return;
		}
		RenderTarget2D renderTarget2D = null;
		RenderTarget2D renderTarget2D2 = null;
		RenderTarget2D renderTarget2D3 = null;
		Point size = new Point(processorRenderTarget.Width, processorRenderTarget.Height);
		renderTarget2D = GetRenderTarget("MainTarget", size);
		renderTarget2D2 = GetRenderTarget("MaskTarget", size);
		RenderTargetBinding[] array = base.GraphicsDeviceManager.GraphicsDevice.GetRenderTargets();
		if (debug)
		{
			renderTarget2D3 = GetRenderTarget("DebugTarget", size);
			base.GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(renderTarget2D3);
			base.GraphicsDeviceManager.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1f, 0);
			base.GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(null);
		}
		base.GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(renderTarget2D);
		base.GraphicsDeviceManager.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1f, 0);
		foreach (Entity entity2 in entities)
		{
			Matrix view = base.SceneState.View;
			Matrix projection = base.SceneState.Projection;
			CacheEffects(entity2);
			if (entity2.CurrentMaskAmount == 0f)
			{
				continue;
			}
			SetEntityEffect(entity2, motionBlurEffect);
			int num = -1;
			foreach (ModelMesh modelMesh in entity2.ModelMeshes)
			{
				EntityMeshWorld value = entity2.ModelMeshTransforms[modelMesh];
				num++;
				Matrix matrix = modelMesh.ParentBone.Transform;
				if (entity2.SceneObject != null)
				{
					value.World = entity2.SceneObject.World;
					int index = num;
					if (entity2.SceneObject.RenderableMeshes.Count - 1 < num)
					{
						index = 0;
					}
					matrix = entity2.SceneObject.RenderableMeshes[index].MeshToObject;
				}
				foreach (Effect effect in modelMesh.Effects)
				{
					if (entity2.SceneObject != null && entity2.SceneObject.SkinBones != null)
					{
						effect.CurrentTechnique = effect.Techniques["AnimatedMotionBlur"];
						effect.Parameters["currentPose"].SetValue(entity2.SceneObject.SkinBones);
						matrix = Matrix.Identity;
					}
					else
					{
						effect.CurrentTechnique = effect.Techniques["MotionBlur"];
					}
					Matrix matrix2 = previousView;
					Matrix matrix3 = previousProjection;
					if (!blurViewMovement)
					{
						matrix2 = view;
						matrix3 = projection;
					}
					effect.Parameters["modelView"].SetValue(matrix * value.World * view);
					effect.Parameters["prevModelView"].SetValue(value.PreviousW * value.PreviousWorld * matrix2);
					effect.Parameters["modelViewProj"].SetValue(matrix * value.World * view * projection);
					effect.Parameters["prevModelViewProj"].SetValue(value.PreviousW * value.PreviousWorld * matrix2 * matrix3);
					effect.Parameters["blurScale"].SetValue(entity2.UseOverrideBlurScale ? entity2.OverrideBlurScale : blurScale);
					effect.Parameters["SceneTexture"].SetValue(debug ? renderTarget2D3 : processorRenderTarget);
				}
				modelMesh.Draw();
				value.PreviousWorld = value.World;
				value.PreviousW = matrix;
				entity2.ModelMeshTransforms[modelMesh] = value;
			}
		}
		base.GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(null);
		Texture2D value2 = renderTarget2D;
		base.GraphicsDeviceManager.GraphicsDevice.SetRenderTarget(renderTarget2D2);
		base.GraphicsDeviceManager.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);
		foreach (Entity entity3 in entities)
		{
			SetEntityEffect(entity3, motionBlurMaskEffect);
			Matrix view2 = base.SceneState.View;
			Matrix projection2 = base.SceneState.Projection;
			foreach (ModelMesh modelMesh2 in entity3.ModelMeshes)
			{
				EntityMeshWorld entityMeshWorld = entity3.ModelMeshTransforms[modelMesh2];
				Matrix matrix4 = modelMesh2.ParentBone.Transform;
				if (entity3.SceneObject != null)
				{
					matrix4 = entity3.SceneObject.RenderableMeshes[0].MeshToObject;
				}
				foreach (Effect effect2 in modelMesh2.Effects)
				{
					if (entity3.SceneObject != null && entity3.SceneObject.SkinBones != null)
					{
						effect2.CurrentTechnique = effect2.Techniques["AnimatedObjectMask"];
						effect2.Parameters["currentPose"].SetValue(entity3.SceneObject.SkinBones);
						matrix4 = Matrix.Identity;
					}
					else
					{
						effect2.CurrentTechnique = effect2.Techniques["ObjectMask"];
					}
					effect2.Parameters["modelViewProj"].SetValue(matrix4 * entityMeshWorld.World * view2 * projection2);
					effect2.Parameters["maskingAmount"].SetValue(entity3.CurrentMaskAmount);
				}
				modelMesh2.Draw();
			}
			RestoreEntityEffect(entity3);
		}
		previousView = base.SceneState.View;
		previousProjection = base.SceneState.Projection;
		base.GraphicsDeviceManager.GraphicsDevice.SetRenderTargets(array);
		Texture2D value3 = renderTarget2D2;
		batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
		motionBlurBlendEffect.Parameters["TargetTexture"].SetValue(value2);
		motionBlurBlendEffect.Parameters["MaskTexture"].SetValue(value3);
		motionBlurBlendEffect.CurrentTechnique.Passes[0].Apply();
		batch.Draw(processorRenderTarget, new Rectangle(0, 0, processorRenderTarget.Width, processorRenderTarget.Height), Color.White);
		batch.End();
	}
}
