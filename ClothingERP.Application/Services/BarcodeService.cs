using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.Rendering;

namespace ClothingERP.Application.Services;

public class BarcodeService : IBarcodeService
{
    public string GenerateSKU(string categoryCode, int sequence)
    {
        var prefix = categoryCode.Length >= 3
            ? categoryCode[..3].ToUpper()
            : categoryCode.ToUpper().PadRight(3, 'X');
        return $"{prefix}{sequence:D5}";
    }

    public string GenerateBarcode(string sku, int sizeId, int colorId)
        => $"{sku}{sizeId:D2}{colorId:D2}";

    public byte[] GenerateBarcode128Image(string barcode, int width = 300, int height = 80)
    {
        var writer = new BarcodeWriterSvg
        {
            Format = BarcodeFormat.CODE_128,
            Options = new EncodingOptions { Width = width, Height = height, Margin = 5, PureBarcode = false }
        };
        var svg = writer.Write(barcode);
        return System.Text.Encoding.UTF8.GetBytes(svg.Content);
    }

    public byte[] GenerateQRCodeImage(string data, int size = 200)
    {
        var writer = new BarcodeWriterSvg
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions { Width = size, Height = size, Margin = 1 }
        };
        var svg = writer.Write(data);
        return System.Text.Encoding.UTF8.GetBytes(svg.Content);
    }
}