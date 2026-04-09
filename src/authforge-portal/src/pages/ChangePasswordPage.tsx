import { useState } from 'react';
import api from '../api/client';
import FormField from '../components/FormField';

export default function ChangePasswordPage() {
  const [current, setCurrent] = useState('');
  const [newPwd, setNewPwd] = useState('');
  const [confirm, setConfirm] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (newPwd !== confirm) { setError('Passwords do not match.'); return; }
    setError(''); setSuccess('');
    setLoading(true);
    try {
      await api.post('/account/change-password', { currentPassword: current, newPassword: newPwd });
      setSuccess('Password changed successfully.');
      setCurrent(''); setNewPwd(''); setConfirm('');
    } catch (err: unknown) {
      const errors = (err as { response?: { data?: { errors?: string[] } } })?.response?.data?.errors;
      setError(errors?.join(' ') || 'Failed to change password.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page">
      <h1>Change Password</h1>
      <div className="auth-card">
        <form onSubmit={handleSubmit}>
          <FormField label="Current Password" type="password" value={current} onChange={e => setCurrent(e.target.value)} required />
          <FormField label="New Password" type="password" value={newPwd} onChange={e => setNewPwd(e.target.value)} required minLength={8} />
          <FormField label="Confirm New Password" type="password" value={confirm} onChange={e => setConfirm(e.target.value)} required />
          {error && <div className="error-msg">{error}</div>}
          {success && <div className="success-msg">{success}</div>}
          <button className="btn-primary" type="submit" disabled={loading}>
            {loading ? 'Changing...' : 'Change Password'}
          </button>
        </form>
      </div>
    </div>
  );
}
