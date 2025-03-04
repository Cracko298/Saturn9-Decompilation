using System;
using System.Collections.Generic;
using BEPUphysics;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.Entities.Prefabs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SynapseGaming.LightingSystem.Rendering;

namespace Saturn9;

public class BEPUDebugDrawer : IDisposable
{
	public static class DisplayStaticMesh
	{
		public static void GetShapeMeshData(ISpaceObject shape, List<VertexPositionColor> vertices, List<int> indices)
		{
			if (!(shape is StaticMesh staticMesh))
			{
				throw new ArgumentException("Wrong shape type");
			}
			for (int i = 0; i < staticMesh.Mesh.Data.Vertices.Length; i++)
			{
				vertices.Add(new VertexPositionColor(staticMesh.Mesh.Data.Vertices[i], debugColorStaticMesh));
			}
			indices.AddRange(staticMesh.Mesh.Data.Indices);
		}
	}

	public static class DisplaySphere
	{
		public static int NumSides = 24;

		public static void GetShapeMeshData(ISpaceObject shape, List<VertexPositionColor> vertices, List<int> indices)
		{
			if (!(shape is Sphere sphere))
			{
				throw new ArgumentException("Wrong shape type");
			}
			Vector3 vector = default(Vector3);
			float num = MathF.PI * 2f / (float)NumSides;
			float radius = sphere.Radius;
			vertices.Add(new VertexPositionColor(new Vector3(0f, radius, 0f), debugColorSphere));
			for (int i = 1; i < NumSides / 2; i++)
			{
				float num2 = MathF.PI / 2f - (float)i * num;
				float y = (float)Math.Sin(num2);
				float num3 = (float)Math.Cos(num2);
				for (int j = 0; j < NumSides; j++)
				{
					float num4 = (float)j * num;
					vector.X = (float)Math.Cos(num4) * num3;
					vector.Y = y;
					vector.Z = (float)Math.Sin(num4) * num3;
					vertices.Add(new VertexPositionColor(vector * radius, debugColorSphere));
				}
			}
			vertices.Add(new VertexPositionColor(new Vector3(0f, 0f - radius, 0f), debugColorSphere));
			for (int k = 0; k < NumSides; k++)
			{
				indices.Add((ushort)(vertices.Count - 1));
				indices.Add((ushort)(vertices.Count - 2 - k));
				indices.Add((ushort)(vertices.Count - 2 - (k + 1) % NumSides));
			}
			for (int l = 0; l < NumSides / 2 - 2; l++)
			{
				for (int m = 0; m < NumSides; m++)
				{
					int num5 = (m + 1) % NumSides;
					indices.Add((ushort)(l * NumSides + num5 + 1));
					indices.Add((ushort)(l * NumSides + m + 1));
					indices.Add((ushort)((l + 1) * NumSides + m + 1));
					indices.Add((ushort)((l + 1) * NumSides + num5 + 1));
					indices.Add((ushort)(l * NumSides + num5 + 1));
					indices.Add((ushort)((l + 1) * NumSides + m + 1));
				}
			}
			for (int n = 0; n < NumSides; n++)
			{
				indices.Add(0);
				indices.Add((ushort)(n + 1));
				indices.Add((ushort)((n + 1) % NumSides + 1));
			}
		}
	}

	public static class DisplayBox
	{
		public static void GetShapeMeshData(ISpaceObject shape, List<VertexPositionColor> vertices, List<int> indices)
		{
			if (!(shape is Box box))
			{
				throw new ArgumentException("Wrong shape type.");
			}
			Vector3[] corners = new BoundingBox(new Vector3(0f - box.HalfWidth, 0f - box.HalfHeight, 0f - box.HalfLength), new Vector3(box.HalfWidth, box.HalfHeight, box.HalfLength)).GetCorners();
			vertices.Add(new VertexPositionColor(corners[0], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[1], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[2], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[3], debugColorBox));
			indices.Add(0);
			indices.Add(1);
			indices.Add(2);
			indices.Add(0);
			indices.Add(2);
			indices.Add(3);
			vertices.Add(new VertexPositionColor(corners[1], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[2], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[5], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[6], debugColorBox));
			indices.Add(4);
			indices.Add(6);
			indices.Add(7);
			indices.Add(4);
			indices.Add(7);
			indices.Add(5);
			vertices.Add(new VertexPositionColor(corners[4], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[5], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[6], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[7], debugColorBox));
			indices.Add(9);
			indices.Add(8);
			indices.Add(11);
			indices.Add(9);
			indices.Add(11);
			indices.Add(10);
			vertices.Add(new VertexPositionColor(corners[0], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[3], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[4], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[7], debugColorBox));
			indices.Add(14);
			indices.Add(12);
			indices.Add(13);
			indices.Add(14);
			indices.Add(13);
			indices.Add(15);
			vertices.Add(new VertexPositionColor(corners[0], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[1], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[4], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[5], debugColorBox));
			indices.Add(16);
			indices.Add(19);
			indices.Add(17);
			indices.Add(16);
			indices.Add(18);
			indices.Add(19);
			vertices.Add(new VertexPositionColor(corners[2], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[3], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[6], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[7], debugColorBox));
			indices.Add(21);
			indices.Add(20);
			indices.Add(22);
			indices.Add(21);
			indices.Add(22);
			indices.Add(23);
		}
	}

