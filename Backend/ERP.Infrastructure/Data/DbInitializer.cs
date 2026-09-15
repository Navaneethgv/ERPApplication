using ClosedXML.Excel;


using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Data;

public static class DbInitializer
{
    public static void Initialize(ErpDbContext context, IConfiguration configuration, IWebHostEnvironment environment)
    {
        try
        {
            // Safely apply pending EF Core migrations
            context.Database.Migrate();

            // Always ensure Security Policy, Menus, Options, and Role Permissions are seeded
            EnsureSecurityAndMenusSeeded(context);

            // If Users table already has data, the main business data is already initialized
            if (context.Users.Any())
            {
                ResetPostgreSqlSequences(context);
                return;
            }

            var excelRelativePath = configuration["StorageSettings:ExcelFilePath"] ?? "Data/ERP.xlsx";
            var excelFullPath = Path.Combine(environment.ContentRootPath, excelRelativePath);

            if (File.Exists(excelFullPath))
            {
                MigrateFromExcel(context, excelFullPath);
            }
            else
            {
                SeedDefaultData(context);
            }

            // Reset PostgreSQL identity sequences so subsequent inserts don't collide with seeded IDs
            ResetPostgreSqlSequences(context);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[DbInitializer Warning] Database initialization encountered an issue: {ex.Message}");
            Console.ResetColor();
        }
    }

