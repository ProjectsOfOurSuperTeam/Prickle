import { useEffect, useState } from 'react';
import { useLocation, useSearchParams } from 'react-router-dom';
import { useApi } from '../services/useApi';
import GalleryCard from '../components/GalleryCard';
import Pagination from '../components/Pagination';

const MOCK_PROJECTS = [
  {
    id: 'a1b2c3d4-e5f6-7890-abcd-ef1234567801',
    userId: 'user-001',
    containerId: 'c0ffe0-0001-0000-0000-000000000001',
    preview: null,
    createdAt: '2026-03-01T11:20:00Z',
    isPublished: true,
    items: [
      { id: 'i-001', projectId: 'a1b2c3d4-e5f6-7890-abcd-ef1234567801', itemType: 'Plant',      itemId: 'plant-elegans',    posX: 0, posY: 0, posZ: 0 },
      { id: 'i-002', projectId: 'a1b2c3d4-e5f6-7890-abcd-ef1234567801', itemType: 'Plant',      itemId: 'plant-fasciata',   posX: 0, posY: 2, posZ: 0 },
      { id: 'i-003', projectId: 'a1b2c3d4-e5f6-7890-abcd-ef1234567801', itemType: 'Plant',      itemId: 'plant-rubrotinctum', posX: 2, posY: 0, posZ: 0 },
      { id: 'i-004', projectId: 'a1b2c3d4-e5f6-7890-abcd-ef1234567801', itemType: 'Decoration', itemId: 'deco-біла морська',  posX: 1, posY: 1, posZ: 0 },
      { id: 'i-005', projectId: 'a1b2c3d4-e5f6-7890-abcd-ef1234567801', itemType: 'Decoration', itemId: 'deco-яшма',          posX: 2, posY: 2, posZ: 0 },
    ],
  },
  {
    id: 'a1b2c3d4-e5f6-7890-abcd-ef1234567802',
    userId: 'user-002',
    containerId: 'c0ffe0-0001-0000-0000-000000000002',
    preview: null,
    createdAt: '2026-03-08T09:00:00Z',
    isPublished: true,
    items: [
      { id: 'i-006', projectId: 'a1b2c3d4-e5f6-7890-abcd-ef1234567802', itemType: 'Plant',      itemId: 'plant-hobbit',    posX: 1, posY: 1, posZ: 0 },
      { id: 'i-007', projectId: 'a1b2c3d4-e5f6-7890-abcd-ef1234567802', itemType: 'Decoration', itemId: 'deco-вулканічна',  posX: 0, posY: 0, posZ: 0 },
    ],
  },
  {
    id: 'a1b2c3d4-e5f6-7890-abcd-ef1234567803',
    userId: 'user-001',
    containerId: 'c0ffe0-0001-0000-0000-000000000003',
    preview: null,
    createdAt: '2026-03-14T16:45:00Z',
    isPublished: true,
    items: [],
  },
];

const PAGE_SIZE = 6;

function GalleryPage() {
  const api = useApi();
  const location = useLocation();
  const [searchParams] = useSearchParams();
  const highlightProjectId = searchParams.get('highlightProjectId');
  const generationStarted = location.state?.generationStarted || searchParams.get('generationStarted') === '1';

  const [items, setItems] = useState([]);
  const [highlightedProject, setHighlightedProject] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [notice, setNotice] = useState(
    generationStarted
      ? 'Генерацію запущено. Картка оновиться автоматично, щойно сервер збереже зображення.'
      : ''
  );
  const [currentPage, setCurrentPage] = useState(1);

  useEffect(() => {
    let cancelled = false;
    const fetchData = async () => {
      setLoading(true);
      setError(null);
      try {
        const result = await api.projects.getAll({ isPublished: true, pageSize: 25 });
        let focusedProject = null;

        if (highlightProjectId) {
          try {
            focusedProject = await api.projects.get(highlightProjectId);
          } catch {
            focusedProject = null;
          }
        }

        if (!cancelled) {
          setItems(result.items ?? []);
          setHighlightedProject(focusedProject);
        }
      } catch {
        if (!cancelled) {
          // API недоступне — використовуємо мок дані
          setItems(MOCK_PROJECTS);
          setHighlightedProject(null);
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    };
    fetchData();
    return () => { cancelled = true; };
  }, [api, highlightProjectId]);

  useEffect(() => {
    if (!highlightProjectId || !highlightedProject || highlightedProject.florariumImageGenerationStatus !== 'Pending') {
      return undefined;
    }

    const intervalId = window.setInterval(async () => {
      try {
        const nextProject = await api.projects.get(highlightProjectId);
        setHighlightedProject(nextProject);

        if (nextProject.florariumImageGenerationStatus === 'Succeeded') {
          setNotice('Зображення готове та збережене на сервері. Воно вже доступне в галереї.');
        }

        if (nextProject.florariumImageGenerationStatus === 'Failed') {
          setNotice(`Генерація завершилась помилкою: ${nextProject.florariumImageGenerationError || 'невідома помилка'}`);
        }
      } catch {
        setNotice('Не вдалося оновити статус генерації. Спробуйте перезавантажити галерею.');
      }
    }, 5000);

    return () => {
      window.clearInterval(intervalId);
    };
  }, [api, highlightProjectId, highlightedProject]);

  const mergedItems = highlightedProject
    ? [highlightedProject, ...items.filter(item => item.id !== highlightedProject.id)]
    : items;

  const totalPages = Math.ceil(mergedItems.length / PAGE_SIZE);
  const pagedItems = mergedItems.slice((currentPage - 1) * PAGE_SIZE, currentPage * PAGE_SIZE);

  return (
    <div style={{ padding: '0 2rem' }}>
      <h1 style={{ marginBottom: '1.5rem' }}>Галерея флораріумів</h1>
      {notice && (
        <div style={{ marginBottom: '1rem', padding: '0.9rem 1rem', borderRadius: '12px', background: '#edf7ee', color: '#23462a' }}>
          {notice}
        </div>
      )}
      {loading && <div>Завантаження...</div>}
      {error && <div>Помилка: {error}</div>}
      {!loading && !error && (
        <>
          {mergedItems.length === 0 ? (
            <div style={{ color: '#888' }}>Поки що немає опублікованих флораріумів.</div>
          ) : (
            <>
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(260px, 1fr))', gap: '2rem' }}>
                {pagedItems.map(item => <GalleryCard key={item.id} item={item} highlighted={item.id === highlightProjectId} />)}
              </div>
              <Pagination currentPage={currentPage} totalPages={totalPages} onPageChange={setCurrentPage} />
            </>
          )}
        </>
      )}
    </div>
  );
}

export default GalleryPage;
