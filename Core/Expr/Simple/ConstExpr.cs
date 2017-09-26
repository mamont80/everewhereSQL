using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.Simple
{
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

        public virtual void Init(object val, SimpleTypes type, int csf)
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
                    valueDateTime = CommonUtils.Convert<DateTime>(val);
                    SetResultType(SimpleTypes.DateTime);
                    GetDateTimeResultOut = AsDateTime;
                    break;
                case SimpleTypes.Date:
                    valueDateTime = CommonUtils.Convert<DateTime>(val);
                    SetResultType(SimpleTypes.Date);
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
                    //_CoordinateSystem = csf;
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
                    return ParserUtils.ConstToStrEscape(GetStrResultOut(null).ToString());
                case SimpleTypes.Integer:
                    return GetIntResultOut(null).ToString();
                case SimpleTypes.Float:
                    return GetFloatResultOut(null).ToStr();
                case SimpleTypes.DateTime:
                    return "datetime '" + GetDateTimeResultOut(null).ToString("dd.MM.yyyy HH:mm:ss") + "'";
                case SimpleTypes.Date:
                    return "date '" + GetDateTimeResultOut(null).ToString("dd.MM.yyyy") + "'";
                case SimpleTypes.Time:
                    return "time '" + GetTimeResultOut(null).ToString("c") + "'";
                case SimpleTypes.Geometry:
                    // TODO: FIXed! ok
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

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            if (builder.DbType == DriverType.PostgreSQL)
                return PostgreSqlObjectToStringSql(GetObjectResultOut(null));
            if (builder.DbType == DriverType.SqlServer)
                return SqlServerObjectToStringSql(GetObjectResultOut(null));
            return ToSqlException();
        }
        #region платформенно-зависимые преобразования
        #region sqlserver
        public virtual string SqlServerObjectToStringSql(object obj)
        {
            if (obj == null) return "null";
            if (obj is string)
            {
                return SqlServerEncodeStr(obj.ToString());
            }
            else if (obj is bool)
            {
                if ((bool)obj) return "1";
                else return "0";
            }
            else
                if (obj is decimal || obj is double || obj is float)
                {
                    return Convert.ToDouble(obj).ToStr();
                }
                else if (obj is DateTime)
                {
                    DateTime d = (DateTime)obj;
                    return " CAST(" + SqlServerEncodeStr(d.ToString("yyyy-MM-dd HH:mm:ss")) + " AS datetime2) ";
                }
                else if (obj is TimeSpan)
                {
                    TimeSpan d = (TimeSpan)obj;
                    return " CAST(" + SqlServerEncodeStr(d.ToString("c")) + " AS time) ";
                }
                else return obj.ToString();
        }

        protected string SqlServerEncodeStr(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return "'"+s.Replace("'", "''")+"'";
        }
        #endregion
        #region postgresql
        public virtual string PostgreSqlObjectToStringSql(object obj)
        {
            return DoPostgreSqlObjectToStringSql(obj);
        }

        protected string DoPostgreSqlObjectToStringSql(object obj)
        {
            if (obj == null) return "null";
            if (obj is string)
            {
                return PostgreSqlEncodeStr(obj.ToString());
            }
            else
                if (obj is decimal || obj is double || obj is float)
                {
                    return CommonUtils.ToStr(Convert.ToDouble(obj));
                }
                else if (obj is DateTime)
                {
                    DateTime d = (DateTime)obj;
                    return " TIMESTAMP " + PostgreSqlEncodeStr(d.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                else if (obj is TimeSpan)
                {
                    TimeSpan d = (TimeSpan)obj;
                    return " TIME " + PostgreSqlEncodeStr(d.ToString("c"));
                }
                else return obj.ToString();
        }


        protected string PostgreSqlEncodeStr(string s)
        {
            return "'" +s.Replace("'","''")+ "'";
        }
        #endregion
        #endregion
    }

}
