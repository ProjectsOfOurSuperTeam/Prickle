export function getApiBaseUrl() {
  if (import.meta.env.DEV) {
    return '';
  }

  const httpsUrl = import.meta.env.services__api__https__0;
  const httpUrl = import.meta.env.services__api__http__0;

  const resolved = (httpsUrl || httpUrl || '').trim();

  if (!resolved) {
    throw new Error(
      'API base URL is not configured. Set services__api__https__0 or services__api__http__0 in environment.',
    );
  }

  return resolved.replace(/\/$/, '');
}
