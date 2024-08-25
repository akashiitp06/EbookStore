using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using System.Security.Claims;

namespace BookShoppingCart.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpcontextAccessor;

        public CartRepository(ApplicationDbContext db, IHttpContextAccessor httpcontextAccessor,
            UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
            _httpcontextAccessor = httpcontextAccessor;
        }

        public async Task<int> AddItem(int bookId, int qty)
        {
            string userId = GetUserId();
            using var transaction = _db.Database.BeginTransaction();
            try
            {

                
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User is not logged-in");
                var cart = await GetCart(userId);
                if (cart is null)
                {
                    cart = new ShoppingCart
                    {
                        UserId = userId
                    };
                    _db.ShoppingCarts.Add(cart);
                }
                _db.SaveChanges();
                //cart details section
                var cartItem = _db.CartDetails
                                    .FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);
                if (cartItem != null)
                {
                    cartItem.Quantity += qty;
                }
                else
                {
                    var book= _db.Books.Find(bookId);
                    cartItem = new CartDetail
                    {
                        BookId = bookId,
                        ShoppingCartId = cart.Id,
                        Quantity = qty,
                        UnitPrice = book.Price

                    };
                    _db.CartDetails.Add(cartItem);
                }
                _db.SaveChanges();
                transaction.Commit();
                
            }
            catch (Exception ex)
            {
                
            }
            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;

        }

        public async Task<int> RemoveItem(int bookId)
        {
            string userId = GetUserId();
            //using var transaction = _db.Database.BeginTransaction();
            try
            {

                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User is not logged-in");
                var cart = await GetCart(userId);
                if (cart is null)
                {
                    throw new Exception("Invalid cart");
                }
                //cart details section
                var cartItem = _db.CartDetails
                                    .FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);
                if (cartItem is null)
                {
                    throw new Exception("Not items in cart");
                }
                else if (cartItem.Quantity == 1)
                    _db.CartDetails.Remove(cartItem);
                
                else
                    cartItem.Quantity = cartItem.Quantity - 1;
                _db.SaveChanges();
                //transaction.Commit();
                
            }
            catch (Exception ex)
            {
               
            }
            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;
        }

        public async Task<ShoppingCart> GetUserCart()
            {
                var userId = GetUserId();
            if (userId == null)
                throw new Exception("Invalid userid");
                var shoppingCart = await _db.ShoppingCarts
                                                .Include(a=>a.CartsDetails)
                                                .ThenInclude(a=>a.Book)
                                                .ThenInclude(a=>a.Genre)
                                                .Where(a=>a.UserId==userId).FirstOrDefaultAsync();
            return shoppingCart;
                                                
            }

        public async Task<ShoppingCart> GetCart(string userId)
        {
            var cart = await _db.ShoppingCarts.FirstOrDefaultAsync(x => x.UserId == userId);
            return cart;

        }

        public async Task<int> GetCartItemCount(string userId="")
        {
            if(string.IsNullOrEmpty(userId))
            {
                userId = GetUserId() ;
            }
            var data = await (from cart in _db.ShoppingCarts
                             join CartDetail in _db.CartDetails
                             on cart.Id equals CartDetail.ShoppingCartId
                             where cart.UserId == userId
                             select new {CartDetail.id}
                             ).ToListAsync();
            return data.Count;
        }

        public async Task<bool> DoCheckout()
        {
            using  var transaction = _db.Database.BeginTransaction();
            try
            {
                //logic
                //move data from  cartDetail to order amd orderDetails then we will remove cart detail
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User is not logged-in");
                var cart = await GetCart(userId);
                if (cart is null)
                    throw new Exception("Invalid Cart");
                var cartDetail = _db.CartDetails
                                          .Where(a => a.ShoppingCartId == cart.Id).ToList();
                if (cartDetail.Count == 0)
                    throw new Exception("Cart is Empty");
                var order = new Order
                {
                    UserId = userId,
                    CreateDate = DateTime.UtcNow,
                    OrderStatusId = 1 //pending

                };
                _db.Orders.Add(order);
                _db.SaveChanges();
                foreach(var item in cartDetail)
                {
                    var orderDetail = new OrderDetail
                    {
                        BookId = item.BookId,
                        OrderId = order.Id,
                        Quantity = item.Quantity,
                        UnitPrice = (int)item.UnitPrice

                    };
                    _db.OrderDetails.Add(orderDetail);
                }
                _db.SaveChanges();
                //removing the cartdetails
                _db.CartDetails.RemoveRange(cartDetail);
                _db.SaveChanges();
                transaction.Commit();
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }
        private string GetUserId()
        {
            var principal = _httpcontextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);
            return userId;
        }
    }
}