    private static void MigrateFromExcel(ErpDbContext context, string excelPath)
    {
        using var workbook = new XLWorkbook(excelPath);

        // 1. Employees
        if (workbook.Worksheets.Contains("Employees"))
        {
            var ws = workbook.Worksheet("Employees");
            var rows = ws.RangeUsed()?.RowsUsed().Skip(1);
            if (rows != null)
            {
                foreach (var r in rows)
                {
                    context.Employees.Add(new Employee
                    {
                        EmployeeId = ExcelHelper.GetInt(r.Cell(1)),
                        FirstName = ExcelHelper.GetString(r.Cell(2)),
                        LastName = ExcelHelper.GetString(r.Cell(3)),
                        Email = ExcelHelper.GetString(r.Cell(4)),
                        Phone = ExcelHelper.GetString(r.Cell(5)),
                        Department = ExcelHelper.GetString(r.Cell(6)),
                        Designation = ExcelHelper.GetString(r.Cell(7)),
                        Salary = ExcelHelper.GetDecimal(r.Cell(8)),
                        JoiningDate = ExcelHelper.GetDateTime(r.Cell(9)),
                        Status = Enum.TryParse<RecordStatus>(ExcelHelper.GetString(r.Cell(10)), true, out var status) ? status : RecordStatus.Active
                    });
                }
                context.SaveChanges();
            }
        }

        // 2. Customers
        if (workbook.Worksheets.Contains("Customers"))
        {
            var ws = workbook.Worksheet("Customers");
            var rows = ws.RangeUsed()?.RowsUsed().Skip(1);
            if (rows != null)
            {
                foreach (var r in rows)
                {
                    context.Customers.Add(new Customer
                    {
                        CustomerId = ExcelHelper.GetInt(r.Cell(1)),
                        Name = ExcelHelper.GetString(r.Cell(2)),
                        ContactPerson = ExcelHelper.GetString(r.Cell(3)),
                        Email = ExcelHelper.GetString(r.Cell(4)),
                        Phone = ExcelHelper.GetString(r.Cell(5)),
                        Company = ExcelHelper.GetString(r.Cell(6)),
                        Address = ExcelHelper.GetString(r.Cell(7)),
                        CreditLimit = ExcelHelper.GetDecimal(r.Cell(8)),
                        CurrentBalance = ExcelHelper.GetDecimal(r.Cell(9)),
                        Status = Enum.TryParse<RecordStatus>(ExcelHelper.GetString(r.Cell(10)), true, out var status) ? status : RecordStatus.Active,
                        CreatedAt = ExcelHelper.GetDateTime(r.Cell(11))
                    });
                }
                context.SaveChanges();
            }
        }

        // 3. Users
        if (workbook.Worksheets.Contains("Users"))
        {
            var ws = workbook.Worksheet("Users");
            var rows = ws.RangeUsed()?.RowsUsed().Skip(1);
            if (rows != null)
            {
                foreach (var r in rows)
                {
                    context.Users.Add(new User
                    {
                        UserId = ExcelHelper.GetInt(r.Cell(1)),
                        Username = ExcelHelper.GetString(r.Cell(2)),
                        PasswordHash = ExcelHelper.GetString(r.Cell(3)),
                        Role = Enum.TryParse<UserRole>(ExcelHelper.GetString(r.Cell(4)), true, out var role) ? role : UserRole.Employee,
                        EmployeeId = ExcelHelper.GetNullableInt(r.Cell(5)),
                        CustomerId = ExcelHelper.GetNullableInt(r.Cell(6)),
                        FullName = ExcelHelper.GetString(r.Cell(7)),
                        Status = Enum.TryParse<RecordStatus>(ExcelHelper.GetString(r.Cell(8)), true, out var status) ? status : RecordStatus.Active,
                        CreatedAt = ExcelHelper.GetDateTime(r.Cell(9))
                    });
                }
                context.SaveChanges();
            }
        }

        // 4. Products
        if (workbook.Worksheets.Contains("Products"))
        {
            var ws = workbook.Worksheet("Products");
            var rows = ws.RangeUsed()?.RowsUsed().Skip(1);
            if (rows != null)
            {
                foreach (var r in rows)
                {
                    context.Products.Add(new Product
                    {
                        ProductId = ExcelHelper.GetInt(r.Cell(1)),
                        SKU = ExcelHelper.GetString(r.Cell(2)),
                        Name = ExcelHelper.GetString(r.Cell(3)),
                        Category = ExcelHelper.GetString(r.Cell(4)),
                        Description = ExcelHelper.GetString(r.Cell(5)),
                        UnitPrice = ExcelHelper.GetDecimal(r.Cell(6)),
                        CostPrice = ExcelHelper.GetDecimal(r.Cell(7)),
                        UnitOfMeasure = ExcelHelper.GetString(r.Cell(8)),
                        ReorderLevel = ExcelHelper.GetInt(r.Cell(9)),
                        Status = Enum.TryParse<RecordStatus>(ExcelHelper.GetString(r.Cell(10)), true, out var status) ? status : RecordStatus.Active
                    });
                }
                context.SaveChanges();
            }
        }

        // 5. Inventory
        if (workbook.Worksheets.Contains("Inventory"))
        {
            var ws = workbook.Worksheet("Inventory");
            var rows = ws.RangeUsed()?.RowsUsed().Skip(1);
            if (rows != null)
            {
                foreach (var r in rows)
                {
                    context.Inventory.Add(new Inventory
                    {
                        InventoryId = ExcelHelper.GetInt(r.Cell(1)),
                        ProductId = ExcelHelper.GetInt(r.Cell(2)),
                        ProductName = ExcelHelper.GetString(r.Cell(3)),
                        SKU = ExcelHelper.GetString(r.Cell(4)),
                        QuantityOnHand = ExcelHelper.GetInt(r.Cell(5)),
                        ReservedQuantity = ExcelHelper.GetInt(r.Cell(6)),
                        Location = ExcelHelper.GetString(r.Cell(7), "Main Warehouse"),
                        LastUpdated = ExcelHelper.GetDateTime(r.Cell(8))
                    });
                }
                context.SaveChanges();
            }
        }

        // 6. Suppliers
        if (workbook.Worksheets.Contains("Suppliers"))
        {
            var ws = workbook.Worksheet("Suppliers");
            var rows = ws.RangeUsed()?.RowsUsed().Skip(1);
            if (rows != null)
            {
                foreach (var r in rows)
                {
                    context.Suppliers.Add(new Supplier
                    {
                        SupplierId = ExcelHelper.GetInt(r.Cell(1)),
                        SupplierCode = ExcelHelper.GetString(r.Cell(2)),
                        Name = ExcelHelper.GetString(r.Cell(3)),
                        ContactPerson = ExcelHelper.GetString(r.Cell(4)),
                        Email = ExcelHelper.GetString(r.Cell(5)),
                        Phone = ExcelHelper.GetString(r.Cell(6)),
                        Address = ExcelHelper.GetString(r.Cell(7)),
                        PaymentTerms = ExcelHelper.GetString(r.Cell(8)),
                        Status = Enum.TryParse<RecordStatus>(ExcelHelper.GetString(r.Cell(9)), true, out var status) ? status : RecordStatus.Active,
                        CreatedAt = ExcelHelper.GetDateTime(r.Cell(10))
                    });
                }
                context.SaveChanges();
            }
        }

        // 7. Purchases & PurchaseItems
        if (workbook.Worksheets.Contains("Purchases"))
        {
            var wsPurch = workbook.Worksheet("Purchases");
            var pRows = wsPurch.RangeUsed()?.RowsUsed().Skip(1);
            if (pRows != null)
            {
                var allItems = new List<PurchaseItem>();
                if (workbook.Worksheets.Contains("PurchaseItems"))
                {
                    var wsItems = workbook.Worksheet("PurchaseItems");
                    var iRows = wsItems.RangeUsed()?.RowsUsed().Skip(1);
                    if (iRows != null)
                    {
                        foreach (var ir in iRows)
                        {
                            allItems.Add(new PurchaseItem
                            {
                                PurchaseItemId = ExcelHelper.GetInt(ir.Cell(1)),
                                PurchaseId = ExcelHelper.GetInt(ir.Cell(2)),
                                ProductId = ExcelHelper.GetInt(ir.Cell(3)),
                                ProductName = ExcelHelper.GetString(ir.Cell(4)),
                                SKU = ExcelHelper.GetString(ir.Cell(5)),
                                Quantity = ExcelHelper.GetInt(ir.Cell(6)),
                                UnitPrice = ExcelHelper.GetDecimal(ir.Cell(7))
                            });
                        }
                    }
                }

                foreach (var r in pRows)
                {
                    int pId = ExcelHelper.GetInt(r.Cell(1));
                    var purchase = new Purchase
                    {
                        PurchaseId = pId,
                        PurchaseNumber = ExcelHelper.GetString(r.Cell(2)),
                        SupplierId = ExcelHelper.GetInt(r.Cell(3)),
                        SupplierName = ExcelHelper.GetString(r.Cell(4)),
                        PurchaseDate = ExcelHelper.GetDateTime(r.Cell(5)),
                        ExpectedDeliveryDate = ExcelHelper.GetDateTime(r.Cell(6)),
                        TotalAmount = ExcelHelper.GetDecimal(r.Cell(7)),
                        Status = Enum.TryParse<PurchaseStatus>(ExcelHelper.GetString(r.Cell(8)), true, out var status) ? status : PurchaseStatus.Draft,
                        Notes = ExcelHelper.GetString(r.Cell(9)),
                        CreatedBy = ExcelHelper.GetString(r.Cell(10)),
                        CreatedAt = ExcelHelper.GetDateTime(r.Cell(11)),
                        Items = allItems.Where(i => i.PurchaseId == pId).ToList()
                    };
                    context.Purchases.Add(purchase);
                }
                context.SaveChanges();
            }
        }

        // 8. Sales & SalesItems
        if (workbook.Worksheets.Contains("Sales"))
        {
            var wsSales = workbook.Worksheet("Sales");
            var sRows = wsSales.RangeUsed()?.RowsUsed().Skip(1);
            if (sRows != null)
            {
                var allItems = new List<SalesItem>();
                if (workbook.Worksheets.Contains("SalesItems"))
                {
                    var wsItems = workbook.Worksheet("SalesItems");
                    var iRows = wsItems.RangeUsed()?.RowsUsed().Skip(1);
                    if (iRows != null)
                    {
                        foreach (var ir in iRows)
                        {
                            allItems.Add(new SalesItem
                            {
                                SalesItemId = ExcelHelper.GetInt(ir.Cell(1)),
                                SaleId = ExcelHelper.GetInt(ir.Cell(2)),
                                ProductId = ExcelHelper.GetInt(ir.Cell(3)),
                                ProductName = ExcelHelper.GetString(ir.Cell(4)),
                                SKU = ExcelHelper.GetString(ir.Cell(5)),
                                Quantity = ExcelHelper.GetInt(ir.Cell(6)),
                                UnitPrice = ExcelHelper.GetDecimal(ir.Cell(7))
                            });
                        }
                    }
                }

                foreach (var r in sRows)
                {
                    int sId = ExcelHelper.GetInt(r.Cell(1));
                    var sale = new Sale
                    {
                        SaleId = sId,
                        SaleOrderNumber = ExcelHelper.GetString(r.Cell(2)),
                        CustomerId = ExcelHelper.GetInt(r.Cell(3)),
                        CustomerName = ExcelHelper.GetString(r.Cell(4)),
                        OrderDate = ExcelHelper.GetDateTime(r.Cell(5)),
                        DeliveryDate = ExcelHelper.GetNullableDateTime(r.Cell(6)),
                        TotalAmount = ExcelHelper.GetDecimal(r.Cell(7)),
                        Status = Enum.TryParse<SaleStatus>(ExcelHelper.GetString(r.Cell(8)), true, out var status) ? status : SaleStatus.Draft,
                        Notes = ExcelHelper.GetString(r.Cell(9)),
                        CreatedBy = ExcelHelper.GetString(r.Cell(10)),
                        CreatedAt = ExcelHelper.GetDateTime(r.Cell(11)),
                        Items = allItems.Where(i => i.SaleId == sId).ToList()
                    };
                    context.Sales.Add(sale);
                }
                context.SaveChanges();
            }
        }

        // 9. Invoices
        if (workbook.Worksheets.Contains("Invoices"))
        {
            var ws = workbook.Worksheet("Invoices");
            var rows = ws.RangeUsed()?.RowsUsed().Skip(1);
            if (rows != null)
            {
                foreach (var r in rows)
                {
                    context.Invoices.Add(new Invoice
                    {
                        InvoiceId = ExcelHelper.GetInt(r.Cell(1)),
                        InvoiceNumber = ExcelHelper.GetString(r.Cell(2)),
                        SaleId = ExcelHelper.GetInt(r.Cell(3)),
                        SaleOrderNumber = ExcelHelper.GetString(r.Cell(4)),
                        CustomerId = ExcelHelper.GetInt(r.Cell(5)),
                        CustomerName = ExcelHelper.GetString(r.Cell(6)),
                        IssueDate = ExcelHelper.GetDateTime(r.Cell(7)),
                        DueDate = ExcelHelper.GetDateTime(r.Cell(8)),
                        TotalAmount = ExcelHelper.GetDecimal(r.Cell(9)),
                        PaidAmount = ExcelHelper.GetDecimal(r.Cell(10)),
                        Status = Enum.TryParse<InvoiceStatus>(ExcelHelper.GetString(r.Cell(11)), true, out var status) ? status : InvoiceStatus.Unpaid,
                        Notes = ExcelHelper.GetString(r.Cell(12)),
                        CreatedAt = ExcelHelper.GetDateTime(r.Cell(13))
                    });
                }
                context.SaveChanges();
            }
        }

        // 10. Transactions
        if (workbook.Worksheets.Contains("Transactions"))
        {
            var ws = workbook.Worksheet("Transactions");
            var rows = ws.RangeUsed()?.RowsUsed().Skip(1);
            if (rows != null)
            {
                foreach (var r in rows)
                {
                    context.Transactions.Add(new Transaction
                    {
                        TransactionId = ExcelHelper.GetInt(r.Cell(1)),
                        TransactionNumber = ExcelHelper.GetString(r.Cell(2)),
                        ReferenceType = Enum.TryParse<ReferenceType>(ExcelHelper.GetString(r.Cell(3)), true, out var refType) ? refType : ReferenceType.Other,
                        ReferenceNumber = ExcelHelper.GetString(r.Cell(4)),
                        Type = Enum.TryParse<TransactionType>(ExcelHelper.GetString(r.Cell(5)), true, out var txType) ? txType : TransactionType.Income,
                        Category = ExcelHelper.GetString(r.Cell(6)),
                        Amount = ExcelHelper.GetDecimal(r.Cell(7)),
                        TransactionDate = ExcelHelper.GetDateTime(r.Cell(8)),
                        PaymentMethod = ExcelHelper.GetString(r.Cell(9), "Bank Transfer"),
                        Status = ExcelHelper.GetString(r.Cell(10), "Completed"),
                        Notes = ExcelHelper.GetString(r.Cell(11)),
                        CreatedBy = ExcelHelper.GetString(r.Cell(12), "System")
                    });
                }
                context.SaveChanges();
            }
        }

        // 11. Notifications
        if (workbook.Worksheets.Contains("Notifications"))
        {
            var ws = workbook.Worksheet("Notifications");
            var rows = ws.RangeUsed()?.RowsUsed().Skip(1);
            if (rows != null)
            {
                foreach (var r in rows)
                {
                    int? orderId = null;
                    var orderIdStr = ExcelHelper.GetString(r.Cell(5));
                    if (int.TryParse(orderIdStr, out int parsedOrderId)) orderId = parsedOrderId;

                    var isReadStr = ExcelHelper.GetString(r.Cell(11), "false");
                    bool isRead = bool.TryParse(isReadStr, out bool parsedRead) && parsedRead;

                    context.Notifications.Add(new Notification
                    {
                        NotificationId = ExcelHelper.GetInt(r.Cell(1)),
                        Title = ExcelHelper.GetString(r.Cell(2), "New Order Placed"),
                        Message = ExcelHelper.GetString(r.Cell(3)),
                        Type = ExcelHelper.GetString(r.Cell(4), "Order"),
                        OrderId = orderId,
                        OrderNumber = ExcelHelper.GetString(r.Cell(6)),
                        CustomerName = ExcelHelper.GetString(r.Cell(7)),
                        TotalAmount = ExcelHelper.GetDecimal(r.Cell(8)),
                        OrderStatus = ExcelHelper.GetString(r.Cell(9), "Confirmed"),
                        CreatedAt = ExcelHelper.GetDateTime(r.Cell(10)),
                        IsRead = isRead,
                        TargetRole = ExcelHelper.GetString(r.Cell(12), "Employee")
                    });
                }
                context.SaveChanges();
            }
        }
    }

