using NexCart.Business;
using NexCart.Business.Mappers;
using NexCart.Business.ViewModels;
using NexCart.Domain;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NexCart.Business.Mappers
{
    public static class UsersMappers
    {
        public static UserViewModel MapToViewModel(this User user)
        {
            return new UserViewModel
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Address = user.Address,
                Payment = user.Payment.MapToViewModel()
            };
        }
        public static UserGridModel MapToGridModel(this User user)
        {
            return new UserGridModel
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Address = user.Address,
                City = user.City,
                ZipCode = user.ZipCode,
                Role = user.Role.ToString(),
                State = user.State
            };
        }
        public static UserDetailsModel MapToUserDetailModel(this User user)
        {
            return new UserDetailsModel
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Address = user.Address,
                Description = user.Description,
                State = user.State,
                City = user.City,
                ZipCode = user.ZipCode,
                Payment = user.Payment != null ? new NexCart.Business.ViewModels.PaymentsProfileViewModel
                {
                    Id = user.Payment.Id,
                    CardHolder = user.Payment.CardHolder,
                    CardNumber = MaskCardNumber(user.Payment.CardNumber),
                    ExpirationDate = user.Payment.ExpirationDate.Month + "/" + user.Payment.ExpirationDate.Year % 2000,

                } : null,
                FavouriteProdutcs = user.FavouriteProducts.Select(x=> new LookupViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Status = true
                }).ToList()
            };
        }
        public static LookupViewModel MapToLookupViewModel(this User user)
        {
            return new LookupViewModel
            {
                Id = user.Id,
                Name = user.FirstName + " " + user.LastName,
                Status = true
            };
        }
        public static LookupViewModel MapToLookupUsernameViewModel(this User user)
        {
            return new LookupViewModel
            {
                Id = user.Id,
                Name = user.Username,
                Status = true
            };
        }

        /// <summary>
        /// Masks a card number, exposing at most the last four digits. Handles both legacy
        /// records that hold a full number and new records that only store the last four.
        /// </summary>
        private static string MaskCardNumber(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
            {
                return string.Empty;
            }

            var digits = new string(cardNumber.Where(char.IsDigit).ToArray());
            var last4 = digits.Length <= 4 ? digits : digits.Substring(digits.Length - 4);
            return "**** **** **** " + last4;
        }
    }
}
