import { useState } from 'react';
import { useApi } from '../services/useApi';
import './ExportPdfButton.css';
import { fetchFlorariumExportPayload } from '../utils/florariumPdf/fetchFlorariumExportPayload';
import { generateFlorariumPdf } from '../utils/florariumPdf/generateFlorariumPdf';

/** @param {unknown} e */
function formatPdfExportError(e) {
  if (e && typeof e === 'object' && 'detail' in e && typeof e.detail === 'string' && e.detail.trim()) {
    return e.detail;
  }
  if (e && typeof e === 'object' && 'errors' in e && e.errors && typeof e.errors === 'object') {
    const first = Object.values(/** @type {Record<string, string[]>} */ (e.errors))[0];
    if (Array.isArray(first) && first[0]) return first[0];
  }
  if (e instanceof Error && e.message) return e.message;
  return 'Не вдалося сформувати PDF.';
}

/**
 * @param {{ projectId: string; className?: string; label?: string; previewLabel?: string; variant?: 'primary' | 'secondary' }} props
 */
export function ExportPdfButton({
  projectId,
  className = '',
  label = 'Завантажити PDF',
  previewLabel = 'Переглянути PDF',
  variant = 'secondary',
}) {
  const api = useApi();
  const [loading, setLoading] = useState(/** @type {'download' | 'preview' | null} */ (null));
  const [error, setError] = useState('');

  async function handleDownload() {
    if (!projectId || loading) return;
    setLoading('download');
    setError('');
    try {
      const payload = await fetchFlorariumExportPayload(api, projectId);
      await generateFlorariumPdf(payload);
    } catch (e) {
      const msg = formatPdfExportError(e);
      setError(msg);
    } finally {
      setLoading(null);
    }
  }

  function handlePreview() {
    if (!projectId || loading) return;
    const previewWindow = window.open('', '_blank');
    if (!previewWindow) {
      setError('Браузер заблокував нове вікно. Дозвольте спливаючі вікна для цього сайту.');
      return;
    }
    previewWindow.document.title = 'Prickle — PDF';
    const doc = previewWindow.document;
    doc.body.style.margin = '0';
    doc.body.style.fontFamily = 'system-ui, sans-serif';
    doc.body.style.background = '#f0f0f0';
    doc.body.innerHTML =
      '<p style="margin:2rem;text-align:center;color:#555;">Формування PDF…</p>';

    setLoading('preview');
    setError('');

    (async () => {
      try {
        const payload = await fetchFlorariumExportPayload(api, projectId);
        await generateFlorariumPdf(payload, { previewWindow });
      } catch (e) {
        try {
          previewWindow.close();
        } catch {
          // ignore
        }
        const msg = formatPdfExportError(e);
        setError(msg);
      } finally {
        setLoading(null);
      }
    })();
  }

  const btnClass =
    variant === 'primary'
      ? 'export-pdf-btn export-pdf-btn--primary'
      : 'export-pdf-btn';

  const busy = Boolean(loading);

  return (
    <div className="export-pdf-wrap">
      <div className="export-pdf-actions">
        <button
          type="button"
          className={`${btnClass} export-pdf-btn--ghost ${className}`.trim()}
          onClick={handlePreview}
          disabled={busy || !projectId}
        >
          {loading === 'preview' ? 'Формування PDF…' : previewLabel}
        </button>
        <button
          type="button"
          className={`${btnClass} ${className}`.trim()}
          onClick={handleDownload}
          disabled={busy || !projectId}
        >
          {loading === 'download' ? 'Формування PDF…' : label}
        </button>
      </div>
      {error && <p className="export-pdf-error">{error}</p>}
    </div>
  );
}

export default ExportPdfButton;