	public static class DisplayCapsule
	{
		public static int NumSides = 24;

		public static void GetShapeMeshData(ISpaceObject shape, List<VertexPositionColor> vertices, List<int> indices)
		{
			if (!(shape is Capsule capsule))
			{
				throw new ArgumentException("Wrong shape type.");
			}
			Vector3 vector = default(Vector3);
			Vector3 vector2 = new Vector3(0f, capsule.Length / 2f, 0f);
			float num = MathF.PI * 2f / (float)NumSides;
			float radius = capsule.Radius;
			vertices.Add(new VertexPositionColor(new Vector3(0f, radius + capsule.Length / 2f, 0f), debugColorCapsule));
			for (int i = 1; i <= NumSides / 4; i++)
			{
				float num2 = MathF.PI / 2f - (float)i * num;
				float y = (float)Math.Sin(num2);
				float num3 = (float)Math.Cos(num2);
				for (int j = 0; j < NumSides; j++)
				{
					float num4 = (float)j * num;
					vector.X = (float)Math.Cos(num4) * num3;
					vector.Y = y;
					vector.Z = (float)Math.Sin(num4) * num3;
					vertices.Add(new VertexPositionColor(vector * radius + vector2, debugColorCapsule));
				}
			}
			for (int k = NumSides / 4; k < NumSides / 2; k++)
			{
				float num5 = MathF.PI / 2f - (float)k * num;
				float y2 = (float)Math.Sin(num5);
				float num6 = (float)Math.Cos(num5);
				for (int l = 0; l < NumSides; l++)
				{
					float num7 = (float)l * num;
					vector.X = (float)Math.Cos(num7) * num6;
					vector.Y = y2;
					vector.Z = (float)Math.Sin(num7) * num6;
					vertices.Add(new VertexPositionColor(vector * radius - vector2, debugColorCapsule));
				}
			}
			vertices.Add(new VertexPositionColor(new Vector3(0f, 0f - radius - capsule.Length / 2f, 0f), debugColorCapsule));
			for (int m = 0; m < NumSides; m++)
			{
				indices.Add(vertices.Count - 1);
				indices.Add(vertices.Count - 2 - m);
				indices.Add(vertices.Count - 2 - (m + 1) % NumSides);
			}
			for (int n = 0; n < NumSides / 2 - 1; n++)
			{
				for (int num8 = 0; num8 < NumSides; num8++)
				{
					int num9 = (num8 + 1) % NumSides;
					indices.Add(n * NumSides + num9 + 1);
					indices.Add(n * NumSides + num8 + 1);
					indices.Add((n + 1) * NumSides + num8 + 1);
					indices.Add((n + 1) * NumSides + num9 + 1);
					indices.Add(n * NumSides + num9 + 1);
					indices.Add((n + 1) * NumSides + num8 + 1);
				}
			}
			for (int num10 = 0; num10 < NumSides; num10++)
			{
				indices.Add(0);
				indices.Add(num10 + 1);
				indices.Add((num10 + 1) % NumSides + 1);
			}
		}
	}

