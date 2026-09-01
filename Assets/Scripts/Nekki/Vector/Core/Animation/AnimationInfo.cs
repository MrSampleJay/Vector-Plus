using Core._Common;
using Nekki.Vector.Core.Frame;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using UnityEditor;

namespace Nekki.Vector.Core.Animation
{
    public class AnimationInfo
    {
        public const int Jumps = 1;

        public const int Slides = 2;

        private int _Id;

        public List<AnimationInterval> Intervals = new List<AnimationInterval>();

        private readonly int _endFrame;

        private readonly bool _isPart;

        private Provider _frames;

        private bool _Loop;

        private static readonly List<AnimationInterval> TempIntervalList = new List<AnimationInterval>();

        public int Id
        {
            get => _Id;
            set => _Id = value;
        }

        public string Name
        {
            get;
        }

        public bool IsTrick
        {
            get;
            protected set;
        }

        public bool Mirror
        {
            get;
        }

        public int Type
        {
            get;
        }

        public int SubType
        {
            get;
            private set;
        }

        public string FileName
        {
            get;
            private set;
        }

        public string Folder;

        public int FirstFrame
        {
            get;
        }

        public int EndFrame
        {
            get
            {
                if (_endFrame < _frames.Length)
                {
                    return _endFrame;
                }
                return _frames.Length;
            }
        }

        public int MidFrames
        {
            get;
        }

        public string PivotNode
        {
            get;
            set;
        }

        public int Priority
        {
            get;
        }

        public Vector3f Velocity
        {
            get;
        }

        public Vector3f Gravity
        {
            get;
        }

        public bool Binding
        {
            get;
        }

        public float DeltaDetectorV
        {
            get;
        }

        public float DeltaDetectorH
        {
            get;
        }

        public float AutoPositionDetectorV
        {
            get;
        }

        public float AutoPositionDetectorH
        {
            get;
        }

        public float LandingPositionDetectorH
        {
            get;
        }

        public float LandingPositionDetectorV
        {
            get;
        }

        public int PlatformAnticipationFrames
        {
            get;
        }

        public bool Loop
        {
            get => _Loop;
            set => _Loop = value;
        }

        public AnimationInfo(XmlNode node, string Path = null)
        {
            Velocity = new Vector3f(0, 0, 0);
            Gravity = new Vector3f(0, 0, 0);

            _isPart = XmlUtils.ParseBool(node.Attributes["Part"]);
            Name = node.Name;
            _Id = XmlUtils.ParseInt(node.Attributes["ID"]);
            FileName = XmlUtils.ParseString(node.Attributes["FileName"]);
            Folder = XmlUtils.ParseString(node.Attributes["Folder"], "Data");
            FirstFrame = XmlUtils.ParseInt(node.Attributes["FirstFrame"]);
            MidFrames = XmlUtils.ParseInt(node.Attributes["MidFrames"], 2);
            _endFrame = XmlUtils.ParseInt(node.Attributes["EndFrame"]);
            PivotNode = node.Attributes["PivotNode"].Value;
            Type = XmlUtils.ParseInt(node.Attributes["Type"], 1);
            SubType = XmlUtils.ParseInt(node.Attributes["SubType"]);
            Priority = XmlUtils.ParseInt(node.Attributes["Priority"], 1);
            Binding = XmlUtils.ParseBool(node.Attributes["Binding"]);
            DeltaDetectorH = XmlUtils.ParseFloat(node.Attributes["DeltaDetectorH"]);
            DeltaDetectorV = XmlUtils.ParseFloat(node.Attributes["DeltaDetectorV"]);
            Mirror = XmlUtils.ParseBool(node.Attributes["Mirror"], true);
            var x = XmlUtils.ParseFloat(node.Attributes["VelocityX"]);
            var y = XmlUtils.ParseFloat(node.Attributes["VelocityY"]);
            Velocity = new Vector3f(x, y);
            Gravity = new Vector3f(0, XmlUtils.ParseFloat(node.Attributes["Gravity"]));
            AutoPositionDetectorH = XmlUtils.ParseFloat(node.Attributes["AutoPositionDetectorH"], -1);
            AutoPositionDetectorV = XmlUtils.ParseFloat(node.Attributes["AutoPositionDetectorV"], -1);
            LandingPositionDetectorH = XmlUtils.ParseFloat(node.Attributes["LandingPositionDetectorH"], -1);
            LandingPositionDetectorV = XmlUtils.ParseFloat(node.Attributes["LandingPositionDetectorV"], -1);
            PlatformAnticipationFrames = (int)XmlUtils.ParseFloat(node.Attributes["PlatformAnticipationFrames"]);
        }

        public void LoadBin()
        {
            FileName = VectorPaths.Animations + "/" + Folder + "/" + FileName;
            if (!_isPart && !IsTrick)
            {
                LoadBinary(true);
            }
        }

        public virtual void LoadBinary(bool useCache)
        {
            _frames = AnimationBinaryParser.ParseFile(FileName, useCache);
        }

        public virtual void UnloadBinary()
        {
            _frames = null;
        }

        public List<AnimationInterval> Interval(int pFrame)
        {
            TempIntervalList.Clear();
            for (int i = 0; i < Intervals.Count; i++)
            {
                var interval = Intervals[i];
                if (pFrame >= interval.BeginFrame && pFrame <= interval.EndFrame)
                {
                    TempIntervalList.Add(interval);
                }
            }
            return TempIntervalList;
        }

        public void CloneFrames(int pStart, int end, KeyFrames frames, int nodesCount)
        {
			int p_to = ((end <= 0 || end > _frames.Length - 1) ? (_frames.Length - 1) : end);
			frames.SetFrames(pStart, p_to, _frames.Data);
        }
    }
}
