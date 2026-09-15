// Authentication & Session Management Module
const Auth = {
  TOKEN_KEY: 'erp_jwt_token',
  USER_KEY: 'erp_user_data',
  PERMS_KEY: 'erp_user_permissions',
  userPermissions: null,
  cachedPolicy: null,

  getToken() {
    const raw = localStorage.getItem(this.TOKEN_KEY);
    if (!raw || raw === 'undefined' || raw === 'null') return null;
    let token = raw.trim();
    if (token.startsWith('"') && token.endsWith('"')) {
      token = token.slice(1, -1).trim();
    }
    return token || null;
  },

  getUser() {
    const raw = localStorage.getItem(this.USER_KEY);
    if (raw && raw !== 'undefined' && raw !== 'null') {
      try {
        const parsed = JSON.parse(raw);
        if (parsed && typeof parsed === 'object') return parsed;
      } catch {}
    }

    // Fallback: reconstruct user from JWT token payload if available
    const token = this.getToken();
    if (token) {
      const payload = this.parseJwtPayload(token);
      if (payload) {
        const role = payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload.Role || 'Admin';
        const user = {
          userId: parseInt(payload.nameid || payload.sub || '0', 10),
          username: payload.unique_name || payload.name || payload.email || '',
          role: role,
          fullName: payload.FullName || payload.name || payload.unique_name || 'Admin User',
          status: payload.Status || 'Active',
          employeeId: payload.EmployeeId ? parseInt(payload.EmployeeId, 10) : null,
          customerId: payload.CustomerId ? parseInt(payload.CustomerId, 10) : null
        };
        try {
          localStorage.setItem(this.USER_KEY, JSON.stringify(user));
        } catch {}
        return user;
      }
    }
    return null;
  },

  parseJwtPayload(token) {
    try {
      if (!token || typeof token !== 'string') return null;
      const parts = token.split('.');
      if (parts.length !== 3) return null;

      // Base64URL to Base64 conversion + padding
      let base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
      const pad = base64.length % 4;
      if (pad) {
        base64 += '='.repeat(4 - pad);
      }

      const json = atob(base64);
      return JSON.parse(json);
    } catch {
      return null;
    }
  },

  getRole() {
    const user = this.getUser();
    if (user && user.role) return user.role;
    const token = this.getToken();
    if (token) {
      const payload = this.parseJwtPayload(token);
      if (payload) {
        return payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload.Role || null;
      }
    }
    return null;
  },

  isAuthenticated() {
    const token = this.getToken();
    if (!token) return false;

    // Validate JWT structure and expiry
    const payload = this.parseJwtPayload(token);
    if (!payload) {
      this.clearSession();
      return false;
    }

    // Leeway of 30 seconds for clock differences
    if (payload.exp && (payload.exp * 1000) <= (Date.now() - 30000)) {
      this.clearSession();
      return false;
    }

    return true;
  },

  isAdmin() {
    const role = this.getRole();
    return typeof role === 'string' && role.trim().toLowerCase() === 'admin';
  },

  isEmployee() {
    const role = this.getRole();
    return typeof role === 'string' && role.trim().toLowerCase() === 'employee';
  },

  isCustomer() {
    const role = this.getRole();
    return typeof role === 'string' && role.trim().toLowerCase() === 'customer';
  },

  setSession(token, user) {
    if (!token) return;
    const cleanToken = typeof token === 'string' ? token.trim() : String(token);
    localStorage.setItem(this.TOKEN_KEY, cleanToken);

    if (user && typeof user === 'object') {
      localStorage.setItem(this.USER_KEY, JSON.stringify(user));
    } else {
      const payload = this.parseJwtPayload(cleanToken);
      if (payload) {
        const role = payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload.Role || 'Admin';
        const reconstructed = {
          userId: parseInt(payload.nameid || payload.sub || '0', 10),
          username: payload.unique_name || payload.name || '',
          role: role,
          fullName: payload.FullName || payload.name || payload.unique_name || 'Admin User',
          status: payload.Status || 'Active',
          employeeId: payload.EmployeeId ? parseInt(payload.EmployeeId, 10) : null,
          customerId: payload.CustomerId ? parseInt(payload.CustomerId, 10) : null
        };
        localStorage.setItem(this.USER_KEY, JSON.stringify(reconstructed));
      }
    }
  },

  clearSession() {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    localStorage.removeItem(this.PERMS_KEY);
    this.userPermissions = null;
  },

  getPermissions() {
    if (this.userPermissions) return this.userPermissions;
    const raw = localStorage.getItem(this.PERMS_KEY);
    if (raw) {
      try {
        this.userPermissions = JSON.parse(raw);
        return this.userPermissions;
      } catch {}
    }
    return null;
  },

  setPermissions(data) {
    this.userPermissions = data;
    try {
      localStorage.setItem(this.PERMS_KEY, JSON.stringify(data));
    } catch {}
  },

  async loadPermissions() {
    try {
      const perms = await Api.get('/RolePermissions/my-permissions');
      if (perms) {
        this.setPermissions(perms);
        return perms;
      }
    } catch (err) {
      console.warn('Failed to load dynamic permissions, using fallback:', err);
    }
    return this.getPermissions();
  },

  hasPermission(route, optionCode) {
    if (this.isAdmin()) return true;

    const perms = this.getPermissions();
    if (perms && perms.routeOptions) {
      const normalizedRoute = (route || '').trim().toLowerCase();
      const normalizedCode = (optionCode || '').trim().toUpperCase();

      const options = perms.routeOptions[normalizedRoute];
      if (Array.isArray(options)) {
        return options.some(c => c.toUpperCase() === normalizedCode);
      }
      return false;
    }

    // Role-based baseline fallback if dynamic permissions not loaded
    const role = this.getRole();
    if (role === 'Employee') {
      if (['employees', 'security'].includes(route)) return false;
      if (optionCode.toUpperCase() === 'DELETE' && ['customers', 'products', 'inventory', 'suppliers'].includes(route)) return false;
      return true;
    } else if (role === 'Customer') {
      if (['products', 'sales', 'invoices', 'transactions', 'profile'].includes(route)) {
        if (optionCode.toUpperCase() === 'VIEW') return true;
        if (route === 'sales' && optionCode.toUpperCase() === 'ADD') return true;
        if (route === 'profile') return true;
      }
      return false;
    }

    return true;
  },

  fillDemo(username, password) {
    document.getElementById('login-username').value = username;
    document.getElementById('login-password').value = password;
  },

  async handleLogin(event) {
    event.preventDefault();
    const alertBox = document.getElementById('auth-alert');
    const submitBtn = document.getElementById('btn-login');
    const spinner = submitBtn.querySelector('.spinner-border');

    alertBox.classList.add('d-none');
    submitBtn.disabled = true;
    spinner.classList.remove('d-none');

    const username = document.getElementById('login-username').value.trim();
    const password = document.getElementById('login-password').value;

    try {
      const result = await Api.post('/auth/login', { username, password });
      const token = result?.token || result?.Token;
      const user = result?.user || result?.User;

      if (!token) {
        throw new Error('Authentication succeeded but no JWT token was received.');
      }

      this.setSession(token, user);

      // Load dynamic role permissions
      await this.loadPermissions();

      App.showToast(`Welcome back, ${user?.fullName || username}!`, 'success');

      if (result.passwordExpired) {
        setTimeout(() => {
          App.showToast('Your password has expired under system security policy. Please update your password in My Profile.', 'warning');
        }, 1500);
      }

      App.initAuthenticatedUI();
      window.location.hash = '#dashboard';
      Router.navigate();
    } catch (error) {
      alertBox.textContent = error.message || 'Login failed. Please check your credentials.';
      alertBox.classList.remove('d-none');
    } finally {
      submitBtn.disabled = false;
      spinner.classList.add('d-none');
    }
  },

  async getPolicy() {
    if (this.cachedPolicy) return this.cachedPolicy;
    try {
      this.cachedPolicy = await Api.get('/PasswordPolicy');
      return this.cachedPolicy;
    } catch {
      return {
        minLength: 8,
        requireUppercase: true,
        requireLowercase: true,
        requireDigit: true,
        requireSpecialChar: true,
        expiryDays: 90
      };
    }
  },

  async toggleRegisterModal() {
    const modalEl = document.getElementById('registerModal');
    const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
    document.getElementById('register-alert').classList.add('d-none');
    document.getElementById('register-customer-form').reset();

    // Render password requirements checklist in registration modal
    await this.renderPasswordChecklist('reg-password', 'reg-password-checklist');

    modal.show();
  },

  async renderPasswordChecklist(inputId, containerId) {
    const container = document.getElementById(containerId);
    const input = document.getElementById(inputId);
    if (!container || !input) return;

    const policy = await this.getPolicy();

    const updateChecklist = () => {
      const pwd = input.value || '';
      const checks = [
        { label: `At least ${policy.minLength} characters`, valid: pwd.length >= policy.minLength },
        { label: 'One uppercase letter (A-Z)', valid: !policy.requireUppercase || /[A-Z]/.test(pwd) },
        { label: 'One lowercase letter (a-z)', valid: !policy.requireLowercase || /[a-z]/.test(pwd) },
        { label: 'One number (0-9)', valid: !policy.requireDigit || /[0-9]/.test(pwd) },
        { label: 'One special symbol (!@#$...)', valid: !policy.requireSpecialChar || /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?~`]/.test(pwd) }
      ];

      container.innerHTML = `
        <div class="p-2 bg-light rounded border small mt-2">
          <div class="fw-semibold text-muted mb-1">Password Requirements:</div>
          <div class="d-flex flex-wrap gap-2">
            ${checks.map(c => `
              <span class="badge ${c.valid ? 'bg-success-subtle text-success' : 'bg-secondary-subtle text-secondary'}">
                <i class="bi ${c.valid ? 'bi-check-circle-fill' : 'bi-circle'} me-1"></i>${c.label}
              </span>
            `).join('')}
          </div>
        </div>
      `;
    };

    input.removeEventListener('input', input._checklistHandler);
    input._checklistHandler = updateChecklist;
    input.addEventListener('input', updateChecklist);
    updateChecklist();
  },

  async handleRegister(event) {
    event.preventDefault();
    const alertBox = document.getElementById('register-alert');
    const submitBtn = document.getElementById('btn-register-submit');
    alertBox.classList.add('d-none');
    submitBtn.disabled = true;

    const payload = {
      name: document.getElementById('reg-name').value.trim(),
      email: document.getElementById('reg-email').value.trim(),
      phone: document.getElementById('reg-phone').value.trim(),
      company: document.getElementById('reg-company').value.trim(),
      address: document.getElementById('reg-address').value.trim(),
      password: document.getElementById('reg-password').value
    };

    try {
      await Api.post('/auth/register-customer', payload);
      const modalEl = document.getElementById('registerModal');
      const modal = bootstrap.Modal.getInstance(modalEl);
      if (modal) modal.hide();

      App.showToast('Account created successfully! Logging you in...', 'success');

      // Auto login
      const result = await Api.post('/auth/login', { username: payload.email, password: payload.password });
      this.setSession(result.token, result.user);
      await this.loadPermissions();
      App.initAuthenticatedUI();
      window.location.hash = '#dashboard';
      Router.navigate();
    } catch (error) {
      alertBox.textContent = error.message || 'Registration failed.';
      alertBox.classList.remove('d-none');
    } finally {
      submitBtn.disabled = false;
    }
  },

  logout() {
    this.clearSession();
    App.initUnauthenticatedUI();
    if (window.location.hash) {
      history.replaceState(null, document.title, window.location.pathname + window.location.search);
    }
  },

  renderSidebarMenu() {
    const navList = document.getElementById('sidebar-nav-list');
    if (!navList) return;

    const role = this.getUser()?.role || 'Customer';
    const perms = this.getPermissions();

    // If dynamic permissions are loaded and have menus defined
    if (perms && perms.menus && perms.menus.length > 0) {
      let html = '';
      const currentRoute = (window.location.hash.slice(1) || 'dashboard').split('/')[0];

      perms.menus.forEach(menu => {
        // Top-level item with no submenus and a route (e.g. Dashboard)
        if ((!menu.subMenus || menu.subMenus.length === 0) && menu.route) {
          const isActive = currentRoute === menu.route ? 'active' : '';
          html += `
            <li class="sidebar-item">
              <a href="#${menu.route}" class="sidebar-link ${isActive}" data-route="${menu.route}">
                <i class="bi ${menu.icon || 'bi-grid'}"></i>${menu.title}
              </a>
            </li>
          `;
        } else {
          // Parent heading or group
          html += `<li class="sidebar-heading">${menu.title}</li>`;
          if (menu.subMenus && menu.subMenus.length > 0) {
            menu.subMenus.forEach(sub => {
              if (sub.route) {
                const isActive = currentRoute === sub.route ? 'active' : '';
                html += `
                  <li class="sidebar-item">
                    <a href="#${sub.route}" class="sidebar-link ${isActive}" data-route="${sub.route}">
                      <i class="bi ${sub.icon || 'bi-circle'}"></i>${sub.title}
                    </a>
                  </li>
                `;
              }
            });
          }
        }
      });

      // If Admin and "security" menu wasn't already included, add it to ensure access
      if (this.isAdmin() && !perms.menus.some(m => m.route === 'security' || (m.subMenus && m.subMenus.some(s => s.route === 'security')))) {
        html += `
          <li class="sidebar-heading">Security</li>
          <li class="sidebar-item">
            <a href="#security" class="sidebar-link ${currentRoute === 'security' ? 'active' : ''}" data-route="security">
              <i class="bi bi-shield-lock-fill"></i>Security & Permissions
            </a>
          </li>
        `;
      }

      // Always ensure "My Profile" at the bottom
      if (!perms.menus.some(m => m.route === 'profile' || (m.subMenus && m.subMenus.some(s => s.route === 'profile')))) {
        html += `
          <li class="sidebar-heading">Account</li>
          <li class="sidebar-item">
            <a href="#profile" class="sidebar-link ${currentRoute === 'profile' ? 'active' : ''}" data-route="profile">
              <i class="bi bi-person-circle"></i>My Profile
            </a>
          </li>
        `;
      }

      navList.innerHTML = html;
      return;
    }

    // Baseline Fallback: If dynamic menus are not yet retrieved
    let html = `
      <li class="sidebar-heading">Overview</li>
      <li class="sidebar-item">
        <a href="#dashboard" class="sidebar-link" data-route="dashboard">
          <i class="bi bi-grid-1x2-fill"></i>Dashboard
        </a>
      </li>
    `;

    if (role === 'Admin') {
      html += `
        <li class="sidebar-heading">Administration</li>
        <li class="sidebar-item">
          <a href="#employees" class="sidebar-link" data-route="employees">
            <i class="bi bi-people-fill"></i>Employees
          </a>
        </li>
        <li class="sidebar-item">
          <a href="#security" class="sidebar-link" data-route="security">
            <i class="bi bi-shield-lock-fill"></i>Security & Permissions
          </a>
        </li>
      `;
    }

    if (role === 'Admin' || role === 'Employee') {
      html += `
        <li class="sidebar-heading">Business Operations</li>
        <li class="sidebar-item">
          <a href="#customers" class="sidebar-link" data-route="customers">
            <i class="bi bi-person-lines-fill"></i>Customers
          </a>
        </li>
        <li class="sidebar-item">
          <a href="#products" class="sidebar-link" data-route="products">
            <i class="bi bi-box-seam-fill"></i>Products
          </a>
        </li>
        <li class="sidebar-item">
          <a href="#inventory" class="sidebar-link" data-route="inventory">
            <i class="bi bi-archive-fill"></i>Inventory
          </a>
        </li>
        <li class="sidebar-item">
          <a href="#suppliers" class="sidebar-link" data-route="suppliers">
            <i class="bi bi-truck"></i>Suppliers
          </a>
        </li>
        <li class="sidebar-item">
          <a href="#purchases" class="sidebar-link" data-route="purchases">
            <i class="bi bi-bag-check-fill"></i>Purchases
          </a>
        </li>
        <li class="sidebar-item">
          <a href="#sales" class="sidebar-link" data-route="sales">
            <i class="bi bi-cart-check-fill"></i>Sales Orders
          </a>
        </li>
        <li class="sidebar-heading">Financials</li>
        <li class="sidebar-item">
          <a href="#invoices" class="sidebar-link" data-route="invoices">
            <i class="bi bi-receipt-cutoff"></i>Invoices
          </a>
        </li>
        <li class="sidebar-item">
          <a href="#transactions" class="sidebar-link" data-route="transactions">
            <i class="bi bi-journal-text"></i>Transactions
          </a>
        </li>
        <li class="sidebar-item">
          <a href="#reports" class="sidebar-link" data-route="reports">
            <i class="bi bi-graph-up-arrow"></i>Reports & Analytics
          </a>
        </li>
      `;
    } else if (role === 'Customer') {
      html += `
        <li class="sidebar-heading">Customer Portal</li>
        <li class="sidebar-item">
          <a href="#products" class="sidebar-link" data-route="products">
            <i class="bi bi-shop"></i>Product Catalog
          </a>
        </li>
        <li class="sidebar-item">
          <a href="#sales" class="sidebar-link" data-route="sales">
            <i class="bi bi-cart3"></i>My Orders
          </a>
        </li>
        <li class="sidebar-item">
          <a href="#invoices" class="sidebar-link" data-route="invoices">
            <i class="bi bi-receipt"></i>My Invoices
          </a>
        </li>
        <li class="sidebar-item">
          <a href="#transactions" class="sidebar-link" data-route="transactions">
            <i class="bi bi-wallet2"></i>Payment History
          </a>
        </li>
      `;
    }

    html += `
      <li class="sidebar-heading">Account</li>
      <li class="sidebar-item">
        <a href="#profile" class="sidebar-link" data-route="profile">
          <i class="bi bi-person-circle"></i>My Profile
        </a>
      </li>
    `;

    navList.innerHTML = html;
  }
};
