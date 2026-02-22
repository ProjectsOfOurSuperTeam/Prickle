import { useContext } from 'react';
import { ApiContext } from "./apiContext";

export function useApi() {
  return useContext(ApiContext);
}
