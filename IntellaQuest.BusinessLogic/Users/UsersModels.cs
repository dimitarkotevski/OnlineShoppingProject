using FluentValidation;
using IntellaQuest.BusinessLogic;
using IntellaQuest.BusinessLogic.Validation;
using IntellaQuest.BusinessLogic.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IntellaQuest.BusinessLogic.ViewModels
{
    public class UserRegistrationViewModel
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    [FluentValidator(typeof(UserLoginViewModelValidator))]
    public class UserLoginViewModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class ChangePasswordUserViewModel
    {
        public Guid Id { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class UserGridModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Address { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Role { get; set; }
    }
    //[Validator(typeof(UserViewModelValidator))]
    public class UserViewModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Role { get; set; }
        public IList<ProductViewModel> FavouriteProducts { get; set; }
        public IList<LookupViewModel> Orders { get; set; }
        public PaymentsViewModel Payment { get; set; }
    }

    public class UserDetailsModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public LookupViewModel Role { get; set; }
        public PaymentsProfileViewModel Payment { get; set; }
        public List<LookupViewModel> FavouriteProdutcs { get; set; }
    }

    public class UserTokenInformation
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
        public string Role { get; set; }
    }
    public class UserViewModelValidator : AbstractValidator<UserViewModel>
    {
        public UserViewModelValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().WithMessage("First Name is required.");
            RuleFor(x => x.LastName).NotEmpty().WithMessage("Last Name is required.");
            RuleFor(x => x.Email).EmailAddress().WithMessage("Invalid email address.");
            RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required.");
            //RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
        }
    }
    public class UserLoginViewModelValidator : AbstractValidator<UserLoginViewModel>
    {
        public UserLoginViewModelValidator()
        {
            RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required.");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
        }
    }
}
