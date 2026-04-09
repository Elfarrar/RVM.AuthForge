export interface UserProfile {
  id: string;
  fullName: string;
  email: string;
  avatarUrl: string | null;
  emailConfirmed: boolean;
  twoFactorEnabled: boolean;
  createdAt: string;
}

export interface LoginResponse {
  message: string;
  userId: string;
  fullName: string;
  requiresTwoFactor?: boolean;
}

export interface TwoFactorSetup {
  sharedKey: string;
  authenticatorUri: string;
}

export interface TwoFactorVerifyResponse {
  recoveryCodes: string[];
}
