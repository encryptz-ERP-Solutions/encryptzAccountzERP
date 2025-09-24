using BusinessLogic.Admin.DTOs;
using FluentValidation;

namespace BusinessLogic.Admin.Validators
{
    public class UserDtoValidator : AbstractValidator<UserDto>
    {
        public UserDtoValidator()
        {
            RuleFor(x => x.userId)
                .NotEmpty().WithMessage("UserID is required.")
                .MaximumLength(50).WithMessage("UserID cannot exceed 50 characters.");

            // Add other rules for UserDto properties here as needed.
            // For example:
            // RuleFor(x => x.userName).NotEmpty().MaximumLength(100);
            // RuleFor(x => x.phoneNo).Matches(@"^\d{10}$").WithMessage("Invalid phone number.");
        }
    }
}
