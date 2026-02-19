import { createKeycloakConfig } from '../config/keycloakConfig';

function buildFormData(payload) {
  const body = new URLSearchParams();
  Object.entries(payload).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') {
      body.set(key, value);
    }
  });
  return body;
}

async function parseError(response) {
  const fallback = `HTTP ${response.status}`;
  try {
    const data = await response.json();
    return data.error_description || data.error || fallback;
  } catch {
    return fallback;
  }
}

function decodeJwtPayload(token) {
  if (!token) {
    return null;
  }

  const parts = token.split('.');
  if (parts.length < 2) {
    return null;
  }

  const normalized = parts[1].replace(/-/g, '+').replace(/_/g, '/');
  const padded = normalized + '='.repeat((4 - (normalized.length % 4)) % 4);

  try {
    const json = decodeURIComponent(
      atob(padded)
        .split('')
        .map((char) => `%${`00${char.charCodeAt(0).toString(16)}`.slice(-2)}`)
        .join(''),
    );
    return JSON.parse(json);
  } catch {
    return null;
  }
}

export class KeycloakService {
  constructor(config = createKeycloakConfig(), fetchFn = (...args) => fetch(...args)) {
    this.config = config;
    this.fetchFn = fetchFn;
  }

  async login({ username, password }) {
    const body = buildFormData({
      grant_type: 'password',
      client_id: this.config.clientId,
      username,
      password,
      scope: 'openid profile email',
    });

    const response = await this.fetchFn(this.config.tokenEndpoint, {
      method: 'POST',
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      body,
    });

    if (!response.ok) {
      throw new Error(await parseError(response));
    }

    const tokens = await response.json();
    return {
      ...tokens,
      decodedAccessToken: decodeJwtPayload(tokens.access_token),
    };
  }

  async refresh(refreshToken) {
    const body = buildFormData({
      grant_type: 'refresh_token',
      client_id: this.config.clientId,
      refresh_token: refreshToken,
    });

    const response = await this.fetchFn(this.config.tokenEndpoint, {
      method: 'POST',
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      body,
    });

    if (!response.ok) {
      throw new Error(await parseError(response));
    }

    const tokens = await response.json();
    return {
      ...tokens,
      decodedAccessToken: decodeJwtPayload(tokens.access_token),
    };
  }

  async logout(refreshToken) {
    const body = buildFormData({
      client_id: this.config.clientId,
      refresh_token: refreshToken,
    });

    const response = await this.fetchFn(this.config.logoutEndpoint, {
      method: 'POST',
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      body,
    });

    if (!response.ok && response.status !== 204) {
      throw new Error(await parseError(response));
    }
  }
}

export function mapTokensToSession(tokens) {
  const expiresAt = Date.now() + tokens.expires_in * 1000;

  return {
    accessToken: tokens.access_token,
    refreshToken: tokens.refresh_token,
    tokenType: tokens.token_type,
    expiresAt,
    profile: {
      id: tokens.decodedAccessToken?.sub,
      email: tokens.decodedAccessToken?.email,
      username: tokens.decodedAccessToken?.preferred_username,
      name: tokens.decodedAccessToken?.name,
    },
  };
}
