import { useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import api from '../api/client';
import FormField from '../components/FormField';

export default function ResetPasswordPage() {
  const [searchParams] = useSearchParams();
  const [password, setPassword] = useState('');
  const [confirm, setConfirm] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (password !== confirm) { setError('Passwords do not match.'); return; }
    setLoading(true);
    try {
      await api.post('/account/reset-password', {
        email: searchParams.get('email'),
        token: searchParams.get('token'),
        newPassword: password,
      });
      navigate('/login');
    } catch (err: unknown) {
      const errors = (err as { response?: { data?: { errors?: string[] } } })?.response?.data?.errors;
      setError(errors?.join(' ') || 'Reset failed.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-card">
        <h1>New Password</h1>
        <form onSubmit={handleSubmit}>
          <FormField label="New Password" type="password" value={password} onChange={e => setPassword(e.target.value)} required minLength={8} />
          <FormField label="Confirm Password" type="password" value={confirm} onChange={e => setConfirm(e.target.value)} required />
          {error && <div className="error-msg">{error}</div>}
          <button className="btn-primary" type="submit" disabled={loading}>
            {loading ? 'Resetting...' : 'Reset Password'}
          </button>
        </form>
      </div>
    </div>
  );
}
