// Purchases Component
const PurchasesComponent = {
  data: [],
  filteredData: [],
  currentPage: 1,
  pageSize: 10,
  suppliersCache: [],
  productsCache: [],

  async render(container, action, id) {
    if (action === 'new') {
      if (!Auth.hasPermission('purchases', 'ADD')) {
        App.showToast('Access restricted: You do not have permission to create purchase orders.', 'warning');
        window.location.hash = '#purchases';
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
        <p>Loading purchase orders...</p>
      </div>
    `;

    try {
      this.data = await Api.get('/purchases');
      this.filteredData = [...this.data];
      this.currentPage = 1;
      this.renderTable(container);
    } catch (error) {
      container.innerHTML = `
        <div class="alert alert-danger">
          <h5><i class="bi bi-exclamation-circle me-2"></i>Failed to Load Purchases</h5>
          <p class="mb-0">${error.message}</p>
        </div>
      `;
    }
  },

  renderTable(container) {
    const totalProcured = this.data.filter(p => p.status !== 'Cancelled').reduce((sum, p) => sum + p.totalAmount, 0);
    const receivedCount = this.data.filter(p => p.status === 'Received').length;
    const pendingCount = this.data.filter(p => p.status === 'Ordered' || p.status === 'Draft').length;

    const start = (this.currentPage - 1) * this.pageSize;
    const pageItems = this.filteredData.slice(start, start + this.pageSize);

    const canAdd = Auth.hasPermission('purchases', 'ADD');

    container.innerHTML = `
      <div class="page-header-container">
        <div>
          <h1 class="page-title">Purchase Orders & Procurement</h1>
          <p class="page-subtitle">Procurement lifecycle, vendor purchase orders, and receiving workflows</p>
        </div>
        <div>
          ${canAdd ? `
            <a href="#purchases/new" class="btn btn-erp-primary btn-sm">
              <i class="bi bi-plus-circle me-1"></i>Create Purchase Order
            </a>
          ` : ''}
        </div>
      </div>

      <!-- KPI Summary -->
      <div class="row g-3 mb-4">
        <div class="col-sm-6 col-xl-4">
          <div class="kpi-card">
            <div class="kpi-header">
              <span class="kpi-title">Total Procurement Spend</span>
              <div class="kpi-icon-box icon-amber"><i class="bi bi-cash-coin"></i></div>
            </div>
            <div class="kpi-value text-warning">${App.formatCurrency(totalProcured)}</div>
            <div class="kpi-subtitle text-muted">All active purchase orders</div>
          </div>
        </div>

        <div class="col-sm-6 col-xl-4">
          <div class="kpi-card">
            <div class="kpi-header">
              <span class="kpi-title">Received & In Stock</span>
              <div class="kpi-icon-box icon-green"><i class="bi bi-check2-all"></i></div>
            </div>
            <div class="kpi-value text-success">${receivedCount}</div>
            <div class="kpi-subtitle text-muted">Fulfilled shipments</div>
          </div>
        </div>

        <div class="col-sm-6 col-xl-4">
          <div class="kpi-card">
            <div class="kpi-header">
              <span class="kpi-title">Pending Delivery</span>
              <div class="kpi-icon-box icon-blue"><i class="bi bi-truck"></i></div>
            </div>
            <div class="kpi-value text-primary">${pendingCount}</div>
            <div class="kpi-subtitle text-muted">Awaiting supplier delivery</div>
          </div>
        </div>
      </div>

      <!-- Filter Row -->
      <div class="erp-card mb-3">
        <div class="erp-card-body p-3">
          <div class="row g-2 align-items-center">
            <div class="col-md-6 col-lg-5">
              <div class="input-group input-group-sm">
                <span class="input-group-text bg-light"><i class="bi bi-search"></i></span>
                <input type="text" id="po-search" class="form-control" placeholder="Search by PO #, supplier name, notes..." oninput="PurchasesComponent.filterList()">
              </div>
            </div>
            <div class="col-md-3 col-lg-3">
              <select id="po-status-filter" class="form-select form-select-sm" onchange="PurchasesComponent.filterList()">
                <option value="">All Statuses</option>
                <option value="Ordered">Ordered (Pending)</option>
                <option value="Received">Received</option>
                <option value="Draft">Draft</option>
                <option value="Cancelled">Cancelled</option>
              </select>
            </div>
            <div class="col-auto ms-auto text-muted small">
              Total: <strong id="po-total-count">${this.filteredData.length}</strong> purchase orders
            </div>
          </div>
        </div>
      </div>

      <!-- Table -->
      <div class="erp-card">
        <div class="erp-table-wrapper">
          <table class="erp-table" id="purchases-table">
            <thead>
              <tr>
                <th>PO Number</th>
                <th>Supplier</th>
                <th>Order Date</th>
                <th>Expected Delivery</th>
                <th>Line Items</th>
                <th>Total Amount</th>
                <th>Status</th>
                <th class="text-end">Actions</th>
              </tr>
            </thead>
            <tbody id="purchases-table-body">
              ${this.buildTableRows(pageItems)}
            </tbody>
          </table>
        </div>
        <div id="purchases-pagination">
          ${App.renderPagination({
            currentPage: this.currentPage,
            pageSize: this.pageSize,
            totalItems: this.filteredData.length,
            componentName: 'PurchasesComponent'
          })}
        </div>
      </div>
    `;
  },

  buildTableRows(list) {
    if (!list || list.length === 0) {
      return `<tr><td colspan="8" class="text-center text-muted py-4"><i class="bi bi-bag fs-2 d-block mb-2 text-muted"></i>No purchase orders found.</td></tr>`;
    }

    return list.map(p => `
      <tr>
        <td class="fw-semibold font-monospace">
          <a href="#purchases/view/${p.purchaseId}">${p.purchaseNumber}</a>
        </td>
        <td><div class="fw-semibold text-dark">${p.supplierName}</div></td>
        <td class="text-muted small">${App.formatDate(p.purchaseDate)}</td>
        <td class="text-muted small">${App.formatDate(p.expectedDeliveryDate)}</td>
        <td><span class="badge bg-light text-dark border">${p.items?.length || 0} items</span></td>
        <td class="fw-bold text-dark">${App.formatCurrency(p.totalAmount)}</td>
        <td><span class="badge-status badge-${p.status.toLowerCase()}">${p.status}</span></td>
        <td class="text-end text-nowrap">
          <div class="table-actions">
            <a href="#purchases/view/${p.purchaseId}" class="btn btn-outline-primary btn-sm" title="View Details">
              <i class="bi bi-eye"></i> View
            </a>
            ${(Auth.hasPermission('purchases', 'EDIT') && (p.status === 'Ordered' || p.status === 'Draft')) ? `
              <button type="button" class="btn btn-success btn-sm" title="Receive Goods & Increase Inventory" onclick="PurchasesComponent.receiveOrder(${p.purchaseId}, '${p.purchaseNumber}')">
                <i class="bi bi-box-arrow-in-down"></i> Receive
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
    const tbody = document.getElementById('purchases-table-body');
    if (tbody) {
      tbody.innerHTML = this.buildTableRows(pageItems);
    }
    const pagContainer = document.getElementById('purchases-pagination');
    if (pagContainer) {
      pagContainer.innerHTML = App.renderPagination({
        currentPage: this.currentPage,
        pageSize: this.pageSize,
        totalItems: this.filteredData.length,
        componentName: 'PurchasesComponent'
      });
    }
    const totalCount = document.getElementById('po-total-count');
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
    const q = (document.getElementById('po-search')?.value || '').toLowerCase();
    const status = document.getElementById('po-status-filter')?.value || '';

    this.filteredData = this.data.filter(p => {
      const matchQ = !q || p.purchaseNumber.toLowerCase().includes(q) || p.supplierName.toLowerCase().includes(q) || (p.notes && p.notes.toLowerCase().includes(q));
      const matchStatus = !status || p.status === status;
      return matchQ && matchStatus;
    });

    this.currentPage = 1;
    this.updateTableView();
  },

  async renderCreateForm(container) {
    container.innerHTML = `
      <div class="loading-spinner-container">
        <div class="spinner-border text-primary mb-2"></div>
        <p>Preparing Purchase Order form...</p>
      </div>
    `;

    try {
      const [suppliers, products] = await Promise.all([
        Api.get('/suppliers'),
        Api.get('/products')
      ]);

      this.suppliersCache = suppliers.filter(s => s.status === 'Active');
      this.productsCache = products.filter(p => p.status === 'Active');

      const today = new Date().toISOString().slice(0, 10);
      const nextWeek = new Date(Date.now() + 7 * 86400000).toISOString().slice(0, 10);

      container.innerHTML = `
        <div class="page-header-container">
          <div>
            <h1 class="page-title">Create Purchase Order</h1>
            <p class="page-subtitle">Order inventory items from verified suppliers</p>
          </div>
          <div>
            <a href="#purchases" class="btn btn-erp-secondary btn-sm">
              <i class="bi bi-arrow-left me-1"></i>Back to Purchases
            </a>
          </div>
        </div>

        <form id="create-po-form" onsubmit="PurchasesComponent.submitCreate(event)">
          <div class="erp-card mb-4">
            <div class="erp-card-body p-4">
              <div class="form-section-title"><i class="bi bi-file-earmark-text me-2"></i>Order & Supplier Information</div>

              <div class="row mb-3">
                <div class="col-md-6 mb-3 mb-md-0">
                  <label class="form-label" for="po-supplier">Supplier <span class="required-asterisk">*</span></label>
                  <select id="po-supplier" class="form-select" required>
                    <option value="">Select Supplier</option>
                    ${this.suppliersCache.map(s => `
                      <option value="${s.supplierId}">${s.supplierCode} - ${s.name} (${s.paymentTerms})</option>
                    `).join('')}
                  </select>
                </div>
                <div class="col-md-3 mb-3 mb-md-0">
                  <label class="form-label" for="po-date">Purchase Date <span class="required-asterisk">*</span></label>
                  <input type="date" id="po-date" class="form-control" value="${today}" required>
                </div>
                <div class="col-md-3">
                  <label class="form-label" for="po-delivery">Expected Delivery <span class="required-asterisk">*</span></label>
                  <input type="date" id="po-delivery" class="form-control" value="${nextWeek}" required>
                </div>
              </div>

              <div class="row mb-2">
                <div class="col-12">
                  <label class="form-label" for="po-notes">Order Notes / Terms</label>
                  <input type="text" id="po-notes" class="form-control" placeholder="Special delivery instructions, freight notes...">
                </div>
              </div>
            </div>
          </div>

          <!-- Dynamic Item Lines -->
          <div class="erp-card mb-4">
            <div class="erp-card-header">
              <h6 class="erp-card-title"><i class="bi bi-box-seam me-2"></i>Purchase Order Items</h6>
              <button type="button" class="btn btn-outline-primary btn-sm" onclick="PurchasesComponent.addItemRow()">
                <i class="bi bi-plus-lg me-1"></i>Add Item Line
              </button>
            </div>
            <div class="erp-card-body p-0">
              <div class="erp-table-wrapper">
                <table class="erp-table mb-0" id="po-items-table">
                  <thead>
                    <tr>
                      <th style="width: 45%;">Product <span class="required-asterisk">*</span></th>
                      <th style="width: 15%;">Quantity <span class="required-asterisk">*</span></th>
                      <th style="width: 20%;">Unit Cost (₹) <span class="required-asterisk">*</span></th>
                      <th style="width: 15%;">Line Total</th>
                      <th style="width: 5%;" class="text-center">Remove</th>
                    </tr>
                  </thead>
                  <tbody id="po-items-tbody">
                    <!-- Dynamic rows rendered here -->
                  </tbody>
                  <tfoot>
                    <tr class="table-light">
                      <td colspan="3" class="text-end fw-bold">Total Order Amount:</td>
                      <td colspan="2" class="fw-bold text-primary fs-6" id="po-total-display">₹0.00</td>
                    </tr>
                  </tfoot>
                </table>
              </div>
            </div>
          </div>

          <div class="d-flex justify-content-end gap-2">
            <a href="#purchases" class="btn btn-erp-secondary">Cancel</a>
            <button type="submit" class="btn btn-erp-primary" id="btn-save-po">
              <i class="bi bi-check2-circle me-1"></i>Submit Purchase Order
            </button>
          </div>
        </form>
      `;

      // Add initial item row
      this.addItemRow();
    } catch (error) {
      App.showToast(error.message, 'danger');
      window.location.hash = '#purchases';
    }
  },

  addItemRow() {
    const tbody = document.getElementById('po-items-tbody');
    if (!tbody) return;

    const rowId = `po-row-${Date.now()}-${Math.floor(Math.random() * 1000)}`;
    const tr = document.createElement('tr');
    tr.id = rowId;
    tr.className = 'po-item-row';

    tr.innerHTML = `
      <td>
        <select class="form-select form-select-sm po-prod-select" required onchange="PurchasesComponent.onProductChange('${rowId}')">
          <option value="">Select Product...</option>
          ${this.productsCache.map(p => `
            <option value="${p.productId}" data-cost="${p.costPrice}">
              ${p.sku} - ${p.name} (Cost: ₹${p.costPrice.toFixed(2)})
            </option>
          `).join('')}
        </select>
      </td>
      <td>
        <input type="number" min="1" class="form-control form-control-sm po-qty-input" value="1" required oninput="PurchasesComponent.calcRowTotal('${rowId}')">
      </td>
      <td>
        <input type="number" step="0.01" min="0.01" class="form-control form-control-sm po-price-input" placeholder="0.00" required oninput="PurchasesComponent.calcRowTotal('${rowId}')">
      </td>
      <td class="fw-bold text-dark po-line-total">
        ₹0.00
      </td>
      <td class="text-center">
        <button type="button" class="btn btn-outline-danger btn-sm py-0 px-2" onclick="PurchasesComponent.removeItemRow('${rowId}')">
          <i class="bi bi-trash"></i>
        </button>
      </td>
    `;

    tbody.appendChild(tr);
  },

  removeItemRow(rowId) {
    const tbody = document.getElementById('po-items-tbody');
    if (tbody.querySelectorAll('tr').length <= 1) {
      App.showToast('Purchase order must have at least one line item.', 'warning');
      return;
    }
    const row = document.getElementById(rowId);
    if (row) row.remove();
    this.calcGrandTotal();
  },

  onProductChange(rowId) {
    const row = document.getElementById(rowId);
    if (!row) return;

    const select = row.querySelector('.po-prod-select');
    const priceInput = row.querySelector('.po-price-input');
    const selectedOption = select.options[select.selectedIndex];
    const cost = selectedOption?.dataset?.cost || '0';

    if (cost && parseFloat(cost) > 0) {
      priceInput.value = parseFloat(cost).toFixed(2);
    }
    this.calcRowTotal(rowId);
  },

  calcRowTotal(rowId) {
    const row = document.getElementById(rowId);
    if (!row) return;

    const qty = parseInt(row.querySelector('.po-qty-input')?.value, 10) || 0;
    const price = parseFloat(row.querySelector('.po-price-input')?.value) || 0;
    const lineTotal = qty * price;

    row.querySelector('.po-line-total').textContent = App.formatCurrency(lineTotal);
    this.calcGrandTotal();
  },

  calcGrandTotal() {
    let grandTotal = 0;
    document.querySelectorAll('.po-item-row').forEach(row => {
      const qty = parseInt(row.querySelector('.po-qty-input')?.value, 10) || 0;
      const price = parseFloat(row.querySelector('.po-price-input')?.value) || 0;
      grandTotal += qty * price;
    });

    const display = document.getElementById('po-total-display');
    if (display) {
      display.textContent = App.formatCurrency(grandTotal);
    }
  },

  async submitCreate(event) {
    event.preventDefault();
    const btn = document.getElementById('btn-save-po');
    btn.disabled = true;

    const items = [];
    const rows = document.querySelectorAll('.po-item-row');
    for (const row of rows) {
      const prodId = parseInt(row.querySelector('.po-prod-select')?.value, 10);
      const qty = parseInt(row.querySelector('.po-qty-input')?.value, 10);
      const price = parseFloat(row.querySelector('.po-price-input')?.value);

      if (!prodId || isNaN(prodId)) {
        App.showToast('Please select a valid product for every item line.', 'warning');
        btn.disabled = false;
        return;
      }
      items.push({ productId: prodId, quantity: qty, unitPrice: price });
    }

    const payload = {
      supplierId: parseInt(document.getElementById('po-supplier').value, 10),
      purchaseDate: document.getElementById('po-date').value,
      expectedDeliveryDate: document.getElementById('po-delivery').value,
      notes: document.getElementById('po-notes').value.trim(),
      items: items
    };

    try {
      const created = await Api.post('/purchases', payload);
      App.showToast(`Purchase order ${created.purchaseNumber} created successfully!`, 'success');
      window.location.hash = `#purchases/view/${created.purchaseId}`;
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
        <p>Loading purchase order #${id}...</p>
      </div>
    `;

    try {
      const po = await Api.get(`/purchases/${id}`);
      container.innerHTML = `
        <div class="page-header-container">
          <div>
            <h1 class="page-title">Purchase Order ${po.purchaseNumber}</h1>
            <p class="page-subtitle">Created by ${po.createdBy} on ${App.formatDate(po.createdAt)}</p>
          </div>
          <div>
            <a href="#purchases" class="btn btn-erp-secondary btn-sm me-2">
              <i class="bi bi-arrow-left me-1"></i>Back
            </a>
            ${(po.status === 'Ordered' || po.status === 'Draft') ? `
              <button type="button" class="btn btn-success btn-sm" onclick="PurchasesComponent.receiveOrder(${po.purchaseId}, '${po.purchaseNumber}')">
                <i class="bi bi-box-arrow-in-down me-1"></i>Receive Goods (Update Stock)
              </button>
            ` : ''}
          </div>
        </div>

        <div class="row g-3 mb-4">
          <div class="col-lg-8">
            <div class="erp-card mb-3">
              <div class="erp-card-header">
                <h6 class="erp-card-title"><i class="bi bi-receipt me-2"></i>Order Line Items</h6>
                <span class="badge-status badge-${po.status.toLowerCase()}">${po.status}</span>
              </div>
              <div class="erp-table-wrapper">
                <table class="erp-table">
                  <thead>
                    <tr>
                      <th>SKU</th>
                      <th>Product Description</th>
                      <th>Quantity</th>
                      <th>Unit Cost</th>
                      <th class="text-end">Total Price</th>
                    </tr>
                  </thead>
                  <tbody>
                    ${po.items.map(item => `
                      <tr>
                        <td class="fw-semibold text-primary font-monospace">${item.sku}</td>
                        <td>${item.productName}</td>
                        <td class="fw-bold">${item.quantity}</td>
                        <td>${App.formatCurrency(item.unitPrice)}</td>
                        <td class="text-end fw-bold">${App.formatCurrency(item.totalPrice)}</td>
                      </tr>
                    `).join('')}
                  </tbody>
                  <tfoot>
                    <tr class="table-light">
                      <td colspan="4" class="text-end fw-bold">Total Order Value:</td>
                      <td class="text-end fw-bold text-primary fs-6">${App.formatCurrency(po.totalAmount)}</td>
                    </tr>
                  </tfoot>
                </table>
              </div>
            </div>

            ${po.notes ? `
              <div class="erp-card">
                <div class="erp-card-body p-3">
                  <h6 class="fw-bold mb-1"><i class="bi bi-info-circle me-1 text-primary"></i>Notes:</h6>
                  <p class="text-muted small mb-0">${po.notes}</p>
                </div>
              </div>
            ` : ''}
          </div>

          <div class="col-lg-4">
            <div class="erp-card">
              <div class="erp-card-header">
                <h6 class="erp-card-title"><i class="bi bi-truck me-2"></i>Vendor Summary</h6>
              </div>
              <div class="erp-card-body">
                <div class="mb-3">
                  <div class="text-muted small">Supplier</div>
                  <div class="fw-bold text-dark fs-6">${po.supplierName}</div>
                </div>
                <div class="mb-3">
                  <div class="text-muted small">Order Date</div>
                  <div class="fw-semibold">${App.formatDate(po.purchaseDate)}</div>
                </div>
                <div class="mb-3">
                  <div class="text-muted small">Expected Delivery</div>
                  <div class="fw-semibold text-primary">${App.formatDate(po.expectedDeliveryDate)}</div>
                </div>
                <div class="mb-0">
                  <div class="text-muted small">Workflow Status</div>
                  <div class="mt-1">
                    <span class="badge-status badge-${po.status.toLowerCase()}">${po.status}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      `;
    } catch (error) {
      App.showToast(error.message, 'danger');
      window.location.hash = '#purchases';
    }
  },

  receiveOrder(id, number) {
    App.confirmAction(
      'Receive Goods',
      `Confirm receipt of goods for Purchase Order "${number}"? This will automatically increase warehouse inventory for all line items and record an expense in the ledger.`,
      async () => {
        try {
          await Api.put(`/purchases/${id}/status`, { status: 'Received' });
          App.showToast(`Goods received for ${number}. Inventory and ledger updated!`, 'success');
          window.location.hash = `#purchases/view/${id}`;
          Router.navigate();
        } catch (error) {
          App.showToast(error.message, 'danger');
        }
      },
      'Receive & Restock'
    );
  }
};
