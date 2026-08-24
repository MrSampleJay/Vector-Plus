using Nekki.Vector.Core.Models;
using Nekki.Vector.Core.Node;
using Nekki.Vector.Core.Trigger;
using Nekki.Vector.Core.Trigger.Events;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using Xml2Prefab;

namespace Nekki.Vector.Core.Location
{
    public class TriggerRunner : QuadRunner
    {
        public enum TriggerColisionType
        {
            OneNode = 0,
            MultiNode = 1
        }

        public enum TriggerAIType
        {
            OneAI = 0,
            MultiAI = 1
        }
        private enum TriggerType
        {
            TT_Rectangle = 0,
            TT_Elips = 1,
            TT_UpDiagonal = 2,
            TT_DownDiagonal = 3,
            TT_Circle = 4
        }

        private TriggerTimer _timer;

        private Variable _AIvar;

        private Variable _nodeVar;

        private Variable _modelVar;

        private Variable _activeVar;

        private ModelHuman _checkedModel;

        private Dictionary<string, Variable> vars = new Dictionary<string, Variable>();

        private List<TriggerLoop> _loops = new List<TriggerLoop>();

        private List<TriggerLine> _lines;

        private List<TE_ChangeVar> _renderEvents = new List<TE_ChangeVar>();

        private TriggerColider _colider;

        private XmlNode _xmlNode;

        private string _statistic;

        private XmlNode _rawNode;

        private float _w;

        private float _h;

        private string _CollisionNodeName;

        public List<string> _NodesName;

        public int[] _MultiAI;

        private TriggerColisionType _CollisionType = TriggerColisionType.OneNode;

        private TriggerAIType _AIType = TriggerAIType.OneAI;

        private TriggerType _TriggerType;

        public string CollisionNodeName
        {
            get
            {
                return _CollisionNodeName;
            }
            set
            {
                _CollisionNodeName = value;
            }
        }

        public List<string> TriggerNodesName
        {
            get
            {
                return _NodesName;
            }
        }

        public int[] TriggerAIs
        {
            get
            {
                return _MultiAI;
            }
        }

        public bool IsRectType
        {
            get
            {
                return _TriggerType == TriggerType.TT_Rectangle;
            }
        }

        public bool IsElipsType
        {
            get
            {
                return _TriggerType == TriggerType.TT_Elips;
            }
        }

        public bool IsUpDiagonal
        {
            get
            {
                return _TriggerType == TriggerType.TT_UpDiagonal;
            }
        }

        public bool IsDownDiagonal
        {
            get
            {
                return _TriggerType == TriggerType.TT_DownDiagonal;
            }
        }

        public bool IsDiagonal
        {
            get
            {
                return IsUpDiagonal || IsDownDiagonal;
            }
        }

        public List<TriggerLine> Lines => _lines;

        public Variable AIVar => _AIvar;

        public Variable ModelVar => _modelVar;

        public string NodeName => _nodeVar == null ? "COM" : _nodeVar.ValueString;

        public bool IsActive => _activeVar.ValueInt == 1;

        public string statistic => _statistic;

        public TriggerColisionType CollisionType => _CollisionType;

        public TriggerAIType AIType => _AIType;

        public TriggerRunner(float p_x, float p_y, float p_width, float p_height, XmlNode p_node)
            : base(p_x, p_y, p_width, p_height, sticky: false, 0, XmlUtils.ParseString(p_node.Attributes["Name"], string.Empty))
        {
            _TypeClass = RunnerType.Trigger;
            _timer = new TriggerTimer(this);
            _w = p_width;
            _h = p_height;
            _xmlNode = p_node["Content"];
            _TriggerType = GetTriggerType(XmlUtils.ParseString(p_node.Attributes["Type"], "Rectangle"));
            _statistic = XmlUtils.ParseString(p_node.Attributes["Statistic"]);
            _rawNode = p_node;
        }

        public void Init()
        {
            parseVariable(_xmlNode["Init"]);
            SetTriggerCollisionType();
            // SetTriggerAIType();
            XmlNode xmlNode = _xmlNode["Template"];
            if (xmlNode != null)
            {
                string value = xmlNode.Attributes["Name"].Value;
                parseTemplate(TemplateModule.getTemplateXmlNode(value));
            }
            parseLoops(_xmlNode);
            InitEvents();
        }

        private void SetTriggerAIType()
        {
            string[] parts = _AIvar.ValueString.Split('|');
            int[] array = new int[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                array[i] = int.Parse(parts[i]);
            }

            if (array.Length == 1)
            {
                _AIType = TriggerAIType.OneAI;
                _AIvar.setValue(array[0]); 
            }
            else
            {
                _AIType = TriggerAIType.MultiAI;
                _MultiAI = array;
            }
        }
        private void SetTriggerCollisionType()
        {
            string[] array = _nodeVar.ValueString.Split('|');
            if (array.Length == 1)
            {
                _CollisionType = TriggerColisionType.OneNode;
                _CollisionNodeName = array[0];
            }
            else
            {
                _CollisionType = TriggerColisionType.MultiNode;
                _NodesName = new List<string>(array);
            }
        }

