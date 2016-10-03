using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace ParserCore
{
    /// <summary>
    /// информация о столбце таблицы в БД
    /// </summary>
    public class Column : IComparable<Column>, IEquatable<Column>
    {
        public string Name { get; set; }
        public virtual string PhysicalName { get; set; }

        private SimpleTypes _simpleType;

        public virtual SimpleTypes SimpleType
        {
            get { return _simpleType; }
            set
            {
                _simpleType = value;
            }
        }

        private DbColumnType _DbType = DbColumnType.Unknow;
        /// <summary>
        /// Exact column type
        /// </summary>
        public DbColumnType DbType {
            get { return _DbType; }
            set { _DbType = value; }
        }

        private bool _IsNullable = true;
        public bool IsNullable
        {
            get { return _IsNullable; }
            set { _IsNullable = value; }
        }

        private bool _AutoIncriment = true;
        public bool AutoIncriment
        {
            get { return _AutoIncriment; }
            set { _AutoIncriment = value; }
        } 
        
        public Column()
        {
        }

        public Column(string name, SimpleTypes simpleType):this()
        {
            Name = name;
            PhysicalName = name;
            SimpleType = simpleType;
        }

        public override string ToString()
        {
            return Name + " type=" + _simpleType.ToString();
        }
        /// <summary>
        /// Простое сравнение по имени и SimpleType
        /// </summary>
        public bool SimpleEquals(Column ci)
        {
            return (Name.EqualsIgnoreCase(ci.Name) && SimpleType == ci.SimpleType);
        }

        public bool Equals(Column other)
        {
            return CompareTo(other) == 0;
        }

        public virtual int CompareTo(Column other)
        {
            int i;
            if (Name.EqualsIgnoreCase(other.Name)) return 0;
            i = Name.CompareTo(other.Name);
            if (i == 0) i = SimpleType.CompareTo(other.SimpleType);
            return i;
        }

        public Column Clone()
        {
            return (Column)this.MemberwiseClone();
        }

        public static readonly SimpleTypes[] AllSimpleTypes = (SimpleTypes[])Enum.GetValues(typeof(SimpleTypes));

        /// <summary>
        /// Можно ли преобразовать тип
        /// </summary>
        /// <param name="fromType">Из какого типа</param>
        /// <param name="toType">В какой тип</param>
        /// <returns>true - совместимы</returns>
        public static bool CompatibleSimpleType(SimpleTypes fromType, SimpleTypes toType)
        {
            if (fromType == toType) return true;
            switch (fromType)
            {
                case SimpleTypes.Boolean:
                    if (toType == SimpleTypes.Geometry || toType == SimpleTypes.Date ||
                        toType == SimpleTypes.DateTime || toType == SimpleTypes.Time) return false;
                    return true;
                case SimpleTypes.Integer:
                    if (toType == SimpleTypes.Geometry) return false;
                    return true;
                case SimpleTypes.Float:
                    if (toType == SimpleTypes.Geometry) return false;
                    return true;
                case SimpleTypes.Geometry:
                    if (toType == SimpleTypes.String) return true;
                    else return false;
                case SimpleTypes.String:
                    if (toType == SimpleTypes.Geometry) return false;
                    return true;
                case SimpleTypes.Date:
                case SimpleTypes.DateTime:
                    if (toType == SimpleTypes.String || toType == SimpleTypes.Date ||
                        toType == SimpleTypes.DateTime || toType == SimpleTypes.Time || 
                        toType == SimpleTypes.Integer || toType == SimpleTypes.Float) return true;
                    return false;
                case SimpleTypes.Time:
                    if (toType == SimpleTypes.String) return true;
                    return false;
            }
            return false;
        }

        /// <summary>
        /// Разбор строки в ColumnSimpleType
        /// </summary>
        public static SimpleTypes ParseColumnSimpleTypeByStr(string str)
        {
            return (SimpleTypes)Enum.Parse(typeof(SimpleTypes), str, true);
        }
    }

}
