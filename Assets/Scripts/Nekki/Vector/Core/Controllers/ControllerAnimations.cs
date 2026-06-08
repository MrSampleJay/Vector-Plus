using System;
using System.Collections.Generic;
using System.Text;
using Nekki.Vector.Core.Animation;
using Nekki.Vector.Core.Animation.Events;
using Nekki.Vector.Core.Camera;
using Nekki.Vector.Core.Detector;
using Nekki.Vector.Core.Location;
using Nekki.Vector.Core.Models;
using Nekki.Vector.Core.Node;
using UnityEngine;
using AnimationInfo = Nekki.Vector.Core.Animation.AnimationInfo;
using Collision = Nekki.Vector.Core.Result.Collision;

namespace Nekki.Vector.Core.Controllers
{
    public class ControllerAnimations
    {
        private Model _Model;

        private ModelObject _ModelObject;

        private AnimationInfo _AnimationOld;

        private KeyFrames _Frames = new KeyFrames();

        private BufferFrames _Buffer = new BufferFrames();

        private int _FirstFrame;

        private int _EndFrame;

        private int _CurrentNode;

        private int _PointFrame;

        private int _AnimationFrame;

        private int _OldFrame;

        private Vector3dList _VelocityOld = new Vector3dList();

        private Vector3dList _TargetPointOld = new Vector3dList();

        private string _PivotNode;

        private bool _isRender;

        private bool _IsMirror;

        private bool _IsNewFrame;

        private List<string> _NodesNewPosition;

        private Vector3d _SpeedTween;

        private Vector3d _SpeedTweenOld = new Vector3d();

        private QuadRunner _VelocityQuad;

        private List<List<AnimationReaction>> _ForSort = new List<List<AnimationReaction>>();

        public AnimationInfo Animation
        {
            get;
            private set;
        }

        public int Sign
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public bool IsPlay
        {
            get;
            private set;
        }

        private List<AnimationInterval> Intervals => Animation.Interval(CurrentFrame);

        private bool IsBuffer => _Buffer != null && !_Buffer.IsBufferEmpty;

        public int CurrentFrame
        {
            get
            {
                if (_AnimationFrame < 3)
                {
                    return _FirstFrame;
                }
                return _FirstFrame + _AnimationFrame - 2;
            }
        }

