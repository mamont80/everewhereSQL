using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.Sql
{
    public class ReplacedFieldExpr: Expression
    {
        public override bool IsOperation() { return false; }
        public override bool IsFunction() { return false; }

        public FieldExpr Field;

        protected Expression Child()
        {
            if (Childs == null || ChildsCount() != 1) throw new Exception("Must be one child");
            return Childs[0];
        }

        public override void Prepare()
        {
            base.Prepare();
            var child = Child();
            SetResultType(child.GetResultType());
            GetBoolResultOut = data => { return child.GetBoolResultOut(data); };
            GetIntResultOut = data => { return child.GetIntResultOut(data); };
            GetStrResultOut = data => { return child.GetStrResultOut(data); };
            GetFloatResultOut = data => { return child.GetFloatResultOut(data); };
            GetDateTimeResultOut = data => { return child.GetDateTimeResultOut(data); };
            GetTimeResultOut = data => { return child.GetTimeResultOut(data); };
            GetGeomResultOut = data => { return child.GetGeomResultOut(data); };
            GetBlobResultOut = data => { return child.GetBlobResultOut(data); };
        }

        public override string ToStr()
        {
            return Field.ToStr();
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            return Child().ToSql(builder);
        }
    }
}
