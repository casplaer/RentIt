﻿using FluentValidation;
using RentIt.Users.Application.Commands.Users.Create;
using System.Text.RegularExpressions;

namespace RentIt.Users.Application.Validators
{
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(u => u)
                .NotNull()
                .WithMessage("Отсутствуют необходимые данные.");

            RuleFor(u => u.FirstName)
                .NotEmpty().WithMessage("Имя обязательно.")
                .Must(name => !Regex.IsMatch(name, @"\d"))
                    .WithMessage("Имя не должно содержать цифр.");

            RuleFor(u => u.LastName)
                .NotEmpty().WithMessage("Фамилия обязательна.")
                .Must(name => !Regex.IsMatch(name, @"\d"))
                    .WithMessage("Фамилия не должна содержать цифр.");

            RuleFor(u => u.Email)
                .NotEmpty().WithMessage("Email обязателен.")
                .EmailAddress().WithMessage("Неверный формат email.");

            RuleFor(u => u.Password)
                .NotEmpty().WithMessage("Пароль обязателен.")
                .MinimumLength(8).WithMessage("Пароль должен содержать минимум 8 символов.")
                .Equal(u => u.ConfirmPassword).WithMessage("Пароли не совпадают.")
                .Matches(@"^[a-zA-Z]+$").WithMessage("Пароль должен содержать только латиницу.") 
                .Matches(@"(?=.*[a-z])").WithMessage("Пароль должен содержать хотя бы одну строчную латинскую букву.")
                .Matches(@"(?=.*[A-Z])").WithMessage("Пароль должен содержать хотя бы одну заглавную латинскую букву.");
        }
    }
}
