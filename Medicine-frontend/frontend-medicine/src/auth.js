/**
 * Authentication utilities
 */

const AUTH_TOKEN_KEY = 'medicine_admin_token';
const AUTH_USER_KEY = 'medicine_admin_user';

/**
 * Store access token in localStorage
 */
export function storeAccessToken(token) {
  if (token) {
    localStorage.setItem(AUTH_TOKEN_KEY, token);
  }
}

/**
 * Retrieve access token from localStorage
 */
export function getStoredAccessToken() {
  return localStorage.getItem(AUTH_TOKEN_KEY) || null;
}

/**
 * Remove access token from localStorage
 */
export function clearAccessToken() {
  localStorage.removeItem(AUTH_TOKEN_KEY);
}

/**
 * Store user info in localStorage
 */
export function storeUserInfo(user) {
  if (user) {
    localStorage.setItem(AUTH_USER_KEY, JSON.stringify(user));
  }
}

/**
 * Retrieve user info from localStorage
 */
export function getStoredUserInfo() {
  try {
    const stored = localStorage.getItem(AUTH_USER_KEY);
    return stored ? JSON.parse(stored) : null;
  } catch {
    return null;
  }
}

/**
 * Remove user info from localStorage
 */
export function clearUserInfo() {
  localStorage.removeItem(AUTH_USER_KEY);
}

/**
 * Logout: clear all auth data
 */
export function logout() {
  clearAccessToken();
  clearUserInfo();
}

/**
 * Check if user is authenticated
 */
export function isAuthenticated() {
  return Boolean(getStoredAccessToken());
}

/**
 * Get auth headers for requests
 */
export function getAuthHeaders() {
  const token = getStoredAccessToken();
  if (token) {
    return {
      'Authorization': `Bearer ${token}`,
    };
  }
  return {};
}
