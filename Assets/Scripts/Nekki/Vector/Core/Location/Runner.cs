using System.Collections.Generic;
using System.Xml;
using Nekki.Vector.Core.Transformation;
using UnityEngine;
using Xml2Prefab;
using Color = UnityEngine.Color;

namespace Nekki.Vector.Core.Location
{
    public abstract class Runner
    {
        protected GameObject _UnityObject;

        protected Transform _CachedTransform;

        protected bool _IsDebug = true;

        protected GameObject _Layer;

        protected bool _IsEnabled = true;

        protected uint _Index;

        protected int _Hash = -1;

        protected string _Name;

        protected RunnerType _TypeClass;

        protected Vector3f _cahedPosition = new Vector3f();

        protected Vector3f _DefautPosition = new Vector3f();

        protected Vector3f _DefaultScale = new Vector3f(1, 1);

        protected Vector3f _DefaulRotation = new Vector3f();

        protected int _ActiveTransformation;

        protected List<TransformSystem> _TransformationData = new List<TransformSystem>();

        protected List<MoveTransform> _ActiveMoveSystem;

        public BaseElements ParentElements;

        public Vector3d TweenPosition = new Vector3d();

        protected string TransformationDataRaw;

        private ChoiceContainer _choice;

        public virtual GameObject UnityObject => _UnityObject;

        public GameObject ComponentHolder => _UnityObject;

        public virtual bool IsDebug
        {
            get => _IsDebug;
            set => _IsDebug = value;
        }

        public virtual GameObject Layer
        {
            get => _Layer;
            set
            {
                _Layer = value;
                if (_UnityObject != null)
                {
                    if (_Layer == null)
                    {
                        _CachedTransform.parent = null;
                        return;
                    }
                    var vector = _CachedTransform.localPosition;
                    _CachedTransform.SetParent(_Layer.transform);
                    _CachedTransform.localPosition = vector;
                }
            }
        }

        public bool IsVisible
        {
            get
            {
                if (!_UnityObject.GetComponent<Renderer>())
                {
                    return false;
                }

                return _UnityObject.GetComponent<Renderer>().isVisible;
            }
        }

        public virtual bool IsEnabled
        {
            get => _IsEnabled;
            set
            {
                _IsEnabled = value;
                if (UnityObject != null)
                {
                    UnityObject.SetActive(_IsEnabled);
                }
            }
        }

        public uint Index
        {
            get => _Index;
            set
            {
                _Index = value;
                if (_CachedTransform == null)
                {
                    _cahedPosition.Z = _Index * -0.0001f - 1;
                }
                else
                {
                    var vector = _CachedTransform.localPosition;
                    vector.z = _Index * -0.0001f - 1;
                    _CachedTransform.localPosition = vector;
                }
            }
        }

        public bool IsInitialized
        {
            get;
            set;
        }

        public int Hash => _Hash;

        public string Name
        {
            get => _Name;
            set
            {
                _Name = value;
                _Hash = value.GetHashCode();
            }
        }

        public RunnerType TypeClass => _TypeClass;

        public virtual Vector3f Position
        {
            get
            {
                if (_CachedTransform == null)
                {
                    return _cahedPosition;
                }
                return _CachedTransform.localPosition;
            }
            set
            {
                if (_CachedTransform == null)
                {
                    _cahedPosition = value;
                    return;
                }
                _CachedTransform.localPosition = (Vector3)value;
            }
        }

        public List<TransformSystem> TransformationData => _TransformationData;

        public ChoiceContainer Choice => _choice;

        public Runner(float X, float Y)
        {
            _cahedPosition = new Vector3f(X, Y);
            _DefautPosition = new Vector3f(X, Y);
        }

        public virtual void Generate()
        {
            GenerateObject();
            IsInitialized = true;
        }

        public virtual void Generate(GameObject existRunner)
        {
            if (existRunner == null)
            {
                return;
            }
            _UnityObject = existRunner;
            _CachedTransform = _UnityObject.transform;
            _UnityObject.transform.localPosition = new Vector3(_cahedPosition.X, _cahedPosition.Y, _cahedPosition.Z);
            if (_Layer != null)
            {
                _UnityObject.transform.SetParent(_Layer.transform);
            }
            IsInitialized = true;
        }

