import { useState } from 'react';
import { Outlet, Link } from 'react-router-dom';
import { HiMenu, HiX } from 'react-icons/hi';
import { useAuth } from '../services/useAuth';
import logo from '../assets/images/logo/logo.png';
import './Layout.css';

// Layout component with navigation
function Layout() {
  const { isAuthenticated } = useAuth();
  const [isMenuOpen, setIsMenuOpen] = useState(false);

  const toggleMenu = () => {
    setIsMenuOpen(!isMenuOpen);
  };

  const closeMenu = () => {
    setIsMenuOpen(false);
  };

  return (
    <div className="layout">
      <nav>
        <div className="nav-container">
          <Link to="/" className="nav-logo" onClick={closeMenu}>
            <img src={logo} alt="Prickle" className="logo-img" />
          </Link>
          <button 
            className="burger-menu"
            onClick={toggleMenu}
            aria-label="Toggle menu"
            aria-expanded={isMenuOpen}
          >
            {isMenuOpen ? <HiX /> : <HiMenu />}
          </button>
          <ul className={`nav-links ${isMenuOpen ? 'nav-links-open' : ''}`}>
            <li><Link to="/" onClick={closeMenu}>Головна</Link></li>
            <li><Link to="/catalog" onClick={closeMenu}>Каталог</Link></li>
            <li><Link to="/constructor" onClick={closeMenu}>Конструктор</Link></li>
            {isAuthenticated && <li><Link to="/profile" onClick={closeMenu}>Профіль</Link></li>}
            {isAuthenticated && <li><Link to="/logout" className="nav-btn-login" onClick={closeMenu}>Вийти</Link></li>}
            {!isAuthenticated && <li><Link to="/auth" className="nav-btn-login" onClick={closeMenu}>Увійти</Link></li>}
          </ul>
        </div>
      </nav>
      <main>
        <Outlet />
      </main>
    </div>
  );
}

export default Layout;
