using System;
using System.Collections.Generic;
using ParserCore;

namespace ParserCore
{
    public class ExpressionFactoryTable : BaseExpressionFactory
    {
        public override Expression GetNode(LexExpr lex, bool uniar, LexemCollection collection)
        {
            Expression res = null;
            if (lex.LexemType == LexType.Command)
            {
                string lowerLexem = lex.Lexem.ToLower();
                switch (lowerLexem)
                {
                    case "count":
                        res = new CountExpr();
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
                    case "select":
                        res = new SelectExpresion();
                        ((SelectExpresion) res).Query.Driver = collection.TableGetter.GetDefaultDriver();
                        break;
                }
            }
            if (res != null) return res;
            res = base.GetNode(lex, uniar, collection);
            
            if (res == null && (lex.LexemType == LexType.Text || lex.LexemType == LexType.Command))
            {
                string[] names = BaseExpressionFactory.ParseStringQuote(lex.Lexem);
                res = AddFiled(names, lex);
            }
            return res;
        }

        public Expression AddFiled(string[] names, LexExpr lexem)
        {
            string fieldAlias;
            string tableAlias;
            if (names == null || names.Length == 0) throw new Exception("Column name not found");
            if (names.Length >= 2)
            {
                fieldAlias = names[names.Length - 1];
                tableAlias = names[names.Length - 2];
            }
            else
            {
                tableAlias = null;
                fieldAlias = names[0];
            }
            return AddFiled(fieldAlias, tableAlias,lexem);
        }

        public Expression AddFiled(string fieldAlias, string tableAlias, LexExpr lexem)
        {
            if (fieldAlias == "*")
            {
                AllColumnExpr a = new AllColumnExpr();
                a.Prefix = tableAlias;
                return a;
            }
            FieldCapExpr res = new FieldCapExpr();
            res.FieldAlias = fieldAlias;
            res.TableAlias = tableAlias;
            res.Lexem = lexem;
            return res;
        }

    }

}
