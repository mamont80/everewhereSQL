using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public class LexemCollection : List<Lexem>
    {
        internal LexemCollection(string originalText)
        {
            OriginalExpressionString = originalText;
        }

        public int IndexLexem = 0;
        public IExpressionFactory NodeFactory = new BaseFactoryComplite();
        public IDictionary<string, object> Variables;
        public string OriginalExpressionString;
        public ITableGetter TableGetter;
        public bool TableGetterUseCache = true;
        public List<ParamDeclaration> ParamDeclarations = new List<ParamDeclaration>();

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
                throw new Exception(message + " " + lex.LexemText + " Position row: " + lex.RowNum.ToString() + ", col: " + lex.ColNum.ToString()+"\r\n" + GetErrorPosition(lex));
        }

        private string GetErrorPosition(Lexem lex)
        {
            if (lex == null) return "";
            var idx = IndexOf(lex);
            int col = 0;
            int row = 0;
            int i = 0;
            while (i < OriginalExpressionString.Length)
            {
                char c = OriginalExpressionString[i];
                if (c == '\n')
                {
                    row++;
                    col = 0;
                }
                else col++;
                if (row > lex.RowNum) break;
                if (row == lex.RowNum && col >= lex.ColNum) break;
                i++;
            }
            int start = i - 50;
            if (start < 0) start = 0;
            int end = i + 10;

            if (end > OriginalExpressionString.Length) end = OriginalExpressionString.Length;

            string s = OriginalExpressionString.Substring(start, i - start + 1);
            s = s + "< error >";
            s = s + OriginalExpressionString.Substring(i+1, end - i - 1);
            if (start != 0) s = "..." + s;
            if (end >= (OriginalExpressionString.Length)) s = s + "...";
            return s;
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

        public virtual SimpleTypes? DotNetTypeToSimpleType(object value)
        {
            if (value == null) return null;
            var type = value.GetType();
            if (type.IsPrimitive)//The primitive types are Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, IntPtr, UIntPtr, Char, Double, and Single.
            {
                if (type == typeof(bool)) return SimpleTypes.Boolean;
                if (type == typeof(Char)) return SimpleTypes.String;
                if (type == typeof(double)) return SimpleTypes.Float;
                if (type == typeof(Single)) return SimpleTypes.Float;

                return SimpleTypes.Integer;
            }
            if (type == typeof(string)) return SimpleTypes.String;
            if (type == typeof(Single)) return SimpleTypes.Float;
            if (type == typeof(decimal)) return SimpleTypes.Float;
            if (type == typeof(DateTime)) return SimpleTypes.DateTime;
            if (type == typeof(TimeSpan)) return SimpleTypes.Time;
            //if (type == typeof(OSGeo.OGR.Geometry)) return SimpleTypes.Geometry;
            return null;
        }
    }
}
