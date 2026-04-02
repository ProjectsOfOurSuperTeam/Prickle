/**
 * Soft compatibility hints for florarium plants (light / water / humidity spread + soil formula match).
 * Not blocking — informational tiers only.
 */

/** @typedef {'ok' | 'mild' | 'strong'} Severity */

/**
 * @param {number[]} values
 * @returns {number}
 */
function maxSpread(values) {
  if (values.length < 2) return 0;
  const nums = values.map((v) => Number(v)).filter((n) => Number.isFinite(n));
  if (nums.length < 2) return 0;
  return Math.max(...nums) - Math.min(...nums);
}

/**
 * @param {number} spread
 * @returns {Severity}
 */
function spreadSeverity(spread) {
  if (spread <= 1) return 'ok';
  if (spread === 2) return 'mild';
  return 'strong';
}

/**
 * @param {Severity} a
 * @param {Severity} b
 * @returns {Severity}
 */
function maxSeverity(a, b) {
  const order = { ok: 0, mild: 1, strong: 2 };
  return order[a] >= order[b] ? a : b;
}

/**
 * @param {Array<{ name: string; entityId?: string; lightLevel: number; waterNeed: number; humidityLevel: number; soilFormulaId?: string | null }>} plants
 */
function dedupePlants(plants) {
  const seen = new Map();
  for (const p of plants) {
    const key = p.entityId != null ? String(p.entityId) : String(p.name);
    if (!seen.has(key)) seen.set(key, p);
  }
  return [...seen.values()];
}

/**
 * @param {number[]} values
 * @returns {{ min: number; max: number } | null}
 */
function minMaxLevels(values) {
  const nums = values.map((v) => Number(v)).filter((n) => Number.isFinite(n));
  if (nums.length === 0) return null;
  return { min: Math.min(...nums), max: Math.max(...nums) };
}

/**
 * Backend PlantCategory enum value (0..6) or SmartEnum name string.
 * @param {unknown} raw
 * @returns {number | null}
 */
function normalizePlantCategory(raw) {
  if (raw == null || raw === '') return null;
  if (typeof raw === 'number' && Number.isFinite(raw)) {
    return raw >= 0 && raw <= 6 ? raw : null;
  }
  const byName = {
    NoCategory: 0,
    Succulents: 1,
    Cacti: 2,
    Tropical: 3,
    Ferns: 4,
    Mosses: 5,
    CarnivorousPlants: 6,
  };
  const s = String(raw);
  if (Object.prototype.hasOwnProperty.call(byName, s)) return byName[s];
  const n = Number(s);
  return Number.isFinite(n) && n >= 0 && n <= 6 ? n : null;
}

/** Ukrainian labels for catalog category ids (1..6). */
const CATEGORY_LABEL_UK = {
  1: 'сукуленти',
  2: 'кактуси',
  3: 'тропічні',
  4: 'папороті',
  5: 'мохи',
  6: 'хижі рослини',
};

/**
 * Incompatible ecology groups (catalog categories) — extra to numeric spread (watering etc.).
 * @param {Array<{ name?: string; category?: unknown }>} plants
 * @returns {{ severity: Severity; lines: string[]; conflictKind: 'carn_dry' | 'carn_trop' | 'dry_trop' | null }}
 */
function analyzeEcologyByCategory(plants) {
  const lines = [];
  /** @type {'carn_dry' | 'carn_trop' | 'dry_trop' | null} */
  let conflictKind = null;
  if (plants.length < 2) return { severity: 'ok', lines, conflictKind: null };

  const cats = new Set();
  for (const p of plants) {
    const c = normalizePlantCategory(p.category);
    if (c != null && c !== 0) cats.add(c);
  }
  if (cats.size < 2) return { severity: 'ok', lines, conflictKind: null };

  const carn = cats.has(6);
  const dry = cats.has(1) || cats.has(2);
  const trop = cats.has(3);

  /** @type {Severity} */
  let sev = 'ok';

  if (carn && dry) {
    sev = 'strong';
    conflictKind = 'carn_dry';
    lines.push(
      'За категоріями каталогу: хижі рослини не поєднуються з сукулентами/кактусами в одній посудині без зонування — вода, мінерали та pH зазвичай несумісні.',
    );
  } else if (carn && trop) {
    sev = 'strong';
    conflictKind = 'carn_trop';
    lines.push(
      'За категоріями каталогу: хижі рослини й тропічні види зазвичай різко розходяться за субстратом і якістю води; спільні умови без поділу ризиковані.',
    );
  } else if (dry && trop) {
    sev = 'strong';
    conflictKind = 'dry_trop';
    lines.push(
      'За категоріями каталогу: сухолюбні й тропічні види в одному об’ємі часто суперечливі за поливом і вологістю субстрату.',
    );
  }

  return { severity: sev, lines, conflictKind };
}

