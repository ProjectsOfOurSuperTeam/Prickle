/**
 * @typedef {Object} ProjectItem
 * @property {string} id
 * @property {string} projectId - FK to Project
 * @property {'Plant'|'Decoration'|'Soil'} itemType
 * @property {string} itemId - ID from corresponding table
 * @property {number} posX - X coordinate in isometric grid
 * @property {number} posY - Y coordinate in isometric grid
 * @property {number} zIndex - Layer order for stacking
 */
