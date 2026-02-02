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
    }
}
