// Profile Component
const ProfileComponent = {
  async render(container) {
    container.innerHTML = `
      <div class="loading-spinner-container">
        <div class="spinner-border text-primary mb-2"></div>
        <p>Loading profile details...</p>
      </div>
    `;

    try {
      const user = await Api.get('/auth/profile');

      container.innerHTML = `
        <div class="page-header-container">
          <div>
            <h1 class="page-title">My Profile & Account Security</h1>
            <p class="page-subtitle">Manage personal information and update authentication credentials</p>
          </div>
        </div>

        <div class="row g-4">
          <!-- Profile Card -->
          <div class="col-lg-5">
            <div class="erp-card">
              <div class="erp-card-header">
                <h6 class="erp-card-title"><i class="bi bi-person-circle me-2"></i>Account Information</h6>
              </div>
              <div class="erp-card-body text-center p-4">
                <div class="user-avatar-circle mx-auto mb-3" style="width: 72px; height: 72px; font-size: 1.75rem;">
                  ${(user.fullName || user.username).slice(0, 2).toUpperCase()}
                </div>
                <h4 class="fw-bold mb-1">${user.fullName || 'ERP User'}</h4>
                <div class="text-muted small mb-3">${user.username}</div>
                <div class="mb-4">
                  <span class="badge bg-primary px-3 py-2 text-uppercase letter-spacing-1">${user.role}</span>
                </div>

                <div class="border-top pt-3 text-start small">
                  <div class="d-flex justify-content-between py-2 border-bottom">
                    <span class="text-muted">User ID:</span>
                    <span class="fw-semibold">#${user.userId}</span>
                  </div>
                  <div class="d-flex justify-content-between py-2 border-bottom">
                    <span class="text-muted">Account Status:</span>
                    <span class="badge-status badge-active">${user.status}</span>
                  </div>
                  ${user.employeeId ? `
                    <div class="d-flex justify-content-between py-2 border-bottom">
                      <span class="text-muted">Employee ID:</span>
                      <span class="fw-semibold">#${user.employeeId}</span>
                    </div>
                  ` : ''}
                  ${user.customerId ? `
                    <div class="d-flex justify-content-between py-2 border-bottom">
                      <span class="text-muted">Customer ID:</span>
                      <span class="fw-semibold">#${user.customerId}</span>
                    </div>
                  ` : ''}
                  ${user.passwordChangedAt ? `
                    <div class="d-flex justify-content-between py-2 border-bottom">
                      <span class="text-muted">Password Last Changed:</span>
                      <span class="fw-semibold">${App.formatDateTime(user.passwordChangedAt)}</span>
                    </div>
                  ` : ''}
                </div>
              </div>
            </div>
          </div>

          <!-- Change Password Card -->
          <div class="col-lg-7">
            <div class="erp-card">
              <div class="erp-card-header">
                <h6 class="erp-card-title"><i class="bi bi-shield-lock me-2"></i>Change Security Password</h6>
              </div>
              <div class="erp-card-body p-4">
                <form id="change-password-form" onsubmit="ProfileComponent.submitPasswordChange(event)">
                  <div class="mb-3">
                    <label class="form-label" for="cp-current">Current Password <span class="required-asterisk">*</span></label>
                    <div class="input-group">
                      <span class="input-group-text bg-light"><i class="bi bi-lock"></i></span>
                      <input type="password" id="cp-current" class="form-control" placeholder="••••••••" required autocomplete="current-password">
                      <button class="btn btn-outline-secondary btn-toggle-password" type="button" onclick="App.togglePasswordVisibility('cp-current', this)" title="Show password" aria-label="Show password">
                        <i class="bi bi-eye"></i>
                      </button>
                    </div>
                  </div>

                  <div class="row mb-3">
                    <div class="col-md-6 mb-3 mb-md-0">
                      <label class="form-label" for="cp-new">New Password <span class="required-asterisk">*</span></label>
                      <div class="input-group">
                        <span class="input-group-text bg-light"><i class="bi bi-lock"></i></span>
                        <input type="password" id="cp-new" class="form-control" placeholder="••••••••" required autocomplete="new-password">
                        <button class="btn btn-outline-secondary btn-toggle-password" type="button" onclick="App.togglePasswordVisibility('cp-new', this)" title="Show password" aria-label="Show password">
                          <i class="bi bi-eye"></i>
                        </button>
                      </div>
                    </div>
                    <div class="col-md-6">
                      <label class="form-label" for="cp-confirm">Confirm New Password <span class="required-asterisk">*</span></label>
                      <div class="input-group">
                        <span class="input-group-text bg-light"><i class="bi bi-lock"></i></span>
                        <input type="password" id="cp-confirm" class="form-control" placeholder="••••••••" required autocomplete="new-password">
                        <button class="btn btn-outline-secondary btn-toggle-password" type="button" onclick="App.togglePasswordVisibility('cp-confirm', this)" title="Show password" aria-label="Show password">
                          <i class="bi bi-eye"></i>
                        </button>
                      </div>
                    </div>
                  </div>

                  <div id="cp-password-checklist" class="mb-3"></div>

                  <div class="p-3 bg-light rounded border text-muted small mb-4">
                    <i class="bi bi-info-circle me-1 text-primary"></i>Passwords are cryptographically salted and hashed using BCrypt before storing into PostgreSQL database. Past passwords cannot be reused.
                  </div>

                  <div class="d-flex justify-content-end">
                    <button type="submit" class="btn btn-erp-primary" id="btn-change-pwd">
                      <i class="bi bi-key me-1"></i>Update Password
                    </button>
                  </div>
                </form>
              </div>
            </div>
          </div>
        </div>
      `;

      await Auth.renderPasswordChecklist('cp-new', 'cp-password-checklist');
    } catch (error) {
      App.showToast(error.message, 'danger');
    }
  },

  async submitPasswordChange(event) {
    event.preventDefault();
    const btn = document.getElementById('btn-change-pwd');
    btn.disabled = true;

    const currentPassword = document.getElementById('cp-current').value;
    const newPassword = document.getElementById('cp-new').value;
    const confirmPassword = document.getElementById('cp-confirm').value;

    if (newPassword !== confirmPassword) {
      App.showToast('New passwords do not match.', 'warning');
      btn.disabled = false;
      return;
    }

    try {
      await Api.post('/auth/change-password', { currentPassword, newPassword });
      App.showToast('Password changed successfully.', 'success');
      document.getElementById('change-password-form').reset();
      await Auth.renderPasswordChecklist('cp-new', 'cp-password-checklist');
    } catch (error) {
      App.showToast(error.message, 'danger');
    } finally {
      btn.disabled = false;
    }
  }
};
