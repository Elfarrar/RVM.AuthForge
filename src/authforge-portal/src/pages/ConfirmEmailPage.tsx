import { useEffect, useState } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import api from '../api/client';

export default function ConfirmEmailPage() {
  const [searchParams] = useSearchParams();
  const [status, setStatus] = useState<'loading' | 'success' | 'error'>('loading');

  useEffect(() => {
    const userId = searchParams.get('userId');
    const token = searchParams.get('token');
    if (!userId || !token) { setStatus('error'); return; }

    api.post('/account/confirm-email', { userId, token })
      .then(() => setStatus('success'))
      .catch(() => setStatus('error'));
  }, [searchParams]);

  return (
    <div className="auth-page">
      <div className="auth-card">
        <h1>Email Confirmation</h1>
        {status === 'loading' && <p>Confirming your email...</p>}
        {status === 'success' && <div className="success-msg">Email confirmed! You can now <Link to="/login">sign in</Link>.</div>}
        {status === 'error' && <div className="error-msg">Invalid or expired confirmation link.</div>}
      </div>
    </div>
  );
}
