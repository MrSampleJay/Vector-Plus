using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Nekki.Vector.Core.Animation.Events;
using Nekki.Vector.Core.Detector;

namespace Nekki.Vector.Core.Animation
{
	public class AnimationInterval
	{
		public int BeginFrame
		{
			get;
			private set;
		}

		public int EndFrame
		{
			get;
			private set;
		}

		public bool IsSafe
		{
			get;
			private set;
		}

		public bool IsLock
		{
			get;
			private set;
		}

		public bool IsAction
		{
			get;
			private set;
		}

		public List<AnimationEventDetector> DetectorHEvents
		{
			get;
			private set;
		}

		public List<AnimationEventDetector> DetectorVEvents
		{
			get;
			private set;
		}

		public List<AnimationEventEnd> EndEvents
		{
			get;
			private set;
		}

		public List<AnimationEventKey> KeyEvents
		{
			get;
			private set;
		}

		public List<AnimationEventCollision> CollisionEvents
		{
			get;
			private set;
		}

		public List<AnimationEventArea> AreaEvents
		{
			get;
			private set;
		}

		public List<AnimationEventFrame> FrameEvents
		{
			get;
			private set;
		}

		public Rectangle BoundingBoxLeft
		{
			get;
			private set;
		}

		public Rectangle BoundingBoxRight
		{
			get;
			private set;
		}

		public Point NoPlatformBound
		{
			get;
		}

		public bool ConditionlessBoundH
		{
			get;
			private set;
		}

		public bool ConditionlessBoundV
		{
			get;
			private set;
		}

		public bool ConditionlessBoundC
		{
			get;
			private set;
		}

		public bool ConditionlessPlatformBound
		{
			get;
			private set;
		}

		public AnimationInterval(XmlNode node)
		{
			DetectorHEvents = new List<AnimationEventDetector>();
			DetectorVEvents = new List<AnimationEventDetector>();
			EndEvents = new List<AnimationEventEnd>();
			KeyEvents = new List<AnimationEventKey>();
			CollisionEvents = new List<AnimationEventCollision>();
			AreaEvents = new List<AnimationEventArea>();
			FrameEvents = new List<AnimationEventFrame>();
			NoPlatformBound = new Point();
			PreParse(node);
			Parse(node);
		}

		public AnimationInterval(XmlNode node, List<XmlNode> groups) : this(node)
		{
			foreach (XmlNode group in groups)
			{
				Parse(group);
			}

		}

		private void PreParse(XmlNode node)
		{
            BeginFrame = node.Attributes["Start"].ParseInt();
            EndFrame = node.Attributes["End"].ParseInt();
            IsSafe = node.Attributes["Safe"].ParseBool();
            IsLock = node.Attributes["Lock"].ParseBool();
            IsAction = node.Attributes["Action"] != null ? true : false;
            NoPlatformBound.X = node.Attributes["NoPlatformBoundX"].ParseFloat();
            NoPlatformBound.Y = node.Attributes["NoPlatformBoundY"].ParseFloat();
			ConditionlessPlatformBound = node.Attributes["ConditionlessPlatformBound"].ParseBool();
            ConditionlessBoundH = node.Attributes["ConditionlessPlatformBoundH"].ParseBool();
            ConditionlessBoundV = node.Attributes["ConditionlessPlatformBoundV"].ParseBool();
            ConditionlessBoundC = node.Attributes["ConditionlessPlatformBoundC"].ParseBool();
            if (node.Attributes["LT"] != null && node.Attributes["RB"] != null)
            {
                string[] array = node.Attributes["LT"].Value.Split('|');
                string[] array2 = node.Attributes["RB"].Value.Split('|');
                float[] array3 = new float[2]
                {
                    float.Parse(array[0]),
                    float.Parse(array[1])
                };
                float[] array4 = new float[2]
                {
                    float.Parse(array2[0]),
                    float.Parse(array2[1])
                };
                BoundingBoxRight = new Rectangle(array3[0], array3[1], array4[0] - array3[0], array4[1] - array3[1]);
                BoundingBoxLeft = new Rectangle(-1f * array4[0], array3[1], array4[0] - array3[0], array4[1] - array3[1]);
            }
            DetectorHEvents = new List<AnimationEventDetector>();
            DetectorVEvents = new List<AnimationEventDetector>();
            EndEvents = new List<AnimationEventEnd>();
            KeyEvents = new List<AnimationEventKey>();
            CollisionEvents = new List<AnimationEventCollision>();
            FrameEvents = new List<AnimationEventFrame>();
            AreaEvents = new List<AnimationEventArea>();
        }

