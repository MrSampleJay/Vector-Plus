using System;
using Xml2Prefab;

namespace Nekki.Vector.Core.Location
{
    public class TrapezoidRunner : QuadRunner
    {
        private string _className;

        private int _type;

        private float _x;

        private float _y;

        private float _width;

        private float _height;

        private float _height1;

        private bool _sticky;

        private float _HeightQuad_L;

        private float _HeightQuad_R;

        public TrapezoidRunner(string name, int type, float x, float y, float width, float height, float height1, bool stikly, string material = "")
            : base(x, y, width, height, stikly, type, name, material)
        {
            _className = name;
            _type = type;
            _x = x;
            _y = y;
            _width = width;
            _height = height;
            _height1 = height1;
            _sticky = stikly;

            _HeightQuad_L = height;
            _HeightQuad_R = height1;
            _HeightQuad = Math.Max(_HeightQuad_L, _HeightQuad_R);
        }

        protected override void CalcPoints()
        {
            _Point1.Set(_XQuad, _YQuad, 0f);
            _Point2.Set(_XQuad + _WidthQuad, _YQuad + (_HeightQuad_L - _HeightQuad_R), 0f);
            _Point3.Set(_XQuad + _WidthQuad, _YQuad + _HeightQuad_L, 0f);
            _Point4.Set(_XQuad, _YQuad + _HeightQuad_L, 0f);
        }

        protected override void SetRectangle()
        {
            rectangle.Set((float)_XQuad, (float)Math.Min(_Point1.Y, _Point2.Y), _WidthQuad, Math.Max(_HeightQuad_L, _HeightQuad_R));
        }

        public override Point GetSize(int p_sign)
        {
            return p_sign <= 0 ? new Point(_WidthQuad, _HeightQuad_R) : new Point(_WidthQuad, _HeightQuad_L);
        }

        protected override void GenerateObject()
        {
            base.GenerateObject();
        }

        protected override void SerializeData()
        {
            if (_UnityObject == null)
            {
                CreateObject();
            }

            _UnityObject.AddComponent<Xml2PrefabTrapezoidContainer>().Init(_className, _type, _x, _y, _width, _height, _height1, _sticky, TransformationDataRaw, Choice); ;
        }

        public override bool Hit(double x, double y, bool equality)
        {
            Vector3d Vector3d = new Vector3d(x, y);
            Vector3d Vector3d2 = new Vector3d(Vector3d.X - _Point4.X, Vector3d.Y - _Point4.Y, 0f);
            Vector3d Vector3d3 = new Vector3d(_Point4.Y - _Point3.Y, _Point3.X - _Point4.X, 0f);
            Vector3d Vector3d4 = new Vector3d(Vector3d.X - _Point1.X, Vector3d.Y - _Point1.Y, 0f);
            Vector3d Vector3d5 = new Vector3d(_Point1.Y - _Point2.Y, _Point2.X - _Point1.X, 0f);
            Vector3d Vector3d6 = new Vector3d(Vector3d.X - _Point3.X, Vector3d.Y - _Point3.Y, 0f);
            Vector3d Vector3d7 = new Vector3d(_Point3.Y - _Point2.Y, _Point2.X - _Point3.X, 0f);
            Vector3d Vector3d8 = new Vector3d(Vector3d.X - _Point4.X, Vector3d.Y - _Point4.Y, 0f);
            Vector3d Vector3d9 = new Vector3d(_Point4.Y - _Point1.Y, _Point1.X - _Point4.X, 0f);
            double num = Vector3d2 * Vector3d3;
            double num2 = Vector3d4 * Vector3d5;
            double num3 = Vector3d6 * Vector3d7;
            double num4 = Vector3d8 * Vector3d9;
            double num5 = num * num2;
            double num6 = num3 * num4;
            return equality ? num5 <= 0.0 && num6 <= 0.0 : num5 < 0.0 && num6 < 0.0;
        }

        //public override bool Hit(double x, double y, bool equality)
        //{
        //    var p1X = _Point1.X;
        //    var p1Y = _Point1.Y;
        //    var p2X = _Point2.X;
        //    var p2Y = _Point2.Y;
        //    var p3X = _Point3.X;
        //    var p3Y = _Point3.Y;
        //    var p4X = _Point4.X;
        //    var p4Y = _Point4.Y;
        //    var num = ((x - p4X) * (p4Y - p3Y) + (y - p4Y) * (p3X - p4X)) * ((x - p1X) * (p1Y - p2Y) + (y - p1Y) * (p2X - p1X));
        //    var num2 = ((y - p4Y) * (p1X - p4X) + (x - p4X) * (p4Y - p1Y)) * ((x - p3X) * (p3Y - p2Y) + (y - p4Y) * (p2X - p3X));
        //    bool flag1 = equality ? num <= 0 : num < 0;
        //    bool flag2 = equality ? num2 <= 0 : num2 < 0;
        //    return flag1 && flag2;
        //}

        public override void TransformResize(Point size)
        {
            
        }
    }
}
