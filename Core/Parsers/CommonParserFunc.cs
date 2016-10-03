using System;
using ParserCore.Expr.Sql;


namespace ParserCore
{
    internal static class CommonParserFunc
    {
        public static string[] ReadTableName(LexemCollection collection)
        {
            if (collection.CurrentLexem() == null) throw new Exception("Alias not found");
            if (collection.CurrentLexem().LexemType == LexType.Command)
            {
                var r = collection.CurrentLexem().LexemText;
                return ParserUtils.ParseStringQuote(r);
            }
            if (collection.CurrentLexem().LexemType == LexType.Text)
            {
                var strs = ParserUtils.ParseStringQuote(collection.CurrentLexem().LexemText);
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
                var r = collection.CurrentLexem().LexemText;
                strs = ParserUtils.ParseStringQuote(r);
            }
            else
                if (collection.CurrentLexem().LexemType == LexType.Text)
                {
                    strs = ParserUtils.ParseStringQuote(collection.CurrentLexem().LexemText);
                }
            if (strs != null)
            {
                if (strs.Length > 1) collection.Error("Composite alias", collection.CurrentLexem());
                return strs[0];
            }
            collection.Error("Alias not found", collection.CurrentLexem());
            return null;
        }

        public static FieldExpr ReadColumn(LexemCollection collection)
        {
            if (collection.CurrentLexem() == null) throw new Exception("Alias not found");
            if (collection.CurrentLexem().LexemType == LexType.Command)
            {
                var r = collection.CurrentLexem().LexemText;
                FieldExpr fc = new FieldExpr();
                fc.FieldName = collection.CurrentLexem().LexemText;
                return fc;
            }
            if (collection.CurrentLexem().LexemType == LexType.Text)
            {
                var strs = ParserUtils.ParseStringQuote(collection.CurrentLexem().LexemText);
                if (strs.Length != 1) collection.Error("Composite column name", collection.CurrentLexem());
                FieldExpr fc = new FieldExpr();
                fc.FieldName = strs[0];

                return fc;
            }
            collection.Error("Column not found", collection.CurrentLexem());
            return null;
        }

        /// <summary>
        /// Читаем название колонки в текущей лексеме. Может быть name, "name", [name]
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static string ReadColumnNameOnly(LexemCollection collection)
        {
            if (collection.CurrentLexem() == null) collection.Error("Alias not found", collection.CurrentOrLast());
            if (collection.CurrentLexem().LexemType == LexType.Command)
            {
                var r = collection.CurrentLexem().LexemText;
                return r;
            }
            if (collection.CurrentLexem().LexemType == LexType.Text)
            {
                var strs = ParserUtils.ParseStringQuote(collection.CurrentLexem().LexemText);
                if (strs.Length != 1) collection.Error("Composite column name", collection.CurrentLexem());
                return strs[0];
            }
            collection.Error("Column not found", collection.CurrentLexem());
            return null;
        }
    }

}
