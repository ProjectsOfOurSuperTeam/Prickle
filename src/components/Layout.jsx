import { Outlet, Link } from 'react-router-dom';

// Layout component with navigation
function Layout() {
  return (
    <div>
      <nav>
        <Link to="/">Головна</Link>
        <Link to="/catalog">Каталог</Link>
        <Link to="/constructor">Конструктор</Link>
        <Link to="/profile">Профіль</Link>
        <Link to="/auth">Вхід</Link>
      </nav>
      <main>
        <Outlet />
      </main>
    </div>
  );
}

export default Layout;
