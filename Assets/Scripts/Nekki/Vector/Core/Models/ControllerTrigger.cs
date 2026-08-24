using Codice.Client.BaseCommands;
using Nekki.Vector.Core.Location;
using Nekki.Vector.Core.Node;
using Nekki.Vector.Core.Trigger.Events;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nekki.Vector.Core.Models
{
    public class ControllerTrigger
    {
        private List<TriggerRunner> _ActiveTriggers = new List<TriggerRunner>();

        private List<TriggerRunner> _LineTrggers = new List<TriggerRunner>();

        private ModelHuman _Model;

        private ModelNode _ComNode;

        public ControllerTrigger(ModelHuman model)
        {
            _Model = model;
        }

        public ModelNode GetNode(TriggerRunner p_trigger)
        {
            return _Model.GetNode(p_trigger.NodeName);
        }

        public ModelNode GetNode(string p_trigger)
        {
            return _Model.GetNode(p_trigger);
        }

        public void Check(TriggerRunner p_triger)
        {
            bool isActive = GetIsActive(p_triger);
            if (CheckAI(p_triger))
            {
                if (GetInside(p_triger))
                {
                    if (!isActive)
                    {
                        Activate(p_triger);
                    }
                }
                else if (isActive)
                {
                    DeActivate(p_triger);
                }
            }
            else if (isActive)
            {
                DeActivate(p_triger, false);
            }
        }

        public void Render()
        {
            CheckLine();
            CheckRenderEvents();
        }

        private void CheckLine()
        {
            for (int i = 0; i < _LineTrggers.Count; i++)
            {
                for (int j = 0; j < _LineTrggers[i].Lines.Count; j++)
                {
                    _LineTrggers[i].Lines[j].Check(_Model);
                }
            }
        }

        private void CheckRenderEvents()
        {
            for (int i = 0; i < _ActiveTriggers.Count; i++)
            {
                _ActiveTriggers[i].CheckRenderEvent(_Model);
            }
        }

        private void Activate(TriggerRunner p_trigger)
        {
            p_trigger.CheckEvent(new TE_Enter(), _Model);
            p_trigger.CheckEvent(new TE_Collision(), _Model);
            _ActiveTriggers.Add(p_trigger);
            if (p_trigger.Lines != null)
            {
                _LineTrggers.Add(p_trigger);
            }

        }

        private void DeActivate(TriggerRunner p_triger, bool p_sendEvent = true)
        {
            if (p_sendEvent)
            {
                TE_Exit p_event = new TE_Exit();
                p_triger.CheckEvent(p_event, _Model);
            }
            _ActiveTriggers.Remove(p_triger);
            if (p_triger.Lines != null)
            {
                _LineTrggers.Remove(p_triger);
            }
            p_triger.ResetRenderEvents();
            _Model.CheckDelayAction(p_triger);
        }

        public void SetKeyEvent(string p_key)
        {
            TE_Key p_event = new TE_Key(p_key);
            for (int i = 0; i < _ActiveTriggers.Count; i++)
            {
                _ActiveTriggers[i].CheckEvent(p_event, _Model);
            }
        }

        private bool GetIsActive(TriggerRunner p_triger)
        {
            return _ActiveTriggers.Contains(p_triger);
        }

        private bool CheckAI(TriggerRunner p_triger)
        {
            switch (p_triger.AIType)
            {
                case TriggerRunner.TriggerAIType.OneAI:
                    return _Model.AI == p_triger.AIVar.ValueInt || p_triger.AIVar.ValueInt == -1;
                case TriggerRunner.TriggerAIType.MultiAI:
                    {
                        for (int i = 0; i < p_triger.TriggerAIs.Length; i++)
                        {
                            if (_Model.AI == p_triger.TriggerAIs[i])
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                default:
                    return false;
            }
            
        }

        public bool GetInside(TriggerRunner p_trigger)
        {
            switch (p_trigger.CollisionType)
            {
                case TriggerRunner.TriggerColisionType.OneNode:
                    return p_trigger.TriggerHit(GetNode(p_trigger));
                case TriggerRunner.TriggerColisionType.MultiNode:
                    {
                        string text = null;
                        for (int i = 0; i < p_trigger.TriggerNodesName.Count; i++)
                        {
                            text = (p_trigger.CollisionNodeName = p_trigger.TriggerNodesName[i]);
                            if (p_trigger.TriggerHit(GetNode(text)))
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                default:
                    return false;
            }
        }

        public void Reset()
        {
            _ActiveTriggers.Clear();
            _LineTrggers.Clear();
        }
    }
}
