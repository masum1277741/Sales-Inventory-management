namespace ClothingERP.Application.Interfaces.Services;

public interface IBarcodeService
{
    string GenerateBarcodeNumber();
    string GenerateBarcodeSvg(string barcodeNumber);
}