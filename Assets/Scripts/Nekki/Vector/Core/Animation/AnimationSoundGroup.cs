using System.Collections.Generic;

namespace Nekki.Vector.Core.Animation
{
	public class AnimationSoundGroup
	{
		public static readonly Dictionary<string, AnimationSoundGroup> Groups = new Dictionary<string, AnimationSoundGroup>();

		public string Name
		{
			get;
			set;
		}

		public List<AnimationSound> Sounds
		{
			get;
			set;
		}

		public static void Add(AnimationSoundGroup group)
		{
			if (group != null)
			{
				Groups[group.Name] = group;
			}
		}

		public static AnimationSoundGroup GetGroup(string name)
		{
			Groups.TryGetValue(name, out AnimationSoundGroup group);
			return group;
		}

		public static List<AnimationSound> GetSounds(string name)
		{
			return GetGroup(name).Sounds;
		}

		public static void ClearGroups()
		{
			Groups.Clear();
		}
	}
}
