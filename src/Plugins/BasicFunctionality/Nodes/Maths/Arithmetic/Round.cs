using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Inbuilt.Nodes.Maths.Arithmetic
{
    public class Round : IFunctionNode
    {
        private readonly INodeField _roundedValue = Constructor.NodeField("Rounded Value").WithOutput<double>();
        private readonly INodeField _roundingMethod = Constructor.NodeField("Rounding Method").WithValue("display", RoundingMethod.Closest, true);
        private readonly INodeField _roundValueField = Constructor.NodeField("Round to").WithInput<double>();
        private readonly INodeField _valueToRound = Constructor.NodeField("Value to Round").WithInput<double>();

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return _roundedValue;
                yield return _valueToRound;
                yield return _roundValueField;
                yield return _roundingMethod;
            }
        }

        public string NodeName { get; } = "Round";

        public void Evaluate()
        {
            object testValueToRound = _valueToRound[INodeField.InputKey];
            double valueToRound = (double)testValueToRound;
            double roundValueField = _roundValueField.GetInput<double>();
            double integerRoundingValue = valueToRound / roundValueField;
            switch (_roundingMethod["display"])
            {
                case RoundingMethod.Closest:
                case 0:
                    integerRoundingValue = Math.Round(integerRoundingValue);
                    break;
                case RoundingMethod.Up:
                case 1:
                    integerRoundingValue = Math.Round(integerRoundingValue, MidpointRounding.ToPositiveInfinity);
                    break;
                case RoundingMethod.Down:
                case 2:
                    integerRoundingValue = Math.Round(integerRoundingValue, MidpointRounding.ToNegativeInfinity);
                    break;
            }
            _roundedValue.SetOutput(integerRoundingValue * _roundValueField.GetInput<double>());
        }

        public enum RoundingMethod : int
        {
            Closest,
            Up,
            Down,
        }
    }
}
