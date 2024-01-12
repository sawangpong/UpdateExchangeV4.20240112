using System;
using System.Collections.Generic;

namespace UpdateExchangeV4.Models
{
	public partial class Exchcurr
	{
		public int Lineid { get; set; }
		public string Currency { get; set; } = null!;
		public decimal Exchangerate { get; set; } = 0m;
		public int Fiscyear { get; set; } = DateTime.Today.Year;
		public int Fiscperiod { get; set; } = DateTime.Today.Month;
		public decimal Effectivedt { get; set; } = DateTime.Today.Date2Num();
		public decimal Expiredt { get; set; } = DateTime.Today.AddDays(1).Date2Num();
		public byte Revision { get; set; } = 0;
		public DateTime Lastupdate { get; set; } = DateTime.Now;
		public DateTime Bankupdate { get; set; } = DateTime.Now;
	}
}
