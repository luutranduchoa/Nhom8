/**
 * Robust API Client for Medicine Application
 * Features: retry with exponential backoff, timeout, AbortController, auto auth header, logging
 */

import { getStoredAccessToken } from './auth.js';

// Configuration for API requests
const API_CONFIG = {
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5054/api',
  timeout: 30000, // 30 seconds default
  retries: 3,
  retryDelay: 1000, // 1 second initial delay
  backoffMultiplier: 2,
  maxBackoffDelay: 30000, // 30 seconds max backoff
};

// Track in-flight requests to prevent duplicates
const inFlightRequests = new Map();

/**
 * Safe JSON parsing that handles empty or non-JSON responses
 */
async function safeJson(response) {
  try {
    const text = await response.text();
    if (!text) return null;
    return JSON.parse(text);
  } catch {
    return { message: text || 'Invalid response' };
  }
}

/**
 * Classify error type for retry decision
 * @param {Error|Response} error
 * @returns {object} { type, shouldRetry, statusCode }
 */
function classifyError(error) {
  // Network error
  if (error instanceof TypeError) {
    return {
      type: 'network',
      shouldRetry: true,
      statusCode: null,
      message: error.message,
    };
  }

  // HTTP error response
  if (error instanceof Response) {
    const statusCode = error.status;
    const shouldRetry =
      statusCode === 408 || // Request Timeout
      statusCode === 429 || // Too Many Requests
      statusCode >= 500; // Server errors

    return {
      type: 'http',
      shouldRetry,
      statusCode,
      message: `HTTP ${statusCode}`,
    };
  }

  return {
    type: 'unknown',
    shouldRetry: false,
    statusCode: null,
    message: error.message || 'Unknown error',
  };
}

/**
 * Calculate exponential backoff delay
 */
function calculateBackoff(attempt) {
  const delay = API_CONFIG.retryDelay * Math.pow(API_CONFIG.backoffMultiplier, attempt);
  return Math.min(delay, API_CONFIG.maxBackoffDelay);
}

/**
 * Sleep utility for retries
 */
