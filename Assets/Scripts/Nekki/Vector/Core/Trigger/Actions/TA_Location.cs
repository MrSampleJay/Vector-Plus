using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Serialization;

namespace Nekki.Vector.Core.Trigger.Actions
{
    public class TA_Location : TriggerAction
    {
        private Variable _TotalBonuses;

        private Variable _TotalTricks;

        private Variable _TotalPoints;

        private TA_Location(TA_Location p_copyAction)
            : base(p_copyAction._ParentLoop)
        {
            _TotalTricks = p_copyAction._TotalTricks;
            _TotalBonuses = p_copyAction._TotalBonuses;
            _TotalPoints = p_copyAction._TotalPoints;
        }

        public TA_Location(XmlNode p_node, TriggerLoop p_parent)
            : base(p_parent)
        {
            XmlAttribute xmlAttribute1 = p_node.Attributes["TotalBonuses"];
            XmlAttribute xmlAttribute2 = p_node.Attributes["TotalTricks"];
            XmlAttribute xmlAttribute3 = p_node.Attributes["TotalPoints"];
            if (xmlAttribute1 != null)
            {
                InitActionVar(p_parent.ParentTrigger, ref _TotalBonuses, xmlAttribute1.Value);
            }
            if (xmlAttribute2 != null)
            {
                InitActionVar(p_parent.ParentTrigger, ref _TotalTricks, xmlAttribute2.Value);
            }
            if (xmlAttribute3 != null)
            {
                InitActionVar(p_parent.ParentTrigger, ref _TotalPoints, xmlAttribute3.Value);
            }
        }

        public override void Activate(ref bool p_isRunNext)
        {
            p_isRunNext = true;
            if(_TotalBonuses != null)
                LevelMainController.current.Location.Sets.totalBonus = _TotalBonuses.ValueInt;
            if (_TotalTricks != null)
                LevelMainController.current.Location.Sets.totalTricks = _TotalTricks.ValueInt;
            if (_TotalPoints != null)
                LevelMainController.current.Location.Sets.totalPoints = _TotalPoints.ValueInt;
        }

        public override TriggerAction Copy()
        {
            return new TA_Location(this);
        }

        public override string ToString()
        {
            return "Location TotalBonuses=" + _TotalBonuses.DebugStringValue + " TotalTricks=" + _TotalTricks.DebugStringValue + " TotalPoints=" + _TotalPoints.DebugStringValue;
        }
    }
}
