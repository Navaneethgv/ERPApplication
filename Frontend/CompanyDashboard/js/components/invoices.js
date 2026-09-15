// Invoices Component
const InvoicesComponent = {
  data: [],
  filteredData: [],
  currentPage: 1,
  pageSize: 10,

  async render(container, action, id) {
    if (action === 'view' && id) {
      await this.renderDetailView(container, id);
    } else {
      await this.renderList(container);
    }
  },

  async renderList(container) {
    container.innerHTML = `
      <div class="loading-spinner-container">
        <div class="spinner-border text-primary mb-2"></div>
        <p>Loading billing invoices...</p>
      </div>
    `;

    try {
      this.data = await Api.get('/invoices');
      this.filteredData = [...this.data];
      this.currentPage = 1;
      this.renderTable(container);
    } catch (error) {
      container.innerHTML = `
        <div class="alert alert-danger">
          <h5><i class="bi bi-exclamation-circle me-2"></i>Failed to Load Invoices</h5>
          <p class="mb-0">${error.message}</p>
        </div>
      `;
    }
  },

  renderTable(container) {
    const isCustomer = Auth.isCustomer();
    const totalInvoiced = this.data.reduce((sum, i) => sum + i.totalAmount, 0);
    const totalPaid = this.data.reduce((sum, i) => sum + i.paidAmount, 0);
    const totalBalance = this.data.reduce((sum, i) => sum + i.balanceAmount, 0);

    const start = (this.currentPage - 1) * this.pageSize;
    const pageItems = this.filteredData.slice(start, start + this.pageSize);

    container.innerHTML = `
      <div class="page-header-container">
        <div>
          <h1 class="page-title">${isCustomer ? 'My Invoices & Billing' : 'Invoices & Accounts Receivable'}</h1>
          <p class="page-subtitle">${isCustomer ? 'Review outstanding balances and process payments' : 'Track client billings, payment settlements, and overdue invoices'}</p>
        </div>
      </div>

      <!-- KPI Summary -->
      <div class="row g-3 mb-4">
        <div class="col-sm-6 col-xl-4">
          <div class="kpi-card">
            <div class="kpi-header">
              <span class="kpi-title">Total Invoiced</span>
              <div class="kpi-icon-box icon-blue"><i class="bi bi-receipt"></i></div>
            </div>
            <div class="kpi-value text-primary">${App.formatCurrency(totalInvoiced)}</div>
            <div class="kpi-subtitle text-muted">All generated invoices</div>
          </div>
        </div>

        <div class="col-sm-6 col-xl-4">
          <div class="kpi-card">
            <div class="kpi-header">
              <span class="kpi-title">Collected Revenue</span>
              <div class="kpi-icon-box icon-green"><i class="bi bi-check-circle"></i></div>
            </div>
            <div class="kpi-value text-success">${App.formatCurrency(totalPaid)}</div>
            <div class="kpi-subtitle text-muted">Total settled payments</div>
          </div>
        </div>

        <div class="col-sm-6 col-xl-4">
          <div class="kpi-card">
            <div class="kpi-header">
              <span class="kpi-title">Outstanding Balance</span>
              <div class="kpi-icon-box icon-rose"><i class="bi bi-clock-history"></i></div>
            </div>
            <div class="kpi-value ${totalBalance > 0 ? 'text-danger' : 'text-success'}">${App.formatCurrency(totalBalance)}</div>
            <div class="kpi-subtitle text-muted">Pending receivables</div>
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
                <input type="text" id="inv-search" class="form-control" placeholder="Search by invoice #, order #, customer name..." oninput="InvoicesComponent.filterList()">
              </div>
            </div>
            <div class="col-md-3 col-lg-3">
              <select id="inv-status-filter" class="form-select form-select-sm" onchange="InvoicesComponent.filterList()">
                <option value="">All Statuses</option>
                <option value="Unpaid">Unpaid</option>
                <option value="PartiallyPaid">Partially Paid</option>
                <option value="Paid">Paid (Settled)</option>
              </select>
            </div>
            <div class="col-auto ms-auto text-muted small">
              Total: <strong id="inv-total-count">${this.filteredData.length}</strong> invoices
            </div>
          </div>
        </div>
      </div>

      <!-- Table -->
      <div class="erp-card">
        <div class="erp-table-wrapper">
          <table class="erp-table" id="invoices-table">
            <thead>
              <tr>
                <th class="text-nowrap" style="min-width: 120px;">Invoice #</th>
                <th class="text-nowrap" style="min-width: 110px;">Order Ref</th>
                ${!isCustomer ? '<th style="min-width: 160px;">Customer</th>' : ''}
                <th class="text-nowrap">Issue Date</th>
                <th class="text-nowrap">Due Date</th>
                <th class="text-end text-nowrap">Total Amount</th>
                <th class="text-end text-nowrap">Paid Amount</th>
                <th class="text-end text-nowrap">Balance Due</th>
                <th class="text-center text-nowrap">Status</th>
                <th class="text-end text-nowrap" style="min-width: 140px;">Actions</th>
              </tr>
            </thead>
            <tbody id="invoices-table-body">
              ${this.buildTableRows(pageItems)}
            </tbody>
          </table>
        </div>
        <div id="invoices-pagination">
          ${App.renderPagination({
            currentPage: this.currentPage,
            pageSize: this.pageSize,
            totalItems: this.filteredData.length,
            componentName: 'InvoicesComponent'
          })}
        </div>
      </div>
    `;
  },

  buildTableRows(list) {
    if (!list || list.length === 0) {
      return `<tr><td colspan="10" class="text-center text-muted py-4"><i class="bi bi-receipt-cutoff fs-2 d-block mb-2 text-muted"></i>No invoices found.</td></tr>`;
    }

    const isCustomer = Auth.isCustomer();

    return list.map(i => `
      <tr>
        <td class="fw-semibold font-monospace text-nowrap">
          <a href="#invoices/view/${i.invoiceId}">${i.invoiceNumber}</a>
        </td>
        <td class="font-monospace text-muted small text-nowrap">${i.saleOrderNumber}</td>
        ${!isCustomer ? `<td><div class="fw-semibold text-dark text-truncate" style="max-width: 200px;" title="${i.customerName}">${i.customerName}</div></td>` : ''}
        <td class="text-muted small text-nowrap">${App.formatDate(i.issueDate)}</td>
        <td class="text-muted small text-nowrap">${App.formatDate(i.dueDate)}</td>
        <td class="fw-bold text-end text-nowrap">${App.formatCurrency(i.totalAmount)}</td>
        <td class="text-success fw-semibold text-end text-nowrap">${App.formatCurrency(i.paidAmount)}</td>
        <td class="fw-bold text-end text-nowrap ${i.balanceAmount > 0 ? 'text-danger' : 'text-success'}">${App.formatCurrency(i.balanceAmount)}</td>
        <td class="text-center text-nowrap"><span class="badge-status badge-${i.status.toLowerCase()}">${i.status}</span></td>
        <td class="text-end text-nowrap">
          <div class="table-actions">
            <a href="#invoices/view/${i.invoiceId}" class="btn btn-outline-primary btn-sm" title="View Printable Invoice">
              <i class="bi bi-eye"></i> View
            </a>
            ${i.balanceAmount > 0 ? `
              <button type="button" class="btn btn-success btn-sm" title="Record Payment" onclick="InvoicesComponent.openPaymentModal(${i.invoiceId}, '${i.invoiceNumber}', ${i.balanceAmount})">
                <i class="bi bi-cash-stack"></i> Pay
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
    const tbody = document.getElementById('invoices-table-body');
    if (tbody) {
      tbody.innerHTML = this.buildTableRows(pageItems);
    }
    const pagContainer = document.getElementById('invoices-pagination');
    if (pagContainer) {
      pagContainer.innerHTML = App.renderPagination({
        currentPage: this.currentPage,
        pageSize: this.pageSize,
        totalItems: this.filteredData.length,
        componentName: 'InvoicesComponent'
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
    const q = (document.getElementById('inv-search')?.value || '').toLowerCase();
    const status = document.getElementById('inv-status-filter')?.value || '';

    this.filteredData = this.data.filter(i => {
      const matchQ = !q || i.invoiceNumber.toLowerCase().includes(q) || i.saleOrderNumber.toLowerCase().includes(q) || i.customerName.toLowerCase().includes(q);
      const matchStatus = !status || i.status === status;
      return matchQ && matchStatus;
    });

    this.currentPage = 1;
    this.updateTableView();
  },

  async renderDetailView(container, id) {
    container.innerHTML = `
      <div class="loading-spinner-container">
        <div class="spinner-border text-primary mb-2"></div>
        <p>Loading invoice details...</p>
      </div>
    `;

    try {
      const [invoice, sale] = await Promise.all([
        Api.get(`/invoices/${id}`),
        Api.get('/sales') // Find matching sale
      ]);

      const matchingSale = sale.find(s => s.saleId === invoice.saleId || s.saleOrderNumber === invoice.saleOrderNumber);

      container.innerHTML = `
        <div class="page-header-container d-print-none">
          <div>
            <h1 class="page-title">Invoice ${invoice.invoiceNumber}</h1>
            <p class="page-subtitle">Billing statement for ${invoice.customerName}</p>
          </div>
          <div>
            <a href="#invoices" class="btn btn-erp-secondary btn-sm me-2">
              <i class="bi bi-arrow-left me-1"></i>Back
            </a>
            <button type="button" class="btn btn-outline-secondary btn-sm me-2" onclick="window.print()">
              <i class="bi bi-printer me-1"></i>Print / PDF
            </button>
            ${(Auth.hasPermission('invoices', 'EDIT') && invoice.balanceAmount > 0) ? `
              <button type="button" class="btn btn-success btn-sm" onclick="InvoicesComponent.openPaymentModal(${invoice.invoiceId}, '${invoice.invoiceNumber}', ${invoice.balanceAmount})">
                <i class="bi bi-cash-stack me-1"></i>Record Payment
              </button>
            ` : ''}
          </div>
        </div>

        <!-- Printable Invoice Sheet -->
        <div class="erp-card p-4 p-md-5">
          <div class="d-flex justify-content-between align-items-start border-bottom pb-4 mb-4">
            <div>
              <h2 class="fw-bold text-primary mb-1"><i class="bi bi-boxes me-2"></i>ApexERP</h2>
              <div class="text-muted small">Apex Enterprise Solutions Inc.</div>
              <div class="text-muted small">100 Enterprise Way, Suite 400</div>
              <div class="text-muted small">contact@erp.com &bull; +1 (800) 555-0199</div>
            </div>
            <div class="text-end">
              <h3 class="fw-bold text-dark mb-1">INVOICE</h3>
              <div class="font-monospace fw-semibold text-primary fs-6">${invoice.invoiceNumber}</div>
              <div class="text-muted small mt-1">Status: <span class="badge-status badge-${invoice.status.toLowerCase()}">${invoice.status}</span></div>
            </div>
          </div>

          <div class="row mb-4">
            <div class="col-sm-6 mb-3 mb-sm-0">
              <h6 class="text-muted text-uppercase small fw-bold mb-2">Billed To:</h6>
              <div class="fw-bold text-dark fs-6">${invoice.customerName}</div>
              <div class="text-muted small">Sales Order Reference: <strong>${invoice.saleOrderNumber}</strong></div>
            </div>
            <div class="col-sm-6 text-sm-end">
              <div class="mb-1"><span class="text-muted small">Issue Date:</span> <strong>${App.formatDate(invoice.issueDate)}</strong></div>
              <div class="mb-1"><span class="text-muted small">Payment Due Date:</span> <strong class="text-danger">${App.formatDate(invoice.dueDate)}</strong></div>
            </div>
          </div>

          <!-- Items Table -->
          <div class="erp-table-wrapper mb-4">
            <table class="erp-table">
              <thead>
                <tr>
                  <th>SKU</th>
                  <th>Item Description</th>
                  <th class="text-center">Qty</th>
                  <th class="text-end">Unit Price</th>
                  <th class="text-end">Total</th>
                </tr>
              </thead>
              <tbody>
                ${(matchingSale?.items || []).map(item => `
                  <tr>
                    <td class="font-monospace text-primary">${item.sku}</td>
                    <td>${item.productName}</td>
                    <td class="text-center">${item.quantity}</td>
                    <td class="text-end">${App.formatCurrency(item.unitPrice)}</td>
                    <td class="text-end fw-bold">${App.formatCurrency(item.totalPrice)}</td>
                  </tr>
                `).join('') || `
                  <tr>
                    <td colspan="4">Total order billing</td>
                    <td class="text-end fw-bold">${App.formatCurrency(invoice.totalAmount)}</td>
                  </tr>
                `}
              </tbody>
              <tfoot>
                <tr>
                  <td colspan="4" class="text-end fw-semibold">Subtotal:</td>
                  <td class="text-end fw-semibold">${App.formatCurrency(invoice.totalAmount)}</td>
                </tr>
                <tr>
                  <td colspan="4" class="text-end fw-semibold text-success">Total Amount Paid:</td>
                  <td class="text-end fw-semibold text-success">- ${App.formatCurrency(invoice.paidAmount)}</td>
                </tr>
                <tr class="table-light">
                  <td colspan="4" class="text-end fw-bold fs-6">Balance Due:</td>
                  <td class="text-end fw-bold fs-6 ${invoice.balanceAmount > 0 ? 'text-danger' : 'text-success'}">
                    ${App.formatCurrency(invoice.balanceAmount)}
                  </td>
                </tr>
              </tfoot>
            </table>
          </div>

          <div class="p-3 bg-light rounded border text-muted small">
            <strong>Payment Terms & Remittance:</strong> Payments can be made via ACH, Wire Transfer, or Corporate Cheque. Please include Invoice #<code>${invoice.invoiceNumber}</code> on all remittance advice.
          </div>
        </div>
      `;
    } catch (error) {
      App.showToast(error.message, 'danger');
      window.location.hash = '#invoices';
    }
  },

  openPaymentModal(id, invoiceNumber, balance) {
    document.getElementById('pay-invoice-id').value = id;
    document.getElementById('pay-modal-inv-num').textContent = invoiceNumber;
    document.getElementById('pay-modal-balance').textContent = App.formatCurrency(balance);
    document.getElementById('pay-amount').value = balance.toFixed(2);
    document.getElementById('pay-amount').max = balance;
    document.getElementById('pay-notes').value = '';

    const modalEl = document.getElementById('paymentModal');
    const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
    modal.show();
  },

  async submitPayment(event) {
    event.preventDefault();
    const btn = document.getElementById('btn-save-payment');
    btn.disabled = true;

    const id = document.getElementById('pay-invoice-id').value;
    const payload = {
      amount: parseFloat(document.getElementById('pay-amount').value) || 0,
      paymentMethod: document.getElementById('pay-method').value,
      notes: document.getElementById('pay-notes').value.trim()
    };

    try {
      await Api.post(`/invoices/${id}/payments`, payload);
      const modalEl = document.getElementById('paymentModal');
      const modal = bootstrap.Modal.getInstance(modalEl);
      if (modal) modal.hide();

      App.showToast('Payment recorded successfully! Financial ledger updated.', 'success');
      Router.navigate();
    } catch (error) {
      App.showToast(error.message, 'danger');
    } finally {
      btn.disabled = false;
    }
  }
};
