import { createContext, useContext, useMemo } from 'react';
import { useAuth } from './useAuth';
import { createApiClients } from './api';

const ApiContext = createContext(null);

export function ApiProvider({ children }) {
  const { getAccessToken } = useAuth();
  const api = useMemo(() => createApiClients(getAccessToken), [getAccessToken]);

  return <ApiContext.Provider value={api}>{children}</ApiContext.Provider>;
}

export function useApi() {
  return useContext(ApiContext);
}