	public static class DisplayCylinder
	{
		public static void GetShapeMeshData(ISpaceObject shape, List<VertexPositionColor> vertices, List<int> indices)
		{
			if (!(shape is Cylinder cylinder))
			{
				throw new ArgumentException("Wrong shape type.");
			}
			Vector3[] corners = new BoundingBox(new Vector3(0f - cylinder.Radius, (0f - cylinder.Height) * 0.5f, 0f - cylinder.Radius), new Vector3(cylinder.Radius, cylinder.Height * 0.5f, cylinder.Radius)).GetCorners();
			vertices.Add(new VertexPositionColor(corners[0], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[1], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[2], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[3], debugColorBox));
			indices.Add(0);
			indices.Add(1);
			indices.Add(2);
			indices.Add(0);
			indices.Add(2);
			indices.Add(3);
			vertices.Add(new VertexPositionColor(corners[1], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[2], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[5], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[6], debugColorBox));
			indices.Add(4);
			indices.Add(6);
			indices.Add(7);
			indices.Add(4);
			indices.Add(7);
			indices.Add(5);
			vertices.Add(new VertexPositionColor(corners[4], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[5], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[6], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[7], debugColorBox));
			indices.Add(9);
			indices.Add(8);
			indices.Add(11);
			indices.Add(9);
			indices.Add(11);
			indices.Add(10);
			vertices.Add(new VertexPositionColor(corners[0], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[3], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[4], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[7], debugColorBox));
			indices.Add(14);
			indices.Add(12);
			indices.Add(13);
			indices.Add(14);
			indices.Add(13);
			indices.Add(15);
			vertices.Add(new VertexPositionColor(corners[0], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[1], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[4], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[5], debugColorBox));
			indices.Add(16);
			indices.Add(19);
			indices.Add(17);
			indices.Add(16);
			indices.Add(18);
			indices.Add(19);
			vertices.Add(new VertexPositionColor(corners[2], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[3], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[6], debugColorBox));
			vertices.Add(new VertexPositionColor(corners[7], debugColorBox));
			indices.Add(21);
			indices.Add(20);
			indices.Add(22);
			indices.Add(21);
			indices.Add(22);
			indices.Add(23);
		}
	}

	public static class DisplayBoundingBox
	{
		public static void GetShapeMeshData(SceneEntity shape, List<VertexPositionColor> vertices, List<int> indices)
		{
			BoundingBox worldBoundingBox = shape.WorldBoundingBox;
			Vector3[] corners = new BoundingBox(new Vector3(worldBoundingBox.Min.X, worldBoundingBox.Min.Y, worldBoundingBox.Min.Z), new Vector3(worldBoundingBox.Max.X, worldBoundingBox.Max.Y, worldBoundingBox.Max.Z)).GetCorners();
			vertices.Add(new VertexPositionColor(corners[0], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[1], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[2], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[3], debugColorBB));
			indices.Add(0);
			indices.Add(1);
			indices.Add(2);
			indices.Add(0);
			indices.Add(2);
			indices.Add(3);
			vertices.Add(new VertexPositionColor(corners[1], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[2], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[5], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[6], debugColorBB));
			indices.Add(4);
			indices.Add(6);
			indices.Add(7);
			indices.Add(4);
			indices.Add(7);
			indices.Add(5);
			vertices.Add(new VertexPositionColor(corners[4], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[5], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[6], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[7], debugColorBB));
			indices.Add(9);
			indices.Add(8);
			indices.Add(11);
			indices.Add(9);
			indices.Add(11);
			indices.Add(10);
			vertices.Add(new VertexPositionColor(corners[0], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[3], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[4], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[7], debugColorBB));
			indices.Add(14);
			indices.Add(12);
			indices.Add(13);
			indices.Add(14);
			indices.Add(13);
			indices.Add(15);
			vertices.Add(new VertexPositionColor(corners[0], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[1], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[4], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[5], debugColorBB));
			indices.Add(16);
			indices.Add(19);
			indices.Add(17);
			indices.Add(16);
			indices.Add(18);
			indices.Add(19);
			vertices.Add(new VertexPositionColor(corners[2], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[3], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[6], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[7], debugColorBB));
			indices.Add(21);
			indices.Add(20);
			indices.Add(22);
			indices.Add(21);
			indices.Add(22);
			indices.Add(23);
		}
	}

