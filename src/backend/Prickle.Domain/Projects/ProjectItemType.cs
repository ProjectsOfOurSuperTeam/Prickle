using System.Runtime.CompilerServices;
using Ardalis.SmartEnum;

namespace Prickle.Domain.Projects;

public sealed class ProjectItemType : SmartEnum<ProjectItemType>
{
    public static readonly ProjectItemType Plant = new(nameof(Plant), 0);
    public static readonly ProjectItemType Decoration = new(nameof(Decoration), 1);
    public static readonly ProjectItemType Soil = new(nameof(Soil), 2);
    private ProjectItemType(string name, int value)
        : base(name, value)
    {
    }
}