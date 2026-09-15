// Employees Component
const EmployeesComponent = {
  data: [],
  filteredData: [],
  currentPage: 1,
  pageSize: 10,

  async render(container, action, id) {
    if (action === 'new') {
      if (!Auth.hasPermission('employees', 'ADD')) {
        App.showToast('Access restricted: You do not have permission to add employees.', 'warning');
        window.location.hash = '#employees';
        return;
      }
      this.renderForm(container, null);
    } else if (action === 'edit' && id) {
      if (!Auth.hasPermission('employees', 'EDIT')) {
        App.showToast('Access restricted: You do not have permission to edit employees.', 'warning');
        window.location.hash = '#employees';
        return;
      }
      container.innerHTML = `
        <div class="loading-spinner-container">
          <div class="spinner-border text-primary mb-2"></div>
          <p>Loading employee record...</p>
        </div>
      `;
      try {
        const emp = await Api.get(`/employees/${id}`);
        this.renderForm(container, emp);
      } catch (error) {
        App.showToast(error.message, 'danger');
        window.location.hash = '#employees';
      }
    } else {
      await this.renderList(container);
    }
  },

  async renderList(container) {
    container.innerHTML = `
      <div class="loading-spinner-container">
        <div class="spinner-border text-primary mb-2"></div>
        <p>Loading employee roster...</p>
      </div>
    `;

    try {
      const res = await Api.get('/employees');
      this.data = Array.isArray(res) ? res : [];
      this.filteredData = [...this.data];
      this.currentPage = 1;
      this.renderTable(container);
    } catch (error) {
      container.innerHTML = `
        <div class="alert alert-danger">
          <h5><i class="bi bi-exclamation-circle me-2"></i>Failed to Load Employees</h5>
          <p class="mb-0">${error.message}</p>
        </div>
      `;
    }
  },

  renderTable(container) {
    if (!Array.isArray(this.data)) this.data = [];
    if (!Array.isArray(this.filteredData)) this.filteredData = [];
    const departments = [...new Set(this.data.map(e => e.department))].filter(Boolean);
    const canAdd = Auth.hasPermission('employees', 'ADD');

    const start = (this.currentPage - 1) * this.pageSize;
    const pageItems = this.filteredData.slice(start, start + this.pageSize);

    container.innerHTML = `
      <div class="page-header-container d-flex flex-column flex-sm-row justify-content-between align-items-start align-items-sm-center gap-2 mb-3 mb-md-4">
        <div>
          <h1 class="page-title">Employee Management</h1>
          <p class="page-subtitle">Manage organizational staff, roles, departments, and payroll access</p>
        </div>
        <div>
          ${canAdd ? `
            <a href="#employees/new" class="btn btn-erp-primary btn-sm">
              <i class="bi bi-person-plus-fill me-1"></i>Add New Employee
            </a>
          ` : ''}
        </div>
      </div>

      <!-- Filters Row -->
      <div class="erp-card mb-3">
        <div class="erp-card-body p-3">
          <div class="row g-2 align-items-center">
            <div class="col-12 col-md-5 col-lg-4">
              <div class="input-group input-group-sm">
                <span class="input-group-text bg-light"><i class="bi bi-search"></i></span>
                <input type="text" id="emp-search" class="form-control" placeholder="Search by name, email, designation..." oninput="EmployeesComponent.filterList()">
              </div>
            </div>
            <div class="col-12 col-sm-6 col-md-3">
              <select id="emp-dept-filter" class="form-select form-select-sm" onchange="EmployeesComponent.filterList()">
                <option value="">All Departments</option>
                ${departments.map(d => `<option value="${d}">${d}</option>`).join('')}
              </select>
            </div>
            <div class="col-12 col-sm-6 col-md-2">
              <select id="emp-status-filter" class="form-select form-select-sm" onchange="EmployeesComponent.filterList()">
                <option value="">All Statuses</option>
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
              </select>
            </div>
            <div class="col-12 col-sm-auto ms-sm-auto text-muted small text-sm-end mt-2 mt-sm-0">
              Total: <strong id="emp-total-count">${this.filteredData.length}</strong> staff
            </div>
          </div>
        </div>
      </div>

      <!-- Employee Table -->
      <div class="erp-card">
        <div class="table-responsive erp-table-wrapper">
          <table class="erp-table align-middle" id="employees-table" style="min-width: 780px;">
            <thead>
              <tr>
                <th>ID</th>
                <th>Employee Name</th>
                <th>Contact Info</th>
                <th>Department</th>
                <th>Designation</th>
                <th>Salary</th>
                <th>Joining Date</th>
                <th>Status</th>
                <th class="text-end">Actions</th>
              </tr>
            </thead>
            <tbody id="employees-table-body">
              ${this.buildTableRows(pageItems)}
            </tbody>
          </table>
        </div>
        <div id="employees-pagination">
          ${App.renderPagination({
            currentPage: this.currentPage,
            pageSize: this.pageSize,
            totalItems: this.filteredData.length,
            componentName: 'EmployeesComponent'
          })}
        </div>
      </div>
    `;
  },

  buildTableRows(list) {
    if (!list || list.length === 0) {
      return `<tr><td colspan="9" class="text-center text-muted py-4"><i class="bi bi-people fs-2 d-block mb-2 text-muted"></i>No employees found matching criteria.</td></tr>`;
    }

    const canEdit = Auth.hasPermission('employees', 'EDIT');
    const canDelete = Auth.hasPermission('employees', 'DELETE');

    return list.map(e => `
      <tr>
        <td class="text-muted">#${e.employeeId}</td>
        <td>
          <div class="fw-semibold text-dark">${e.fullName}</div>
        </td>
        <td>
          <div class="small"><i class="bi bi-envelope me-1 text-muted"></i>${e.email}</div>
          ${e.phone ? `<div class="small text-muted"><i class="bi bi-telephone me-1"></i>${e.phone}</div>` : ''}
        </td>
        <td><span class="badge bg-light text-dark border">${e.department}</span></td>
        <td class="text-muted">${e.designation}</td>
        <td class="fw-semibold">${App.formatCurrency(e.salary)}</td>
        <td class="text-muted small">${App.formatDate(e.joiningDate)}</td>
        <td><span class="badge-status badge-${e.status.toLowerCase()}">${e.status}</span></td>
        <td class="text-end text-nowrap">
          <div class="table-actions">
            ${canEdit ? `
              <a href="#employees/edit/${e.employeeId}" class="btn btn-outline-primary btn-sm" title="Edit Employee">
                <i class="bi bi-pencil"></i>
              </a>
            ` : ''}
            ${canDelete ? `
              <button type="button" class="btn btn-outline-danger btn-sm" title="Delete Employee" onclick="EmployeesComponent.confirmDelete(${e.employeeId}, '${e.fullName.replace(/'/g, "\\'")}')">
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
    const tbody = document.getElementById('employees-table-body');
    if (tbody) {
      tbody.innerHTML = this.buildTableRows(pageItems);
    }
    const pagContainer = document.getElementById('employees-pagination');
    if (pagContainer) {
      pagContainer.innerHTML = App.renderPagination({
        currentPage: this.currentPage,
        pageSize: this.pageSize,
        totalItems: this.filteredData.length,
        componentName: 'EmployeesComponent'
      });
    }
    const totalCount = document.getElementById('emp-total-count');
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
    const q = (document.getElementById('emp-search')?.value || '').toLowerCase();
    const dept = document.getElementById('emp-dept-filter')?.value || '';
    const status = document.getElementById('emp-status-filter')?.value || '';

    this.filteredData = this.data.filter(e => {
      const matchQ = !q || e.fullName.toLowerCase().includes(q) || e.email.toLowerCase().includes(q) || e.designation.toLowerCase().includes(q);
      const matchDept = !dept || e.department === dept;
      const matchStatus = !status || e.status === status;
      return matchQ && matchDept && matchStatus;
    });

    this.currentPage = 1;
    this.updateTableView();
  },

  renderForm(container, emp) {
    const isEdit = !!emp;
    container.innerHTML = `
      <div class="page-header-container d-flex flex-column flex-sm-row justify-content-between align-items-start align-items-sm-center gap-2 mb-3 mb-md-4">
        <div>
          <h1 class="page-title">${isEdit ? 'Edit Employee' : 'Add New Employee'}</h1>
          <p class="page-subtitle">${isEdit ? `Updating profile for ${emp.fullName}` : 'Register a new team member and assign access'}</p>
        </div>
        <div>
          <a href="#employees" class="btn btn-erp-secondary btn-sm">
            <i class="bi bi-arrow-left me-1"></i>Back to Roster
          </a>
        </div>
      </div>

      <div class="erp-card">
        <div class="erp-card-body p-3 p-md-4">
          <form id="employee-form" onsubmit="EmployeesComponent.submitForm(event, ${isEdit ? emp.employeeId : 'null'})">
            <div class="form-section-title"><i class="bi bi-person-badge me-2"></i>Personal & Contact Information</div>
            
            <div class="row g-3 mb-3">
              <div class="col-12 col-md-6">
                <label class="form-label" for="emp-first-name">First Name <span class="required-asterisk">*</span></label>
                <input type="text" id="emp-first-name" class="form-control" value="${emp?.firstName || ''}" required>
              </div>
              <div class="col-12 col-md-6">
                <label class="form-label" for="emp-last-name">Last Name <span class="required-asterisk">*</span></label>
                <input type="text" id="emp-last-name" class="form-control" value="${emp?.lastName || ''}" required>
              </div>
            </div>

            <div class="row g-3 mb-4">
              <div class="col-12 col-md-6">
                <label class="form-label" for="emp-email">Email Address <span class="required-asterisk">*</span></label>
                <input type="email" id="emp-email" class="form-control" value="${emp?.email || ''}" required>
              </div>
              <div class="col-12 col-md-6">
                <label class="form-label" for="emp-phone">Phone Number</label>
                <input type="tel" id="emp-phone" class="form-control" value="${emp?.phone || ''}">
              </div>
            </div>

            <div class="form-section-title"><i class="bi bi-briefcase me-2"></i>Department & Compensation</div>

            <div class="row g-3 mb-3">
              <div class="col-12 col-md-6">
                <label class="form-label" for="emp-department">Department <span class="required-asterisk">*</span></label>
                <select id="emp-department" class="form-select" required>
                  <option value="">Select Department</option>
                  <option value="Executive" ${emp?.department === 'Executive' ? 'selected' : ''}>Executive</option>
                  <option value="Operations" ${emp?.department === 'Operations' ? 'selected' : ''}>Operations</option>
                  <option value="Sales" ${emp?.department === 'Sales' ? 'selected' : ''}>Sales</option>
                  <option value="Logistics" ${emp?.department === 'Logistics' ? 'selected' : ''}>Logistics</option>
                  <option value="Finance" ${emp?.department === 'Finance' ? 'selected' : ''}>Finance</option>
                  <option value="Human Resources" ${emp?.department === 'Human Resources' ? 'selected' : ''}>Human Resources</option>
                </select>
              </div>
              <div class="col-12 col-md-6">
                <label class="form-label" for="emp-designation">Job Title / Designation <span class="required-asterisk">*</span></label>
                <input type="text" id="emp-designation" class="form-control" placeholder="e.g. Operations Specialist" value="${emp?.designation || ''}" required>
              </div>
            </div>

            <div class="row g-3 mb-4">
              <div class="col-12 col-md-4">
                <label class="form-label" for="emp-salary">Annual Salary (₹) <span class="required-asterisk">*</span></label>
                <input type="number" step="0.01" min="0" id="emp-salary" class="form-control" value="${emp?.salary || 0}" required>
              </div>
              <div class="col-12 col-sm-6 col-md-4">
                <label class="form-label" for="emp-joining-date">Joining Date <span class="required-asterisk">*</span></label>
                <input type="date" id="emp-joining-date" class="form-control" value="${emp ? emp.joiningDate.slice(0, 10) : new Date().toISOString().slice(0, 10)}" required>
              </div>
              <div class="col-12 col-sm-6 col-md-4">
                <label class="form-label" for="emp-status">Employment Status <span class="required-asterisk">*</span></label>
                <select id="emp-status" class="form-select" required>
                  <option value="Active" ${emp?.status === 'Active' ? 'selected' : ''}>Active</option>
                  <option value="Inactive" ${emp?.status === 'Inactive' ? 'selected' : ''}>Inactive</option>
                </select>
              </div>
            </div>

            ${!isEdit ? `
              <div class="form-section-title"><i class="bi bi-shield-lock me-2"></i>ERP Login Credentials</div>
              <div class="row g-3 mb-4">
                <div class="col-12">
                  <div class="form-check">
                    <input class="form-check-input" type="checkbox" id="emp-create-login" onchange="document.getElementById('emp-password-row').style.display = this.checked ? 'block' : 'none'">
                    <label class="form-check-label fw-semibold" for="emp-create-login">
                      Create ERP Employee Login Account
                    </label>
                  </div>
                </div>
                <div class="col-12 col-md-6" id="emp-password-row" style="display: none;">
                  <label class="form-label" for="emp-password">Initial Password <span class="required-asterisk">*</span></label>
                  <input type="password" id="emp-password" class="form-control" placeholder="Minimum 6 characters">
                </div>
              </div>
            ` : ''}

            <div class="d-flex flex-column-reverse flex-sm-row justify-content-end gap-2 border-top pt-3">
              <a href="#employees" class="btn btn-erp-secondary">Cancel</a>
              <button type="submit" class="btn btn-erp-primary" id="btn-save-emp">
                <i class="bi bi-check2 me-1"></i>${isEdit ? 'Save Changes' : 'Create Employee'}
              </button>
            </div>
          </form>
        </div>
      </div>
    `;
  },

  async submitForm(event, id) {
    event.preventDefault();
    const btn = document.getElementById('btn-save-emp');
    btn.disabled = true;

    const payload = {
      firstName: document.getElementById('emp-first-name').value.trim(),
      lastName: document.getElementById('emp-last-name').value.trim(),
      email: document.getElementById('emp-email').value.trim(),
      phone: document.getElementById('emp-phone').value.trim(),
      department: document.getElementById('emp-department').value,
      designation: document.getElementById('emp-designation').value.trim(),
      salary: parseFloat(document.getElementById('emp-salary').value) || 0,
      joiningDate: document.getElementById('emp-joining-date').value,
      status: document.getElementById('emp-status').value
    };

    if (!id) {
      payload.createLoginAccount = document.getElementById('emp-create-login')?.checked || false;
      payload.password = document.getElementById('emp-password')?.value || '';
      if (payload.createLoginAccount && !payload.password) {
        App.showToast('Please enter a password for the login account.', 'warning');
        btn.disabled = false;
        return;
      }
    }

    try {
      if (id) {
        await Api.put(`/employees/${id}`, payload);
        App.showToast('Employee updated successfully.', 'success');
      } else {
        await Api.post('/employees', payload);
        App.showToast('Employee registered successfully.', 'success');
      }
      window.location.hash = '#employees';
    } catch (error) {
      App.showToast(error.message, 'danger');
    } finally {
      btn.disabled = false;
    }
  },

  confirmDelete(id, name) {
    App.confirmAction(
      'Delete Employee',
      `Are you sure you want to permanently remove employee "${name}" and any associated user login?`,
      async () => {
        try {
          await Api.delete(`/employees/${id}`);
          App.showToast(`Employee "${name}" deleted successfully.`, 'success');
          this.data = this.data.filter(e => e.employeeId !== id);
          this.filterList();
        } catch (error) {
          App.showToast(error.message, 'danger');
        }
      },
      'Delete Employee'
    );
  }
};
