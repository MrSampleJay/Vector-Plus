using System.Collections.Generic;
using System.Xml;
using Core._Common;
using Nekki.Vector.Core.Camera;
using Nekki.Vector.Core.Controllers;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nekki.Vector.Core.Animation
{
    public class AnimationLoader
    {
        public string _xmlPath;

        Dictionary<string, XmlNode> _eventGroups = new Dictionary<string, XmlNode>();

        public static AnimationLoader Current
        {
            get;
        }

        public AnimationInfo Info
        {
            get;
        }

        static AnimationLoader()
        {
            Current = new AnimationLoader();
        }

        public void Init()
        {
            _xmlPath = VectorPaths.Animations;
            ParseAnimations();
        }

        public void ReloadAnimations()
        {
            AnimationBinaryParser.ClearCachedBinary();
            AnimationGroup.ClearGroups();
            Animations.Animation.Clear();
            AnimationReaction.Reactions.Clear();
            AnimationTrickInfo.TricksLoaded.Clear();
            ParseAnimations();
        }

        public void ParseAnimations()
        {
            XmlNode config = XmlUtils.OpenXMLDocument(_xmlPath, "config.xml")["Config"];

            // Parse Every ReactionGroup and EventGroup First
            foreach (XmlNode File in config.ChildNodes)
            {
                XmlNode xmlNode = XmlUtils.OpenXMLDocument(_xmlPath, File.Attributes["Name"].Value)["root"];

                if (File.Attributes["ParseReactionGroups"]?.Value == "1" || File.Attributes["Main"]?.Value == "1") 
                    ParseGroups(xmlNode["ReactionGroups"]);

                if (File.Attributes["ParseEventGroups"]?.Value == "1" || File.Attributes["Main"]?.Value == "1")
                {
                    foreach (XmlNode childNode in xmlNode["EventGroups"].ChildNodes)
                        _eventGroups[childNode.Attributes["Name"].Value] = childNode;
                }
            }

            // Parse The Rest
            foreach (XmlNode File in config.ChildNodes)
            {
                XmlNode xmlNode = XmlUtils.OpenXMLDocument(_xmlPath, File.Attributes["Name"].Value)["root"];

                if (File.Attributes["Main"]?.Value == "1")
                {
                    XmlNode movesNode = xmlNode["Moves"];
                    ParseConfigs(xmlNode["Config"]);
                    ParseMoves(movesNode);
                    continue;
                }

                if (File.Attributes["ParseMoves"]?.Value == "1")
                {
                    XmlNode movesNode = xmlNode["Moves"];
                    ParseMoves(movesNode);
                }

                if (File.Attributes["ParseAnimationIntervals"]?.Value == "1")
                    ParseExtraIntervals(xmlNode["MoveIntervals"]);

                if (File.Attributes["ParseSoundDictionaries"]?.Value == "1")
                {
                    Debug.Log("SoundDictionary Loading");
                    foreach (XmlNode childNode in xmlNode["SoundDictionaries"].ChildNodes)
                        SoundDictionary.ParseSoundDictionaries(childNode);
                }
            }
            AnimationBinaryParser.ClearCachedBinary();
        }

        public void ParseMoves(XmlNode moves)
        {
            foreach (XmlNode childNode2 in moves.ChildNodes)
            {
                AnimationInfo animationInfo = null;
                animationInfo = ((childNode2.Attributes["Trick"] != null && XmlUtils.ParseInt(childNode2.Attributes["Trick"]) != 0) ? new AnimationTrickInfo(childNode2) : new AnimationInfo(childNode2));

                string BinaryPath = moves.Attributes["BinPath"]?.Value;

                if (!string.IsNullOrEmpty(BinaryPath))
                    animationInfo.Folder = BinaryPath;

                foreach (XmlNode childNode3 in childNode2.ChildNodes)
                {
                    if (childNode3.Name != "Interval")
                    {
                        continue;
                    }
                    AnimationInterval animationInterval = null;
                    if (childNode3.Attributes["Groups"] == null)
                    {
                        animationInterval = new AnimationInterval(childNode3);
                    }
                    else
                    {
                        List<XmlNode> list = new List<XmlNode>();
                        string[] array = childNode3.Attributes["Groups"].Value.Split('|');
                        foreach (string key in array)
                        {
                            if (!_eventGroups.ContainsKey(key))
                            {
                                continue;
                            }
                            list.Add(_eventGroups[key]);
                        }
                        animationInterval = new AnimationInterval(childNode3, list);
                    }
                    animationInfo.Intervals.Add(animationInterval);
                }// Interval Processing
                Animations.Animation[animationInfo.Name] = animationInfo;
            }
        }
        public void ParseExtraIntervals(XmlNode moves)
        {
            foreach (XmlNode modNode in moves.ChildNodes)
            {
                // modNode is the move from the file that will override existing intervals
                if (!Animations.Animation.TryGetValue(modNode.Name, out AnimationInfo moveOverride))
                {
                    continue;
                }

                foreach (XmlNode interval in modNode.ChildNodes)
                {
                    if (interval.Name != "Interval")
                    {
                        continue;
                    }

                    // Interval Parsing Begins
                    AnimationInterval newInterval = null;
                    if (interval.Attributes["Groups"] == null)
                    {
                        newInterval = new AnimationInterval(interval);
                    }
                    else
                    {
                        List<XmlNode> list = new List<XmlNode>();
                        string[] array = interval.Attributes["Groups"].Value.Split('|');
                        foreach (string key in array)
                        {
                            if (!_eventGroups.ContainsKey(key))
                            {
                                continue;
                            }
                            list.Add(_eventGroups[key]);
                        }
                        newInterval = new AnimationInterval(interval, list);
                    }

                    if (interval.Attributes["OverrideIndex"] != null)
                    {
                        int index = XmlUtils.ParseInt(interval.Attributes["OverrideIndex"]);
                        if (!(index >= 0 && index < moveOverride.Intervals.Count))
                        {
                            continue;
                        }
                        moveOverride.Intervals[index] = newInterval;
                    }
                    else
                    {
                        moveOverride.Intervals.Insert(moveOverride.Intervals.Count, newInterval);
                    }
                   
                }
            }
        }

        public static void ParseGroups(XmlNode node)
        {   
            foreach (XmlNode childNode in node.ChildNodes)
            {
                List<AnimationReaction> list = new List<AnimationReaction>();
                foreach (XmlNode childNode2 in childNode.ChildNodes)
                {
                    list.Add(new AnimationReaction(childNode2));
                }
                AnimationGroup animationGroup = new AnimationGroup();
                animationGroup.Reactions = list;
                animationGroup.Name = childNode.Attributes["Name"].Value;
                if (node.Attributes["Append"]?.Value == "1")
                {
                    AnimationGroup.AddReactions(animationGroup.Name, list);
                    continue;
                }
                AnimationGroup.Add(animationGroup);
            }
        }

        public static void ParseSoundGroups(XmlNode node)
        {
            foreach (XmlNode childNode in node.ChildNodes)
            {
                List<AnimationSound> list = new List<AnimationSound>();
                foreach (XmlNode childNode2 in childNode.ChildNodes)
                {
                    list.Add(new AnimationSound(childNode2));
                }
                AnimationSoundGroup animationGroup = new AnimationSoundGroup();
                animationGroup.Sounds = list;
                animationGroup.Name = childNode.Attributes["Name"].Value;
                AnimationSoundGroup.Add(animationGroup);
            }
        }

        public static AnimationReaction ParseReaction(string[] pArray)
        {
            if (pArray.Length < 1 && int.Parse(pArray[1]) == 0)
            {
                return null;
            }
            string name = pArray[0];
            int frame = !string.IsNullOrEmpty(pArray[1]) ? int.Parse(pArray[1]) : 0;
            AnimationReaction animationReaction = new AnimationReaction(name, frame);
            if (pArray.Length == 2)
            {
                animationReaction.Reverse = false;
            }
            else
            {
                animationReaction.Reverse = int.Parse(pArray[2]) > 0;
            }
            return animationReaction;
        }

        private void ParseConfigs(XmlNode xmlNode)
        {
            XmlNode cameraNode = xmlNode["Camera"];
            LocationCamera.MinZoom = cameraNode.Attributes["MinZoom"].ParseFloat();
            LocationCamera.MaxZoom = cameraNode.Attributes["MaxZoom"].ParseFloat();
            LocationCamera.CurrentZoom = cameraNode.Attributes["CurrZoom"].ParseFloat();

            XmlNode taserNode = xmlNode["Taser"];
            ControllerCatching.DistanceFactor = taserNode.Attributes["Distance"].ParseFloat();
            ControllerCatching.Timeout = taserNode.Attributes["Time"].ParseFloat();
            ControllerCatching.HeightFactor = taserNode.Attributes["HeightFactor"].ParseFloat();
        }
    }
}
