// Global API Communication Module
const Api = {
  // If opened via external dev servers (Live Server 5500, Vite, React, etc.) or file://, target the ASP.NET Core API server at port 5129
  baseUrl: (function() {
    if (window.location.protocol === 'file:') return 'http://localhost:5129/api';
    const port = window.location.port;
    if (port === '5129' || port === '7269') return '/api';
    if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
      return 'http://localhost:5129/api';
    }
    return '/api';
  })(),

  async request(endpoint, options = {}) {
    const token = Auth.getToken();
    const headers = {
      'Content-Type': 'application/json',
      ...(options.headers || {})
    };

    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    const path = endpoint.startsWith('/') ? endpoint : `/${endpoint}`;

    try {
      const response = await fetch(`${this.baseUrl}${path}`, {
        ...options,
        headers
      });

      // If no content returned (e.g. 204 or empty response)
      if (response.status === 204 || response.headers.get('content-length') === '0') {
        return null;
      }

      const contentType = response.headers.get('content-type');
      let data = null;
      try {
        if (contentType && contentType.includes('application/json')) {
          data = await response.json();
        } else {
          data = await response.text();
        }
      } catch {
        data = null;
      }

      if (contentType && contentType.includes('text/html')) {
        throw new Error(`API returned HTML instead of JSON. The requested endpoint '${endpoint}' may be missing or invalid.`);
      }

      if (response.status === 401) {
        if (endpoint === '/auth/login') {
          const loginMsg = (data && data.message) ? data.message : 'Invalid username or password, or account is disabled.';
          throw new Error(loginMsg);
        }

        const wasLoggedIn = Boolean(Auth.getToken());
        Auth.logout();
        if (wasLoggedIn) {
          App.showToast('Session expired. Please log in again.', 'warning');
        }
        const errMsg = (data && data.message) ? data.message : 'Unauthorized';
        throw new Error(errMsg);
      }

      if (response.status === 403) {
        const forbidMsg = (data && data.message) ? data.message : 'Access denied: You do not have permission for this resource.';
        App.showToast(forbidMsg, 'danger');
        throw new Error(forbidMsg);
      }

      if (!response.ok) {
        let errorMsg = 'An error occurred.';
        if (data) {
          if (typeof data === 'string') {
            errorMsg = data;
          } else if (data.message) {
            errorMsg = data.message;
          } else if (data.errors && typeof data.errors === 'object') {
            errorMsg = Object.values(data.errors).flat().join(' ');
          } else if (data.title) {
            errorMsg = data.title;
          }
        }
        throw new Error(errorMsg);
      }

      return data;
    } catch (error) {
      if (error.message !== 'Unauthorized' && endpoint !== '/auth/login') {
        console.error(`API Error on [${options.method || 'GET'}] ${endpoint}:`, error);
      }
      throw error;
    }
  },

  get(endpoint) {
    return this.request(endpoint, { method: 'GET' });
  },

  post(endpoint, body) {
    return this.request(endpoint, {
      method: 'POST',
      body: JSON.stringify(body)
    });
  },

  put(endpoint, body) {
    return this.request(endpoint, {
      method: 'PUT',
      body: JSON.stringify(body)
    });
  },

  delete(endpoint) {
    return this.request(endpoint, { method: 'DELETE' });
  }
};
