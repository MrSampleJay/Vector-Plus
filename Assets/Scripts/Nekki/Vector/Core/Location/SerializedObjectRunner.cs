using Nekki.Vector.Core.Location.LevelCreation;
using Nekki.Vector.Core.Transformation;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using Xml2Prefab;

namespace Nekki.Vector.Core.Location
{
    public class SerializedObjectRunner : BaseObjectRunner
    {
        private GameObject _cached;

        public SerializedObjectRunner(uint index, BaseObjectRunner parent)
            : base(index, parent)
        {
        }

        public uint Parse(Xml2PrefabObjectRunnerContainer container, Dictionary<string, string> choices)
        {
            _cached = container.gameObject;
            _Name = container.Name;
            _Position = new Point(container.transform.localPosition.x, container.transform.localPosition.y);
            _LocalPosition = new Point(_Position);
            _Factor = container.Factor;
            if (_Parent != null)
            {
                if (Mathf.Approximately(_Factor, -1))
                {
                    _Factor = _Parent.Factor;
                }
                _Position.Add(_Parent.Position);
            }
            if (!string.IsNullOrEmpty(container.LocalDynamicTransform))
            {
                ParseTransformationData(XmlUtils.OpenXMLElementFromString(container.LocalDynamicTransform));
            }
            Index++;
            if (!string.IsNullOrEmpty(container.ExternalDynamicTransform))
            {
                ParseTransformationData(XmlUtils.OpenXMLElementFromString(container.ExternalDynamicTransform));
            }
            return CreateElement(container, Index, choices);;
        }

        public uint CreateElement(Xml2PrefabObjectRunnerContainer container, uint index, Dictionary<string, string> choices)
        {
            var element = new SerializedElements(this, index);
            uint num = element.Parse(container.Runners, choices);
            if (_Element == null)
            {
                _Element = element;
            }
            else
            {
                _Element.CopyFrom(element);
            }
            return num;
        }

        public uint CreateChild(Xml2PrefabObjectRunnerContainer container, uint index, Dictionary<string, string> choices)
        {
            if (container == null)
            {
                return index;
            }
            var obj = new SerializedObjectRunner(index, this);
            uint num = obj.Parse(container, choices);
            obj.Init();
            _Childs.Add(obj);
            return num;
        }

        protected override void InitElements()
        {
            foreach (var runner in _Element.Runners)
            {
                runner.InitRunner(Position, false);
                BuildTranformationTable(runner.TransformationData);
            }
        }

        protected override GameObject CreateObject()
        {
            if (_cached == null)
            {
                return base.CreateObject();
            }
            return _cached;
        }

        public override void ParseTransformationData(XmlNode node)
        {
            if (node["Dynamic"] == null)
            {
                return;
            }

            foreach (XmlNode child in node["Dynamic"])
            {
                if (child.Name == "Transformation")
                {
                    var system = TransformSystem.Create(child);
                    system.MainParent = this;
                    AddTransformationData(system);
                }
            }
        }
    }
}
