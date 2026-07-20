namespace ClothingERP.Application.Interfaces.Services;
public interface ICurrentBranchProvider
{
    int GetCurrentBranchId();
    void SetCurrentBranchId(int branchId);
}