// Hash-Based Single Page Application Router
const Router = {
  activeCharts: [],

  init() {
    window.addEventListener('hashchange', () => this.navigate());
    window.addEventListener('popstate', () => this.navigate());
  },

  destroyCharts() {
    while (this.activeCharts.length > 0) {
      const chart = this.activeCharts.pop();
      if (chart && typeof chart.destroy === 'function') {
        chart.destroy();
      }
    }
  },

  registerChart(chartInstance) {
    if (chartInstance) {
      this.activeCharts.push(chartInstance);
    }
  },

  async navigate() {
    if (!Auth.isAuthenticated()) {
      App.initUnauthenticatedUI();
      return;
    }

    if (typeof App.closeMobileSidebar === 'function') {
      App.closeMobileSidebar();
    }

    this.destroyCharts();

    const hash = window.location.hash.slice(1) || 'dashboard';
    const parts = hash.split('/');
    const mainRoute = parts[0] || 'dashboard';
    const subAction = parts[1] || '';
    const paramId = parts[2] || '';

    // Update active sidebar link
    document.querySelectorAll('.sidebar-link').forEach(link => {
      if (link.dataset.route === mainRoute) {
        link.classList.add('active');
      } else {
        link.classList.remove('active');
      }
    });

    // Update Breadcrumbs
    this.updateBreadcrumb(mainRoute, subAction, paramId);

    const container = document.getElementById('app-content');
    if (!container) return;

    // Security & Administration Permission Guard
    if (mainRoute === 'security') {
      if (!Auth.isAdmin()) {
        App.showToast('Security administration is strictly restricted to Administrators.', 'warning');
        window.location.hash = '#dashboard';
        return;
      }
    }

    // Dynamic Permission Check for all other routes
    if (!Auth.isAdmin() && !['dashboard', 'profile'].includes(mainRoute)) {
      if (!Auth.hasPermission(mainRoute, 'VIEW')) {
        App.showToast(`Access restricted: You do not have permission to view the ${mainRoute} module.`, 'warning');
        window.location.hash = '#dashboard';
        return;
      }
    }

    try {
      switch (mainRoute) {
        case 'dashboard':
          await DashboardComponent.render(container);
          break;
        case 'security':
          await SecurityComponent.render(container, subAction, paramId);
          break;
        case 'employees':
          await EmployeesComponent.render(container, subAction, paramId);
          break;
        case 'customers':
          await CustomersComponent.render(container, subAction, paramId);
          break;
        case 'products':
          await ProductsComponent.render(container, subAction, paramId);
          break;
        case 'inventory':
          await InventoryComponent.render(container, subAction, paramId);
          break;
        case 'suppliers':
          await SuppliersComponent.render(container, subAction, paramId);
          break;
        case 'purchases':
          await PurchasesComponent.render(container, subAction, paramId);
          break;
        case 'sales':
          await SalesComponent.render(container, subAction, paramId);
          break;
        case 'invoices':
          await InvoicesComponent.render(container, subAction, paramId);
          break;
        case 'transactions':
          await TransactionsComponent.render(container, subAction, paramId);
          break;
        case 'reports':
          await ReportsComponent.render(container);
          break;
        case 'profile':
          await ProfileComponent.render(container);
          break;
        default:
          container.innerHTML = `
            <div class="empty-state">
              <i class="bi bi-compass"></i>
              <h4>404 - Page Not Found</h4>
              <p>The requested route <code>#${hash}</code> could not be found.</p>
              <a href="#dashboard" class="btn btn-erp-primary btn-sm">Return to Dashboard</a>
            </div>
          `;
          break;
      }
    } catch (error) {
      console.error('Route render error:', error);
      container.innerHTML = `
        <div class="alert alert-danger shadow-sm">
          <h5 class="alert-heading"><i class="bi bi-exclamation-triangle-fill me-2"></i>Failed to Load Module</h5>
          <p class="mb-0">${error.message || 'An unexpected error occurred while loading this page.'}</p>
        </div>
      `;
    }
  },

  updateBreadcrumb(main, sub, param) {
    const el = document.getElementById('breadcrumb-nav');
    if (!el) return;

    const capitalize = (str) => str ? str.charAt(0).toUpperCase() + str.slice(1) : '';
    const mainTitle = main === 'security' ? 'Security & Permissions' : capitalize(main);
    let html = `<span>ApexERP</span> / <a href="#${main}">${mainTitle}</a>`;

    if (sub) {
      if (sub === 'new') {
        html += ` / <span class="fw-semibold text-dark">Add New</span>`;
      } else if (sub === 'edit') {
        html += ` / <span class="fw-semibold text-dark">Edit #${param}</span>`;
      } else if (sub === 'view') {
        html += ` / <span class="fw-semibold text-dark">Details #${param}</span>`;
      } else if (sub === 'adjust') {
        html += ` / <span class="fw-semibold text-dark">Stock Adjustment</span>`;
      } else {
        html += ` / <span class="fw-semibold text-dark">${capitalize(sub)}</span>`;
      }
    }
    el.innerHTML = html;
  }
};
