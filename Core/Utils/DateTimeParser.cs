using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TableQuery
{
    public enum ParserDateTimeStatus { Error, Date, DateTime, Time }
    
    internal class DateTimeParser
    {
        private int idx = 0;
        private List<DtLexem> lst;
        int yyyy = 0;
        int MM = 0;
        int dd = 0;
        int hh = 0;
        int mm = 0;
        int ss = 0;
        bool time = false;
        bool date = false;
        private bool ErrorOnUnknowEnding = true;

        public ParserDateTimeStatus ParseDateTime(string str, out DateTime dt)
        {
            dt = DateTime.MinValue;
            lst = StrToLexem(str);
            lst.ForEach(a => a.ParseValType());
            if (lst.Count < 2) return ParserDateTimeStatus.Error;
            TryGetDate();
            //пропуск допустимого соединитиля даты и времени
            if (date && Check(ValueType.Symbol) && (lst[idx].Lexem == "," || lst[idx].Lexem == "T"))
                idx++;
            TryGetTime();
            if (!date && time && Check(ValueType.Symbol) && (lst[idx].Lexem == ",")) idx++;
            TryGetDate();

            if ((date || time) && Check(ValueType.Other))
            {
                string lastWord = lst[idx].Lexem.ToLower();
                if (lastWord == "utc" || lastWord == "z")
                    idx++;
            }
            if ((date || time) && Check(ValueType.Symbol)) idx++;
            if (ErrorOnUnknowEnding && idx != lst.Count) return ParserDateTimeStatus.Error;
            try
            {
                if (date && time)
                {
                    dt = new DateTime(yyyy, MM, dd, hh, mm, ss);
                    return ParserDateTimeStatus.DateTime;
                }
                if (date)
                {
                    dt = new DateTime(yyyy, MM, dd);
                    return ParserDateTimeStatus.Date;
                }
                if (time)
                {
                    dt = new DateTime(1970, 1, 1, hh, mm, ss);
                    return ParserDateTimeStatus.Time;
                }
            }
            catch { }
            return ParserDateTimeStatus.Error;
        }

        private bool TryGetTime()
        { 
            // 23:59
            // 23:58:12
            // 23:58:12:12345
            // 23:58:12.12345
            // 11:31 PM
            // 23:58:13Z
            if (time) return false;
            //23:58
            if (!time && Check(ValueType.Number2, ValueType.Symbol, ValueType.Number2) && lst[idx + 1].Lexem == ":" && lst[idx].Value <= 24 && lst[idx + 2].Value <= 60)
            {
                // 23:58:12
                if (Check(ValueType.Number2, ValueType.Symbol, ValueType.Number2, ValueType.Symbol, ValueType.Number2) &&
                (!time && lst[idx + 1].Lexem == lst[idx + 3].Lexem && (lst[idx + 1].Lexem == ":") &&
                        lst[idx].Value <= 24 && lst[idx + 2].Value <= 60 && lst[idx + 4].Value <= 60))
                    {
                        hh = lst[idx + 0].Value;
                        mm = lst[idx + 2].Value;
                        ss = lst[idx + 4].Value;
                        time = true;
                        idx += 5;
                        // 23:58:12:12345
                        // 23:58:12.12345
                        if ((Check(ValueType.Symbol, ValueType.Number2) || Check(ValueType.Symbol, ValueType.Number4) || Check(ValueType.Symbol, ValueType.Other))
                            && (lst[idx].Lexem == "." || lst[idx].Lexem == ":"))
                            idx += 2;
                        else// 23:58:13Z
                            if ((Check(ValueType.Symbol) || Check(ValueType.Other)) && lst[idx].Lexem.ToLower() == "z") idx += 1;
                    }else
                    if (Check(ValueType.Number2, ValueType.Symbol, ValueType.Number2, ValueType.Other)
                        && lst[idx].Value <= 12 && lst[idx + 2].Value <= 60 && (lst[idx + 3].Lexem.ToLower() == "am" || lst[idx + 3].Lexem.ToLower() == "pm"))
                    {// 11:31 PM
                        hh = lst[idx].Value;
                        mm = lst[idx + 2].Value;
                        if (lst[idx + 3].Lexem.ToLower() == "pm") hh += 12;
                        idx += 4;
                        time = true;
                    }
                    else
                    {
                        hh = lst[idx].Value;
                        mm = lst[idx + 2].Value;
                        idx += 3;
                        time = true;
                    }
            }
            return time;
        }

        private bool TryGetDate()
        {
            if (date) return false;
           // int d;
            bool ok = false;
            //31.12.99
            ok = Check(ValueType.Number2, ValueType.Symbol, ValueType.Number2, ValueType.Symbol, ValueType.Number2)
                || Check(ValueType.Number2, ValueType.Symbol, ValueType.Number2, ValueType.Symbol, ValueType.Number4);
               
            if (!date && ok && lst[idx + 1].Lexem == lst[idx + 3].Lexem && (lst[idx + 1].Lexem == "." || lst[idx + 1].Lexem == "-") && lst[idx].Value <= 31 && lst[idx + 2].Value <= 12)
            {
                dd = lst[idx].Value;
                MM = lst[idx + 2].Value;
                yyyy = TwoDigitYear(lst[idx + 4].Value);
                idx = idx + 5;
                date = true;
            }
            
            //12/31/99
            //12/31/1999
            ok = Check(ValueType.Number2, ValueType.Symbol, ValueType.Number2, ValueType.Symbol, ValueType.Number2)
                || Check(ValueType.Number2, ValueType.Symbol, ValueType.Number2, ValueType.Symbol, ValueType.Number4);
                
            if (!date && ok && lst[idx + 1].Lexem == lst[idx + 3].Lexem && (lst[idx + 1].Lexem == "/") && lst[idx].Value <= 12 && lst[idx + 2].Value <= 31)
            {
                MM = lst[idx].Value;
                dd = lst[idx + 2].Value;
                yyyy = TwoDigitYear(lst[idx + 4].Value);
                idx = idx + 5;
                date = true;
            }
            //1999-12-31
            //1999:12:31
            //1999.12.31
            ok = Check(ValueType.Number4, ValueType.Symbol, ValueType.Number2, ValueType.Symbol, ValueType.Number2);
            if (!date && ok && lst[idx + 1].Lexem == lst[idx + 3].Lexem && (lst[idx + 1].Lexem == "-" || lst[idx + 1].Lexem == "." || lst[idx + 1].Lexem == ":") && lst[idx + 2].Value <= 12 && lst[idx + 4].Value <= 31)
            {
                dd = lst[idx + 4].Value;
                MM = lst[idx + 2].Value;
                yyyy = lst[idx + 0].Value;
                idx = idx + 5;
                date = true;
            }
            /*         
         * 12 января 99
         * 12 Январь 99*/

            //feb 12 99
            //july 12 1999
            ok = Check(ValueType.MonthWord, ValueType.Number2, ValueType.Number2) || Check(ValueType.MonthWord, ValueType.Number2, ValueType.Number4);
            if (!date && ok && lst[idx + 1].Value <= 31)
            {
                dd = lst[idx + 1].Value;
                MM = lst[idx + 0].Value;
                yyyy = TwoDigitYear(lst[idx + 2].Value);
                idx = idx + 3;
                date = true;
            }
            //12 february 1999
            //12 february 99
            ok = Check(ValueType.Number2, ValueType.MonthWord, ValueType.Number4) || Check(ValueType.Number2, ValueType.MonthWord, ValueType.Number2);
            if (!date && ok && lst[idx + 0].Value <= 31)
            {
                dd = lst[idx + 0].Value;
                MM = lst[idx + 1].Value;
                yyyy = TwoDigitYear(lst[idx + 2].Value);
                idx = idx + 3;
                date = true;
            }
            //12-february-1999
            //12-february-99
            ok = Check(ValueType.Number2, ValueType.Symbol, ValueType.MonthWord, ValueType.Symbol, ValueType.Number4) || Check(ValueType.Number2, ValueType.Symbol, ValueType.MonthWord, ValueType.Symbol, ValueType.Number2);
            if (!date && ok && lst[idx + 1].Lexem == lst[idx + 3].Lexem && (lst[idx + 1].Lexem == "-") && lst[idx].Value <= 31)
            {
                dd = lst[idx].Value;
                MM = lst[idx + 2].Value;
                yyyy = TwoDigitYear(lst[idx + 4].Value);
                idx = idx + 5;
                date = true;
            }
            return date;
        }

        private int TwoDigitYear(int v)
        {
            if (v > 99) return v;
            if (v > 30)
                return v + 1900;
            else return v + 2000;
        }

        private bool Check(params ValueType[] prms)
        {
            if (lst.Count < prms.Length + idx) return false;
            bool ok = true;
            for (int i = 0; i < prms.Length; i++)
            {
                if (lst[idx + i].ValType != prms[i])
                { ok = false; break; }
            }
            if (!ok) return false;
            return true;
        }

        private enum LexemType
        {
            Number,
            Word,
            Symbol
        }

        private enum ValueType
        {
            Number2,
            Number4,
            MonthWord,
            Symbol,//спец. символ или одиночная буква
            Other
        }

        private class DtLexem
        {
            public string Lexem = "";
            public LexemType Type;
            public ValueType ValType;
            public int Value = -1;
            public void ParseValType()
            {
                if (Type == LexemType.Number)
                {
                    if (!int.TryParse(Lexem, out Value))
                    {
                        Type = LexemType.Word;
                        ValType = ValueType.Other;
                        Value = -1;
                        return;
                    }
                    if (Lexem.Length <= 2)
                        ValType = ValueType.Number2;
                    else
                        if (Lexem.Length == 4)
                            ValType = ValueType.Number4;
                        else ValType = ValueType.Other;
                }
                if (Type == LexemType.Symbol)
                    ValType = ValueType.Symbol;
                if (Type == LexemType.Word)
                {
                    if (Lexem.Length == 1) ValType = ValueType.Symbol;
                    else {
                        string s2 = Lexem.ToLower();
                        int m = 0;
                        if (s2 == "jan" || s2 == "january" || s2 == "янв" || s2 == "января" || s2 == "январь") m = 1;
                        else
                            if (s2 == "feb" || s2 == "february" || s2 == "фев" || s2 == "февраль" || s2 == "февраля") m = 2;
                            else
                                if (s2 == "mar" || s2 == "march" || s2 == "мар" || s2 == "мрт" || s2 == "март" || s2 == "марта") m = 3;
                                else
                                    if (s2 == "apr" || s2 == "april" || s2 == "апр" || s2 == "апрель" || s2 == "апреля") m = 4;
                                    else
                                        if (s2 == "may" || s2 == "may" || s2 == "май" || s2 == "мая") m = 5;
                                        else
                                            if (s2 == "jun" || s2 == "june" || s2 == "июнь" || s2 == "июн" || s2 == "июня") m = 6;
                                            else
                                                if (s2 == "jul" || s2 == "july" || s2 == "июл" || s2 == "июль" || s2 == "июля") m = 7;
                                                else
                                                    if (s2 == "aug" || s2 == "august" || s2 == "авг" || s2 == "август" || s2 == "августа") m = 8;
                                                    else
                                                        if (s2 == "sep" || s2 == "sept" || s2 == "september" || s2 == "сен" || s2 == "сент" || s2 == "сентябрь" || s2 == "сентября") m = 9;
                                                        else
                                                            if (s2 == "oct" || s2 == "october" || s2 == "окт" || s2 == "октябрь" || s2 == "октября") m = 10;
                                                            else
                                                                if (s2 == "nov" || s2 == "november" || s2 == "нояб" || s2 == "нбр" || s2 == "ноя" || s2 == "ноябрь" || s2 == "ноября") m = 11;
                                                                else
                                                                    if (s2 == "december" || s2 == "dec" || s2 == "дек" || s2 == "декабрь" || s2 == "декабря") m = 12;
                        if (m > 0)
                        {
                            Value = m;
                            ValType = ValueType.MonthWord;
                        }
                        else ValType = ValueType.Other;
                    }
                }
            }
        }
        //строки разбираются на числа и слова. Пробелы расцениваются как разделители, но не считаются лексемами.
        private static List<DtLexem> StrToLexem(string str)
        {
            List<DtLexem> res = new List<DtLexem>();
            string s = str.Trim();
            if (s.Length == 0) return res;
            DtLexem l = null;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == ' ')
                {
                    if (l != null)
                    {
                        res.Add(l);
                        l = null;
                    }
                    continue;
                }
                LexemType lt = GetLexemType(c);
                if (lt == LexemType.Symbol)
                {
                    if (l != null) res.Add(l);
                    l = new DtLexem();
                    l.Type = lt;
                    l.Lexem = c.ToString();
                    continue;
                }
                if (l == null)
                {
                    l = new DtLexem();
                    l.Type = lt;
                    l.Lexem = c.ToString();
                }
                else
                {
                    if (l.Type == lt)
                    {
                        l.Lexem += c;
                    }
                    else
                    {
                        if (l != null) res.Add(l);
                        l = new DtLexem();
                        l.Type = lt;
                        l.Lexem = c.ToString();   
                    }
                }
            }
            if (l != null) res.Add(l);
            return res;
        }

        private static LexemType GetLexemType(char c)
        {
            if (c >= '0' && c <= '9') return LexemType.Number;
            if (c == ':' || c == '/' || c == '\\' || c == '-' || c == '.' || c == ',') return LexemType.Symbol;
            return LexemType.Word;
        }
    }
}
