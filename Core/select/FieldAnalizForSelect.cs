using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    //второй проход по предразобранному SQL запросу с присвоением колонок к таблицам
    public class FieldCreator
    {
        public LexemCollection Collection { get; protected set; }

        public FieldCreator(LexemCollection collection)
        {
            Collection = collection;
        }

        public void MakeFields(Statment stmt)
        {
            if (stmt is SelectStatment) MakeFieldsSelectStatment(stmt as SelectStatment);
            if (stmt is UpdateStatment) MakeFieldsUpdateStatment(stmt as UpdateStatment);
            if (stmt is DeleteStatment) MakeFieldsDeleteStatment(stmt as DeleteStatment);
            if (stmt is InsertStatment) MakeFieldsInsertStatment(stmt as InsertStatment);
        }

        private void MakeFieldsDeleteStatment(DeleteStatment delete)
        {
            List<SelectTable> lst = new List<SelectTable>();
            lst.Add(delete.Table);
            if (delete.Where != null) delete.Where = RecursiveExpr(delete.Where, lst);
        }

        private void MakeFieldsInsertStatment(InsertStatment insert)
        {
            List<SelectTable> lst = new List<SelectTable>();
            lst.Add(insert.Table);
            for (int i = 0; i < insert.Values.Count; i++)
            {
                insert.Values[i] = RecursiveExpr(insert.Values[i], lst);
            }
            for (int i = 0; i < insert.Columns.Count; i++)
            {
                insert.Columns[i] = RecursiveExpr(insert.Columns[i], lst);
            }
            if (insert.Select != null) MakeFields(insert.Select);
        }

        private void MakeFieldsUpdateStatment(UpdateStatment update)
        {
            List<SelectTable> lst = new List<SelectTable>();
            lst.Add(update.Table);
            for (int i = 0; i < update.Set.Count; i++)
            {
                var set = update.Set[i];
                set.Column = RecursiveExpr(set.Column, lst);
                set.Value = RecursiveExpr(set.Value, lst);
            }
            if (update.Where != null) update.Where = RecursiveExpr(update.Where, lst);
        }

        private void MakeFieldsSelectStatment(SelectStatment select)
        {
            MakeFields(select.Select);
        }


        public void MakeFields(SelectExpresion sExpr)
        {
            PreprocessSubSelect(sExpr);
            DoSelect(sExpr, new List<SelectTable>());
        }

        private void PreprocessSubSelect(SelectExpresion sExpr)
        {
            for (int i = 0; i < sExpr.Query.Tables.Count; i++)
            {
                if (sExpr.Query.Tables[i].Table is SubSelectTableDesc)
                {
                    var sst = sExpr.Query.Tables[i].Table as SubSelectTableDesc;
                    MakeFields(sst.Select);
                    sst.Select.Prepare();
                    sst.Columns.Clear();

                    var lst = sst.Select.Query.Columns.ToArray();
                    HashSet<string> str = new HashSet<string>();
                    foreach (var c in lst)
                    {
                        string alias = c.ExtractAlias();
                        if (alias == null) throw new Exception("Alias for column not found: " + alias);
                        if (str.Contains(alias)) throw new Exception("column alias is not unique: " + alias);
                        str.Add(alias);
                        var tp = c.ColumnExpression.GetResultType();
                        if (tp == SimpleTypes.Geometry)
                        {
                            sst.CoordinateSystem = c.ColumnExpression.GetCoordinateSystem();
                        }
                        sst.Columns.Add(new Column(alias, tp));
                    }
                    if (string.IsNullOrEmpty(sExpr.Query.Tables[i].Alias)) throw new Exception("Alias subselect not found");
                    sst.Alias = sExpr.Query.Tables[i].Alias;
                }
            }
        }

        private void DoSelect(SelectExpresion sExpr, List<SelectTable> parentTables)
        {
            List<SelectTable> tables = new List<SelectTable>(parentTables);
            tables.AddRange(sExpr.Query.Tables);
            for (int i = 0; i < sExpr.Query.Tables.Count; i++)
            {
                if (sExpr.Query.Tables[i].OnExpression != null)
                {
                    sExpr.Query.Tables[i].OnExpression = RecursiveExpr(sExpr.Query.Tables[i].OnExpression, tables);
                }
            }
            for (int i = 0; i < sExpr.Query.Columns.Count; i++)
            {
                if (sExpr.Query.Columns[i].ColumnExpression != null && sExpr.Query.Columns[i].ColumnExpression is AllColumnExpr)
                {
                    AllColumnExpr a = sExpr.Query.Columns[i].ColumnExpression as AllColumnExpr;
                    sExpr.Query.Columns.RemoveAt(i);
                    for (int j = 0; j < sExpr.Query.Tables.Count; j++)
                    {
                        var st = sExpr.Query.Tables[j];
                        var t = sExpr.Query.Tables[j].Table;
                        if (!string.IsNullOrEmpty(a.Prefix))
                        {
                            string tableAlias = a.Prefix;
                            if (StringComparer.InvariantCultureIgnoreCase.Compare(st.Name, tableAlias) != 0 &&
                                StringComparer.InvariantCultureIgnoreCase.Compare(st.Alias, tableAlias) != 0) continue;
                        }
                        for (int k = 0; k < t.Columns.Count; k++)
                        {
                            ColumnClause cc = new ColumnClause();
                            FieldExpr fe = new FieldExpr();
                            cc.ColumnExpression = fe;
                            MakeField(sExpr.Query.Tables[j], t.Columns[k].Name, fe);
                            sExpr.Query.Columns.Insert(i, cc);
                        }
                    }
                }
                else
                {
                    sExpr.Query.Columns[i].ColumnExpression = RecursiveExpr(sExpr.Query.Columns[i].ColumnExpression, tables);
                    // TODO: fix  Это можно выкинуть. Наверно.
                    /*if (sExpr.Query.Columns[i].Alias == null) sExpr.Query.Columns[i].Alias = GetDefaultColumnAlias(sExpr.Query.Columns[i].ColumnExpression);
                     */
                }
            }
            if (sExpr.Query.WhereExpr != null) sExpr.Query.WhereExpr = RecursiveExpr(sExpr.Query.WhereExpr, tables);
            if (sExpr.Query.Having != null) sExpr.Query.Having = RecursiveExpr(sExpr.Query.Having, tables);
            if (sExpr.Query.GroupBys != null)
            {
                for (int i = 0; i < sExpr.Query.GroupBys.Count; i++)
                {
                    sExpr.Query.GroupBys[i].Expression = RecursiveExpr(sExpr.Query.GroupBys[i].Expression, tables);
                }
            }
            if (sExpr.Query.OrderBys != null)
            {
                for (int i = 0; i < sExpr.Query.OrderBys.Count; i++)
                {
                    sExpr.Query.OrderBys[i].Expression = RecursiveExpr(sExpr.Query.OrderBys[i].Expression, tables);
                }
            }
        }

        public Expression MakeFields(Expression expr, SelectTable table)
        {
            List<SelectTable> lst = new List<SelectTable>();
            lst.Add(table);
            return RecursiveExpr(expr, lst);
        }

        private Expression RecursiveExpr(Expression expr, List<SelectTable> Tables)
        {
            if (expr.Childs != null)
            {
                for (int i = 0; i < expr.Childs.Count; i++)
                {
                    expr.Childs[i] = RecursiveExpr(expr.Childs[i], Tables);
                }
            }
            if (expr is FieldCapExpr)
            {
                return FindAndMakeField(expr as FieldCapExpr, Tables);
            }
            if (expr is SelectExpresion)
            {
                DoSelect((expr as SelectExpresion), Tables);
                return expr;
            }
            else return expr;
        }

        private Expression FindAndMakeField(FieldCapExpr expr, List<SelectTable> Tables)
        {
            Expression res = null;
            string tableAlias = expr.TableAlias;
            string fieldAlias = expr.FieldAlias;
            foreach (var st in Tables)
            {
                if (!string.IsNullOrEmpty(tableAlias))
                {
                    if (StringComparer.InvariantCultureIgnoreCase.Compare(st.Name, tableAlias) != 0 &&
                        StringComparer.InvariantCultureIgnoreCase.Compare(st.Alias, tableAlias) != 0) continue;
                }
                bool ok = false;
                if (st.Table.ByName(fieldAlias) != null)
                {
                    ok = true;
                }
                // TODO: Fix for geomixer
                /*
                if (!ok && fieldAlias == GeometryFeature.IdColumnName)
                {
                    fieldAlias = ExpUtils.GetIDColumnName(st);
                    if (st.Table.ByName(fieldAlias) != null) ok = true;
                }
                if (!ok && fieldAlias.ToLower() == GeometryFeature.GeometryJsonKey)
                {
                    return ExpUtils.GetGeometryExpression(st, GeometryFeature.GeometryJsonKey);
                }*/
                if (ok)
                {
                    res = new FieldExpr();
                    MakeField(st, fieldAlias, res as FieldExpr);
                    return res;
                }
            }
            if (res == null)
            {
                if (Collection != null) Collection.Error("Column " + fieldAlias + " not found", expr.Lexem);
                else throw new Exception("Column " + fieldAlias + " not found");
            }
            return res;

        }

        public static void MakeField(SelectTable st, string fieldName, FieldExpr res)
        {
            Column ci = st.Table.ByName(fieldName);
            res.FieldName = ci.Name;//fieldName;
            res.Table = st;
            res.Init(ci.SimpleType, st.Table.CoordinateSystem);
        }

        


    }
}
