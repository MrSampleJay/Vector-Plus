using System.Collections.Generic;
using System.Xml;

namespace Nekki.Vector.Core.Animation
{
	public class AnimationSound
	{
		private List<string> _names;
        public List<string> Names
        {
            get => _names;
            set => Names = value;
        }

        public string _voice;

		public float _pitch;

        public AnimationSound(XmlNode name)
        {
            _names = new List<string>(name.Attributes["Name"].Value.Split('|'));
			_voice = XmlUtils.ParseString(name.Attributes["Voice"]);
        }
        public AnimationSound(string name, int type, string p_voice = null)
		{
			_names = new List<string>(name.Split('|'));
			_voice = p_voice;
		}

        public void Play(float p_volume = 1f, string voice = null, string material = null)
		{
			var num = UnityEngine.Random.Range(0, _names.Count);
            string newName = _names[num];

            // Catch the if the Sound matches the Model's Voice First
            if (_voice != null)
            {
                if (_voice != voice)
                    return;
            }

            // Check the dictionary
            if (voice != null || material != null)
                newName = SoundDictionary.ReplaceString(_names[num], voice, material);

            SoundsManager.Instance.PlaySoundsOnce(newName, p_volume);
		}
    }
}
