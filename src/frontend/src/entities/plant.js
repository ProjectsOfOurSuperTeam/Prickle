/**
 * @typedef {Object} Plant
 * @property {string} id
 * @property {string} nameUa - Ukrainian name
 * @property {string} nameLatin - Latin name
 * @property {string|null} description
 * @property {number} lightLevel - 1-5, how much light it likes
 * @property {number} waterNeed - 1-5, watering frequency
 * @property {number} humidityLevel - 1-5, humidity sensitivity
 * @property {number} maxSize - For container volume calculation
 * @property {string} soilFormulaId - FK to SoilFormula
 * @property {string|null} image - Path to image file (e.g. "entities/plants/echeveria_elegans.png")
 * @property {string|null} imageIsometric - Path to isometric sprite for constructor
 */
