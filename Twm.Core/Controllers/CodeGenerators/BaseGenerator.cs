using System;
using System.Collections.Generic;
using System.Text;

namespace Twm.Core.Controllers.CodeGenerators
{
    public abstract class BaseCodeGenerator
    {

        protected Dictionary<Type, string> TypeAlias = new Dictionary<Type, string>
        {
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(object), "object" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(string), "string" },
            { typeof(uint), "uint" },
            { typeof(ulong), "ulong" },
            // Yes, this is an odd one.  Technically it's a type though.
            { typeof(void), "void" }
        };


        protected StringBuilder Builder;

        protected  const string NameSpace = "Twm.Core.DataCalc";

        protected const string IndicatorNameSpace = "Twm.Custom.Indicators";

        protected const string StrategyNameSpace = "Twm.Custom.Strategies";

        protected BaseCodeGenerator()
        {
            Builder = new StringBuilder();
        }

        public abstract void Build();


        protected void BeginNameSpace(string nameSpace)
        {
            Builder.AppendLine("namespace " + nameSpace);
            Builder.AppendLine("{");
        }

        protected void EndNameSpace()
        {
            Builder.AppendLine("}");
        }


        protected void AppendLine(int intend, string line)
        {
            Builder.AppendLine(AddIntend(intend) + line);
        }

        protected string AddIntend(int count)
        {
            var result = "";
            for (int i = 0; i < count; i++)
            {
                result += "\t";
            }

            return result;
        }
    }
}