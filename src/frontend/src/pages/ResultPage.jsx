import { useEffect, useRef, useState } from 'react';
import { useLocation, useNavigate, useSearchParams } from 'react-router-dom';
import { useApi } from '../services/useApi';
import { useAuth } from '../services/useAuth';
import { ExportPdfButton } from '../components/ExportPdfButton';
import './ResultPage.css';

function ResultPage() {
  const api = useApi();
  const { isAuthenticated } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  const projectId =
    location.state?.projectId ?? searchParams.get('projectId') ?? null;

  const canvasSnapshot = location.state?.canvasSnapshot ?? null;
  const generatedUrlRef = useRef(null);

  const [snapshotPreviewUrl, setSnapshotPreviewUrl] = useState(null);
  const [generating, setGenerating] = useState(false);
  const [generatedImageUrl, setGeneratedImageUrl] = useState(null);
  const [error, setError] = useState('');

  useEffect(() => {
    return () => {
      if (generatedUrlRef.current) {
        URL.revokeObjectURL(generatedUrlRef.current);
      }
    };
  }, []);

  useEffect(() => {
    if (!canvasSnapshot) {
      setSnapshotPreviewUrl(null);
      return undefined;
    }

    const previewUrl = URL.createObjectURL(canvasSnapshot);
    setSnapshotPreviewUrl(previewUrl);

    return () => {
      URL.revokeObjectURL(previewUrl);
    };
  }, [canvasSnapshot]);

  async function handleGenerate() {
    if (!canvasSnapshot) {
      setError('Відсутній знімок полотна з конструктора.');
      return;
    }

    setGenerating(true);
    setError('');
    setGeneratedImageUrl(null);

    try {
      const { blob } = await api.projects.generateFlorariumImage(projectId, canvasSnapshot);
      if (generatedUrlRef.current) {
        URL.revokeObjectURL(generatedUrlRef.current);
      }
      const url = URL.createObjectURL(blob);
      generatedUrlRef.current = url;
      setGeneratedImageUrl(url);
    } catch (err) {
      setError(`Помилка генерації: ${err?.detail || err?.message || 'Невідома помилка'}`);
    } finally {
      setGenerating(false);
    }
  }

  function handleDownload() {
    if (!generatedImageUrl) return;
    const a = document.createElement('a');
    a.href = generatedImageUrl;
    a.download = 'florarium.png';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
  }

  if (!isAuthenticated) {
    return (
      <section className="result-page">
        <div className="result-auth-guard">
          <h1>Результат</h1>
          <p>Для перегляду результатів необхідно авторизуватися.</p>
        </div>
      </section>
    );
  }

  if (!projectId) {
    return (
      <section className="result-page">
        <div className="result-empty">
          <h1>Результат</h1>
          <p>Спочатку збережіть проєкт у конструкторі.</p>
          <button type="button" className="result-btn" onClick={() => navigate('/constructor')}>
            Перейти до конструктора
          </button>
        </div>
      </section>
    );
  }

  const canGenerateAi = Boolean(canvasSnapshot);

  if (!canGenerateAi) {
    return (
      <section className="result-page">
        <header className="result-header">
          <h1>Експорт проєкту</h1>
          <p className="result-subtitle">
            Знімок полотна для ШІ недоступний (наприклад, після оновлення сторінки). Ви все одно можете завантажити PDF зі
            списком покупок і інструкцією за збереженим проєктом.
          </p>
        </header>
        <div className="result-actions">
          <ExportPdfButton projectId={projectId} variant="primary" />
          <button type="button" className="result-btn" onClick={() => navigate('/constructor')}>
            Повернутися до конструктора
          </button>
        </div>
      </section>
    );
  }

  return (
    <section className="result-page">
      <header className="result-header">
        <h1>Генерація зображення флораріуму</h1>
        <p className="result-subtitle">
          Контейнер буде взято з серверних даних про проєкт, а полотно конструктора вже передано як еталон композиції.
        </p>
      </header>

      {snapshotPreviewUrl && (
        <div className="result-preview">
          <h2>Знімок полотна конструктора</h2>
          <div className="result-preview-image-wrap">
            <img src={snapshotPreviewUrl} alt="Знімок конструктора" />
          </div>
        </div>
      )}

      <div className="result-export-pdf-block">
        <h2 className="result-section-title">Список покупок і інструкція</h2>
        <p className="result-subtitle result-export-hint">
          Завантажте PDF з розрахунком компонентів субстрату (за об&apos;ємом контейнера), переліком рослин і декору та
          короткою покроковою інструкцією.
        </p>
        <ExportPdfButton projectId={projectId} variant="primary" />
      </div>

      {error && <p className="result-error">{error}</p>}

      <div className="result-actions">
        <button
          type="button"
          className="result-btn result-btn-primary"
          onClick={handleGenerate}
          disabled={generating}
        >
          {generating ? 'Генерація зображення…' : 'Згенерувати зображення'}
        </button>
        <button
          type="button"
          className="result-btn"
          onClick={() => navigate('/constructor')}
        >
          Повернутися до конструктора
        </button>
      </div>

      {generating && (
        <div className="result-loading">
          <div className="result-spinner" />
          <p>ШІ генерує фотореалістичне зображення вашого флораріуму. Це може зайняти до хвилини…</p>
        </div>
      )}

      {generatedImageUrl && (
        <div className="result-preview">
          <h2>Результат</h2>
          <div className="result-preview-image-wrap">
            <img src={generatedImageUrl} alt="Згенерований флораріум" />
          </div>
          <div className="result-preview-actions">
            <button type="button" className="result-btn result-btn-primary" onClick={handleDownload}>
              Завантажити зображення
            </button>
          </div>
        </div>
      )}
    </section>
  );
}

export default ResultPage;
