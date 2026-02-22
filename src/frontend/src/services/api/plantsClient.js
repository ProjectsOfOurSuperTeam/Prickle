import './models.js';

const API_BASE = '/api/plants';

/**
 * @param {{ request: (path: string, init?: { method?: string; body?: string; params?: Record<string, unknown> }) => Promise<unknown> }} base
 * @returns {{
 *   getAll: (params?: PlantsListParams) => Promise<PagedResponse<PlantResponse>>,
 *   get: (id: string) => Promise<PlantResponse>,
 *   add: (body: AddPlantRequest) => Promise<PlantResponse>,
 *   update: (id: string, body: UpdatePlantRequest) => Promise<PlantResponse>,
 *   delete: (id: string) => Promise<void>,
 *   getCategories: () => Promise<PlantCategoriesResponse>,
 *   getLightLevels: () => Promise<PlantLightLevelsResponse>,
 *   getWaterNeeds: () => Promise<PlantWaterNeedsResponse>,
 *   getHumidityLevels: () => Promise<PlantHumidityLevelsResponse>,
 *   getItemSizes: () => Promise<PlantItemSizesResponse>,
 * }}
 */
export function createPlantsClient(base) {
  const { request } = base;
  return {
    async getAll(params = {}) {
      return request(API_BASE, { method: 'GET', params });
    },
    async get(id) {
      return request(`${API_BASE}/${id}`, { method: 'GET' });
    },
    async add(body) {
      return request(API_BASE, { method: 'POST', body: JSON.stringify(body) });
    },
    async update(id, body) {
      return request(`${API_BASE}/${id}`, { method: 'PATCH', body: JSON.stringify(body) });
    },
    async delete(id) {
      return request(`${API_BASE}/${id}`, { method: 'DELETE' });
    },
    async getCategories() {
      return request(`${API_BASE}/categories`, { method: 'GET' });
    },
    async getLightLevels() {
      return request(`${API_BASE}/light-levels`, { method: 'GET' });
    },
    async getWaterNeeds() {
      return request(`${API_BASE}/water-needs`, { method: 'GET' });
    },
    async getHumidityLevels() {
      return request(`${API_BASE}/humidity-levels`, { method: 'GET' });
    },
    async getItemSizes() {
      return request(`${API_BASE}/item-sizes`, { method: 'GET' });
    },
  };
}
