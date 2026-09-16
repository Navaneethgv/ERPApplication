// Global Application Controller & Helpers
const App = {
  confirmCallback: null,

  init() {
    Router.init();

    // Bind confirmation modal confirm button
    const confirmBtn = document.getElementById('confirmModalBtn');
    if (confirmBtn) {
      confirmBtn.addEventListener('click', () => {
        if (typeof this.confirmCallback === 'function') {
          this.confirmCallback();
        }
        const modalEl = document.getElementById('confirmModal');
        const modal = bootstrap.Modal.getInstance(modalEl);
        if (modal) modal.hide();
      });
    }

    if (Auth.isAuthenticated()) {
      this.initAuthenticatedUI();
      if (!window.location.hash) {
        window.location.hash = '#dashboard';
      }
      Router.navigate();
    } else {
      this.initUnauthenticatedUI();
    }

    // Responsive window resize & escape key handler for mobile drawer
    window.addEventListener('resize', () => {
      if (window.innerWidth >= 992) {
        this.closeMobileSidebar();
      }
    });

    document.addEventListener('keydown', (e) => {
      if (e.key === 'Escape') {
        this.closeMobileSidebar();
      }
    });
  },

  initAuthenticatedUI() {
    document.body.classList.remove('auth-active', 'unauthenticated');
    document.body.classList.add('authenticated');

    const authView = document.getElementById('auth-view');
    const appLayout = document.getElementById('app-layout');

    if (authView) {
      authView.classList.add('d-none');
      authView.style.setProperty('display', 'none', 'important');
    }
    if (appLayout) {
      appLayout.classList.remove('d-none');
      appLayout.style.setProperty('display', 'flex', 'important');
    }

    const user = Auth.getUser();
    if (user) {
      const initials = (user.fullName || user.username || 'U')
        .split(' ')
        .map(n => n[0])
        .join('')
        .toUpperCase()
        .slice(0, 2);

      const avatarSidebar = document.getElementById('sidebar-user-avatar');
      const avatarHeader = document.getElementById('header-user-avatar');
      const nameSidebar = document.getElementById('sidebar-user-name');
      const roleSidebar = document.getElementById('sidebar-user-role');
      const nameHeader = document.getElementById('header-user-name');
      const emailDropdown = document.getElementById('dropdown-user-email');

      if (avatarSidebar) avatarSidebar.textContent = initials;
      if (avatarHeader) avatarHeader.textContent = initials;
      if (nameSidebar) nameSidebar.textContent = user.fullName || user.username;
      if (roleSidebar) roleSidebar.textContent = (user.role || 'USER').toUpperCase();
      if (nameHeader) nameHeader.textContent = user.fullName || user.username;
      if (emailDropdown) emailDropdown.textContent = user.username;

      Auth.renderSidebarMenu();
      if (typeof Auth.loadPermissions === 'function') {
        Auth.loadPermissions().then(() => {
          Auth.renderSidebarMenu();
        });
      }

      if (typeof NotificationsComponent !== 'undefined') {
        NotificationsComponent.init();
      }
    }
  },

  initUnauthenticatedUI() {
    if (typeof NotificationsComponent !== 'undefined') {
      NotificationsComponent.stop();
    }
    this.closeMobileSidebar();

    document.body.classList.remove('authenticated');
    document.body.classList.add('auth-active', 'unauthenticated');

    const authView = document.getElementById('auth-view');
    const appLayout = document.getElementById('app-layout');
    const appContent = document.getElementById('app-content');

    if (authView) {
      authView.classList.remove('d-none');
      authView.style.setProperty('display', 'flex', 'important');
    }
    if (appLayout) {
      appLayout.classList.add('d-none');
      appLayout.style.setProperty('display', 'none', 'important');
    }
    if (appContent) appContent.innerHTML = '';

    // Clear hash from URL so unauthenticated users cannot retain protected route in address bar
    if (window.location.hash) {
      history.replaceState(null, document.title, window.location.pathname + window.location.search);
    }
  },

  toggleSidebar() {
    if (window.innerWidth < 992) {
      this.toggleMobileSidebar();
    } else {
      document.body.classList.toggle('sidebar-collapsed');
    }
  },

  toggleMobileSidebar() {
    const sidebar = document.getElementById('sidebar-wrapper');
    if (!sidebar) return;

    const isOpen = sidebar.classList.toggle('show');
    let backdrop = document.querySelector('.sidebar-backdrop');

    if (isOpen) {
      if (!backdrop) {
        backdrop = document.createElement('div');
        backdrop.className = 'sidebar-backdrop';
        backdrop.addEventListener('click', () => this.closeMobileSidebar());
        document.body.appendChild(backdrop);
      }
    } else {
      if (backdrop) {
        backdrop.remove();
      }
    }
  },

  closeMobileSidebar() {
    const sidebar = document.getElementById('sidebar-wrapper');
    if (sidebar) {
      sidebar.classList.remove('show');
    }
    const backdrop = document.querySelector('.sidebar-backdrop');
    if (backdrop) {
      backdrop.remove();
    }
  },

  showToast(message, type = 'info') {
    const container = document.getElementById('toastContainer');
    if (!container) return;

    const iconMap = {
      success: 'bi-check-circle-fill text-success',
      danger: 'bi-x-circle-fill text-danger',
      warning: 'bi-exclamation-triangle-fill text-warning',
      info: 'bi-info-circle-fill text-info'
    };

    const id = `toast-${Date.now()}`;
    const toastEl = document.createElement('div');
    toastEl.className = 'toast align-items-center border-0 shadow-lg mb-2';
    toastEl.id = id;
    toastEl.setAttribute('role', 'alert');
    toastEl.setAttribute('aria-live', 'assertive');
    toastEl.setAttribute('aria-atomic', 'true');

    toastEl.innerHTML = `
      <div class="d-flex p-2">
        <div class="toast-body d-flex align-items-center">
          <i class="bi ${iconMap[type] || iconMap.info} fs-5 me-2"></i>
          <span>${message}</span>
        </div>
        <button type="button" class="btn-close me-2 m-auto" data-bs-dismiss="toast"></button>
      </div>
    `;

    container.appendChild(toastEl);
    const toast = new bootstrap.Toast(toastEl, { delay: 4000 });
    toast.show();

    toastEl.addEventListener('hidden.bs.toast', () => {
      toastEl.remove();
    });
  },

  confirmAction(title, message, callback, confirmButtonText = 'Confirm') {
    document.getElementById('confirmModalTitle').textContent = title;
    document.getElementById('confirmModalBody').textContent = message;
    const confirmBtn = document.getElementById('confirmModalBtn');
    confirmBtn.textContent = confirmButtonText;
    this.confirmCallback = callback;

    const modalEl = document.getElementById('confirmModal');
    const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
    modal.show();
  },

  togglePasswordVisibility(inputId, btnEl) {
    const input = document.getElementById(inputId);
    if (!input) return;
    const isPassword = input.type === 'password';
    input.type = isPassword ? 'text' : 'password';

    const icon = btnEl?.querySelector('i') || (btnEl?.tagName === 'I' ? btnEl : null);
    if (icon) {
      if (isPassword) {
        icon.classList.remove('bi-eye');
        icon.classList.add('bi-eye-slash');
        btnEl?.setAttribute('aria-label', 'Hide password');
        btnEl?.setAttribute('title', 'Hide password');
      } else {
        icon.classList.remove('bi-eye-slash');
        icon.classList.add('bi-eye');
        btnEl?.setAttribute('aria-label', 'Show password');
        btnEl?.setAttribute('title', 'Show password');
      }
    }
  },

  formatCurrency(amount) {
    return new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 2 }).format(amount || 0);
  },

  parseDate(dateInput) {
    if (!dateInput) return null;
    if (dateInput instanceof Date) return isNaN(dateInput.getTime()) ? null : dateInput;
    if (typeof dateInput !== 'string') return null;

    const str = dateInput.trim();
    if (!str) return null;

    // Handle date-only "YYYY-MM-DD"
    if (/^\d{4}-\d{2}-\d{2}$/.test(str)) {
      const [year, month, day] = str.split('-').map(Number);
      return new Date(year, month - 1, day, 12, 0, 0); // Noon avoids midnight timezone boundary issues
    }

    // Replace space between date and time with 'T' if missing
    let normalized = str;
    if (/^\d{4}-\d{2}-\d{2} \d{2}:\d{2}/.test(str)) {
      normalized = str.replace(' ', 'T');
    }

    const d = new Date(normalized);
    return isNaN(d.getTime()) ? null : d;
  },

  formatDate(dateStr) {
    if (!dateStr) return '-';
    try {
      const d = this.parseDate(dateStr);
      if (!d) return dateStr;
      return d.toLocaleDateString('en-IN', {
        year: 'numeric',
        month: 'short',
        day: '2-digit'
      });
    } catch {
      return dateStr;
    }
  },

  formatTime(dateStr) {
    if (!dateStr) return '-';
    try {
      const d = this.parseDate(dateStr);
      if (!d) return '-';
      return d.toLocaleTimeString('en-IN', {
        hour: '2-digit',
        minute: '2-digit',
        hour12: true
      });
    } catch {
      return '-';
    }
  },

  formatDateTime(dateStr) {
    if (!dateStr) return '-';
    try {
      const d = this.parseDate(dateStr);
      if (!d) return dateStr;
      const datePart = d.toLocaleDateString('en-IN', { year: 'numeric', month: 'short', day: '2-digit' });
      const timePart = d.toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit', hour12: true });
      return `${datePart}, ${timePart}`;
    } catch {
      return dateStr;
    }
  },

  renderPagination({
    currentPage = 1,
    pageSize = 10,
    totalItems = 0,
    componentName = ''
  }) {
    const totalPages = Math.max(1, Math.ceil(totalItems / pageSize));
    const startItem = totalItems === 0 ? 0 : (currentPage - 1) * pageSize + 1;
    const endItem = Math.min(currentPage * pageSize, totalItems);

    const delta = 1;
    const range = [];
    for (let i = Math.max(2, currentPage - delta); i <= Math.min(totalPages - 1, currentPage + delta); i++) {
      range.push(i);
    }
    if (currentPage - delta > 2) {
      range.unshift('...');
    }
    if (currentPage + delta < totalPages - 1) {
      range.push('...');
    }
    range.unshift(1);
    if (totalPages > 1) {
      range.push(totalPages);
    }

    const uniqueRange = [];
    range.forEach(p => {
      if (p === '...' || !uniqueRange.includes(p)) uniqueRange.push(p);
    });

    const pagesHtml = uniqueRange.map(p => {
      if (p === '...') {
        return `<li class="page-item disabled"><span class="page-link">…</span></li>`;
      }
      return `
        <li class="page-item ${p === currentPage ? 'active' : ''}">
          <button class="page-link" type="button" onclick="${componentName}.goToPage(${p})">${p}</button>
        </li>
      `;
    }).join('');

    return `
      <div class="erp-pagination-wrapper d-flex flex-column flex-md-row justify-content-between align-items-center gap-2 px-3 py-2 border-top bg-white">
        <div class="erp-pagination-info text-muted small text-center text-md-start">
          Showing <span class="fw-semibold text-dark mx-1">${startItem}</span> to <span class="fw-semibold text-dark mx-1">${endItem}</span> of <span class="fw-semibold text-dark mx-1">${totalItems}</span> entries
        </div>
        <div class="erp-pagination-controls d-flex flex-wrap align-items-center justify-content-center gap-2">
          <div class="erp-page-size-wrapper d-inline-flex align-items-center gap-1">
            <span class="erp-page-size-label small text-muted">Show:</span>
            <select class="form-select form-select-sm erp-page-size-select" aria-label="Entries per page" onchange="${componentName}.changePageSize(Number(this.value))">
              <option value="5" ${pageSize === 5 ? 'selected' : ''}>5</option>
              <option value="10" ${pageSize === 10 ? 'selected' : ''}>10</option>
              <option value="25" ${pageSize === 25 ? 'selected' : ''}>25</option>
              <option value="50" ${pageSize === 50 ? 'selected' : ''}>50</option>
            </select>
          </div>
          <ul class="pagination pagination-sm mb-0">
            <li class="page-item ${currentPage <= 1 ? 'disabled' : ''}">
              <button class="page-link" type="button" aria-label="Previous" onclick="${componentName}.goToPage(${currentPage - 1})">
                <i class="bi bi-chevron-left"></i>
              </button>
            </li>
            ${pagesHtml}
            <li class="page-item ${currentPage >= totalPages ? 'disabled' : ''}">
              <button class="page-link" type="button" aria-label="Next" onclick="${componentName}.goToPage(${currentPage + 1})">
                <i class="bi bi-chevron-right"></i>
              </button>
            </li>
          </ul>
        </div>
      </div>
    `;
  }
};

// Bootstrap the application when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
  App.init();
});
