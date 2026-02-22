import { useMemo } from 'react';
import { useAuth } from './useAuth';
import { createApiClients } from './api';
import { ApiContext } from "./apiContext";

export function ApiProvider({ children }) {
  const { getAccessToken } = useAuth();
  const api = useMemo(() => createApiClients(getAccessToken), [getAccessToken]);

  return <ApiContext.Provider value={api}>{children}</ApiContext.Provider>;
}
