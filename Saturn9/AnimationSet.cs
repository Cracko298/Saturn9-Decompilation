using System;
using Microsoft.Xna.Framework;
using SgMotion;
using SgMotion.Controllers;
using SynapseGaming.LightingSystem.Rendering;

namespace Saturn9;

public class AnimationSet
{
	public const int FULLBODY = 0;

	public const int UPPER = 1;

	private AnimationController[] m_AnimationController;

	private int m_NumControllers;

	public Matrix[] m_SkinTransforms;

	public bool HasFinished => m_AnimationController[0].hasFinished;

	public InterpolationMode TranslationInterpolation
	{
		get
		{
			return m_AnimationController[0].translationInterpolation;
		}
		set
		{
			m_AnimationController[0].translationInterpolation = value;
		}
	}

	public InterpolationMode OrientationInterpolation
	{
		get
		{
			return m_AnimationController[0].orientationInterpolation;
		}
		set
		{
			m_AnimationController[0].orientationInterpolation = value;
		}
	}

	public InterpolationMode ScaleInterpolation
	{
		get
		{
			return m_AnimationController[0].scaleInterpolation;
		}
		set
		{
			m_AnimationController[0].scaleInterpolation = value;
		}
	}

	public Matrix[] SkinnedBoneTransforms => m_AnimationController[0].skinnedBoneTransforms;

	public bool LoopEnabled
	{
		get
		{
			return m_AnimationController[0].loopEnabled;
		}
		set
		{
			m_AnimationController[0].loopEnabled = value;
			if (m_AnimationController[0].hasFinished && m_AnimationController[0].loopEnabled)
			{
				m_AnimationController[0].hasFinished = false;
			}
		}
	}

	public TimeSpan Time
	{
		get
		{
			return m_AnimationController[0].time;
		}
		set
		{
			m_AnimationController[0].time = value;
		}
	}

	public float Speed
	{
		get
		{
			return m_AnimationController[0].speed;
		}
		set
		{
			m_AnimationController[0].speed = value;
		}
	}

	public bool IsPlaying => m_AnimationController[0].isPlaying;

	public bool IsUpperPlaying => m_AnimationController[1].isPlaying;

	public float DeltaZ => m_AnimationController[0].m_DeltaZ;

	public bool UseMotionExtraction
	{
		get
		{
			return m_AnimationController[0].m_bUseMotionExtraction;
		}
		set
		{
			m_AnimationController[0].m_bUseMotionExtraction = value;
		}
	}

	public AnimationSet(int numControllers, SkinnedModelBoneCollection skeleton)
	{
		m_NumControllers = numControllers;
		m_AnimationController = new AnimationController[numControllers];
		for (int i = 0; i < numControllers; i++)
		{
			m_AnimationController[i] = new AnimationController(skeleton);
		}
		m_SkinTransforms = new Matrix[skeleton.Count];
	}

	public void Update(TimeSpan elapsedTime, Matrix parent)
	{
		m_AnimationController[0].Update(elapsedTime, Matrix.Identity);
		if (m_NumControllers == 2)
		{
			m_AnimationController[1].Update(elapsedTime, m_AnimationController[0].skinnedBoneTransforms[0]);
		}
	}

	public void CopyCombinedBoneTransforms(SceneObject s)
	{
		if (m_NumControllers == 1)
		{
			s.SkinBones = m_AnimationController[0].SkinnedBoneTransforms;
			return;
		}
		for (int i = 0; i < m_AnimationController[0].skeleton.Count; i++)
		{
			if (m_AnimationController[0].GetBoneMask(i))
			{
				ref Matrix reference = ref m_SkinTransforms[i];
				reference = m_AnimationController[0].SkinnedBoneTransforms[i];
			}
		}
		if (m_AnimationController[1].IsPlaying)
		{
			for (int j = 0; j < m_AnimationController[1].skeleton.Count; j++)
			{
				if (m_AnimationController[1].GetBoneMask(j))
				{
					ref Matrix reference2 = ref m_SkinTransforms[j];
					reference2 = m_AnimationController[1].SkinnedBoneTransforms[j];
				}
			}
		}
		s.SkinBones = m_SkinTransforms;
	}

	public void PlayUpperBodyAnim(AnimationClip animationClip, bool loop, TimeSpan blend)
	{
		CrossFade(1, animationClip, blend);
		m_AnimationController[1].LoopEnabled = loop;
	}

	public Matrix GetBoneAbsoluteTransform(string boneName)
	{
		if (m_NumControllers == 1)
		{
			return m_AnimationController[0].GetBoneAbsoluteTransform(boneName);
		}
		if (m_AnimationController[1].IsPlaying)
		{
			return m_AnimationController[1].GetBoneAbsoluteTransform(boneName);
		}
		return m_AnimationController[0].GetBoneAbsoluteTransform(boneName);
	}

	public int GetBoneId(string name)
	{
		if (m_NumControllers == 1)
		{
			return m_AnimationController[0].GetBoneId(name);
		}
		if (m_AnimationController[1].IsPlaying)
		{
			return m_AnimationController[1].GetBoneId(name);
		}
		return m_AnimationController[0].GetBoneId(name);
	}

	public void StartClip(AnimationClip animationClip)
	{
		m_AnimationController[0].StartClip(animationClip);
	}

	public void StartClip(int idx, AnimationClip animationClip)
	{
		m_AnimationController[idx].StartClip(animationClip);
	}

	public void StopClip()
	{
		m_AnimationController[0].StopClip();
	}

	public void StopClip(int idx)
	{
		m_AnimationController[idx].StopClip();
	}

	public void SetBoneController(string boneName, ref Matrix angles)
	{
		m_AnimationController[0].SetBoneController(boneName, ref angles);
		if (m_NumControllers == 2)
		{
			m_AnimationController[1].SetBoneController(boneName, ref angles);
		}
	}

	public void SetBoneControllerAdditive(string boneName, ref Matrix angles)
	{
		m_AnimationController[0].SetBoneControllerAdditive(boneName, ref angles);
		if (m_NumControllers == 2)
		{
			m_AnimationController[1].SetBoneControllerAdditive(boneName, ref angles);
		}
	}

	public void CrossFade(AnimationClip animationClip, TimeSpan fadeTime)
	{
		m_AnimationController[0].CrossFade(animationClip, fadeTime);
	}

	public void CrossFade(int idx, AnimationClip animationClip, TimeSpan fadeTime)
	{
		m_AnimationController[idx].CrossFade(animationClip, fadeTime);
	}

	public void ClearBoneController(string boneName)
	{
		m_AnimationController[0].ClearBoneController(boneName);
		if (m_NumControllers == 2)
		{
			m_AnimationController[1].ClearBoneController(boneName);
		}
	}

	public void ClearBoneControllers()
	{
		m_AnimationController[0].ClearBoneControllers();
		if (m_NumControllers == 2)
		{
			m_AnimationController[1].ClearBoneControllers();
		}
	}

	public void ClearBoneMasks(int idx)
	{
		m_AnimationController[idx].ClearBoneMasks();
	}

	public void SetBoneMask(int idx, string name)
	{
		m_AnimationController[idx].SetBoneMask(name);
	}
}
