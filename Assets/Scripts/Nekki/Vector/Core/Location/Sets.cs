using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Core._Common;
using Nekki.Vector.Core.Location.LevelCreation;
using Nekki.Vector.Core.Visual;
using UnityEngine;
using Xml2Prefab;

namespace Nekki.Vector.Core.Location
{
    public class Sets : BaseSets
    {
        private List<XmlNode> _XmlObjects = new List<XmlNode>();

        private List<XmlNode> _xmlBuilds = new List<XmlNode>();

        private HashSet<string> _filesBuilds = new HashSet<string>();

        private HashSet<string> _filesObjects = new HashSet<string>();

        private XmlDocument _document;

        public Sets(XmlDocument document)
        {
            Current = this;
            _document = document;
            LoadLevel();
        }

        private void LoadLevel()
        {
            var root = _document["Root"];
            if (root["Sets"] != null)
            {
                foreach (XmlNode set in root["Sets"])
                {
                    if (set.Name == "City")
                    {
                        _filesBuilds.Add(set.Attributes["FileName"].Value);
                    }
                    else
                    {
                        _filesObjects.Add(set.Attributes["FileName"].Value);
                    }
                }
            }
            LoadBuilds();
            LoadObjects();
        }

        private void LoadBuilds()
        {
            foreach (var file in _filesBuilds)
            {
                var document = XmlUtils.OpenXMLDocument(VectorPaths.XmlRoot, file);
                if (document["Root"] != null && document["Root"]["Objects"] != null)
                {
                    _xmlBuilds.Add(document["Root"]["Objects"]);
                }
                if (document["Root"] != null && document["Root"]["Sets"] != null)
                {
                    foreach (XmlNode set in document["Root"]["Sets"])
                    {
                        if (!_filesObjects.Contains(set.Attributes["FileName"].Value))
                            _filesObjects.Add(set.Attributes["FileName"].Value);
                    }
                }
            }
        }

        private void LoadObjects()
        {
            foreach (var file in _filesObjects)
            {
                var document = XmlUtils.OpenXMLDocument(VectorPaths.XmlRoot, file);
                var root = document["Root"];
                if (root == null)
                {
                    Debug.LogError("Empty node from file \"" + file + "\"!");
                    continue;
                }
                _XmlObjects.Add(root["Objects"]);
            }
            Parse();
        }

        private void Parse()
        {
            _ObjectsNodes = new Dictionary<string, XmlNode>();
            ParseChoices(_document["Root"]["Choises"]);
            ParseObjects();
            ParseBuilds();
            ParseLevel();
            _ObjectsNodes = null;
        }

        public void ParseObjects()
        {
            foreach (XmlNode node in _XmlObjects)
            {
                foreach (XmlNode obj in node)
                {
                    if (obj.Attributes["Name"] != null)
                        _ObjectsNodes[obj.Attributes["Name"].Value] = obj;
                }
            }
        }

        public void ParseBuilds()
        {
            foreach (XmlNode node in _xmlBuilds)
            {
                foreach (XmlNode build in node)
                {
                    _ObjectsNodes[build.Attributes["Name"].Value] = build;
                }
            }

        }

        public void ParseLevel()
        {
            var root = _document["Root"];
            TotalCoins = root["Coins"].Attributes["Value"].ParseInt();
            ParseModels(root);
            AddMusics(root["Music"]);
            CreateObjects();
            GetElements(_Objects);
            InitRunners();
            InitTriggers();
            InitParticles();
            InitAnimations();
            InitPrimitives();
            InitAreas();
            InitVisual();
            SetTotals();
        }

        protected void ParseModels(XmlNode nodes)
        {
            ParseModels(nodes.ChildNodes.Cast<XmlNode>().Where(node => node.Name == "Models"));
        }

        public void AddMusics(XmlNode node)
        {
            _music = node.Attributes["Name"].Value;
        }

        public void CreateObjects()
        {
            XmlNode root = _document["Root"]["Track"];
            List<float> factors = new List<float>();
            GameObject levelObject = new GameObject("Level_root_object");
            levelObject.transform.position = Vector3.zero;
            levelObject.transform.localScale = Vector3.one;
            foreach (XmlNode xmlNode in root)
            {
                if (xmlNode.Attributes["Factor"] != null)
                {
                    factors.Add(float.Parse(xmlNode.Attributes["Factor"].Value, CultureInfo.InvariantCulture.NumberFormat));
                }
            }
            factors.Sort();
            uint num = 0;
            for (int i = 0; i < root.ChildNodes.Count; i++)
            {
                var childNode = root.ChildNodes[i];
                BaseObjectRunner objectRunner = null;
                string name = string.Empty;
                if (childNode.Attributes["Name"] != null)
                {
                    name = childNode.Attributes["Name"].Value;
                }
                if (!Xml2PrefabRoot.UseOnlyXML)
                {
                    var obj = Xml2PrefabUtils.LoadPrefab(name);
                    if (obj == null)
                    {
                        objectRunner = new ObjectRunner(num, null);
                        num += (objectRunner as ObjectRunner).Parse(childNode, null, _ChoisesDictionary);
                        objectRunner.Init();
                    }
                    else
                    {
                        var container = obj.GetComponent<Xml2PrefabObjectRunnerContainer>();
                        objectRunner = new SerializedObjectRunner(num, null);
                        num += (objectRunner as SerializedObjectRunner).Parse(container, _ChoisesDictionary);
                        objectRunner.Init();
                    }
                }
                else
                {
                    objectRunner = new ObjectRunner(num, null);
                    num += (objectRunner as ObjectRunner).Parse(childNode, null, _ChoisesDictionary);
                    objectRunner.Init();
                }
                _Objects.Add(objectRunner);
                VisualContainer visualContainer = null;
                if (!_Containers.ContainsKey(objectRunner.Factor))
                {
                    visualContainer = new VisualContainer(objectRunner.Factor, (factors.Count - factors.IndexOf(objectRunner.Factor)) * 10);
                    visualContainer.Object.transform.SetParent(levelObject.transform);
                    _Containers[objectRunner.Factor] = visualContainer;
                }
                else
                {
                    visualContainer = _Containers[objectRunner.Factor];
                }
                objectRunner.SetLayer(visualContainer.Object);
            }
        }

        public static XmlNode ObjectNode(string name)
        {
            if (Current != null && Current._ObjectsNodes.ContainsKey(name))
            {
                return Current._ObjectsNodes[name];
            }
            Debug.LogError("Object not find: " + name);
            return null;
        }
    }
}
