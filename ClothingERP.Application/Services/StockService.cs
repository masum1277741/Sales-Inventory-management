namespace ClothingERP.Application.Services;

public class StockService : IStockService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public StockService(IUnitOfWork uow, IMapper mapper) => (_uow, _mapper) = (uow, mapper);

    public async Task<IEnumerable<StockListDto>> GetAllAsync()
        => _mapper.Map<IEnumerable<StockListDto>>(await _uow.Stocks.GetWithDetailsAsync());

    public async Task<StockDto?> GetByVariantIdAsync(int variantId)
    {
        var s = await _uow.Stocks.GetByVariantIdAsync(variantId);
        return s == null ? null : _mapper.Map<StockDto>(s);
    }

    public async Task<IEnumerable<StockListDto>> GetLowStockAsync()
        => _mapper.Map<IEnumerable<StockListDto>>(await _uow.Stocks.GetLowStockAsync());

    public async Task<IEnumerable<StockListDto>> GetOutOfStockAsync()
        => _mapper.Map<IEnumerable<StockListDto>>(await _uow.Stocks.GetOutOfStockAsync());

    public async Task<decimal> GetTotalStockValueAsync()
        => await _uow.Stocks.GetTotalStockValueAsync();

    public async Task<ServiceResult> AdjustStockAsync(StockAdjustmentDto dto, int userId)
    {
        var stock = await _uow.Stocks.GetByVariantIdAsync(dto.ProductVariantId);
        if (stock == null) return ServiceResult.Fail("Stock record not found.");

        var prevQty = stock.Quantity;
        stock.Quantity = dto.NewQuantity;
        stock.UpdatedBy = userId;
        _uow.Stocks.Update(stock);

        await _uow.StockMovements.AddAsync(new StockMovement
        {
            StockId = stock.Id,
            MovementType = StockMovementType.Adjustment,
            Quantity = Math.Abs(dto.NewQuantity - prevQty),
            PreviousQuantity = prevQty,
            NewQuantity = dto.NewQuantity,
            Reason = dto.Reason,
            MovementDate = DateTime.UtcNow,
            CreatedBy = userId
        });

        await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Stock adjusted.");
    }

    public async Task UpdateStockAsync(int variantId, decimal quantity, StockMovementType type,
                                       string referenceNumber, int userId)
    {
        var stock = await _uow.Stocks.GetByVariantIdAsync(variantId);
        if (stock == null)
        {
            stock = new Stock { ProductVariantId = variantId, Quantity = 0, CreatedBy = userId };
            await _uow.Stocks.AddAsync(stock);
            await _uow.SaveChangesAsync();
        }

        var prev = stock.Quantity;
        stock.Quantity += quantity;
        stock.UpdatedBy = userId;
        _uow.Stocks.Update(stock);

        await _uow.StockMovements.AddAsync(new StockMovement
        {
            StockId = stock.Id,
            MovementType = type,
            Quantity = Math.Abs(quantity),
            PreviousQuantity = prev,
            NewQuantity = stock.Quantity,
            ReferenceNumber = referenceNumber,
            MovementDate = DateTime.UtcNow,
            CreatedBy = userId
        });
    }
}