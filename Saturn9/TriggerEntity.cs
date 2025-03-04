using System;
using SynapseGaming.LightingSystem.Core;
using SynapseGaming.LightingSystem.Editor;
using SynapseGaming.LightingSystem.Rendering;

namespace Saturn9;

[Serializable]
[EditorCreatedObject]
public class TriggerEntity : SceneEntity
{
	public override void RenderEditorIcon(ISceneState scenestate, BoundingBoxRenderHelper renderhelper, bool highlighted, bool selected, bool sceneoccludedpass)
	{
		g.m_App.m_BEPUDebugDrawer.Draw(base.World, g.m_App.sceneState.View, g.m_App.sceneState.Projection);
		base.RenderEditorIcon(scenestate, renderhelper, highlighted, selected, sceneoccludedpass);
	}
}