    private static void SeedDefaultData(ErpDbContext context)
    {
        // 1. Employees
        var emp1 = new Employee { EmployeeId = 1, FirstName = "Admin", LastName = "User", Email = "admin@erp.com", Phone = "+1-555-0100", Department = "Executive", Designation = "System Administrator", Salary = 120000m, JoiningDate = TimeHelper.Now.AddYears(-3), Status = RecordStatus.Active };
        var emp2 = new Employee { EmployeeId = 2, FirstName = "Sarah", LastName = "Jenkins", Email = "sarah.ops@erp.com", Phone = "+1-555-0101", Department = "Operations", Designation = "Operations Manager", Salary = 85000m, JoiningDate = TimeHelper.Now.AddYears(-2), Status = RecordStatus.Active };
        var emp3 = new Employee { EmployeeId = 3, FirstName = "John", LastName = "Doe", Email = "john.sales@erp.com", Phone = "+1-555-0102", Department = "Sales", Designation = "Senior Sales Executive", Salary = 72000m, JoiningDate = TimeHelper.Now.AddYears(-1), Status = RecordStatus.Active };
        var emp4 = new Employee { EmployeeId = 4, FirstName = "Michael", LastName = "Chang", Email = "m.chang@erp.com", Phone = "+1-555-0103", Department = "Logistics", Designation = "Warehouse Lead", Salary = 60000m, JoiningDate = TimeHelper.Now.AddMonths(-8), Status = RecordStatus.Active };
        context.Employees.AddRange(emp1, emp2, emp3, emp4);
        context.SaveChanges();

        // 2. Customers
        var cust1 = new Customer { CustomerId = 1, Name = "Acme Global Industries", ContactPerson = "Alice Walker", Email = "acme.buyer@client.com", Phone = "+1-555-0201", Company = "Acme Corp", Address = "742 Evergreen Terrace, Springfield", CreditLimit = 50000m, CurrentBalance = 1200m, Status = RecordStatus.Active, CreatedAt = TimeHelper.Now.AddMonths(-6) };
        var cust2 = new Customer { CustomerId = 2, Name = "Apex Retail Enterprises", ContactPerson = "Bob Martinez", Email = "apex.buyer@client.com", Phone = "+1-555-0202", Company = "Apex Retail LLC", Address = "100 Innovation Way, Austin, TX", CreditLimit = 75000m, CurrentBalance = 4500m, Status = RecordStatus.Active, CreatedAt = TimeHelper.Now.AddMonths(-4) };
        var cust3 = new Customer { CustomerId = 3, Name = "Nexus Tech Solutions", ContactPerson = "Clara Oswald", Email = "clara@nexustech.io", Phone = "+1-555-0203", Company = "Nexus Tech Inc", Address = "450 Silicon Ave, San Jose, CA", CreditLimit = 100000m, CurrentBalance = 0m, Status = RecordStatus.Active, CreatedAt = TimeHelper.Now.AddMonths(-2) };
        context.Customers.AddRange(cust1, cust2, cust3);
        context.SaveChanges();

        // 3. Users
        string adminHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
        string employeeHash = BCrypt.Net.BCrypt.HashPassword("Employee@123");
        string customerHash = BCrypt.Net.BCrypt.HashPassword("Customer@123");

        context.Users.AddRange(
            new User { UserId = 1, Username = "admin@erp.com", PasswordHash = adminHash, Role = UserRole.Admin, EmployeeId = 1, FullName = "Admin User", Status = RecordStatus.Active, CreatedAt = TimeHelper.Now.AddYears(-3) },
            new User { UserId = 2, Username = "sarah.ops@erp.com", PasswordHash = employeeHash, Role = UserRole.Employee, EmployeeId = 2, FullName = "Sarah Jenkins", Status = RecordStatus.Active, CreatedAt = TimeHelper.Now.AddYears(-2) },
            new User { UserId = 3, Username = "john.sales@erp.com", PasswordHash = employeeHash, Role = UserRole.Employee, EmployeeId = 3, FullName = "John Doe", Status = RecordStatus.Active, CreatedAt = TimeHelper.Now.AddYears(-1) },
            new User { UserId = 4, Username = "acme.buyer@client.com", PasswordHash = customerHash, Role = UserRole.Customer, CustomerId = 1, FullName = "Alice Walker (Acme Corp)", Status = RecordStatus.Active, CreatedAt = TimeHelper.Now.AddMonths(-6) },
            new User { UserId = 5, Username = "apex.buyer@client.com", PasswordHash = customerHash, Role = UserRole.Customer, CustomerId = 2, FullName = "Bob Martinez (Apex Retail)", Status = RecordStatus.Active, CreatedAt = TimeHelper.Now.AddMonths(-4) }
        );
        context.SaveChanges();

        // 4. Products
        context.Products.AddRange(
            new Product { ProductId = 1, SKU = "PROD-LAP-001", Name = "Enterprise Pro Laptop 15\"", Category = "Electronics", Description = "High-performance enterprise laptop 32GB RAM 1TB SSD", UnitPrice = 1299.99m, CostPrice = 850.00m, UnitOfMeasure = "Unit", ReorderLevel = 10, Status = RecordStatus.Active },
            new Product { ProductId = 2, SKU = "PROD-MON-002", Name = "UltraWide 4K Monitor 34\"", Category = "Electronics", Description = "Color calibrated curved monitor for workstations", UnitPrice = 599.99m, CostPrice = 380.00m, UnitOfMeasure = "Unit", ReorderLevel = 15, Status = RecordStatus.Active },
            new Product { ProductId = 3, SKU = "PROD-KEY-003", Name = "Ergonomic Mechanical Keyboard", Category = "Peripherals", Description = "Wireless quiet mechanical keyboard with wrist rest", UnitPrice = 129.50m, CostPrice = 65.00m, UnitOfMeasure = "Unit", ReorderLevel = 25, Status = RecordStatus.Active },
            new Product { ProductId = 4, SKU = "PROD-MOU-004", Name = "Precision Wireless Mouse", Category = "Peripherals", Description = "High precision multi-device laser mouse", UnitPrice = 69.90m, CostPrice = 32.00m, UnitOfMeasure = "Unit", ReorderLevel = 30, Status = RecordStatus.Active },
            new Product { ProductId = 5, SKU = "PROD-DSK-005", Name = "Adjustable Standing Desk 60\"", Category = "Furniture", Description = "Motorized dual-motor height adjustable sit-stand desk", UnitPrice = 549.00m, CostPrice = 310.00m, UnitOfMeasure = "Unit", ReorderLevel = 8, Status = RecordStatus.Active },
            new Product { ProductId = 6, SKU = "PROD-CHR-006", Name = "Executive Ergonomic Mesh Chair", Category = "Furniture", Description = "Lumbar support breathable mesh office chair", UnitPrice = 389.00m, CostPrice = 210.00m, UnitOfMeasure = "Unit", ReorderLevel = 12, Status = RecordStatus.Active },
            new Product { ProductId = 7, SKU = "PROD-CAB-007", Name = "Cat6 Gigabit Ethernet Cable 50ft", Category = "Networking", Description = "Heavy-duty shielded high-speed patch cable", UnitPrice = 24.99m, CostPrice = 9.50m, UnitOfMeasure = "Unit", ReorderLevel = 50, Status = RecordStatus.Active },
            new Product { ProductId = 8, SKU = "PROD-SWT-008", Name = "24-Port Gigabit Managed Switch", Category = "Networking", Description = "Layer 2+ smart managed rackmount switch with PoE", UnitPrice = 349.99m, CostPrice = 220.00m, UnitOfMeasure = "Unit", ReorderLevel = 5, Status = RecordStatus.Active }
        );
        context.SaveChanges();

        // 5. Inventory
        context.Inventory.AddRange(
            new Inventory { InventoryId = 1, ProductId = 1, ProductName = "Enterprise Pro Laptop 15\"", SKU = "PROD-LAP-001", QuantityOnHand = 45, ReservedQuantity = 5, Location = "Warehouse A-01", LastUpdated = TimeHelper.Now },
            new Inventory { InventoryId = 2, ProductId = 2, ProductName = "UltraWide 4K Monitor 34\"", SKU = "PROD-MON-002", QuantityOnHand = 30, ReservedQuantity = 2, Location = "Warehouse A-02", LastUpdated = TimeHelper.Now },
            new Inventory { InventoryId = 3, ProductId = 3, ProductName = "Ergonomic Mechanical Keyboard", SKU = "PROD-KEY-003", QuantityOnHand = 85, ReservedQuantity = 10, Location = "Warehouse B-01", LastUpdated = TimeHelper.Now },
            new Inventory { InventoryId = 4, ProductId = 4, ProductName = "Precision Wireless Mouse", SKU = "PROD-MOU-004", QuantityOnHand = 120, ReservedQuantity = 15, Location = "Warehouse B-02", LastUpdated = TimeHelper.Now },
            new Inventory { InventoryId = 5, ProductId = 5, ProductName = "Adjustable Standing Desk 60\"", SKU = "PROD-DSK-005", QuantityOnHand = 18, ReservedQuantity = 0, Location = "Warehouse C-01", LastUpdated = TimeHelper.Now },
            new Inventory { InventoryId = 6, ProductId = 6, ProductName = "Executive Ergonomic Mesh Chair", SKU = "PROD-CHR-006", QuantityOnHand = 22, ReservedQuantity = 2, Location = "Warehouse C-02", LastUpdated = TimeHelper.Now },
            new Inventory { InventoryId = 7, ProductId = 7, ProductName = "Cat6 Gigabit Ethernet Cable 50ft", SKU = "PROD-CAB-007", QuantityOnHand = 140, ReservedQuantity = 0, Location = "Warehouse D-01", LastUpdated = TimeHelper.Now },
            new Inventory { InventoryId = 8, ProductId = 8, ProductName = "24-Port Gigabit Managed Switch", SKU = "PROD-SWT-008", QuantityOnHand = 7, ReservedQuantity = 1, Location = "Warehouse D-02", LastUpdated = TimeHelper.Now }
        );
        context.SaveChanges();

        // 6. Suppliers
        context.Suppliers.AddRange(
            new Supplier { SupplierId = 1, SupplierCode = "SUP-TECH-01", Name = "TechSource Global Supply", ContactPerson = "David Lee", Email = "orders@techsource.com", Phone = "+1-555-0301", Address = "800 Industrial Pkwy, Chicago, IL", PaymentTerms = "Net 30", Status = RecordStatus.Active, CreatedAt = TimeHelper.Now.AddMonths(-10) },
            new Supplier { SupplierId = 2, SupplierCode = "SUP-FURN-02", Name = "ErgoComfort Manufacturing", ContactPerson = "Emma Watson", Email = "sales@ergocomfort.com", Phone = "+1-555-0302", Address = "250 Timber Rd, Grand Rapids, MI", PaymentTerms = "Net 60", Status = RecordStatus.Active, CreatedAt = TimeHelper.Now.AddMonths(-8) },
            new Supplier { SupplierId = 3, SupplierCode = "SUP-NETW-03", Name = "PrimeLink Network Hardware", ContactPerson = "Robert Frost", Email = "contact@primelink.net", Phone = "+1-555-0303", Address = "1200 Cable St, Seattle, WA", PaymentTerms = "Net 30", Status = RecordStatus.Active, CreatedAt = TimeHelper.Now.AddMonths(-5) }
        );
        context.SaveChanges();

        // 7. Purchases
        var po1 = new Purchase
        {
            PurchaseId = 1,
            PurchaseNumber = "PO-2026-0001",
            SupplierId = 1,
            SupplierName = "TechSource Global Supply",
            PurchaseDate = TimeHelper.Now.AddMonths(-2),
            ExpectedDeliveryDate = TimeHelper.Now.AddMonths(-2).AddDays(5),
            TotalAmount = 25500.00m,
            Status = PurchaseStatus.Received,
            Notes = "Initial Q1 hardware restock",
            CreatedBy = "sarah.ops@erp.com",
            CreatedAt = TimeHelper.Now.AddMonths(-2),
            Items = new List<PurchaseItem>
            {
                new PurchaseItem { PurchaseItemId = 1, PurchaseId = 1, ProductId = 1, ProductName = "Enterprise Pro Laptop 15\"", SKU = "PROD-LAP-001", Quantity = 30, UnitPrice = 850.00m }
            }
        };

        var po2 = new Purchase
        {
            PurchaseId = 2,
            PurchaseNumber = "PO-2026-0002",
            SupplierId = 2,
            SupplierName = "ErgoComfort Manufacturing",
            PurchaseDate = TimeHelper.Now.AddMonths(-1),
            ExpectedDeliveryDate = TimeHelper.Now.AddMonths(-1).AddDays(7),
            TotalAmount = 10400.00m,
            Status = PurchaseStatus.Received,
            Notes = "Office furniture inventory order",
            CreatedBy = "sarah.ops@erp.com",
            CreatedAt = TimeHelper.Now.AddMonths(-1),
            Items = new List<PurchaseItem>
            {
                new PurchaseItem { PurchaseItemId = 2, PurchaseId = 2, ProductId = 5, ProductName = "Adjustable Standing Desk 60\"", SKU = "PROD-DSK-005", Quantity = 20, UnitPrice = 310.00m },
                new PurchaseItem { PurchaseItemId = 3, PurchaseId = 2, ProductId = 6, ProductName = "Executive Ergonomic Mesh Chair", SKU = "PROD-CHR-006", Quantity = 20, UnitPrice = 210.00m }
            }
        };

        var po3 = new Purchase
        {
            PurchaseId = 3,
            PurchaseNumber = "PO-2026-0003",
            SupplierId = 3,
            SupplierName = "PrimeLink Network Hardware",
            PurchaseDate = TimeHelper.Now.AddDays(-5),
            ExpectedDeliveryDate = TimeHelper.Now.AddDays(5),
            TotalAmount = 4400.00m,
            Status = PurchaseStatus.Ordered,
            Notes = "Switches and patch cables",
            CreatedBy = "sarah.ops@erp.com",
            CreatedAt = TimeHelper.Now.AddDays(-5),
            Items = new List<PurchaseItem>
            {
                new PurchaseItem { PurchaseItemId = 4, PurchaseId = 3, ProductId = 8, ProductName = "24-Port Gigabit Managed Switch", SKU = "PROD-SWT-008", Quantity = 20, UnitPrice = 220.00m }
            }
        };

        context.Purchases.AddRange(po1, po2, po3);
        context.SaveChanges();

        // 8. Sales
        var so1 = new Sale
        {
            SaleId = 1,
            SaleOrderNumber = "SO-2026-0001",
            CustomerId = 1,
            CustomerName = "Acme Global Industries",
            OrderDate = TimeHelper.Now.AddMonths(-1).AddDays(-10),
            DeliveryDate = TimeHelper.Now.AddMonths(-1).AddDays(-5),
            TotalAmount = 6499.95m,
            Status = SaleStatus.Fulfilled,
            Notes = "Executive fleet laptops",
            CreatedBy = "john.sales@erp.com",
            CreatedAt = TimeHelper.Now.AddMonths(-1).AddDays(-10),
            Items = new List<SalesItem>
            {
                new SalesItem { SalesItemId = 1, SaleId = 1, ProductId = 1, ProductName = "Enterprise Pro Laptop 15\"", SKU = "PROD-LAP-001", Quantity = 5, UnitPrice = 1299.99m }
            }
        };

        var so2 = new Sale
        {
            SaleId = 2,
            SaleOrderNumber = "SO-2026-0002",
            CustomerId = 2,
            CustomerName = "Apex Retail Enterprises",
            OrderDate = TimeHelper.Now.AddDays(-15),
            DeliveryDate = TimeHelper.Now.AddDays(-10),
            TotalAmount = 4500.00m,
            Status = SaleStatus.Fulfilled,
            Notes = "New branch setup equipment",
            CreatedBy = "john.sales@erp.com",
            CreatedAt = TimeHelper.Now.AddDays(-15),
            Items = new List<SalesItem>
            {
                new SalesItem { SalesItemId = 2, SaleId = 2, ProductId = 2, ProductName = "UltraWide 4K Monitor 34\"", SKU = "PROD-MON-002", Quantity = 5, UnitPrice = 599.99m },
                new SalesItem { SalesItemId = 3, SaleId = 2, ProductId = 5, ProductName = "Adjustable Standing Desk 60\"", SKU = "PROD-DSK-005", Quantity = 2, UnitPrice = 549.00m }
            }
        };

        var so3 = new Sale
        {
            SaleId = 3,
            SaleOrderNumber = "SO-2026-0003",
            CustomerId = 1,
            CustomerName = "Acme Global Industries",
            OrderDate = TimeHelper.Now.AddDays(-3),
            DeliveryDate = null,
            TotalAmount = 1200.00m,
            Status = SaleStatus.Confirmed,
            Notes = "Peripherals expansion",
            CreatedBy = "acme.buyer@client.com",
            CreatedAt = TimeHelper.Now.AddDays(-3),
            Items = new List<SalesItem>
            {
                new SalesItem { SalesItemId = 4, SaleId = 3, ProductId = 3, ProductName = "Ergonomic Mechanical Keyboard", SKU = "PROD-KEY-003", Quantity = 5, UnitPrice = 129.50m },
                new SalesItem { SalesItemId = 5, SaleId = 3, ProductId = 4, ProductName = "Precision Wireless Mouse", SKU = "PROD-MOU-004", Quantity = 8, UnitPrice = 69.90m }
            }
        };

        context.Sales.AddRange(so1, so2, so3);
        context.SaveChanges();

        // 9. Invoices
        context.Invoices.AddRange(
            new Invoice { InvoiceId = 1, InvoiceNumber = "INV-2026-0001", SaleId = 1, SaleOrderNumber = "SO-2026-0001", CustomerId = 1, CustomerName = "Acme Global Industries", IssueDate = TimeHelper.Now.AddMonths(-1).AddDays(-5), DueDate = TimeHelper.Now.AddDays(25), TotalAmount = 6499.95m, PaidAmount = 6499.95m, Status = InvoiceStatus.Paid, Notes = "Paid via ACH transfer", CreatedAt = TimeHelper.Now.AddMonths(-1).AddDays(-5) },
            new Invoice { InvoiceId = 2, InvoiceNumber = "INV-2026-0002", SaleId = 2, SaleOrderNumber = "SO-2026-0002", CustomerId = 2, CustomerName = "Apex Retail Enterprises", IssueDate = TimeHelper.Now.AddDays(-10), DueDate = TimeHelper.Now.AddDays(20), TotalAmount = 4500.00m, PaidAmount = 0.00m, Status = InvoiceStatus.Unpaid, Notes = "Net 30 terms", CreatedAt = TimeHelper.Now.AddDays(-10) },
            new Invoice { InvoiceId = 3, InvoiceNumber = "INV-2026-0003", SaleId = 3, SaleOrderNumber = "SO-2026-0003", CustomerId = 1, CustomerName = "Acme Global Industries", IssueDate = TimeHelper.Now.AddDays(-1), DueDate = TimeHelper.Now.AddDays(29), TotalAmount = 1200.00m, PaidAmount = 0.00m, Status = InvoiceStatus.Unpaid, Notes = "Pending settlement", CreatedAt = TimeHelper.Now.AddDays(-1) }
        );
        context.SaveChanges();

        // 10. Transactions
        context.Transactions.AddRange(
            new Transaction { TransactionId = 1, TransactionNumber = "TXN-2026-0001", ReferenceType = ReferenceType.Purchase, ReferenceNumber = "PO-2026-0001", Type = TransactionType.Expense, Category = "Inventory Purchase", Amount = 25500.00m, TransactionDate = TimeHelper.Now.AddMonths(-2), PaymentMethod = "Bank Transfer", Status = "Completed", Notes = "Supplier settlement PO-2026-0001", CreatedBy = "sarah.ops@erp.com" },
            new Transaction { TransactionId = 2, TransactionNumber = "TXN-2026-0002", ReferenceType = ReferenceType.Purchase, ReferenceNumber = "PO-2026-0002", Type = TransactionType.Expense, Category = "Inventory Purchase", Amount = 10400.00m, TransactionDate = TimeHelper.Now.AddMonths(-1), PaymentMethod = "Bank Transfer", Status = "Completed", Notes = "Supplier settlement PO-2026-0002", CreatedBy = "sarah.ops@erp.com" },
            new Transaction { TransactionId = 3, TransactionNumber = "TXN-2026-0003", ReferenceType = ReferenceType.InvoicePayment, ReferenceNumber = "INV-2026-0001", Type = TransactionType.Income, Category = "Sales Revenue", Amount = 6499.95m, TransactionDate = TimeHelper.Now.AddDays(-20), PaymentMethod = "Bank Transfer", Status = "Completed", Notes = "Customer payment for INV-2026-0001", CreatedBy = "john.sales@erp.com" },
            new Transaction { TransactionId = 4, TransactionNumber = "TXN-2026-0004", ReferenceType = ReferenceType.Salary, ReferenceNumber = "PAY-2026-01", Type = TransactionType.Expense, Category = "Payroll Expense", Amount = 28000.00m, TransactionDate = TimeHelper.Now.AddDays(-28), PaymentMethod = "Direct Deposit", Status = "Completed", Notes = "Monthly employee payroll", CreatedBy = "admin@erp.com" }
        );
        context.SaveChanges();
    }

