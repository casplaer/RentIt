﻿namespace RentIt.Users.Application.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string to, string subject, string htmlMessage, CancellationToken cancellationToken);
    }
}
