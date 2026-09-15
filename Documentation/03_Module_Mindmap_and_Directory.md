# Module Overview & Directory Structure

## 1. Module Interaction Map

```
                     ┌──────────────────┐
                     │  Authentication  │
                     └────────┬─────────┘
                              │ Logins & Role Permissions
                              ▼
                     ┌──────────────────┐
                     │    Dashboard     │
                     └────────┬─────────┘
                              │
     ┌────────────────────────┼────────────────────────┐
     ▼                        ▼                        ▼
┌─────────────┐        ┌─────────────┐        ┌─────────────┐
│  Employees  │        │  Customers  │        │  Suppliers  │
└─────────────┘        └──────┬──────┘        └──────┬──────┘
                              │                      │
                              │                      │
     ┌────────────────────────┼──────────────────────┤
     │                        │                      │
     ▼                        ▼                      ▼
┌─────────────┐        ┌─────────────┐        ┌─────────────┐
│  Products   │───────▶│ Sales Orders│        │ Purchases   │
└──────┬──────┘        └──────┬──────┘        └──────┬──────┘
       │                      │                      │
       │ Stock Check & Decr   │ Decrement Stock      │ Increment Stock
       ▼                      ▼                      ▼
┌─────────────┐        ┌─────────────┐               │
│  Inventory  │◀───────┤  Invoices   │               │
└─────────────┘        └──────┬──────┘               │
                              │                      │
                              │ Record Payment       │ Record Expense
                              ▼                      ▼
                     ┌──────────────────┐
                     │   Transactions   │
                     └────────┬─────────┘
                              │ Audit & Balances
                              ▼
                     ┌──────────────────┐
                     │     Reports      │
                     └──────────────────┘
```

---

## 2. Directory Layout of the Codebase

```
d:/ERPApplication/
├── Controllers/                 # REST API Controllers with Role-Based Authorization
│   ├── AuthController.cs
│   ├── CustomersController.cs
│   ├── DashboardController.cs
│   ├── EmployeesController.cs
│   ├── InventoryController.cs
│   ├── InvoicesController.cs
│   ├── ProductsController.cs
│   ├── PurchasesController.cs
│   ├── ReportsController.cs
│   ├── SalesController.cs
│   ├── SuppliersController.cs
│   └── TransactionsController.cs
│
├── Data/                        # Excel Database Storage & Engine
│   ├── ERP.xlsx                 # The 12-Worksheet Excel Database
│   └── Excel/
│       └── ExcelDbContext.cs    # ClosedXML Concurrency & Auto-Seed Engine
│
├── Documentation/               # Comprehensive System Guides & Workflows
│   ├── 01_ERP_Overview.md
│   ├── 02_System_Architecture.md
│   ├── 03_Module_Mindmap_and_Directory.md
│   ├── 04_Role_Permissions_Matrix.md
│   ├── 05_Business_Workflows.md
│   ├── 06_Excel_Data_Storage.md
│   └── 07_API_Reference.md
│
├── DTOs/                        # Request & Response Data Transfer Objects
│   ├── AuthDtos.cs
│   ├── CustomerDtos.cs
│   ├── DashboardDtos.cs
│   ├── EmployeeDtos.cs
│   ├── InventoryDtos.cs
│   ├── InvoiceDtos.cs
│   ├── ProductDtos.cs
│   ├── PurchaseDtos.cs
│   ├── ReportDtos.cs
│   ├── SalesDtos.cs
│   ├── SupplierDtos.cs
│   └── TransactionDtos.cs
│
├── Models/                      # Core Domain Models and Enums
│   ├── Customer.cs
│   ├── Employee.cs
│   ├── Enums.cs
│   ├── Inventory.cs
│   ├── Invoice.cs
│   ├── Product.cs
│   ├── Purchase.cs
│   ├── Sale.cs
│   ├── Supplier.cs
│   ├── Transaction.cs
│   └── User.cs
│
├── Repositories/                # ClosedXML Data Access & Row Mappers
│   ├── CustomerRepository.cs
│   ├── EmployeeRepository.cs
│   ├── ExcelHelper.cs
│   ├── InventoryRepository.cs
│   ├── InvoiceRepository.cs
│   ├── ProductRepository.cs
│   ├── PurchaseRepository.cs
│   ├── SalesRepository.cs
│   ├── SupplierRepository.cs
│   ├── TransactionRepository.cs
│   └── UserRepository.cs
│
├── Services/                    # Business Logic, Validation & Workflow Automation
│   ├── AuthService.cs
│   ├── CustomerService.cs
│   ├── DashboardService.cs
│   ├── EmployeeService.cs
│   ├── InventoryService.cs
│   ├── InvoiceService.cs
│   ├── ProductService.cs
│   ├── PurchaseService.cs
│   ├── ReportService.cs
│   ├── SalesService.cs
│   ├── SupplierService.cs
│   └── TransactionService.cs
│
├── wwwroot/                     # Frontend Single Page Application (SPA)
│   ├── css/
│   │   └── erp.css              # Custom Enterprise Styling & Themes
│   ├── js/
│   │   ├── components/          # Module View Components
│   │   │   ├── customers.js
│   │   │   ├── dashboard.js
│   │   │   ├── employees.js
│   │   │   ├── inventory.js
│   │   │   ├── invoices.js
│   │   │   ├── products.js
│   │   │   ├── profile.js
│   │   │   ├── purchases.js
│   │   │   ├── reports.js
│   │   │   ├── sales.js
│   │   │   ├── suppliers.js
│   │   │   └── transactions.js
│   │   ├── api.js               # REST Client with JWT Interceptors
│   │   ├── app.js               # Global UI Controller & Modal Helpers
│   │   ├── auth.js              # Session & Role UI Filter Management
│   │   └── router.js            # Hash-Based SPA Router with Back/Forward Support
│   └── index.html               # Main SPA Entry Point & HTML Layout
│
├── appsettings.json             # JWT Configuration & File Storage Path
├── ERPApplication.csproj        # .NET 10 Project File
└── Program.cs                   # Application Host, DI Setup, Middleware Pipeline
```
