// Real-Time Order Notifications Component
const NotificationsComponent = {
  pollingInterval: null,
  lastUnreadCount: 0,
  isInitialized: false,

  init() {
    const isStaff = Auth.isAdmin() || Auth.isEmployee();
    const bellWrapper = document.getElementById('notification-bell-wrapper');

    if (!isStaff) {
      if (bellWrapper) bellWrapper.style.display = 'none';
      this.stop();
      return;
    }

    if (bellWrapper) bellWrapper.style.display = 'block';

    if (!this.isInitialized) {
      this.isInitialized = true;
      this.bindDropdownEvents();
    }

    // Initial fetch
    this.fetchUnreadCount();

    // Start background polling every 6 seconds
    this.startPolling();
  },

  startPolling() {
    this.stop();
    this.pollingInterval = setInterval(() => {
      if (Auth.isAuthenticated() && (Auth.isAdmin() || Auth.isEmployee())) {
        this.fetchUnreadCount();
      } else {
        this.stop();
      }
    }, 6000);
  },

  stop() {
    if (this.pollingInterval) {
      clearInterval(this.pollingInterval);
      this.pollingInterval = null;
    }
    this.lastUnreadCount = 0;
  },

  bindDropdownEvents() {
    const dropdownEl = document.getElementById('notificationDropdown');
    if (dropdownEl) {
      dropdownEl.addEventListener('show.bs.dropdown', () => {
        this.loadDropdown();
      });
    }
  },

  async fetchUnreadCount() {
    try {
      const data = await Api.get('/notifications/unread-count');
      const count = data.unreadCount || 0;
      this.updateBadge(count);

      // Trigger notification sound / toast if new order arrived
      if (count > this.lastUnreadCount && this.lastUnreadCount !== 0) {
        App.showToast(`New order placed! You have ${count} unread order notifications.`, 'info');
        this.animateBell();
      }

      this.lastUnreadCount = count;
    } catch {
      // Ignore background network polling errors gracefully
    }
  },

  updateBadge(count) {
    const badge = document.getElementById('notification-badge');
    if (!badge) return;

    if (count > 0) {
      badge.textContent = count > 99 ? '99+' : count;
      badge.classList.remove('d-none');
    } else {
      badge.textContent = '0';
      badge.classList.add('d-none');
    }
  },

  animateBell() {
    const bell = document.querySelector('#notificationDropdown i.bi-bell');
    if (bell) {
      bell.classList.add('bell-ring');
      setTimeout(() => bell.classList.remove('bell-ring'), 1000);
    }
  },

  async loadDropdown() {
    const container = document.getElementById('notification-items-list');
    if (!container) return;

    container.innerHTML = `
      <div class="p-3 text-center text-muted">
        <div class="spinner-border spinner-border-sm text-primary mb-1"></div>
        <div class="small">Checking for orders...</div>
      </div>
    `;

    try {
      const notifications = await Api.get('/notifications?limit=15');
      this.renderDropdownItems(notifications, container);
    } catch (error) {
      container.innerHTML = `
        <div class="p-3 text-center text-danger small">
          <i class="bi bi-exclamation-circle me-1"></i>Failed to load notifications
        </div>
      `;
    }
  },

  renderDropdownItems(notifications, container) {
    if (!notifications || notifications.length === 0) {
      container.innerHTML = `
        <div class="p-4 text-center text-muted">
          <i class="bi bi-bell-slash fs-3 d-block mb-2 text-secondary"></i>
          <div class="fw-semibold small">No notifications yet</div>
          <div class="text-muted small">New customer orders will appear here in real-time.</div>
        </div>
      `;
      return;
    }

    const unreadCount = notifications.filter(n => !n.IsRead && !n.isRead).length;
    const headerCount = document.getElementById('dropdown-unread-count');
    if (headerCount) {
      headerCount.textContent = unreadCount > 0 ? `${unreadCount} unread` : '';
      headerCount.style.display = unreadCount > 0 ? 'inline-block' : 'none';
    }

    container.innerHTML = notifications.map(n => {
      const isRead = n.isRead || n.IsRead;
      const orderId = n.orderId || n.OrderId || '';
      const orderNum = n.orderNumber || n.OrderNumber || 'Order';
      const custName = n.customerName || n.CustomerName || 'Customer';
      const amount = n.totalAmount || n.TotalAmount || 0;
      const status = n.orderStatus || n.OrderStatus || 'Confirmed';
      const notifId = n.notificationId || n.NotificationId;
      const dateStr = n.createdAt || n.CreatedAt;

      return `
        <div class="notification-item ${isRead ? 'read' : 'unread'}" onclick="NotificationsComponent.handleItemClick(${notifId}, ${orderId})">
          <div class="notification-item-icon">
            <i class="bi bi-cart-check-fill text-primary"></i>
          </div>
          <div class="notification-item-content">
            <div class="d-flex justify-content-between align-items-baseline mb-1">
              <span class="notification-customer text-truncate" title="${custName}">${custName}</span>
              <span class="notification-time text-muted small">${App.formatDateTime(dateStr)}</span>
            </div>
            <div class="d-flex justify-content-between align-items-center mb-1">
              <span class="font-monospace fw-semibold text-primary small">${orderNum}</span>
              <span class="fw-bold text-success small">${App.formatCurrency(amount)}</span>
            </div>
            <div class="d-flex justify-content-between align-items-center">
              <span class="badge-status badge-${status.toLowerCase()} py-0 px-2 small">${status}</span>
              ${!isRead ? '<span class="unread-dot" title="Unread"></span>' : ''}
            </div>
          </div>
        </div>
      `;
    }).join('');
  },

  async handleItemClick(notificationId, orderId) {
    try {
      await Api.put(`/notifications/${notificationId}/read`);
      this.fetchUnreadCount();
    } catch {
      // Ignore failure
    }

    // Close Bootstrap Dropdown
    const dropdownEl = document.getElementById('notificationDropdown');
    const dropdownInstance = bootstrap.Dropdown.getInstance(dropdownEl);
    if (dropdownInstance) {
      dropdownInstance.hide();
    }

    // Navigate to order details
    if (orderId) {
      window.location.hash = `#sales/view/${orderId}`;
    } else {
      window.location.hash = '#sales';
    }
  },

  async markAllAsRead() {
    try {
      await Api.put('/notifications/mark-all-read');
      this.updateBadge(0);
      this.lastUnreadCount = 0;
      App.showToast('All notifications marked as read.', 'success');
      this.loadDropdown();
    } catch (error) {
      App.showToast(error.message || 'Failed to mark all as read.', 'danger');
    }
  }
};
