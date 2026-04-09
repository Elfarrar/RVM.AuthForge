import { useState, useEffect } from 'react';
import QRCode from 'react-qr-code';
import api from '../api/client';
import type { TwoFactorSetup } from '../types/models';
import FormField from '../components/FormField';

export default function TwoFactorPage() {
  const [enabled, setEnabled] = useState(false);
  const [setup, setSetup] = useState<TwoFactorSetup | null>(null);
  const [code, setCode] = useState('');
  const [recoveryCodes, setRecoveryCodes] = useState<string[]>([]);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.get('/account/2fa/status')
      .then(res => setEnabled(res.data.enabled))
      .finally(() => setLoading(false));
  }, []);

  const handleEnable = async () => {
    const res = await api.post<TwoFactorSetup>('/account/2fa/enable');
    setSetup(res.data);
  };

  const handleVerify = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    try {
      const res = await api.post('/account/2fa/verify', { code });
      setRecoveryCodes(res.data.recoveryCodes);
      setEnabled(true);
      setSetup(null);
    } catch {
      setError('Invalid code. Try again.');
    }
  };

  const handleDisable = async () => {
    await api.post('/account/2fa/disable');
    setEnabled(false);
    setSetup(null);
    setRecoveryCodes([]);
  };

  const handleNewCodes = async () => {
    const res = await api.post('/account/2fa/recovery-codes');
    setRecoveryCodes(res.data.recoveryCodes);
  };

  if (loading) return <div className="loading">Loading...</div>;

  return (
    <div className="page">
      <h1>Two-Factor Authentication</h1>

      <div className="tfa-card">
        <p className="tfa-status">
          Status: <strong className={enabled ? 'text-success' : 'text-muted'}>{enabled ? 'Enabled' : 'Disabled'}</strong>
        </p>

        {!enabled && !setup && (
          <button className="btn-primary" onClick={handleEnable}>Enable 2FA</button>
        )}

        {setup && (
          <div className="tfa-setup">
            <p>Scan this QR code with your authenticator app:</p>
            <div className="qr-container">
              <QRCode value={setup.authenticatorUri} size={200} bgColor="#0d1117" fgColor="#f0f6fc" />
            </div>
            <p className="tfa-key">Manual key: <code>{setup.sharedKey}</code></p>
            <form onSubmit={handleVerify}>
              <FormField label="Verification Code" value={code} onChange={e => setCode(e.target.value)}
                placeholder="000000" maxLength={6} required />
              {error && <div className="error-msg">{error}</div>}
              <button className="btn-primary" type="submit">Verify & Enable</button>
            </form>
          </div>
        )}

        {enabled && (
          <div className="tfa-actions">
            <button className="btn-secondary" onClick={handleNewCodes}>Generate Recovery Codes</button>
            <button className="btn-danger" onClick={handleDisable}>Disable 2FA</button>
          </div>
        )}

        {recoveryCodes.length > 0 && (
          <div className="recovery-codes">
            <h3>Recovery Codes</h3>
            <p>Save these codes in a safe place. Each can be used once.</p>
            <div className="codes-grid">
              {recoveryCodes.map((c, i) => <code key={i}>{c}</code>)}
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
