using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.Simple
{
    public abstract class Custom_Arifmetic : Custom_TwoOperand
    {
    }

    public class Plus_Arifmetic : Custom_Arifmetic
    {
        public override void Prepare()
        {
            base.Prepare();
            SimpleTypes t1 = Operand1.GetResultType();
            SimpleTypes t2 = Operand2.GetResultType();
            //public enum ColumnSimpleTypes { Integer = 1, Float = 2, String = 3, Geometry = 4, Date = 5, DateTime = 6, Time = 7, Boolean = 8 }
            if (t1 == SimpleTypes.String && t2 == SimpleTypes.String)
            {
                GetStrResultOut = CalcAsStr;
                SetResultType(SimpleTypes.String);
                return;
            }
            if (t1 == SimpleTypes.Integer && t2 == SimpleTypes.Integer)
            {
                GetIntResultOut = CalcAsInt;
                SetResultType(SimpleTypes.Integer);
                return;
            }
            if ((t1 == SimpleTypes.Integer || t1 == SimpleTypes.Float)
                && (t2 == SimpleTypes.Integer || t2 == SimpleTypes.Float))
            {
                GetFloatResultOut = CalcAsFloat;
                SetResultType(SimpleTypes.Float);
                return;
            }
            if ((t1 == SimpleTypes.DateTime || t1 == SimpleTypes.Date) && t2 == SimpleTypes.Time)
            {
                GetDateTimeResultOut = CalcAsDateTimeAndTime1;
                SetResultType(SimpleTypes.DateTime);
                return;
            }
            if ((t2 == SimpleTypes.DateTime || t2 == SimpleTypes.Date) && t1 == SimpleTypes.Time)
            {
                GetDateTimeResultOut = CalcAsDateTimeAndTime2;
                SetResultType(SimpleTypes.DateTime);
                return;
            }
            if (t2 == SimpleTypes.Time && t1 == SimpleTypes.Time)
            {
                GetTimeResultOut = CalcAsTimeAndTime;
                SetResultType(SimpleTypes.Time);
                return;
            }
            /*if (t2 == ColumnSimpleTypes.Geometry && t1 == ColumnSimpleTypes.Geometry)
            {
                GetGeomResultOut = CalcAsGeom;
                SetResultType(ColumnSimpleTypes.Geometry);
                return;
            }*/
            TypesException();
        }

        protected long CalcAsInt(object data) { return Operand1.GetIntResultOut(data) + Operand2.GetIntResultOut(data); }
        protected string CalcAsStr(object data) { return Operand1.GetStrResultOut(data) + Operand2.GetStrResultOut(data); }
        protected double CalcAsFloat(object data) { return Operand1.GetFloatResultOut(data) + Operand2.GetFloatResultOut(data); }
        //protected Geometry CalcAsGeom(object data){ return Operand1.GetGeomResultOut(data).Union(Operand2.GetGeomResultOut(data));}

        protected DateTime CalcAsDateTimeAndTime1(object data) { return Operand1.GetDateTimeResultOut(data).Add(Operand2.GetTimeResultOut(data)); }
        protected DateTime CalcAsDateTimeAndTime2(object data) { return Operand2.GetDateTimeResultOut(data).Add(Operand1.GetTimeResultOut(data)); }
        protected TimeSpan CalcAsTimeAndTime(object data) { return Operand1.GetTimeResultOut(data).Add(Operand2.GetTimeResultOut(data)); }

        public override string ToStr() { return Operand1.ToStr() + " + " + Operand2.ToStr(); }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            if (builder.DbType == DriverType.SqlServer)
            {
                string op = "+";
                SimpleTypes t1 = Operand1.GetResultType();
                SimpleTypes t2 = Operand2.GetResultType();
                if (t2 == SimpleTypes.Time && t1 == SimpleTypes.Time)
                {
                    return "CONVERT(time, CONVERT(datetime, " + Operand1.ToSql(builder) + ")" + op + "CONVERT(datetime, " + Operand2.ToSql(builder) + "))";
                }
                if (t1 == SimpleTypes.Date && t2 == SimpleTypes.Time)
                {
                    return "(CONVERT(datetime, " + Operand1.ToSql(builder) + ")" + op + Operand2.ToSql(builder) + ")";
                }
                if (t2 == SimpleTypes.Date && t1 == SimpleTypes.Time)
                {
                    return "(CONVERT(datetime, " + Operand2.ToSql(builder) + ")" + op + Operand1.ToSql(builder) + ")";
                }
                if (t1 == SimpleTypes.Geometry && t2 == SimpleTypes.Geometry)
                {
                    if (op == "+") return "(" + Operand2.ToSql(builder) + ".STUnion(" + Operand1.ToSql(builder) + "))";
                    if (op == "-") return "(" + Operand2.ToSql(builder) + ".STDifference(" + Operand1.ToSql(builder) + "))";
                }
                return "(" + Operand1.ToSql(builder) + op + Operand2.ToSql(builder) + ")";
            }
            if (builder.DbType == DriverType.PostgreSQL)
            {
                string op = "+";
                SimpleTypes t1 = Operand1.GetResultType();
                SimpleTypes t2 = Operand2.GetResultType();
                if (t1 == SimpleTypes.String || t2 == SimpleTypes.String)
                {
                    op = "||";
                }
                if (t2 == SimpleTypes.Time && t1 == SimpleTypes.Time)
                {
                    return "(" + Operand1.ToSql(builder) + op + "CAST(" + Operand2.ToSql(builder) + " as interval))";
                }
                return "(" + Operand1.ToSql(builder) + op + Operand2.ToSql(builder) + ")";
            }
            return ToSqlException();
        }
        public override int Priority() { return PriorityConst.PlusMinus; }
    }
}
