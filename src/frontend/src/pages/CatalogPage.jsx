import { useEffect, useState } from 'react';
import axios from 'axios';
import CatalogFilters from '../components/CatalogFilters';
import CatalogItemsList from '../components/CatalogItemsList';
import Pagination from '../components/Pagination';

function CatalogPage() {
  const [filters, setFilters] = useState({});
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 9;

  useEffect(() => {
    setCurrentPage(1);
  }, [filters.category, filters.plantCategory, filters.lightLevel, filters.waterNeed, filters.humidityLevel, filters.plantSize, filters.containerForm, filters.containerVolume, filters.isClosed, filters.search]);

  useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      setError(null);
      try {
        const params = {};
        if (filters.search) params.name = filters.search;

        const [plantsRes, containersRes] = await Promise.all([
          filters.category === 'container'
            ? Promise.resolve({ data: { items: [] } })
            : axios.get('/api/plants', { params }),
          filters.category === 'plant'
            ? Promise.resolve({ data: { items: [] } })
            : axios.get('/api/containers', { params })
        ]);

        const plants = (plantsRes.data.items || []).map(p => ({ ...p, _category: 'plant' }));
        const containers = (containersRes.data.items || []).map(c => ({ ...c, _category: 'container' }));
        setItems([...plants, ...containers]);
      } catch {
        setError('Не вдалося завантажити каталог.');
        setItems([]);
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, [filters.category, filters.search]);

  const getContainerVolumeBucket = (volume) => {
    if (volume <= 2.5) return 'small';
    if (volume <= 4) return 'medium';
    return 'large';
  };

  const filteredItems = items.filter(item => {
    if (filters.category === 'plant' && item._category !== 'plant') return false;
    if (filters.category === 'container' && item._category !== 'container') return false;

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
