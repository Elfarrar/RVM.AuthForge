import type { InputHTMLAttributes } from 'react';

interface Props extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  error?: string;
}

export default function FormField({ label, error, ...rest }: Props) {
  return (
    <div className="form-field">
      <label>{label}</label>
      <input {...rest} />
      {error && <span className="field-error">{error}</span>}
    </div>
  );
}
