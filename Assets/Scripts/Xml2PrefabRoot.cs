using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Core._Common;
using Nekki.Vector.Core.Location;
using Nekki.Vector.Core.Location.LevelCreation;
using Nekki.Vector.Core.Visual;
using UnityEditor;
using UnityEngine;
using Xml2Prefab;
using Object = UnityEngine.Object;

public class Xml2PrefabRoot
{
    public static bool Serialize;
    public static bool UseOnlyXML;

    private Dictionary<XmlNode, int> _DepthDictionary = new Dictionary<XmlNode, int>();
    private string _debugSymbols = "__Vector_Debug_Objects__;VEC_DEBUG";
    private List<string> _excludeFromBuildFiles = new List<string>();
#if UNITY_EDITOR
    [MenuItem("Xml2Prefab/Parse")]
    public static void Start()
    {
        new Xml2PrefabRoot().Parse();
    }

    [MenuItem("Xml2Prefab/Only Levels")]
    public static void OnlyLevels()
    {
        Serialize = true;
        new Xml2PrefabRoot().Levels();
    }
#endif
    public void ParseOnlyLevel(string level)
    {
        Serialize = true;
        Levels(level);
    }

    public void ParseOnlyBuilding(string building)
    {
        Serialize = true;
        Buildings(building);
    }

    public void Parse()
    {
        Serialize = true;
        UseOnlyXML = false;
        Objects(Directory.GetFiles("Assets/Resources/" + VectorPaths.XmlRoot));
        Buildings(Directory.GetFiles("Assets/Resources/" + VectorPaths.XmlRoot));
        Levels();
    }


    public void ExcludeFromBuild(IEnumerable<string> files)
    {
        _excludeFromBuildFiles = new List<string>(files);
    }

    private void Objects(string[] allFiles)
    {
        allFiles = allFiles.Select(p => Path.GetFileName(p)).Where(s => s.Contains("objects") && !s.Contains(".meta") && !_excludeFromBuildFiles.Contains(s)).ToArray();
        foreach (string file in allFiles)
        {
            var document = XmlUtils.OpenXMLDocument(VectorPaths.XmlRoot, file);
            ParseObjects(document, file.Replace(".xml", ""), "Objects");
        }
    }

    private void Buildings(string[] allFiles)
    {
        allFiles = allFiles.Select(p => Path.GetFileName(p)).Where(s => s.Contains("buildings") && !s.Contains(".meta") && !_excludeFromBuildFiles.Contains(s)).ToArray();
        foreach (string file in allFiles)
        {
            var document = XmlUtils.OpenXMLDocument(VectorPaths.XmlRoot, file);
            ParseObjects(document, file.Replace(".xml", ""), "Objects");
        }
    }

    private void Levels()
    {
        if (!Directory.Exists("Assets/Resources/" + VectorPaths.LevelsPrefab))
        {
            Directory.CreateDirectory("Assets/Resources/" + VectorPaths.LevelsPrefab);
        }
        var files = Directory.GetFiles("Assets/Resources/" + VectorPaths.XmlLevels).Select(p => Path.GetFileName(p)).Where(s => !s.Contains("Trigger") && !s.Contains("buildings") && !s.Contains("objects") && !s.Contains(".meta") && !_excludeFromBuildFiles.Contains(s));
        foreach (var file in files)
        {
            var document = XmlUtils.OpenXMLDocument(VectorPaths.XmlLevels, file);
            ParseLevel(document, file.Replace(".xml", ""), "Track");
        }
    }

    private void Buildings(string building)
    {
        ParseObjects(XmlUtils.OpenXMLDocument(VectorPaths.XmlRoot, building), building.Replace(".xml", ""), "Objects");
    }

    private void Levels(string level)
    {
        var document = XmlUtils.OpenXMLDocument(VectorPaths.XmlLevels, level);
        ParseLevel(document, level.Replace(".xml", ""), "Track");
    }

    private void ParseObjects(XmlDocument document, string fileName, string rootNode)
    {
        if (document["Root"][rootNode] == null)
        {
            return;
        }
        var root = document["Root"][rootNode];
        string filePath = "Assets/Resources/LevelContent/Prefabs/" + fileName + "/";
        if (Directory.Exists(filePath))
        {
            return;
        }
        Directory.CreateDirectory(filePath);
        GameObject obj = new GameObject();
        foreach (var (node, depth) in FindLowLevelNodes(root.ChildNodes))
        {
            if (node.Name != "Object") continue;
            var objectRunner = new ObjectRunner((uint)depth, null);
            objectRunner.Parse(node, null, new Dictionary<string, string>());
            objectRunner.Init();
            objectRunner.SetLayer(obj);
#if UNITY_EDITOR
            PrefabUtility.SaveAsPrefabAsset(objectRunner.UnityObject, filePath + objectRunner.UnityObject.name + ".prefab");
            AssetDatabase.Refresh();
#endif
            Object.DestroyImmediate(objectRunner.UnityObject);

        }
        Object.DestroyImmediate(obj);
    }

