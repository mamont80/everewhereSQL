using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public interface IExpressionFactory
    {
        Expression GetNode(ExpressionParser parser);
    }
}
