using NexCart.Business.Exceptions;
using NexCart.Business.Exceptions.ExceptionMassages;
using NexCart.Business.Requests;
using NexCart.Business.Responses;
using NexCart.Business.Mappers;
using NexCart.Business.ViewModels;
using NexCart.Data.NHibernate.ConfigurationRepository;
using NexCart.Data.NHibernate.Repositories;
using NexCart.Domain;
using NexCart.Domain.Enums;
using Microsoft.IdentityModel.Tokens;
using NexCart.Data.NHibernate.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using NexCart.Business.Security;
using System.Runtime.InteropServices;
using FluentNHibernate.Conventions;
using NexCart.Business.ViewModels;
using NexCart.Business.Mappers;
using NHibernate.AdoNet;

namespace NexCart.Business.Services
{
    public interface IUserService
    {
        void Registration(UserRegistrationViewModel registrationModel);
        UserTokenInformation Login(UserLoginViewModel loginModel);
        void UpdatePassword(ChangePasswordUserViewModel model);
        ResponseModel<UserGridModel> GetAll(RequestModel request);
        UserDetailsModel Get(Guid Id);
        bool Create(UserViewModel model);
        void Update(UserViewModel model);
        void Delete(Guid Id);
        double GetAmountMoneyOfUser(Guid userId);
        void AddPayment(Guid userId, PaymentsViewModelInfo model);
        void DeletePayment(Guid userId);
    }
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IProductRepository _productRepository;
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly IShoppingCartDetailRepository _shoppingCartDetailRepository;
        private readonly JwtSettings _jwtSettings;

        public UserService(
                    IUnitOfWork unitOfWork,
                    IUserRepository userRepository, 
                    IOrderRepository orderRepository, 
                    IPaymentRepository paymentRepository,
                    IProductRepository productRepository,
                    IShoppingCartRepository shoppingCartRepository,
                    IShoppingCartDetailRepository shoppingCartDetailRepository,
                    JwtSettings jwtSettings
            )
        {
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _orderRepository = orderRepository;
            _paymentRepository = paymentRepository;
            _productRepository = productRepository;
            _shoppingCartRepository = shoppingCartRepository;
            _shoppingCartDetailRepository = shoppingCartDetailRepository;
            _jwtSettings = jwtSettings;
        }
        
        #region User Authentification

