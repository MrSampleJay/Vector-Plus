using System.Collections.Generic;

namespace Nekki.Vector.Core.Animation
{
	public class AnimationGroup
	{
		public static readonly Dictionary<string, AnimationGroup> Groups = new Dictionary<string, AnimationGroup>();

		public string Name
		{
			get;
			set;
		}

		public List<AnimationReaction> Reactions
		{
			get;
			set;
		}

		public static void Add(AnimationGroup group)
		{
			if (group != null)
			{
				Groups[group.Name] = group;
			}
		}

		public static AnimationGroup GetGroup(string name)
		{
			Groups.TryGetValue(name, out AnimationGroup group);
			return group;
		}

		public static List<AnimationReaction> GetReactions(string name)
		{
			return GetGroup(name).Reactions;
		}
		public static void AddReactions(string name, List<AnimationReaction> additionalReactions)
		{
			Groups[name].Reactions.AddRange(additionalReactions);
		}
		public static void ClearGroups()
		{
			Groups.Clear();
		}
	}
}
