using Nekki.Vector.Core.Location.Animation;
using Nekki.Vector.Core.Location.LevelCreation;
using Nekki.Vector.Core.Utilites;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using Xml2Prefab;
using Nekki.Vector.Core.Models;


namespace Nekki.Vector.Core.Location
{
    public class Elements : BaseElements
    {

        public Elements(BaseObjectRunner parent, uint index)
            : base(parent, index)
        {
        }

        public uint Parse(XmlNode mainNode, Dictionary<string, string> choices)
        {
            if (mainNode == null)
            {
                return Index;
            }
            uint num = 0;
            for (int i = 0; i < mainNode.ChildNodes.Count; i++)
            {
                XmlNode node = mainNode.ChildNodes[i];
                ChoiceContainer choice = null;
                Runner runner = null;
                if (node["Properties"] != null && node["Properties"]["Static"] != null && node["Properties"]["Static"]["Selection"] != null)
                {
                    var selection = node["Properties"]["Static"]["Selection"];
                    choice = new ChoiceContainer(selection.Attributes["Choice"].Value, selection.Attributes["Variant"].Value);
                    if (!Xml2PrefabRoot.Serialize)
                    {
                        if (choices[choice.Name] != choice.Variant)
                        {
                            continue;
                        }
                    }
                }

                switch (node.Name)
                {
                    case "Object":
                        num += (_Parent as ObjectRunner).CreateChild(node, (num + Index) - 1, choices);
                        continue;
                    case "Image":
                        runner = CreateVisual(node);
                        if (runner != null)
                            _Visuals.Add((VisualRunner)runner);
                        break;
                    case "Trigger":
                        runner = CreateTrigger(node);
                        if (runner != null)
                            _Triggers.Add((TriggerRunner)runner);
                        break;
                    case "Area":
                        runner = CreateArea(node);
                        if (runner != null)
                            _Areas.Add((AreaRunner)runner);
                        break;
                    case "Spawn":
                        runner = CreateSpawn(node);
                        if (runner != null)
                            _Spawns.Add((SpawnRunner)runner);
                        break;
                    case "Camera":
                        runner = CreateCamera(node);
                        if (runner != null)
                            _Cameras.Add((CameraRunner)runner);
                        break;
                    case "Platform":
                        runner = CreatePlatform(node);
                        if (runner != null)
                            _Platforms.Add((PlatformRunner)runner);
                        break;
                    case "Trapezoid":
                        runner = CreateTrapezoid(node);
                        if (runner != null)
                            _Trapezoids.Add((TrapezoidRunner)runner);
                        break;
                    case "Item":
                        runner = CreateItem(node);
                        if (runner != null)
                            _Items.Add((ItemRunner)runner);
                        break;
                    case "Model":
                        runner = CreatePrimitive(node);
                        if (runner != null)
                            _Primitives.Add((PrimitiveRunner)runner);
                        break;
                    case "Animation":
                        runner = CreateAnimation(node);
                        if (runner != null)
                            _Animations.Add((AnimationRunner)runner);
                        break;
                    case "Particle":
                        runner = CreateParticle(node);
                        if (runner != null)
                            _Particles.Add((ParticleRunner)runner);
                        break;
                    case "SoundSource":
                        runner = CreateSoundsource(node);
                        if (runner != null)
                            _SoundSources.Add((SoundSourceRunner)runner);
                        break;
                     }

                if (runner != null)
                {
                    num++;
                    runner.SetVariant(choice);
                    _runners.Add(runner);
                    runner.Generate();
                    _Parent.LayerOrder.Add(runner);
                    runner.Index = num + Index;
                }
            }
            Init();
            return num;
        }
        private SoundSourceRunner CreateSoundsource(XmlNode Node)
        {
            if (Node == null)
            {
                return null;
            }
            float p_x = XmlUtils.ParseFloat(Node.Attributes["X"]);
            float p_y = XmlUtils.ParseFloat(Node.Attributes["Y"]);
            SoundSourceRunner soundSourceRunner = new SoundSourceRunner(p_x, p_y, Node);
            soundSourceRunner.Layer = _Parent.Layer;
            soundSourceRunner.SetXmlList(Xml2PrefabUtils.GetTransformationNode(Node));
            return soundSourceRunner;
        }
        private TriggerRunner CreateTrigger(XmlNode Node)
        {
            var x = Node.Attributes["X"].ParseFloat();
            var y = Node.Attributes["Y"].ParseFloat();
            var width = Node.Attributes["Width"].ParseFloat();
            var height = Node.Attributes["Height"].ParseFloat();
            var trigger = new TriggerRunner(x, y, width, height, Node);
            trigger.Layer = _Parent.Layer;
            trigger.SetXmlList(Xml2PrefabUtils.GetTransformationNode(Node));
            return trigger;
        }

