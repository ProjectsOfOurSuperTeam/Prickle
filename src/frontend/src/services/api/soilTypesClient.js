import './models.js';

const API_BASE = '/api/soil/types';

/**
 * @param {{ request: (path: string, init?: { method?: string; body?: string; params?: Record<string, unknown> }) => Promise<unknown> }} base
 * @returns {{
 *   getAll: (params?: SoilTypesListParams) => Promise<PagedResponse<SoilTypeResponse>>,
 *   get: (id: number) => Promise<SoilTypeResponse>,
 *   add: (body: AddSoilTypeRequest) => Promise<SoilTypeResponse>,
 *   update: (id: number, body: UpdateSoilTypeRequest) => Promise<SoilTypeResponse>,
 *   delete: (id: number) => Promise<void>,
 * }}
 */
export function createSoilTypesClient(base) {
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
  };
}