/**
 * @param {Severity} sev
 * @returns {number}
 */
function severityBaseWeight(sev) {
  if (sev === 'strong') return 28;
  if (sev === 'mild') return 14;
  return 0;
}

/**
 * @param {Array<{ name?: string; category?: unknown }>} plants
 * @param {'carn_dry' | 'carn_trop' | 'dry_trop' | null} conflictKind
 * @returns {string}
 */
function ecologyFactorDetail(plants, conflictKind) {
  if (!conflictKind) return '';
  const byCat = new Map();
  for (const p of plants) {
    const c = normalizePlantCategory(p.category);
    if (c == null || c === 0) continue;
    if (!byCat.has(c)) byCat.set(c, []);
    byCat.get(c).push(p.name ? String(p.name) : '?');
  }
  const parts = [];
  for (const [cid, names] of byCat) {
    const lab = CATEGORY_LABEL_UK[/** @type {keyof typeof CATEGORY_LABEL_UK} */ (cid)] ?? `кат.${cid}`;
    parts.push(`${lab}: ${names.join(', ')}`);
  }
  const combo =
    conflictKind === 'carn_dry'
      ? 'хижі × сукуленти/кактуси'
      : conflictKind === 'carn_trop'
        ? 'хижі × тропічні'
        : 'сухолюбні × тропічні';
  return `тип конфлікту «${combo}»; у проєкті: ${parts.join(' · ')}`;
}

/**
 * Relative importance weights (normalized to 100% for active factors only).
 * @param {{
 *   plants: Array<{ name?: string; category?: unknown }>;
 *   ecology: { severity: Severity; conflictKind: 'carn_dry' | 'carn_trop' | 'dry_trop' | null };
 *   sevL: Severity; sevW: Severity; sevH: Severity;
 *   lightSpread: number; waterSpread: number; humiditySpread: number;
 *   soilIssue: boolean;
 *   soilMismatch: boolean;
 * }} ctx
 * @returns {Array<{ key: string; label: string; detail: string; weightPct: number }>}
 */
