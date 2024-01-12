
using System.Collections.Specialized;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using UpdateExchangeV4.Models;

namespace UpdateExchangeV4.Services
{
    public interface ICoreService
    {
        Exchcurr GetExchangeRateFromAPI();
        GoldPrice GetGoldPriceFromAPI(double ExchangeRate);
        SilverPrice GetSilverPriceFromAPI(double ExchangeRate);
        Task UpdateDB(Exchcurr ex, GoldPrice gp, SilverPrice sp);
        Task LineNotify(string function, string message);
        Task WriteTextFile(string message);
    }

    public class CoreService : ICoreService
    {
        private readonly ILogger<CoreService> _logger;
        private readonly IOptions<APIConfig> _api;
        private readonly IOptions<URLConfig> _url;
        private readonly GoldPrice _gold;
        private readonly SilverPrice _silver;
        private readonly DBContext _dbContext;

        public CoreService(ILogger<CoreService> logger,
                                 IOptions<APIConfig> api,
                                 IOptions<URLConfig> url,
                                 GoldPrice gold,
                                 SilverPrice silver,
                                 DBContext dBContext)
        {
            _logger = logger;
            _api = api;
            _url = url;
            _gold = gold;
            _silver = silver;
            _dbContext = dBContext;
        }


        public Exchcurr GetExchangeRateFromAPI()
        {
            Exchcurr lsExch = new Exchcurr();

            using (HttpClient client = new HttpClient(new HttpClientHandler() { AutomaticDecompression = System.Net.DecompressionMethods.GZip }))
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _api.Value.BangkokBang_API_Key);
                var result = client.GetAsync(_url.Value.URL_BKKAPI).Result;

                HttpResponseMessage message = result.EnsureSuccessStatusCode();
                JArray jArray = JArray.Parse(message.Content.ReadAsStringAsync().Result);

                lsExch.Currency = jArray[2]["Family"]!.ToString().Substring(0, 3);
                lsExch.Exchangerate = Convert.ToDecimal(jArray[2]["SellingRates"]);
                lsExch.Fiscyear = DateTime.Now.Year;
                lsExch.Fiscperiod = DateTime.Now.Month;
                lsExch.Effectivedt = DateTime.Now.Date2Num();
                lsExch.Expiredt = DateTime.Now.AddDays(1).Date2Num();
                lsExch.Revision = Convert.ToByte(jArray[2]["Update"]);
                lsExch.Lastupdate = DateTime.Now;
                lsExch.Bankupdate = DateTime.ParseExact($"{jArray[2]["Ddate"]!.ToString().Trim()} {jArray[2]["DTime"]!.ToString().Trim()}", SysUtils.DATE_FORMAT_STRING, null);

                client.Dispose();

                _logger.LogInformation("GetExchangeRateFromAPI Success");

            }

