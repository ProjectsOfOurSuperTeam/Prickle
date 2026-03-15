import { useEffect, useState } from 'react';
import { useApi } from '../services/useApi';
import CatalogFilters from '../components/CatalogFilters';
import CatalogItemsList from '../components/CatalogItemsList';
import Pagination from '../components/Pagination';

function CatalogPage() {
  const api = useApi();
  const [filters, setFilters] = useState({});
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 9;

  useEffect(() => {
    setCurrentPage(1);
  }, [filters.category, filters.plantCategory, filters.lightLevel, filters.waterNeed, filters.humidityLevel, filters.plantSize, filters.containerForm, filters.containerVolume, filters.isClosed, filters.decorationCategory, filters.search]);

  useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      setError(null);
      try {
        const params = filters.search ? { name: filters.search } : {};

        const shouldFetchPlants = !filters.category || filters.category === 'plant';
        const shouldFetchContainers = !filters.category || filters.category === 'container';
        const shouldFetchDecorations = !filters.category || filters.category === 'decoration';

        const [plantsRes, containersRes, decorationsRes] = await Promise.all([
          shouldFetchPlants ? api.plants.getAll(params) : Promise.resolve({ items: [] }),
          shouldFetchContainers ? api.containers.getAll(params) : Promise.resolve({ items: [] }),
          shouldFetchDecorations ? api.decorations.getAll(params) : Promise.resolve({ items: [] }),
        ]);

        const plants = (plantsRes.items || []).map(p => ({ ...p, _category: 'plant' }));
        const containers = (containersRes.items || []).map(c => ({ ...c, _category: 'container' }));
        const decorations = (decorationsRes.items || []).map(d => ({ ...d, _category: 'decoration' }));
        setItems([...plants, ...containers, ...decorations]);
      } catch {
        setError('Не вдалося завантажити каталог.');
        setItems([]);
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, [filters.category, filters.search, api.plants, api.containers, api.decorations]);

  const getContainerVolumeBucket = (volume) => {
    if (volume <= 2.5) return 'small';
    if (volume <= 4) return 'medium';
    return 'large';
  };

  const filteredItems = items.filter(item => {
    if (filters.category && item._category !== filters.category) return false;

    if (item._category === 'plant') {
      if (filters.plantCategory && item.category !== filters.plantCategory) return false;
      if (filters.lightLevel && item.lightLevel !== filters.lightLevel) return false;
      if (filters.waterNeed && item.waterNeed !== filters.waterNeed) return false;
      if (filters.humidityLevel && item.humidityLevel !== filters.humidityLevel) return false;
      if (filters.plantSize && item.itemMaxSize !== filters.plantSize) return false;
    }

    if (item._category === 'container') {
      if (filters.containerForm && item.name !== filters.containerForm) return false;
      if (filters.containerVolume && getContainerVolumeBucket(item.volume) !== filters.containerVolume) return false;
      if (filters.isClosed !== '' && filters.isClosed !== undefined && String(item.isClosed) !== filters.isClosed) return false;
    }

    if (item._category === 'decoration') {
      if (filters.decorationCategory && item.category !== filters.decorationCategory) return false;
    }

    return !filters.search || item.name?.toLowerCase().includes(filters.search.toLowerCase());
  });

  const totalPages = Math.ceil(filteredItems.length / pageSize);
  const pagedItems = filteredItems.slice((currentPage - 1) * pageSize, currentPage * pageSize);

  return (
    <div style={{ display: 'flex', gap: '2rem' }}>
      <div style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem', minWidth: 260 }}>
        <CatalogFilters filters={filters} onChange={setFilters} />
      </div>
      <div style={{ flex: 1 }}>
        {loading && <div>Завантаження...</div>}
        {error && <div>Помилка: {error}</div>}
        {!loading && !error && <>
          <CatalogItemsList items={pagedItems} />
          <Pagination currentPage={currentPage} totalPages={totalPages} onPageChange={setCurrentPage} />
        </>}
      </div>
    </div>
  );
}

export default CatalogPage;
