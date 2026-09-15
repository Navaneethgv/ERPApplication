# Role-Based Permissions & Security Matrix

## 1. Role Definitions

- **Admin**: Executive oversight of entire business. Full CRUD access to all modules, including Staff compensation, System users, Suppliers, Financial audit logs, and Executive reports.
- **Employee**: Operational staff (Operations Managers, Sales Specialists, Warehouse Leads). Access to day-to-day operations: Customers, Products, Inventory, Suppliers, Purchases, Sales, Invoices, and Operations Analytics. (Excluded from Employee HR/Payroll management).
- **Customer**: External client account. Confined strictly to the Customer Portal: viewing their personal profile, browsing product catalog, placing orders, viewing their own invoices, and reviewing their payment history.

---

## 2. Granular Permissions Matrix

| ERP Module / Action | Admin | Employee | Customer | Backend Enforcement |
| :--- | :---: | :---: | :---: | :--- |
| **Authentication & Profile** |
| Login / Logout | ✅ | ✅ | ✅ | `AllowAnonymous` / `Authorize` |
| Change Own Password | ✅ | ✅ | ✅ | `[Authorize]` (Claims identity) |
| Register Customer Portal | ✅ | ✅ | ✅ | `AllowAnonymous` |
| **Dashboard** |
| Executive Business Dashboard | ✅ | ❌ | ❌ | `[Authorize(Roles = "Admin")]` |
| Operations Staff Dashboard | ✅ | ✅ | ❌ | `[Authorize(Roles = "Admin,Employee")]` |
| Customer Portal Dashboard | ❌ | ❌ | ✅ | `[Authorize(Roles = "Admin,Customer")]` |
| **Employees & HR** |
| View Employees Roster | ✅ | ❌ | ❌ | `[Authorize(Roles = "Admin")]` |
| Create / Edit Employee | ✅ | ❌ | ❌ | `[Authorize(Roles = "Admin")]` |
| Delete Employee | ✅ | ❌ | ❌ | `[Authorize(Roles = "Admin")]` |
| **Customers** |
| View All Customers | ✅ | ✅ | ❌ | `[Authorize]` (Role check) |
| View Own Customer Profile | ✅ | ✅ | ✅ | `[Authorize]` (CustomerId token filter) |
| Create / Edit Customer | ✅ | ✅ | ❌ (Own profile only) | `[Authorize]` |
| Delete Customer | ✅ | ❌ | ❌ | `[Authorize(Roles = "Admin")]` |
| **Products & Catalog** |
| View Product Catalog | ✅ | ✅ | ✅ | `[Authorize]` |
| Add / Edit Products | ✅ | ✅ | ❌ | `[Authorize(Roles = "Admin,Employee")]` |
| Delete Product | ✅ | ❌ | ❌ | `[Authorize(Roles = "Admin")]` |
| **Inventory** |
| View Warehouse Stock Levels | ✅ | ✅ | ❌ | `[Authorize(Roles = "Admin,Employee")]` |
| Stock Adjustments (In/Out/Damage) | ✅ | ✅ | ❌ | `[Authorize(Roles = "Admin,Employee")]` |
| **Suppliers & Vendors** |
| View Suppliers Directory | ✅ | ✅ | ❌ | `[Authorize(Roles = "Admin,Employee")]` |
| Create / Edit Supplier | ✅ | ✅ | ❌ | `[Authorize(Roles = "Admin,Employee")]` |
| Delete Supplier | ✅ | ❌ | ❌ | `[Authorize(Roles = "Admin")]` |
| **Purchases (Procurement)** |
| View Purchase Orders | ✅ | ✅ | ❌ | `[Authorize(Roles = "Admin,Employee")]` |
| Create Purchase Order | ✅ | ✅ | ❌ | `[Authorize(Roles = "Admin,Employee")]` |
| Receive Goods (Stock In) | ✅ | ✅ | ❌ | `[Authorize(Roles = "Admin,Employee")]` |
| **Sales Orders** |
| View All Sales Orders | ✅ | ✅ | ❌ (Own orders only) | `[Authorize]` (Claims check) |
| Create Sales Order | ✅ | ✅ | ✅ (Own account only) | Stock check + Claims check |
| Update Order Status / Fulfill | ✅ | ✅ | ❌ | `[Authorize(Roles = "Admin,Employee")]` |
| **Invoices & Billing** |
| View All Invoices | ✅ | ✅ | ❌ (Own invoices only)| `[Authorize]` (Claims check) |
| Record Invoice Payment | ✅ | ✅ | ✅ (Against own invoice)| `[Authorize]` |
| **Transactions (General Ledger)**|
| View Full Ledger | ✅ | ✅ | ❌ (Own payments only)| `[Authorize]` (Claims check) |
| Record Manual Ledger Entry | ✅ | ✅ | ❌ | `[Authorize(Roles = "Admin,Employee")]` |
| **Reports & Analytics** |
| Sales & Procurement Reports | ✅ | ✅ | ❌ | `[Authorize(Roles = "Admin,Employee")]` |
| Inventory & P&L Statement | ✅ | ✅ | ❌ | `[Authorize(Roles = "Admin,Employee")]` |