function sleep(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

/**
 * Generate cache key for request deduplication
 */
function getCacheKey(url, method) {
  return `${method.toUpperCase()}:${url}`;
}

/**
 * Core request function with retry logic
 */
async function request(url, options = {}) {
  const {
    method = 'GET',
    body,
    headers = {},
    timeout = API_CONFIG.timeout,
    retries = API_CONFIG.retries,
    skipRetry = false, // Don't retry for POSTs, PUTs, DELETEs by default
    requestKey, // For deduplication
  } = options;

  const fullUrl = url.startsWith('http') ? url : `${API_CONFIG.baseURL}${url}`;

  // Check for duplicate in-flight requests (GET only by default)
  if (method === 'GET' && requestKey) {
    const cacheKey = getCacheKey(fullUrl, method);
    if (inFlightRequests.has(cacheKey)) {
      logDebug(`Returning cached request for ${cacheKey}`);
      return inFlightRequests.get(cacheKey);
    }
  }

  // Add authorization header if token exists
  const authToken = getStoredAccessToken();
  const finalHeaders = {
    'Content-Type': 'application/json',
    ...headers,
  };
  if (authToken) {
    finalHeaders.Authorization = `Bearer ${authToken}`;
  }

  const controller = new AbortController();
  const timeoutId = setTimeout(() => controller.abort(), timeout);

  let lastError;
  let attempt = 0;

  while (attempt <= retries) {
    try {
      logDebug(`[${method}] ${fullUrl} (attempt ${attempt + 1}/${retries + 1})`);

      const response = await fetch(fullUrl, {
        method,
        headers: finalHeaders,
        body: body ? JSON.stringify(body) : undefined,
        signal: controller.signal,
      });

      clearTimeout(timeoutId);

      if (!response.ok) {
        throw response;
      }

      const data = await safeJson(response);

      // Cache successful GET requests
      if (method === 'GET' && requestKey) {
        const cacheKey = getCacheKey(fullUrl, method);
        inFlightRequests.delete(cacheKey);
      }

      return {
        success: true,
        data,
        status: response.status,
      };
    } catch (error) {
      lastError = error;
      const errorInfo = classifyError(error);

      logDebug(`Error: ${errorInfo.type} - ${errorInfo.message}`);

      // Abort error (timeout)
      if (error.name === 'AbortError') {
        logDebug(`Request timeout after ${timeout}ms`);
        return {
          success: false,
          error: 'timeout',
          message: `Request timeout after ${timeout}ms`,
          status: 408,
        };
      }

      // Decide whether to retry
      const shouldRetry =
        !skipRetry &&
        attempt < retries &&
        errorInfo.shouldRetry &&
        errorInfo.statusCode !== 401 && // Don't retry auth errors
        errorInfo.statusCode !== 403 && // Don't retry forbidden
        errorInfo.statusCode !== 404; // Don't retry not found

      if (!shouldRetry) {
        clearTimeout(timeoutId);

        // For HTTP errors, try to extract error message from response
        if (error instanceof Response) {
          const errorData = await safeJson(error);
          return {
            success: false,
            error: errorInfo.type,
            message: errorData?.message || errorInfo.message,
            status: error.status,
            data: errorData,
          };
        }

        return {
          success: false,
          error: errorInfo.type,
          message: errorInfo.message,
          status: errorInfo.statusCode,
        };
      }

      // Wait before retry
      const backoffDelay = calculateBackoff(attempt);
      logDebug(`Retrying in ${backoffDelay}ms...`);
      await sleep(backoffDelay);
      attempt += 1;
    }
  }

  // All retries exhausted
  clearTimeout(timeoutId);
  const errorInfo = classifyError(lastError);

  if (lastError instanceof Response) {
    const errorData = await safeJson(lastError);
    return {
      success: false,
      error: 'max_retries',
      message: `Failed after ${retries} retries: ${errorInfo.message}`,
      status: lastError.status,
      data: errorData,
    };
  }

  return {
    success: false,
    error: 'max_retries',
    message: `Failed after ${retries} retries: ${errorInfo.message}`,
    status: null,
  };
}

/**
 * HTTP verb convenience methods
 */
const apiClient = {
  /**
   * GET request (allows retry by default)
   */
  get: async (url, options = {}) => {
    return request(url, {
      method: 'GET',
      ...options,
      skipRetry: options.skipRetry ?? false,
    });
  },

  /**
   * POST request (no retry by default for data mutation safety)
   */
  post: async (url, body, options = {}) => {
    return request(url, {
      method: 'POST',
      body,
      ...options,
      skipRetry: options.skipRetry ?? true,
    });
  },

  /**
   * PUT request (no retry by default)
   */
  put: async (url, body, options = {}) => {
    return request(url, {
      method: 'PUT',
      body,
      ...options,
      skipRetry: options.skipRetry ?? true,
    });
  },

  /**
   * PATCH request (no retry by default)
   */
  patch: async (url, body, options = {}) => {
    return request(url, {
      method: 'PATCH',
      body,
      ...options,
      skipRetry: options.skipRetry ?? true,
    });
  },

  /**
   * DELETE request (no retry by default)
   */
  delete: async (url, options = {}) => {
    return request(url, {
      method: 'DELETE',
      ...options,
      skipRetry: options.skipRetry ?? true,
    });
  },

  /**
   * Form data upload (for multipart/form-data)
   */
  uploadForm: async (url, formData, options = {}) => {
    const authToken = getStoredAccessToken();
    const headers = {};
    if (authToken) {
      headers.Authorization = `Bearer ${authToken}`;
    }

    try {
      logDebug(`[UPLOAD] ${url}`);

      const controller = new AbortController();
      const timeout = options.timeout || API_CONFIG.timeout;
      const timeoutId = setTimeout(() => controller.abort(), timeout);

      const response = await fetch(
        url.startsWith('http') ? url : `${API_CONFIG.baseURL}${url}`,
        {
          method: 'POST',
          headers, // Don't set Content-Type, let browser set it for multipart
          body: formData,
          signal: controller.signal,
        },
      );

      clearTimeout(timeoutId);

      if (!response.ok) {
        const errorData = await safeJson(response);
        return {
          success: false,
          error: 'http',
          message: errorData?.message || `HTTP ${response.status}`,
          status: response.status,
          data: errorData,
        };
      }

      const data = await safeJson(response);
      return {
        success: true,
        data,
        status: response.status,
      };
    } catch (error) {
      if (error.name === 'AbortError') {
        return {
          success: false,
          error: 'timeout',
          message: `Upload timeout after ${options.timeout || API_CONFIG.timeout}ms`,
          status: 408,
        };
      }

      return {
        success: false,
        error: 'network',
        message: error.message,
        status: null,
      };
    }
  },

  /**
   * Clear in-flight request cache
   */
  clearCache: () => {
    inFlightRequests.clear();
  },

  /**
   * Configure API settings
   */
  configure: (config) => {
    Object.assign(API_CONFIG, config);
  },

  // Expose config for debugging
  config: API_CONFIG,
};

/**
 * Debug logging function (only logs in dev mode)
 */
function logDebug(message, data) {
  if (import.meta.env.DEV) {
    const timestamp = new Date().toLocaleTimeString();
    console.log(`[API ${timestamp}] ${message}`, data || '');
  }
}

export default apiClient;
export { safeJson, classifyError, API_CONFIG };
