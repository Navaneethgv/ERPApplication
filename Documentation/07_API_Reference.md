# REST API Endpoints & Swagger Reference

## 1. Authentication & Security
All secured endpoints require an HTTP `Authorization` header formatted as:
```http
Authorization: Bearer <your_jwt_token>
```
Swagger UI is accessible directly at:
```
http://localhost:<port>/swagger
```

---

## 2. API Endpoint Directory

### Authentication (`/api/auth`)
| Method | Route | Access | Request Body | Description |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/login` | Public | `LoginRequestDto` | Authenticates username/password, returns JWT token & profile. |
| `GET` | `/api/auth/profile` | Authorized | None | Returns active user profile (excludes password hash). |
| `POST` | `/api/auth/change-password` | Authorized | `ChangePasswordDto` | Updates current user password. |
| `POST` | `/api/auth/register-customer` | Public | `RegisterCustomerDto`| Self-registers a customer account & portal login. |

### Dashboard (`/api/dashboard`)
| Method | Route | Access | Description |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/dashboard/summary` | Authorized | Returns role-tailored dashboard payload for current user. |
| `GET` | `/api/dashboard/admin` | Admin | Returns executive KPIs, charts, and financial health metrics. |
| `GET` | `/api/dashboard/employee` | Admin, Employee | Returns operational metrics, pending orders, and alerts. |
| `GET` | `/api/dashboard/customer` | Admin, Customer | Returns customer personal orders, spend, and invoice status. |

### Employees (`/api/employees`)
| Method | Route | Access | Request Body | Description |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/employees` | Admin | None | Lists all organizational staff. |
| `GET` | `/api/employees/{id}` | Admin | None | Gets employee details by ID. |
| `POST` | `/api/employees` | Admin | `CreateEmployeeDto` | Registers a new employee (optional user login). |
| `PUT` | `/api/employees/{id}` | Admin | `UpdateEmployeeDto` | Updates employee details and compensation. |
| `DELETE`| `/api/employees/{id}` | Admin | None | Removes employee and associated login. |

### Customers (`/api/customers`)
| Method | Route | Access | Request Body | Description |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/customers` | Authorized | None | Lists customers (Staff sees all, Customer sees self). |
| `GET` | `/api/customers/{id}` | Authorized | None | Gets customer by ID. |
| `POST` | `/api/customers` | Admin, Employee| `CreateCustomerDto`| Adds a new customer profile. |
| `PUT` | `/api/customers/{id}` | Authorized | `UpdateCustomerDto`| Updates customer profile details. |
| `DELETE`| `/api/customers/{id}` | Admin | None | Deletes customer account. |

### Products (`/api/products`)
| Method | Route | Access | Request Body | Description |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/products` | Authorized | None | Lists catalog products with stock levels. |
| `GET` | `/api/products/{id}` | Authorized | None | Gets product by ID. |
| `POST` | `/api/products` | Admin, Employee| `CreateProductDto` | Creates product and initializes inventory. |
| `PUT` | `/api/products/{id}` | Admin, Employee| `UpdateProductDto` | Updates product metadata and pricing. |
| `DELETE`| `/api/products/{id}` | Admin | None | Deletes product from catalog. |

### Inventory (`/api/inventory`)
| Method | Route | Access | Request Body | Description |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/inventory` | Admin, Employee| None | Returns all warehouse stock balances & valuation. |
| `GET` | `/api/inventory/product/{id}`| Admin, Employee| None | Returns stock record for specific product. |
| `GET` | `/api/inventory/low-stock` | Admin, Employee| None | Filters products at or below reorder threshold. |
| `POST` | `/api/inventory/adjust` | Admin, Employee| `StockAdjustmentDto`| Posts manual adjustment (In/Out/Damage/Audit). |

### Suppliers (`/api/suppliers`)
| Method | Route | Access | Request Body | Description |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/suppliers` | Admin, Employee| None | Lists all registered vendors. |
| `GET` | `/api/suppliers/{id}` | Admin, Employee| None | Gets supplier details by ID. |
| `POST` | `/api/suppliers` | Admin, Employee| `CreateSupplierDto` | Adds a new vendor. |
| `PUT` | `/api/suppliers/{id}` | Admin, Employee| `UpdateSupplierDto` | Updates vendor details and payment terms. |
| `DELETE`| `/api/suppliers/{id}` | Admin | None | Deletes vendor from directory. |

### Purchases (`/api/purchases`)
| Method | Route | Access | Request Body | Description |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/purchases` | Admin, Employee| None | Lists all purchase orders with line items. |
| `GET` | `/api/purchases/{id}` | Admin, Employee| None | Gets PO details. |
| `POST` | `/api/purchases` | Admin, Employee| `CreatePurchaseDto`| Creates PO in Ordered status. |
| `PUT` | `/api/purchases/{id}/status`| Admin, Employee| `UpdatePurchaseStatusDto` | On 'Received', automatically increments inventory and records expense txn. |
| `DELETE`| `/api/purchases/{id}` | Admin | None | Removes PO. |

### Sales Orders (`/api/sales`)
| Method | Route | Access | Request Body | Description |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/sales` | Authorized | None | Lists sales orders (Customer sees own). |
| `GET` | `/api/sales/{id}` | Authorized | None | Gets sales order details. |
| `POST` | `/api/sales` | Authorized | `CreateSaleDto` | Validates available stock, fulfills order, decrements stock, and generates invoice. |
| `PUT` | `/api/sales/{id}/status`| Admin, Employee| `UpdateSaleStatusDto`| Updates order status. |
| `DELETE`| `/api/sales/{id}` | Admin | None | Deletes sales order. |

### Invoices (`/api/invoices`)
| Method | Route | Access | Request Body | Description |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/invoices` | Authorized | None | Lists all invoices (Customer sees own). |
| `GET` | `/api/invoices/{id}` | Authorized | None | Gets invoice details for printable view. |
| `POST` | `/api/invoices/{id}/payments`| Authorized | `RecordPaymentDto` | Records payment against invoice, updates balance, and logs income transaction. |

### Transactions (`/api/transactions`)
| Method | Route | Access | Request Body | Description |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/transactions` | Authorized | None | Lists general ledger transactions. |
| `GET` | `/api/transactions/{id}`| Authorized | None | Gets transaction by ID. |
| `POST` | `/api/transactions` | Admin, Employee| `CreateTransactionDto`| Records manual expense / income ledger entry. |

### Reports (`/api/reports`)
| Method | Route | Access | Query Parameters | Description |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/reports/sales` | Admin, Employee| `startDate, endDate, customerId, status` | Sales totals, averages, category distribution. |
| `GET` | `/api/reports/purchases` | Admin, Employee| `startDate, endDate, supplierId, status` | Procurement spend and vendor breakdown. |
| `GET` | `/api/reports/inventory-valuation`| Admin, Employee| `category` | Stock on hand, cost vs retail valuation, profit margin. |
| `GET` | `/api/reports/financial-summary` | Admin, Employee| `startDate, endDate` | Total income, total expenses, net profit, balance sheet. |
