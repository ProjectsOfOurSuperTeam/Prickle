namespace Prickle.Api.Endpoints;

public static class ApiEndpoints
{
    public const string ApiBase = "/api";
    public static class Soil
    {
        private const string Base = ApiBase + "/soil";
        public static class Types
        {
            private const string TypesBase = Base + "/types";
            public const string Add = TypesBase;
            public const string Get = $"{TypesBase}/{{id:int}}";
            public const string GetAll = TypesBase;
            public const string Update = $"{TypesBase}/{{id:int}}";
            public const string Delete = $"{TypesBase}/{{id:int}}";
        }
        public static class Formulas
        {
            private const string FormulasBase = Base + "/formulas";
            public const string Add = FormulasBase;
            public const string Get = $"{FormulasBase}/{{id:guid}}";
            public const string GetAll = FormulasBase;
            public const string Update = $"{FormulasBase}/{{id:guid}}";
            public const string Delete = $"{FormulasBase}/{{id:guid}}";
        }
    }

    public static class Decorations
    {
        public const string DecorationsBase = ApiBase + "/decorations";
        public const string GetCategories = $"{DecorationsBase}/categories";
        public const string Add = DecorationsBase;
        public const string Get = $"{DecorationsBase}/{{id:guid}}";
        public const string GetAll = DecorationsBase;
        public const string Update = $"{DecorationsBase}/{{id:guid}}";
        public const string Delete = $"{DecorationsBase}/{{id:guid}}";
    }
}
