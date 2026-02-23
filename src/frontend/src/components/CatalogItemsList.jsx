import CatalogItemCard from './CatalogItemCard';
import './CatalogItemsList.css';

function CatalogItemsList({ items }) {
  return (
    <section className="catalog-section">
      <h2 className="catalog-section__title">Товари</h2>
      <div className="catalog-section__grid">
        {items && items.length > 0 ? (
          items.map((item) => (
            <CatalogItemCard key={item.id} item={item} />
          ))
        ) : (
          <p className="catalog-section__empty">Нічого не знайдено</p>
        )}
      </div>
    </section>
  );
}

export default CatalogItemsList;