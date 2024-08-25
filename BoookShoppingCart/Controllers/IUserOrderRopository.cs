
using Microsoft.AspNetCore.Mvc;

namespace BookShoppingCart.Controllers
{
    public interface IUserOrderRopository
    {
        Task<IActionResult> UserOrders();
    }
}