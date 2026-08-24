using System.Xml;
using Nekki.Vector.Core.Animation;
using Nekki.Vector.Core.Controllers;
using Nekki.Vector.Core.Models;
using Xml2Prefab;

namespace Nekki.Vector.Core.Location
{
    public class AreaRunner : QuadRunner
    {
        public enum AreaType
        {
            None,
            Trick,
            Help,
            Catch
        }

        private AreaType _Type;

        protected float _X;

        protected float _Y;

        protected float _W;

        protected float _H;

        private readonly XmlNode _node;

        protected string _TypeName;

        public int _Direction;

        public bool IsNone => _Type == AreaType.None;

        public bool IsHelp => _Type == AreaType.Help;

        public bool IsTrick => _Type == AreaType.Trick;

        public bool IsCatch => _Type == AreaType.Catch;

        public AreaRunner(AreaType type, float x, float y, float width, float height, string typeName, string name, int direction = 0)
            : base(x, y, width, height, sticky: false, string.IsNullOrEmpty(typeName) ? 1 : 0, name)
        {
            _Type = type;
            _TypeName = typeName;
            _X = x;
            _Y = y;
            _W = width;
            _H = height;
            _Direction = direction; 
            _TypeClass = RunnerType.Area;

            LoadBinaryIfTrick();
        }

        protected override void SerializeData()
        {
            if (_UnityObject == null)
            {
                CreateObject();
            }
            _UnityObject.AddComponent<Xml2PrefabAreaContainer>().Init(TransformationDataRaw, _TypeName, _Name, _X, _Y, _W, _H, Choice);
            _CachedTransform = _UnityObject.transform;
        }

        private void LoadBinaryIfTrick()
        {
            if (Animations.Animation.ContainsKey(_Name.Replace("Trigger", "")))
            {
                AnimationInfo animationInfo = Animations.Animation[Name.Replace("Trigger", "")];
                if (animationInfo.IsTrick)
                {
                    AnimationTrickInfo.LoadAnimation(animationInfo as AnimationTrickInfo);
                }
            }
        }

        public virtual bool OnKeyClick(KeyVariables Key)
        {
            return true;
        }

        public virtual void Activate(ModelHuman model)
        {
        }

        public virtual void Deactivate(ModelHuman model)
        {
        }
    }
}
