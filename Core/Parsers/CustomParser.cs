using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public abstract class CustomParser
    {
        public LexemCollection Collection { get; private set; }

        public List<Expression> Results = new List<Expression>();

        public CustomParser(LexemCollection collection)
        {
            Collection = collection;
        }

        public Expression Single()
        {
            if (Results.Count != 1) throw new Exception("Error in expression");
            return Results[0];
        }

        public abstract void Parse();
    }
}