        private AreaRunner CreateArea(XmlNode node)
        {
            AreaRunner area = null;
            var name = node.Attributes["Name"].ParseString();
            string type = node.Attributes["Type"].ParseString();
            var x = node.Attributes["X"].ParseFloat();
            var y = node.Attributes["Y"].ParseFloat();
            var width = node.Attributes["Width"].ParseFloat();
            var height = node.Attributes["Height"].ParseFloat();
            int direction = node.Attributes["Direction"].ParseInt(0); ;
            switch (type)
            {
                case "Trick":
                    string itemName = XmlUtils.ParseString(node.Attributes["ItemName"]);
                    int score = XmlUtils.ParseInt(node.Attributes["Score"]);
                    area = new TrickAreaRunner(x, y, width, height, type, name, itemName, score, direction);
                    break;
                case "Catch":
                    float distance = node.Attributes["Distance"].ParseFloat(200);
                    area = new ArrestAreaRunner(x, y, width, height, type, name, distance);
                    break;
                case "Help":
                    string key = XmlUtils.ParseString(node.Attributes["Key"]);
                    string description = XmlUtils.ParseString(node.Attributes["Description"]);
                    area = new TutorialAreaRunner(x, y, width, height, type, name, key, description, direction);
                    break;
                default:
                    area = new AreaRunner(AreaRunner.AreaType.None, x, y, width, height, type, name, direction);
                    break;
            }
            area.Layer = _Parent.Layer;
            area.SetXmlList(Xml2PrefabUtils.GetTransformationNode(node));
            return area;
        }

        private VisualRunner CreateVisual(XmlNode node)
        {
            var name = node.Attributes["ClassName"].ParseString();
            int type = node.Attributes["Type"].ParseInt();
            var x = node.Attributes["X"].ParseFloat();
            var y = node.Attributes["Y"].ParseFloat();
            var width = node.Attributes["Width"].ParseFloat(float.NaN);
            var height = node.Attributes["Height"].ParseFloat(float.NaN);
            if (node.Attributes["TrMatrix"] != null)
            {
                width = node.Attributes["NativeX"].ParseFloat(float.NaN);
                height = node.Attributes["NativeY"].ParseFloat(float.NaN);
            }
            Color color = Color.white;
            if (node.Attributes["Color"] != null)
            {
                color = ColorUtils.FromHex(node.Attributes["Color"].Value);
            }
            if (node["Properties"] != null && node["Properties"]["Static"] != null && node["Properties"]["Static"]["StartColor"] != null)
            {
                color = ColorUtils.FromHex(node["Properties"]["Static"]["StartColor"].Attributes["Color"].Value);
            }
            int depth = XmlUtils.ParseInt(node.Attributes["Depth"], -1);
            XmlNode matrixNode = null;
            if (node["Properties"] != null && node["Properties"]["Static"] != null && node["Properties"]["Static"]["Matrix"] != null)
            {
                matrixNode = node["Properties"]["Static"]["Matrix"];
                var tX = matrixNode.Attributes["Tx"].ParseFloat();
                var tY = matrixNode.Attributes["Ty"].ParseFloat();
                x += tX;
                y += tY;
            }
            var visual = new VisualRunner(type, name, new Pointd(x, y), width, height, color, depth, matrixNode);
            visual.Layer = _Parent.Layer;
            visual.SetXmlList(Xml2PrefabUtils.GetTransformationNode(node));
            return visual;
        }

        private PlatformRunner CreatePlatform(XmlNode node)
        {
            var name = node.Attributes["Name"].ParseString();
            var x = node.Attributes["X"].ParseFloat();
            var y = node.Attributes["Y"].ParseFloat();
            var width = node.Attributes["Width"].ParseFloat();
            var height = node.Attributes["Height"].ParseFloat();
            var sticky = node.Attributes["Sticky"].ParseBool(true);
            var material = node.Attributes["Material"].ParseString("");
            var platform = new PlatformRunner(name, x, y, width, height, sticky, material);
            platform.Layer = _Parent.Layer;
            platform.SetXmlList(Xml2PrefabUtils.GetTransformationNode(node));
            return platform;
        }

        private TrapezoidRunner CreateTrapezoid(XmlNode node)
        {
            var name = node.Attributes["Name"].ParseString();
            var x = node.Attributes["X"].ParseFloat();
            var y = node.Attributes["Y"].ParseFloat();
            int type = node.Attributes["Type"].ParseInt(1);
            var width = node.Attributes["Width"].ParseFloat();
            var height = node.Attributes["Height"].ParseFloat();
            var height1 = node.Attributes["Height1"].ParseFloat();
            var sticky = node.Attributes["Sticky"].ParseBool(true);
            var material = node.Attributes["Material"].ParseString("");
            var trapezoid = new TrapezoidRunner(name, type, x, y, width, height, height1, sticky, material);
            trapezoid.Layer = _Parent.Layer;
            trapezoid.SetXmlList(Xml2PrefabUtils.GetTransformationNode(node));
            return trapezoid;
        }

