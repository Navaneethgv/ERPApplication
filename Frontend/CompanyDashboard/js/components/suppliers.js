// Suppliers Component
const SuppliersComponent = {
  data: [],
  filteredData: [],
  currentPage: 1,
  pageSize: 10,

  async render(container, action, id) {
    if (action === 'new') {
      if (!Auth.hasPermission('suppliers', 'ADD')) {
        App.showToast('Access restricted: You do not have permission to add suppliers.', 'warning');
        window.location.hash = '#suppliers';
        return;
      }
      this.renderForm(container, null);
    } else if (action === 'edit' && id) {
      if (!Auth.hasPermission('suppliers', 'EDIT')) {
        App.showToast('Access restricted: You do not have permission to edit suppliers.', 'warning');
        window.location.hash = '#suppliers';
        return;
      }
      container.innerHTML = `
        <div class="loading-spinner-container">
          <div class="spinner-border text-primary mb-2"></div>
          <p>Loading supplier details...</p>
        </div>
      `;
      try {
        const supp = await Api.get(`/suppliers/${id}`);
        this.renderForm(container, supp);
      } catch (error) {
        App.showToast(error.message, 'danger');
        window.location.hash = '#suppliers';
      }
    } else {
      await this.renderList(container);
    }
  },

  async renderList(container) {
    container.innerHTML = `
      <div class="loading-spinner-container">
        <div class="spinner-border text-primary mb-2"></div>
        <p>Loading supplier directory...</p>
      </div>
    `;

    try {
      this.data = await Api.get('/suppliers');
      this.filteredData = [...this.data];
      this.currentPage = 1;
      this.renderTable(container);
    } catch (error) {
      container.innerHTML = `
        <div class="alert alert-danger">
          <h5><i class="bi bi-exclamation-circle me-2"></i>Failed to Load Suppliers</h5>
          <p class="mb-0">${error.message}</p>
        </div>
      `;
    }
  },

  renderTable(container) {
    const canAdd = Auth.hasPermission('suppliers', 'ADD');

    const start = (this.currentPage - 1) * this.pageSize;
    const pageItems = this.filteredData.slice(start, start + this.pageSize);

    container.innerHTML = `
      <div class="page-header-container">
        <div>
          <h1 class="page-title">Supplier Management</h1>
          <p class="page-subtitle">Manage vendors, procurement contacts, and payment terms</p>
        </div>
        <div>
          ${canAdd ? `
            <a href="#suppliers/new" class="btn btn-erp-primary btn-sm">
              <i class="bi bi-plus-circle me-1"></i>Add New Supplier
            </a>
          ` : ''}
        </div>
      </div>

      <!-- Filters -->
      <div class="erp-card mb-3">
        <div class="erp-card-body p-3">
          <div class="row g-2 align-items-center">
            <div class="col-md-6 col-lg-5">
              <div class="input-group input-group-sm">
                <span class="input-group-text bg-light"><i class="bi bi-search"></i></span>
                <input type="text" id="supp-search" class="form-control" placeholder="Search by vendor code, name, email, contact..." oninput="SuppliersComponent.filterList()">
              </div>
            </div>
            <div class="col-md-3 col-lg-3">
              <select id="supp-status-filter" class="form-select form-select-sm" onchange="SuppliersComponent.filterList()">
                <option value="">All Statuses</option>
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
              </select>
            </div>
            <div class="col-auto ms-auto text-muted small">
              Total: <strong id="supp-total-count">${this.filteredData.length}</strong> suppliers
            </div>
          </div>
        </div>
      </div>

      <!-- Suppliers Table -->
      <div class="erp-card">
        <div class="erp-table-wrapper">
          <table class="erp-table" id="suppliers-table">
            <thead>
              <tr>
                <th>Code</th>
                <th>Supplier Name</th>
                <th>Contact Info</th>
                <th>Address</th>
                <th>Payment Terms</th>
                <th>Status</th>
                <th class="text-end">Actions</th>
              </tr>
            </thead>
            <tbody id="suppliers-table-body">
              ${this.buildTableRows(pageItems)}
            </tbody>
          </table>
        </div>
        <div id="suppliers-pagination">
          ${App.renderPagination({
            currentPage: this.currentPage,
            pageSize: this.pageSize,
            totalItems: this.filteredData.length,
            componentName: 'SuppliersComponent'
          })}
        </div>
      </div>
    `;
  },

  buildTableRows(list) {
    if (!list || list.length === 0) {
      return `<tr><td colspan="7" class="text-center text-muted py-4"><i class="bi bi-truck fs-2 d-block mb-2 text-muted"></i>No suppliers found.</td></tr>`;
    }

    const canEdit = Auth.hasPermission('suppliers', 'EDIT');
    const canDelete = Auth.hasPermission('suppliers', 'DELETE');

    return list.map(s => `
      <tr>
        <td class="fw-semibold text-primary font-monospace">${s.supplierCode}</td>
        <td><div class="fw-semibold text-dark">${s.name}</div></td>
        <td>
          <div class="small"><i class="bi bi-person me-1 text-muted"></i>${s.contactPerson || '-'}</div>
          <div class="small"><i class="bi bi-envelope me-1 text-muted"></i>${s.email}</div>
          ${s.phone ? `<div class="small text-muted"><i class="bi bi-telephone me-1"></i>${s.phone}</div>` : ''}
        </td>
        <td class="text-muted small" style="max-width: 200px;">${s.address || '-'}</td>
        <td><span class="badge bg-light text-dark border">${s.paymentTerms}</span></td>
        <td><span class="badge-status badge-${s.status.toLowerCase()}">${s.status}</span></td>
        <td class="text-end text-nowrap">
          <div class="table-actions">
            ${canEdit ? `
              <a href="#suppliers/edit/${s.supplierId}" class="btn btn-outline-primary btn-sm" title="Edit Supplier">
                <i class="bi bi-pencil"></i>
              </a>
            ` : ''}
            ${canDelete ? `
              <button type="button" class="btn btn-outline-danger btn-sm" title="Delete Supplier" onclick="SuppliersComponent.confirmDelete(${s.supplierId}, '${s.name.replace(/'/g, "\\'")}')">
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
    const tbody = document.getElementById('suppliers-table-body');
    if (tbody) {
      tbody.innerHTML = this.buildTableRows(pageItems);
    }
    const pagContainer = document.getElementById('suppliers-pagination');
    if (pagContainer) {
      pagContainer.innerHTML = App.renderPagination({
        currentPage: this.currentPage,
        pageSize: this.pageSize,
        totalItems: this.filteredData.length,
        componentName: 'SuppliersComponent'
      });
    }
    const totalCount = document.getElementById('supp-total-count');
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
    const q = (document.getElementById('supp-search')?.value || '').toLowerCase();
    const status = document.getElementById('supp-status-filter')?.value || '';

    this.filteredData = this.data.filter(s => {
      const matchQ = !q || s.supplierCode.toLowerCase().includes(q) || s.name.toLowerCase().includes(q) || s.email.toLowerCase().includes(q) || (s.contactPerson && s.contactPerson.toLowerCase().includes(q));
      const matchStatus = !status || s.status === status;
      return matchQ && matchStatus;
    });

    this.currentPage = 1;
    this.updateTableView();
  },

  renderForm(container, supp) {
    const isEdit = !!supp;
    container.innerHTML = `
      <div class="page-header-container">
        <div>
          <h1 class="page-title">${isEdit ? 'Edit Supplier' : 'Add New Supplier'}</h1>
          <p class="page-subtitle">${isEdit ? `Update profile for ${supp.name}` : 'Register a new vendor for procurement'}</p>
        </div>
        <div>
          <a href="#suppliers" class="btn btn-erp-secondary btn-sm">
            <i class="bi bi-arrow-left me-1"></i>Back to Suppliers
          </a>
        </div>
      </div>

      <div class="erp-card">
        <div class="erp-card-body p-4">
          <form id="supplier-form" onsubmit="SuppliersComponent.submitForm(event, ${isEdit ? supp.supplierId : 'null'})">
            <div class="form-section-title"><i class="bi bi-truck me-2"></i>Vendor Information</div>

            <div class="row mb-3">
              <div class="col-md-4 mb-3 mb-md-0">
                <label class="form-label" for="supp-code">Supplier Code <span class="required-asterisk">*</span></label>
                <input type="text" id="supp-code" class="form-control font-monospace" value="${supp?.supplierCode || ''}" placeholder="e.g. SUP-TECH-04" required>
              </div>
              <div class="col-md-8">
                <label class="form-label" for="supp-name">Supplier / Vendor Name <span class="required-asterisk">*</span></label>
                <input type="text" id="supp-name" class="form-control" value="${supp?.name || ''}" placeholder="e.g. Pacific Electronics Direct" required>
              </div>
            </div>

            <div class="row mb-3">
              <div class="col-md-4 mb-3 mb-md-0">
                <label class="form-label" for="supp-contact">Contact Person</label>
                <input type="text" id="supp-contact" class="form-control" value="${supp?.contactPerson || ''}" placeholder="e.g. David Miller">
              </div>
              <div class="col-md-4 mb-3 mb-md-0">
                <label class="form-label" for="supp-email">Email Address <span class="required-asterisk">*</span></label>
                <input type="email" id="supp-email" class="form-control" value="${supp?.email || ''}" required>
              </div>
              <div class="col-md-4">
                <label class="form-label" for="supp-phone">Phone Number <span class="required-asterisk">*</span></label>
                <input type="tel" id="supp-phone" class="form-control" value="${supp?.phone || ''}" required>
              </div>
            </div>

            <div class="row mb-4">
              <div class="col-md-6 mb-3 mb-md-0">
                <label class="form-label" for="supp-address">Address</label>
                <input type="text" id="supp-address" class="form-control" value="${supp?.address || ''}" placeholder="Street, City, State">
              </div>
              <div class="col-md-3 mb-3 mb-md-0">
                <label class="form-label" for="supp-terms">Payment Terms <span class="required-asterisk">*</span></label>
                <select id="supp-terms" class="form-select" required>
                  <option value="Net 15" ${supp?.paymentTerms === 'Net 15' ? 'selected' : ''}>Net 15</option>
                  <option value="Net 30" ${(!supp || supp.paymentTerms === 'Net 30') ? 'selected' : ''}>Net 30</option>
                  <option value="Net 60" ${supp?.paymentTerms === 'Net 60' ? 'selected' : ''}>Net 60</option>
                  <option value="Due on Receipt" ${supp?.paymentTerms === 'Due on Receipt' ? 'selected' : ''}>Due on Receipt</option>
                </select>
              </div>
              <div class="col-md-3">
                <label class="form-label" for="supp-status">Status <span class="required-asterisk">*</span></label>
                <select id="supp-status" class="form-select" required>
                  <option value="Active" ${supp?.status === 'Active' ? 'selected' : ''}>Active</option>
                  <option value="Inactive" ${supp?.status === 'Inactive' ? 'selected' : ''}>Inactive</option>
                </select>
              </div>
            </div>

            <div class="d-flex justify-content-end gap-2 border-top pt-3">
              <a href="#suppliers" class="btn btn-erp-secondary">Cancel</a>
              <button type="submit" class="btn btn-erp-primary" id="btn-save-supp">
                <i class="bi bi-check2 me-1"></i>${isEdit ? 'Save Changes' : 'Create Supplier'}
              </button>
            </div>
          </form>
        </div>
      </div>
    `;
  },

  async submitForm(event, id) {
    event.preventDefault();
    const btn = document.getElementById('btn-save-supp');
    btn.disabled = true;

    const payload = {
      supplierCode: document.getElementById('supp-code').value.trim().toUpperCase(),
      name: document.getElementById('supp-name').value.trim(),
      contactPerson: document.getElementById('supp-contact').value.trim(),
      email: document.getElementById('supp-email').value.trim(),
      phone: document.getElementById('supp-phone').value.trim(),
      address: document.getElementById('supp-address').value.trim(),
      paymentTerms: document.getElementById('supp-terms').value,
      status: document.getElementById('supp-status').value
    };

    try {
      if (id) {
        await Api.put(`/suppliers/${id}`, payload);
        App.showToast('Supplier updated successfully.', 'success');
      } else {
        await Api.post('/suppliers', payload);
        App.showToast('Supplier created successfully.', 'success');
      }
      window.location.hash = '#suppliers';
    } catch (error) {
      App.showToast(error.message, 'danger');
    } finally {
      btn.disabled = false;
    }
  },

  confirmDelete(id, name) {
    App.confirmAction(
      'Delete Supplier',
      `Are you sure you want to permanently delete supplier "${name}"?`,
      async () => {
        try {
          await Api.delete(`/suppliers/${id}`);
          App.showToast(`Supplier "${name}" deleted.`, 'success');
          this.data = this.data.filter(s => s.supplierId !== id);
          this.filterList();
        } catch (error) {
          App.showToast(error.message, 'danger');
        }
      },
      'Delete Supplier'
    );
  }
};
