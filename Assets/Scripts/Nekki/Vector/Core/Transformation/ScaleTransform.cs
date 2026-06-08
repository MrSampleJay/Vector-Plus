using System;
using System.Collections.Generic;
using System.Xml;
using Nekki.Vector.Core.Location;

namespace Nekki.Vector.Core.Transformation
{
    public class SizeTransform : TransformPrototype
    {
        private int _Frames;

        private int _CurrentFrame;

        private float _FinalWidth;

        private float _FinalHeight;

        private List<Point> _Points = new List<Point>();

        public override int Frames => _Frames;

        public SizeTransform()
        {
            _Type = Type.Scale;
        }

        public static SizeTransform Create(XmlNode node)
        {
            if (node == null)
                return null;
            SizeTransform size = new SizeTransform();
            size.Parse(node);
            return size;
        }

        public override void Parse(XmlNode node)
        {
            _Frames = node.Attributes["Frames"].ParseInt();

            _FinalWidth = node.Attributes["FinalWidth"].ParseFloat(-1);
            _FinalHeight = node.Attributes["FinalHeight"].ParseFloat(-1);
        }

        protected void ReplaceZeroByEps(Point p_point)
        {
            if (p_point.X == 0f)
            {
                p_point.X = 1E-05f;
            }
            if (p_point.Y == 0f)
            {
                p_point.Y = 1E-05f;
            }
        }

        public override bool Iteration()
        {
            if (_Runner == null || _Pause)
            {
                return false;
            }
            _Runner.TransformResize(_Points[_CurrentFrame]);
            _CurrentFrame++;
            if (_CurrentFrame >= _Frames)
            {
                _Pause = true;
                return false;
            }
            return true;
        }

        public override void CalcDelta()
        {
            _Points.Clear();
            var visualRunner = _Runner as VisualRunner;
            var quadRunner = _Runner as QuadRunner;
            if (visualRunner != null)
            {
                if (_FinalWidth < 0)
                {
                    _FinalWidth = visualRunner.Rectangle.Size.Width;
                }
                if (_FinalHeight < 0)
                {
                    _FinalHeight = visualRunner.Rectangle.Size.Height;
                }
                _Points.Add(new Point(visualRunner.Rectangle.Size.Width, visualRunner.Rectangle.Size.Height));
            }
            if (quadRunner != null)
            {
                if (_FinalWidth < 0)
                {
                    _FinalWidth = quadRunner.WidthQuad;
                }
                if (_FinalHeight < 0)
                {
                    _FinalHeight = quadRunner.HeightQuad;
                }
                _Points.Add(new Point(quadRunner.WidthQuad, quadRunner.HeightQuad));
            }
            _Points.Add(new Point(_FinalWidth, _FinalHeight));
            CalculatePoints();
        }

        private void CalculatePoints()
        {
            Point point = _Points[0];
            Point point2 = _Points[_Points.Count - 1];
            List<Point> list = new List<Point>();
            for (int i = 1; i < _Points.Count - 1; i++)
            {
                list.Add(_Points[i]);
            }
            _Points.Clear();
            float num = 0f;
            for (int j = 0; j < _Frames; j++)
            {
                double num2 = Math.Pow(1f - num, list.Count + 1);
                double num3 = num2 * point.X;
                double num4 = num2 * point.Y;
                int num5 = 0;
                for (num5 = 0; num5 < list.Count; num5++)
                {
                    num2 = (list.Count + 1) * Math.Pow(1f - num, list.Count - num5) * Math.Pow(num, num5 + 1);
                    num3 += num2 * list[num5].X;
                    num4 += num2 * list[num5].Y;
                }
                num2 = Math.Pow(num, num5 + 1);
                num3 += num2 * point2.X;
                num4 += num2 * point2.Y;
                _Points.Add(new Point((float)num3, (float)num4));
                num += 1f / _Frames;
            }
            _Points.Add(point2);
            ReplaceZeroByEps(_Points[_Points.Count - 1]);
            for (int num1 = _Points.Count - 1; num > 1; num--)
            {
                ReplaceZeroByEps(_Points[num1 - 1]);
                //_Points[num1].Round(100);
            }
            _Points.RemoveAt(0);
        }

        public override void Reset()
        {
            base.Reset();
            _CurrentFrame = 0;
        }

        public override TransformPrototype Clone()
        {
            var size = new SizeTransform();
            size._Frames = _Frames;
            size._FinalHeight = _FinalHeight;
            size._FinalWidth = _FinalWidth;
            return size;
        }
    }
}

