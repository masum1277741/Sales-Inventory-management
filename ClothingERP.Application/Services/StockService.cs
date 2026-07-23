namespace ClothingERP.Application.Services;

public class StockService : IStockService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IRealtimeNotifier _realtime;
    private readonly ICurrentBranchProvider _branchProvider;

    public StockService(IUnitOfWork uow, IMapper mapper, IRealtimeNotifier realtime,
                         ICurrentBranchProvider branchProvider)
        => (_uow, _mapper, _realtime, _branchProvider) = (uow, mapper, realtime, branchProvider);

    private int ResolveBranch(int? branchId)
        => (branchId is > 0) ? branchId.Value : _branchProvider.GetCurrentBranchId();


    public async Task<IEnumerable<StockListDto>> GetAllAsync(int? branchId = null)
    {
        var all = await _uow.Stocks.GetWithDetailsAsync();

       
        var filtered = branchId.HasValue ? all.Where(s => s.BranchId == branchId.Value) : all;

        return _mapper.Map<IEnumerable<StockListDto>>(filtered);
    }

    public async Task<StockDto?> GetByVariantIdAsync(int variantId, int? branchId = null)
    {
        var resolvedBranch = ResolveBranch(branchId);
        var s = await _uow.Stocks.GetByVariantAndBranchAsync(variantId, resolvedBranch);
        return s == null ? null : _mapper.Map<StockDto>(s);
    }

    public async Task<decimal> GetVariantQuantityAsync(int variantId, int? branchId = null)
    {
        var resolvedBranch = ResolveBranch(branchId);
        var s = await _uow.Stocks.GetByVariantAndBranchAsync(variantId, resolvedBranch);
        return s?.Quantity ?? 0;
    }

    public async Task<IEnumerable<StockListDto>> GetLowStockAsync(int? branchId = null)
        => _mapper.Map<IEnumerable<StockListDto>>(await _uow.Stocks.GetLowStockAsync(branchId));

    public async Task<IEnumerable<StockListDto>> GetOutOfStockAsync(int? branchId = null)
        => _mapper.Map<IEnumerable<StockListDto>>(await _uow.Stocks.GetOutOfStockAsync(branchId));

    public async Task<decimal> GetTotalStockValueAsync(int? branchId = null)
        => await _uow.Stocks.GetTotalStockValueAsync(branchId);

    // ── Manual Adjustment (branch-aware) ───────────────────────────────────
    public async Task<ServiceResult> AdjustStockAsync(StockAdjustmentDto dto, int userId)
    {
        var branchId = ResolveBranch(dto.BranchId);

        var stock = await _uow.Stocks.GetByVariantAndBranchAsync(dto.ProductVariantId, branchId);

        if (stock == null)
        {
            stock = new Stock
            {
                ProductVariantId = dto.ProductVariantId,
                BranchId = branchId,
                Quantity = 0,
                CreatedBy = userId,
                UpdatedBy = userId
            };
            await _uow.Stocks.AddAsync(stock);
            await _uow.SaveChangesAsync(); 
        }

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

        var variant = await _uow.ProductVariants.GetByIdAsync(dto.ProductVariantId);
        await _realtime.NotifyStockUpdatedAsync(
            dto.ProductVariantId, variant?.Barcode ?? "", (int)dto.NewQuantity, variant?.Product?.Name ?? "");

        return ServiceResult.Ok("Stock adjusted successfully.");
    }

    public async Task UpdateStockAsync(int variantId, int branchId, decimal quantity,
                                        StockMovementType type, string referenceNumber, int userId)
    {
        var stock = await _uow.Stocks.GetByVariantAndBranchAsync(variantId, branchId);
        if (stock == null)
        {
            stock = new Stock
            {
                ProductVariantId = variantId,
                BranchId = branchId,
                Quantity = 0,
                CreatedBy = userId,
                UpdatedBy = userId
            };
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