using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public class SqlFactoryComplite: CustomMultiFactory
    {
        public SqlFactoryComplite() : base()
        {
            Factories.Add(new VariableFactory());
            Factories.Add(new SqlOnlyFactory());//
            Factories.Add(new ExtendFunctions());
            Factories.Add(new NullableFunctions());//
            Factories.Add(new SimpleExpressionFactory());
            Factories.Add(new CommandAsColumn());
        }
    }
}
