using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore;
using ParserCore.Expr.Simple;

namespace ParserCore.Expr.Aggregate
{
    public class UnionAggregateExpr : FuncExpr_OneOperand
    {
        public override void Prepare()
        {
            base.Prepare();
            if (Operand.GetResultType() != SimpleTypes.Geometry) TypesException();
            SetResultType(SimpleTypes.Geometry);
            GetGeomResultOut = GetGeomResultOut2;
        }

        private object GetGeomResultOut2(object data)
        {
            throw new Exception("Can not on fly calculation");
        }

        protected override bool CanCalcOnline() { return false; }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            if (builder.DbType == DriverType.SqlServer)
                return " UnionAggregate(" + Operand.ToSql(builder) + ")";
            if (builder.DbType == DriverType.PostgreSQL)
                return " ST_UNION(" + Operand.ToSql(builder) + ")";
            return ToSqlException();
        }

        public override string ToStr()
        {
            return "UnionAggregate(" + Operand.ToStr() + ")";
        }
    }
}
