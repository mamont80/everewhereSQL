using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.CMD
{
    public struct TableName
    {
        public string Name;
        public string Schema;

        public string ToStr()
        {
            string r = "";
            if (!string.IsNullOrEmpty(Schema)) r = ParserUtils.TableToStrEscape(Schema)+".";
            r += ParserUtils.TableToStrEscape(Name);
            return r;
        }

        public string ToSql(ExpressionSqlBuilder builder, bool nameOnly = false)
        {
            string r = "";
            if (!nameOnly && !string.IsNullOrEmpty(Schema)) r = builder.EncodeTable(Schema) + ".";
            r += builder.EncodeTable(Name);
            return r;
        }

        public static TableName Parse(ExpressionParser parser)
        {
            string[] strTables = CommonParserFunc.ReadTableName(parser.Collection);
            TableName tn = new TableName();
            if (strTables.Length < 1 || strTables.Length > 2) parser.Collection.ErrorUnexpected(parser.Collection.CurrentOrLast());
            if (strTables.Length > 1)
            {
                tn.Schema = strTables[0];
                tn.Name = strTables[1];
            }
            else tn.Name = strTables[0];
            return tn;
        }
    }
}
