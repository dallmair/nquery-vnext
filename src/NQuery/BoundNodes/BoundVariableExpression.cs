using System;

using NQuery.Symbols;

namespace NQuery.BoundNodes
{
    internal sealed class BoundVariableExpression :  BoundExpression
    {
        private readonly VariableSymbol _variableSymbol;

        public BoundVariableExpression(VariableSymbol variableSymbol)
        {
            _variableSymbol = variableSymbol;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.VariableExpression; }
        }

        public override Type Type
        {
            get { return _variableSymbol.Type; }
        }

        public VariableSymbol Symbol
        {
            get { return _variableSymbol; }
        }

        public override string ToString()
        {
            return string.Format("@{0}", _variableSymbol.Name);
        }
    }
}