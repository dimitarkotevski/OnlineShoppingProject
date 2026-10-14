using System;

namespace IntellaQuest.BusinessLogic.Validation
{
    /// <summary>
    /// Associates a FluentValidation validator with a view model. Replaces the
    /// FluentValidation.Attributes.ValidatorAttribute that was removed in FluentValidation 11.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class FluentValidatorAttribute : Attribute
    {
        public FluentValidatorAttribute(Type validatorType)
        {
            ValidatorType = validatorType;
        }

        public Type ValidatorType { get; }
    }
}
