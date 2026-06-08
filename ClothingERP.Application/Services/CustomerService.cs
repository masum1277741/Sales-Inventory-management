namespace ClothingERP.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _uow; private readonly IMapper _mapper;
    public CustomerService(IUnitOfWork uow, IMapper mapper) => (_uow, _mapper) = (uow, mapper);

    public async Task<IEnumerable<CustomerListDto>> GetAllAsync()
    {
        var list = await _uow.Customers.GetQueryable()
            .Include(c => c.CustomerGroup).Where(c => !c.IsDeleted).OrderBy(c => c.Name).ToListAsync();
        return _mapper.Map<IEnumerable<CustomerListDto>>(list);
    }

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        var c = await _uow.Customers.GetQueryable().Include(x => x.CustomerGroup)
                          .FirstOrDefaultAsync(x => x.Id == id);
        return c == null ? null : _mapper.Map<CustomerDto>(c);
    }

    public async Task<CustomerDto?> GetByPhoneAsync(string phone)
    {
        var c = await _uow.Customers.GetByPhoneAsync(phone);
        return c == null ? null : _mapper.Map<CustomerDto>(c);
    }

    public async Task<ServiceResult<CustomerDto>> CreateAsync(CreateCustomerDto dto, int userId)
    {
        if (!string.IsNullOrEmpty(dto.PhoneNumber) && await _uow.Customers.IsPhoneExistsAsync(dto.PhoneNumber))
            return ServiceResult<CustomerDto>.Fail("Phone number already registered.");
        var entity = _mapper.Map<Customer>(dto); entity.CreatedBy = userId;
        await _uow.Customers.AddAsync(entity); await _uow.SaveChangesAsync();
        return ServiceResult<CustomerDto>.Ok(_mapper.Map<CustomerDto>(entity), "Customer created.");
    }

    public async Task<ServiceResult<CustomerDto>> UpdateAsync(int id, UpdateCustomerDto dto, int userId)
    {
        var entity = await _uow.Customers.GetByIdAsync(id);
        if (entity == null) return ServiceResult<CustomerDto>.Fail("Not found.");
        if (!string.IsNullOrEmpty(dto.PhoneNumber) && await _uow.Customers.IsPhoneExistsAsync(dto.PhoneNumber, id))
            return ServiceResult<CustomerDto>.Fail("Phone already used.");
        entity.Name = dto.Name; entity.PhoneNumber = dto.PhoneNumber; entity.Email = dto.Email;
        entity.Address = dto.Address; entity.NIDNumber = dto.NIDNumber;
        entity.CustomerGroupId = dto.CustomerGroupId; entity.IsActive = dto.IsActive;
        entity.UpdatedBy = userId;
        if (dto.ProfileImagePath != null) entity.ProfileImage = dto.ProfileImagePath;
        _uow.Customers.Update(entity); await _uow.SaveChangesAsync();
        return ServiceResult<CustomerDto>.Ok(_mapper.Map<CustomerDto>(entity), "Updated.");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var entity = await _uow.Customers.GetByIdAsync(id);
        if (entity == null) return ServiceResult.Fail("Not found.");
        _uow.Customers.Remove(entity); await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Deleted.");
    }

    public async Task<ServiceResult> ToggleStatusAsync(int id, int userId)
    {
        var entity = await _uow.Customers.GetByIdAsync(id);
        if (entity == null) return ServiceResult.Fail("Not found.");
        entity.IsActive = !entity.IsActive; entity.UpdatedBy = userId;
        _uow.Customers.Update(entity); await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Status toggled.");
    }

    public async Task<IEnumerable<CustomerLedgerDto>> GetLedgerAsync(int customerId, DateTime? from = null, DateTime? to = null)
        => _mapper.Map<IEnumerable<CustomerLedgerDto>>(await _uow.CustomerLedgers.GetByCustomerIdAsync(customerId, from, to));

    public async Task<decimal> GetBalanceAsync(int customerId)
        => await _uow.CustomerLedgers.GetCurrentBalanceAsync(customerId);

    public async Task<ServiceResult> AddPaymentAsync(int customerId, decimal amount, string description, string? reference, int userId)
    {
        var customer = await _uow.Customers.GetByIdAsync(customerId);
        if (customer == null) return ServiceResult.Fail("Customer not found.");

        var currentBal = await _uow.CustomerLedgers.GetCurrentBalanceAsync(customerId);
        var newBal = currentBal - amount;

        await _uow.CustomerLedgers.AddAsync(new CustomerLedger
        {
            CustomerId = customerId,
            EntryType = LedgerEntryType.Payment,
            Debit = 0,
            Credit = amount,
            Balance = newBal,
            ReferenceNumber = reference,
            Description = description,
            EntryDate = DateTime.UtcNow,
            CreatedBy = userId
        });

        customer.CurrentBalance = newBal; customer.UpdatedBy = userId;
        _uow.Customers.Update(customer);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Payment recorded.");
    }

    public async Task<IEnumerable<CustomerGroupDto>> GetGroupsAsync()
    {
        var list = await _uow.CustomerGroups.GetQueryable()
            .Include("Customers").Where(g => !g.IsDeleted).ToListAsync();
        return _mapper.Map<IEnumerable<CustomerGroupDto>>(list);
    }

    public async Task<ServiceResult<CustomerGroupDto>> CreateGroupAsync(CreateCustomerGroupDto dto, int userId)
    {
        var entity = new CustomerGroup { Name = dto.Name, DiscountPercentage = dto.DiscountPercentage, IsActive = true, CreatedBy = userId };
        await _uow.CustomerGroups.AddAsync(entity); await _uow.SaveChangesAsync();
        return ServiceResult<CustomerGroupDto>.Ok(_mapper.Map<CustomerGroupDto>(entity), "Group created.");
    }
}