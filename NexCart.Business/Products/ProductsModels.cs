using System;

namespace NexCart.Business.ViewModels
{
    public class ProductViewModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string ImageUrl { get; set; }
        public string Image { get; set; }
        public DateTime DateCreated { get; set; }
        public CategoryLookupViewModel Category { get; set; }
    }
}
