using System.Collections.Generic;
using System.Xml;
using Nekki.Vector.Core.User;
using Nekki.Vector.Core.Utilites;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Xml2Prefab
{
    public static class Xml2PrefabUtils
    {
        public static List<string> AllowedFolders;

        public static List<string> _directories = new List<string>
        {
            "buildings",
            "buildings_construction",
            "buildings_downtown",
            "buildings_techpark",
            "buildings_techpark_bonus",
            "objects",
            "objects_construction",
            "objects_downtown",
            "objects_techpark"
        };

        public static UserData GetUserData(XmlNode node)
        {
            var userData = new UserData("");
            userData.Name = node.Attributes["Name"].Value;
            userData.BirthSpawn = node.Attributes["BirthSpawn"].ParseString("");
            userData.isIcon = node.Attributes["Icon"].ParseBool();
            userData.Color = node.Attributes["Color"] == null ? Color.black : ColorUtils.FromHex(node.Attributes["Color"].Value);
            userData.Skins = node.Attributes["Skins"] == null ? new List<string>() : new List<string>(node.Attributes["Skins"].Value.Split('|'));
            userData.Stocks = node.Attributes["Stocks"] == null ? new List<string>() : new List<string>(node.Attributes["Stocks"].Value.Split('|'));
            userData.Arrests = node.Attributes["Arrests"] == null ? new List<string>() : new List<string>(node.Attributes["Arrests"].Value.Split('|'));
            userData.Murders = node.Attributes["Murders"] == null ? new List<string>() : new List<string>(node.Attributes["Murders"].Value.Split('|'));
            userData.Respawns = node.Attributes["Respawns"] == null ? new List<string>() : new List<string>(node.Attributes["Respawns"].Value.Split('|'));
            userData.Births = node.Attributes["AllowedSpawns"] == null ? new List<string>() : new List<string>(node.Attributes["AllowedSpawns"].Value.Split('|'));
            userData.IsSelf = node.Attributes["Type"].ParseBool();
            userData.IsTrick = node.Attributes["Trick"].ParseBool();
            userData.IsItem = node.Attributes["Item"].ParseBool();
            userData.IsVictory = node.Attributes["Victory"].ParseBool();
            userData.IsLost = node.Attributes["Lose"].ParseBool();
            userData.AI = int.Parse(node.Attributes["AI"].Value);
            userData.StartTime = node.Attributes["Time"].ParseFloat();
            userData.LiveTime = node.Attributes["LifeTime"].ParseFloat(2);
            userData.Voice = node.Attributes["Voice"].ParseString("");
            if (userData.Skins.Count == 0)
            {
                userData.Skins.Add("1");
            }
            userData.Init();
            return userData;
        }

        public static XmlNode GetTransformationNode(XmlNode node)
        {
            if (node["Properties"] != null && node["Properties"]["Dynamic"] != null)
                return node["Properties"];
            return null;
        }

        public static GameObject LoadPrefab(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            foreach (var directory in _directories)
            {
                var path = "LevelContent/Prefabs/" + directory + "/" + name;
                var obj = Resources.Load<GameObject>(path);

                if (obj != null)
                {
#if UNITY_EDITOR
                    return (GameObject)PrefabUtility.InstantiatePrefab(obj);
#else
                    var inst = Object.Instantiate(obj);
                    inst.name = name;
                    return inst;
#endif

                }
            }
            return null;
        }

        public static GameObject LoadPrefabAsAsset(string name)
        {
            foreach (var directory in _directories)
            {
                var path = "LevelContent/Prefabs/" + directory + "/" + name;
                var obj = Resources.Load<GameObject>(path);
                if (obj != null)
                {
                    return obj;
                }
            }
            return null;
        }
    }
}
