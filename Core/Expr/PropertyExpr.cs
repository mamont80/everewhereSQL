using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public class PropertyExpr : Expression
    {
        public string FieldName;
        public Lexem Lexem;

        public void Init(SimpleTypes tp)
        {
            Init(tp, 0);
        }

        protected int _CoordinateSystem = -1;
        public override int GetCoordinateSystem()
        {
            return _CoordinateSystem;
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
            if (!string.IsNullOrEmpty(FieldName))
            {
                if (!string.IsNullOrEmpty(res)) res += ".";
                res += ParserUtils.TableToStrEscape(FieldName);
            }
            return res;
        }

        protected override bool CanCalcOnline() { return false; }
    }
}
