using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public static class CommonFunc
    {
        public static string[] ReadTableName(LexemCollection collection)
        {
            if (collection.CurrentLexem() == null) throw new Exception("Alias not found");
            if (collection.CurrentLexem().LexemType == LexType.Command)
            {
                var r = collection.CurrentLexem().Lexem;
                return BaseExpressionFactory.ParseStringQuote(r);
            }
            if (collection.CurrentLexem().LexemType == LexType.Text)
            {
                var strs = BaseExpressionFactory.ParseStringQuote(collection.CurrentLexem().Lexem);
                return strs;
            }
            collection.Error("Alias not found", collection.CurrentLexem());
            return null;
        }

        public static string ReadAlias(LexemCollection collection)
        {
            if (collection.CurrentLexem() == null) throw new Exception("Alias not found");
            string[] strs = null;
            if (collection.CurrentLexem().LexemType == LexType.Command)
            {
                var r = collection.CurrentLexem().Lexem;
                strs = BaseExpressionFactory.ParseStringQuote(r);
            }else
            if (collection.CurrentLexem().LexemType == LexType.Text)
            {
                strs = BaseExpressionFactory.ParseStringQuote(collection.CurrentLexem().Lexem);
            }
            if (strs != null)
            {
                if (strs.Length > 1) collection.Error("Composite alias", collection.CurrentLexem());
            }
            collection.Error("Alias not found", collection.CurrentLexem());
            return null;
        }

        public static Expression ReadColumn(LexemCollection collection)
        {
            if (collection.CurrentLexem() == null) throw new Exception("Alias not found");
            if (collection.CurrentLexem().LexemType == LexType.Command)
            {
                var r = collection.CurrentLexem().Lexem;
                FieldCapExpr fc = new FieldCapExpr();
                fc.FieldAlias = collection.CurrentLexem().Lexem;
                return fc;
            }
            if (collection.CurrentLexem().LexemType == LexType.Text)
            {
                var strs = BaseExpressionFactory.ParseStringQuote(collection.CurrentLexem().Lexem);
                if (strs.Length != 1) collection.Error("Composite column name", collection.CurrentLexem());
                FieldCapExpr fc = new FieldCapExpr();
                fc.FieldAlias = strs[0];
                
                return fc;
            }
            collection.Error("Column not found", collection.CurrentLexem());
            return null;
        }

    }

    public class LexExpr
    {
        public string Lexem;
        public LexType LexemType;
        public int ColNum;
        public int RowNum;

        public Expression Expr;
        public int Prior;

        public bool IsSkobraClose()
        {
            return LexemType == LexType.Skobka && Lexem == ")";
        }
        public bool IsSkobraOpen()
        {
            return LexemType == LexType.Skobka && Lexem == "(";
        }
    }

    public class LexemCollection : List<LexExpr>
    {
        public int IndexLexem = 0;
        public BaseExpressionFactory NodeFactory = new BaseExpressionFactory();
        public string OriginalExpressionString;
        public ITableGetter TableGetter;

        public LexExpr GotoNext()
        {
            IndexLexem++;
            if (IndexLexem >= Count) return null;
            return CurrentLexem();
        }

        public LexExpr GotoNextMust()
        {
            IndexLexem++;
            if (IndexLexem >= Count) UnexceptEndError();
            return CurrentLexem();            
        }

        public LexExpr GetLast()
        {
            if (Count > 0) return this[Count - 1];
            return null;
        }

        public LexExpr Get(int index)
        {
            int i = IndexLexem - 1;
            if (i >= 0 && i < Count && Count > 0) return this[i];
            return null;
        }

        public LexExpr GetPrev()
        {
            int i = IndexLexem - 1;
            if (i >= 0 && Count > 0) return this[i];
            else return null;
        }

        public LexExpr GetNext()
        {
            int i = IndexLexem + 1;
            if (i < Count && Count > 0) return this[i];
            else return null;
        }

        public bool IsEnd()
        {
            return (IndexLexem >= Count);
        }

        public LexExpr CurrentLexem()
        {
            if (IndexLexem >= Count) return null;
            return this[IndexLexem];
        }

        public LexExpr SeekLexem(int delta)
        {
            int i = IndexLexem + delta;
            if (i < 0 || i >= Count) return null;
            return this[i];
        }

        public void Error(string message, LexExpr lex)
        {
            if (lex == null) throw new Exception(message);
            else
            throw new Exception(message + " " + lex.Lexem);
        }

        public void UnexceptEndError()
        {
            Error("Unexcepted end statment", GetLast());
        }
    }

    public class CustomToNode
    {
        public LexemCollection Collection { get; protected set; }

        public bool noWaitLexem = false;
        public List<Expression> Results = new List<Expression>();

        public Expression Single()
        {
            if (Results.Count != 1) throw new Exception("Error in expression");
            return Results[0];
        }

        public virtual void Parse(LexemCollection collection)
        {
            Collection = collection;
        }
    }

}
