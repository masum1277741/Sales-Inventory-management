namespace ClothingERP.Application.Services;

public class BranchService : IBranchService
{
    private readonly IUnitOfWork _uow;

    public BranchService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<BranchDto>> GetAllAsync()
    {
        var branches = await _uow.Branches.GetQueryable()
            .Where(b => !b.IsDeleted)
            .OrderByDescending(b => b.IsMainBranch)
            .ThenBy(b => b.Name)
            .ToListAsync();

        var today = DateTime.UtcNow.Date;
        var result = new List<BranchDto>();

        foreach (var b in branches)
        {
            var staffCount = await _uow.UserBranches.GetQueryable()
                .CountAsync(ub => ub.BranchId == b.Id && !ub.IsDeleted);

            var todaySales = await _uow.SalesInvoices.GetQueryable()
                .Where(i => i.BranchId == b.Id && i.InvoiceDate.Date == today &&
                            i.Status != InvoiceStatus.Cancelled && !i.IsHold)
                .SumAsync(i => (decimal?)i.TotalAmount) ?? 0;

            result.Add(new BranchDto
            {
                Id = b.Id,
                Code = b.Code,
                Name = b.Name,
                Address = b.Address,
                PhoneNumber = b.PhoneNumber,
                Country = b.Country,
                IsMainBranch = b.IsMainBranch,
                IsActive = b.IsActive,
                StaffCount = staffCount,
                TodaySales = todaySales
            });
        }
        return result;
    }

    public async Task<BranchDto?> GetByIdAsync(int id)
    {
        var b = await _uow.Branches.GetByIdAsync(id);
        if (b == null) return null;
        return new BranchDto { Id = b.Id, Code = b.Code, Name = b.Name, Address = b.Address, PhoneNumber = b.PhoneNumber, Country = b.Country, IsMainBranch = b.IsMainBranch, IsActive = b.IsActive };
    }

    public async Task<ServiceResult<BranchDto>> CreateAsync(CreateBranchDto dto, int userId)
    {
        var codeExists = await _uow.Branches.GetQueryable().AnyAsync(b => b.Code == dto.Code && !b.IsDeleted);
        if (codeExists) return ServiceResult<BranchDto>.Fail("এই Branch Code ইতিমধ্যে ব্যবহার হয়েছে।");

        var branch = new Branch
        {
            Code = dto.Code.ToUpper(),
            Name = dto.Name,
            Address = dto.Address,
            PhoneNumber = dto.PhoneNumber,
            Country = dto.Country,
            IsActive = dto.IsActive,
            CreatedBy = userId
        };
        await _uow.Branches.AddAsync(branch);
        await _uow.SaveChangesAsync();

        return ServiceResult<BranchDto>.Ok(
            new BranchDto { Id = branch.Id, Code = branch.Code, Name = branch.Name, IsActive = branch.IsActive },
            $"শাখা '{branch.Name}' তৈরি হয়েছে।");
    }

    public async Task<ServiceResult> UpdateAsync(int id, CreateBranchDto dto, int userId)
    {
        var branch = await _uow.Branches.GetByIdAsync(id);
        if (branch == null) return ServiceResult.Fail("Branch not found.");

        branch.Name = dto.Name; branch.Address = dto.Address; branch.PhoneNumber = dto.PhoneNumber;
        branch.Country = dto.Country; branch.IsActive = dto.IsActive;
        branch.UpdatedBy = userId; branch.UpdatedAt = DateTime.UtcNow;
        _uow.Branches.Update(branch);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Branch updated successfully.");
    }

    public async Task<ServiceResult> ToggleStatusAsync(int id, int userId)
    {
        var branch = await _uow.Branches.GetByIdAsync(id);
        if (branch == null) return ServiceResult.Fail("Branch not found.");
        if (branch.IsMainBranch) return ServiceResult.Fail("Main branch বন্ধ করা যাবে না।");

        branch.IsActive = !branch.IsActive;
        branch.UpdatedBy = userId; branch.UpdatedAt = DateTime.UtcNow;
        _uow.Branches.Update(branch);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok($"Branch {(branch.IsActive ? "activated" : "deactivated")}.");
    }

