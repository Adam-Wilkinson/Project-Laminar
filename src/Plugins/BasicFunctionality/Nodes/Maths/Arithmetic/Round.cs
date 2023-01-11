using System;
using Laminar.PluginFramework.NodeSystem;

namespace BasicFunctionality.Nodes.Maths.Arithmetic;

public class Round : INode
{

    public string NodeName { get; } = "Round";

    public void Evaluate()
    {
        //object testValueToRound = _valueToRound[INodeField.InputKey];
        //double valueToRound = (double)testValueToRound;
        //double roundValueField = _roundValueField.GetInput<double>();
        //double integerRoundingValue = valueToRound / roundValueField;
        //switch (_roundingMethod["display"])
        //{
        //    case RoundingMethod.Closest:
        //    case 0:
        //        integerRoundingValue = Math.Round(integerRoundingValue);
        //        break;
        //    case RoundingMethod.Up:
        //    case 1:
        //        integerRoundingValue = Math.Round(integerRoundingValue, MidpointRounding.ToPositiveInfinity);
        //        break;
        //    case RoundingMethod.Down:
        //    case 2:
        //        integerRoundingValue = Math.Round(integerRoundingValue, MidpointRounding.ToNegativeInfinity);
        //        break;
        //}
        //_roundedValue.SetOutput(integerRoundingValue * _roundValueField.GetInput<double>());
    }

    public enum RoundingMethod : int
    {
        Closest,
        Up,
        Down,
    }
}
