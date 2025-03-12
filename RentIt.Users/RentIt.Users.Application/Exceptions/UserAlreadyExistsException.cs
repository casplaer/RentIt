namespace RentIt.Users.Application.Exceptions
{
    public class UserAlreadyExistsException : Exception
    {
        public UserAlreadyExistsException(string message) 
            : base(message: message) { }
    }
}