            return lsExch;
        }

        public GoldPrice GetGoldPriceFromAPI(double ExchangeRate)
        {
            GoldPrice lsGold = new GoldPrice();
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(_url.Value.URL_Goldprice);

            double GOLD_PRICE_OZ = Convert.ToDouble(xdoc.DocumentElement!.SelectSingleNode("/mts/items/item")!["ask"]!.InnerText) + SysUtils.MARKUP_FACTOR;
            double GOLD_BUY_PRICE_THB = GOLD_PRICE_OZ / SysUtils.OZ2GRAM * ExchangeRate;
            double GOLD_SELL_PRICE_THB = GOLD_BUY_PRICE_THB * SysUtils.MARKUP_FACTOR_GOLD;

            lsGold.GOLD_PRICE_OZ = Convert.ToDecimal(GOLD_PRICE_OZ);
            lsGold.GOLD_BUY_PRICE_THB = Convert.ToDecimal(GOLD_BUY_PRICE_THB);
            lsGold.GOLD_SELL_PRICE_THB = Convert.ToDecimal(GOLD_SELL_PRICE_THB);

            _logger.LogInformation("GetGoldPriceFromAPI Success");
            return lsGold;
        }

        public SilverPrice GetSilverPriceFromAPI(double ExchangeRate)
        {
            SilverPrice lsSilver = new SilverPrice();
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(_url.Value.URL_Silverprice);

            double SILVER_PRICE_OZ = Convert.ToDouble(xdoc.DocumentElement!.SelectSingleNode("/mts/items/item")!["ask"]!.InnerText) + SysUtils.MARKUP_FACTOR;
            double SILVER_BASE_PRICE_THB = SILVER_PRICE_OZ / SysUtils.OZ2GRAM * ExchangeRate;
            double SILVER_SELL_PRICE_THB = SILVER_BASE_PRICE_THB * SysUtils.MARKUP_FACTOR_SILVER;

            lsSilver.SILVER_PRICE_OZ = Convert.ToDecimal(SILVER_PRICE_OZ);
            lsSilver.SILVER_BUY_PRICE_THB = Convert.ToDecimal(SILVER_BASE_PRICE_THB);
            lsSilver.SILVER_SELL_PRICE_THB = Convert.ToDecimal(SILVER_SELL_PRICE_THB);

            _logger.LogInformation("GetSilverPriceFromAPI Success");
            return lsSilver;
        }

        public Task UpdateDB(Exchcurr ex, GoldPrice gp, SilverPrice sp)
        {
            return Task.Run(() =>
            {
                decimal numDateNow = DateTime.Now.Date2Num();
                try
                {
                    // insert data to database
                    if (_dbContext.Exchcurrs.Any(x => x.Effectivedt == numDateNow))
                    {
                        Exchcurr updateData = _dbContext.Exchcurrs.Where(x => x.Effectivedt == numDateNow).ToList().MaxBy(x => x.Revision)!;
                        if (updateData.Revision == ex.Revision)
                        {
                            _logger!.LogInformation("Update OldRevision");
                            updateData.Lastupdate = DateTime.Now;
                            updateData.Bankupdate = ex.Bankupdate;
                        }
                        else
                        {
                            _logger!.LogInformation("Add Exchcurr NewRevision");
                            _dbContext.Exchcurrs.Add(ex);
                        }
                    }
                    else
                    {
                        _logger!.LogInformation("Add Exchcurr NewDay");
                        _dbContext.Exchcurrs.Add(ex);
                    }

                    Matprice goldPrice = new Matprice()
                    {
                        Matid = 12,
                        Pricedate = numDateNow,
                        Fiscyear = numDateNow.Num2Date().Year,
                        Fiscperiod = numDateNow.Num2Date().Month,
                        Orgpriceusd = gp.GOLD_PRICE_OZ,
                        Exchangerate = ex.Exchangerate,
                        Exchdate = ex.Bankupdate.Date2Num(),
                        Costthbgram = gp.GOLD_BUY_PRICE_THB,
                        Pricethbgram = gp.GOLD_SELL_PRICE_THB,
                        Audituser = "ServiceExChange",
                        Auditdate = numDateNow
                    };

                    Matprice silverPrice = new Matprice()
                    {
                        Matid = 2,
                        Pricedate = numDateNow,
                        Fiscyear = numDateNow.Num2Date().Year,
                        Fiscperiod = numDateNow.Num2Date().Month,
                        Orgpriceusd = sp.SILVER_PRICE_OZ,
                        Exchangerate = ex.Exchangerate,
                        Exchdate = ex.Bankupdate.Date2Num(),
                        Costthbgram = sp.SILVER_BUY_PRICE_THB,
                        Pricethbgram = sp.SILVER_SELL_PRICE_THB,
                        Audituser = "ServiceExChange",
                        Auditdate = numDateNow
                    };

                    _dbContext.Matprices.Add(goldPrice);
                    _dbContext.Matprices.Add(silverPrice);
                    _dbContext.SaveChangesAsync().ContinueWith((value) =>
                    {
                        if (value.IsCompletedSuccessfully)
                        {
                            _logger.LogInformation("Write DB Success");
                            // notify to Line Apps after completed insert data
                            string message = $"Date   : {ex.Lastupdate} \n GoldPrice   : {gp.GOLD_SELL_PRICE_THB:N2} THB \n SilverPrice : {sp.SILVER_SELL_PRICE_THB:N2} THB ";
                            LineNotify(" \n Update Material Price", $"Rev. {ex.Revision} \n {message}");
                            WriteTextFile(message);
                        }
                    });

                }
                catch (Exception ex)
                {
                    _logger!.LogError(ex.Message.ToString());
                }
            });
        }

        public Task LineNotify(string function, string message)
        {
            return Task.Run(async () =>
            {
                try
                {
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_api.Value.LineToken}");
                    client.DefaultRequestHeaders.ConnectionClose = true;

                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes($"message={function} : {message}");
                    ByteArrayContent content = new ByteArrayContent(buffer);
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                    await client.PostAsync(_url.Value.URL_LineAPI, content);
                }
                catch (Exception ex)
                {
                    _logger!.LogError(ex.Message.ToString());
                }
            });
        }

        public Task WriteTextFile(string message)
        {
            return Task.Run(async () =>
            {
                try
                {
                    string _path = AppDomain.CurrentDomain.BaseDirectory + "UpdateExchange_log.txt";
                    await File.AppendAllLinesAsync(_path, new[] { message });
                    _logger.LogInformation(message);
                }
                catch (Exception ex)
                {
                    _logger!.LogError(ex.Message.ToString());
                }
            });
        }
    }
}