using Microsoft.Xna.Framework;

namespace Saturn9;

internal interface IMessageDisplay : IDrawable, IUpdateable
{
	void ShowMessage(string message, params object[] parameters);
}
