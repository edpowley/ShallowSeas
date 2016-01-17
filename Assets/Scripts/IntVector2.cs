using System;
using UnityEngine;

public struct IntVector2 : IEquatable<IntVector2>
{
    public readonly int X, Y;

    public IntVector2(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    public bool Equals(IntVector2 other)
    {
        return this.X == other.X && this.Y == other.Y;
    }

    public override bool Equals(object obj)
    {
        return obj is IntVector2 && this.Equals((IntVector2)obj);
    }

    public override int GetHashCode()
    {
        return X * 0x10000 + Y;
    }

    public override string ToString()
    {
        return string.Format("({0},{1})", X, Y);
    }

    public static bool operator ==(IntVector2 a, IntVector2 b) { return a.X == b.X && a.Y == b.Y; }
    public static bool operator !=(IntVector2 a, IntVector2 b) { return a.X != b.X || a.Y != b.Y; }

    public static IntVector2 operator +(IntVector2 a, IntVector2 b) { return new IntVector2(a.X + b.X, a.Y + b.Y); }
    public static IntVector2 operator -(IntVector2 a, IntVector2 b) { return new IntVector2(a.X - b.X, a.Y - b.Y); }

    public int SqrMagnitude { get { return X * X + Y * Y; } }
    public float Magnitude { get { return Mathf.Sqrt(SqrMagnitude); } }
}

