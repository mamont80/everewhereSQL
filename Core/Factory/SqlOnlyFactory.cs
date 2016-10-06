using System;
using System.Collections.Generic;
using ParserCore.Expr.CMD;
using ParserCore.Expr.Sql;
using ParserCore;
using ParserCore.Expr.Aggregate;

namespace ParserCore
{
    public class SqlOnlyFactory : IExpressionFactory
    {
        public Expression GetNode(ExpressionParser parser)
        {
            Lexem lex = parser.Collection.CurrentLexem();
            Expression res = null;
            if (lex.LexemType == LexType.Command)
            {
                string lowerLexem = lex.LexemText.ToLower();
                if (ParserUtils.ParseCommandPhrase(parser.Collection, "create view", false, false))
                {
                    res = new CreateView();
                }
                if (ParserUtils.ParseCommandPhrase(parser.Collection, "create table", false, false))
                {
                    res = new CreateTable();
                }
                if (res == null && ParserUtils.ParseCommandPhrase(parser.Collection, "alter table", false, false))
                {
                    res = new AlterTable();
                }
                if (res == null && ParserUtils.ParseCommandPhrase(parser.Collection, "drop table", false, false))
                {
                    res = new DropTable();
                }
                if (res == null && ParserUtils.ParseCommandPhrase(parser.Collection, "drop index", false, false))
                {
                    res = new DropIndex();
                }
                if (res == null && (ParserUtils.ParseCommandPhrase(parser.Collection, "create unique index", false, false) ||
                    ParserUtils.ParseCommandPhrase(parser.Collection, "create index", false, false)))
                {
                    res = new CreateIndex();
                }
                if (res == null)
                {
                    switch (lowerLexem)
                    {
                        case "between"://не функция
                            res = new Between();
                            break;
                        case "count":
                            res = new CountExpr();
                            break;
                        case "unionaggregate":
                            res = new UnionAggregateExpr();
                            break;
                        case "sum":
                            res = new SumExpr();
                            break;
                        case "min":
                            res = new MinExpr();
                            break;
                        case "max":
                            res = new MaxExpr();
                            break;
                        case "avg":
                            res = new AvgExpr();
                            break;
                        case "lastinsertrowid":
                            res = new LastInsertRowidExpr();
                            break;
                        case "exists":
                            res = new ExistsExpr();
                            break;
                        case "any":
                            res = new AnyExpr();
                            break;
                        case "select":
                            res = new SelectExpresion();
                            break;
                        case "update":
                            res = new UpdateStatement();
                            break;
                        case "insert":
                            res = new InsertStatement();
                            break;
                        case "delete":
                            res = new DeleteStatement();
                            break;
                    }
                }
            }
            if (res != null) return res;
            return res;
        }

        public static Expression AddFiled(string[] names, Lexem lexem, ExpressionParser parser)
        {
            string fieldAlias = null;
            string tableAlias = null;
            string schemaAlias = null;
            if (names == null || names.Length == 0) throw new Exception("Column name not found");
            if (names.Length >= 2)
            {
                fieldAlias = names[names.Length - 1];
                tableAlias = names[names.Length - 2];
                if (names.Length == 3) schemaAlias = names[names.Length - 3];
                if (names.Length > 3) parser.Collection.Error("Composit alias", lexem);
            }
            else
            {
                tableAlias = null;
                fieldAlias = names[0];
            }
            return AddFiled(fieldAlias, tableAlias, schemaAlias, lexem, parser);
        }

        public static Expression AddFiled(string fieldAlias, string tableAlias, string schema, Lexem lexem,ExpressionParser parser)
        {
            if (fieldAlias == "*")
            {
                if (schema != null) parser.Collection.Error("Can not use schema in * expression", lexem);
                AllColumnExpr a = new AllColumnExpr();
                a.Prefix = tableAlias;
                return a;
            }
            FieldExpr res = new FieldExpr();
            res.FieldName = fieldAlias;
            res.TableAlias = tableAlias;
            res.Schema = schema;
            res.Lexem = lexem;
            return res;
        }

    }

}
