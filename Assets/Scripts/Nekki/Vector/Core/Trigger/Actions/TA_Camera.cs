using Nekki.Vector.Core.Camera;
using Nekki.Vector.Core.Models;
using System.Collections.Generic;
using System;
using System.Runtime.CompilerServices;
using System.Xml;
using UnityEngine;

namespace Nekki.Vector.Core.Trigger.Actions
{
	public class TA_Camera : TriggerAction
	{
		private Variable _FollowVar;

		private Variable _ZoomVar;

		private Variable _SmoothnessVar;

		private Variable _EffectVar;

		private Variable _FramesVar;

        private Variable _ZoomFramesVar;

		private Variable _IsStopVar;

        private Variable _StopPosX;

        private Variable _StopPosY;

        private TA_Camera(TA_Camera p_copyAction)
			: base(p_copyAction._ParentLoop)
		{
            _FollowVar = p_copyAction._FollowVar;
            _ZoomVar = p_copyAction._ZoomVar;
            _SmoothnessVar = p_copyAction._SmoothnessVar;
            _FramesVar = p_copyAction._FramesVar;
            _IsStopVar = p_copyAction._IsStopVar;
        }

		public TA_Camera(XmlNode p_node, TriggerLoop p_parent)
			: base(p_parent)
		{
            var attributeMap = new Dictionary<string, Action<string>>
            {
                ["Zoom"] = v => InitActionVar(p_parent.ParentTrigger, ref _ZoomVar, v),
                ["Smoothness"] = v => InitActionVar(p_parent.ParentTrigger, ref _SmoothnessVar, v),
                ["Frames"] = v => InitActionVar(p_parent.ParentTrigger, ref _FramesVar, v),
                ["Follow"] = v => InitActionVar(p_parent.ParentTrigger, ref _FollowVar, v),
                ["Stop"] = v => InitActionVar(p_parent.ParentTrigger, ref _IsStopVar, v),
                ["ZoomFrames"] = v => InitActionVar(p_parent.ParentTrigger, ref _ZoomFramesVar, v),
                ["StopX"] = v => InitActionVar(p_parent.ParentTrigger, ref _StopPosX, v),
                ["StopY"] = v => InitActionVar(p_parent.ParentTrigger, ref _StopPosY, v),
            };

            foreach (var entry in attributeMap)
            {
                var attr = p_node.Attributes[entry.Key];
                if (attr != null)
                    entry.Value(attr.Value);
            }
        }

        public override void Activate(ref bool p_isRunNext)
		{
            p_isRunNext = true;
            if (_FollowVar != null)
            {
                string valueString = _FollowVar.ValueString;
                ModelHuman model = GetModel(valueString);
                if (model != null)
                {
                    LocationCamera.Current.Node = model.ModelObject.CameraNode;
                }
                else
                {
                    LocationCamera.Current.Stop();
                }
            }
            if (_SmoothnessVar != null)
            {
                switch (_SmoothnessVar.Type)
                {
                    case VariableTypeE.VT_INT:
                        LocationCamera.FluencyCurrent = _SmoothnessVar.ValueInt;
                        break;
                    case VariableTypeE.VT_DOUBLE:
                        LocationCamera.FluencyCurrent = _SmoothnessVar.ValueFloat;
                        break;
                }
            }
            if (_ZoomVar != null)
            {
                int frames = 30;
                if (_ZoomFramesVar != null)
                {
                   frames = _ZoomFramesVar.ValueInt == 0 ? 1 : _ZoomFramesVar.ValueInt;
                }
                float currentZoom = LocationCamera.CurrentZoom;
                switch (_ZoomVar.Type)
                {
                    case VariableTypeE.VT_INT:
                        LocationCamera.Current.Zooming((float)_ZoomVar.ValueInt * currentZoom, false, frames);
                        break;
                    case VariableTypeE.VT_DOUBLE:
                        LocationCamera.Current.Zooming(_ZoomVar.ValueFloat * currentZoom, false, frames);
                        break;
                }
            }
            if (_IsStopVar != null)
            {
                LocationCamera.Current.Stop();
            }
            if (_StopPosX != null || _StopPosY != null)
            {
                float? X = null;
                float? Y = null;
                if (_StopPosX != null)
                {
                    X = _StopPosX.ValueFloat;
                }
                if (_StopPosY != null)
                {
                    Y = _StopPosY.ValueFloat;
                }
                LocationCamera.Current.Stop(X,Y);
            }
            if (_FramesVar != null)
            {
                //LocationCamera.Current.fra(_FramesVar.ValueInt);
            }
        }

		public override TriggerAction Copy()
		{
            return new TA_Camera(this);
        }

        public override string ToString()
		{
            string text = "Camera:";
            if (_FollowVar != null)
            {
                text = text + " Follow: " + _FollowVar.DebugStringValue + "|";
            }
            if (_SmoothnessVar != null)
            {
                switch (_SmoothnessVar.Type)
                {
                    case VariableTypeE.VT_INT:
                        LocationCamera.FluencyCurrent = _SmoothnessVar.ValueInt;
                        break;
                    case VariableTypeE.VT_DOUBLE:
                        LocationCamera.FluencyCurrent = _SmoothnessVar.ValueFloat;
                        break;
                }
                text = text + " Smoothness: " + _SmoothnessVar.DebugStringValue;
            }
            if (_ZoomVar != null)
            {
                text = text + " Zoom: " + _ZoomVar.DebugStringValue + "|";
            }
            if (_IsStopVar != null)
            {
                text += "Stop: 1";
            }
            return text;
        }
	}
}
