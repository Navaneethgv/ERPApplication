// Customers Component
const CustomersComponent = {
  data: [],
  filteredData: [],
  currentPage: 1,
  pageSize: 10,

  async render(container, action, id) {
    if (action === 'new') {
      if (!Auth.hasPermission('customers', 'ADD')) {
        App.showToast('Access restricted: You do not have permission to add customers.', 'warning');
        window.location.hash = '#customers';
        return;
      }
      this.renderForm(container, null);
    } else if (action === 'edit' && id) {
      if (!Auth.hasPermission('customers', 'EDIT')) {
        App.showToast('Access restricted: You do not have permission to edit customers.', 'warning');
        window.location.hash = '#customers';
        return;
      }
      container.innerHTML = `
        <div class="loading-spinner-container">
          <div class="spinner-border text-primary mb-2"></div>
          <p>Loading customer profile...</p>
        </div>
      `;
      try {
        const cust = await Api.get(`/customers/${id}`);
        this.renderForm(container, cust);
      } catch (error) {
        App.showToast(error.message, 'danger');
        window.location.hash = '#customers';
      }
    } else {
      this.renderList(container);
    }
  },

  async renderList(container) {
    container.innerHTML = `
      <div class="loading-spinner-container">
        <div class="spinner-border text-primary mb-2"></div>
        <p>Loading customer directory...</p>
      </div>
    `;

    try {
      this.data = await Api.get('/customers');
      this.filteredData = [...this.data];
      this.currentPage = 1;
      this.renderTable(container);
    } catch (error) {
      container.innerHTML = `
        <div class="alert alert-danger">
          <h5><i class="bi bi-exclamation-circle me-2"></i>Failed to Load Customers</h5>
          <p class="mb-0">${error.message}</p>
        </div>
      `;
    }
  },

  renderTable(container) {
    const canAdd = Auth.hasPermission('customers', 'ADD');

    const start = (this.currentPage - 1) * this.pageSize;
    const pageItems = this.filteredData.slice(start, start + this.pageSize);

    container.innerHTML = `
      <div class="page-header-container d-flex flex-column flex-sm-row justify-content-between align-items-start align-items-sm-center gap-2 mb-3 mb-md-4">
        <div>
          <h1 class="page-title">Customer Directory</h1>
          <p class="page-subtitle">Client profiles, credit terms, and accounts receivable balances</p>
        </div>
        <div>
          ${canAdd ? `
            <a href="#customers/new" class="btn btn-erp-primary btn-sm">
              <i class="bi bi-plus-circle me-1"></i>Add New Customer
            </a>
          ` : ''}
        </div>
      </div>

      <!-- Search & Filters -->
      <div class="erp-card mb-3">
        <div class="erp-card-body p-3">
          <div class="row g-2 align-items-center">
            <div class="col-12 col-md-5 col-lg-4">
              <div class="input-group input-group-sm">
                <span class="input-group-text bg-light"><i class="bi bi-search"></i></span>
                <input type="text" id="cust-search" class="form-control" placeholder="Search by customer name, company, email, phone..." oninput="CustomersComponent.filterList()">
              </div>
            </div>
            <div class="col-12 col-sm-6 col-md-3 col-lg-2">
              <select id="cust-status-filter" class="form-select form-select-sm" onchange="CustomersComponent.filterList()">
                <option value="">All Statuses</option>
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
              </select>
            </div>
            <div class="col-12 col-sm-auto ms-sm-auto text-muted small text-sm-end mt-2 mt-sm-0">
              Total: <strong id="cust-total-count">${this.filteredData.length}</strong> clients
            </div>
          </div>
        </div>
      </div>

      <!-- Customers Table -->
      <div class="erp-card">
        <div class="table-responsive erp-table-wrapper">
          <table class="erp-table align-middle" id="customers-table" style="min-width: 720px;">
            <thead>
              <tr>
                <th>ID</th>
                <th>Customer / Company</th>
                <th>Contact Details</th>
                <th>Address</th>
                <th>Credit Limit</th>
                <th>Current Balance</th>
                <th>Status</th>
                <th class="text-end">Actions</th>
              </tr>
            </thead>
            <tbody id="customers-table-body">
              ${this.buildTableRows(pageItems)}
            </tbody>
          </table>
        </div>
        <div id="customers-pagination">
          ${App.renderPagination({
            currentPage: this.currentPage,
            pageSize: this.pageSize,
            totalItems: this.filteredData.length,
            componentName: 'CustomersComponent'
          })}
        </div>
      </div>
    `;
  },

  buildTableRows(list) {
    if (!list || list.length === 0) {
      return `<tr><td colspan="8" class="text-center text-muted py-4"><i class="bi bi-building fs-2 d-block mb-2 text-muted"></i>No customers found.</td></tr>`;
    }

    const canEdit = Auth.hasPermission('customers', 'EDIT');
    const canDelete = Auth.hasPermission('customers', 'DELETE');

    return list.map(c => `
      <tr>
        <td class="text-muted">#${c.customerId}</td>
        <td>
          <div class="fw-semibold text-dark">${c.name}</div>
          <div class="small text-muted">${c.company || '-'}</div>
        </td>
        <td>
          <div class="small"><i class="bi bi-person me-1 text-muted"></i>${c.contactPerson || c.name}</div>
          <div class="small"><i class="bi bi-envelope me-1 text-muted"></i>${c.email}</div>
          ${c.phone ? `<div class="small text-muted"><i class="bi bi-telephone me-1"></i>${c.phone}</div>` : ''}
        </td>
        <td class="text-muted small" style="max-width: 200px;">${c.address || '-'}</td>
        <td class="fw-semibold">${App.formatCurrency(c.creditLimit)}</td>
        <td class="fw-bold ${c.currentBalance > 0 ? 'text-danger' : 'text-success'}">
          ${App.formatCurrency(c.currentBalance)}
        </td>
        <td><span class="badge-status badge-${c.status.toLowerCase()}">${c.status}</span></td>
        <td class="text-end text-nowrap">
          <div class="table-actions">
            ${canEdit ? `
              <a href="#customers/edit/${c.customerId}" class="btn btn-outline-primary btn-sm" title="Edit Profile">
                <i class="bi bi-pencil"></i>
              </a>
            ` : ''}
            ${canDelete ? `
              <button type="button" class="btn btn-outline-danger btn-sm" title="Delete Customer" onclick="CustomersComponent.confirmDelete(${c.customerId}, '${c.name.replace(/'/g, "\\'")}')">
                <i class="bi bi-trash"></i>
              </button>
            ` : ''}
          </div>
        </td>
      </tr>
    `).join('');
  },

  updateTableView() {
    const start = (this.currentPage - 1) * this.pageSize;
    const pageItems = this.filteredData.slice(start, start + this.pageSize);
    const tbody = document.getElementById('customers-table-body');
    if (tbody) {
      tbody.innerHTML = this.buildTableRows(pageItems);
    }
    const pagContainer = document.getElementById('customers-pagination');
    if (pagContainer) {
      pagContainer.innerHTML = App.renderPagination({
        currentPage: this.currentPage,
        pageSize: this.pageSize,
        totalItems: this.filteredData.length,
        componentName: 'CustomersComponent'
      });
    }
    const totalCount = document.getElementById('cust-total-count');
    if (totalCount) {
      totalCount.textContent = this.filteredData.length;
    }
  },

  goToPage(page) {
    const totalPages = Math.max(1, Math.ceil(this.filteredData.length / this.pageSize));
    if (page < 1 || page > totalPages) return;
    this.currentPage = page;
    this.updateTableView();
  },

  changePageSize(size) {
    this.pageSize = size;
    this.currentPage = 1;
    this.updateTableView();
  },

  filterList() {
    const q = (document.getElementById('cust-search')?.value || '').toLowerCase();
    const status = document.getElementById('cust-status-filter')?.value || '';

    this.filteredData = this.data.filter(c => {
      const matchQ = !q || c.name.toLowerCase().includes(q) || (c.company && c.company.toLowerCase().includes(q)) || c.email.toLowerCase().includes(q) || (c.phone && c.phone.includes(q));
      const matchStatus = !status || c.status === status;
      return matchQ && matchStatus;
    });

    this.currentPage = 1;
    this.updateTableView();
  },

  renderForm(container, cust) {
    const isEdit = !!cust;
    container.innerHTML = `
      <div class="page-header-container d-flex flex-column flex-sm-row justify-content-between align-items-start align-items-sm-center gap-2 mb-3 mb-md-4">
        <div>
          <h1 class="page-title">${isEdit ? 'Edit Customer' : 'Add New Customer'}</h1>
          <p class="page-subtitle">${isEdit ? `Update details for ${cust.name}` : 'Register a customer profile and credit terms'}</p>
        </div>
        <div>
          <a href="#customers" class="btn btn-erp-secondary btn-sm">
            <i class="bi bi-arrow-left me-1"></i>Back to Directory
          </a>
        </div>
      </div>

      <div class="erp-card">
        <div class="erp-card-body p-3 p-md-4">
          <form id="customer-form" onsubmit="CustomersComponent.submitForm(event, ${isEdit ? cust.customerId : 'null'})">
            <div class="form-section-title"><i class="bi bi-building me-2"></i>Account & Organization</div>

            <div class="row g-3 mb-3">
              <div class="col-12 col-md-6">
                <label class="form-label" for="cust-name">Customer / Organization Name <span class="required-asterisk">*</span></label>
                <input type="text" id="cust-name" class="form-control" value="${cust?.name || ''}" placeholder="e.g. Acme Global Industries" required>
              </div>
              <div class="col-12 col-md-6">
                <label class="form-label" for="cust-company">Trading / Business Name</label>
                <input type="text" id="cust-company" class="form-control" value="${cust?.company || ''}" placeholder="e.g. Acme Corp LLC">
              </div>
            </div>

            <div class="row g-3 mb-4">
              <div class="col-12 col-md-4">
                <label class="form-label" for="cust-contact">Primary Contact Person</label>
                <input type="text" id="cust-contact" class="form-control" value="${cust?.contactPerson || ''}" placeholder="e.g. Alice Walker">
              </div>
              <div class="col-12 col-sm-6 col-md-4">
                <label class="form-label" for="cust-email">Email Address <span class="required-asterisk">*</span></label>
                <input type="email" id="cust-email" class="form-control" value="${cust?.email || ''}" required>
              </div>
              <div class="col-12 col-sm-6 col-md-4">
                <label class="form-label" for="cust-phone">Phone Number <span class="required-asterisk">*</span></label>
                <input type="tel" id="cust-phone" class="form-control" value="${cust?.phone || ''}" required>
              </div>
            </div>

            <div class="form-section-title"><i class="bi bi-geo-alt me-2"></i>Billing & Credit Management</div>

            <div class="row g-3 mb-3">
              <div class="col-12">
                <label class="form-label" for="cust-address">Billing & Delivery Address</label>
                <textarea id="cust-address" class="form-control" rows="2" placeholder="Street Address, City, State, ZIP">${cust?.address || ''}</textarea>
              </div>
            </div>

            <div class="row g-3 mb-4">
              <div class="col-12 col-md-6">
                <label class="form-label" for="cust-credit">Credit Limit (₹) <span class="required-asterisk">*</span></label>
                <input type="number" step="100" min="0" id="cust-credit" class="form-control" value="${cust?.creditLimit || 10000}" required>
              </div>
              <div class="col-12 col-md-6">
                <label class="form-label" for="cust-status">Account Status <span class="required-asterisk">*</span></label>
                <select id="cust-status" class="form-select" required>
                  <option value="Active" ${cust?.status === 'Active' ? 'selected' : ''}>Active</option>
                  <option value="Inactive" ${cust?.status === 'Inactive' ? 'selected' : ''}>Inactive</option>
                </select>
              </div>
            </div>

            ${!isEdit ? `
              <div class="form-section-title"><i class="bi bi-shield-lock me-2"></i>Customer Portal Credentials</div>
              <div class="row g-3 mb-4">
                <div class="col-12">
                  <div class="form-check">
                    <input class="form-check-input" type="checkbox" id="cust-create-login" onchange="document.getElementById('cust-password-row').style.display = this.checked ? 'block' : 'none'">
                    <label class="form-check-label fw-semibold" for="cust-create-login">
                      Create Customer Portal Account
                    </label>
                  </div>
                </div>
                <div class="col-12 col-md-6" id="cust-password-row" style="display: none;">
                  <label class="form-label" for="cust-password">Portal Password <span class="required-asterisk">*</span></label>
                  <input type="password" id="cust-password" class="form-control" placeholder="Minimum 6 characters">
                </div>
              </div>
            ` : ''}

            <div class="d-flex flex-column-reverse flex-sm-row justify-content-end gap-2 border-top pt-3">
              <a href="#customers" class="btn btn-erp-secondary">Cancel</a>
              <button type="submit" class="btn btn-erp-primary" id="btn-save-cust">
                <i class="bi bi-check2 me-1"></i>${isEdit ? 'Save Changes' : 'Register Customer'}
              </button>
            </div>
          </form>
        </div>
      </div>
    `;
  },

  async submitForm(event, id) {
    event.preventDefault();
    const btn = document.getElementById('btn-save-cust');
    btn.disabled = true;

    const payload = {
      name: document.getElementById('cust-name').value.trim(),
      company: document.getElementById('cust-company').value.trim(),
      contactPerson: document.getElementById('cust-contact').value.trim(),
      email: document.getElementById('cust-email').value.trim(),
      phone: document.getElementById('cust-phone').value.trim(),
      address: document.getElementById('cust-address').value.trim(),
      creditLimit: parseFloat(document.getElementById('cust-credit').value) || 0,
      status: document.getElementById('cust-status').value
    };

    if (!id) {
      payload.createLoginAccount = document.getElementById('cust-create-login')?.checked || false;
      payload.password = document.getElementById('cust-password')?.value || '';
      if (payload.createLoginAccount && !payload.password) {
        App.showToast('Please enter a password for the customer portal login.', 'warning');
        btn.disabled = false;
        return;
      }
    }

    try {
      if (id) {
        await Api.put(`/customers/${id}`, payload);
        App.showToast('Customer updated successfully.', 'success');
      } else {
        await Api.post('/customers', payload);
        App.showToast('Customer registered successfully.', 'success');
      }
      window.location.hash = '#customers';
    } catch (error) {
      App.showToast(error.message, 'danger');
    } finally {
      btn.disabled = false;
    }
  },

  confirmDelete(id, name) {
    App.confirmAction(
      'Delete Customer',
      `Are you sure you want to permanently delete customer "${name}"?`,
      async () => {
        try {
          await Api.delete(`/customers/${id}`);
          App.showToast(`Customer "${name}" deleted.`, 'success');
          this.data = this.data.filter(c => c.customerId !== id);
          this.filterList();
        } catch (error) {
          App.showToast(error.message, 'danger');
        }
      },
      'Delete Customer'
    );
  }
};
