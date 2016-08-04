using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public class DeleteStatment :Statment
    {
        public SelectTable Table;
        public Expression Where;

        public override string ToStr()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("delete from ").Append(Table.ToStr());
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
            sb.Append("delete from ").Append(Table.ToSql(builder));
            if (Where != null) sb.Append(" where ").Append(Where.ToSQL(builder));
            return sb.ToString();
        }

        public override void Prepare()
        {
            if (Where != null) Where.Prepare();
        }

        public override void Optimize()
        {
            if (Where != null) Where = Where.PrepareAndOptimize();
        }

    }
}
