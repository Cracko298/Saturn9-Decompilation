using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Saturn9;

public class InputState
{
	public const int MaxInputs = 4;

	public readonly KeyboardState[] CurrentKeyboardStates;

	public readonly GamePadState[] CurrentGamePadStates;

	public readonly KeyboardState[] LastKeyboardStates;

	public readonly GamePadState[] LastGamePadStates;

	public readonly bool[] GamePadWasConnected;

	public TouchCollection TouchState;

	public readonly List<GestureSample> Gestures = new List<GestureSample>();

	public InputState()
	{
		CurrentKeyboardStates = new KeyboardState[4];
		CurrentGamePadStates = new GamePadState[4];
		LastKeyboardStates = new KeyboardState[4];
		LastGamePadStates = new GamePadState[4];
		GamePadWasConnected = new bool[4];
	}

	public void Update()
	{
		for (int i = 0; i < 4; i++)
		{
			ref KeyboardState reference = ref LastKeyboardStates[i];
			reference = CurrentKeyboardStates[i];
			ref GamePadState reference2 = ref LastGamePadStates[i];
			reference2 = CurrentGamePadStates[i];
			ref KeyboardState reference3 = ref CurrentKeyboardStates[i];
			reference3 = Keyboard.GetState((PlayerIndex)i);
			ref GamePadState reference4 = ref CurrentGamePadStates[i];
			reference4 = GamePad.GetState((PlayerIndex)i);
			if (CurrentGamePadStates[i].IsConnected)
			{
				GamePadWasConnected[i] = true;
			}
		}
		TouchState = TouchPanel.GetState();
		Gestures.Clear();
		while (TouchPanel.IsGestureAvailable)
		{
			Gestures.Add(TouchPanel.ReadGesture());
		}
	}

	public bool IsNewKeyPress(Keys key, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
	{
		if (controllingPlayer.HasValue)
		{
			playerIndex = controllingPlayer.Value;
			int num = (int)playerIndex;
			if (CurrentKeyboardStates[num].IsKeyDown(key))
			{
				return LastKeyboardStates[num].IsKeyUp(key);
			}
			return false;
		}
		if (!IsNewKeyPress(key, PlayerIndex.One, out playerIndex) && !IsNewKeyPress(key, PlayerIndex.Two, out playerIndex) && !IsNewKeyPress(key, PlayerIndex.Three, out playerIndex))
		{
			return IsNewKeyPress(key, PlayerIndex.Four, out playerIndex);
		}
		return true;
	}

	public bool IsNewButtonPress(Buttons button, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
	{
		if (controllingPlayer.HasValue)
		{
			playerIndex = controllingPlayer.Value;
			int num = (int)playerIndex;
			if (CurrentGamePadStates[num].IsButtonDown(button))
			{
				return LastGamePadStates[num].IsButtonUp(button);
			}
			return false;
		}
		if (!IsNewButtonPress(button, PlayerIndex.One, out playerIndex) && !IsNewButtonPress(button, PlayerIndex.Two, out playerIndex) && !IsNewButtonPress(button, PlayerIndex.Three, out playerIndex))
		{
			return IsNewButtonPress(button, PlayerIndex.Four, out playerIndex);
		}
		return true;
	}

	public bool IsMenuSelect(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
	{
		if (!IsNewKeyPress(Keys.Space, controllingPlayer, out playerIndex) && !IsNewKeyPress(Keys.Enter, controllingPlayer, out playerIndex) && !IsNewButtonPress(Buttons.A, controllingPlayer, out playerIndex))
		{
			return IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex);
		}
		return true;
	}

	public bool IsMenuCancel(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
	{
		if (!IsNewKeyPress(Keys.Escape, controllingPlayer, out playerIndex) && !IsNewButtonPress(Buttons.B, controllingPlayer, out playerIndex))
		{
			return IsNewButtonPress(Buttons.Back, controllingPlayer, out playerIndex);
		}
		return true;
	}

	public bool IsMenuUp(PlayerIndex? controllingPlayer)
	{
		if (!IsNewKeyPress(Keys.Up, controllingPlayer, out var playerIndex) && !IsNewButtonPress(Buttons.DPadUp, controllingPlayer, out playerIndex))
		{
			return IsNewButtonPress(Buttons.LeftThumbstickUp, controllingPlayer, out playerIndex);
		}
		return true;
	}

	public bool IsMenuDown(PlayerIndex? controllingPlayer)
	{
		if (!IsNewKeyPress(Keys.Down, controllingPlayer, out var playerIndex) && !IsNewButtonPress(Buttons.DPadDown, controllingPlayer, out playerIndex))
		{
			return IsNewButtonPress(Buttons.LeftThumbstickDown, controllingPlayer, out playerIndex);
		}
		return true;
	}

	public bool IsPauseGame(PlayerIndex? controllingPlayer)
	{
		if (!IsNewKeyPress(Keys.Delete, controllingPlayer, out var playerIndex))
		{
			return IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex);
		}
		return true;
	}
}
