using Microsoft.Xna.Framework;
using SgMotion;
using SynapseGaming.LightingSystem.Rendering;

namespace Saturn9
{
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
      this.m_Id = -1;
      this.m_Position = Vector3.Zero;
      this.m_Rotation = Vector3.Zero;
      this.m_PrevPosition = Vector3.Zero;
      this.m_PrevRotation = Vector3.Zero;
      this.m_NetworkPosition = Vector3.Zero;
      this.m_NetworkRotation = 0.0f;
      this.m_Model = (SkinnedModel) null;
      this.m_SceneObject = (SceneObject) null;
    }

    public Vector3 Position() => this.m_Position;

    public Vector3 Rotation() => this.m_Rotation;

    public virtual void SetPosition(Vector3 pos) => this.m_Position = pos;

    public virtual void SetRotation(Vector3 rot) => this.m_Rotation = rot;

    public void SetModel(SkinnedModel m) => this.m_Model = m;
  }
}