		private void Parse(XmlNode pNode)
		{
            foreach (XmlNode item in pNode)
            {
                string p_GroupNames = item.Attributes["Groups"].ParseString();
                switch (item.Name)
                {
                    case "Keys":
                        KeyEvents.Add(new AnimationEventKey(GetEventParam(item, p_GroupNames), item));
                        break;
                    case "OnEnd":
                        EndEvents.Add(new AnimationEventEnd(GetEventParam(item, p_GroupNames)));
                        break;
                    case "DetectorH":
                        {
                            DetectorEvent.DetectorEventType p_type2 = (DetectorEvent.DetectorEventType)item.Attributes["Type"].ParseInt();
                            DetectorHEvents.Add(new AnimationEventDetector(p_type2, GetEventParam(item, p_GroupNames)));
                            break;
                        }
                    case "DetectorV":
                        {
                            DetectorEvent.DetectorEventType p_type = (DetectorEvent.DetectorEventType)item.Attributes["Type"].ParseInt();
                            DetectorVEvents.Add(new AnimationEventDetector(p_type, GetEventParam(item, p_GroupNames)));
                            break;
                        }
                    case "OnCollision":
                        {
                            List<AnimationEventCollision.Type> list = new List<AnimationEventCollision.Type>();
                            if (item.Attributes["Types"] != null)
                            {
                                List<string> list2 = new List<string>(item.Attributes["Types"].Value.Split('|'));
                                foreach (string item2 in list2)
                                {
									if (!string.IsNullOrEmpty(item2))
										list.Add((AnimationEventCollision.Type)int.Parse(item2));
                                }
                            }
                            CollisionEvents.Add(new AnimationEventCollision(list, GetEventParam(item)));
                            break;
                        }
                    case "OnFrame":
						string[] array = item.Attributes["Frame"].Value.Split('|');
						int[] newarray = new int[array.Length];
						for (int ii = 0; ii < array.Length; ii++)
						{
							newarray[ii] = int.Parse(array[ii]);
                            FrameEvents.Add(new AnimationEventFrame(newarray[ii], GetEventParam(item)));
                        }
                        break;
                    case "OnTrigger":
					case "OnArea":
                        AreaEvents.Add(new AnimationEventArea(GetEventParam(item)));
                        break;
                }
            }
        }

		public AnimationEventParam GetEventParam(XmlNode node, string groupNames = null)
		{
            List<AnimationReaction> list = new List<AnimationReaction>();
            List<AnimationSound> list2 = new List<AnimationSound>();
            XmlNode xmlNode = node["Reactions"];
            if (xmlNode != null)
            {
                foreach (XmlNode item in xmlNode)
                {
                    list.Add(new AnimationReaction(item));
                }
            }
            if (groupNames != null)
            {
                string[] array = groupNames.Split('|');
                foreach (string name in array)
                {
                    AnimationGroup group = AnimationGroup.GetGroup(name);
                    list.AddRange(group.Reactions);
                }
            }
            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.Name == "Sound")
                {
                    if (childNode.Attributes["Group"] != null)
                    {
                        string soundgroupname = childNode.Attributes["Group"].Value;
                        AnimationSoundGroup group = AnimationSoundGroup.GetGroup(soundgroupname);
                        list2.AddRange(group.Sounds);
                    }
					else
					{
                        AnimationSound newSound = new AnimationSound(childNode.Attributes["Name"].Value, int.Parse(childNode.Attributes["Type"].Value), XmlUtils.ParseString(childNode.Attributes["Voice"], null));
                        list2.Add(newSound);
                    }
                }
            }
            if (list2.Count == 0)
            {
                list2 = null;
            }
            if (list.Count == 0)
            {
                list = null;
            }
            return new AnimationEventParam(list, list2);
        }

		public static string TypeToString(AnimationEventType pType)
		{
			return null;
		}
	}
}
