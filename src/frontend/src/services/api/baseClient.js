import { getApiBaseUrl } from '../../config/apiConfig';
import { ApiError } from './apiError';

/**
 * @typedef {Object} RequestOptions
 * @property {() => string | null | undefined} [getAccessToken]
 * @property {typeof fetch} [fetchFn]
 */

/**
 * Builds full URL from API base + path.
 * @param {string} path - Path starting with /api/...
 * @returns {string}
 */
function buildUrl(path) {
  const base = getApiBaseUrl();
  const normalizedPath = path.startsWith('/') ? path : `/${path}`;
  return `${base}${normalizedPath}`;
}

/**
 * Builds query string from params object (skips undefined/null/empty).
 * @param {Record<string, string | number | boolean | undefined | null | (string | number)[]>} params
 * @returns {string}
 */
function buildSearchParams(params) {
  const search = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value === undefined || value === null || value === '') continue;
    if (Array.isArray(value)) {
      value.forEach((v) => search.append(key, String(v)));
    } else {
      search.set(key, String(value));
    }
  }
  const qs = search.toString();
  return qs ? `?${qs}` : '';
}

/**
 * Parses RFC 7807 problem details from response body.
 * @param {Record<string, unknown>} body
 * @param {number} status
 * @returns {ApiError}
 */
function parseProblemDetails(body, status) {
  const title = typeof body.title === 'string' ? body.title : undefined;
  const detail = typeof body.detail === 'string' ? body.detail : undefined;
  const ext = body && typeof body === 'object' && 'errors' in body ? body.errors : undefined;
  const errors =
    ext && typeof ext === 'object' && !Array.isArray(ext) ? (ext) : undefined;
  return new ApiError(status, title, detail, errors);
}

/**
 * Creates a base API client that sends authenticated JSON requests and throws ApiError on failure.
 * @param {RequestOptions} [options]
 * @returns {{ request: (path: string, init: RequestInit & { params?: Record<string, string | number | boolean | undefined | null | (string | number)[]> }) => Promise<any> }}
 */
export function createBaseClient(options = {}) {
  const { getAccessToken, fetchFn = fetch } = options;

  /**
   * @param {string} path - Path e.g. /api/containers
   * @param {RequestInit & { params?: Record<string, string | number | boolean | undefined | null | (string | number)[]> }} init - fetch init + optional params for query string
   * @returns {Promise<any>} Parsed JSON or undefined for 204
   */
  async function request(path, init = {}) {
    const { params, ...fetchInit } = init;
    let url = buildUrl(path);
    if (params && Object.keys(params).length > 0) {
      url += buildSearchParams(params);
    }

    const headers = new Headers(fetchInit.headers);
    if (!headers.has('Content-Type') && fetchInit.body != null && typeof fetchInit.body === 'string') {
      headers.set('Content-Type', 'application/json');
    }
    headers.set('api-version', '1.0');

    const token = getAccessToken?.();
    if (token) {
      headers.set('Authorization', `Bearer ${token}`);
    }

    const response = await fetchFn(url, {
      ...fetchInit,
      headers,
    });

    const contentType = response.headers.get('Content-Type') ?? '';
    const isJson = contentType.includes('application/json') || contentType.includes('application/problem+json');

    if (!response.ok) {
      let body = null;
      if (isJson) {
        try {
          body = await response.json();
        } catch {
          // ignore
        }
      }
      if (body && typeof body === 'object') {
        throw parseProblemDetails(body, response.status);
      }
      throw new ApiError(response.status, undefined, `Request failed with status ${response.status}`, undefined);
    }

    if (response.status === 204) {
      return undefined;
    }
    if (isJson) {
      return response.json();
    }
    return undefined;
  }

  return { request };
}
