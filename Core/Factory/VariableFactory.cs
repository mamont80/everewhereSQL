using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using ParserCore.Expr.Simple;
using ParserCore.Expr.Sql;

namespace ParserCore
{
    public class VariableFactory : IExpressionFactory
    {
        public Expression GetNode(ExpressionParser parser)
        {
            var collection = parser.Collection;
            Lexem lex = parser.Collection.CurrentLexem();
            bool uniar = parser.waitValue;
            Expression res = null;
            if (lex.LexemType == LexType.Command)
            {
                if (lex.LexemText.StartsWith("@"))
                {
                    var varName = lex.LexemText;
                    for (int i = 0; i < collection.ParamDeclarations.Count; i++)
                    {
                        var pd = collection.ParamDeclarations[i];
                        if (pd.Name == varName)
                        {
                            var tp = collection.DotNetTypeToSimpleType(pd.Value);
                            if (tp == null) collection.Error("Unknow variable type (" + pd.Name + ")", lex);
                            res = new VariableExpr();
                            ((VariableExpr)res).VariableName = lex.LexemText;
                            ((VariableExpr)res).Bind(pd.Value, tp.Value);
                        }
                    }
                }
            }
            return res;
        }
    }
}
