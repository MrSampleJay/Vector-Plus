using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Nekki.Vector.Core.Models;

namespace Nekki.Vector.Core.Animation
{
    public class SoundDictionary
    {
        public static Dictionary<(string original, string voice), string> VoiceSounds 
        { get; } = new();
        public static Dictionary<(string original, string material), string> MaterialSounds
        { get; } = new();
        public static void ParseSoundDictionaries(XmlNode node)
        {
            string originalName = node.Attributes["Original"].Value;

            foreach (XmlNode childNode in node.ChildNodes)
            {
                string voiceType = XmlUtils.ParseString(childNode.Attributes["Type"]);
                if (voiceType == null)
                {
                    continue;
                }
                string newName = childNode.Attributes["Sound"].Value;

                if (childNode.Name == "Voice")
                {
                    VoiceSounds[(originalName, childNode.Attributes["Type"].Value)] = newName;
                }
                else
                {
                    MaterialSounds[(originalName, childNode.Attributes["Type"].Value)] = newName;
                }
                
            }
        }

        public static string ReplaceString(string sound, string voice = null, string material = null)
        {
            if (VoiceSounds.TryGetValue((sound, voice), out string newvoiceSound))
            {
                return newvoiceSound;
            }
            if (MaterialSounds.TryGetValue((sound, material), out string newmatSound))
            {
                return newmatSound;
            }
            return sound;
        }
    }
}
