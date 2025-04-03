namespace RentIt.Housing.Domain.Contracts.Requests.Reviews
{
    public record UpdateReviewRequest(
        int Rating,
        string Comment
        );
}
