import { useEffect, useState } from 'react';
import { Link, Navigate } from 'react-router-dom';
import { useAuth } from '../services/useAuth';
import { useApi } from '../services/useApi';
import { ApiError } from '../services/api/apiError';
import './ProfilePage.css';

function volumeToSize(volume) {
  if (volume <= 2.5) return 'Small';
  if (volume <= 4) return 'Medium';
  return 'Large';
}

function formatCreatedAt(isoDate) {
  try {
    const d = new Date(isoDate);
    return new Intl.DateTimeFormat('uk-UA', {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
    }).format(d);
  } catch {
    return isoDate;
  }
}

function ProfilePage() {
  const { isAuthenticated, user } = useAuth();
  const api = useApi();

  const [projects, setProjects] = useState(null);
  const [containersMap, setContainersMap] = useState(new Map());
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    if (!user?.id) {
      setLoading(false);
      return;
    }

    let cancelled = false;

    async function fetchData() {
      try {
        setLoading(true);
        setError(null);
        const [projectsRes, containersRes] = await Promise.all([
          api.projects.getAll({
            userId: user.id,
            page: 1,
            pageSize: 20,
            sortBy: '-createdat',
          }),
          api.containers.getAll({ pageSize: 25 }),
        ]);

        if (cancelled) return;

        const map = new Map();
        for (const c of containersRes.items ?? []) {
          map.set(c.id, { name: c.name, volume: c.volume });
        }
        setContainersMap(map);
        setProjects(projectsRes);
      } catch (e) {
        if (cancelled) return;
        setError(e instanceof ApiError ? (e.detail ?? 'Не вдалося завантажити дані.') : 'Не вдалося завантажити дані.');
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    fetchData();
    return () => { cancelled = true; };
  }, [user?.id, api.projects, api.containers]);

  if (!isAuthenticated) {
    return <Navigate to="/auth" replace />;
  }

  const stats = (() => {
    if (loading || error || !projects) {
      return [
        { label: 'Збережені флораріуми', value: loading ? '...' : error ? '—' : '0' },
        { label: 'Опубліковано', value: loading ? '...' : error ? '—' : '0' },
        { label: 'Останнє створення', value: loading ? '...' : error ? '—' : '—' },
      ];
    }
    const total = projects.total ?? 0;
    const published = (projects.items ?? []).filter((p) => p.isPublished).length;
    const latest = projects.items?.[0]?.createdAt
      ? formatCreatedAt(projects.items[0].createdAt)
      : '—';
    return [
      { label: 'Збережені флораріуми', value: total },
      { label: 'Опубліковано', value: published },
      { label: 'Останнє створення', value: latest },
    ];
  })();

  const terrariums = (() => {
    if (loading || error || !projects) return [];
    const items = projects.items ?? [];
    return items.map((p) => {
      const c = containersMap.get(p.containerId);
      return {
        id: p.id,
        name: c?.name ?? 'Проєкт',
        size: c != null ? volumeToSize(c.volume) : 'Medium',
        updatedAt: formatCreatedAt(p.createdAt),
      };
    });
  })();

  return (
    <div className="profile-page">
      <section className="profile-hero">
        <div>
          <p className="profile-tag">Профіль</p>
          <h1>Вітаємо, {user?.name || user?.username || user?.email || 'користувачу'}!</h1>
          <p className="profile-subtitle">
            Тут зібрані ваші флораріуми, ескізи та персональні налаштування.
          </p>
        </div>
        <div className="profile-actions">
          <Link to="/constructor" className="profile-action-btn">
            Створити новий
          </Link>
          <Link to="/profile/edit" className="ghost">
            Редагувати профіль
          </Link>
        </div>
      </section>

      {error && (
        <section className="profile-error">
          <p>{error}</p>
        </section>
      )}

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
          <Link to="/catalog" className="ghost">
            Перейти в каталог
          </Link>
        </div>
        <div className="profile-list">
          {loading ? (
            <p className="profile-loading">Завантаження...</p>
          ) : terrariums.length === 0 ? (
            <p className="profile-empty">Немає збережених флораріумів.</p>
          ) : (
            terrariums.map((item) => (
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
            ))
          )}
        </div>
      </section>

      <section className="profile-section">
        <div className="section-header">
          <h2>Налаштування профілю</h2>
        </div>
        <div className="profile-settings">
          <div>
            <span className="setting-label">Email</span>
            <strong>{user?.email ?? '?'}</strong>
          </div>
          <div>
            <span className="setting-label">Телефон</span>
            <strong>{user?.phone ?? '?'}</strong>
          </div>
          <div>
            <span className="setting-label">План</span>
            <strong>{user?.plan ?? '?'}</strong>
          </div>
        </div>
      </section>
    </div>
  );
}

export default ProfilePage;
