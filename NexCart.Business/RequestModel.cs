using NHibernate.Mapping;
using System;
using System.Collections.Generic;

namespace NexCart.Business.Requests
{
    public class RequestModel
    {
        public string SearchString { get; set; }
        public string SortName { get; set; }
        public string isAscending { get; set; }
        public int PageNeeded { get; set; }
        public int Size { get; set; }
        public Guid? EntityId { get; set; }
        public List<string> SearchBrands { get; set; }
        public List<string> SearchList { get; set; }
        public int? PriceFrom { get; set; }
        public int? PriceTo { get; set; }
        public string SearchStatus { get; set; }
        public string EmailEnding { get; set; }
    }
}
