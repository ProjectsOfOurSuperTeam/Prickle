import { useEffect, useState } from 'react';
import GalleryCard from '../components/GalleryCard';

function GalleryPage() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    setError(null);
    // Мок дані для тестування
    setLoading(true);
    setTimeout(() => {
      setItems([
        {
          id: 1,
          name: 'Флораріум "Весна"',
          imageUrl: '/public/vite.svg',
          description: 'Яскравий флораріум з весняними рослинами.'
        },
        {
          id: 2,
          name: 'Флораріум "Мінімалізм"',
          imageUrl: '/src/assets/logo/logo.png',
          description: 'Стильний флораріум для сучасного інтер’єру.'
        },
        {
          id: 3,
          name: 'Флораріум "Тропік"',
          imageUrl: '/src/assets/images/plants/plant1.png',
          description: 'Тропічний флораріум з екзотичними рослинами.'
        }
      ]);
      setLoading(false);
    }, 500);
  }, []);

  if (loading) return <div>Завантаження...</div>;
  if (error) return <div>Помилка: {error}</div>;

  return (
    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(260px, 1fr))', gap: '2rem' }}>
      {items.map(item => <GalleryCard key={item.id} item={item} />)}
    </div>
  );
}

export default GalleryPage;
