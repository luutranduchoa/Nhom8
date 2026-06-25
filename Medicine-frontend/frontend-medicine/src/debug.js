/**
 * Debug logging utility for API calls and performance monitoring
 */

class APIDebugger {
  constructor() {
    this.enabled = import.meta.env.DEV;
    this.requests = [];
    this.maxRequests = 100;
  }

  /**
   * Log API request start
   */
  logRequest(url, method, options = {}) {
    if (!this.enabled) return;

    const request = {
      id: `${method}:${url}:${Date.now()}`,
      url,
      method,
      startTime: Date.now(),
      timestamp: new Date().toLocaleTimeString(),
      headers: options.headers || {},
    };

    this.requests.push(request);
    if (this.requests.length > this.maxRequests) {
      this.requests.shift();
    }

    console.log(`[API] ${method.padEnd(6)} ${url}`, {
      headers: options.headers,
      timestamp: request.timestamp,
    });

    return request.id;
  }

  /**
   * Log API response
   */
  logResponse(requestId, status, duration, data = null) {
    if (!this.enabled) return;

    const statusColor = status >= 400 ? '❌' : '✅';
    console.log(`${statusColor} [API] ${requestId} - Status ${status} (${duration}ms)`, data);
  }

  /**
   * Log API error
   */
  logError(requestId, error, duration) {
    if (!this.enabled) return;

    console.error(`❌ [API] ${requestId} - Error after ${duration}ms:`, error);
  }

  /**
   * Log retry attempt
   */
  logRetry(url, method, attempt, delay) {
    if (!this.enabled) return;

    console.warn(`⚠️  [API] Retrying ${method} ${url} (attempt ${attempt}, delay ${delay}ms)`);
  }

  /**
   * Get performance metrics
   */
  getMetrics(method = null) {
    if (!this.enabled) return null;

    const filtered = method
      ? this.requests.filter((r) => r.method === method)
      : this.requests;

    if (filtered.length === 0) return null;

    const durations = filtered
      .map((r) => r.duration || 0)
      .filter((d) => d > 0);

    if (durations.length === 0) return null;

    const sorted = durations.sort((a, b) => a - b);
    const avg = sorted.reduce((a, b) => a + b, 0) / sorted.length;
    const median = sorted[Math.floor(sorted.length / 2)];
    const max = sorted[sorted.length - 1];
    const min = sorted[0];

    return {
      count: filtered.length,
      avg: Math.round(avg),
      median,
      min,
      max,
    };
  }

  /**
   * Print summary of API calls
   */
  printSummary() {
    if (!this.enabled) return;

    console.group('[API] Summary');

    const methods = ['GET', 'POST', 'PUT', 'PATCH', 'DELETE'];
    methods.forEach((method) => {
      const metrics = this.getMetrics(method);
      if (metrics) {
        console.log(
          `${method}: ${metrics.count} calls | avg: ${metrics.avg}ms | min: ${metrics.min}ms | max: ${metrics.max}ms`,
        );
      }
    });

    console.groupEnd();
  }

  /**
   * Clear log history
   */
  clear() {
    this.requests = [];
  }

  /**
   * Export logs as JSON
   */
  exportLogs() {
    return JSON.stringify(this.requests, null, 2);
  }
}

export const debugger = new APIDebugger();

// Expose to window for console access in dev mode
if (import.meta.env.DEV) {
  window.__apiDebugger = debugger;
  console.log('API Debugger available via window.__apiDebugger');
}

export default debugger;
