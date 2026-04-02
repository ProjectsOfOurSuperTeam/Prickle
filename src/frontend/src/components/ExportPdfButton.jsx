import { useState } from 'react';
import { useApi } from '../services/useApi';
import './ExportPdfButton.css';
import { FloraCompatibilityModal } from './FloraCompatibilityModal';
import { analyzeFloraCompatibility } from '../utils/florariumCompatibility/analyzeFloraCompatibility';
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
 * @param {*} payload
 * @returns {ReturnType<typeof analyzeFloraCompatibility>}
 */
function compatibilityFromPayload(payload) {
  const plants = (payload.plants ?? []).map((p) => ({
    name: p.name,
    lightLevel: Number(p.lightLevel),
    waterNeed: Number(p.waterNeed),
    humidityLevel: Number(p.humidityLevel),
    soilFormulaId: p.soilFormulaId ?? null,
    category: p.category ?? null,
  }));
  return analyzeFloraCompatibility({
    plants,
    selectedSoilFormulaId: payload.selectedSoilFormulaId ?? null,
    resolveSoilFormulaName: (id) => {
      const name = payload.soilFormulaNames?.[String(id)];
      return name != null && name !== '' ? name : null;
    },
  });
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
  const [modal, setModal] = useState(
    /** @type {null | { action: 'download' | 'preview'; payload: object; analysis: ReturnType<typeof analyzeFloraCompatibility> }} */
    (null),
  );

  async function runDownloadPdf(payload) {
    setLoading('download');
    setError('');
    try {
      await generateFlorariumPdf(payload);
    } catch (e) {
      setError(formatPdfExportError(e));
    } finally {
      setLoading(null);
    }
  }

  async function runPreviewPdf(payload) {
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
    try {
      await generateFlorariumPdf(payload, { previewWindow });
    } catch (e) {
      try {
        previewWindow.close();
      } catch {
        // ignore
      }
      setError(formatPdfExportError(e));
    } finally {
      setLoading(null);
    }
  }

  async function handleDownload() {
    if (!projectId || loading || modal) return;
    setLoading('download');
    setError('');
    try {
      const payload = await fetchFlorariumExportPayload(api, projectId);
      const analysis = compatibilityFromPayload(payload);
      if (analysis.shouldPrompt) {
        setModal({ action: 'download', payload, analysis });
        setLoading(null);
        return;
      }
      await generateFlorariumPdf(payload);
    } catch (e) {
      setError(formatPdfExportError(e));
    } finally {
      setLoading(null);
    }
  }

  async function handlePreview() {
    if (!projectId || loading || modal) return;
    setLoading('preview');
    setError('');
    try {
      const payload = await fetchFlorariumExportPayload(api, projectId);
      const analysis = compatibilityFromPayload(payload);
      if (analysis.shouldPrompt) {
        setModal({ action: 'preview', payload, analysis });
        setLoading(null);
        return;
      }
      await runPreviewPdf(payload);
    } catch (e) {
      setError(formatPdfExportError(e));
    } finally {
      setLoading(null);
    }
  }

  function handleModalClose() {
    setModal(null);
  }

  function handleModalContinue() {
    if (!modal) return;
    const { action, payload } = modal;
    setModal(null);
    if (action === 'download') {
      void runDownloadPdf(payload);
    } else {
      void runPreviewPdf(payload);
    }
  }

  const btnClass =
    variant === 'primary'
      ? 'export-pdf-btn export-pdf-btn--primary'
      : 'export-pdf-btn';

  const busy = Boolean(loading);

  const modalActionLabel = modal?.action === 'preview' ? previewLabel : label;

  return (
    <div className="export-pdf-wrap">
      <div className="export-pdf-actions">
        <button
          type="button"
          className={`${btnClass} export-pdf-btn--ghost ${className}`.trim()}
          onClick={handlePreview}
          disabled={busy || !projectId || Boolean(modal)}
        >
          {loading === 'preview' ? 'Формування PDF…' : previewLabel}
        </button>
        <button
          type="button"
          className={`${btnClass} ${className}`.trim()}
          onClick={handleDownload}
          disabled={busy || !projectId || Boolean(modal)}
        >
          {loading === 'download' ? 'Формування PDF…' : label}
        </button>
      </div>
      {error && <p className="export-pdf-error">{error}</p>}

      <FloraCompatibilityModal
        open={Boolean(modal)}
        analysis={modal?.analysis ?? null}
        onClose={handleModalClose}
        onContinue={handleModalContinue}
        actionLabel={modalActionLabel}
      />
    </div>
  );
}

export default ExportPdfButton;
