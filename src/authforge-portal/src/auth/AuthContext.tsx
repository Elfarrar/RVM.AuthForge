import { createContext, useContext, useState, useCallback, type ReactNode } from 'react';
import api, { setToken } from '../api/client';
import type { UserProfile } from '../types/models';

interface AuthState {
  user: UserProfile | null;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<{ requiresTwoFactor?: boolean }>;
  register: (fullName: string, email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
  loadProfile: () => Promise<void>;
}

const AuthContext = createContext<AuthState | null>(null);

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserProfile | null>(null);

  const loadProfile = useCallback(async () => {
    try {
      const res = await api.get<UserProfile>('/account/profile');
      setUser(res.data);
    } catch {
      setUser(null);
      setToken(null);
    }
  }, []);

  const login = useCallback(async (email: string, password: string) => {
    const res = await api.post('/account/login', { email, password });
    if (res.data.requiresTwoFactor) {
      return { requiresTwoFactor: true };
    }
    // For cookie-based auth, the token comes via the sign-in
    // For demo, we use cookie auth from SignInAsync
    await loadProfile();
    return {};
  }, [loadProfile]);

  const register = useCallback(async (fullName: string, email: string, password: string) => {
    await api.post('/account/register', { fullName, email, password });
  }, []);

  const logout = useCallback(async () => {
    try { await api.post('/account/logout'); } catch { /* ignore */ }
    setToken(null);
    setUser(null);
  }, []);

  return (
    <AuthContext.Provider value={{ user, isAuthenticated: !!user, login, register, logout, loadProfile }}>
      {children}
    </AuthContext.Provider>
  );
}