        public void Registration(UserRegistrationViewModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Email))
            {
                throw new BllException("Username and email are required.");
            }

            if (!PasswordHasher.MeetsPolicy(model.Password))
            {
                throw new BllException($"Password must be at least {PasswordHasher.MinimumLength} characters long.");
            }

            if (_userRepository.CheckExist(x=>x.Username == model.Username))
                throw new BllException(string.Format(ShopExceptionMassages.UserExceptionMassages.USERNAME_ALREADY_EXIST, model.Username));

            if (_userRepository.CheckExist(x => x.Email == model.Email))
                throw new BllException(string.Format(ShopExceptionMassages.UserExceptionMassages.EMAIL_ALREADY_EXIST, model.Email));
            User userEntity;
            using (_unitOfWork.BeginTransaction())
            {
                userEntity = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Username = model.Username,
                    Role = UserRole.User,
                    Password = PasswordHasher.Hash(model.Password),
                };
                _userRepository.Add(userEntity);
                _unitOfWork.Commit();
            }
            MakeShoppingCardForTheUser(userEntity);
        }
        public void MakeShoppingCardForTheUser(User model)
        {
            using (_unitOfWork.BeginTransaction())
            {
                ShoppingCart shoppingCart = new ShoppingCart
                {
                    User = model,
                    Active = true,
                    ShoppingCartDetails = new List<ShoppingCartDetail>()
                };
                _shoppingCartRepository.Add(shoppingCart);
                _unitOfWork.Commit();
            }
        }
        public UserTokenInformation Login(UserLoginViewModel loginModel)
        {
            if (string.IsNullOrWhiteSpace(loginModel.Username) || string.IsNullOrWhiteSpace(loginModel.Password))
            {
                throw new BllException(ShopExceptionMassages.UserExceptionMassages.USERNAME_PASSWORD_INCORRECT);
            }

            using (_unitOfWork.BeginTransaction())
            {
                var user = _userRepository
                    .FilterBy(x => x.Username == loginModel.Username)
                    .FirstOrDefault();

                // The same error is returned whether the user exists or not, so the endpoint
                // cannot be used to enumerate accounts.
                if (user == null || !PasswordHasher.Verify(loginModel.Password, user.Password))
                {
                    throw new BllException(ShopExceptionMassages.UserExceptionMassages.USERNAME_PASSWORD_INCORRECT);
                }

                // Transparently upgrade legacy (reversible-AES) hashes to salted PBKDF2.
                if (PasswordHasher.NeedsRehash(user.Password))
                {
                    user.Password = PasswordHasher.Hash(loginModel.Password);
                    _userRepository.Update(user);
                }

                _unitOfWork.Commit();

                return new UserTokenInformation
                {
                    Id = user.Id,
                    Username = user.Username,
                    Role = user.Role.ToString(),
                    Token = CreateToken(user)
                };
            }
        }
        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                // The real user id (not a random value) so endpoints can bind resources to the caller.
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new JwtSecurityToken(
                                        _jwtSettings.Issuer, 
                                        _jwtSettings.Audience, 
                                        claims,
                                        expires: DateTime.UtcNow.AddDays(_jwtSettings.ExpiryDays <= 0 ? 1 : _jwtSettings.ExpiryDays), 
                                        signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
        public void UpdatePassword(ChangePasswordUserViewModel model)
        {
            if (!PasswordHasher.MeetsPolicy(model.NewPassword))
            {
                throw new BllException($"Password must be at least {PasswordHasher.MinimumLength} characters long.");
            }

            using (_unitOfWork.BeginTransaction())
            {
                var user = _userRepository.FindBy(model.Id)
                    ?? throw new BllException(ShopExceptionMassages.UserExceptionMassages.NOT_FOUND_EXCEPTION);

                if (!PasswordHasher.Verify(model.OldPassword, user.Password))
                {
                    throw new BllException(ShopExceptionMassages.UserExceptionMassages.PASSWORD_IS_INCORRECT);
                }
                if (PasswordHasher.Verify(model.NewPassword, user.Password))
                {
                    throw new BllException(ShopExceptionMassages.UserExceptionMassages.SAME_OLD_AND_NEW_PASWORD);
                }

                user.Password = PasswordHasher.Hash(model.NewPassword);
                _userRepository.Update(user);
                _unitOfWork.Commit();
            }
        }

        #endregion

        public double GetAmountMoneyOfUser(Guid userId)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var user = _userRepository.FindBy(userId)
                    ?? throw new BllException(ShopExceptionMassages.UserExceptionMassages.NOT_FOUND_EXCEPTION);

                return user.Amount;
            }
        }

        #region Payment
        public void DeletePayment(Guid userId)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var user = _userRepository.FindBy(userId);
                if (user == null)
                {
                    throw new BllException(ShopExceptionMassages.UserExceptionMassages.NOT_FOUND_EXCEPTION);
                }

                var paymentId = user.Payment.Id;
                user.Payment = null;

                var payment = _paymentRepository.FindBy(paymentId);
                if (payment == null)
                {
                    throw new BllException(ShopExceptionMassages.PaymentExceptionMassages.NOT_FOUND);
                }

                _paymentRepository.Delete(payment);

                _userRepository.Update(user);
                _unitOfWork.Commit();
            }
        }

        public void AddPayment(Guid userId, PaymentsViewModelInfo model)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var user = _userRepository.FindBy(userId);
                if(user == null)
                {
                    throw new BllException(ShopExceptionMassages.UserExceptionMassages.NOT_FOUND_EXCEPTION);
                }

                if(user.Payment == null)
                {
                    var payment = new Payment
                    {
                        CardHolder = model.CardHolder,
                        // Only the last four digits are retained; the full card number and the
                        // security code (CVV) must never be stored.
                        CardNumber = LastFourDigits(model.CardNumber),
                        ExpirationDate = model.ExpirationDate,
                    };

                    user.Payment = payment;

                    _paymentRepository.Add(payment);
                    _userRepository.Update(user);

                    _unitOfWork.Commit();
                }
            }
        }

        /// <summary>Returns the last four digits of a card number (digits only).</summary>
        private static string LastFourDigits(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
            {
                return null;
            }

            var digits = new string(cardNumber.Where(char.IsDigit).ToArray());
            return digits.Length <= 4 ? digits : digits.Substring(digits.Length - 4);
        }

        #endregion

        #region ADMIN 

        public ResponseModel<UserGridModel> GetAll(RequestModel request)
        {
            return FilterAndPage(request);
        }
        private ResponseModel<UserGridModel> FilterAndPage(RequestModel request)
        {
            using (_unitOfWork.BeginTransaction())
            {
                ResponseModel<UserGridModel> response = new ResponseModel<UserGridModel>();
                IQueryable<User> listUsersForFiltering = _userRepository.All();

                if (!string.IsNullOrEmpty(request.SearchString))
                {
                    listUsersForFiltering = listUsersForFiltering
                        .Where(x => (
                            x.FirstName.Contains(request.SearchString) ||
                            x.LastName.Contains(request.SearchString)) ||
                            (x.LastName + " " + x.FirstName).Equals(request.SearchString) 
                            || x.Email.Contains(request.SearchString) 
                            || x.Username.Contains(request.SearchString));
                }
                if (!string.IsNullOrEmpty(request.EmailEnding))
                {
                    listUsersForFiltering = listUsersForFiltering.Where(x => x.FirstName.EndsWith(request.EmailEnding));
                }


                switch (request.SortName)
                {
                    case "FirstName":
                        if (string.IsNullOrEmpty(request.isAscending))
                        {
                            break;
                        }
                        else if (request.isAscending.Equals("asc"))
                        {
                            listUsersForFiltering = listUsersForFiltering.OrderBy(x => x.FirstName);
                        }
                        else
                        {
                            listUsersForFiltering = listUsersForFiltering.OrderByDescending(x => x.FirstName);
                        }
                        break;
                    case "LastName":
                        if (string.IsNullOrEmpty(request.isAscending))
                        {
                            break;
                        }
                        else if (request.isAscending.Equals("asc"))
                        {
                            listUsersForFiltering = listUsersForFiltering.OrderBy(x => x.LastName);
                        }
                        else
                        {
                            listUsersForFiltering = listUsersForFiltering.OrderByDescending(x => x.LastName);
                        }
                        break;
                    case "Email":
                        if (string.IsNullOrEmpty(request.isAscending))
                        {
                            break;
                        }
                        else if (request.isAscending.Equals("asc"))
                        {
                            listUsersForFiltering = listUsersForFiltering.OrderBy(x => x.Email);
                        }
                        else
                        {
                            listUsersForFiltering = listUsersForFiltering.OrderByDescending(x => x.Email);
                        }
                        break;
                    case "Username":
                        if (string.IsNullOrEmpty(request.isAscending))
                        {
                            break;
                        }
                        else if (request.isAscending.Equals("asc"))
                        {
                            listUsersForFiltering = listUsersForFiltering.OrderBy(x => x.Username);
                        }
                        else
                        {
                            listUsersForFiltering = listUsersForFiltering.OrderByDescending(x => x.Username);
                        }
                        break;
                    case "NumberOfOrders":
                        if (string.IsNullOrEmpty(request.isAscending))
                        {
                            break;
                        }
                        else if (request.isAscending.Equals("asc"))
                        {
                            listUsersForFiltering = listUsersForFiltering.OrderBy(x => x.Orders.Count());
                        }
                        else
                        {
                            listUsersForFiltering = listUsersForFiltering.OrderByDescending(x => x.Orders.Count());
                        }
                        break;
                    default:
                        break;
                }
                if (request.Size == 0 && request.PageNeeded == 0)
                {
                    request.PageNeeded = 1;
                    request.Size = listUsersForFiltering.Count();
                }

                response.Size = request.Size;
                response.CurrentPage = request.PageNeeded;
                response.Items = listUsersForFiltering
                                    .Skip((response.CurrentPage - 1) * response.Size)
                                    .Take(response.Size).Select(x => x.MapToGridModel()).ToList();
                response.TotalItems = listUsersForFiltering.Count();

                _unitOfWork.Commit();
                return response;
            }
        }
        public bool Create(UserViewModel model)
        {
            if (!PasswordHasher.MeetsPolicy(model.Password))
            {
                throw new BllException($"Password must be at least {PasswordHasher.MinimumLength} characters long.");
            }

            if (_userRepository.CheckExist(x => x.Email == model.Email))
                throw new BllException(string.Format(ShopExceptionMassages.UserExceptionMassages.EMAIL_ALREADY_EXIST, model.Email));
            
            if (_userRepository.CheckExist(x => x.Username == model.Username))
                throw new BllException(string.Format(ShopExceptionMassages.UserExceptionMassages.USERNAME_ALREADY_EXIST, model.Username));

            // Unknown or missing values fall back to the least-privileged role.
            UserRole role = ParseRole(model.Role);

            using (_unitOfWork.BeginTransaction())
            {
                var userEntity = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Username = model.Username,
                    Password = PasswordHasher.Hash(model.Password),
                    State = model.State,
                    ZipCode = model.ZipCode,
                    Address = model.Address,
                    Role = role,
                    //FavouriteProducts = new List<FavouriteProducts>(),
                    Orders = null,
                    Payment = null
                };
                _userRepository.Add(userEntity);
                _unitOfWork.Commit();
                return true;
            }
        }

        /// <summary>
        /// Maps a role name coming from the client to the <see cref="UserRole"/> enum. Unknown
        /// or missing values fall back to the least-privileged role rather than failing.
        /// </summary>
        private static UserRole ParseRole(string role)
        {
            return Enum.TryParse(role, ignoreCase: true, out UserRole parsed) ? parsed : UserRole.User;
        }

        public void Delete(Guid Id)
        {
            using (_unitOfWork.BeginTransaction())
            {
                _userRepository.Delete(Id);
                _unitOfWork.Commit();
            }
        }
        public UserDetailsModel Get(Guid userId)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var user = _userRepository.FindBy(userId);
                if (user == null)
                    throw new BllException(ShopExceptionMassages.UserExceptionMassages.NOT_FOUND_EXCEPTION);

                var mappedUser = user.MapToUserDetailModel();

                return mappedUser;
            }
        }
        public void Update(UserViewModel model)
        {
            using (_unitOfWork.BeginTransaction())
            {
                var user = _userRepository.FindBy(model.Id);
                if (user == null)
                {
                    throw new BllException(ShopExceptionMassages.UserExceptionMassages.NOT_FOUND_EXCEPTION);
                };

                user.LastName = model.LastName;
                user.FirstName = model.FirstName;
                user.Username = model.Username;
                user.Email = model.Email;

                user.Address = model.Address;
                user.City = model.City;
                user.State = model.State;
                user.ZipCode = model.ZipCode;

                if(model.Password != null)
                {
                    if (!PasswordHasher.MeetsPolicy(model.Password))
                    {
                        throw new BllException($"Password must be at least {PasswordHasher.MinimumLength} characters long.");
                    }

                    user.Password = PasswordHasher.Hash(model.Password);
                }

                if(model.Role != null)
                {
                    // Unknown values fall back to the least-privileged role.
                    user.Role = ParseRole(model.Role);
                }

                _userRepository.Update(user);
                _unitOfWork.Commit();
            }
        }

        //
        //public void SetUserDetails(UserDetailsModel model)
        //{
        //    using (_unitOfWork.BeginTransaction())
        //    {
        //        var user = 
        //    }
        //}

        //public void UpdatePassword(Guid userId,string oldPassword, string newPassword)
        //{
        //    using (_unitOfWork.BeginTransaction())
        //    {
        //        var user = _userRepository.FindBy(userId);
        //        if (user == null)
        //        {
        //            throw new BllException(ShopExceptionMassages.UserExceptionMassages.NOT_FOUND_EXCEPTION);
        //        };


        //        var oldP = PasswordEncryption.Encryption(oldPassword);
        //        var newP = PasswordEncryption.Encryption(newPassword);

        //        if(oldP != user.Password)
        //        {
        //            throw new BllException(ShopExceptionMassages.UserExceptionMassages.PASSWORD_IS_INCORRECT);
        //        }

        //        if (oldP == newP)
        //        {
        //            throw new BllException(ShopExceptionMassages.UserExceptionMassages.SAME_OLD_AND_NEW_PASWORD);
        //        }

        //        user.Password = newP;

        //        _userRepository.Update(user);
        //        _unitOfWork.Commit();
        //    }
        //}

        #endregion

    }
}
