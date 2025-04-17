using RentIt.Bookings.Application.Interfaces.Services;
using RentIt.Bookings.Application.Services.Grpc;
using RentIt.Bookings.Application.Specifications.Bookings;
using RentIt.Bookings.Core.Enums;
using RentIt.Bookings.Core.Interfaces.Repositories;
using Serilog;

namespace RentIt.Bookings.Application.Services
{
    public class BookingStatusService : IBookingStatusService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly UserIntegrationService _userIntegrationService;
        private readonly ILogger _logger;

        public BookingStatusService(
            IUnitOfWork unitOfWork, 
            IEmailSender emailSender,
            UserIntegrationService userIntegrationService,
            ILogger logger)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _userIntegrationService = userIntegrationService;
            _logger = logger;
        }

        public async Task UpdateActiveBookingsAsync(CancellationToken cancellationToken)
        {
            var specification = new SearchBookingSpecification(status: BookingStatus.Active);

            var bookingsToUpdate = await _unitOfWork.Bookings.GetAllFilteredBookingsAsync(specification, cancellationToken);

            foreach (var booking in bookingsToUpdate)
            {
                if (booking.EndDate <= DateTime.UtcNow)
                {
                    booking.Status = BookingStatus.Completed;

                    var userInfo = await _userIntegrationService.GetUserInfoAsync(booking.UserId);

                    var emailSubject = "Спасибо, что выбрали нас!";
                    var emailBody = $"Здравствуйте, {userInfo.FirstName} {userInfo.LastName},\n\n" +
                                    $"Спасибо, что воспользовались нашим сервисом для бронирования.\n\n" +
                                    "Мы рады, что могли помочь вам с вашим поиском и надеемся, что в будущем вы снова выберете нас.\n\n" +
                                    "С наилучшими пожеланиями,\n" +
                                    "Ваша команда RentIt.";

                    await _emailSender.SendEmailAsync(userInfo.Email, emailSubject, emailBody, cancellationToken);

                    _unitOfWork.Bookings.Update(booking);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateConfirmedBookingsAsync(CancellationToken cancellationToken)
        {
            var specification = new SearchBookingSpecification(status: BookingStatus.Confirmed);

            var bookingsToUpdate = await _unitOfWork.Bookings.GetAllFilteredBookingsAsync(specification, cancellationToken);

            foreach (var booking in bookingsToUpdate)
            {
                if ((booking.StartDate - DateTime.UtcNow).TotalHours <= 12)
                {
                    booking.Status = BookingStatus.Cancelled;

                    var userInfo = await _userIntegrationService.GetUserInfoAsync(booking.UserId);

                    var emailSubject = "Ваше бронирование отменено";
                    var emailBody = $"Здравствуйте, {userInfo.FirstName} {userInfo.LastName},\n\n" +
                                    $"К сожалению, ваше бронирование с {booking.StartDate.Date} по {booking.EndDate.Date} было отменено, так как оно не было оплачено за 12 часов до начала.\n\n" +
                                    "Пожалуйста, свяжитесь с нами, если у вас возникли вопросы.";

                    await _emailSender.SendEmailAsync(userInfo.Email, emailSubject, emailBody, cancellationToken);

                    _unitOfWork.Bookings.Update(booking);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdatePaidBookingsAsync(CancellationToken cancellationToken)
        {
            var specification = new SearchBookingSpecification(status: BookingStatus.Paid);

            var bookingsToUpdate = await _unitOfWork.Bookings.GetAllFilteredBookingsAsync(specification, cancellationToken);

            foreach (var booking in bookingsToUpdate)
            {
                if (booking.StartDate.Date == DateTime.UtcNow.Date)
                {
                    booking.Status = BookingStatus.Active;

                    _unitOfWork.Bookings.Update(booking);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdatePendingBookingsAsync(CancellationToken cancellationToken)
        {
            var specification = new SearchBookingSpecification(status: BookingStatus.Pending);

            var bookingsToUpdate = await _unitOfWork.Bookings.GetAllFilteredBookingsAsync(specification, cancellationToken);

            foreach (var booking in bookingsToUpdate)
            {
                var isCreatedMoreThan48HoursAgo = (DateTime.UtcNow - booking.CreatedAt).TotalHours > 48;

                var isLessThan24HoursToStart = (booking.StartDate - DateTime.UtcNow).TotalHours < 24;

                if (isCreatedMoreThan48HoursAgo || isLessThan24HoursToStart)
                {
                    booking.Status = BookingStatus.Cancelled;

                    _unitOfWork.Bookings.Update(booking);

                    var userInfo = await _userIntegrationService.GetUserInfoAsync(booking.UserId);

                    string subject;
                    string body;

                    if (isCreatedMoreThan48HoursAgo)
                    {
                        subject = "Ваше бронирование отменено";
                        body = $"Уважаемый {userInfo.FirstName} {userInfo.LastName},\n\n" +
                               "Мы вынуждены отменить ваше бронирование, так как оно было создано более 48 часов назад и так и не было подтверждено собственником объявления.\n\n" +
                               "Мы приносим извинения и надеемся, что в будущем такого больше не повторится.\n\n" +
                               "С уважением,\nКоманда RentIt.";
                    }
                    else
                    {
                        subject = "Ваше бронирование отменено";
                        body = $"Уважаемый {userInfo.FirstName} {userInfo.LastName},\n\n" +
                               "Мы вынуждены отменить ваше бронирование, так как до его начала осталось менее 24 часов, но оно не было подтверждено собственником объявления.\n\n" +
                               "Мы приносим извинения и надеемся, что в будущем такого больше не повторится.\n\n" +
                               "С уважением,\nКоманда RentIt.";
                    }

                    await _emailSender.SendEmailAsync(userInfo.Email, subject, body, cancellationToken);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
