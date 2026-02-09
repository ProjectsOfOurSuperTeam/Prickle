using Ardalis.SmartEnum;

namespace Prickle.Domain.Projects;

public sealed class ProjectItemSize : SmartEnum<ProjectItemSize, int>
{
    public static readonly ProjectItemSize Small = new(nameof(Small), 0);//1x1
    public static readonly ProjectItemSize Medium = new(nameof(Medium), 1);//2x2
    public static readonly ProjectItemSize Large = new(nameof(Large), 2);//3x3
    public static readonly ProjectItemSize ExtraLarge = new(nameof(ExtraLarge), 3);//4x4

    private ProjectItemSize(string name, int value)
        : base(name, value)
    {
    }
}
