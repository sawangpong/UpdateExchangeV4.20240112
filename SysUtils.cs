namespace UpdateExchangeV4
{
    public static class SysUtils
    {
        public const double OZ2GRAM = 31.10348d;
        public const double MARKUP_FACTOR = 0.6d;
        public const double MARKUP_FACTOR_GOLD = 1.1d;
        public const double MARKUP_FACTOR_SILVER = 1.045d;
        public const string DATE_FORMAT_STRING = "d/M/yyyy HH:mm";
        public const string DATE2NUM_FORMAT = "yyyyMMdd";

        public static decimal Date2Num(this DateTime DateValue) 
            => Convert.ToDecimal($"{DateValue.Year}{("00".Substring(0, 2 - DateValue.Month.ToString().Length) + DateValue.Month)}{("00".Substring(0, 2 - DateValue.Day.ToString().Length) + DateValue.Day)}");
        public static DateTime Num2Date(this decimal DateValue) 
            => DateTime.ParseExact($"{DateValue}", DATE2NUM_FORMAT, null);
    }
}
