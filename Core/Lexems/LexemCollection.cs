using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public class LexemCollection : List<Lexem>
    {
        public LexemCollection()
        {
        }

        public int IndexLexem = 0;
        public IExpressionFactory NodeFactory = new BaseFactoryComplite();
        public IDictionary<string, object> Variables;
        public string OriginalExpressionString;
        public ITableGetter TableGetter;

        public Lexem GotoNext()
        {
            IndexLexem++;
            if (IndexLexem >= Count) return null;
            return CurrentLexem();
        }

        public Lexem GotoNextMust()
        {
            IndexLexem++;
            if (IndexLexem >= Count) UnexceptEndError();
            return CurrentLexem();
        }

        public Lexem CurrentOrLast()
        {
            if (CurrentLexem() != null) return CurrentLexem();
            return GetLast();
        }

        public Lexem GetLast()
        {
            if (Count > 0) return this[Count - 1];
            return null;
        }

        public Lexem Get(int index)
        {
            int i = index;
            if (i >= 0 && i < Count) return this[i];
            return null;
        }

        public Lexem GetPrev()
        {
            int i = IndexLexem - 1;
            if (i >= 0 && Count > 0) return this[i];
            else return null;
        }

        public Lexem GetNext()
        {
            int i = IndexLexem + 1;
            if (i < Count && Count > 0) return this[i];
            else return null;
        }

        public bool IsEnd()
        {
            return (IndexLexem >= Count);
        }

        public Lexem CurrentLexem()
        {
            if (IndexLexem >= Count) return null;
            return this[IndexLexem];
        }

        public Lexem SeekLexem(int delta)
        {
            int i = IndexLexem + delta;
            if (i < 0 || i >= Count) return null;
            return this[i];
        }

        public void Error(string message, Lexem lex)
        {
            if (lex == null) throw new Exception(message);
            else
                throw new Exception(message + " " + lex.LexemText + " Position row: " + lex.RowNum.ToString() + ", col: " + lex.ColNum.ToString());
        }

        public void ErrorUnexpected(Lexem lex)
        {
            Error("unexpected ending", lex);
        }

        public void ErrorWaitKeyWord(string keyword, Lexem lex)
        {
            Error("Not found keyword "+keyword, lex);
        }

        public void UnexceptEndError()
        {
            Error("Unexcepted end statment", GetLast());
        }
    }
}
