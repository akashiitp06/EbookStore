﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShoppingCart.Models
{
    [Table("ShoppingCart")]
    public class ShoppingCart
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        public bool IsDeleted { get; set; } = false;
        public ICollection<CartDetail> CartsDetails { get; set; }


    }
}
