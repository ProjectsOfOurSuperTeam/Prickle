import { registerPricklePdfFonts } from './registerPricklePdfFonts';

function formatLiters(n) {
  if (!Number.isFinite(n)) return '—';
  const rounded = Math.round(n * 10) / 10;
  return `${rounded} л`;
}

function formatDate(iso) {
  if (!iso) return '—';
  try {
    return new Intl.DateTimeFormat('uk-UA', {
      dateStyle: 'medium',
      timeStyle: 'short',
    }).format(new Date(iso));
  } catch {
    return iso;
  }
}

/**
 * @param {*} payload - output of fetchFlorariumExportPayload
 * @param {{ fileName?: string; previewWindow?: Window | null }} [options] - pass a window from synchronous window.open() before any await for preview (avoids popup blockers)
 * @returns {Promise<void>}
 */
export async function generateFlorariumPdf(payload, options = {}) {
  const { default: pdfMake } = await import('pdfmake/build/pdfmake');
  await registerPricklePdfFonts(pdfMake);
  if (!pdfMake.vfs || !Object.keys(pdfMake.vfs).some((k) => k.endsWith('.ttf'))) {
    throw new Error('Не вдалося завантажити шрифти для PDF (vfs).');
  }

  const fileStem = options.fileName ?? `prickle-florarium-${payload.projectId?.slice(0, 8) ?? 'export'}`;

  const soilTableBody = [
    [
      { text: 'Шар', style: 'tableHeader', alignment: 'center' },
      { text: 'Компонент', style: 'tableHeader' },
      { text: '%', style: 'tableHeader', alignment: 'right' },
      { text: 'Обʼєм (л)', style: 'tableHeader', alignment: 'right' },
    ],
  ];

  if (payload.soilSection?.rows?.length) {
    for (const r of payload.soilSection.rows) {
      soilTableBody.push([
        String(r.order),
        r.componentName,
        { text: String(r.percentage), alignment: 'right' },
        { text: formatLiters(r.liters), alignment: 'right' },
      ]);
    }
  }

  const plants = payload.plants ?? [];
  const decorations = payload.decorations ?? [];

  const plantRows = plants.map((p) => {
    const latin = (p.nameLatin || '').trim();
    const title = latin ? `${p.name} (${latin})` : p.name;
    const lightText = typeof p.lightHint === 'string' && p.lightHint.trim() ? p.lightHint.trim() : '—';
    const waterText = typeof p.waterHint === 'string' && p.waterHint.trim() ? p.waterHint.trim() : '—';
    return [title, String(p.count), lightText, waterText];
  });

  const decoRows = decorations.map((d) => [d.name, String(d.count)]);

  const instructionParagraphs = buildInstructionParagraphs(payload);

  const docDefinition = {
    pageSize: 'A4',
    pageMargins: [40, 50, 40, 50],
    defaultStyle: {
      font: 'Montserrat',
      fontSize: 10,
    },
    content: [
      { text: 'Prickle — пакет флораріуму', style: 'title', margin: [0, 0, 0, 6] },
      {
        text: `Проєкт: ${payload.projectId}\nДата: ${formatDate(payload.createdAt)}`,
        style: 'muted',
        margin: [0, 0, 0, 16],
      },

      { text: 'Контейнер', style: 'h2' },
      {
        text: [
          { text: 'Форма: ', bold: true },
          `${payload.container.name}\n`,
          { text: 'Обʼєм: ', bold: true },
          `${formatLiters(payload.container.volume)} (розрахунок субстрату)\n`,
          { text: 'Тип: ', bold: true },
          payload.container.isClosed ? 'закритий (кришка / висока вологість)' : 'відкритий (краща вентиляція, швидше просихання)',
        ],
        margin: [0, 0, 0, 12],
      },

      { text: 'Субстрат — список покупок', style: 'h2' },
      ...(payload.soilSection
        ? [
            {
              text: `Формула: ${payload.soilSection.formulaName}`,
              margin: [0, 0, 0, 6],
            },
            ...(payload.soilSection.totalPercentage < 100
              ? [
                  {
                    text: `Увага: сума відсотків у формулі ${payload.soilSection.totalPercentage}% (не 100%). Обʼєми в таблиці — пропорційно заданим часткам.`,
                    color: '#555',
                    margin: [0, 0, 0, 6],
                  },
                ]
              : []),
            {
              table: {
                headerRows: 1,
                widths: [40, '*', 50, 70],
                body: soilTableBody,
              },
              layout: 'lightHorizontalLines',
              margin: [0, 0, 0, 16],
            },
          ]
        : [
            {
              text: 'Формула ґрунту в проєкті не обрана — розрахунок компонентів субстрату недоступний. Оберіть формулу в конструкторі та збережіть проєкт знову.',
              italics: true,
              margin: [0, 0, 0, 16],
            },
          ]),

      { text: 'Рослини', style: 'h2' },
      ...(plantRows.length
        ? [
            {
              table: {
                headerRows: 1,
                widths: ['*', 40, '*', '*'],
                body: [
                  [
                    { text: 'Назва', style: 'tableHeader' },
                    { text: 'К-ть', style: 'tableHeader', alignment: 'center' },
                    { text: 'Світло', style: 'tableHeader' },
                    { text: 'Вода', style: 'tableHeader' },
                  ],
                  ...plantRows,
                ],
              },
              layout: 'lightHorizontalLines',
              margin: [0, 0, 0, 16],
            },
          ]
        : [{ text: 'Рослин у проєкті немає.', italics: true, margin: [0, 0, 0, 16] }]),

      { text: 'Декор та наповнювачі', style: 'h2' },
      ...(decoRows.length
        ? [
            {
              table: {
                headerRows: 1,
                widths: ['*', 50],
                body: [
                  [
                    { text: 'Назва', style: 'tableHeader' },
                    { text: 'К-ть', style: 'tableHeader', alignment: 'center' },
                  ],
                  ...decoRows,
                ],
              },
              layout: 'lightHorizontalLines',
              margin: [0, 0, 0, 16],
            },
          ]
        : [{ text: 'Декору в проєкті немає.', italics: true, margin: [0, 0, 0, 16] }]),

      { text: 'Покрокова інструкція', style: 'h2' },
      ...instructionParagraphs,
    ],
    styles: {
      title: { font: 'Unbounded', fontSize: 18, bold: true },
      h2: { font: 'Unbounded', fontSize: 13, bold: true, margin: [0, 12, 0, 6] },
      muted: { fontSize: 9, color: '#555' },
      tableHeader: { bold: true, fillColor: '#eeeeee' },
    },
  };

  const pdfDoc = pdfMake.createPdf(docDefinition);
  const filename = `${fileStem}.pdf`;
  const previewWindow = options.previewWindow;

  if (previewWindow != null) {
    try {
      await pdfDoc.open(previewWindow);
    } catch (err) {
      try {
        previewWindow.close();
      } catch {
        // ignore
      }
      throw err instanceof Error ? err : new Error(String(err));
    }
    return;
  }

  // pdfmake 0.3+: download() returns Promise; without await errors stay unhandled and UI shows success too early.
  try {
    await pdfDoc.download(filename);
  } catch (firstErr) {
    try {
      const blob = await pdfDoc.getBlob();
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = filename;
      a.rel = 'noopener';
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
    } catch {
      throw firstErr instanceof Error ? firstErr : new Error(String(firstErr));
    }
  }
}

