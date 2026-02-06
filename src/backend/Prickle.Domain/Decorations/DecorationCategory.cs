using Ardalis.SmartEnum;

namespace Prickle.Domain.Decorations;

public sealed class DecorationCategory : SmartEnum<DecorationCategory, int>
{
    public static readonly DecorationCategory NoCategory = new(nameof(NoCategory), 0);
    public static readonly DecorationCategory Stones = new(nameof(Stones), 1);
    public static readonly DecorationCategory Sand = new(nameof(Sand), 2);
    public static readonly DecorationCategory Figurines = new(nameof(Figurines), 3);

    private DecorationCategory(string name, int value)
        : base(name, value)
    {
    }
}