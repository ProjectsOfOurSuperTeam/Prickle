import { createBaseClient } from './baseClient';
import { createContainersClient } from './containersClient';
import { createDecorationsClient } from './decorationsClient';
import { createPlantsClient } from './plantsClient';
import { createProjectsClient } from './projectsClient';
import { createSoilFormulasClient } from './soilFormulasClient';
import { createSoilTypesClient } from './soilTypesClient';

// Re-export models so consumers can import types: import { type ContainerResponse } from './services/api'
import './models.js';

export { ApiError } from './apiError';
export { createBaseClient } from './baseClient';
export { getApiBaseUrl } from '../../config/apiConfig';
export { createContainersClient } from './containersClient';
export { createDecorationsClient } from './decorationsClient';
export { createPlantsClient } from './plantsClient';
export { createProjectsClient } from './projectsClient';
export { createSoilFormulasClient } from './soilFormulasClient';
export { createSoilTypesClient } from './soilTypesClient';

/**
 * Creates API clients bound to the given access token getter.
 * Use from React: createApiClients(() => session?.accessToken) where session comes from useAuth().
 * @param {() => string | null | undefined} getAccessToken
 * @param {{ fetchFn?: typeof fetch }} [options]
 * @returns {{ containers: ReturnType<typeof createContainersClient>; plants: ReturnType<typeof createPlantsClient>; decorations: ReturnType<typeof createDecorationsClient>; soil: { types: ReturnType<typeof createSoilTypesClient>; formulas: ReturnType<typeof createSoilFormulasClient> }; projects: ReturnType<typeof createProjectsClient> }}
 */
export function createApiClients(getAccessToken, options = {}) {
  const base = createBaseClient({ getAccessToken, fetchFn: options.fetchFn });
  return {
    containers: createContainersClient(base),
    plants: createPlantsClient(base),
    decorations: createDecorationsClient(base),
    soil: {
      types: createSoilTypesClient(base),
      formulas: createSoilFormulasClient(base),
    },
    projects: createProjectsClient(base),
  };
}
