namespace Prickle.Application.Abstractions.Authentication;

public interface IUserContext
{
    Guid UserId { get; }

    string Email { get; }
}
