import { useState, useEffect } from 'react';
import { useAuth } from '../auth/AuthContext';
import api from '../api/client';
import FormField from '../components/FormField';

export default function ProfilePage() {
  const { user, loadProfile } = useAuth();
  const [fullName, setFullName] = useState('');
  const [avatarUrl, setAvatarUrl] = useState('');
  const [message, setMessage] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (user) {
      setFullName(user.fullName);
      setAvatarUrl(user.avatarUrl || '');
    }
  }, [user]);

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      await api.put('/account/profile', { fullName, avatarUrl: avatarUrl || null });
      await loadProfile();
      setMessage('Profile updated.');
    } catch {
      setMessage('Failed to update profile.');
    } finally {
      setLoading(false);
    }
  };

  if (!user) return <div className="loading">Loading profile...</div>;

  return (
    <div className="page">
      <h1>Profile</h1>
      <div className="profile-card">
        <div className="profile-info">
          <p><strong>Email:</strong> {user.email}</p>
          <p><strong>Email Confirmed:</strong> {user.emailConfirmed ? 'Yes' : 'No'}</p>
          <p><strong>2FA:</strong> {user.twoFactorEnabled ? 'Enabled' : 'Disabled'}</p>
          <p><strong>Member since:</strong> {new Date(user.createdAt).toLocaleDateString()}</p>
        </div>

        <form onSubmit={handleSave}>
          <FormField label="Full Name" value={fullName} onChange={e => setFullName(e.target.value)} required />
          <FormField label="Avatar URL" value={avatarUrl} onChange={e => setAvatarUrl(e.target.value)} placeholder="https://..." />
          {message && <div className="info-msg">{message}</div>}
          <button className="btn-primary" type="submit" disabled={loading}>
            {loading ? 'Saving...' : 'Save'}
          </button>
        </form>
      </div>
    </div>
  );
}
