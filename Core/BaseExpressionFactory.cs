using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public class BaseExpressionFactory
    {
        public Expression GetExpression2(string text)
        {
            var collection = ParserLexem.Parse(text);
            collection.NodeFactory = this;
            ExpressionToNode2 expToNode = new ExpressionToNode2();
            expToNode.Parse(collection);
            return expToNode.Single();
        }

        public static string TableSqlCodeEscape(string s)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('"');
            foreach (var c in s)
            {
                if (c == '"') sb.Append('"').Append('"');
                else sb.Append(c);
            }
            sb.Append('"');
            return sb.ToString();
        }

        public static string StandartCodeEscape(string s)
        {
            return StandartCodeEscape(s, '[', ']');
        }
        /// <summary>
        /// Возвращает строку в заданных кавычках с закодированными спец. символами. Поддерживает " ' [ ]
        /// </summary>
        public static string StandartCodeEscape(string s, char quoteBegin, char quoteEnd)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(quoteBegin);
            foreach (var c in s)
            {
                if (c == quoteBegin || c == quoteEnd) sb.Append("\\").Append(c);
                else
                {
                    switch (c)
                    {
                        case '\\':
                            sb.Append("\\\\");
                            break;
                        case '\r':
                            sb.Append("\\r");
                            break;
                        case '\n':
                            sb.Append("\\n");
                            break;
                        case '\t':
                            sb.Append("\\t");
                            break;
                        default:
                            sb.Append(c);
                            break;
                    }
                }
            }
            sb.Append(quoteEnd);
            return sb.ToString();
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
            if ((f == '[' && l == ']') || (f == '\'' && l == '\'') || (f == '"' && l == '"')) quoted = true;
            if (quoted) s = s.Substring(1, s.Length - 2);
            for (int i = 0; i < s.Length;)
            {
                char c = s[i];
                if (c == '\\')
                {
                    char n;
                    if (i == (s.Length - 1)) { sb.Append(c); break; }
                    i++;
                    n = s[i];
                    if (n == f || n == l || n == '\\') sb.Append(n);
                    else
                        if (n == 'r') sb.Append('\r');
                        else
                            if (n == 'n') sb.Append('\n');
                            else
                                if (n == 't') sb.Append('\t');
                                else sb.Append(c).Append(n);
                }
                else sb.Append(c);
                i++;
            }
            return sb.ToString();
        }

        public static string[] ParseStringQuote(string text)
        {
            char q = '\0';
            List<string> res = new List<string>();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < text.Length;)
            {
                char c = text[i];
                char nc = '\0';
                if (i != text.Length - 1) nc = text[i + 1];
                if (c == '\\')
                {
                    if (nc == '[' || nc == ']' || nc == '"')
                    {
                        sb.Append(nc);
                        i+=2;
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
        /// 
        /// </summary>
        /// <param name="lexem"></param>
        /// <param name="tp">Тип операнда в первом приближении</param>
        public virtual Expression GetNode(LexExpr lex, bool uniar, LexemCollection collection)
        {
            Expression ex = null;
            if (lex.LexemType == LexType.Arfimetic)
            {
                switch (lex.Lexem)
                {
                    case "+":
                        if (uniar) ex = new UniarPlus_BoolExpr();
                        else
                        ex = new Plus_Arifmetic();
                        break;
                    case "-":
                        if (uniar) ex = new UniarMinus_BoolExpr();
                        else
                            ex = new Minus_Arifmetic();
                        break;
                    case "*":
                        if (uniar) ex = new AllColumnExpr();
                        else
                            ex = new Multi_Arifmetic();
                        break;
                    case "/":
                        ex = new Div_Arifmetic();
                        break;
                    case "<":
                        ex = new Less_CompExpr();
                        break;
                    case "<=":
                        ex = new LessOrEqual_CompExpr();
                        break;
                    case ">=":
                        ex = new GreatOrEqual_CompExpr();
                        break;
                    case ">": 
                        ex = new Great_CompExpr();
                        break;
                    case "=":
                        ex = new Equal_CompExpr();
                        break;
                    case "<>":
                    case "!=":
                        ex = new NotEqual_CompExpr();
                        break;
                }
            }
            if (lex.LexemType == LexType.Number)
            {
                ex = new ConstExpr();
                if (lex.Lexem.Contains('.'))
                {
                    (ex as ConstExpr).Init(lex.Lexem.ParseDouble(), ColumnSimpleTypes.Float);
                }
                else (ex as ConstExpr).Init(long.Parse(lex.Lexem), ColumnSimpleTypes.Integer);
            }
            if (lex.LexemType == LexType.Text)
            {
                /*
                if (lexem.StartsWith("[") || lexem.StartsWith("\""))
                {
                    ex = new FieldExpr();
                    (ex as FieldExpr).FieldName = lexem.Substring(1, lexem.Length - 2);
                }*/
                if (lex.Lexem.StartsWith("'"))
                {
                    ex = new ConstExpr();
                    (ex as ConstExpr).Init(StandartDecodeEscape(lex.Lexem), ColumnSimpleTypes.String);
                }
            }
            LexExpr n1;
            if (lex.LexemType == LexType.Command)
            {
                switch (lex.Lexem.ToLower())
                { 
                    case "null":
                        ex = new NullConstExpr();
                        break;
                    case "is":
                        n1 = collection.SeekLexem(1);
                        if (n1 != null && n1.LexemType == LexType.Command && n1.Lexem.ToLower() == "not")
                        {
                            ex = new IsNotNullExpr();
                            collection.GotoNext();
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
                    case "not":
                        n1 = collection.SeekLexem(1);
                        if (n1 != null && n1.LexemType == LexType.Command && n1.Lexem.ToLower() == "in")
                        {
                            ex = new NotInExpr();
                            collection.GotoNext();
                            break;
                        }
                        ex = new Not_BoolExpr();
                        break;
                    case "in":
                        ex = new InExpr();
                        break;
                    case "and":
                        ex = new And_BoolExpr();
                        break;
                    case "or":
                        ex = new Or_BoolExpr();
                        break;
                    case "trim":
                        ex = new Trim_operation();
                        break;
                    case "length":
                        ex = new Length_operation();
                        break;
                    case "upper":
                        ex = new Upper_operation();
                        break;
                    case "lower":
                        ex = new Lower_operation();
                        break;
                    case "left":
                        ex = new Left_operation();
                        break;
                    case "right":
                        ex = new Right_operation();
                        break;
                    case "coalesce":
                        ex = new Coalesce_FuncExpr();
                        break;
                    case "true":
                        ex = new ConstExpr();
                        (ex as ConstExpr).Init(true, ColumnSimpleTypes.Boolean);
                        break;
                    case "false":
                        ex = new ConstExpr();
                        (ex as ConstExpr).Init(false, ColumnSimpleTypes.Boolean);
                        break;
                    case "now":
                        ex = new Now();
                        break;
                    case "strtotime":
                        ex = new StrToTime();
                        break;
                    case "strtodatetime":
                        ex = new StrToDateTime();
                        break;
                    case "addseconds":
                        ex = new AddSeconds();
                        break;
                    case "addminutes":
                        ex = new AddMinutes();
                        break;
                    case "addhours":
                        ex = new AddHours();
                        break;
                    case "adddays":
                        ex = new AddDays();
                        break;
                    case "day":
                        ex = new Day();
                        break;
                    case "month":
                        ex = new Month();
                        break;
                    case "year":
                        ex = new Year();
                        break;
                }
            }
            return ex;
        }
    }
}