	public static class DisplayMarker
	{
		public static void GetShapeMeshData(List<VertexPositionColor> vertices, List<int> indices)
		{
			Vector3[] corners = new BoundingBox(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f)).GetCorners();
			vertices.Add(new VertexPositionColor(corners[0], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[1], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[2], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[3], debugColorBB));
			indices.Add(0);
			indices.Add(1);
			indices.Add(2);
			indices.Add(0);
			indices.Add(2);
			indices.Add(3);
			vertices.Add(new VertexPositionColor(corners[1], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[2], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[5], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[6], debugColorBB));
			indices.Add(4);
			indices.Add(6);
			indices.Add(7);
			indices.Add(4);
			indices.Add(7);
			indices.Add(5);
			vertices.Add(new VertexPositionColor(corners[4], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[5], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[6], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[7], debugColorBB));
			indices.Add(9);
			indices.Add(8);
			indices.Add(11);
			indices.Add(9);
			indices.Add(11);
			indices.Add(10);
			vertices.Add(new VertexPositionColor(corners[0], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[3], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[4], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[7], debugColorBB));
			indices.Add(14);
			indices.Add(12);
			indices.Add(13);
			indices.Add(14);
			indices.Add(13);
			indices.Add(15);
			vertices.Add(new VertexPositionColor(corners[0], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[1], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[4], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[5], debugColorBB));
			indices.Add(16);
			indices.Add(19);
			indices.Add(17);
			indices.Add(16);
			indices.Add(18);
			indices.Add(19);
			vertices.Add(new VertexPositionColor(corners[2], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[3], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[6], debugColorBB));
			vertices.Add(new VertexPositionColor(corners[7], debugColorBB));
			indices.Add(21);
			indices.Add(20);
			indices.Add(22);
			indices.Add(21);
			indices.Add(22);
			indices.Add(23);
		}
	}

	public static class DisplayArrow
	{
		public static void GetShapeMeshData(List<VertexPositionColor> vertices, List<int> indices)
		{
			vertices.Add(new VertexPositionColor(new Vector3(0f, 0f, 0f), Color.White));
			vertices.Add(new VertexPositionColor(new Vector3(0f, 0f, 1f), Color.White));
			vertices.Add(new VertexPositionColor(new Vector3(0f, 0.1f, 0f), Color.White));
			indices.Add(0);
			indices.Add(1);
			indices.Add(2);
			indices.Add(2);
			indices.Add(1);
			indices.Add(0);
		}
	}

	public static class DisplayLine
	{
		public static void GetShapeMeshData(List<VertexPositionColor> vertices, List<int> indices, Vector3 start, Vector3 end)
		{
			Color color = new Color(1f, 0f, 0f, 0.5f);
			vertices.Add(new VertexPositionColor(start, color));
			vertices.Add(new VertexPositionColor(end, color));
			vertices.Add(new VertexPositionColor(start + new Vector3(0f, 0.0001f, 0f), color));
			indices.Add(0);
			indices.Add(1);
			indices.Add(2);
			indices.Add(2);
			indices.Add(1);
			indices.Add(0);
		}
	}

	private static Color debugColorStaticMesh = Color.White;

	private static Color debugColorSphere = Color.Green;

	private static Color debugColorBox = Color.Red;

	private static Color debugColorCapsule = Color.Blue;

	private static Color debugColorBB = Color.Yellow;

	private BasicEffect basicEffect;

	private GraphicsDevice device;

	private List<VertexPositionColor> vertices;

	private List<int> indices;

	private List<VertexPositionColor> vertices2;

	private List<int> indices2;

	private RasterizerState wireFrameState;

	private RasterizerState oldFrameState;

	public BEPUDebugDrawer(GraphicsDevice g)
	{
		device = g;
		basicEffect = new BasicEffect(device);
		basicEffect.AmbientLightColor = Vector3.One;
		basicEffect.VertexColorEnabled = true;
		wireFrameState = new RasterizerState();
		wireFrameState.FillMode = FillMode.WireFrame;
	}

	public void Dispose()
	{
		if (basicEffect != null)
		{
			basicEffect.Dispose();
		}
		if (vertices != null)
		{
			vertices.Clear();
		}
		if (indices != null)
		{
			indices.Clear();
		}
	}

	public void Draw(ISpaceObject shape, Matrix viewMatrix, Matrix projectionMatrix)
	{
		vertices = new List<VertexPositionColor>();
		indices = new List<int>();
		if (shape is Capsule)
		{
			DisplayCapsule.GetShapeMeshData(shape, vertices, indices);
		}
		else if (shape is Box)
		{
			DisplayBox.GetShapeMeshData(shape, vertices, indices);
		}
		else if (shape is Sphere)
		{
			DisplaySphere.GetShapeMeshData(shape, vertices, indices);
		}
		else if (shape is StaticMesh)
		{
			DisplayStaticMesh.GetShapeMeshData(shape, vertices, indices);
		}
		else if (shape is Cylinder)
		{
			DisplayCylinder.GetShapeMeshData(shape, vertices, indices);
		}
		bool flag = false;
		if (shape is Capsule)
		{
			flag = true;
		}
		else if (shape is Box)
		{
			flag = true;
		}
		else if (shape is Sphere)
		{
			flag = true;
		}
		else if (shape is StaticMesh)
		{
			flag = true;
		}
		else if (shape is Cylinder)
		{
			flag = true;
		}
		if (!flag || vertices.Count <= 0 || indices.Count <= 0)
		{
			return;
		}
		device.BlendState = BlendState.Opaque;
		device.DepthStencilState = DepthStencilState.Default;
		oldFrameState = device.RasterizerState;
		device.RasterizerState = wireFrameState;
		basicEffect.View = viewMatrix;
		basicEffect.Projection = projectionMatrix;
		if (shape is Capsule)
		{
			basicEffect.World = (shape as Capsule).WorldTransform;
		}
		else if (shape is Box)
		{
			basicEffect.World = (shape as Box).WorldTransform;
		}
		else if (shape is Sphere)
		{
			basicEffect.World = (shape as Sphere).WorldTransform;
		}
		else if (shape is StaticMesh)
		{
			basicEffect.World = (shape as StaticMesh).WorldTransform.Matrix;
		}
		else if (shape is Cylinder)
		{
			basicEffect.World = (shape as Cylinder).WorldTransform;
		}
		foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
		{
			pass.Apply();
			device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices.ToArray(), 0, vertices.Count, indices.ToArray(), 0, indices.Count / 3);
		}
		device.RasterizerState = oldFrameState;
	}

	public void Draw(SceneEntity shape, Matrix viewMatrix, Matrix projectionMatrix)
	{
		vertices2 = new List<VertexPositionColor>();
		indices2 = new List<int>();
		DisplayBoundingBox.GetShapeMeshData(shape, vertices2, indices2);
		if (vertices2.Count <= 0 || indices2.Count <= 0)
		{
			return;
		}
		device.BlendState = BlendState.Opaque;
		device.DepthStencilState = DepthStencilState.Default;
		oldFrameState = device.RasterizerState;
		device.RasterizerState = wireFrameState;
		basicEffect.View = viewMatrix;
		basicEffect.Projection = projectionMatrix;
		basicEffect.World = Matrix.Identity;
		foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
		{
			pass.Apply();
			device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices2.ToArray(), 0, vertices2.Count, indices2.ToArray(), 0, indices2.Count / 3);
		}
		device.RasterizerState = oldFrameState;
	}

	public void Draw(Vector3 pos, Matrix viewMatrix, Matrix projectionMatrix)
	{
		vertices2 = new List<VertexPositionColor>();
		indices2 = new List<int>();
		DisplayMarker.GetShapeMeshData(vertices2, indices2);
		if (vertices2.Count <= 0 || indices2.Count <= 0)
		{
			return;
		}
		device.BlendState = BlendState.Opaque;
		device.DepthStencilState = DepthStencilState.Default;
		oldFrameState = device.RasterizerState;
		device.RasterizerState = wireFrameState;
		basicEffect.View = viewMatrix;
		basicEffect.Projection = projectionMatrix;
		basicEffect.World = Matrix.CreateTranslation(pos);
		foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
		{
			pass.Apply();
			device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices2.ToArray(), 0, vertices2.Count, indices2.ToArray(), 0, indices2.Count / 3);
		}
		device.RasterizerState = oldFrameState;
	}

	public void Draw(Matrix world, Matrix viewMatrix, Matrix projectionMatrix)
	{
		vertices2 = new List<VertexPositionColor>();
		indices2 = new List<int>();
		DisplayArrow.GetShapeMeshData(vertices2, indices2);
		if (vertices2.Count <= 0 || indices2.Count <= 0)
		{
			return;
		}
		device.BlendState = BlendState.Opaque;
		device.DepthStencilState = DepthStencilState.Default;
		oldFrameState = device.RasterizerState;
		device.RasterizerState = wireFrameState;
		basicEffect.View = viewMatrix;
		basicEffect.Projection = projectionMatrix;
		basicEffect.World = world;
		foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
		{
			pass.Apply();
			device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices2.ToArray(), 0, vertices2.Count, indices2.ToArray(), 0, indices2.Count / 3);
		}
		device.RasterizerState = oldFrameState;
	}

	public void Draw(Matrix world, Vector3 start, Vector3 end, Matrix viewMatrix, Matrix projectionMatrix)
	{
		vertices2 = new List<VertexPositionColor>();
		indices2 = new List<int>();
		DisplayLine.GetShapeMeshData(vertices2, indices2, start, end);
		if (vertices2.Count <= 0 || indices2.Count <= 0)
		{
			return;
		}
		device.BlendState = BlendState.Opaque;
		device.DepthStencilState = DepthStencilState.Default;
		oldFrameState = device.RasterizerState;
		device.RasterizerState = wireFrameState;
		basicEffect.View = viewMatrix;
		basicEffect.Projection = projectionMatrix;
		basicEffect.World = Matrix.Identity;
		foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
		{
			pass.Apply();
			device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices2.ToArray(), 0, vertices2.Count, indices2.ToArray(), 0, indices2.Count / 3);
		}
		device.RasterizerState = oldFrameState;
	}
}