        private TriggerType GetTriggerType(string p_value)
        {
            switch (p_value)
            {
                case "Rectangle":
                    return TriggerType.TT_Rectangle;
                case "Ellipse":
                    return TriggerType.TT_Elips;
                case "UpDiagonal":
                    return TriggerType.TT_UpDiagonal;
                case "DownDiagonal":
                    return TriggerType.TT_DownDiagonal;
                case "Circle":
                    return TriggerType.TT_Circle;
                default:
                    return TriggerType.TT_Rectangle;
            }
        }
        public bool TriggerHit(ModelNode p_node, bool Equality = false)
        {
            switch (_TriggerType)
            {
                case TriggerType.TT_Elips:
                    return HitElips(p_node.Start);
                case TriggerType.TT_Rectangle:
                    return base.Hit(p_node.Start, Equality);
                case TriggerType.TT_UpDiagonal:
                    return HitUpDiagonal(p_node);
                case TriggerType.TT_DownDiagonal:
                    return HitDownDiagonal(p_node);
                case TriggerType.TT_Circle:
                    return HitCircle(p_node.Start);
                default:
                    return false;
            }
        }

        public bool HitElips(Vector3d p_point)
        {
            bool flag = rectangle.Size.Width >= rectangle.Size.Height;
            float num = ((!flag) ? (rectangle.Size.Height / 2f) : (rectangle.Size.Width / 2f));
            float num2 = ((!flag) ? (rectangle.Size.Width / 2f) : (rectangle.Size.Height / 2f));
            float num3 = Mathf.Sqrt(1f - num2 * num2 / (num * num));
            float num4 = num * num3;
            float midX = rectangle.MidX;
            float midY = rectangle.MidY;
            Vector2 vector = ((!flag) ? new Vector2(midX, midY - num4) : new Vector2(midX - num4, midY));
            Vector2 vector2 = ((!flag) ? new Vector2(midX, midY + num4) : new Vector2(midX + num4, midY));
            float num5 = Mathf.Sqrt(Mathf.Pow(vector.x - (float)p_point.X, 2f) + Mathf.Pow(vector.y - (float)p_point.Y, 2f));
            float num6 = Mathf.Sqrt(Mathf.Pow(vector2.x - (float)p_point.X, 2f) + Mathf.Pow(vector2.y - (float)p_point.Y, 2f));
            if (num5 + num6 < 2f * num)
            {
                return true;
            }
            return false;
        }

        private bool HitUpDiagonal(ModelNode p_node)
        {
            return p_node.CroosLine(rectangle.BottomLeft, rectangle.TopRight);
        }

        private bool HitDownDiagonal(ModelNode p_node)
        {
            return p_node.CroosLine(rectangle.TopLeft, rectangle.BottomRight);
        }

        private bool HitCircle(Vector3d p_point)
        {
            float num = Mathf.Sqrt(Mathf.Pow(rectangle.MidX - (float)p_point.X, 2f) + Mathf.Pow(rectangle.MidY - (float)p_point.Y, 2f));
            return num < rectangle.Size.Width / 2f;
        }
        protected override void SerializeData()
        {
            _UnityObject.AddComponent<Xml2PrefabTriggerContainer>().Init(_rawNode.OuterXml, _h, _w, Choice);
            _CachedTransform = _UnityObject.transform;
        }

        private void InitEvents()
        {
            foreach (TriggerLoop loop in _loops)
            {
                foreach (TriggerEvent @event in loop.Events)
                {
                    if (@event.Type == TriggerEvent.EventType.TET_ON_SHOW || @event.Type == TriggerEvent.EventType.TET_ON_HIDE)
                    {
                        CreateCollider();
                    }
                    if (@event.Type == TriggerEvent.EventType.TET_VAR_CHANGE)
                    {
                        _renderEvents.Add((TE_ChangeVar)@event);
                    }
                }
            }
        }

        private void CreateCollider()
        {
            if (_colider == null)
            {
                var controller = UnityObject.GetComponent<TriggerController>();
                controller.OnBecameVisibleEvent += OnBecameVisible;
                controller.OnBecameInvisibleEvent += OnBecameUnvisible;
            }
        }

        private void OnBecameVisible()
        {
            CheckEvent(new TE_OnShow(), null);
        }

        private void OnBecameUnvisible()
        {
            CheckEvent(new TE_OnHide(), null);
        }

        public void parseTemplate(XmlNode p_node)
        {
            parseLoops(p_node);
        }

