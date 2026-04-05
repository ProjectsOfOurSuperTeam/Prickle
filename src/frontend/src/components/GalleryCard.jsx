import { useNavigate } from 'react-router-dom';
import { HiColorSwatch } from 'react-icons/hi';

function resolveProjectImage(item) {
  const imageData = item.generatedFlorariumImage ?? item.preview;
  if (!imageData) {
    return null;
  }

  const mimeType = item.generatedFlorariumImage
    ? item.generatedFlorariumImageMimeType ?? 'image/png'
    : 'image/png';

  return `data:${mimeType};base64,${imageData}`;
}

function getStatusCopy(status) {
  switch (status) {
    case 'Pending':
      return 'Зображення генерується на сервері';
    case 'Succeeded':
      return 'Зображення збережено на сервері';
    case 'Failed':
      return 'Генерація завершилась помилкою';
    default:
      return null;
  }
}

// GalleryCard: картка опублікованого флораріуму (ProjectResponse)
function GalleryCard({ item, highlighted = false }) {
  const navigate = useNavigate();
  const formattedDate = item.createdAt
    ? new Date(item.createdAt).toLocaleDateString('uk-UA', { day: 'numeric', month: 'long', year: 'numeric' })
    : null;
  const allItems = Array.isArray(item.items) ? item.items : [];
  const plantsCount = allItems.filter(i => i.itemType === 'Plant').length;
  const decoCount = allItems.filter(i => i.itemType === 'Decoration').length;
  const shortId = item.id?.slice(-6)?.toUpperCase() ?? '';
  const imageSrc = resolveProjectImage(item);
  const statusCopy = getStatusCopy(item.florariumImageGenerationStatus);

  function handleReproduce() {
    navigate('/constructor', { state: { fromProject: item } });
  }

  return (
    <div style={{ background: '#fff', borderRadius: '12px', boxShadow: highlighted ? '0 0 0 2px #3f7a47, 0 12px 24px rgba(44,62,80,0.12)' : '0 2px 8px rgba(44,62,80,0.08)', padding: '1.2rem', display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '1rem' }}>
      {imageSrc ? (
        <img src={imageSrc} alt={`Флораріум #${shortId}`} style={{ width: '100%', maxWidth: 320, borderRadius: '8px', objectFit: 'cover' }} />
      ) : (
        <div style={{ width: '100%', maxWidth: 320, height: 200, borderRadius: '8px', background: '#f0f4ef', display: 'flex', alignItems: 'center', justifyContent: 'center', color: '#aaa', fontSize: '0.9rem' }}>
          {item.florariumImageGenerationStatus === 'Pending' ? 'Генерація триває…' : 'Без превʼю'}
        </div>
      )}
      <h3 style={{ fontSize: '1.1rem', fontWeight: 600, margin: 0 }}>Флораріум #{shortId}</h3>
      <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '0.25rem', width: '100%' }}>
        {formattedDate && <p style={{ color: '#555', fontSize: '0.9rem', margin: 0 }}>📅 {formattedDate}</p>}
        {statusCopy && (
          <p style={{ color: item.florariumImageGenerationStatus === 'Failed' ? '#b42318' : '#3f7a47', fontSize: '0.9rem', margin: 0, textAlign: 'center' }}>
            {statusCopy}
          </p>
        )}
        {item.florariumImageGenerationStatus === 'Failed' && item.florariumImageGenerationError && (
          <p style={{ color: '#b42318', fontSize: '0.85rem', margin: 0, textAlign: 'center' }}>
            {item.florariumImageGenerationError}
          </p>
        )}
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
        style={{ marginTop: '0.25rem', padding: '0.5rem 1.2rem', borderRadius: '8px', border: 'none', background: '#3f7a47', color: '#fff', fontWeight: 600, fontSize: '0.95rem', cursor: 'pointer' }}
      >
        Відтворити
      </button>
    </div>
  );
}

export default GalleryCard;
