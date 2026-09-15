# System Architecture & Request-Response Pipeline

## 1. Multi-Tier Layered Architecture

ApexERP enforces a strict separation of concerns across presentation, API, business logic, data access, and physical storage:

```
┌────────────────────────────────────────────────────────┐
│               Frontend Presentation (SPA)              │
│       HTML5 + CSS3 + Bootstrap 5 + Vanilla JS          │
└──────────────────────────┬─────────────────────────────┘
                           │ HTTP REST / JWT Bearer
                           ▼
┌────────────────────────────────────────────────────────┐
│                   Controllers Layer                    │
│      (AuthController, SalesController, etc.)           │
└──────────────────────────┬─────────────────────────────┘
                           │ Request/Response DTOs
                           ▼
┌────────────────────────────────────────────────────────┐
│                    Services Layer                      │
│    (Business Rules, Stock Validation, Workflows)       │
└──────────────────────────┬─────────────────────────────┘
                           │ Domain Entities
                           ▼
┌────────────────────────────────────────────────────────┐
│                   Repositories Layer                   │
│   (Entity Mappings, Row Parsing, Cell Operations)      │
└──────────────────────────┬─────────────────────────────┘
                           │ ReaderWriterLock / Lock
                           ▼
┌────────────────────────────────────────────────────────┐
│             ExcelDbContext (ClosedXML Engine)          │
│          Thread-Safe File I/O & Auto-Seeding           │
└──────────────────────────┬─────────────────────────────┘
                           │ Open / Read / Write / Save
                           ▼
┌────────────────────────────────────────────────────────┐
│                   Data/ERP.xlsx File                   │
│                    (12 Worksheets)                     │
└────────────────────────────────────────────────────────┘
```

---

## 2. End-to-End Data Flow Explanation

```
Frontend ➔ API ➔ Controller ➔ Service ➔ Repository ➔ ClosedXML ➔ ERP.xlsx ➔ Response ➔ Frontend
```

1. **Frontend Request**: The client SPA triggers an asynchronous `fetch()` call with the user's JWT Bearer token in the `Authorization` header.
2. **Routing & Authentication**: ASP.NET Core middleware authenticates the token, validates claims (User ID, Role, Customer ID), and routes the request to the matching controller endpoint.
3. **Controller Execution**: The controller binds and validates the incoming DTO and passes sanitized requests to the corresponding business service.
4. **Service Orchestration**:
   - Executes business rules (e.g. verifying warehouse stock before confirming a sales order, calculating financial balances).
   - Coordinates multi-entity workflows (e.g. on PO receipt, increments inventory and logs an expense transaction).
5. **Repository Access**: The repository maps domain entities to ClosedXML worksheet operations.
6. **ExcelDbContext File Lock**: A thread synchronization lock (`_fileLock`) is acquired to guarantee isolated read/write access to `ERP.xlsx`, preventing file locks or corrupted workbooks.
7. **ClosedXML Workbook Read/Write**: ClosedXML reads or modifies rows and saves changes back to `ERP.xlsx`.
8. **Response Return**: Entity results are mapped to clean response DTOs (excluding sensitive data like password hashes) and returned as JSON (200 OK / 201 Created).
9. **Frontend UI Update**: The SPA receives the JSON payload, updates the main workspace, and refreshes the Chart.js visualizations.
