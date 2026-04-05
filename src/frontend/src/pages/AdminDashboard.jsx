import { useCallback, useEffect, useState } from 'react';
import { useApi } from '../services/useApi';
import { ApiError } from '../services/api/apiError';
import './AdminDashboard.css';

const TABS = [
  { id: 'containers', label: 'Контейнери' },
  { id: 'plants', label: 'Рослини' },
  { id: 'decorations', label: 'Декор' },
  { id: 'soilTypes', label: 'Типи ґрунту' },
  { id: 'soilFormulas', label: 'Формули ґрунту' },
];

const emptyContainerForm = () => ({ name: '', description: '', volume: 2.5, isClosed: false });
const emptyPlantForm = () => ({
  name: '', nameLatin: '', description: '', category: 0, lightLevel: 0, waterNeed: 0,
  humidityLevel: 0, itemMaxSize: 0, soilFormulaId: '',
});
const emptyDecorationForm = () => ({ name: '', description: '', category: 0 });
const emptySoilTypeForm = () => ({ name: '' });
const emptySoilFormulaForm = () => ({ name: '', formulaItems: [{ soilTypeId: 0, percentage: 100, order: 0 }] });

function AdminDashboard() {
  const api = useApi();
  const [activeTab, setActiveTab] = useState('containers');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const [containers, setContainers] = useState([]);
  const [plants, setPlants] = useState([]);
  const [decorations, setDecorations] = useState([]);
  const [soilTypes, setSoilTypes] = useState([]);
  const [soilFormulas, setSoilFormulas] = useState([]);

  const [categories, setCategories] = useState([]);
  const [lightLevels, setLightLevels] = useState([]);
  const [waterNeeds, setWaterNeeds] = useState([]);
  const [humidityLevels, setHumidityLevels] = useState([]);
  const [itemSizes, setItemSizes] = useState([]);
  const [decorationCategories, setDecorationCategories] = useState([]);

  const [containerEditId, setContainerEditId] = useState(null);
  const [containerAddOpen, setContainerAddOpen] = useState(false);
  const [containerDeleteId, setContainerDeleteId] = useState(null);
  const [containerForm, setContainerForm] = useState(emptyContainerForm);

  const [plantEditId, setPlantEditId] = useState(null);
  const [plantAddOpen, setPlantAddOpen] = useState(false);
  const [plantDeleteId, setPlantDeleteId] = useState(null);
  const [plantForm, setPlantForm] = useState(emptyPlantForm);

  const [decorationEditId, setDecorationEditId] = useState(null);
  const [decorationAddOpen, setDecorationAddOpen] = useState(false);
  const [decorationDeleteId, setDecorationDeleteId] = useState(null);
  const [decorationForm, setDecorationForm] = useState(emptyDecorationForm);

  const [soilTypeEditId, setSoilTypeEditId] = useState(null);
  const [soilTypeAddOpen, setSoilTypeAddOpen] = useState(false);
  const [soilTypeDeleteId, setSoilTypeDeleteId] = useState(null);
  const [soilTypeForm, setSoilTypeForm] = useState(emptySoilTypeForm);

  const [soilFormulaEditId, setSoilFormulaEditId] = useState(null);
  const [soilFormulaAddOpen, setSoilFormulaAddOpen] = useState(false);
  const [soilFormulaDeleteId, setSoilFormulaDeleteId] = useState(null);
  const [soilFormulaForm, setSoilFormulaForm] = useState(emptySoilFormulaForm);

  const setApiError = useCallback((e) => {
    setError(e instanceof ApiError ? (e.detail || 'Помилка API') : 'Щось пішло не так.');
  }, []);

  const loadAll = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [
        containersRes, plantsRes, decorationsRes, soilTypesRes, soilFormulasRes,
        categoriesRes, lightLevelsRes, waterNeedsRes, humidityLevelsRes, itemSizesRes, decorationCategoriesRes,
      ] = await Promise.all([
        api.containers.getAll({ pageSize: 25 }),
        api.plants.getAll({ pageSize: 25 }),
        api.decorations.getAll({ pageSize: 25 }),
        api.soil.types.getAll({ pageSize: 25 }),
        api.soil.formulas.getAll({ pageSize: 25 }),
        api.plants.getCategories(),
        api.plants.getLightLevels(),
        api.plants.getWaterNeeds(),
        api.plants.getHumidityLevels(),
        api.plants.getItemSizes(),
        api.decorations.getCategories(),
      ]);
      setContainers(containersRes.items ?? []);
      setPlants(plantsRes.items ?? []);
      setDecorations(decorationsRes.items ?? []);
      setSoilTypes(soilTypesRes.items ?? []);
      setSoilFormulas(soilFormulasRes.items ?? []);
      setCategories(categoriesRes.categories ?? []);
      setLightLevels(lightLevelsRes.lightLevels ?? []);
      setWaterNeeds(waterNeedsRes.waterNeeds ?? []);
      setHumidityLevels(humidityLevelsRes.humidityLevels ?? []);
      setItemSizes(itemSizesRes.itemSizes ?? []);
      setDecorationCategories(decorationCategoriesRes.items ?? []);
    } catch (e) {
      setApiError(e);
    } finally {
      setLoading(false);
    }
  }, [api.containers, api.plants, api.decorations, api.soil.types, api.soil.formulas, setApiError]);

  useEffect(() => {
    loadAll();
  }, [loadAll]);

  const refreshContainers = useCallback(async () => {
    try {
      const res = await api.containers.getAll({ pageSize: 25 });
      setContainers(res.items ?? []);
    } catch (e) { setApiError(e); }
  }, [api.containers, setApiError]);
  const refreshPlants = useCallback(async () => {
    try {
      const res = await api.plants.getAll({ pageSize: 25 });
      setPlants(res.items ?? []);
    } catch (e) { setApiError(e); }
  }, [api.plants, setApiError]);
  const refreshDecorations = useCallback(async () => {
    try {
      const res = await api.decorations.getAll({ pageSize: 25 });
      setDecorations(res.items ?? []);
    } catch (e) { setApiError(e); }
  }, [api.decorations, setApiError]);
  const refreshSoilTypes = useCallback(async () => {
    try {
      const res = await api.soil.types.getAll({ pageSize: 25 });
      setSoilTypes(res.items ?? []);
    } catch (e) { setApiError(e); }
  }, [api.soil.types, setApiError]);
  const refreshSoilFormulas = useCallback(async () => {
    try {
      const res = await api.soil.formulas.getAll({ pageSize: 25 });
      setSoilFormulas(res.items ?? []);
    } catch (e) { setApiError(e); }
  }, [api.soil.formulas, setApiError]);

  const handleContainerAdd = async (e) => {
    e.preventDefault();
    setError(null);
    try {
      await api.containers.add({
        name: containerForm.name.trim(),
        description: containerForm.description?.trim() || null,
        volume: Number(containerForm.volume),
        isClosed: Boolean(containerForm.isClosed),
      });
      setContainerForm(emptyContainerForm());
      setContainerAddOpen(false);
      await refreshContainers();
    } catch (e) { setApiError(e); }
  };
  const handleContainerEdit = async (e) => {
    e.preventDefault();
    if (!containerEditId) return;
    setError(null);
    try {
      await api.containers.update(containerEditId, {
        name: containerForm.name.trim(),
        description: containerForm.description?.trim() || null,
        volume: Number(containerForm.volume),
        isClosed: Boolean(containerForm.isClosed),
      });
      setContainerEditId(null);
      setContainerForm(emptyContainerForm());
      await refreshContainers();
    } catch (e) { setApiError(e); }
  };
  const handleContainerDelete = async (id) => {
    setError(null);
    try {
      await api.containers.delete(id);
      setContainerDeleteId(null);
      await refreshContainers();
    } catch (e) { setApiError(e); }
  };

  const handlePlantAdd = async (e) => {
    e.preventDefault();
    setError(null);
    try {
      await api.plants.add({
        name: plantForm.name.trim(),
        nameLatin: plantForm.nameLatin.trim(),
        description: plantForm.description?.trim() || null,
        category: Number(plantForm.category),
        lightLevel: Number(plantForm.lightLevel),
        waterNeed: Number(plantForm.waterNeed),
        humidityLevel: Number(plantForm.humidityLevel),
        itemMaxSize: Number(plantForm.itemMaxSize),
        soilFormulaId: plantForm.soilFormulaId.trim(),
      });
      setPlantForm(emptyPlantForm());
      setPlantAddOpen(false);
      await refreshPlants();
    } catch (e) { setApiError(e); }
  };
  const handlePlantEdit = async (e) => {
    e.preventDefault();
    if (!plantEditId) return;
    setError(null);
    try {
      await api.plants.update(plantEditId, {
        name: plantForm.name.trim(),
        nameLatin: plantForm.nameLatin.trim(),
        description: plantForm.description?.trim() || null,
        category: Number(plantForm.category),
        lightLevel: Number(plantForm.lightLevel),
        waterNeed: Number(plantForm.waterNeed),
        humidityLevel: Number(plantForm.humidityLevel),
        itemMaxSize: Number(plantForm.itemMaxSize),
      });
      setPlantEditId(null);
      setPlantForm(emptyPlantForm());
      await refreshPlants();
    } catch (e) { setApiError(e); }
  };
  const handlePlantDelete = async (id) => {
    setError(null);
    try {
      await api.plants.delete(id);
      setPlantDeleteId(null);
      await refreshPlants();
    } catch (e) { setApiError(e); }
  };

  const handleDecorationAdd = async (e) => {
    e.preventDefault();
    setError(null);
    try {
      await api.decorations.add({
        name: decorationForm.name.trim(),
        description: decorationForm.description?.trim() || null,
        category: Number(decorationForm.category),
      });
      setDecorationForm(emptyDecorationForm());
      setDecorationAddOpen(false);
      await refreshDecorations();
    } catch (e) { setApiError(e); }
  };
  const handleDecorationEdit = async (e) => {
    e.preventDefault();
    if (!decorationEditId) return;
    setError(null);
    try {
      await api.decorations.update(decorationEditId, {
        name: decorationForm.name.trim(),
        description: decorationForm.description?.trim() || null,
        category: Number(decorationForm.category),
      });
      setDecorationEditId(null);
      setDecorationForm(emptyDecorationForm());
      await refreshDecorations();
    } catch (e) { setApiError(e); }
  };
  const handleDecorationDelete = async (id) => {
    setError(null);
    try {
      await api.decorations.delete(id);
      setDecorationDeleteId(null);
      await refreshDecorations();
    } catch (e) { setApiError(e); }
  };

  const handleSoilTypeAdd = async (e) => {
    e.preventDefault();
    setError(null);
    try {
      await api.soil.types.add({ name: soilTypeForm.name.trim() });
      setSoilTypeForm(emptySoilTypeForm());
      setSoilTypeAddOpen(false);
      await refreshSoilTypes();
    } catch (e) { setApiError(e); }
  };
  const handleSoilTypeEdit = async (e) => {
    e.preventDefault();
    if (soilTypeEditId == null) return;
    setError(null);
    try {
      await api.soil.types.update(soilTypeEditId, { newName: soilTypeForm.name.trim() });
      setSoilTypeEditId(null);
      setSoilTypeForm(emptySoilTypeForm());
      await refreshSoilTypes();
    } catch (e) { setApiError(e); }
  };
  const handleSoilTypeDelete = async (id) => {
    setError(null);
    try {
      await api.soil.types.delete(id);
      setSoilTypeDeleteId(null);
      await refreshSoilTypes();
    } catch (e) { setApiError(e); }
  };

  const handleSoilFormulaAdd = async (e) => {
    e.preventDefault();
    setError(null);
    const formulaItems = soilFormulaForm.formulaItems
      .filter((i) => i.soilTypeId && i.percentage > 0)
      .map((i, idx) => ({ soilTypeId: Number(i.soilTypeId), percentage: Number(i.percentage), order: idx }));
    if (formulaItems.length === 0) {
      setError('Додайте хоча б один компонент.');
      return;
    }
    try {
      await api.soil.formulas.add({ name: soilFormulaForm.name.trim(), formulaItems });
      setSoilFormulaForm(emptySoilFormulaForm());
      setSoilFormulaAddOpen(false);
      await refreshSoilFormulas();
    } catch (e) { setApiError(e); }
  };
  const handleSoilFormulaEdit = async (e) => {
    e.preventDefault();
    if (!soilFormulaEditId) return;
    setError(null);
    const formulaItems = soilFormulaForm.formulaItems
      .filter((i) => i.soilTypeId && i.percentage > 0)
      .map((i, idx) => ({ soilTypeId: Number(i.soilTypeId), percentage: Number(i.percentage), order: idx }));
    try {
      await api.soil.formulas.update(soilFormulaEditId, { newName: soilFormulaForm.name.trim(), formulaItems });
      setSoilFormulaEditId(null);
      setSoilFormulaForm(emptySoilFormulaForm());
      await refreshSoilFormulas();
    } catch (e) { setApiError(e); }
  };
  const handleSoilFormulaDelete = async (id) => {
    setError(null);
    try {
      await api.soil.formulas.delete(id);
      setSoilFormulaDeleteId(null);
      await refreshSoilFormulas();
    } catch (e) { setApiError(e); }
  };

  const openContainerEdit = async (id) => {
    try {
      const c = await api.containers.get(id);
      setContainerForm({
        name: c.name ?? '',
        description: c.description ?? '',
        volume: c.volume ?? 2.5,
        isClosed: c.isClosed ?? false,
      });
      setContainerEditId(id);
      setContainerAddOpen(false);
    } catch (e) { setApiError(e); }
  };
  const openPlantEdit = async (id) => {
    try {
      const p = await api.plants.get(id);
      setPlantForm({
        name: p.name ?? '',
        nameLatin: p.nameLatin ?? '',
        description: p.description ?? '',
        category: p.category ?? 0,
        lightLevel: p.lightLevel ?? 0,
        waterNeed: p.waterNeed ?? 0,
        humidityLevel: p.humidityLevel ?? 0,
        itemMaxSize: p.itemMaxSize ?? 0,
        soilFormulaId: p.soilFormulaId ?? '',
      });
      setPlantEditId(id);
      setPlantAddOpen(false);
    } catch (e) { setApiError(e); }
  };
  const openDecorationEdit = async (id) => {
    try {
      const d = await api.decorations.get(id);
      setDecorationForm({
        name: d.name ?? '',
        description: d.description ?? '',
        category: d.category ?? 0,
      });
      setDecorationEditId(id);
      setDecorationAddOpen(false);
    } catch (e) { setApiError(e); }
  };
  const openSoilTypeEdit = (item) => {
    setSoilTypeForm({ name: item.name ?? '' });
    setSoilTypeEditId(item.id);
    setSoilTypeAddOpen(false);
  };
  const openSoilFormulaEdit = (item) => {
    setSoilFormulaForm({
      name: item.name ?? '',
      formulaItems: (item.items ?? []).length
        ? item.items.map((i) => ({
          soilTypeId: i.soilType?.id ?? 0,
          percentage: i.percentage ?? 0,
          order: i.order ?? 0,
        }))
        : [{ soilTypeId: 0, percentage: 100, order: 0 }],
    });
    setSoilFormulaEditId(item.id);
    setSoilFormulaAddOpen(false);
  };

  const findName = (list, id) => list.find((x) => x.id === id)?.name ?? id;

  if (loading) {
    return (
      <div className="admin-dashboard">
        <h1>Панель адміністратора</h1>
        <p className="admin-subtitle">Управління каталогом</p>
        <div className="admin-loading">Завантаження...</div>
      </div>
    );
  }

  return (
    <div className="admin-dashboard">
      <h1>Панель адміністратора</h1>
      <p className="admin-subtitle">Управління каталогом</p>

      {error && (
        <div className="admin-error" role="alert">
          {error}
        </div>
      )}

      <div className="admin-tabs">
        {TABS.map((tab) => (
          <button
            key={tab.id}
            type="button"
            className={activeTab === tab.id ? 'active' : ''}
            onClick={() => setActiveTab(tab.id)}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {activeTab === 'containers' && (
        <div className="admin-section">
          <div className="admin-section-header">
            <h2>Контейнери</h2>
            <button
              type="button"
              className="admin-btn admin-btn-primary"
              onClick={() => {
                setContainerAddOpen(true);
                setContainerEditId(null);
                setContainerForm(emptyContainerForm());
              }}
            >
              Додати
            </button>
          </div>
          <div className="admin-table-wrap">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Назва</th>
                  <th>Обʼєм (л)</th>
                  <th>Закритий</th>
                  <th>Дії</th>
                </tr>
              </thead>
              <tbody>
                {containers.map((c) => (
                  <tr key={c.id}>
                    <td>{c.name}</td>
                    <td>{c.volume}</td>
                    <td>{c.isClosed ? 'Так' : 'Ні'}</td>
                    <td>
                      <div className="admin-actions">
                        <button type="button" className="admin-btn admin-btn-ghost" onClick={() => openContainerEdit(c.id)}>Редагувати</button>
                        {containerDeleteId === c.id ? (
                          <div className="admin-confirm">
                            <span>Видалити?</span>
                            <div>
                              <button type="button" className="admin-btn admin-btn-danger" onClick={() => handleContainerDelete(c.id)}>Так</button>
                              <button type="button" className="admin-btn admin-btn-ghost" onClick={() => setContainerDeleteId(null)}>Ні</button>
                            </div>
                          </div>
                        ) : (
                          <button type="button" className="admin-btn admin-btn-danger" onClick={() => setContainerDeleteId(c.id)}>Видалити</button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          {(containerAddOpen || containerEditId) && (
            <form className="admin-form" onSubmit={containerEditId ? handleContainerEdit : handleContainerAdd}>
              <div className="admin-form-row">
                <div>
                  <label>Назва</label>
                  <input value={containerForm.name} onChange={(e) => setContainerForm((f) => ({ ...f, name: e.target.value }))} required />
                </div>
                <div>
                  <label>Опис</label>
                  <input value={containerForm.description} onChange={(e) => setContainerForm((f) => ({ ...f, description: e.target.value }))} />
                </div>
                <div>
                  <label>Обʼєм (л)</label>
                  <input type="number" step="0.1" min="0" value={containerForm.volume} onChange={(e) => setContainerForm((f) => ({ ...f, volume: e.target.value }))} required />
                </div>
                <div>
                  <label>Закритий</label>
                  <select value={containerForm.isClosed ? '1' : '0'} onChange={(e) => setContainerForm((f) => ({ ...f, isClosed: e.target.value === '1' }))}>
                    <option value="0">Ні</option>
                    <option value="1">Так</option>
                  </select>
                </div>
              </div>
              <div className="admin-form-actions">
                <button type="submit" className="admin-btn admin-btn-primary">{containerEditId ? 'Зберегти' : 'Додати'}</button>
                <button type="button" className="admin-btn admin-btn-ghost" onClick={() => { setContainerAddOpen(false); setContainerEditId(null); setContainerForm(emptyContainerForm()); }}>Скасувати</button>
              </div>
            </form>
          )}
        </div>
      )}

      {activeTab === 'plants' && (
        <div className="admin-section">
          <div className="admin-section-header">
            <h2>Рослини</h2>
            <button
              type="button"
              className="admin-btn admin-btn-primary"
              onClick={() => {
                setPlantAddOpen(true);
                setPlantEditId(null);
                setPlantForm({
                  ...emptyPlantForm(),
                  category: categories[0]?.id ?? 0,
                  lightLevel: lightLevels[0]?.id ?? 0,
                  waterNeed: waterNeeds[0]?.id ?? 0,
                  humidityLevel: humidityLevels[0]?.id ?? 0,
                  itemMaxSize: itemSizes[0]?.id ?? 0,
                  soilFormulaId: soilFormulas[0]?.id ?? '',
                });
              }}
            >
              Додати
            </button>
          </div>
          <div className="admin-table-wrap">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Назва</th>
                  <th>Латинська</th>
                  <th>Категорія</th>
                  <th>Дії</th>
                </tr>
              </thead>
              <tbody>
                {plants.map((p) => (
                  <tr key={p.id}>
                    <td>{p.name}</td>
                    <td>{p.nameLatin}</td>
                    <td>{findName(categories, p.category)}</td>
                    <td>
                      <div className="admin-actions">
                        <button type="button" className="admin-btn admin-btn-ghost" onClick={() => openPlantEdit(p.id)}>Редагувати</button>
                        {plantDeleteId === p.id ? (
                          <div className="admin-confirm">
                            <span>Видалити?</span>
                            <div>
                              <button type="button" className="admin-btn admin-btn-danger" onClick={() => handlePlantDelete(p.id)}>Так</button>
                              <button type="button" className="admin-btn admin-btn-ghost" onClick={() => setPlantDeleteId(null)}>Ні</button>
                            </div>
                          </div>
                        ) : (
                          <button type="button" className="admin-btn admin-btn-danger" onClick={() => setPlantDeleteId(p.id)}>Видалити</button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          {(plantAddOpen || plantEditId) && (
            <form className="admin-form" onSubmit={plantEditId ? handlePlantEdit : handlePlantAdd}>
              <div className="admin-form-row">
                <div>
                  <label>Назва</label>
                  <input value={plantForm.name} onChange={(e) => setPlantForm((f) => ({ ...f, name: e.target.value }))} required />
                </div>
                <div>
                  <label>Латинська назва</label>
                  <input value={plantForm.nameLatin} onChange={(e) => setPlantForm((f) => ({ ...f, nameLatin: e.target.value }))} required />
                </div>
                <div>
                  <label>Опис</label>
                  <input value={plantForm.description} onChange={(e) => setPlantForm((f) => ({ ...f, description: e.target.value }))} />
                </div>
                <div>
                  <label>Категорія</label>
                  <select value={plantForm.category} onChange={(e) => setPlantForm((f) => ({ ...f, category: e.target.value }))} required>
                    {categories.map((cat) => (
                      <option key={cat.id} value={cat.id}>{cat.name}</option>
                    ))}
                  </select>
                </div>
                <div>
                  <label>Світло</label>
                  <select value={plantForm.lightLevel} onChange={(e) => setPlantForm((f) => ({ ...f, lightLevel: e.target.value }))}>
                    {lightLevels.map((l) => (
                      <option key={l.id} value={l.id}>{l.name}</option>
                    ))}
                  </select>
                </div>
                <div>
                  <label>Вода</label>
                  <select value={plantForm.waterNeed} onChange={(e) => setPlantForm((f) => ({ ...f, waterNeed: e.target.value }))}>
                    {waterNeeds.map((w) => (
                      <option key={w.id} value={w.id}>{w.name}</option>
                    ))}
                  </select>
                </div>
                <div>
                  <label>Вологість</label>
                  <select value={plantForm.humidityLevel} onChange={(e) => setPlantForm((f) => ({ ...f, humidityLevel: e.target.value }))}>
                    {humidityLevels.map((h) => (
                      <option key={h.id} value={h.id}>{h.name}</option>
                    ))}
                  </select>
                </div>
                <div>
                  <label>Розмір</label>
                  <select value={plantForm.itemMaxSize} onChange={(e) => setPlantForm((f) => ({ ...f, itemMaxSize: e.target.value }))}>
                    {itemSizes.map((s) => (
                      <option key={s.id} value={s.id}>{s.name}</option>
                    ))}
                  </select>
                </div>
                {plantAddOpen && (
                  <div>
                    <label>Формула ґрунту</label>
                    <select value={plantForm.soilFormulaId} onChange={(e) => setPlantForm((f) => ({ ...f, soilFormulaId: e.target.value }))} required>
                      <option value="">Оберіть</option>
                      {soilFormulas.map((f) => (
                        <option key={f.id} value={f.id}>{f.name}</option>
                      ))}
                    </select>
                  </div>
                )}
              </div>
              <div className="admin-form-actions">
                <button type="submit" className="admin-btn admin-btn-primary">{plantEditId ? 'Зберегти' : 'Додати'}</button>
                <button type="button" className="admin-btn admin-btn-ghost" onClick={() => { setPlantAddOpen(false); setPlantEditId(null); setPlantForm(emptyPlantForm()); }}>Скасувати</button>
              </div>
            </form>
          )}
        </div>
      )}

      {activeTab === 'decorations' && (
        <div className="admin-section">
          <div className="admin-section-header">
            <h2>Декор</h2>
            <button
              type="button"
              className="admin-btn admin-btn-primary"
              onClick={() => {
                setDecorationAddOpen(true);
                setDecorationEditId(null);
                setDecorationForm({ ...emptyDecorationForm(), category: decorationCategories[0]?.id ?? 0 });
              }}
            >
              Додати
            </button>
          </div>
          <div className="admin-table-wrap">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Назва</th>
                  <th>Категорія</th>
                  <th>Дії</th>
                </tr>
              </thead>
              <tbody>
                {decorations.map((d) => (
                  <tr key={d.id}>
                    <td>{d.name}</td>
                    <td>{findName(decorationCategories, d.category)}</td>
                    <td>
                      <div className="admin-actions">
                        <button type="button" className="admin-btn admin-btn-ghost" onClick={() => openDecorationEdit(d.id)}>Редагувати</button>
                        {decorationDeleteId === d.id ? (
                          <div className="admin-confirm">
                            <span>Видалити?</span>
                            <div>
                              <button type="button" className="admin-btn admin-btn-danger" onClick={() => handleDecorationDelete(d.id)}>Так</button>
                              <button type="button" className="admin-btn admin-btn-ghost" onClick={() => setDecorationDeleteId(null)}>Ні</button>
                            </div>
                          </div>
                        ) : (
                          <button type="button" className="admin-btn admin-btn-danger" onClick={() => setDecorationDeleteId(d.id)}>Видалити</button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          {(decorationAddOpen || decorationEditId) && (
            <form className="admin-form" onSubmit={decorationEditId ? handleDecorationEdit : handleDecorationAdd}>
              <div className="admin-form-row">
                <div>
                  <label>Назва</label>
                  <input value={decorationForm.name} onChange={(e) => setDecorationForm((f) => ({ ...f, name: e.target.value }))} required />
                </div>
                <div>
                  <label>Опис</label>
                  <input value={decorationForm.description} onChange={(e) => setDecorationForm((f) => ({ ...f, description: e.target.value }))} />
                </div>
                <div>
                  <label>Категорія</label>
                  <select value={decorationForm.category} onChange={(e) => setDecorationForm((f) => ({ ...f, category: e.target.value }))} required>
                    {decorationCategories.map((cat) => (
                      <option key={cat.id} value={cat.id}>{cat.name}</option>
                    ))}
                  </select>
                </div>
              </div>
              <div className="admin-form-actions">
                <button type="submit" className="admin-btn admin-btn-primary">{decorationEditId ? 'Зберегти' : 'Додати'}</button>
                <button type="button" className="admin-btn admin-btn-ghost" onClick={() => { setDecorationAddOpen(false); setDecorationEditId(null); setDecorationForm(emptyDecorationForm()); }}>Скасувати</button>
              </div>
            </form>
          )}
        </div>
      )}

      {activeTab === 'soilTypes' && (
        <div className="admin-section">
          <div className="admin-section-header">
            <h2>Типи ґрунту</h2>
            <button
              type="button"
              className="admin-btn admin-btn-primary"
              onClick={() => {
                setSoilTypeAddOpen(true);
                setSoilTypeEditId(null);
                setSoilTypeForm(emptySoilTypeForm());
              }}
            >
              Додати
            </button>
          </div>
          <div className="admin-table-wrap">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Назва</th>
                  <th>Дії</th>
                </tr>
              </thead>
              <tbody>
                {soilTypes.map((st) => (
                  <tr key={st.id}>
                    <td>{st.name}</td>
                    <td>
                      <div className="admin-actions">
                        <button type="button" className="admin-btn admin-btn-ghost" onClick={() => openSoilTypeEdit(st)}>Редагувати</button>
                        {soilTypeDeleteId === st.id ? (
                          <div className="admin-confirm">
                            <span>Видалити?</span>
                            <div>
                              <button type="button" className="admin-btn admin-btn-danger" onClick={() => handleSoilTypeDelete(st.id)}>Так</button>
                              <button type="button" className="admin-btn admin-btn-ghost" onClick={() => setSoilTypeDeleteId(null)}>Ні</button>
                            </div>
                          </div>
                        ) : (
                          <button type="button" className="admin-btn admin-btn-danger" onClick={() => setSoilTypeDeleteId(st.id)}>Видалити</button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          {(soilTypeAddOpen || soilTypeEditId != null) && (
            <form className="admin-form" onSubmit={soilTypeEditId != null ? handleSoilTypeEdit : handleSoilTypeAdd}>
              <div className="admin-form-row">
                <div>
                  <label>Назва</label>
                  <input value={soilTypeForm.name} onChange={(e) => setSoilTypeForm((f) => ({ ...f, name: e.target.value }))} required />
                </div>
              </div>
              <div className="admin-form-actions">
                <button type="submit" className="admin-btn admin-btn-primary">{soilTypeEditId != null ? 'Зберегти' : 'Додати'}</button>
                <button type="button" className="admin-btn admin-btn-ghost" onClick={() => { setSoilTypeAddOpen(false); setSoilTypeEditId(null); setSoilTypeForm(emptySoilTypeForm()); }}>Скасувати</button>
              </div>
            </form>
          )}
        </div>
      )}

      {activeTab === 'soilFormulas' && (
        <div className="admin-section">
          <div className="admin-section-header">
            <h2>Формули ґрунту</h2>
            <button
              type="button"
              className="admin-btn admin-btn-primary"
              onClick={() => {
                setSoilFormulaAddOpen(true);
                setSoilFormulaEditId(null);
                setSoilFormulaForm(emptySoilFormulaForm());
              }}
            >
              Додати
            </button>
          </div>
          <div className="admin-table-wrap">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Назва</th>
                  <th>Складові</th>
                  <th>Дії</th>
                </tr>
              </thead>
              <tbody>
                {soilFormulas.map((sf) => (
                  <tr key={sf.id}>
                    <td>{sf.name}</td>
                    <td>{(sf.items ?? []).map((i) => `${i.soilType?.name ?? '?'} ${i.percentage}%`).join(', ') || '—'}</td>
                    <td>
                      <div className="admin-actions">
                        <button type="button" className="admin-btn admin-btn-ghost" onClick={() => openSoilFormulaEdit(sf)}>Редагувати</button>
                        {soilFormulaDeleteId === sf.id ? (
                          <div className="admin-confirm">
                            <span>Видалити?</span>
                            <div>
                              <button type="button" className="admin-btn admin-btn-danger" onClick={() => handleSoilFormulaDelete(sf.id)}>Так</button>
                              <button type="button" className="admin-btn admin-btn-ghost" onClick={() => setSoilFormulaDeleteId(null)}>Ні</button>
                            </div>
                          </div>
                        ) : (
                          <button type="button" className="admin-btn admin-btn-danger" onClick={() => setSoilFormulaDeleteId(sf.id)}>Видалити</button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          {(soilFormulaAddOpen || soilFormulaEditId) && (
            <form className="admin-form" onSubmit={soilFormulaEditId ? handleSoilFormulaEdit : handleSoilFormulaAdd}>
              <div className="admin-form-row">
                <div>
                  <label>Назва</label>
                  <input value={soilFormulaForm.name} onChange={(e) => setSoilFormulaForm((f) => ({ ...f, name: e.target.value }))} required />
                </div>
              </div>
              <div>
                <label>Складові (тип ґрунту, %)</label>
                {soilFormulaForm.formulaItems.map((item, idx) => (
                  <div key={idx} className="admin-form-row" style={{ marginTop: '0.5rem' }}>
                    <select
                      value={item.soilTypeId}
                      onChange={(e) => setSoilFormulaForm((f) => ({
                        ...f,
                        formulaItems: f.formulaItems.map((it, i) => i === idx ? { ...it, soilTypeId: Number(e.target.value) } : it),
                      }))}
                    >
                      <option value="0">Оберіть тип</option>
                      {soilTypes.map((st) => (
                        <option key={st.id} value={st.id}>{st.name}</option>
                      ))}
                    </select>
                    <input
                      type="number"
                      min="0"
                      max="100"
                      placeholder="%"
                      value={item.percentage}
                      onChange={(e) => setSoilFormulaForm((f) => ({
                        ...f,
                        formulaItems: f.formulaItems.map((it, i) => i === idx ? { ...it, percentage: Number(e.target.value) } : it),
                      }))}
                    />
                    {soilFormulaForm.formulaItems.length > 1 && (
                      <button type="button" className="admin-btn admin-btn-ghost" onClick={() => setSoilFormulaForm((f) => ({ ...f, formulaItems: f.formulaItems.filter((_, i) => i !== idx) }))}>Видалити</button>
                    )}
                  </div>
                ))}
                <button type="button" className="admin-btn admin-btn-ghost" style={{ marginTop: '0.5rem' }} onClick={() => setSoilFormulaForm((f) => ({ ...f, formulaItems: [...f.formulaItems, { soilTypeId: 0, percentage: 0, order: f.formulaItems.length }] }))}>+ Додати компонент</button>
              </div>
              <div className="admin-form-actions">
                <button type="submit" className="admin-btn admin-btn-primary">{soilFormulaEditId ? 'Зберегти' : 'Додати'}</button>
                <button type="button" className="admin-btn admin-btn-ghost" onClick={() => { setSoilFormulaAddOpen(false); setSoilFormulaEditId(null); setSoilFormulaForm(emptySoilFormulaForm()); }}>Скасувати</button>
              </div>
            </form>
          )}
        </div>
      )}
    </div>
  );
}

export default AdminDashboard;