        protected virtual void GenerateObject()
        {
            _UnityObject = new GameObject("[" + GetType().Name + "]");
            _CachedTransform = _UnityObject.transform;
            _CachedTransform.localPosition = new Vector3(_DefautPosition.X, _DefautPosition.Y);
            if (_Layer != null)
            {
                _UnityObject.transform.SetParent(_Layer.transform);
            }
            IsInitialized = true;
        }

        protected virtual void UpdateUnityObjectPosition(Vector3 position)
        {
            if (_CachedTransform != null)
            {
                _CachedTransform.localPosition = position;
            }
        }

        public void SetVariant(ChoiceContainer choice)
        {
            if (_choice != null)
            {
                Debug.LogError("OVERRIDE");
            }
            _choice = choice;
        }

        public virtual void InitRunner(Point point, bool serialize = false)
        {
            if (serialize)
            {
                SerializeData();
            }
            if (!Xml2PrefabRoot.Serialize)
            {
                Move(point);
            }
            if (_CachedTransform != null)
            {
                _DefautPosition.Set(_CachedTransform.localPosition);
                _DefaultScale.Set(_CachedTransform.localScale);
                _DefaulRotation.Set(_CachedTransform.localEulerAngles);
            }

        }

        public virtual void Move(Point point)
        {
            if (point != null)
            {
                UpdateUnityObjectPosition(new Vector3(Position.X + point.X, Position.Y + point.Y, Position.Z));
            }
        }

        public virtual void Reset()
        {
            ResetPosition();
            UpdateUnityObjectPosition((Vector3)_DefautPosition);
            if (_CachedTransform != null)
            {
                _CachedTransform.localScale = _DefaultScale;
                _CachedTransform.localEulerAngles = _DefaulRotation;
            }
            _ActiveTransformation = 0;
            if (_ActiveMoveSystem != null)
                _ActiveMoveSystem.Clear();
        }

        public virtual void ResetPosition()
        {
            TweenPosition.Reset();
            foreach (var transform in _TransformationData)
            {
                transform.Reset();
            }
            if (_ActiveMoveSystem != null)
                _ActiveMoveSystem.Clear();
        }

        public virtual void UpdatePosition(Point point)
        {
            if (point != null)
            {
                TweenPosition.Set(point.X, point.Y, 0);
                Move(point);
            }
        }

        public abstract bool Render();

        public virtual void UpdateLog()
        {
        }

        public void SetXmlList(XmlNode node)
        {
            if (node != null)
            {
                TransformationDataRaw = node.OuterXml;
                ParseTransformation(node["Dynamic"]);
            }
        }

        public void SetXmlListSerialized(XmlNode node)
        {
            if (node != null)
            {
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.Name == "Dynamic")
                    {
                        ParseTransformation(child);
                    }
                }
            }
        }

        public void ParseMisc(XmlNode node)
        {
            if (node != null)
            {

            }
        }

        public void ParseTransformation(XmlNode node)
        {
            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "Transformation")
                {
                    var transform = TransformSystem.Create(childNode);
                    if (transform != null)
                    {
                        transform.Parent = this;
                        AddTransformationData(transform);
                    }
                }
            }
        }

        public void AddMoveSystem(MoveTransform system)
        {
            if (_ActiveMoveSystem == null)
                _ActiveMoveSystem = new List<MoveTransform>();
            _ActiveMoveSystem.Add(system);
        }

        public void RemoveMoveSystem(MoveTransform system)
        {
            if (_ActiveMoveSystem != null)
            {
                _ActiveMoveSystem.Remove(system);
            }
        }

        public virtual void TransformationStart()
        {
            _ActiveTransformation++;
        }

        public virtual void TransformationEnd()
        {
            _ActiveTransformation--;
        }

        private void AddTransformationData(TransformSystem p_data)
        {
            if (_TransformationData == null)
                _TransformationData = new List<TransformSystem>();
            _TransformationData.Add(p_data);
        }

        public virtual void TransformColor(Color color)
        {

        }

        public virtual void TransformColorEnd(Color color)
        {

        }

        public virtual void TransformResize(Point size)
        {

        }

        public virtual void TransformRotate(float angle)
        {
            Vector3 localEulerAngles = _CachedTransform.localEulerAngles;
            localEulerAngles.z += angle;
            _CachedTransform.localEulerAngles = localEulerAngles;
        }

        protected virtual void SerializeData()
        {
        }
    }
}
