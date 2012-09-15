using System.Collections.Generic;
using NQuery.Language.Runtime;
using NQuery.Language.Symbols;

namespace NQuery.Language
{
    public sealed class DataContextBuilder
    {
        public DataContextBuilder()
        {
            Tables = new List<TableSymbol>();
            Functions = new List<FunctionSymbol>(BuiltInFunctions.GetFunctions());
            Aggregates = new List<AggregateSymbol>(BuiltInAggregates.GetAggregates());
            Variables = new List<VariableSymbol>();
            PropertyProviders = new TypeRegistry<IPropertyProvider>();
            MethodProviders = new TypeRegistry<IMethodProvider>();
        }

        public DataContextBuilder(DataContext dataContext)
        {
            Tables = new List<TableSymbol>(dataContext.Tables);
            Functions = new List<FunctionSymbol>(dataContext.Functions);
            Aggregates = new List<AggregateSymbol>(dataContext.Aggregates);
            Variables = new List<VariableSymbol>(dataContext.Variables);
            PropertyProviders = new TypeRegistry<IPropertyProvider>(dataContext.PropertyProviders);
            MethodProviders = new TypeRegistry<IMethodProvider>(dataContext.MethodProviders);
        }

        public DataContext GetResult()
        {
            return new DataContext(Tables.ToArray(),
                                   Functions.ToArray(),
                                   Aggregates.ToArray(),
                                   Variables.ToArray(),
                                   new TypeRegistry<IPropertyProvider>(PropertyProviders), 
                                   new TypeRegistry<IMethodProvider>(MethodProviders));
        }

        public List<TableSymbol> Tables { get; private set; }
        public List<FunctionSymbol> Functions { get; private set; }
        public List<AggregateSymbol> Aggregates { get; private set; }
        public List<VariableSymbol> Variables { get; private set; }
        public TypeRegistry<IPropertyProvider> PropertyProviders { get; private set; }
        public TypeRegistry<IMethodProvider> MethodProviders { get; private set; }
    }
}