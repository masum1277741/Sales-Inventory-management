namespace ClothingERP.Web.Helpers;

public static class CurrencyHelper
{
    public static CurrencyDisplayModel FormatAll(decimal usd, decimal rateBDT, decimal rateMVR)
    {
        return new CurrencyDisplayModel
        {
            USD = usd,
            BDT = Math.Round(usd * rateBDT, 2),
            MVR = Math.Round(usd * rateMVR, 2),
            RateBDT = rateBDT,
            RateMVR = rateMVR
        };
    }

    public static string FormatUSD(decimal amount) => $"${amount:N2}";
    public static string FormatBDT(decimal amount) => $"৳{amount:N2}";
    public static string FormatINR(decimal amount) => $"₹{amount:N2}";
    public static string FormatMVR(decimal amount) => $"Rf{amount:N2}";
}

public class CurrencyDisplayModel
{
    public decimal USD { get; set; }
    public decimal BDT { get; set; }
    public decimal INR { get; set; }
    public decimal MVR { get; set; }
    public decimal RateBDT { get; set; }
    public decimal RateMVR { get; set; }

    public string USDFormatted => $"${USD:N2}";
    public string BDTFormatted => $"৳{BDT:N2}";
    public string INRFormatted => $"₹{INR:N2}";
    public string MVRFormatted => $"Rf{MVR:N2}";
}
