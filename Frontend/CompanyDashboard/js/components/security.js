// Security & Access Control: Password Policy, Menu Master, Menu Options, Role Permissions
const SecurityComponent = {
  currentTab: 'policy',
  policyData: null,
  menusData: [],
  matrixData: null,
  selectedRole: 'Employee',

  async render(container, subAction = '', paramId = '') {
    if (!Auth.isAdmin()) {
      App.showToast('Security and permissions management requires Administrator privileges.', 'danger');
      window.location.hash = '#dashboard';
      return;
    }

    if (subAction && ['policy', 'menus', 'options', 'matrix'].includes(subAction)) {
      this.currentTab = subAction;
    }

    container.innerHTML = `
      <div class="content-header d-flex flex-wrap justify-content-between align-items-center mb-4 gap-2">
        <div>
          <h2 class="content-title mb-1"><i class="bi bi-shield-lock-fill text-danger me-2"></i>Security & Permissions</h2>
          <p class="text-muted small mb-0">Manage password enforcement, menu hierarchies, granular actions, and role access control.</p>
        </div>
        <div class="d-flex gap-2">
          <button class="btn btn-outline-secondary btn-sm" onclick="SecurityComponent.refreshCurrentTab()">
            <i class="bi bi-arrow-clockwise me-1"></i>Refresh
          </button>
        </div>
      </div>

      <!-- Navigation Tabs -->
      <ul class="nav nav-pills custom-nav-pills flex-nowrap overflow-x-auto text-nowrap pb-2 mb-4" id="securityTabs">
        <li class="nav-item">
          <button class="nav-link ${this.currentTab === 'policy' ? 'active' : ''}" onclick="SecurityComponent.switchTab('policy')">
            <i class="bi bi-key-fill me-2"></i>Password Policy
          </button>
        </li>
        <li class="nav-item">
          <button class="nav-link ${this.currentTab === 'menus' ? 'active' : ''}" onclick="SecurityComponent.switchTab('menus')">
            <i class="bi bi-diagram-3-fill me-2"></i>Menu Master
          </button>
        </li>
        <li class="nav-item">
          <button class="nav-link ${this.currentTab === 'options' ? 'active' : ''}" onclick="SecurityComponent.switchTab('options')">
            <i class="bi bi-sliders me-2"></i>Menu Options
          </button>
        </li>
        <li class="nav-item">
          <button class="nav-link ${this.currentTab === 'matrix' ? 'active' : ''}" onclick="SecurityComponent.switchTab('matrix')">
            <i class="bi bi-person-check-fill me-2"></i>Role Permissions Matrix
          </button>
        </li>
      </ul>

      <!-- Tab Content Mount -->
      <div id="security-tab-content">
        <div class="text-center py-5">
          <div class="spinner-border text-primary" role="status"></div>
          <p class="text-muted mt-2 small">Loading security settings...</p>
        </div>
      </div>

      <!-- Modals Container -->
      <div id="security-modals-mount"></div>
    `;

    await this.loadTabContent();
  },

  async switchTab(tab) {
    this.currentTab = tab;
    document.querySelectorAll('#securityTabs .nav-link').forEach(btn => {
      btn.classList.remove('active');
    });
    const activeBtn = Array.from(document.querySelectorAll('#securityTabs .nav-link')).find(b => b.textContent.toLowerCase().includes(tab.slice(0, 4)));
    if (activeBtn) activeBtn.classList.add('active');
    await this.loadTabContent();
  },

  async refreshCurrentTab() {
    await this.loadTabContent();
    App.showToast('Security data refreshed.', 'info');
  },

  async loadTabContent() {
    const mount = document.getElementById('security-tab-content');
    if (!mount) return;

    try {
      if (this.currentTab === 'policy') {
        await this.renderPolicyTab(mount);
      } else if (this.currentTab === 'menus') {
        await this.renderMenusTab(mount);
      } else if (this.currentTab === 'options') {
        await this.renderOptionsTab(mount);
      } else if (this.currentTab === 'matrix') {
        await this.renderMatrixTab(mount);
      }
    } catch (err) {
      mount.innerHTML = `
        <div class="alert alert-danger shadow-sm">
          <i class="bi bi-exclamation-triangle-fill me-2"></i>Failed to load settings: ${err.message}
        </div>
      `;
    }
  },

  // ================= 1. PASSWORD POLICY TAB =================
  async renderPolicyTab(mount) {
    this.policyData = await Api.get('/PasswordPolicy');
    const p = this.policyData;

    mount.innerHTML = `
      <div class="row g-4">
        <!-- Settings Form -->
        <div class="col-12 col-lg-7">
          <div class="card border-0 shadow-sm">
            <div class="card-header bg-white py-3 border-bottom d-flex flex-wrap justify-content-between align-items-center gap-2">
              <h5 class="card-title mb-0 fw-bold"><i class="bi bi-shield-check text-primary me-2"></i>Password Security Configuration</h5>
              <span class="badge bg-secondary-subtle text-secondary small">Last updated: ${App.formatDateTime(p.updatedAt)}</span>
            </div>
            <div class="card-body p-3 p-md-4">
              <form id="password-policy-form" onsubmit="SecurityComponent.savePasswordPolicy(event)">
                <div class="row g-3">
                  <div class="col-12 col-md-6">
                    <label class="form-label fw-semibold" for="pol-min-len">Minimum Length</label>
                    <div class="input-group">
                      <span class="input-group-text"><i class="bi bi-123"></i></span>
                      <input type="number" id="pol-min-len" class="form-control" min="6" max="64" value="${p.minLength}" required>
                    </div>
                    <div class="form-text small">Recommended: At least 8 characters.</div>
                  </div>

                  <div class="col-12 col-md-6">
                    <label class="form-label fw-semibold" for="pol-expiry">Password Expiry (Days)</label>
                    <div class="input-group">
                      <span class="input-group-text"><i class="bi bi-calendar-event"></i></span>
                      <input type="number" id="pol-expiry" class="form-control" min="0" max="365" value="${p.expiryDays}" required>
                    </div>
                    <div class="form-text small">Enter 0 to disable periodic expiry.</div>
                  </div>

                  <div class="col-12 col-md-6">
                    <label class="form-label fw-semibold" for="pol-history">Password History Count</label>
                    <div class="input-group">
                      <span class="input-group-text"><i class="bi bi-clock-history"></i></span>
                      <input type="number" id="pol-history" class="form-control" min="0" max="24" value="${p.historyCount}" required>
                    </div>
                    <div class="form-text small">Number of previous passwords user cannot reuse.</div>
                  </div>

                  <div class="col-12 col-md-6">
                    <label class="form-label fw-semibold" for="pol-attempts">Max Failed Attempts (Lockout)</label>
                    <div class="input-group">
                      <span class="input-group-text"><i class="bi bi-person-x-fill text-danger"></i></span>
                      <input type="number" id="pol-attempts" class="form-control" min="1" max="20" value="${p.maxFailedAttempts}" required>
                    </div>
                    <div class="form-text small">Failed logins before account is locked.</div>
                  </div>

                  <div class="col-12 col-md-6">
                    <label class="form-label fw-semibold" for="pol-lockout-min">Lockout Duration (Minutes)</label>
                    <div class="input-group">
                      <span class="input-group-text"><i class="bi bi-stopwatch"></i></span>
                      <input type="number" id="pol-lockout-min" class="form-control" min="1" max="1440" value="${p.lockoutDurationMinutes}" required>
                    </div>
                    <div class="form-text small">Time user must wait after lockout.</div>
                  </div>
                </div>

                <hr class="my-4">

                <h6 class="fw-bold mb-3"><i class="bi bi-toggle2-on text-primary me-2"></i>Character Complexity Rules</h6>
                <div class="list-group list-group-flush mb-4">
                  <div class="list-group-item d-flex justify-content-between align-items-center px-0">
                    <div>
                      <div class="fw-semibold">Require Uppercase Letter (A-Z)</div>
                      <div class="text-muted small">Passwords must contain at least one uppercase letter</div>
                    </div>
                    <div class="form-check form-switch fs-5">
                      <input class="form-check-input" type="checkbox" id="pol-req-upper" ${p.requireUppercase ? 'checked' : ''} onchange="SecurityComponent.testPolicyLive()">
                    </div>
                  </div>

                  <div class="list-group-item d-flex justify-content-between align-items-center px-0">
                    <div>
                      <div class="fw-semibold">Require Lowercase Letter (a-z)</div>
                      <div class="text-muted small">Passwords must contain at least one lowercase letter</div>
                    </div>
                    <div class="form-check form-switch fs-5">
                      <input class="form-check-input" type="checkbox" id="pol-req-lower" ${p.requireLowercase ? 'checked' : ''} onchange="SecurityComponent.testPolicyLive()">
                    </div>
                  </div>

                  <div class="list-group-item d-flex justify-content-between align-items-center px-0">
                    <div>
                      <div class="fw-semibold">Require Numerical Digit (0-9)</div>
                      <div class="text-muted small">Passwords must contain at least one number</div>
                    </div>
                    <div class="form-check form-switch fs-5">
                      <input class="form-check-input" type="checkbox" id="pol-req-digit" ${p.requireDigit ? 'checked' : ''} onchange="SecurityComponent.testPolicyLive()">
                    </div>
                  </div>

                  <div class="list-group-item d-flex justify-content-between align-items-center px-0">
                    <div>
                      <div class="fw-semibold">Require Special Character (!@#$%^&*)</div>
                      <div class="text-muted small">Passwords must contain at least one special symbol</div>
                    </div>
                    <div class="form-check form-switch fs-5">
                      <input class="form-check-input" type="checkbox" id="pol-req-special" ${p.requireSpecialChar ? 'checked' : ''} onchange="SecurityComponent.testPolicyLive()">
                    </div>
                  </div>
                </div>

                <div class="d-flex flex-column-reverse flex-sm-row justify-content-end gap-2">
                  <button type="submit" class="btn btn-erp-primary" id="btn-save-policy">
                    <i class="bi bi-save me-1"></i>Save Policy Settings
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>

        <!-- Policy Tester & Security Overview -->
        <div class="col-12 col-lg-5">
          <div class="card border-0 shadow-sm mb-4">
            <div class="card-header bg-white py-3 border-bottom">
              <h5 class="card-title mb-0 fw-bold"><i class="bi bi-check-all text-success me-2"></i>Live Policy Tester</h5>
            </div>
            <div class="card-body p-4">
              <p class="text-muted small">Type a candidate password to verify its compliance against active policy rules:</p>
              <div class="input-group mb-3">
                <span class="input-group-text"><i class="bi bi-key"></i></span>
                <input type="text" id="live-test-password" class="form-control" placeholder="e.g. TestPass@2026" oninput="SecurityComponent.testPolicyLive()">
              </div>

              <div id="live-policy-checklist" class="p-3 bg-light rounded border">
                <!-- Live indicators -->
              </div>
            </div>
          </div>

          <div class="card border-0 shadow-sm bg-primary text-white">
            <div class="card-body p-4">
              <h5 class="fw-bold mb-2"><i class="bi bi-shield-lock me-2"></i>BCrypt Protection</h5>
              <p class="small text-white-50 mb-0">
                All credentials stored within the PostgreSQL database use salted and workload-hardened BCrypt hashes. Previous password entries are stored in encrypted history to enforce non-repetition without exposing plaintext.
              </p>
            </div>
          </div>
        </div>
      </div>
    `;

    this.testPolicyLive();
  },

  testPolicyLive() {
    const input = document.getElementById('live-test-password');
    const mount = document.getElementById('live-policy-checklist');
    if (!input || !mount) return;

    const pwd = input.value || '';
    const minLen = parseInt(document.getElementById('pol-min-len')?.value || 8, 10);
    const reqUpper = document.getElementById('pol-req-upper')?.checked ?? true;
    const reqLower = document.getElementById('pol-req-lower')?.checked ?? true;
    const reqDigit = document.getElementById('pol-req-digit')?.checked ?? true;
    const reqSpecial = document.getElementById('pol-req-special')?.checked ?? true;

    const checks = [
      { label: `At least ${minLen} characters long`, valid: pwd.length >= minLen },
      { label: 'Contains uppercase letter (A-Z)', valid: !reqUpper || /[A-Z]/.test(pwd) },
      { label: 'Contains lowercase letter (a-z)', valid: !reqLower || /[a-z]/.test(pwd) },
      { label: 'Contains numerical digit (0-9)', valid: !reqDigit || /[0-9]/.test(pwd) },
      { label: 'Contains special character (!@#$...)', valid: !reqSpecial || /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?~`]/.test(pwd) }
    ];

    const allValid = checks.every(c => c.valid) && pwd.length > 0;

    let html = `
      <div class="d-flex justify-content-between align-items-center mb-2">
        <span class="small fw-bold text-dark">Password Compliance:</span>
        <span class="badge ${allValid ? 'bg-success' : 'bg-secondary'}">${allValid ? 'Valid Password' : 'Incomplete'}</span>
      </div>
      <div class="d-flex flex-column gap-2 small">
    `;

    checks.forEach(c => {
      const icon = c.valid
        ? '<i class="bi bi-check-circle-fill text-success me-2"></i>'
        : '<i class="bi bi-x-circle-fill text-danger me-2"></i>';
      const color = c.valid ? 'text-success' : 'text-muted';
      html += `<div class="${color}">${icon}${c.label}</div>`;
    });

    html += `</div>`;
    mount.innerHTML = html;
  },

  async savePasswordPolicy(e) {
    e.preventDefault();
    const btn = document.getElementById('btn-save-policy');
    btn.disabled = true;

    const payload = {
      minLength: parseInt(document.getElementById('pol-min-len').value, 10),
      expiryDays: parseInt(document.getElementById('pol-expiry').value, 10),
      historyCount: parseInt(document.getElementById('pol-history').value, 10),
      maxFailedAttempts: parseInt(document.getElementById('pol-attempts').value, 10),
      lockoutDurationMinutes: parseInt(document.getElementById('pol-lockout-min').value, 10),
      requireUppercase: document.getElementById('pol-req-upper').checked,
      requireLowercase: document.getElementById('pol-req-lower').checked,
      requireDigit: document.getElementById('pol-req-digit').checked,
      requireSpecialChar: document.getElementById('pol-req-special').checked
    };

    try {
      this.policyData = await Api.put('/PasswordPolicy', payload);
      App.showToast('Password security policy successfully saved!', 'success');
      this.renderPolicyTab(document.getElementById('security-tab-content'));
    } catch (err) {
      App.showToast(`Failed to update policy: ${err.message}`, 'danger');
    } finally {
      btn.disabled = false;
    }
  },

  // ================= 2. MENU MASTER TAB =================
  async renderMenusTab(mount) {
    this.menusData = await Api.get('/Menus');

    let rowsHtml = '';
    const renderMenuRow = (m, level = 0) => {
      const indent = level > 0 ? `<span class="text-muted me-2" style="margin-left: ${level * 24}px;">↳</span>` : '';
      const typeBadge = level === 0 && (!m.route || m.subMenus?.length > 0)
        ? `<span class="badge bg-primary-subtle text-primary">Parent Group</span>`
        : `<span class="badge bg-light text-dark border">Menu Item</span>`;

      const optionsBadges = (m.options || []).map(o => `<span class="badge bg-info-subtle text-info me-1">${o.code}</span>`).join('');

      rowsHtml += `
        <tr>
          <td>
            <div class="d-flex align-items-center">
              ${indent}
              <i class="bi ${m.icon || 'bi-circle'} me-2 text-primary fs-5"></i>
              <div>
                <span class="fw-semibold text-dark">${m.title}</span>
                ${m.route ? `<br><small class="text-muted">Route: <code>#${m.route}</code></small>` : ''}
              </div>
            </div>
          </td>
          <td>${typeBadge}</td>
          <td><code>${m.route || '-'}</code></td>
          <td>${optionsBadges || '<span class="text-muted small">None</span>'}</td>
          <td><span class="badge bg-secondary-subtle text-secondary">${m.sortOrder}</span></td>
          <td>
            <span class="badge ${m.isActive ? 'bg-success-subtle text-success' : 'bg-danger-subtle text-danger'}">
              ${m.isActive ? 'Active' : 'Inactive'}
            </span>
          </td>
          <td class="text-end">
            <button class="btn btn-outline-primary btn-sm me-1" title="Edit Menu" onclick="SecurityComponent.openEditMenuModal(${m.menuId})">
              <i class="bi bi-pencil"></i>
            </button>
            <button class="btn btn-outline-danger btn-sm" title="Delete Menu" onclick="SecurityComponent.confirmDeleteMenu(${m.menuId}, '${m.title.replace(/'/g, "\\'")}')">
              <i class="bi bi-trash"></i>
            </button>
          </td>
        </tr>
      `;

      if (m.subMenus && m.subMenus.length > 0) {
        m.subMenus.forEach(child => renderMenuRow(child, level + 1));
      }
    };

    this.menusData.forEach(m => renderMenuRow(m, 0));

    mount.innerHTML = `
      <div class="card border-0 shadow-sm">
        <div class="card-header bg-white py-3 border-bottom d-flex flex-column flex-sm-row justify-content-between align-items-start align-items-sm-center gap-2">
          <div>
            <h5 class="card-title mb-0 fw-bold"><i class="bi bi-diagram-3-fill text-primary me-2"></i>Application Menu Hierarchy</h5>
            <small class="text-muted">Manage system menus, submenus, icons, and routes.</small>
          </div>
          <button class="btn btn-erp-primary btn-sm" onclick="SecurityComponent.openCreateMenuModal()">
            <i class="bi bi-plus-circle me-1"></i>Add New Menu
          </button>
        </div>
        <div class="table-responsive erp-table-wrapper">
          <table class="table table-hover align-middle mb-0" style="min-width: 720px;">
            <thead class="table-light">
              <tr>
                <th>Menu Title</th>
                <th>Type</th>
                <th>Route Path</th>
                <th>Granted Options</th>
                <th>Sort Order</th>
                <th>Status</th>
                <th class="text-end">Actions</th>
              </tr>
            </thead>
            <tbody>
              ${rowsHtml || `<tr><td colspan="7" class="text-center py-4 text-muted">No menus defined.</td></tr>`}
            </tbody>
          </table>
        </div>
      </div>
    `;
  },

  openCreateMenuModal(parentId = null) {
    const parentOptions = this.menusData
      .map(m => `<option value="${m.menuId}" ${parentId === m.menuId ? 'selected' : ''}>${m.title}</option>`)
      .join('');

    const modalMount = document.getElementById('security-modals-mount');
    modalMount.innerHTML = `
      <div class="modal fade" id="createMenuModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered modal-dialog-scrollable">
          <div class="modal-content border-0 shadow">
            <div class="modal-header">
              <h5 class="modal-title fw-bold"><i class="bi bi-plus-circle text-primary me-2"></i>Create New Menu</h5>
              <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <form id="create-menu-form" onsubmit="SecurityComponent.submitCreateMenu(event)">
              <div class="modal-body">
                <div class="mb-3">
                  <label class="form-label" for="menu-title">Menu Title <span class="required-asterisk">*</span></label>
                  <input type="text" id="menu-title" class="form-control" placeholder="e.g. Analytics Hub" required>
                </div>
                <div class="row g-3 mb-3">
                  <div class="col-12 col-md-6">
                    <label class="form-label" for="menu-route">Route Identifier</label>
                    <input type="text" id="menu-route" class="form-control" placeholder="e.g. analytics (or blank for parent group)">
                  </div>
                  <div class="col-12 col-md-6">
                    <label class="form-label" for="menu-icon">Bootstrap Icon Class</label>
                    <input type="text" id="menu-icon" class="form-control" placeholder="bi-graph-up" value="bi-app">
                  </div>
                </div>
                <div class="row g-3 mb-3">
                  <div class="col-12 col-md-6">
                    <label class="form-label" for="menu-parent">Parent Menu</label>
                    <select id="menu-parent" class="form-select">
                      <option value="">None (Top-Level Menu)</option>
                      ${parentOptions}
                    </select>
                  </div>
                  <div class="col-12 col-md-6">
                    <label class="form-label" for="menu-sort">Sort Order</label>
                    <input type="number" id="menu-sort" class="form-control" value="10" min="0">
                  </div>
                </div>

                <div class="mb-3">
                  <label class="form-label fw-semibold">Default Permission Actions to Generate:</label>
                  <div class="d-flex flex-wrap gap-3">
                    <label class="form-check"><input class="form-check-input" type="checkbox" id="gen-view" checked> View</label>
                    <label class="form-check"><input class="form-check-input" type="checkbox" id="gen-add" checked> Add</label>
                    <label class="form-check"><input class="form-check-input" type="checkbox" id="gen-edit" checked> Edit</label>
                    <label class="form-check"><input class="form-check-input" type="checkbox" id="gen-del" checked> Delete</label>
                  </div>
                </div>

                <div class="form-check form-switch mt-2">
                  <input class="form-check-input" type="checkbox" id="menu-active" checked>
                  <label class="form-check-label fw-semibold" for="menu-active">Active in Navigation</label>
                </div>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-erp-secondary btn-sm" data-bs-dismiss="modal">Cancel</button>
                <button type="submit" class="btn btn-erp-primary btn-sm" id="btn-submit-create-menu">Create Menu</button>
              </div>
            </form>
          </div>
        </div>
      </div>
    `;

    const modal = new bootstrap.Modal(document.getElementById('createMenuModal'));
    modal.show();
  },

  async submitCreateMenu(e) {
    e.preventDefault();
    const btn = document.getElementById('btn-submit-create-menu');
    btn.disabled = true;

    const parentVal = document.getElementById('menu-parent').value;
    const defaultOptions = [];
    if (document.getElementById('gen-view').checked) defaultOptions.push('VIEW');
    if (document.getElementById('gen-add').checked) defaultOptions.push('ADD');
    if (document.getElementById('gen-edit').checked) defaultOptions.push('EDIT');
    if (document.getElementById('gen-del').checked) defaultOptions.push('DELETE');

    const payload = {
      title: document.getElementById('menu-title').value.trim(),
      route: document.getElementById('menu-route').value.trim() || null,
      icon: document.getElementById('menu-icon').value.trim() || 'bi-app',
      parentMenuId: parentVal ? parseInt(parentVal, 10) : null,
      sortOrder: parseInt(document.getElementById('menu-sort').value, 10),
      isActive: document.getElementById('menu-active').checked,
      defaultOptions: defaultOptions
    };

    try {
      await Api.post('/Menus', payload);
      bootstrap.Modal.getInstance(document.getElementById('createMenuModal'))?.hide();
      App.showToast('Menu created successfully!', 'success');
      await Auth.loadPermissions();
      await this.renderMenusTab(document.getElementById('security-tab-content'));
    } catch (err) {
      App.showToast(`Failed to create menu: ${err.message}`, 'danger');
    } finally {
      btn.disabled = false;
    }
  },

  async openEditMenuModal(menuId) {
    const menu = await Api.get(`/Menus/${menuId}`);
    if (!menu) return;

    const parentOptions = this.menusData
      .filter(m => m.menuId !== menuId)
      .map(m => `<option value="${m.menuId}" ${menu.parentMenuId === m.menuId ? 'selected' : ''}>${m.title}</option>`)
      .join('');

    const modalMount = document.getElementById('security-modals-mount');
    modalMount.innerHTML = `
      <div class="modal fade" id="editMenuModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered modal-dialog-scrollable">
          <div class="modal-content border-0 shadow">
            <div class="modal-header">
              <h5 class="modal-title fw-bold"><i class="bi bi-pencil-square text-primary me-2"></i>Edit Menu: ${menu.title}</h5>
              <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <form id="edit-menu-form" onsubmit="SecurityComponent.submitEditMenu(event, ${menuId})">
              <div class="modal-body">
                <div class="mb-3">
                  <label class="form-label" for="edit-menu-title">Menu Title <span class="required-asterisk">*</span></label>
                  <input type="text" id="edit-menu-title" class="form-control" value="${menu.title}" required>
                </div>
                <div class="row g-3 mb-3">
                  <div class="col-12 col-md-6">
                    <label class="form-label" for="edit-menu-route">Route Identifier</label>
                    <input type="text" id="edit-menu-route" class="form-control" value="${menu.route || ''}">
                  </div>
                  <div class="col-12 col-md-6">
                    <label class="form-label" for="edit-menu-icon">Bootstrap Icon Class</label>
                    <input type="text" id="edit-menu-icon" class="form-control" value="${menu.icon || ''}">
                  </div>
                </div>
                <div class="row g-3 mb-3">
                  <div class="col-12 col-md-6">
                    <label class="form-label" for="edit-menu-parent">Parent Menu</label>
                    <select id="edit-menu-parent" class="form-select">
                      <option value="">None (Top-Level Menu)</option>
                      ${parentOptions}
                    </select>
                  </div>
                  <div class="col-12 col-md-6">
                    <label class="form-label" for="edit-menu-sort">Sort Order</label>
                    <input type="number" id="edit-menu-sort" class="form-control" value="${menu.sortOrder}" min="0">
                  </div>
                </div>

                <div class="form-check form-switch mt-2">
                  <input class="form-check-input" type="checkbox" id="edit-menu-active" ${menu.isActive ? 'checked' : ''}>
                  <label class="form-check-label fw-semibold" for="edit-menu-active">Active in Navigation</label>
                </div>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-erp-secondary btn-sm" data-bs-dismiss="modal">Cancel</button>
                <button type="submit" class="btn btn-erp-primary btn-sm" id="btn-submit-edit-menu">Save Changes</button>
              </div>
            </form>
          </div>
        </div>
      </div>
    `;

    const modal = new bootstrap.Modal(document.getElementById('editMenuModal'));
    modal.show();
  },

  async submitEditMenu(e, menuId) {
    e.preventDefault();
    const btn = document.getElementById('btn-submit-edit-menu');
    btn.disabled = true;

    const parentVal = document.getElementById('edit-menu-parent').value;
    const payload = {
      title: document.getElementById('edit-menu-title').value.trim(),
      route: document.getElementById('edit-menu-route').value.trim() || null,
      icon: document.getElementById('edit-menu-icon').value.trim() || null,
      parentMenuId: parentVal ? parseInt(parentVal, 10) : null,
      sortOrder: parseInt(document.getElementById('edit-menu-sort').value, 10),
      isActive: document.getElementById('edit-menu-active').checked
    };

    try {
      await Api.put(`/Menus/${menuId}`, payload);
      bootstrap.Modal.getInstance(document.getElementById('editMenuModal'))?.hide();
      App.showToast('Menu updated successfully!', 'success');
      await Auth.loadPermissions();
      await this.renderMenusTab(document.getElementById('security-tab-content'));
    } catch (err) {
      App.showToast(`Failed to update menu: ${err.message}`, 'danger');
    } finally {
      btn.disabled = false;
    }
  },

  confirmDeleteMenu(menuId, title) {
    App.confirmAction(
      'Delete Menu',
      `Are you sure you want to delete the menu "${title}"? Any associated permission mappings will also be removed.`,
      async () => {
        try {
          await Api.delete(`/Menus/${menuId}`);
          App.showToast(`Menu "${title}" deleted.`, 'success');
          await Auth.loadPermissions();
          await SecurityComponent.renderMenusTab(document.getElementById('security-tab-content'));
        } catch (err) {
          App.showToast(`Failed to delete menu: ${err.message}`, 'danger');
        }
      },
      'Delete'
    );
  },

  // ================= 3. MENU OPTIONS TAB =================
  async renderOptionsTab(mount) {
    this.menusData = await Api.get('/Menus');

    // Flatten all options
    const allOptions = [];
    const collectOptions = (m) => {
      if (m.options && m.options.length > 0) {
        m.options.forEach(o => {
          allOptions.push({ ...o, menuTitle: m.title, menuRoute: m.route });
        });
      }
      if (m.subMenus && m.subMenus.length > 0) {
        m.subMenus.forEach(collectOptions);
      }
    };
    this.menusData.forEach(collectOptions);

    let rowsHtml = allOptions.map(opt => `
      <tr>
        <td><strong>${opt.menuTitle}</strong> ${opt.menuRoute ? `<small class="text-muted">(${opt.menuRoute})</small>` : ''}</td>
        <td><span class="badge bg-primary-subtle text-primary fw-bold">${opt.code}</span></td>
        <td>${opt.name}</td>
        <td><span class="text-muted small">${opt.description || '-'}</span></td>
        <td><span class="badge bg-secondary-subtle text-secondary">${opt.sortOrder}</span></td>
        <td class="text-end">
          <button class="btn btn-outline-primary btn-sm me-1" title="Edit Option" onclick="SecurityComponent.openEditOptionModal(${opt.menuOptionId}, '${opt.name.replace(/'/g, "\\'")}', '${opt.code}', '${(opt.description || '').replace(/'/g, "\\'")}', ${opt.sortOrder})">
            <i class="bi bi-pencil"></i>
          </button>
          <button class="btn btn-outline-danger btn-sm" title="Delete Option" onclick="SecurityComponent.confirmDeleteOption(${opt.menuOptionId}, '${opt.code}')">
            <i class="bi bi-trash"></i>
          </button>
        </td>
      </tr>
    `).join('');

    mount.innerHTML = `
      <div class="card border-0 shadow-sm">
        <div class="card-header bg-white py-3 border-bottom d-flex flex-column flex-sm-row justify-content-between align-items-start align-items-sm-center gap-2">
          <div>
            <h5 class="card-title mb-0 fw-bold"><i class="bi bi-sliders text-primary me-2"></i>Granular Menu Options</h5>
            <small class="text-muted">Manage actionable operations per menu (View, Add, Edit, Delete, Export, etc.)</small>
          </div>
          <button class="btn btn-erp-primary btn-sm" onclick="SecurityComponent.openCreateOptionModal()">
            <i class="bi bi-plus-circle me-1"></i>Add Option
          </button>
        </div>
        <div class="table-responsive erp-table-wrapper">
          <table class="table table-hover align-middle mb-0" style="min-width: 660px;">
            <thead class="table-light">
              <tr>
                <th>Menu Module</th>
                <th>Action Code</th>
                <th>Display Name</th>
                <th>Description</th>
                <th>Sort Order</th>
                <th class="text-end">Actions</th>
              </tr>
            </thead>
            <tbody>
              ${rowsHtml || `<tr><td colspan="6" class="text-center py-4 text-muted">No options configured.</td></tr>`}
            </tbody>
          </table>
        </div>
      </div>
    `;
  },

  openCreateOptionModal() {
    const menuOptions = [];
    const collectMenus = (m, level = 0) => {
      const prefix = level > 0 ? '-- '.repeat(level) : '';
      menuOptions.push(`<option value="${m.menuId}">${prefix}${m.title}</option>`);
      if (m.subMenus) m.subMenus.forEach(s => collectMenus(s, level + 1));
    };
    this.menusData.forEach(m => collectMenus(m));

    const modalMount = document.getElementById('security-modals-mount');
    modalMount.innerHTML = `
      <div class="modal fade" id="createOptionModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered modal-dialog-scrollable">
          <div class="modal-content border-0 shadow">
            <div class="modal-header">
              <h5 class="modal-title fw-bold"><i class="bi bi-plus-circle text-primary me-2"></i>Add Menu Option</h5>
              <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <form id="create-option-form" onsubmit="SecurityComponent.submitCreateOption(event)">
              <div class="modal-body">
                <div class="mb-3">
                  <label class="form-label" for="opt-menu-id">Select Menu Module <span class="required-asterisk">*</span></label>
                  <select id="opt-menu-id" class="form-select" required>
                    ${menuOptions.join('')}
                  </select>
                </div>
                <div class="row g-3 mb-3">
                  <div class="col-12 col-md-6">
                    <label class="form-label" for="opt-code">Action Code <span class="required-asterisk">*</span></label>
                    <input type="text" id="opt-code" class="form-control text-uppercase" placeholder="e.g. EXPORT" required>
                  </div>
                  <div class="col-12 col-md-6">
                    <label class="form-label" for="opt-name">Display Name <span class="required-asterisk">*</span></label>
                    <input type="text" id="opt-name" class="form-control" placeholder="e.g. Export Data" required>
                  </div>
                </div>
                <div class="mb-3">
                  <label class="form-label" for="opt-desc">Description</label>
                  <input type="text" id="opt-desc" class="form-control" placeholder="Optional notes regarding this action">
                </div>
                <div class="mb-3">
                  <label class="form-label" for="opt-sort">Sort Order</label>
                  <input type="number" id="opt-sort" class="form-control" value="5" min="0">
                </div>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-erp-secondary btn-sm" data-bs-dismiss="modal">Cancel</button>
                <button type="submit" class="btn btn-erp-primary btn-sm" id="btn-submit-create-opt">Add Option</button>
              </div>
            </form>
          </div>
        </div>
      </div>
    `;

    const modal = new bootstrap.Modal(document.getElementById('createOptionModal'));
    modal.show();
  },

  async submitCreateOption(e) {
    e.preventDefault();
    const btn = document.getElementById('btn-submit-create-opt');
    btn.disabled = true;

    const menuId = parseInt(document.getElementById('opt-menu-id').value, 10);
    const payload = {
      name: document.getElementById('opt-name').value.trim(),
      code: document.getElementById('opt-code').value.trim().toUpperCase(),
      description: document.getElementById('opt-desc').value.trim() || null,
      sortOrder: parseInt(document.getElementById('opt-sort').value, 10)
    };

    try {
      await Api.post(`/Menus/${menuId}/options`, payload);
      bootstrap.Modal.getInstance(document.getElementById('createOptionModal'))?.hide();
      App.showToast('Menu option created successfully!', 'success');
      await Auth.loadPermissions();
      await this.renderOptionsTab(document.getElementById('security-tab-content'));
    } catch (err) {
      App.showToast(`Failed to add option: ${err.message}`, 'danger');
    } finally {
      btn.disabled = false;
    }
  },

  openEditOptionModal(optionId, name, code, desc, sort) {
    const modalMount = document.getElementById('security-modals-mount');
    modalMount.innerHTML = `
      <div class="modal fade" id="editOptionModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered modal-dialog-scrollable">
          <div class="modal-content border-0 shadow">
            <div class="modal-header">
              <h5 class="modal-title fw-bold"><i class="bi bi-pencil-square text-primary me-2"></i>Edit Option: ${code}</h5>
              <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <form id="edit-option-form" onsubmit="SecurityComponent.submitEditOption(event, ${optionId})">
              <div class="modal-body">
                <div class="row g-3 mb-3">
                  <div class="col-12 col-md-6">
                    <label class="form-label" for="edit-opt-code">Action Code <span class="required-asterisk">*</span></label>
                    <input type="text" id="edit-opt-code" class="form-control text-uppercase" value="${code}" required>
                  </div>
                  <div class="col-12 col-md-6">
                    <label class="form-label" for="edit-opt-name">Display Name <span class="required-asterisk">*</span></label>
                    <input type="text" id="edit-opt-name" class="form-control" value="${name}" required>
                  </div>
                </div>
                <div class="mb-3">
                  <label class="form-label" for="edit-opt-desc">Description</label>
                  <input type="text" id="edit-opt-desc" class="form-control" value="${desc || ''}">
                </div>
                <div class="mb-3">
                  <label class="form-label" for="edit-opt-sort">Sort Order</label>
                  <input type="number" id="edit-opt-sort" class="form-control" value="${sort}" min="0">
                </div>
              </div>
              <div class="modal-footer d-flex flex-column-reverse flex-sm-row justify-content-end gap-2">
                <button type="button" class="btn btn-erp-secondary btn-sm w-100 w-sm-auto" data-bs-dismiss="modal">Cancel</button>
                <button type="submit" class="btn btn-erp-primary btn-sm w-100 w-sm-auto" id="btn-submit-edit-opt">Save Option</button>
              </div>
            </form>
          </div>
        </div>
      </div>
    `;

    const modal = new bootstrap.Modal(document.getElementById('editOptionModal'));
    modal.show();
  },

  async submitEditOption(e, optionId) {
    e.preventDefault();
    const btn = document.getElementById('btn-submit-edit-opt');
    btn.disabled = true;

    const payload = {
      name: document.getElementById('edit-opt-name').value.trim(),
      code: document.getElementById('edit-opt-code').value.trim().toUpperCase(),
      description: document.getElementById('edit-opt-desc').value.trim() || null,
      sortOrder: parseInt(document.getElementById('edit-opt-sort').value, 10)
    };

    try {
      await Api.put(`/Menus/options/${optionId}`, payload);
      bootstrap.Modal.getInstance(document.getElementById('editOptionModal'))?.hide();
      App.showToast('Menu option updated successfully!', 'success');
      await Auth.loadPermissions();
      await this.renderOptionsTab(document.getElementById('security-tab-content'));
    } catch (err) {
      App.showToast(`Failed to update option: ${err.message}`, 'danger');
    } finally {
      btn.disabled = false;
    }
  },

  confirmDeleteOption(optionId, code) {
    App.confirmAction(
      'Delete Option',
      `Are you sure you want to delete option "${code}"? Any active role permissions mapped to this option will be revoked.`,
      async () => {
        try {
          await Api.delete(`/Menus/options/${optionId}`);
          App.showToast(`Option "${code}" deleted.`, 'success');
          await Auth.loadPermissions();
          await SecurityComponent.renderOptionsTab(document.getElementById('security-tab-content'));
        } catch (err) {
          App.showToast(`Failed to delete option: ${err.message}`, 'danger');
        }
      },
      'Delete'
    );
  },

  // ================= 4. ROLE PERMISSIONS MATRIX TAB =================
  async renderMatrixTab(mount) {
    this.matrixData = await Api.get(`/RolePermissions?role=${this.selectedRole}`);
    const matrix = this.matrixData;

    let rowsHtml = '';
    (matrix.menus || []).forEach(m => {
      const indent = m.parentMenuId ? `<span class="text-muted me-2" style="margin-left: 20px;">↳</span>` : '';
      
      let optionsCheckboxesHtml = '';
      if (m.options && m.options.length > 0) {
        optionsCheckboxesHtml = m.options.map(opt => `
          <div class="form-check form-check-inline me-3">
            <input class="form-check-input matrix-perm-cb" type="checkbox" 
              id="perm-${m.menuId}-${opt.menuOptionId}"
              data-menu-id="${m.menuId}"
              data-option-id="${opt.menuOptionId}"
              ${opt.isGranted ? 'checked' : ''}
              ${this.selectedRole === 'Admin' ? 'disabled title="Admin always has full rights"' : ''}>
            <label class="form-check-label small fw-semibold" for="perm-${m.menuId}-${opt.menuOptionId}">
              ${opt.optionCode}
            </label>
          </div>
        `).join('');
      } else {
        optionsCheckboxesHtml = `<span class="text-muted small italic">No options defined</span>`;
      }

      rowsHtml += `
        <tr>
          <td style="width: 35%;">
            <div class="d-flex align-items-center">
              ${indent}
              <div>
                <strong class="text-dark">${m.menuTitle}</strong>
                ${m.menuRoute ? `<span class="badge bg-light text-muted border ms-2">#${m.menuRoute}</span>` : ''}
              </div>
            </div>
          </td>
          <td style="width: 50%;">
            <div class="d-flex flex-wrap align-items-center">
              ${optionsCheckboxesHtml}
            </div>
          </td>
          <td class="text-end" style="width: 15%;">
            ${this.selectedRole !== 'Admin' && m.options?.length > 0 ? `
              <button type="button" class="btn btn-link btn-sm text-decoration-none p-0 me-2 small" onclick="SecurityComponent.toggleMenuRowPerms(${m.menuId}, true)">All</button>
              <button type="button" class="btn btn-link btn-sm text-decoration-none p-0 text-secondary small" onclick="SecurityComponent.toggleMenuRowPerms(${m.menuId}, false)">None</button>
            ` : ''}
          </td>
        </tr>
      `;
    });

    mount.innerHTML = `
      <div class="card border-0 shadow-sm">
        <div class="card-header bg-white py-3 border-bottom d-flex flex-column flex-md-row justify-content-between align-items-start align-items-md-center gap-3">
          <div class="d-flex flex-column flex-sm-row align-items-start align-items-sm-center gap-3 w-100 w-md-auto">
            <h5 class="card-title mb-0 fw-bold"><i class="bi bi-person-check-fill text-primary me-2"></i>Access Control Matrix</h5>
            <div class="btn-group flex-wrap" role="group">
              <button type="button" class="btn btn-sm ${this.selectedRole === 'Admin' ? 'btn-danger' : 'btn-outline-secondary'}" onclick="SecurityComponent.changeMatrixRole('Admin')">
                <i class="bi bi-shield-lock-fill me-1"></i>Admin
              </button>
              <button type="button" class="btn btn-sm ${this.selectedRole === 'Employee' ? 'btn-primary' : 'btn-outline-secondary'}" onclick="SecurityComponent.changeMatrixRole('Employee')">
                <i class="bi bi-people-fill me-1"></i>Employee
              </button>
              <button type="button" class="btn btn-sm ${this.selectedRole === 'Customer' ? 'btn-success' : 'btn-outline-secondary'}" onclick="SecurityComponent.changeMatrixRole('Customer')">
                <i class="bi bi-building me-1"></i>Customer
              </button>
            </div>
          </div>

          <div class="d-flex flex-wrap gap-2 w-100 w-md-auto justify-content-start justify-content-md-end">
            ${this.selectedRole !== 'Admin' ? `
              <button type="button" class="btn btn-outline-secondary btn-sm" onclick="SecurityComponent.toggleAllMatrixPerms(true)">
                <i class="bi bi-check-all me-1"></i>Grant All
              </button>
              <button type="button" class="btn btn-outline-secondary btn-sm" onclick="SecurityComponent.toggleAllMatrixPerms(false)">
                <i class="bi bi-slash-circle me-1"></i>Revoke All
              </button>
              <button type="button" class="btn btn-erp-primary btn-sm" id="btn-save-matrix" onclick="SecurityComponent.saveMatrixPermissions()">
                <i class="bi bi-save me-1"></i>Save Role Permissions
              </button>
            ` : `
              <span class="badge bg-danger-subtle text-danger p-2 small">
                <i class="bi bi-info-circle me-1"></i>Administrators possess unrestricted full system access.
              </span>
            `}
          </div>
        </div>

        <div class="table-responsive erp-table-wrapper">
          <table class="table table-hover align-middle mb-0" style="min-width: 760px;">
            <thead class="table-light">
              <tr>
                <th>Menu Module</th>
                <th>Permitted Operations (View, Add, Edit, Delete, etc.)</th>
                <th class="text-end">Quick Row Toggle</th>
              </tr>
            </thead>
            <tbody>
              ${rowsHtml || `<tr><td colspan="3" class="text-center py-4 text-muted">No permissions found.</td></tr>`}
            </tbody>
          </table>
        </div>
      </div>
    `;
  },

  async changeMatrixRole(role) {
    this.selectedRole = role;
    await this.renderMatrixTab(document.getElementById('security-tab-content'));
  },

  toggleMenuRowPerms(menuId, state) {
    document.querySelectorAll(`.matrix-perm-cb[data-menu-id="${menuId}"]`).forEach(cb => {
      cb.checked = state;
    });
  },

  toggleAllMatrixPerms(state) {
    document.querySelectorAll('.matrix-perm-cb').forEach(cb => {
      cb.checked = state;
    });
  },

  async saveMatrixPermissions() {
    const btn = document.getElementById('btn-save-matrix');
    if (btn) btn.disabled = true;

    const checkboxes = document.querySelectorAll('.matrix-perm-cb');
    const permissions = [];

    checkboxes.forEach(cb => {
      permissions.push({
        menuId: parseInt(cb.dataset.menuId, 10),
        menuOptionId: parseInt(cb.dataset.optionId, 10),
        isGranted: cb.checked
      });
    });

    const payload = {
      role: this.selectedRole,
      permissions
    };

    try {
      await Api.post('/RolePermissions/batch-update', payload);
      App.showToast(`Permissions for ${this.selectedRole} saved successfully!`, 'success');
      // Reload current session permissions so changes are immediately felt
      await Auth.loadPermissions();
      Auth.renderSidebarMenu();
      await this.renderMatrixTab(document.getElementById('security-tab-content'));
    } catch (err) {
      App.showToast(`Failed to save permissions: ${err.message}`, 'danger');
    } finally {
      if (btn) btn.disabled = false;
    }
  }
};
