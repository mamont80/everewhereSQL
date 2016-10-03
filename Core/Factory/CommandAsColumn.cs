using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public class CommandAsColumn: IExpressionFactory
    {
        public Expression GetNode(ExpressionParser parser)
        {
            Lexem lex = parser.Collection.CurrentLexem();
            Expression res = null;
            if (lex.LexemType == LexType.Text || lex.LexemType == LexType.Command)
            {
                string[] names = ParserUtils.ParseStringQuote(lex.LexemText);
                res = SqlOnlyFactory.AddFiled(names, lex, parser);
            }
            return res;
        }
    }
}
