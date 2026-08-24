using System.Xml;
using Nekki.Vector.Core.Models;
using UnityEngine;

namespace Nekki.Vector.Core.Location
{
    public class TrickAreaRunner : AreaRunner
    {
        private const float ExpansionWidthRatio = 1f;

        private readonly float _originalWidth;

        private bool _wasExpanded;

        private readonly XmlNode _node;

        public static TrickAreaRunner Current;

        private bool _isActive;

        private string _itemName;

        private int _score;

        private TrickGO _content;

        public bool isActive => _isActive;

        public string itemName => _itemName;

        public int score => _score;

        public TrickAreaRunner(float x, float y, float width, float height, string typeName, string name, string itemName, int score, int direction = 0)
            : base(AreaType.Trick, x, y, width, height, typeName, name, direction)
        {
            _itemName = itemName;
            _score = score;
            _originalWidth = width;
            if (Xml2PrefabRoot.Serialize)
            {
                _isActive = true;
            }
            else
            {
                _isActive = Game.Instance.Snail ? true : Game.IsTrickBought(_itemName);
            }
        }

        public override void Generate(GameObject existRunner)
        {
            base.Generate(existRunner);
            _content = _UnityObject.GetComponent<TrickGO>();
            _content.Init(_itemName, _isActive, _W, _H);
        }

        protected override void GenerateObject()
        {
            var obj = Resources.Load<GameObject>("LevelContent/Prefabs/Trick");
            obj = Object.Instantiate(obj);
            _UnityObject = obj;
            _CachedTransform = obj.transform;
            var vector = new Vector3();
            vector.x = _DefautPosition.X;
            vector.y = _DefautPosition.Y;
            _CachedTransform.localPosition = vector;
            if (_Layer != null)
            {
                _CachedTransform.SetParent(_Layer.transform, false);
            }
            _content = _UnityObject.GetComponent<TrickGO>();
            _content.Init(_itemName, _isActive, _W, _H);
        }

        public override void InitRunner(Point point, bool serialize = false)
        {
            base.InitRunner(point, serialize);
            UpdateUnityObjectPosition(Position);

            _DefautPosition.Set(Position);
        }

        protected override void SerializeData()
        {
            _UnityObject.AddComponent<Xml2PrefabTrickAreaContainer>().Init(TransformationDataRaw, _TypeName, _Name, _X, _Y, _W, _H, _itemName, _score, Choice);
            _CachedTransform = _UnityObject.transform;
        }

        public override void Activate(ModelHuman model)
        {
            if (model.UserData.IsTrick)
            {
                Current = this;
            }
        }

        public override void Deactivate(ModelHuman model)
        {
            if (model.UserData.IsTrick)
            {
                Current = null;
            }
        }

        protected override void UpdateUnityObjectPosition(Vector3 position)
        {
            position.z = -10;
            _CachedTransform.localPosition = position;
        }

        public static void TrickGetByModel(ModelHuman model)
        {
            if (Current == null || !model.UserData.IsSelf)
            {
                return;
            }
            SoundsManager.Instance.PlaySounds(SoundType.trick_activate);
            model.CollectTrick(Current);
            Current._content.RunActivate();
            LevelMainController.current.levelSceneController.ShowActivatedTrick(Current);
        }

        public override void Reset()
        {
            base.Reset();
            UpdateUnityObjectPosition(Position);
            Current = null;
            _isActive = Game.Instance.Snail ? true : Game.IsTrickBought(_itemName);
            _content.Init(_itemName, _isActive, _W, _H);
            _wasExpanded = false;
        }

        public void Refresh()
        {
            if (!_isActive)
            {
                _isActive = Game.Instance.Snail ? true : Game.IsTrickBought(_itemName);
                _content.Init(_itemName, _isActive, _W, _H);
            }
        }

        public void ExpandTrickAreaOnce(int sign)
        {
            if (!_wasExpanded)
            {
                if (sign < 0)
                {
                    _Point2.X += _originalWidth;
                    _Point3.X += _originalWidth;
                }
                else
                {
                    _Point1.X -= _originalWidth;
                    _Point4.X -= _originalWidth;
                }

                _rectangle.Set((float)_Point1.X, (float)_Point1.Y, (float)(_Point2.X - _Point1.X), (float)(_Point4.Y - _Point1.Y));
                _wasExpanded = true;
            }
        }
    }
}
