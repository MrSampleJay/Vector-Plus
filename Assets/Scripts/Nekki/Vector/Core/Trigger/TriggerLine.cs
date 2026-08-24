using Nekki.Vector.Core.Location;
using Nekki.Vector.Core.Models;
using Nekki.Vector.Core.Node;
using Nekki.Vector.Core.Trigger.Events;
using System.Collections.Generic;

namespace Nekki.Vector.Core.Trigger
{
	public class TriggerLine
	{
		private TriggerLineType _Type;

		private Variable _Line;

		private int _EventID;

		private TriggerRunner _Parent;

		private Dictionary<ModelHuman, bool> _StatusModel = new Dictionary<ModelHuman, bool>();

		public TriggerLine(TriggerRunner p_parent, int p_EventID)
		{
            _Parent = p_parent;
            _Type = TriggerLineType.NONE;
            _EventID = p_EventID;
            _Line = null;
        }

		public void Check(ModelHuman p_model)
		{
            ModelNode modelNode = p_model.GetNode(_Parent.NodeName);
            int num = 0;
            bool flag = false;
            int num2 = 0;
            switch (_Type)
            {
                case TriggerLineType.VERTICAL:
                    num2 = (int)(_Parent.XQuad + (float)_Line.ValueInt);
                    num = (int)modelNode.Start.X;
                    flag = (modelNode.Start - modelNode.End).X > 0f;
                    break;
                case TriggerLineType.HORIZONTAL:
                    num2 = (int)(_Parent.YQuad + (float)_Line.ValueInt);
                    num = (int)modelNode.Start.Y;
                    flag = true;
                    break;
            }
            bool flag2 = (flag && num2 <= num) || (!flag && num < num2);
            if (!_StatusModel.ContainsKey(p_model))
            {
                _StatusModel[p_model] = flag2;
                return;
            }
            bool flag3 = _StatusModel[p_model] != flag2;
            _StatusModel[p_model] = flag2;
            if (flag3)
            {
                TE_Line p_event = new TE_Line(null, null, _EventID);
                _Parent.CheckEvent(p_event, p_model);
            }
        }

		public void SetLine(string p_type, Variable p_line)
		{
            _Type = GetLineType(p_type);
            _Line = p_line;
        }

		public bool IsVertical()
		{
            return _Type == TriggerLineType.VERTICAL;
        }

        public void Reset()
		{
            _StatusModel.Clear();
		}

		public TriggerLineType GetLineType(string p_type)
		{
            if (p_type == "Vertical")
            {
                return TriggerLineType.VERTICAL;
            }
            if (p_type == "Horizontal")
            {
                return TriggerLineType.HORIZONTAL;
            }
            return TriggerLineType.NONE;
        }

		public int GetValue()
		{
            return _Line.ValueInt;
        }
    }
}
