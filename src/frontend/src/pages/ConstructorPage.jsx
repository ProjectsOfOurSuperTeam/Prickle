import { useState } from 'react';
import { useApi } from '../services/useApi';

// Constructor Workspace: Main page with Canvas (Drag-and-Drop), tool panel and compatibility indicator
function ConstructorPage() {
  return (
    <div>
      <h1>Конструктор</h1>
      <p>Робоча область з Canvas для створення флораріумів</p>
    </div>
  );
}

export default ConstructorPage;
