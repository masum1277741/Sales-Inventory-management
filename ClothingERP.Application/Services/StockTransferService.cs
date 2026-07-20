namespace ClothingERP.Application.Services;

public class StockTransferService : IStockTransferService
{
    private readonly IUnitOfWork _uow;
    private readonly IRealtimeNotifier _realtime;   

    public StockTransferService(IUnitOfWork uow, IRealtimeNotifier realtime) => (_uow, _realtime) = (uow, realtime);

    public async Task<IEnumerable<StockTransferListDto>> GetAllAsync(int? branchId = null)
    {
        var query = _uow.StockTransfers.GetQueryable()
            .Include(t => t.FromBranch).Include(t => t.ToBranch).Include(t => t.Items)
            .Where(t => !t.IsDeleted);

        if (branchId.HasValue)
            query = query.Where(t => t.FromBranchId == branchId || t.ToBranchId == branchId);

        var transfers = await query.OrderByDescending(t => t.TransferDate).ToListAsync();

        return transfers.Select(t => new StockTransferListDto
        {
            Id = t.Id,
            TransferNumber = t.TransferNumber,
            FromBranchName = t.FromBranch.Name,
            ToBranchName = t.ToBranch.Name,
            Status = t.Status,
            ItemCount = t.Items.Count,
            TransferDate = t.TransferDate
        });
    }

    public async Task<StockTransferDto?> GetByIdAsync(int id)
    {
        var t = await _uow.StockTransfers.GetQueryable()
            .Include(x => x.FromBranch).Include(x => x.ToBranch)
            .Include(x => x.Items).ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Product)
            .Include(x => x.Items).ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Size)
            .Include(x => x.Items).ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Color)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (t == null) return null;

        var itemDtos = new List<StockTransferItemDto>();
        foreach (var item in t.Items)
        {
            var sourceStock = await _uow.Stocks.GetByVariantAndBranchAsync(item.ProductVariantId, t.FromBranchId);
            itemDtos.Add(new StockTransferItemDto
            {
                ProductVariantId = item.ProductVariantId,
                ProductName = item.ProductVariant.Product.Name,
                SizeName = item.ProductVariant.Size.Name,
                ColorName = item.ProductVariant.Color.Name,
                RequestedQty = item.RequestedQty,
                ReceivedQty = item.ReceivedQty,
                AvailableAtSource = (int)(sourceStock?.Quantity ?? 0)
            });
        }

        return new StockTransferDto
        {
            Id = t.Id,
            TransferNumber = t.TransferNumber,
            FromBranchName = t.FromBranch.Name,
            ToBranchName = t.ToBranch.Name,
            Status = t.Status,
            ItemCount = t.Items.Count,
            TransferDate = t.TransferDate,
            Notes = t.Notes,
            Items = itemDtos
        };
    }


    public async Task<ServiceResult<StockTransferDto>> CreateAsync(CreateStockTransferDto dto, int fromBranchId, int userId)
    {
        if (fromBranchId == dto.ToBranchId)
            return ServiceResult<StockTransferDto>.Fail("একই branch এ transfer করা যাবে না।");

        var transfer = new StockTransfer
        {
            TransferNumber = $"TRF-{DateTime.Now:yyyyMMddHHmmss}",
            FromBranchId = fromBranchId,
            ToBranchId = dto.ToBranchId,
            Status = "InTransit",
            Notes = dto.Notes,
            CreatedBy = userId
        };
        await _uow.StockTransfers.AddAsync(transfer);
        await _uow.SaveChangesAsync();

        foreach (var item in dto.Items)
        {

            var success = await _uow.Stocks.TryDecrementAsync(item.ProductVariantId, fromBranchId, item.Quantity);
            if (!success)
            {
                var variant = await _uow.ProductVariants.GetByIdAsync(item.ProductVariantId);
                return ServiceResult<StockTransferDto>.Fail(
                    $"'{variant?.Product?.Name}' এর জন্য পর্যাপ্ত stock নেই উৎস branch এ।");
            }

            await _uow.StockTransferItems.AddAsync(new StockTransferItem
            {
                StockTransferId = transfer.Id,
                ProductVariantId = item.ProductVariantId,
                RequestedQty = item.Quantity,
                CreatedBy = userId
            });
        }
        await _uow.SaveChangesAsync();

        var result = await GetByIdAsync(transfer.Id);
        return ServiceResult<StockTransferDto>.Ok(result!, $"Transfer {transfer.TransferNumber} তৈরি হয়েছে — গন্তব্য branch এ receive করার অপেক্ষায়।");
    }


    public async Task<ServiceResult> ReceiveAsync(ReceiveStockTransferDto dto, int userId)
    {
        var transfer = await _uow.StockTransfers.GetByIdAsync(dto.StockTransferId);
        if (transfer == null) return ServiceResult.Fail("Transfer not found.");
        if (transfer.Status != "InTransit") return ServiceResult.Fail("এই transfer ইতিমধ্যে process হয়ে গেছে।");

        var items = await _uow.StockTransferItems.GetQueryable()
            .Where(i => i.StockTransferId == transfer.Id && !i.IsDeleted).ToListAsync();

        foreach (var receiveItem in dto.Items)
        {
            var transferItem = items.FirstOrDefault(i => i.ProductVariantId == receiveItem.ProductVariantId);
            if (transferItem == null) continue;

            transferItem.ReceivedQty = receiveItem.ReceivedQty;
            _uow.StockTransferItems.Update(transferItem);


            await _uow.Stocks.IncrementAsync(receiveItem.ProductVariantId, transfer.ToBranchId, receiveItem.ReceivedQty);

            // ── Real-time broadcast (Feature #13) ───────────────────────────────
            var stock = await _uow.Stocks.GetByVariantAndBranchAsync(receiveItem.ProductVariantId, transfer.ToBranchId);
            var variant = await _uow.ProductVariants.GetByIdAsync(receiveItem.ProductVariantId);
            await _realtime.NotifyStockUpdatedAsync(receiveItem.ProductVariantId, variant?.Barcode ?? "", (int)(stock?.Quantity ?? 0), variant?.Product?.Name ?? "");
        }

        transfer.Status = "Received";
        transfer.ReceivedDate = DateTime.UtcNow;
        transfer.ReceivedBy = userId;
        _uow.StockTransfers.Update(transfer);
        await _uow.SaveChangesAsync();

        return ServiceResult.Ok($"Transfer {transfer.TransferNumber} সফলভাবে receive করা হয়েছে।");
    }

 
    public async Task<ServiceResult> CancelAsync(int id, int userId)
    {
        var transfer = await _uow.StockTransfers.GetByIdAsync(id);
        if (transfer == null) return ServiceResult.Fail("Transfer not found.");
        if (transfer.Status != "InTransit") return ServiceResult.Fail("শুধু InTransit transfer বাতিল করা যাবে।");

        var items = await _uow.StockTransferItems.GetQueryable()
            .Where(i => i.StockTransferId == id && !i.IsDeleted).ToListAsync();

        foreach (var item in items)
            await _uow.Stocks.IncrementAsync(item.ProductVariantId, transfer.FromBranchId, item.RequestedQty);   

        transfer.Status = "Cancelled";
        transfer.UpdatedBy = userId; transfer.UpdatedAt = DateTime.UtcNow;
        _uow.StockTransfers.Update(transfer);
        await _uow.SaveChangesAsync();

        return ServiceResult.Ok("Transfer বাতিল করা হলো — stock উৎস branch এ ফিরিয়ে দেওয়া হয়েছে।");
    }
}