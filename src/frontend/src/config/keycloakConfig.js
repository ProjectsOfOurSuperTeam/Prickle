const REALM = 'prickle';
const CLIENT_ID = 'public-client';

function resolveKeycloakBaseUrl() {
  const httpsUrl = import.meta.env.services__keycloak__https__0;
  const httpUrl = import.meta.env.services__keycloak__http__0;

  const resolved = (httpsUrl || httpUrl || '').trim();

  if (!resolved) {
    throw new Error(
      'Keycloak base URL is not configured. Set services__keycloak__https__0 or services__keycloak__http__0 in environment.',
    );
  }

  return resolved.replace(/\/$/, '');
}

export function createKeycloakConfig() {
  const baseUrl = resolveKeycloakBaseUrl();

  return {
    baseUrl,
    realm: REALM,
    clientId: CLIENT_ID,
    tokenEndpoint: `${baseUrl}/realms/${REALM}/protocol/openid-connect/token`,
    logoutEndpoint: `${baseUrl}/realms/${REALM}/protocol/openid-connect/logout`,
    userInfoEndpoint: `${baseUrl}/realms/${REALM}/protocol/openid-connect/userinfo`,
  };
}
