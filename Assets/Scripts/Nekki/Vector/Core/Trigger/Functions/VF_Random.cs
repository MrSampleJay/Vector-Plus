using Nekki.Vector.Core.Location;
using System.Collections.Generic;
using UnityEngine;

namespace Nekki.Vector.Core.Trigger.Functions
{
	public class VF_Random : VariableFunction
	{
        private List<Variable> _Vars;

        private List<Variable> _Weights;

        private Variable tmpVar;

        private static readonly System.Random rng = new();

        public override int ValueInt
        {
            get
            {
                if (_Weights == null)
                {
                    return GetRangeInt();
                }
                return GetListInt();
            }
        }

        public override float ValueFloat
        {
            get
            {
                if (_Weights == null)
                {
                    return GetRangeFloat();
                }
                return GetListFloat();
            }
        }

        public override string ValueString
        {
            get
            {
                if (_Weights == null)
                {
                    return GetRangeString();
                }
                return GetListString();
            }
        }

        public override string DebugStringValue
        {
            get
            {
                return "Random[" + _Vars[0].ValueInt + ":" + _Vars[1].ValueInt + "]";
            }
        }

        // I have no darn clue what this does

        //public override string ValueForSave
        //{
        //    get
        //    {
        //        string text = string.Empty;
        //        if (_IsPointer)
        //        {
        //            text = "_";
        //        }
        //        string text2 = "?" + text;
        //        string text3 = text2;
        //        return text3 + "RangeRandom[" + _Vars[0].ValueInt + ":" + _Vars[1].ValueInt + "]";
        //    }
        //}

        protected internal VF_Random(string p_value, string p_name, TriggerRunner p_parent)
            : base(p_name, p_parent)
        {
            _Vars = new List<Variable>();
            p_value = VariableFunction.TrimSpaces(p_value);
            int num = p_value.IndexOf("[") + 1;
            int num2 = p_value.LastIndexOf("]");
            string p_value2 = p_value.Substring(num, num2 - num);
            if (p_value.IndexOf("?Range") == 0)
            {
                ParseRange(p_value2, p_parent);
            }
            if (p_value.IndexOf("?List") == 0)
            {
                ParseList(p_value2, p_parent);
            }
        }

        private void ParseRange(string p_value, TriggerRunner p_parent)
        {
            List<string> list = ParseParams(p_value, false);
            foreach (string item in list)
            {
                VariableFunction.InitFuncVar(p_parent, ref tmpVar, item);
                _Vars.Add(tmpVar);
            }
        }

        private void ParseList(string p_value, TriggerRunner p_parent)
        {
            _Weights = new List<Variable>();
            List<string> list = ParseParams(p_value, true);
            foreach (string item in list)
            {
                string[] array = item.Split(':');
                VariableFunction.InitFuncVar(p_parent, ref tmpVar, array[0]);
                _Vars.Add(tmpVar);
                VariableFunction.InitFuncVar(p_parent, ref tmpVar, (array.Length != 2) ? "1" : array[1]);
                _Weights.Add(tmpVar);
            }
        }

        private List<string> ParseParams(string p_value, bool p_isList)
        {
            if (p_isList)
            {
                p_isList = p_value.Contains(":");
            }
            string[] array = p_value.Split(',');
            List<string> list = new List<string>();
            string text = string.Empty;
            for (int i = 0; i < array.Length; i++)
            {
                if (text != string.Empty)
                {
                    text += ",";
                }
                text += array[i];
                if (text.Contains("["))
                {
                    if (ChechBraces(text) && (!p_isList || text.Contains(":")))
                    {
                        list.Add(text);
                        text = string.Empty;
                    }
                }
                else
                {
                    list.Add(text);
                    text = string.Empty;
                }
            }
            return list;
        }

        private bool ChechBraces(string p_value)
        {
            int num = 0;
            int num2 = 0;
            foreach (char c in p_value)
            {
                if (c == '[')
                {
                    num++;
                }
                if (c == ']')
                {
                    num2++;
                }
            }
            return num == num2;
        }

        private int GetRangeInt()
        {
            int valueInt = _Vars[0].ValueInt;
            int num = _Vars[1].ValueInt + 1;
            if (valueInt < 0)
            {
                int maxRange = num - valueInt;
                int r = rng.Next(0, maxRange);
                return r + valueInt;
            }
            return rng.Next(valueInt, num);
        }

        private float GetRangeFloat()
        {
            return GetRangeInt();
        }

        private string GetRangeString()
        {
            return GetRangeInt().ToString();
        }

        private Variable GetListVar()
        {
            List<int> list = new List<int>();
            foreach (Variable weight in _Weights)
            {
                list.Add(weight.ValueInt);
            }

            uint total = 0;
            foreach (int w in list)
            {
                total += (uint)w;
            }

            int roll = rng.Next(0, (int)total + 1);

            int cumulative = 0;
            for (int i = 0; i < list.Count; i++)
            {
                cumulative += list[i];
                if (cumulative > roll)
                {
                    return _Vars[i];
                }
            }

            return _Vars[0];
        }

        private int GetListInt()
        {
            return GetListVar().ValueInt;
        }

        private float GetListFloat()
        {
            return GetListVar().ValueFloat;
        }

        private string GetListString()
        {
            return GetListVar().ValueString;
        }

        public override void SimplifyArguments()
        {
            for (int i = 0; i < _Vars.Count; i++)
            {
                Variable argument = _Vars[i];
                SimplifyArgument(ref argument);
                _Vars[i] = argument;
            }
            for (int j = 0; j < _Weights.Count; j++)
            {
                Variable argument2 = _Weights[j];
                SimplifyArgument(ref argument2);
                _Weights[j] = argument2;
            }
        }
    }
}
