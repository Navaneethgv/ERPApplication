namespace ERP.Domain.Enums;

public enum UserRole
{
    Admin,
    Employee,
    Customer
}

public enum RecordStatus
{
    Active,
    Inactive,
    Suspended
}

public enum PurchaseStatus
{
    Draft,
    Ordered,
    Received,
    Cancelled
}

public enum SaleStatus
{
    Draft,
    Confirmed,
    Fulfilled,
    Cancelled
}

public enum InvoiceStatus
{
    Unpaid,
    PartiallyPaid,
    Paid,
    Overdue,
    Cancelled
}

public enum TransactionType
{
    Income,
    Expense
}

public enum ReferenceType
{
    Purchase,
    Sale,
    InvoicePayment,
    Adjustment,
    Salary,
    Other
}

public enum InventoryAdjustmentType
{
    StockIn,
    StockOut,
    Correction,
    Damaged,
    Audit
}

