/**
 * @typedef {Object} Project
 * @property {string} id
 * @property {string} userId - FK to User
 * @property {string} containerId - FK to Container
 * @property {string|null} preview - Path to AI or Canvas render image
 * @property {string} createdAt - ISO date string
 * @property {boolean} isPublished - Default false
 */
