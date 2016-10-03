using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.Simple
{
    /// <summary>
    /// общий класс для = !=
    /// </summary>
    public abstract class CustomEqual : Custom_TwoOperand
    {
        public override void Prepare()
        {
            base.Prepare();
            if (!DoPrepare()) TypesException();
        }

        public static SimpleTypes? GetCompareType(Expression Operand1, Expression Operand2)
        {
            SimpleTypes t1 = Operand1.GetResultType();
            SimpleTypes t2 = Operand2.GetResultType();
            //если один операнд строка, а второй не строка. Проверка для констант
            if ((t1 == SimpleTypes.String && t2 != SimpleTypes.String) ||
                (t2 == SimpleTypes.String && t1 != SimpleTypes.String))
            {
                //находим не строковый элемент
                Expression notStrOper = Operand1;
                Expression StrOper = Operand2;
                if (StrOper.GetResultType() != SimpleTypes.String) { notStrOper = Operand2; StrOper = Operand1; }
                SimpleTypes notStr = notStrOper.GetResultType();
                if (StrOper is ConstExpr)
                {
                    switch (notStr)
                    {
                        case SimpleTypes.Integer:
                            (StrOper as ConstExpr).PrepareFor(SimpleTypes.Integer);
                            return SimpleTypes.Integer;
                        case SimpleTypes.Float:
                            (StrOper as ConstExpr).PrepareFor(SimpleTypes.Float);
                            return SimpleTypes.Float;
                        case SimpleTypes.Boolean:
                            (StrOper as ConstExpr).PrepareFor(SimpleTypes.Boolean);
                            return SimpleTypes.Boolean;
                        case SimpleTypes.Date:
                        case SimpleTypes.DateTime:
                            (StrOper as ConstExpr).PrepareFor(SimpleTypes.DateTime);
                            return SimpleTypes.DateTime;
                        case SimpleTypes.Time:
                            (StrOper as ConstExpr).PrepareFor(SimpleTypes.Time);
                            return SimpleTypes.Time;
                    }
                }
                else throw new Exception("Use explicit type conversion");
            }

            if (t1 == SimpleTypes.Integer && t2 == SimpleTypes.Integer)
            {
                return SimpleTypes.Integer;
            }
            if ((t1 == SimpleTypes.Integer || t1 == SimpleTypes.Float)
                && (t2 == SimpleTypes.Integer || t2 == SimpleTypes.Float))
            {
                return SimpleTypes.Float;
            }
            if ((t1 == SimpleTypes.Date || t1 == SimpleTypes.DateTime)
                && (t2 == SimpleTypes.Date || t2 == SimpleTypes.DateTime))
            {
                return SimpleTypes.DateTime;
            }
            if (t1 == SimpleTypes.Time && t2 == SimpleTypes.Time)
            {
                return SimpleTypes.Time;
            }
            if (t1 == t2)
            {
                if (t1 == SimpleTypes.Boolean) return SimpleTypes.Boolean;
                if (t1 == SimpleTypes.String) return SimpleTypes.String;
                if (t1 == SimpleTypes.Geometry) return SimpleTypes.Geometry;
            }
            return null;
        }

        protected bool DoPrepare()
        {
            SetResultType(SimpleTypes.Boolean);
            SimpleTypes? r = GetCompareType(Operand1, Operand2);
            if (r == null) return false;
            switch (r.Value)
            {
                case SimpleTypes.Boolean:
                    GetBoolResultOut = CompareAsBool;
                    break;
                case SimpleTypes.Date:
                case SimpleTypes.DateTime:
                    GetBoolResultOut = CompareAsDateTime;
                    break;
                case SimpleTypes.Float:
                    GetBoolResultOut = CompareAsFloat;
                    break;
                case SimpleTypes.Geometry:
                    throw new Exception("Can not compare geometries");
                //GetBoolResultOut = CompareAsGeom;
                //break;
                case SimpleTypes.Integer:
                    GetBoolResultOut = CompareAsInt;
                    break;
                case SimpleTypes.String:
                    GetBoolResultOut = CompareAsStr;
                    break;
                case SimpleTypes.Time:
                    GetBoolResultOut = CompareAsTime;
                    break;
            }
            return true;
        }
        protected abstract bool CompareAsBool(object data);
        //protected abstract bool CompareAsGeom(object data);
        protected abstract bool CompareAsStr(object data);
        protected abstract bool CompareAsInt(object data);
        protected abstract bool CompareAsFloat(object data);
        protected abstract bool CompareAsDateTime(object data);
        protected abstract bool CompareAsTime(object data);
        public override int Priority() { return PriorityConst.Compare; }
    }
}
