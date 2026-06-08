using UnityEngine;

namespace Core._Common
{
    public class VectorPaths
    {
        public static string CurrentResources = Application.streamingAssetsPath;

        public static string XmlRoot = CurrentResources + "/XmlRoot";

        public static string XmlLevels = XmlRoot + "/Levels";

        public static string Models = CurrentResources + "/Models";

        public static string Animations = CurrentResources + "/Animations";

        public static string AnimationBinary = Animations + "/Data";

        public static string LevelsPrefab = "LevelContent/Prefabs/Levels";

        public static string Localization = CurrentResources + "/localization";

        public static string Commons = CurrentResources + "/commons";

        public static string Music = CurrentResources + "/music";

        public static string Sounds = CurrentResources + "/sounds";

        public static string Textures = CurrentResources + "/textures";

        public static string AnimatedTextures = CurrentResources + "/animatedtextures";

        public static string Icons = CurrentResources + "/icons";
    }
}
