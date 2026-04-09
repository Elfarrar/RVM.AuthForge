import { useState } from 'react';
import { Link } from 'react-router-dom';
import api from '../api/client';
import FormField from '../components/FormField';

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState('');
  const [sent, setSent] = useState(false);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      await api.post('/account/forgot-password', { email });
      setSent(true);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-card">
        <h1>Reset Password</h1>
        {sent ? (
          <div className="success-msg">If the email exists, a reset link has been sent.</div>
        ) : (
          <form onSubmit={handleSubmit}>
            <FormField label="Email" type="email" value={email} onChange={e => setEmail(e.target.value)} required />
            <button className="btn-primary" type="submit" disabled={loading}>
              {loading ? 'Sending...' : 'Send Reset Link'}
            </button>
          </form>
        )}
        <div className="auth-links">
          <Link to="/login">Back to login</Link>
        </div>
      </div>
    </div>
  );
}
