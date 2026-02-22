// GalleryCard: картка флораріуму
function GalleryCard({ item }) {
  return (
    <div style={{ background: '#fff', borderRadius: '12px', boxShadow: '0 2px 8px rgba(44,62,80,0.08)', padding: '1.2rem', display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '1rem' }}>
      <img src={item.imageUrl} alt={item.name} style={{ width: '100%', maxWidth: 220, borderRadius: '8px', objectFit: 'cover', marginBottom: '0.5rem' }} />
      <h3 style={{ fontSize: '1.1rem', fontWeight: 600, margin: 0 }}>{item.name}</h3>
      <p style={{ color: '#555', fontSize: '0.97rem', margin: 0 }}>{item.description}</p>
    </div>
  );
}

export default GalleryCard;

