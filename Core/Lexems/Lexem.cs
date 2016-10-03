using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public class Lexem
    {
        public string LexemText;
        public LexType LexemType;
        public int ColNum = -1;
        public int RowNum = -1;

        public Expression Expr;
        public int Prior;

        public bool IsSkobraClose()
        {
            return LexemType == LexType.Skobka && LexemText == ")";
        }
        public bool IsSkobraOpen()
        {
            return LexemType == LexType.Skobka && LexemText == "(";
        }
        public override string ToString()
        {
            if (LexemText != null) return LexemText +" ("+LexemType.ToString()+")";
            else return "(" + LexemType.ToString() + ")";
        }
    }
}
