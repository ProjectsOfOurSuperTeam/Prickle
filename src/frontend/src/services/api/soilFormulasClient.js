import './models.js';

const API_BASE = '/api/soil/formulas';

/**
 * @param {{ request: (path: string, init?: { method?: string; body?: string; params?: Record<string, unknown> }) => Promise<unknown> }} base
 * @returns {{
 *   getAll: (params?: SoilFormulasListParams) => Promise<PagedResponse<SoilFormulaResponse>>,
 *   get: (id: string) => Promise<SoilFormulaResponse>,
 *   add: (body: AddSoilFormulaRequest) => Promise<SoilFormulaResponse>,
 *   update: (id: string, body: UpdateSoilFormulaRequest) => Promise<SoilFormulaResponse>,
 *   delete: (id: string) => Promise<void>,
 * }}
 */
export function createSoilFormulasClient(base) {
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
