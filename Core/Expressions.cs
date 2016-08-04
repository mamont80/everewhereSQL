using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
/*
 *   Новая версия лексем с возможностью быстрого вычисления результата
 *   
 * Из описания SQL Server 
8
~ (побитовое НЕ)
7
* (умножение), / (деление), % (остаток от деления)
6
+ (положительное), - (отрицательное), + (сложение), (+ объединение), - (вычитание), & (побитовое И), ^ (побитовое исключающее ИЛИ), | (побитовое ИЛИ)
5
=, >, <, >=, <=, <>, !=, !>, !< (операторы сравнения)
4
NOT
3
And
2
ALL, ANY, BETWEEN, IN, LIKE, OR, SOME
1
= (присваивание)
 * 
 *   Приоритеты операций:
 *   * /            300
 *   + - UiarMinus    200
 *   > >= < <=      120
 *   <> =          110
 *   ( )            1
 *   and or not     100
 *   contains 150
 */
namespace ParserCore
{

    public static class PriorityConst
    {
        public const int UnarMinus = 800;
        /// <summary>
        /// * (умножение), / (деление), % (остаток от деления)
        /// </summary>
        public const int MultiDiv = 700;
        /// <summary>
        /// + (положительное), - (отрицательное), + (сложение), (+ объединение), - (вычитание), & (побитовое И), ^ (побитовое исключающее ИЛИ), | (побитовое ИЛИ)
        /// </summary>
        public const int PlusMinus = 600;

        public const int Is = 590;

        public const int Default = 580;
        
        public const int In = 560;
        /// <summary>
        /// like contain
        /// </summary>
        public const int Like = 550;
        /// <summary>
        /// =, >, <, >=, <=, <>, !=, !>, !< (операторы сравнения)
        /// </summary>
        public const int Compare = 500;
        public const int Not = 400;
        public const int And = 300;
        public const int Or = 110;
        /// <summary>
        /// = (присваивание)
        /// </summary>
        public const int Assign = 100;
    }

    /// <summary>
    /// Используется как аргумент для построения SQL строки where из expression
    /// </summary>
    public class ExpressionSqlBuilder
    {

        public IDbDriver Driver;

        public int InternalTableAliasCounter = 0;
        /// <summary>
        /// Нужно ли ставить ковычки вокруг названия поля
        /// </summary>
        public bool FieldQuote = true;
        /// <summary>
        /// Нужно ли ставить кавычки вокруг названия таблицы
        /// </summary>
        public bool TableQuote = true;
    }

    public abstract class Expression:ICloneable
    {
        private SimpleTypes SimpleType;
        public SimpleTypes GetResultType() { return SimpleType; }

        protected int _CoordinateSystem;

        public virtual int GetCoordinateSystem()
        {
            return _CoordinateSystem;
        }

        public bool IsVirtualField { get; set; }

        public List<Expression> Childs = null;
        public virtual object Clone()
        {
            Expression ex = (Expression)this.MemberwiseClone();
            if (ex.Childs != null)
            {
                List<Expression> NewChilds = new List<Expression>(ex.Childs.Count);
                for (int i = 0; i < Childs.Count; i++) NewChilds.Add((Expression)ex.Childs[i].Clone());
                ex.Childs = NewChilds;
            }
            return ex;
        }
        /// <summary>
        /// Это операция или значение
        /// </summary>
        public virtual bool IsOperation() { return true; }
        public virtual bool IsFunction() { return false; }
        /// <summary>
        /// Право ассоциированный оператор: унарный минус, NOT. Обычные операторы чередуются с выражениями. Типа: exp1 op1 exp2 op1 exp3
        /// Эта хрень означает что этот принцип не действует и может быть: op1 op2 op3 exp1
        /// </summary>
        public virtual bool IsRightAssociate() { return false; }

        //приоритет
        public virtual int Priority()
        {
            return PriorityConst.Default;
        }
        /// <summary>
        /// Преобразует в SQL строку для использвания в РСУБД
        /// </summary>
        /// <param name="builder">Параметры для построения</param>
        /// <returns></returns>
        public virtual string ToSQL(ExpressionSqlBuilder builder)
        {
            return string.Empty;
        }
        /// <summary>
        /// Выдаёт выражение со скобками
        /// </summary>
        public abstract string ToStr();

        /// <summary>
        /// количество аргументов у операций. Для функций не обязательно его указывать. Можут быть 1 или 2
        /// </summary>
        public virtual int NumChilds() { return 0; }
        public void AddChild(Expression child)
        {
            if (Childs == null) Childs = new List<Expression>();
            Childs.Add(child);
        }
        public void AddInvertChild(Expression child)
        {
            if (Childs == null) Childs = new List<Expression>();
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
        public BoolResult GetBoolResultOut;
        public IntResult GetIntResultOut;
        public StrResult GetStrResultOut;

        public FloatResult GetFloatResultOut;
        public DateTimeResult GetDateTimeResultOut;
        public TimeResult GetTimeResultOut;
        public GeomResult GetGeomResultOut;

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
                default:
                    throw new Exception("Unknow type result expression");
            }
        }

        protected void TypesException() { throw new Exception("incompatible types"); }
        protected void CoordinateSystemIncompatibleException() { throw new Exception("incompatible coordinate systems"); }
        protected void CoordinateSystemUnknowException() { throw new Exception("unknow coordinate systems"); }
        protected void OperandOnlyConstException(int num_param) { throw new Exception("operand number " + num_param.ToString() + " constant"); }

        protected void OperandNotFoundException() { throw new Exception("operand not found"); }

