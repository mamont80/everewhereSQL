﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public class DeleteStatement : OneTableStatement
    {
        private Expression _Where;
        public Expression Where
        {
            get { return _Where; }
            set
            {
                _Where = value;
                if (value != null) value.ParentToken = this;
            }
        }


        public override IExplore Expolore(DelegateExpessionExplorer del)
        {
            if (Where != null) Where = (Expression)Where.Expolore(del);
            return base.Expolore(del);
        }

        public override string ToStr()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("delete from ").Append(TableClause.ToStr());
            if (Where != null) sb.Append(" where ").Append(Where.ToStr());
            AddReturningToStr(sb);
            return sb.ToString();
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("delete ");
            if (builder.DbType == DriverType.SqlServer)
            {
                if (!string.IsNullOrEmpty(TableClause.Alias)) sb.Append(builder.EncodeTable(TableClause.Alias)).Append(" ");
            }
            sb.Append("from ");
            sb.Append(TableClause.ToSql(builder)).Append(" ");
            AddReturningToSql1(builder, sb);
            if (Where != null) sb.Append(" where ").Append(Where.ToSql(builder));
            AddReturningToSql2(builder, sb);
            return sb.ToString();
        }

        public override void Prepare()
        {
            base.Prepare();
            if (Where != null)
            {
                Where.Prepare();
                ExpUtils.CheckWhere(Where);
            }
        }

        public override void ParseInside(ExpressionParser parser)
        {
            var collection = parser.Collection;
            var lex = collection.CurrentLexem();
            if (lex.LexemText.ToLower() != "delete") throw new Exception("Not DELETE statment");
            lex = collection.GotoNextMust();
            if (lex.LexemText.ToLower() != "from") throw new Exception("Keyword 'FROM' is not found");
            lex = collection.GotoNextMust();

            FromParser fc = new FromParser();
            fc.Parse(collection);
            if (fc.Tables.Count > 1) collection.Error("Multi tables in update", collection.CurrentLexem());
            if (fc.Tables.Count == 0) collection.Error("Not table clause", collection.CurrentLexem());
            TableClause = fc.Tables[0];

            //string[] tablename = CommonParserFunc.ReadTableName(collection);
            // TODO: fixed! ok
            //TableClause = TableClause.CreateByTable(tablename, collection.TableGetter.GetTableByName(tablename));

            lex = collection.CurrentLexem();

            if (lex == null) return;
            if (lex.LexemText.ToLower() == "where")
            {
                collection.GotoNextMust();
                ExpressionParser e = new ExpressionParser(collection);
                e.Parse();
                Where = e.Single();
            }
            ParseReturining(parser);
        }
    }
}