    public async Task<MyBranchAccessDto> GetUserAccessAsync(int userId, string roleName)
    {
        if (roleName.Equals("Administrator", StringComparison.OrdinalIgnoreCase))
        {
            var all = (await GetAllAsync()).Where(b => b.IsActive).ToList();
            var defaultBranch = all.FirstOrDefault(b => b.IsMainBranch)?.Id ?? all.FirstOrDefault()?.Id ?? 1;
            return new MyBranchAccessDto { AccessibleBranches = all, DefaultBranchId = defaultBranch, CanAccessAllBranches = true };
        }

        var assignments = await _uow.UserBranches.GetQueryable()
            .Include(ub => ub.Branch)
            .Where(ub => ub.UserId == userId && !ub.IsDeleted && ub.Branch.IsActive)
            .ToListAsync();

        var branches = assignments.Select(a => new BranchDto
        {
            Id = a.Branch.Id,
            Code = a.Branch.Code,
            Name = a.Branch.Name,
            IsActive = a.Branch.IsActive,
            IsMainBranch = a.Branch.IsMainBranch
        }).ToList();

        var defaultId = assignments.FirstOrDefault(a => a.IsDefault)?.BranchId ?? assignments.FirstOrDefault()?.BranchId ?? 1;

        return new MyBranchAccessDto { AccessibleBranches = branches, DefaultBranchId = defaultId, CanAccessAllBranches = false };
    }

    public async Task<int> GetUserDefaultBranchIdAsync(int userId)
    {
        var assignment = await _uow.UserBranches.GetQueryable()
            .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.IsDefault && !ub.IsDeleted);
        if (assignment != null) return assignment.BranchId;

        var any = await _uow.UserBranches.GetQueryable().FirstOrDefaultAsync(ub => ub.UserId == userId && !ub.IsDeleted);
        if (any != null) return any.BranchId;

        var mainBranch = await _uow.Branches.GetQueryable().FirstOrDefaultAsync(b => b.IsMainBranch);
        return mainBranch?.Id ?? 1;
    }

    public async Task<ServiceResult> AssignUserToBranchesAsync(UserBranchAssignmentDto dto, int userId)
    {
        var existing = await _uow.UserBranches.GetQueryable()
            .Where(ub => ub.UserId == dto.UserId && !ub.IsDeleted).ToListAsync();
        foreach (var e in existing) _uow.UserBranches.Remove(e);
        await _uow.SaveChangesAsync();

        foreach (var branchId in dto.BranchIds)
        {
            await _uow.UserBranches.AddAsync(new UserBranch
            {
                UserId = dto.UserId,
                BranchId = branchId,
                IsDefault = branchId == (dto.DefaultBranchId ?? dto.BranchIds.First()),
                CreatedBy = userId
            });
        }
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok("শাখা assignment সফলভাবে আপডেট হয়েছে।");
    }


    public async Task<List<BranchStockComparisonDto>> CompareStockAcrossBranchesAsync(string? keyword = null)
    {
        var variants = await _uow.ProductVariants.GetAllWithDetailsAsync();
        if (!string.IsNullOrEmpty(keyword))
            variants = variants.Where(v => v.Product.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();

        var result = new List<BranchStockComparisonDto>();
        foreach (var v in variants.Take(100))   // performance safeguard
        {
            var stocks = await _uow.Stocks.GetAllVariantStockAcrossBranchesAsync(v.Id);
            if (!stocks.Any()) continue;

            result.Add(new BranchStockComparisonDto
            {
                ProductVariantId = v.Id,
                ProductName = v.Product.Name,
                SizeName = v.Size.Name,
                ColorName = v.Color.Name,
                StockByBranch = stocks.Select(s => new BranchStockDto
                {
                    BranchId = s.BranchId,
                    BranchName = s.Branch.Name,
                    Quantity = (int)s.Quantity
                }).ToList(),
                TotalAcrossBranches = (int)stocks.Sum(s => s.Quantity)
            });
        }
        return result;
    }
}