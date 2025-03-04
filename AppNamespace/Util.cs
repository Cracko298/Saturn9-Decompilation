using System;
using Microsoft.Xna.Framework;

namespace AppNamespace;

public class Util
{
	public static Vector3 SmoothRamp(Vector3 From, Vector3 To, float Time, Vector3 Vel)
	{
		CalcAccelReq(From, To, Time, Vel);
		float num = (From - To).Length() / Time;
		if (Vel.LengthSquared() > num * num)
		{
			Vel.Normalize();
			Vel *= num;
		}
		return From + Vel;
	}

	public static Vector3 CalcAccelReq(Vector3 start, Vector3 end, float t, Vector3 Vel)
	{
		return (end - start - Vel * t) / (0.5f * t * t);
	}

	public static Vector3 CalcVelocityReq(Vector3 start, Vector3 end, float t, float g)
	{
		Vector3 vector = new Vector3(0f, 0f - g, 0f);
		return (end - start - 0.5f * vector * (t * t)) / t;
	}

	public static Vector2 CalcVelocityReq(Vector2 start, Vector2 end, float t, float g)
	{
		Vector2 vector = new Vector2(0f, 0f - g);
		return (end - start - 0.5f * vector * (t * t)) / t;
	}

	public static float CalcLaunchAngle(float V, float X, float Y, float G, bool bHigh)
	{
		float num = V * V;
		float num2 = num * num - G * (G * X * X + 2f * Y * num);
		if (num2 < 0f)
		{
			return MathF.PI / 4f;
		}
		float num3 = (float)Math.Sqrt(num2);
		float num4 = G * X;
		if (bHigh)
		{
			return (float)Math.Atan((num + num3) / num4);
		}
		return (float)Math.Atan((num - num3) / num4);
	}
}
