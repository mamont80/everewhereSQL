using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.CMD
{
    public class DropIndex : CustomCmd
    {
        public TableName IndexName { get; set; }

        public bool IfExists = false;

        public override string ToStr()
        {
            string s = "DROP INDEX ";
            if (IfExists) s += "IF EXISTS ";
            s += IndexName.ToStr();
            return s;
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            string s = "";
            if (builder.DbType == DriverType.SqlServer)
            {
                if (IfExists)
                {
                    s = string.Format("IF EXISTS (SELECT name FROM sysindexes WHERE name = {0}) DROP INDEX {1}", 
                        builder.SqlConstant(IndexName.ToSql(builder, true)),
                        IndexName.ToSql(builder));
                }
                else s = "DROP INDEX " + IndexName.ToSql(builder, true);
            }
            if (builder.DbType == DriverType.PostgreSQL)
            {
                s = "DROP INDEX ";
                if (IfExists) s += "IF EXISTS ";
                s += IndexName.ToSql(builder);
            }
            return s;
        }

        public override void ParseInside(ExpressionParser parser)
        {
            var collection = parser.Collection;
            if (!ParserUtils.ParseCommandPhrase(collection, "drop index")) collection.ErrorWaitKeyWord("drop index", collection.CurrentOrLast());
            collection.GotoNextMust();
            if (ParserUtils.ParseCommandPhrase(collection, "if exists"))
            {
                IfExists = true;
                collection.GotoNextMust();
            }
            IndexName = TableName.Parse(parser);
            collection.GotoNext();
        }

    }
}
