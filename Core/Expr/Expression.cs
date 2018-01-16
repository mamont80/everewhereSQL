using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore.Expr.Simple;

namespace ParserCore
{
    public delegate bool BoolResult(object data);
    public delegate long IntResult(object data);
    public delegate string StrResult(object data);

    public delegate double FloatResult(object data);
    public delegate DateTime DateTimeResult(object data);//отвечает за Date и DateTime
    public delegate TimeSpan TimeResult(object data);
    public delegate object GeomResult(object data);

    public delegate byte[] BlobResult(object data);

    public abstract class Expression : SqlToken, ISqlConvertible
    {
        private SimpleTypes SimpleType = SimpleTypes.Unknow;
        public SimpleTypes GetResultType() { return SimpleType; }

        public virtual int GetCoordinateSystem()
        {
            if (ChildsCount() > 0)
            {
                int cs = -1;
                foreach (var exp in Childs)
                {
                    if (exp.GetResultType() == SimpleTypes.Geometry)
                    {
                        var cs1 = exp.GetCoordinateSystem();
                        if (cs == -1) cs = cs1;
                        else if (cs >= 0)
                        {
                            if (cs != cs1) cs = -2; //разные
                        }
                    }
                }
                return cs;
            }
            return -1;
        }

        public TokenList<Expression> Childs { get; protected set; }

        /// <summary>
        /// Это операция или значение
        /// </summary>
        public virtual bool IsOperation() { return true; }
        public virtual bool IsFunction() { return false; }
        /// <summary>
        /// Право ассоциированный оператор: унарный минус, NOT. Обычные операторы чередуются с выражениями. Типа: exp1 op1 exp2 op1 exp3
        /// Эта хрень означает что этот принцип не действует и может быть: op1 op2 op3 exp1
        /// </summary>
        public virtual bool IsLeftOperand() { return false; }

        //приоритет
        public virtual int Priority()
        {
            return PriorityConst.Default;
        }
        public override string ToString()
        {
            try
            {
                return GetType().Name + " "+ToStr();
            }
            catch 
            {
                return GetType().Name;
            }
        }
        /// <summary>
        /// Преобразует в SQL строку для использвания в РСУБД
        /// </summary>
        /// <param name="builder">Параметры для построения</param>
        /// <returns></returns>
        public override string ToSql(ExpressionSqlBuilder builder)
        {
            throw new Exception("Can not convert to SQL");
        }

        /// <summary>
        /// количество аргументов у операций. Для функций не обязательно его указывать. Можут быть 1 или 2
        /// </summary>
        public virtual int NumChilds() { return 0; }

        public void ClearChilds()
        {
            if (Childs == null) return;
            Childs.Clear();
        }

        public void AddChild(Expression child)
        {
            if (Childs == null) Childs = new TokenList<Expression>(this);
            Childs.Add(child);
        }
        public void AddInvertChild(Expression child)
        {
            if (Childs == null) Childs = new TokenList<Expression>(this);
            Childs.Insert(0, child);
        }

        public int ChildsCount()
        {
            if (Childs == null) return 0;
            return Childs.Count;
        }
        public Expression GetChild(int index)
        {
            if (Childs == null) return null;
            return Childs[index];
        }

        //Внешний вызов. Возвращает возможно преобразованное значение
        public BoolResult GetBoolResultOut = data => { throw new NotImplementedException(); };
        public IntResult GetIntResultOut = data => { throw new NotImplementedException(); };
        public StrResult GetStrResultOut = data => { throw new NotImplementedException(); };

        public FloatResult GetFloatResultOut = data => { throw new NotImplementedException(); };
        public DateTimeResult GetDateTimeResultOut = data => { throw new NotImplementedException(); };
        public TimeResult GetTimeResultOut = data => { throw new NotImplementedException(); };
        public GeomResult GetGeomResultOut = data => { throw new NotImplementedException(); };
        public BlobResult GetBlobResultOut = data => { throw new NotImplementedException(); };

        /// <summary>
        /// Возвращает ли выражение null. Сделана только частичная поддержка третичной логики с null. Только для сравнения [column] is null
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual bool GetNullResultOut(object data)
        {
            return false;
        }

