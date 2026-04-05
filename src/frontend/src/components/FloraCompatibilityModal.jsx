import './FloraCompatibilityModal.css';

/**
 * @param {{ open: boolean; onClose: () => void; onContinue: () => void; analysis: object | null; actionLabel: string }} props
 */
export function FloraCompatibilityModal({ open, onClose, onContinue, analysis, actionLabel }) {
  if (!open || !analysis) return null;

  const userSummary = /** @type {{ title?: string; problems?: string[]; whatToDo?: string[]; level?: string } | undefined} */ (
    analysis.userSummary
  );
  const careLines = /** @type {string[]} */ (analysis.careLines ?? []);
  const soilLines = /** @type {string[]} */ (analysis.soilLines ?? []);
  const allLines = [...careLines, ...soilLines];
  const pairingVerdict = /** @type {string | undefined} */ (analysis.pairingVerdict);
  const pairingLevel = /** @type {string | undefined} */ (analysis.pairingVerdictLevel);
  const pairingFactors = /** @type {{ key: string; label: string; detail: string; weightPct: number }[] | undefined} */ (
    analysis.pairingFactors
  );
  const useSimple = Boolean(userSummary?.title);

  return (
    <div className="flora-compat-overlay" role="presentation">
      <button type="button" className="flora-compat-backdrop" aria-label="Закрити" onClick={onClose} />
      <div
        className="flora-compat-dialog"
        role="dialog"
        aria-modal="true"
        aria-labelledby="flora-compat-title"
      >
        <h2 id="flora-compat-title" className="flora-compat-title">
          Перевірка перед PDF
        </h2>
        <p className="flora-compat-lead">
          Коротка оцінка сумісності рослин і ґрунту. Це підказки, а не обмеження — можна продовжити будь-коли.
        </p>
        {useSimple ? (
          <>
            <p className={`flora-compat-verdict flora-compat-verdict--${userSummary?.level || 'neutral'}`}>
              {userSummary?.title}
            </p>
            {userSummary?.problems?.length ? (
              <ul className="flora-compat-user-problems" aria-label="У чому справа">
                {userSummary.problems.map((line, idx) => (
                  <li key={`p-${idx}`}>{line}</li>
                ))}
              </ul>
            ) : null}
            {userSummary?.whatToDo?.length ? (
              <div className="flora-compat-user-do">
                <strong className="flora-compat-user-do-label">Що зробити</strong>
                <ul className="flora-compat-user-do-list">
                  {userSummary.whatToDo.map((line, idx) => (
                    <li key={`w-${idx}`}>{line}</li>
                  ))}
                </ul>
              </div>
            ) : null}
          </>
        ) : (
          <>
            {pairingVerdict ? (
              <p className={`flora-compat-verdict flora-compat-verdict--${pairingLevel || 'neutral'}`}>{pairingVerdict}</p>
            ) : null}
            {pairingFactors?.length ? (
              <ul className="flora-compat-factors" aria-label="Показники та відносні ваги">
                {pairingFactors.map((f) => (
                  <li key={f.key}>
                    <span className="flora-compat-factor-label">{f.label}</span>
                    {' — '}
                    {f.detail}{' '}
                    <span className="flora-compat-factor-w">(вага {f.weightPct}%)</span>
                  </li>
                ))}
              </ul>
            ) : null}
            {allLines.length === 0 ? (
              <p className="flora-compat-empty">Особливих зауважень немає.</p>
            ) : (
              <ul className="flora-compat-list">
                {allLines.map((line, idx) => (
                  <li key={`${idx}-${line.slice(0, 48)}`}>{line}</li>
                ))}
              </ul>
            )}
          </>
        )}
        <div className="flora-compat-actions">
          <button type="button" className="flora-compat-btn flora-compat-btn--ghost" onClick={onClose}>
            Скасувати
          </button>
          <button type="button" className="flora-compat-btn flora-compat-btn--primary" onClick={onContinue}>
            {actionLabel}
          </button>
        </div>
      </div>
    </div>
  );
}

export default FloraCompatibilityModal;
