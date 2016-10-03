using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.Simple
{
    public abstract class FuncExpr_WithoutOperand : FuncExpr
    {
        public override int NumChilds() { return 0; }
        public override int Priority() { return PriorityConst.Default; }
    }

    public abstract class FuncExpr_OneOperand : FuncExpr
    {
        public Expression Operand
        {
            get
            {
                if (Childs == null) return null;
                return Childs[0];
            }
        }
        public override void Prepare()
        {
            base.Prepare();
            if (Childs == null || Childs.Count != 1) throw new Exception("Wrong number operands");
        }
        public override int NumChilds() { return 1; }
        public override int Priority() { return PriorityConst.Default; }
    }

    public abstract class FuncExpr_TwoOperand : FuncExpr
    {
        public Expression Operand1
        {
            get
            {
                if (Childs == null) return null;
                return Childs[0];
            }
        }
        public Expression Operand2
        {
            get
            {
                if (Childs == null) return null;
                return Childs[1];
            }
        }
        public override void Prepare()
        {
            base.Prepare();
            if (Childs == null || Childs.Count != 2) throw new Exception("Wrong number operands");
        }
        public override int NumChilds() { return 2; }
    }

    public abstract class Custom_TwoOperand : Expression
    {
        public Expression Operand1 {
            get { if (Childs == null) return null;
                return Childs[0]; }
        }

        public Expression Operand2
        {
            get { if (Childs == null) return null; 
                return Childs[1]; }
        }
        public override void Prepare()
        {
            base.Prepare();
            if (Childs == null || Childs.Count != 2) throw new Exception("Wrong number operands");
        }
        public override int NumChilds() { return 2; }
    }

    public abstract class Custom_OneOperand : Expression
    {
        public Expression Operand;
        public override void Prepare()
        {
            base.Prepare();
            if (Childs == null || Childs.Count != 1) throw new Exception("Wrong number operands");
            Operand = Childs[0];
        }
        public override int NumChilds() { return 1; }
    }
}
