using ClothingERP.Application.DTOs;
using ClothingERP.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClothingERP.Web.Controllers;

[Authorize]  
public class BranchSwitchController : BaseController
{
    private readonly ICurrentBranchProvider _currentBranchProvider;

    public BranchSwitchController(ICurrentBranchProvider currentBranchProvider)
        => _currentBranchProvider = currentBranchProvider;

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult SwitchBranch(int branchId)
    {
        _currentBranchProvider.SetCurrentBranchId(branchId);
        return JsonSuccess(message: "Branch পরিবর্তন করা হয়েছে।");
    }
}