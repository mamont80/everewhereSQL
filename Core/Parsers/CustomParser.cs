using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public abstract class CustomParser
    {
        public LexemCollection Collection { get; protected set; }

        public List<Expression> Results = new List<Expression>();

        public Expression Single()
        {
            if (Results.Count != 1) throw new Exception("Error in expression");
            return Results[0];
        }

        public virtual void Parse(LexemCollection collection)
        {
            Collection = collection;
        }
    }
}
