using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    // Number - число (константа)
    // Skobka - открывающаяся или закрывающаяся скобка
    // Arifmetic - арифметические опреации + - * /
    // command - кодовое слово, возможно: переменная, функция, опреация (and, or, not)
    // Text - операнд, возможно: поле, текстовая константа
    // Zpt - запятая
    public enum LexType { None, Number, Skobka, Arfimetic, Command, Text, Zpt }

    public static class ParserLexem
    {
        public static LexemCollection Parse(string text)
        {
            ParserLexemClass p = new ParserLexemClass();
            p.LexemParse(text);
            return p.res;
        }
    }

    internal class ParserLexemClass
    {
        /* особые символы: , ( ) * + - / > < >= <= =
         * строки [ ] " '
         * Между особыми символами игнорируем пробелы
         * детектируем строки без особых меток
         * Отдельно детектируем числа, после числа не должно быть особых меток строк и просто строк
         * 
         */

        //что мы читали на предыдущем символе
        private StringBuilder cur = new StringBuilder();
        private int curRowNum;
        private int curColNum;
        public LexemCollection res = new LexemCollection();
        private LexType status = LexType.None;
        private char TextQuote = ' ';
        private bool DotWas = false;
        private int i;
        private char c;
        private string currentString;
        private int rowNum = 0;
        private int colNum = 0;
            

        private char PrevChar()
        {
            char pc;
            if (i == 0) pc = '\0'; else pc = currentString[i - 1];
            return pc;
        }
        private char NextChar()
        {
            char pc;
            if ((i + 1) < currentString.Length) pc = currentString[i + 1]; else pc = '\0';
            return pc;
        }

        /// <summary>
        /// Осуществляет разбор на лексемы
        /// </summary>
        public void LexemParse(string s)
        {
            rowNum = 0;
            colNum = 0;
            currentString = s;

            for (i = 0; i < s.Length; i++)
            {
                c = s[i];
                char? nextC = null;
                if (i < s.Length - 1) nextC = s[i + 1];
                rowNum++;

                if (status != LexType.Text && (c == '-' && nextC.HasValue && nextC.Value == '-'))
                {
                    PushString();
                    while (c != '\n' && i < s.Length-1)
                    {
                        i++;
                        c = s[i];
                    }
                    continue;
                }
                if (status != LexType.Text && (c == '/' && nextC.HasValue && nextC.Value == '/'))
                {
                    PushString();
                    while (c != '\n' && i < s.Length-1)
                    {
                        i++;
                        c = s[i];
                    }
                    continue;
                }

                if (status != LexType.Text && (c == '/' && nextC.HasValue && nextC.Value == '*'))
                {
                    PushString();
                    while (i < s.Length - 1)
                    {
                        i++;
                        c = s[i];
                        if (i < s.Length - 1) nextC = s[i + 1];
                        else nextC = null;
                        rowNum++;
                        if (c == '\n')
                        {
                            colNum++;
                            rowNum = 0;
                        }
                        if (c == '*' && nextC.HasValue && nextC.Value == '/') break;
                    }
                    i++;
                    continue;
                }

                switch (c)
                {
                    case ',':
                        if (status == LexType.Text) cur.Append(c);
                        else
                        {
                            PushString();
                            AddRes(c.ToString(), LexType.Zpt, colNum, rowNum);
                        }
                        break;
                    case ')':
                    case '(':
                        if (status == LexType.Text) cur.Append(c);
                        else 
                        {
                            PushString();
                            AddRes(c.ToString(), LexType.Skobka, colNum, rowNum);
                        }
                        break;
                    case '!':
                    case '*':
                    case '/': 
                    case '+': 
                    case '-':
                    case '<':
                    case '>':
                    case '=':
                        if (status == LexType.Text) cur.Append(c);
                        else
                        {
                            string curv = cur.ToString();
                            if (c == '*' && status == LexType.Command && curv.Last() == '.')
                            {
                                cur.Append(c);
                            }else
                            if (status == LexType.Arfimetic && ((curv == "<" && c == '>') || (curv == ">" && c == '=') || (curv == "<" && c == '=') || (curv == "!" && c == '=')))
                            {
                                cur.Append(c);
                            }
                            else
                            {
                                PushString();
                                cur.Append(c);
                                status = LexType.Arfimetic;
                            }
                        }
                        break;
                    case ' ':
                    case '\r':
                    case '\n':
                        if (c == '\n')
                        {
                            colNum++;
                            rowNum = 0;
                        }
                        if (status == LexType.Text) cur.Append(c);
                        else PushString();
                        break;
                    case '\'':
                    case '"':
                        if (status != LexType.Text && status != LexType.Command)
                        {
                            PushString(); cur.Append(c); TextQuote = c; status = LexType.Text;
                        }
                        else if (status == LexType.Command)
                        {//меняем тип
                            if (PrevChar() != '.') { PushString(); cur.Append(c); TextQuote = c; status = LexType.Text; }
                            else { cur.Append(c); TextQuote = c; status = LexType.Text; }
                        }
                        else//status = LexType.Text
                        {
                            if (c == TextQuote)
                            {
                                if (NextChar() == c)
                                {
                                    cur.Append(c);
                                    i++;
                                    continue;
                                }
                                else { cur.Append(c); PushString(); }
                            }
                            else cur.Append(c);
                        }
                        break;
                    case '[':
                        if (status != LexType.Text && status != LexType.Command) 
                            { PushString(); cur.Append(c); TextQuote = c; status = LexType.Text; }
                        else if (status == LexType.Command)
                        {//меняем тип
                            if (PrevChar() != '.')
                            {
                                PushString(); cur.Append(c); TextQuote = c; status = LexType.Text;
                            }
                            else
                            {
                                cur.Append(c);
                                TextQuote = c;
                                status = LexType.Text;
                            }
                        }
                        else//status = LexType.Text
                        {
                            cur.Append(c);
                        }
                        break;
                    case ']':
                        if (status == LexType.Text) 
                        {
                            if (TextQuote == '[')
                            {
                                if (NextChar() == ']')
                                {
                                    cur.Append(c);
                                    i++;
                                    continue;
                                }
                                else
                                {
                                    cur.Append(c);
                                    PushString();
                                }
                            } else cur.Append(c);
                        }
                        else MakeError();
                      break;
                   case '.':
                        if (status == LexType.Text) cur.Append(c);
                        else if (status == LexType.Number)
                        {
                            if (DotWas) throw new Exception("Two dot in number");
                            else
                            {
                                DotWas = true;
                                cur.Append(c);
                            }
                            break;
                        }else if (status == LexType.Command)
                        {
                            cur.Append(c);
                            break;
                        }else if (status == LexType.None)
                        {
                            if (res.Count > 0 && res.Last().LexemType == LexType.Text)
                            {
                                ReOpenString();
                                cur.Append(c);
                            }else MakeError();
                        }else
                        MakeError();
                        break;
                    default:
                        if (c >= '0' && c <= '9')
                        {
                            if (status == LexType.Number || status == LexType.Text || status == LexType.Command) cur.Append(c);
                            else
                            {
                                PushString();
                                DotWas = false;
                                cur.Append(c);
                                status = LexType.Number;
                            }
                        }else
                        {
                            //Какой-то символ, типа буквы
                            if (status == LexType.Command || status == LexType.Text) cur.Append(c);else
                            {
                                PushString();
                                status = LexType.Command;
                                cur.Append(c);
                            }
                        }
                        break;
                }
            }
            PushString();
        }

        private void MakeError()
        {
            throw new Exception("error in position "+i.ToString());
        }

        private void AddRes(string text, LexType tp, int col, int row)
        {
            res.Add(new Lexem(){LexemText = text, LexemType = tp, ColNum = col, RowNum = row});
            status = LexType.None;
        }

        private void PushString()
        {
            if (status == LexType.None) return;
            string s = cur.ToString();
            if (s == "") { status = LexType.None; return; }
            if (status == LexType.Command)
            {
                s = s.Trim();
                if (s.Last() == '.') throw new Exception("Dot in position" + i.ToString());
            }
            
            if (status == LexType.Text)
            {
                if (s.Length < 2) throw new Exception("Not closed string in position " + i.ToString());
                if (TextQuote == '[')
                {
                    if (s[s.Length - 1] != ']') throw new Exception("Not closed string in position " + i.ToString());
                }else
                if (TextQuote != s[s.Length - 1]) throw new Exception("Not closed string in position " + i.ToString());
            }
            //s = RemoveSpecChars(s, status);
            AddRes(s, status, curColNum, curRowNum);
            TextQuote = ' ';
            cur = new StringBuilder();
            curColNum = colNum;
            curRowNum = rowNum;
        }

        private void ReOpenString()
        {
            cur = new StringBuilder();
            curColNum = res.Last().ColNum;
            curRowNum = res.Last().RowNum;
            cur.Append(InsertSpecChars(res.Last().LexemText, res.Last().LexemType));
            TextQuote = ' ';
            res.RemoveAt(res.Count - 1);
            status = LexType.Command;
        }

        private string InsertSpecChars(string text, LexType tp)
        {
            if (tp == LexType.Text || tp == LexType.Command)
                return text.Replace("\r", "\\r").Replace("\n", "\\n");
            else return text;
        }

        private string RemoveSpecChars(string text, LexType tp)
        { 
            string text2 = text;
            if (tp == LexType.Text || tp == LexType.Command)
            {
                StringBuilder sb = new StringBuilder();
                bool WasSlash = false;//был ли предыдущий символ слешем
                for (int i = 0; i < text.Length; i++)
                {
                    char c = text[i];
                    if (c == '\\')
                    {
                        if (WasSlash)
                        {
                            sb.Append(c);
                            WasSlash = false;
                            continue;
                        }
                        WasSlash = true;
                        continue;
                    }
                    if (WasSlash && c == 'r') sb.Append("\r");
                    else if (WasSlash && c == 'n') sb.Append("\n");
                    else sb.Append(c);
                    WasSlash = false;
                }
                if (WasSlash) sb.Append('\\');
                text2 = sb.ToString();
            }
            return text2;
        }
    }
}
