using Nekki.Vector.Core.Models;
using System.Collections.Generic;
using System.Xml;
using Nekki.Vector.Core.Result;
using Nekki.Vector.Core.Location;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Nekki.Vector.Core.Animation
{
	public class AnimationSound
	{
		private List<string> _Names;
        public List<string> Names
        {
            get => _Names;
            set => Names = value;
        }

        private string _Voice;

        private string _Material;

		public float _pitch;

        public AnimationSound(XmlNode name)
        {
            _Names = new List<string>(name.Attributes["Name"].Value.Split('|'));
			_Voice = name.Attributes["Voice"].ParseString();
            _Material = name.Attributes["Material"].ParseString();
        }
        public AnimationSound(string name, int type, string p_voice = null)
		{
			_Names = new List<string>(name.Split('|'));
			_Voice = p_voice;
		}

        public void Play(ModelHuman p_model, float p_volume = 1f)
        {
            var num = UnityEngine.Random.Range(0, _Names.Count);
            string newName = _Names[num];

            List<Belong> belongList = p_model.ControllerCollisions.ControllerPlatforms.Belongs;
            QuadRunner platform = null;

            if (string.IsNullOrEmpty(_Voice))
            {
                newName = SoundDictionary.ReplaceString(_Names[num], p_model.Voice);
            }
            else if (_Voice != p_model.Voice)
                return;

            if (belongList.Count > 0)
                platform = belongList[belongList.Count - 1].Platform;

            if (string.IsNullOrEmpty(_Material))
            {
                // Detector Filtering is Possible... But is it useful?

                if (platform != null && platform is PlatformRunner || platform is TrapezoidRunner)
                    newName = SoundDictionary.ReplaceString(newName, null, platform.Material);
            }
            else if(platform != null && _Material != platform.Material)
                return;

            SoundsManager.Instance.PlaySoundsOnce(newName, p_volume);
		}

        public void Play(float p_volume = 1f)
        {
            var num = UnityEngine.Random.Range(0, _Names.Count);

            SoundsManager.Instance.PlaySoundsOnce(_Names[num], p_volume);
        }
    }
}
