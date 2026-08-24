using Nekki.Vector.Core.Models;
using System.Xml;
using UnityEngine;

namespace Nekki.Vector.Core.Trigger.Actions
{
    internal class TA_Impulse : TriggerAction
    {
        private Variable _ModelVar;

        private Variable _Impuls;

        private Variable _R;

        private Variable _Absorption;

        public TA_Impulse(TA_Impulse p_copyAction)
          : base(p_copyAction._ParentLoop)
        {
            _ModelVar = p_copyAction._ModelVar;
        }
        public TA_Impulse(XmlNode p_node, TriggerLoop p_parent)
          : base(p_parent)
        {
            InitActionVar(p_parent.ParentTrigger, ref _ModelVar, p_node.Attributes["Model"].Value);
            InitActionVar(p_parent.ParentTrigger, ref _Impuls, p_node.Attributes["Impulse"].Value);
            InitActionVar(p_parent.ParentTrigger, ref _R, p_node.Attributes["R"].Value);
            if (p_node.Attributes["Absorption"] != null)
            {
                InitActionVar(p_parent.ParentTrigger, ref _Absorption, p_node.Attributes["Absorption"].Value);
            }
        }

        public override void Activate(ref bool p_isRunNext)
        {
            p_isRunNext = true;
            ModelHuman model = GetModel(_ModelVar.ValueString);
            if (model != null)
            {
                if (_Absorption != null)
                {
                    model.ResetImpuls(_Absorption.ValueFloat);
                }
                model.Stricke(_Impuls.ValueFloat, _R.ValueFloat, new Vector3(_ParentLoop.ParentTrigger.rectangle.MidX, _ParentLoop.ParentTrigger.rectangle.MidY, 0f));
            }
        }
        public override TriggerAction Copy()
        {
            return new TA_Impulse(this);
        }
        public override string ToString()
        {
            return "Impuls Model=" + _ModelVar.DebugStringValue + " Impuls=" + _Impuls.ValueFloat + " R=" + _R.ValueFloat;
        }
    }
}
