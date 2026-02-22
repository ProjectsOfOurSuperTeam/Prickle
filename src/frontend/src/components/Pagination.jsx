function Pagination({ currentPage, totalPages, onPageChange }) {
  if (totalPages <= 1) return null;
  const pages = [];
  for (let i = 1; i <= totalPages; i++) {
    pages.push(i);
  }
  return (
    <nav style={{
      display: 'flex',
      justifyContent: 'center',
      gap: 12,
      marginTop: 40,
      marginBottom: 0,
      background: 'rgba(255,255,255,0.95)',
      borderRadius: 12,
      boxShadow: '0 2px 12px rgba(44,62,80,0.08)',
      padding: '1rem 2rem',
      alignItems: 'center',
      width: 'fit-content',
      marginLeft: 'auto',
      marginRight: 'auto',
    }}>
      <button
        onClick={() => onPageChange(currentPage - 1)}
        disabled={currentPage === 1}
        style={{
          padding: '0.6rem 1.2rem',
          borderRadius: 8,
          border: 'none',
          background: currentPage === 1 ? '#e9ecef' : '#f0f4ff',
          color: '#6c63ff',
          fontWeight: 700,
          fontSize: '1.1rem',
          cursor: currentPage === 1 ? 'not-allowed' : 'pointer',
          boxShadow: '0 1px 2px rgba(44,62,80,0.04)'
        }}
        aria-label="Попередня сторінка"
      >
        {'<'}
      </button>
      {pages.map(page => (
        <button
          key={page}
          onClick={() => onPageChange(page)}
          style={{
            padding: '0.6rem 1.2rem',
            borderRadius: 8,
            border: page === currentPage ? '2px solid #6c63ff' : '1.5px solid #ced4da',
            background: page === currentPage ? '#e0e7ff' : '#f8f9fa',
            color: page === currentPage ? '#2d3a4a' : '#495057',
            fontWeight: page === currentPage ? 700 : 500,
            fontSize: '1.1rem',
            cursor: page === currentPage ? 'default' : 'pointer',
            boxShadow: page === currentPage ? '0 2px 8px rgba(44,62,80,0.08)' : '0 1px 2px rgba(44,62,80,0.03)'
          }}
          disabled={page === currentPage}
          aria-current={page === currentPage ? 'page' : undefined}
        >
          {page}
        </button>
      ))}
      <button
        onClick={() => onPageChange(currentPage + 1)}
        disabled={currentPage === totalPages}
        style={{
          padding: '0.6rem 1.2rem',
          borderRadius: 8,
          border: 'none',
          background: currentPage === totalPages ? '#e9ecef' : '#f0f4ff',
          color: '#6c63ff',
          fontWeight: 700,
          fontSize: '1.1rem',
          cursor: currentPage === totalPages ? 'not-allowed' : 'pointer',
          boxShadow: '0 1px 2px rgba(44,62,80,0.04)'
        }}
        aria-label="Наступна сторінка"
      >
        {'>'}
      </button>
    </nav>
  );
}

export default Pagination;
