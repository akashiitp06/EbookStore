using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShoppingCart.Models
{
    [Table("OrderStatus")]
    public class OrderStatus
    {
        public int Id { get; set; }
        [Required]
        public int StatusId { get; set; }
        [Required]
        [MaxLength(20)]
        public string? StatusName { get; set; }

        public static implicit operator OrderStatus(int v)
        {
            throw new NotImplementedException();
        }
    }
}
