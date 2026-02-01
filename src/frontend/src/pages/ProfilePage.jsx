import './ProfilePage.css';

function ProfilePage() {
  const stats = [
    { label: 'Збережені флораріуми', value: 4 },
    { label: 'Створено ескізів', value: 12 },
    { label: 'Останнє оновлення', value: '2 дні тому' },
  ];

  const terrariums = [
    {
      id: 't-001',
      name: 'Лісова тераса',
      size: 'Small',
      updatedAt: '24 січня 2026',
    },
    {
      id: 't-002',
      name: 'Скляний сад',
      size: 'Medium',
      updatedAt: '20 січня 2026',
    },
    {
      id: 't-003',
      name: 'Тропічний міні',
      size: 'Large',
      updatedAt: '16 січня 2026',
    },
  ];

  return (
    <div className="profile-page">
      <section className="profile-hero">
        <div>
          <p className="profile-tag">Профіль</p>
          <h1>Вітаємо, Маріє!</h1>
          <p className="profile-subtitle">
            Тут зібрані ваші флораріуми, ескізи та персональні налаштування.
          </p>
        </div>
        <div className="profile-actions">
          <button type="button">Створити новий</button>
          <button type="button" className="ghost">
            Редагувати профіль
          </button>
        </div>
      </section>

      <section className="profile-grid">
        {stats.map((stat) => (
          <div className="profile-stat" key={stat.label}>
            <span>{stat.label}</span>
            <strong>{stat.value}</strong>
          </div>
        ))}
      </section>

      <section className="profile-section">
        <div className="section-header">
          <h2>Збережені флораріуми</h2>
          <button type="button" className="ghost">
            Перейти в каталог
          </button>
        </div>
        <div className="profile-list">
          {terrariums.map((item) => (
            <article className="profile-card" key={item.id}>
              <div>
                <h3>{item.name}</h3>
                <p>Розмір: {item.size}</p>
              </div>
              <div className="card-meta">
                <span>Оновлено</span>
                <strong>{item.updatedAt}</strong>
              </div>
            </article>
          ))}
        </div>
      </section>

      <section className="profile-section">
        <div className="section-header">
          <h2>Налаштування профілю</h2>
        </div>
        <div className="profile-settings">
          <div>
            <span className="setting-label">Email</span>
            <strong>maria@example.com</strong>
          </div>
          <div>
            <span className="setting-label">Телефон</span>
            <strong>+38 (097) 123-45-67</strong>
          </div>
          <div>
            <span className="setting-label">План</span>
            <strong>Free</strong>
          </div>
        </div>
      </section>
    </div>
  );
}

export default ProfilePage;
