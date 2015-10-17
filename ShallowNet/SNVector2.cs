using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShallowNet
{
	public struct SNVector2
	{
		public float x, y;

		public SNVector2(float x, float y)
		{
			this.x = x;
			this.y = y;
		}

		public SNVector2(SNVector2 other)
		{
			this.x = other.x;
			this.y = other.y;
		}

		public override string ToString()
		{
			return string.Format("Vector2({0}, {1})", x, y);
		}

		public static SNVector2 operator +(SNVector2 a, SNVector2 b)
		{
			return new SNVector2(a.x + b.x, a.y + b.y);
		}

		public static SNVector2 operator -(SNVector2 a, SNVector2 b)
		{
			return new SNVector2(a.x - b.x, a.y - b.y);
		}

		public static SNVector2 operator *(float s, SNVector2 v)
		{
			return new SNVector2(s * v.x, s * v.y);
		}

		public static SNVector2 operator *(SNVector2 v, float s)
		{
			return new SNVector2(s * v.x, s * v.y);
		}

		public static SNVector2 operator /(SNVector2 v, float s)
		{
			return new SNVector2(v.x / s, v.y / s);
		}
	}
}
