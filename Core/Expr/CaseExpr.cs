using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore;

namespace ParserCore
{
    public class WhenThen: SqlToken
    {
        private Expression _When;
        public Expression When
        {
            get { return _When; }
            set
            {
                _When = value;
                if (value != null) value.ParentToken = this;
            }
        }

        private Expression _Then;
        public Expression Then
        {
            get { return _Then; }
            set
            {
                _Then = value;
                if (value != null) value.ParentToken = this;
            }
        }

        public override IExplore Expolore(DelegateExpessionExplorer del)
        {
            When = (Expression)When.Expolore(del);
            Then = (Expression)Then.Expolore(del);
            return base.Expolore(del);
        }

        public override string ToStr()
        {
            return "WHEN " + When.ToStr()+" THEN "+Then.ToStr();
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            return "WHEN " + When.ToSql(builder) + " THEN " + Then.ToSql(builder);
        }

        public override void Prepare()
        {
            When.Prepare();
            Then.Prepare();
        }
    }

    public class CaseExpr: Expression
    {
        private Expression _CaseArg;
        public Expression CaseArg
        {
            get { return _CaseArg; }
            set
            {
                _CaseArg = value;
                if (value != null) value.ParentToken = this;
            }
        }

        public TokenList<WhenThen> ListWhenThen;

        private Expression _Else;
        public Expression Else
        {
            get { return _Else; }
            set
            {
                _Else = value;
                if (value != null) value.ParentToken = this;
            }
        }

        private CaseAsXXX CaseCompare;

        public CaseExpr()
        {
            ListWhenThen = new TokenList<WhenThen>(this);
        }

        public override void Prepare()
        {
            if (CaseArg != null) CaseArg.Prepare();
            foreach (var wt in ListWhenThen)
            {
                wt.Prepare();
            }
            if (Else != null) Else.Prepare();
            base.Prepare();
            if (ListWhenThen.Count == 0) throw new Exception("Not found \"When\"");
            SimpleTypes st1 = ListWhenThen[0].Then.GetResultType();
            foreach (var wt in ListWhenThen)
            {
                SimpleTypes st2 = wt.Then.GetResultType();
                if (st1 != st2)
                {
                    if (st1.IsNumber() && st2.IsNumber()) st1 = SimpleTypes.Float;
                    else this.TypesException();
                }
            }
            //Case.GetObjectResultOut()
            SetResultType(st1);
            switch (st1)
            {
                case SimpleTypes.Date:
                case SimpleTypes.DateTime:
                    GetDateTimeResultOut = AsDateTime;
                    break;
                case SimpleTypes.Time:
                    GetTimeResultOut = AsTime;
                    break;
                case SimpleTypes.Float:
                    GetFloatResultOut = AsFloat;
                    break;
                case SimpleTypes.Boolean:
                    GetBoolResultOut = AsBool;
                    break;
                case SimpleTypes.Integer:
                    GetIntResultOut = AsInt;
                    break;
                case SimpleTypes.String:
                    GetStrResultOut = AsStr;
                    break;
            }
            if (CaseArg != null)
            {
                var st2 = CaseArg.GetResultType();
                switch (st2)
                {
                    case SimpleTypes.Date:
                    case SimpleTypes.DateTime:
                        CaseCompare = CaseAsDateTime;
                        break;
                    case SimpleTypes.Float:
                        CaseCompare = CaseAsFloat;
                        break;
                    case SimpleTypes.Boolean:
                        CaseCompare = CaseAsBool;
                        break;
                    case SimpleTypes.Integer:
                        CaseCompare = CaseAsInt;
                        break;
                    case SimpleTypes.String:
                        CaseCompare = CaseAsStr;
                        break;
                    case SimpleTypes.Time:
                        CaseCompare = CaseAsTime;
                        break;
                    default:
                        throw new Exception("Can not compare arguments");
                }
            }
        }
        public override bool IsOperation()
        {
            return false;
        }

        delegate bool CaseAsXXX(WhenThen wt, object data);

        public bool CaseAsInt(WhenThen wt, object data)
        {
            return CaseArg.GetIntResultOut(data) == wt.When.GetIntResultOut(data);
        }

        public bool CaseAsFloat(WhenThen wt, object data)
        {
            return CaseArg.GetFloatResultOut(data) == wt.When.GetFloatResultOut(data);
        }

        public bool CaseAsStr(WhenThen wt, object data)
        {
            return CaseArg.GetStrResultOut(data) == wt.When.GetStrResultOut(data);
        }

        public bool CaseAsBool(WhenThen wt, object data)
        {
            return CaseArg.GetBoolResultOut(data) == wt.When.GetBoolResultOut(data);
        }

        public bool CaseAsDateTime(WhenThen wt, object data)
        {
            return CaseArg.GetDateTimeResultOut(data) == wt.When.GetDateTimeResultOut(data);
        }

        public bool CaseAsTime(WhenThen wt, object data)
        {
            return CaseArg.GetTimeResultOut(data) == wt.When.GetTimeResultOut(data);
        }