        public bool SafeInterval
        {
            get
            {
                AnimationInterval animationInterval = null;
                for (int i = 0; i < Intervals.Count; i++)
                {
                    animationInterval = Intervals[i];
                    if (animationInterval.IsSafe)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool LockInterval
        {
            get
            {
                AnimationInterval animationInterval = null;
                for (int i = 0; i < Intervals.Count; i++)
                {
                    animationInterval = Intervals[i];
                    if (animationInterval.IsLock)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public float AutoPositionDetectorH => Animation.AutoPositionDetectorH;

        public float AutoPositionDetectorV => Animation.AutoPositionDetectorV;

        public float LandingPositionDetectorH => Animation.LandingPositionDetectorH;

        public float LandingPositionDetectorV => Animation.LandingPositionDetectorV;

        public Rectangle BoundingBox
        {
            get
            {
                if (Animation == null)
                {
                    return _ModelObject.Rectangle;
                }
                AnimationInterval animationInterval = null;
                for (int i = 0; i < Intervals.Count; i++)
                {
                    animationInterval = Intervals[i];
                    if (!(animationInterval.BoundingBoxLeft == null))
                    {
                        return Sign != 1 ? animationInterval.BoundingBoxLeft : animationInterval.BoundingBoxRight;
                    }
                }
                return _ModelObject.Rectangle;
            }
        }

        public bool IsActionInterval
        {
            get
            {
                AnimationInterval animationInterval = null;
                for (int i = 0; i < Intervals.Count; i++)
                {
                    animationInterval = Intervals[i];
                    if (animationInterval.IsAction)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private bool IsDelayReaction => _Model is ModelHuman && ((ModelHuman)_Model).DelayReaction != null;

        private Point PlatformBound
        {
            get
            {
                AnimationInterval animationInterval = null;
                for (int i = 0; i < Intervals.Count; i++)
                {
                    animationInterval = Intervals[i];
                    if (animationInterval.NoPlatformBound.X != 0f || animationInterval.NoPlatformBound.Y != 0f)
                    {
                        return animationInterval.NoPlatformBound;
                    }
                }
                return null;
            }
        }

        public ControllerAnimations(Model model)
        {
            Sign = 1;
            _Model = model;
            _ModelObject = model.ModelObject;
        }

        private void Init(AnimationInfo info, bool reverse, int firstFrame)
        {
            _Buffer.Reset();
            _Frames.Reset();
            Animation = info;
            Name = info.Name;
            _FirstFrame = firstFrame >= 0 ? firstFrame : info.FirstFrame;
            if (reverse)
            {
                Sign *= -1;
            }
            _Frames.InterruptFramesSeted();
            _EndFrame = info.EndFrame;
            Animation.CloneFrames(_FirstFrame, _EndFrame, _Frames, 0);
            SetInterruptFrames();
            _PivotNode = info.PivotNode;
            _CurrentNode = _ModelObject.GetNodeIdByName(_PivotNode);
            ShiftPoint();
            VelocityPoint();
            MirrorNode("NHeel_1", "NHeel_2");
            SetNodesNewPosition();
            if (_ModelObject.DetectorVerticalLine != null)
            {
                _ModelObject.DetectorVerticalLine.Delta(info.DeltaDetectorV);
            }
            if (_ModelObject.DetectorHorizontalLine != null)
            {
                _ModelObject.DetectorHorizontalLine.Delta(info.DeltaDetectorH);
            }
            _AnimationOld = Animation;
            IsPlay = true;
            _isRender = true;
            _IsNewFrame = false;
            _AnimationFrame = 0;
            _OldFrame = 0;
        }

        public void Render()
        {
            if (!IsPlay && !_isRender)
            {
                return;
            }
            CheckEvent();
            if (!IsBuffer && _Frames.SizeMinusActiveFrame == 2)
            {
                IsPlay = false;
                _isRender = false;
                return;
            }
            if (!IsBuffer)
            {
                SetBufferFrame();
            }
            if (IsBuffer)
            {
                DrawFrame();
            }

        }

        private void SetNodesNewPosition()
        {
            if (_NodesNewPosition == null)
            {
                return;
            }
            ModelNode modelNode = null;
            for (int i = 0; i < _NodesNewPosition.Count; i++)
            {
                modelNode = _ModelObject.GetNode(_NodesNewPosition[i]);
                if (modelNode != null)
                {
                    Vector3d p_vector = _Frames.GetFrame(2)[modelNode.Id];
                    _Frames.GetFrame(0)[modelNode.Id].Set(p_vector);
                    _Frames.GetFrame(1)[modelNode.Id].Set(p_vector);
                }
            }
            _NodesNewPosition = null;
        }

        public void Clear()
        {
            _AnimationOld = null;
            Animation = null;
            Sign = 1;
            _isRender = false;
            _VelocityQuad = null;
            _SpeedTween = null;
            _SpeedTweenOld.Reset();
            Stop();
        }

        public void Play(AnimationInfo p_info, bool p_reverse, int p_firstFrame)
        {
            if (p_info != null)
            {
                Init(p_info, p_reverse, p_firstFrame);
            }
        }

        private void SaveLog(string p_type, bool p_isSelected, int p_selectedFirstFrame, string p_selectedAnimName, string p_data)
        {
        }

        private void PlayReaction(AnimationReaction reaction, AnimationEventType type, string p_data = null)
        {
            if (reaction == null)
            {
                return;
            }
            ModelHuman modelHuman = (ModelHuman)_Model;
            modelHuman.VelocityQuads = new Vector3d();
            if (reaction.Name == "Death")
            {
                _Model.StartPhysics();
                LevelMainController.current.Death(modelHuman, 0);
                return;
            }
            if (reaction.IsTrick)
            {
                TrickAreaRunner.TrickGetByModel(modelHuman);
            }
            if (reaction.IsAnimationArrest)
            {
                modelHuman.Arrest();
            }
            if (reaction.OnEndTrigger && modelHuman != null)
            {
                modelHuman.DelayReaction = reaction;
                return;
            }
            if (reaction.CornerCling)
            {
                CorenerPoint();
            }
            if (reaction.CornerClingV >= 0)
            {
                CornerPoint(reaction.CornerClingV, true);
            }
            if (reaction.CornerClingH >= 0)
            {
                CornerPoint(reaction.CornerClingH, false);
            }
            if (reaction.NodesWi != null && reaction.NodesWi.Count > 0)
            {
                _NodesNewPosition = reaction.NodesWi;
            }
            _ModelObject.DetectorHorizontalLine.Safe = reaction.SafeHorizontal;
            _ModelObject.DetectorVerticalLine.Safe = reaction.SafeVertical;
            if (_Model.Type != ModelType.Human)
            {
                return;
            }
            if (type == AnimationEventType.Controller)
            {
                modelHuman.ControllerKeys.ClearAnimation();
            }
            modelHuman.PlayAnimation(reaction);
        }

        public void Stop()
        {
            IsPlay = false;
        }

        private void SetInterruptFrames()
        {
            List<Vector3d> frame = _Frames.GetFrame(0);
            List<Vector3d> frame2 = _Frames.GetFrame(1);
            ModelNode modelNode = null;
            double num = (_PointFrame + 1) * 0.5f;
            int count = Mathf.Min(frame.Count, _ModelObject.NodesAll.Count);
            for (int i = 0; i < count; i++)
            {
                modelNode = _ModelObject.NodesAll[i];
                double num2 = (modelNode.Start.X - modelNode.End.X) * num;
                double num3 = (modelNode.Start.Y - modelNode.End.Y) * num;
                double num4 = (modelNode.Start.Z - modelNode.End.Z) * num;
                frame[i].Set(modelNode.Start.X - num2, modelNode.Start.Y - num3, modelNode.Start.Z - num4);
                frame2[i].Set(modelNode.Start.X + num2, modelNode.Start.Y + num3, modelNode.Start.Z + num4);
            }
        }

        private void SetBufferFrame()
        {
            _PointFrame = (int)(Math.Max(Animation.MidFrames, 1) * LevelMainController.current.slowModeFrames);
            _Buffer.InitBuffer(_PointFrame + 1);
            List<Vector3d> activeFrame = _Frames.GetActiveFrame(0);
            List<Vector3d> activeFrame2 = _Frames.GetActiveFrame(1);
            List<Vector3d> activeFrame3 = _Frames.GetActiveFrame(2);
            _Buffer.ResizeIfNeed(activeFrame3.Count);
            Vector3d vector3f = new Vector3d(0f, 0f, 0f);
            Vector3d vector3f2 = new Vector3d(0f, 0f, 0f);
            for (int i = 0; i < activeFrame3.Count; i++)
            {
                if (activeFrame[i] != null && activeFrame2[i] != null && activeFrame3[i] != null)
                {
                    Vector3d p_point = activeFrame[i];
                    Vector3d vector3f3 = activeFrame2[i];
                    Vector3d p_point2 = activeFrame3[i];
                    Vector3d.Middle(p_point, vector3f3, vector3f);
                    Vector3d.Middle(vector3f3, p_point2, vector3f2);
                    for (int j = 0; j <= _PointFrame; j++)
                    {
                        double num = (j + 1f) / (_PointFrame + 1f);
                        double num2 = (1f - num) * (1f - num);
                        double num3 = num * num;
                        double num4 = 2f * num * (1f - num);
                        double p_x = vector3f.X * num2 + vector3f3.X * num4 + vector3f2.X * num3;
                        double p_y = vector3f.Y * num2 + vector3f3.Y * num4 + vector3f2.Y * num3;
                        double p_z = vector3f.Z * num2 + vector3f3.Z * num4 + vector3f2.Z * num3;
                        _Buffer.GetFrame(j)[i].Set(p_x, p_y, p_z);
                    }
                }
            }
            _Frames.NextActiveFrame();
        }

        public void DrawFrame()
        {
            List<Vector3d> activeFrame = _Buffer.GetActiveFrame(0);
            _Buffer.NextActiveFrame();
            for (int i = 0; i < activeFrame.Count; i++)
            {
                ModelNode node = _ModelObject.GetNode(i);
                if (node != null)
                {
                    node.EndAssignStart();
                    Vector3d vector3f = activeFrame[i];
                    node.PositionStart(vector3f.X, vector3f.Y, vector3f.Z);
                }
            }
            if (_Buffer.IsBufferEmpty)
            {
                _AnimationFrame++;
                if (CurrentFrame != _OldFrame)
                {
                    _OldFrame = CurrentFrame;
                    _IsNewFrame = true;
                }
            }
        }

        public void PlaySounds(List<AnimationSound> sounds)
        {
            if (sounds != null)
            {
                float zoom = LocationCamera.Current.Zoom;
                Vector3d start = LocationCamera.Current.Node.Start;
                Vector3d start2 = _Model.GetNode("COM").Start;
                double val = 1.5f - Math.Abs(start2.X - start.X) / 760f * zoom;
                double val2 = 1.5f - Math.Abs(start2.Y - start.Y) / 760f * zoom;
                double val3 = Math.Min(1f, Math.Min(val, val2));
                double num = Math.Max(0f, val3);
                if (_Model is ModelHuman && (_Model as ModelHuman).IsBot)
                {
                    num = !(num < 0.5f) ? num / 1.1f : num * 1.5f;
                }
                for (int i = 0; i < sounds.Count; i++)
                {
                    sounds[i].Play((float)num);
                }
            }
        }

        public void VelocityQuads()
        {
            if (_SpeedTween == null)
            {
                return;
            }
            Point platformBound = PlatformBound;
            var speed = new Vector3d(_SpeedTween.X, _SpeedTween.Y);
            if (platformBound == null)
            {
                _SpeedTweenOld.Set(_SpeedTween);
            }
            else
            {
                if (platformBound.X > 0f)
                {
                    speed.X = _SpeedTweenOld.X;
                }
                if (platformBound.Y > 0f)
                {
                    speed.Y = _SpeedTweenOld.Y;
                }
            }
            if (speed.X != 0f || speed.Y != 0f)
            {
                VelocityVector(speed, _TargetPointOld);
                VelocityFrames(speed);
                VelocityBuffer(speed);
                VelocityNodes(speed);
                ((ModelHuman)_Model).VelocityQuads = speed;
            }
        }

        public void SetVelocityQuads(DetectorEvent p_event, bool isSort, QuadRunner platform = null)
        {
            if ((isSort || IsConditionlessPlatformBound(p_event)) && (p_event == null || p_event.Type == DetectorEvent.DetectorEventType.On))
            {
                if (platform == null)
                {
                    platform = p_event.Platform;
                }
                _VelocityQuad = platform;
                if (platform.Sticky)
                {
                    _SpeedTween = platform.TweenPosition;
                }

            }
        }

        public void VelocityVector(Vector3d point, Vector3dList array)
        {
            for (int i = 0; i < array.size; i++)
            {
                array[i].Add(point);
            }
        }

        public void VelocityFrames(Vector3d point)
        {
            List<Vector3d> list = null;
            for (int i = 0; i < _Frames.Size; i++)
            {
                list = _Frames.GetFrame(i);
                for (int j = 0; j < list.Count; j++)
                {
                    list[j].Add(point);
                }
            }
        }

        public void VelocityBuffer(Vector3d point)
        {
            _Buffer.VelocityBuffer(point);
        }

        public void VelocityNodes(Vector3d point)
        {
            ModelNode modelNode = null;
            for (int i = 0; i < _ModelObject.NodesAll.Count; i++)
            {
                modelNode = _ModelObject.NodesAll[i];
                modelNode.Start.Add(point);
                modelNode.End.Add(point);
            }
        }

        public void VelocityPoint()
        {
            if (Animation.Binding)
            {
                int index = Math.Max(0, _AnimationFrame - 1);
                Vector3d vector3d = new Vector3d(0f, 0f, 0f);
                Vector3d vector3d2 = new Vector3d(0f, 0f, 0f);
                if (IsOldType(2))
                {
                    vector3d.Add(_TargetPointOld[index]);
                    ReverseVelocity();
                    vector3d2.Add(_VelocityOld[index]);
                }
                else
                {
                    vector3d.Add(_Frames.GetFrame(2)[_CurrentNode]);
                    vector3d2.Add(Animation.Velocity);
                }
                vector3d2.X *= Sign;
                _VelocityOld.Clear();
                _TargetPointOld.Clear();
                List<Vector3d> list = null;
                for (int i = 2; i < _Frames.Size; i++)
                {
                    list = _Frames.GetFrame(i);
                    vector3d.Add(vector3d2);
                    Position(vector3d, list);
                    vector3d2.Add(Animation.Gravity);
                    _TargetPointOld.Add(new Vector3d(vector3d));
                    _VelocityOld.Add(new Vector3d(vector3d2));
                }
            }
        }

        public bool IsOldType(int type)
        {
            if (_AnimationOld == null)
            {
                return false;
            }
            return Animation.Type == type && Animation.Type == _AnimationOld.Type;
        }

        public void ReverseVelocity()
        {
            if (Sign == 1)
            {
                return;
            }
            for (int i = 0; i < _VelocityOld.size; i++)
            {
                _VelocityOld[i].X *= Sign;
            }
        }

        private void CheckEvent()
        {
            CheckController();
            if (_IsNewFrame)
            {
                CheckFrame();
                CheckEndFrame();
                _IsNewFrame = false;
            }
        }

        private void CheckController()
        {
            if (!_Model.IsEnabled || IsDelayReaction)
            {
                return;
            }
            ModelHuman modelHuman = _Model as ModelHuman;
            if (modelHuman == null)
            {
                return;
            }
            KeyVariables keyVariables = modelHuman.ControllerKeys.KeyVariables;
            if (keyVariables == null)
            {
                return;
            }
            _ForSort.Clear();
            AnimationInterval animationInterval = null;
            AnimationEventKey animationEventKey = null;
            for (int i = 0; i < Intervals.Count; i++)
            {
                animationInterval = Intervals[i];
                for (int j = 0; j < animationInterval.KeyEvents.Count; j++)
                {
                    animationEventKey = animationInterval.KeyEvents[j];
                    List<AnimationReaction> validateReactions = modelHuman.ControllerKeys.GetValidateReactions(keyVariables, animationEventKey, Sign);
                    _ForSort.Add(validateReactions);
                }
            }
            AnimationReaction p_reaction = Sort(_ForSort);
            PlayReaction(p_reaction, AnimationEventType.Controller, string.Empty);
        }

        public bool CheckCollision(Collision collisionResult, AnimationEventCollision.Type type)
        {
            _ForSort.Clear();
            AnimationInterval animationInterval = null;
            AnimationEventCollision animationEventCollision = null;
            for (int i = 0; i < Intervals.Count; i++)
            {
                animationInterval = Intervals[i];
                for (int j = 0; j < animationInterval.CollisionEvents.Count; j++)
                {
                    animationEventCollision = animationInterval.CollisionEvents[j];
                    if (animationEventCollision.Reaction != null && (animationEventCollision.Types.Count == 0 || animationEventCollision.Types.Contains(type)))
                    {
                        _ForSort.Add(animationEventCollision.Reaction);
                    }
                }
            }
            AnimationDeltaData p_delta = type != AnimationEventCollision.Type.Quad ? null : new AnimationDeltaData(collisionResult.Platform, new Vector3d(collisionResult.Point), Sign);
            AnimationReaction animationReaction = Sort(_ForSort, p_delta, collisionResult.Platform);
            if (type == AnimationEventCollision.Type.Quad)
                SetVelocityQuads(null, animationReaction != null, collisionResult.Platform);
            string empty = string.Empty;
            PlayReaction(animationReaction, AnimationEventType.OnCollision, empty);
            return animationReaction != null;
        }

        public void CheckEndFrame()
        {
            if (!_Model.IsEnabled)
            {
                return;
            }
            _ForSort.Clear();
            AnimationInterval animationInterval = null;
            for (int i = 0; i < Intervals.Count; i++)
            {
                animationInterval = Intervals[i];
                if (CurrentFrame >= animationInterval.EndFrame)
                {
                    for (int j = 0; j < animationInterval.EndEvents.Count; j++)
                    {
                        _ForSort.Add(animationInterval.EndEvents[j].Reaction);
                    }
                }
            }
            if (_ForSort.Count > 0)
            {
                AnimationReaction p_reaction = Sort(_ForSort);
                PlayReaction(p_reaction, AnimationEventType.OnEnd, string.Empty);
            }
        }

        public AnimationReaction Sort(List<List<AnimationReaction>> reactions, AnimationDeltaData delta = null, QuadRunner platform = null)
        {
            if (reactions.Count <= 0 || _Model.Type != ModelType.Human)
            {
                return null;
            }
            return ((ModelHuman)_Model).SortAnimation(reactions, delta, platform);
        }

        public void CheckFrame()
        {
            if (IsDelayReaction)
            {
                return;
            }
            _ForSort.Clear();
            AnimationInterval animationInterval = null;
            AnimationEventFrame animationEventFrame = null;
            for (int i = 0; i < Intervals.Count; i++)
            {
                animationInterval = Intervals[i];
                for (int j = 0; j < animationInterval.FrameEvents.Count; j++)
                {
                    animationEventFrame = animationInterval.FrameEvents[j];
                    if (animationEventFrame.Frame == CurrentFrame)
                    {
                        _ForSort.Add(animationEventFrame.Reaction);
                        PlaySounds(animationEventFrame.Sound);
                    }
                }
            }
            AnimationReaction p_reaction = Sort(_ForSort);
            PlayReaction(p_reaction, AnimationEventType.OnFrame, string.Empty);
        }

        public void Position(Vector3d shift, List<Vector3d> array)
        {
            Vector3d p_vector = shift - array[_CurrentNode];
            for (int i = 0; i < array.Count; i++)
            {
                array[i].Add(p_vector);
            }
        }

        public void CheckDetectors(DetectorEvent p_event)
        {
            if (!_Model.IsEnabled || IsDelayReaction)
            {
                return;
            }
            _ForSort.Clear();
            AnimationInterval animationInterval = null;
            AnimationEventDetector animationEventDetector = null;
            AnimationReaction animationReaction = null;
            for (int i = 0; i < Intervals.Count; i++)
            {
                animationInterval = Intervals[i];
                List<AnimationEventDetector> list = !p_event.IsVertical ? animationInterval.DetectorHEvents : animationInterval.DetectorVEvents;
                for (int j = 0; j < list.Count; j++)
                {
                    animationEventDetector = list[j];
                    if (animationEventDetector.Type != 0 && animationEventDetector.Type != p_event.Type)
                    {
                        continue;
                    }
                    for (int k = 0; k < animationEventDetector.Reaction.Count; k++)
                    {
                        animationReaction = animationEventDetector.Reaction[k];
                        var list1 = new List<AnimationReaction>();
                        if (animationReaction.IsSide(p_event.Side, Sign))
                        {
                            list1.Add(animationReaction);
                        }
                        _ForSort.Add(list1);

                    }
                }
            }
            AnimationReaction animationReaction2 = Sort(_ForSort, null, p_event.Platform);
            _ForSort.Clear();
            if (animationReaction2 == null)
            {
                SetVelocityQuads(p_event, false);
                return;
            }
            ModelHuman modelHuman = (ModelHuman)_Model;
            Vector3d vector3f = _ModelObject.Velocity + modelHuman._VelocityQuadCurrent;
            Vector3d vector3f2 = new Vector3d(0f, 0f, 0f);
            if (LandingPositionDetectorH >= 0f && p_event.IsHorizontal)
            {
                vector3f2.X = LandingPositionDetectorH * Sign - 4f * vector3f.X;
            }
            if (LandingPositionDetectorV >= 0f && p_event.IsVertical)
            {
                vector3f2.Y = LandingPositionDetectorV - 4f * vector3f.Y;
            }
            p_event.DeltaPosition(vector3f2);

            SetVelocityQuads(p_event, true);
            AnimationEventType p_type;
            switch (p_event.Type)
            {
                case DetectorEvent.DetectorEventType.On:
                    p_type = AnimationEventType.DetectorOn;
                    break;
                case DetectorEvent.DetectorEventType.Off:
                    p_type = AnimationEventType.DetectorOff;
                    break;
                default:
                    p_type = AnimationEventType.Controller;
                    break;
            }
            string empty = string.Empty;
            PlayReaction(animationReaction2, p_type, empty);
        }

        public void CheckArea(QuadRunner runner)
        {
            if (!_isRender)
            {
                return;
            }
            _ForSort.Clear();
            foreach (var interval in Intervals)
            {
                foreach (var areaEvent in interval.AreaEvents)
                {
                    _ForSort.Add(areaEvent.Reaction);
                }
            }
            PlayReaction(Sort(_ForSort), AnimationEventType.OnArea);
        }

        public void CorenerPoint()
        {
            DetectorLine detectorLine = _ModelObject.DetectorVerticalLine;
            QuadRunner data = detectorLine.Node.Data;
            if (data != null)
            {
                Vector3d vector3f = new Vector3d(detectorLine.Position);
                vector3f.Z = 0f;
                Vector3d vector3f3 = data.Corner(Sign, 0);
                if (vector3f3 != null)
                {
                    detectorLine.Node.Start.Add(vector3f3 - vector3f);
                    detectorLine.Node.EndAssignStart();
                }
            }
        }

        public void CornerPoint(int p_cornernum, bool p_isDetectorV)
        {
            DetectorLine detectorLine = !p_isDetectorV ? _ModelObject.DetectorHorizontalLine : _ModelObject.DetectorVerticalLine;
            QuadRunner data = detectorLine.Node.Data;
            if (data != null)
            {
                Vector3d vector3f = new Vector3d(detectorLine.Position);
                vector3f.Z = 0f;
                Vector3d vector3f2 = vector3f;
                Vector3d vector3f3 = data.Corner(Sign, p_cornernum);
                if (vector3f3 != null)
                {
                    detectorLine.Node.Start.Add(vector3f3 - vector3f2);
                    detectorLine.Node.EndAssignStart();
                }
            }
        }

        public bool IsConditionlessPlatformBound(DetectorEvent dEvent)
        {
            AnimationInterval animationInterval = null;
            for (int i = 0; i < Intervals.Count; i++)
            {
                animationInterval = Intervals[i];
                if ((dEvent == null && animationInterval.ConditionlessBoundC) || (dEvent != null && dEvent.IsVertical && animationInterval.ConditionlessBoundV) || (dEvent != null && dEvent.IsHorizontal && animationInterval.ConditionlessBoundH))
                {
                    return true;
                }
            }
            return false;
        }

        public Vector3d Shift(List<Vector3d> p_frameNodes)
        {
            return p_frameNodes[_CurrentNode] - _Frames.GetFrame(2)[_CurrentNode];
        }

        public void ShiftPoint()
        {
            ReverseFrames();
            Vector3d p_vector = Shift(_Frames.GetFrame(1));
            List<Vector3d> list = null;
            for (int i = 2; i < _Frames.Size; i++)
            {
                list = _Frames.GetFrame(i);
                ShiftNodesPoint(list, p_vector);
            }
        }

        public void ReverseFrames()
        {
            if (Sign == 1)
            {
                return;
            }
            List<Vector3d> list = null;
            for (int i = 2; i < _Frames.Size; i++)
            {
                list = _Frames.GetFrame(i);
                for (int j = 0; j < list.Count; j++)
                {
                    list[j].X *= Sign;
                }
            }
        }

        public void ShiftNodesPoint(List<Vector3d> array, Vector3d value)
        {
            for (int i = 0; i < array.Count; i++)
            {
                array[i].Add(value);
            }
        }

        public void MirrorNode(string name1, string name2)
        {
            if (Sign == -1)
            {
                string text = name1;
                name1 = name2;
                name2 = text;
            }
            ModelNode node = _ModelObject.GetNode(name1);
            ModelNode node2 = _ModelObject.GetNode(name2);
            if (node == null || node2 == null)
            {
                return;
            }
            Vector3d start = node.Start;
            Vector3d start2 = node2.Start;
            List<Vector3d> frame = _Frames.GetFrame(2);
            Vector3d vector3f = frame[node.Id];
            Vector3d vector3f2 = frame[node2.Id];
            if (Animation.Mirror)
            {
                _IsMirror = start.X >= start2.X != vector3f.X >= vector3f2.X;
            }
            if (!_IsMirror)
            {
                return;
            }
            _CurrentNode = _ModelObject.GetNodeIdByName(BothNodeName(Animation.PivotNode));
            List<int[]> bothNodeList = _ModelObject.BothNodeList;
            List<Vector3d> list = null;
            for (int i = 2; i < _Frames.Size; i++)
            {
                list = _Frames.GetFrame(i);
                for (int j = 0; j < bothNodeList.Count; j++)
                {
                    int[] array = bothNodeList[j];
                    Vector3d value = list[array[0]];
                    list[array[0]] = list[array[1]];
                    list[array[1]] = value;
                }
            }
        }

        public string BothNodeName(string name)
        {
            StringBuilder stringBuilder = new StringBuilder(name);
            switch (stringBuilder[name.Length - 1])
            {
                case '1':
                    stringBuilder[name.Length - 1] = '2';
                    break;
                case '2':
                    stringBuilder[name.Length - 1] = '1';
                    break;
            }
            return stringBuilder.ToString();
        }
    }
}
