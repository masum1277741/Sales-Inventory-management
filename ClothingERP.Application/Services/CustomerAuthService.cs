namespace ClothingERP.Application.Services;

public class CustomerAuthService : ICustomerAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CustomerAuthService(IUnitOfWork uow, IMapper mapper) => (_uow, _mapper) = (uow, mapper);

    public async Task<ServiceResult<int>> RegisterAsync(CustomerRegisterDto dto)
    {
        var existsByEmail = await _uow.Customers.GetQueryable()
            .AnyAsync(c => c.Email == dto.Email && !c.IsDeleted);
        if (existsByEmail) return ServiceResult<int>.Fail("এই ইমেইল দিয়ে ইতিমধ্যে account আছে।");

        var existingByPhone = await _uow.Customers.GetQueryable()
            .FirstOrDefaultAsync(c => c.PhoneNumber == dto.Phone && !c.IsDeleted);

        Customer customer;
        if (existingByPhone != null)
        {
            if (!string.IsNullOrEmpty(existingByPhone.PasswordHash))
                return ServiceResult<int>.Fail("এই ফোন নম্বর দিয়ে ইতিমধ্যে account আছে — Login করুন।");

            existingByPhone.Email = dto.Email;
            existingByPhone.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            existingByPhone.UpdatedAt = DateTime.UtcNow;
            _uow.Customers.Update(existingByPhone);
            customer = existingByPhone;
        }
        else
        {
            customer = new Customer
            {
                Name = dto.Name,
                PhoneNumber = dto.Phone,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IsActive = true
            };
            await _uow.Customers.AddAsync(customer);
        }

        await _uow.SaveChangesAsync();
        return ServiceResult<int>.Ok(customer.Id, "Account সফলভাবে তৈরি হয়েছে!");
    }

    public async Task<ServiceResult<CustomerDto>> LoginAsync(CustomerLoginDto dto)
    {
        var customer = await _uow.Customers.GetQueryable()
            .FirstOrDefaultAsync(c => (c.Email == dto.EmailOrPhone || c.PhoneNumber == dto.EmailOrPhone) && !c.IsDeleted);

        if (customer == null || string.IsNullOrEmpty(customer.PasswordHash))
            return ServiceResult<CustomerDto>.Fail("ইমেইল/ফোন অথবা পাসওয়ার্ড সঠিক নয়।");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, customer.PasswordHash))
            return ServiceResult<CustomerDto>.Fail("ইমেইল/ফোন অথবা পাসওয়ার্ড সঠিক নয়।");

        if (!customer.IsActive)
            return ServiceResult<CustomerDto>.Fail("আপনার account নিষ্ক্রিয় করা আছে।");

        return ServiceResult<CustomerDto>.Ok(_mapper.Map<CustomerDto>(customer), "লগইন সফল!");
    }

    public async Task<IEnumerable<MyOrderListDto>> GetMyOrdersAsync(int customerId)
    {
        var orders = await _uow.OnlineOrders.GetQueryable()
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId && !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(o => new MyOrderListDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            OrderDate = o.CreatedAt,
            TotalUSD = o.TotalUSD,
            Status = o.Status,
            ItemCount = o.Items.Count
        });
    }
}