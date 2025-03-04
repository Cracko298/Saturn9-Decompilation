using System;
using Microsoft.Xna.Framework;

namespace Maths;

public class Fn
{
	public static Vector2 vUp = new Vector2(0f, 1f);

	public static float DegToRad(float deg)
	{
		return deg * (MathF.PI / 180f);
	}

	public static void Rotate(ref Vector2 vec, float ang)
	{
		float x = vec.X;
		float num = (float)Math.Cos(0f - ang);
		float num2 = (float)Math.Sin(0f - ang);
		vec.X = vec.X * num - vec.Y * num2;
		vec.Y = x * num2 + vec.Y * num;
	}

	public static float ClerpT(float from, float to, float step, float tol)
	{
		float num = NormRot(to - from) * step;
		if (Math.Abs(num) > tol)
		{
			num = ((!(num < 0f)) ? tol : (0f - tol));
		}
		return from + num;
	}

	public static float Clerp(float from, float to, float step)
	{
		float num = MathHelper.WrapAngle(to - from) * step;
		return from + num;
	}

	public static float DotProduct(Vector2 a, Vector2 b)
	{
		return a.X * b.X + a.Y * b.Y;
	}

	public static Vector2 ClosestPointOnLine(Vector2 l1, Vector2 l2, Vector2 pt)
	{
		Vector2 vector = l2 - l1;
		Vector2 value = pt - l1;
		if (Vector2.Dot(value, vector) < 0f)
		{
			return l1;
		}
		Vector2 vector2 = new Vector2(vector.X, vector.Y);
		vector2.Normalize();
		float num = Vector2.Dot(value, vector2);
		if (num * num > Vector2.Dot(vector, vector))
		{
			return l2;
		}
		return l1 + vector2 * num;
	}

	public static float NormRot(float x)
	{
		if (Math.Abs(x) < MathF.PI)
		{
			return x;
		}
		float num = x % (MathF.PI * 2f);
		if (Math.Abs(num) < MathF.PI)
		{
			return num;
		}
		if (x >= 0f)
		{
			return num - MathF.PI * 2f;
		}
		return num + MathF.PI * 2f;
	}

	public static float LerpCosT(float from, float to, float step, float tol)
	{
		float num = (to - from) * step;
		if (Math.Abs(num) > tol)
		{
			num = ((!(num < 0f)) ? tol : (0f - tol));
		}
		float num2 = (1f - (float)Math.Cos(num * MathF.PI)) * 0.5f;
		return from * (1f - num2) + to * num2;
	}

	public float Signed2DTriArea(Vector2 a, Vector2 b, Vector2 c)
	{
		return (a.X - c.X) * (b.Y - c.Y) - (a.Y - c.Y) * (b.X - c.X);
	}

	public int Test2DSegmentSegment(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t, Vector2 p)
	{
		float num = Signed2DTriArea(a, b, d);
		float num2 = Signed2DTriArea(a, b, c);
		if (num * num2 < 0f)
		{
			float num3 = Signed2DTriArea(c, d, a);
			float num4 = num3 + num2 - num;
			if (num3 * num4 < 0f)
			{
				t = num3 / (num3 - num4);
				p = a + t * (b - a);
				return 1;
			}
		}
		return 0;
	}
}
