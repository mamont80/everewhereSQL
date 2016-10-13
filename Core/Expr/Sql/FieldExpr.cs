using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.Sql
{
    public class FieldExpr : Expression
    {
        public string FieldName;
        public string TableAlias;
        public string Schema;
        public Column PhysicalColumn;
        public TableClause TableClause;
        public bool IsLocalColumn = false;
        public Lexem Lexem;
        public bool IsBinded = false;

        protected int _CoordinateSystem = -1;
        public override int GetCoordinateSystem()
        {
            return _CoordinateSystem;
        }

        public void Init(SimpleTypes tp)
        {
            Init(tp, 0);
        }

        public virtual void Init(SimpleTypes tp, int csf)
        {
            if (tp == SimpleTypes.Geometry)
            {
                _CoordinateSystem = csf;
            }
            SetResultType(tp);
        }

        /// <summary>
        /// Это операция или значение
        /// </summary>
        public override bool IsOperation() { return false; }

        public override string ToStr()
        {
            string res = "";
            if (!string.IsNullOrEmpty(Schema))
            {
                if (!string.IsNullOrEmpty(res)) res += ".";
                res += ParserUtils.TableToStrEscape(Schema);
            }
            if (!string.IsNullOrEmpty(TableAlias))
            {
                if (!string.IsNullOrEmpty(res)) res += ".";
                res += ParserUtils.TableToStrEscape(TableAlias);
            }
            if (!string.IsNullOrEmpty(FieldName))
            {
                if (!string.IsNullOrEmpty(res)) res += ".";
                res += ParserUtils.TableToStrEscape(FieldName);
            }
            return res;
        }
        protected override bool CanCalcOnline() { return false; }

        /// <summary>
        /// Используется для:
        ///  insert into xx ([вот здесь]) 
        ///  update xx set [вот здесь]=..
        ///  select OVER ( ORDER BY [вот здесь] ) from ...
        /// </summary>
        public string ToSqlShort(ExpressionSqlBuilder builder)
        {
            return builder.EncodeTable(PhysicalColumn.PhysicalName);
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            if (PhysicalColumn == null) throw new Exception("Column \"" + FieldName + "\" is not found");
            string res = "";
            if (builder.FieldsAsInsertedAlias)
            {
                res += "INSERTED";
            }
            if (!IsLocalColumn)
            {
                if (!string.IsNullOrEmpty(TableAlias))
                {
                    if ((StringComparer.InvariantCultureIgnoreCase.Compare(TableAlias, TableClause.Alias) == 0))
                    {
                        if (!string.IsNullOrEmpty(res)) res += ".";
                        res += builder.EncodeTable(TableAlias);
                    }
                    else res += MinTableName(builder); //TableClause.Table.ToSql(builder);
                }
                else
                {
                    if (!string.IsNullOrEmpty(TableClause.Alias)) res += builder.EncodeTable(TableClause.Alias);
                    else
                        res += MinTableName(builder);// TableClause.Table.ToSql(builder);
                }
            }
            if (!string.IsNullOrEmpty(PhysicalColumn.PhysicalName))
            {
                if (!string.IsNullOrEmpty(res)) res += ".";
                res += builder.EncodeTable(PhysicalColumn.PhysicalName);
            }
            else
            {//на всякий пожарный, но эта ветка не используется
                if (!string.IsNullOrEmpty(res)) res += ".";
                res += builder.EncodeTable(FieldName);
            }
            return res;
        }

        private string MinTableName(ExpressionSqlBuilder builder)
        {
            string res;
            TableClause tc1 = this.TableClause;
            if (!(tc1.Table is TableDesc)) throw new Exception("Can not find alias table " + tc1.Table.ToStr());
            ITableSource item = this.FindParentTableSource();
            var arr = item.GetTables();
            if (arr.Length == 1 && arr[0] == tc1) return "";
            while (item != null)
            {
                res = DoMinTableName(builder, item, tc1);
                if (res != null)
                {
                    return res;
                }
                var t = item.FindParentTableSource();
                if (t == item) throw new Exception("internal parser error (132)");
                item = t;
            }
            throw new Exception("Table clause not found in expression");
        }

        private static string DoMinTableName(ExpressionSqlBuilder builder, ITableSource parent, TableClause tc1)
        {
            TableDesc td1 = (TableDesc)tc1.Table;
            var arr = parent.GetTables();
            bool find = false;
            bool findDubName = false;
            bool findDubSchema = false;
            foreach (var tc in arr)
            {
                if (tc == tc1)
                {
                    find = true;
                }
                else
                    if (string.IsNullOrEmpty(tc.Alias) &&
                        (tc.Table is TableDesc)
                        )
                    {
                        TableDesc td = (TableDesc)tc.Table;
                        if (td.PhysicalTableName.ToLower() == td1.PhysicalTableName.ToLower())
                        {
                            findDubName = true;
                            if (td.PhysicalSchema.ToLower() == td1.PhysicalSchema.ToLower())
                            {
                                findDubSchema = true;
                            }
                        }
                    }
            }
            if (!find) return null;
            if (!findDubName) return builder.EncodeTable(td1.PhysicalTableName);
            if (!findDubSchema) return builder.EncodeTable(td1.PhysicalSchema)+"."+builder.EncodeTable(td1.PhysicalTableName);
            throw new Exception("Not unique table " + builder.EncodeTable(td1.PhysicalSchema) + "." + builder.EncodeTable(td1.PhysicalTableName));
        }

        public void Bind(TableClause tableClause, string fieldName)
        {
            Column ci = tableClause.Table.ByName(fieldName);
            if (ci == null) throw new Exception("Column " + fieldName + " is not found");
            FieldName = fieldName;
            PhysicalColumn = ci;
            TableClause = tableClause;
            Init(ci.SimpleType, tableClause.Table.CoordinateSystem);
            IsBinded = true;
        }

        public void Bind(SelectExpresion select, string fieldName)
        {
            Column ci = select.ByName(fieldName);
            if (ci == null) throw new Exception("Column " + fieldName + " is not found");
            FieldName = fieldName;
            PhysicalColumn = ci;
            IsLocalColumn = true;
            Init(ci.SimpleType);
            IsBinded = true;
        }

        public override void Prepare()
        {
            base.Prepare();
            if (!IsBinded) FindAndMakeField();
        }

        private void FindAndMakeField()
        {
            FieldExpr f = this;
            string tableAlias = f.TableAlias;
            string fieldAlias = f.FieldName;
            string schema = f.Schema;
            bool ok = false;
            ITableSource tsource = CommonUtils.FindParentTableSource(this);
            while (tsource != null)
            {
                var tables = tsource.GetTables();
                foreach (var st in tables)
                {
                    bool okTable = false;
                    if (!string.IsNullOrEmpty(tableAlias))
                    {
                        if (st.CompareWithColumn(new string[2] { schema, tableAlias })) okTable = true;
                    }
                    else okTable = true;
                    if (okTable)
                    {
                        var c = st.Table.ByName(fieldAlias);
                        if (c != null)
                        {
                            if (ok) throw new Exception("Невозможно определить однозначно колонку");
                            f.Bind(st, fieldAlias);
                            ok = true;
                        }
                    }
                }
                if (ok) break;
                if (string.IsNullOrEmpty(tableAlias) && tsource is SelectExpresion)
                {
                    SelectExpresion select = tsource as SelectExpresion;
                    foreach (var c in select.TableColumns)
                    {
                        if (StringComparer.InvariantCultureIgnoreCase.Compare(c.Name, fieldAlias) == 0)
                        {
                            if (ok) throw new Exception("Невозможно определить однозначно колонку");
                            ok = true;
                            f.Bind(select, fieldAlias);
                        }
                    }
                }
                if (ok) break;

                tsource = CommonUtils.FindParentTableSource(tsource);
            }
            if (!ok) throw new Exception("Column \"" + fieldAlias + "\" is not found");
        }
    }
}