        public void parseVariable(XmlNode p_node, bool isTemplate = false)
        {
            if (p_node == null)
            {
                return;
            }
            foreach (XmlNode childNode in p_node.ChildNodes)
            {
                if (!childNode.LocalName.Equals("SetVariable"))
                {
                    continue;
                }
                string text = childNode.Attributes["Name"].ParseString();
                string text2 = childNode.Attributes["Value"].ParseString(string.Empty);
                if (vars.ContainsKey("_" + text))
                {
                    switch (vars["_" + text].Type)
                    {
                        case VariableTypeE.VT_INT:
                            vars["_" + text].setValue(int.Parse(text2.ToString()));
                            break;
                        case VariableTypeE.VT_DOUBLE:
                            vars["_" + text].setValue(float.Parse(text2));
                            break;
                        case VariableTypeE.VT_STRING:
                            vars["_" + text].setValue(text2);
                            break;
                    }
                }
                else
                {
                    vars["_" + text] = Variable.createVariable(text2, text, this);
                }
            }
            if (!isTemplate)
            {
                _AIvar = vars["_$AI"];
                _nodeVar = vars["_$Node"];
                _activeVar = vars["_$Active"];
                vars["_$ActionID"] = Variable.createVariable(" ", "$ActionID", this);
                if (!vars.ContainsKey("_$Model"))
                {
                    _modelVar = Variable.createVariable(" ", "$Model", this);
                    vars["_$Model"] = _modelVar;
                }
                else
                {
                    _modelVar = vars["_$Model"];
                }
                vars["_$Key"] = Variable.createVariable(" ", "$Key", this);
                if (_AIvar == null)
                {
                    _AIvar = Variable.createVariable("-1", "$AI");
                }
                if (_activeVar == null)
                {
                }
                if (_nodeVar != null)
                {
                }
            }
        }

        public void parseLoops(XmlNode p_node)
        {
            if (p_node == null)
            {
                return;
            }
            foreach (XmlNode childNode in p_node.ChildNodes)
            {
                if (string.Equals(childNode.LocalName, "Loop"))
                {
                    TriggerLoop triggerRunnerLoop;
                    if (childNode.Attributes["Template"] != null)
                    {
                        string value = childNode.Attributes["Template"].Value;
                        XmlNode templateLoopXML = TemplateModule.getTemplateLoopXML(value);
                        triggerRunnerLoop = TriggerLoop.createLoop(templateLoopXML, this);
                        _loops.Add(triggerRunnerLoop);
                    }
                    else
                    {
                        triggerRunnerLoop = TriggerLoop.createLoop(childNode, this);
                        _loops.Add(triggerRunnerLoop);
                    }
                }
            }
        }

        public void CheckEvent(TriggerEvent p_event, ModelHuman p_model)
        {
            if (!IsEnabled || (!IsActive && !p_event.IsTimeOutOrActivate()) || (p_model != null && p_model.IsPhysics && !p_event.IsCollision()))
            {
                return;
            }
            _checkedModel = p_model;
            List<List<TriggerAction>> list = new List<List<TriggerAction>>();
            foreach (var loop in _loops)
            {
                if (loop.CheckEvent(p_event))
                {
                    list.Add(loop.Actions);
                }
            }
            if (list.Count != 0)
            {
                foreach (var actions in list)
                {
                    TriggerActionsRenderer.Current.AddActions(actions);
                }
                if (p_model != null)
                {
                    if (p_model.UserData.IsSelf)
                    {
                        p_model.controllerStatistics.SetTrigger(this);
                    }
                }
                _modelVar.setValue("");
                _checkedModel = null;
            }
        }

        public void ResetRenderEvents()
        {
            foreach (var @event in _renderEvents)
            {
                @event.Reset();
            }
        }

        public void CheckRenderEvent(ModelHuman p_model)
        {
            if (_renderEvents == null)
            {
                return;
            }
            _modelVar.setValue(p_model.ModelName);
            foreach (TE_ChangeVar renderEvent in _renderEvents)
            {
                if (renderEvent.IsChange())
                {
                    CheckEvent(renderEvent, p_model);
                    _modelVar.setValue(p_model.ModelName);
                }
            }
            _modelVar.setValue(string.Empty);
        }

        public override bool Render()
        {
            return _timer.Render();
        }

        public override void Reset()
        {
            base.Reset();
            _timer.Reset();
            foreach (var vars in vars.Values)
            {
                vars.resetValues();
            }
            foreach (var loop in _loops)
            {
                loop.Reset();
            }
            if (_lines != null)
            {
                foreach (var line in _lines)
                {
                    line.Reset();
                }
            }
        }

        public Variable GetVar(string p_key)
        {
            if (!vars.ContainsKey(p_key))
            {
                DebugUtils.Dialog("No Var Name = " + p_key + " in trigger " + Name, true);
                return null;
            }
            return vars[p_key];
        }

        public SpawnRunner GetSpawnByName(string p_name)
        {
            foreach (var spawn in ParentElements.Spawns)
            {
                if (spawn.Name == p_name)
                {
                    return spawn;
                }
            }
            return null;
        }

        public void SetModelVar()
        {
            if (_checkedModel == null)
            {
                return;
            }
            _modelVar.setValue(_checkedModel.ModelName);
        }

        public void AddLine(TriggerLine p_line)
        {
            if (_lines == null)
            {
                _lines = new List<TriggerLine>();
            }
            _lines.Add(p_line);
        }

        public void SetKeyVar(string p_key)
        {
            vars["_$Key"].setValue(p_key);
        }

        public void SetTimer(int p_frames)
        {
            _timer.Start(p_frames);
        }

        public override string ToString()
        {
            return null;
        }
    }
}
