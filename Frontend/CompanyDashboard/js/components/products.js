// Products Component
const ProductsComponent = {
  data: [],
  filteredData: [],
  currentPage: 1,
  pageSize: 10,

  async render(container, action, id) {
    if (action === 'new') {
      if (!Auth.hasPermission('products', 'ADD')) {
        App.showToast('Access restricted: You do not have permission to add products.', 'warning');
        window.location.hash = '#products';
        return;
      }
      this.renderForm(container, null);
    } else if (action === 'edit' && id) {
      if (!Auth.hasPermission('products', 'EDIT')) {
        App.showToast('Access restricted: You do not have permission to edit products.', 'warning');
        window.location.hash = '#products';
        return;
      }
      container.innerHTML = `
        <div class="loading-spinner-container">
          <div class="spinner-border text-primary mb-2"></div>
          <p>Loading product details...</p>
        </div>
      `;
      try {
        const prod = await Api.get(`/products/${id}`);
        this.renderForm(container, prod);
      } catch (error) {
        App.showToast(error.message, 'danger');
        window.location.hash = '#products';
      }
    } else {
      this.renderList(container);
    }
  },

  async renderList(container) {
    container.innerHTML = `
      <div class="loading-spinner-container">
        <div class="spinner-border text-primary mb-2"></div>
        <p>Loading product catalog...</p>
      </div>
    `;

    try {
      this.data = await Api.get('/products');
      this.filteredData = [...this.data];
      this.currentPage = 1;
      this.renderTable(container);
    } catch (error) {
      container.innerHTML = `
        <div class="alert alert-danger">
          <h5><i class="bi bi-exclamation-circle me-2"></i>Failed to Load Products</h5>
          <p class="mb-0">${error.message}</p>
        </div>
      `;
    }
  },

  renderTable(container) {
    const categories = [...new Set(this.data.map(p => p.category))].filter(Boolean);
    const isStaff = Auth.isAdmin() || Auth.isEmployee();
    const isCustomer = Auth.isCustomer();

    const start = (this.currentPage - 1) * this.pageSize;
    const pageItems = this.filteredData.slice(start, start + this.pageSize);

    container.innerHTML = `
      <div class="page-header-container d-flex flex-column flex-sm-row justify-content-between align-items-start align-items-sm-center gap-2 mb-3 mb-md-4">
        <div>
          <h1 class="page-title">${isCustomer ? 'Product Catalog' : 'Product Inventory & Catalog'}</h1>
          <p class="page-subtitle">${isCustomer ? 'Browse items available for purchase' : 'Manage SKUs, unit pricing, cost prices, and stock threshold levels'}</p>
        </div>
        <div>
          ${Auth.hasPermission('products', 'ADD') ? `
            <a href="#products/new" class="btn btn-erp-primary btn-sm">
              <i class="bi bi-plus-circle me-1"></i>Add New Product
            </a>
          ` : (isCustomer ? `
            <a href="#sales/new" class="btn btn-erp-primary btn-sm">
              <i class="bi bi-cart-plus me-1"></i>Order Items
            </a>
          ` : '')}
        </div>
      </div>

      <!-- Filters -->
      <div class="erp-card mb-3">
        <div class="erp-card-body p-3">
          <div class="row g-2 align-items-center">
            <div class="col-12 col-md-5 col-lg-4">
              <div class="input-group input-group-sm">
                <span class="input-group-text bg-light"><i class="bi bi-search"></i></span>
                <input type="text" id="prod-search" class="form-control" placeholder="Search by SKU, product name, description..." oninput="ProductsComponent.filterList()">
              </div>
            </div>
            <div class="col-6 col-md-3 col-lg-3">
              <select id="prod-cat-filter" class="form-select form-select-sm" onchange="ProductsComponent.filterList()">
                <option value="">All Categories</option>
                ${categories.map(c => `<option value="${c}">${c}</option>`).join('')}
              </select>
            </div>
            ${isStaff ? `
              <div class="col-6 col-md-4 col-lg-3">
                <select id="prod-stock-filter" class="form-select form-select-sm" onchange="ProductsComponent.filterList()">
                  <option value="">All Stock Levels</option>
                  <option value="low">Low Stock Only</option>
                  <option value="in">In Stock Only</option>
                </select>
              </div>
            ` : ''}
            <div class="col-12 col-lg-auto ms-lg-auto text-muted small mt-1 mt-lg-0">
              Total: <strong id="prod-total-count">${this.filteredData.length}</strong> items
            </div>
          </div>
        </div>
      </div>

      <!-- Products Table -->
      <div class="erp-card">
        <div class="erp-table-wrapper table-responsive">
          <table class="erp-table" id="products-table">
            <thead>
              <tr>
                <th class="text-nowrap" style="min-width: 120px;">SKU</th>
                <th style="min-width: 180px;">Product Name & Info</th>
                <th class="text-nowrap" style="min-width: 120px;">Category</th>
                <th class="text-nowrap" style="min-width: 110px;">Selling Price</th>
                ${isStaff ? `<th class="text-nowrap" style="min-width: 110px;">Cost Price</th>` : ''}
                <th class="text-nowrap" style="min-width: 110px;">Stock Available</th>
                ${isStaff ? `<th class="text-nowrap" style="min-width: 110px;">Reorder Level</th>` : ''}
                <th class="text-nowrap" style="min-width: 90px;">Status</th>
                ${isStaff ? `<th class="text-end text-nowrap" style="min-width: 100px;">Actions</th>` : ''}
              </tr>
            </thead>
            <tbody id="products-table-body">
              ${this.buildTableRows(pageItems)}
            </tbody>
          </table>
        </div>
        <div id="products-pagination">
          ${App.renderPagination({
            currentPage: this.currentPage,
            pageSize: this.pageSize,
            totalItems: this.filteredData.length,
            componentName: 'ProductsComponent'
          })}
        </div>
      </div>
    `;
  },

  buildTableRows(list) {
    if (!list || list.length === 0) {
      return `<tr><td colspan="9" class="text-center text-muted py-4"><i class="bi bi-box-seam fs-2 d-block mb-2 text-muted"></i>No products found.</td></tr>`;
    }

    const isStaff = Auth.isAdmin() || Auth.isEmployee();
    const canEdit = Auth.hasPermission('products', 'EDIT');
    const canDelete = Auth.hasPermission('products', 'DELETE');

    return list.map(p => `
      <tr>
        <td class="fw-semibold text-primary font-monospace">${p.sku}</td>
        <td>
          <div class="fw-semibold text-dark">${p.name}</div>
          <div class="small text-muted" style="max-width: 250px;">${p.description || '-'}</div>
        </td>
        <td><span class="badge bg-light text-dark border">${p.category}</span></td>
        <td class="fw-bold text-dark">${App.formatCurrency(p.unitPrice)}</td>
        ${isStaff ? `<td class="text-muted">${App.formatCurrency(p.costPrice)}</td>` : ''}
        <td>
          ${p.quantityOnHand <= 0
            ? '<span class="badge bg-danger">Out of Stock</span>'
            : (p.isLowStock
              ? `<span class="badge bg-warning text-dark">${p.quantityOnHand} (Low)</span>`
              : `<span class="badge bg-success-subtle text-success border border-success-subtle">${p.quantityOnHand} ${p.unitOfMeasure}s</span>`
            )
          }
        </td>
        ${isStaff ? `<td class="text-muted">${p.reorderLevel}</td>` : ''}
        <td><span class="badge-status badge-${p.status.toLowerCase()}">${p.status}</span></td>
        ${(canEdit || canDelete) ? `
          <td class="text-end text-nowrap">
            <div class="table-actions">
              ${canEdit ? `
                <a href="#products/edit/${p.productId}" class="btn btn-outline-primary btn-sm" title="Edit Product">
                  <i class="bi bi-pencil"></i>
                </a>
              ` : ''}
              ${canDelete ? `
                <button type="button" class="btn btn-outline-danger btn-sm" title="Delete Product" onclick="ProductsComponent.confirmDelete(${p.productId}, '${p.name.replace(/'/g, "\\'")}')">
                  <i class="bi bi-trash"></i>
                </button>
              ` : ''}
            </div>
          </td>
        ` : ''}
      </tr>
    `).join('');
  },

  updateTableView() {
    const start = (this.currentPage - 1) * this.pageSize;
    const pageItems = this.filteredData.slice(start, start + this.pageSize);
    const tbody = document.getElementById('products-table-body');
    if (tbody) {
      tbody.innerHTML = this.buildTableRows(pageItems);
    }
    const pagContainer = document.getElementById('products-pagination');
    if (pagContainer) {
      pagContainer.innerHTML = App.renderPagination({
        currentPage: this.currentPage,
        pageSize: this.pageSize,
        totalItems: this.filteredData.length,
        componentName: 'ProductsComponent'
      });
    }
    const totalCount = document.getElementById('prod-total-count');
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
    const q = (document.getElementById('prod-search')?.value || '').toLowerCase();
    const cat = document.getElementById('prod-cat-filter')?.value || '';
    const stock = document.getElementById('prod-stock-filter')?.value || '';

    this.filteredData = this.data.filter(p => {
      const matchQ = !q || p.sku.toLowerCase().includes(q) || p.name.toLowerCase().includes(q) || (p.description && p.description.toLowerCase().includes(q));
      const matchCat = !cat || p.category === cat;
      const matchStock = !stock || (stock === 'low' ? p.isLowStock : p.quantityOnHand > p.reorderLevel);
      return matchQ && matchCat && matchStock;
    });

    this.currentPage = 1;
    this.updateTableView();
  },

  renderForm(container, prod) {
    const isEdit = !!prod;
    container.innerHTML = `
      <div class="page-header-container d-flex flex-column flex-sm-row justify-content-between align-items-start align-items-sm-center gap-2 mb-3 mb-md-4">
        <div>
          <h1 class="page-title">${isEdit ? 'Edit Product' : 'Add New Product'}</h1>
          <p class="page-subtitle">${isEdit ? `Update specifications for ${prod.name}` : 'Create a new product item in catalog'}</p>
        </div>
        <div>
          <a href="#products" class="btn btn-erp-secondary btn-sm">
            <i class="bi bi-arrow-left me-1"></i>Back to Products
          </a>
        </div>
      </div>

      <div class="erp-card">
        <div class="erp-card-body p-3 p-sm-4">
          <form id="product-form" onsubmit="ProductsComponent.submitForm(event, ${isEdit ? prod.productId : 'null'})">
            <div class="form-section-title"><i class="bi bi-box-seam me-2"></i>Product Identification</div>

            <div class="row g-3 mb-3">
              <div class="col-12 col-md-4">
                <label class="form-label" for="prod-sku">Product SKU Code <span class="required-asterisk">*</span></label>
                <input type="text" id="prod-sku" class="form-control font-monospace" value="${prod?.sku || ''}" placeholder="e.g. PROD-LAP-009" required>
              </div>
              <div class="col-12 col-md-5">
                <label class="form-label" for="prod-name">Product Name <span class="required-asterisk">*</span></label>
                <input type="text" id="prod-name" class="form-control" value="${prod?.name || ''}" placeholder="e.g. Ultra Gaming Monitor 27\"" required>
              </div>
              <div class="col-12 col-md-3">
                <label class="form-label" for="prod-category">Category <span class="required-asterisk">*</span></label>
                <select id="prod-category" class="form-select" required>
                  <option value="">Select Category</option>
                  <option value="Electronics" ${prod?.category === 'Electronics' ? 'selected' : ''}>Electronics</option>
                  <option value="Peripherals" ${prod?.category === 'Peripherals' ? 'selected' : ''}>Peripherals</option>
                  <option value="Furniture" ${prod?.category === 'Furniture' ? 'selected' : ''}>Furniture</option>
                  <option value="Networking" ${prod?.category === 'Networking' ? 'selected' : ''}>Networking</option>
                  <option value="Software" ${prod?.category === 'Software' ? 'selected' : ''}>Software</option>
                  <option value="General" ${prod?.category === 'General' ? 'selected' : ''}>General</option>
                </select>
              </div>
            </div>

            <div class="row g-3 mb-4">
              <div class="col-12">
                <label class="form-label" for="prod-desc">Description</label>
                <textarea id="prod-desc" class="form-control" rows="2" placeholder="Product specifications, model details, etc.">${prod?.description || ''}</textarea>
              </div>
            </div>

            <div class="form-section-title"><i class="bi bi-tag me-2"></i>Pricing & Inventory Controls</div>

            <div class="row g-3 mb-3">
              <div class="col-12 col-md-4">
                <label class="form-label" for="prod-unit-price">Selling Unit Price (₹) <span class="required-asterisk">*</span></label>
                <input type="number" step="0.01" min="0.01" id="prod-unit-price" class="form-control" value="${prod?.unitPrice || ''}" placeholder="0.00" required>
              </div>
              <div class="col-12 col-md-4">
                <label class="form-label" for="prod-cost-price">Cost / Purchase Price (₹) <span class="required-asterisk">*</span></label>
                <input type="number" step="0.01" min="0" id="prod-cost-price" class="form-control" value="${prod?.costPrice || ''}" placeholder="0.00" required>
              </div>
              <div class="col-12 col-md-4">
                <label class="form-label" for="prod-uom">Unit of Measure <span class="required-asterisk">*</span></label>
                <input type="text" id="prod-uom" class="form-control" value="${prod?.unitOfMeasure || 'Unit'}" required>
              </div>
            </div>

            <div class="row g-3 mb-4">
              <div class="col-12 col-md-4">
                <label class="form-label" for="prod-reorder">Low Stock Alert Level <span class="required-asterisk">*</span></label>
                <input type="number" min="0" id="prod-reorder" class="form-control" value="${prod?.reorderLevel || 10}" required>
              </div>
              ${!isEdit ? `
                <div class="col-12 col-md-4">
                  <label class="form-label" for="prod-initial-stock">Initial Stock On Hand</label>
                  <input type="number" min="0" id="prod-initial-stock" class="form-control" value="0">
                </div>
              ` : ''}
              <div class="col-12 col-md-4">
                <label class="form-label" for="prod-status">Catalog Status <span class="required-asterisk">*</span></label>
                <select id="prod-status" class="form-select" required>
                  <option value="Active" ${prod?.status === 'Active' ? 'selected' : ''}>Active</option>
                  <option value="Inactive" ${prod?.status === 'Inactive' ? 'selected' : ''}>Inactive</option>
                </select>
              </div>
            </div>

            <div class="d-flex flex-column-reverse flex-sm-row justify-content-end gap-2 border-top pt-3">
              <a href="#products" class="btn btn-erp-secondary">Cancel</a>
              <button type="submit" class="btn btn-erp-primary" id="btn-save-prod">
                <i class="bi bi-check2 me-1"></i>${isEdit ? 'Save Changes' : 'Create Product'}
              </button>
            </div>
          </form>
        </div>
      </div>
    `;
  },

  async submitForm(event, id) {
    event.preventDefault();
    const btn = document.getElementById('btn-save-prod');
    btn.disabled = true;

    const payload = {
      sku: document.getElementById('prod-sku').value.trim().toUpperCase(),
      name: document.getElementById('prod-name').value.trim(),
      category: document.getElementById('prod-category').value,
      description: document.getElementById('prod-desc').value.trim(),
      unitPrice: parseFloat(document.getElementById('prod-unit-price').value) || 0,
      costPrice: parseFloat(document.getElementById('prod-cost-price').value) || 0,
      unitOfMeasure: document.getElementById('prod-uom').value.trim(),
      reorderLevel: parseInt(document.getElementById('prod-reorder').value, 10) || 10,
      status: document.getElementById('prod-status').value
    };

    if (!id) {
      payload.initialStock = parseInt(document.getElementById('prod-initial-stock')?.value, 10) || 0;
    }

    try {
      if (id) {
        await Api.put(`/products/${id}`, payload);
        App.showToast('Product updated successfully.', 'success');
      } else {
        await Api.post('/products', payload);
        App.showToast('Product created successfully.', 'success');
      }
      window.location.hash = '#products';
    } catch (error) {
      App.showToast(error.message, 'danger');
    } finally {
      btn.disabled = false;
    }
  },

  confirmDelete(id, name) {
    App.confirmAction(
      'Delete Product',
      `Are you sure you want to permanently delete "${name}"?`,
      async () => {
        try {
          await Api.delete(`/products/${id}`);
          App.showToast(`Product "${name}" deleted.`, 'success');
          this.data = this.data.filter(p => p.productId !== id);
          this.filterList();
        } catch (error) {
          App.showToast(error.message, 'danger');
        }
      },
      'Delete Product'
    );
  }
};
