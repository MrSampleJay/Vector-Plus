using System.Collections.Generic;
using System.Text;
using Nekki.Vector.Core.Location;
using Nekki.Vector.Core.Trigger.Functions;

namespace Nekki.Vector.Core.Trigger
{
	public class VariableFunction : Variable
	{
        protected bool _IsPointer;

        protected bool _IsStringFunction;

        protected string _StrValue;

        public bool IsPointer
        {
            get
            {
                return _IsPointer;
            }
        }

        public bool IsStringFunction
        {
            get
            {
                return _IsStringFunction;
            }
        }
        protected VariableFunction(string p_name, TriggerRunner p_parent) : base(p_name, p_parent)
		{
			type = VariableTypeE.VT_FUNCTION;
		}

		public static VariableFunction Create(string p_value, string p_name, TriggerRunner p_parent)
		{
            string text = p_value.Substring(0, p_value.IndexOf("["));
            if (text.IndexOf("getModel") != -1)
            {
                return new VF_Model(p_value, p_name, p_parent);
            }
            if (text.IndexOf("Random") != -1)
            {
                return new VF_Random(p_value, p_name, p_parent);
            }
            return null;
        }

		public static void InitFuncVar(TriggerRunner p_parent, ref Variable p_var, string p_name)
		{
            if (p_name.Length != 0 && p_name[0] == '_')
            {
                p_var = p_parent.GetVar(p_name);
            }
            else
            {
                p_var = Variable.createVariable(p_name, string.Empty, p_parent);
            }
        }

        public static string TrimSpaces(string p_in)
        {
            if (p_in.IndexOf('\'') == -1)
            {
                return p_in.Replace(" ", string.Empty);
            }
            StringBuilder stringBuilder = new StringBuilder();
            int num = 0;
            int num2 = -1;
            int num3 = -1;
            while (true)
            {
                num = p_in.IndexOf('\'', num2 + 1);
                num2 = p_in.IndexOf('\'', num + 1);
                num++;
                num3++;
                if (num == 0)
                {
                    break;
                }
                stringBuilder.Append(p_in.Substring(num3, num - num3 - 1).Replace(" ", string.Empty));
                stringBuilder.Append(p_in.Substring(num, num2 - num));
                num3 = num2;
            }
            stringBuilder.Append(p_in.Substring(num3, p_in.Length - num3).Replace(" ", string.Empty));
            return stringBuilder.ToString();
        }

        public static List<string> GetFuncArgs(string p_value, char p_separator, string[] p_braces)
        {
            if (p_braces.Length < 1)
            {
                return null;
            }
            List<string> list = new List<string>();
            string[] array = p_value.Split(p_separator);
            string text = string.Empty;
            int i = 0;
            for (int num = array.Length; i < num; i++)
            {
                if (text.Length > 0)
                {
                    text += p_separator;
                }
                text += array[i];
                if (CheckConteinsBraces(text, p_braces))
                {
                    if (ChechBraces(text, p_braces))
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

        private static bool CheckConteinsBraces(string p_value, string[] p_braces)
        {
            for (int i = 0; i < p_braces.Length; i++)
            {
                if (p_value.IndexOf(p_braces[i][0]) != -1)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool ChechBraces(string p_value, string[] p_braces)
        {
            for (int i = 0; i < p_braces.Length; i++)
            {
                char c = p_braces[i][0];
                char c2 = p_braces[i][1];
                int num = 0;
                int num2 = 0;
                foreach (char c3 in p_value)
                {
                    if (c3 == c)
                    {
                        num++;
                    }
                    if (c3 == c2)
                    {
                        num2++;
                    }
                }
                if (num != num2)
                {
                    return false;
                }
            }
            return true;
        }

        public virtual void SimplifyArguments()
        {
        }

        protected void SimplifyArgument(ref Variable argument)
        {
            if (argument.Type == VariableTypeE.VT_FUNCTION)
            {
                VariableFunction variableFunction = argument as VariableFunction;
                variableFunction.SimplifyArguments();
                if (variableFunction.IsPointer)
                {
                    return;
                }
            }
            argument = Variable.createVariable(argument.ValueString, argument.getName());
        }
    }
}
