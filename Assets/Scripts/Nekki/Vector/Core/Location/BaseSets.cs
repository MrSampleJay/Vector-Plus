using Nekki.Vector.Core.Location.Animation;
using Nekki.Vector.Core.Location.LevelCreation;
using Nekki.Vector.Core.User;
using Nekki.Vector.Core.Visual;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Xml2Prefab;

namespace Nekki.Vector.Core.Location
{
    public class BaseSets
    {
        protected Dictionary<string, string> _ChoisesDictionary = new Dictionary<string, string>();

        protected Dictionary<float, VisualContainer> _Containers = new Dictionary<float, VisualContainer>();

        protected List<Runner> _allRunners = new List<Runner>();

        public Dictionary<string, XmlNode> _ObjectsNodes = new Dictionary<string, XmlNode>();

        protected List<UserData> _UserData = new List<UserData>();

        protected string _music;

        protected bool _ReloadMusicOnReset;

        protected List<BaseObjectRunner> _Objects = new List<BaseObjectRunner>();

        public List<VisualRunner> Visuals = new List<VisualRunner>();

        public List<PlatformRunner> Platforms = new List<PlatformRunner>();

        public List<TriggerRunner> Triggers = new List<TriggerRunner>();

        public List<AreaRunner> Areas = new List<AreaRunner>();

        public List<AnimationRunner> Animations = new List<AnimationRunner>();

        public List<ParticleRunner> Particles = new List<ParticleRunner>();

        public List<ItemRunner> Items = new List<ItemRunner>();

        public List<TrapezoidRunner> Trapezoids = new List<TrapezoidRunner>();

        public List<CameraRunner> Cameras = new List<CameraRunner>();

        public List<SpawnRunner> Spawns = new List<SpawnRunner>();

        public List<QuadRunner> Quads = new List<QuadRunner>();

        public List<QuadRunner> QuadsAll = new List<QuadRunner>();

        public List<PrimitiveRunner> Primitives = new List<PrimitiveRunner>();

        public static BaseSets Current
        {
            get;
            protected set;
        }

        public Dictionary<float, VisualContainer> Containers => _Containers;

        public List<Runner> AllRunners => _allRunners;

        public int TotalCoins
        {
            get;
            protected set;
        }

        public List<UserData> UserData => _UserData;

        public string Music => _music;

        public bool ResetMusicOnReload => _ReloadMusicOnReset;

        public bool DebugMode
        {
            set
            {
                foreach (var obj in _Objects)
                {
                    obj.DebugMode = value;
                }
            }
        }

        public int totalPoints;

        public int totalTricks;

        public int totalBonus;

        protected BaseSets()
        {
            Current = this;
        }

        public void ParseChoices(XmlNode nodes)
        {
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    _ChoisesDictionary[node.Attributes["Name"].Value] = node.Attributes["Variant"].Value;
                }
            }
            string mode = "CommonMode";
            if (LevelMainController.IsHunterMode)
            {
                mode = "HunterMode";
            }
            _ChoisesDictionary["AITriggers"] = mode;
        }

        public void ParseChoices(List<ChoiceContainer> data)
        {
            foreach (var choice in data)
            {
                _ChoisesDictionary.Add(choice.Name, choice.Variant);
            }
            if (Xml2PrefabRoot.Serialize)
            {
                return;
            }
            string mode = "CommonMode";
            if (LevelMainController.IsHunterMode)
            {
                mode = "HunterMode";
            }
            _ChoisesDictionary["AITriggers"] = mode;
        }

        protected void ParseModels(IEnumerable<XmlNode> nodes)
        {
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes["Choice"] == null)
                {
                    AddModels(node);
                    return;
                }
                string choice = node.Attributes["Choice"].Value;
                string variant = node.Attributes["Variant"].Value;

                if (_ChoisesDictionary[choice] == variant)
                {
                    AddModels(node);
                }
            }
        }

        public void AddModels(XmlNode Nodes)
        {
            foreach (XmlNode node in Nodes.ChildNodes)
            {
                _UserData.Add(Xml2PrefabUtils.GetUserData(node));
            }
        }

        public void InitRunners()
        {
            foreach (var runner in AllRunners)
            {
                //runner.InitRunner(null, Xml2PrefabRoot.Serialize);
            }
        }

        public void InitTriggers()
        {
            foreach (var trigger in Triggers)
            {
                trigger.Init();
            }
        }

        public void InitParticles()
        {
            foreach (var particle in Particles)
            {
                particle.Init();
            }
        }

        public void InitAnimations()
        {
            foreach (var animation in Animations)
            {
                animation.Init();
            }
        }

        public void InitPrimitives()
        {
            foreach (var primitive in Primitives)
            {
                primitive.Load();
            }
        }

        public void InitVisual()
        {
            foreach (var visual in Visuals)
            {
                visual.Init();
            }
        }

        public void InitAreas()
        {
            foreach (var area in Areas)
            {

            }
        }

        public void GetElements(List<BaseObjectRunner> objectRunners)
        {
            foreach (var objectRunner in objectRunners)
            {
                foreach (var runner in objectRunner.Element.Runners)
                {
                    _allRunners.Add(runner);
                }
                Add(objectRunner.Element);
                GetElements(objectRunner.Childs);
            }
        }

        public void SetTotals()
        {
            totalBonus = Items.Count(o => o.Type == 0);
            totalTricks = Areas.Count(t => t is TrickAreaRunner);
            int points = 0;
            points += Areas.OfType<TrickAreaRunner>().Sum(i => i.score);
            points += Items.OfType<ItemScoreRunner>().Sum(l => l.score);

            totalPoints = points;
        }
        public void Add(BaseElements elements)
        {
            Visuals.AddRange(elements.Visuals);
            Platforms.AddRange(elements.Platforms);
            Triggers.AddRange(elements.Triggers);
            Areas.AddRange(elements.Areas);
            Spawns.AddRange(elements.Spawns);
            Cameras.AddRange(elements.Cameras);
            Trapezoids.AddRange(elements.Trapezoids);
            Primitives.AddRange(elements.Primitives);
            Animations.AddRange(elements.Animations);
            Particles.AddRange(elements.Particles);
            Items.AddRange(elements.Items);

            Quads.AddRange(elements.Platforms);
            QuadsAll.AddRange(elements.Platforms);

            QuadsAll.AddRange(elements.Triggers);
            QuadsAll.AddRange(elements.Areas);

            Quads.AddRange(elements.Trapezoids);
            QuadsAll.AddRange(elements.Trapezoids);

        }
    }
}
