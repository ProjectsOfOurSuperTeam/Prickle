import { normalizeProjectItemType } from './normalizeProjectItemType';

/**
 * @param {unknown} raw
 * @param {number} [fallback]
 * @returns {number} integer 1–5
 */
function normalizeLevel1to5(raw, fallback = 3) {
  const n = Math.round(Number(raw));
  if (!Number.isFinite(n)) return fallback;
  return Math.min(5, Math.max(1, n));
}

/** Short Ukrainian hints for care (levels 1–5). */
const WATER_HINT = {
  1: 'полив рідко — після повного просихання субстрату (часто раз на кілька тижнів)',
  2: 'полив помірний — коли підсохне нижній шар',
  3: 'полив коли підсохне верхній шар (часто кожні 7–10 днів у кімнаті)',
  4: 'полив частіше; підтримуйте вологість без застою води',
  5: 'постійна волога субстрату або піддон; для вологолюбних видів',
};

const HUMIDITY_HINT = {
  1: 'сухе повітря — лише відкриті форми',
  2: 'звичайна кімната',
  3: 'обприскування або частково закрита форма',
  4: 'підвищена вологість, напівзакриті форми',
  5: 'герметичні закриті системи',
};

const LIGHT_HINT = {
  1: 'тінь або далеко від вікна; прямого сонця уникати',
  2: 'півтінь, розсіяне світло (зручно східне чи північне вікно)',
  3: 'яскраве розсіяне світло; типове місце в кімнаті біля вікна',
  4: 'дуже світле місце; ближче до вікна або додаткове досвітлення',
  5: 'максимум світла: південне вікно або вихід на сонячну сторону (обережно з опіками)',
};

/**
 * Build shopping-list + instruction payload for PDF export.
 * @param {*} api - API clients from createApiClients (useApi)
 * @param {string} projectId
 */
export async function fetchFlorariumExportPayload(api, projectId) {
  const project = await api.projects.get(projectId);
  const container = await api.containers.get(project.containerId);

  const items = project.items ?? [];
  const plantCounts = new Map();
  const decorationCounts = new Map();
  let soilFormulaId = null;

  for (const row of items) {
    const kind = normalizeProjectItemType(row.itemType);
    if (kind === 'plant') {
      plantCounts.set(row.itemId, (plantCounts.get(row.itemId) ?? 0) + 1);
    } else if (kind === 'decoration') {
      decorationCounts.set(row.itemId, (decorationCounts.get(row.itemId) ?? 0) + 1);
    } else if (kind === 'soil') {
      if (!soilFormulaId) soilFormulaId = row.itemId;
    }
  }

  const plantEntries = await Promise.all(
    [...plantCounts.entries()].map(async ([id, count]) => {
      const p = await api.plants.get(id);
      const wn = normalizeLevel1to5(p.waterNeed);
      const hl = normalizeLevel1to5(p.humidityLevel);
      const ll = normalizeLevel1to5(p.lightLevel);
      return {
        name: p.name,
        nameLatin: p.nameLatin ?? '',
        count,
        category: p.category ?? null,
        waterNeed: wn,
        humidityLevel: hl,
        lightLevel: ll,
        soilFormulaId: p.soilFormulaId != null ? String(p.soilFormulaId) : null,
        waterHint: WATER_HINT[wn] ?? WATER_HINT[3],
        humidityHint: HUMIDITY_HINT[hl] ?? HUMIDITY_HINT[3],
        lightHint: LIGHT_HINT[ll] ?? LIGHT_HINT[3],
        description: p.description ?? '',
      };
    }),
  );

  /** @type {Set<string>} */
  const soilFormulaIdsForNames = new Set();
  if (soilFormulaId) soilFormulaIdsForNames.add(String(soilFormulaId));
  for (const e of plantEntries) {
    if (e.soilFormulaId) soilFormulaIdsForNames.add(String(e.soilFormulaId));
  }
  /** @type {Record<string, string>} */
  const soilFormulaNames = {};
  await Promise.all(
    [...soilFormulaIdsForNames].map(async (id) => {
      try {
        const f = await api.soil.formulas.get(id);
        soilFormulaNames[id] = f?.name ?? '';
      } catch {
        soilFormulaNames[id] = '';
      }
    }),
  );

  const decorationEntries = await Promise.all(
    [...decorationCounts.entries()].map(async ([id, count]) => {
      const d = await api.decorations.get(id);
      return {
        name: d.name,
        count,
        description: d.description ?? '',
      };
    }),
  );

  let soilSection = null;
  if (soilFormulaId) {
    const formula = await api.soil.formulas.get(soilFormulaId);
    const volume = Number(container.volume) || 0;
    const rows = [...(formula.items ?? [])]
      .sort((a, b) => a.order - b.order)
      .map((it) => {
        const pct = Number(it.percentage) || 0;
        const liters = volume * (pct / 100);
        return {
          order: it.order,
          componentName: it.soilType?.name ?? '—',
          percentage: pct,
          liters,
        };
      });
    const totalPct = rows.reduce((s, r) => s + r.percentage, 0);
    // Avoid empty PDF table when formula has no line items (data inconsistency).
    if (rows.length > 0) {
      soilSection = {
        formulaName: formula.name,
        rows,
        totalPercentage: totalPct,
        volumeLiters: volume,
      };
    }
  }

  return {
    projectId: project.id,
    createdAt: project.createdAt,
    selectedSoilFormulaId: soilFormulaId != null ? String(soilFormulaId) : null,
    container: {
      name: container.name,
      volume: container.volume,
      isClosed: container.isClosed,
      description: container.description ?? '',
    },
    soilSection,
    plants: plantEntries,
    decorations: decorationEntries,
    /** For compatibility modal: resolve formula id → display name (same as constructor). */
    soilFormulaNames,
  };
}
