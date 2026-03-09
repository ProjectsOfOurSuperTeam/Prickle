import { useEffect, useMemo, useRef, useState } from 'react';
import { useApi } from '../services/useApi';
import './ConstructorPage.css';

const GRID_PRESETS = [3, 5, 7, 9];
const TILE_WIDTH = 94;
const TILE_HEIGHT = 52;
const GRID_TOP_OFFSET = TILE_HEIGHT * 1.4;
const MAX_PAGE_SIZE = 25;
const SOIL_KINDS = new Set(['soilType', 'soilFormula']);

const CATALOG_TABS = [
  { key: 'plants', label: 'Рослини' },
  { key: 'soilTypes', label: 'Типи грунту' },
  { key: 'soilFormulas', label: 'Формули грунту' },
  { key: 'decorations', label: 'Декор' },
  { key: 'containers', label: 'Контейнери' },
];

const SOIL_COLOR_GRAY = [126, 121, 110];
const SOIL_COLOR_YELLOW = [182, 154, 88];
const SOIL_COLOR_BROWN = [124, 88, 52];

function resolveLayer(kind) {
  return SOIL_KINDS.has(kind) ? 'soil' : 'objects';
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

function estimatePlantFootprint(plant) {
  if (plant.itemMaxSize <= 10) return 1;
  if (plant.itemMaxSize <= 25) return 2;
  return 3;
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

function ConstructorPage() {
  const api = useApi();
  const boardRef = useRef(null);
  const boardWrapRef = useRef(null);
  const panRef = useRef({ active: false, startX: 0, startY: 0, scrollLeft: 0, scrollTop: 0 });
  const suppressBoardClickRef = useRef(false);
  const hoverRevealTimerRef = useRef(null);

  const [gridSize, setGridSize] = useState(5);
  const [activeTab, setActiveTab] = useState('plants');
  const [search, setSearch] = useState('');
  const [selectedCatalogItem, setSelectedCatalogItem] = useState(null);

  const [plants, setPlants] = useState([]);
  const [soilTypes, setSoilTypes] = useState([]);
  const [soilFormulas, setSoilFormulas] = useState([]);
  const [decorations, setDecorations] = useState([]);
  const [containers, setContainers] = useState([]);

  const [placedItems, setPlacedItems] = useState([]);
  const [dragHoverCell, setDragHoverCell] = useState(null);
  const [notice, setNotice] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [isPanning, setIsPanning] = useState(false);
  const [revealedItemId, setRevealedItemId] = useState(null);

  useEffect(() => {
    return () => {
      if (hoverRevealTimerRef.current) {
        window.clearTimeout(hoverRevealTimerRef.current);
      }
    };
  }, []);

  useEffect(() => {
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
  }, [api]);

  const catalogItemsByType = useMemo(() => {
    return {
      plants: plants.map((item) => ({
        id: `plant-${item.id}`,
        entityId: String(item.id),
        kind: 'plant',
        name: item.name,
        subtitle: item.nameLatin,
        details: `Світло ${item.lightLevel}/5 • Вода ${item.waterNeed}/5`,
        footprint: estimatePlantFootprint(item),
        image: resolveImageUrl(item.imageIsometricUrl || item.imageUrl),
        layer: resolveLayer('plant'),
      })),
      soilTypes: soilTypes.map((item) => ({
        id: `soilType-${item.id}`,
        entityId: String(item.id),
        kind: 'soilType',
        name: item.name,
        subtitle: 'Базовий тип грунту',
        details: 'Рекомендований нижній шар',
        footprint: 1,
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
        footprint: 2,
        image: null,
        layer: resolveLayer('soilFormula'),
      })),
      decorations: decorations.map((item) => ({
        id: `decoration-${item.id}`,
        entityId: String(item.id),
        kind: 'decoration',
        name: item.name,
        subtitle: `Категорія #${item.category}`,
        details: item.description || 'Декоративний елемент',
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
        footprint: estimateContainerFootprint(item),
        image: resolveImageUrl(item.imageIsometricUrl || item.imageUrl),
        layer: resolveLayer('container'),
      })),
    };
  }, [containers, decorations, plants, soilFormulas, soilTypes]);

  const visibleCatalogItems = useMemo(() => {
    const items = catalogItemsByType[activeTab] || [];
    const query = search.trim().toLowerCase();

    if (!query) return items;

    return items.filter((item) => {
      return (
        item.name.toLowerCase().includes(query)
        || (item.subtitle || '').toLowerCase().includes(query)
        || (item.details || '').toLowerCase().includes(query)
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

  const soilCellColors = useMemo(() => {
    const colors = new Map();

    placedItems
      .filter((item) => item.layer === 'soil')
      .forEach((item) => {
        const gradient = getSoilGradientByKey(`${item.type}:${item.entityId}:${item.name}`);
        for (let row = item.row; row < item.row + item.size; row += 1) {
          for (let col = item.col; col < item.col + item.size; col += 1) {
            colors.set(`${row}-${col}`, gradient);
          }
        }
      });

    return colors;
  }, [placedItems]);

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

  function handleDragStart(event, item) {
    event.dataTransfer.effectAllowed = 'copy';
    event.dataTransfer.setData('application/prickle-item', JSON.stringify(item));
    event.dataTransfer.setData('text/plain', item.name);
  }

  function handleDragOver(event) {
    event.preventDefault();
    if (!boardRef.current) return;

    const rect = boardRef.current.getBoundingClientRect();
    const localX = event.clientX - rect.left;
    const localY = event.clientY - rect.top;
    const candidate = toGridCell(localX, localY, (gridSize * TILE_WIDTH) / 2, GRID_TOP_OFFSET);

    setDragHoverCell(candidate);
  }

  function handlePlacedDragStart(event, item) {
    event.stopPropagation();
    event.dataTransfer.effectAllowed = 'move';
    event.dataTransfer.setData('application/prickle-move', JSON.stringify({
      instanceId: item.instanceId,
      size: item.size,
      layer: item.layer,
    }));
    event.dataTransfer.setData('text/plain', item.name);
  }

  function placeItem(payload, candidate, excludingId = null) {
    if (!canPlaceItem(candidate, payload.layer, excludingId)) {
      return false;
    }

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
    if (!selectedCatalogItem) return;
    if (!boardRef.current) return;
    if (suppressBoardClickRef.current) {
      suppressBoardClickRef.current = false;
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
      setNotice(`Шар ${selectedCatalogItem.layer === 'soil' ? 'грунту' : "об'єктів"} зайнятий або вихід за межі.`);
      return;
    }

    setNotice('');
  }

  function handleDrop(event) {
    event.preventDefault();
    setNotice('');

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

    const candidate = toPlacementCandidate(
      localX,
      localY,
      payload.footprint,
      (gridSize * TILE_WIDTH) / 2,
      GRID_TOP_OFFSET,
    );

    if (!canPlaceItem(candidate, payload.layer)) {
      setNotice(`Шар ${payload.layer === 'soil' ? 'грунту' : "об'єктів"} зайнятий або вихід за межі.`);
      return;
    }

    placeItem(payload, candidate);
    setDragHoverCell(null);
    suppressBoardClickRef.current = true;
  }

  function handleDragLeaveBoard(event) {
    if (!boardRef.current?.contains(event.relatedTarget)) {
      setDragHoverCell(null);
    }
  }

  function handlePanStart(event) {
    if (!boardWrapRef.current) return;
    if (event.button !== 0) return;
    if (event.target.closest('.placed-item-remove')) return;
    if (event.target.closest('.placed-item')) return;

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
    setPlacedItems((prev) => prev.filter((item) => item.instanceId !== instanceId));
  }

  function handlePlacedItemMouseEnter(instanceId) {
    if (hoverRevealTimerRef.current) {
      window.clearTimeout(hoverRevealTimerRef.current);
    }

    hoverRevealTimerRef.current = window.setTimeout(() => {
      setRevealedItemId(instanceId);
    }, 550);
  }

  function handlePlacedItemMouseLeave(instanceId) {
    if (hoverRevealTimerRef.current) {
      window.clearTimeout(hoverRevealTimerRef.current);
      hoverRevealTimerRef.current = null;
    }

    setRevealedItemId((prev) => (prev === instanceId ? null : prev));
  }

  function resetWorkspace() {
    setPlacedItems([]);
    setNotice('');
  }

  return (
    <section className="constructor-page">
      <aside className="constructor-catalog">
        <h1 className="constructor-title">Конструктор флораріуму</h1>
        <p className="constructor-subtitle">Перетягуйте рослини, грунт та декор на ізометричну сітку.</p>

        <label className="constructor-search-wrap">
          <span>Пошук</span>
          <input
            className="constructor-search"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Назва, латина або деталі"
            type="text"
          />
        </label>

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
              className={`constructor-card ${selectedCatalogItem?.id === item.id ? 'constructor-card-selected' : ''}`}
              draggable
              onDragStart={(event) => handleDragStart(event, item)}
              onClick={() => {
                setSelectedCatalogItem((prev) => (prev?.id === item.id ? null : item));
              }}
            >
              <div className="constructor-card-head">
                <h3>{item.name}</h3>
                <span>{item.footprint}x{item.footprint}</span>
              </div>
              {item.subtitle && <p className="constructor-card-subtitle">{item.subtitle}</p>}
              {item.details && <p className="constructor-card-details">{item.details}</p>}
              {item.image && <img src={item.image} alt={item.name} className="constructor-card-image" />}
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

          <div className="constructor-actions">
            <span>Елементів: {placedItems.length}</span>
            <span>Режим: {selectedCatalogItem ? `Клік-плейс (${selectedCatalogItem.name})` : 'Drag-and-drop'}</span>
            <button type="button" onClick={resetWorkspace}>Очистити</button>
          </div>
        </header>

        {notice && <p className="constructor-notice">{notice}</p>}

        <div
          ref={boardWrapRef}
          className={`constructor-board-wrap ${isPanning ? 'constructor-board-wrap-panning' : ''}`}
          onMouseDown={handlePanStart}
          onMouseMove={handlePanMove}
          onMouseUp={stopPan}
          onMouseLeave={stopPan}
        >
          <div
            ref={boardRef}
            className="constructor-board"
            style={{ width: `${boardWidth}px`, height: `${boardHeight}px` }}
            onDragOver={handleDragOver}
            onDrop={handleDrop}
            onDragLeave={handleDragLeaveBoard}
            onClick={handleBoardClick}
          >
            {gridCells.map((cell) => (
              <div
                key={`${cell.row}-${cell.col}`}
                className={`iso-cell ${dragHoverCell?.row === cell.row && dragHoverCell?.col === cell.col ? 'iso-cell-hover' : ''}`}
                style={{
                  left: `${cell.left}px`,
                  top: `${cell.top}px`,
                  ...(soilCellColors.has(`${cell.row}-${cell.col}`)
                    ? {
                      background: `linear-gradient(160deg, ${soilCellColors.get(`${cell.row}-${cell.col}`).start} 0%, ${soilCellColors.get(`${cell.row}-${cell.col}`).end} 100%)`,
                      borderColor: soilCellColors.get(`${cell.row}-${cell.col}`).border,
                    }
                    : {}),
                }}
              />
            ))}

            {placedItemsView.map((item) => (
              <div
                key={item.instanceId}
                className={`placed-item placed-item-${item.type}`}
                draggable
                onDragStart={(event) => handlePlacedDragStart(event, item)}
                onMouseEnter={() => handlePlacedItemMouseEnter(item.instanceId)}
                onMouseLeave={() => handlePlacedItemMouseLeave(item.instanceId)}
                style={{
                  left: `${item.left}px`,
                  top: `${item.top}px`,
                  width: `${item.size * TILE_WIDTH}px`,
                  height: `${item.size * TILE_HEIGHT}px`,
                }}
                title={`${item.name} (${item.size}x${item.size})`}
              >
                <div className="placed-item-footprint" />
                {item.image && <img src={item.image} alt={item.name} className="placed-item-image" />}
                <span
                  className={`placed-item-label ${revealedItemId === item.instanceId && item.layer === 'soil' ? 'placed-item-label-visible' : ''}`}
                >
                  {item.name}
                </span>
                <button
                  type="button"
                  className={`placed-item-remove ${revealedItemId === item.instanceId ? 'placed-item-remove-visible' : ''}`}
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
            ))}
          </div>
        </div>

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
    </section>
  );
}

export default ConstructorPage;
