// Dashboard Component - Renders tailored views for Admin, Employee, and Customer
const DashboardComponent = {
  async render(container) {
    container.innerHTML = `
      <div class="loading-spinner-container">
        <div class="spinner-border text-primary mb-3" role="status"></div>
        <p>Loading real-time business analytics...</p>
      </div>
    `;

    try {
      const res = await Api.get('/dashboard/summary');
      const role = res.role;
      const data = res.data;

      if (role === 'Admin') {
        this.renderAdmin(container, data);
      } else if (role === 'Employee') {
        this.renderEmployee(container, data);
      } else if (role === 'Customer') {
        this.renderCustomer(container, data);
      }
    } catch (error) {
      container.innerHTML = `
        <div class="alert alert-danger shadow-sm">
          <h5><i class="bi bi-exclamation-octagon me-2"></i>Failed to Load Dashboard Data</h5>
          <p class="mb-0">${error.message}</p>
        </div>
      `;
    }
  },

  renderAdmin(container, data) {
    container.innerHTML = `
      <div class="page-header-container">
        <div>
          <h1 class="page-title">Executive Dashboard</h1>
          <p class="page-subtitle">High-level enterprise performance metrics and real-time ledger</p>
        </div>
        <div>
          <a href="#reports" class="btn btn-erp-primary btn-sm"><i class="bi bi-bar-chart me-1"></i>View Full Reports</a>
        </div>
      </div>

      <!-- KPI Cards Row 1 -->
      <div class="row g-3 mb-4">
        <div class="col-sm-6 col-xl-3">
          <div class="kpi-card">
            <div class="kpi-header">
              <span class="kpi-title">Total Employees</span>
              <div class="kpi-icon-box icon-purple"><i class="bi bi-people-fill"></i></div>
            </div>
            <div class="kpi-value">${data.totalEmployees}</div>
            <div class="kpi-subtitle text-muted"><a href="#employees" class="text-decoration-none text-muted">Manage Staff &rarr;</a></div>
          </div>
        </div>

        <div class="col-sm-6 col-xl-3">
          <div class="kpi-card">
            <div class="kpi-header">
              <span class="kpi-title">Active Customers</span>
              <div class="kpi-icon-box icon-blue"><i class="bi bi-building"></i></div>
            </div>
            <div class="kpi-value">${data.totalCustomers}</div>
            <div class="kpi-subtitle text-muted"><a href="#customers" class="text-decoration-none text-muted">Customer Directory &rarr;</a></div>
          </div>
        </div>

        <div class="col-sm-6 col-xl-3">
          <div class="kpi-card">
            <div class="kpi-header">
              <span class="kpi-title">Catalog Products</span>
              <div class="kpi-icon-box icon-teal"><i class="bi bi-boxes"></i></div>
            </div>
            <div class="kpi-value">${data.totalProducts}</div>
            <div class="kpi-subtitle text-muted"><a href="#products" class="text-decoration-none text-muted">Product Catalog &rarr;</a></div>
          </div>
        </div>

        <div class="col-sm-6 col-xl-3">
          <div class="kpi-card">
            <div class="kpi-header">
              <span class="kpi-title">Low Stock Items</span>
              <div class="kpi-icon-box icon-rose"><i class="bi bi-exclamation-triangle-fill"></i></div>
            </div>
            <div class="kpi-value text-danger">${data.lowStockCount}</div>
            <div class="kpi-subtitle text-muted"><a href="#inventory" class="text-decoration-none text-danger">Check Inventory &rarr;</a></div>
          </div>
        </div>
      </div>

      <!-- KPI Cards Row 2 (Financials) -->
      <div class="row g-3 mb-4">
        <div class="col-sm-6 col-xl-3">
          <div class="kpi-card">
            <div class="kpi-header">
              <span class="kpi-title">Total Sales Volume</span>
              <div class="kpi-icon-box icon-green"><i class="bi bi-cart-check-fill"></i></div>
            </div>
            <div class="kpi-value text-success">${App.formatCurrency(data.totalSalesAmount)}</div>
            <div class="kpi-subtitle text-muted">All active sales orders</div>
          </div>
        </div>

        <div class="col-sm-6 col-xl-3">
          <div class="kpi-card">
            <div class="kpi-header">
              <span class="kpi-title">Total Purchases</span>
              <div class="kpi-icon-box icon-amber"><i class="bi bi-bag-dash-fill"></i></div>
            </div>
            <div class="kpi-value text-warning">${App.formatCurrency(data.totalPurchasesAmount)}</div>
            <div class="kpi-subtitle text-muted">Supplier procurement spend</div>
          </div>
        </div>

        <div class="col-sm-6 col-xl-3">
          <div class="kpi-card">
            <div class="kpi-header">
              <span class="kpi-title">Realized Revenue</span>
              <div class="kpi-icon-box icon-blue"><i class="bi bi-cash-stack"></i></div>
            </div>
            <div class="kpi-value text-primary">${App.formatCurrency(data.totalRevenue)}</div>
            <div class="kpi-subtitle text-muted">Actual ledger cash inflow</div>
          </div>
        </div>

        <div class="col-sm-6 col-xl-3">
          <div class="kpi-card">
            <div class="kpi-header">
              <span class="kpi-title">Pending Receivables</span>
              <div class="kpi-icon-box icon-purple"><i class="bi bi-clock-history"></i></div>
            </div>
            <div class="kpi-value">${App.formatCurrency(data.outstandingReceivables)}</div>
            <div class="kpi-subtitle text-muted">Unpaid customer invoices</div>
          </div>
        </div>
      </div>

      <!-- Charts Row -->
      <div class="row g-3 mb-4">
        <div class="col-lg-8">
          <div class="erp-card h-100">
            <div class="erp-card-header">
              <h6 class="erp-card-title"><i class="bi bi-bar-chart-line me-2 text-primary"></i>Sales vs Purchases (Last 6 Months)</h6>
            </div>
            <div class="erp-card-body">
              <canvas id="chart-sales-purchases" height="110"></canvas>
            </div>
          </div>
        </div>

        <div class="col-lg-4">
          <div class="erp-card h-100">
            <div class="erp-card-header">
              <h6 class="erp-card-title"><i class="bi bi-pie-chart me-2 text-primary"></i>Inventory Status</h6>
            </div>
            <div class="erp-card-body d-flex justify-content-center align-items-center">
              <div style="max-height: 240px; width: 100%;">
                <canvas id="chart-inventory-status"></canvas>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Charts Row 2 -->
      <div class="row g-3 mb-4">
        <div class="col-lg-6">
          <div class="erp-card h-100">
            <div class="erp-card-header">
              <h6 class="erp-card-title"><i class="bi bi-graph-up me-2 text-success"></i>Revenue Growth Trend</h6>
            </div>
            <div class="erp-card-body">
              <canvas id="chart-revenue-trend" height="120"></canvas>
            </div>
          </div>
        </div>

        <div class="col-lg-6">
          <div class="erp-card h-100">
            <div class="erp-card-header">
              <h6 class="erp-card-title"><i class="bi bi-diagram-3 me-2 text-info"></i>Product Category Distribution</h6>
            </div>
            <div class="erp-card-body d-flex justify-content-center align-items-center">
              <div style="max-height: 240px; width: 100%;">
                <canvas id="chart-category-dist"></canvas>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Tables Row -->
      <div class="row g-3">
        <!-- Recent Transactions -->
        <div class="col-lg-7">
          <div class="erp-card">
            <div class="erp-card-header">
              <h6 class="erp-card-title"><i class="bi bi-journal-text me-2 text-primary"></i>Recent Financial Ledger</h6>
              <a href="#transactions" class="btn btn-outline-secondary btn-sm py-0 px-2">View All</a>
            </div>
            <div class="erp-table-wrapper">
              <table class="erp-table">
                <thead>
                  <tr>
                    <th>Ref #</th>
                    <th>Type</th>
                    <th>Category</th>
                    <th>Amount</th>
                    <th>Date</th>
                  </tr>
                </thead>
                <tbody>
                  ${(data.recentTransactions || []).map(t => `
                    <tr>
                      <td class="fw-semibold">${t.referenceNumber || t.transactionNumber}</td>
                      <td><span class="badge-status ${t.type === 'Income' ? 'badge-completed' : 'badge-lowstock'}">${t.type}</span></td>
                      <td>${t.category}</td>
                      <td class="fw-bold ${t.type === 'Income' ? 'text-success' : 'text-danger'}">
                        ${t.type === 'Income' ? '+' : '-'}${App.formatCurrency(t.amount)}
                      </td>
                      <td class="text-muted small">${App.formatDate(t.transactionDate)}</td>
                    </tr>
                  `).join('') || '<tr><td colspan="5" class="text-center text-muted">No transactions recorded yet.</td></tr>'}
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <!-- Critical Low Stock Alerts -->
        <div class="col-lg-5">
          <div class="erp-card">
            <div class="erp-card-header">
              <h6 class="erp-card-title text-danger"><i class="bi bi-exclamation-triangle me-2"></i>Critical Low Stock</h6>
              <a href="#inventory" class="btn btn-outline-danger btn-sm py-0 px-2">Inventory</a>
            </div>
            <div class="erp-table-wrapper">
              <table class="erp-table">
                <thead>
                  <tr>
                    <th>SKU / Product</th>
                    <th>On Hand</th>
                    <th>Reorder</th>
                    <th>Action</th>
                  </tr>
                </thead>
                <tbody>
                  ${(data.lowStockAlerts || []).map(p => `
                    <tr>
                      <td>
                        <div class="fw-semibold">${p.name}</div>
                        <div class="small text-muted">${p.sku}</div>
                      </td>
                      <td><span class="badge bg-danger">${p.quantityOnHand}</span></td>
                      <td>${p.reorderLevel}</td>
                      <td>
                        <a href="#purchases/new" class="btn btn-outline-primary btn-sm py-0 px-1" title="Order Stock">
                          <i class="bi bi-plus-circle"></i> Restock
                        </a>
                      </td>
                    </tr>
                  `).join('') || '<tr><td colspan="4" class="text-center text-muted">All stock levels healthy!</td></tr>'}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    `;

    // Initialize Chart.js charts
    this.renderAdminCharts(data);
  },

  renderAdminCharts(data) {
    // 1. Sales vs Purchases Bar Chart
    const ctx1 = document.getElementById('chart-sales-purchases');
    if (ctx1) {
      const chart1 = new Chart(ctx1, {
        type: 'bar',
        data: {
          labels: data.salesVsPurchasesChart.labels,
          datasets: [
            {
              label: 'Sales (₹)',
              data: data.salesVsPurchasesChart.series[0]?.data || [],
              backgroundColor: 'rgba(37, 99, 235, 0.85)',
              borderRadius: 4
            },
            {
              label: 'Purchases (₹)',
              data: data.salesVsPurchasesChart.series[1]?.data || [],
              backgroundColor: 'rgba(245, 158, 11, 0.85)',
              borderRadius: 4
            }
          ]
        },
        options: {
          responsive: true,
          plugins: { legend: { position: 'top' } },
          scales: { y: { beginAtZero: true } }
        }
      });
      Router.registerChart(chart1);
    }

    // 2. Revenue Trend Line Chart
    const ctx2 = document.getElementById('chart-revenue-trend');
    if (ctx2) {
      const chart2 = new Chart(ctx2, {
        type: 'line',
        data: {
          labels: data.revenueTrendsChart.labels,
          datasets: [{
            label: 'Monthly Revenue (₹)',
            data: data.revenueTrendsChart.series[0]?.data || [],
            borderColor: '#10b981',
            backgroundColor: 'rgba(16, 185, 129, 0.1)',
            fill: true,
            tension: 0.3,
            borderWidth: 2
          }]
        },
        options: {
          responsive: true,
          plugins: { legend: { display: false } },
          scales: { y: { beginAtZero: true } }
        }
      });
      Router.registerChart(chart2);
    }

    // 3. Category Distribution Doughnut
    const ctx3 = document.getElementById('chart-category-dist');
    if (ctx3) {
      const chart3 = new Chart(ctx3, {
        type: 'doughnut',
        data: {
          labels: data.categoryDistributionChart.labels,
          datasets: [{
            data: data.categoryDistributionChart.values || [],
            backgroundColor: ['#3b82f6', '#10b981', '#f59e0b', '#8b5cf6', '#ec4899', '#06b6d4']
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: { legend: { position: 'right' } }
        }
      });
      Router.registerChart(chart3);
    }

    // 4. Inventory Status Doughnut
    const ctx4 = document.getElementById('chart-inventory-status');
    if (ctx4) {
      const chart4 = new Chart(ctx4, {
        type: 'doughnut',
        data: {
          labels: data.inventoryStatusChart.labels,
          datasets: [{
            data: data.inventoryStatusChart.values || [],
            backgroundColor: ['#10b981', '#f59e0b', '#ef4444']
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: { legend: { position: 'right' } }
        }
      });
      Router.registerChart(chart4);
    }
  },

  renderEmployee(container, data) {
    const profile = data.profile || {};
    container.innerHTML = `
      <div class="page-header-container">
        <div>
          <h1 class="page-title">Operations Portal</h1>
          <p class="page-subtitle">Welcome, <strong>${profile.fullName || 'Team Member'}</strong> &bull; ${data.department} &bull; ${data.designation}</p>
        </div>
        <div>
          <a href="#sales/new" class="btn btn-erp-primary btn-sm me-2"><i class="bi bi-cart-plus me-1"></i>New Sale</a>
          <a href="#purchases/new" class="btn btn-outline-primary btn-sm"><i class="bi bi-bag-plus me-1"></i>New Purchase</a>
        </div>
      </div>

      <!-- Employee KPI Cards -->
      <div class="row g-3 mb-4">
        <div class="col-sm-6 col-xl-3">
          <div class="kpi-card">
            <div class="kpi-header">
              <span class="kpi-title">Sales Orders</span>
              <div class="kpi-icon-box icon-blue"><i class="bi bi-cart-check"></i></div>
            </div>
            <div class="kpi-value">${data.totalSalesOrders}</div>
            <div class="kpi-subtitle text-muted"><a href="#sales" class="text-decoration-none">View Orders &rarr;</a></div>
          </div>
        </div>

        <div class="col-sm-6 col-xl-3">
          <div class="kpi-card">
            <div class="kpi-header">
              <span class="kpi-title">Pending Orders</span>
              <div class="kpi-icon-box icon-amber"><i class="bi bi-hourglass-split"></i></div>
            </div>
            <div class="kpi-value text-warning">${data.pendingOrders}</div>
            <div class="kpi-subtitle text-muted">Awaiting fulfillment</div>
          </div>
        </div>

        <div class="col-sm-6 col-xl-3">
          <div class="kpi-card">
            <div class="kpi-header">
              <span class="kpi-title">Sales Volume</span>
              <div class="kpi-icon-box icon-green"><i class="bi bi-cash"></i></div>
            </div>
            <div class="kpi-value text-success">${App.formatCurrency(data.totalSalesGenerated)}</div>
            <div class="kpi-subtitle text-muted">Processed sales value</div>
          </div>
        </div>

        <div class="col-sm-6 col-xl-3">
          <div class="kpi-card">
            <div class="kpi-header">
              <span class="kpi-title">Low Stock Alerts</span>
              <div class="kpi-icon-box icon-rose"><i class="bi bi-bell-fill"></i></div>
            </div>
            <div class="kpi-value text-danger">${data.lowStockCount}</div>
            <div class="kpi-subtitle text-muted"><a href="#inventory" class="text-danger text-decoration-none">Check Stock &rarr;</a></div>
          </div>
        </div>
      </div>

      <!-- Charts -->
      <div class="row g-3 mb-4">
        <div class="col-lg-8">
          <div class="erp-card h-100">
            <div class="erp-card-header">
              <h6 class="erp-card-title"><i class="bi bi-bar-chart me-2 text-primary"></i>Operational Performance Trend</h6>
            </div>
            <div class="erp-card-body">
              <canvas id="chart-employee-perf" height="120"></canvas>
            </div>
          </div>
        </div>

        <div class="col-lg-4">
          <div class="erp-card h-100">
            <div class="erp-card-header">
              <h6 class="erp-card-title"><i class="bi bi-pie-chart me-2 text-info"></i>Sales Order Status</h6>
            </div>
            <div class="erp-card-body d-flex justify-content-center align-items-center">
              <div style="max-height: 240px; width: 100%;">
                <canvas id="chart-employee-status"></canvas>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Recent Tables -->
      <div class="row g-3">
        <div class="col-lg-6">
          <div class="erp-card">
            <div class="erp-card-header">
              <h6 class="erp-card-title"><i class="bi bi-cart me-2 text-primary"></i>Recent Sales Orders</h6>
              <a href="#sales" class="btn btn-outline-secondary btn-sm py-0 px-2">View All</a>
            </div>
            <div class="erp-table-wrapper">
              <table class="erp-table">
                <thead>
                  <tr>
                    <th>Order #</th>
                    <th>Customer</th>
                    <th>Amount</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  ${(data.recentSales || []).map(s => `
                    <tr>
                      <td class="fw-semibold"><a href="#sales/view/${s.saleId}">${s.saleOrderNumber}</a></td>
                      <td>${s.customerName}</td>
                      <td class="fw-bold">${App.formatCurrency(s.totalAmount)}</td>
                      <td><span class="badge-status badge-${s.status.toLowerCase()}">${s.status}</span></td>
                    </tr>
                  `).join('') || '<tr><td colspan="4" class="text-center text-muted">No sales orders found.</td></tr>'}
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <div class="col-lg-6">
          <div class="erp-card">
            <div class="erp-card-header">
              <h6 class="erp-card-title"><i class="bi bi-bag me-2 text-primary"></i>Recent Purchase Orders</h6>
              <a href="#purchases" class="btn btn-outline-secondary btn-sm py-0 px-2">View All</a>
            </div>
            <div class="erp-table-wrapper">
              <table class="erp-table">
                <thead>
                  <tr>
                    <th>PO #</th>
                    <th>Supplier</th>
                    <th>Amount</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  ${(data.recentPurchases || []).map(p => `
                    <tr>
                      <td class="fw-semibold"><a href="#purchases/view/${p.purchaseId}">${p.purchaseNumber}</a></td>
                      <td>${p.supplierName}</td>
                      <td class="fw-bold">${App.formatCurrency(p.totalAmount)}</td>
                      <td><span class="badge-status badge-${p.status.toLowerCase()}">${p.status}</span></td>
                    </tr>
                  `).join('') || '<tr><td colspan="4" class="text-center text-muted">No purchase orders found.</td></tr>'}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    `;

    // Charts
    const ctx1 = document.getElementById('chart-employee-perf');
    if (ctx1) {
      const c1 = new Chart(ctx1, {
        type: 'bar',
        data: {
          labels: data.monthlyPerformanceChart.labels,
          datasets: [{
            label: 'Sales Volume (₹)',
            data: data.monthlyPerformanceChart.series[0]?.data || [],
            backgroundColor: 'rgba(37, 99, 235, 0.85)',
            borderRadius: 4
          }]
        },
        options: {
          responsive: true,
          plugins: { legend: { display: false } },
          scales: { y: { beginAtZero: true } }
        }
      });
      Router.registerChart(c1);
    }

    const ctx2 = document.getElementById('chart-employee-status');
    if (ctx2) {
      const c2 = new Chart(ctx2, {
        type: 'doughnut',
        data: {
          labels: data.ordersStatusChart.labels,
          datasets: [{
            data: data.ordersStatusChart.values || [],
            backgroundColor: ['#f59e0b', '#10b981', '#64748b']
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: { legend: { position: 'right' } }
        }
      });
      Router.registerChart(c2);
    }
  },

  renderCustomer(container, data) {
    const profile = data.profile || {};
    container.innerHTML = `
      <div class="page-header-container">
        <div>
          <h1 class="page-title">Customer Portal</h1>
          <p class="page-subtitle">Welcome, <strong>${profile.name || 'Valued Customer'}</strong> &bull; ${profile.company || ''}</p>
        </div>
        <div>
          ${Auth.hasPermission('sales', 'ADD') ? `<a href="#sales/new" class="btn btn-erp-primary btn-sm"><i class="bi bi-cart-plus me-1"></i>Place New Order</a>` : ''}
        </div>
      </div>

      <!-- Customer KPI Cards -->
      <div class="row g-3 mb-4">
        <div class="col-sm-6 col-xl-3">
          <div class="kpi-card">
            <div class="kpi-header">
              <span class="kpi-title">Total Orders</span>
              <div class="kpi-icon-box icon-blue"><i class="bi bi-cart3"></i></div>
            </div>
            <div class="kpi-value">${data.totalOrders}</div>
            <div class="kpi-subtitle text-muted"><a href="#sales" class="text-decoration-none">My Order History &rarr;</a></div>
          </div>
        </div>

        <div class="col-sm-6 col-xl-3">
          <div class="kpi-card">
            <div class="kpi-header">
              <span class="kpi-title">Pending Orders</span>
              <div class="kpi-icon-box icon-amber"><i class="bi bi-clock-history"></i></div>
            </div>
            <div class="kpi-value text-warning">${data.pendingOrders}</div>
            <div class="kpi-subtitle text-muted">Processing / In Transit</div>
          </div>
        </div>

        <div class="col-sm-6 col-xl-3">
          <div class="kpi-card">
            <div class="kpi-header">
              <span class="kpi-title">Total Spend</span>
              <div class="kpi-icon-box icon-green"><i class="bi bi-cash-stack"></i></div>
            </div>
            <div class="kpi-value text-success">${App.formatCurrency(data.totalPurchasedAmount)}</div>
            <div class="kpi-subtitle text-muted">Lifetime purchases</div>
          </div>
        </div>

        <div class="col-sm-6 col-xl-3">
          <div class="kpi-card">
            <div class="kpi-header">
              <span class="kpi-title">Outstanding Balance</span>
              <div class="kpi-icon-box icon-rose"><i class="bi bi-receipt"></i></div>
            </div>
            <div class="kpi-value ${data.outstandingBalance > 0 ? 'text-danger' : 'text-success'}">
              ${App.formatCurrency(data.outstandingBalance)}
            </div>
            <div class="kpi-subtitle text-muted"><a href="#invoices" class="text-decoration-none">View Invoices &rarr;</a></div>
          </div>
        </div>
      </div>

      <!-- Charts -->
      <div class="row g-3 mb-4">
        <div class="col-lg-8">
          <div class="erp-card h-100">
            <div class="erp-card-header">
              <h6 class="erp-card-title"><i class="bi bi-graph-up me-2 text-primary"></i>My Purchase History (Last 6 Months)</h6>
            </div>
            <div class="erp-card-body">
              <canvas id="chart-customer-spend" height="120"></canvas>
            </div>
          </div>
        </div>

        <div class="col-lg-4">
          <div class="erp-card h-100">
            <div class="erp-card-header">
              <h6 class="erp-card-title"><i class="bi bi-pie-chart me-2 text-info"></i>Order Status Breakdown</h6>
            </div>
            <div class="erp-card-body d-flex justify-content-center align-items-center">
              <div style="max-height: 240px; width: 100%;">
                <canvas id="chart-customer-orders"></canvas>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Recent Orders & Invoices -->
      <div class="row g-3">
        <div class="col-lg-6">
          <div class="erp-card">
            <div class="erp-card-header">
              <h6 class="erp-card-title"><i class="bi bi-cart me-2 text-primary"></i>My Recent Orders</h6>
              <a href="#sales" class="btn btn-outline-secondary btn-sm py-0 px-2">View All</a>
            </div>
            <div class="erp-table-wrapper">
              <table class="erp-table">
                <thead>
                  <tr>
                    <th>Order #</th>
                    <th>Date</th>
                    <th>Amount</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  ${(data.recentOrders || []).map(s => `
                    <tr>
                      <td class="fw-semibold"><a href="#sales/view/${s.saleId}">${s.saleOrderNumber}</a></td>
                      <td class="text-muted small">${App.formatDate(s.orderDate)}</td>
                      <td class="fw-bold">${App.formatCurrency(s.totalAmount)}</td>
                      <td><span class="badge-status badge-${s.status.toLowerCase()}">${s.status}</span></td>
                    </tr>
                  `).join('') || '<tr><td colspan="4" class="text-center text-muted">No orders found.</td></tr>'}
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <div class="col-lg-6">
          <div class="erp-card">
            <div class="erp-card-header">
              <h6 class="erp-card-title"><i class="bi bi-receipt me-2 text-primary"></i>My Invoices</h6>
              <a href="#invoices" class="btn btn-outline-secondary btn-sm py-0 px-2">View All</a>
            </div>
            <div class="erp-table-wrapper">
              <table class="erp-table">
                <thead>
                  <tr>
                    <th>Invoice #</th>
                    <th>Due Date</th>
                    <th>Balance</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  ${(data.recentInvoices || []).map(i => `
                    <tr>
                      <td class="fw-semibold"><a href="#invoices/view/${i.invoiceId}">${i.invoiceNumber}</a></td>
                      <td class="text-muted small">${App.formatDate(i.dueDate)}</td>
                      <td class="fw-bold ${i.balanceAmount > 0 ? 'text-danger' : 'text-success'}">${App.formatCurrency(i.balanceAmount)}</td>
                      <td><span class="badge-status badge-${i.status.toLowerCase()}">${i.status}</span></td>
                    </tr>
                  `).join('') || '<tr><td colspan="4" class="text-center text-muted">No invoices found.</td></tr>'}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    `;

    // Charts
    const ctx1 = document.getElementById('chart-customer-spend');
    if (ctx1) {
      const c1 = new Chart(ctx1, {
        type: 'line',
        data: {
          labels: data.purchaseHistoryChart.labels,
          datasets: [{
            label: 'Monthly Purchases (₹)',
            data: data.purchaseHistoryChart.series[0]?.data || [],
            borderColor: '#2563eb',
            backgroundColor: 'rgba(37, 99, 235, 0.1)',
            fill: true,
            tension: 0.3
          }]
        },
        options: {
          responsive: true,
          plugins: { legend: { display: false } },
          scales: { y: { beginAtZero: true } }
        }
      });
      Router.registerChart(c1);
    }

    const ctx2 = document.getElementById('chart-customer-orders');
    if (ctx2) {
      const c2 = new Chart(ctx2, {
        type: 'doughnut',
        data: {
          labels: data.orderStatusChart.labels,
          datasets: [{
            data: data.orderStatusChart.values || [],
            backgroundColor: ['#10b981', '#f59e0b', '#64748b']
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: { legend: { position: 'right' } }
        }
      });
      Router.registerChart(c2);
    }
  }
};
