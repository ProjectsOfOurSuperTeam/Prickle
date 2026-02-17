using Ardalis.SmartEnum;

namespace Prickle.Domain.Decorations;

public sealed class DecorationCategory : SmartEnum<DecorationCategory, int>
{
    public static readonly DecorationCategory NoCategory = new(nameof(NoCategory), 0);
    public static readonly DecorationCategory Stones = new(nameof(Stones), 1); // Каміння
    public static readonly DecorationCategory Sand = new(nameof(Sand), 2); // Пісок
    public static readonly DecorationCategory Wood = new(nameof(Wood), 3); // Дерево
    public static readonly DecorationCategory Figurines = new(nameof(Figurines), 4); // Фігурки
    public static readonly DecorationCategory Nature = new(nameof(Nature), 5); // Природа
    public static readonly DecorationCategory Minerals = new(nameof(Minerals), 6); // Мінерали

    private DecorationCategory(string name, int value)
        : base(name, value)
    {
    }
}