    private static void EnsureSecurityAndMenusSeeded(ErpDbContext context)
    {
        // 1. Ensure Password Policy
        if (!context.PasswordPolicies.Any())
        {
            context.PasswordPolicies.Add(new PasswordPolicy
            {
                MinLength = 8,
                RequireUppercase = true,
                RequireLowercase = true,
                RequireDigit = true,
                RequireSpecialChar = true,
                ExpiryDays = 90,
                HistoryCount = 5,
                MaxFailedAttempts = 5,
                LockoutDurationMinutes = 15,
                UpdatedAt = TimeHelper.Now
            });
            context.SaveChanges();
        }

        // 2. Ensure Menus and Options
        if (!context.Menus.Any())
        {
            // Dashboard
            var mDashboard = new Menu { Title = "Dashboard", Route = "dashboard", Icon = "bi-grid-1x2-fill", SortOrder = 1, IsActive = true };
            context.Menus.Add(mDashboard);

            // Administration Parent
            var mAdminParent = new Menu { Title = "Administration", Route = null, Icon = "bi-shield-check", SortOrder = 2, IsActive = true };
            context.Menus.Add(mAdminParent);
            context.SaveChanges();

            var mEmployees = new Menu { Title = "Employees", Route = "employees", Icon = "bi-people-fill", ParentMenuId = mAdminParent.MenuId, SortOrder = 1, IsActive = true };
            var mSecurity = new Menu { Title = "Security & Permissions", Route = "security", Icon = "bi-shield-lock-fill", ParentMenuId = mAdminParent.MenuId, SortOrder = 2, IsActive = true };
            context.Menus.AddRange(mEmployees, mSecurity);

            // Business Operations Parent
            var mOpsParent = new Menu { Title = "Business Operations", Route = null, Icon = "bi-kanban", SortOrder = 3, IsActive = true };
            context.Menus.Add(mOpsParent);
            context.SaveChanges();

            var mCustomers = new Menu { Title = "Customers", Route = "customers", Icon = "bi-person-lines-fill", ParentMenuId = mOpsParent.MenuId, SortOrder = 1, IsActive = true };
            var mProducts = new Menu { Title = "Products", Route = "products", Icon = "bi-box-seam-fill", ParentMenuId = mOpsParent.MenuId, SortOrder = 2, IsActive = true };
            var mInventory = new Menu { Title = "Inventory", Route = "inventory", Icon = "bi-archive-fill", ParentMenuId = mOpsParent.MenuId, SortOrder = 3, IsActive = true };
            var mSuppliers = new Menu { Title = "Suppliers", Route = "suppliers", Icon = "bi-truck", ParentMenuId = mOpsParent.MenuId, SortOrder = 4, IsActive = true };
            var mPurchases = new Menu { Title = "Purchases", Route = "purchases", Icon = "bi-bag-check-fill", ParentMenuId = mOpsParent.MenuId, SortOrder = 5, IsActive = true };
            var mSales = new Menu { Title = "Sales Orders", Route = "sales", Icon = "bi-cart-check-fill", ParentMenuId = mOpsParent.MenuId, SortOrder = 6, IsActive = true };
            context.Menus.AddRange(mCustomers, mProducts, mInventory, mSuppliers, mPurchases, mSales);

            // Financials Parent
            var mFinParent = new Menu { Title = "Financials", Route = null, Icon = "bi-cash-coin", SortOrder = 4, IsActive = true };
            context.Menus.Add(mFinParent);
            context.SaveChanges();

            var mInvoices = new Menu { Title = "Invoices", Route = "invoices", Icon = "bi-receipt-cutoff", ParentMenuId = mFinParent.MenuId, SortOrder = 1, IsActive = true };
            var mTransactions = new Menu { Title = "Transactions", Route = "transactions", Icon = "bi-journal-text", ParentMenuId = mFinParent.MenuId, SortOrder = 2, IsActive = true };
            var mReports = new Menu { Title = "Reports & Analytics", Route = "reports", Icon = "bi-graph-up-arrow", ParentMenuId = mFinParent.MenuId, SortOrder = 3, IsActive = true };
            context.Menus.AddRange(mInvoices, mTransactions, mReports);

            // Profile
            var mProfile = new Menu { Title = "My Profile", Route = "profile", Icon = "bi-person-circle", SortOrder = 5, IsActive = true };
            context.Menus.Add(mProfile);
            context.SaveChanges();

            // Options helper
            void AddCrudOptions(Menu m)
            {
                context.MenuOptions.AddRange(
                    new MenuOption { MenuId = m.MenuId, Code = "VIEW", Name = "View", Description = $"View {m.Title}", SortOrder = 1 },
                    new MenuOption { MenuId = m.MenuId, Code = "ADD", Name = "Add", Description = $"Add {m.Title}", SortOrder = 2 },
                    new MenuOption { MenuId = m.MenuId, Code = "EDIT", Name = "Edit", Description = $"Edit {m.Title}", SortOrder = 3 },
                    new MenuOption { MenuId = m.MenuId, Code = "DELETE", Name = "Delete", Description = $"Delete {m.Title}", SortOrder = 4 }
                );
            }

            void AddReadOnlyOption(Menu m)
            {
                context.MenuOptions.Add(new MenuOption { MenuId = m.MenuId, Code = "VIEW", Name = "View", Description = $"View {m.Title}", SortOrder = 1 });
            }

            AddReadOnlyOption(mDashboard);
            AddCrudOptions(mEmployees);
            context.MenuOptions.AddRange(
                new MenuOption { MenuId = mSecurity.MenuId, Code = "VIEW", Name = "View", Description = "View Security & Permissions", SortOrder = 1 },
                new MenuOption { MenuId = mSecurity.MenuId, Code = "EDIT", Name = "Edit", Description = "Manage Security & Permissions", SortOrder = 2 }
            );
            AddCrudOptions(mCustomers);
            AddCrudOptions(mProducts);
            AddCrudOptions(mInventory);
            AddCrudOptions(mSuppliers);
            AddCrudOptions(mPurchases);
            AddCrudOptions(mSales);
            AddCrudOptions(mInvoices);
            AddCrudOptions(mTransactions);
            AddReadOnlyOption(mReports);
            context.MenuOptions.AddRange(
                new MenuOption { MenuId = mProfile.MenuId, Code = "VIEW", Name = "View", Description = "View Profile", SortOrder = 1 },
                new MenuOption { MenuId = mProfile.MenuId, Code = "EDIT", Name = "Edit", Description = "Edit Profile", SortOrder = 2 }
            );

            context.SaveChanges();

            // 3. Seed Role Permissions
            var allOptions = context.MenuOptions.ToList();

            // Admin: All options granted
            foreach (var opt in allOptions)
            {
                context.RolePermissions.Add(new RolePermission
                {
                    Role = UserRole.Admin,
                    MenuId = opt.MenuId,
                    MenuOptionId = opt.MenuOptionId,
                    IsGranted = true
                });
            }

            // Employee: Standard staff access
            var employeeGranted = new HashSet<(string route, string code)>
            {
                ("dashboard", "VIEW"),
                ("customers", "VIEW"), ("customers", "ADD"), ("customers", "EDIT"),
                ("products", "VIEW"), ("products", "ADD"), ("products", "EDIT"),
                ("inventory", "VIEW"), ("inventory", "ADD"), ("inventory", "EDIT"),
                ("suppliers", "VIEW"), ("suppliers", "ADD"), ("suppliers", "EDIT"),
                ("purchases", "VIEW"), ("purchases", "ADD"), ("purchases", "EDIT"),
                ("sales", "VIEW"), ("sales", "ADD"), ("sales", "EDIT"),
                ("invoices", "VIEW"), ("invoices", "ADD"), ("invoices", "EDIT"),
                ("transactions", "VIEW"), ("transactions", "ADD"),
                ("reports", "VIEW"),
                ("profile", "VIEW"), ("profile", "EDIT")
            };

            var menuLookup = context.Menus.ToDictionary(m => m.MenuId);
            foreach (var opt in allOptions)
            {
                var menu = menuLookup[opt.MenuId];
                bool granted = menu.Route != null && employeeGranted.Contains((menu.Route, opt.Code));
                context.RolePermissions.Add(new RolePermission
                {
                    Role = UserRole.Employee,
                    MenuId = opt.MenuId,
                    MenuOptionId = opt.MenuOptionId,
                    IsGranted = granted
                });
            }

            // Customer: Portal access
            var customerGranted = new HashSet<(string route, string code)>
            {
                ("dashboard", "VIEW"),
                ("products", "VIEW"),
                ("sales", "VIEW"), ("sales", "ADD"),
                ("invoices", "VIEW"),
                ("transactions", "VIEW"),
                ("profile", "VIEW"), ("profile", "EDIT")
            };

            foreach (var opt in allOptions)
            {
                var menu = menuLookup[opt.MenuId];
                bool granted = menu.Route != null && customerGranted.Contains((menu.Route, opt.Code));
                context.RolePermissions.Add(new RolePermission
                {
                    Role = UserRole.Customer,
                    MenuId = opt.MenuId,
                    MenuOptionId = opt.MenuOptionId,
                    IsGranted = granted
                });
            }

            context.SaveChanges();
        }
    }

