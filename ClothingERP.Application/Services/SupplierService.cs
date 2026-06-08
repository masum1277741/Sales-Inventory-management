namespace ClothingERP.Application.Services;

public class SupplierService : ISupplierService
{
    private readonly IUnitOfWork _uow; private readonly IMapper _mapper;
    public SupplierService(IUnitOfWork uow, IMapper mapper) => (_uow, _mapper) = (uow, mapper);

    public async Task<IEnumerable<SupplierListDto>> GetAllAsync()
        => _mapper.Map<IEnumerable<SupplierListDto>>(await _uow.Suppliers.GetQueryable()
           .Where(s => !s.IsDeleted).OrderBy(s => s.CompanyName).ToListAsync());

    public async Task<SupplierDto?> GetByIdAsync(int id)
    {
        var s = await _uow.Suppliers.GetByIdAsync(id);
        return s == null ? null : _mapper.Map<SupplierDto>(s);
    }

    public async Task<ServiceResult<SupplierDto>> CreateAsync(CreateSupplierDto dto, int userId)
    {
        var entity = _mapper.Map<Supplier>(dto); entity.CreatedBy = userId;
        await _uow.Suppliers.AddAsync(entity); await _uow.SaveChangesAsync();
        return ServiceResult<SupplierDto>.Ok(_mapper.Map<SupplierDto>(entity), "Supplier created.");
    }

    public async Task<ServiceResult<SupplierDto>> UpdateAsync(int id, UpdateSupplierDto dto, int userId)
    {
        var entity = await _uow.Suppliers.GetByIdAsync(id);
        if (entity == null) return ServiceResult<SupplierDto>.Fail("Not found.");
        entity.CompanyName = dto.CompanyName; entity.ContactPerson = dto.ContactPerson;
        entity.PhoneNumber = dto.PhoneNumber; entity.Email = dto.Email;
        entity.Address = dto.Address; entity.BankName = dto.BankName;
        entity.BankAccountNumber = dto.BankAccountNumber;
        entity.IsActive = dto.IsActive; entity.UpdatedBy = userId;
        _uow.Suppliers.Update(entity); await _uow.SaveChangesAsync();
        return ServiceResult<SupplierDto>.Ok(_mapper.Map<SupplierDto>(entity), "Updated.");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var entity = await _uow.Suppliers.GetByIdAsync(id);
        if (entity == null) return ServiceResult.Fail("Not found.");
        _uow.Suppliers.Remove(entity); await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Deleted.");
    }

    public async Task<ServiceResult> ToggleStatusAsync(int id, int userId)
    {
        var entity = await _uow.Suppliers.GetByIdAsync(id);
        if (entity == null) return ServiceResult.Fail("Not found.");
        entity.IsActive = !entity.IsActive; entity.UpdatedBy = userId;
        _uow.Suppliers.Update(entity); await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Toggled.");
    }

    public async Task<IEnumerable<SupplierLedgerDto>> GetLedgerAsync(int supplierId, DateTime? from = null, DateTime? to = null)
        => _mapper.Map<IEnumerable<SupplierLedgerDto>>(await _uow.SupplierLedgers.GetBySupplierIdAsync(supplierId, from, to));

    public async Task<decimal> GetBalanceAsync(int supplierId)
        => await _uow.SupplierLedgers.GetCurrentBalanceAsync(supplierId);

    public async Task<ServiceResult> AddPaymentAsync(int supplierId, decimal amount, PaymentMethod method, string? reference, int userId)
    {
        var supplier = await _uow.Suppliers.GetByIdAsync(supplierId);
        if (supplier == null) return ServiceResult.Fail("Not found.");
        var currentBal = await _uow.SupplierLedgers.GetCurrentBalanceAsync(supplierId);
        var newBal = currentBal - amount;

        await _uow.SupplierLedgers.AddAsync(new SupplierLedger
        {
            SupplierId = supplierId,
            EntryType = LedgerEntryType.Payment,
            Debit = amount,
            Credit = 0,
            Balance = newBal,
            ReferenceNumber = reference,
            Description = $"Payment via {method}",
            EntryDate = DateTime.UtcNow,
            CreatedBy = userId
        });

        supplier.CurrentBalance = newBal; supplier.UpdatedBy = userId;
        _uow.Suppliers.Update(supplier);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Payment recorded.");
    }
}