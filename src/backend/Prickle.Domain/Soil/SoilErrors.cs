namespace Prickle.Domain.Soil;

public static class SoilErrors
{
    public static class SoilType
    {
        public static readonly Error SoilTypeNameEmpty = Error.Problem(
            "SoilType.NameEmpty",
            "Soil type name cannot be empty."
        );
        public static Error SoilTypeAlreadyExists(string name) => Error.Problem(
            "SoilType.AlreadyExists",
            $"Soil type with name '{name}' already exists."
        );
        public static Error FailedToCreate(string name) => Error.Problem(
            "SoilType.FailedToCreate",
            $"Failed to create soil type with name '{name}'."
        );
        public static Error SoilTypeNotFound(int id) => Error.Problem(
            "SoilType.NotFound",
            $"Soil type with ID '{id}' was not found."
        );
    }
    public static class SoilFormulas
    {
        public static readonly Error SoilFormulasNameEmpty = Error.Problem(
            "SoilFormulas.NameEmpty",
            "Soil formulas name cannot be empty."
        );
        public static readonly Error SoilFormulasFormulaEmpty = Error.Problem(
            "SoilFormulas.FormulaEmpty",
            "Soil formulas must contain at least one soil type."
        );
        public static readonly Error SoilFormulasPercentageInvalid = Error.Problem(
            "SoilFormulas.PercentageInvalid",
            "Soil formulas percentages must be between 1 and 100 and sum to 100."
        );

        public static readonly Error InvalidSoilTypeId = Error.Problem(
            "SoilFormulas.InvalidSoilTypeId",
            "Soil type ID must be greater than zero."
        );

        public static readonly Error InvalidPercentage = Error.Problem(
            "SoilFormulas.InvalidPercentage",
            "Percentage must be between 1 and 100."
        );

        public static readonly Error InvalidOrder = Error.Problem(
            "SoilFormulas.InvalidOrder",
            "Order must be zero or a positive integer."
        );
    }
}
