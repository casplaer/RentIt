namespace RentIt.Housing.Domain.Contracts.Requests.Reviews
{
    public record CreateReviewRequest(
        int Rating,
        string Comment
        );
}
