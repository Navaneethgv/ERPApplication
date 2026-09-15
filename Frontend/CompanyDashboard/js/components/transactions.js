// Transactions & Payment History Component
const TransactionsComponent = {
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
        <p>Loading financial transactions...</p>
      </div>
    `;

    try {
      this.data = await Api.get('/transactions');
      this.filteredData = [...this.data];
      this.currentPage = 1;
      this.renderView(container);
    } catch (error) {
      container.innerHTML = `
        <div class="alert alert-danger">
          <h5><i class="bi bi-exclamation-circle me-2"></i>Failed to Load Transactions</h5>
          <p class="mb-0">${error.message}</p>
        </div>
      `;
    }
  },

  renderView(container) {
    const isCustomer = Auth.isCustomer();
    const isStaff = Auth.isAdmin() || Auth.isEmployee();

    const totalIncome = this.data.filter(t => t.type === 'Income').reduce((sum, t) => sum + t.amount, 0);
    const totalExpense = this.data.filter(t => t.type === 'Expense').reduce((sum, t) => sum + t.amount, 0);
    const netFlow = totalIncome - totalExpense;

    const start = (this.currentPage - 1) * this.pageSize;
    const pageItems = this.filteredData.slice(start, start + this.pageSize);

    container.innerHTML = `
      <div class="page-header-container d-flex flex-column flex-sm-row justify-content-between align-items-start align-items-sm-center gap-2 mb-3 mb-md-4">
        <div>
          <h1 class="page-title">${isCustomer ? 'My Payment History' : 'Financial Ledger & Transactions'}</h1>
          <p class="page-subtitle">${isCustomer ? 'Record of settled payments and invoice settlements' : 'Audited general ledger of revenue, procurement, and expenses'}</p>
        </div>
        <div>
          ${Auth.hasPermission('transactions', 'ADD') ? `
            <button type="button" class="btn btn-erp-primary btn-sm" onclick="TransactionsComponent.openNewTxnModal()">
              <i class="bi bi-plus-circle me-1"></i>Record Expense / Entry
            </button>
          ` : ''}
        </div>
      </div>

      <!-- Financial Summary KPI Cards -->
      <div class="row g-3 mb-4">
        <div class="col-12 col-sm-6 col-xl-4">
          <div class="kpi-card h-100">
            <div class="kpi-header">
              <span class="kpi-title">${isCustomer ? 'Total Payments Settled' : 'Total Cash Inflow (Revenue)'}</span>
              <div class="kpi-icon-box icon-green"><i class="bi bi-arrow-down-left"></i></div>
            </div>
            <div class="kpi-value text-success">${App.formatCurrency(totalIncome)}</div>
            <div class="kpi-subtitle text-muted">Customer receipts & revenue</div>
          </div>
        </div>

        ${!isCustomer ? `
          <div class="col-12 col-sm-6 col-xl-4">
            <div class="kpi-card h-100">
              <div class="kpi-header">
                <span class="kpi-title">Total Cash Outflow (Expenses)</span>
                <div class="kpi-icon-box icon-rose"><i class="bi bi-arrow-up-right"></i></div>
              </div>
              <div class="kpi-value text-danger">${App.formatCurrency(totalExpense)}</div>
              <div class="kpi-subtitle text-muted">Procurement & overheads</div>
            </div>
          </div>

          <div class="col-12 col-sm-6 col-xl-4">
            <div class="kpi-card h-100">
              <div class="kpi-header">
                <span class="kpi-title">Net Operating Cash Flow</span>
                <div class="kpi-icon-box icon-blue"><i class="bi bi-wallet2"></i></div>
              </div>
              <div class="kpi-value ${netFlow >= 0 ? 'text-primary' : 'text-danger'}">${App.formatCurrency(netFlow)}</div>
              <div class="kpi-subtitle text-muted">Inflow less Outflow</div>
            </div>
          </div>
        ` : ''}
      </div>

      <!-- Filter Row -->
      <div class="erp-card mb-3">
        <div class="erp-card-body p-3">
          <div class="row g-2 align-items-center">
            <div class="col-12 col-md-5 col-lg-4">
              <div class="input-group input-group-sm">
                <span class="input-group-text bg-light"><i class="bi bi-search"></i></span>
                <input type="text" id="txn-search" class="form-control" placeholder="${isCustomer ? 'Search by txn #, ref #, notes...' : 'Search by txn #, ref #, category, notes...'}" oninput="TransactionsComponent.filterList()">
              </div>
            </div>
            ${!isCustomer ? `
              <div class="col-12 col-sm-6 col-md-3">
                <select id="txn-type-filter" class="form-select form-select-sm" onchange="TransactionsComponent.filterList()">
                  <option value="">All Types (Income & Expense)</option>
                  <option value="Income">Income Only</option>
                  <option value="Expense">Expense Only</option>
                </select>
              </div>
            ` : ''}
            <div class="col-12 col-sm-auto ms-sm-auto text-muted small text-sm-end mt-2 mt-sm-0">
              Total: <strong id="txn-total-count">${this.filteredData.length}</strong> entries
            </div>
          </div>
        </div>
      </div>

      <!-- Table -->
      <div class="erp-card">
        <div class="table-responsive erp-table-wrapper">
          <table class="erp-table align-middle" id="transactions-table" style="min-width: 820px;">
            <thead>
              <tr>
                ${isCustomer ? `
                  <th class="text-nowrap" style="min-width: 140px;">Txn #</th>
                  <th class="text-nowrap" style="min-width: 130px;">Reference</th>
                  <th class="text-end text-nowrap" style="min-width: 130px;">Amount</th>
                  <th class="text-center text-nowrap" style="min-width: 145px;">Payment Method</th>
                  <th class="text-nowrap" style="min-width: 115px;">Date</th>
                  <th class="text-nowrap" style="min-width: 105px;">Time</th>
                  <th class="text-center text-nowrap" style="min-width: 120px;">Status</th>
                  <th style="min-width: 250px;">Notes</th>
                  <th class="text-center text-nowrap" style="min-width: 100px;">Actions</th>
                ` : `
                  <th class="text-nowrap" style="min-width: 140px;">Txn #</th>
                  <th class="text-nowrap" style="min-width: 130px;">Reference</th>
                  <th class="text-center text-nowrap" style="min-width: 115px;">Type</th>
                  <th class="text-nowrap" style="min-width: 140px;">Category</th>
                  <th class="text-end text-nowrap" style="min-width: 130px;">Amount</th>
                  <th class="text-center text-nowrap" style="min-width: 145px;">Payment Method</th>
                  <th class="text-nowrap" style="min-width: 165px;">Date & Time</th>
                  <th class="text-center text-nowrap" style="min-width: 120px;">Status</th>
                  <th style="min-width: 250px;">Notes</th>
                  <th class="text-center text-nowrap" style="min-width: 100px;">Actions</th>
                `}
              </tr>
            </thead>
            <tbody id="transactions-table-body">
              ${this.buildTableRows(pageItems)}
            </tbody>
          </table>
        </div>
        <div id="transactions-pagination">
          ${App.renderPagination({
            currentPage: this.currentPage,
            pageSize: this.pageSize,
            totalItems: this.filteredData.length,
            componentName: 'TransactionsComponent'
          })}
        </div>
      </div>

      <!-- New Transaction Modal (Dynamically Available) -->
      <div class="modal fade" id="newTxnModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered modal-dialog-scrollable">
          <div class="modal-content border-0 shadow">
            <div class="modal-header">
              <h5 class="modal-title fw-bold"><i class="bi bi-wallet2 text-primary me-2"></i>Record Transaction / Expense</h5>
              <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <form id="new-txn-form" onsubmit="TransactionsComponent.submitNewTxn(event)">
              <div class="modal-body">
                <div class="row g-3 mb-3">
                  <div class="col-12 col-sm-6">
                    <label class="form-label" for="nt-type">Transaction Type <span class="required-asterisk">*</span></label>
                    <select id="nt-type" class="form-select" required>
                      <option value="Expense">Expense</option>
                      <option value="Income">Income</option>
                    </select>
                  </div>
                  <div class="col-12 col-sm-6">
                    <label class="form-label" for="nt-amount">Amount (₹) <span class="required-asterisk">*</span></label>
                    <input type="number" step="0.01" min="0.01" id="nt-amount" class="form-control" placeholder="0.00" required>
                  </div>
                </div>
                <div class="row g-3 mb-3">
                  <div class="col-12 col-sm-6">
                    <label class="form-label" for="nt-category">Category <span class="required-asterisk">*</span></label>
                    <input type="text" id="nt-category" class="form-control" placeholder="e.g. Office Supplies, Shipping" required>
                  </div>
                  <div class="col-12 col-sm-6">
                    <label class="form-label" for="nt-method">Payment Method <span class="required-asterisk">*</span></label>
                    <select id="nt-method" class="form-select" required>
                      <option value="Bank Transfer">Bank Transfer</option>
                      <option value="Credit Card">Credit Card</option>
                      <option value="Cash">Cash</option>
                      <option value="Cheque">Cheque</option>
                      <option value="UPI">UPI / Digital</option>
                    </select>
                  </div>
                </div>
                <div class="mb-3">
                  <label class="form-label" for="nt-ref">Reference Number</label>
                  <input type="text" id="nt-ref" class="form-control font-monospace" placeholder="e.g. INV-2024-001 or CHQ-9923">
                </div>
                <div class="mb-2">
                  <label class="form-label" for="nt-notes">Notes / Memo</label>
                  <textarea id="nt-notes" class="form-control" rows="2" placeholder="Describe purpose or vendor information..."></textarea>
                </div>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-erp-secondary btn-sm" data-bs-dismiss="modal">Cancel</button>
                <button type="submit" class="btn btn-erp-primary btn-sm" id="btn-save-nt">
                  <i class="bi bi-check2-circle me-1"></i>Save Entry
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    `;
  },

  buildTableRows(list) {
    const isCustomer = Auth.isCustomer();
    const colSpan = isCustomer ? 9 : 10;

    if (!list || list.length === 0) {
      return `<tr><td colspan="${colSpan}" class="text-center text-muted py-4"><i class="bi bi-journal-x fs-2 d-block mb-2 text-muted"></i>No transactions recorded.</td></tr>`;
    }

    if (isCustomer) {
      return list.map(t => `
        <tr>
          <td class="fw-semibold font-monospace text-nowrap">
            <a href="#transactions/view/${t.transactionId}">${t.transactionNumber}</a>
          </td>
          <td class="font-monospace text-muted text-nowrap">${t.referenceNumber || '-'}</td>
          <td class="fw-bold text-success text-end text-nowrap">+${App.formatCurrency(t.amount)}</td>
          <td class="text-center text-nowrap"><span class="badge bg-light text-dark border px-2 py-1">${t.paymentMethod || 'Bank Transfer'}</span></td>
          <td class="text-muted small text-nowrap">${App.formatDate(t.transactionDate)}</td>
          <td class="text-muted small text-nowrap">${App.formatTime(t.transactionDate)}</td>
          <td class="text-center text-nowrap"><span class="badge-status badge-completed">${t.status}</span></td>
          <td style="min-width: 250px; max-width: 420px; white-space: normal; word-break: break-word; overflow-wrap: break-word;">
            <div class="text-muted small" style="line-height: 1.45;">${t.notes || '-'}</div>
          </td>
          <td class="text-center text-nowrap">
            <a href="#transactions/view/${t.transactionId}" class="btn btn-outline-primary btn-sm" title="View Payment Receipt">
              <i class="bi bi-eye"></i> View
            </a>
          </td>
        </tr>
      `).join('');
    }

    return list.map(t => `
      <tr>
        <td class="fw-semibold font-monospace text-nowrap">
          <a href="#transactions/view/${t.transactionId}">${t.transactionNumber}</a>
        </td>
        <td class="font-monospace text-muted text-nowrap">${t.referenceNumber || '-'}</td>
        <td class="text-center text-nowrap">
          <span class="badge-status ${t.type === 'Income' ? 'badge-completed' : 'badge-lowstock'}">
            ${t.type === 'Income' ? '<i class="bi bi-arrow-down-left"></i> Income' : '<i class="bi bi-arrow-up-right"></i> Expense'}
          </span>
        </td>
        <td class="fw-semibold text-dark text-nowrap" title="${t.category}">${t.category}</td>
        <td class="fw-bold text-end text-nowrap ${t.type === 'Income' ? 'text-success' : 'text-danger'}">
          ${t.type === 'Income' ? '+' : '-'}${App.formatCurrency(t.amount)}
        </td>
        <td class="text-center text-nowrap"><span class="badge bg-light text-dark border px-2 py-1">${t.paymentMethod}</span></td>
        <td class="text-muted small text-nowrap">${App.formatDateTime(t.transactionDate)}</td>
        <td class="text-center text-nowrap"><span class="badge-status badge-completed">${t.status}</span></td>
        <td style="min-width: 250px; max-width: 420px; white-space: normal; word-break: break-word; overflow-wrap: break-word;">
          <div class="text-muted small" style="line-height: 1.45;">${t.notes || '-'}</div>
        </td>
        <td class="text-center text-nowrap">
          <a href="#transactions/view/${t.transactionId}" class="btn btn-outline-primary btn-sm" title="View Transaction Details">
            <i class="bi bi-eye"></i> View
          </a>
        </td>
      </tr>
    `).join('');
  },

  updateTableView() {
    const start = (this.currentPage - 1) * this.pageSize;
    const pageItems = this.filteredData.slice(start, start + this.pageSize);
    const tbody = document.getElementById('transactions-table-body');
    if (tbody) {
      tbody.innerHTML = this.buildTableRows(pageItems);
    }
    const pagContainer = document.getElementById('transactions-pagination');
    if (pagContainer) {
      pagContainer.innerHTML = App.renderPagination({
        currentPage: this.currentPage,
        pageSize: this.pageSize,
        totalItems: this.filteredData.length,
        componentName: 'TransactionsComponent'
      });
    }
    const totalCount = document.getElementById('txn-total-count');
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
    const q = (document.getElementById('txn-search')?.value || '').toLowerCase();
    const type = document.getElementById('txn-type-filter')?.value || '';

    this.filteredData = this.data.filter(t => {
      const matchQ = !q || t.transactionNumber.toLowerCase().includes(q) || (t.referenceNumber && t.referenceNumber.toLowerCase().includes(q)) || (t.category && t.category.toLowerCase().includes(q)) || (t.notes && t.notes.toLowerCase().includes(q));
      const matchType = !type || t.type === type;
      return matchQ && matchType;
    });

    this.currentPage = 1;
    this.updateTableView();
  },

  async renderDetailView(container, id) {
    container.innerHTML = `
      <div class="loading-spinner-container">
        <div class="spinner-border text-primary mb-2"></div>
        <p>Loading transaction details...</p>
      </div>
    `;

    try {
      const txn = await Api.get(`/transactions/${id}`);
      let matchingInvoice = null;

      // Look up linked invoice if reference exists
      if (txn.referenceNumber) {
        try {
          const invoices = await Api.get('/invoices');
          matchingInvoice = invoices.find(i => i.invoiceNumber === txn.referenceNumber);
        } catch {
          // Ignore invoice lookup failure if user doesn't have access or invoice not found
        }
      }

      const isCustomer = Auth.isCustomer();
      const isIncome = txn.type === 'Income';

      container.innerHTML = `
        <div class="page-header-container d-print-none d-flex flex-column flex-sm-row justify-content-between align-items-start align-items-sm-center gap-2 mb-3 mb-md-4">
          <div>
            <h1 class="page-title">${isCustomer ? 'Payment Receipt' : 'Transaction Details'} ${txn.transactionNumber}</h1>
            <p class="page-subtitle">${isIncome ? 'Payment confirmation & settled ledger entry' : 'Audited general ledger expenditure'}</p>
          </div>
          <div class="d-flex flex-wrap gap-2">
            <a href="#transactions" class="btn btn-erp-secondary btn-sm">
              <i class="bi bi-arrow-left me-1"></i>Back to List
            </a>
            <button type="button" class="btn btn-outline-secondary btn-sm" onclick="window.print()">
              <i class="bi bi-printer me-1"></i>Print / PDF
            </button>
            ${matchingInvoice ? `
              <a href="#invoices/view/${matchingInvoice.invoiceId}" class="btn btn-outline-primary btn-sm">
                <i class="bi bi-receipt me-1"></i>View Invoice #${matchingInvoice.invoiceNumber}
              </a>
              ${matchingInvoice.balanceAmount > 0 ? `
                <button type="button" class="btn btn-success btn-sm" onclick="InvoicesComponent.openPaymentModal(${matchingInvoice.invoiceId}, '${matchingInvoice.invoiceNumber}', ${matchingInvoice.balanceAmount})">
                  <i class="bi bi-cash-stack me-1"></i>Pay Balance
                </button>
              ` : ''}
            ` : ''}
          </div>
        </div>

        <!-- Printable Payment Receipt Card -->
        <div class="erp-card p-3 p-md-5">
          <div class="d-flex flex-column flex-sm-row justify-content-between align-items-start border-bottom pb-4 mb-4 gap-3">
            <div>
              <h2 class="fw-bold text-primary mb-1"><i class="bi bi-boxes me-2"></i>ApexERP</h2>
              <div class="text-muted small">Apex Enterprise Solutions Inc.</div>
              <div class="text-muted small">100 Enterprise Way, Suite 400</div>
              <div class="text-muted small">contact@erp.com &bull; +1 (800) 555-0199</div>
            </div>
            <div class="text-sm-end">
              <h3 class="fw-bold text-dark mb-1">${isIncome ? 'PAYMENT RECEIPT' : 'TRANSACTION VOUCHER'}</h3>
              <div class="font-monospace fw-semibold text-primary fs-6">${txn.transactionNumber}</div>
              <div class="text-muted small mt-1">Status: <span class="badge-status badge-completed">${txn.status}</span></div>
            </div>
          </div>

          <!-- Metadata Section -->
          <div class="row g-3 mb-4">
            <div class="col-12 col-sm-6">
              <h6 class="text-muted text-uppercase small fw-bold mb-2">${isIncome ? 'Payment For / Account:' : 'Expenditure Account:'}</h6>
              <div class="fw-bold text-dark fs-6">${matchingInvoice?.customerName || (isCustomer ? Auth.getUser()?.fullName : 'Apex General Ledger')}</div>
              ${txn.referenceNumber ? `<div class="text-muted small mt-1">Reference Document: <strong>${txn.referenceNumber}</strong></div>` : ''}
              ${matchingInvoice?.saleOrderNumber ? `<div class="text-muted small">Sales Order Reference: <strong>${matchingInvoice.saleOrderNumber}</strong></div>` : ''}
              ${!isCustomer ? `<div class="text-muted small">Recorded By: <strong>${txn.createdBy || 'System'}</strong></div>` : ''}
            </div>
            <div class="col-12 col-sm-6 text-sm-end">
              <div class="mb-1"><span class="text-muted small">Payment Date:</span> <strong>${App.formatDate(txn.transactionDate)}</strong></div>
              <div class="mb-1"><span class="text-muted small">Processing Time:</span> <strong>${App.formatTime(txn.transactionDate)}</strong></div>
              <div class="mb-1"><span class="text-muted small">Payment Method:</span> <span class="badge bg-light text-dark border ms-1">${txn.paymentMethod || 'Bank Transfer'}</span></div>
              <div class="mb-1"><span class="text-muted small">Classification:</span> <span class="badge-status ${isIncome ? 'badge-completed' : 'badge-lowstock'} ms-1">${txn.type}</span></div>
            </div>
          </div>

          <!-- Settlement Summary Table -->
          <div class="table-responsive erp-table-wrapper mb-4">
            <table class="erp-table align-middle" style="min-width: 580px;">
              <thead>
                <tr>
                  <th style="min-width: 140px;">Transaction #</th>
                  <th>Description / Allocation</th>
                  <th class="text-nowrap" style="min-width: 140px;">Reference #</th>
                  <th class="text-nowrap" style="min-width: 130px;">Payment Method</th>
                  <th class="text-end text-nowrap" style="min-width: 140px;">Settled Amount</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td class="font-monospace fw-semibold text-primary">${txn.transactionNumber}</td>
                  <td>
                    <div class="fw-semibold text-dark">${txn.category || (isIncome ? 'Sales Invoice Settlement' : 'Expense')}</div>
                    ${txn.notes ? `<div class="text-muted small mt-1">${txn.notes}</div>` : ''}
                  </td>
                  <td class="font-monospace text-muted">${txn.referenceNumber || '-'}</td>
                  <td><span class="badge bg-light text-dark border">${txn.paymentMethod || 'Bank Transfer'}</span></td>
                  <td class="text-end fw-bold fs-6 ${isIncome ? 'text-success' : 'text-danger'}">
                    ${isIncome ? '+' : '-'}${App.formatCurrency(txn.amount)}
                  </td>
                </tr>
              </tbody>
              <tfoot>
                <tr class="table-light">
                  <td colspan="4" class="text-end fw-bold fs-6">Total Settled Amount:</td>
                  <td class="text-end fw-bold fs-6 ${isIncome ? 'text-success' : 'text-danger'}">
                    ${App.formatCurrency(txn.amount)}
                  </td>
                </tr>
              </tfoot>
            </table>
          </div>

          <!-- Linked Invoice Summary Card if present -->
          ${matchingInvoice ? `
            <div class="erp-card mb-4 border bg-light">
              <div class="erp-card-body p-3">
                <div class="row g-2 align-items-center">
                  <div class="col-12 col-md-6 mb-2 mb-md-0">
                    <div class="d-flex align-items-center">
                      <i class="bi bi-receipt fs-3 text-primary me-3"></i>
                      <div>
                        <div class="fw-bold text-dark">Linked Invoice: ${matchingInvoice.invoiceNumber}</div>
                        <div class="text-muted small">Total: <strong>${App.formatCurrency(matchingInvoice.totalAmount)}</strong> &bull; Total Paid: <strong class="text-success">${App.formatCurrency(matchingInvoice.paidAmount)}</strong></div>
                      </div>
                    </div>
                  </div>
                  <div class="col-12 col-md-6 text-md-end">
                    <span class="badge-status badge-${matchingInvoice.status.toLowerCase()} me-2">${matchingInvoice.status}</span>
                    <span class="small me-2">Remaining Balance: <strong class="${matchingInvoice.balanceAmount > 0 ? 'text-danger' : 'text-success'}">${App.formatCurrency(matchingInvoice.balanceAmount)}</strong></span>
                    <a href="#invoices/view/${matchingInvoice.invoiceId}" class="btn btn-outline-primary btn-sm">
                      <i class="bi bi-box-arrow-up-right me-1"></i>View Invoice
                    </a>
                  </div>
                </div>
              </div>
            </div>
          ` : ''}

          <!-- Notes Callout -->
          ${txn.notes ? `
            <div class="p-3 bg-light rounded border text-muted small mb-4">
              <strong>Transaction Notes / Memo:</strong>
              <div class="mt-1">${txn.notes}</div>
            </div>
          ` : ''}

          <!-- Verification Notice -->
          <div class="p-3 rounded border text-muted small" style="background-color: #f8fafc;">
            <strong>Electronic Receipt Verification:</strong> This electronic voucher confirms the receipt of funds and successful financial reconciliation in the ApexERP Ledger. For inquiries, quote Transaction ID <code>${txn.transactionNumber}</code>.
          </div>
        </div>
      `;
    } catch (error) {
      App.showToast(error.message, 'danger');
      window.location.hash = '#transactions';
    }
  },

  openNewTxnModal() {
    document.getElementById('new-txn-form').reset();
    const modalEl = document.getElementById('newTxnModal');
    const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
    modal.show();
  },

  async submitNewTxn(event) {
    event.preventDefault();
    const btn = document.getElementById('btn-save-nt');
    btn.disabled = true;

    const payload = {
      referenceType: 'Other',
      referenceNumber: document.getElementById('nt-ref').value.trim(),
      type: document.getElementById('nt-type').value,
      category: document.getElementById('nt-category').value.trim(),
      amount: parseFloat(document.getElementById('nt-amount').value) || 0,
      paymentMethod: document.getElementById('nt-method').value,
      notes: document.getElementById('nt-notes').value.trim()
    };

    try {
      await Api.post('/transactions', payload);
      const modalEl = document.getElementById('newTxnModal');
      const modal = bootstrap.Modal.getInstance(modalEl);
      if (modal) modal.hide();

      App.showToast('Ledger entry recorded successfully.', 'success');
      this.render(document.getElementById('app-content'));
    } catch (error) {
      App.showToast(error.message, 'danger');
    } finally {
      btn.disabled = false;
    }
  }
};