        /// <summary>
        /// Подготавливает выражение: определяет типы выражений, сверяет их, выставля
        /// </summary>
        public void Prepare()
        {
            BeforePrepare();
        }

        public Expression PrepareAndOptimize()
        {
            Prepare();
            return Optimize();
        }

        /// <summary>
        /// Замена подвыражений на константы там где это возможно
        /// </summary>
        protected Expression Optimize()
        {
            bool ch = true;
            Expression exp = this;
            while (ch)
            {
                exp = exp.DoOptimize(out ch);
                if (ch) exp.Prepare();   
            }
            return exp;
        }

        protected virtual Expression DoOptimize(out bool changed)
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
            if (!OnlyOnline())
            {
                DoPrepareCoordinateSystem();
            }
            if (OnlyOnline() && !(this is ConstExpr))
            {
                ConstExpr ce = new ConstExpr();
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
                    case SimpleTypes.Geometry:
                        ce.Init(GetGeomResultOut(null), SimpleTypes.Geometry, GetCoordinateSystem());
                        break;
                }
                changed = true;
                return ce;
            }
            return this;
        }
        
        protected virtual bool DoPrepareCoordinateSystem()
        {
            // TODO: Fix for geomixer
            return true;
            /*
            //вычисляем систему координат колонок (её не трогаем)
            int csStatic = null;
            CoordinateSystemFull firstConst = null;
            if (Childs == null) return false;
            for (int i = 0; i < Childs.Count; i++)
            {
                Expression exp = Childs[i];
                if (exp.GetResultType() == ColumnSimpleTypes.Geometry)
                {
                    if (!exp.CanCalcOnline())
                    {
                        if (csStatic == null) csStatic = exp.GetCoordinateSystem();
                        else
                        {
                            if (!csStatic.IsEqual(exp.GetCoordinateSystem())) throw new Exception("Can not transform geometry fields online");
                        }
                    }
                    else
                    {
                        if (firstConst == null) firstConst = exp.GetCoordinateSystem();
                    }
                }
            }
            CoordinateSystemFull target = null;
            //выбираем одну целевую проекцию
            if (csStatic != null) target = csStatic;
                else target = firstConst;
            if (target == null) return false;
            //перекодируем геометрии-константы
            bool transformed = false;
            for (int i = 0; i < Childs.Count; i++)
            {
                Expression exp = Childs[i];
                if (exp is ConstExpr && exp.GetResultType() == ColumnSimpleTypes.Geometry && !exp.GetCoordinateSystem().IsEqual(target))
                {
                    ConstExpr ce = (ConstExpr)exp;
                    Geometry g1 = ce.GetGeomResultOut(null);
                    g1 = CoordSystemConverter.QuickTransform(g1, ce.GetCoordinateSystem(), target);
                    if (!g1.MyIsValid()) g1 = g1.MyMakeValid();
                    ce.Init(g1, ColumnSimpleTypes.Geometry, target);
                    transformed = true;
                }
            }
            return transformed;
             */
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

        protected virtual void BeforePrepare() 
        {
            if (Childs != null)
            {
                foreach (Expression e in Childs) e.BeforePrepare();
            }
        }

        //{ Integer = 1, Float = 2, String = 3, Geometry = 4, Date = 5, DateTime = 6, Time = 7, Boolean = 8 }
        protected void SortType(ref SimpleTypes tp1, ref SimpleTypes tp2)
        {
            SimpleTypes tmp;
            if (tp1 > tp2) { tmp = tp1; tp1 = tp2; tp2 = tmp; }
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
            }
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

    public delegate bool BoolResult(object data);
    public delegate long IntResult(object data);
    public delegate string StrResult(object data);

    public delegate double FloatResult(object data);
    public delegate DateTime DateTimeResult(object data);//отвечает за Date и DateTime
    public delegate TimeSpan TimeResult(object data);
    public delegate object GeomResult(object data);

    public class ConstExpr : Expression
    {
        protected bool valueBool;
        protected long valueInt;
        protected string valueStr;
        protected double valueFloat;
        protected object valueGeom;
        protected DateTime valueDateTime;
        protected TimeSpan valueTime;

        /// <summary>
        /// Это операция или значение
        /// </summary>
        public override bool IsOperation() { return false; }

        public void Init(object val, SimpleTypes type)
        {
            Init(val, type, 0);
        }

        public void Init(object val, SimpleTypes type, int csf)
        {
            //проводим инициализацию и сразу подготовку
            #region Хитрое выставление типов
            switch (type)
            {
                case SimpleTypes.Boolean:
                    valueBool = CommonUtils.Convert<bool>(val);
                    GetBoolResultOut = AsBool;
                    SetResultType(SimpleTypes.Boolean);
                    break;
                case SimpleTypes.String:
                    valueStr = CommonUtils.Convert<string>(val);
                    GetStrResultOut = AsStr;
                    SetResultType(SimpleTypes.String);
                    break;
                case SimpleTypes.Integer:
                    valueInt = CommonUtils.Convert<long>(val);
                    GetIntResultOut = AsInt;
                    SetResultType(SimpleTypes.Integer);
                    break;
                case SimpleTypes.Float:
                    valueFloat = CommonUtils.Convert<double>(val);
                    SetResultType(SimpleTypes.Float);
                    GetFloatResultOut = AsFloat;
                    break;
                case SimpleTypes.DateTime:
                case SimpleTypes.Date:
                    valueDateTime = CommonUtils.Convert<DateTime>(val);
                    SetResultType(SimpleTypes.DateTime);
                    GetDateTimeResultOut = AsDateTime;
                    break;
                case SimpleTypes.Time:
                    valueTime = CommonUtils.Convert<TimeSpan>(val);
                    SetResultType(SimpleTypes.Time);
                    GetTimeResultOut = AsTime;
                    break;
                case SimpleTypes.Geometry:
                    valueGeom = val;
                    SetResultType(SimpleTypes.Geometry);
                    GetGeomResultOut = AsGeom;
                    _CoordinateSystem = csf;
                    break;
            }
            #endregion
        }

        public override string ToStr()
        {
            switch (GetResultType())
            {
                case SimpleTypes.Boolean:
                    return GetBoolResultOut(null).ToString();
                case SimpleTypes.String:
                    return BaseExpressionFactory.StandartCodeEscape(GetStrResultOut(null).ToString(), '\'', '\'');
                case SimpleTypes.Integer:
                    return GetIntResultOut(null).ToString();
                case SimpleTypes.Float:
                    return GetFloatResultOut(null).ToStr();
                case SimpleTypes.DateTime:
                    return "StrToDateTime('" + GetDateTimeResultOut(null).ToString("dd.MM.yyyy HH:mm:ss") + "')";
                case SimpleTypes.Date:
                    return "StrToDateTime('" + GetDateTimeResultOut(null).ToString("dd.MM.yyyy") + "')";
                case SimpleTypes.Time:
                    return "StrToTime('" + GetTimeResultOut(null).ToString("c") + "')";
                case SimpleTypes.Geometry:
                    // TODO: FIXed!
                    throw new Exception("Can not convert geometry constant to string");
                    //return "_Geometry_";
                    /*Geometry g = GetGeomResultOut(null);
                    if (g == null) g = new Geometry(wkbGeometryType.wkbPolygon);
                    return "GeometryFromWkbHex(" + BaseExpressionFactory.StandartCodeEscape(CustomDbDriver.BytesToStr(g.MyExportToWKB()), '\'', '\'') +","+this.GetCoordinateSystem().EpsgCode.ToString()+ ")";
                     */
                default:
                    throw new Exception("Unknown data type");
            }
        }

        public void PrepareFor(SimpleTypes forType)
        {
            switch (forType)
            {
                case SimpleTypes.Boolean:
                    valueBool = GetBoolResultOut(null);
                    Init(valueBool, forType);
                    break;
                case SimpleTypes.String:
                    valueStr = GetStrResultOut(null);
                    Init(valueStr, forType);
                    break;
                case SimpleTypes.Integer:
                    valueInt = GetIntResultOut(null);
                    Init(valueInt, forType);
                    //GetIntResultOut = AsInt;
                    break;
                case SimpleTypes.Float:
                    valueFloat = GetFloatResultOut(null);
                    Init(valueFloat, forType);
                    break;
                case SimpleTypes.DateTime:
                case SimpleTypes.Date:
                    valueDateTime = GetDateTimeResultOut(null);
                    Init(valueDateTime, forType);
                    //GetDateTimeResultOut = AsDateTime;
                    break;
                case SimpleTypes.Time:
                    valueTime = GetTimeResultOut(null);
                    Init(valueTime, forType);
                    break;
                default:
                    throw new Exception("Uncapabilities types");
            }
        }

        private bool AsBool(object data) { return valueBool; }
        private string AsStr(object data) { return valueStr; }
        private long AsInt(object data) { return valueInt; }
        private DateTime AsDateTime(object data) { return valueDateTime; }
        private TimeSpan AsTime(object data) { return valueTime; }
        private double AsFloat(object data) { return valueFloat; }
        private object AsGeom(object data) { return valueGeom; }

        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return builder.Driver.ToSql(this, builder);
        }
    }

    public class NullConstExpr : ConstExpr
    {
        public NullConstExpr() : base()
        {
            Init("", SimpleTypes.String);
        }

        public override bool GetNullResultOut(object data)
        {
            return true;
        }

        public override string ToStr()
        {
            return "null";
        }

        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return "null";
        }
    }

    public class IsExpr : Custom_TwoOperand
    {
        public override int Priority()
        {
            return PriorityConst.Is;
        }

        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (!(Operand2 is NullConstExpr)) throw new Exception("allowed only the expression \"is null\"");
            GetBoolResultOut = GetResult;
            SetResultType(SimpleTypes.Boolean);
        }

        private bool GetResult(object data)
        {
            return Operand1.GetNullResultOut(data);
        }

        public override string ToStr()
        {
            return Operand1.ToStr() + " is null";
        }

        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return Operand1.ToSQL(builder) + " is " + Operand2.ToSQL(builder);
        }
    }

    public class IsNotNullExpr : Custom_OneOperand
    {
        public override int Priority()
        {
            return PriorityConst.Is;
        }
        public override bool IsRightAssociate() { return true; }
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            GetBoolResultOut = GetResult;
            SetResultType(SimpleTypes.Boolean);
        }

        private bool GetResult(object data)
        {
            return !Operand.GetNullResultOut(data);
        }

        public override string ToStr()
        {
            return Operand.ToStr() + " is not null";
        }

        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return Operand.ToSQL(builder) + " is not null";
        }
    }

    public class IsNullExpr : IsNotNullExpr
    {
        private bool GetResult(object data)
        {
            return Operand.GetNullResultOut(data);
        }

        public override string ToStr()
        {
            return Operand.ToStr() + " is null";
        }

        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return Operand.ToSQL(builder) + " is null";
        }
    }

    /// <summary>
    /// Класс переменной. Эта фиговина пока толком не реализована и не используется. Сделать как понадобится.
    /// </summary>
    public class VariableExpr : ConstExpr
    {
        public string VariableName;
        /// <summary>
        /// Это операция или значение
        /// </summary>
        public override bool IsOperation() { return false; }
        public override string ToStr() { return VariableName; } 

    }

    public class InExpr : Custom_TwoOperand
    {
        public override bool IsOperation() { return true; }
        public override bool IsFunction() { return false; }
        public override int NumChilds() { return 2; }
        public override int Priority()
        {
            return PriorityConst.In;
        }

        public delegate bool BoolItemResult(Expression Operand1, Expression Operand2, object data);

        protected BoolItemResult CompareItem;

        protected override void BeforePrepare()
        {
            if (Childs.Count != 2) throw new Exception("invalid IN operation");
            if (!(Childs[1] is SubExpression)) throw new Exception("After IN operation wait '()'");
            ((SubExpression) (Childs[1])).ForInExpression = true;
            base.BeforePrepare();

            SetResultType(SimpleTypes.Boolean);
            if (Childs[1].ChildsCount() == 1 && Childs[1].Childs[0] is SelectExpresion) return;

            List<SimpleTypes> types = new List<SimpleTypes>();
            for (int i = 0; i < Childs[1].Childs.Count; i++)
            {
                var tp = CustomEqual.GetCompareType(Childs[0], Childs[1].Childs[i]);
                if (tp == null) TypesException();
                types.Add(tp.Value);
            }
            types = types.Distinct().ToList();
            if (types.Count == 0 || types.Count > 2) TypesException();
            SimpleTypes t = types[0];
            if (types.Count == 2)
            {
                if ((types[0] == SimpleTypes.Float && types[1] == SimpleTypes.Integer) || (types[1] == SimpleTypes.Float && types[0] == SimpleTypes.Integer))
                {
                    t = SimpleTypes.Float;
                }else TypesException();
            }
            //CompareItem
            switch (t)
            {
                case SimpleTypes.Boolean:
                    CompareItem = CompareAsBool;
                    break;
                case SimpleTypes.Date:
                case SimpleTypes.DateTime:
                    CompareItem = CompareAsDateTime;
                    break;
                case SimpleTypes.Float:
                    CompareItem = CompareAsFloat;
                    break;
                case SimpleTypes.Geometry:
                    throw new Exception("Can not compare geometries");
                //    CompareItem = CompareAsGeom;
                //    break;
                case SimpleTypes.Integer:
                    CompareItem = CompareAsInt;
                    break;
                case SimpleTypes.String:
                    CompareItem = CompareAsStr;
                    break;
                case SimpleTypes.Time:
                    CompareItem = CompareAsTime;
                    break;
            }

            GetBoolResultOut = GetResult;
        }

        protected virtual bool GetResult(object data)
        {
            for (int i = 0; i < Operand2.Childs.Count; i++)
            {
                if (CompareItem(Operand1, Operand2.Childs[i], data)) return true;
            }
            return false;
        }

        protected static bool CompareAsInt(Expression Operand1, Expression Operand2, object data) { return (Operand1.GetIntResultOut(data) == Operand2.GetIntResultOut(data)); }
        protected static bool CompareAsFloat(Expression Operand1, Expression Operand2, object data) { return (Operand1.GetFloatResultOut(data) == Operand2.GetFloatResultOut(data)); }
        protected static bool CompareAsDateTime(Expression Operand1, Expression Operand2, object data) { return (Operand1.GetDateTimeResultOut(data) == Operand2.GetDateTimeResultOut(data)); }
        protected static bool CompareAsTime(Expression Operand1, Expression Operand2, object data) { return (Operand1.GetTimeResultOut(data) == Operand2.GetTimeResultOut(data)); }
        protected static bool CompareAsBool(Expression Operand1, Expression Operand2, object data) { return (Operand1.GetBoolResultOut(data) == Operand2.GetBoolResultOut(data)); }
        //protected static bool CompareAsGeom(Expression Operand1, Expression Operand2, object data) { return (Operand1.GetGeomResultOut(data).Equal(Operand2.GetGeomResultOut(data))); }
        protected static bool CompareAsStr(Expression Operand1, Expression Operand2, object data) { return (Operand1.GetStrResultOut(data) == Operand2.GetStrResultOut(data)); }

        public override string ToStr()
        {
            string s = Operand1.ToStr() + " in " + Operand2.ToStr();
            return s;
        }
        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            string s = Operand1.ToSQL(builder) + " in " + Operand2.ToSQL(builder);
            return s;
        }
    }

    public class NotInExpr : InExpr
    {
        protected override bool GetResult(object data)
        {
            return !base.GetResult(data);
        }

        public override string ToStr()
        {
            string s = Operand1.ToStr() + " not in " + Operand2.ToStr();
            return s;
        }
        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            string s = Operand1.ToSQL(builder) + " not in " + Operand2.ToSQL(builder);
            return s;
        }
    }


    public class SubExpression : Expression
    {
        public override bool IsOperation() { return false; }
        public override bool IsFunction() { return false; }
        /// <summary>
        /// Эти скобки для операции: arg in (arg1, arg2, ...)
        /// </summary>
        public bool ForInExpression = false;

        protected override bool CanCalcOnline() //запрещаем оптимизировать если это для операции IN (1,2)
        {
            if (ForInExpression) return false;
            return true;
        }

        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (Childs.Count == 0 && !ForInExpression) throw new Exception("Empty subexpression");
            if (Childs.Count > 1 && !ForInExpression) throw new Exception("Invalid subexpression");
            if (Childs.Count == 1)
            {
                var tp = Childs[0].GetResultType();
                SetResultType(Childs[0].GetResultType());
                switch (tp)
                {
                    case SimpleTypes.Boolean:
                        GetBoolResultOut = CalcAsBool;
                        break;
                    case SimpleTypes.Date:
                    case SimpleTypes.DateTime:
                        GetDateTimeResultOut = CalcDateTimeResult;
                        break;
                    case SimpleTypes.Float:
                        GetFloatResultOut = CalcFloatResult;
                        break;
                    case SimpleTypes.Geometry:
                        GetGeomResultOut = CalcGeomResult;
                        break;
                    case SimpleTypes.Integer:
                        GetIntResultOut = CalcIntResult;
                        break;
                    case SimpleTypes.String:
                        GetStrResultOut = CalcStrResult;
                        break;
                    case SimpleTypes.Time:
                        GetTimeResultOut = CalcTimeResult;
                        break;
                }
            }
        }
        public override bool GetNullResultOut(object data) { return Childs[0].GetNullResultOut(data); }

        public bool CalcAsBool(object data) { return Childs[0].GetBoolResultOut(data); }
        public long CalcIntResult(object data) { return Childs[0].GetIntResultOut(data); }
        public string CalcStrResult(object data) { return Childs[0].GetStrResultOut(data); }
        public double CalcFloatResult(object data) { return Childs[0].GetFloatResultOut(data); }
        public DateTime CalcDateTimeResult(object data) { return Childs[0].GetDateTimeResultOut(data); }
        public TimeSpan CalcTimeResult(object data) { return Childs[0].GetTimeResultOut(data); }
        public object CalcGeomResult(object data) { return Childs[0].GetGeomResultOut(data); }

        public override int NumChilds() { return -1; }
        
        public override string ToStr()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            for (int i = 0; i < Childs.Count; i++)
            {
                if (i != 0) sb.Append(", ");
                sb.Append(Childs[i].ToStr());
            }
            sb.Append(")");
            return sb.ToString();
        }

        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            for (int i = 0; i < Childs.Count; i++)
            {
                if (i != 0) sb.Append(", ");
                sb.Append(Childs[i].ToSQL(builder));
            }
            sb.Append(")");
            return sb.ToString();
        }

    }

    /// <summary>
    /// Общий класс для всех сложных функций. К ним не относится Not()
    /// </summary>
    public class FuncExpr: Expression
    {
        public string FunctionName;
        //Функции идут как значения
        public override bool IsOperation() { return false; }
        public override bool IsFunction() { return true; }

        public override string ToStr() 
        {
            string s = FunctionName + "(";
            for(int i = 0; i < Childs.Count; i++)
            {
                if (i != 0) s += ",";
                s += Childs[i].ToStr();
            }
            s += ")";
            return s; 
        } 
    }

    public abstract class FuncExpr_WithoutOperand : FuncExpr
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
        }
        public override int NumChilds() { return 0; }
        public override int Priority() { return PriorityConst.Default; }
    }

    public abstract class FuncExpr_OneOperand : FuncExpr
    {
        public Expression Operand
        {
            get
            {
                if (Childs == null) return null;
                return Childs[0];
            }
        }
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (Childs == null || Childs.Count != 1) throw new Exception("Wrong number operands");
        }
        public override int NumChilds() { return 1; }
        public override int Priority() { return PriorityConst.Default; }
    }

    public abstract class FuncExpr_TwoOperand : FuncExpr
    {
        public Expression Operand1 {
            get { 
                if (Childs == null) return null;
                return Childs[0];
            }
        }
        public Expression Operand2
        {
            get
            {
                if (Childs == null) return null;
                return Childs[1];
            }
        }
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (Childs == null || Childs.Count != 2) throw new Exception("Wrong number operands");
        }
        public override int NumChilds() { return 2; }
    }

    public abstract class Custom_TwoOperand : Expression
    {
        public Expression Operand1 = null;
        public Expression Operand2 = null;
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (Childs == null ||Childs.Count != 2) throw new Exception("Wrong number operands");
            Operand1 = Childs[0];
            Operand2 = Childs[1];
        }
        public override int NumChilds() { return 2; }
    }

    public abstract class Custom_OneOperand : Expression
    {
        public Expression Operand;
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (Childs == null || Childs.Count != 1) throw new Exception("Wrong number operands");
                Operand = Childs[0];
        }
        public override int NumChilds() { return 1; }
    }

    /// <summary>
    /// Операция логического NOT
    /// </summary>
    public class Not_BoolExpr : Custom_OneOperand
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (Operand == null) OperandNotFoundException();
            if (!(Operand is NullConstExpr) && (Operand.GetResultType() != SimpleTypes.Boolean)) this.TypesException();
            GetBoolResultOut = DoNot;
            SetResultType(SimpleTypes.Boolean);
        }
        public override bool IsRightAssociate() { return true; }

        private bool DoNot(object data) { return !Operand.GetBoolResultOut(data); }
        public override int Priority() { return PriorityConst.Not; }
        public override string ToStr() { return "not(" + Operand.ToStr() + ")"; }
        public override string ToSQL(ExpressionSqlBuilder builder) { return " not(" + Operand.ToSQL(builder) + ")"; }
    }

    public class UniarMinus_BoolExpr : Custom_OneOperand
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (Operand == null) OperandNotFoundException();
            if (Operand.GetResultType() == SimpleTypes.Integer)
            {
                GetIntResultOut = DoMinusInt;
                SetResultType(SimpleTypes.Integer);
                return;
            }
            if (Operand.GetResultType() == SimpleTypes.Float)
            {
                GetFloatResultOut = DoMinusFloat;
                SetResultType(SimpleTypes.Float);
                return;
            }/*
            if (Operand.GetResultType() == ColumnSimpleTypes.Time)
            {
                GetTimeResultOut = DoMinusTime;
                SimpleType = ColumnSimpleTypes.Time;
                SetConvertors(ColumnSimpleTypes.Time);
                return;
            }*/
            TypesException();
        }
        protected virtual long DoMinusInt(object data) { return -Operand.GetIntResultOut(data); }
        protected virtual double DoMinusFloat(object data) { return -Operand.GetFloatResultOut(data); }
        //private TimeSpan DoMinusTime() { return -Operand.GetTimeResultOut(); }

        public override bool IsRightAssociate() { return true; }
        public override int Priority() { return PriorityConst.UnarMinus; }
        public override string ToStr() { return "-" + Operand.ToStr(); }
        public override string ToSQL(ExpressionSqlBuilder builder) { return " -(" + Operand.ToSQL(builder) + ")"; }
    }

    public class UniarPlus_BoolExpr : UniarMinus_BoolExpr
    {
        protected override long DoMinusInt(object data) { return Operand.GetIntResultOut(data); }
        protected override double DoMinusFloat(object data) { return Operand.GetFloatResultOut(data); }
        public override string ToStr() { return "+" + Operand.ToStr(); }
        public override string ToSQL(ExpressionSqlBuilder builder) { return " +(" + Operand.ToSQL(builder) + ")"; }
    }

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
                    switch(notStr)
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
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (!DoPrepare()) TypesException();
        }
        protected abstract bool CompareAsInt(object data);
        protected abstract bool CompareAsFloat(object data);
        protected abstract bool CompareAsDateTime(object data);
        protected abstract bool CompareAsTime(object data);
        protected abstract bool CompareAsStr(object data);

        public override int Priority() { return PriorityConst.Compare; }
    }
    /// <summary>
    /// общий класс для = !=
    /// </summary>
    public abstract class CustomEqual : Custom_TwoOperand
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
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
            base.BeforePrepare();
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
    /// <summary>
    /// Опреация РАВНО
    /// </summary>
    public class Equal_CompExpr : CustomEqual
    {
        protected override bool CompareAsInt(object data) { return (Operand1.GetIntResultOut(data) == Operand2.GetIntResultOut(data)); }
        protected override bool CompareAsFloat(object data) { return (Operand1.GetFloatResultOut(data) == Operand2.GetFloatResultOut(data)); }
        protected override bool CompareAsDateTime(object data) { return (Operand1.GetDateTimeResultOut(data) == Operand2.GetDateTimeResultOut(data)); }
        protected override bool CompareAsTime(object data) { return (Operand1.GetTimeResultOut(data) == Operand2.GetTimeResultOut(data)); }
        protected override bool CompareAsBool(object data) { return (Operand1.GetBoolResultOut(data) == Operand2.GetBoolResultOut(data)); }
        //protected override bool CompareAsGeom(object data) { return (Operand1.GetGeomResultOut(data).Equal(Operand2.GetGeomResultOut(data))); }
        protected override bool CompareAsStr(object data) { return (Operand1.GetStrResultOut(data) == Operand2.GetStrResultOut(data)); }

        public override string ToStr() { return "(" + Operand1.ToStr() + " = " + Operand2.ToStr() + ")"; }
        public override string ToSQL(ExpressionSqlBuilder builder) { return "(" + Operand1.ToSQL(builder) + " = " + Operand2.ToSQL(builder) + ")"; }
    }
    /// <summary>
    /// Опреация НЕ РАВНО
    /// </summary>
    public class NotEqual_CompExpr : CustomEqual
    {
        protected override bool CompareAsInt(object data) { return (Operand1.GetIntResultOut(data) != Operand2.GetIntResultOut(data)); }
        protected override bool CompareAsFloat(object data) { return (Operand1.GetFloatResultOut(data) != Operand2.GetFloatResultOut(data)); }
        protected override bool CompareAsDateTime(object data) { return (Operand1.GetDateTimeResultOut(data) != Operand2.GetDateTimeResultOut(data)); }
        protected override bool CompareAsTime(object data) { return (Operand1.GetTimeResultOut(data) != Operand2.GetTimeResultOut(data)); }
        protected override bool CompareAsBool(object data) { return (Operand1.GetBoolResultOut(data) != Operand2.GetBoolResultOut(data)); }
        //protected override bool CompareAsGeom(object data) { return !(Operand1.GetGeomResultOut(data).Equal(Operand2.GetGeomResultOut(data))); }
        protected override bool CompareAsStr(object data) { return (Operand1.GetStrResultOut(data) != Operand2.GetStrResultOut(data)); }
        public override string ToStr() { return "(" + Operand1.ToStr() + " <> " + Operand2.ToStr() + ")"; }
        public override string ToSQL(ExpressionSqlBuilder builder) { return "(" + Operand1.ToSQL(builder) + " <> " + Operand2.ToSQL(builder) + ")"; }
    }
    /// <summary>
    /// Опреация МЕНЬШЕ
    /// </summary>
    public class Less_CompExpr : Custom_CompareExpr
    {
        protected override bool CompareAsInt(object data) { return (Operand1.GetIntResultOut(data) < Operand2.GetIntResultOut(data)); }
        protected override bool CompareAsFloat(object data) { return (Operand1.GetFloatResultOut(data) < Operand2.GetFloatResultOut(data)); }
        protected override bool CompareAsDateTime(object data) { return (Operand1.GetDateTimeResultOut(data) < Operand2.GetDateTimeResultOut(data)); }
        protected override bool CompareAsTime(object data) { return (Operand1.GetTimeResultOut(data) < Operand2.GetTimeResultOut(data)); }
        protected override bool CompareAsStr(object data) { return (StringComparer.InvariantCultureIgnoreCase.Compare(Operand1.GetStrResultOut(data), Operand2.GetStrResultOut(data)) < 0); }
        public override string ToStr() { return "(" + Operand1.ToStr() + " < " + Operand2.ToStr() + ")"; }
        public override string ToSQL(ExpressionSqlBuilder builder) { return "(" + Operand1.ToSQL(builder) + " < " + Operand2.ToSQL(builder) + ")"; }
    }
    /// <summary>
    /// Опреация МЕНЬШЕ ИЛИ РАНО
    /// </summary>
    public class LessOrEqual_CompExpr : Custom_CompareExpr
    {
        protected override bool CompareAsInt(object data) { return (Operand1.GetIntResultOut(data) <= Operand2.GetIntResultOut(data)); }
        protected override bool CompareAsFloat(object data) { return (Operand1.GetFloatResultOut(data) <= Operand2.GetFloatResultOut(data)); }
        protected override bool CompareAsDateTime(object data) { return (Operand1.GetDateTimeResultOut(data) <= Operand2.GetDateTimeResultOut(data)); }
        protected override bool CompareAsTime(object data) { return (Operand1.GetTimeResultOut(data) <= Operand2.GetTimeResultOut(data)); }
        protected override bool CompareAsStr(object data) { return (StringComparer.InvariantCultureIgnoreCase.Compare(Operand1.GetStrResultOut(data), Operand2.GetStrResultOut(data)) <= 0); }
        public override string ToStr() { return "(" + Operand1.ToStr() + " <= " + Operand2.ToStr() + ")"; }
        public override string ToSQL(ExpressionSqlBuilder builder) { return "(" + Operand1.ToSQL(builder) + " <= " + Operand2.ToSQL(builder) + ")"; }
    }
    /// <summary>
    /// Опреация БОЛЬШЕ
    /// </summary>
    public class Great_CompExpr : Custom_CompareExpr
    {
        protected override bool CompareAsInt(object data) { return (Operand1.GetIntResultOut(data) > Operand2.GetIntResultOut(data)); }
        protected override bool CompareAsFloat(object data) { return (Operand1.GetFloatResultOut(data) > Operand2.GetFloatResultOut(data)); }
        protected override bool CompareAsDateTime(object data) { return (Operand1.GetDateTimeResultOut(data) > Operand2.GetDateTimeResultOut(data)); }
        protected override bool CompareAsTime(object data) { return (Operand1.GetTimeResultOut(data) > Operand2.GetTimeResultOut(data)); }
        protected override bool CompareAsStr(object data) { return (StringComparer.InvariantCultureIgnoreCase.Compare(Operand1.GetStrResultOut(data), Operand2.GetStrResultOut(data)) > 0); }
        public override string ToStr() { return "(" + Operand1.ToStr() + " > " + Operand2.ToStr() + ")"; }
        public override string ToSQL(ExpressionSqlBuilder builder) { return "(" + Operand1.ToSQL(builder) + " > " + Operand2.ToSQL(builder) + ")"; }
    }
    /// <summary>
    /// Опреация БОЛЬШЕ ИЛИ РАНО
    /// </summary>
    public class GreatOrEqual_CompExpr : Custom_CompareExpr
    {
        protected override bool CompareAsInt(object data) { return (Operand1.GetIntResultOut(data) >= Operand2.GetIntResultOut(data)); }
        protected override bool CompareAsFloat(object data) { return (Operand1.GetFloatResultOut(data) >= Operand2.GetFloatResultOut(data)); }
        protected override bool CompareAsDateTime(object data) { return (Operand1.GetDateTimeResultOut(data) >= Operand2.GetDateTimeResultOut(data)); }
        protected override bool CompareAsTime(object data) { return (Operand1.GetTimeResultOut(data) >= Operand2.GetTimeResultOut(data)); }
        protected override bool CompareAsStr(object data) { return (StringComparer.InvariantCultureIgnoreCase.Compare(Operand1.GetStrResultOut(data), Operand2.GetStrResultOut(data)) >= 0); }
        public override string ToStr() { return "(" + Operand1.ToStr() + " >= " + Operand2.ToStr() + ")"; }
        public override string ToSQL(ExpressionSqlBuilder builder) { return "(" + Operand1.ToSQL(builder) + " >= " + Operand2.ToSQL(builder) + ")"; }
    }

    public abstract class Custom_BoolExpr : Custom_TwoOperand
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (Operand1.GetResultType() != SimpleTypes.Boolean) this.TypesException();
            if (Operand2.GetResultType() != SimpleTypes.Boolean) this.TypesException();
            GetBoolResultOut = AsBool;
            SetResultType(SimpleTypes.Boolean);
        }
        protected abstract bool AsBool(object data);
    }
    /// <summary>
    /// Опреация логического AND
    /// </summary>
    public class And_BoolExpr : Custom_BoolExpr
    {
        protected override bool AsBool(object data) { return Operand1.GetBoolResultOut(data) && Operand2.GetBoolResultOut(data); }
        public override string ToStr() { return "("+Operand1.ToStr()+" and "+Operand2.ToStr() + ")"; }
        public override string ToSQL(ExpressionSqlBuilder builder) { return "(" + Operand1.ToSQL(builder) + " and " + Operand2.ToSQL(builder) + ")"; }
        public override int Priority() { return PriorityConst.And; }
    }
    /// <summary>
    /// Опреация логического OR
    /// </summary>
    public class Or_BoolExpr : Custom_BoolExpr
    {
        protected override bool AsBool(object data) { return Operand1.GetBoolResultOut(data) || Operand2.GetBoolResultOut(data); }
        public override string ToStr() { return "(" + Operand1.ToStr() + " or " + Operand2.ToStr() + ")"; }
        public override string ToSQL(ExpressionSqlBuilder builder) { return "(" + Operand1.ToSQL(builder) + " or " + Operand2.ToSQL(builder) + ")"; }
        public override int Priority() { return PriorityConst.Or; }
    }

    public abstract class Custom_Arifmetic : Custom_TwoOperand
    {
    }
    public class Plus_Arifmetic : Custom_Arifmetic
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
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

        public override string ToStr() { return "(" + Operand1.ToStr() + " + " + Operand2.ToStr() + ")"; }

        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return builder.Driver.ToSql(this, builder);
            //return "(" + Operand1.ToSQL(builder) + " + " + Operand2.ToSQL(builder) + ")";
        }
        public override int Priority() { return PriorityConst.PlusMinus; }
    }

    public abstract class Other_Arifmetic : Custom_Arifmetic
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            SimpleTypes t1 = Operand1.GetResultType();
            SimpleTypes t2 = Operand2.GetResultType();
            //public enum ColumnSimpleTypes { Integer = 1, Float = 2, String = 3, Geometry = 4, Date = 5, DateTime = 6, Time = 7, Boolean = 8 }
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
            TypesException();
        }
        protected abstract long CalcAsInt(object data);
        protected abstract double CalcAsFloat(object data);
    }
    public class Minus_Arifmetic : Custom_Arifmetic
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            SimpleTypes t1 = Operand1.GetResultType();
            SimpleTypes t2 = Operand2.GetResultType();
            //public enum ColumnSimpleTypes { Integer = 1, Float = 2, String = 3, Geometry = 4, Date = 5, DateTime = 6, Time = 7, Boolean = 8 }
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
            if (t1 == SimpleTypes.DateTime && t2 == SimpleTypes.Time)
            {
                GetDateTimeResultOut = CalcAsDateTimeAndTime1;
                SetResultType(SimpleTypes.DateTime);
                return;
            }
            if (t2 == SimpleTypes.DateTime && t1 == SimpleTypes.Time)
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

        protected long CalcAsInt(object data) { return Operand1.GetIntResultOut(data) - Operand2.GetIntResultOut(data); }
        protected double CalcAsFloat(object data) { return Operand1.GetFloatResultOut(data) - Operand2.GetFloatResultOut(data); }
        protected DateTime CalcAsDateTimeAndTime1(object data) { return Operand1.GetDateTimeResultOut(data).Add(-Operand2.GetTimeResultOut(data)); }
        protected DateTime CalcAsDateTimeAndTime2(object data) { return Operand2.GetDateTimeResultOut(data).Add(-Operand1.GetTimeResultOut(data)); }
        protected TimeSpan CalcAsTimeAndTime(object data) { return Operand1.GetTimeResultOut(data).Add(-Operand2.GetTimeResultOut(data)); }
        //protected Geometry CalcAsGeom(object data) { return Operand1.GetGeomResultOut(data).Difference(Operand2.GetGeomResultOut(data)); }

        public override string ToStr() { return "(" + Operand1.ToStr() + " - " + Operand2.ToStr() + ")"; }

        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return builder.Driver.ToSql(this, builder);
            //return "(" + Operand1.ToSQL(builder) + " + " + Operand2.ToSQL(builder) + ")";
        }
        public override int Priority() { return PriorityConst.PlusMinus; }
    }
    public class Multi_Arifmetic : Other_Arifmetic
    {
        protected override long CalcAsInt(object data) { return Operand1.GetIntResultOut(data) * Operand2.GetIntResultOut(data); }
        protected override double CalcAsFloat(object data) { return Operand1.GetFloatResultOut(data) * Operand2.GetFloatResultOut(data); }
        public override string ToStr() { return "(" + Operand1.ToStr() + " * " + Operand2.ToStr() + ")"; }
        public override string ToSQL(ExpressionSqlBuilder builder) { return "(" + Operand1.ToSQL(builder) + " * " + Operand2.ToSQL(builder) + ")"; }
        public override int Priority() { return PriorityConst.MultiDiv; }
    }
    public class Div_Arifmetic : Other_Arifmetic
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            //Всегда Float
            GetFloatResultOut = CalcAsFloat;
            SetResultType(SimpleTypes.Float);
        }

        protected override long CalcAsInt(object data) { return (int)(Operand1.GetIntResultOut(data) / Operand2.GetIntResultOut(data)); }
        protected override double CalcAsFloat(object data) { return Operand1.GetFloatResultOut(data) / Operand2.GetFloatResultOut(data); }
        public override string ToStr() { return "(" + Operand1.ToStr() + " / " + Operand2.ToStr() + ")"; }
        public override string ToSQL(ExpressionSqlBuilder builder) { return "(" + Operand1.ToSQL(builder) + " / " + Operand2.ToSQL(builder) + ")"; }
        public override int Priority() { return PriorityConst.MultiDiv; }
    }

}
