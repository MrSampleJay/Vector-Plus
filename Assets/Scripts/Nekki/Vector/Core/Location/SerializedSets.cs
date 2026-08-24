using System;
using System.Linq;
using System.Xml;
using Nekki.Vector.Core.Visual;
using Xml2Prefab;

namespace Nekki.Vector.Core.Location
{
    public class SerializedSets : BaseSets
    {
        private readonly Xml2PrefabLevelContainer _model;

        public SerializedSets(Xml2PrefabLevelContainer model) : base()
        {
            _model = model;
            ParseLevel();
        }

        private void ParseLevel()
        {
            ParseChoices(_model.Choices);
            TotalCoins = _model.Coins;
            var nodes = _model.Models.Select<string, XmlNode>((xmlString) =>
            {
                return XmlUtils.OpenXMLElementFromString(xmlString);
            });
            ParseModels(nodes);
            _music = _model.Music;
            CreateObjects();
            GetElements(_Objects);
            InitRunners();
            InitTriggers();
            InitParticles();
            InitAnimations();
            InitPrimitives();
            InitAreas();
            InitVisual();
        }

        private void CreateObjects()
        {
            Xml2PrefabRoot.Serialize = false;
            foreach (var container in _model.Visuals)
            {
                _Containers.Add(container.Factor, new VisualContainer(container.Factor, container.gameObject));
            }
            for (int i = 0; i < _model.Runners.Count; i++)
            {
                var obj = new SerializedObjectRunner((uint)i, null);
                obj.Parse(_model.Runners[i], _ChoisesDictionary);
                obj.Init();
                _Objects.Add(obj);
                obj.SetLayer(_Containers[obj.Factor].Object);
            }
        }
    }
}
