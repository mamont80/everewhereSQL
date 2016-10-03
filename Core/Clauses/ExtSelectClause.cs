using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore.Expr.Sql;

namespace ParserCore
{
    public enum SelectOperation
    {
        Union,
        UnionAll,
        Intersect,
        Except
    }

    public class ExtSelectClause: SqlToken
    {
        public SelectOperation Operation;
        public SelectExpresion Select;

        public static string SelectOperationToString(SelectOperation se)
        {
            switch (se)
            {
                case SelectOperation.Except:
                    return "EXCEPT";
                case SelectOperation.Intersect:
                    return "INTERSECT";
                case SelectOperation.Union:
                    return "UNION";
                case SelectOperation.UnionAll:
                    return "UNION ALL";
            }
            throw new Exception("Unknow select operation");
        }

        public override string ToStr()
        {
            return " " + SelectOperationToString(Operation) + " " + Select.ToStr();
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            return " " + SelectOperationToString(Operation) + " " + Select.ToSql(builder);
        }

        public override void Prepare()
        {
            Select.Prepare();
        }

        public override IExplore Expolore(DelegateExpessionExplorer del)
        {
            Select = (SelectExpresion)Select.Expolore(del);
            return base.Expolore(del);
        }

    }
}
