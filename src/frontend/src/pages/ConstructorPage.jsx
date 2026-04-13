import { useEffect, useMemo, useRef, useState } from 'react';
import html2canvas from 'html2canvas';
import { Link, Navigate, useLocation, useNavigate } from 'react-router-dom';
import { useApi } from '../services/useApi';
import { useAuth } from '../services/useAuth';
import { ExportPdfButton } from '../components/ExportPdfButton';
import { analyzeFloraCompatibility } from '../utils/florariumCompatibility/analyzeFloraCompatibility';
import { normalizeProjectItemType } from '../utils/florariumPdf/normalizeProjectItemType';
import './ConstructorPage.css';

const GRID_PRESETS = [3, 5, 7, 9];
const TILE_WIDTH = 94;
const TILE_HEIGHT = 52;
/** Padding around footprint — delete control sits slightly outside the tile rect. */
const PLACED_ITEM_HOVER_PAD = 12;
const GRID_TOP_OFFSET = TILE_HEIGHT * 1.4;
const MAX_PAGE_SIZE = 25;
const MAX_UNDO_HISTORY = 50;
const BOARD_ZOOM_MIN = 0.5;
const BOARD_ZOOM_MAX = 2;
const BOARD_ZOOM_STEP = 0.1;
const SOIL_KINDS = new Set(['soilType', 'soilFormula']);

const CATALOG_TABS = [
  { key: 'plants', label: 'Рослини' },
  { key: 'soilFormulas', label: 'Формули ґрунту' },
  { key: 'decorations', label: 'Декор' },
  { key: 'containers', label: 'Контейнери' },
];

const SOIL_COLOR_GRAY = [126, 121, 110];
const SOIL_COLOR_YELLOW = [182, 154, 88];
const SOIL_COLOR_BROWN = [124, 88, 52];

function resolveLayer(kind) {
  return 'objects';
}

function hashString(value) {
  let hash = 2166136261;
  for (let index = 0; index < value.length; index += 1) {
    hash ^= value.charCodeAt(index);
    hash = Math.imul(hash, 16777619);
  }
  return hash >>> 0;
}

function mixChannel(start, end, amount) {
  return Math.round(start + (end - start) * amount);
}

function mixColor(first, second, amount) {
  return [
    mixChannel(first[0], second[0], amount),
    mixChannel(first[1], second[1], amount),
    mixChannel(first[2], second[2], amount),
  ];
}

function colorToRgb(color) {
  return `rgb(${color[0]}, ${color[1]}, ${color[2]})`;
}

function getSoilGradientByKey(key) {
  const ratio = hashString(key) / 4294967295;
  const base = ratio < 0.5
    ? mixColor(SOIL_COLOR_GRAY, SOIL_COLOR_YELLOW, ratio / 0.5)
    : mixColor(SOIL_COLOR_YELLOW, SOIL_COLOR_BROWN, (ratio - 0.5) / 0.5);

  const light = mixColor(base, [230, 220, 195], 0.26);
  const dark = mixColor(base, [82, 61, 35], 0.22);

  return {
    start: colorToRgb(light),
    end: colorToRgb(dark),
    border: colorToRgb(mixColor(base, [60, 44, 25], 0.28)),
  };
}

/**
 * Maps API ProjectItemSize (numeric enum value or name string) to isometric grid span.
 * Domain: Small=1x1, Medium=2x2, Large=3x3, ExtraLarge=4x4.
 */
function projectItemSizeToFootprint(itemMaxSize) {
  if (typeof itemMaxSize === 'number' && Number.isFinite(itemMaxSize)) {
    if (itemMaxSize >= 0 && itemMaxSize <= 3) return itemMaxSize + 1;
  }
  if (typeof itemMaxSize === 'string') {
    const trimmed = itemMaxSize.trim();
    const byName = {
      Small: 1,
      Medium: 2,
      Large: 3,
      ExtraLarge: 4,
    };
    if (byName[trimmed] !== undefined) return byName[trimmed];
    const n = Number(trimmed);
    if (Number.isFinite(n) && n >= 0 && n <= 3) return n + 1;
  }
  return 2;
}

function estimatePlantFootprint(plant) {
  return projectItemSizeToFootprint(plant.itemMaxSize);
}

function estimateContainerFootprint(container) {
  if (container.volume <= 2.5) return 2;
  if (container.volume <= 4) return 3;
  return 4;
}

function resolveImageUrl(imagePath) {
  if (!imagePath) return null;
  if (imagePath.startsWith('http://') || imagePath.startsWith('https://') || imagePath.startsWith('/')) {
    return imagePath;
  }
  return `/assets/images/${imagePath}`;
}

function normalizeLevel(level) {
  if (typeof level === 'number') {
    return Math.max(0, Math.min(5, level));
  }

  if (typeof level === 'string') {
    const trimmed = level.trim();
    const asNumber = Number(trimmed);
    if (Number.isFinite(asNumber)) {
      return Math.max(0, Math.min(5, asNumber));
    }

    const normalized = trimmed.toLowerCase();
    const levelMap = {
      verylow: 1,
      low: 2,
      medium: 3,
      high: 4,
      veryhigh: 5,
      very_high: 5,
      'very-high': 5,
    };

    return levelMap[normalized] ?? 0;
  }

  return 0;
}

function renderLevelIcons(level, icon, label, tone = 'default') {
  const safeLevel = normalizeLevel(level);
  return (
    <div className="constructor-level" aria-label={`${label}: ${safeLevel}/5`}>
      <span className="constructor-level-icons">
        {Array.from({ length: 5 }, (_, index) => (
          <span
            key={`${label}-${index}`}
            className={`constructor-level-icon constructor-level-icon-${tone} ${index < safeLevel ? 'constructor-level-icon-active' : ''}`}
            aria-hidden="true"
          >
            {icon}
          </span>
        ))}
      </span>
    </div>
  );
}

function toIsoPosition(row, col, originX, originY) {
  return {
    left: (col - row) * (TILE_WIDTH / 2) + originX,
    top: (col + row) * (TILE_HEIGHT / 2) + originY,
  };
}

function toGridCell(localX, localY, originX, originY) {
  const normalizedX = (localX - originX) / (TILE_WIDTH / 2);
  const normalizedY = (localY - originY) / (TILE_HEIGHT / 2);
  const col = Math.round((normalizedX + normalizedY) / 2);
  const row = Math.round((normalizedY - normalizedX) / 2);
  return { row, col };
}

function toPlacementCandidate(localX, localY, footprint, originX, originY) {
  const targetCell = toGridCell(localX, localY, originX, originY);
  const offset = Math.floor(footprint / 2);
  return {
    row: targetCell.row - offset,
    col: targetCell.col - offset,
    size: footprint,
    hoverRow: targetCell.row,
    hoverCol: targetCell.col,
  };
}

function rectanglesOverlap(first, second) {
  return (
    first.row < second.row + second.size
    && first.row + first.size > second.row
    && first.col < second.col + second.size
    && first.col + first.size > second.col
  );
}

/**
 * Resolve plant/decoration entity when restoring a saved project (API id or mock slug).
 * @returns {{ kind: 'plant'|'decoration'; entity: object } | null}
 */
function resolveProjectItemEntity(pi, plants, decorations) {
  const kind = normalizeProjectItemType(pi.itemType ?? pi.ItemType);
  if (kind !== 'plant' && kind !== 'decoration') return null;
  const itemId = pi.itemId ?? pi.ItemId;
  const catalogSource = kind === 'plant' ? plants : decorations;
  const entity = catalogSource.find((e) => String(e.id) === String(itemId))
    ?? catalogSource.find((e) => {
      const slug = String(itemId).toLowerCase().replace(/^(plant|deco)-/, '');
      const name = (e.name || '').toLowerCase();
      const latin = (e.nameLatin || '').toLowerCase();
      return name.includes(slug) || latin.includes(slug);
    });
  if (!entity) return null;
  return { kind, entity };
}

/**
 * Read grid coords from API (camelCase or PascalCase). Preserves 0 — do not use `|| 0`.
 */
function readProjectItemCoord(pi, axis) {
  const camel = axis === 'x' ? 'posX' : 'posY';
  const pascal = axis === 'x' ? 'PosX' : 'PosY';
  const raw = pi[camel] ?? pi[pascal];
  if (raw === undefined || raw === null || raw === '') return 0;
  const n = Number(raw);
  return Number.isFinite(n) ? Math.trunc(n) : 0;
}

function isCellInsideCandidate(cell, candidate) {
  if (!candidate) return false;
  return (
    cell.row >= candidate.row
    && cell.row < candidate.row + candidate.size
    && cell.col >= candidate.col
    && cell.col < candidate.col + candidate.size
  );
}

