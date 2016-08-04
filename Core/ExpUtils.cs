using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LayerData;
using ParserCore;


namespace LayerData
{


    //утилиты с привязкой к геомиксеру
    public static class ExpUtils
    {
        /*public static GmSqlQuery ParseSelectSql(string text)
        {
            var collection = ParserLexem.Parse(text);
            collection.NodeFactory = new ExpressionFactoryTable();
            ExpressionToNode2 expToNode = new ExpressionToNode2();
            expToNode.Parse(collection);

            return expToNode.Single();
        }*/


        public static List<ColumnClause> GetAllColumnsFromTable(GeometryTable gt, string aliasForGeometry = null)
        {
            List<ColumnClause> cols = new List<ColumnClause>();
            foreach (ColumnInfo ci in gt.Columns)
            {
                if (ci.ColumnSimpleType != ColumnSimpleTypes.Geometry)
                {
                    ColumnClause cs = new ColumnClause();
                    cs.ColumnExpression = ExpUtils.ParseSubSql(BaseExpressionFactory.StandartCodeEscape(ci.Name));
                    cols.Add(cs);
                }
            }
            //отдельно добавляем колонку с геометрией
            // TODO: Fix for geomixer
            /*
            ColumnClause csg = new ColumnClause();
            csg.ColumnExpression = ExpUtils.ParseSubSql(GeometryFeature.GeometryJsonKey);
            csg.Alias = GeometryFeature.GeometryJsonKey;
            if (!string.IsNullOrEmpty(aliasForGeometry)) csg.Alias = aliasForGeometry;
            cols.Add(csg);
             */
            return cols;
        }

        public static void CreateFields(SelectExpresion select, LexemCollection collection = null)
        {
            FieldCreator fc = new FieldCreator(collection);
            fc.MakeFields(select);
        }

        public static Expression ParseSubSql(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            var collection = ParserLexem.Parse(text);
            collection.NodeFactory = new ExpressionFactoryTable();
            ExpressionToNode2 expToNode = new ExpressionToNode2();
            expToNode.Parse(collection);

            if (collection.CurrentLexem() != null) collection.Error("Unknow symbols after expression", collection.CurrentLexem());
            return expToNode.Single();
        }

    }
}
