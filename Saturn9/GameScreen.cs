using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Saturn9;

public abstract class GameScreen
{
	private bool isPopup;

	private TimeSpan transitionOnTime = TimeSpan.Zero;

	private TimeSpan transitionOffTime = TimeSpan.Zero;

	private float transitionPosition = 1f;

	private ScreenState screenState;

	private bool isExiting;

	private bool otherScreenHasFocus;

	private ScreenManager screenManager;

	private PlayerIndex? controllingPlayer;

	private GestureType enabledGestures;

	public bool IsPopup
	{
		get
		{
			return isPopup;
		}
		protected set
		{
			isPopup = value;
		}
	}

	public TimeSpan TransitionOnTime
	{
		get
		{
			return transitionOnTime;
		}
		protected set
		{
			transitionOnTime = value;
		}
	}

	public TimeSpan TransitionOffTime
	{
		get
		{
			return transitionOffTime;
		}
		protected set
		{
			transitionOffTime = value;
		}
	}

	public float TransitionPosition
	{
		get
		{
			return transitionPosition;
		}
		protected set
		{
			transitionPosition = value;
		}
	}

	public float TransitionAlpha => 1f - TransitionPosition;

	public ScreenState ScreenState
	{
		get
		{
			return screenState;
		}
		protected set
		{
			screenState = value;
		}
	}

	public bool IsExiting
	{
		get
		{
			return isExiting;
		}
		protected internal set
		{
			isExiting = value;
		}
	}

	public bool IsActive
	{
		get
		{
			if (!otherScreenHasFocus)
			{
				if (screenState != 0)
				{
					return screenState == ScreenState.Active;
				}
				return true;
			}
			return false;
		}
	}

	public ScreenManager ScreenManager
	{
		get
		{
			return screenManager;
		}
		internal set
		{
			screenManager = value;
		}
	}

	public PlayerIndex? ControllingPlayer
	{
		get
		{
			return controllingPlayer;
		}
		internal set
		{
			controllingPlayer = value;
		}
	}

	public GestureType EnabledGestures
	{
		get
		{
			return enabledGestures;
		}
		protected set
		{
			enabledGestures = value;
			if (ScreenState == ScreenState.Active)
			{
				TouchPanel.EnabledGestures = value;
			}
		}
	}

	public virtual void LoadContent()
	{
	}

	public virtual void UnloadContent()
	{
	}

	public virtual void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		this.otherScreenHasFocus = otherScreenHasFocus;
		if (isExiting)
		{
			screenState = ScreenState.TransitionOff;
			if (!UpdateTransition(gameTime, transitionOffTime, 1))
			{
				ScreenManager.RemoveScreen(this);
			}
		}
		else if (coveredByOtherScreen)
		{
			if (UpdateTransition(gameTime, transitionOffTime, 1))
			{
				screenState = ScreenState.TransitionOff;
			}
			else
			{
				screenState = ScreenState.Hidden;
			}
		}
		else if (UpdateTransition(gameTime, transitionOnTime, -1))
		{
			screenState = ScreenState.TransitionOn;
		}
		else
		{
			screenState = ScreenState.Active;
		}
	}

	private bool UpdateTransition(GameTime gameTime, TimeSpan time, int direction)
	{
		float num = ((!(time == TimeSpan.Zero)) ? ((float)(gameTime.ElapsedGameTime.TotalMilliseconds / time.TotalMilliseconds)) : 1f);
		transitionPosition += num * (float)direction;
		if ((direction < 0 && transitionPosition <= 0f) || (direction > 0 && transitionPosition >= 1f))
		{
			transitionPosition = MathHelper.Clamp(transitionPosition, 0f, 1f);
			return false;
		}
		return true;
	}

	public virtual void HandleInput(InputState input)
	{
	}

	public virtual void Draw(GameTime gameTime)
	{
	}

	public void ExitScreen()
	{
		if (TransitionOffTime == TimeSpan.Zero)
		{
			ScreenManager.RemoveScreen(this);
		}
		else
		{
			isExiting = true;
		}
	}
}
