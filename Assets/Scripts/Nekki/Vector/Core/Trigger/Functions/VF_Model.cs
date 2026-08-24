using Nekki.Vector.Core.Camera;
using Nekki.Vector.Core.Location;
using Nekki.Vector.Core.Models;
using Nekki.Vector.Core.Node;
using UnityEngine;

namespace Nekki.Vector.Core.Trigger.Functions
{
	public class VF_Model : VariableFunction
	{
		private delegate int ModelVFGeter();

		private VF_ModelNode _NodeFunc;

		private Variable paramVar;

		private ModelVFGeter _Geter;

        public override int ValueInt
        {
            get
            {
                if (_NodeFunc != null)
                {
                    return _NodeFunc.GetValue(GetModel());
                }
                return _Geter();
            }
        }

        public override float ValueFloat
        {
            get
            {
                return ValueInt;
            }
        }

        public override string ValueString
        {
            get
            {
                return ValueInt.ToString();
            }
        }

        public override string DebugStringValue
        {
            get
            {
                ModelHuman model = GetModel();
                if (model == null)
                {
                    return "Model=null";
                }
                return ValueInt.ToString();
            }
        }

        public VF_Model(string p_value, string p_name, TriggerRunner p_parent)
			: base(p_name, p_parent)
		{
            _NodeFunc = null;
            _Geter = null;
            string[] array = p_value.Split('.');
            int num = array[0].IndexOf("[") + 1;
            int num2 = array[0].LastIndexOf("]");
            string p_name2 = p_value.Substring(num, num2 - num);
            InitFuncVar(p_parent, ref paramVar, p_name2);
            string text = array[1];
            switch (text)
            {
                case "AI":
                    _Geter = GetAI;
                    break;
                case "localPositionX":
                    _Geter = GetLocalPositionX;
                    break;
                case "localPositionY":
                    _Geter = GetLocalPositionY;
                    break;
                case "worldPositionX":
                    _Geter = GetWorldPositionX;
                    break;
                case "worldPositionY":
                    _Geter = GetWorldPositionY;
                    break;
                case "direction":
                    _Geter = GetDirection;
                    break;
                case "isControlled":
                    _Geter = GetIsControlled;
                    break;
                case "isCameraFollow":
                    _Geter = GetIsCameraFollow;
                    break;
                case "animationName":
                    _Geter = GetAnimationName;
                    break;
                case "animationFrame":
                    _Geter = GetAnimationFrame;
                    break;
                case "condition":
                    _Geter = GetCondition;
                    break;
                default:
                    if (text.IndexOf("getNode") != -1)
                    {
                        _NodeFunc = new VF_ModelNode(text, array[2], p_parent);
                    }
                    break;
            }
            if (_Geter == null && _NodeFunc == null)
            {
                DebugUtils.Dialog("TF_Func init error by geterName = " + text, true);
            }
        }

		public ModelHuman GetModel()
		{
            ModelHuman modelByName = LevelMainController.current.Location.GetModelByName(paramVar.ValueString);
            if (modelByName == null)
            {
                Debug.LogError("No model named = \"" + paramVar.ValueString + "\" in trigger \"" + parent.Name + "\"");
            }
            return modelByName;
        }

		private int GetAI()
		{
            return GetModel().AI;
        }

        private int GetLocalPositionX()
		{
            ModelNode modelNode = GetModel().GetNode("COM");
            return (int)(modelNode.Start.X - parent.XQuad);
        }

		private int GetLocalPositionY()
		{
            ModelNode modelNode = GetModel().GetNode("COM");
            return (int)(modelNode.Start.Y - parent.YQuad);
        }

		private int GetWorldPositionX()
		{
            ModelNode modelNode = GetModel().GetNode("COM");
            return (int)modelNode.Start.X;
        }

		private int GetWorldPositionY()
		{
            ModelNode modelNode = GetModel().GetNode("COM");
            return (int)modelNode.Start.Y;
        }

		private int GetDirection()
		{
            ModelNode modelNode = GetModel().GetNode("COM");
            return ((modelNode.Start - modelNode.End).X > 0f) ? 1 : (-1);
        }

		private int GetCondition()
		{
            return 0;
        }

        private int GetAnimationName()
		{
            var model = GetModel();
            if (model.ControllerAnimations.Name == null)
            {
                return int.MaxValue;
            }
            return GetModel().ControllerAnimations.Name.GetHashCode();
        }

        private int GetAnimationFrame()
		{
            return GetModel().ControllerAnimations.CurrentFrame;
        }

        private int GetIsControlled()
		{
            return GetModel().ControllerKeys.Enable ? 1 : 0;
        }

        private int GetIsCameraFollow()
		{
            return LocationCamera.Current.Node.Equals(GetModel().ModelObject.CameraNode) ? 1 : 0;
        }
    }
}
