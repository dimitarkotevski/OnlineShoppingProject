using System.Collections.Generic;

namespace NexCart.Business.Responses
{
    public class ResponseModel<T>
    {
        public int TotalItems { get; set; }
        public List<T> Items { get; set; }
        public int Size { get; set; }
        public int CurrentPage { get; set; }
    }
    public class ResponseListModel<T>
    {
        public int TotalItems { get; set; }
        public List<T> Items { get; set; }
    }
}
