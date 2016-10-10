using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore.Expr.Extend;
using ParserCore.Expr.Simple;

namespace ParserCore
{
    public class NullableFunctions: IExpressionFactory
    {
        public Expression GetNode(ExpressionParser parser)
        {
            Lexem lex = parser.Collection.CurrentLexem();
            Expression ex = null;
            Lexem n1;
            if (lex.LexemType == LexType.Command)
            {
                if (parser.Collection.GetNext() != null && parser.Collection.GetNext().IsSkobraOpen())
                {
                    switch (lex.LexemText.ToLower())
                    {
                        case "coalesce":
                            ex = new Coalesce_FuncExpr();
                            break;
                    }
                }
                switch (lex.LexemText.ToLower())
                {
                    case "null":
                        ex = new NullConstExpr();
                        break;
                    case "is":
                        n1 = parser.Collection.GetNext();
                        if (n1 != null && n1.LexemType == LexType.Command && n1.LexemText.ToLower() == "not")
                        {
                            ex = new IsNotNullExpr();
                            parser.Collection.GotoNext();
                            break;
                        }
                        ex = new IsExpr();
                        break;
                    case "isnull":
                        ex = new IsNullExpr();
                        break;
                    case "notnull":
                    case "isnotnull":
                        ex = new IsNotNullExpr();
                        break;
                }
            }
            return ex;
        }


    }
}
