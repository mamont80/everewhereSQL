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

        public static Column[] GetColumnInfo(SelectExpresion select)
        {
            List<Column> res = new List<Column>();
            foreach (var cs in select.Columns)
            {
                if (cs.ColumnExpression is AllColumnExpr)
                {
                    AllColumnExpr all = cs.ColumnExpression as AllColumnExpr;
                    foreach (var column in all.Columns)
                    {
                        string nm2 = GetVisibleColumnName(column);
                        Column ci2 = new Column(nm2, column.ColumnExpression.GetResultType());
                        res.Add(ci2);
                    }
                }
                else
                {
                    string nm = GetVisibleColumnName(cs);
                    Column ci = new Column(nm, cs.ColumnExpression.GetResultType());
                    res.Add(ci);
                }
            }
            return res.ToArray();
        }

        public static string GetVisibleColumnName(ColumnClause cs)
        {
            string nm = cs.Alias;
            if (string.IsNullOrEmpty(nm))
            {
                if (cs.ColumnExpression is FieldExpr)
                {
                    nm = ((FieldExpr)cs.ColumnExpression).FieldName;
                }
                else nm = cs.ColumnExpression.ToStr();
            }
            return nm;
        }

        public static void BindVariables(this ISqlConvertible token, IDictionary<string, object> variables)
        {
            token.Expolore(exp =>
                {
                    if (exp != null && exp is VariableExpr)
                    {
                        VariableExpr ve = (VariableExpr) exp;
                        if (variables.ContainsKey(ve.VariableName))
                        {
                            ve.Bind(variables[ve.VariableName]);
                        }
                    }
                    return exp;
                });
        }
    }
}
