using System.Collections.Generic;
using System.Xml;
using Nekki.Vector.Core.User;
using UnityEngine;

namespace Nekki.Vector.Core.Location
{
    public class SoundSourceRunner : Runner
    {
        public class IntervalVolume
        {
            public float DistMin;

            public float DistMax;

            public float VolumeNear;

            public float VolumeFar;

            public IntervalVolume(XmlNode intervalNode)
            {
                DistMin = XmlUtils.ParseFloat(intervalNode.Attributes["MinDistance"]);
                DistMax = XmlUtils.ParseFloat(intervalNode.Attributes["MaxDistance"]);
                VolumeNear = XmlUtils.ParseFloat(intervalNode.Attributes["NearVolume"]);
                VolumeFar = XmlUtils.ParseFloat(intervalNode.Attributes["FarVolume"]);
            }
        }

        private AudioSource _AudioSource;

        private List<IntervalVolume> _IntervalsVolume = new List<IntervalVolume>();

        private float _VolumeFactor;

        private string _SoundName;

        private bool _Looped;

        private int _StartSoundFrame;

        private int _EndSoundFrame;

        private AudioRolloffMode _RolloffMode;
        private bool _StopCallingRender;

        public SoundSourceRunner(float p_x, float p_y, XmlNode p_node)
            : base(p_x, p_y)
        {
            ParseSoundSource(p_node);
        }
        protected override void GenerateObject()
        {
            base.GenerateObject();
            _AudioSource = _UnityObject.AddComponent<AudioSource>();
            _AudioSource.enabled = true;
            _AudioSource.clip = ResourcesLoader.LoadAudioClip(_SoundName);
            _AudioSource.maxDistance = _IntervalsVolume[0].DistMax;
            _AudioSource.minDistance = _IntervalsVolume[0].DistMin;
            _AudioSource.rolloffMode = _RolloffMode;
            _AudioSource.loop = _Looped;
            _AudioSource.playOnAwake = false;
            _AudioSource.dopplerLevel = 0f;
            _AudioSource.spatialBlend = 1f;
            _AudioSource.mute = !UserDataManager.Instance.Options.Sound;
            _StartSoundFrame = 0;
            _EndSoundFrame = int.MaxValue;
            _StopCallingRender = true;
        }

        private void ParseSoundSource(XmlNode p_soundSourceNode)
        {
            base.Name = XmlUtils.ParseString(p_soundSourceNode.Attributes["Name"], string.Empty);
            _RolloffMode = ((XmlUtils.ParseString(p_soundSourceNode.Attributes["RolloffMode"], "Linear") == "Linear") ? AudioRolloffMode.Linear : AudioRolloffMode.Logarithmic);
            XmlNode xmlNode = p_soundSourceNode["Properties"]["Static"]["Sound"];
            _SoundName = XmlUtils.ParseString(xmlNode.Attributes["Name"], string.Empty);
            _Looped = XmlUtils.ParseBool(xmlNode.Attributes["Looped"]);
            if (xmlNode != null && xmlNode.HasChildNodes)
            {
                IntervalVolume item = new IntervalVolume(xmlNode.FirstChild);
                _IntervalsVolume.Add(item);
            }
        }

        private void StopPlaying(float time)
        {
            StopPlaying();
        }

        private void StopPlaying()
        {
            if (_AudioSource.enabled)
            {
                _AudioSource.enabled = false;
                _AudioSource.Stop();
            }
        }

        public override bool Render()
        {
            if (_AudioSource.timeSamples > _EndSoundFrame)
            {
                if (!_Looped)
                {
                    StopPlaying();
                    return true;
                }
                _AudioSource.Stop();
                _AudioSource.timeSamples = _StartSoundFrame;
                _AudioSource.Play();
            }
            return _StopCallingRender;
        }

        private void SoundBehaviourPause(bool p_pause)
        {
            _AudioSource.mute = LevelMainController.current.pauseRender || UserDataManager.Instance.Options.Sound;
        }

        private void SoundVolumeChanged(float p_volume)
        {
            _AudioSource.volume = p_volume * _VolumeFactor;
        }
    }
}
