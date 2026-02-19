import { useEffect, useState } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../services/useAuth';

function LogoutPage() {
  const { logout } = useAuth();
  const [isCompleted, setIsCompleted] = useState(false);

  useEffect(() => {
    let isActive = true;

    const performLogout = async () => {
      await logout();
      if (isActive) {
        setIsCompleted(true);
      }
    };

    performLogout();

    return () => {
      isActive = false;
    };
  }, [logout]);

  if (isCompleted) {
    return <Navigate to="/auth" replace />;
  }

  return <p>Вихід...</p>;
}

export default LogoutPage;