function buildPairingFactors(ctx) {
  const { plants, ecology, sevL, sevW, sevH, lightSpread, waterSpread, humiditySpread, soilIssue, soilMismatch } =
    ctx;
  const lm = plants.length >= 2 ? minMaxLevels(plants.map((p) => p.lightLevel)) : null;
  const wm = plants.length >= 2 ? minMaxLevels(plants.map((p) => p.waterNeed)) : null;
  const hm = plants.length >= 2 ? minMaxLevels(plants.map((p) => p.humidityLevel)) : null;

  /** @type {Array<{ key: string; label: string; detail: string; w: number }>} */
  const raw = [];

  if (ecology.severity !== 'ok' && ecology.conflictKind) {
    raw.push({
      key: 'ecology',
      label: 'Екологічні групи (каталог)',
      detail: ecologyFactorDetail(plants, ecology.conflictKind),
      w: ecology.severity === 'strong' ? 42 : 24,
    });
  }

  if (sevL !== 'ok' && lm) {
    raw.push({
      key: 'light',
      label: 'Світло (шкала 1–5)',
      detail: `діапазон між видами ${lm.min}–${lm.max}, розкид ${lightSpread}`,
      w: severityBaseWeight(sevL),
    });
  }
  if (sevW !== 'ok' && wm) {
    raw.push({
      key: 'water',
      label: 'Полив (шкала 1–5)',
      detail: `діапазон між видами ${wm.min}–${wm.max}, розкид ${waterSpread}`,
      w: severityBaseWeight(sevW),
    });
  }
  if (sevH !== 'ok' && hm) {
    raw.push({
      key: 'humidity',
      label: 'Вологість повітря (1–5)',
      detail: `діапазон між видами ${hm.min}–${hm.max}, розкид ${humiditySpread}`,
      w: severityBaseWeight(sevH),
    });
  }

  if (soilIssue) {
    raw.push({
      key: soilMismatch ? 'soil_mismatch' : 'soil_missing',
      label: 'Ґрунт (проєкт ↔ каталог)',
      detail: soilMismatch
        ? 'для частини видів обрана формула не збігається з рекомендованою в каталозі'
        : 'формула ґрунту в проєкті не обрана — перевірка субстрату не виконана',
      w: soilMismatch ? 20 : 16,
    });
  }

  if (raw.length === 0) return [];

  const sumW = raw.reduce((s, x) => s + x.w, 0);
  if (sumW <= 0) return [];

  const scaled = raw.map((x) => ({
    key: x.key,
    label: x.label,
    detail: x.detail,
    weightPct: Math.floor((x.w / sumW) * 100),
  }));
  let rem = 100 - scaled.reduce((s, x) => s + x.weightPct, 0);
  let i = 0;
  while (rem > 0 && scaled.length > 0) {
    scaled[i % scaled.length].weightPct += 1;
    rem -= 1;
    i += 1;
  }
  return scaled;
}

/**
 * Short headline when structured factors list is shown below.
 * @param {number} plantsCount
 * @param {Severity} careSeverity
 * @param {boolean} soilIssue
 * @param {{ ecologyStrong?: boolean; spreadStrong?: boolean; hasPairingFactors?: boolean }} [meta]
 * @returns {{ text: string; level: 'ok' | 'caution' | 'risk' | 'neutral' }}
 */
function buildPairingVerdict(plantsCount, careSeverity, soilIssue, meta) {
  const hasFactors = meta?.hasPairingFactors === true;

  if (plantsCount === 0) {
    return { text: '', level: 'neutral' };
  }
  if (plantsCount < 2) {
    if (soilIssue) {
      return {
        text: hasFactors
          ? 'Одна рослина — порівняння між видами не застосовується; показник ґрунту та вага нижче.'
          : 'Одна рослина на полотні — порівняння між видами не застосовується; нижче — перевірка ґрунту.',
        level: 'caution',
      };
    }
    return {
      text:
        'Одна рослина на полотні — висновок «чи підходять одна одній» з’явиться після додавання ще одного виду.',
      level: 'neutral',
    };
  }

  if (careSeverity === 'ok' && !soilIssue) {
    return {
      text:
        'Так — обрані види за даними каталогу добре узгоджені між собою за доглядом (світло, полив, вологість); ґрунт відповідає рекомендаціям для всіх.',
      level: 'ok',
    };
  }

  if (hasFactors) {
    if (careSeverity === 'ok' && soilIssue) {
      return {
        text: 'Частково — за доглядом види близькі, але є зауваження щодо ґрунту (див. нижче).',
        level: 'caution',
      };
    }
    if (careSeverity === 'mild') {
      return {
        text: soilIssue
          ? 'Частково — є відмінності в догляді та/або зауваження щодо ґрунту (див. нижче).'
          : 'Ймовірно з обережністю — між видами є відмінності за доглядом (див. нижче).',
        level: 'caution',
      };
    }
    return {
      text: 'Ні — поєднання ризиковане для спільної посудини без зонування (див. короткий опис нижче).',
      level: 'risk',
    };
  }

  if (careSeverity === 'ok' && soilIssue) {
    return {
      text:
        'Частково — за доглядом види між собою близькі, але є зауваження щодо ґрунту (див. нижче).',
      level: 'caution',
    };
  }
  if (careSeverity === 'mild' && !soilIssue) {
    return {
      text:
        'Ймовірно з обережністю — між видами є помірні відмінності за однією або кількома шкалами догляду.',
      level: 'caution',
    };
  }
  if (careSeverity === 'mild' && soilIssue) {
    return {
      text:
        'Частково — є і помірні відмінності за доглядом між видами, і зауваження щодо ґрунту.',
      level: 'caution',
    };
  }
  const ecologyStrong = meta?.ecologyStrong === true;
  const spreadStrong = meta?.spreadStrong === true;

  if (ecologyStrong && spreadStrong) {
    const base =
      'Ні — і за трьома шкалами догляду, і за екологічними групами каталогу поєднання виглядає ризикованим.';
    if (soilIssue) {
      return { text: `${base} Є також зауваження щодо ґрунту (див. нижче).`, level: 'risk' };
    }
    return { text: base, level: 'risk' };
  }
  if (ecologyStrong && !spreadStrong) {
    const base =
      'Ні — у проєкті одночасно види з різних екологічних груп (категорії каталогу). Шкали 1–5 у каталозі якраз показують різний полив і догляд; конфлікт у тому, що в одній посудині без зонування один спільний режим зазвичай не підходить усім (додатково pH, вода, тип субстрату).';
    if (soilIssue) {
      return { text: `${base} Є також зауваження щодо ґрунту (див. нижче).`, level: 'risk' };
    }
    return { text: base, level: 'risk' };
  }
  if (!soilIssue) {
    return {
      text:
        'Ні — за каталогом вимоги видів сильно розходяться між собою; одна посудина без зонування чи поділу середовища ризикована.',
      level: 'risk',
    };
  }
  return {
    text:
      'Ні — сильні відмінності за доглядом між видами та/або критичні зауваження щодо ґрунту.',
    level: 'risk',
  };
}

