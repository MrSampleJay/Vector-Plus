using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Nekki.Vector.Core.Detector;
using Nekki.Vector.Core.Result;
using Math = Nekki.Vector.Core.Utilites.Math;

namespace Nekki.Vector.Core.Location
{
    public class QuadRunner : Runner
    {
        protected int _NameHash;

        private int _Type;

        protected float _XQuad;

        protected float _YQuad;

        protected float _WidthQuad;

        protected float _HeightQuad;

        private float _DefaultWidth;

        private float _DefaultHeight;

        private double _DefaultX;

        private double _DefaultY;

        private bool _Sticky;

        private string _Material;

        private List<Vector3d> _Points = new List<Vector3d>();

        private List<Edge> _Edges = new List<Edge>();

        private List<Vector3d> _Normales = new List<Vector3d>();

        protected Vector3d _Point1;

        protected Vector3d _Point2;

        protected Vector3d _Point3;

        protected Vector3d _Point4;

        protected List<Vector3d> _DefaultPoints = new List<Vector3d>();

        protected List<Vector3d> _FuturePoints = new List<Vector3d>();

        protected List<Vector3d> _TempPoints = new List<Vector3d>();

        protected Vector3f _Velocity = new Vector3f();

        protected Rectangle _rectangle = new Rectangle();

        public Action<QuadRunner> OnTransformationStart;

        public Action<QuadRunner> OnTransformationEnd;

        public bool IsRender;

        public int NameHash => _NameHash;

        public int Type => _Type;

        public float XQuad => _XQuad;

        public float YQuad => _YQuad;

        public float WidthQuad => _WidthQuad;

        public float HeightQuad => _HeightQuad;

        public bool Sticky
        {
            get => _Sticky;
            set => _Sticky = value;
        }

        public string Material
        {
            get => _Material;
            set => _Material = value;
        }

        public Vector3d Point1 => _Point1;

        public Vector3d Point2 => _Point2;

        public Vector3d Point3 => _Point3;

        public Vector3d Point4 => _Point4;

        public Rectangle rectangle => _rectangle;

        public override Vector3f Position
        {
            get;
            set;
        } = new Vector3f();

        public QuadRunner(float x, float y, float width, float height, bool sticky, int type = 0, string name = "", string material = "")
            : base(x, y)
        {
            Position = new Vector3f(x, y);
            _XQuad = x;
            _YQuad = y;
            _WidthQuad = width;
            _HeightQuad = height;
            _DefaultWidth = width;
            _DefaultHeight = height;
            CreatePoints();
            _Type = type;
            _Name = name;
            _Sticky = sticky;
            _Material = material;
            if (!string.IsNullOrEmpty(name))
            {
                _NameHash = _Name.GetHashCode();
            }
        }

        private void CreatePoints()
        {
            _Point1 = new Vector3d(_XQuad, _YQuad, 0f);
            _Point2 = new Vector3d(_XQuad + _WidthQuad, _YQuad, 0f);
            _Point3 = new Vector3d(_XQuad + _WidthQuad, _YQuad + _HeightQuad, 0f);
            _Point4 = new Vector3d(_XQuad, _YQuad + _HeightQuad, 0f);
        }

        protected override void GenerateObject()
        {
            if (Xml2PrefabRoot.Serialize)
                base.GenerateObject();
        }

        protected void CreateObject()
        {
            if (_UnityObject != null)
            {
                return;
            }

            base.GenerateObject();
            _CachedTransform.localPosition = new UnityEngine.Vector3((float)_XQuad, (float)_YQuad);
        }

        public override void InitRunner(Point point, bool serialize = false)
        {
            if (serialize)
            {
                SerializeData();
            }

            if (point != null && !Xml2PrefabRoot.Serialize)
            {
                Move(point);
            }

            _DefautPosition.Set(Position);

        }

        public override bool Render()
        {
            return true;
        }

        public override void Move(Point point)
        {
            Position.Add(point);
            SetProperties();

            if (_UnityObject != null)
            {
                UpdateUnityObjectPosition(Position);
            }
        }

        public override void Reset()
        {
            LevelMainController.current.Location.Grid.RemoveQuad(this);

            _WidthQuad = _DefaultWidth;
            _HeightQuad = _DefaultHeight;

            Position.Set(_DefautPosition);

            base.Reset();
            SetProperties();

            LevelMainController.current.Location.Grid.AddQuad(this);
        }

