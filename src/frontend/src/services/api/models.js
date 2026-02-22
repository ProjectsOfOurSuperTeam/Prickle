// ----- Paged list -----

/**
 * @template T
 * @typedef {{ items: T[]; page: number; pageSize: number; total: number }} PagedResponse
 */

// ----- Containers -----

/** @typedef {{ id: string; name: string; description?: string | null; volume: number; isClosed: boolean; imageUrl?: string | null; imageIsometricUrl?: string | null }} ContainerResponse */
/** @typedef {{ name: string; description?: string | null; volume: number; isClosed: boolean; imageUrl?: string | null; imageIsometricUrl?: string | null }} AddContainerRequest */
/** @typedef {{ name: string; description?: string | null; volume: number; isClosed: boolean; imageUrl?: string | null; imageIsometricUrl?: string | null }} UpdateContainerRequest */

// ----- Plants -----

/** @typedef {{ id: number; name: string }} IdNamePair */
/** @typedef {{ id: string; name: string; nameLatin: string; description?: string | null; imageUrl?: string | null; imageIsometricUrl?: string | null; category: number; lightLevel: number; waterNeed: number; humidityLevel: number; itemMaxSize: number; soilFormulaId: string }} PlantResponse */
/** @typedef {{ name: string; nameLatin: string; description?: string | null; imageUrl?: string | null; imageIsometricUrl?: string | null; category: number; lightLevel: number; waterNeed: number; humidityLevel: number; itemMaxSize: number; soilFormulaId: string }} AddPlantRequest */
/** @typedef {{ name: string; nameLatin: string; description?: string | null; imageUrl?: string | null; imageIsometricUrl?: string | null; category: number; lightLevel: number; waterNeed: number; humidityLevel: number; itemMaxSize: number }} UpdatePlantRequest */
/** @typedef {{ categories: IdNamePair[] }} PlantCategoriesResponse */
/** @typedef {{ lightLevels: IdNamePair[] }} PlantLightLevelsResponse */
/** @typedef {{ waterNeeds: IdNamePair[] }} PlantWaterNeedsResponse */
/** @typedef {{ humidityLevels: IdNamePair[] }} PlantHumidityLevelsResponse */
/** @typedef {{ itemSizes: IdNamePair[] }} PlantItemSizesResponse */

// ----- Decorations -----

/** @typedef {{ id: string; name: string; description?: string | null; category: number; imageUrl?: string | null; imageIsometricUrl?: string | null }} DecorationResponse */
/** @typedef {{ name: string; description?: string | null; category: number; imageUrl?: string | null; imageIsometricUrl?: string | null }} AddDecorationRequest */
/** @typedef {{ name: string; description?: string | null; category: number; imageUrl?: string | null; imageIsometricUrl?: string | null }} UpdateDecorationRequest */
/** @typedef {{ items: IdNamePair[] }} DecorationCategoriesResponse */

// ----- Soil types -----

/** @typedef {{ id: number; name: string }} SoilTypeResponse */
/** @typedef {{ name: string }} AddSoilTypeRequest */
/** @typedef {{ newName: string }} UpdateSoilTypeRequest */

// ----- Soil formulas -----

/** @typedef {{ soilType: SoilTypeResponse; percentage: number; order: number }} SoilFormulaItemResponse */
/** @typedef {{ id: string; name: string; items: SoilFormulaItemResponse[] }} SoilFormulaResponse */
/** @typedef {{ soilTypeId: number; percentage: number; order: number }} SoilFormulaItemRequest */
/** @typedef {{ name: string; formulaItems: SoilFormulaItemRequest[] }} AddSoilFormulaRequest */
/** @typedef {{ newName: string; formulaItems: SoilFormulaItemRequest[] }} UpdateSoilFormulaRequest */

// ----- Projects -----

/** @typedef {{ id: string; projectId: string; itemType: number; itemId: string; posX: number; posY: number; posZ: number }} ProjectItemResponse */
/** @typedef {{ id: string; userId: string; containerId: string; preview?: string | null; createdAt: string; isPublished: boolean; items: ProjectItemResponse[] }} ProjectResponse */
/** @typedef {{ containerId: string; preview?: string | null }} AddProjectRequest */
/** @typedef {{ preview?: string | null }} UpdateProjectRequest */
/** @typedef {{ itemType: number; itemId: string; posX: number; posY: number; posZ: number }} AddProjectItemRequest */
/** @typedef {{ posX: number; posY: number; posZ: number }} UpdateProjectItemRequest */

// ----- List params (query) -----

/** @typedef {{ page?: number; pageSize?: number; sortBy?: string; name?: string }} ContainersListParams */
/** @typedef {{ page?: number; pageSize?: number; sortBy?: string; name?: string }} PlantsListParams */
/** @typedef {{ page?: number; pageSize?: number; sortBy?: string; name?: string }} DecorationsListParams */
/** @typedef {{ page?: number; pageSize?: number; sortBy?: string; name?: string }} SoilTypesListParams */
/** @typedef {{ page?: number; pageSize?: number; sortBy?: string; name?: string; soilTypeIds?: number[] }} SoilFormulasListParams */
/** @typedef {{ page?: number; pageSize?: number; sortBy?: string; userId?: string; isPublished?: boolean }} ProjectsListParams */