/**
 * @param {{
 *   plants: Array<{ name: string; entityId?: string; lightLevel: number; waterNeed: number; humidityLevel: number; soilFormulaId?: string | null; category?: unknown }>;
 *   selectedSoilFormulaId: string | null | undefined;
 * }} input
 */
/**
 * @param {string[]} arr
 * @returns {string[]}
 */
function dedupeStrings(arr) {
  const out = [];
  const seen = new Set();
  for (const s of arr) {
    const t = String(s).trim();
    if (!t || seen.has(t)) continue;
    seen.add(t);
    out.push(t);
  }
  return out;
}

/**
 * Most frequent recommended soil formula id among plants that have soilFormulaId (same logic as constructor «Розрахувати»).
 * @param {Array<{ soilFormulaId?: string | null }>} plants
 * @returns {{ id: string; count: number } | null}
 */
function computeMajoritySoilFormulaId(plants) {
  const ids = [];
  for (const p of plants) {
    if (p.soilFormulaId != null && String(p.soilFormulaId).trim() !== '') {
      ids.push(String(p.soilFormulaId));
    }
  }
  if (ids.length === 0) return null;
  const counts = new Map();
  for (const id of ids) counts.set(id, (counts.get(id) ?? 0) + 1);
  let bestId = ids[0];
  let bestC = 0;
  for (const [id, c] of counts) {
    if (c > bestC) {
      bestId = id;
      bestC = c;
    }
  }
  return { id: bestId, count: bestC };
}

/**
 * Rich soil mismatch copy when formula names are available from catalog.
 * @param {Array<{ name: string; soilFormulaId?: string | null }>} plants
 * @param {string | null} selected
 * @param {(id: string) => string | null | undefined} resolveName
 * @returns {{ problemLines: string[] } | null}
 */
