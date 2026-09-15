# ApexERP - Enterprise Management System Overview

## 1. System Introduction
**ApexERP** is a full-featured, lightweight Enterprise Resource Planning (ERP) application engineered to handle end-to-end commercial workflows—from procurement and inventory management to sales, billing, and financial auditing.

The system is constructed with modern C# ASP.NET Core Web API (.NET 10), ClosedXML Excel storage engine (`ERP.xlsx`), JWT role-based authorization, and a responsive HTML5 / Bootstrap 5 / Vanilla JavaScript Single Page Application (SPA).

---

## 2. Technology Stack

| Tier | Technology | Description |
| :--- | :--- | :--- |
| **Backend Framework** | C# / ASP.NET Core Web API (.NET 10) | RESTful API endpoints with structured DTO request/response pipelines. |
| **Authentication** | JWT (JSON Web Tokens) + BCrypt | Stateless, cryptographically signed Bearer authentication with salted password hashing. |
| **Data Persistence** | ClosedXML (.NET Excel Library) | Zero-SQL database engine storing relational data across 12 worksheets in `ERP.xlsx`. |
| **Thread Safety** | ReaderWriterLock / Lock Synchronization | Safe multi-threaded file access preventing race conditions and workbook corruption. |
| **Frontend Framework**| HTML5, CSS3, Bootstrap 5, Vanilla JS | High-performance Single Page Application (SPA) with persistent layout & hash routing. |
| **Data Visualization** | Chart.js 4.x | Real-time interactive charts (bar, line, doughnut, pie) reflecting live Excel data. |
| **API Documentation** | Swagger / OpenAPI | Interactive API explorer with JWT Bearer token authorization support. |

---

## 3. Core Modules Mind-Map

```
ERP
├── Authentication (JWT, BCrypt, Role Claims, Session Management)
├── Dashboard (Role-Tailored: Admin, Employee, Customer Views + Real-time KPIs)
├── Employees (Roster, Departments, Compensation, Staff Access)
├── Customers (Accounts Receivable, Contact Details, Credit Limits)
├── Products (SKUs, Categories, Selling Prices, Cost Prices, Reorder Levels)
├── Inventory (Warehouse Tracking, Quantity on Hand, Low Stock Alerts, Adjustments)
├── Suppliers (Vendor Directory, Procurement Contacts, Payment Terms)
├── Purchases (Purchase Orders, Line Items, Goods Reception, Stock-In)
├── Sales (Sales Orders, Line Items, Stock Availability Check, Stock-Out)
├── Invoices (Billing Statements, Payment Processing, Accounts Receivable)
├── Transactions (General Ledger, Inflow/Outflow Audit Trail, P&L Metrics)
└── Reports & Analytics (Sales Trends, Vendor Spend, Stock Valuation, Financials)
```

---

## 4. Default Seed Login Credentials

| Role | Username / Email | Password | Primary Scope & Access |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@erp.com` | `Admin@123` | Full ERP access (Staff, Customers, Inventory, Procurement, Sales, Ledger, Reports, System Settings). |
| **Employee (Ops)** | `sarah.ops@erp.com` | `Employee@123` | Operations Lead: Inventory, Suppliers, Purchase Orders, Sales Orders, Invoices, Transactions, Reports. |
| **Employee (Sales)** | `john.sales@erp.com` | `Employee@123` | Sales Specialist: Customer Management, Sales Orders, Invoices, Operational Reports. |
| **Customer (Acme)** | `acme.buyer@client.com` | `Customer@123` | Customer Portal: Dashboard, My Orders, Invoices, Payment History, Profile. |
| **Customer (Apex)** | `apex.buyer@client.com` | `Customer@123` | Customer Portal: Dashboard, My Orders, Invoices, Payment History, Profile. |

---

## 5. Security & Isolation Guarantee
- **Zero SQL / Entity Framework dependency**: All relational data is safely stored in `ERP.xlsx`.
- **Password Masking**: Passwords and `PasswordHash` fields are NEVER exposed in DTO responses, frontend JavaScript, or API output.
- **Strict Role Isolation**: Customers cannot view internal ERP modules or data from other customers. Employees cannot view or alter employee HR records.
