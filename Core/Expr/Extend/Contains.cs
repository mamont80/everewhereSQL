using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore.Expr.Simple;

namespace ParserCore.Expr.Extend
{
    public class Contains : Custom_TwoOperand
    {
        public override int Priority()
        {
            return PriorityConst.Like;
        }

        public override void Prepare()
        {
            base.Prepare();
            if (!(Operand1.GetResultType() == SimpleTypes.String && Operand2.GetResultType() == SimpleTypes.String)) TypesException();
            //if (!Operand2.OnlyOnline()) OperandOnlyConstException(2);
            SetResultType(SimpleTypes.Boolean);
            GetBoolResultOut = CalcAsBool;
        }
        private bool CalcAsBool(object data) { return Operand1.GetStrResultOut(data).Contains(Operand2.GetStrResultOut(data)); }

        public override string ToStr() { return "(" + Operand1.ToStr() + " contains " + Operand2.ToStr() + ")"; }
        public override string ToSql(ExpressionSqlBuilder builder)
        {
            //return ((IDriverDatabaseGeomixer)builder.Driver).BuildExpContains(Operand2.ToSQL(builder), Operand1.ToSQL(builder));
            if (builder.DbType == DriverType.SqlServer)
            {
                return string.Format("(CHARINDEX({0},{1})>0)", Operand2.ToSql(builder), Operand1.ToSql(builder));
            }
            if (builder.DbType == DriverType.PostgreSQL)
            {
                return string.Format("(position({0} in {1})>0)", Operand2.ToSql(builder), Operand1.ToSql(builder));
            }
            return ToSqlException();
        }
    }
}
