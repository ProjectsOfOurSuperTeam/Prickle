export const authContextValue = {
  isAuthenticated: false,
  isLoading: false,
  user: null,
  error: '',
  getAccessToken: () => null,
  login: async () => {},
  logout: async () => {},
  clearError: () => {},
};
