using Nekki.Vector.Core.Utilites;
using System;
using System.Xml;
using UnityEngine;

public class Point
{
    public static readonly Point ZeroPoint = new Point();

    private float _X;

    private float _Y;

    public float X
    {
        get => _X;
        set => _X = value;
    }

    public float Y
    {
        get => _Y;
        set => _Y = value;
    }

    public string RangeString => string.Format("[{0} {1}]", _X, _Y);

    public Point(Point p_point)
    {
        _X = p_point.X;
        _Y = p_point.Y;
    }

    public Point(float p_x = 0f, float p_y = 0f)
    {
        _X = p_x;
        _Y = p_y;
    }

    public Point Add(Point p_point)
    {
        _X += p_point.X;
        _Y += p_point.Y;
        return this;
    }

    public Point Add(float p_x, float p_y)
    {
        _X += p_x;
        _Y += p_y;
        return this;
    }

    public Point Subtract(Point p_point)
    {
        _X -= p_point.X;
        _Y -= p_point.Y;
        return this;
    }

    public Point Multiply(float p_value)
    {
        _X *= p_value;
        _Y *= p_value;
        return this;
    }

    public void Set(Point p_point)
    {
        _X = p_point._X;
        _Y = p_point._Y;
    }

    public void Round(int p_pow)
    {
        _X = MathUtils.Round(_X, p_pow);
        _Y = MathUtils.Round(_Y, p_pow);
    }

    public void IRound()
    {
        _X = MathUtils.Round(_X, 1f);
    }

    public float DistBP(Point p_point)
    {
        return (_X - p_point._X) * (_X - p_point._Y) + (_Y - p_point._Y) * (_Y - p_point._Y);
    }
    public static Point Create(XmlNode Node)
    {
        if (Node == null)
        {
            return null;
        }
        Point vector3f = new Point(0f, 0f);
        try
        {
            vector3f._X = Node.Attributes["X"].ParseFloat();
        }
        catch
        {
            throw new Exception("Error : parse X eeror type");
        }
        try
        {
            vector3f._Y = Node.Attributes["Y"].ParseFloat();
            return vector3f;
        }
        catch
        {
            throw new Exception("Error : parse Y eeror type");
        }
    }

    public Point Clone()
    {
        return new Point(this);
    }

    public override string ToString()
    {
        return string.Format("[Point: X={0}, Y={1}]", X.ToString("G9"), Y.ToString("G9"));
    }

    public static Point operator -(Point p_point1, Point p_point2)
    {
        return new Point(p_point1.X - p_point2.X, p_point1.Y - p_point2.Y);
    }

    public static Point operator +(Point p_point1, Point p_point2)
    {
        return new Point(p_point1.X + p_point2.X, p_point1.Y + p_point2.Y);
    }

    public static bool operator ==(Point p_point1, Point p_point2)
    {
        if (ReferenceEquals(p_point1, p_point2))
        {
            return true;
        }
        if (ReferenceEquals(p_point1, null) || ReferenceEquals(p_point2, null))
        {
            return false;
        }
        return p_point1.X == p_point2.X && p_point1.Y == p_point2.Y;
    }

    public static bool operator !=(Point p_point1, Point p_point2)
    {
        return !(p_point1 == p_point2);
    }

    public static implicit operator Vector3(Point p_point)
    {
        return new Vector3(p_point.X, p_point.Y, 0f);
    }
}
