import axios from 'axios';

const api = axios.create({ baseURL: '/api' });

let accessToken: string | null = null;

export function setToken(token: string | null) {
  accessToken = token;
}

export function getToken() {
  return accessToken;
}

api.interceptors.request.use(config => {
  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`;
  }
  return config;
});

api.interceptors.response.use(
  res => res,
  async error => {
    if (error.response?.status === 401 && accessToken) {
      accessToken = null;
      window.location.href = '/login';
    }
    return Promise.reject(error);
  },
);

export default api;
