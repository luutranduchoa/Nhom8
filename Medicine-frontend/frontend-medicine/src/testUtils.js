/**
 * Testing utilities for simulating network delays and failures
 * Only enabled in development mode for testing resilience
 */

/**
 * Simulate network delay
 * @param {number} ms - milliseconds to delay
 */
export function simulateNetworkDelay(ms = 2000) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

/**
 * Simulate network error
 * @throws {Error} Network error
 */
export function simulateNetworkError() {
  throw new TypeError('Failed to fetch');
}

/**
 * Simulate server error
 * @throws {Response} Server error response
 */
export function simulateServerError(status = 500, message = 'Internal Server Error') {
  const response = new Response(JSON.stringify({ message }), { status });
  throw response;
}

/**
 * Wrap an API call with configurable failure simulation
 * Useful for testing retry and error handling
 */
export function withFailureSimulation(config = {}) {
  const {
    failureRate = 0, // 0-1, probability of failure
    failureType = 'network', // 'network', 'server', 'timeout'
    failAfterRetries = 0, // Fail only after N retries
    delay = 0, // Add delay to all requests
  } = config;

  let attemptCount = 0;

  return async (fetchFn) => {
    attemptCount++;

    // Add delay
    if (delay > 0) {
      await simulateNetworkDelay(delay);
    }

    // Decide whether to fail
    if (
      Math.random() < failureRate &&
      attemptCount > failAfterRetries
    ) {
      switch (failureType) {
        case 'network':
          throw new TypeError('Failed to fetch (simulated)');
        case 'server':
          const response = new Response(
            JSON.stringify({ message: 'Server error (simulated)' }),
            { status: 500 },
          );
          throw response;
        case 'timeout':
          const error = new Error('Request timeout (simulated)');
          error.name = 'AbortError';
          throw error;
        default:
          throw new Error('Unknown failure type');
      }
    }

    return fetchFn();
  };
}

/**
 * Create a mock fetch function for testing
 * @example
 * const mockFetch = createMockFetch({
 *   '/api/test': { success: true, data: { id: 1 } }
 * });
 */
export function createMockFetch(responses = {}) {
  return async (url, options) => {
    const pathname = typeof url === 'string' ? new URL(url, 'http://localhost').pathname : url;
    const response = responses[pathname];

    if (!response) {
      return new Response(JSON.stringify({ message: 'Not found' }), { status: 404 });
    }

    if (response instanceof Error) {
      throw response;
    }

    if (response instanceof Response) {
      return response;
    }

    return new Response(JSON.stringify(response), {
      status: response.status || 200,
      headers: { 'Content-Type': 'application/json' },
    });
  };
}

/**
 * Enable network throttling simulation
 * Shows how long typical operations take with network delays
 */
export const networkConditions = {
  // Connection type : { delay in ms, description }
  FAST_3G: { delay: 400, description: 'Fast 3G' },
  SLOW_3G: { delay: 2000, description: 'Slow 3G' },
  DIAL_UP: { delay: 4000, description: 'Dial-up (56k)' },
  OFFLINE: { delay: null, description: 'Offline', throwError: true },
};

/**
 * Helper to test UI behavior under various network conditions
 */
export async function testNetworkCondition(condition, testFn) {
  const { delay, throwError, description } = networkConditions[condition];

  console.log(`Testing with ${description}...`);

  if (throwError) {
    try {
      await testFn();
    } catch (error) {
      console.log(`✓ ${description} error handling working`);
      return;
    }
    throw new Error(`Expected error for ${description}`);
  }

  if (delay) {
    await simulateNetworkDelay(delay);
  }

  await testFn();
  console.log(`✓ ${description} test passed`);
}

/**
 * Development helper to inject failures into apiClient
 * Usage (in your test file):
 * enableFailureInjection(apiClient, { failureRate: 0.3 });
 */
export function enableFailureInjection(apiClient, config) {
  if (!import.meta.env.DEV) {
    console.warn('Failure injection only available in dev mode');
    return;
  }

  console.log('⚠️  Failure injection ENABLED - some requests will fail!', config);

  const originalGet = apiClient.get;
  const originalPost = apiClient.post;

  const failureSimulator = withFailureSimulation(config);

  apiClient.get = async (...args) => {
    try {
      await failureSimulator(() => Promise.resolve());
      return originalGet.apply(apiClient, args);
    } catch (error) {
      if (error.name === 'AbortError') {
        return {
          success: false,
          error: 'timeout',
          message: 'Simulated timeout',
          status: 408,
        };
      }
      throw error;
    }
  };

  apiClient.post = async (...args) => {
    try {
      await failureSimulator(() => Promise.resolve());
      return originalPost.apply(apiClient, args);
    } catch (error) {
      if (error instanceof Response) {
        const data = await error.json().catch(() => ({}));
        return {
          success: false,
          error: 'http',
          message: data.message || `HTTP ${error.status}`,
          status: error.status,
          data,
        };
      }
      throw error;
    }
  };

  // Expose disable function
  window.__disableFailureInjection = () => {
    apiClient.get = originalGet;
    apiClient.post = originalPost;
    console.log('✓ Failure injection disabled');
  };
}

export default {
  simulateNetworkDelay,
  simulateNetworkError,
  simulateServerError,
  withFailureSimulation,
  createMockFetch,
  networkConditions,
  testNetworkCondition,
  enableFailureInjection,
};
