import { BrowserRouter, Routes, Route } from 'react-router-dom';
import Layout from './components/Layout';
import LandingPage from './pages/LandingPage';
import AuthPage from './pages/AuthPage';
import CatalogPage from './pages/CatalogPage';
import ConstructorPage from './pages/ConstructorPage';
import ResultPage from './pages/ResultPage';
import ProfilePage from './pages/ProfilePage';
import AdminDashboard from './pages/AdminDashboard';
import './App.css';

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
          <Route path="admin" element={<AdminDashboard />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
