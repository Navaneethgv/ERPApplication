# PostgreSQL Database Storage & Schema Design (`erp_db`)

## 1. Storage Overview
All enterprise entities and transactional state are persisted in **PostgreSQL** using **Entity Framework Core 10** (`Npgsql.EntityFrameworkCore.PostgreSQL`). The database replaces single-file Excel workbook locking with concurrent multi-user ACID transactions, indexed lookups, and relational referential integrity.

---

## 2. Database Connection Configuration
Configured in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=erp_db;Username=postgres;Password=postgres;Include Error Detail=true"
  }
}
```

---

## 3. Relational Schema & Table Specifications

### 1. `Users` Table
Stores authentication credentials and role links.
- `UserId` (int, Primary Key, Identity)
- `Username` (varchar(150), Unique Index, required)
- `PasswordHash` (text, BCrypt cryptographic hash, required)
- `Role` (varchar(50): `Admin`, `Employee`, `Customer`)
- `EmployeeId` (int, nullable Foreign Key -> `Employees.EmployeeId`)
- `CustomerId` (int, nullable Foreign Key -> `Customers.CustomerId`)
- `FullName` (varchar(150))
- `Status` (varchar(50): `Active`, `Inactive`, `Suspended`)
- `CreatedAt` (timestamp without time zone)

### 2. `Employees` Table
- `EmployeeId` (int, Primary Key, Identity)
- `FirstName` (varchar(100), required)
- `LastName` (varchar(100))
- `Email` (varchar(150), required)
- `Phone` (varchar(50))
- `Department` (varchar(100))
- `Designation` (varchar(100))
- `Salary` (numeric(18, 2))
- `JoiningDate` (timestamp without time zone)
- `Status` (varchar(50): `Active`, `Inactive`)

### 3. `Customers` Table
- `CustomerId` (int, Primary Key, Identity)
- `Name` (varchar(150), required)
- `ContactPerson` (varchar(100))
- `Email` (varchar(150))
- `Phone` (varchar(50))
- `Company` (varchar(150))
- `Address` (varchar(500))
- `CreditLimit` (numeric(18, 2))
- `CurrentBalance` (numeric(18, 2))
- `Status` (varchar(50): `Active`, `Inactive`)
- `CreatedAt` (timestamp without time zone)

### 4. `Products` Table
- `ProductId` (int, Primary Key, Identity)
- `SKU` (varchar(100), Unique Index, required)
- `Name` (varchar(200), required)
- `Category` (varchar(100))
- `Description` (varchar(1000))
- `UnitPrice` (numeric(18, 2))
- `CostPrice` (numeric(18, 2))
- `UnitOfMeasure` (varchar(50))
- `ReorderLevel` (int)
- `Status` (varchar(50): `Active`, `Inactive`)

### 5. `Inventory` Table
- `InventoryId` (int, Primary Key, Identity)
- `ProductId` (int, Foreign Key -> `Products.ProductId`)
- `ProductName` (varchar(200))
- `SKU` (varchar(100))
- `QuantityOnHand` (int)
- `ReservedQuantity` (int)
- `Location` (varchar(100))
- `LastUpdated` (timestamp without time zone)

### 6. `Suppliers` Table
- `SupplierId` (int, Primary Key, Identity)
- `SupplierCode` (varchar(100), Unique Index, required)
- `Name` (varchar(200), required)
- `ContactPerson` (varchar(100))
- `Email` (varchar(150))
- `Phone` (varchar(50))
- `Address` (varchar(500))
- `PaymentTerms` (varchar(50): `Net 15`, `Net 30`, `Net 60`)
- `Status` (varchar(50): `Active`, `Inactive`)
- `CreatedAt` (timestamp without time zone)

### 7. `Purchases` & `PurchaseItems` Tables
- **`Purchases`**:
  - `PurchaseId` (int, Primary Key, Identity)
  - `PurchaseNumber` (varchar(100), Unique Index, required)
  - `SupplierId` (int)
  - `SupplierName` (varchar(200))
  - `PurchaseDate` (timestamp without time zone)
  - `ExpectedDeliveryDate` (timestamp without time zone)
  - `TotalAmount` (numeric(18, 2))
  - `Status` (varchar(50): `Draft`, `Ordered`, `Received`, `Cancelled`)
  - `Notes` (varchar(1000))
  - `CreatedBy` (varchar(100))
  - `CreatedAt` (timestamp without time zone)
- **`PurchaseItems`**:
  - `PurchaseItemId` (int, Primary Key, Identity)
  - `PurchaseId` (int, Foreign Key -> `Purchases.PurchaseId`, Cascade Delete)
  - `ProductId` (int)
  - `ProductName` (varchar(200))
  - `SKU` (varchar(100))
  - `Quantity` (int)
  - `UnitPrice` (numeric(18, 2))

### 8. `Sales` & `SalesItems` Tables
- **`Sales`**:
  - `SaleId` (int, Primary Key, Identity)
  - `SaleOrderNumber` (varchar(100), Unique Index, required)
  - `CustomerId` (int)
  - `CustomerName` (varchar(200))
  - `OrderDate` (timestamp without time zone)
  - `DeliveryDate` (timestamp without time zone, nullable)
  - `TotalAmount` (numeric(18, 2))
  - `Status` (varchar(50): `Draft`, `Confirmed`, `Fulfilled`, `Cancelled`)
  - `Notes` (varchar(1000))
  - `CreatedBy` (varchar(100))
  - `CreatedAt` (timestamp without time zone)
- **`SalesItems`**:
  - `SalesItemId` (int, Primary Key, Identity)
  - `SaleId` (int, Foreign Key -> `Sales.SaleId`, Cascade Delete)
  - `ProductId` (int)
  - `ProductName` (varchar(200))
  - `SKU` (varchar(100))
  - `Quantity` (int)
  - `UnitPrice` (numeric(18, 2))

### 9. `Invoices` Table
- `InvoiceId` (int, Primary Key, Identity)
- `InvoiceNumber` (varchar(100), Unique Index, required)
- `SaleId` (int)
- `SaleOrderNumber` (varchar(100))
- `CustomerId` (int)
- `CustomerName` (varchar(200))
- `IssueDate` (timestamp without time zone)
- `DueDate` (timestamp without time zone)
- `TotalAmount` (numeric(18, 2))
- `PaidAmount` (numeric(18, 2))
- `Status` (varchar(50): `Unpaid`, `PartiallyPaid`, `Paid`, `Overdue`, `Cancelled`)
- `Notes` (varchar(1000))
- `CreatedAt` (timestamp without time zone)

### 10. `Transactions` Table
- `TransactionId` (int, Primary Key, Identity)
- `TransactionNumber` (varchar(100), Unique Index, required)
- `ReferenceType` (varchar(50): `Purchase`, `Sale`, `InvoicePayment`, `Adjustment`, `Salary`, `Other`)
- `ReferenceNumber` (varchar(100))
- `Type` (varchar(50): `Income`, `Expense`)
- `Category` (varchar(100))
- `Amount` (numeric(18, 2))
- `TransactionDate` (timestamp without time zone)
- `PaymentMethod` (varchar(100))
- `Status` (varchar(50): `Completed`, `Pending`, `Failed`)
- `Notes` (varchar(1000))
- `CreatedBy` (varchar(100))

### 11. `Notifications` Table
- `NotificationId` (int, Primary Key, Identity)
- `Title` (varchar(200), required)
- `Message` (varchar(1000))
- `Type` (varchar(50))
- `OrderId` (int, nullable)
- `OrderNumber` (varchar(100))
- `CustomerName` (varchar(200))
- `TotalAmount` (numeric(18, 2))
- `OrderStatus` (varchar(50))
- `CreatedAt` (timestamp without time zone)
- `IsRead` (boolean)
- `TargetRole` (varchar(50): `Admin`, `Employee`, `All`)

---

## 4. Startup Auto-Initialization & Excel Migration
`DbInitializer.Initialize()` runs on application startup:
1. Calls `context.Database.EnsureCreated()` to verify or create the PostgreSQL database and tables.
2. Checks if data already exists in the `Users` table. If so, skips initialization.
3. If the database is empty and `Data/ERP.xlsx` is present, it reads all records and imports them directly into PostgreSQL.
4. If no Excel file is present, it seeds the standard enterprise demo dataset.
5. Runs sequence resets (`pg_get_serial_sequence`) so auto-increment IDs continue seamlessly without collisions.
