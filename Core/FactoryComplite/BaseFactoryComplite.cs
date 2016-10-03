using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public class BaseFactoryComplite: CustomMultiFactory
    {
        public BaseFactoryComplite() : base()
        {
            Factories.Add(new ExtendFunctions());
            Factories.Add(new SimpleExpressionFactory());
        }
    }
}
