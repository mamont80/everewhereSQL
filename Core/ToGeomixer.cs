using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
    /*
    public class Contains_operation : Custom_TwoOperand
    {
        public override int Priority()
        {
            return PriorityConst.Like;
        }

        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (!(Operand1.GetResultType() == ColumnSimpleTypes.String && Operand2.GetResultType() == ColumnSimpleTypes.String)) TypesException();
            //if (!Operand2.OnlyOnline()) OperandOnlyConstException(2);
            SetResultType(ColumnSimpleTypes.Boolean);
            GetBoolResultOut = CalcAsBool;
        }
        private bool CalcAsBool(object data) { return Operand1.GetStrResultOut(data).Contains(Operand2.GetStrResultOut(data)); }

        public override string ToStr() { return "(" + Operand1.ToStr() + " contains " + Operand2.ToStr() + ")"; }
        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return builder.Driver.BuildExpContains(Operand2.ToSQL(builder), Operand1.ToSQL(builder));
        }
    }

    public class ContainsIgnoreCase_operation : Custom_TwoOperand
    {
        public override int Priority()
        {
            return PriorityConst.Like;
        }

        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (!(Operand1.GetResultType() == ColumnSimpleTypes.String && Operand2.GetResultType() == ColumnSimpleTypes.String)) TypesException();
            if (!Operand2.OnlyOnline()) OperandOnlyConstException(2);
            SetResultType(ColumnSimpleTypes.Boolean);
            GetBoolResultOut = CalcAsBool;
        }
        private bool CalcAsBool(object data) { return Operand1.GetStrResultOut(data).IndexOf(Operand2.GetStrResultOut(data), StringComparison.OrdinalIgnoreCase) >= 0; }

        public override string ToStr() { return "(" + Operand1.ToStr() + " containsIC " + Operand2.ToStr() + ")"; }
        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return builder.Driver.BuildExpContains(Operand2.ToSQL(builder), Operand1.ToSQL(builder));
        }
    }
    public class StartWith_operation : Custom_TwoOperand
    {
        public override int Priority()
        {
            return PriorityConst.Like;
        }

        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (!(Operand1.GetResultType() == ColumnSimpleTypes.String && Operand2.GetResultType() == ColumnSimpleTypes.String)) TypesException();
            //if (!Operand2.OnlyOnline()) OperandOnlyConstException(2);
            SetResultType(ColumnSimpleTypes.Boolean);
            GetBoolResultOut = CalcAsBool;
        }
        private bool CalcAsBool(object data) { return Operand1.GetStrResultOut(data).StartsWith(Operand2.GetStrResultOut(data)); }

        public override string ToStr() { return "(" + Operand1.ToStr() + " StartWith " + Operand2.ToStr() + ")"; }
        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return builder.Driver.BuildExpStartWith(Operand2.ToSQL(builder), Operand1.ToSQL(builder));
        }
    }
    
      
     



     public class GeometryToWkbHex : FuncExpr_OneOperand
        {
            protected override void BeforePrepare()
            {
                base.BeforePrepare();
                if (Operand.GetResultType() != ColumnSimpleTypes.Geometry) TypesException();
                SetResultType(ColumnSimpleTypes.String);
                GetStrResultOut = GetResult;
            }

            private string GetResult(object data)
            {
                Geometry g = Operand.GetGeomResultOut(data);
                byte[] buf= new byte[g.WkbSize()];
                g.ExportToWkb(buf);
                string s = CustomDbDriver.BytesToStr(buf);
                return s;
            }

            public override string ToSQL(ExpressionSqlBuilder builder)
            {
                return builder.Driver.ToSql(this, builder);
            }

            public override string ToStr(){ return "GeometryToWkbHex(" + Operand.ToStr() + ")"; }
        }

        public class GeometryFromVectorLayer : FuncExpr_TwoOperand, IExpressionWithAccessControl
        {
            private Geometry _geom;
            protected override void BeforePrepare()
            {
                base.BeforePrepare();
                if (Operand1.GetResultType() != ColumnSimpleTypes.String) TypesException();
                if (!Operand1.OnlyOnline()) OperandOnlyConstException(1);
                if (Operand2.GetResultType() != ColumnSimpleTypes.Integer) TypesException();
                if (!Operand2.OnlyOnline()) OperandOnlyConstException(2);
                string layerName = Operand1.GetStrResultOut(null);
                long id = Operand2.GetIntResultOut(null);
                LayerData.VectorLayer l = LayersCache.GetLayer(layerName) as VectorLayer;
                if (l == null) throw new ObjectNotFoundException("Layer not found");

                _CoordinateSystem = l.GetGeomTable().CoordinateSystem;
                _geom = l.GetGeomTable().GetGeometryFeatureById((int) id).Geometry;
                SetResultType(ColumnSimpleTypes.Geometry);
                GetGeomResultOut = GetResult;
            }

            public void VarifyAccess(User u)
            {
                string layerName = Operand1.GetStrResultOut(null);
                LayerData.VectorLayer l = LayersCache.GetLayer(layerName) as VectorLayer;
                if (l == null) throw new ObjectNotFoundException("Layer not found");

                if (!UserAccess.CanUserViewLayer(u, l)) throw new Exception("Access denied to layer " + layerName);
            }

            private OSGeo.OGR.Geometry GetResult(object data)
            {
                return _geom;
            }

            public override string ToSQL(ExpressionSqlBuilder builder)
            {
                throw new Exception("Can not make SQL expression");
            }
            public override string ToStr()
            {
                return "GeometryFromVectorLayer(" + Operand1.ToStr() + "," + Operand2.ToStr()+")";
            }
        }

        public class GeometryFromVectorLayerUnion : FuncExpr_OneOperand, IExpressionWithAccessControl
        {
            private Geometry _geom;

            protected override void BeforePrepare()
            {
                base.BeforePrepare();
                if (Operand.GetResultType() != ColumnSimpleTypes.String) TypesException();
                if (!Operand.OnlyOnline()) OperandOnlyConstException(1);
                string layerName = Operand.GetStrResultOut(null);
                LayerData.VectorLayer l = LayersCache.GetLayer(layerName) as VectorLayer;
                if (l == null) throw new Exception("Layer not found");

                _CoordinateSystem = l.GetGeomTable().CoordinateSystem;
                _geom = l.GetGeomTable().GetGeometriesUnion();
                SetResultType(ColumnSimpleTypes.Geometry);
                GetGeomResultOut = GetResult;
            }

            public void VarifyAccess(User u)
            {
                string layerName = Operand.GetStrResultOut(null);
                LayerData.VectorLayer l = LayersCache.GetLayer(layerName) as VectorLayer;
                if (l == null) throw new ObjectNotFoundException("Layer not found");

                if (!UserAccess.CanUserViewLayer(u, l)) throw new Exception("Access denied to layer " + layerName);
            }

            private OSGeo.OGR.Geometry GetResult(object data)
            {
                return _geom;
            }

            public override string ToSQL(ExpressionSqlBuilder builder)
            {
                throw new Exception("Can not make SQL expression");
            }
            public override string ToStr()
            {
                return "GeometryFromVectorLayerUnion(" + Operand.ToStr() + ")";
            }
        }

        public class GeometryFromRasterLayer : FuncExpr_OneOperand, IExpressionWithAccessControl
        {
            private Geometry _geom;
            protected override void BeforePrepare()
            {
                base.BeforePrepare();
                if (Operand.GetResultType() != ColumnSimpleTypes.String) TypesException();
                if (!Operand.OnlyOnline()) OperandOnlyConstException(1);
                string layerName = Operand.GetStrResultOut(null);
                _geom = RasterCatalog.GetBorderByRcAliasOrRasterName(layerName, null, false);
                _CoordinateSystem = CoordinateSystemFull.DefMercator();
                SetResultType(ColumnSimpleTypes.Geometry);
                GetGeomResultOut = GetResult;
            }

            public void VarifyAccess(User u)
            {
                string layerName = Operand.GetStrResultOut(null);
                RasterCatalog.GetBorderByRcAliasOrRasterName(layerName, u, true);
            }
            private OSGeo.OGR.Geometry GetResult(object data)
            {
                return _geom;
            }

            public override string ToSQL(ExpressionSqlBuilder builder)
            {
                throw new Exception("Can not make SQL expression");
            }
            public override string ToStr()
            {
                return "GeometryFromRasterLayer(" + Operand.ToStr() + ")";
            }
        }

        public class GeometryFromWkbHex : FuncExpr_TwoOperand
        {
            protected override void BeforePrepare()
            {
                base.BeforePrepare();
                if (Operand1.GetResultType() != ColumnSimpleTypes.String) TypesException();
                if (!Operand1.OnlyOnline()) OperandOnlyConstException(1);
                if (Operand2.GetResultType() != ColumnSimpleTypes.Integer) TypesException();
                if (!Operand2.OnlyOnline()) OperandOnlyConstException(2);
                _CoordinateSystem = CoordinateSystemFull.CreateByEPSG((int)Operand2.GetIntResultOut(null));
                SetResultType(ColumnSimpleTypes.Geometry);
                GetGeomResultOut = GetResult;
            }

            private OSGeo.OGR.Geometry GetResult(object data)
            {
                string wkb = Operand1.GetStrResultOut(data);
                byte[] buf = CustomDbDriver.StrToBytes(wkb);
                return OSGeo.OGR.Geometry.CreateFromWkb(buf);
            }

            public override string ToSQL(ExpressionSqlBuilder builder)
            {
                return builder.Driver.ToSql(this, builder);
            }
            public override string ToStr()
            {
                return "GeometryFromWkbHex("+Operand1.ToStr()+","+Operand2.ToStr()+")";
            }
        }
 
    public class GeometryBuffer : FuncExpr_TwoOperand
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (Operand1.GetResultType() != ColumnSimpleTypes.Geometry) TypesException();
            if (Operand2.GetResultType() != ColumnSimpleTypes.Float && Operand2.GetResultType() != ColumnSimpleTypes.Integer) TypesException();
            if (!Operand1.OnlyOnline()) OperandOnlyConstException(1);
            if (!Operand2.OnlyOnline()) OperandOnlyConstException(2);
            SetResultType(ColumnSimpleTypes.Geometry);
            _CoordinateSystem = Operand1.GetCoordinateSystem();
            GetGeomResultOut = GetResult;
        }

        private OSGeo.OGR.Geometry GetResult(object data)
        {
            Geometry g = Operand1.GetGeomResultOut(data);
            double d = Operand2.GetFloatResultOut(data);
            return g.Buffer(d, 10);
        }

        public override string ToStr() { return "Buffer(" + Operand1.ToStr() + "," + Operand2.ToStr() + ")"; }
        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return builder.Driver.ToSql(this, builder);
        }
    }
     

    public class GeometryFromGeoJson : FuncExpr_TwoOperand
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (Operand1.GetResultType() != ColumnSimpleTypes.String) TypesException();
            if (Operand2.GetResultType() != ColumnSimpleTypes.Integer) TypesException();
            if (!Operand1.OnlyOnline()) OperandOnlyConstException(1);
            if (!Operand2.OnlyOnline()) OperandOnlyConstException(2);
            _CoordinateSystem = CoordinateSystemFull.CreateByEPSG((int)Operand2.GetIntResultOut(null));
            SetResultType(ColumnSimpleTypes.Geometry);
            GetGeomResultOut = GetResult;
        }

        private OSGeo.OGR.Geometry GetResult(object data)
        {
            string json = Operand1.GetStrResultOut(data);
            JObject jo = JsonConvert.DeserializeObject<JObject>(json);
            Geometry gg = GeometryExt.GeometryFromGeoJson(jo);
            if (gg != null && !gg.MyIsEmpty() && !gg.MyIsValid()) gg = gg.MyMakeValid();
            return gg;
        }

        public override string ToStr() { return "GeometryFromGeoJson(" + Operand1.ToStr() + "," + Operand2.ToStr() + ")"; }
        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return builder.Driver.ToSql(this, builder);
        }
    }


    public class GeometryFromWKT : FuncExpr_TwoOperand
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (Operand1.GetResultType() != ColumnSimpleTypes.String) TypesException();
            if (Operand2.GetResultType() != ColumnSimpleTypes.Integer) TypesException();
            if (!Operand1.OnlyOnline()) OperandOnlyConstException(1);
            if (!Operand2.OnlyOnline()) OperandOnlyConstException(2);
            _CoordinateSystem = CoordinateSystemFull.CreateByEPSG((int)Operand2.GetIntResultOut(null));
            SetResultType(ColumnSimpleTypes.Geometry);
            GetGeomResultOut = GetResult;
        }

        private OSGeo.OGR.Geometry GetResult(object data)
        {
            string wkt = Operand1.GetStrResultOut(data);
            Geometry gg = OSGeo.OGR.Geometry.CreateFromWkt(wkt);
            if (gg != null && !gg.MyIsEmpty() && !gg.MyIsValid()) gg = gg.MyMakeValid();
            return gg;
        }

        public override string ToStr() { return "GeometryFromWKT(" + Operand1.ToStr() + "," + Operand2.ToStr() + ")"; }
        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return builder.Driver.ToSql(this, builder);
        }
    }

    public class GeometryMakeValid : FuncExpr_OneOperand
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (Operand.GetResultType() != ColumnSimpleTypes.Geometry) TypesException();
            SetResultType(ColumnSimpleTypes.Geometry);
            GetGeomResultOut = GetResult;
            _CoordinateSystem = Operand.GetCoordinateSystem();
        }

        private OSGeo.OGR.Geometry GetResult(object data)
        {
            Geometry g = Operand.GetGeomResultOut(data);
            return g.MyMakeValid();
        }

        public override string ToStr() { return "MakeValid(" + Operand.ToStr() + ")"; }
        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return builder.Driver.ToSql(this, builder);
        }
    }

    public class GeometryEnvelopeMinX : FuncExpr_OneOperand
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (Operand.GetResultType() != ColumnSimpleTypes.Geometry) TypesException();
            SetResultType(ColumnSimpleTypes.Float);
            GetFloatResultOut = GetResult;
        }

        private double GetResult(object data)
        {
            Geometry g = Operand.GetGeomResultOut(data);
            return g.GetRect().MinX;
        }

        public override string ToStr() { return "STEnvelopeMinX(" + Operand.ToStr() + ")"; }
        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return builder.Driver.ToSql(this, builder);
        }
    }
    public class GeometryEnvelopeMaxX : FuncExpr_OneOperand
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (Operand.GetResultType() != ColumnSimpleTypes.Geometry) TypesException();
            SetResultType(ColumnSimpleTypes.Float);
            GetFloatResultOut = GetResult;
        }

        private double GetResult(object data)
        {
            Geometry g = Operand.GetGeomResultOut(data);
            return g.GetRect().MaxX;
        }

        public override string ToStr() { return "STEnvelopeMaxX(" + Operand.ToStr() + ")"; }
        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return builder.Driver.ToSql(this, builder);
        }
    }
    public class GeometryEnvelopeMaxY : FuncExpr_OneOperand
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (Operand.GetResultType() != ColumnSimpleTypes.Geometry) TypesException();
            SetResultType(ColumnSimpleTypes.Float);
            GetFloatResultOut = GetResult;
        }

        private double GetResult(object data)
        {
            Geometry g = Operand.GetGeomResultOut(data);
            return g.GetRect().MaxY;
        }

        public override string ToStr() { return "STEnvelopeMaxY(" + Operand.ToStr() + ")"; }
        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return builder.Driver.ToSql(this, builder);
        }
    }
    public class GeometryEnvelopeMinY : FuncExpr_OneOperand
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (Operand.GetResultType() != ColumnSimpleTypes.Geometry) TypesException();
            SetResultType(ColumnSimpleTypes.Float);
            GetFloatResultOut = GetResult;
        }

        private double GetResult(object data)
        {
            Geometry g = Operand.GetGeomResultOut(data);
            return g.GetRect().MinY;
        }

        public override string ToStr() { return "STEnvelopeMinY(" + Operand.ToStr() + ")"; }
        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return builder.Driver.ToSql(this, builder);
        }
    }

    /// <summary>
    /// Пересекаются ли две геометрии. Булевый результат.
    /// </summary>
    public class GeometryIntersects : FuncExpr_TwoOperand, IOperationForSpatialIndex
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (Operand1.GetResultType() != ColumnSimpleTypes.Geometry && Operand2.GetResultType() != ColumnSimpleTypes.Geometry) TypesException();
            if (Operand1.GetCoordinateSystem() == null || Operand2.GetCoordinateSystem() == null) CoordinateSystemUnknowException();
            
            SetResultType(ColumnSimpleTypes.Boolean);
            GetBoolResultOut = GetResult;
        }

        protected override bool CanCalcOnline()
        {
            return (Operand1.GetCoordinateSystem().IsEqual(Operand2.GetCoordinateSystem()));
        }

        private bool GetResult(object data)
        {
            return Operand1.GetGeomResultOut(data).Intersects(Operand2.GetGeomResultOut(data));
        }

        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return builder.Driver.ToSql(this, builder);
        }
        public override string ToStr() { return "Intersects(" + Operand1.ToStr() + "," + Operand2.ToStr() + ")"; }
    }

    public class MakePoint : FuncExpr_TwoOperand
    {
        public CoordinateSystemFull CoordinateSystemGeometry
        {
            get { return GetCoordinateSystem(); }
            set { _CoordinateSystem = value; }
        }

        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (!(Operand1.GetResultType() == ColumnSimpleTypes.Float || Operand1.GetResultType() == ColumnSimpleTypes.Integer)) TypesException();
            if (!(Operand2.GetResultType() == ColumnSimpleTypes.Float || Operand2.GetResultType() == ColumnSimpleTypes.Integer)) TypesException();
            SetResultType(ColumnSimpleTypes.Geometry);
            GetGeomResultOut = GetResult;
        }

        private Geometry GetResult(object data)
        {
            Geometry g = new Geometry(wkbGeometryType.wkbPoint);
            g.SetPoint_2D(0, Operand1.GetFloatResultOut(data), Operand2.GetFloatResultOut(data));
            return g;
        }

        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return builder.Driver.ToSql(this, builder);
            //return "geometry::STGeomFromText('POINT ('+cast(" + Operand1.ToSQL(builder) + " as nvarchar)+' '+cast(" + Operand2.ToSQL(builder) + " as nvarchar)+')',0)";
        }
        public override string ToStr() { return "MakePoint(" + Operand1.ToStr() + "," + Operand2.ToStr() + ")"; }
    }
    public class GeomIsEmpty : FuncExpr_OneOperand
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (Operand == null) OperandNotFoundException();
            if (Operand.GetResultType() != ColumnSimpleTypes.Geometry) this.TypesException();
            SetResultType(ColumnSimpleTypes.Boolean);
            GetBoolResultOut = DoIsEmpty;
        }

        private bool DoIsEmpty(object data)
        {
            Geometry g = Operand.GetGeomResultOut(data);
            if (g == null) return true;
            return g.IsEmpty();
        }
        public override int Priority() { return PriorityConst.Default; }
        public override string ToStr() { return "GeomIsEmpty(" + Operand.ToStr() + ")"; }
        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return builder.Driver.ToSql(this, builder);
        }
    }

    public class FieldXYExpr : FieldExpr
    {
        public string X;
        public string Y;
        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            var gExp = new MakePoint();
            FieldExpr fX = new FieldExpr();
            TableQuery.FieldCreator.MakeField(Table, X, fX);
            FieldExpr fY = new FieldExpr();
            TableQuery.FieldCreator.MakeField(Table, Y, fY);
            ((MakePoint)gExp).AddChild(fX);
            ((MakePoint)gExp).AddChild(fY);
            ((MakePoint) gExp).CoordinateSystemGeometry = ExpUtils.GetGeometryTable(Table).CoordinateSystem;
            gExp.Prepare();
            return builder.Driver.ToSql(gExp, builder);

        }
    }
     

     * кусок из BaseExpressionFactory
                    case "stenvelopeminx":
                        ex = new GeometryEnvelopeMinX();
                        break;
                    case "stenvelopeminy":
                        ex = new GeometryEnvelopeMinY();
                        break;
                    case "stenvelopemaxy":
                        ex = new GeometryEnvelopeMaxY();
                        break;
                    case "stenvelopemaxx":
                        ex = new GeometryEnvelopeMaxX();
                        break;
                    case "stintersects":
                    case "intersects":
                        ex = new GeometryIntersects();
                        break;
                    case "geometrytowkbhex"://пока не использовать! Возникает неопределённость с системой координат
                        ex = new GeometryToWkbHex();
                        break;
                    case "geometryfromwkbhex"://пока не использовать! Возникает неопределённость с системой координат
                        ex = new GeometryFromWkbHex();
                        break;
                    case "geometryfromgeojson":
                        ex = new GeometryFromGeoJson();
                        break;
                    case "geometryfromrasterlayer":
                        ex = new GeometryFromRasterLayer();
                        break;
                    case "geometryfromvectorlayer":
                        ex = new GeometryFromVectorLayer();
                        break;
                    case "geometryfromvectorlayerunion":
                        ex = new GeometryFromVectorLayerUnion();
                        break;
                    case "geomfromtext":
                    case "geometryfromwkt":
                        ex = new GeometryFromWKT();
                        break;
                    case "st_buffer":
                    case "buffer":
                        ex = new GeometryBuffer();
                        break;
                    case "makevalid":
                        ex = new GeometryMakeValid();
                        break;
                    case "makepoint":
                        ex = new MakePoint();
                        break;
                    case "geomisempty":
                        ex = new GeomIsEmpty();
                        break;
                    case "contains":
                        ex = new Contains_operation();
                        break;
                    case "containsic"://ic = ignore case
                        ex = new ContainsIgnoreCase_operation();
                        break;
                    case "startwith":
                        ex = new StartWith_operation();
                        break;
     
    public interface IExpressionWithAccessControl
    {
        void VarifyAccess(User u);
    }

     * из FieldCreator
        public virtual string GetDefaultColumnAlias(Expression expr)
        {
            if (expr is SubExpression && ((SubExpression) expr).Childs != null && ((SubExpression) expr).Childs.Count == 0) return GetDefaultColumnAlias(expr.Childs[0]);
            if (expr is FieldXYExpr) return ((FieldXYExpr) expr).FieldName;
            return null;
        }

     
     */
}
