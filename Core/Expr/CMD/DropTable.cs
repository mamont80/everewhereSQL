using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.CMD
{
    public class DropTable : CustomCmd
    {
        public TableName Table { get; set; }

        public bool IfExists = false;

        public override string ToStr()
        {
            string s = "DROP TABLE ";
            if (IfExists) s += "IF EXISTS ";
            s += Table.ToStr();
            return s;
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            string s = "";
            if (builder.DbType == DriverType.SqlServer)
            {
                if (IfExists)
                {
                    s = string.Format("IF OBJECT_ID({0}, 'U') IS NOT NULL DROP TABLE {1}", builder.SqlConstant(Table.ToSql(builder)), Table.ToSql(builder));
                }
                else s = "DROP TABLE " + Table.ToSql(builder);
            }
            if (builder.DbType == DriverType.PostgreSQL)
            {
                s = "DROP TABLE ";
                if (IfExists) s += "IF EXISTS ";
                s += Table.ToSql(builder);
            }
            return s;
        }

        public override void ParseInside(ExpressionParser parser)
        {
            var collection = parser.Collection;
            if (!ParserUtils.ParseCommandPhrase(collection, "drop table")) collection.ErrorWaitKeyWord("drop table", collection.CurrentOrLast());
            collection.GotoNextMust();
            if (ParserUtils.ParseCommandPhrase(collection, "if exists"))
            {
                IfExists = true;
                collection.GotoNextMust();
            }
            Table = TableName.Parse(parser);
            collection.GotoNext();
        }
    }
}
