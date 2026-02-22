/**
 * Error thrown when the API returns a non-2xx response (RFC 7807 Problem Details).
 * @property {number} status - HTTP status code
 * @property {string} [code] - Error code from problem details (title/code)
 * @property {string} [detail] - Human-readable detail
 * @property {Record<string, string[]>} [errors] - Validation errors by field name
 */
export class ApiError extends Error {
  /**
   * @param {number} status
   * @param {string} [code]
   * @param {string} [detail]
   * @param {Record<string, string[]>} [errors]
   */
  constructor(status, code, detail, errors) {
    super(detail || `Request failed with status ${status}`);
    this.name = 'ApiError';
    this.status = status;
    this.code = code;
    this.detail = detail;
    this.errors = errors;
  }
}
