/**
 * Niramoy Health Centralized API Client
 * Manages HTTP communication, headers, auth tokens, guest tokens, and error transformations.
 */

const API_BASE_URL = '/api/v1';

// Guest token initialization
export function getOrCreateGuestToken() {
  let guestToken = localStorage.getItem('niramoy_guest_token');
  if (!guestToken) {
    guestToken = crypto.randomUUID();
    localStorage.setItem('niramoy_guest_token', guestToken);
  }
  return guestToken;
}

export function clearGuestToken() {
  localStorage.removeItem('niramoy_guest_token');
}

// Token management
export function getAccessToken() {
  return localStorage.getItem('niramoy_access_token');
}

export function getRefreshToken() {
  return localStorage.getItem('niramoy_refresh_token');
}

export function setTokens(accessToken, refreshToken) {
  if (accessToken) localStorage.setItem('niramoy_access_token', accessToken);
  if (refreshToken) localStorage.setItem('niramoy_refresh_token', refreshToken);
}

export function clearTokens() {
  localStorage.removeItem('niramoy_access_token');
  localStorage.removeItem('niramoy_refresh_token');
  localStorage.removeItem('niramoy_user');
}

let isRefreshing = false;
let refreshQueue = [];

async function handleTokenRefresh() {
  const refreshToken = getRefreshToken();
  if (!refreshToken) {
    clearTokens();
    window.dispatchEvent(new CustomEvent('auth:expired'));
    throw new Error('Session expired');
  }

  try {
    const response = await fetch(`${API_BASE_URL}/auth/refresh`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken }),
    });

    const result = await response.json();
    if (!response.ok || !result.success) {
      clearTokens();
      window.dispatchEvent(new CustomEvent('auth:expired'));
      throw new Error(result.message || 'Failed to refresh token');
    }

    setTokens(result.data.accessToken, result.data.refreshToken);
    window.dispatchEvent(new CustomEvent('auth:refreshed', { detail: result.data.user }));
    return result.data.accessToken;
  } catch (err) {
    clearTokens();
    window.dispatchEvent(new CustomEvent('auth:expired'));
    throw err;
  }
}

export async function apiClient(endpoint, options = {}) {
  const url = endpoint.startsWith('http') ? endpoint : `${API_BASE_URL}${endpoint.startsWith('/') ? endpoint : `/${endpoint}`}`;
  
  const headers = {
    ...options.headers,
  };

  // Only set application/json if body is not FormData
  if (!(options.body instanceof FormData) && !headers['Content-Type']) {
    headers['Content-Type'] = 'application/json';
  }

  const token = getAccessToken();
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  } else {
    headers['X-Guest-Token'] = getOrCreateGuestToken();
  }

  const config = {
    ...options,
    headers,
  };

  let response = await fetch(url, config);

  // Handle 401 Unauthorized with token refresh retry
  if (response.status === 401 && token && !options._isRetry) {
    if (!isRefreshing) {
      isRefreshing = true;
      try {
        const newToken = await handleTokenRefresh();
        isRefreshing = false;
        refreshQueue.forEach(cb => cb(newToken));
        refreshQueue = [];
      } catch (err) {
        isRefreshing = false;
        refreshQueue = [];
        throw err;
      }
    } else {
      await new Promise(resolve => refreshQueue.push(resolve));
    }

    // Retry request with new token
    return apiClient(endpoint, {
      ...options,
      _isRetry: true,
      headers: {
        ...options.headers,
        'Authorization': `Bearer ${getAccessToken()}`,
      },
    });
  }

  const contentType = response.headers.get('content-type');
  let data = null;
  if (contentType && contentType.includes('application/json')) {
    data = await response.json();
  } else {
    data = await response.text();
  }

  if (!response.ok) {
    const error = new Error(
      (data && data.message) || `HTTP error! Status: ${response.status}`
    );
    error.status = response.status;
    error.data = data;
    error.errors = data?.errors || null;
    throw error;
  }

  return data;
}
