import { normalizeProjectItemType } from './normalizeProjectItemType';

/** Short Ukrainian hints for care (levels 1–5). */
const WATER_HINT = {
  1: 'полив після повного просихання субстрату (рідко)',
  2: 'полив при просиханні нижніх шарів',
  3: 'полив при просиханні верхнього шару (типово 7–10 днів)',
  4: 'підтримувати вологість субстрату без перезволоження',
  5: 'постійна волога / піддон з водою (для гідрофітів)',
};

const HUMIDITY_HINT = {
  1: 'сухе повітря — лише відкриті форми',
  2: 'звичайна кімната',
  3: 'обприскування або частково закрита форма',
  4: 'підвищена вологість, напівзакриті форми',
  5: 'герметичні закриті системи',
};

const LIGHT_HINT = {
  1: 'тінь / мало світла',
  2: 'півтінь',
  3: 'розсіяне світло',
  4: 'яскраве світло',
  5: 'пряме сонце / максимум світла',
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
      const wn = Number(p.waterNeed);
      const hl = Number(p.humidityLevel);
      const ll = Number(p.lightLevel);
      return {
        name: p.name,
        nameLatin: p.nameLatin ?? '',
        count,
        waterNeed: wn,
        humidityLevel: hl,
        lightLevel: ll,
        waterHint: WATER_HINT[wn] ?? '',
        humidityHint: HUMIDITY_HINT[hl] ?? '',
        lightHint: LIGHT_HINT[ll] ?? '',
        description: p.description ?? '',
      };
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
    container: {
      name: container.name,
      volume: container.volume,
      isClosed: container.isClosed,
      description: container.description ?? '',
    },
    soilSection,
    plants: plantEntries,
    decorations: decorationEntries,
  };
}
