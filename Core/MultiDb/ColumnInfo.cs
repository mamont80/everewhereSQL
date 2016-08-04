using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace ParserCore
{
    /// <summary>
    /// Упрощённые типы полей. Коды не менять! Можно только добавлять. Цифры используются для сортировки операций в TableQuery.Expression - SortType
    /// </summary>
    public enum ColumnSimpleTypes { Integer = 1, Float = 2, String = 3, Geometry = 4, Date = 5, DateTime = 6, Time = 7, Boolean = 8 }

    /// <summary>
    /// информация о столбце таблицы в БД
    /// </summary>
    public class ColumnInfo : IComparable<ColumnInfo>, IEquatable<ColumnInfo>
    {
        public string Name { get; set; }

        private int _MaxChars = -1;
        public int MaxChars { get { return _MaxChars; } set { _MaxChars = value; } }

        public ColumnInfo(string name, ColumnSimpleTypes simpleType)
        {
            Name = name;
            ColumnSimpleType = simpleType;
        }

        public override string ToString()
        {
            return Name + " type=" + _ColumnSimpleType.ToString();
        }
        /// <summary>
        /// Простое сравнение по имени и SimpleType
        /// </summary>
        public bool SimpleEquals(ColumnInfo ci)
        {
            return (Name.EqualsIgnoreCase(ci.Name) && ColumnSimpleType == ci.ColumnSimpleType);
        }

        public bool Equals(ColumnInfo other)
        {
            return CompareTo(other) == 0;
        }

        public virtual int CompareTo(ColumnInfo other)
        {
            int i;
            if (Name.EqualsIgnoreCase(other.Name)) return 0;
            i = Name.CompareTo(other.Name);
            if (i == 0) i = ColumnSimpleType.CompareTo(other.ColumnSimpleType);
            return i;
        }



        // представление INFORMATION_SCHEMA.COLUMNS столбец DATA_TYPE
        //public string SqlType { get; set; }
        
        public bool IsPrimary { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsComputed { get; set; }
        /// <summary>
        /// Хранилище геометрии. Это: или тип "геометрия" или Xcol, Ycol
        /// </summary>
        public bool IsGeometryStorage { get; set; }
        private ColumnSimpleTypes _ColumnSimpleType;

        public ColumnSimpleTypes ColumnSimpleType
        {
            get { return _ColumnSimpleType; }
            set
            {
                _ColumnSimpleType = value;
                if (value == ColumnSimpleTypes.Geometry) IsGeometryStorage = true;
            }
        }

        public ColumnInfo Clone()
        {
            return (ColumnInfo)this.MemberwiseClone();
        }

        public static readonly ColumnSimpleTypes[] AllSimpleTypes = (ColumnSimpleTypes[])Enum.GetValues(typeof(ColumnSimpleTypes));

        /// <summary>
        /// Можно ли преобразовать тип
        /// </summary>
        /// <param name="fromType">Из какого типа</param>
        /// <param name="toType">В какой тип</param>
        /// <returns>true - совместимы</returns>
        public static bool CompatibleSimpleType(ColumnSimpleTypes fromType, ColumnSimpleTypes toType)
        {
            if (fromType == toType) return true;
            switch (fromType)
            {
                case ColumnSimpleTypes.Boolean:
                    if (toType == ColumnSimpleTypes.Geometry || toType == ColumnSimpleTypes.Date ||
                        toType == ColumnSimpleTypes.DateTime || toType == ColumnSimpleTypes.Time) return false;
                    return true;
                case ColumnSimpleTypes.Integer:
                    if (toType == ColumnSimpleTypes.Geometry) return false;
                    return true;
                case ColumnSimpleTypes.Float:
                    if (toType == ColumnSimpleTypes.Geometry) return false;
                    return true;
                case ColumnSimpleTypes.Geometry:
                    if (toType == ColumnSimpleTypes.String) return true;
                    else return false;
                case ColumnSimpleTypes.String:
                    if (toType == ColumnSimpleTypes.Geometry) return false;
                    return true;
                case ColumnSimpleTypes.Date:
                case ColumnSimpleTypes.DateTime:
                    if (toType == ColumnSimpleTypes.String || toType == ColumnSimpleTypes.Date ||
                        toType == ColumnSimpleTypes.DateTime || toType == ColumnSimpleTypes.Time || 
                        toType == ColumnSimpleTypes.Integer || toType == ColumnSimpleTypes.Float) return true;
                    return false;
                case ColumnSimpleTypes.Time:
                    if (toType == ColumnSimpleTypes.String) return true;
                    return false;
            }
            return false;
        }
       

        /// <summary>
        /// Разбор строки в ColumnSimpleType
        /// </summary>
        public static ColumnSimpleTypes ParseColumnSimpleTypeByStr(string str)
        {
            return (ColumnSimpleTypes)Enum.Parse(typeof(ColumnSimpleTypes), str, true);
        }

    }

}
