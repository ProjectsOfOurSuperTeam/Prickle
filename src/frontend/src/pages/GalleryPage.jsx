import { useEffect, useState } from 'react';
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
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [currentPage, setCurrentPage] = useState(1);

  useEffect(() => {
    let cancelled = false;
    const fetchData = async () => {
      setLoading(true);
      setError(null);
      try {
        const result = await api.projects.getAll({ isPublished: true, pageSize: 25 });
        if (!cancelled) {
          setItems(result.items ?? []);
        }
      } catch {
        if (!cancelled) {
          // API недоступне — використовуємо мок дані
          setItems(MOCK_PROJECTS);
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    };
    fetchData();
    return () => { cancelled = true; };
  }, [api]);

  const totalPages = Math.ceil(items.length / PAGE_SIZE);
  const pagedItems = items.slice((currentPage - 1) * PAGE_SIZE, currentPage * PAGE_SIZE);

  return (
    <div style={{ padding: '0 2rem' }}>
      <h1 style={{ marginBottom: '1.5rem' }}>Галерея флораріумів</h1>
      {loading && <div>Завантаження...</div>}
      {error && <div>Помилка: {error}</div>}
      {!loading && !error && (
        <>
          {items.length === 0 ? (
            <div style={{ color: '#888' }}>Поки що немає опублікованих флораріумів.</div>
          ) : (
            <>
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(260px, 1fr))', gap: '2rem' }}>
                {pagedItems.map(item => <GalleryCard key={item.id} item={item} />)}
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
