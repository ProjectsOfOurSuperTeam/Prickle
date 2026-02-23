import './CatalogFilters.css';

const PLANT_CATEGORIES = [
  { value: 'Succulents', label: 'Сукуленти' },
  { value: 'Cacti', label: 'Кактуси' },
  { value: 'Tropical', label: 'Тропічні' },
  { value: 'Ferns', label: 'Папороті' },
  { value: 'Mosses', label: 'Мохи' },
  { value: 'CarnivorousPlants', label: 'Хижі рослини' },
];

const LIGHT_LEVELS = [
  { value: 'VeryLow', label: 'Дуже низьке' },
  { value: 'Low', label: 'Низьке' },
  { value: 'Medium', label: 'Середнє' },
  { value: 'High', label: 'Високе' },
  { value: 'VeryHigh', label: 'Дуже високе' },
];

const WATER_NEEDS = [
  { value: 'VeryLow', label: 'Дуже рідко' },
  { value: 'Low', label: 'Рідко' },
  { value: 'Medium', label: 'Помірно' },
  { value: 'High', label: 'Часто' },
  { value: 'VeryHigh', label: 'Дуже часто' },
];

const HUMIDITY_LEVELS = [
  { value: 'VeryLow', label: 'Дуже низька' },
  { value: 'Low', label: 'Низька' },
  { value: 'Medium', label: 'Середня' },
  { value: 'High', label: 'Висока' },
  { value: 'VeryHigh', label: 'Дуже висока' },
];

const PLANT_SIZES = [
  { value: 'Small', label: 'Малий' },
  { value: 'Medium', label: 'Середній' },
  { value: 'Large', label: 'Великий' },
  { value: 'ExtraLarge', label: 'Дуже великий' },
];

const CONTAINER_VOLUME_OPTIONS = [
  { value: 'small', label: 'Малий (до 2.5 л)' },
  { value: 'medium', label: 'Середній (до 4 л)' },
  { value: 'large', label: 'Великий (понад 4 л)' },
];

const CONTAINER_FORMS = [
  'Ікосаедр (Геометрія)',
  'Флораріум "Крапля"',
  'Мінімалістичний Куб',
  'Еко-Сфера (Закрита)',
  'Додекаедр XL',
  'Велика Піраміда',
  'Призма "Кристал"',
  'Чаша "Лотос"',
  'Лабораторний Циліндр',
  'Пісочний годинник',
];

function CatalogFilters({ filters, onChange }) {
  const handle = (key) => (e) => onChange({ ...filters, [key]: e.target.value });

  return (
    <aside className="catalog-filters">
      <h2 className="catalog-filters__title">Фільтри</h2>
      <div className="catalog-filters__group">
        <label className="catalog-filters__label">
          Пошук:
          <input className="catalog-filters__input" type='text' value={filters.search || ''} onChange={handle('search')} placeholder='Назва...' />
        </label>
      </div>
      <div className="catalog-filters__group">
        <label className="catalog-filters__label">
          Категорія:
          <select className="catalog-filters__select" value={filters.category || ''} onChange={handle('category')}>
            <option value=''>Всі</option>
            <option value='plant'>Рослини</option>
            <option value='container'>Контейнери</option>
          </select>
        </label>
        {filters.category === 'plant' && (
          <>
            <label className="catalog-filters__label">
              Категорія рослини:
              <select className="catalog-filters__select" value={filters.plantCategory || ''} onChange={handle('plantCategory')}>
                <option value=''>Всі</option>
                {PLANT_CATEGORIES.map(c => <option key={c.value} value={c.value}>{c.label}</option>)}
              </select>
            </label>
            <label className="catalog-filters__label">
              Освітлення:
              <select className="catalog-filters__select" value={filters.lightLevel || ''} onChange={handle('lightLevel')}>
                <option value=''>Всі</option>
                {LIGHT_LEVELS.map(l => <option key={l.value} value={l.value}>{l.label}</option>)}
              </select>
            </label>
            <label className="catalog-filters__label">
              Потреба у воді:
              <select className="catalog-filters__select" value={filters.waterNeed || ''} onChange={handle('waterNeed')}>
                <option value=''>Всі</option>
                {WATER_NEEDS.map(w => <option key={w.value} value={w.value}>{w.label}</option>)}
              </select>
            </label>
            <label className="catalog-filters__label">
              Вологість:
              <select className="catalog-filters__select" value={filters.humidityLevel || ''} onChange={handle('humidityLevel')}>
                <option value=''>Всі</option>
                {HUMIDITY_LEVELS.map(h => <option key={h.value} value={h.value}>{h.label}</option>)}
              </select>
            </label>
            <label className="catalog-filters__label">
              Розмір:
              <select className="catalog-filters__select" value={filters.plantSize || ''} onChange={handle('plantSize')}>
                <option value=''>Всі</option>
                {PLANT_SIZES.map(s => <option key={s.value} value={s.value}>{s.label}</option>)}
              </select>
            </label>
          </>
        )}
        {filters.category === 'container' && (
          <>
            <label className="catalog-filters__label">
              Форма:
              <select className="catalog-filters__select" value={filters.containerForm || ''} onChange={handle('containerForm')}>
                <option value=''>Всі</option>
                {CONTAINER_FORMS.map(f => <option key={f} value={f}>{f}</option>)}
              </select>
            </label>
            <label className="catalog-filters__label">
              Обʼєм:
              <select className="catalog-filters__select" value={filters.containerVolume || ''} onChange={handle('containerVolume')}>
                <option value=''>Всі</option>
                {CONTAINER_VOLUME_OPTIONS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
              </select>
            </label>
            <label className="catalog-filters__label">
              Тип:
              <select className="catalog-filters__select" value={filters.isClosed ?? ''} onChange={handle('isClosed')}>
                <option value=''>Всі</option>
                <option value='true'>Закритий</option>
                <option value='false'>Відкритий</option>
              </select>
            </label>
          </>
        )}
      </div>
    </aside>
  );
}

export default CatalogFilters;
