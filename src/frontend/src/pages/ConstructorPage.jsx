import { useState } from 'react';
import { useApi } from '../services/useApi';

// Constructor Workspace: Main page with Canvas (Drag-and-Drop), tool panel and compatibility indicator
// function ConstructorPage() {
//   return (
//     <div>
//       <h1>Конструктор</h1>
//       <p>Робоча область з Canvas для створення флораріумів</p>
//     </div>
//   );
// }

function ConstructorPage() {
  const api = useApi();

  const [result, setResult] = useState(null);
  const [loading, setLoading] = useState(false);

  // Додаємо стани для динамічного ID та тіла запиту
  const [recordId, setRecordId] = useState('');
  const [requestBody, setRequestBody] = useState('{\n  "name": "Тестовий",\n  "volume": 10\n}');

  const handleRequest = async (apiCall) => {
    setLoading(true);
    setResult('Завантаження...');
    try {
      const response = await apiCall();
      setResult({ success: true, data: response });
    } catch (error) {
      setResult({
        success: false,
        status: error.status,
        message: error.message,
        detail: error.detail,
        errors: error.errors
      });
    } finally {
      setLoading(false);
    }
  };

  // Парсимо JSON з текстового поля безпечно
  const getParsedBody = () => {
    try {
      return JSON.parse(requestBody);
    } catch {
      alert('Невалідний JSON у полі "Тіло запиту"!');
      return null;
    }
  };

  return (
      <div style={{ padding: '20px', maxWidth: '800px' }}>
        <h1>Containers (Тест API)</h1>

        {/* Панель налаштувань */}
        <div style={{ display: 'flex', gap: '20px', marginBottom: '20px' }}>
          <div>
            <label style={{ display: 'block', marginBottom: '5px' }}>ID запису (для GET/PUT/DELETE):</label>
            <input
                type="text"
                value={recordId}
                onChange={(e) => setRecordId(e.target.value)}
                placeholder="Введи ID..."
                style={{ padding: '8px', width: '250px' }}
            />
          </div>
          <div style={{ flexGrow: 1 }}>
            <label style={{ display: 'block', marginBottom: '5px' }}>Тіло запиту (JSON для POST/PUT):</label>
            <textarea
                value={requestBody}
                onChange={(e) => setRequestBody(e.target.value)}
                style={{ width: '100%', height: '80px', padding: '8px', fontFamily: 'monospace' }}
            />
          </div>
        </div>

        {/* Кнопки */}
        <div style={{ display: 'flex', gap: '10px', flexWrap: 'wrap' }}>
          <button disabled={loading} onClick={() => handleRequest(() => api.containers.getAll())}>
            GET Всі
          </button>

          <button disabled={loading} onClick={() => {
            const body = getParsedBody();
            if (body) handleRequest(() => api.containers.add(body));
          }}>
            POST Створити
          </button>

          <button disabled={loading || !recordId} onClick={() => handleRequest(() => api.containers.get(recordId))}>
            GET Один (по ID)
          </button>

          <button disabled={loading || !recordId} onClick={() => {
            const body = getParsedBody();
            if (body) handleRequest(() => api.containers.update(recordId, body));
          }}>
            PUT Оновити
          </button>

          <button disabled={loading || !recordId} onClick={() => handleRequest(() => api.containers.delete(recordId))}>
            DELETE Видалити
          </button>
        </div>

        {/* Результат */}
        <div style={{ marginTop: '20px', padding: '15px', backgroundColor: '#2d2d2d', color: '#fff', borderRadius: '8px' }}>
          <h3 style={{ marginTop: 0 }}>Результат:</h3>
          <pre style={{ margin: 0, whiteSpace: 'pre-wrap', wordBreak: 'break-word' }}>
            {result ? JSON.stringify(result, null, 2) : 'Чекаю на запит...'}
          </pre>
        </div>
      </div>
  );
}

export default ConstructorPage;