function ConstructorPage() {
  const api = useApi();
  const { isAuthenticated } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const fromProject = location.state?.fromProject ?? null;
  const resumeProjectId = location.state?.resumeProjectId ?? null;
  /** false = landing (new sketch vs gallery); true = full editor. Skip when restoring from gallery. */
  const [editorOpen, setEditorOpen] = useState(() => Boolean(fromProject || resumeProjectId));
  /** Avoid re-running gallery restore when catalog arrays get new references after user edits. */
  const restoredFromProjectIdRef = useRef(null);
  const boardRef = useRef(null);
  const boardWrapRef = useRef(null);
  const dragFootprintRef = useRef(1);
  const dragLayerRef = useRef(null);
  const panRef = useRef({ active: false, startX: 0, startY: 0, scrollLeft: 0, scrollTop: 0 });
  const suppressBoardClickRef = useRef(false);

  const [gridSize, setGridSize] = useState(5);
  const [activeTab, setActiveTab] = useState('plants');
  const [search, setSearch] = useState('');
  const [plantNameLanguage, setPlantNameLanguage] = useState('uk');
  const [selectedCatalogItem, setSelectedCatalogItem] = useState(null);

  const [plants, setPlants] = useState([]);
  const [soilTypes, setSoilTypes] = useState([]);
  const [soilFormulas, setSoilFormulas] = useState([]);
  const [decorations, setDecorations] = useState([]);
  const [containers, setContainers] = useState([]);

  const [placedItems, setPlacedItems] = useState([]);
  const [dragHoverCell, setDragHoverCell] = useState(null);
  const [notice, setNotice] = useState('');
  const [loading, setLoading] = useState(() => (isAuthenticated ? Boolean(fromProject || resumeProjectId) : false));
  const [error, setError] = useState('');
  const [isPanning, setIsPanning] = useState(false);
  /** Placed item selected by click — delete button shows only for this instance. */
  const [selectedPlacedItemId, setSelectedPlacedItemId] = useState(null);
  const [hideObjectsLayer, setHideObjectsLayer] = useState(false);
  /** Soil visual on grid / save only after user clicks «Застосувати» (recommended formula). */
  const [appliedSoilFormula, setAppliedSoilFormula] = useState(null);
  const [selectedContainer, setSelectedContainer] = useState(null);
  const [shouldRedirectToAuth, setShouldRedirectToAuth] = useState(false);
  const [savedProjectId, setSavedProjectId] = useState(null);
  /** Mirrors server: gallery lists only published; drafts live in profile. */
  const [savedProjectPublished, setSavedProjectPublished] = useState(false);
  const [saving, setSaving] = useState(false);
  const [publishing, setPublishing] = useState(false);
  const [preparingResult, setPreparingResult] = useState(false);
  const [boardZoom, setBoardZoom] = useState(1);
  /** Loaded when returning from /result via state.resumeProjectId */
  const [resumeProject, setResumeProject] = useState(null);
  const [noticeDismissed, setNoticeDismissed] = useState(false);
  const [galleryHintDismissed, setGalleryHintDismissed] = useState(false);
  const [compatPanelExpanded, setCompatPanelExpanded] = useState(true);
  const historyRef = useRef({ past: [], future: [] });
  const [, setHistoryTick] = useState(0);

  useEffect(() => {
    setNoticeDismissed(false);
  }, [notice]);

  useEffect(() => {
    if (isAuthenticated) {
      setShouldRedirectToAuth(false);
      return undefined;
    }

    const timerId = window.setTimeout(() => {
      setShouldRedirectToAuth(true);
    }, 5000);

    return () => {
      window.clearTimeout(timerId);
    };
  }, [isAuthenticated]);

  // Open editor when navigating here with a project (e.g. «Відтворити» from gallery or resume from result page).
  useEffect(() => {
    if (fromProject || resumeProjectId) setEditorOpen(true);
  }, [fromProject, resumeProjectId]);

  useEffect(() => {
    if (!resumeProjectId || !isAuthenticated) {
      setResumeProject(null);
      return undefined;
    }
    let cancelled = false;
    (async () => {
      try {
        const project = await api.projects.get(resumeProjectId);
        if (!cancelled) setResumeProject(project);
      } catch {
        if (!cancelled) {
          setResumeProject(null);
          setNotice('Не вдалося завантажити проєкт для редагування.');
        }
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [resumeProjectId, isAuthenticated, api.projects]);

  const projectForRestore = fromProject ?? resumeProject;

  useEffect(() => {
    function onEscape(event) {
      if (event.key === 'Escape') setSelectedPlacedItemId(null);
    }
    window.addEventListener('keydown', onEscape);
    return () => window.removeEventListener('keydown', onEscape);
  }, []);

  useEffect(() => {
    if (!isAuthenticated) {
      setLoading(false);
      return undefined;
    }
    if (!editorOpen) {
      setLoading(false);
      return undefined;
    }

    let active = true;

    async function fetchAllPages(getAll) {
      let page = 1;
      let total = Number.POSITIVE_INFINITY;
      const allItems = [];

      while (allItems.length < total) {
        const response = await getAll({ page, pageSize: MAX_PAGE_SIZE });
        const items = response?.items || [];
        const safeTotal = Number.isFinite(response?.total) ? response.total : items.length;
        total = safeTotal;

        allItems.push(...items);

        if (items.length < MAX_PAGE_SIZE) break;
        page += 1;
      }

      return allItems;
    }

    async function loadCatalog() {
      setLoading(true);
      setError('');

      try {
        const [plantsItems, soilTypesItems, soilFormulasItems, decorationsItems, containersItems] = await Promise.all([
          fetchAllPages(api.plants.getAll),
          fetchAllPages(api.soil.types.getAll),
          fetchAllPages(api.soil.formulas.getAll),
          fetchAllPages(api.decorations.getAll),
          fetchAllPages(api.containers.getAll),
        ]);

        if (!active) return;

        setPlants(plantsItems);
        setSoilTypes(soilTypesItems);
        setSoilFormulas(soilFormulasItems);
        setDecorations(decorationsItems);
        setContainers(containersItems);
      } catch {
        if (!active) return;
        setError('Не вдалося завантажити елементи конструктора.');
      } finally {
        if (active) setLoading(false);
      }
    }

    loadCatalog();

    return () => {
      active = false;
    };
  }, [api, isAuthenticated, editorOpen]);

  // Restore project from gallery "Відтворити" or /result "Повернутися до конструктора"
  useEffect(() => {
    if (loading || !projectForRestore?.id) {
      if (!projectForRestore) restoredFromProjectIdRef.current = null;
      return;
    }

    const projectKey = String(projectForRestore.id);
    if (restoredFromProjectIdRef.current === projectKey) return;

    const projectItems = projectForRestore.items ?? [];
    if (projectItems.length === 0) return;

    const itemTypeOf = (pi) => pi.itemType ?? pi.ItemType;
    const hasPlantItems = projectItems.some((pi) => normalizeProjectItemType(itemTypeOf(pi)) === 'plant');
    const hasDecoItems = projectItems.some((pi) => normalizeProjectItemType(itemTypeOf(pi)) === 'decoration');
    const hasSoilItem = projectItems.some((pi) => normalizeProjectItemType(itemTypeOf(pi)) === 'soil');
    if ((hasPlantItems && plants.length === 0) || (hasDecoItems && decorations.length === 0)) {
      return;
    }
    if (hasSoilItem && soilFormulas.length === 0) {
      return;
    }
    if (projectForRestore.containerId && containers.length === 0) {
      return;
    }

    let maxExtent = 0;
    const restored = projectItems.flatMap((pi) => {
      const resolved = resolveProjectItemEntity(pi, plants, decorations);
      if (!resolved) return [];

      const { kind, entity } = resolved;
      const footprint = kind === 'plant'
        ? estimatePlantFootprint(entity)
        : projectItemSizeToFootprint(entity.itemMaxSize);
      const r = readProjectItemCoord(pi, 'x');
      const c = readProjectItemCoord(pi, 'y');
      const itemId = pi.itemId ?? pi.ItemId;
      maxExtent = Math.max(maxExtent, r + footprint - 1, c + footprint - 1);

      return [{
        instanceId: `restored-${pi.id ?? pi.Id ?? itemId}-${r}-${c}`,
        type: kind,
        entityId: String(entity.id),
        name: entity.name,
        subtitle: '',
        size: footprint,
        image: resolveImageUrl(entity.imageIsometricUrl || entity.imageUrl),
        layer: 'objects',
        row: r,
        col: c,
      }];
    });

    const neededSize = GRID_PRESETS.find((s) => s > maxExtent) ?? GRID_PRESETS[GRID_PRESETS.length - 1];
    setGridSize(neededSize);

    if (projectForRestore.containerId) {
      const restoredContainer = containers.find((c) => String(c.id) === String(projectForRestore.containerId));
      if (restoredContainer) {
        setSelectedContainer({
          id: `container-${restoredContainer.id}`,
          entityId: String(restoredContainer.id),
          kind: 'container',
          name: restoredContainer.name,
          subtitle: `${restoredContainer.volume} л • ${restoredContainer.isClosed ? 'Закритий' : 'Відкритий'}`,
          details: restoredContainer.description || 'Основа композиції',
          searchText: `${restoredContainer.name || ''} ${restoredContainer.description || ''}`,
          footprint: estimateContainerFootprint(restoredContainer),
          image: resolveImageUrl(restoredContainer.imageIsometricUrl || restoredContainer.imageUrl),
          layer: resolveLayer('container'),
        });
      }
    }

    const soilPi = projectItems.find((pi) => normalizeProjectItemType(itemTypeOf(pi)) === 'soil');
    let soilResolved = false;
    if (soilPi) {
      const soilId = soilPi.itemId ?? soilPi.ItemId;
      const rawFormula = soilFormulas.find((sf) => String(sf.id) === String(soilId));
      if (rawFormula) {
        soilResolved = true;
        setAppliedSoilFormula({
          id: `soilFormula-${rawFormula.id}`,
          entityId: String(rawFormula.id),
          kind: 'soilFormula',
          name: rawFormula.name,
          subtitle: `${rawFormula.items?.length || 0} компонентів`,
          details: 'Готовий мікс для флораріуму',
          searchText: `${rawFormula.name || ''} ${rawFormula.items?.length || 0} Готовий мікс для флораріуму`,
          footprint: 2,
          image: null,
          layer: resolveLayer('soilFormula'),
          rawItem: rawFormula,
        });
      }
    }

    setPlacedItems(restored);
    restoredFromProjectIdRef.current = projectKey;
    setSavedProjectId(String(projectForRestore.id));
    setSavedProjectPublished(Boolean(projectForRestore.isPublished));

    const accounted = restored.length + (soilResolved ? 1 : 0);
    const skipped = projectItems.length - accounted;
    setNotice(
      skipped > 0
        ? `Відтворено ${accounted} з ${projectItems.length} елементів. ${skipped} не знайдено в каталозі або без формули ґрунту. Позиції збережено з проєкту.`
        : `Проєкт відтворено (${projectItems.length} елементів, позиції як при збереженні).`,
    );
  }, [
    loading,
    projectForRestore,
    plants,
    decorations,
    containers,
    soilFormulas,
  ]);

  const catalogItemsByType = useMemo(() => {
    return {
      plants: plants.map((item) => ({
        id: `plant-${item.id}`,
        entityId: String(item.id),
        kind: 'plant',
        name: plantNameLanguage === 'latin'
          ? (item.nameLatin || item.name)
          : item.name,
        subtitle: '',
        details: '',
        lightLevel: item.lightLevel,
        waterNeed: item.waterNeed,
        searchText: `${item.name || ''} ${item.nameLatin || ''}`,
        footprint: estimatePlantFootprint(item),
        image: resolveImageUrl(item.imageIsometricUrl || item.imageUrl),
        layer: resolveLayer('plant'),
      })),
      soilTypes: soilTypes.map((item) => ({
        id: `soilType-${item.id}`,
        entityId: String(item.id),
        kind: 'soilType',
        name: item.name,
        subtitle: 'Базовий тип ґрунту',
        details: 'Рекомендований нижній шар',
        searchText: `${item.name || ''} Базовий тип ґрунту Рекомендований нижній шар`,
        footprint: projectItemSizeToFootprint(item.itemMaxSize),
        image: null,
        layer: resolveLayer('soilType'),
      })),
      soilFormulas: soilFormulas.map((item) => ({
        id: `soilFormula-${item.id}`,
        entityId: String(item.id),
        kind: 'soilFormula',
        name: item.name,
        subtitle: `${item.items?.length || 0} компонентів`,
        details: 'Готовий мікс для флораріуму',
        searchText: `${item.name || ''} ${item.items?.length || 0} Готовий мікс для флораріуму`,
        footprint: 2,
        image: null,
        layer: resolveLayer('soilFormula'),
        rawItem: item, // keep raw item for layers
      })),
      decorations: decorations.map((item) => ({
        id: `decoration-${item.id}`,
        entityId: String(item.id),
        kind: 'decoration',
        name: item.name,
        subtitle: `Категорія #${item.category}`,
        details: item.description || 'Декоративний елемент',
        searchText: `${item.name || ''} ${item.description || ''}`,
        footprint: 1,
        image: resolveImageUrl(item.imageIsometricUrl || item.imageUrl),
        layer: resolveLayer('decoration'),
      })),
      containers: containers.map((item) => ({
        id: `container-${item.id}`,
        entityId: String(item.id),
        kind: 'container',
        name: item.name,
        subtitle: `${item.volume} л • ${item.isClosed ? 'Закритий' : 'Відкритий'}`,
        details: item.description || 'Основа композиції',
        searchText: `${item.name || ''} ${item.description || ''}`,
        footprint: estimateContainerFootprint(item),
        image: resolveImageUrl(item.imageIsometricUrl || item.imageUrl),
        layer: resolveLayer('container'),
      })),
    };
  }, [containers, decorations, plantNameLanguage, plants, soilFormulas, soilTypes]);

  const visibleCatalogItems = useMemo(() => {
    const items = catalogItemsByType[activeTab] || [];
    const query = search.trim().toLowerCase();

    if (!query) return items;

    return items.filter((item) => {
      return (
        item.name.toLowerCase().includes(query)
        || (item.subtitle || '').toLowerCase().includes(query)
        || (item.details || '').toLowerCase().includes(query)
        || (item.searchText || '').toLowerCase().includes(query)
      );
    });
  }, [activeTab, catalogItemsByType, search]);

  const gridCells = useMemo(() => {
    const originX = (gridSize * TILE_WIDTH) / 2;
    const originY = GRID_TOP_OFFSET;
    return Array.from({ length: gridSize * gridSize }, (_, index) => {
      const row = Math.floor(index / gridSize);
      const col = index % gridSize;
      return { row, col, ...toIsoPosition(row, col, originX, originY) };
    });
  }, [gridSize]);

  const boardWidth = gridSize * TILE_WIDTH + TILE_WIDTH * 2;
  const boardHeight = gridSize * TILE_HEIGHT + TILE_HEIGHT * 5;

  useEffect(() => {
    const wrap = boardWrapRef.current;
    if (!wrap) return;

    const frame = window.requestAnimationFrame(() => {
      wrap.scrollLeft = Math.max(0, (wrap.scrollWidth - wrap.clientWidth) / 2);
      wrap.scrollTop = Math.max(0, (wrap.scrollHeight - wrap.clientHeight) / 2);
    });

    return () => window.cancelAnimationFrame(frame);
  }, [boardHeight, boardWidth]);

  const placedItemsView = useMemo(() => {
    const originX = (gridSize * TILE_WIDTH) / 2;
    const originY = GRID_TOP_OFFSET;
    const layerRank = { soil: 0, objects: 1 };
    return [...placedItems]
      .sort((a, b) => {
        const rankDelta = (layerRank[a.layer] || 0) - (layerRank[b.layer] || 0);
        if (rankDelta !== 0) return rankDelta;
        return (a.row + a.col) - (b.row + b.col);
      })
      .map((item) => {
        const anchor = toIsoPosition(item.row, item.col, originX, originY);
        return {
          ...item,
          // In isometric projection, expanding equally in row+col keeps X the same.
          // Only Y shifts to keep the visual footprint centered on logical occupied cells.
          left: anchor.left,
          top: anchor.top + ((item.size - 1) * TILE_HEIGHT) / 2,
        };
      });
  }, [gridSize, placedItems]);

  const visiblePlacedItemsView = useMemo(() => {
    if (!hideObjectsLayer) return placedItemsView;
    return placedItemsView.filter((item) => item.layer !== 'objects');
  }, [hideObjectsLayer, placedItemsView]);

  const floraCompatibility = useMemo(() => {
    const plantPlaced = placedItems.filter((i) => i.type === 'plant');
    if (plantPlaced.length === 0) return null;
    const plantPayloads = [];
    for (const inst of plantPlaced) {
      const p = plants.find((pl) => String(pl.id) === String(inst.entityId));
      if (!p) continue;
      plantPayloads.push({
        name: inst.name,
        entityId: String(p.id),
        lightLevel: p.lightLevel,
        waterNeed: p.waterNeed,
        humidityLevel: p.humidityLevel,
        soilFormulaId: p.soilFormulaId != null ? String(p.soilFormulaId) : null,
        category: p.category ?? null,
      });
    }
    if (plantPayloads.length === 0) return null;
    return analyzeFloraCompatibility({
      plants: plantPayloads,
      selectedSoilFormulaId: appliedSoilFormula?.entityId != null ? String(appliedSoilFormula.entityId) : null,
      resolveSoilFormulaName: (id) => {
        const f = soilFormulas.find((sf) => String(sf.id) === String(id));
        return f?.name ?? null;
      },
    });
  }, [placedItems, plants, appliedSoilFormula, soilFormulas]);

  const placedPlantItems = useMemo(() => placedItems.filter((i) => i.type === 'plant'), [placedItems]);

  const soilMixStepBlocked = Boolean(floraCompatibility?.soilMixStepBlocked);

  // Plants require a compatible mix and an applied soil formula before save.
  const saveBlockedByPlantsAndSoil =
    placedPlantItems.length > 0 && (soilMixStepBlocked || !appliedSoilFormula);

  const proposedSoilFormula = useMemo(() => {
    if (placedPlantItems.length === 0) return null;
    const ids = [];
    for (const inst of placedPlantItems) {
      const p = plants.find((pl) => String(pl.id) === String(inst.entityId));
      if (p?.soilFormulaId) ids.push(String(p.soilFormulaId));
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
    const formulas = catalogItemsByType.soilFormulas || [];
    return formulas.find((f) => String(f.entityId) === String(bestId)) ?? null;
  }, [placedPlantItems, plants, catalogItemsByType.soilFormulas]);

  function captureWorkspaceSnapshot() {
    return {
      placedItems: JSON.parse(JSON.stringify(placedItems)),
      appliedSoilEntityId: appliedSoilFormula?.entityId ?? null,
      selectedContainerEntityId: selectedContainer?.entityId ?? null,
      gridSize,
    };
  }

  function resolveWorkspaceSnapshot(snapshot) {
    const soil = snapshot.appliedSoilEntityId
      ? catalogItemsByType.soilFormulas.find((f) => String(f.entityId) === String(snapshot.appliedSoilEntityId))
      : null;
    const cont = snapshot.selectedContainerEntityId
      ? catalogItemsByType.containers.find((c) => String(c.entityId) === String(snapshot.selectedContainerEntityId))
      : null;
    return {
      placedItems: snapshot.placedItems,
      appliedSoilFormula: soil ?? null,
      selectedContainer: cont ?? null,
      gridSize: snapshot.gridSize,
    };
  }

  function commitHistoryBeforeMutation() {
    const snap = captureWorkspaceSnapshot();
    historyRef.current.past.push(snap);
    if (historyRef.current.past.length > MAX_UNDO_HISTORY) {
      historyRef.current.past.shift();
    }
    historyRef.current.future = [];
    setHistoryTick((t) => t + 1);
  }

  function applyWorkspaceSnapshot(snapshot) {
    const resolved = resolveWorkspaceSnapshot(snapshot);
    setPlacedItems(resolved.placedItems);
    setAppliedSoilFormula(resolved.appliedSoilFormula);
    setSelectedContainer(resolved.selectedContainer);
    setGridSize(resolved.gridSize);
    setSelectedPlacedItemId(null);
    setNotice('');
  }

  function undoWorkspace() {
    const { past, future } = historyRef.current;
    if (past.length === 0) return;
    const current = captureWorkspaceSnapshot();
    const previous = past.pop();
    future.unshift(current);
    applyWorkspaceSnapshot(previous);
    setHistoryTick((t) => t + 1);
  }

  function redoWorkspace() {
    const { past, future } = historyRef.current;
    if (future.length === 0) return;
    const current = captureWorkspaceSnapshot();
    const next = future.shift();
    past.push(current);
    applyWorkspaceSnapshot(next);
    setHistoryTick((t) => t + 1);
  }

  function focusCatalogSoilFormula(entityId, label) {
    setActiveTab('soilFormulas');
    setSelectedCatalogItem(null);
    setSearch('');
    const id = String(entityId);
    window.requestAnimationFrame(() => {
      const el = document.querySelector(`[data-catalog-entity-id="${id}"]`);
      if (el) {
        el.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        return;
      }
      if (label) {
        setSearch(label);
        window.requestAnimationFrame(() => {
          document.querySelector(`[data-catalog-entity-id="${id}"]`)?.scrollIntoView({
            behavior: 'smooth',
            block: 'nearest',
          });
        });
      }
    });
  }

  function handleApplyRecommendedSoil() {
    if (!proposedSoilFormula) return;
    commitHistoryBeforeMutation();
    setAppliedSoilFormula(proposedSoilFormula);
    setNotice('Формулу ґрунту розраховано та застосовано — шари на сітці оновлено.');
  }

  const globalSoilColor = useMemo(() => {
    if (!appliedSoilFormula) return null;

    const items = appliedSoilFormula.rawItem?.items || [];
    if (items.length === 0) return null;

    const topLayer = [...items].sort((a, b) => a.order - b.order)[0];
    const hexColor = topLayer.soilType?.hexColor;

    if (hexColor) {
      return {
         start: hexColor,
         end: hexColor,
         border: '#00000044',
      };
    }

    return getSoilGradientByKey(`soilFormula:${appliedSoilFormula.entityId}:${appliedSoilFormula.name}`);
  }, [appliedSoilFormula]);

  const sortedSoilLayers = useMemo(() => {
    if (!appliedSoilFormula) return [];
    return [...(appliedSoilFormula.rawItem?.items || [])].sort((a, b) => a.order - b.order);
  }, [appliedSoilFormula]);

  function canPlaceItem(candidate, layer, excludingId = null) {
    if (candidate.row < 0 || candidate.col < 0) return false;
    if (candidate.row + candidate.size > gridSize) return false;
    if (candidate.col + candidate.size > gridSize) return false;

    return !placedItems.some((item) => {
      if (item.instanceId === excludingId) return false;
      if (item.layer !== layer) return false;
      return rectanglesOverlap(candidate, item);
    });
  }

  function tryMoveSelectedPlacedItem(deltaRow, deltaCol) {
    if (!selectedPlacedItemId) return;
    const item = placedItems.find((i) => i.instanceId === selectedPlacedItemId);
    if (!item) return;
    const candidate = {
      row: item.row + deltaRow,
      col: item.col + deltaCol,
      size: item.size,
    };
    if (!canPlaceItem(candidate, item.layer, item.instanceId)) {
      setNotice('Не можна перемістити: межі сітки або клітинка зайнята.');
      return;
    }
    commitHistoryBeforeMutation();
    setPlacedItems((prev) => prev.map((it) => (
      it.instanceId === item.instanceId ? { ...it, row: candidate.row, col: candidate.col } : it
    )));
    setNotice('');
  }

  function handleDragStart(event, item) {
    event.dataTransfer.effectAllowed = 'copy';
    event.dataTransfer.setData('application/prickle-item', JSON.stringify(item));
    event.dataTransfer.setData('text/plain', item.name);
    dragFootprintRef.current = Math.max(1, Number(item.footprint) || 1);
    dragLayerRef.current = item.layer || null;
    setNotice('');
    setDragHoverCell(null);
  }

  function handleDragOver(event) {
    event.preventDefault();
    if (!boardRef.current) return;

    const rect = boardRef.current.getBoundingClientRect();
    const localX = event.clientX - rect.left;
    const localY = event.clientY - rect.top;
    const moveRaw = event.dataTransfer.getData('application/prickle-move');
    const itemRaw = event.dataTransfer.getData('application/prickle-item');
    let footprint = Math.max(1, Number(dragFootprintRef.current) || 1);

    let layer = dragLayerRef.current || selectedCatalogItem?.layer || null;
    let excludingId = null;

    if (moveRaw) {
      try {
        const movePayload = JSON.parse(moveRaw);
        footprint = Math.max(1, Number(movePayload?.size) || 1);
        // Ignore soil type dragging
        if (movePayload.layer === 'soil') return;

        layer = movePayload.layer || layer;
        excludingId = movePayload?.instanceId || null;
      } catch {
        footprint = 1;
      }
    } else if (itemRaw) {
      try {
        const itemPayload = JSON.parse(itemRaw);
        footprint = Math.max(1, Number(itemPayload?.footprint) || 1);
        layer = itemPayload?.layer || layer;
      } catch {
        footprint = 1;
      }
    }

    const candidate = toPlacementCandidate(
      localX,
      localY,
      footprint,
      (gridSize * TILE_WIDTH) / 2,
      GRID_TOP_OFFSET,
    );

    setDragHoverCell({
      ...candidate,
      layer,
      excludingId,
    });
  }

  function handlePlacedDragStart(event, item) {
    event.stopPropagation();
    setNotice('');
    setDragHoverCell(null);
    event.dataTransfer.effectAllowed = 'move';
    event.dataTransfer.setData('application/prickle-move', JSON.stringify({
      instanceId: item.instanceId,
      size: item.size,
      layer: item.layer,
    }));
    event.dataTransfer.setData('text/plain', item.name);
    dragFootprintRef.current = Math.max(1, Number(item.size) || 1);
    dragLayerRef.current = item.layer || null;
  }

  function placeItem(payload, candidate, excludingId = null) {
    if (!canPlaceItem(candidate, payload.layer, excludingId)) {
      return false;
    }

    commitHistoryBeforeMutation();

    if (excludingId) {
      setPlacedItems((prev) => prev.map((item) => {
        if (item.instanceId !== excludingId) return item;
        return {
          ...item,
          row: candidate.row,
          col: candidate.col,
        };
      }));
      return true;
    }

    const nextItem = {
      instanceId: `${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
      type: payload.kind,
      entityId: payload.entityId,
      name: payload.name,
      subtitle: payload.subtitle,
      size: payload.footprint,
      image: payload.image,
      layer: payload.layer,
      row: candidate.row,
      col: candidate.col,
    };

    setPlacedItems((prev) => [...prev, nextItem]);
    return true;
  }

  function handleBoardClick(event) {
    if (!boardRef.current) return;
    if (suppressBoardClickRef.current) {
      suppressBoardClickRef.current = false;
      return;
    }

    if (!event.target.closest('.placed-item-hover-wrap')) {
      setSelectedPlacedItemId(null);
    }

    if (!selectedCatalogItem) return;

    if (event.target.closest('.placed-item-hover-wrap') || event.target.closest('.placed-item')) {
      return;
    }

    if (selectedCatalogItem.kind === 'soilFormula') {
      setNotice('Ґрунт на сітці задається кнопкою «Застосувати» після узгодження рослин, або з каталогу після першого застосування.');
      return;
    }

    if (selectedCatalogItem.kind === 'container') {
      setSelectedContainer(selectedCatalogItem);
      setNotice(`Контейнер обрано: ${selectedCatalogItem.name}`);
      return;
    }

    const rect = boardRef.current.getBoundingClientRect();
    const localX = event.clientX - rect.left;
    const localY = event.clientY - rect.top;
    const candidate = toPlacementCandidate(
      localX,
      localY,
      selectedCatalogItem.footprint,
      (gridSize * TILE_WIDTH) / 2,
      GRID_TOP_OFFSET,
    );

    const placed = placeItem(selectedCatalogItem, candidate);
    if (!placed) {
      setNotice(`Шар ${selectedCatalogItem.layer === 'soil' ? 'ґрунту' : "об'єктів"} зайнятий або вихід за межі.`);
      return;
    }

    setNotice('');
  }

  function handleDrop(event) {
    event.preventDefault();
    setNotice('');
    setDragHoverCell(null);
    dragFootprintRef.current = 1;
    dragLayerRef.current = null;

    const rect = boardRef.current?.getBoundingClientRect();
    if (!rect) return;
    const localX = event.clientX - rect.left;
    const localY = event.clientY - rect.top;

    const moveRaw = event.dataTransfer.getData('application/prickle-move');
    if (moveRaw) {
      let movePayload;
      try {
        movePayload = JSON.parse(moveRaw);
      } catch {
        return;
      }

      const candidate = toPlacementCandidate(
        localX,
        localY,
        movePayload.size,
        (gridSize * TILE_WIDTH) / 2,
        GRID_TOP_OFFSET,
      );

      if (!canPlaceItem(candidate, movePayload.layer, movePayload.instanceId)) {
        setNotice('Не можна перемістити сюди: вихід за межі сітки або перетин з іншим елементом.');
        return;
      }
      placeItem({ layer: movePayload.layer }, candidate, movePayload.instanceId);

      setDragHoverCell(null);
      suppressBoardClickRef.current = true;
      return;
    }

    const raw = event.dataTransfer.getData('application/prickle-item');
    if (!raw || !boardRef.current) return;

    let payload;
    try {
      payload = JSON.parse(raw);
    } catch {
      return;
    }

    if (payload.layer === 'soil') return;
    if (payload.kind === 'container') return;

    const candidate = toPlacementCandidate(
      localX,
      localY,
      payload.footprint,
      (gridSize * TILE_WIDTH) / 2,
      GRID_TOP_OFFSET,
    );

    if (!canPlaceItem(candidate, payload.layer)) {
      setNotice(`Шар ${payload.layer === 'soil' ? 'ґрунту' : "об'єктів"} зайнятий або вихід за межі.`);
      return;
    }

    placeItem(payload, candidate);
    setDragHoverCell(null);
    suppressBoardClickRef.current = true;
  }

  function handleDragLeaveBoard(event) {
    if (!boardRef.current?.contains(event.relatedTarget)) {
      setDragHoverCell(null);
      dragFootprintRef.current = 1;
      dragLayerRef.current = null;
    }
  }

  function handlePanStart(event) {
    if (!boardWrapRef.current) return;
    if (event.button !== 0) return;
    if (event.target.closest('.placed-item-remove')) return;
    if (event.target.closest('.placed-item-hover-wrap') || event.target.closest('.placed-item')) return;

    panRef.current = {
      active: true,
      startX: event.clientX,
      startY: event.clientY,
      scrollLeft: boardWrapRef.current.scrollLeft,
      scrollTop: boardWrapRef.current.scrollTop,
    };
    setIsPanning(true);
  }

  function handlePanMove(event) {
    if (!panRef.current.active || !boardWrapRef.current) return;

    const deltaX = event.clientX - panRef.current.startX;
    const deltaY = event.clientY - panRef.current.startY;

    boardWrapRef.current.scrollLeft = panRef.current.scrollLeft - deltaX;
    boardWrapRef.current.scrollTop = panRef.current.scrollTop - deltaY;

    if (Math.abs(deltaX) > 2 || Math.abs(deltaY) > 2) {
      suppressBoardClickRef.current = true;
    }
  }

  function stopPan() {
    if (!panRef.current.active) return;
    panRef.current.active = false;
    setIsPanning(false);
  }

  function removePlacedItem(instanceId) {
    commitHistoryBeforeMutation();
    setPlacedItems((prev) => prev.filter((item) => item.instanceId !== instanceId));
    setSelectedPlacedItemId((prev) => (prev === instanceId ? null : prev));
  }

  function resetWorkspace() {
    commitHistoryBeforeMutation();
    setPlacedItems([]);
    setAppliedSoilFormula(null);
    setNotice('');
    setSavedProjectId(null);
    setSavedProjectPublished(false);
    setSelectedPlacedItemId(null);
  }

  async function loadImageForSnapshot(src) {
    return await new Promise((resolve) => {
      if (!src) {
        resolve(null);
        return;
      }

      const image = new Image();
      image.crossOrigin = 'anonymous';
      image.onload = () => resolve(image);
      image.onerror = () => resolve(null);
      image.src = src;
    });
  }

  async function renderConstructorSnapshotManually() {
    const width = boardWidth;
    const soilDepth = sortedSoilLayers.length > 0 ? 80 : 0;
    const height = boardHeight + soilDepth;
    const scale = window.devicePixelRatio > 1 ? 2 : 1;

    const canvas = document.createElement('canvas');
    canvas.width = Math.max(1, Math.floor(width * scale));
    canvas.height = Math.max(1, Math.floor(height * scale));

    const context = canvas.getContext('2d');
    if (!context) {
      return null;
    }

    context.scale(scale, scale);
    context.fillStyle = '#edf4e5';
    context.fillRect(0, 0, width, height);

    function normalizeHexColor(hexColor, fallback = '#8c7b64') {
      if (typeof hexColor !== 'string') return fallback;
      const normalized = hexColor.trim();
      if (/^#[0-9a-fA-F]{6}$/.test(normalized)) return normalized;
      if (/^#[0-9a-fA-F]{3}$/.test(normalized)) {
        const short = normalized.slice(1);
        return `#${short[0]}${short[0]}${short[1]}${short[1]}${short[2]}${short[2]}`;
      }
      return fallback;
    }

    function shadeColor(hexColor, factor) {
      const hex = normalizeHexColor(hexColor).slice(1);
      const r = Number.parseInt(hex.slice(0, 2), 16);
      const g = Number.parseInt(hex.slice(2, 4), 16);
      const b = Number.parseInt(hex.slice(4, 6), 16);
      const nextR = Math.max(0, Math.min(255, Math.round(r * factor)));
      const nextG = Math.max(0, Math.min(255, Math.round(g * factor)));
      const nextB = Math.max(0, Math.min(255, Math.round(b * factor)));
      return `rgb(${nextR}, ${nextG}, ${nextB})`;
    }

    function buildSoilLayerHeights(totalDepth) {
      if (sortedSoilLayers.length === 0 || totalDepth <= 0) {
        return [];
      }

      const rawHeights = sortedSoilLayers.map((layer) => Math.max(8, (layer.percentage / 100) * totalDepth));
      const rawTotal = rawHeights.reduce((sum, value) => sum + value, 0);
      const scaleFactor = rawTotal > 0 ? totalDepth / rawTotal : 1;
      return rawHeights.map((value) => value * scaleFactor);
    }

    let minX = Number.POSITIVE_INFINITY;
    let minY = Number.POSITIVE_INFINITY;
    let maxX = Number.NEGATIVE_INFINITY;
    let maxY = Number.NEGATIVE_INFINITY;

    for (const cell of gridCells) {
      minX = Math.min(minX, cell.left - TILE_WIDTH / 2);
      maxX = Math.max(maxX, cell.left + TILE_WIDTH / 2);
      minY = Math.min(minY, cell.top - TILE_HEIGHT / 2);
      maxY = Math.max(maxY, cell.top + TILE_HEIGHT / 2);
    }

    for (const item of visiblePlacedItemsView) {
      const itemWidth = item.size * TILE_WIDTH;
      const itemHeight = item.size * TILE_HEIGHT;
      const drawHeight = itemHeight * 1.5;
      const drawTopShift = item.type === 'plant' ? itemHeight * 0.1 : 0;

      minX = Math.min(minX, item.left - itemWidth / 2);
      maxX = Math.max(maxX, item.left + itemWidth / 2);
      minY = Math.min(minY, item.top - drawHeight / 2 - drawTopShift);
      maxY = Math.max(maxY, item.top + drawHeight / 2);
    }

    if (soilDepth > 0) {
      const originX = (gridSize * TILE_WIDTH) / 2;
      const originY = GRID_TOP_OFFSET;
      const leftCorner = toIsoPosition(gridSize, 0, originX, originY);
      const rightCorner = toIsoPosition(0, gridSize, originX, originY);
      const bottomTip = toIsoPosition(gridSize, gridSize, originX, originY);

      minX = Math.min(minX, leftCorner.left, bottomTip.left, rightCorner.left);
      maxX = Math.max(maxX, leftCorner.left, bottomTip.left, rightCorner.left);
      minY = Math.min(minY, leftCorner.top - TILE_HEIGHT / 2, bottomTip.top - TILE_HEIGHT / 2, rightCorner.top - TILE_HEIGHT / 2);
      maxY = Math.max(maxY, leftCorner.top - TILE_HEIGHT / 2 + soilDepth, bottomTip.top - TILE_HEIGHT / 2 + soilDepth, rightCorner.top - TILE_HEIGHT / 2 + soilDepth);
    }

    if (!Number.isFinite(minX) || !Number.isFinite(minY) || !Number.isFinite(maxX) || !Number.isFinite(maxY)) {
      minX = 0;
      minY = 0;
      maxX = width;
      maxY = height;
    }

    const offsetX = (width - (maxX - minX)) / 2 - minX;
    const offsetY = (height - (maxY - minY)) / 2 - minY;

    if (soilDepth > 0) {
      const originX = (gridSize * TILE_WIDTH) / 2;
      const originY = GRID_TOP_OFFSET;
      const leftCorner = toIsoPosition(gridSize, 0, originX, originY);
      const rightCorner = toIsoPosition(0, gridSize, originX, originY);
      const bottomTip = toIsoPosition(gridSize, gridSize, originX, originY);
      const topYLeft = leftCorner.top - TILE_HEIGHT / 2;
      const topYBottom = bottomTip.top - TILE_HEIGHT / 2;
      const topYRight = rightCorner.top - TILE_HEIGHT / 2;
      const layerHeights = buildSoilLayerHeights(soilDepth);

      let depthOffset = 0;
      for (let index = 0; index < sortedSoilLayers.length; index += 1) {
        const layer = sortedSoilLayers[index];
        const layerHeight = layerHeights[index] ?? 0;
        const baseColor = normalizeHexColor(layer.soilType?.hexColor);

        context.fillStyle = shadeColor(baseColor, 0.86);
        context.beginPath();
        context.moveTo(leftCorner.left + offsetX, topYLeft + depthOffset + offsetY);
        context.lineTo(bottomTip.left + offsetX, topYBottom + depthOffset + offsetY);
        context.lineTo(bottomTip.left + offsetX, topYBottom + depthOffset + layerHeight + offsetY);
        context.lineTo(leftCorner.left + offsetX, topYLeft + depthOffset + layerHeight + offsetY);
        context.closePath();
        context.fill();

        context.fillStyle = shadeColor(baseColor, 0.74);
        context.beginPath();
        context.moveTo(bottomTip.left + offsetX, topYBottom + depthOffset + offsetY);
        context.lineTo(rightCorner.left + offsetX, topYRight + depthOffset + offsetY);
        context.lineTo(rightCorner.left + offsetX, topYRight + depthOffset + layerHeight + offsetY);
        context.lineTo(bottomTip.left + offsetX, topYBottom + depthOffset + layerHeight + offsetY);
        context.closePath();
        context.fill();

        depthOffset += layerHeight;
      }
    }

    for (const cell of gridCells) {
      context.beginPath();
      context.moveTo(cell.left + offsetX, cell.top - TILE_HEIGHT / 2 + offsetY);
      context.lineTo(cell.left + TILE_WIDTH / 2 + offsetX, cell.top + offsetY);
      context.lineTo(cell.left + offsetX, cell.top + TILE_HEIGHT / 2 + offsetY);
      context.lineTo(cell.left - TILE_WIDTH / 2 + offsetX, cell.top + offsetY);
      context.closePath();

      if (globalSoilColor) {
        const gradient = context.createLinearGradient(
          cell.left - TILE_WIDTH / 2 + offsetX,
          cell.top - TILE_HEIGHT / 2 + offsetY,
          cell.left + TILE_WIDTH / 2 + offsetX,
          cell.top + TILE_HEIGHT / 2 + offsetY,
        );
        gradient.addColorStop(0, globalSoilColor.start);
        gradient.addColorStop(1, globalSoilColor.end);
        context.fillStyle = gradient;
        context.strokeStyle = globalSoilColor.border;
      } else {
        const gradient = context.createLinearGradient(
          cell.left - TILE_WIDTH / 2 + offsetX,
          cell.top - TILE_HEIGHT / 2 + offsetY,
          cell.left + TILE_WIDTH / 2 + offsetX,
          cell.top + TILE_HEIGHT / 2 + offsetY,
        );
        gradient.addColorStop(0, '#dce8d4');
        gradient.addColorStop(1, '#c5d9b8');
        context.fillStyle = gradient;
        context.strokeStyle = 'rgba(31, 52, 32, 0.15)';
      }

      context.lineWidth = 1;
      context.fill();
      context.stroke();
    }

    const imagesCache = new Map();
    const itemsToDraw = visiblePlacedItemsView;

    for (const item of itemsToDraw) {
      const itemWidth = item.size * TILE_WIDTH;
      const itemHeight = item.size * TILE_HEIGHT;
      const areaLeft = item.left - itemWidth / 2;
      const areaTop = item.top - itemHeight / 2;

      if (!item.image) {
        continue;
      }

      if (!imagesCache.has(item.image)) {
        imagesCache.set(item.image, await loadImageForSnapshot(item.image));
      }

      const image = imagesCache.get(item.image);
      if (!image) {
        continue;
      }

      const imageRatio = image.width / image.height;
      let drawWidth = itemWidth * 0.82;
      let drawHeight = drawWidth / imageRatio;

      const maxHeight = itemHeight * 1.5;
      if (drawHeight > maxHeight) {
        drawHeight = maxHeight;
        drawWidth = drawHeight * imageRatio;
      }

      let drawX = areaLeft + (itemWidth - drawWidth) / 2;
      let drawY = areaTop + (itemHeight - drawHeight) / 2;

      if (item.type === 'plant') {
        drawY -= itemHeight * 0.1;
      }

      context.drawImage(image, drawX + offsetX, drawY + offsetY, drawWidth, drawHeight);
    }

    return await new Promise((resolve) => {
      canvas.toBlob((blob) => resolve(blob), 'image/png');
    });
  }

  async function captureConstructorCanvasBlob() {
    if (!boardRef.current) {
      return null;
    }

    boardRef.current.classList.add('constructor-board-capture-mode');

    const scale = window.devicePixelRatio > 1 ? 2 : 1;

    async function renderSnapshotWithOptions(foreignObjectRendering) {
      return html2canvas(boardRef.current, {
        backgroundColor: '#edf4e5',
        scale,
        useCORS: true,
        allowTaint: true,
        foreignObjectRendering,
        logging: false,
        removeContainer: true,
      });
    }

    function hasVisiblePixels(canvas) {
      const context = canvas.getContext('2d', { willReadFrequently: true });
      if (!context) {
        return true;
      }

      try {
        const { data } = context.getImageData(0, 0, canvas.width, canvas.height);
        for (let i = 3; i < data.length; i += 4) {
          if (data[i] !== 0) {
            return true;
          }
        }
      } catch {
        // If browser blocks pixel reads, treat it as visible and let blob conversion decide.
        return true;
      }

      return false;
    }

    function looksLikeFlatBackground(canvas) {
      const context = canvas.getContext('2d', { willReadFrequently: true });
      if (!context) {
        return false;
      }

      let imageData;
      try {
        imageData = context.getImageData(0, 0, canvas.width, canvas.height).data;
      } catch {
        return false;
      }

      const bgR = 237;
      const bgG = 244;
      const bgB = 229;
      const width = canvas.width;
      const height = canvas.height;
      const stride = Math.max(1, Math.floor(Math.min(width, height) / 180));

      let sampled = 0;
      let different = 0;

      for (let y = 0; y < height; y += stride) {
        for (let x = 0; x < width; x += stride) {
          const index = (y * width + x) * 4;
          const alpha = imageData[index + 3];
          if (alpha < 8) {
            continue;
          }

          sampled += 1;

          const r = imageData[index];
          const g = imageData[index + 1];
          const b = imageData[index + 2];
          const delta = Math.abs(r - bgR) + Math.abs(g - bgG) + Math.abs(b - bgB);
          if (delta > 18) {
            different += 1;
          }
        }
      }

      if (sampled === 0) {
        return true;
      }

      return (different / sampled) < 0.006;
    }

    async function canvasToBlob(snapshotCanvas) {
      try {
        return await new Promise((resolve) => {
          snapshotCanvas.toBlob((blob) => resolve(blob), 'image/png');
        });
      } catch {
        return null;
      }
    }

    let canvas = null;
    try {
      // Prefer foreignObject mode first because it preserves clip-path diamonds for isometric cells.
      canvas = await renderSnapshotWithOptions(true);
      if (!hasVisiblePixels(canvas)) {
        canvas = await renderSnapshotWithOptions(false);
      }
      if (!hasVisiblePixels(canvas)) {
        // Last fallback: documented default call.
        canvas = await html2canvas(boardRef.current);
      }
    } finally {
      boardRef.current.classList.remove('constructor-board-capture-mode');
    }

    if (canvas && hasVisiblePixels(canvas) && !looksLikeFlatBackground(canvas)) {
      const domBlob = await canvasToBlob(canvas);
      if (domBlob) {
        return domBlob;
      }
    }

    // Fallback to manual renderer when DOM capture is blank or blob conversion fails.
    return await renderConstructorSnapshotManually();
  }

  const ITEM_TYPE_MAP = { plant: 0, decoration: 1, soil: 2 };

  async function handleSave() {
    if (!selectedContainer) {
      setNotice('Оберіть контейнер у каталозі перед збереженням проєкту.');
      return;
    }

    const nonContainerItems = placedItems;
    const plantOnly = placedItems.filter((i) => i.type === 'plant');

    if (plantOnly.length > 0 && soilMixStepBlocked) {
      setNotice(
        'Збереження недоступне: набір рослин несумісний за даними каталогу. Змініть склад або умови.',
      );
      return;
    }

    if (plantOnly.length > 0 && !appliedSoilFormula) {
      setNotice(
        proposedSoilFormula
          ? 'Спочатку натисніть «Розрахувати формулу ґрунту», щоб застосувати її на сітці.'
          : 'Оберіть і застосуйте формулу ґрунту в каталозі (клік на картці) перед збереженням.',
      );
      return;
    }

    if (nonContainerItems.length === 0 && !appliedSoilFormula) {
      setNotice('Додайте хоча б одну рослину, декорацію або формулу ґрунту.');
      return;
    }

    setSaving(true);
    setNotice('');

    try {
      const project = await api.projects.add({ containerId: selectedContainer.entityId });

      const itemsToAdd = nonContainerItems
        .filter((item) => ITEM_TYPE_MAP[item.type] !== undefined)
        .map((item) => ({
          itemType: ITEM_TYPE_MAP[item.type],
          itemId: item.entityId,
          posX: item.row,
          posY: item.col,
          posZ: 0,
        }));

      if (appliedSoilFormula) {
        itemsToAdd.push({
          itemType: 2,
          itemId: appliedSoilFormula.entityId,
          posX: 0,
          posY: 0,
          posZ: 0,
        });
      }

      for (const body of itemsToAdd) {
        await api.projects.addItem(project.id, body);
      }

      setSavedProjectId(project.id);
      setSavedProjectPublished(Boolean(project.isPublished));
      setNotice(
        'Чернетку збережено. Список проєктів — у профілі. У спільній галереї робота з’явиться лише після публікації.',
      );
    } catch (err) {
      setNotice(`Помилка збереження: ${err?.detail || err?.message || 'Невідома помилка'}`);
    } finally {
      setSaving(false);
    }
  }

  async function handlePublishToGallery() {
    if (!savedProjectId) return;
    setPublishing(true);
    setNotice('');
    try {
      const res = await api.projects.publish(savedProjectId);
      setSavedProjectPublished(Boolean(res.isPublished));
      setNotice('Проєкт опубліковано — він з’явиться в галереї.');
    } catch (err) {
      setNotice(`Не вдалося опублікувати: ${err?.detail || err?.message || 'Невідома помилка'}`);
    } finally {
      setPublishing(false);
    }
  }

  async function handleUnpublishFromGallery() {
    if (!savedProjectId) return;
    setPublishing(true);
    setNotice('');
    try {
      const res = await api.projects.unpublish(savedProjectId);
      setSavedProjectPublished(Boolean(res.isPublished));
      setNotice('Проєкт знято з галереї. Чернетка залишається в профілі.');
    } catch (err) {
      setNotice(`Не вдалося зняти з публікації: ${err?.detail || err?.message || 'Невідома помилка'}`);
    } finally {
      setPublishing(false);
    }
  }

  async function handleGoToResult() {
    if (!savedProjectId) {
      setNotice('Спочатку збережіть проєкт.');
      return;
    }

    setPreparingResult(true);
    setNotice('');

    try {
      const canvasSnapshot = await captureConstructorCanvasBlob();
      if (!canvasSnapshot) {
        setNotice('Не вдалося зняти знімок полотна конструктора.');
        return;
      }

      navigate(`/result?projectId=${encodeURIComponent(savedProjectId)}`, {
        state: { projectId: savedProjectId, canvasSnapshot },
      });
    } catch {
      setNotice('Не вдалося підготувати зображення полотна для генерації.');
    } finally {
      setPreparingResult(false);
    }
  }

  useEffect(() => {
    if (!editorOpen) return;
    function onKeyDown(e) {
      if (e.defaultPrevented) return;
      const t = e.target;
      if (t instanceof HTMLElement && t.closest('input, textarea, select, [contenteditable="true"]')) return;

      if (e.key === 'Escape') {
        setSelectedPlacedItemId(null);
        return;
      }

      if (e.key === 'Delete' && selectedPlacedItemId) {
        e.preventDefault();
        removePlacedItem(selectedPlacedItemId);
        return;
      }

      if (['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight'].includes(e.key)) {
        if (!selectedPlacedItemId) return;
        const deltas = {
          ArrowUp: { row: -1, col: 0 },
          ArrowDown: { row: 1, col: 0 },
          ArrowLeft: { row: 0, col: -1 },
          ArrowRight: { row: 0, col: 1 },
        };
        const d = deltas[e.key];
        if (!d) return;
        e.preventDefault();
        tryMoveSelectedPlacedItem(d.row, d.col);
        return;
      }

      if ((e.ctrlKey || e.metaKey) && e.key === 'z') {
        e.preventDefault();
        if (e.shiftKey) redoWorkspace();
        else undoWorkspace();
        return;
      }
      if ((e.ctrlKey || e.metaKey) && e.key === 'y') {
        e.preventDefault();
        redoWorkspace();
      }
    }
    window.addEventListener('keydown', onKeyDown);
    return () => window.removeEventListener('keydown', onKeyDown);
  }, [
    editorOpen,
    selectedPlacedItemId,
    placedItems,
    gridSize,
    appliedSoilFormula,
    selectedContainer,
  ]);

  if (!isAuthenticated && shouldRedirectToAuth) {
    return <Navigate to="/auth" replace />;
  }

  if (!isAuthenticated) {
    return (
      <section className="constructor-page">
        <div className="constructor-auth-guard">
          <div className="constructor-auth-card">
            <p className="constructor-auth-card-label">Доступ обмежено</p>
            <h2>Для перегляду конструктора, будь ласка, авторизуйтесь</h2>
            <p>Перенаправляємо на сторінку входу через 5 секунд...</p>
          </div>
        </div>
      </section>
    );
  }

  if (!editorOpen) {
    return (
      <section className="constructor-page constructor-entry-page">
        <div className="constructor-entry-inner">
          <div className="constructor-entry-card">
            <p className="constructor-entry-label">Конструктор</p>
            <h2 className="constructor-entry-title">Що робимо далі?</h2>
            <p className="constructor-entry-text">
              Створіть новий ескіз на сітці або відкрийте галерею збережених і опублікованих флораріумів.
            </p>
            <div className="constructor-entry-actions">
              <button
                type="button"
                className="constructor-entry-btn constructor-entry-btn-primary"
                onClick={() => {
                  setNotice('');
                  setEditorOpen(true);
                }}
              >
                Створити новий ескіз
              </button>
              <button
                type="button"
                className="constructor-entry-btn constructor-entry-btn-secondary"
                onClick={() => navigate('/gallery')}
              >
                Переглянути вже створені
              </button>
            </div>
          </div>
        </div>
      </section>
    );
  }

  return (
    <section className="constructor-page">
      <aside className="constructor-catalog">
        <div className="constructor-catalog-head">
          <h1 className="constructor-title">Конструктор флораріуму</h1>
          <div className="constructor-language-switch">
            <button
              type="button"
              className="constructor-language-icon-toggle"
              onClick={() => setPlantNameLanguage((prev) => (prev === 'uk' ? 'latin' : 'uk'))}
              aria-label={`Перемкнути мову назв рослин. Поточна: ${plantNameLanguage === 'uk' ? 'українська' : 'латина'}`}
              title={`Мова назв: ${plantNameLanguage === 'uk' ? 'Українська' : 'Латина'}`}
            >
              <span aria-hidden="true">🌐</span>
              <span>{plantNameLanguage === 'uk' ? 'УКР' : 'LAT'}</span>
            </button>
          </div>
        </div>

        <div className="constructor-search-wrap">
          <input
            className="constructor-search"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Назва, латина або деталі"
            type="text"
            aria-label="Пошук у каталозі"
          />
        </div>

        <div className="constructor-tabs" role="tablist" aria-label="Категорії каталогу">
          {CATALOG_TABS.map((tab) => (
            <button
              key={tab.key}
              type="button"
              className={`constructor-tab ${tab.key === activeTab ? 'constructor-tab-active' : ''}`}
              onClick={() => {
                setActiveTab(tab.key);
                setSelectedCatalogItem(null);
              }}
            >
              {tab.label}
            </button>
          ))}
        </div>
        <div className="constructor-catalog-list">
          {loading && <p className="constructor-muted">Завантаження елементів...</p>}
          {!loading && error && <p className="constructor-error">{error}</p>}
          {!loading && !error && visibleCatalogItems.length === 0 && (
            <p className="constructor-muted">Нічого не знайдено для цього фільтра.</p>
          )}

          {!loading && !error && visibleCatalogItems.map((item) => (
            <article
              key={item.id}
              data-catalog-entity-id={item.entityId}
              className={`constructor-card ${(selectedCatalogItem?.id === item.id || (item.kind === 'container' && selectedContainer?.id === item.id)) ? 'constructor-card-selected' : ''} ${item.kind === 'soilFormula' && appliedSoilFormula?.id === item.id ? 'constructor-card-applied' : ''} ${item.kind === 'soilFormula' && proposedSoilFormula?.id === item.id && !appliedSoilFormula ? 'constructor-card-soil-proposed' : ''}`}
              draggable={item.kind !== 'soilFormula' && item.kind !== 'container'}
              onDragStart={(event) => {
                if (item.kind === 'soilFormula' || item.kind === 'container') { event.preventDefault(); return; }
                handleDragStart(event, item);
              }}
              onClick={() => {
                if (item.kind === 'soilFormula') {
                  if (soilMixStepBlocked) {
                    setNotice('Спочатку змініть склад рослин — за каталогом цей мікс занадто конфліктний для спільної посудини.');
                    return;
                  }
                  if (appliedSoilFormula) {
                    commitHistoryBeforeMutation();
                    setAppliedSoilFormula(item);
                    setNotice(`Формулу ґрунту змінено на: ${item.name}`);
                    return;
                  }
                  if (proposedSoilFormula && item.id === proposedSoilFormula.id) {
                    setNotice('Натисніть «Розрахувати формулу ґрунту» у блоці над сіткою.');
                    return;
                  }
                  if (proposedSoilFormula) {
                    setNotice('Спочатку розрахуйте рекомендовану формулу кнопкою над сіткою; потім можна обрати іншу в каталозі.');
                    return;
                  }
                  commitHistoryBeforeMutation();
                  setAppliedSoilFormula(item);
                  setNotice(`Формулу ґрунту застосовано: ${item.name}`);
                  return;
                }

                if (item.kind === 'container') {
                  commitHistoryBeforeMutation();
                  setSelectedContainer(item);
                  setNotice(`Контейнер обрано: ${item.name}`);
                  return;
                }

                setSelectedCatalogItem((prev) => (prev?.id === item.id ? null : item));
              }}
            >
              <div className="constructor-card-media">
                {item.image && <img src={item.image} alt={item.name} className="constructor-card-image" />}
                {!item.image && (
                  <span className="constructor-card-media-placeholder">
                    {item.kind === 'soilFormula' ? '🌱' : `${item.footprint}x${item.footprint}`}
                  </span>
                )}
              </div>
              <div className="constructor-card-content">
                <div className="constructor-card-head">
                  <h3>{item.name}</h3>
                  {item.kind !== 'soilFormula' && <span>{item.footprint}x{item.footprint}</span>}
                  {item.kind === 'soilFormula' && appliedSoilFormula?.id === item.id && (
                    <span className="constructor-applied-badge">На сітці</span>
                  )}
                  {item.kind === 'soilFormula' && proposedSoilFormula?.id === item.id && !appliedSoilFormula && (
                    <span className="constructor-proposed-badge">Рекомендовано</span>
                  )}
                </div>
                {item.kind !== 'plant' && item.subtitle && <p className="constructor-card-subtitle">{item.subtitle}</p>}
                {item.kind === 'plant' && (
                  <div className="constructor-plant-levels">
                    {renderLevelIcons(item.lightLevel, '☀', 'Світло', 'sun')}
                    {renderLevelIcons(item.waterNeed, '💧', 'Вода', 'water')}
                  </div>
                )}
                {item.kind !== 'plant' && item.details && <p className="constructor-card-details">{item.details}</p>}
              </div>
            </article>
          ))}
        </div>
      </aside>

      <div className="constructor-workspace">
        <header className="constructor-toolbar">
          <div className="constructor-grid-size">
            <span>Розмір сітки</span>
            <div>
              {GRID_PRESETS.map((size) => (
                <button
                  key={size}
                  type="button"
                  className={size === gridSize ? 'constructor-size-active' : ''}
                  onClick={() => {
                    commitHistoryBeforeMutation();
                    setGridSize(size);
                    setPlacedItems((prev) => prev.filter(
                      (item) => item.row + item.size <= size && item.col + item.size <= size,
                    ));
                  }}
                >
                  {size}x{size}
                </button>
              ))}
            </div>
          </div>

          <div className="constructor-toolbar-undo" role="group" aria-label="Історія змін">
            <button
              type="button"
              className="constructor-toolbar-icon-btn"
              onClick={undoWorkspace}
              disabled={historyRef.current.past.length === 0}
              title="Назад (Ctrl+Z)"
              aria-label="Скасувати дію"
            >
              Назад
            </button>
            <button
              type="button"
              className="constructor-toolbar-icon-btn"
              onClick={redoWorkspace}
              disabled={historyRef.current.future.length === 0}
              title="Вперед (Ctrl+Y або Ctrl+Shift+Z)"
              aria-label="Повторити дію"
            >
              Вперед
            </button>
          </div>

          <div className="constructor-toolbar-zoom" role="group" aria-label="Масштаб полотна">
            <button
              type="button"
              className="constructor-toolbar-icon-btn"
              onClick={() => setBoardZoom((z) => Math.max(BOARD_ZOOM_MIN, Math.round((z - BOARD_ZOOM_STEP) * 10) / 10))}
              disabled={boardZoom <= BOARD_ZOOM_MIN}
              aria-label="Зменшити масштаб"
            >
              −
            </button>
            <span className="constructor-toolbar-zoom-value">{Math.round(boardZoom * 100)}%</span>
            <button
              type="button"
              className="constructor-toolbar-icon-btn"
              onClick={() => setBoardZoom((z) => Math.min(BOARD_ZOOM_MAX, Math.round((z + BOARD_ZOOM_STEP) * 10) / 10))}
              disabled={boardZoom >= BOARD_ZOOM_MAX}
              aria-label="Збільшити масштаб"
            >
              +
            </button>
            <button
              type="button"
              className="constructor-toolbar-zoom-reset"
              onClick={() => setBoardZoom(1)}
              aria-label="Масштаб 100 відсотків"
            >
              100%
            </button>
          </div>

          <div className="constructor-actions">
            <span>Елементів: {placedItems.length}</span>
            <span>Контейнер: {selectedContainer ? selectedContainer.name : 'не обрано'}</span>
            <span>Режим: {selectedCatalogItem ? `Клік-плейс (${selectedCatalogItem.name})` : 'Drag-and-drop'}</span>
            <button
              type="button"
              onClick={() => setHideObjectsLayer((prev) => !prev)}
            >
              {hideObjectsLayer ? 'Показати об\'єкти' : 'Сховати об\'єкти'}
            </button>
            <button type="button" onClick={resetWorkspace}>Очистити</button>
            <button
              type="button"
              onClick={handleSave}
              disabled={saving || saveBlockedByPlantsAndSoil}
              title={
                saveBlockedByPlantsAndSoil
                  ? (soilMixStepBlocked
                    ? 'Несумісний набір рослин — збереження недоступне'
                    : 'Спочатку розрахуйте або застосуйте формулу ґрунту')
                  : 'Зберігає чернетку на сервер (не в галереї). Кожне натискання створює новий проєкт.'
              }
            >
              {saving ? 'Збереження...' : 'Зберегти чернетку'}
            </button>
            {savedProjectId && !savedProjectPublished && (
              <button
                type="button"
                className="constructor-toolbar-publish"
                onClick={handlePublishToGallery}
                disabled={publishing || saveBlockedByPlantsAndSoil}
                title={
                  saveBlockedByPlantsAndSoil
                    ? 'Узгодьте склад рослин і ґрунт перед публікацією'
                    : 'Показати цей проєкт у спільній галереї'
                }
              >
                {publishing ? 'Публікація...' : 'Опублікувати в галереї'}
              </button>
            )}
            {savedProjectId && savedProjectPublished && (
              <button
                type="button"
                className="constructor-toolbar-unpublish"
                onClick={handleUnpublishFromGallery}
                disabled={publishing}
              >
                {publishing ? 'Оновлення...' : 'Зняти з галереї'}
              </button>
            )}
            {savedProjectId && (
              <button type="button" onClick={handleGoToResult} disabled={preparingResult}>
                {preparingResult ? 'Підготовка...' : 'Згенерувати зображення'}
              </button>
            )}
            {savedProjectId && (
              <ExportPdfButton
                projectId={savedProjectId}
                variant="secondary"
                className="constructor-export-pdf"
              />
            )}
          </div>
        </header>

        {notice && !noticeDismissed && (
          <div className="constructor-dismissible constructor-dismissible--notice">
            <p className="constructor-dismissible__text constructor-notice">{notice}</p>
            <button
              type="button"
              className="constructor-dismissible__close"
              onClick={() => setNoticeDismissed(true)}
              aria-label="Закрити повідомлення"
            >
              ×
            </button>
          </div>
        )}

        {!galleryHintDismissed && (
          <div className="constructor-dismissible constructor-dismissible--hint">
            <p className="constructor-dismissible__text constructor-draft-hint">
              Галерея показує лише опубліковані роботи. Усі збережені проєкти (чернетки) — у{' '}
              <Link to="/profile">профілі</Link>
              {savedProjectId ? (
                <>
                  {' '}
                  · поточний проєкт:{' '}
                  <strong>{savedProjectPublished ? 'у галереї' : 'чернетка'}</strong>
                </>
              ) : null}
              .
            </p>
            <button
              type="button"
              className="constructor-dismissible__close"
              onClick={() => setGalleryHintDismissed(true)}
              aria-label="Закрити підказку про галерею"
            >
              ×
            </button>
          </div>
        )}

        {soilMixStepBlocked && placedPlantItems.length >= 2 && (
          <div className="constructor-plant-mix-warning" role="alert">
            <strong>Увага:</strong> за даними каталогу цей набір рослин не варто тримати разом у одній посудині без зонування або поділу середовища. Замініть види або розділіть умови — після узгодження з’явиться рекомендація ґрунту.
          </div>
        )}

        {!soilMixStepBlocked && placedPlantItems.length >= 1 && proposedSoilFormula && !appliedSoilFormula && (
          <div className="constructor-soil-proposal">
            <p className="constructor-soil-proposal-text">
              Рекомендована формула ґрунту для поточного міксу:{' '}
              <strong>{proposedSoilFormula.name}</strong>
            </p>
            <button type="button" className="constructor-soil-proposal-apply" onClick={handleApplyRecommendedSoil}>
              Розрахувати формулу ґрунту
            </button>
          </div>
        )}

        {!soilMixStepBlocked && placedPlantItems.length >= 1 && !proposedSoilFormula && !appliedSoilFormula && (
          <p className="constructor-soil-manual-hint">
            Для рослин у каталозі не знайдено прив’язки до формули ґрунту — після узгодження видів оберіть формулу вручну в каталозі (клік застосує її на сітці).
          </p>
        )}

        {floraCompatibility
          && (floraCompatibility.shouldPrompt || floraCompatibility.pairingVerdictLevel === 'ok')
          && floraCompatibility.userSummary?.title && (
          <div className="constructor-compat-banner" role="status">
            <div className="constructor-compat-banner-head">
              <strong className="constructor-compat-banner-title">Сумісність рослин і ґрунту</strong>
              <button
                type="button"
                className="constructor-compat-banner-toggle"
                onClick={() => setCompatPanelExpanded((v) => !v)}
                aria-expanded={compatPanelExpanded}
                aria-controls="constructor-compat-panel-body"
                title={compatPanelExpanded ? 'Згорнути' : 'Розгорнути'}
              >
                {compatPanelExpanded ? '−' : '+'}
              </button>
            </div>
            {compatPanelExpanded && (
              <div id="constructor-compat-panel-body" className="constructor-compat-banner-body">
                <p
                  className={`constructor-compat-verdict constructor-compat-verdict--${floraCompatibility.userSummary.level}`}
                >
                  {floraCompatibility.userSummary.title}
                </p>
                {floraCompatibility.userSummary.problems?.length ? (
                  <ul className="constructor-compat-user-problems" aria-label="У чому справа">
                    {floraCompatibility.userSummary.problems.map((line, idx) => (
                      <li key={`compat-p-${idx}-${line.slice(0, 24)}`}>{line}</li>
                    ))}
                  </ul>
                ) : null}
                {floraCompatibility.soilCatalogLinks?.length ? (
                  <div className="constructor-compat-soil-links" aria-label="Перейти до формул у каталозі">
                    <span className="constructor-compat-soil-links-label">У каталозі:</span>
                    {floraCompatibility.soilCatalogLinks.map((link) => (
                      <button
                        key={link.entityId}
                        type="button"
                        className="constructor-compat-soil-link"
                        onClick={() => focusCatalogSoilFormula(link.entityId, link.label)}
                      >
                        {link.label}
                      </button>
                    ))}
                  </div>
                ) : null}
                {floraCompatibility.userSummary.whatToDo?.length ? (
                  <div className="constructor-compat-user-do">
                    <strong className="constructor-compat-user-do-label">Що зробити</strong>
                    <ul className="constructor-compat-user-do-list" aria-label="Рекомендовані дії">
                      {floraCompatibility.userSummary.whatToDo.map((line, idx) => (
                        <li key={`compat-w-${idx}-${line.slice(0, 24)}`}>{line}</li>
                      ))}
                    </ul>
                  </div>
                ) : null}
              </div>
            )}
          </div>
        )}

        <div
          ref={boardWrapRef}
          className={`constructor-board-wrap ${isPanning ? 'constructor-board-wrap-panning' : ''}`}
          onMouseDown={handlePanStart}
          onMouseMove={handlePanMove}
          onMouseUp={stopPan}
          onMouseLeave={stopPan}
        >
          <div
            className="constructor-board-zoom-inner"
            style={{
              zoom: boardZoom,
            }}
          >
            <div
              ref={boardRef}
              className="constructor-board"
              style={{ width: `${boardWidth}px`, height: `${boardHeight + (sortedSoilLayers.length > 0 ? 80 : 0)}px` }}
              onDragOver={handleDragOver}
              onDrop={handleDrop}
              onDragLeave={handleDragLeaveBoard}
              onClick={handleBoardClick}
            >
            {gridCells.map((cell) => {
              const hoverClass = (() => {
                if (!isCellInsideCandidate(cell, dragHoverCell)) return '';

                const isOutOfBounds = !dragHoverCell
                  || dragHoverCell.row < 0
                  || dragHoverCell.col < 0
                  || dragHoverCell.row + dragHoverCell.size > gridSize
                  || dragHoverCell.col + dragHoverCell.size > gridSize;

                const isOccupied = placedItems.some((item) => {
                  if (item.instanceId === dragHoverCell?.excludingId) return false;
                  if (item.layer !== dragHoverCell?.layer) return false;
                  return (
                    cell.row >= item.row
                    && cell.row < item.row + item.size
                    && cell.col >= item.col
                    && cell.col < item.col + item.size
                  );
                });

                return isOutOfBounds || isOccupied ? 'iso-cell-hover-blocked' : 'iso-cell-hover';
              })();

              const isHovered = hoverClass !== '';

              return (
                <div
                  key={`${cell.row}-${cell.col}`}
                  className={`iso-cell ${hoverClass}`}
                  style={{
                    left: `${cell.left}px`,
                    top: `${cell.top}px`,
                    ...(globalSoilColor && !isHovered
                      ? {
                        background: `linear-gradient(160deg, ${globalSoilColor.start} 0%, ${globalSoilColor.end} 100%)`,
                        borderColor: globalSoilColor.border,
                      }
                      : {}),
                  }}
                />
              );
            })}

            {visiblePlacedItemsView.map((item) => (
              <div
                key={item.instanceId}
                className={`placed-item-hover-wrap ${selectedPlacedItemId === item.instanceId ? 'placed-item-hover-wrap--selected' : ''}`}
                draggable
                onDragStart={(event) => handlePlacedDragStart(event, item)}
                onClick={(event) => {
                  event.stopPropagation();
                  setSelectedPlacedItemId((prev) => (prev === item.instanceId ? null : item.instanceId));
                }}
                style={{
                  left: `${item.left}px`,
                  top: `${item.top}px`,
                  padding: `${PLACED_ITEM_HOVER_PAD}px`,
                  boxSizing: 'content-box',
                }}
                title={`${item.name} (${item.size}x${item.size})`}
              >
                <div
                  className={`placed-item placed-item-${item.type}`}
                  style={{
                    width: `${item.size * TILE_WIDTH}px`,
                    height: `${item.size * TILE_HEIGHT}px`,
                  }}
                >
                  <div className="placed-item-footprint" />
                  {item.image && <img src={item.image} alt={item.name} className="placed-item-image" />}
                  <span
                    className={`placed-item-label ${selectedPlacedItemId === item.instanceId && item.layer === 'soil' ? 'placed-item-label-visible' : ''}`}
                  >
                    {item.name}
                  </span>
                  <button
                    type="button"
                    className={`placed-item-remove ${selectedPlacedItemId === item.instanceId ? 'placed-item-remove-visible' : ''}`}
                    aria-label={`Видалити ${item.name} з полотна`}
                    onMouseDown={(event) => {
                      event.stopPropagation();
                    }}
                    onClick={(event) => {
                      event.stopPropagation();
                      removePlacedItem(item.instanceId);
                    }}
                  >
                    x
                  </button>
                </div>
              </div>
            ))}

            {/* 3D platform depth — two isometric side walls */}
            {sortedSoilLayers.length > 0 && (() => {
              const originX = (gridSize * TILE_WIDTH) / 2;
              const originY = GRID_TOP_OFFSET;
              const leftCorner = toIsoPosition(gridSize, 0, originX, originY);
              const bottomTip = toIsoPosition(gridSize, gridSize, originX, originY);

              const wallWidth = bottomTip.left - leftCorner.left;
              const skewAngle = Math.atan(TILE_HEIGHT / TILE_WIDTH) * (180 / Math.PI);
              const totalDepth = 80;

              const buildStripes = (keyPrefix) => sortedSoilLayers.map((layer, i) => {
                const h = Math.max(8, (layer.percentage / 100) * totalDepth);
                const hex = layer.soilType?.hexColor || '#8c7b64';
                return (
                  <div
                    key={`${keyPrefix}-${i}`}
                    style={{
                      width: '100%',
                      height: `${h}px`,
                      backgroundColor: hex,
                      borderBottom: i < sortedSoilLayers.length - 1 ? '1px solid rgba(0,0,0,0.12)' : 'none',
                    }}
                  />
                );
              });

              return (
                <>
                  {/* Left face */}
                  <div
                    className="constructor-soil-wall"
                    style={{
                      position: 'absolute',
                      left: `${leftCorner.left}px`,
                      top: `${leftCorner.top - TILE_HEIGHT / 2}px`,
                      width: `${wallWidth}px`,
                      transformOrigin: 'top left',
                      transform: `skewY(${skewAngle}deg)`,
                      overflow: 'hidden',
                      zIndex: 0,
                      filter: 'brightness(0.86)',
                    }}
                  >
                    {buildStripes('left')}
                  </div>
                  {/* Right face */}
                  <div
                    className="constructor-soil-wall"
                    style={{
                      position: 'absolute',
                      left: `${bottomTip.left}px`,
                      top: `${bottomTip.top - TILE_HEIGHT / 2}px`,
                      width: `${wallWidth}px`,
                      transformOrigin: 'top left',
                      transform: `skewY(-${skewAngle}deg)`,
                      overflow: 'hidden',
                      zIndex: 0,
                      filter: 'brightness(0.74)',
                    }}
                  >
                    {buildStripes('right')}
                  </div>
                </>
              );
            })()}
          </div>
          </div>
        </div>

        <div className="constructor-bottom-panels">
          {appliedSoilFormula && sortedSoilLayers.length > 0 && (
            <section className="constructor-soil-layers-section">
              <h2>Шари ґрунту: {appliedSoilFormula.name}</h2>
              <div className="constructor-soil-layers-preview">
                {sortedSoilLayers.map((layer, index) => {
                  const hexColor = layer.soilType?.hexColor || '#8c7b64';
                  return (
                    <div
                      key={`${layer.soilType?.id || layer.soilTypeId}-${index}`}
                      className="constructor-soil-layer-slice"
                      style={{
                        backgroundColor: hexColor,
                        height: `${Math.max(20, (layer.percentage / 100) * 150)}px`,
                      }}
                      title={`${layer.soilType?.name || 'Невідомий шар'} (${layer.percentage}%)`}
                    >
                      <span className="constructor-soil-layer-label">
                        {layer.soilType?.name || 'Невідомий шар'} — {layer.percentage}%
                      </span>
                    </div>
                  );
                })}
              </div>
            </section>
          )}

          <section className="constructor-placed-list">
            <h2>Додані елементи</h2>
            {placedItems.length === 0 && <p className="constructor-muted">Ще нічого не додано на сітку.</p>}
            {placedItems.length > 0 && (
              <ul>
                {placedItems.map((item) => (
                  <li key={item.instanceId}>
                    <span>{item.name}</span>
                    <span>{item.row + 1}:{item.col + 1}</span>
                    <span>{item.size}x{item.size}</span>
                    <button type="button" onClick={() => removePlacedItem(item.instanceId)}>Видалити</button>
                  </li>
                ))}
              </ul>
            )}
          </section>
        </div>
      </div>
    </section>
  );
}

export default ConstructorPage;