        /// <summary>
        /// Возвращает значение выражения в универсальном виде - объекте
        /// </summary>
        public object GetObjectResultOut(object data)
        {
            switch (GetResultType())
            {
                case SimpleTypes.Boolean: 
                    return GetBoolResultOut(data);
                case SimpleTypes.Date:
                case SimpleTypes.DateTime:
                    return GetDateTimeResultOut(data);
                case SimpleTypes.Float:
                    return GetFloatResultOut(data);
                case SimpleTypes.Geometry:
                    return GetGeomResultOut(data);
                case SimpleTypes.Integer:
                    return GetIntResultOut(data);
                case SimpleTypes.String:
                    return GetStrResultOut(data);
                case SimpleTypes.Time:
                    return GetTimeResultOut(data);
                case SimpleTypes.Blob:
                    return GetBlobResultOut(data);
                default:
                    throw new Exception("Unknow type result expression");
            }
        }

        protected void ErrorWrongNumberParams(int mustParams)
        {
            if (mustParams != ChildsCount()) throw new Exception("Wrong number parameters");
        }

        protected void TypesException() { throw new Exception("incompatible types"); }
        protected string ToSqlException()
        {
            throw new Exception("can not convert to SQL");
        }
        protected void CoordinateSystemIncompatibleException() { throw new Exception("incompatible coordinate systems"); }
        protected void CoordinateSystemUnknowException() { throw new Exception("unknow coordinate systems"); }
        protected void OperandOnlyConstException(int num_param) { throw new Exception("operand number " + num_param.ToString() + " constant"); }

        protected void OperandNotFoundException() { throw new Exception("operand not found"); }

        protected void ThrowException(string message)
        {
            throw new Exception(message);
        }

        public virtual void ParseInside(ExpressionParser parser)
        {
            parser.Collection.GotoNext();
        }

        public Expression PrepareAndOptimize()
        {
            Prepare();
            return Optimize();
        }

        /// <summary>
        /// Замена подвыражений на константы там где это возможно
        /// </summary>
        public Expression Optimize()
        {
            bool ch = true;
            Expression exp = this;
            exp = exp.DoOptimize(out ch);
            return exp;
        }

        protected Expression DoOptimize(out bool changed)
        {
            changed = false;
            if (Childs != null)
            {
                for (int i = 0; i < Childs.Count; i++)
                {
                    bool ch;
                    Childs[i] = Childs[i].DoOptimize(out ch);
                    if (ch) changed = true;
                }
            }
            if (OnlyOnline() && !(this is ConstExpr))
            {
                ConstExpr ce = CreateConst();
                switch (GetResultType())
                {
                    case SimpleTypes.Boolean:
                        ce.Init(GetBoolResultOut(null), SimpleTypes.Boolean);
                        break;
                    case SimpleTypes.Date:
                        ce.Init(GetDateTimeResultOut(null), SimpleTypes.Date);
                        break;
                    case SimpleTypes.DateTime:
                        ce.Init(GetDateTimeResultOut(null), SimpleTypes.DateTime);
                        break;
                    case SimpleTypes.Float:
                        ce.Init(GetFloatResultOut(null), SimpleTypes.Float);
                        break;
                    case SimpleTypes.String:
                        ce.Init(GetStrResultOut(null), SimpleTypes.String);
                        break;
                    case SimpleTypes.Time:
                        ce.Init(GetTimeResultOut(null), SimpleTypes.Time);
                        break;
                    case SimpleTypes.Integer:
                        ce.Init(GetIntResultOut(null), SimpleTypes.Integer);
                        break;
                    case SimpleTypes.Blob:
                        ce.Init(GetBlobResultOut(null), SimpleTypes.Blob);
                        break;
                    case SimpleTypes.Geometry:
                        ce.Init(GetGeomResultOut(null), SimpleTypes.Geometry, GetCoordinateSystem());
                        break;
                }
                changed = true;
                return ce;
            }
            return this;
        }

        protected virtual ConstExpr CreateConst()
        {
            return new ConstExpr();
        }

