namespace ClothingERP.Web.Helpers;

public static class FormatHelper
{
    public static string BDT(object? value)
        => $"৳{Convert.ToDecimal(value ?? 0):N2}";

    public static string USD(object? value)
        => $"${Convert.ToDecimal(value ?? 0):N2}";

    public static string MVR(object? value)
        => $"Rf{Convert.ToDecimal(value ?? 0):N2}";

    public static string Num(object? value)
        => $"{Convert.ToDecimal(value ?? 0):N0}";
}