        public Vector3d GetCornerByIndex(int value)
        {
            switch (value)
            {
                case 0:
                    return _Point1;
                case 1:
                    return _Point2;
                case 2:
                    return _Point3;
                case 3:
                    return _Point4;
                default:
                    return null;
            }
        }

        public virtual Point GetSize(int sign)
        {
            return new Point(_WidthQuad, _HeightQuad);
        }

        public void RecoveryPoints()
        {
            if (_TempPoints.Count != 0)
            {
                _Point1.Set(_TempPoints[0]);
                _Point2.Set(_TempPoints[1]);
                _Point3.Set(_TempPoints[2]);
                _Point4.Set(_TempPoints[3]);
                SetProperties(false);
                _TempPoints.Clear();
                _FuturePoints.Clear();
            }
        }

        public Vector3d Subtract(Vector3d Point)
        {
            return new Vector3d(Point.X - Position.X, Point.Y - Position.Y);
        }

        public Vector3dLine Friction(Vector3d p_end, Vector3d p_start, List<Cross> crossList)
        {
            if (!Hit(p_start.X, p_start.Y))
            {
                return null;
            }
            Vector3d vector3f = new Vector3d(p_start);
            Vector3d vector3f2 = new Vector3d(p_end);
            vector3f.Z = 0f;
            vector3f2.Z = 0f;
            crossList.Clear();
            CrossByEdge(p_end, p_start, crossList);
            int p_side = NearestEdge(vector3f2);
            Vector3d vector3f3 = crossList.Count != 0 ? crossList[0].Point : Closest(vector3f2, p_side);
            Vector3d vector3f4 = Closest(vector3f, p_side);
            double num = Vector3d.Distance(vector3f4, vector3f3);
            double num2 = Vector3d.Distance(vector3f4, vector3f);
            double num3 = num2 * 0.20000000298023224;
            return !(num3 < num) ? new Vector3dLine(vector3f3, vector3f3) : new Vector3dLine(vector3f4 + (vector3f3 - vector3f4).Normalize().Multiply(num3), vector3f3);
        }

        public int NearestEdge(Vector3d vector)
        {
            int result = 0;
            int side = 0;
            double num2 = 1.7976931348623157e+308;
            for (int i = 0; i < _Edges.Count; i++)
            {
                var distance = Vector3d.Distance(Closest(vector, side), vector);
                int num = side;
                if (side != 0 && num2 <= distance)
                {
                    distance = num2;
                    num = result;
                }
                result = num;
                side++;
                num2 = distance;
            }
            return result;
        }

