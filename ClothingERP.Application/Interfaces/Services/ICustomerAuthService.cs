namespace ClothingERP.Application.Interfaces.Services;

public interface ICustomerAuthService
{
    Task<ServiceResult<int>> RegisterAsync(CustomerRegisterDto dto);
    Task<ServiceResult<CustomerDto>> LoginAsync(CustomerLoginDto dto);
    Task<IEnumerable<MyOrderListDto>> GetMyOrdersAsync(int customerId);
}