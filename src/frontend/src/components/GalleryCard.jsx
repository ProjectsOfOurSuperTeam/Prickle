import { useNavigate } from 'react-router-dom';
import { HiColorSwatch } from 'react-icons/hi';

// GalleryCard: картка опублікованого флораріуму (ProjectResponse)
function GalleryCard({ item }) {
  const navigate = useNavigate();
  const formattedDate = item.createdAt
    ? new Date(item.createdAt).toLocaleDateString('uk-UA', { day: 'numeric', month: 'long', year: 'numeric' })
    : null;
  const allItems = Array.isArray(item.items) ? item.items : [];
  const plantsCount = allItems.filter(i => i.itemType === 'Plant').length;
  const decoCount = allItems.filter(i => i.itemType === 'Decoration').length;
  const shortId = item.id?.slice(-6)?.toUpperCase() ?? '';

  function handleReproduce() {
    navigate('/constructor', { state: { fromProject: item } });
  }

  return (
    <div style={{ background: '#fff', borderRadius: '12px', boxShadow: '0 2px 8px rgba(44,62,80,0.08)', padding: '1.2rem', display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '1rem' }}>
      {item.preview ? (
        <img src={item.preview} alt={`Флораріум #${shortId}`} style={{ width: '100%', maxWidth: 320, borderRadius: '8px', objectFit: 'cover' }} />
      ) : (
        <div style={{ width: '100%', maxWidth: 320, height: 200, borderRadius: '8px', background: '#f0f4ef', display: 'flex', alignItems: 'center', justifyContent: 'center', color: '#aaa', fontSize: '0.9rem' }}>
          Без прев'ю
        </div>
      )}
      <h3 style={{ fontSize: '1.1rem', fontWeight: 600, margin: 0 }}>Флораріум #{shortId}</h3>
      <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '0.25rem', width: '100%' }}>
        {formattedDate && <p style={{ color: '#555', fontSize: '0.9rem', margin: 0 }}>📅 {formattedDate}</p>}
        {allItems.length > 0 ? (
          <>
            {plantsCount > 0 && <p style={{ color: '#555', fontSize: '0.9rem', margin: 0 }}>🌿 Рослин: {plantsCount}</p>}
            {decoCount > 0 && (
              <p
                style={{
                  color: '#555',
                  fontSize: '0.9rem',
                  margin: 0,
                  display: 'inline-flex',
                  alignItems: 'center',
                  gap: '0.35rem',
                }}
              >
                <HiColorSwatch aria-hidden style={{ flexShrink: 0, fontSize: '1.05rem', color: '#5c6bc0' }} />
                <span>Декору: {decoCount}</span>
              </p>
            )}
          </>
        ) : (
          <p style={{ color: '#aaa', fontSize: '0.9rem', margin: 0 }}>Елементів: 0</p>
        )}
      </div>
      <button
        type="button"
        onClick={handleReproduce}
        style={{ marginTop: '0.25rem', padding: '0.5rem 1.2rem', borderRadius: '8px', border: 'none', background: '#6c63ff', color: '#fff', fontWeight: 600, fontSize: '0.95rem', cursor: 'pointer' }}
      >
        Відтворити
      </button>
    </div>
  );
}

export default GalleryCard;
