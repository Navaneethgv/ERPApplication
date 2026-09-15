// Sales Component
const SalesComponent = {
  data: [],
  filteredData: [],
  currentPage: 1,
  pageSize: 10,
  customersCache: [],
  productsCache: [],

  async render(container, action, id) {
    if (action === 'new') {
      if (!Auth.hasPermission('sales', 'ADD')) {
        App.showToast('Access restricted: You do not have permission to create sales orders.', 'warning');
        window.location.hash = '#sales';
        return;
      }
      await this.renderCreateForm(container);
    } else if (action === 'view' && id) {
      await this.renderDetailView(container, id);
    } else {
      await this.renderList(container);
    }
  },

  async renderList(container) {
    container.innerHTML = `
      <div class="loading-spinner-container">
        <div class="spinner-border text-primary mb-2"></div>
        <p>Loading sales orders...</p>
      </div>
    `;

    try {
      this.data = await Api.get('/sales');
      this.filteredData = [...this.data];
      this.currentPage = 1;
      this.renderTable(container);
    } catch (error) {
      container.innerHTML = `
        <div class="alert alert-danger">
          <h5><i class="bi bi-exclamation-circle me-2"></i>Failed to Load Sales Orders</h5>
          <p class="mb-0">${error.message}</p>
        </div>
      `;
    }
  },

  renderTable(container) {
    const isCustomer = Auth.isCustomer();
    const totalSales = this.data.filter(s => s.status !== 'Cancelled').reduce((sum, s) => sum + s.totalAmount, 0);
    const fulfilledCount = this.data.filter(s => s.status === 'Fulfilled').length;
    const pendingCount = this.data.filter(s => s.status === 'Confirmed' || s.status === 'Draft').length;

    const start = (this.currentPage - 1) * this.pageSize;
    const pageItems = this.filteredData.slice(start, start + this.pageSize);

    const canAdd = Auth.hasPermission('sales', 'ADD');

    container.innerHTML = `
      <div class="page-header-container d-flex flex-column flex-sm-row justify-content-between align-items-start align-items-sm-center gap-2 mb-3 mb-md-4">
        <div>
          <h1 class="page-title">${isCustomer ? 'My Sales Orders' : 'Sales Orders & Fulfillment'}</h1>
          <p class="page-subtitle">${isCustomer ? 'Track your placed orders and fulfillment status' : 'Manage customer orders, inventory allocation, and automatic invoicing'}</p>
        </div>
        <div>
          ${canAdd ? `
            <a href="#sales/new" class="btn btn-erp-primary btn-sm">
              <i class="bi bi-cart-plus me-1"></i>${isCustomer ? 'Place New Order' : 'Create Sales Order'}
            </a>
          ` : ''}
        </div>
      </div>

      <!-- KPI Summary -->
      <div class="row g-3 mb-4">
        <div class="col-12 col-sm-6 col-xl-4">
          <div class="kpi-card h-100">
            <div class="kpi-header">
              <span class="kpi-title">${isCustomer ? 'Total Order Value' : 'Total Sales Volume'}</span>
              <div class="kpi-icon-box icon-green"><i class="bi bi-cash-stack"></i></div>
            </div>
            <div class="kpi-value text-success">${App.formatCurrency(totalSales)}</div>
            <div class="kpi-subtitle text-muted">All active sales orders</div>
          </div>
        </div>

        <div class="col-12 col-sm-6 col-xl-4">
          <div class="kpi-card h-100">
            <div class="kpi-header">
              <span class="kpi-title">Fulfilled & Invoiced</span>
              <div class="kpi-icon-box icon-blue"><i class="bi bi-check2-circle"></i></div>
            </div>
            <div class="kpi-value text-primary">${fulfilledCount}</div>
            <div class="kpi-subtitle text-muted">Dispatched orders</div>
          </div>
        </div>

        <div class="col-12 col-sm-6 col-xl-4">
          <div class="kpi-card h-100">
            <div class="kpi-header">
              <span class="kpi-title">Pending Orders</span>
              <div class="kpi-icon-box icon-amber"><i class="bi bi-hourglass-split"></i></div>
            </div>
            <div class="kpi-value text-warning">${pendingCount}</div>
            <div class="kpi-subtitle text-muted">Processing orders</div>
          </div>
        </div>
      </div>

      <!-- Filter Row -->
      <div class="erp-card mb-3">
        <div class="erp-card-body p-3">
          <div class="row g-2 align-items-center">
            <div class="col-12 col-md-5 col-lg-4">
              <div class="input-group input-group-sm">
                <span class="input-group-text bg-light"><i class="bi bi-search"></i></span>
                <input type="text" id="so-search" class="form-control" placeholder="Search by order #, customer, items..." oninput="SalesComponent.filterList()">
              </div>
            </div>
            <div class="col-12 col-sm-6 col-md-3">
              <select id="so-status-filter" class="form-select form-select-sm" onchange="SalesComponent.filterList()">
                <option value="">All Statuses</option>
                <option value="Confirmed">Confirmed</option>
                <option value="Fulfilled">Fulfilled</option>
                <option value="Draft">Draft</option>
                <option value="Cancelled">Cancelled</option>
              </select>
            </div>
            <div class="col-12 col-sm-auto ms-sm-auto text-muted small mt-1 mt-sm-0">
              Total: <strong id="so-total-count">${this.filteredData.length}</strong> orders
            </div>
          </div>
        </div>
      </div>

      <!-- Table -->
      <div class="erp-card">
        <div class="erp-table-wrapper table-responsive">
          <table class="erp-table" id="sales-table">
            <thead>
              <tr>
                <th class="text-nowrap" style="min-width: 130px;">Order Number</th>
                ${!isCustomer ? '<th style="min-width: 170px;">Customer</th>' : ''}
                <th class="text-nowrap" style="min-width: 110px;">Order Date</th>
                <th class="text-nowrap" style="min-width: 100px;">Line Items</th>
                <th class="text-nowrap" style="min-width: 120px;">Total Amount</th>
                <th class="text-nowrap" style="min-width: 90px;">Status</th>
                <th class="text-end text-nowrap" style="min-width: 100px;">Actions</th>
              </tr>
            </thead>
            <tbody id="sales-table-body">
              ${this.buildTableRows(pageItems)}
            </tbody>
          </table>
        </div>
        <div id="sales-pagination">
          ${App.renderPagination({
            currentPage: this.currentPage,
            pageSize: this.pageSize,
            totalItems: this.filteredData.length,
            componentName: 'SalesComponent'
          })}
        </div>
      </div>
    `;
  },

  buildTableRows(list) {
    if (!list || list.length === 0) {
      return `<tr><td colspan="7" class="text-center text-muted py-4"><i class="bi bi-cart fs-2 d-block mb-2 text-muted"></i>No sales orders found.</td></tr>`;
    }

    const isCustomer = Auth.isCustomer();

    return list.map(s => `
      <tr>
        <td class="fw-semibold font-monospace">
          <a href="#sales/view/${s.saleId}">${s.saleOrderNumber}</a>
        </td>
        ${!isCustomer ? `<td><div class="fw-semibold text-dark">${s.customerName}</div></td>` : ''}
        <td class="text-muted small">${App.formatDate(s.orderDate)}</td>
        <td><span class="badge bg-light text-dark border">${s.items?.length || 0} items</span></td>
        <td class="fw-bold text-dark">${App.formatCurrency(s.totalAmount)}</td>
        <td><span class="badge-status badge-${s.status.toLowerCase()}">${s.status}</span></td>
        <td class="text-end text-nowrap">
          <div class="table-actions">
            <a href="#sales/view/${s.saleId}" class="btn btn-outline-primary btn-sm" title="View Order">
              <i class="bi bi-eye"></i> View
            </a>
          </div>
        </td>
      </tr>
    `).join('');
  },

  updateTableView() {
    const start = (this.currentPage - 1) * this.pageSize;
    const pageItems = this.filteredData.slice(start, start + this.pageSize);
    const tbody = document.getElementById('sales-table-body');
    if (tbody) {
      tbody.innerHTML = this.buildTableRows(pageItems);
    }
    const pagContainer = document.getElementById('sales-pagination');
    if (pagContainer) {
      pagContainer.innerHTML = App.renderPagination({
        currentPage: this.currentPage,
        pageSize: this.pageSize,
        totalItems: this.filteredData.length,
        componentName: 'SalesComponent'
      });
    }
    const totalCount = document.getElementById('so-total-count');
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
    const q = (document.getElementById('so-search')?.value || '').toLowerCase();
    const status = document.getElementById('so-status-filter')?.value || '';

    this.filteredData = this.data.filter(s => {
      const matchQ = !q || s.saleOrderNumber.toLowerCase().includes(q) || s.customerName.toLowerCase().includes(q) || (s.notes && s.notes.toLowerCase().includes(q));
      const matchStatus = !status || s.status === status;
      return matchQ && matchStatus;
    });

    this.currentPage = 1;
    this.updateTableView();
  },

  async renderCreateForm(container) {
    container.innerHTML = `
      <div class="loading-spinner-container">
        <div class="spinner-border text-primary mb-2"></div>
        <p>Preparing Sales Order form and validating stock...</p>
      </div>
    `;

    try {
      const isStaff = Auth.isAdmin() || Auth.isEmployee();
      const [customers, products] = await Promise.all([
        isStaff ? Api.get('/customers') : Promise.resolve([]),
        Api.get('/products')
      ]);

      this.customersCache = customers.filter(c => c.status === 'Active');
      this.productsCache = products.filter(p => p.status === 'Active');

      const today = new Date().toISOString().slice(0, 10);

      container.innerHTML = `
        <div class="page-header-container d-flex flex-column flex-sm-row justify-content-between align-items-start align-items-sm-center gap-2 mb-3 mb-md-4">
          <div>
            <h1 class="page-title">${isStaff ? 'Create Sales Order' : 'Place New Order'}</h1>
            <p class="page-subtitle">Configure customer order items with real-time stock verification</p>
          </div>
          <div>
            <a href="#sales" class="btn btn-erp-secondary btn-sm">
              <i class="bi bi-arrow-left me-1"></i>Back to Orders
            </a>
          </div>
        </div>

        <form id="create-so-form" onsubmit="SalesComponent.submitCreate(event)">
          <div class="erp-card mb-4">
            <div class="erp-card-body p-3 p-sm-4">
              <div class="form-section-title"><i class="bi bi-file-earmark-person me-2"></i>Order & Customer Details</div>

              <div class="row g-3 mb-3">
                ${isStaff ? `
                  <div class="col-12 col-md-6">
                    <label class="form-label" for="so-customer">Customer Account <span class="required-asterisk">*</span></label>
                    <select id="so-customer" class="form-select" required>
                      <option value="">Select Customer</option>
                      ${this.customersCache.map(c => `
                        <option value="${c.customerId}">${c.name} (${c.company || 'Direct'}) - Balance: ₹${c.currentBalance.toFixed(2)}</option>
                      `).join('')}
                    </select>
                  </div>
                ` : ''}
                <div class="${isStaff ? 'col-12 col-md-6' : 'col-12 col-md-6'}">
                  <label class="form-label" for="so-date">Order Date <span class="required-asterisk">*</span></label>
                  <input type="date" id="so-date" class="form-control" value="${today}" required>
                </div>
              </div>

              <div class="row g-3 mb-2">
                <div class="col-12">
                  <label class="form-label" for="so-notes">Order Notes / Instructions</label>
                  <input type="text" id="so-notes" class="form-control" placeholder="Purchase order reference, shipping remarks...">
                </div>
              </div>
            </div>
          </div>

          <!-- Dynamic Item Lines -->
          <div class="erp-card mb-4">
            <div class="erp-card-header d-flex flex-wrap justify-content-between align-items-center gap-2">
              <h6 class="erp-card-title mb-0"><i class="bi bi-cart-check me-2"></i>Order Items & Inventory Availability</h6>
              <button type="button" class="btn btn-outline-primary btn-sm" onclick="SalesComponent.addItemRow()">
                <i class="bi bi-plus-lg me-1"></i>Add Item Line
              </button>
            </div>
            <div class="erp-card-body p-0">
              <div class="erp-table-wrapper table-responsive">
                <table class="erp-table mb-0" id="so-items-table" style="min-width: 620px;">
                  <thead>
                    <tr>
                      <th style="min-width: 240px;">Product <span class="required-asterisk">*</span></th>
                      <th style="min-width: 100px;">Quantity <span class="required-asterisk">*</span></th>
                      <th style="min-width: 130px;">Unit Price (₹)</th>
                      <th style="min-width: 110px;">Line Total</th>
                      <th style="min-width: 60px;" class="text-center">Remove</th>
                    </tr>
                  </thead>
                  <tbody id="so-items-tbody">
                    <!-- Dynamic rows -->
                  </tbody>
                  <tfoot>
                    <tr class="table-light">
                      <td colspan="3" class="text-end fw-bold">Total Sales Order Amount:</td>
                      <td colspan="2" class="fw-bold text-success fs-6" id="so-total-display">₹0.00</td>
                    </tr>
                  </tfoot>
                </table>
              </div>
            </div>
          </div>

          <div class="d-flex flex-column-reverse flex-sm-row justify-content-end gap-2">
            <a href="#sales" class="btn btn-erp-secondary">Cancel</a>
            <button type="submit" class="btn btn-erp-primary" id="btn-save-so">
              <i class="bi bi-check2-circle me-1"></i>Confirm & Submit Order
            </button>
          </div>
        </form>
      `;

      this.addItemRow();
    } catch (error) {
      App.showToast(error.message, 'danger');
      window.location.hash = '#sales';
    }
  },

  addItemRow() {
    const tbody = document.getElementById('so-items-tbody');
    if (!tbody) return;

    const rowId = `so-row-${Date.now()}-${Math.floor(Math.random() * 1000)}`;
    const tr = document.createElement('tr');
    tr.id = rowId;
    tr.className = 'so-item-row';

    tr.innerHTML = `
      <td>
        <select class="form-select form-select-sm so-prod-select" required onchange="SalesComponent.onProductChange('${rowId}')">
          <option value="">Select Product...</option>
          ${this.productsCache.map(p => `
            <option value="${p.productId}" data-price="${p.unitPrice}" data-stock="${p.availableQuantity}" ${p.availableQuantity <= 0 ? 'disabled' : ''}>
              ${p.sku} - ${p.name} (₹${p.unitPrice.toFixed(2)}) - [${p.availableQuantity} available]
            </option>
          `).join('')}
        </select>
        <div class="small text-muted so-stock-hint mt-1"></div>
      </td>
      <td>
        <input type="number" min="1" class="form-control form-control-sm so-qty-input" value="1" required oninput="SalesComponent.calcRowTotal('${rowId}')">
      </td>
      <td>
        <input type="number" step="0.01" min="0.01" class="form-control form-control-sm so-price-input" readonly>
      </td>
      <td class="fw-bold text-dark so-line-total">
        ₹0.00
      </td>
      <td class="text-center">
        <button type="button" class="btn btn-outline-danger btn-sm py-0 px-2" onclick="SalesComponent.removeItemRow('${rowId}')">
          <i class="bi bi-trash"></i>
        </button>
      </td>
    `;

    tbody.appendChild(tr);
  },

  removeItemRow(rowId) {
    const tbody = document.getElementById('so-items-tbody');
    if (tbody.querySelectorAll('tr').length <= 1) {
      App.showToast('Sales order must have at least one line item.', 'warning');
      return;
    }
    const row = document.getElementById(rowId);
    if (row) row.remove();
    this.calcGrandTotal();
  },

  onProductChange(rowId) {
    const row = document.getElementById(rowId);
    if (!row) return;

    const select = row.querySelector('.so-prod-select');
    const priceInput = row.querySelector('.so-price-input');
    const hint = row.querySelector('.so-stock-hint');
    const qtyInput = row.querySelector('.so-qty-input');
    const selectedOption = select.options[select.selectedIndex];

    const price = selectedOption?.dataset?.price || '0';
    const stock = parseInt(selectedOption?.dataset?.stock || '0', 10);

    priceInput.value = parseFloat(price).toFixed(2);
    qtyInput.max = stock;

    if (stock <= 5 && stock > 0) {
      hint.innerHTML = `<span class="text-warning"><i class="bi bi-exclamation-triangle me-1"></i>Only ${stock} units remaining in warehouse!</span>`;
    } else {
      hint.innerHTML = `<span class="text-success"><i class="bi bi-check me-1"></i>${stock} units available in warehouse</span>`;
    }

    this.calcRowTotal(rowId);
  },

  calcRowTotal(rowId) {
    const row = document.getElementById(rowId);
    if (!row) return;

    const qty = parseInt(row.querySelector('.so-qty-input')?.value, 10) || 0;
    const price = parseFloat(row.querySelector('.so-price-input')?.value) || 0;
    const lineTotal = qty * price;

    row.querySelector('.so-line-total').textContent = App.formatCurrency(lineTotal);
    this.calcGrandTotal();
  },

  calcGrandTotal() {
    let grandTotal = 0;
    document.querySelectorAll('.so-item-row').forEach(row => {
      const qty = parseInt(row.querySelector('.so-qty-input')?.value, 10) || 0;
      const price = parseFloat(row.querySelector('.so-price-input')?.value) || 0;
      grandTotal += qty * price;
    });

    const display = document.getElementById('so-total-display');
    if (display) {
      display.textContent = App.formatCurrency(grandTotal);
    }
  },

  async submitCreate(event) {
    event.preventDefault();
    const btn = document.getElementById('btn-save-so');
    btn.disabled = true;

    const items = [];
    const rows = document.querySelectorAll('.so-item-row');
    for (const row of rows) {
      const select = row.querySelector('.so-prod-select');
      const prodId = parseInt(select.value, 10);
      const qty = parseInt(row.querySelector('.so-qty-input')?.value, 10);
      const price = parseFloat(row.querySelector('.so-price-input')?.value);
      const stock = parseInt(select.options[select.selectedIndex]?.dataset?.stock || '0', 10);

      if (!prodId || isNaN(prodId)) {
        App.showToast('Please select a product for all item rows.', 'warning');
        btn.disabled = false;
        return;
      }

      if (qty > stock) {
        App.showToast(`Insufficient stock for selected product! Available: ${stock}, Requested: ${qty}`, 'danger');
        btn.disabled = false;
        return;
      }

      items.push({ productId: prodId, quantity: qty, unitPrice: price });
    }

    const custSelect = document.getElementById('so-customer');
    const payload = {
      customerId: custSelect ? parseInt(custSelect.value, 10) : null,
      orderDate: document.getElementById('so-date').value,
      notes: document.getElementById('so-notes').value.trim(),
      items: items
    };

    try {
      const created = await Api.post('/sales', payload);
      App.showToast(`Sales order ${created.saleOrderNumber} confirmed! Invoice generated automatically.`, 'success');
      window.location.hash = `#sales/view/${created.saleId}`;
    } catch (error) {
      App.showToast(error.message, 'danger');
    } finally {
      btn.disabled = false;
    }
  },

  async renderDetailView(container, id) {
    container.innerHTML = `
      <div class="loading-spinner-container">
        <div class="spinner-border text-primary mb-2"></div>
        <p>Loading sales order #${id}...</p>
      </div>
    `;

    try {
      const so = await Api.get(`/sales/${id}`);
      container.innerHTML = `
        <div class="page-header-container d-flex flex-column flex-sm-row justify-content-between align-items-start align-items-sm-center gap-2 mb-3 mb-md-4">
          <div>
            <h1 class="page-title">Sales Order ${so.saleOrderNumber}</h1>
            <p class="page-subtitle">Placed by ${so.customerName} on ${App.formatDate(so.orderDate)}</p>
          </div>
          <div class="d-flex flex-wrap gap-2">
            <a href="#sales" class="btn btn-erp-secondary btn-sm">
              <i class="bi bi-arrow-left me-1"></i>Back
            </a>
            <a href="#invoices" class="btn btn-outline-primary btn-sm">
              <i class="bi bi-receipt me-1"></i>View Invoices
            </a>
          </div>
        </div>

        <div class="row g-3 mb-4">
          <div class="col-12 col-lg-8">
            <div class="erp-card mb-3">
              <div class="erp-card-header d-flex justify-content-between align-items-center">
                <h6 class="erp-card-title mb-0"><i class="bi bi-cart-check me-2"></i>Ordered Line Items</h6>
                <span class="badge-status badge-${so.status.toLowerCase()}">${so.status}</span>
              </div>
              <div class="erp-table-wrapper table-responsive">
                <table class="erp-table table-compact">
                  <thead>
                    <tr>
                      <th class="text-nowrap" style="min-width: 120px;">SKU</th>
                      <th style="min-width: 170px;">Product Name</th>
                      <th class="text-center text-nowrap" style="min-width: 80px;">Quantity</th>
                      <th class="text-end text-nowrap" style="min-width: 110px;">Unit Price</th>
                      <th class="text-end text-nowrap" style="min-width: 110px;">Line Total</th>
                    </tr>
                  </thead>
                  <tbody>
                    ${so.items.map(item => `
                      <tr>
                        <td class="fw-semibold text-primary font-monospace">${item.sku}</td>
                        <td>${item.productName}</td>
                        <td class="fw-bold text-center">${item.quantity}</td>
                        <td class="text-end">${App.formatCurrency(item.unitPrice)}</td>
                        <td class="text-end fw-bold">${App.formatCurrency(item.totalPrice)}</td>
                      </tr>
                    `).join('')}
                  </tbody>
                  <tfoot>
                    <tr class="table-light">
                      <td colspan="4" class="text-end fw-bold">Grand Total:</td>
                      <td class="text-end fw-bold text-success fs-6">${App.formatCurrency(so.totalAmount)}</td>
                    </tr>
                  </tfoot>
                </table>
              </div>
            </div>

            ${so.notes ? `
              <div class="erp-card mb-3 mb-lg-0">
                <div class="erp-card-body p-3">
                  <h6 class="fw-bold mb-1"><i class="bi bi-chat-left-text me-1 text-primary"></i>Customer Notes:</h6>
                  <p class="text-muted small mb-0">${so.notes}</p>
                </div>
              </div>
            ` : ''}
          </div>

          <div class="col-12 col-lg-4">
            <div class="erp-card">
              <div class="erp-card-header">
                <h6 class="erp-card-title mb-0"><i class="bi bi-person-lines-fill me-2"></i>Client Summary</h6>
              </div>
              <div class="erp-card-body">
                <div class="mb-3">
                  <div class="text-muted small">Customer</div>
                  <div class="fw-bold text-dark fs-6">${so.customerName}</div>
                </div>
                <div class="mb-3">
                  <div class="text-muted small">Order Placed Date</div>
                  <div class="fw-semibold">${App.formatDate(so.orderDate)}</div>
                </div>
                <div class="mb-0">
                  <div class="text-muted small">Fulfillment Status</div>
                  <div class="mt-1">
                    <span class="badge-status badge-${so.status.toLowerCase()}">${so.status}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      `;
    } catch (error) {
      App.showToast(error.message, 'danger');
      window.location.hash = '#sales';
    }
  }
};
