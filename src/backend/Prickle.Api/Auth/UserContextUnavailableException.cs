namespace Prickle.Api.Auth;

public sealed class UserContextUnavailableException : Exception
{
    public UserContextUnavailableException() : base("User context is unavailable")
    {
    }
}
