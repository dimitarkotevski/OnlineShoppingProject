using System;

namespace NexCart.Business.ViewModels
{
    public class CategoryLookupViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool Status { get; set; }
        public string Url { get; set; }
    }
}
