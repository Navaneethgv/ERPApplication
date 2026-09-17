// Inventory Component
const InventoryComponent = {
  data: [],
  filteredData: [],
  currentPage: 1,
  pageSize: 10,
  selectedCategory: '',
  searchQuery: '',
  statusFilter: '',

  async render(container, action, id) {
    if (action === 'adjust') {
      if (!Auth.hasPermission('inventory', 'EDIT')) {
        App.showToast('Access restricted: You do not have permission to adjust inventory stock.', 'warning');
        window.location.hash = '#inventory';
        return;
      }
      this.renderAdjustmentForm(container, id);
    } else {
      await this.renderList(container);
    }
  },

  async renderList(container) {
    container.innerHTML = `
      <div class="loading-spinner-container">
        <div class="spinner-border text-primary mb-2"></div>
        <p>Loading inventory balances...</p>
      </div>
    `;

    try {
      this.data = (await Api.get('/inventory')) || [];
      this.currentPage = 1;
      this.applyFilter();
      this.renderView(container);
    } catch (error) {
      container.innerHTML = `
        <div class="alert alert-danger">
          <h5><i class="bi bi-exclamation-circle me-2"></i>Failed to Load Inventory</h5>
          <p class="mb-0">${error.message}</p>
        </div>
      `;
    }
  },

  applyFilter() {
    const q = (this.searchQuery || '').trim().toLowerCase();
    const filterCat = (this.selectedCategory || '').trim().toLowerCase();
    const status = (this.statusFilter || '').trim().toLowerCase();

    this.filteredData = (this.data || []).filter(i => {
      const sku = (i.sku || '').toLowerCase();
      const name = (i.productName || '').toLowerCase();
      const location = (i.location || '').toLowerCase();
      const itemCat = (i.category || i.Category || '').trim().toLowerCase();

      // Search match across SKU, Product Name, Location, and Category
      const matchQ = !q || sku.includes(q) || name.includes(q) || location.includes(q) || itemCat.includes(q);

      // Category match (case-insensitive & trimmed)
      const matchCat = !filterCat || itemCat === filterCat;

      // Status match
      let matchStatus = true;
      if (status === 'low') {
        matchStatus = Boolean(i.isLowStock || (i.quantityOnHand <= (i.reorderLevel ?? 10)));
      } else if (status === 'good') {
        matchStatus = Boolean(!i.isLowStock && (i.quantityOnHand > (i.reorderLevel ?? 10)));
      }

      return matchQ && matchCat && matchStatus;
    });
  },

  renderView(container) {
    const totalValuation = (this.data || []).reduce((sum, item) => sum + (item.totalValuation || 0), 0);
    const totalUnits = (this.data || []).reduce((sum, item) => sum + (item.quantityOnHand || 0), 0);
    const lowStockCount = (this.data || []).filter(item => item.isLowStock).length;

    // Collect clean unique categories (case-insensitive deduplication, preserved display casing)
    const catMap = new Map();
    (this.data || []).forEach(i => {
      const raw = (i.category || i.Category || '').trim();
      if (raw) {
        const lower = raw.toLowerCase();
        if (!catMap.has(lower)) {
          catMap.set(lower, raw.charAt(0).toUpperCase() + raw.slice(1));
        }
      }
    });
    const categories = Array.from(catMap.values()).sort();
    const canEdit = Auth.hasPermission('inventory', 'EDIT');
    const canProcure = Auth.hasPermission('purchases', 'ADD');

    const start = (this.currentPage - 1) * this.pageSize;
    const pageItems = this.filteredData.slice(start, start + this.pageSize);

    container.innerHTML = `
      <div class="page-header-container d-flex flex-column flex-sm-row justify-content-between align-items-start align-items-sm-center gap-2 mb-3 mb-md-4">
        <div>
          <h1 class="page-title">Warehouse & Inventory Management</h1>
          <p class="page-subtitle">Real-time stock tracking, warehouse locations, adjustments, and valuation</p>
        </div>
        <div class="d-flex flex-wrap gap-2">
          ${canEdit ? `
            <a href="#inventory/adjust" class="btn btn-erp-primary btn-sm">
              <i class="bi bi-arrow-left-right me-1"></i>Stock Adjustment
            </a>
          ` : ''}
          ${canProcure ? `
            <a href="#purchases/new" class="btn btn-outline-primary btn-sm">
              <i class="bi bi-plus-circle me-1"></i>Procure Stock
            </a>
          ` : ''}
        </div>
      </div>

      <!-- Inventory KPI Cards -->
      <div class="row g-3 mb-4">
        <div class="col-12 col-sm-6 col-xl-4">
          <div class="kpi-card h-100">
            <div class="kpi-header">
              <span class="kpi-title">Total Cost Valuation</span>
              <div class="kpi-icon-box icon-green"><i class="bi bi-cash-stack"></i></div>
            </div>
            <div class="kpi-value text-success">${App.formatCurrency(totalValuation)}</div>
            <div class="kpi-subtitle text-muted">Total inventory asset value</div>
          </div>
        </div>

        <div class="col-12 col-sm-6 col-xl-4">
          <div class="kpi-card h-100">
            <div class="kpi-header">
              <span class="kpi-title">Total Units On Hand</span>
              <div class="kpi-icon-box icon-blue"><i class="bi bi-box-seam"></i></div>
            </div>
            <div class="kpi-value">${totalUnits.toLocaleString()}</div>
            <div class="kpi-subtitle text-muted">Across all warehouse zones</div>
          </div>
        </div>

        <div class="col-12 col-sm-6 col-xl-4">
          <div class="kpi-card h-100">
            <div class="kpi-header">
              <span class="kpi-title">Low Stock SKUs</span>
              <div class="kpi-icon-box icon-rose"><i class="bi bi-exclamation-triangle-fill"></i></div>
            </div>
            <div class="kpi-value ${lowStockCount > 0 ? 'text-danger' : 'text-success'}">${lowStockCount}</div>
            <div class="kpi-subtitle text-muted">Items at or below reorder limit</div>
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
                <input type="text" id="inv-search" class="form-control" placeholder="Search by SKU, product name, warehouse location..." value="${this.searchQuery ? this.searchQuery.replace(/"/g, '&quot;') : ''}" oninput="InventoryComponent.filterList()">
              </div>
            </div>
            <div class="col-12 col-sm-6 col-md-3">
              <select id="inv-cat-filter" class="form-select form-select-sm" onchange="InventoryComponent.filterList()">
                <option value="" ${!this.selectedCategory ? 'selected' : ''}>All Categories</option>
                ${categories.map(c => `<option value="${c}" ${this.selectedCategory && this.selectedCategory.toLowerCase() === c.toLowerCase() ? 'selected' : ''}>${c}</option>`).join('')}
              </select>
            </div>
            <div class="col-12 col-sm-6 col-md-2">
              <select id="inv-status-filter" class="form-select form-select-sm" onchange="InventoryComponent.filterList()">
                <option value="" ${!this.statusFilter ? 'selected' : ''}>All Statuses</option>
                <option value="low" ${this.statusFilter === 'low' ? 'selected' : ''}>Low Stock Only</option>
                <option value="good" ${this.statusFilter === 'good' ? 'selected' : ''}>Adequate Stock</option>
              </select>
            </div>
            <div class="col-12 col-sm-auto ms-sm-auto text-muted small text-sm-end mt-2 mt-sm-0">
              Total: <strong id="inv-total-count">${this.filteredData.length}</strong> items
            </div>
          </div>
        </div>
      </div>

      <!-- Inventory Table -->
      <div class="erp-card">
        <div class="table-responsive erp-table-wrapper">
          <table class="erp-table align-middle" id="inventory-table" style="min-width: 780px;">
            <thead>
              <tr>
                <th>SKU</th>
                <th>Product Name</th>
                <th>Category</th>
                <th>Location</th>
                <th>On Hand</th>
                <th>Reserved</th>
                <th>Available</th>
                <th>Valuation</th>
                <th>Status</th>
                <th class="text-end">Actions</th>
              </tr>
            </thead>
            <tbody id="inventory-table-body">
              ${this.buildTableRows(pageItems)}
            </tbody>
          </table>
        </div>
        <div id="inventory-pagination">
          ${App.renderPagination({
            currentPage: this.currentPage,
            pageSize: this.pageSize,
            totalItems: this.filteredData.length,
            componentName: 'InventoryComponent'
          })}
        </div>
      </div>
    `;

    // Attach explicit event listeners to ensure immediate response across all browsers
    const searchInput = container.querySelector('#inv-search');
    const catSelect = container.querySelector('#inv-cat-filter');
    const statusSelect = container.querySelector('#inv-status-filter');

    if (searchInput) {
      searchInput.addEventListener('input', () => this.filterList());
      searchInput.addEventListener('change', () => this.filterList());
    }
    if (catSelect) {
      catSelect.addEventListener('change', () => this.filterList());
    }
    if (statusSelect) {
      statusSelect.addEventListener('change', () => this.filterList());
    }
  },

  buildTableRows(list) {
    if (!list || list.length === 0) {
      return `<tr><td colspan="10" class="text-center text-muted py-4"><i class="bi bi-archive fs-2 d-block mb-2 text-muted"></i>No inventory records found.</td></tr>`;
    }

    return list.map(i => `
      <tr>
        <td class="fw-semibold text-primary font-monospace">${i.sku || ''}</td>
        <td><div class="fw-semibold text-dark">${i.productName || ''}</div></td>
        <td><span class="badge bg-light text-dark border">${i.category || i.Category || 'General'}</span></td>
        <td><i class="bi bi-geo-alt text-muted me-1"></i>${i.location || 'Warehouse'}</td>
        <td class="fw-bold">${i.quantityOnHand ?? 0}</td>
        <td class="text-muted">${i.reservedQuantity ?? 0}</td>
        <td class="fw-bold text-success">${i.availableQuantity ?? 0}</td>
        <td class="fw-semibold text-dark">${App.formatCurrency(i.totalValuation ?? 0)}</td>
        <td>
          ${(i.quantityOnHand ?? 0) <= 0
            ? '<span class="badge bg-danger">Out of Stock</span>'
            : (i.isLowStock
              ? `<span class="badge bg-warning text-dark"><i class="bi bi-exclamation-circle me-1"></i>Low (${i.quantityOnHand} &le; ${i.reorderLevel ?? 10})</span>`
              : '<span class="badge bg-success-subtle text-success border border-success-subtle"><i class="bi bi-check-circle me-1"></i>Healthy</span>'
            )
          }
        </td>
        <td class="text-end text-nowrap">
          <div class="table-actions">
            ${Auth.hasPermission('inventory', 'EDIT') ? `
              <a href="#inventory/adjust/${i.productId}" class="btn btn-outline-primary btn-sm" title="Adjust Stock">
                <i class="bi bi-sliders"></i> Adjust
              </a>
            ` : ''}
          </div>
        </td>
      </tr>
    `).join('');
  },

  updateTableView() {
    const totalPages = Math.max(1, Math.ceil(this.filteredData.length / this.pageSize));
    if (this.currentPage > totalPages) {
      this.currentPage = 1;
    }
    const start = (this.currentPage - 1) * this.pageSize;
    const pageItems = this.filteredData.slice(start, start + this.pageSize);
    const tbody = document.getElementById('inventory-table-body');
    if (tbody) {
      tbody.innerHTML = this.buildTableRows(pageItems);
    }
    const pagContainer = document.getElementById('inventory-pagination');
    if (pagContainer) {
      pagContainer.innerHTML = App.renderPagination({
        currentPage: this.currentPage,
        pageSize: this.pageSize,
        totalItems: this.filteredData.length,
        componentName: 'InventoryComponent'
      });
    }
    const totalCount = document.getElementById('inv-total-count');
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
    const searchEl = document.getElementById('inv-search');
    const catEl = document.getElementById('inv-cat-filter');
    const statusEl = document.getElementById('inv-status-filter');

    this.searchQuery = searchEl ? searchEl.value : '';
    this.selectedCategory = catEl ? catEl.value : '';
    this.statusFilter = statusEl ? statusEl.value : '';

    this.currentPage = 1;
    this.applyFilter();
    this.updateTableView();
  },

  async renderAdjustmentForm(container, selectedProductId) {
    container.innerHTML = `
      <div class="loading-spinner-container">
        <div class="spinner-border text-primary mb-2"></div>
        <p>Loading products for adjustment...</p>
      </div>
    `;

    try {
      const products = await Api.get('/products');
      container.innerHTML = `
        <div class="page-header-container d-flex flex-column flex-sm-row justify-content-between align-items-start align-items-sm-center gap-2 mb-3 mb-md-4">
          <div>
            <h1 class="page-title">Inventory Stock Adjustment</h1>
            <p class="page-subtitle">Manual balance correction, stock-in, damage write-offs, or audit synchronization</p>
          </div>
          <div>
            <a href="#inventory" class="btn btn-erp-secondary btn-sm">
              <i class="bi bi-arrow-left me-1"></i>Back to Inventory
            </a>
          </div>
        </div>

        <div class="erp-card">
          <div class="erp-card-body p-3 p-md-4">
            <form id="adjustment-form" onsubmit="InventoryComponent.submitAdjustment(event)">
              <div class="form-section-title"><i class="bi bi-sliders me-2"></i>Adjustment Parameters</div>

              <div class="row g-3 mb-3">
                <div class="col-12 col-md-6">
                  <label class="form-label" for="adj-product">Target Product <span class="required-asterisk">*</span></label>
                  <select id="adj-product" class="form-select" required onchange="InventoryComponent.updateSelectedStockInfo(this.value)">
                    <option value="">Select a Product</option>
                    ${products.map(p => `
                      <option value="${p.productId}" ${selectedProductId == p.productId ? 'selected' : ''}>
                        ${p.sku} - ${p.name} (Current: ${p.quantityOnHand} ${p.unitOfMeasure}s)
                      </option>
                    `).join('')}
                  </select>
                </div>
                <div class="col-12 col-md-6">
                  <label class="form-label" for="adj-type">Adjustment Action <span class="required-asterisk">*</span></label>
                  <select id="adj-type" class="form-select" required>
                    <option value="StockIn">Stock In (Add Quantity)</option>
                    <option value="StockOut">Stock Out (Deduct Quantity)</option>
                    <option value="Damaged">Damaged / Expired Write-off</option>
                    <option value="Correction">Physical Count Correction</option>
                    <option value="Audit">Audit Reconciliation</option>
                  </select>
                </div>
              </div>

              <div class="row g-3 mb-3">
                <div class="col-12 col-md-6">
                  <label class="form-label" for="adj-qty">Quantity Change <span class="required-asterisk">*</span></label>
                  <input type="number" min="1" id="adj-qty" class="form-control" placeholder="Quantity to adjust" required>
                </div>
                <div class="col-12 col-md-6">
                  <label class="form-label" for="adj-loc">Warehouse Location</label>
                  <input type="text" id="adj-loc" class="form-control" value="Main Warehouse" placeholder="e.g. Warehouse A-01">
                </div>
              </div>

              <div class="row g-3 mb-4">
                <div class="col-12">
                  <label class="form-label" for="adj-reason">Reason / Audit Justification <span class="required-asterisk">*</span></label>
                  <textarea id="adj-reason" class="form-control" rows="2" placeholder="Explain the reason for this inventory adjustment..." required></textarea>
                </div>
              </div>

              <div class="d-flex flex-column-reverse flex-sm-row justify-content-end gap-2 border-top pt-3">
                <a href="#inventory" class="btn btn-erp-secondary">Cancel</a>
                <button type="submit" class="btn btn-erp-primary" id="btn-save-adj">
                  <i class="bi bi-check2 me-1"></i>Apply Adjustment
                </button>
              </div>
            </form>
          </div>
        </div>
      `;
    } catch (error) {
      App.showToast(error.message, 'danger');
      window.location.hash = '#inventory';
    }
  },

  async submitAdjustment(event) {
    event.preventDefault();
    const btn = document.getElementById('btn-save-adj');
    btn.disabled = true;

    const payload = {
      productId: parseInt(document.getElementById('adj-product').value, 10),
      adjustmentType: document.getElementById('adj-type').value,
      quantity: parseInt(document.getElementById('adj-qty').value, 10),
      location: document.getElementById('adj-loc').value.trim(),
      reason: document.getElementById('adj-reason').value.trim()
    };

    try {
      await Api.post('/inventory/adjust', payload);
      App.showToast('Stock level adjusted successfully.', 'success');
      window.location.hash = '#inventory';
    } catch (error) {
      App.showToast(error.message, 'danger');
    } finally {
      btn.disabled = false;
    }
  }
};

window.InventoryComponent = InventoryComponent;
