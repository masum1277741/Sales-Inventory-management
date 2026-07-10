namespace ClothingERP.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _uow; private readonly IMapper _mapper;
    private readonly INotificationService _notificationSvc;
    public CustomerService(IUnitOfWork uow, IMapper mapper, INotificationService notificationService)
     => (_uow, _mapper, _notificationSvc) = (uow, mapper, notificationService);

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
    // ── Get Ledger (Invoice + Payment combined timeline) ────────────────────
    public async Task<IEnumerable<CustomerLedgerDto>> GetLedgerAsync(
        int customerId, DateTime from, DateTime to)
    {
        // ── Debit entries: Sales Invoices ────────────────────────────────────
        var invoices = await _uow.SalesInvoices.GetQueryable()
            .Where(i => !i.IsDeleted &&
                        i.CustomerId == customerId &&
                        i.Status != InvoiceStatus.Cancelled &&
                        !i.IsHold &&
                        i.InvoiceDate >= from && i.InvoiceDate <= to)
            .ToListAsync();

        // ── Credit entries: Standalone Customer Payments ─────────────────────
        var payments = await _uow.CustomerPayments.GetQueryable()
            .Where(p => !p.IsDeleted &&
                        p.CustomerId == customerId &&
                        p.PaymentDate >= from && p.PaymentDate <= to)
            .ToListAsync();

        // ── Credit entries: Sales-Invoice Payments (POS এ paid amount) ────────
        var invoicePayments = await _uow.SalesPayments.GetQueryable()
            .Where(p => !p.IsDeleted &&
                        p.SalesInvoice.CustomerId == customerId &&
                        p.PaymentDate >= from && p.PaymentDate <= to)
            .Include(p => p.SalesInvoice)
            .ToListAsync();

        var entries = new List<CustomerLedgerDto>();

        entries.AddRange(invoices.Select(i => new CustomerLedgerDto
        {
            Id = i.Id,
            EntryDate = i.InvoiceDate,
            EntryType = "Invoice",
            Description = $"Sale — {i.InvoiceNumber}",
            ReferenceNumber = i.InvoiceNumber,
            Debit = i.TotalAmount,
            Credit = 0
        }));

        entries.AddRange(payments.Select(p => new CustomerLedgerDto
        {
            Id = p.Id,
            EntryDate = p.PaymentDate,
            EntryType = "Payment",
            Description = p.Description,
            ReferenceNumber = p.ReferenceNumber,
            Debit = 0,
            Credit = p.Amount
        }));

        entries.AddRange(invoicePayments.Select(p => new CustomerLedgerDto
        {
            Id = p.Id,
            EntryDate = p.PaymentDate,
            EntryType = "Payment",
            Description = $"Invoice Payment — {p.SalesInvoice.InvoiceNumber}",
            ReferenceNumber = p.ReferenceNumber,
            Debit = 0,
            Credit = p.Amount
        }));

        var sorted = entries.OrderBy(e => e.EntryDate).ToList();
        decimal running = 0;
        foreach (var e in sorted)
        {
            running += e.Debit - e.Credit;
            e.Balance = running;
        }

        return sorted;
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

    public async Task<ServiceResult> AddPaymentAsync(
     int customerId, decimal amount, string description,
     string? reference, int userId)
    {
        var customer = await _uow.Customers.GetByIdAsync(customerId);
        if (customer == null) return ServiceResult.Fail("Customer not found.");


        var payment = new CustomerPayment
        {
            CustomerId = customerId,
            Amount = amount,
            Description = description,
            ReferenceNumber = reference,
            PaymentDate = DateTime.UtcNow,
            CreatedBy = userId
        };
        await _uow.CustomerPayments.AddAsync(payment);


        customer.CurrentBalance = Math.Max(0, customer.CurrentBalance - amount);
        customer.UpdatedBy = userId;
        customer.UpdatedAt = DateTime.UtcNow;
        _uow.Customers.Update(customer);

        await _uow.SaveChangesAsync();

        // ── Payment Received Notification ─────────────────────────────────
        await _notificationSvc.CreateAsync(new CreateNotificationDto
        {
            UserId = null,
            Title = "Payment Received",
            Message = $"{customer.Name} থেকে ${amount:N2} পেমেন্ট পাওয়া গেছে।",
            Type = "Payment",
            Severity = "success",
            Icon = "bi-cash-coin",
            ActionUrl = $"/Customer/Ledger/{customerId}"
        });

        return ServiceResult.Ok("Payment recorded successfully.");
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