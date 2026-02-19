import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../services/useAuth';
import './AuthPage.css';

const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const initialLogin = { email: '', password: '' };
const initialRegister = { name: '', email: '', password: '', confirmPassword: '' };

function AuthPage() {
  const navigate = useNavigate();
  const { login, isLoading, error: authError, clearError } = useAuth();
  const [mode, setMode] = useState('login');
  const [loginForm, setLoginForm] = useState(initialLogin);
  const [registerForm, setRegisterForm] = useState(initialRegister);
  const [submitted, setSubmitted] = useState({ login: false, register: false });
  const [status, setStatus] = useState({ login: '', register: '' });

  const loginErrors = useMemo(() => {
    const errors = {};
    if (!loginForm.email.trim()) {
      errors.email = 'Вкажіть email.';
    } else if (!emailRegex.test(loginForm.email)) {
      errors.email = 'Невірний формат email.';
    }
    if (!loginForm.password.trim()) {
      errors.password = 'Вкажіть пароль.';
    }
    return errors;
  }, [loginForm]);

  const registerErrors = useMemo(() => {
    const errors = {};
    if (!registerForm.name.trim()) {
      errors.name = "Вкажіть ім'я.";
    }
    if (!registerForm.email.trim()) {
      errors.email = 'Вкажіть email.';
    } else if (!emailRegex.test(registerForm.email)) {
      errors.email = 'Невірний формат email.';
    }
    if (!registerForm.password.trim()) {
      errors.password = 'Вкажіть пароль.';
    } else if (registerForm.password.length < 8) {
      errors.password = 'Пароль має містити щонайменше 8 символів.';
    }
    if (!registerForm.confirmPassword.trim()) {
      errors.confirmPassword = 'Підтвердіть пароль.';
    } else if (registerForm.confirmPassword !== registerForm.password) {
      errors.confirmPassword = 'Паролі не збігаються.';
    }
    return errors;
  }, [registerForm]);

  const isLoginValid = Object.keys(loginErrors).length === 0;
  const isRegisterValid = Object.keys(registerErrors).length === 0;

  const handleLoginSubmit = async (event) => {
    event.preventDefault();
    setSubmitted((prev) => ({ ...prev, login: true }));
    clearError();

    if (!isLoginValid) {
      setStatus((prev) => ({ ...prev, login: 'Перевірте помилки у формі.' }));
      return;
    }

    try {
      await login({ email: loginForm.email.trim(), password: loginForm.password });
      setStatus((prev) => ({ ...prev, login: 'Вхід успішний.' }));
      navigate('/profile');
    } catch {
      setStatus((prev) => ({ ...prev, login: 'Помилка входу. Перевірте облікові дані.' }));
    }
  };

  const handleRegisterSubmit = (event) => {
    event.preventDefault();
    setSubmitted((prev) => ({ ...prev, register: true }));

    if (!isRegisterValid) {
      setStatus((prev) => ({ ...prev, register: 'Перевірте помилки у формі.' }));
      return;
    }

    // TODO: Registration via Keycloak API is intentionally deferred by request.
    setStatus((prev) => ({
      ...prev,
      register: 'Реєстрацію тимчасово не підключено. Скористайтеся входом.',
    }));
  };

  const showLoginErrors = submitted.login && !isLoginValid;
  const showRegisterErrors = submitted.register && !isRegisterValid;

  return (
    <div className="auth-page">
      <div className="auth-card">
        <header className="auth-header">
          <h1>Авторизація</h1>
          <p>Увійдіть або створіть новий акаунт.</p>
        </header>

        <div className="auth-tabs" role="tablist">
          <button
            type="button"
            className={`auth-tab ${mode === 'login' ? 'is-active' : ''}`}
            onClick={() => setMode('login')}
            role="tab"
            aria-selected={mode === 'login'}
          >
            Вхід
          </button>
          <button
            type="button"
            className={`auth-tab ${mode === 'register' ? 'is-active' : ''}`}
            onClick={() => setMode('register')}
            role="tab"
            aria-selected={mode === 'register'}
          >
            Реєстрація
          </button>
        </div>

        {mode === 'login' ? (
          <form className="auth-form" onSubmit={handleLoginSubmit} noValidate>
            <label className="field">
              <span>Email</span>
              <input
                type="email"
                name="email"
                value={loginForm.email}
                onChange={(event) =>
                  setLoginForm((prev) => ({ ...prev, email: event.target.value }))
                }
                autoComplete="email"
                aria-invalid={Boolean(showLoginErrors && loginErrors.email)}
                aria-describedby="login-email-error"
              />
              {showLoginErrors && loginErrors.email ? (
                <span className="error" id="login-email-error">
                  {loginErrors.email}
                </span>
              ) : null}
            </label>

            <label className="field">
              <span>Пароль</span>
              <input
                type="password"
                name="password"
                value={loginForm.password}
                onChange={(event) =>
                  setLoginForm((prev) => ({ ...prev, password: event.target.value }))
                }
                autoComplete="current-password"
                aria-invalid={Boolean(showLoginErrors && loginErrors.password)}
                aria-describedby="login-password-error"
              />
              {showLoginErrors && loginErrors.password ? (
                <span className="error" id="login-password-error">
                  {loginErrors.password}
                </span>
              ) : null}
            </label>

            {status.login ? <p className="auth-hint">{status.login}</p> : null}
            {authError ? <p className="auth-hint">{authError}</p> : null}

            <div className="auth-actions">
              <button type="submit" disabled={isLoading}>
                {isLoading ? 'Вхід...' : 'Увійти'}
              </button>
              <button
                type="button"
                className="auth-link"
                onClick={() => setMode('register')}
              >
                Ще немає акаунта?
              </button>
            </div>
          </form>
        ) : (
          <form className="auth-form" onSubmit={handleRegisterSubmit} noValidate>
            <label className="field">
              <span>Ім'я</span>
              <input
                type="text"
                name="name"
                value={registerForm.name}
                onChange={(event) =>
                  setRegisterForm((prev) => ({ ...prev, name: event.target.value }))
                }
                autoComplete="given-name"
                aria-invalid={Boolean(showRegisterErrors && registerErrors.name)}
                aria-describedby="register-name-error"
              />
              {showRegisterErrors && registerErrors.name ? (
                <span className="error" id="register-name-error">
                  {registerErrors.name}
                </span>
              ) : null}
            </label>

            <label className="field">
              <span>Email</span>
              <input
                type="email"
                name="email"
                value={registerForm.email}
                onChange={(event) =>
                  setRegisterForm((prev) => ({ ...prev, email: event.target.value }))
                }
                autoComplete="email"
                aria-invalid={Boolean(showRegisterErrors && registerErrors.email)}
                aria-describedby="register-email-error"
              />
              {showRegisterErrors && registerErrors.email ? (
                <span className="error" id="register-email-error">
                  {registerErrors.email}
                </span>
              ) : null}
            </label>

            <label className="field">
              <span>Пароль</span>
              <input
                type="password"
                name="password"
                value={registerForm.password}
                onChange={(event) =>
                  setRegisterForm((prev) => ({ ...prev, password: event.target.value }))
                }
                autoComplete="new-password"
                aria-invalid={Boolean(showRegisterErrors && registerErrors.password)}
                aria-describedby="register-password-error"
              />
              {showRegisterErrors && registerErrors.password ? (
                <span className="error" id="register-password-error">
                  {registerErrors.password}
                </span>
              ) : null}
            </label>

            <label className="field">
              <span>Підтвердіть пароль</span>
              <input
                type="password"
                name="confirmPassword"
                value={registerForm.confirmPassword}
                onChange={(event) =>
                  setRegisterForm((prev) => ({ ...prev, confirmPassword: event.target.value }))
                }
                autoComplete="new-password"
                aria-invalid={Boolean(showRegisterErrors && registerErrors.confirmPassword)}
                aria-describedby="register-confirm-error"
              />
              {showRegisterErrors && registerErrors.confirmPassword ? (
                <span className="error" id="register-confirm-error">
                  {registerErrors.confirmPassword}
                </span>
              ) : null}
            </label>

            {status.register ? <p className="auth-hint">{status.register}</p> : null}

            <div className="auth-actions">
              <button type="submit">Створити акаунт</button>
              <button
                type="button"
                className="auth-link"
                onClick={() => setMode('login')}
              >
                Уже маєте акаунт?
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
}

export default AuthPage;
