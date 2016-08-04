using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LayerData;

namespace ParserCore
{
    public class UpdateStatment : Statment
    {
        public SelectTable Table;
        public List<SetClause> Set = new List<SetClause>();
        public Expression Where;

        public override string ToStr()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("update ").Append(Table.ToStr());
            if (Set.Count > 0)
            {
                sb.Append(" SET ");
                for (int i = 0; i < Set.Count; i++)
                {
                    var sc = Set[i];
                    if (i > 0) sb.Append(", ");
                    sb.Append(sc.Column.ToStr());
                    sb.Append("=");
                    sb.Append(sc.Value.ToStr());
                }
            }
            if (Where != null) sb.Append(" where ").Append(Where.ToStr());
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
            sb.Append("update ").Append(Table.ToSql(builder));
            if (Set.Count > 0)
            {
                sb.Append(" SET ");
                for (int i = 0; i < Set.Count; i++)
                {
                    var sc = Set[i];
                    if (i > 0) sb.Append(", ");
                    sb.Append(sc.Column.ToSQL(builder));
                    sb.Append("=");
                    sb.Append(sc.Value.ToSQL(builder));
                }
            }
            if (Where != null) sb.Append(" where ").Append(Where.ToSQL(builder));
            return sb.ToString();
        }

        public override void Prepare()
        {
            for (int i = 0; i < Set.Count; i++ )
            {
                Set[i].Column = Set[i].Column.PrepareAndOptimize();
                Set[i].Value = Set[i].Value.PrepareAndOptimize();
            }
            if (Where != null) Where.Prepare();
        }

        public override void Optimize()
        {
            foreach (var sc in Set)
            {
                sc.Value = sc.Value.PrepareAndOptimize();
            }
            if (Where != null) Where = Where.PrepareAndOptimize();
        }
    }
}
