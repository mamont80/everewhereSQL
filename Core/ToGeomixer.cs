using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
    /*
     

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
