import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import Layout from './components/Layout';
import LandingPage from './pages/LandingPage';
import AuthPage from './pages/AuthPage';
import CatalogPage from './pages/CatalogPage';
import ConstructorPage from './pages/ConstructorPage';
import ResultPage from './pages/ResultPage';
import ProfilePage from './pages/ProfilePage';
import AdminDashboard from './pages/AdminDashboard';
import LogoutPage from './pages/LogoutPage';
import { useAuth } from './services/useAuth';
import './App.css';

function AdminRoute() {
  const { isAuthenticated, isAdmin } = useAuth();
  if (!isAuthenticated || !isAdmin) return <Navigate to="/" replace />;
  return <AdminDashboard />;
}

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Layout />}>
          <Route index element={<LandingPage />} />
          <Route path="auth" element={<AuthPage />} />
          <Route path="catalog" element={<CatalogPage />} />
          <Route path="constructor" element={<ConstructorPage />} />
          <Route path="result" element={<ResultPage />} />
          <Route path="profile" element={<ProfilePage />} />
          <Route path="logout" element={<LogoutPage />} />
          <Route path="admin" element={<AdminRoute />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
