using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.CMD
{
    public abstract class CustomCmd: Expression
    {
        public override bool IsFunction() { return false; }
        public override bool IsOperation() { return false; }
        protected override bool CanCalcOnline() { return false; }
    }
}
