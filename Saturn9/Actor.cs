using Microsoft.Xna.Framework;
using SgMotion;
using SynapseGaming.LightingSystem.Rendering;

namespace Saturn9;

public class Actor
{
	public int m_Id;

	public Vector3 m_Position;

	public Vector3 m_PrevPosition;

	public Vector3 m_Rotation;

	public Vector3 m_PrevRotation;

	public Vector3 m_NetworkPosition;

	public float m_NetworkRotation;

	public SkinnedModel m_Model;

	public SceneObject m_SceneObject;

	public Matrix[] m_Transforms = new Matrix[75];

	public Actor()
	{
		m_Id = -1;
		m_Position = Vector3.Zero;
		m_Rotation = Vector3.Zero;
		m_PrevPosition = Vector3.Zero;
		m_PrevRotation = Vector3.Zero;
		m_NetworkPosition = Vector3.Zero;
		m_NetworkRotation = 0f;
		m_Model = null;
		m_SceneObject = null;
	}

	public Vector3 Position()
	{
		return m_Position;
	}

	public Vector3 Rotation()
	{
		return m_Rotation;
	}

	public virtual void SetPosition(Vector3 pos)
	{
		m_Position = pos;
	}

	public virtual void SetRotation(Vector3 rot)
	{
		m_Rotation = rot;
	}

	public void SetModel(SkinnedModel m)
	{
		m_Model = m;
	}
}
