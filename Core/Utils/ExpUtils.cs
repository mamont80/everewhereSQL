using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore.Expr.Sql;
using ParserCore.Expr.Simple;

namespace ParserCore
{
    //утилиты с привязкой к геомиксеру
    public static class ExpUtils
    {
        public static void CheckWhere(Expression WhereExpr)
        {
            if (WhereExpr.GetResultType() != SimpleTypes.Boolean) throw new Exception(String.Format("argument of WHERE must be type boolean, not type " + WhereExpr.GetResultType().ToString()));
        }

        public static ITableDesc[] ExtractTables(ISqlConvertible token)
        {
            List<ITableDesc> res = new List<ITableDesc>();
            token.Expolore(a =>
                {
                    if ((a is ITableDesc) && !(a is ISelect)) res.Add((ITableDesc) a);
                    return a;
                });
            return res.ToArray();
        }

        public static Column[] GetColumnInfo(CustomStatement select)
        {
            List<Column> res = new List<Column>();
            var ccList = @select.GetAllColumns();

            foreach (var cs in ccList)
            {
                string nm = GetVisibleColumnName(cs);
                Column ci = new Column(nm, cs.ColumnExpression.GetResultType());
                res.Add(ci);
            }
            return res.ToArray();
        }

        public static string GetVisibleColumnName(ColumnClause cs)
        {
            string nm = cs.Alias;
            if (String.IsNullOrEmpty(nm))
            {
                if (cs.ColumnExpression is ReplacedFieldExpr)
                {
                    nm = ((ReplacedFieldExpr) cs.ColumnExpression).Field.FieldName;
                }else
                if (cs.ColumnExpression is FieldExpr)
                {
                    nm = ((FieldExpr)cs.ColumnExpression).FieldName;
                }
                else nm = cs.ColumnExpression.ToStr();
            }
            return nm;
        }

        public static void OptimizeChilds(this ISqlConvertible expr)
        {
            expr.Expolore(e =>
                {
                    if (e != null && e is Expression)
                    {
                        Expression e2 = (Expression) e;
                        return e2.Optimize();
                    }
                    return e;
                });
        }
    }
}
