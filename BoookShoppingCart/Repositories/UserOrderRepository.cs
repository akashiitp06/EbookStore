using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookShoppingCart.Repositories
{
    public class UserOrderRepository : IUserOrderRepository

    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _httpcontextAccessor;
        private readonly UserManager<IdentityUser> _userManager;

        public UserOrderRepository(ApplicationDbContext db,
            UserManager<IdentityUser> userManager,
            IHttpContextAccessor httpcontextAccessor)
        {
            _db = db;
            _httpcontextAccessor = httpcontextAccessor;
            _userManager = userManager;
        }

        public async Task<IEnumerable<Order>> UserOrders()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                throw new Exception("User is not logged-in");
            var order = await _db.Orders
				                  .Include(x => x.Orderstatus)
								  .Include(x=>x.OrderDetails)
                                  .ThenInclude(x=>x.Book)
                                  .ThenInclude(x=>x.Genre)
                                  .Where(a => a.UserId == userId)
                                  .ToListAsync();
            return order;
        }
        private string GetUserId()
        {
            var principal = _httpcontextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);
            return userId;
        }
    }
}
