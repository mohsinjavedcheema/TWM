using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Twm.Core.Controllers.CodeGenerators
{
    public class IndicatorCodeGenerator : BaseCodeGenerator
    {
        private string _indicatorName;


        private readonly string _paramsWithTypes;

        private readonly List<string> _paramList;

        private readonly string _params;


        /// <summary>
        /// Define the class.
        /// </summary>
        public IndicatorCodeGenerator(string className, List<PropertyDeclarationSyntax> properties)
        {
            _indicatorName = className;

            var propertiesDictionary = properties.ToDictionary(x => x.Identifier.ToString(), x => x.Type.ToString());

            _paramsWithTypes = String.Join(", ", propertiesDictionary.Select(x => x.Value
                                                                            + " " + char.ToLowerInvariant(x.Key[0]) +
                                                                            x.Key.Substring(1)));


            _paramList = propertiesDictionary.Select(x => char.ToLowerInvariant(x.Key[0]) + x.Key.Substring(1)).ToList();
                
            

            _params = String.Join(", ",
                propertiesDictionary.Select(x => char.ToLowerInvariant(x.Key[0]) + x.Key.Substring(1)));
        }

        public override void Build()
        {
            AppendLine(0, "#region Twm generated code. Neither change nor remove.");
            AppendLine(0, "");
            
            BeginNameSpace(IndicatorNameSpace);
            AddIndicatorClass(1);
            EndNameSpace();

            BeginNameSpace(StrategyNameSpace);
            AddStrategyClass(1);
            EndNameSpace();

            AppendLine(0, "");
            AppendLine(0, "#endregion");
        }


        private void AddIndicatorClass(int intend)
        {
            AppendLine(intend, "public partial class Indicator : " + NameSpace + ".IndicatorBase");
            AppendLine(intend, "{");
            AddIndicatorExtendedMethod(intend + 1);
            AppendLine(0, "");
            AddIndicatorBaseMethod(intend + 1);
            AppendLine(intend, "}");
        }

        private void AddStrategyClass(int intend)
        {
            AppendLine(intend, "public partial class Strategy : " + NameSpace + ".StrategyBase");
            AppendLine(intend, "{");
            AddStrategyExtendedMethod(intend + 1);
            AppendLine(0, "");
            AddStrategyBaseMethod(intend + 1);
            AppendLine(intend, "}");
        }


        private void AddIndicatorExtendedMethod(int intend)
        {
            AppendLine(intend,
                string.Format("public {0} {0}({1}{2}{3})", _indicatorName, _paramsWithTypes, _paramsWithTypes.Any()?", ":"", "ScriptOptions options = null"));
            AppendLine(intend, "{");


            AppendLine(intend + 1,
                string.Format("return {0}(Input{1}{2}, options);", _indicatorName, _params.Any() ? ", " : "", _params));

            AppendLine(intend, "}");
        }


        private void AddIndicatorBaseMethod(int intend)
        {
            var condition = "cacheIndicator.EqualsInput(input)";
            foreach (var param in _paramList)
            {
                condition += $" && cacheIndicator.{char.ToUpperInvariant(param[0]) + param.Substring(1)} == {param}";
            }
            
            AppendLine(intend,
                string.Format("public {0} {0}(ISeries<double> input{1}{2}, ScriptOptions options = null)",
                    _indicatorName,
                    _paramsWithTypes.Any() ? ", " : "", _paramsWithTypes));
            AppendLine(intend, "{");
            AppendLine(intend + 1, $"IEnumerable<{_indicatorName}> cache = DataCalcContext.GetIndicatorCache<{_indicatorName}>();");
            AppendLine(intend + 1, "foreach (var cacheIndicator in cache)");
            AppendLine(intend + 1, "{");
            AppendLine(intend + 2, $"if ({condition})");
            AppendLine(intend + 3, "return cacheIndicator;");
            AppendLine(intend + 1, "}");
            AppendLine(intend + 1, $"var indicator = DataCalcContext.CreateIndicator<{_indicatorName}>(input, false, options);");
            foreach (var param in _paramList)
            {
                AppendLine(intend + 1, $"indicator.{char.ToUpperInvariant(param[0]) + param.Substring(1)} = {param};");
            }
            /*AppendLine(intend + 1, "if (options != null)");
            AppendLine(intend + 2, "indicator.Options = options;");*/
            AppendLine(intend + 1, "indicator.ChangeState();");
            AppendLine(intend + 1, "return indicator;");
            AppendLine(intend, "}");
        }


        private void AddStrategyExtendedMethod(int intend)
        {
            AppendLine(intend,
                string.Format("public {0} {0}({1}{2}{3})", _indicatorName, _paramsWithTypes, _paramsWithTypes.Any() ? ", " : "", "ScriptOptions options = null"));
            AppendLine(intend, "{");

            AppendLine(intend + 1, "indicator.SetDataCalcContext(GetDataCalcContext());");
            AppendLine(intend + 1,
                string.Format("return indicator.{0}(Input{1}{2}, options);", _indicatorName, _params.Any() ? ", " : "", _params));

            AppendLine(intend, "}");
        }


        private void AddStrategyBaseMethod(int intend)
        {
            
            AppendLine(intend,
                string.Format("public {0} {0}(ISeries<double> input{1}{2}, ScriptOptions options = null)",
                    _indicatorName,
                    _paramsWithTypes.Any() ? ", " : "", _paramsWithTypes));
            AppendLine(intend, "{");
            AppendLine(intend + 1, "indicator.SetDataCalcContext(GetDataCalcContext());");
            AppendLine(intend + 1,

                string.Format("return indicator.{0}(input{1}{2}, options);", _indicatorName, _params.Any() ? ", " : "", _params));

            AppendLine(intend, "}");
        }

        public string Generate()
        {
            return Builder.ToString();
        }
    }
}