using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.Simple
{
    /// <summary>
    /// общий для > >= < <=
    /// </summary>
    public abstract class Custom_CompareExpr : Custom_TwoOperand
    {
        protected bool DoPrepare()
        {
            SimpleTypes t1 = Operand1.GetResultType();
            SimpleTypes t2 = Operand2.GetResultType();
            //SortType(ref t1, ref t2);
            SetResultType(SimpleTypes.Boolean);
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
                            GetBoolResultOut = CompareAsInt;
                            (StrOper as ConstExpr).PrepareFor(SimpleTypes.Integer);
                            return true;
                        case SimpleTypes.Float:
                            GetBoolResultOut = CompareAsFloat;
                            (StrOper as ConstExpr).PrepareFor(SimpleTypes.Float);
                            return true;
                        case SimpleTypes.Date:
                        case SimpleTypes.DateTime:
                            GetBoolResultOut = CompareAsDateTime;
                            (StrOper as ConstExpr).PrepareFor(SimpleTypes.DateTime);
                            return true;
                        case SimpleTypes.Time:
                            GetBoolResultOut = CompareAsTime;
                            (StrOper as ConstExpr).PrepareFor(SimpleTypes.Time);
                            return true;
                    }
                }
            }
            if (t1 == SimpleTypes.String && t2 == SimpleTypes.String)
            {
                GetBoolResultOut = CompareAsStr;
                return true;
            }
            if (t1 == SimpleTypes.Integer && t2 == SimpleTypes.Integer)
            {
                GetBoolResultOut = CompareAsInt;
                return true;
            }
            if ((t1 == SimpleTypes.Integer || t1 == SimpleTypes.Float)
                && (t2 == SimpleTypes.Integer || t2 == SimpleTypes.Float))
            {
                GetBoolResultOut = CompareAsFloat;
                return true;
            }
            if ((t1 == SimpleTypes.Date || t1 == SimpleTypes.DateTime) && (t2 == SimpleTypes.Date || t2 == SimpleTypes.DateTime))
            {
                GetBoolResultOut = CompareAsDateTime;
                return true;
            }
            if (t1 == SimpleTypes.Time && t2 == SimpleTypes.Time)
            {
                GetBoolResultOut = CompareAsTime;
                return true;
            }
            return false;
        }
        public override void Prepare()
        {
            base.Prepare();
            if (!DoPrepare()) TypesException();
        }
        protected abstract bool CompareAsInt(object data);
        protected abstract bool CompareAsFloat(object data);
        protected abstract bool CompareAsDateTime(object data);
        protected abstract bool CompareAsTime(object data);
        protected abstract bool CompareAsStr(object data);

        public override int Priority() { return PriorityConst.Compare; }
    }

}
