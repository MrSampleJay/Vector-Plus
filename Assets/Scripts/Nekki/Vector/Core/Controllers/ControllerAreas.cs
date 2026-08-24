using Nekki.Vector.Core.Location;
using Nekki.Vector.Core.Models;
using Nekki.Vector.Core.Node;
using System.Collections.Generic;

namespace Nekki.Vector.Core.Controllers
{
	public class ControllerAreas
	{
		private ModelHuman _Model;

		private ModelNode _ComNode;

		private List<AreaRunner> _ActiveAreas = new List<AreaRunner>();

		public List<AreaRunner> ActiveAreas => _ActiveAreas;

		public ControllerAreas(ModelHuman model)
		{
            _Model = model;
            _ComNode = model.GetNode("COM");
        }

		private bool IsInside(AreaRunner p_value)
		{
            return p_value.Hit(_ComNode.Start);
        }

		private bool IsActive(AreaRunner p_value)
		{
            return ActiveAreas.Contains(p_value);
        }

        private void Activate(AreaRunner p_value)
		{
            ModelNode modelNode = _Model.GetNode("COM");
            int modelDirection = ((modelNode.Start - modelNode.End).X > 0f) ? 1 : (-1);
            if (p_value._Direction != 0 && p_value._Direction != modelDirection)
            {
                return;
            }
            _ActiveAreas.Add(p_value);
            p_value.Activate(_Model);
            _Model.OnActiveArea(p_value);
        }

		private void Deactivate(AreaRunner p_value)
		{
            _Model.CheckDelayAction(p_value);
            p_value.Deactivate(_Model);
            _ActiveAreas.Remove(p_value);
        }

		public void Check(AreaRunner p_value)
		{
            bool flag = IsInside(p_value);
            if (IsActive(p_value))
            {
                if (!flag)
                {
                    Deactivate(p_value);
                }
            }
            else if (flag)
            {
                Activate(p_value);
            }
        }

		public void Reset()
		{
            _ActiveAreas.Clear();
        }
    }
}
