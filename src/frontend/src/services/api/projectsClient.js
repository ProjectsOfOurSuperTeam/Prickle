import './models.js';

const API_BASE = '/api/projects';

/**
 * @param {{ request: (path: string, init?: { method?: string; body?: string; params?: Record<string, unknown> }) => Promise<unknown> }} base
 * @returns {{
 *   getAll: (params?: ProjectsListParams) => Promise<PagedResponse<ProjectResponse>>,
 *   get: (id: string) => Promise<ProjectResponse>,
 *   add: (body: AddProjectRequest) => Promise<ProjectResponse>,
 *   update: (id: string, body: UpdateProjectRequest) => Promise<ProjectResponse>,
 *   delete: (id: string) => Promise<void>,
 *   publish: (id: string) => Promise<ProjectResponse>,
 *   unpublish: (id: string) => Promise<ProjectResponse>,
 *   addItem: (projectId: string, body: AddProjectItemRequest) => Promise<ProjectItemResponse>,
 *   updateItem: (projectId: string, itemId: string, body: UpdateProjectItemRequest) => Promise<ProjectItemResponse>,
 *   removeItem: (projectId: string, itemId: string) => Promise<void>,
 * }}
 */
export function createProjectsClient(base) {
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
    async publish(id) {
      return request(`${API_BASE}/${id}/publish`, { method: 'POST' });
    },
    async unpublish(id) {
      return request(`${API_BASE}/${id}/unpublish`, { method: 'POST' });
    },
    async addItem(projectId, body) {
      return request(`${API_BASE}/${projectId}/items`, { method: 'POST', body: JSON.stringify(body) });
    },
    async updateItem(projectId, itemId, body) {
      return request(`${API_BASE}/${projectId}/items/${itemId}`, {
        method: 'PATCH',
        body: JSON.stringify(body),
      });
    },
    async removeItem(projectId, itemId) {
      return request(`${API_BASE}/${projectId}/items/${itemId}`, { method: 'DELETE' });
    },
  };
}