/** @param {*} payload */
function buildInstructionParagraphs(payload) {
  const plantCount = payload.plants?.length ?? 0;
  const lines = [];

  lines.push(
    'Підготуйте контейнер: промийте скло, перевірте герметичність дренажного отвору (якщо є).',
  );

  if (payload.container.isClosed) {
    lines.push(
      'Закрита форма: після поливу не залишайте застій води; періодично провітрюйте, щоб уникнути цвілі.',
    );
  } else {
    lines.push(
      'Відкрита форма: субстрат швидше сохне — контролюйте полив частіше, ніж у закритих флораріумах.',
    );
  }

  if (payload.soilSection?.rows?.length) {
    lines.push(
      'Насипте субстрат шарами у порядку, зазначеному в таблиці (знизу вгору або згідно з вашою композицією). Злегка ущільнюйте кожен шар.',
    );
    lines.push(
      'Обʼєми в таблиці — орієнтовні (літри сухого матеріалу); у магазині зазвичай вказують обʼєм упаковки — підберіть найближче за обʼємом або зважте на вагових упаковках за потреби.',
    );
  } else {
    lines.push(
      'Оберіть і підготуйте субстрат згідно з обраними рослинами (у PDF не вказано формулу — додайте її в конструкторі).',
    );
  }

  if (plantCount > 0) {
    lines.push(
      'Висадіть рослини з урахуванням поєднання світла та поливу (див. таблицю). Не змішуйте різко різні потреби без додаткового зонування всередині посудини.',
    );
  }

  lines.push(
    'Після збирання композиції злегка полийте (або обприсніть) залежно від обраних видів; дайте стеклу висохнути ззовні.',
  );

  lines.push(
    'Перші тижні спостерігайте за конденсатом і станом листя — коригуйте полив і вентиляцію.',
  );

  return lines.map((t, i) => ({
    text: `${i + 1}. ${t}`,
    margin: [0, 0, 0, 8],
    alignment: 'justify',
  }));
}
