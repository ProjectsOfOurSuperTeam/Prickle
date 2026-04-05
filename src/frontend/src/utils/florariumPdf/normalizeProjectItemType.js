/**
 * Map API project item type to internal kind (backend: Plant=0, Decoration=1, Soil=2).
 * @param {number|string|{ value?: number; name?: string }|null|undefined} raw
 * @returns {'plant'|'decoration'|'soil'|null}
 */
export function normalizeProjectItemType(raw) {
  if (raw != null && typeof raw === 'object') {
    if ('value' in raw && raw.value !== undefined) {
      return normalizeProjectItemType(raw.value);
    }
    if (typeof raw.name === 'string') {
      return normalizeProjectItemType(raw.name);
    }
    return null;
  }
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
