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

    public static class Containers
    {
        public const string ContainersBase = ApiBase + "/containers";
        public const string Add = ContainersBase;
        public const string Get = $"{ContainersBase}/{{id:guid}}";
        public const string GetAll = ContainersBase;
        public const string Update = $"{ContainersBase}/{{id:guid}}";
        public const string Delete = $"{ContainersBase}/{{id:guid}}";
    }

    public static class Plants
    {
        public const string PlantsBase = ApiBase + "/plants";
        public const string GetCategories = $"{PlantsBase}/categories";
        public const string GetLightLevels = $"{PlantsBase}/light-levels";
        public const string GetWaterNeeds = $"{PlantsBase}/water-needs";
        public const string GetHumidityLevels = $"{PlantsBase}/humidity-levels";
        public const string GetItemSizes = $"{PlantsBase}/item-sizes";
        public const string Add = PlantsBase;
        public const string Get = $"{PlantsBase}/{{id:guid}}";
        public const string GetAll = PlantsBase;
        public const string Update = $"{PlantsBase}/{{id:guid}}";
        public const string Delete = $"{PlantsBase}/{{id:guid}}";
    }

    public static class Projects
    {
        public const string ProjectsBase = ApiBase + "/projects";
        public const string Add = ProjectsBase;
        public const string Get = $"{ProjectsBase}/{{id:guid}}";
        public const string GetAll = ProjectsBase;
        public const string Update = $"{ProjectsBase}/{{id:guid}}";
        public const string Delete = $"{ProjectsBase}/{{id:guid}}";
        public const string Publish = $"{ProjectsBase}/{{id:guid}}/publish";
        public const string Unpublish = $"{ProjectsBase}/{{id:guid}}/unpublish";
        public const string AddItem = $"{ProjectsBase}/{{id:guid}}/items";
        public const string UpdateItem = $"{ProjectsBase}/{{projectId:guid}}/items/{{itemId:guid}}";
        public const string RemoveItem = $"{ProjectsBase}/{{projectId:guid}}/items/{{itemId:guid}}";
    }
}