        /// <summary>
        /// Возвращает все выражения в дереве
        /// </summary>
        /// <returns></returns>
        public List<Expression> GetAllExpressions()
        {
            List<Expression> lst = new List<Expression>();
            IntGetAllExpressions(lst);
            return lst;
        }

        protected void IntGetAllExpressions(List<Expression> lst)
        {
            lst.Add(this);
            if (Childs != null)
            {
                foreach (var e in Childs)
                {
                    e.IntGetAllExpressions(lst);
                }
            }
        }

        public bool OnlyOnline()
        {
            if (!this.CanCalcOnline()) return false;
            else
            {
                if (Childs != null)
                {
                    foreach (Expression e in Childs) if (e.OnlyOnline() == false) return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Можно ли посчитать "на лету" (true), или нужно обращение у курсору (false)
        /// </summary>
        protected virtual bool CanCalcOnline() { return true; }

        /// <summary>
        /// Подготавливает выражение: определяет типы выражений, сверяет их, выставля
        /// </summary>
        public override void Prepare()
        {
            if (Childs != null)
            {
                foreach (Expression e in Childs) e.Prepare();
            }
        }

        //{ Integer = 1, Float = 2, String = 3, Geometry = 4, Date = 5, DateTime = 6, Time = 7, Boolean = 8 }
        protected void SortType(ref SimpleTypes tp1, ref SimpleTypes tp2)
        {
            SimpleTypes tmp;
            if (tp1 > tp2) { tmp = tp1; tp1 = tp2; tp2 = tmp; }
        }

        public override IExplore Expolore(DelegateExpessionExplorer del)
        {
            if (Childs != null)
            {
                TokenList<Expression> Childs2 = new TokenList<Expression>(this);
                foreach (var e in Childs)
                {
                    Expression e2 = (Expression)e.Expolore(del);
                    if (e2 != null) Childs2.Add(e2);
                }
                Childs = Childs2;
            }
            return base.Expolore(del);
        }

        #region Преобразования типов
        protected void SetResultType(SimpleTypes resultType)
        {
            SimpleType = resultType;
            switch (resultType)
            {
                case SimpleTypes.Boolean:
                    GetIntResultOut = GetBoolAsInt;
                    GetStrResultOut = GetBoolAsStr;
                    GetFloatResultOut = GetBoolAsFloat;
                    GetDateTimeResultOut = GetBoolAsDateTime;
                    GetTimeResultOut = GetBoolAsTime;
                    GetGeomResultOut = GetBoolAsGeom;
                    break;
                case SimpleTypes.String:
                    GetBoolResultOut = GetStrAsBool;
                    GetIntResultOut = GetStrAsInt;
                    GetFloatResultOut = GetStrAsFloat;
                    GetDateTimeResultOut = GetStrAsDateTime;
                    GetTimeResultOut = GetStrAsTime;
                    GetGeomResultOut = GetStrAsGeom;
                    break;
                case SimpleTypes.Integer:
                    GetBoolResultOut = GetIntAsBool;
                    GetStrResultOut = GetIntAsStr;
                    GetFloatResultOut = GetIntAsFloat;
                    GetDateTimeResultOut = GetIntAsDateTime;
                    GetTimeResultOut = GetIntAsTime;
                    GetGeomResultOut = GetIntAsGeom;
                    break;
                case SimpleTypes.Float:
                    GetBoolResultOut = GetFloatAsBool;
                    GetStrResultOut = GetFloatAsStr;
                    GetIntResultOut = GetFloatAsInt;
                    GetDateTimeResultOut = GetFloatAsDateTime;
                    GetTimeResultOut = GetFloatAsTime;
                    GetGeomResultOut = GetFloatAsGeom;
                    break;
                case SimpleTypes.Date:
                    GetBoolResultOut = GetDateTimeAsBool;
                    GetStrResultOut = GetDateAsStr;
                    GetFloatResultOut = GetDateTimeAsFloat;
                    GetIntResultOut = GetDateTimeAsInt;
                    GetTimeResultOut = GetDateTimeAsTime;
                    GetGeomResultOut = GetDateTimeAsGeom;
                    break;
                case SimpleTypes.DateTime:
                    GetBoolResultOut = GetDateTimeAsBool;
                    GetStrResultOut = GetDateTimeAsStr;
                    GetFloatResultOut = GetDateTimeAsFloat;
                    GetIntResultOut = GetDateTimeAsInt;
                    GetTimeResultOut = GetDateTimeAsTime;
                    GetGeomResultOut = GetDateTimeAsGeom;
                    break;
                case SimpleTypes.Time:
                    GetBoolResultOut = GetTimeAsBool;
                    GetStrResultOut = GetTimeAsStr;
                    GetFloatResultOut = GetTimeAsFloat;
                    GetIntResultOut = GetTimeAsInt;
                    GetDateTimeResultOut = GetTimeAsDateTime;
                    GetGeomResultOut = GetTimeAsGeom;
                    break;
                case SimpleTypes.Geometry:
                    GetBoolResultOut = GetGeomAsBool;
                    GetStrResultOut = GetGeomAsStr;
                    GetFloatResultOut = GetGeomAsFloat;
                    GetIntResultOut = GetGeomAsInt;
                    GetDateTimeResultOut = GetGeomAsDateTime;
                    GetTimeResultOut = GetGeomAsTime;
                    break;
                case SimpleTypes.Blob:
                    GetBoolResultOut = GetBlobAsBool;
                    GetStrResultOut = GetBlobAsStr;
                    GetFloatResultOut = GetBlobAsFloat;
                    GetIntResultOut = GetBlobAsInt;
                    GetDateTimeResultOut = GetBlobAsDateTime;
                    GetTimeResultOut = GetBlobAsTime;
                    break;
            }
        }
        // из Blob
        protected bool GetBlobAsBool(object data)
        {
            throw new NotImplementedException();
        }
        protected long GetBlobAsInt(object data)
        {
            throw new NotImplementedException();
        }
        protected string GetBlobAsStr(object data)
        {
            throw new NotImplementedException();
        }
        protected double GetBlobAsFloat(object data)
        {
            throw new NotImplementedException();
        }
        protected DateTime GetBlobAsDateTime(object data)
        {
            throw new NotImplementedException();
        }
        protected TimeSpan GetBlobAsTime(object data)
        {
            throw new NotImplementedException();
        }
        protected object GetBlobAsGeom(object data)
        {
            throw new NotImplementedException();
        }
        // из Bool
        protected long GetBoolAsInt(object data)
        {
            return GetBoolResultOut(data) ? 1 : 0;
        }
        protected string GetBoolAsStr(object data)
        {
            return GetBoolResultOut(data).ToString();
        }
        protected double GetBoolAsFloat(object data)
        {
            return GetBoolResultOut(data) ? 1 : 0;
        }
        protected DateTime GetBoolAsDateTime(object data)
        {
            return DateTime.MinValue;
        }
        protected TimeSpan GetBoolAsTime(object data)
        {
            return TimeSpan.FromDays(0);
        }
        protected object GetBoolAsGeom(object data)
        {
            return null;
        }
        // из Int
        protected string GetIntAsStr(object data)
        {
            return GetIntResultOut(data).ToString();
        }
        protected bool GetIntAsBool(object data)
        {
            return GetIntResultOut(data) == 0 ? false : true;
        }
        protected double GetIntAsFloat(object data)
        {
            return (double)GetIntResultOut(data);
        }
        protected DateTime GetIntAsDateTime(object data)
        {
            return CommonUtils.ConvertFromUnixTimestamp(GetIntResultOut(data));
        }
        protected TimeSpan GetIntAsTime(object data)
        {
            return CommonUtils.ConvertFromGeomixerTime((int)GetIntResultOut(data));
        }
        protected object GetIntAsGeom(object data)
        {
            return null;
        }

        // из Float
        protected string GetFloatAsStr(object data)
        {
            return GetFloatResultOut(data).ToStr();
        }
        protected bool GetFloatAsBool(object data)
        {
            return GetFloatResultOut(data) == 0 ? false : true;
        }
        protected long GetFloatAsInt(object data)
        {
            return (long)GetFloatResultOut(data);
        }
        protected DateTime GetFloatAsDateTime(object data)
        {
            return CommonUtils.ConvertFromUnixTimestamp(GetFloatResultOut(data));
        }
        protected TimeSpan GetFloatAsTime(object data)
        {
            return CommonUtils.ConvertFromGeomixerTime((int)GetFloatResultOut(data));
        }
        protected object GetFloatAsGeom(object data)
        {
            return null;
        }

        // из String
        protected long GetStrAsInt(object data)
        {
            long res = 0;
            if (long.TryParse(GetStrResultOut(data), out res)) return res; else return 0;
        }
        protected bool GetStrAsBool(object data)
        {
            return GetStrResultOut(data).ToLower() == "true" ? true : false;
        }
        protected double GetStrAsFloat(object data)
        {
            double v;
            return GetStrResultOut(data).TryParseDouble(out v) ? v : 0;
        }
        protected DateTime GetStrAsDateTime(object data)
        {
            DateTime dt = DateTime.MinValue;
            ParserDateTimeStatus pt = CommonUtils.ParseDateTime(GetStrResultOut(data), out dt);
            if (pt == ParserDateTimeStatus.Date || pt == ParserDateTimeStatus.DateTime) return dt; else return DateTime.MinValue;
        }
        protected TimeSpan GetStrAsTime(object data)
        {
            DateTime dt = DateTime.MinValue;
            ParserDateTimeStatus pt = CommonUtils.ParseDateTime(GetStrResultOut(data), out dt);
            if (pt == ParserDateTimeStatus.Time || pt == ParserDateTimeStatus.DateTime) return dt.TimeOfDay; else return TimeSpan.FromDays(0);
        }
        protected object GetStrAsGeom(object data)
        {
            return null;
        }

        // из DateTime
        protected string GetDateAsStr(object data)
        {
            return GetDateTimeResultOut(data).ToString("dd.MM.yyyy");
        }
        protected string GetDateTimeAsStr(object data)
        {
            return GetDateTimeResultOut(data).ToString();
        }
        protected bool GetDateTimeAsBool(object data)
        {
            return false;
        }
        protected double GetDateTimeAsFloat(object data)
        {
            return CommonUtils.ConvertToUnixTimestamp(GetDateTimeResultOut(data));
        }
        protected long GetDateTimeAsInt(object data)
        {
            return CommonUtils.ConvertToUnixTimestamp(GetDateTimeResultOut(data));
        }
        protected TimeSpan GetDateTimeAsTime(object data)
        {
            return GetDateTimeResultOut(data).TimeOfDay;
        }
        protected object GetDateTimeAsGeom(object data)
        {
            return null;
        }

        // из Time
        protected string GetTimeAsStr(object data)
        {
            return GetTimeResultOut(data).ToString();
        }
        protected bool GetTimeAsBool(object data)
        {
            return false;
        }
        protected double GetTimeAsFloat(object data)
        {
            return CommonUtils.ConvertToGeomixerTime(GetTimeResultOut(data));
        }
        protected long GetTimeAsInt(object data)
        {
            return CommonUtils.ConvertToGeomixerTime(GetTimeResultOut(data));
        }
        protected DateTime GetTimeAsDateTime(object data)
        {
            return new DateTime(1970, 1, 1).Add(GetTimeResultOut(data));
        }
        protected object GetTimeAsGeom(object data)
        {
            return null;
        }

        // из Geometry
        protected string GetGeomAsStr(object data)
        {
            return "";
        }
        protected bool GetGeomAsBool(object data)
        {
            return false;
        }
        protected double GetGeomAsFloat(object data)
        {
            return 0;
        }
        protected long GetGeomAsInt(object data)
        {
            return 0;
        }
        protected DateTime GetGeomAsDateTime(object data)
        {
            return DateTime.MinValue;
        }
        protected TimeSpan GetGeomAsTime(object data)
        {
            return TimeSpan.FromDays(0);
        }

        #endregion
    }

}
