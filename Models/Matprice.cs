using System;
using System.Collections.Generic;

namespace UpdateExchangeV4.Models
{
	public partial class Matprice
	{
		public int Seq { get; set; }
		public int Matid { get; set; }
		public decimal Pricedate { get; set; } = 0;
		public int Fiscyear { get; set; } = DateTime.Today.Year;
		public int Fiscperiod { get; set; } = DateTime.Today.Month;
		public decimal Orgpriceusd { get; set; } = 0;
		public decimal Exchangerate { get; set; } = 0;
		public decimal Exchdate { get; set; } = 0;
		public decimal Costthbgram { get; set; } = 0;
		public decimal Pricethbgram { get; set; } = 0;
		public string Audituser { get; set; } = string.Empty;
		public decimal Auditdate { get; set; } = DateTime.Now.Date2Num();
		public string Modiuser { get; set; } = string.Empty;
		public decimal Modidate { get; set; } = 0;
	}
}
