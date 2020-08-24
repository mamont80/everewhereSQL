using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore.Expr.Sql;
using ParserCore.Expr.Simple;

namespace ParserCore
{
    public static class ParserUtils
    {

        public static bool IsNumber(this SimpleTypes st)
        {
            return (st == SimpleTypes.Float || st == SimpleTypes.Integer);
        }

        public static SelectExpresion FindSelect(Expression exp)
        {
            if (exp == null) return null;
            if (exp is SelectExpresion) return exp as SelectExpresion;
            if (exp is SubExpression && exp.Childs != null && exp.Childs.Count == 1) return FindSelect(exp.Childs[0]);
            return null;

        }

        /// <summary>
        /// Формирует простое выражение, не-SQL типа
        /// </summary>
        public static Expression GetSimpleExpression(string text, IExpressionFactory factory)
        {
            var collection = ParserLexem.Parse(text);
            collection.NodeFactory = factory;
            ExpressionParser expToNode = new ExpressionParser(collection);
            expToNode.Parse();
            var res = expToNode.Single();
            res = res.PrepareAndOptimize();
            return res;
        }

        public static string ConstToStrEscape(string s)
        {
            return "'" + s.Replace("'", "''") + "'";
        }

        public static string ColumnToStrEscape(string s)
        {
            return TableToStrEscape(s);
        }
        public static string TableToStrEscape(string s)
        {
            return "[" + s.Replace("]", "]]") + "]";
        }
        /// <summary>
        /// Декодирует escape символы. Строка может быть в кавычках [] '' "" В зависимости от этого парсятся символы
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string StandartDecodeEscape(string s)
        {
            if (string.IsNullOrEmpty(s) || s.Length == 1) return s;
            StringBuilder sb = new StringBuilder();
            char f = s[0];
            char l = s[s.Length - 1];
            bool quoted = false;
            char skip = '\0';
            if (f == '[' && l == ']') //|| (f == '\'' && l == '\'') || (f == '"' && l == '"'))
            {
                quoted = true;
                skip = ']';
            }
            if (f == '\'' && l == '\'') //|| (f == '"' && l == '"'))
            {
                quoted = true;
                skip = '\'';
            }
            if (f == '"' && l == '"')
            {
                quoted = true;
                skip = '"';
            }

            if (quoted)
            {
                return s.Substring(1, s.Length - 2).Replace(skip.ToString()+skip.ToString(), skip.ToString());
            }
            else return s;
        }

        public static string[] ParseStringQuote(string text)
        {
            char q = '\0';
            List<string> res = new List<string>();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < text.Length; )
            {
                char c = text[i];
                char nc = '\0';
                if (i != text.Length - 1) nc = text[i + 1];
                if (c == '\\')
                {
                    if (nc == '[' || nc == ']' || nc == '"')
                    {
                        sb.Append(nc);
                        i += 2;
                        continue;
                    }
                }
                if ((c == '"' && q == '"') || (c == ']' && q == '[') || (q == '\0' && c == '.'))
                {
                    string s = sb.ToString();
                    if (!string.IsNullOrEmpty(s)) res.Add(s);
                    sb = new StringBuilder();
                    q = '\0';
                    i++;
                    continue;
                }
                if ((c == '"' || c == '[') && q == '\0')
                {
                    q = c;
                    sb = new StringBuilder();
                    i++;
                    continue;
                }
                sb.Append(c);
                i++;
            }
            string ss = sb.ToString();
            if (!string.IsNullOrEmpty(ss)) res.Add(ss);
            return res.ToArray();
        }

        /// <summary>
        /// Проверяет наличие фразы из команд. Если есть первое слово, то должны быть и остальные слова иначе ошибка.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="phrase">Фраза разделённая пробелами</param>
        /// <param name="shift">True - в случае нахождения фразы позиция в collection передвигается на последнее слово фразы. False - не сдвигать</param>
        /// <returns></returns>
        public static bool ParseCommandPhrase(LexemCollection collection, string phrase, bool shift = true, bool exceptOnNotFull = true)
        {
            var lex = collection.CurrentLexem();
            if (lex == null) return false;
            string[] str = phrase.Split(' ').Select(a => a.Trim().ToLower()).Where(a => a != "").ToArray();
            if (str.Length == 0) return false;
            if (lex.LexemType == LexType.Command && lex.LexemText.ToLower() == str[0])
            {
                var idx = collection.IndexLexem;
                for (int i = 0; i < str.Length; i++)
                {
                    if (lex == null || lex.LexemType != LexType.Command || lex.LexemText.ToLower() != str[i])
                    {
                        if (exceptOnNotFull)
                            collection.Error("Waiting phrase " + phrase, collection.GetPrev());
                        else return false;
                    }
                    idx++;
                    lex = collection.Get(idx);
                }
                if (shift) collection.IndexLexem = idx - 1;
                return true;
            }
            return false;
        }
    }
}
