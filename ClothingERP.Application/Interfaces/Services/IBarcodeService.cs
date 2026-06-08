namespace ClothingERP.Application.Interfaces.Services;

public interface IBarcodeService
{
    string GenerateSKU(string categoryCode, int sequence);
    string GenerateBarcode(string sku, int sizeId, int colorId);
    byte[] GenerateBarcode128Image(string barcode, int width = 300, int height = 80);
    byte[] GenerateQRCodeImage(string data, int size = 200);
}