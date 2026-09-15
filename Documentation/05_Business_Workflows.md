# Core ERP Business Workflows

## 1. Purchase & Procurement Workflow

```mermaid
sequenceDiagram
    autonumber
    actor Staff as Admin / Operations Staff
    participant PC as PurchasesController
    participant PS as PurchaseService
    participant PR as PurchaseRepository
    participant IR as InventoryRepository
    participant TR as TransactionRepository
    participant XL as ClosedXML (ERP.xlsx)

    Staff->>PC: Create Purchase Order (Supplier + Products + Pricing)
    PC->>PS: Create(dto)
    PS->>PR: Add(Purchase with Status: Ordered)
    PR->>XL: Append to Purchases & PurchaseItems sheets
    PS-->>Staff: Purchase Order Confirmed (PO-2026-XXXX)

    Note over Staff,XL: When physical shipment arrives at warehouse

    Staff->>PC: Mark PO as Received (Receive Goods)
    PC->>PS: UpdateStatus(id, "Received")
    PS->>IR: AdjustStock(item.ProductId, +item.Quantity)
    IR->>XL: Increment QuantityOnHand in Inventory sheet
    PS->>TR: Add(Expense Transaction for Supplier PO)
    TR->>XL: Record Outflow in Transactions sheet
    PS->>PR: Update(Status: Received)
    PR->>XL: Save Status in Purchases sheet
    PS-->>Staff: Stock Level Increased & Expense Recorded
```

---

## 2. Sales Order & Automatic Invoicing Workflow

```mermaid
sequenceDiagram
    autonumber
    actor User as Customer / Sales Rep
    participant SC as SalesController
    participant SS as SalesService
    participant IR as InventoryRepository
    participant SR as SalesRepository
    participant InvR as InvoiceRepository
    participant CR as CustomerRepository
    participant XL as ClosedXML (ERP.xlsx)

    User->>SC: Submit Sales Order (Customer + Items)
    SC->>SS: Create(dto)
  
    rect rgb(240, 248, 255)
        Note over SS,IR: Step 1: Real-Time Stock Availability Validation
        SS->>IR: Check AvailableQuantity for each product
        alt Insufficient Stock Available
            SS-->>User: 400 Bad Request ("Insufficient stock for Product X")
        end
    end

    rect rgb(240, 255, 240)
        Note over SS,XL: Step 2: Order Fulfillment & Inventory Decrement
        SS->>SR: Add(Sale with Status: Confirmed)
        SR->>XL: Append to Sales & SalesItems sheets
        SS->>IR: AdjustStock(item.ProductId, -item.Quantity)
        IR->>XL: Decrement QuantityOnHand in Inventory sheet
    end

    rect rgb(255, 250, 240)
        Note over SS,XL: Step 3: Automatic Invoice Generation
        SS->>InvR: Add(Invoice with Status: Unpaid)
        InvR->>XL: Append to Invoices sheet (INV-2026-XXXX)
        SS->>CR: Update Customer Balance (+TotalAmount)
        CR->>XL: Save Outstanding Balance in Customers sheet
    end

    SS-->>User: Order Confirmed & Invoice Issued
```

---

## 3. Invoice Settlement & Payment Workflow

```mermaid
sequenceDiagram
    autonumber
    actor Customer as Customer / Finance Staff
    participant IC as InvoicesController
    participant IS as InvoiceService
    participant InvR as InvoiceRepository
    participant CR as CustomerRepository
    participant TR as TransactionRepository
    participant XL as ClosedXML (ERP.xlsx)

    Customer->>IC: Record Payment (InvoiceId, Amount, PaymentMethod)
    IC->>IS: RecordPayment(id, dto)
    IS->>InvR: GetById(id)
    IS->>InvR: Update PaidAmount & Status (Paid / PartiallyPaid)
    InvR->>XL: Update row in Invoices sheet
    IS->>CR: Decrement Customer CurrentBalance (-PaymentAmount)
    CR->>XL: Update row in Customers sheet
    IS->>TR: Add(Income Transaction: "Sales Revenue")
    TR->>XL: Append to Transactions sheet
    IS-->>Customer: Payment Processed & Receipt Issued
```

---

## 4. Manual Stock Adjustment & Audit Workflow

```mermaid
sequenceDiagram
    autonumber
    actor Staff as Warehouse Lead / Admin
    participant InvC as InventoryController
    participant InvS as InventoryService
    participant InvR as InventoryRepository
    participant TR as TransactionRepository
    participant XL as ClosedXML (ERP.xlsx)

    Staff->>InvC: Post Stock Adjustment (ProductId, Type: Damaged/StockIn/Audit, Qty, Reason)
    InvC->>InvS: AdjustStock(dto)
    InvS->>InvR: AdjustStock(productId, delta, location)
    InvR->>XL: Update QuantityOnHand in Inventory sheet
    alt Adjustment is Damaged / Write-off
        InvS->>TR: Add(Expense: "Inventory Loss / Write-off")
        TR->>XL: Record Loss in Transactions sheet
    end
    InvS-->>Staff: Stock Adjusted & Audit Trail Recorded
```