        private ItemRunner CreateItem(XmlNode node)
        {
            ItemRunner item = null;
            int type = node.Attributes["Type"].ParseInt();
            var x = node.Attributes["X"].ParseFloat();
            var y = node.Attributes["Y"].ParseFloat();
            if (type == 0)
            {
                int score = node.Attributes["Score"].ParseInt();
                string animIdle = node.Attributes["IdleClassName"].ParseString("bonus_v4");
                string animOff =  node.Attributes["OffClassName"].ParseString("bonus_v4_off");
                item = new ItemScoreRunner(type, "ScoreItem", score, x, y, animIdle, animOff);
            }
            else
            {
                int groupId = node.Attributes["GroupId"].ParseInt();
                int score = node.Attributes["Score"].ParseInt();
                item = new CoinRunner(type, "CoinItem", groupId, score, x, y);
            }
            item.Layer = _Parent.Layer;
            item.SetXmlList(Xml2PrefabUtils.GetTransformationNode(node));
            return item;
        }

        private PrimitiveRunner CreatePrimitive(XmlNode node)
        {
            PrimitiveRunner primitive = null;
            var name = node.Attributes["ClassName"].ParseString();
            int type = node.Attributes["Type"].ParseInt();
            var x = node.Attributes["X"].ParseFloat();
            var y = node.Attributes["Y"].ParseFloat();
            var impulse = node.Attributes["Impuls"].ParseFloat(30);
            List<string> sounds = node.Attributes["Sounds"].ParseString(string.Empty).Split('|').ToList();
            Vector3f deltaPosition = new Vector3f(x, y);
            if (type == 0)
            {
                primitive = new PrimitiveRunner(0, name, Color.black, deltaPosition, impulse, sounds);
            }
            else
            {
                primitive = new PrimitiveAnimatedRunner(type, name, Color.black, deltaPosition, impulse, sounds);
                primitive.Type = ModelType.PrimitiveAnimated;
            }
            primitive.Layer = _Parent.Layer;
            primitive.SetXmlList(Xml2PrefabUtils.GetTransformationNode(node));
            return primitive;
        }

        private ParticleRunner CreateParticle(XmlNode node)
        {
            var name = node.Attributes["ClassName"].ParseString();
            var x = node.Attributes["X"].ParseFloat();
            var y = node.Attributes["Y"].ParseFloat();
            var width = node.Attributes["Width"].ParseFloat();
            var height = node.Attributes["Height"].ParseFloat();
            var particle = new ParticleRunner(x, y, width, height, name);
            particle.Layer = _Parent.Layer;
            particle.SetXmlList(Xml2PrefabUtils.GetTransformationNode(node));
            return particle;
        }

        private AnimationRunner CreateAnimation(XmlNode node)
        {
            AnimationRunner animation = null;
            var name = node.Attributes["ClassName"].ParseString();
            if (name == "p_dust2")
            {
                return null;
            }
            int type = node.Attributes["Type"].ParseInt();
            var x = node.Attributes["X"].ParseFloat();
            var y = node.Attributes["Y"].ParseFloat();
            var width = node.Attributes["Width"].ParseFloat();
            var height = node.Attributes["Height"].ParseFloat();
            var scaleX = node.Attributes["ScaleX"].ParseFloat(1);
            var scaleY = node.Attributes["ScaleY"].ParseFloat(1);
            if (type == 1)
            {
                Vector2 direction = Vector2.zero;
                Vector2 acceleration = Vector2.zero;
                int life = node.Attributes["Time"].ParseInt();
                if (node.Attributes["Direction"] != null)
                {
                    string[] value = node.Attributes["Direction"].Value.Split('|');
                    direction = new Vector2(float.Parse(value[0]), float.Parse(value[1]));
                }
                if (node.Attributes["Acceleration"] != null)
                {
                    string[] value = node.Attributes["Acceleration"].Value.Split('|');
                    acceleration = new Vector2(float.Parse(value[0]), float.Parse(value[1]));
                }
                animation = new AnimationVectorRunner(x, y, width, height, name, 1, scaleX, scaleY, direction, acceleration, life * 60);
            }
            else
            {
                animation = new AnimationRunner(x, y, width, height, name, type, scaleX, scaleY, false, 2);
            }
            animation.Layer = _Parent.Layer;
            animation.SetXmlList(Xml2PrefabUtils.GetTransformationNode(node));
            return animation;
        }

        private CameraRunner CreateCamera(XmlNode node)
        {
            var x = node.Attributes["X"].ParseFloat();
            var y = node.Attributes["Y"].ParseFloat();
            CameraRunner camera = new CameraRunner(x, y, "", "");
            camera.Layer = _Parent.Layer;
            camera.SetXmlList(Xml2PrefabUtils.GetTransformationNode(node));
            return camera;
        }

        private SpawnRunner CreateSpawn(XmlNode node)
        {
            var x = node.Attributes["X"].ParseFloat();
            var y = node.Attributes["Y"].ParseFloat();
            var animation = node.Attributes["Animation"].ParseString();
            var name = node.Attributes["Name"].ParseString();
            SpawnRunner spawn = new SpawnRunner(x, y, name, animation);
            spawn.Layer = _Parent.Layer;
            spawn.SetXmlList(Xml2PrefabUtils.GetTransformationNode(node));
            return spawn;
        }
    }
}
