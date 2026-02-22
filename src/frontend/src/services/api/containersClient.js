import './models.js';

const API_BASE = '/api/containers';

/**
 * @param {{ request: (path: string, init?: { method?: string; body?: string; params?: Record<string, unknown> }) => Promise<unknown> }} base
 * @returns {{
 *   getAll: (params?: ContainersListParams) => Promise<PagedResponse<ContainerResponse>>,
 *   get: (id: string) => Promise<ContainerResponse>,
 *   add: (body: AddContainerRequest) => Promise<ContainerResponse>,
 *   update: (id: string, body: UpdateContainerRequest) => Promise<ContainerResponse>,
 *   delete: (id: string) => Promise<void>,
 * }}
 */
export function createContainersClient(base) {
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
      return request(`${API_BASE}/${id}`, { method: 'PUT', body: JSON.stringify(body) });
    },
    async delete(id) {
      return request(`${API_BASE}/${id}`, { method: 'DELETE' });
    },
  };
}
