
namespace ParserCore
{

    public class FieldCapExpr : SubExpression
    {
        public string FieldAlias;
        public string TableAlias;
        public LexExpr Lexem;
    }

    /// <summary>
    /// Специальный класс динамических величин. Всё что не Field можно заменить константой
    /// </summary>
    public class FieldExpr : Expression
    {
        public string FieldName;
        public string TableAlias;
        public SelectTable Table;

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
            string tableAlias = "";
            if (Table != null)
            {
                tableAlias = Table.Alias;
                //if (string.IsNullOrEmpty(tableAlias)) tableAlias = Table.Name;
                if (!string.IsNullOrEmpty(tableAlias))
                    return BaseExpressionFactory.TableSqlCodeEscape(tableAlias) + "." + BaseExpressionFactory.StandartCodeEscape(FieldName, '[', ']');
            }

            return BaseExpressionFactory.StandartCodeEscape(FieldName, '[', ']');
        }
        protected override bool CanCalcOnline() { return false; }

        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            string tbl = string.Empty;
            if (Table != null)
            {
                tbl = Table.InternalAlias;
                //if (string.IsNullOrEmpty(tbl)) tbl = Table.Name;
                if (!string.IsNullOrEmpty(tbl) && builder.TableQuote)
                    tbl = BaseExpressionFactory.TableSqlCodeEscape(tbl);
            }
            string field;
            if (builder.FieldQuote) field = BaseExpressionFactory.TableSqlCodeEscape(FieldName);
            else field = FieldName;

            if (string.IsNullOrEmpty(tbl))
            {
                return field;
            }
            else
            {
                return tbl + "." + field;
            }
        }
    }


    public class SelectExpresion : Expression
    {
        public override bool IsFunction() { return false; }
        public override bool IsOperation() { return false; }
        protected override bool CanCalcOnline() { return false; }

        public GmSqlQuery Query = new GmSqlQuery();

        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            Query.Prepare();
        }

        public override string ToStr()
        {
            return Query.ToStr();
        }

        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return "(" + Query.MakeSelectExpression(builder) +")";
        }
    }

}
