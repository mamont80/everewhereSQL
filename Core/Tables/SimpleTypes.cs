using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    /// <summary>
    /// Упрощённые типы полей. Коды не менять! Можно только добавлять. Цифры используются для сортировки операций в TableQuery.Expression - SortType
    /// </summary>
    public enum SimpleTypes
    {
        Unknow = 0,
        Integer = 1,
        Float = 2,
        String = 3,
        Geometry = 4,
        Date = 5,
        DateTime = 6,
        Time = 7,
        Boolean = 8,
        Blob = 9
    } 
}
