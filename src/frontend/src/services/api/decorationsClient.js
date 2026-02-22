import './models.js';

const API_BASE = '/api/decorations';

/**
 * @param {{ request: (path: string, init?: { method?: string; body?: string; params?: Record<string, unknown> }) => Promise<unknown> }} base
 * @returns {{
 *   getAll: (params?: DecorationsListParams) => Promise<PagedResponse<DecorationResponse>>,
 *   get: (id: string) => Promise<DecorationResponse>,
 *   add: (body: AddDecorationRequest) => Promise<DecorationResponse>,
 *   update: (id: string, body: UpdateDecorationRequest) => Promise<DecorationResponse>,
 *   delete: (id: string) => Promise<void>,
 *   getCategories: () => Promise<DecorationCategoriesResponse>,
 * }}
 */
export function createDecorationsClient(base) {
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
  };
}
