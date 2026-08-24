using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Nekki.Vector.Core.Location.Animation;
using Nekki.Vector.Core.Location.LevelCreation;
using UnityEngine;
using Xml2Prefab;

namespace Nekki.Vector.Core.Location
{
    public abstract class BaseElements
    {
        protected BaseObjectRunner _Parent;

        protected List<VisualRunner> _Visuals = new List<VisualRunner>();

        protected List<PlatformRunner> _Platforms = new List<PlatformRunner>();

        protected List<TriggerRunner> _Triggers = new List<TriggerRunner>();

        protected List<AreaRunner> _Areas = new List<AreaRunner>();

        protected List<AnimationRunner> _Animations = new List<AnimationRunner>();

        protected List<ParticleRunner> _Particles = new List<ParticleRunner>();

        protected List<TrapezoidRunner> _Trapezoids = new List<TrapezoidRunner>();

        protected List<ItemRunner> _Items = new List<ItemRunner>();

        protected List<SpawnRunner> _Spawns = new List<SpawnRunner>();

        protected List<CameraRunner> _Cameras = new List<CameraRunner>();

        protected List<SoundSourceRunner> _SoundSources = new List<SoundSourceRunner>();

        protected List<PrimitiveRunner> _Primitives = new List<PrimitiveRunner>();

        protected List<Runner> _runners = new List<Runner>();

        private readonly List<Component> _models = new List<Component>();

        public BaseObjectRunner ParentObject => _Parent;

        public List<VisualRunner> Visuals => _Visuals;

        public List<PlatformRunner> Platforms => _Platforms;

        public List<TriggerRunner> Triggers => _Triggers;

        public List<AreaRunner> Areas => _Areas;

        public List<AnimationRunner> Animations => _Animations;

        public List<ParticleRunner> Particles => _Particles;

        public List<TrapezoidRunner> Trapezoids => _Trapezoids;

        public List<ItemRunner> Items => _Items;

        public List<SpawnRunner> Spawns => _Spawns;

        public List<CameraRunner> Cameras => _Cameras;

        public List<PrimitiveRunner> Primitives => _Primitives;

        public List<Runner> Runners => _runners;

        public uint Index
        {
            get;
            private set;
        }

        public List<Component> Models => _models;

        protected BaseElements(BaseObjectRunner parent, uint index)
        {
            _Parent = parent;
            Index = index;
        }

        protected void Init()
        {
            foreach (var runner in _runners)
            {
                runner.ParentElements = this;
            }
        }

        public void InitSerializedData()
        {
            _models.Clear();
            _models.AddRange(_Visuals.Select(runner => { return runner.ComponentHolder.GetComponent<Xml2PrefabVisualRunnerContainer>(); }));
            _models.AddRange(_Platforms.Select(runner => { return runner.ComponentHolder.GetComponent<Xml2PrefabPlatformContainer>(); }));
            _models.AddRange(_Triggers.Select(runner => { return runner.ComponentHolder.GetComponent<Xml2PrefabTriggerContainer>(); }));
            _models.AddRange(_Areas.Select(runner => { return runner.ComponentHolder.GetComponent<Xml2PrefabAreaContainer>(); }));
            _models.AddRange(_Animations.Select(runner => { return runner.ComponentHolder.GetComponent<Xml2PrefabAnimationContainer>(); }));
            _models.AddRange(_Particles.Select(runner => { return runner.ComponentHolder.GetComponent<Xml2PrefabParticleContainer>(); }));
            _models.AddRange(_Trapezoids.Select(runner => { return runner.ComponentHolder.GetComponent<Xml2PrefabTrapezoidContainer>(); }));
            _models.AddRange(_Items.Select(runner => { return runner.ComponentHolder.GetComponent<Xml2PrefabItemContainer>(); }));
            _models.AddRange(_Cameras.Select(runner => { return runner.ComponentHolder.GetComponent<Xml2PrefabCameraContainer>(); }));
            _models.AddRange(_Spawns.Select(runner => { return runner.ComponentHolder.GetComponent<Xml2PrefabSpawnContainer>(); }));
            _models.AddRange(_Primitives.Select(runner => { return runner.ComponentHolder.GetComponent<Xml2PrefabPrimitiveContainer>(); }));
        }

        public void CopyFrom(BaseElements element)
        {
            _Visuals.AddRange(element.Visuals);
            _Platforms.AddRange(element.Platforms);
            _Triggers.AddRange(element.Triggers);
            _Areas.AddRange(element.Areas);
            _Animations.AddRange(element.Animations);
            _Particles.AddRange(element.Particles);
            _Trapezoids.AddRange(element.Trapezoids);
            _Items.AddRange(element.Items);
            _Cameras.AddRange(element.Cameras);
            _Spawns.AddRange(element.Spawns);
            _Primitives.AddRange(element.Primitives);
            _runners.AddRange(element.Runners);
            element.Reset();
            Init();
        }

        private void Reset()
        {
            _Visuals.Clear();
            _Platforms.Clear();
            _Triggers.Clear();
            _Areas.Clear();
            _Animations.Clear();
            _Particles.Clear();
            _Trapezoids.Clear();
            _Items.Clear();
            _Cameras.Clear();
            _Spawns.Clear();
            _Primitives.Clear();
            _runners.Clear();
        }
    }
}