function buildSoilMismatchNarrative(plants, selected, resolveName) {
  if (!selected || typeof resolveName !== 'function') return null;
  const mismatches = plants.filter(
    (p) => p.soilFormulaId != null && String(p.soilFormulaId) !== selected,
  );
  if (mismatches.length === 0) return null;

  const selectedName = resolveName(selected) || 'обрана формула';
  const maj = computeMajoritySoilFormulaId(plants);
  const majName = maj?.id ? resolveName(maj.id) : null;

  /** @type {string[]} */
  const problemLines = [];
  for (const p of mismatches) {
    const rec = resolveName(String(p.soilFormulaId));
    problemLines.push(
      `«${p.name}» — у каталозі рекомендовано «${rec || 'іншу формулу'}».`,
    );
  }

  /** @type {{ entityId: string; label: string }[]} */
  const catalogLinks = [];
  const seen = new Set();
  const pushLink = (id) => {
    if (id == null || String(id).trim() === '') return;
    const sid = String(id);
    if (seen.has(sid)) return;
    seen.add(sid);
    catalogLinks.push({
      entityId: sid,
      label: resolveName(sid) || `Формула (${sid})`,
    });
  };

  pushLink(selected);
  for (const p of mismatches) pushLink(p.soilFormulaId);
  if (maj?.id) pushLink(maj.id);

  if (plants.length === 1) {
    problemLines.push(
      `Зараз на полотні обрано «${selectedName}». Якщо це не та формула, що в каталозі для цього виду, ріст може відрізнятися від очікуваного.`,
    );
    return { problemLines, catalogLinks };
  }

  if (maj?.id && majName) {
    if (String(maj.id) === selected) {
      problemLines.push(
        `Зараз обрано «${selectedName}» — це найчастіша рекомендація серед рослин на полотні. Для перелічених вище видів у каталозі вказано іншу формулу; це не заборона, але варто врахувати.`,
      );
    } else {
      problemLines.push(
        `Зараз обрано «${selectedName}». Найчастіше рекомендована серед усіх рослин на полотні — «${majName}» (кнопка «Розрахувати формулу ґрунту» підбирає її за більшістю).`,
      );
    }
  } else if (maj?.id && plants.length > 1) {
    problemLines.push(
      `Зараз обрано «${selectedName}». Найчастіша рекомендація серед рослин на полотні може відрізнятися — кнопка «Розрахувати формулу ґрунту» підбирає формулу за більшістю видів.`,
    );
  }

  return { problemLines, catalogLinks };
}

/**
 * Short plain-language summary for UI (no weights or technical scales).
 * @param {{
 *   plants: Array<{ name?: string }>;
 *   ecology: { severity: Severity; conflictKind: 'carn_dry' | 'carn_trop' | 'dry_trop' | null; lines: string[] };
 *   careSeverity: Severity;
 *   soilIssue: boolean;
 *   soilMismatch: boolean;
 *   sevL: Severity;
 *   sevW: Severity;
 *   sevH: Severity;
 * }} ctx
 * @returns {{ title: string; problems: string[]; whatToDo: string[]; level: 'ok' | 'caution' | 'risk' | 'neutral' }}
 */
