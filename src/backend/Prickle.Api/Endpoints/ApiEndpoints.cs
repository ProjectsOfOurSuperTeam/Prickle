namespace Prickle.Api.Endpoints;

public static class ApiEndpoints
{
    public const string ApiBase = "/api";

    public static class Soil
    {
        private const string Base = ApiBase + "/soil";
        public const string Add = Base;
        public const string Get = $"{Base}/{{id:int}}";
    }
}
