using System;
using Microsoft.Xna.Framework;

namespace Saturn9;

public class CameraManager
{
	private const float DEFAULT_FOV_DEGREES = 45f;

	public Vector3 m_Position;

	public Vector3 m_LookAt;

	public Matrix m_ProjectionMatrix;

	public Matrix m_ViewMatrix;

	private float m_Fov;

	public float m_TargetFov;

	public Vector3 m_Up = Vector3.Up;

	public float m_BobY;

	public float m_Pitch;

	public float m_ZTilt;

	public float m_ZTiltTarget;

	public float m_ZTiltDir = 1f;

	public int m_Helmet = -1;

	private Matrix m_MtxRot = Matrix.Identity;

	private Quaternion m_Quat = Quaternion.Identity;

	public bool m_ShakeyCam;

	public int m_LookAtPlayerId = -1;

	private float m_BlendOutTime;

	public void Init()
	{
		m_ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45f), g.m_App.graphics.GraphicsDevice.Viewport.AspectRatio, 0.1f, 200f);
		m_BobY = 0f;
		m_Pitch = 0f;
		m_ZTilt = 0f;
		m_ZTiltTarget = 0f;
		m_Fov = 45f;
		m_TargetFov = 45f;
		m_Up = Vector3.Up;
		m_BlendOutTime = 0f;
		m_LookAtPlayerId = -1;
	}

	public void Update()
	{
		switch (g.m_PlayerManager.GetLocalPlayer().m_State)
		{
		case Player.STATE.JoinTeam:
		case Player.STATE.ChooseCharacter:
			m_Position = new Vector3(110f, 4.3f, -68f);
			m_LookAt = new Vector3(119f, 3.11f, -73f);
			m_ViewMatrix = Matrix.CreateLookAt(m_Position, m_LookAt, Vector3.Up);
			break;
		case Player.STATE.Grabbed:
			if (m_LookAtPlayerId != -1)
			{
				Matrix boneAbsoluteTransform = g.m_PlayerManager.m_Player[m_LookAtPlayerId].m_AnimationSet.GetBoneAbsoluteTransform("Bip01_Head");
				boneAbsoluteTransform *= g.m_PlayerManager.m_Player[m_LookAtPlayerId].m_SceneObject.World;
				m_Position = boneAbsoluteTransform.Translation - boneAbsoluteTransform.Forward * 1f + new Vector3(0f, 0.25f, 0f);
				Vector3 vector = boneAbsoluteTransform.Translation + new Vector3(0f, 0.25f, 0f);
				m_LookAt.X = MathHelper.Lerp(m_LookAt.X, vector.X, 0.5f);
				m_LookAt.Y = MathHelper.Lerp(m_LookAt.Y, vector.Y, 0.5f);
				m_LookAt.Z = MathHelper.Lerp(m_LookAt.Z, vector.Z, 0.5f);
				m_ViewMatrix = Matrix.CreateLookAt(m_Position, m_LookAt, m_Up);
				m_ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(m_Fov), g.m_App.graphics.GraphicsDevice.Viewport.AspectRatio, 0.1f, g.m_App.environment.VisibleDistance);
				if (m_Helmet != -1)
				{
					Matrix world = Matrix.CreateWorld(m_Position, Vector3.Normalize(m_LookAt - m_Position), Vector3.Up);
					g.m_ItemManager.m_Item[m_Helmet].m_SceneObject.World = world;
				}
			}
			break;
		case Player.STATE.InGame:
		case Player.STATE.UsingLCD1:
		case Player.STATE.UsingLCD2:
		case Player.STATE.UsingLCD3:
		case Player.STATE.UsingSafe:
		case Player.STATE.UsingLCD4:
		{
			Matrix matrix2 = Matrix.CreateRotationY(MathF.PI) * g.m_PlayerManager.GetLocalPlayer().m_ViewAnimationSet.GetBoneAbsoluteTransform("Bip01_Head");
			matrix2 *= g.m_PlayerManager.GetLocalPlayer().m_ViewSceneObject.World;
			m_Position = matrix2.Translation;
			Matrix mtxRot = Matrix.Identity;
			if (!m_ShakeyCam)
			{
				m_Position += GetBob();
				mtxRot = Matrix.CreateRotationX(m_Pitch + g.m_PlayerManager.GetLocalPlayer().m_PunchAngle) * Matrix.CreateRotationY(g.m_PlayerManager.GetLocalPlayer().m_Rotation.Y);
				if (m_LookAtPlayerId != -1 && g.m_PlayerManager.m_Player[m_LookAtPlayerId].m_Id != -1)
				{
					float radians = (float)Math.Atan2(g.m_PlayerManager.GetLocalPlayer().m_Position.X - g.m_PlayerManager.m_Player[m_LookAtPlayerId].m_Position.X, g.m_PlayerManager.GetLocalPlayer().m_Position.Z - g.m_PlayerManager.m_Player[m_LookAtPlayerId].m_Position.Z);
					mtxRot = Matrix.CreateRotationY(radians);
					Vector3 vector2 = m_Position + mtxRot.Forward * 10f;
					m_LookAt.X = MathHelper.Lerp(m_LookAt.X, vector2.X, 0.25f);
					m_LookAt.Y = MathHelper.Lerp(m_LookAt.Y, vector2.Y, 0.25f);
					m_LookAt.Z = MathHelper.Lerp(m_LookAt.Z, vector2.Z, 0.25f);
					m_BlendOutTime = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 0.5f;
				}
				else
				{
					float amount = 1f;
					if (m_BlendOutTime > (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds)
					{
						amount = 0.25f;
					}
					Vector3 vector3 = m_Position + mtxRot.Forward * 10f;
					m_LookAt.X = MathHelper.Lerp(m_LookAt.X, vector3.X, amount);
					m_LookAt.Y = MathHelper.Lerp(m_LookAt.Y, vector3.Y, amount);
					m_LookAt.Z = MathHelper.Lerp(m_LookAt.Z, vector3.Z, amount);
				}
				UpdateTilt();
				Vector3 up = Vector3.Up;
				m_Up = Vector3.Transform(up, Matrix.CreateFromAxisAngle(mtxRot.Forward, m_ZTilt));
				m_ViewMatrix = Matrix.CreateLookAt(m_Position, m_LookAt, m_Up);
			}
			else
			{
				GetBob();
				Vector3 vector4 = m_Position + matrix2.Forward * 10f;
				m_LookAt.X = MathHelper.Lerp(m_LookAt.X, vector4.X, 0.25f);
				m_LookAt.Y = MathHelper.Lerp(m_LookAt.Y, vector4.Y, 0.25f);
				m_LookAt.Z = MathHelper.Lerp(m_LookAt.Z, vector4.Z, 0.25f);
				m_Up.X = MathHelper.Lerp(m_Up.X, matrix2.Right.X, 0.25f);
				m_Up.Y = MathHelper.Lerp(m_Up.Y, matrix2.Right.Y, 0.25f);
				m_Up.Z = MathHelper.Lerp(m_Up.Z, matrix2.Right.Z, 0.25f);
				m_ViewMatrix = Matrix.CreateLookAt(m_Position, m_LookAt, m_Up);
				m_BlendOutTime = (float)g.m_App.m_GameTime.TotalGameTime.TotalSeconds + 1f;
			}
			m_Fov = MathHelper.Lerp(m_Fov, m_TargetFov, 0.1f);
			m_ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(m_Fov), g.m_App.graphics.GraphicsDevice.Viewport.AspectRatio, 0.1f, g.m_App.environment.VisibleDistance);
			m_MtxRot = mtxRot;
			m_MtxRot.Translation = Vector3.Zero;
			if (m_Helmet != -1)
			{
				Matrix world2 = Matrix.CreateWorld(m_Position, Vector3.Normalize(m_LookAt - m_Position), Vector3.Up);
				g.m_ItemManager.m_Item[m_Helmet].m_SceneObject.World = world2;
			}
			break;
		}
		case Player.STATE.LocalDeath:
		{
			m_Position.X = g.m_PlayerManager.GetLocalPlayer().m_CharacterController.Body.Position.X;
			m_Position.Y = g.m_PlayerManager.GetLocalPlayer().m_CharacterController.Body.Position.Y;
			m_Position.Z = g.m_PlayerManager.GetLocalPlayer().m_CharacterController.Body.Position.Z;
			float num = g.m_PlayerManager.GetLocalPlayer().m_Position.Y - 2.286f;
			if (m_Position.Y > num)
			{
				m_Position.Y -= 0.25f;
			}
			else
			{
				m_Position.Y = num;
			}
			Matrix matrix = Matrix.CreateRotationX(m_Pitch + g.m_PlayerManager.GetLocalPlayer().m_PunchAngle) * g.m_PlayerManager.GetLocalPlayer().m_ViewSceneObject.World;
			m_LookAt = m_Position + matrix.Forward * 10f;
			m_ViewMatrix = Matrix.CreateLookAt(m_Position, m_LookAt, Vector3.Up);
			m_ViewMatrix *= Matrix.CreateRotationZ(MathHelper.ToRadians(20f));
			m_Fov = MathHelper.Lerp(m_Fov, m_TargetFov, 0.1f);
			m_ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(m_Fov), g.m_App.graphics.GraphicsDevice.Viewport.AspectRatio, 0.1f, g.m_App.environment.VisibleDistance);
			m_MtxRot = Matrix.CreateRotationZ(MathHelper.ToRadians(-20f)) * matrix;
			m_MtxRot.Translation = Vector3.Zero;
			g.m_ItemManager.m_Item[m_Helmet].m_SceneObject.World = m_MtxRot * Matrix.CreateTranslation(m_Position);
			break;
		}
		case Player.STATE.Intermission:
			break;
		}
	}

	private void UpdateTilt()
	{
		if (g.m_PlayerManager.GetLocalPlayer().m_Hallucinate)
		{
			if (g.m_PlayerManager.GetLocalPlayer().m_OceanSFX != null)
			{
				m_ZTiltDir += 0.06f;
				m_ZTiltTarget = 0.7f * (float)Math.Sin(m_ZTiltDir);
			}
			else
			{
				m_ZTiltDir += 0.01f;
				m_ZTiltTarget = 0.2f * (float)Math.Sin(m_ZTiltDir);
			}
		}
		else
		{
			m_ZTiltTarget = 0f;
		}
		m_ZTilt = MathHelper.Lerp(m_ZTilt, m_ZTiltTarget, 0.01f);
	}

	public Vector3 GetBob()
	{
		if (!g.m_PlayerManager.GetLocalPlayer().m_CharacterController.SupportFinder.HasTraction)
		{
			return Vector3.Zero;
		}
		Vector3 vector = g.m_PlayerManager.GetLocalPlayer().m_FrameMove * 0.25f;
		vector.Y = 0f;
		float num = vector.Length();
		float num2 = (float)Math.Sin(m_BobY);
		float num3 = (float)Math.Sin(m_BobY * 0.5f);
		if (num > 1E-06f)
		{
			g.m_SoundManager.PlayLocalFootsteps(num2);
		}
		num2 *= 0.75f;
		num2 *= num;
		m_BobY += 9f * num;
		num3 *= 0.75f;
		num3 *= num;
		Vector3 vector2 = Vector3.Transform(new Vector3(num3, 0f, 0f), Matrix.CreateRotationY(g.m_PlayerManager.GetLocalPlayer().m_Rotation.Y));
		return new Vector3(0f, 0f - num2, 0f) + vector2;
	}

	public Vector3 WorldToScreen(Vector3 position)
	{
		Matrix identity = Matrix.Identity;
		identity.Translation = position;
		return g.m_App.GraphicsDevice.Viewport.Project(Vector3.Zero, m_ProjectionMatrix, m_ViewMatrix, identity);
	}

	public void SetTargetFov(float degrees)
	{
		m_TargetFov = degrees;
	}

	public void SetDefaultFov()
	{
		m_TargetFov = 45f;
	}

	public Vector3 QuaternionToEuler(Quaternion q)
	{
		Vector3 result = default(Vector3);
		result.X = (float)Math.Atan2(2f * q.Y * q.W - 2f * q.X * q.Z, 1.0 - 2.0 * Math.Pow(q.Y, 2.0) - 2.0 * Math.Pow(q.Z, 2.0));
		result.Y = (float)Math.Asin(2f * q.X * q.Y + 2f * q.Z * q.W);
		result.Z = (float)Math.Atan2(2f * q.X * q.W - 2f * q.Y * q.Z, 1.0 - 2.0 * Math.Pow(q.X, 2.0) - 2.0 * Math.Pow(q.Z, 2.0));
		if ((double)(q.X * q.Y + q.Z * q.W) == 0.5)
		{
			result.X = (float)(2.0 * Math.Atan2(q.X, q.W));
			result.Z = 0f;
		}
		else if ((double)(q.X * q.Y + q.Z * q.W) == -0.5)
		{
			result.X = (float)(-2.0 * Math.Atan2(q.X, q.W));
			result.Z = 0f;
		}
		return result;
	}
}