function buildUserSummary(ctx) {
  const {
    plants,
    ecology,
    careSeverity,
    soilIssue,
    soilMismatch,
    sevL,
    sevW,
    sevH,
    soilMismatchNarrative,
  } = ctx;
  /** @type {string[]} */
  const problems = [];
  /** @type {string[]} */
  const whatToDo = [];

  if (plants.length === 0) {
    return { title: '', problems: [], whatToDo: [], level: 'neutral' };
  }

  if (plants.length === 1) {
    if (soilIssue) {
      if (soilMismatchNarrative?.problemLines?.length) {
        problems.push(...soilMismatchNarrative.problemLines);
      } else if (soilMismatch) {
        problems.push(
          'Для цієї рослини в каталозі інша рекомендована формула ґрунту, ніж та, що зараз обрана.',
        );
      } else {
        problems.push(
          'Ще не обрано формулу ґрунту — не перевірено, чи субстрат підходить цій рослині.',
        );
      }
      if (soilMismatch) {
        whatToDo.push(
          'Можна зберегти чернетку та згенерувати зображення. Відмінність від картки рослини в каталозі — лише довідка; змінювати формулу не обов’язково.',
        );
      } else {
        whatToDo.push(
          'Оберіть формулу ґрунту в каталозі (клік по картці) або скористайтеся кнопкою «Розрахувати формулу ґрунту», якщо вона з’явилась.',
        );
      }
      return {
        title: 'Потрібно обрати ґрунт',
        problems,
        whatToDo,
        level: 'caution',
      };
    }
    return {
      title: 'Одна рослина — порівняння між видами ще не потрібне.',
      problems: [],
      whatToDo: [],
      level: 'neutral',
    };
  }

  if (careSeverity === 'strong') {
    if (ecology.conflictKind) {
      for (const line of ecology.lines) problems.push(line);
      whatToDo.push(
        'Приберіть зайві види або замініть їх на близькі за типом. Наприклад, не тримайте разом хижі й сухолюбні рослини в одній посудині без окремих зон.',
      );
    } else {
      const parts = [];
      if (sevL === 'strong') parts.push('світло');
      if (sevW === 'strong') parts.push('полив');
      if (sevH === 'strong') parts.push('вологість повітря');
      problems.push(
        `Між рослинами дуже великий розрив за ${parts.join(', ')}. У спільній посудині без окремих зон такий мікс зазвичай шкодить частині рослин.`,
      );
      whatToDo.push('Залиште види з близькими потребами або сплануйте окремі зони в композиції.');
    }

    if (soilIssue) {
      if (soilMismatch) {
        if (soilMismatchNarrative?.problemLines?.length) {
          problems.push(...soilMismatchNarrative.problemLines);
        } else {
          problems.push('Обрана формула ґрунту не збігається з рекомендаціями каталогу для частини рослин.');
        }
        whatToDo.push(
          'Формула вже на полотні, але для частини видів у каталозі інші рекомендації. Спочатку узгодьте склад рослин (або зонування), за потреби змініть формулу у вкладці «Формули ґрунту».',
        );
      } else {
        whatToDo.push(
          'Коли набір стане сумісним, оберіть або розрахуйте формулу ґрунту — тоді можна буде зберегти проєкт.',
        );
      }
    }

    return {
      title: 'Цей набір рослин не варто тримати разом в одній посудині',
      problems: dedupeStrings(problems),
      whatToDo: dedupeStrings(whatToDo),
      level: 'risk',
    };
  }

  if (careSeverity === 'mild') {
    problems.push('Є помітні відмінності між рослинами в освітленні, поливі або вологості повітря.');
    whatToDo.push(
      'Орієнтуйтеся на «найвимогливіший» вид: полив і місце підлаштовуйте під нього або спростіть склад.',
    );
    if (soilIssue) {
      if (soilMismatchNarrative?.problemLines?.length) {
        problems.push(...soilMismatchNarrative.problemLines);
      } else if (soilMismatch) {
        problems.push('Для частини рослин у каталозі інша рекомендована формула ґрунту.');
      } else {
        problems.push('Формула ґрунту ще не обрана.');
      }
      if (soilMismatch) {
        whatToDo.push(
          'Збережіть проєкт і згенеруйте зображення за потреби. Різниця з каталогом по ґрунту — на ваш розсуд; формулу міняти не обов’язково.',
        );
      } else {
        whatToDo.push('Оберіть формулу ґрунту в каталозі або за підказкою над сіткою.');
      }
    }
    return {
      title: 'Можна спробувати, але є відмінності в догляді',
      problems: dedupeStrings(problems),
      whatToDo: dedupeStrings(whatToDo),
      level: 'caution',
    };
  }

  if (soilIssue) {
    if (soilMismatchNarrative?.problemLines?.length) {
      problems.push(...soilMismatchNarrative.problemLines);
    } else if (soilMismatch) {
      problems.push('Для частини рослин у каталозі інша рекомендована формула, ніж обрана зараз.');
    } else {
      problems.push(
        'Формула ґрунту ще не обрана — не перевірено, чи субстрат підходить усім рослинам.',
      );
    }
    if (soilMismatch) {
      whatToDo.push(
        'Усе обрано: збережіть чернетку та натисніть «Згенерувати зображення». Тексти про ґрунт вище — лише для довідки; змінювати формулу не потрібно.',
      );
    } else {
      whatToDo.push('Натисніть «Розрахувати формулу ґрунту» або оберіть формулу в каталозі.');
    }
    return {
      title: 'Рослини загалом підходять одна одній',
      problems: dedupeStrings(problems),
      whatToDo: dedupeStrings(whatToDo),
      level: 'caution',
    };
  }

  return {
    title: 'За каталогом цей мікс виглядає узгодженим',
    problems: [],
    whatToDo: [],
    level: 'ok',
  };
}

