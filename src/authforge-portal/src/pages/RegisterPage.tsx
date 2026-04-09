import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import FormField from '../components/FormField';

export default function RegisterPage() {
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirm, setConfirm] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { register } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (password !== confirm) { setError('Passwords do not match.'); return; }
    setError('');
    setLoading(true);
    try {
      await register(fullName, email, password);
      navigate('/login');
    } catch (err: unknown) {
      const errors = (err as { response?: { data?: { errors?: string[] } } })?.response?.data?.errors;
      setError(errors?.join(' ') || 'Registration failed.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-card">
        <h1>Create Account</h1>
        <form onSubmit={handleSubmit}>
          <FormField label="Full Name" value={fullName} onChange={e => setFullName(e.target.value)} required />
          <FormField label="Email" type="email" value={email} onChange={e => setEmail(e.target.value)} required />
          <FormField label="Password" type="password" value={password} onChange={e => setPassword(e.target.value)} required minLength={8} />
          <FormField label="Confirm Password" type="password" value={confirm} onChange={e => setConfirm(e.target.value)} required />
          {error && <div className="error-msg">{error}</div>}
          <button className="btn-primary" type="submit" disabled={loading}>
            {loading ? 'Creating...' : 'Register'}
          </button>
        </form>
        <div className="auth-links">
          <Link to="/login">Already have an account?</Link>
        </div>
      </div>
    </div>
  );
}
