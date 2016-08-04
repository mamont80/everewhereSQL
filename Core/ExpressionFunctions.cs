using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace TableQuery
{
    /* пока не получилось сделать функцию с не фиксированным числом параметров
    public class StringComparer_operation : FuncExpr
    {
        public override bool IsOperation()
        {
            return true;
        }
        public override int Priority()
        {
            return 150;
        }

        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            FunctionName = "StringComparer";
            if (Childs == null || Childs.Count < 2) throw new Exception("Wrong number operands");

            foreach(Expression e in Childs) if (e.GetResultType() != ColumnSimpleTypes.String) TypesException();
            SimpleType = ColumnSimpleTypes.Boolean;
            SetConvertors(SimpleType);
            GetBoolResultOut = CalcAsBool;
        }

        public override int NumChilds() { return -1; }

        public override void AddChild(Expression child)
        {
            if (Childs == null) Childs = new List<Expression>();
            Childs.Add(child);
        }
        private bool CalcAsBool() 
        { 
            //return Operand1.GetStrResultOut().Contains(Operand2.GetStrResultOut()); 
            return true;
        }

        public override string ToDebugStr() { return "(StringComparer())"; }
    }*/


    /// <summary>
    /// Привязывается к выражениям. Указывает что данное выражение является пространственной операцией, которая может использовать пространственный индекс
    /// </summary>
    public interface IOperationForSpatialIndex
    {
    }

    public class Length_operation : FuncExpr_OneOperand
    {

        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (!(Operand.GetResultType() == ColumnSimpleTypes.String)) TypesException();
            SetResultType(ColumnSimpleTypes.Integer);
            GetIntResultOut = CalcRes;
        }
        private Int64 CalcRes(object data) { return Operand.GetStrResultOut(data).Length; }

        public override string ToStr() { return "length(" + Operand.ToStr()+ ")"; }

        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return builder.Driver.ToSql(this, builder);
        }
    }
    public class Lower_operation : FuncExpr_OneOperand
    {

        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (!(Operand.GetResultType() == ColumnSimpleTypes.String)) TypesException();
            SetResultType(ColumnSimpleTypes.String);
            GetStrResultOut = CalcRes;
        }
        private string CalcRes(object data) { return Operand.GetStrResultOut(data).ToLower(); }

        public override string ToStr() { return "lower(" + Operand.ToStr() + ")"; }

        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return "lower(" + Operand.ToSQL(builder) + ")";
        }
    }
    public class Upper_operation : FuncExpr_OneOperand
    {

        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (!(Operand.GetResultType() == ColumnSimpleTypes.String)) TypesException();
            SetResultType(ColumnSimpleTypes.String);
            GetStrResultOut = CalcRes;
        }
        private string CalcRes(object data) { return Operand.GetStrResultOut(data).ToUpper(); }

        public override string ToStr() { return "upper(" + Operand.ToStr() + ")"; }
        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return "upper(" + Operand.ToSQL(builder) + ")";
        }
    }

    public class Trim_operation : FuncExpr_OneOperand
    {

        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (!(Operand.GetResultType() == ColumnSimpleTypes.String)) TypesException();
            SetResultType(ColumnSimpleTypes.String);
            GetStrResultOut = CalcRes;
        }
        private string CalcRes(object data) { return Operand.GetStrResultOut(data).Trim(); }

        public override string ToStr() { return "trim(" + Operand.ToStr() + ")"; }
        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return "ltrim(rtrim(" + Operand.ToSQL(builder) + "))";
        }
    }

    public class Coalesce_FuncExpr : Expression
    {
        public override bool IsOperation() { return false; }
        public override bool IsFunction() { return true; }
        public override int NumChilds() { return -1; }

        protected override void BeforePrepare()
        {
            base.BeforePrepare();

            if (Childs.Count < 1) throw new Exception("Нехватает операндов в выражении IN");

            List<ColumnSimpleTypes> types = new List<ColumnSimpleTypes>();
            for (int i = 0; i < Childs.Count; i++)
            {
                types.Add(Childs[i].GetResultType());
            }
            types = types.Distinct().ToList();
            if (types.Count == 0 || types.Count > 2) TypesException();
            ColumnSimpleTypes t = types[0];
            if (types.Count == 2)
            {
                if ((types[0] == ColumnSimpleTypes.Float && types[1] == ColumnSimpleTypes.Integer) || (types[1] == ColumnSimpleTypes.Float && types[0] == ColumnSimpleTypes.Integer))
                {
                    t = ColumnSimpleTypes.Float;
                }
                else TypesException();
            }
            //CompareItem
            switch (t)
            {
                case ColumnSimpleTypes.Boolean:
                    SetResultType(ColumnSimpleTypes.Boolean);
                    GetStrResultOut = StrRes;
                    break;
                case ColumnSimpleTypes.Date:
                case ColumnSimpleTypes.DateTime:
                    SetResultType(ColumnSimpleTypes.DateTime);
                    GetDateTimeResultOut = DateTimeRes;
                    break;
                case ColumnSimpleTypes.Float:
                    SetResultType(ColumnSimpleTypes.Float);
                    GetFloatResultOut = FloatRes;
                    break;
                case ColumnSimpleTypes.Geometry:
                    SetResultType(ColumnSimpleTypes.Geometry);
                    GetGeomResultOut = GeomRes;
                    break;
                case ColumnSimpleTypes.Integer:
                    SetResultType(ColumnSimpleTypes.Integer);
                    GetIntResultOut = IntRes;
                    break;
                case ColumnSimpleTypes.String:
                    SetResultType(ColumnSimpleTypes.String);
                    GetStrResultOut = StrRes;
                    break;
                case ColumnSimpleTypes.Time:
                    SetResultType(ColumnSimpleTypes.Time);
                    GetTimeResultOut = TimeRes;
                    break;
            }
        }

        private string StrRes(object data)
        {
            for (int i = 0; i < Childs.Count; i++)
            {
                if (!Childs[i].GetNullResultOut(data)) return Childs[i].GetStrResultOut(data);
            }
            return Childs.Last().GetStrResultOut(data);
        }

        private Int64 IntRes(object data)
        {
            for (int i = 0; i < Childs.Count; i++)
            {
                if (!Childs[i].GetNullResultOut(data)) return Childs[i].GetIntResultOut(data);
            }
            return Childs.Last().GetIntResultOut(data);
        }

        private double FloatRes(object data)
        {
            for (int i = 0; i < Childs.Count; i++)
            {
                if (!Childs[i].GetNullResultOut(data)) return Childs[i].GetFloatResultOut(data);
            }
            return Childs.Last().GetFloatResultOut(data);
        }

        private DateTime DateTimeRes(object data)
        {
            for (int i = 0; i < Childs.Count; i++)
            {
                if (!Childs[i].GetNullResultOut(data)) return Childs[i].GetDateTimeResultOut(data);
            }
            return Childs.Last().GetDateTimeResultOut(data);
        }

        private TimeSpan TimeRes(object data)
        {
            for (int i = 0; i < Childs.Count; i++)
            {
                if (!Childs[i].GetNullResultOut(data)) return Childs[i].GetTimeResultOut(data);
            }
            return Childs.Last().GetTimeResultOut(data);
        }

        private object GeomRes(object data)
        {
            for (int i = 0; i < Childs.Count; i++)
            {
                if (!Childs[i].GetNullResultOut(data)) return Childs[i].GetGeomResultOut(data);
            }
            return Childs.Last().GetGeomResultOut(data);
        }

        private bool BoolRes(object data)
        {
            for (int i = 0; i < Childs.Count; i++)
            {
                if (!Childs[i].GetNullResultOut(data)) return Childs[i].GetBoolResultOut(data);
            }
            return Childs.Last().GetBoolResultOut(data);
        }


        public override string ToStr()
        {
            string s = "Coalesce(";
            for (int i = 0; i < Childs.Count; i++)
            {
                if (i >= 1) s += ", ";
                s += Childs[i].ToStr();
            }
            s += ")";
            return s;
        }
        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            string s = "coalesce(";
            for (int i = 0; i < Childs.Count; i++)
            {
                if (i >= 1) s += ", ";
                s += Childs[i].ToSQL(builder);
            }
            s += ")";
            return s;
        }
    }

    public class Right_operation : FuncExpr_TwoOperand
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (!(Operand1.GetResultType() == ColumnSimpleTypes.String && Operand2.GetResultType() == ColumnSimpleTypes.Integer)) TypesException();
            SetResultType(ColumnSimpleTypes.String);
            GetStrResultOut = CalcAsStr;
        }
        private string CalcAsStr(object data)
        {
            string s = Operand1.GetStrResultOut(data);
            return s.Substring(s.Length - (int)Operand2.GetIntResultOut(data));
        }

        public override string ToStr() { return "Right(" + Operand1.ToStr() + ", " + Operand2.ToStr() + ")"; }
        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return "right(" + Operand1.ToSQL(builder) + "," + Operand2.ToSQL(builder) + ")";
        }
    }

    public class Left_operation : FuncExpr_TwoOperand
    {

        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (!(Operand1.GetResultType() == ColumnSimpleTypes.String && Operand2.GetResultType() == ColumnSimpleTypes.Integer)) TypesException();
            SetResultType(ColumnSimpleTypes.String);
            GetStrResultOut = CalcAsStr;
        }
        private string CalcAsStr(object data) { return Operand1.GetStrResultOut(data).Substring(0, (int)Operand2.GetIntResultOut(data)); }

        public override string ToStr() { return "left(" + Operand1.ToStr() + ", " + Operand2.ToStr() + ")"; }
        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return "left(" + Operand1.ToSQL(builder) + "," + Operand2.ToSQL(builder) + ")";
        }
    }

    public class StrToTime : FuncExpr_OneOperand
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (Operand.GetResultType() != ColumnSimpleTypes.String) TypesException();
            SetResultType(ColumnSimpleTypes.Time);
            GetTimeResultOut = GetResult;
        }

        private TimeSpan GetResult(object data)
        {
            string s = Operand.GetStrResultOut(data);
            DateTime dt;
            if (CommonUtils.ParseDateTime(s, out dt) != ParserDateTimeStatus.Time) throw new Exception("String value is not time type");
            return dt.TimeOfDay;
        }

        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return builder.Driver.ToSql(this, builder);
            //"(geometry::STGeomFromText(" + Operand.ToSQL(builder) + ", 0)"; 
        }
        public override string ToStr() { return "StrToTime(" + Operand.ToStr() + ")"; }
    }

    public class StrToDateTime : FuncExpr_OneOperand
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (Operand.GetResultType() != ColumnSimpleTypes.String) TypesException();
            SetResultType(ColumnSimpleTypes.DateTime);
            GetDateTimeResultOut = GetResult;
        }

        private DateTime GetResult(object data)
        {
            string s = Operand.GetStrResultOut(data);
            DateTime dt;
            ParserDateTimeStatus st = CommonUtils.ParseDateTime(s, out dt);
            if (st == ParserDateTimeStatus.Date || st == ParserDateTimeStatus.DateTime) return dt;
            throw new Exception("String value is not date type");
        }

        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return builder.Driver.ToSql(this, builder);
        }
        public override string ToStr() { return "StrToDateTime(" + Operand.ToStr() + ")"; }
    }


    public class Month : FuncExpr_OneOperand
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            var t = Operand.GetResultType();
            if (t != ColumnSimpleTypes.DateTime && t != ColumnSimpleTypes.Date) TypesException();
            SetResultType(ColumnSimpleTypes.Integer);
            GetIntResultOut = GetResult;
        }

        private long GetResult(object data)
        {
            DateTime dt = Operand.GetDateTimeResultOut(data);
            return dt.Month;
        }

        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return builder.Driver.ToSql(this, builder);
        }
    }

    public class Year : FuncExpr_OneOperand
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            var t = Operand.GetResultType();
            if (t != ColumnSimpleTypes.DateTime && t != ColumnSimpleTypes.Date) TypesException();
            SetResultType(ColumnSimpleTypes.Integer);
            GetIntResultOut = GetResult;
        }

        private long GetResult(object data)
        {
            DateTime dt = Operand.GetDateTimeResultOut(data);
            return dt.Year;
        }

        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return builder.Driver.ToSql(this, builder);
        }
    }

    public class Day : FuncExpr_OneOperand
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            var t = Operand.GetResultType();
            if (t != ColumnSimpleTypes.DateTime && t != ColumnSimpleTypes.Date) TypesException();
            SetResultType(ColumnSimpleTypes.Integer);
            GetIntResultOut = GetResult;
        }

        private long GetResult(object data)
        {
            DateTime dt = Operand.GetDateTimeResultOut(data);
            return dt.Day;
        }

        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return builder.Driver.ToSql(this, builder);
        }
    }

    public class AddMinutes : FuncExpr_TwoOperand
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            ColumnSimpleTypes st = ColumnSimpleTypes.DateTime;
            if (Operand1.GetResultType() == ColumnSimpleTypes.DateTime)
            {
                st = ColumnSimpleTypes.DateTime;
                GetDateTimeResultOut = GetResultDate;
            }
            if (Operand1.GetResultType() == ColumnSimpleTypes.Date)
            {
                st = ColumnSimpleTypes.Date;
                GetDateTimeResultOut = GetResultDate;
            }
            if (Operand1.GetResultType() == ColumnSimpleTypes.Time)
            {
                st = ColumnSimpleTypes.Time;
                GetTimeResultOut = GetResultTime;
            }
            SetResultType(st);
        }

        public virtual DateTime GetResultDate(object data)
        {
            return Operand1.GetDateTimeResultOut(data).AddMinutes(Operand2.GetFloatResultOut(data));
        }
        public virtual TimeSpan GetResultTime(object data)
        {
            return Operand1.GetTimeResultOut(data).Add(TimeSpan.FromMinutes(Operand2.GetFloatResultOut(data)));
        }
        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return builder.Driver.ToSql(this, builder);
        }
        public override string ToStr() { return "AddMinutes(" + Operand1.ToStr() +"," + Operand2.ToStr() + ")"; }
    }

    public class AddHours : AddMinutes
    {
        public override DateTime GetResultDate(object data)
        {
            return Operand1.GetDateTimeResultOut(data).AddHours(Operand2.GetFloatResultOut(data));
        }
        public override TimeSpan GetResultTime(object data)
        {
            return Operand1.GetTimeResultOut(data).Add(TimeSpan.FromHours(Operand2.GetFloatResultOut(data)));
        }
        public override string ToStr() { return "AddHours(" + Operand1.ToStr() + "," + Operand2.ToStr() + ")"; }
    }

    public class AddDays : AddMinutes
    {
        public override DateTime GetResultDate(object data)
        {
            return Operand1.GetDateTimeResultOut(data).AddDays(Operand2.GetFloatResultOut(data));
        }
        public override TimeSpan GetResultTime(object data)
        {
            return Operand1.GetTimeResultOut(data).Add(TimeSpan.FromDays(Operand2.GetFloatResultOut(data)));
        }
        public override string ToStr() { return "AddDays(" + Operand1.ToStr() + "," + Operand2.ToStr() + ")"; }
    }

    public class AddSeconds : AddMinutes
    {
        public override DateTime GetResultDate(object data)
        {
            return Operand1.GetDateTimeResultOut(data).AddSeconds(Operand2.GetFloatResultOut(data));
        }
        public override TimeSpan GetResultTime(object data)
        {
            return Operand1.GetTimeResultOut(data).Add(TimeSpan.FromSeconds(Operand2.GetFloatResultOut(data)));
        }
        public override string ToStr() { return "AddSeconds(" + Operand1.ToStr() + "," + Operand2.ToStr() + ")"; }
    }

    public class Now : FuncExpr_WithoutOperand
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            SetResultType(ColumnSimpleTypes.DateTime);
            GetDateTimeResultOut = GetResult;
        }

        private DateTime GetResult(object data)
        {
            return DateTime.Now;
        }

        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return builder.Driver.ToSql(this, builder);
        }
        public override string ToStr() { return "Now()"; }
    }
    

}
