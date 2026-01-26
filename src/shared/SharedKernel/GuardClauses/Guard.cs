namespace SharedKernel.GuardClauses;

/// <summary>
/// An entry point to a set of Guard Clauses defined as extension methods on IGuardClause.
/// </summary>
public class Guard : IGuardClause
{
    /// <summary>
    /// An entry point to a set of Guard Clauses.
    /// </summary>
    public static IGuardClause Against { get; } = new Guard();

    private Guard() { }
}
