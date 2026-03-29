/**
 * Map API project item type to internal kind (backend: Plant=0, Decoration=1, Soil=2).
 * @param {number|string} raw
 * @returns {'plant'|'decoration'|'soil'|null}
 */
export function normalizeProjectItemType(raw) {
  if (raw === 0 || raw === '0') return 'plant';
  if (raw === 1 || raw === '1') return 'decoration';
  if (raw === 2 || raw === '2') return 'soil';
  if (typeof raw === 'string') {
    const u = raw.toLowerCase();
    if (u === 'plant') return 'plant';
    if (u === 'decoration') return 'decoration';
    if (u === 'soil') return 'soil';
  }
  return null;
}
