using IntellaQuest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntellaQuest.BusinessLogic.ViewModels
{
    public class FavouriteProductsViewModel
    {
        public Guid Id { get; set; }
        public LookupViewModel User { get; set; }
        public LookupViewModel Product { get; set; }
    }
}
