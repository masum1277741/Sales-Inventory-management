namespace ClothingERP.Application.Interfaces.Services;

public interface IProductAttributeService
{
    // Sizes
    Task<IEnumerable<SizeDto>> GetSizesAsync();
    Task<ServiceResult<SizeDto>> CreateSizeAsync(CreateSizeDto dto, int userId);
    Task<ServiceResult<SizeDto>> UpdateSizeAsync(int id, CreateSizeDto dto, int userId);
    Task<ServiceResult> DeleteSizeAsync(int id);

    // Colors
    Task<IEnumerable<ColorDto>> GetColorsAsync();
    Task<ServiceResult<ColorDto>> CreateColorAsync(CreateColorDto dto, int userId);
    Task<ServiceResult<ColorDto>> UpdateColorAsync(int id, CreateColorDto dto, int userId);
    Task<ServiceResult> DeleteColorAsync(int id);
}