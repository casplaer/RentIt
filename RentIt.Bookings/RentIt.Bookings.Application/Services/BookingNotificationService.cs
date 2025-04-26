using RentIt.Bookings.Application.Interfaces.Services;
using RentIt.Bookings.Application.Interfaces.Services.Grpc;
using RentIt.Bookings.Contracts.Dto;
using RentIt.Bookings.Contracts.Requests.Bookings;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Application.Interfaces.Services;

namespace RentIt.Bookings.Application.Services
{
    public class BookingNotificationService : IBookingNotificationService
    {
        private readonly IUserIntegrationService _userIntegrationService;
        private readonly IHousingIntegrationService _housingIntegrationService;
        private readonly IEmailSender _emailSender;
        private readonly IAppLogger _logger;

        public BookingNotificationService(
            IUserIntegrationService userIntegrationService,
            IHousingIntegrationService housingIntegrationService,
            IEmailSender emailSender,
            IAppLogger logger)
        {
            _userIntegrationService = userIntegrationService;
            _housingIntegrationService = housingIntegrationService;
            _emailSender = emailSender;
            _logger = logger;
        }

        private string GetEmailTemplate(string content)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; color: #333; line-height: 1.6; max-width: 600px; margin: 0 auto; padding: 20px; }}
                        h2 {{ color: #2c3e50; }}
                        p {{ margin: 10px 0; }}
                        .button {{ 
                            display: inline-block; 
                            padding: 10px 20px; 
                            background-color: #3498db; 
                            color: white !important; 
                            text-decoration: none; 
                            border-radius: 5px; 
                            font-weight: bold; 
                        }}
                        .button:hover {{ background-color: #2980b9; }}
                        .footer {{ margin-top: 20px; font-size: 12px; color: #777; }}
                    </style>
                </head>
                <body>
                    {content}
                    <p class='footer'>С уважением,<br>Команда RentIt</p>
                </body>
                </html>";
        }

        public async Task NotifyOwnerAboutNewBookingAsync(HousingInfoDto housingResponse, CreateBookingRequest request, CancellationToken cancellationToken)
        {
            var ownerInfo = await _userIntegrationService.GetUserInfoAsync(housingResponse.OwnerId);
            if (ownerInfo == null)
            {
                _logger.LogWarning("Не удалось получить информацию о владельце с ID {OwnerId}", housingResponse.OwnerId);
                return;
            }

            var baseUrl = "https://localhost:3000";
            var bookingsUrl = $"{baseUrl}/my-housings/bookings";

            var content = $@"<h2>Новая заявка на ваше объявление!</h2>
                <p>Здравствуйте, {ownerInfo.FirstName} {ownerInfo.LastName},</p>
                <p>На ваше объявление <strong>{housingResponse.HousingName}</strong> создана новая заявка на период с <strong>{request.StartDate:dd.MM.yyyy}</strong> по <strong>{request.EndDate:dd.MM.yyyy}</strong>.</p>
                <p>Пожалуйста, просмотрите заявку.</p>
                <p><a href='{bookingsUrl}' class='button'>Посмотреть заявки</a></p>";

            var body = GetEmailTemplate(content);
            await _emailSender.SendEmailAsync(ownerInfo.Email, "У вас новая заявка!", body, cancellationToken);

            _logger.LogInformation("Письмо отправлено владельцу жилья на почту: {OwnerEmail}", ownerInfo.Email);
        }

        public async Task NotifyUserAboutBookingCreationAsync(HousingInfoDto housingResponse, CreateBookingRequest request, Guid userGuid, CancellationToken cancellationToken)
        {
            var userInfo = await _userIntegrationService.GetUserInfoAsync(userGuid);
            if (userInfo == null)
            {
                _logger.LogWarning("Не удалось получить информацию о пользователе с ID {UserId}", userGuid);
                return;
            }

            var content = $@"<h2>Заявка успешно создана!</h2>
                <p>Здравствуйте, {userInfo.FirstName} {userInfo.LastName},</p>
                <p>Поздравляем! Вы создали заявку на <strong>{housingResponse.HousingName}</strong> с <strong>{request.StartDate:dd.MM.yyyy}</strong> по <strong>{request.EndDate:dd.MM.yyyy}</strong>.</p>
                <p>Свяжитесь с владельцем объявления для подтверждения и организации вашего отдыха.</p>
                <p>Если у вас есть вопросы, наша поддержка всегда готова помочь: <a href='mailto:support@rentit.com'>support@rentit.com</a>.</p>";

            var body = GetEmailTemplate(content);
            await _emailSender.SendEmailAsync(userInfo.Email, "Заявка успешно создана!", body, cancellationToken);

            _logger.LogInformation("Письмо отправлено пользователю на почту: {UserEmail}", userInfo.Email);
        }

        public async Task NotifyUserAboutBookingCancellationAsync(Booking bookingToCancel, CancellationToken cancellationToken)
        {
            var userInfo = await _userIntegrationService.GetUserInfoAsync(bookingToCancel.UserId);
            if (userInfo == null)
            {
                _logger.LogWarning("Не удалось получить информацию о пользователе с ID {UserId}", bookingToCancel.UserId);
                return;
            }

            var housingInfo = await _housingIntegrationService.GetHousingInfoAsync(bookingToCancel.HousingId);
            if (housingInfo == null)
            {
                _logger.LogWarning("Не удалось получить информацию о жилье с ID {HousingId}", bookingToCancel.HousingId);
                return;
            }

            var content = $@"<h2>Бронирование отменено</h2>
                <p>Здравствуйте, {userInfo.FirstName} {userInfo.LastName},</p>
                <p>Ваше бронирование <strong>{housingInfo.HousingName}</strong> с <strong>{bookingToCancel.StartDate:dd.MM.yyyy}</strong> по <strong>{bookingToCancel.EndDate:dd.MM.yyyy}</strong> было успешно отменено.</p>
                <p>Если у вас есть вопросы, свяжитесь с нами: <a href='mailto:support@rentit.com'>support@rentit.com</a>.</p>
                <p><a href='https://localhost:3000' class='button'>Найти новое жилье</a></p>";

            var body = GetEmailTemplate(content);
            await _emailSender.SendEmailAsync(userInfo.Email, "Отмена бронирования", body, cancellationToken);

            _logger.LogInformation("Уведомление об отмене бронирования отправлено пользователю {UserEmail}", userInfo.Email);
        }

        public async Task NotifyUserAboutBookingRejectionAsync(Booking bookingToReject, HousingInfoDto housingInfo, CancellationToken cancellationToken)
        {
            var userInfo = await _userIntegrationService.GetUserInfoAsync(bookingToReject.UserId);
            if (userInfo == null)
            {
                _logger.LogWarning("Не удалось получить информацию о пользователе с ID {UserId} для отправки email", bookingToReject.UserId);
                return;
            }

            var content = $@"<h2>Бронирование отклонено</h2>
                <p>Здравствуйте, {userInfo.FirstName} {userInfo.LastName},</p>
                <p>Ваша заявка на бронирование <strong>{housingInfo.HousingName}</strong> с <strong>{bookingToReject.StartDate:dd.MM.yyyy}</strong> по <strong>{bookingToReject.EndDate:dd.MM.yyyy}</strong> была отклонена владельцем.</p>
                <p>Попробуйте найти другое жилье или свяжитесь с нами для помощи: <a href='mailto:support@rentit.com'>support@rentit.com</a>.</p>
                <p><a href='https://localhost:3000' class='button'>Найти новое жилье</a></p>";

            var body = GetEmailTemplate(content);
            await _emailSender.SendEmailAsync(userInfo.Email, "Бронирование отклонено", body, cancellationToken);

            _logger.LogInformation("Уведомление об отклонении бронирования отправлено пользователю {UserEmail}", userInfo.Email);
        }

        public async Task NotifyUserAboutBookingConfirmationAsync(Booking booking, Payment payment, CancellationToken cancellationToken)
        {
            var userInfo = await _userIntegrationService.GetUserInfoAsync(booking.UserId);
            if (userInfo == null)
            {
                _logger.LogWarning("Не удалось получить информацию о пользователе с ID {UserId}", booking.UserId);
                return;
            }

            var housingInfo = await _housingIntegrationService.GetHousingInfoAsync(booking.HousingId);
            if (housingInfo == null)
            {
                _logger.LogWarning("Не удалось получить информацию о жилье с ID {HousingId}", booking.HousingId);
                return;
            }

            var baseUrl = "https://localhost:3000";
            var paymentUrl = $"{baseUrl}/my-bookings/{booking.BookingId}/checkout";

            var content = $@"<h2>Бронирование подтверждено!</h2>
                <p>Здравствуйте, {userInfo.FirstName} {userInfo.LastName},</p>
                <p>Ваше бронирование <strong>{housingInfo.HousingName}</strong> с <strong>{booking.StartDate:dd.MM.yyyy}</strong> по <strong>{booking.EndDate:dd.MM.yyyy}</strong> было подтверждено владельцем.</p>
                <p>Общая стоимость бронирования: <strong>{payment.Amount:C}</strong>.</p>
                <p><a href='{paymentUrl}' class='button'>Оплатить бронирование</a></p>
                <p>Если у вас есть вопросы, свяжитесь с нами: <a href='mailto:support@rentit.com'>support@rentit.com</a>.</p>";

            var body = GetEmailTemplate(content);
            await _emailSender.SendEmailAsync(userInfo.Email, "Ваше бронирование подтверждено!", body, cancellationToken);

            _logger.LogInformation("Уведомление о подтверждении бронирования отправлено пользователю {UserEmail}", userInfo.Email);
        }

        public async Task NotifyUserAboutPaymentSuccessAsync(Booking booking, Payment payment, CancellationToken cancellationToken)
        {
            var userInfo = await _userIntegrationService.GetUserInfoAsync(booking.UserId);
            if (userInfo == null)
            {
                _logger.LogWarning("Не удалось получить информацию о пользователе с ID {UserId}", booking.UserId);
                return;
            }

            var content = $@"<h2>Оплата успешно завершена!</h2>
                <p>Здравствуйте, {userInfo.FirstName} {userInfo.LastName},</p>
                <p>Ваш платеж на сумму <strong>{payment.Amount:C}</strong> успешно завершен.</p>
                <p>Желаем вам приятного отдыха!</p>
                <p>Если у вас есть вопросы, свяжитесь с нами: <a href='mailto:support@rentit.com'>support@rentit.com</a>.</p>";

            var body = GetEmailTemplate(content);
            await _emailSender.SendEmailAsync(userInfo.Email, "Спасибо за оплату!", body, cancellationToken);

            _logger.LogInformation("Письмо об успешной оплате отправлено пользователю на почту: {UserEmail}", userInfo.Email);
        }

        public async Task NotifyUserAboutRefundAsync(Booking booking, Payment payment, bool isFined, decimal finePercent, decimal refundAmount, CancellationToken cancellationToken)
        {
            var userInfo = await _userIntegrationService.GetUserInfoAsync(booking.UserId);
            if (userInfo == null)
            {
                _logger.LogWarning("Не удалось получить информацию о пользователе с ID {UserId}", booking.UserId);
                return;
            }

            var housingInfo = await _housingIntegrationService.GetHousingInfoAsync(booking.HousingId);
            if (housingInfo == null)
            {
                _logger.LogWarning("Не удалось получить информацию о жилье с ID {HousingId}", booking.HousingId);
                return;
            }

            var refundDetails = isFined
                ? $"Удержан штраф <strong>{finePercent}%</strong> за позднюю отмену. Итоговая сумма возврата: <strong>{refundAmount:C}</strong>."
                : "Вы получили полный возврат средств.";

            var content = $@"<h2>Возврат средств</h2>
                <p>Здравствуйте, {userInfo.FirstName} {userInfo.LastName},</p>
                <p>Ваш платеж за бронирование <strong>{housingInfo.HousingName}</strong> на сумму <strong>{payment.Amount:C}</strong> был возвращен.</p>
                <p>{refundDetails}</p>
                <p>Если у вас есть вопросы, свяжитесь с нами: <a href='mailto:support@rentit.com'>support@rentit.com</a>.</p>
                <p><a href='https://localhost:3000' class='button'>Найти новое жилье</a></p>";

            var body = GetEmailTemplate(content);
            await _emailSender.SendEmailAsync(userInfo.Email, "Возврат средств по вашему платежу", body, cancellationToken);

            _logger.LogInformation("Уведомление о возврате средств отправлено пользователю {UserEmail}", userInfo.Email);
        }

        public async Task NotifyUserAboutBookingCompletionAsync(Booking booking, CancellationToken cancellationToken)
        {
            var userInfo = await _userIntegrationService.GetUserInfoAsync(booking.UserId);
            if (userInfo == null)
            {
                _logger.LogWarning("Не удалось получить информацию о пользователе с ID {UserId}", booking.UserId);
                return;
            }

            var housingInfo = await _housingIntegrationService.GetHousingInfoAsync(booking.HousingId);

            if (housingInfo == null)
            {
                _logger.LogWarning("Не удалось получить информацию о собственности с ID {HousingId}", booking.HousingId);
                return;
            }

            var content = $@"<h2>Спасибо, что выбрали нас!</h2>
                <p>Здравствуйте, {userInfo.FirstName} {userInfo.LastName},</p>
                <p>Мы рады, что помогли вам с бронированием <strong>{housingInfo.HousingName}</strong>.</p>
                <p>Надеемся, ваш отдых прошел замечательно, и вы снова выберете нас для будущих поездок!</p>
                <p><a href='https://localhost:3000' class='button'>Найти новое жилье</a></p>";

            var body = GetEmailTemplate(content);
            await _emailSender.SendEmailAsync(userInfo.Email, "Спасибо, что выбрали нас!", body, cancellationToken);

            _logger.LogInformation("Письмо о завершении бронирования отправлено пользователю на почту: {UserEmail}", userInfo.Email);
        }

        public async Task NotifyUserAboutBookingCancellationDueToNonPaymentAsync(Booking booking, CancellationToken cancellationToken)
        {
            var userInfo = await _userIntegrationService.GetUserInfoAsync(booking.UserId);
            if (userInfo == null)
            {
                _logger.LogWarning("Не удалось получить информацию о пользователе с ID {UserId}", booking.UserId);

                return;
            }

            var housingInfo = await _housingIntegrationService.GetHousingInfoAsync(booking.HousingId);

            if (housingInfo == null)
            {
                _logger.LogWarning("Не удалось получить информацию о собственности с ID {HousingId}", booking.HousingId);
                return;
            }

            var content = $@"<h2>Бронирование отменено</h2>
                <p>Здравствуйте, {userInfo.FirstName} {userInfo.LastName},</p>
                <p>К сожалению, ваше бронирование <strong>{housingInfo.HousingName}</strong> с <strong>{booking.StartDate:dd.MM.yyyy}</strong> по <strong>{booking.EndDate:dd.MM.yyyy}</strong> было отменено, так как оплата не была произведена за 12 часов до начала.</p>
                <p>Свяжитесь с нами, если у вас есть вопросы: <a href='mailto:support@rentit.com'>support@rentit.com</a>.</p>
                <p><a href='https://localhost:3000' class='button'>Найти другое жилье</a></p>";

            var body = GetEmailTemplate(content);
            await _emailSender.SendEmailAsync(userInfo.Email, "Ваше бронирование отменено", body, cancellationToken);

            _logger.LogInformation("Письмо об отмене бронирования (не оплачено) отправлено пользователю на почту: {UserEmail}", userInfo.Email);
        }

        public async Task NotifyUserAboutBookingCancellationDueToNonConfirmationAsync(Booking booking, bool isCreatedMoreThan48HoursAgo, CancellationToken cancellationToken)
        {
            var userInfo = await _userIntegrationService.GetUserInfoAsync(booking.UserId);
            if (userInfo == null)
            {
                _logger.LogWarning("Не удалось получить информацию о пользователе с ID {UserId}", booking.UserId);
                return;
            }

            var housingInfo = await _housingIntegrationService.GetHousingInfoAsync(booking.HousingId);

            if (housingInfo == null)
            {
                _logger.LogWarning("Не удалось получить информацию о собственности с ID {HousingId}", booking.HousingId);
                return;
            }

            string reason = isCreatedMoreThan48HoursAgo
                ? "оно было создано более 48 часов назад и не было подтверждено владельцем."
                : "до его начала осталось менее 24 часов, и оно не было подтверждено владельцем.";

            var content = $@"<h2>Бронирование отменено</h2>
                <p>Здравствуйте, {userInfo.FirstName} {userInfo.LastName},</p>
                <p>К сожалению, ваше бронирование <strong>{housingInfo.HousingName}</strong> с <strong>{booking.StartDate:dd.MM.yyyy}</strong> по <strong>{booking.EndDate:dd.MM.yyyy}</strong> было отменено, так как {reason}</p>
                <p>Мы приносим извинения за неудобства. Попробуйте найти другое жилье или свяжитесь с нашей поддержкой: <a href='mailto:support@rentit.com'>support@rentit.com</a>.</p>
                <p><a href='https://localhost:3000' class='button'>Найти новое жилье</a></p>";

            var body = GetEmailTemplate(content);
            await _emailSender.SendEmailAsync(userInfo.Email, "Ваше бронирование отменено", body, cancellationToken);

            _logger.LogInformation("Письмо об отмене бронирования (не подтверждено) отправлено пользователю на почту: {UserEmail}", userInfo.Email);
        }
    }
}