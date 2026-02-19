import { useCallback, useMemo, useState } from 'react';
import { AuthContext } from './authContext';
import { KeycloakService, mapTokensToSession } from './keycloakService';

const STORAGE_KEY = 'prickle.auth.session';

function readPersistedSession() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) {
      return null;
    }

    const parsed = JSON.parse(raw);
    if (!parsed?.accessToken || !parsed?.refreshToken) {
      return null;
    }

    if (typeof parsed.expiresAt !== 'number' || parsed.expiresAt <= Date.now()) {
      return null;
    }

    return parsed;
  } catch {
    return null;
  }
}

function persistSession(session) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
}

function clearPersistedSession() {
  localStorage.removeItem(STORAGE_KEY);
}

export function AuthProvider({ children }) {
  const [session, setSession] = useState(() => readPersistedSession());
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  const service = useMemo(() => new KeycloakService(), []);

  const login = useCallback(
    async ({ email, password }) => {
      setIsLoading(true);
      setError('');

      try {
        const tokens = await service.login({ username: email, password });
        const nextSession = mapTokensToSession(tokens);
        setSession(nextSession);
        persistSession(nextSession);
      } catch (err) {
        const message = err instanceof Error ? err.message : 'Не вдалося увійти.';
        setError(message);
        throw err;
      } finally {
        setIsLoading(false);
      }
    },
    [service],
  );

  const logout = useCallback(async () => {
    setError('');

    if (session?.refreshToken) {
      try {
        await service.logout(session.refreshToken);
      } catch (err) {
        const message = err instanceof Error ? err.message : 'Не вдалося завершити сесію на сервері.';
        setError(message);
      }
    }

    setSession(null);
    clearPersistedSession();
  }, [service, session]);

  const clearError = useCallback(() => {
    setError('');
  }, []);

  const value = useMemo(
    () => ({
      isAuthenticated: Boolean(session?.accessToken),
      isLoading,
      user: session?.profile || null,
      error,
      login,
      logout,
      clearError,
    }),
    [session, isLoading, error, login, logout, clearError],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
