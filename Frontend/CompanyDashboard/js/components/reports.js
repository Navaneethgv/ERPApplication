// Reports & Analytics Component
const ReportsComponent = {
  activeTab: 'sales',

  async render(container) {
    container.innerHTML = `
      <div class="page-header-container d-print-none d-flex flex-column flex-sm-row justify-content-between align-items-start align-items-sm-center gap-2 mb-3 mb-md-4">
        <div>
          <h1 class="page-title">Executive Reports & Business Intelligence</h1>
          <p class="page-subtitle">Real-time financial, operational, and inventory performance analytics</p>
        </div>
        <div>
          <button type="button" class="btn btn-outline-secondary btn-sm" onclick="window.print()">
            <i class="bi bi-printer me-1"></i>Print Report
          </button>
        </div>
      </div>

      <!-- Report Tabs & Filters -->
      <div class="erp-card mb-4 d-print-none">
        <div class="erp-card-body p-3">
          <ul class="nav nav-pills flex-nowrap overflow-x-auto text-nowrap pb-2 mb-3" id="reports-nav">
            <li class="nav-item">
              <button class="nav-link ${this.activeTab === 'sales' ? 'active' : ''}" onclick="ReportsComponent.switchTab('sales')">
                <i class="bi bi-cart-check me-1"></i>Sales Analysis
              </button>
            </li>
            <li class="nav-item">
              <button class="nav-link ${this.activeTab === 'purchases' ? 'active' : ''}" onclick="ReportsComponent.switchTab('purchases')">
                <i class="bi bi-bag-check me-1"></i>Procurement & Vendors
              </button>
            </li>
            <li class="nav-item">
              <button class="nav-link ${this.activeTab === 'inventory' ? 'active' : ''}" onclick="ReportsComponent.switchTab('inventory')">
                <i class="bi bi-boxes me-1"></i>Inventory Valuation
              </button>
            </li>
            <li class="nav-item">
              <button class="nav-link ${this.activeTab === 'financial' ? 'active' : ''}" onclick="ReportsComponent.switchTab('financial')">
                <i class="bi bi-cash-stack me-1"></i>Financial Statement (P&L)
              </button>
            </li>
          </ul>

          <div class="row g-2 align-items-end border-top pt-3">
            <div class="col-12 col-sm-6 col-md-3">
              <label class="form-label small mb-1" for="rep-start-date">From Date:</label>
              <input type="date" id="rep-start-date" class="form-control form-control-sm">
            </div>
            <div class="col-12 col-sm-6 col-md-3">
              <label class="form-label small mb-1" for="rep-end-date">To Date:</label>
              <input type="date" id="rep-end-date" class="form-control form-control-sm">
            </div>
            <div class="col-12 col-md-6 mt-2 mt-md-0 d-flex flex-wrap gap-2">
              <button type="button" class="btn btn-erp-primary btn-sm" onclick="ReportsComponent.applyFilter()">
                <i class="bi bi-funnel me-1"></i>Apply Filters
              </button>
              <button type="button" class="btn btn-erp-secondary btn-sm" onclick="ReportsComponent.resetFilter()">
                Reset
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Report Content Container -->
      <div id="report-view-container">
        <div class="loading-spinner-container">
          <div class="spinner-border text-primary mb-2"></div>
          <p>Generating report metrics from Excel ledger...</p>
        </div>
      </div>
    `;

    await this.loadActiveTabReport();
  },

  switchTab(tab) {
    this.activeTab = tab;
    document.querySelectorAll('#reports-nav .nav-link').forEach(btn => {
      if (btn.textContent.toLowerCase().includes(tab)) {
        btn.classList.add('active');
      } else {
        btn.classList.remove('active');
      }
    });
    this.loadActiveTabReport();
  },

  applyFilter() {
    this.loadActiveTabReport();
  },

  resetFilter() {
    document.getElementById('rep-start-date').value = '';
    document.getElementById('rep-end-date').value = '';
    this.loadActiveTabReport();
  },

  getFilterQuery() {
    const start = document.getElementById('rep-start-date')?.value;
    const end = document.getElementById('rep-end-date')?.value;
    const params = new URLSearchParams();
    if (start) params.append('startDate', start);
    if (end) params.append('endDate', end);
    const qs = params.toString();
    return qs ? `?${qs}` : '';
  },

  async loadActiveTabReport() {
    Router.destroyCharts();
    const container = document.getElementById('report-view-container');
    if (!container) return;

    container.innerHTML = `
      <div class="loading-spinner-container">
        <div class="spinner-border text-primary mb-2"></div>
        <p>Calculating aggregates...</p>
      </div>
    `;

    const qs = this.getFilterQuery();

    try {
      if (this.activeTab === 'sales') {
        const data = await Api.get(`/reports/sales${qs}`);
        this.renderSalesReport(container, data);
      } else if (this.activeTab === 'purchases') {
        const data = await Api.get(`/reports/purchases${qs}`);
        this.renderPurchasesReport(container, data);
      } else if (this.activeTab === 'inventory') {
        const data = await Api.get(`/reports/inventory-valuation${qs}`);
        this.renderInventoryReport(container, data);
      } else if (this.activeTab === 'financial') {
        const data = await Api.get(`/reports/financial-summary${qs}`);
        this.renderFinancialReport(container, data);
      }
    } catch (error) {
      container.innerHTML = `
        <div class="alert alert-danger">
          <h5><i class="bi bi-exclamation-circle me-2"></i>Failed to Generate Report</h5>
          <p class="mb-0">${error.message}</p>
        </div>
      `;
    }
  },

  renderSalesReport(container, data) {
    container.innerHTML = `
      <!-- KPI Row -->
      <div class="row g-3 mb-4">
        <div class="col-12 col-sm-6 col-xl-3">
          <div class="kpi-card h-100">
            <div class="kpi-header"><span class="kpi-title">Total Orders</span><div class="kpi-icon-box icon-blue"><i class="bi bi-cart"></i></div></div>
            <div class="kpi-value">${data.totalOrders}</div>
          </div>
        </div>
        <div class="col-12 col-sm-6 col-xl-3">
          <div class="kpi-card h-100">
            <div class="kpi-header"><span class="kpi-title">Total Revenue</span><div class="kpi-icon-box icon-green"><i class="bi bi-cash"></i></div></div>
            <div class="kpi-value text-success">${App.formatCurrency(data.totalSalesAmount)}</div>
          </div>
        </div>
        <div class="col-12 col-sm-6 col-xl-3">
          <div class="kpi-card h-100">
            <div class="kpi-header"><span class="kpi-title">Average Order Value</span><div class="kpi-icon-box icon-purple"><i class="bi bi-calculator"></i></div></div>
            <div class="kpi-value">${App.formatCurrency(data.averageOrderValue)}</div>
          </div>
        </div>
        <div class="col-12 col-sm-6 col-xl-3">
          <div class="kpi-card h-100">
            <div class="kpi-header"><span class="kpi-title">Fulfilled Rate</span><div class="kpi-icon-box icon-teal"><i class="bi bi-check2-all"></i></div></div>
            <div class="kpi-value">${data.totalOrders > 0 ? Math.round((data.fulfilledOrders / data.totalOrders) * 100) : 0}%</div>
          </div>
        </div>
      </div>

      <!-- Charts Row -->
      <div class="row g-3 mb-4">
        <div class="col-12 col-lg-7">
          <div class="erp-card h-100">
            <div class="erp-card-header"><h6 class="erp-card-title mb-0">Sales Revenue by Month</h6></div>
            <div class="erp-card-body"><canvas id="rep-sales-trend" height="130"></canvas></div>
          </div>
        </div>
        <div class="col-12 col-lg-5">
          <div class="erp-card h-100">
            <div class="erp-card-header"><h6 class="erp-card-title mb-0">Revenue by Product Category</h6></div>
            <div class="erp-card-body d-flex justify-content-center align-items-center">
              <div style="max-height: 240px; width: 100%;"><canvas id="rep-sales-cat"></canvas></div>
            </div>
          </div>
        </div>
      </div>

      <!-- Orders Breakdown Table -->
      <div class="erp-card">
        <div class="erp-card-header"><h6 class="erp-card-title mb-0">Detailed Sales Orders</h6></div>
        <div class="table-responsive erp-table-wrapper">
          <table class="erp-table align-middle" style="min-width: 580px;">
            <thead>
              <tr>
                <th>Order #</th>
                <th>Customer</th>
                <th>Date</th>
                <th>Amount</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              ${(data.orders || []).map(o => `
                <tr>
                  <td class="fw-semibold font-monospace">${o.saleOrderNumber}</td>
                  <td>${o.customerName}</td>
                  <td>${App.formatDate(o.orderDate)}</td>
                  <td class="fw-bold">${App.formatCurrency(o.totalAmount)}</td>
                  <td><span class="badge-status badge-${o.status.toLowerCase()}">${o.status}</span></td>
                </tr>
              `).join('') || '<tr><td colspan="5" class="text-center text-muted">No records match the filter.</td></tr>'}
            </tbody>
          </table>
        </div>
      </div>
    `;

    // Charts
    const ctx1 = document.getElementById('rep-sales-trend');
    if (ctx1) {
      const c1 = new Chart(ctx1, {
        type: 'bar',
        data: {
          labels: data.monthlySalesTrendChart.labels,
          datasets: [{
            label: 'Sales Revenue (₹)',
            data: data.monthlySalesTrendChart.series[0]?.data || [],
            backgroundColor: 'rgba(37, 99, 235, 0.85)',
            borderRadius: 4
          }]
        },
        options: { responsive: true, scales: { y: { beginAtZero: true } } }
      });
      Router.registerChart(c1);
    }

    const ctx2 = document.getElementById('rep-sales-cat');
    if (ctx2) {
      const c2 = new Chart(ctx2, {
        type: 'doughnut',
        data: {
          labels: data.categorySalesChart.labels,
          datasets: [{
            data: data.categorySalesChart.values || [],
            backgroundColor: ['#2563eb', '#10b981', '#f59e0b', '#8b5cf6', '#ec4899', '#06b6d4']
          }]
        },
        options: { responsive: true, maintainAspectRatio: false }
      });
      Router.registerChart(c2);
    }
  },

  renderPurchasesReport(container, data) {
    container.innerHTML = `
      <div class="row g-3 mb-4">
        <div class="col-12 col-sm-6 col-xl-3">
          <div class="kpi-card h-100">
            <div class="kpi-header"><span class="kpi-title">Total POs</span><div class="kpi-icon-box icon-amber"><i class="bi bi-bag"></i></div></div>
            <div class="kpi-value">${data.totalPurchases}</div>
          </div>
        </div>
        <div class="col-12 col-sm-6 col-xl-3">
          <div class="kpi-card h-100">
            <div class="kpi-header"><span class="kpi-title">Procurement Spend</span><div class="kpi-icon-box icon-rose"><i class="bi bi-cash-stack"></i></div></div>
            <div class="kpi-value text-danger">${App.formatCurrency(data.totalPurchasedAmount)}</div>
          </div>
        </div>
        <div class="col-12 col-sm-6 col-xl-3">
          <div class="kpi-card h-100">
            <div class="kpi-header"><span class="kpi-title">Received POs</span><div class="kpi-icon-box icon-green"><i class="bi bi-box-seam"></i></div></div>
            <div class="kpi-value text-success">${data.receivedPurchases}</div>
          </div>
        </div>
        <div class="col-12 col-sm-6 col-xl-3">
          <div class="kpi-card h-100">
            <div class="kpi-header"><span class="kpi-title">Pending Delivery</span><div class="kpi-icon-box icon-blue"><i class="bi bi-truck"></i></div></div>
            <div class="kpi-value text-primary">${data.pendingPurchases}</div>
          </div>
        </div>
      </div>

      <div class="row g-3 mb-4">
        <div class="col-12 col-lg-6">
          <div class="erp-card h-100">
            <div class="erp-card-header"><h6 class="erp-card-title mb-0">Procurement Spend by Vendor</h6></div>
            <div class="erp-card-body d-flex justify-content-center align-items-center">
              <div style="max-height: 240px; width: 100%;"><canvas id="rep-supp-spend"></canvas></div>
            </div>
          </div>
        </div>
        <div class="col-12 col-lg-6">
          <div class="erp-card h-100">
            <div class="erp-card-header"><h6 class="erp-card-title mb-0">Recent Purchase Orders</h6></div>
            <div class="table-responsive erp-table-wrapper">
              <table class="erp-table align-middle" style="min-width: 480px;">
                <thead>
                  <tr>
                    <th>PO #</th>
                    <th>Supplier</th>
                    <th>Date</th>
                    <th>Amount</th>
                  </tr>
                </thead>
                <tbody>
                  ${(data.purchases || []).slice(0, 5).map(p => `
                    <tr>
                      <td class="fw-semibold font-monospace">${p.purchaseNumber}</td>
                      <td>${p.supplierName}</td>
                      <td>${App.formatDate(p.purchaseDate)}</td>
                      <td class="fw-bold">${App.formatCurrency(p.totalAmount)}</td>
                    </tr>
                  `).join('') || '<tr><td colspan="4" class="text-center text-muted">No records found.</td></tr>'}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    `;

    const ctx = document.getElementById('rep-supp-spend');
    if (ctx) {
      const c = new Chart(ctx, {
        type: 'pie',
        data: {
          labels: data.supplierSpendChart.labels,
          datasets: [{
            data: data.supplierSpendChart.values || [],
            backgroundColor: ['#f59e0b', '#3b82f6', '#10b981', '#8b5cf6', '#ec4899']
          }]
        },
        options: { responsive: true, maintainAspectRatio: false }
      });
      Router.registerChart(c);
    }
  },

  renderInventoryReport(container, data) {
    container.innerHTML = `
      <div class="row g-3 mb-4">
        <div class="col-12 col-sm-6 col-xl-3">
          <div class="kpi-card h-100">
            <div class="kpi-header"><span class="kpi-title">Catalog SKUs</span><div class="kpi-icon-box icon-teal"><i class="bi bi-box-seam"></i></div></div>
            <div class="kpi-value">${data.totalItems}</div>
          </div>
        </div>
        <div class="col-12 col-sm-6 col-xl-3">
          <div class="kpi-card h-100">
            <div class="kpi-header"><span class="kpi-title">Cost Valuation</span><div class="kpi-icon-box icon-blue"><i class="bi bi-tag"></i></div></div>
            <div class="kpi-value text-primary">${App.formatCurrency(data.totalCostValuation)}</div>
          </div>
        </div>
        <div class="col-12 col-sm-6 col-xl-3">
          <div class="kpi-card h-100">
            <div class="kpi-header"><span class="kpi-title">Retail Valuation</span><div class="kpi-icon-box icon-green"><i class="bi bi-cash-stack"></i></div></div>
            <div class="kpi-value text-success">${App.formatCurrency(data.totalRetailValuation)}</div>
          </div>
        </div>
        <div class="col-12 col-sm-6 col-xl-3">
          <div class="kpi-card h-100">
            <div class="kpi-header"><span class="kpi-title">Estimated Margin</span><div class="kpi-icon-box icon-purple"><i class="bi bi-percent"></i></div></div>
            <div class="kpi-value text-purple">${data.potentialProfitMargin}%</div>
          </div>
        </div>
      </div>

      <div class="erp-card">
        <div class="erp-card-header"><h6 class="erp-card-title mb-0">Stock Valuation Matrix</h6></div>
        <div class="table-responsive erp-table-wrapper">
          <table class="erp-table align-middle" style="min-width: 720px;">
            <thead>
              <tr>
                <th>SKU</th>
                <th>Product Name</th>
                <th>Category</th>
                <th>On Hand</th>
                <th>Cost Price</th>
                <th>Retail Price</th>
                <th>Total Cost</th>
                <th>Total Retail</th>
              </tr>
            </thead>
            <tbody>
              ${(data.inventoryItems || []).map(i => `
                <tr>
                  <td class="font-monospace text-primary fw-semibold">${i.sku}</td>
                  <td>${i.productName}</td>
                  <td><span class="badge bg-light text-dark border">${i.category}</span></td>
                  <td class="fw-bold">${i.quantityOnHand}</td>
                  <td>${App.formatCurrency(i.costPrice)}</td>
                  <td>${App.formatCurrency(i.unitPrice)}</td>
                  <td class="fw-bold text-dark">${App.formatCurrency(i.quantityOnHand * i.costPrice)}</td>
                  <td class="fw-bold text-success">${App.formatCurrency(i.quantityOnHand * i.unitPrice)}</td>
                </tr>
              `).join('')}
            </tbody>
          </table>
        </div>
      </div>
    `;
  },

  renderFinancialReport(container, data) {
    const netProfit = data.netProfit;

    container.innerHTML = `
      <div class="row g-3 mb-4">
        <div class="col-12 col-sm-6 col-xl-3">
          <div class="kpi-card h-100">
            <div class="kpi-header"><span class="kpi-title">Total Revenue (Inflow)</span><div class="kpi-icon-box icon-green"><i class="bi bi-arrow-down-left"></i></div></div>
            <div class="kpi-value text-success">${App.formatCurrency(data.totalIncome)}</div>
          </div>
        </div>
        <div class="col-12 col-sm-6 col-xl-3">
          <div class="kpi-card h-100">
            <div class="kpi-header"><span class="kpi-title">Total Expenses (Outflow)</span><div class="kpi-icon-box icon-rose"><i class="bi bi-arrow-up-right"></i></div></div>
            <div class="kpi-value text-danger">${App.formatCurrency(data.totalExpenses)}</div>
          </div>
        </div>
        <div class="col-12 col-sm-6 col-xl-3">
          <div class="kpi-card h-100">
            <div class="kpi-header"><span class="kpi-title">Net Operating Profit</span><div class="kpi-icon-box icon-blue"><i class="bi bi-pie-chart"></i></div></div>
            <div class="kpi-value ${netProfit >= 0 ? 'text-primary' : 'text-danger'}">${App.formatCurrency(netProfit)}</div>
          </div>
        </div>
        <div class="col-12 col-sm-6 col-xl-3">
          <div class="kpi-card h-100">
            <div class="kpi-header"><span class="kpi-title">Pending Receivables</span><div class="kpi-icon-box icon-amber"><i class="bi bi-receipt"></i></div></div>
            <div class="kpi-value">${App.formatCurrency(data.outstandingReceivables)}</div>
          </div>
        </div>
      </div>

      <div class="row g-3 mb-4">
        <div class="col-12 col-lg-6">
          <div class="erp-card h-100">
            <div class="erp-card-header"><h6 class="erp-card-title mb-0">Income vs Expense Distribution</h6></div>
            <div class="erp-card-body d-flex justify-content-center align-items-center">
              <div style="max-height: 240px; width: 100%;"><canvas id="rep-pnl-chart"></canvas></div>
            </div>
          </div>
        </div>
        <div class="col-12 col-lg-6">
          <div class="erp-card h-100">
            <div class="erp-card-header"><h6 class="erp-card-title mb-0">Balance Sheet Summary</h6></div>
            <div class="erp-card-body">
              <div class="table-responsive">
                <table class="table table-borderless small mb-0">
                  <tbody>
                    <tr class="border-bottom">
                      <td class="fw-semibold">Total Revenue Collected:</td>
                      <td class="text-end fw-bold text-success">${App.formatCurrency(data.totalIncome)}</td>
                    </tr>
                    <tr class="border-bottom">
                      <td class="fw-semibold">Total Expenses & Procurements:</td>
                      <td class="text-end fw-bold text-danger">- ${App.formatCurrency(data.totalExpenses)}</td>
                    </tr>
                    <tr class="border-bottom table-light">
                      <td class="fw-bold">Net Profit / Margin:</td>
                      <td class="text-end fw-bold ${netProfit >= 0 ? 'text-primary' : 'text-danger'}">${App.formatCurrency(netProfit)}</td>
                    </tr>
                    <tr class="border-bottom">
                      <td class="fw-semibold">Inventory Valuation (Cost):</td>
                      <td class="text-end fw-bold">${App.formatCurrency(data.totalInventoryValue)}</td>
                    </tr>
                    <tr>
                      <td class="fw-semibold">Accounts Receivable (Unpaid Invoices):</td>
                      <td class="text-end fw-bold">${App.formatCurrency(data.outstandingReceivables)}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      </div>
    `;

    const ctx = document.getElementById('rep-pnl-chart');
    if (ctx) {
      const c = new Chart(ctx, {
        type: 'pie',
        data: {
          labels: data.incomeVsExpenseChart.labels,
          datasets: [{
            data: data.incomeVsExpenseChart.values || [],
            backgroundColor: ['#10b981', '#ef4444', '#3b82f6']
          }]
        },
        options: { responsive: true, maintainAspectRatio: false }
      });
      Router.registerChart(c);
    }
  }
};