        public static bool IsCrossIndex(List<Cross> crossList1, List<Cross> crossList2)
        {
            foreach (Cross item in crossList1)
            {
                foreach (Cross item2 in crossList2)
                {
                    if (item.Index == item2.Index)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void DetectorAffiliation(DetectorLine detector, Affiliation result)
        {
            result.Clear();
            var start = detector.Start;
            var end = detector.End;
            end.Start.Add(_Velocity);
            end.End.Add(_Velocity);
            CrossByEdge(start.End, end.End, result.CrossList1);
            CrossByEdge(start.Start, end.Start, result.CrossList2);
            CrossByEdge(start.Start, start.End, result.CrossList3);
            var startX = Math.Round(start.Start.X, 1000);
            var startY = Math.Round(start.Start.Y, 1000);
            bool hit = Hit(startX, startY, true);
            var endX = Math.Round(start.End.X, 1000);
            var endY = Math.Round(start.End.Y, 1000);
            bool hit2 = Hit(endX, endY, true);
            result.SetType(hit, hit2);
            if (result.Type > 0)
            {
                return;
            }
            result.Clear();

        }

        public static Cross MinCross(List<Cross> crossList, Vector3d point)
        {
            double num = double.MaxValue;
            Cross result = new Cross();
            foreach (Cross p_cross in crossList)
            {
                double num2 = Vector3d.Distance(p_cross.Point, point);
                if (!(num2 > num))
                {
                    num = num2;
                    result = p_cross;
                }
            }
            return result;
        }

        public static int Side(DetectorLine detector, int sign)
        {
            switch (detector.Type)
            {
                case DetectorLine.DetectorType.Vertical:
                    return sign != 1 ? 1 : 3;
                case DetectorLine.DetectorType.Horizontal:
                    return 0;
                default:
                    return -1;
            }

        }

        public static int SideForType5Or6(DetectorLine p_detector, Rectangle p_rect, int p_sign, int p_type)
        {
            switch (p_detector.Type)
            {
                case DetectorLine.DetectorType.Vertical:
                    {
                        Vector3d vector3f = null;
                        vector3f = p_type != 5 ? p_detector.Start.End : p_detector.Start.Start;
                        if (vector3f.X >= p_rect.MinX - 0.01f && vector3f.X <= p_rect.MinX + 0.01f)
                        {
                            return 3;
                        }
                        if (vector3f.X >= p_rect.MaxX - 0.01f && vector3f.X <= p_rect.MaxX + 0.01f)
                        {
                            return 1;
                        }
                        if (vector3f.Y >= p_rect.MinY - 0.01f && vector3f.Y <= p_rect.MinY + 0.01f)
                        {
                            return 0;
                        }
                        if (vector3f.Y >= p_rect.MaxY - 0.01f && vector3f.Y <= p_rect.MaxY + 0.01f)
                        {
                            return 2;
                        }
                        return p_sign != 1 ? 1 : 3;
                    }
                case DetectorLine.DetectorType.Horizontal:
                    return 0;
                default:
                    return -1;
            }
        }

        public int Side(DetectorLine detector, Affiliation affiliationResult, int sign)
        {
            Vector3dLine start = detector.Start;
            Vector3dLine end = detector.End;
            Vector3dLine perpendicular = detector.Perpendicular;
            List<Cross> list = new List<Cross>();
            Cross cross;
            switch (affiliationResult.Type)
            {
                case 1:
                    return !IsCrossIndex(affiliationResult.CrossList1, affiliationResult.CrossList2) ? Side(detector, sign) : MinCross(affiliationResult.CrossList1, end.Start).Index;
                case 2:
                    CrossByEdge(start.End, perpendicular.End, list);
                    cross = list.Count != 0 ? MinCross(list, end.End) : MinCross(affiliationResult.CrossList1, end.End);
                    return cross.Index;
                case 3:
                    CrossByEdge(start.Start, perpendicular.Start, list);
                    cross = list.Count != 0 ? MinCross(list, end.Start) : MinCross(affiliationResult.CrossList2, end.Start);
                    return cross.Index;
                case 4:
                    return Side(detector, sign);
                case 5:
                    return SideForType5Or6(detector, rectangle, sign, 5);
                case 6:
                    return SideForType5Or6(detector, rectangle, sign, 6);
                default:
                    return -1;
            }
        }

        public void CrossByEdge(Vector3d p1, Vector3d p2, List<Cross> result)
        {
            for (int i = 0; i < _Edges.Count; i++)
            {
                Vector3d vector3f = Vector3d.Cross(p1, p2, _Edges[i].Point1, _Edges[i].Point2);
                if (vector3f != null)
                {
                    result.Add(new Cross(vector3f, i));
                }
            }
        }

        public Vector3d DeltaEdge(Vector3d vector, int side)
        {
            if (side == -1)
            {
                return new Vector3d(0, 0, 0);
            }
            var vector1 = new Vector3d(vector);
            if (_TempPoints.Count > 0)
            {
                vector1.Add(_Point1 - _TempPoints[0]);
            }
            return Closest(vector1, side) - vector1;
        }

        public Vector3d Closest(Vector3d vector, int side)
        {
            Vector3d point = _Edges[side].Point1;
            Vector3d p_lineDirection = _Edges[side].Point2 - point;
            return Vector3d.Closest(vector, point, p_lineDirection);
        }

        public bool IsPointToLine(Vector3d vector, int side)
        {
            var v1 = vector - _Edges[side].Point1;
            var v2 = vector - _Edges[side].Point2;
            return v1 * v2 < 0;
        }

        public double Distance(Vector3d point, int side)
        {
            return System.Math.Abs(Distance(point, _Edges[side].Point1, _Edges[side].Point2));
        }

        public double Distance(Vector3d point, Vector3d v1, Vector3d v2)
        {
            Vector3d vector3f = new Vector3d(v2.Y - v1.Y, v1.X - v2.X, 0f);
            return vector3f.Normalize() * (point - v1);
        }

        public Vector3d CornerByIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return _Point1;
                case 1:
                    return _Point2;
                case 2:
                    return _Point3;
                case 3:
                    return _Point4;
                default:
                    return null;
            }
        }

        public int Normal(DetectorLine detector)
        {
            for (int i = 0; i < _Normales.Count; i++)
            {
                var v1 = detector.Subtract();
                return (int)(v1 * _Normales[i]);
            }
            return -1;
        }

        public Vector3d Corner(int sign, int p_cornernum)
        {
            if (sign > 0)
            {
                switch (p_cornernum)
                {
                    case 0:
                        return _TempPoints.Count <= 0 ? _Point1 : _TempPoints[0];
                    case 1:
                        return _TempPoints.Count <= 0 ? _Point2 : _TempPoints[1];
                    case 2:
                        return _TempPoints.Count <= 0 ? _Point3 : _TempPoints[2];
                    case 3:
                        return _TempPoints.Count <= 0 ? _Point4 : _TempPoints[3];
                }
            }
            else
            {
                switch (p_cornernum)
                {
                    case 0:
                        return _TempPoints.Count <= 0 ? _Point2 : _TempPoints[1];
                    case 1:
                        return _TempPoints.Count <= 0 ? _Point1 : _TempPoints[0];
                    case 2:
                        return _TempPoints.Count <= 0 ? _Point4 : _TempPoints[3];
                    case 3:
                        return _TempPoints.Count <= 0 ? _Point3 : _TempPoints[2];
                }
            }
            return null;
        }

        public bool Hit(Vector3d point, bool equality = false)
        {
            return Hit(point.X, point.Y, equality);
        }

        public virtual bool Hit(double x, double y, bool equality = false)
        {
            return _rectangle.Contains(x, y);
        }

        public Vector3d Add(Vector3d point)
        {
            return new Vector3d(Position.X + point.X, Position.Y + point.Y, point.Z);
        }

        public void SetProperties(bool IsPoint = true)
        {
            SetX();
            SetY();
            CalcPoints();
            SetEdge();
            SetNormals();
            SetRectangle();
        }

        protected virtual void SetRectangle()
        {
            rectangle.Set(_XQuad, _YQuad, _WidthQuad, _HeightQuad);
        }

        protected virtual void CalcPoints()
        {
            _Point1.Set(_XQuad, _YQuad, 0f);
            _Point2.Set(_XQuad + _WidthQuad, _YQuad, 0f);
            _Point3.Set(_XQuad + _WidthQuad, _YQuad + _HeightQuad, 0f);
            _Point4.Set(_XQuad, _YQuad + _HeightQuad, 0f);
        }

        public void SetPoints()
        {
            _Point1 = Add(_Point1);
            _Point2 = Add(_Point2);
            _Point3 = Add(_Point3);
            _Point4 = Add(_Point4);
        }

        public void SetTempPoint(int frames)
        {
            _Velocity.Set(TweenPosition);
            if (frames == 0)
            {
                return;
            }
            _TempPoints.Clear();
            SetProperties(false);
        }

        public void SetEdge()
        {
            _Edges.Clear();
            _Edges.Add(new Edge
            {
                Point1 = _Point1,
                Point2 = _Point2
            });
            _Edges.Add(new Edge
            {
                Point1 = _Point2,
                Point2 = _Point3
            });
            _Edges.Add(new Edge
            {
                Point1 = _Point3,
                Point2 = _Point4
            });
            _Edges.Add(new Edge
            {
                Point1 = _Point4,
                Point2 = _Point1
            });
        }

        public void SetNormals()
        {
            foreach (var edge in _Edges)
            {
                _Normales.Add(Vector3d.Normal(edge.Point1, edge.Point2));
            }
        }

        public void SetX()
        {
            _XQuad = Position.X;
        }

        public void SetY()
        {
            _YQuad = Position.Y;
        }

        public Point Size(int sign)
        {
            return new Point(_WidthQuad, _HeightQuad);
        }

        public override void TransformationStart()
        {
            if (_ActiveTransformation == 0)
            {
                OnTransformationStart?.Invoke(this);
            }
            base.TransformationStart();
        }

        public override void TransformationEnd()
        {
            base.TransformationEnd();
            if (_ActiveTransformation == 0)
            {
                OnTransformationEnd?.Invoke(this);
            }
        }

        public override void TransformResize(Point size)
        {
            //var scale = _CachedTransform.localScale;
            //scale.x = size.X / _WidthQuad;
            //scale.y = size.Y / _HeightQuad;
            //_CachedTransform.localScale = scale;
            
            _WidthQuad = size.X;
            _HeightQuad = size.Y;
            SetProperties();
        }

        public override void TransformRotate(float angle)
        {
        }
    }
}
