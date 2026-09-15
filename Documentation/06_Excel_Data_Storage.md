# Excel Data Storage & Schema Design (`ERP.xlsx`)

## 1. Storage Overview
All enterprise entities and transactional state are persisted in `Data/ERP.xlsx` using **ClosedXML**. The database requires no external database service (SQL Server, Postgres, MySQL) and runs self-contained.

---

## 2. Worksheet Schemas & Column Mappings

### 1. `Users` Worksheet
Stores authentication credentials and role links.
- `UserId` (int, Primary Key)
- `Username` (string, unique email or user handle)
- `PasswordHash` (string, BCrypt cryptographic hash)
- `Role` (string: Admin, Employee, Customer)
- `EmployeeId` (nullable int, Foreign Key -> Employees.EmployeeId)
- `CustomerId` (nullable int, Foreign Key -> Customers.CustomerId)
- `FullName` (string)
- `Status` (string: Active, Inactive, Suspended)
- `CreatedAt` (datetime)

### 2. `Employees` Worksheet
- `EmployeeId` (int, Primary Key)
- `FirstName` (string)
- `LastName` (string)
- `Email` (string, unique)
- `Phone` (string)
- `Department` (string)
- `Designation` (string)
- `Salary` (decimal)
- `JoiningDate` (date)
- `Status` (string: Active, Inactive)

### 3. `Customers` Worksheet
- `CustomerId` (int, Primary Key)
- `Name` (string)
- `ContactPerson` (string)
- `Email` (string)
- `Phone` (string)
- `Company` (string)
- `Address` (string)
- `CreditLimit` (decimal)
- `CurrentBalance` (decimal, tracks unpaid invoice totals)
- `Status` (string: Active, Inactive)
- `CreatedAt` (datetime)

### 4. `Products` Worksheet
- `ProductId` (int, Primary Key)
- `SKU` (string, unique)
- `Name` (string)
- `Category` (string)
- `Description` (string)
- `UnitPrice` (decimal, retail selling price)
- `CostPrice` (decimal, procurement cost)
- `UnitOfMeasure` (string: Unit, Box, Set, Pack)
- `ReorderLevel` (int, threshold for low stock alert)
- `Status` (string: Active, Inactive)

### 5. `Inventory` Worksheet
- `InventoryId` (int, Primary Key)
- `ProductId` (int, Foreign Key -> Products.ProductId)
- `ProductName` (string)
- `SKU` (string)
- `QuantityOnHand` (int)
- `ReservedQuantity` (int)
- `Location` (string, warehouse aisle / zone)
- `LastUpdated` (datetime)

### 6. `Suppliers` Worksheet
- `SupplierId` (int, Primary Key)
- `SupplierCode` (string, unique)
- `Name` (string)
- `ContactPerson` (string)
- `Email` (string)
- `Phone` (string)
- `Address` (string)
- `PaymentTerms` (string: Net 15, Net 30, Net 60)
- `Status` (string: Active, Inactive)
- `CreatedAt` (datetime)

### 7. `Purchases` & `PurchaseItems` Worksheets
- **Purchases**: `PurchaseId`, `PurchaseNumber`, `SupplierId`, `SupplierName`, `PurchaseDate`, `ExpectedDeliveryDate`, `TotalAmount`, `Status` (Draft, Ordered, Received, Cancelled), `Notes`, `CreatedBy`, `CreatedAt`
- **PurchaseItems**: `PurchaseItemId`, `PurchaseId` (FK), `ProductId` (FK), `ProductName`, `SKU`, `Quantity`, `UnitPrice`

### 8. `Sales` & `SalesItems` Worksheets
- **Sales**: `SaleId`, `SaleOrderNumber`, `CustomerId` (FK), `CustomerName`, `OrderDate`, `DeliveryDate`, `TotalAmount`, `Status` (Draft, Confirmed, Fulfilled, Cancelled), `Notes`, `CreatedBy`, `CreatedAt`
- **SalesItems**: `SalesItemId`, `SaleId` (FK), `ProductId` (FK), `ProductName`, `SKU`, `Quantity`, `UnitPrice`

### 9. `Invoices` Worksheet
- `InvoiceId` (int, Primary Key)
- `InvoiceNumber` (string, unique e.g. INV-2026-0001)
- `SaleId` (int, Foreign Key -> Sales.SaleId)
- `SaleOrderNumber` (string)
- `CustomerId` (int, Foreign Key -> Customers.CustomerId)
- `CustomerName` (string)
- `IssueDate` (date)
- `DueDate` (date)
- `TotalAmount` (decimal)
- `PaidAmount` (decimal)
- `Status` (string: Unpaid, PartiallyPaid, Paid, Overdue)
- `Notes` (string)
- `CreatedAt` (datetime)

### 10. `Transactions` Worksheet
- `TransactionId` (int, Primary Key)
- `TransactionNumber` (string, unique e.g. TXN-2026-0001)
- `ReferenceType` (string: Purchase, Sale, InvoicePayment, Adjustment, Salary, Other)
- `ReferenceNumber` (string e.g. PO-2026-0001, INV-2026-0001)
- `Type` (string: Income, Expense)
- `Category` (string: Sales Revenue, Inventory Purchase, Payroll, Office Expense, etc.)
- `Amount` (decimal)
- `TransactionDate` (datetime)
- `PaymentMethod` (string: Bank Transfer, Credit Card, Cheque, Cash)
- `Status` (string: Completed, Pending)
- `Notes` (string)
- `CreatedBy` (string)

---

## 3. Concurrency, Locking & File Lifecycle
- `ExcelDbContext` is registered as a **Singleton** service in the ASP.NET Core DI container.
- An internal synchronization lock (`private readonly object _fileLock = new();`) wraps every read and write block.
- Workbooks are opened within `using var workbook = new XLWorkbook(_filePath);` blocks ensuring immediate, deterministic disposal of file handles.
- Header rows are styled on initialization with bold white fonts on slate-900 backgrounds and automatic column width sizing.
