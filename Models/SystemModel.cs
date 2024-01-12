namespace UpdateExchangeV4.Models
{
    public class Scheduled
    {
        public DateTime ScheduledTime1 { get; set; } = new DateTime();
        public DateTime ScheduledTime2 { get; set; } = new DateTime();
        public DateTime ScheduledTime3 { get; set; } = new DateTime();
    }

    public class APIConfig
    {
        public string BangkokBang_API_Key { get; set; } = string.Empty;
        public string LineToken { get; set; } = string.Empty;
    }

    public class URLConfig
    {
        public string URL_Goldprice { get; set; } = string.Empty;
        public string URL_Silverprice { get; set; } = string.Empty;
        public string URL_BKKAPI { get; set; } = string.Empty;
        public string URL_LineAPI { get; set; } = string.Empty;
    }

    public class GoldPrice
    {
        public decimal GOLD_PRICE_OZ { get; set; } = 0m;
        public decimal GOLD_BUY_PRICE_THB { get; set; } = 0m;
        public decimal GOLD_SELL_PRICE_THB { get; set; } = 0m;
    }

    public class SilverPrice
    {
        public decimal SILVER_PRICE_OZ { get; set; } = 0m;
        public decimal SILVER_BUY_PRICE_THB { get; set; } = 0m;
        public decimal SILVER_SELL_PRICE_THB { get; set; } = 0m;
    }

}