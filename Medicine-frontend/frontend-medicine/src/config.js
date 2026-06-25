/**
 * Application configuration
 * Supports environment-based overrides via .env files
 */

const config = {
  // API Configuration
  api: {
    // Base URL for API calls (from env or default for dev)
    baseURL: import.meta.env.VITE_API_URL || https://romp-hatchery-underhand.ngrok-free.dev/api',

    // Request timeout in milliseconds
    timeout: import.meta.env.VITE_API_TIMEOUT ? parseInt(import.meta.env.VITE_API_TIMEOUT) : 30000,

    // Maximum number of auto-retries for GET requests
    retries: import.meta.env.VITE_API_RETRIES ? parseInt(import.meta.env.VITE_API_RETRIES) : 3,

    // Initial retry delay in milliseconds
    retryDelay: import.meta.env.VITE_API_RETRY_DELAY ? parseInt(import.meta.env.VITE_API_RETRY_DELAY) : 1000,

    // Exponential backoff multiplier
    backoffMultiplier: 2,

    // Maximum backoff delay in milliseconds
    maxBackoffDelay: 30000,
  },

  // Feature Flags
  features: {
    // Enable debug logging in development
    enableDebugLogging: import.meta.env.DEV,

    // Enable API call metrics
    enableMetrics: import.meta.env.DEV,

    // Enable request deduplication
    enableDeduplication: true,
  },

  // Authentication
  auth: {
    // Where to store the JWT token (localStorage or sessionStorage)
    tokenStorage: 'localStorage',

    // Token key name in storage
    tokenKey: 'medicine_admin_token',

    // User info key name in storage
    userKey: 'medicine_admin_user',

    // Auto-inject authorization header
    autoInjectHeader: true,
  },

  // UI Configuration
  ui: {
    // Show success notifications
    showNotifications: true,

    // Toast duration in milliseconds
    toastDuration: 3000,

    // Debounce delay for search inputs (ms)
    debounceDelay: 300,

    // Pagination size
    pageSize: 10,
  },

  // Development Tools
  dev: {
    // Log all API calls to console
    logApiCalls: import.meta.env.DEV,

    // Log response times
    logResponseTimes: import.meta.env.DEV,

    // Log Redux actions
    logReduxActions: import.meta.env.DEV,
  },
};

// Validate and ensure defaults
function ensureDefaults() {
  if (!config.api.baseURL) {
    config.api.baseURL = 'http://localhost:5054/api';
  }
  if (config.api.timeout < 1000) {
    config.api.timeout = 30000;
  }
  if (config.api.retries < 0) {
    config.api.retries = 3;
  }
}

ensureDefaults();

export default config;

/**
 * Get a nested config value
 * @param {string} path - dot-separated path (e.g., 'api.baseURL')
 * @param {any} defaultValue - fallback value if path not found
 */
export function getConfig(path, defaultValue = null) {
  return path.split('.').reduce((obj, key) => obj?.[key], config) ?? defaultValue;
}

/**
 * Set a config value
 * @param {string} path - dot-separated path (e.g., 'api.timeout')
 * @param {any} value - new value
 */
export function setConfig(path, value) {
  const keys = path.split('.');
  const lastKey = keys.pop();
  let obj = config;

  for (const key of keys) {
    if (!obj[key]) {
      obj[key] = {};
    }
    obj = obj[key];
  }

  obj[lastKey] = value;
}

// Expose to window for debugging
if (import.meta.env.DEV) {
  window.__config = config;
  window.__getConfig = getConfig;
  window.__setConfig = setConfig;
}