        public Expression GetResultExpession(object data)
        {
            if (CaseArg == null)
            {
                foreach (var wt in ListWhenThen)
                {
                    if (wt.When.GetBoolResultOut(data)) return wt.Then;
                }
                if (Else != null) return Else;
            }
            else
            {
                foreach (var wt in ListWhenThen)
                {
                    if (CaseCompare(wt, data)) return wt.Then;
                }
                if (Else != null) return Else;
            }
            return null;
        }

        public override bool GetNullResultOut(object data)
        {
            var e = GetResultExpession(data);
            if (e == null) return true;
            return e.GetNullResultOut(data);
        }

        public long AsInt(object data)
        {
            return GetResultExpession(data).GetIntResultOut(data);
        }

        public double AsFloat(object data)
        {
            return GetResultExpession(data).GetFloatResultOut(data);
        }

        public bool AsBool(object data)
        {
            return GetResultExpession(data).GetBoolResultOut(data);
        }
        public string AsStr(object data)
        {
            return GetResultExpession(data).GetStrResultOut(data);
        }

        public DateTime AsDateTime(object data)
        {
            return GetResultExpession(data).GetDateTimeResultOut(data);
        }
        public TimeSpan AsTime(object data)
        {
            return GetResultExpession(data).GetTimeResultOut(data);
        }

        protected override bool CanCalcOnline()
        {
            if (CaseArg != null && !CaseArg.OnlyOnline()) return false;
            if (Else != null && !Else.OnlyOnline()) return false;
            foreach (var wt in ListWhenThen)
            {
                if (wt.Then != null && !wt.Then.OnlyOnline()) return false;
                if (wt.When != null && !wt.When.OnlyOnline()) return false;
            }
            return true;
        }

        public override IExplore Expolore(DelegateExpessionExplorer del)
        {
            return base.Expolore(del);
        }

        public override string ToStr()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" CASE ");
            if (CaseArg != null) sb.Append(CaseArg.ToStr()).Append(" ");
            foreach (var wt in ListWhenThen)
            {
                sb.Append(wt.ToStr()).Append(" ");
            }
            if (Else != null) sb.Append(" ELSE ").Append(Else.ToStr()).Append(" ");
            sb.Append("END ");
            return sb.ToString();
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" CASE ");
            if (CaseArg != null) sb.Append(CaseArg.ToSql(builder)).Append(" ");
            foreach (var wt in ListWhenThen)
            {
                sb.Append(wt.ToSql(builder)).Append(" ");
            }
            if (Else != null) sb.Append(" ELSE ").Append(Else.ToSql(builder)).Append(" ");
            sb.Append("END ");
            return sb.ToString();
        }

        public override void ParseInside(ExpressionParser parser)
        {
            var collection = parser.Collection;

            var le = collection.GotoNextMust();
            if (le.LexemType != LexType.Command || le.LexemText.ToLower() != "when")
            {
                int idx = collection.IndexLexem;
                ExpressionParser tonode = new ExpressionParser(collection);
                tonode.Parse();
                if (tonode.Results.Count != 1) collection.Error("не верное число параметров", collection.Get(idx));
                CaseArg = tonode.Single();
                le = collection.CurrentLexem();
            }

            if (le.LexemType != LexType.Command || le.LexemText.ToLower() != "when")
            {
                collection.Error("Ожидалось WHEN", collection.CurrentOrLast());
            }
            while (le.LexemType == LexType.Command && le.LexemText.ToLower() == "when")
            {
                WhenThen wt = new WhenThen();
                collection.GotoNextMust();
                int idx = collection.IndexLexem;
                ExpressionParser tonode = new ExpressionParser(collection);
                tonode.Parse();
                if (tonode.Results.Count != 1) collection.Error("не верное число параметров", collection.Get(idx));
                wt.When = tonode.Single();
                le = collection.CurrentLexem();
                if (le == null || le.LexemType != LexType.Command || le.LexemText.ToLower() != "then")
                    collection.Error("ожидалось THEN", collection.CurrentOrLast());
                le = collection.GotoNextMust();
                idx = collection.IndexLexem;
                tonode = new ExpressionParser(collection);
                tonode.Parse();
                if (tonode.Results.Count != 1) collection.Error("не верное число параметров", collection.Get(idx));
                wt.Then = tonode.Single();
                ListWhenThen.Add(wt);
                le = collection.CurrentLexem();
                if (le == null) collection.Error("Не найден END", collection.GetLast());
                if (le.LexemText.ToLower() == "end") break;
                if (le.LexemText.ToLower() == "when") continue;
                if (le.LexemText.ToLower() == "else")
                {
                    le = collection.GotoNextMust();
                    idx = collection.IndexLexem;
                    tonode = new ExpressionParser(collection);
                    tonode.Parse();
                    if (tonode.Results.Count != 1) collection.Error("не верное число параметров", collection.Get(idx));
                    Else = tonode.Single();
                    le = collection.CurrentLexem();
                    if (le == null || le.LexemText.ToLower() != "end") collection.Error("Ожидался END", collection.CurrentOrLast());
                    break;
                }
                collection.Error("Не ожиданный конец", collection.CurrentOrLast());
            }
            base.ParseInside(parser);
        }
    }
}
