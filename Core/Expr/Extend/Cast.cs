using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore.Expr.Simple;

namespace ParserCore.Expr.Extend
{
    public class Cast : FuncExpr_OneOperand
    {
        public ExactType ToType;

        public override void Prepare()
        {
            base.Prepare();
            var simpleOut = ToType.GetSimpleType();
            switch (simpleOut)
            {
                case SimpleTypes.Blob:
                    GetBlobResultOut = data => { return Operand.GetBlobResultOut(data); };
                    break;
                case SimpleTypes.String:
                    GetStrResultOut = data =>
                        {
                            return Operand.GetStrResultOut(data);
                        };
                    break;
                case SimpleTypes.Boolean:
                    GetBoolResultOut = data => { return Operand.GetBoolResultOut(data); };
                    break;
                case SimpleTypes.Date:
                case SimpleTypes.DateTime:
                    GetDateTimeResultOut = data => { return Operand.GetDateTimeResultOut(data); };
                    break;
                case SimpleTypes.Float:
                    GetFloatResultOut = data => { return Operand.GetFloatResultOut(data); };
                    break;
                case SimpleTypes.Geometry:
                    GetGeomResultOut = data => { return Operand.GetGeomResultOut(data); };
                    break;
                case SimpleTypes.Integer:
                    GetIntResultOut = data => { return Operand.GetIntResultOut(data); };
                    break;
                case SimpleTypes.Time:
                    GetTimeResultOut = data => { return Operand.GetTimeResultOut(data); };
                    break;
                default:
                    throw new Exception("unsupported type");
            }
            SetResultType(simpleOut);
        }

        public override string ToStr()
        {
            return "cast(" + Operand.ToStr() + " as " + ToType.ToStr() + ")";
        }

        public override string  ToSql(ExpressionSqlBuilder builder)
        {
            return "cast(" + Operand.ToSql(builder) + " as " + ToType.ToSql(builder) + ")";
        }

        public override void ParseInside(ExpressionParser parser)
        {
            var collection = parser.Collection;
            collection.GotoNextMust();
            var lex = collection.GotoNextMust();

            ExpressionParser tonode = new ExpressionParser();
            tonode.Parse(collection);
            AddChild(tonode.Single());
            lex = collection.CurrentLexem();
            if (lex.LexemType != LexType.Command || lex.LexemText.ToLower() != "as") collection.ErrorWaitKeyWord("AS", collection.CurrentOrLast());
            lex = collection.GotoNextMust();
            var ex = ExactType.Parse(collection);
            if (ex == null) collection.Error("Type is unknow", collection.CurrentOrLast());
            ToType = ex.Value;
            lex = collection.GotoNextMust();
            if (!lex.IsSkobraClose()) collection.Error("not closed function", collection.CurrentOrLast());
            collection.GotoNext();
        }
    }
}
