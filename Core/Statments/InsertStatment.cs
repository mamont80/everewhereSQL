using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LayerData;

namespace ParserCore
{
    public class InsertStatment : Statment
    {
        public SelectTable Table;
        public List<Expression> Columns = new List<Expression>();
        public List<Expression> Values = new List<Expression>();
        public SelectExpresion Select;

        public override string ToStr()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("insert into ").Append(Table.ToStr());
            if (Columns.Count > 0)
            {
                sb.Append(" ( ");
                for (int i = 0; i < Columns.Count; i++)
                {
                    var sc = Columns[i];
                    if (i > 0) sb.Append(", ");
                    sb.Append(sc.ToStr());
                }
                sb.Append(")");
            }
            if (Values != null && Values.Count > 0)
            {
                sb.Append(" values(");
                for (int i = 0; i < Values.Count; i++)
                {
                    var sc = Values[i];
                    if (i > 0) sb.Append(", ");
                    sb.Append(sc.ToStr());
                }
                sb.Append(")");
            }
            else
            {
                sb.Append(" ").Append(Select.ToStr());
            }
           
            return sb.ToString();
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            if (builder == null)
            {
                builder = new ExpressionSqlBuilder();
                builder.Driver = Table.Table.DbDriver;
            }


            StringBuilder sb = new StringBuilder();
            sb.Append("insert into ").Append(Table.ToSql(builder));
            if (Columns.Count > 0)
            {
                sb.Append(" ( ");
                for (int i = 0; i < Columns.Count; i++)
                {
                    var sc = Columns[i];
                    if (i > 0) sb.Append(", ");
                    sb.Append(sc.ToSQL(builder));
                }
                sb.Append(")");
            }
            if (Values != null && Values.Count > 0)
            {
                sb.Append(" values(");
                for (int i = 0; i < Values.Count; i++)
                {
                    var sc = Values[i];
                    if (i > 0) sb.Append(", ");
                    sb.Append(sc.ToSQL(builder));
                }
                sb.Append(")");
            }
            else
            {
                sb.Append(" ").Append(Select.ToSQL(builder));
            }

            return sb.ToString();
        }

        public override void Prepare()
        {
            if (Columns != null)
            {
                foreach (var sc in Columns)
                {
                    sc.Prepare();
                }
            }
            if (Values != null)
            {
                foreach (var sc in Values)
                {
                    sc.Prepare();
                }
            }
            if (Select != null) Select.Prepare();

        }

        public override void Optimize()
        {
            if (Columns != null)
            {
                for (int i = 0; i < Columns.Count; i++ )
                {
                    Columns[i] = Columns[i].PrepareAndOptimize();
                }
            }
            if (Values != null)
            {
                for (int i = 0; i < Values.Count; i++)
                {
                    Values[i] = Values[i].PrepareAndOptimize();
                }
            }
            if (Select != null) Select.Prepare();
        }

    }
}