/**
 * @param {{
 *   plants: Array<{ name: string; entityId?: string; lightLevel: number; waterNeed: number; humidityLevel: number; soilFormulaId?: string | null; category?: string | null }>;
 *   selectedSoilFormulaId?: string | null;
 *   resolveSoilFormulaName?: (id: string) => string | null | undefined;
 * }} input
 */
export function analyzeFloraCompatibility(input) {
  const raw = input.plants ?? [];
  const plants = dedupePlants(raw);
  const selected = input.selectedSoilFormulaId != null && input.selectedSoilFormulaId !== ''
    ? String(input.selectedSoilFormulaId)
    : null;

  /** @type {string[]} */
  const careLines = [];

  const lightSpread = maxSpread(plants.map((p) => p.lightLevel));
  const waterSpread = maxSpread(plants.map((p) => p.waterNeed));
  const humiditySpread = maxSpread(plants.map((p) => p.humidityLevel));

  const sevL = spreadSeverity(lightSpread);
  const sevW = spreadSeverity(waterSpread);
  const sevH = spreadSeverity(humiditySpread);

  /** @type {Severity} */
  let numericCareSeverity = 'ok';
  numericCareSeverity = maxSeverity(numericCareSeverity, sevL);
  numericCareSeverity = maxSeverity(numericCareSeverity, sevW);
  numericCareSeverity = maxSeverity(numericCareSeverity, sevH);

  const ecology = analyzeEcologyByCategory(plants);
  const careSeverity = maxSeverity(numericCareSeverity, ecology.severity);

  if (plants.length >= 2) {
    // Always show numeric ranges so light / water / humidity are visible (not only when spread > 1).
    const lm = minMaxLevels(plants.map((p) => p.lightLevel));
    const wm = minMaxLevels(plants.map((p) => p.waterNeed));
    const hm = minMaxLevels(plants.map((p) => p.humidityLevel));
    if (lm) {
      careLines.push(
        `Світло (шкала 1–5): у вибраних видів рівні від ${lm.min} до ${lm.max}${lm.min === lm.max ? ' (однаково)' : ''}.`,
      );
    }
    if (wm) {
      careLines.push(
        `Полив (1–5): від ${wm.min} до ${wm.max}${wm.min === wm.max ? ' (однаково)' : ''}.`,
      );
    }
    if (hm) {
      careLines.push(
        `Вологість повітря (1–5): від ${hm.min} до ${hm.max}${hm.min === hm.max ? ' (однаково)' : ''}.`,
      );
    }

    for (const line of ecology.lines) {
      careLines.push(line);
    }

    if (sevL === 'ok' && sevW === 'ok' && sevH === 'ok' && ecology.severity === 'ok') {
      careLines.push(
        'По кожній шкалі окремо (світло, полив, вологість) розкид між видами в проєкті не більше ніж на 1 пункт — тобто порівнюємо лише світло зі світлом, полив з поливом тощо. На картці однієї рослини «сонце» і «краплі» — це різні шкали; велика різниця між ними не є помилкою. Зазвичай таке поєднання видів у одній посудині реалістичне, якщо полив підлаштувати під найсухіший або найвологіший вид.',
      );
    }

    if (sevL !== 'ok') {
      careLines.push(
        sevL === 'mild'
          ? `Світло: різниця між рослинами — ${lightSpread} рівні (помірно; часто ще можна поєднати з обережним розміщенням).`
          : `Світло: різниця — ${lightSpread} рівні; вимоги сильно різні — без зонування в одній посудині буде важко.`,
      );
    }
    if (sevW !== 'ok') {
      careLines.push(
        sevW === 'mild'
          ? `Полив: різниця — ${waterSpread} рівні (помірно; уточніть полив «на око» під найсухіший або найвологіший вид).`
          : `Полив: різниця — ${waterSpread} рівні; однаковий режим поливу для всіх може зашкодити частині рослин.`,
      );
    }
    if (sevH !== 'ok') {
      careLines.push(
        sevH === 'mild'
          ? `Вологість повітря: різниця — ${humiditySpread} рівні (помірна; закрита форма допоможе зрівняти мікроклімат).`
          : `Вологість повітря: різниця — ${humiditySpread} рівні; поєднання в одному обʼємі без перегородок ризиковане.`,
      );
    }

    if (lightSpread >= 2 && waterSpread >= 2) {
      careLines.push(
        'Одночасно помітно відрізняються і світло, і полив — якщо залишаєте всі разом, варто зонування або окремі «кармашки» субстрату.',
      );
    }
  }

  /** @type {string[]} */
  const soilLines = [];
  let soilIssue = false;

  if (plants.length > 0 && !selected) {
    soilIssue = true;
    soilLines.push('У проєкті не обрано формулу ґрунту — відповідність субстрату кожній рослині з каталогу не перевірена.');
  }

  let soilMismatch = false;
  /** @type {{ problemLines: string[]; catalogLinks?: { entityId: string; label: string }[] } | null} */
  let soilMismatchNarrative = null;
  if (selected && plants.length > 0) {
    const mismatches = plants.filter(
      (p) => p.soilFormulaId != null && String(p.soilFormulaId) !== selected,
    );
    if (mismatches.length > 0) {
      soilIssue = true;
      soilMismatch = true;
      soilMismatchNarrative = buildSoilMismatchNarrative(
        plants,
        selected,
        input.resolveSoilFormulaName,
      );
      if (soilMismatchNarrative?.problemLines?.length) {
        soilLines.push(...soilMismatchNarrative.problemLines);
      } else {
        const names = mismatches.map((p) => `«${p.name}»`).join(', ');
        soilLines.push(
          `Для ${names} у каталозі вказана інша рекомендована формула ґрунту, ніж обрана зараз у проєкті. Це не заборона, але ріст може відрізнятися від очікуваного.`,
        );
      }
    }
  }

  let soilSeverity = 'ok';
  if (soilIssue) soilSeverity = 'mild';

  const summarySeverity = maxSeverity(careSeverity, soilSeverity);

  const shouldPrompt =
    plants.length >= 2
      ? careSeverity !== 'ok' || soilIssue
      : plants.length === 1 && soilIssue;

  const spreadStrong = sevL === 'strong' || sevW === 'strong' || sevH === 'strong';
  const ecologyStrong = ecology.severity === 'strong';

  const pairingFactors = buildPairingFactors({
    plants,
    ecology,
    sevL,
    sevW,
    sevH,
    lightSpread,
    waterSpread,
    humiditySpread,
    soilIssue,
    soilMismatch,
  });
  const hasPairingFactors = pairingFactors.length > 0;

  const pairing = buildPairingVerdict(plants.length, careSeverity, soilIssue, {
    ecologyStrong,
    spreadStrong,
    hasPairingFactors,
  });

  const userSummary = buildUserSummary({
    plants,
    ecology,
    careSeverity,
    soilIssue,
    soilMismatch,
    sevL,
    sevW,
    sevH,
    soilMismatchNarrative,
  });

  return {
    summarySeverity,
    shouldPrompt,
    careSeverity,
    soilSeverity,
    /** True when plant mix is too conflicting to recommend a shared soil step (strong tier). */
    soilMixStepBlocked: careSeverity === 'strong',
    lightSpread,
    waterSpread,
    humiditySpread,
    careLines,
    soilLines,
    pairingVerdict: pairing.text,
    pairingVerdictLevel: pairing.level,
    pairingFactors,
    /** Plain-language blocks for constructor / PDF modal (prefer over pairingFactors + long careLines). */
    userSummary,
    /** Soil formula ids + labels for «jump to catalog» in constructor (when narrative is available). */
    soilCatalogLinks: soilMismatchNarrative?.catalogLinks ?? [],
  };
}
