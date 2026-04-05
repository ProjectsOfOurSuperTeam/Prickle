import montserratRegularUrl from '../../assets/fonts/pdf/Montserrat-Regular.ttf?url';
import montserratItalicUrl from '../../assets/fonts/pdf/Montserrat-Italic.ttf?url';
import montserratBoldUrl from '../../assets/fonts/pdf/Montserrat-Bold.ttf?url';
import montserratBoldItalicUrl from '../../assets/fonts/pdf/Montserrat-BoldItalic.ttf?url';
import unboundedRegularUrl from '../../assets/fonts/pdf/Unbounded-Regular.ttf?url';
import unboundedBoldUrl from '../../assets/fonts/pdf/Unbounded-Bold.ttf?url';

/** @param {ArrayBuffer} buffer */
function bufferToBase64(buffer) {
  const bytes = new Uint8Array(buffer);
  let binary = '';
  for (let i = 0; i < bytes.byteLength; i += 1) {
    binary += String.fromCharCode(bytes[i]);
  }
  return btoa(binary);
}

/**
 * Loads Montserrat + Unbounded (latin subset TTF from Google Fonts) into pdfmake vfs.
 * Unbounded has no italic file — italics map to Regular / Bold.
 * @param {*} pdfMake - pdfmake default export
 * @returns {Promise<void>}
 */
export async function registerPricklePdfFonts(pdfMake) {
  /** @type {Array<[string, string]>} */
  const files = [
    ['Montserrat-Regular.ttf', montserratRegularUrl],
    ['Montserrat-Italic.ttf', montserratItalicUrl],
    ['Montserrat-Bold.ttf', montserratBoldUrl],
    ['Montserrat-BoldItalic.ttf', montserratBoldItalicUrl],
    ['Unbounded-Regular.ttf', unboundedRegularUrl],
    ['Unbounded-Bold.ttf', unboundedBoldUrl],
  ];

  /** @type {Record<string, string>} */
  const vfs = {};
  await Promise.all(
    files.map(async ([vfsName, url]) => {
      const res = await fetch(url);
      if (!res.ok) {
        throw new Error(`Failed to load font asset: ${vfsName}`);
      }
      vfs[vfsName] = bufferToBase64(await res.arrayBuffer());
    }),
  );

  pdfMake.vfs = vfs;
  if (typeof pdfMake.addVirtualFileSystem === 'function') {
    pdfMake.addVirtualFileSystem(vfs);
  }

  pdfMake.fonts = {
    Montserrat: {
      normal: 'Montserrat-Regular.ttf',
      bold: 'Montserrat-Bold.ttf',
      italics: 'Montserrat-Italic.ttf',
      bolditalics: 'Montserrat-BoldItalic.ttf',
    },
    Unbounded: {
      normal: 'Unbounded-Regular.ttf',
      bold: 'Unbounded-Bold.ttf',
      italics: 'Unbounded-Regular.ttf',
      bolditalics: 'Unbounded-Bold.ttf',
    },
  };
}
