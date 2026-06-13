namespace ClothingERP.Application.Services;

public class BarcodeService : IBarcodeService
{
    // ── Unique Barcode Number Generate ─────────────────────────────────────
    // Format: CLZ + Year(2) + Random(6) = "CLZ24000001"
    public string GenerateBarcodeNumber()
    {
        var year = DateTime.Now.ToString("yy");
        var random = new Random();
        var number = random.Next(100000, 999999);
        return $"CLZ{year}{number}";
    }

    // ── SVG Barcode Image Generate (ZXing) ─────────────────────────────────
    public string GenerateBarcodeSvg(string barcodeNumber)
    {
        try
        {
            var writer = new ZXing.BarcodeWriterSvg
            {
                Format = ZXing.BarcodeFormat.CODE_128,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = 200,
                    Height = 60,
                    Margin = 4,
                    PureBarcode = false
                }
            };

            var svg = writer.Write(barcodeNumber);
            return svg.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }
}