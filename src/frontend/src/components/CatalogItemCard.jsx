import { useState } from 'react';

const DECORATION_CATEGORY_LABELS = {
  Stones: 'Каміння',
  Sand: 'Пісок',
  Wood: 'Дерево',
  Figurines: 'Фігурки',
  Nature: 'Природа',
  Minerals: 'Мінерали',
};

function CatalogItemCard({ item }) {
  const [showPopup, setShowPopup] = useState(false);
  const MAX_DESC = 100;
  const isLong = item.description && item.description.length > MAX_DESC;
  const shortDesc = isLong ? item.description.slice(0, MAX_DESC) + '...' : item.description;
  const imageSrc = item.imageUrl ? `assets/images/${item.imageUrl}` : null;

  return (
    <div style={{ background: '#fff', borderRadius: '12px', boxShadow: '0 2px 8px rgba(44,62,80,0.08)', padding: '1.2rem', display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '1rem', minWidth: 220, position: 'relative' }}>
      {imageSrc && <img src={imageSrc} alt={item.name} style={{ width: '100%', maxWidth: 180, borderRadius: '8px', objectFit: 'cover', marginBottom: '0.5rem' }} />}
      <h3 style={{ fontSize: '1.1rem', fontWeight: 600, margin: 0 }}>{item.name}</h3>
      {item.description && (
        <>
          <p style={{ color: '#555', fontSize: '0.97rem', margin: 0 }}>
            {shortDesc}
            {isLong && (
              <button style={{ marginLeft: 8, color: '#6c63ff', background: 'none', border: 'none', cursor: 'pointer', textDecoration: 'underline', fontSize: '0.97rem' }} onClick={() => setShowPopup(true)}>
                Докладніше
              </button>
            )}
          </p>
          {showPopup && (
            <div style={{ position: 'fixed', top: 0, left: 0, width: '100vw', height: '100vh', background: 'rgba(44,62,80,0.25)', zIndex: 1000, display: 'flex', alignItems: 'center', justifyContent: 'center' }} onClick={() => setShowPopup(false)}>
              <div style={{ background: '#fff', borderRadius: 12, padding: '2rem', maxWidth: 420, boxShadow: '0 4px 24px rgba(44,62,80,0.18)', position: 'relative', display: 'flex', flexDirection: 'column', alignItems: 'center' }} onClick={e => e.stopPropagation()}>
                <button style={{ position: 'absolute', top: 8, right: 12, background: 'none', border: 'none', fontSize: 22, cursor: 'pointer', color: '#888' }} onClick={() => setShowPopup(false)}>&times;</button>
                {imageSrc && <img src={imageSrc} alt={item.name} style={{ width: 220, maxWidth: '90%', borderRadius: 10, objectFit: 'cover', marginBottom: 16 }} />}
                <h4 style={{ marginTop: 0 }}>{item.name}</h4>
                <div style={{ color: '#444', fontSize: '1rem', whiteSpace: 'pre-line', textAlign: 'center' }}>{item.description}</div>
              </div>
            </div>
          )}
        </>
      )}
      {item.material && <div style={{ fontSize: '0.95rem', color: '#888' }}>Матеріал: {item.material}</div>}
      {item.size && <div style={{ fontSize: '0.95rem', color: '#888' }}>Розмір: {item.size}</div>}
      {item.shape && <div style={{ fontSize: '0.95rem', color: '#888' }}>Форма: {item.shape}</div>}
      {item.type && <div style={{ fontSize: '0.95rem', color: '#888' }}>Тип: {item.type}</div>}
      {item._category === 'decoration' && item.category && (
        <div style={{ fontSize: '0.95rem', color: '#888' }}>Категорія: {DECORATION_CATEGORY_LABELS[item.category] || item.category}</div>
      )}
    </div>
  );
}

export default CatalogItemCard;