    private static void ResetPostgreSqlSequences(ErpDbContext context)
    {
        try
        {
            var sql = @"
                DO $$
                DECLARE
                    t text;
                    col text;
                    seq text;
                BEGIN
                    FOR t, col IN VALUES 
                        ('Users', 'UserId'),
                        ('Employees', 'EmployeeId'),
                        ('Customers', 'CustomerId'),
                        ('Products', 'ProductId'),
                        ('Inventory', 'InventoryId'),
                        ('Suppliers', 'SupplierId'),
                        ('Purchases', 'PurchaseId'),
                        ('PurchaseItems', 'PurchaseItemId'),
                        ('Sales', 'SaleId'),
                        ('SalesItems', 'SalesItemId'),
                        ('Invoices', 'InvoiceId'),
                        ('Transactions', 'TransactionId'),
                        ('Notifications', 'NotificationId'),
                        ('PasswordPolicies', 'Id'),
                        ('PasswordHistories', 'PasswordHistoryId'),
                        ('Menus', 'MenuId'),
                        ('MenuOptions', 'MenuOptionId'),
                        ('RolePermissions', 'RolePermissionId')
                    LOOP
                        seq := pg_get_serial_sequence(quote_ident(t), col);
                        IF seq IS NOT NULL THEN
                            EXECUTE format('SELECT setval(%L, COALESCE((SELECT MAX(%I) FROM %I), 0) + 1, false)', seq, col, t);
                        END IF;
                    END LOOP;
                END $$;
            ";
            context.Database.ExecuteSqlRaw(sql);
        }
        catch
        {
            // Non-critical: Ignore if database provider or sequence lookup differs
        }
    }
}


