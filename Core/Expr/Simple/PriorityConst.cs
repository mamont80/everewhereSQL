using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.Simple
{
    /*
     *   Новая версия лексем с возможностью быстрого вычисления результата
     *   
     * Из описания SQL Server 
    8
    ~ (побитовое НЕ)
    7
    * (умножение), / (деление), % (остаток от деления)
    6
    + (положительное), - (отрицательное), + (сложение), (+ объединение), - (вычитание), & (побитовое И), ^ (побитовое исключающее ИЛИ), | (побитовое ИЛИ)
    5
    =, >, <, >=, <=, <>, !=, !>, !< (операторы сравнения)
    4
    NOT
    3
    And
    2
    ALL, ANY, BETWEEN, IN, LIKE, OR, SOME
    1
    = (присваивание)
     * 
     *   Приоритеты операций:
     *   * /            300
     *   + - UiarMinus    200
     *   > >= < <=      120
     *   <> =          110
     *   ( )            1
     *   and or not     100
     *   contains 150
     */
    public static class PriorityConst
    {
        public const int UnarMinus = 800;
        /// <summary>
        /// * (умножение), / (деление), % (остаток от деления)
        /// </summary>
        public const int MultiDiv = 700;
        /// <summary>
        /// + (положительное), - (отрицательное), + (сложение), (+ объединение), - (вычитание), & (побитовое И), ^ (побитовое исключающее ИЛИ), | (побитовое ИЛИ)
        /// </summary>
        public const int PlusMinus = 600;

        public const int Is = 590;

        public const int Default = 580;

        public const int In = 560;
        /// <summary>
        /// like contain
        /// </summary>
        public const int Like = 550;
        /// <summary>
        /// =, >, <, >=, <=, <>, !=, !>, !< (операторы сравнения)
        /// </summary>
        public const int Compare = 500;
        public const int Not = 400;
        public const int And = 300;
        public const int Or = 110;
        public const int Between = 550;
        /// <summary>
        /// = (присваивание)
        /// </summary>
        public const int Assign = 100;
    }
}
