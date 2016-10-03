using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public class CustomMultiFactory : IExpressionFactory
    {
        public List<IExpressionFactory> Factories = new List<IExpressionFactory>();

        public Expression GetNode(ExpressionParser parser)
        {
            foreach (var f in Factories)
            {
                var r = f.GetNode(parser);
                if (r != null) return r;
            }
            return null;
        }
    }
}
