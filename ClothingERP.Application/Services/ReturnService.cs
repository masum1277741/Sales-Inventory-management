namespace ClothingERP.Application.Services;

public class ReturnService : IReturnService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IStockService _stock;
    private readonly IGiftCardService _giftCardSvc;

    public ReturnService(IUnitOfWork uow, IMapper mapper, IStockService stock, IGiftCardService giftCardService)
        => (_uow, _mapper, _stock, _giftCardSvc) = (uow, mapper, stock, giftCardService);

    public async Task<IEnumerable<SalesReturnListDto>> GetAllSalesReturnsAsync()
    {
        var list = await _uow.SalesReturns.GetQueryable()
            .Include(r => r.SalesInvoice).Include(r => r.Customer)
            .Where(r => !r.IsDeleted).OrderByDescending(r => r.ReturnDate).ToListAsync();
        return _mapper.Map<IEnumerable<SalesReturnListDto>>(list);
    }

    public async Task<SalesReturnDto?> GetSalesReturnByIdAsync(int id)
    {
        var r = await _uow.SalesReturns.GetWithDetailsAsync(id);
        return r == null ? null : _mapper.Map<SalesReturnDto>(r);
    }

    public async Task<ServiceResult<SalesReturnDto>> CreateSalesReturnAsync(CreateSalesReturnDto dto, int userId)
    {
        await _uow.BeginTransactionAsync();
        try
        {
            var inv = await _uow.SalesInvoices.GetByIdAsync(dto.SalesInvoiceId);
            if (inv == null) return ServiceResult<SalesReturnDto>.Fail("Invoice not found.");

            var ret = new SalesReturn
            {
                ReturnNumber = await _uow.SalesReturns.GenerateReturnNumberAsync(),
                SalesInvoiceId = dto.SalesInvoiceId,
                CustomerId = inv.CustomerId,
                ReturnDate = DateTime.UtcNow,
                ReturnType = dto.ReturnType,
                Reason = dto.Reason,
                RefundAmount = dto.RefundAmount,
                RefundMethod = dto.RefundMethod,
                CreatedBy = userId
            };

            decimal total = 0;
            foreach (var itemDto in dto.Items)
            {
                var lineTotal = itemDto.ReturnQuantity * itemDto.UnitPrice;
                ret.Items.Add(new SalesReturnItem
                {
                    ProductVariantId = itemDto.ProductVariantId,
                    ReturnQuantity = itemDto.ReturnQuantity,
                    UnitPrice = itemDto.UnitPrice,
                    TotalAmount = lineTotal,
                    DefectDescription = itemDto.DefectDescription,
                    CreatedBy = userId
                });
                total += lineTotal;
                // Restore stock
                await _stock.UpdateStockAsync(itemDto.ProductVariantId, itemDto.ReturnQuantity,
                    StockMovementType.SalesReturn, ret.ReturnNumber, userId);
            }
            ret.TotalAmount = total;
            await _uow.SalesReturns.AddAsync(ret);
            await _uow.SaveChangesAsync();

            // Store Credit Refund Logic - যখন refund method "StoreCredit" বাছা হবে
            if (dto.RefundMethod == RefundMethod.StoreCredit && ret.CustomerId.HasValue)
            {
                var storeCreditResult = await _giftCardSvc.IssueStoreCreditAsync(new IssueStoreCreditDto
                {
                    CustomerId = ret.CustomerId.Value,
                    Amount = ret.RefundAmount,
                    SourceReturnId = ret.Id,
                    Notes = $"Refund for return {ret.ReturnNumber}"
                }, userId);

                if (!storeCreditResult.Success)
                {
                    await _uow.RollbackTransactionAsync();
                    return ServiceResult<SalesReturnDto>.Fail($"Store credit issuance failed: {storeCreditResult.Message}");
                }
            }

            await _uow.CommitTransactionAsync();

            var result = await _uow.SalesReturns.GetWithDetailsAsync(ret.Id);
            return ServiceResult<SalesReturnDto>.Ok(_mapper.Map<SalesReturnDto>(result!), "Return created.");
        }
        catch (Exception ex) { await _uow.RollbackTransactionAsync(); return ServiceResult<SalesReturnDto>.Fail(ex.Message); }
    }

    public async Task<IEnumerable<PurchaseReturnListDto>> GetAllPurchaseReturnsAsync()
    {
        var list = await _uow.PurchaseReturns.GetQueryable()
            .Include(r => r.PurchaseOrder).Include(r => r.Supplier)
            .Where(r => !r.IsDeleted).OrderByDescending(r => r.ReturnDate).ToListAsync();
        return _mapper.Map<IEnumerable<PurchaseReturnListDto>>(list);
    }

    public async Task<PurchaseReturnDto?> GetPurchaseReturnByIdAsync(int id)
    {
        var r = await _uow.PurchaseReturns.GetWithDetailsAsync(id);
        return r == null ? null : _mapper.Map<PurchaseReturnDto>(r);
    }

    public async Task<ServiceResult<PurchaseReturnDto>> CreatePurchaseReturnAsync(CreatePurchaseReturnDto dto, int userId)
    {
        await _uow.BeginTransactionAsync();
        try
        {
            var ret = new PurchaseReturn
            {
                ReturnNumber = await _uow.PurchaseReturns.GenerateReturnNumberAsync(),
                PurchaseOrderId = dto.PurchaseOrderId,
                SupplierId = dto.SupplierId,
                ReturnDate = DateTime.UtcNow,
                Reason = dto.Reason,
                CreatedBy = userId
            };
            decimal total = 0;
            foreach (var itemDto in dto.Items)
            {
                var lineTotal = itemDto.ReturnQuantity * itemDto.UnitCost;
                ret.Items.Add(new PurchaseReturnItem
                {
                    ProductVariantId = itemDto.ProductVariantId,
                    ReturnQuantity = itemDto.ReturnQuantity,
                    UnitCost = itemDto.UnitCost,
                    TotalCost = lineTotal,
                    DefectDescription = itemDto.DefectDescription,
                    CreatedBy = userId
                });
                total += lineTotal;
                await _stock.UpdateStockAsync(itemDto.ProductVariantId, -itemDto.ReturnQuantity,
                    StockMovementType.PurchaseReturn, ret.ReturnNumber, userId);
            }
            ret.TotalAmount = total;
            await _uow.PurchaseReturns.AddAsync(ret);
            await _uow.SaveChangesAsync();
            await _uow.CommitTransactionAsync();

            var result = await _uow.PurchaseReturns.GetWithDetailsAsync(ret.Id);
            return ServiceResult<PurchaseReturnDto>.Ok(_mapper.Map<PurchaseReturnDto>(result!), "Return created.");
        }
        catch (Exception ex) { await _uow.RollbackTransactionAsync(); return ServiceResult<PurchaseReturnDto>.Fail(ex.Message); }
    }
}