    private void ParseLevel(XmlDocument document, string fileName, string rootNode)
    {
        if (document["Root"][rootNode] == null)
        {
            return;
        }
        var levelobj = new GameObject(fileName);
        var filepath = "Assets/Resources/LevelContent/Prefabs/levels/" + fileName + ".prefab";
        Directory.CreateDirectory("Assets/Resources/LevelContent/Prefabs/levels/");
        var root = document["Root"][rootNode];
        List<float> factors = new List<float>();
        foreach (XmlNode xmlNode in root)
        {
            if (xmlNode.Attributes["Factor"] != null)
            {
                factors.Add(float.Parse(xmlNode.Attributes["Factor"].Value, CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        factors.Sort();
        var choices = ParseChoices(document);
        Dictionary<float, VisualContainer> factorsDicto = new Dictionary<float, VisualContainer>();
        var objectRunners = new List<Xml2PrefabObjectRunnerContainer>();
        var visualContainers = new List<Xml2PrefabVisualContainer>();
        foreach (var (node, depth) in FindLowLevelNodes(root.ChildNodes))
        {
            BaseObjectRunner objectRunner = null;
            string name = string.Empty;
            if (node.Attributes["Name"] != null)
            {
                name = node.Attributes["Name"].Value;
            }
            objectRunner = new ObjectRunner((uint)depth, null);
            (objectRunner as ObjectRunner).Parse(node, null, choices);
            objectRunner.Init();
            VisualContainer visualContainer = null;
            if (!factorsDicto.ContainsKey(objectRunner.Factor))
            {
                visualContainer = new VisualContainer(objectRunner.Factor, (factors.Count - factors.IndexOf(objectRunner.Factor)) * 10);
                factorsDicto[objectRunner.Factor] = visualContainer;
            }
            else
            {
                visualContainer = factorsDicto[objectRunner.Factor];
            }
            var container = visualContainer.Object.AddComponent<Xml2PrefabVisualContainer>();
            container.Init(visualContainer.Factor);
            visualContainers.Add(container);
            objectRunners.Add(objectRunner.UnityObject.GetComponent<Xml2PrefabObjectRunnerContainer>());
            objectRunner.SetLayer(visualContainer.Object);
        }
        foreach (var factor in visualContainers)
        {
            factor.gameObject.transform.SetParent(levelobj.transform);
        }
        var sets = document["Root"]["Sets"].InnerXml;
        var models = ParseModels(document["Root"]);
        var coins = document["Root"]["Coins"].Attributes["Value"].ParseInt();
        var music = document["Root"]["Music"].Attributes["Name"].Value;
        List<ChoiceContainer> choiceContainers = choices.Select(x => new ChoiceContainer(x.Value, x.Key)).ToList();
        levelobj.AddComponent<Xml2PrefabLevelContainer>().Init(sets, music, coins, "", choiceContainers, models, objectRunners, visualContainers);
#if UNITY_EDITOR
        PrefabUtility.SaveAsPrefabAsset(levelobj, filepath);
#endif
        Object.DestroyImmediate(levelobj);
    }

    private IEnumerable<(XmlNode node, int depth)> FindLowLevelNodes(XmlNodeList root)
    {
        _DepthDictionary.Clear();
        foreach (XmlNode n in root)
            if (n.NodeType == XmlNodeType.Element)
                _DepthDictionary[n] = FindDepth(n);

        return _DepthDictionary.OrderBy(kv => kv.Value)
                   .Select(kv => (kv.Key, kv.Value));
    }



    private int FindDepth(XmlNode node)
    {
        if (node["Content"] == null)
        {
            return 0;
        }
        List<int> list = new List<int>();
        foreach (XmlNode child in node["Content"])
        {
            if (child.Name == "Object")
            {
                list.Add(FindDepth(child) + 1);
            }
        }
        if (!list.Any())
        {
            return 0;
        }
        return list.Max();
    }

    public List<string> ParseModels(XmlNode nodes)
    {
        List<string> models = new List<string>();
        foreach (XmlNode node in nodes.ChildNodes.Cast<XmlNode>().Where(node => node.Name == "Models"))
        {
            models.Add(node.OuterXml);
        }
        return models;
    }

    private Dictionary<string, string> ParseChoices(XmlDocument document)
    {
        var nodes = document["Root"]["Choises"];
        var choices = new Dictionary<string, string>();
        if (nodes != null)
        {
            foreach (XmlNode node in nodes)
            {
                choices[node.Attributes["Name"].Value] = node.Attributes["Variant"].Value;
            }
        }
        string mode = "CommonMode";
        choices["AITriggers"] = mode;
        return choices;
    }
}
