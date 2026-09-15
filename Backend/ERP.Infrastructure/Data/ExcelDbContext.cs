using ClosedXML.Excel;


using BCrypt.Net;

namespace ERP.Infrastructure.Data;

public class ExcelDbContext
{
    private readonly string _filePath;
    private readonly object _fileLock = new();

    public ExcelDbContext(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var relativePath = configuration["StorageSettings:ExcelFilePath"] ?? "Data/ERP.xlsx";
        _filePath = Path.Combine(environment.ContentRootPath, relativePath);
        InitializeDatabase();
    }

    public void InitializeDatabase()
    {
        lock (_fileLock)
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(_filePath))
            {
                using var workbook = new XLWorkbook();
                CreateAndSeedAllWorksheets(workbook);
                workbook.SaveAs(_filePath);
            }
            else
            {
                // Verify all worksheets exist, if any is missing, create it
                using var workbook = new XLWorkbook(_filePath);
                bool modified = EnsureWorksheetsExist(workbook);
                if (modified)
                {
                    workbook.Save();
                }
            }
        }
    }

    private bool EnsureWorksheetsExist(IXLWorkbook workbook)
    {
        bool modified = false;
        var sheets = new (string Name, string[] Headers)[]
        {
            ("Users", new[] { "UserId", "Username", "PasswordHash", "Role", "EmployeeId", "CustomerId", "FullName", "Status", "CreatedAt" }),
            ("Employees", new[] { "EmployeeId", "FirstName", "LastName", "Email", "Phone", "Department", "Designation", "Salary", "JoiningDate", "Status" }),
            ("Customers", new[] { "CustomerId", "Name", "ContactPerson", "Email", "Phone", "Company", "Address", "CreditLimit", "CurrentBalance", "Status", "CreatedAt" }),
            ("Products", new[] { "ProductId", "SKU", "Name", "Category", "Description", "UnitPrice", "CostPrice", "UnitOfMeasure", "ReorderLevel", "Status" }),
            ("Inventory", new[] { "InventoryId", "ProductId", "ProductName", "SKU", "QuantityOnHand", "ReservedQuantity", "Location", "LastUpdated" }),
            ("Suppliers", new[] { "SupplierId", "SupplierCode", "Name", "ContactPerson", "Email", "Phone", "Address", "PaymentTerms", "Status", "CreatedAt" }),
            ("Purchases", new[] { "PurchaseId", "PurchaseNumber", "SupplierId", "SupplierName", "PurchaseDate", "ExpectedDeliveryDate", "TotalAmount", "Status", "Notes", "CreatedBy", "CreatedAt" }),
            ("PurchaseItems", new[] { "PurchaseItemId", "PurchaseId", "ProductId", "ProductName", "SKU", "Quantity", "UnitPrice" }),
            ("Sales", new[] { "SaleId", "SaleOrderNumber", "CustomerId", "CustomerName", "OrderDate", "DeliveryDate", "TotalAmount", "Status", "Notes", "CreatedBy", "CreatedAt" }),
            ("SalesItems", new[] { "SalesItemId", "SaleId", "ProductId", "ProductName", "SKU", "Quantity", "UnitPrice" }),
            ("Invoices", new[] { "InvoiceId", "InvoiceNumber", "SaleId", "SaleOrderNumber", "CustomerId", "CustomerName", "IssueDate", "DueDate", "TotalAmount", "PaidAmount", "Status", "Notes", "CreatedAt" }),
            ("Transactions", new[] { "TransactionId", "TransactionNumber", "ReferenceType", "ReferenceNumber", "Type", "Category", "Amount", "TransactionDate", "PaymentMethod", "Status", "Notes", "CreatedBy" }),
            ("Notifications", new[] { "NotificationId", "Title", "Message", "Type", "OrderId", "OrderNumber", "CustomerName", "TotalAmount", "OrderStatus", "CreatedAt", "IsRead", "TargetRole" })
        };

        foreach (var (name, headers) in sheets)
        {
            if (!workbook.Worksheets.Contains(name))
            {
                var ws = workbook.Worksheets.Add(name);
                FormatHeaderRow(ws, headers);
                modified = true;
            }
        }

        return modified;
    }

    private void FormatHeaderRow(IXLWorksheet ws, string[] headers)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(15, 23, 42); // Slate-900
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }
        ws.Row(1).Height = 25;
    }

    private void CreateAndSeedAllWorksheets(IXLWorkbook workbook)
    {
        // 1. Employees Worksheet
        var wsEmp = workbook.Worksheets.Add("Employees");
        FormatHeaderRow(wsEmp, new[] { "EmployeeId", "FirstName", "LastName", "Email", "Phone", "Department", "Designation", "Salary", "JoiningDate", "Status" });
        var employees = new[]
        {
            new { Id = 1, First = "Admin", Last = "User", Email = "admin@erp.com", Phone = "+1-555-0100", Dept = "Executive", Desig = "System Administrator", Salary = 120000m, Join = TimeHelper.Now.AddYears(-3), Status = "Active" },
            new { Id = 2, First = "Sarah", Last = "Jenkins", Email = "sarah.ops@erp.com", Phone = "+1-555-0101", Dept = "Operations", Desig = "Operations Manager", Salary = 85000m, Join = TimeHelper.Now.AddYears(-2), Status = "Active" },
            new { Id = 3, First = "John", Last = "Doe", Email = "john.sales@erp.com", Phone = "+1-555-0102", Dept = "Sales", Desig = "Senior Sales Executive", Salary = 72000m, Join = TimeHelper.Now.AddYears(-1), Status = "Active" },
            new { Id = 4, First = "Michael", Last = "Chang", Email = "m.chang@erp.com", Phone = "+1-555-0103", Dept = "Logistics", Desig = "Warehouse Lead", Salary = 60000m, Join = TimeHelper.Now.AddMonths(-8), Status = "Active" }
        };
        for (int i = 0; i < employees.Length; i++)
        {
            var e = employees[i];
            int r = i + 2;
            wsEmp.Cell(r, 1).Value = e.Id;
            wsEmp.Cell(r, 2).Value = e.First;
            wsEmp.Cell(r, 3).Value = e.Last;
            wsEmp.Cell(r, 4).Value = e.Email;
            wsEmp.Cell(r, 5).Value = e.Phone;
            wsEmp.Cell(r, 6).Value = e.Dept;
            wsEmp.Cell(r, 7).Value = e.Desig;
            wsEmp.Cell(r, 8).Value = e.Salary;
            wsEmp.Cell(r, 9).Value = e.Join.ToString("yyyy-MM-dd");
            wsEmp.Cell(r, 10).Value = e.Status;
        }
        wsEmp.Columns().AdjustToContents();

        // 2. Customers Worksheet
        var wsCust = workbook.Worksheets.Add("Customers");
        FormatHeaderRow(wsCust, new[] { "CustomerId", "Name", "ContactPerson", "Email", "Phone", "Company", "Address", "CreditLimit", "CurrentBalance", "Status", "CreatedAt" });
        var customers = new[]
        {
            new { Id = 1, Name = "Acme Global Industries", Contact = "Alice Walker", Email = "acme.buyer@client.com", Phone = "+1-555-0201", Company = "Acme Corp", Addr = "742 Evergreen Terrace, Springfield", Credit = 50000m, Bal = 1200m, Status = "Active", Created = TimeHelper.Now.AddMonths(-6) },
            new { Id = 2, Name = "Apex Retail Enterprises", Contact = "Bob Martinez", Email = "apex.buyer@client.com", Phone = "+1-555-0202", Company = "Apex Retail LLC", Addr = "100 Innovation Way, Austin, TX", Credit = 75000m, Bal = 4500m, Status = "Active", Created = TimeHelper.Now.AddMonths(-4) },
            new { Id = 3, Name = "Nexus Tech Solutions", Contact = "Clara Oswald", Email = "clara@nexustech.io", Phone = "+1-555-0203", Company = "Nexus Tech Inc", Addr = "450 Silicon Ave, San Jose, CA", Credit = 100000m, Bal = 0m, Status = "Active", Created = TimeHelper.Now.AddMonths(-2) }
        };
        for (int i = 0; i < customers.Length; i++)
        {
            var c = customers[i];
            int r = i + 2;
            wsCust.Cell(r, 1).Value = c.Id;
            wsCust.Cell(r, 2).Value = c.Name;
            wsCust.Cell(r, 3).Value = c.Contact;
            wsCust.Cell(r, 4).Value = c.Email;
            wsCust.Cell(r, 5).Value = c.Phone;
            wsCust.Cell(r, 6).Value = c.Company;
            wsCust.Cell(r, 7).Value = c.Addr;
            wsCust.Cell(r, 8).Value = c.Credit;
            wsCust.Cell(r, 9).Value = c.Bal;
            wsCust.Cell(r, 10).Value = c.Status;
            wsCust.Cell(r, 11).Value = c.Created.ToString("yyyy-MM-dd HH:mm:ss");
        }
        wsCust.Columns().AdjustToContents();

        // 3. Users Worksheet
        var wsUsers = workbook.Worksheets.Add("Users");
        FormatHeaderRow(wsUsers, new[] { "UserId", "Username", "PasswordHash", "Role", "EmployeeId", "CustomerId", "FullName", "Status", "CreatedAt" });
        
        string adminHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
        string employeeHash = BCrypt.Net.BCrypt.HashPassword("Employee@123");
        string customerHash = BCrypt.Net.BCrypt.HashPassword("Customer@123");

        var users = new[]
        {
            new { Id = 1, Username = "admin@erp.com", Hash = adminHash, Role = "Admin", EmpId = (int?)1, CustId = (int?)null, Name = "Admin User", Status = "Active", Created = TimeHelper.Now.AddYears(-3) },
            new { Id = 2, Username = "sarah.ops@erp.com", Hash = employeeHash, Role = "Employee", EmpId = (int?)2, CustId = (int?)null, Name = "Sarah Jenkins", Status = "Active", Created = TimeHelper.Now.AddYears(-2) },
            new { Id = 3, Username = "john.sales@erp.com", Hash = employeeHash, Role = "Employee", EmpId = (int?)3, CustId = (int?)null, Name = "John Doe", Status = "Active", Created = TimeHelper.Now.AddYears(-1) },
            new { Id = 4, Username = "acme.buyer@client.com", Hash = customerHash, Role = "Customer", EmpId = (int?)null, CustId = (int?)1, Name = "Alice Walker (Acme Corp)", Status = "Active", Created = TimeHelper.Now.AddMonths(-6) },
            new { Id = 5, Username = "apex.buyer@client.com", Hash = customerHash, Role = "Customer", EmpId = (int?)null, CustId = (int?)2, Name = "Bob Martinez (Apex Retail)", Status = "Active", Created = TimeHelper.Now.AddMonths(-4) }
        };
        for (int i = 0; i < users.Length; i++)
        {
            var u = users[i];
            int r = i + 2;
            wsUsers.Cell(r, 1).Value = u.Id;
            wsUsers.Cell(r, 2).Value = u.Username;
            wsUsers.Cell(r, 3).Value = u.Hash;
            wsUsers.Cell(r, 4).Value = u.Role;
            if (u.EmpId.HasValue) wsUsers.Cell(r, 5).Value = u.EmpId.Value;
            if (u.CustId.HasValue) wsUsers.Cell(r, 6).Value = u.CustId.Value;
            wsUsers.Cell(r, 7).Value = u.Name;
            wsUsers.Cell(r, 8).Value = u.Status;
            wsUsers.Cell(r, 9).Value = u.Created.ToString("yyyy-MM-dd HH:mm:ss");
        }
        wsUsers.Columns().AdjustToContents();

        // 4. Products Worksheet
        var wsProd = workbook.Worksheets.Add("Products");
        FormatHeaderRow(wsProd, new[] { "ProductId", "SKU", "Name", "Category", "Description", "UnitPrice", "CostPrice", "UnitOfMeasure", "ReorderLevel", "Status" });
        var products = new[]
        {
            new { Id = 1, SKU = "PROD-LAP-001", Name = "Enterprise Pro Laptop 15\"", Cat = "Electronics", Desc = "High-performance enterprise laptop 32GB RAM 1TB SSD", UnitPrice = 1299.99m, CostPrice = 850.00m, UOM = "Unit", Reorder = 10, Status = "Active" },
            new { Id = 2, SKU = "PROD-MON-002", Name = "UltraWide 4K Monitor 34\"", Cat = "Electronics", Desc = "Color calibrated curved monitor for workstations", UnitPrice = 599.99m, CostPrice = 380.00m, UOM = "Unit", Reorder = 15, Status = "Active" },
            new { Id = 3, SKU = "PROD-KEY-003", Name = "Ergonomic Mechanical Keyboard", Cat = "Peripherals", Desc = "Wireless quiet mechanical keyboard with wrist rest", UnitPrice = 129.50m, CostPrice = 65.00m, UOM = "Unit", Reorder = 25, Status = "Active" },
            new { Id = 4, SKU = "PROD-MOU-004", Name = "Precision Wireless Mouse", Cat = "Peripherals", Desc = "High precision multi-device laser mouse", UnitPrice = 69.90m, CostPrice = 32.00m, UOM = "Unit", Reorder = 30, Status = "Active" },
            new { Id = 5, SKU = "PROD-DSK-005", Name = "Adjustable Standing Desk 60\"", Cat = "Furniture", Desc = "Motorized dual-motor height adjustable sit-stand desk", UnitPrice = 549.00m, CostPrice = 310.00m, UOM = "Unit", Reorder = 8, Status = "Active" },
            new { Id = 6, SKU = "PROD-CHR-006", Name = "Executive Ergonomic Mesh Chair", Cat = "Furniture", Desc = "Lumbar support breathable mesh office chair", UnitPrice = 389.00m, CostPrice = 210.00m, UOM = "Unit", Reorder = 12, Status = "Active" },
            new { Id = 7, SKU = "PROD-CAB-007", Name = "Cat6 Gigabit Ethernet Cable 50ft", Cat = "Networking", Desc = "Heavy-duty shielded high-speed patch cable", UnitPrice = 24.99m, CostPrice = 9.50m, UOM = "Unit", Reorder = 50, Status = "Active" },
            new { Id = 8, SKU = "PROD-SWT-008", Name = "24-Port Gigabit Managed Switch", Cat = "Networking", Desc = "Layer 2+ smart managed rackmount switch with PoE", UnitPrice = 349.99m, CostPrice = 220.00m, UOM = "Unit", Reorder = 5, Status = "Active" }
        };
        for (int i = 0; i < products.Length; i++)
        {
            var p = products[i];
            int r = i + 2;
            wsProd.Cell(r, 1).Value = p.Id;
            wsProd.Cell(r, 2).Value = p.SKU;
            wsProd.Cell(r, 3).Value = p.Name;
            wsProd.Cell(r, 4).Value = p.Cat;
            wsProd.Cell(r, 5).Value = p.Desc;
            wsProd.Cell(r, 6).Value = p.UnitPrice;
            wsProd.Cell(r, 7).Value = p.CostPrice;
            wsProd.Cell(r, 8).Value = p.UOM;
            wsProd.Cell(r, 9).Value = p.Reorder;
            wsProd.Cell(r, 10).Value = p.Status;
        }
        wsProd.Columns().AdjustToContents();

        // 5. Inventory Worksheet
        var wsInv = workbook.Worksheets.Add("Inventory");
        FormatHeaderRow(wsInv, new[] { "InventoryId", "ProductId", "ProductName", "SKU", "QuantityOnHand", "ReservedQuantity", "Location", "LastUpdated" });
        var inventories = new[]
        {
            new { Id = 1, ProdId = 1, Name = "Enterprise Pro Laptop 15\"", SKU = "PROD-LAP-001", Qty = 45, Reserved = 5, Loc = "Warehouse A-01" },
            new { Id = 2, ProdId = 2, Name = "UltraWide 4K Monitor 34\"", SKU = "PROD-MON-002", Qty = 30, Reserved = 2, Loc = "Warehouse A-02" },
            new { Id = 3, ProdId = 3, Name = "Ergonomic Mechanical Keyboard", SKU = "PROD-KEY-003", Qty = 85, Reserved = 10, Loc = "Warehouse B-01" },
            new { Id = 4, ProdId = 4, Name = "Precision Wireless Mouse", SKU = "PROD-MOU-004", Qty = 120, Reserved = 15, Loc = "Warehouse B-02" },
            new { Id = 5, ProdId = 5, Name = "Adjustable Standing Desk 60\"", SKU = "PROD-DSK-005", Qty = 18, Reserved = 0, Loc = "Warehouse C-01" },
            new { Id = 6, ProdId = 6, Name = "Executive Ergonomic Mesh Chair", SKU = "PROD-CHR-006", Qty = 22, Reserved = 2, Loc = "Warehouse C-02" },
            new { Id = 7, ProdId = 7, Name = "Cat6 Gigabit Ethernet Cable 50ft", SKU = "PROD-CAB-007", Qty = 140, Reserved = 0, Loc = "Warehouse D-01" },
            new { Id = 8, ProdId = 8, Name = "24-Port Gigabit Managed Switch", SKU = "PROD-SWT-008", Qty = 7, Reserved = 1, Loc = "Warehouse D-02" } // Low stock example
        };
        for (int i = 0; i < inventories.Length; i++)
        {
            var inv = inventories[i];
            int r = i + 2;
            wsInv.Cell(r, 1).Value = inv.Id;
            wsInv.Cell(r, 2).Value = inv.ProdId;
            wsInv.Cell(r, 3).Value = inv.Name;
            wsInv.Cell(r, 4).Value = inv.SKU;
            wsInv.Cell(r, 5).Value = inv.Qty;
            wsInv.Cell(r, 6).Value = inv.Reserved;
            wsInv.Cell(r, 7).Value = inv.Loc;
            wsInv.Cell(r, 8).Value = TimeHelper.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        wsInv.Columns().AdjustToContents();

        // 6. Suppliers Worksheet
        var wsSupp = workbook.Worksheets.Add("Suppliers");
        FormatHeaderRow(wsSupp, new[] { "SupplierId", "SupplierCode", "Name", "ContactPerson", "Email", "Phone", "Address", "PaymentTerms", "Status", "CreatedAt" });
        var suppliers = new[]
        {
            new { Id = 1, Code = "SUP-TECH-01", Name = "TechSource Global Supply", Contact = "David Lee", Email = "orders@techsource.com", Phone = "+1-555-0301", Addr = "800 Industrial Pkwy, Chicago, IL", Terms = "Net 30", Status = "Active", Created = TimeHelper.Now.AddMonths(-10) },
            new { Id = 2, Code = "SUP-FURN-02", Name = "ErgoComfort Manufacturing", Contact = "Emma Watson", Email = "sales@ergocomfort.com", Phone = "+1-555-0302", Addr = "250 Timber Rd, Grand Rapids, MI", Terms = "Net 60", Status = "Active", Created = TimeHelper.Now.AddMonths(-8) },
            new { Id = 3, Code = "SUP-NETW-03", Name = "PrimeLink Network Hardware", Contact = "Robert Frost", Email = "contact@primelink.net", Phone = "+1-555-0303", Addr = "1200 Cable St, Seattle, WA", Terms = "Net 30", Status = "Active", Created = TimeHelper.Now.AddMonths(-5) }
        };
        for (int i = 0; i < suppliers.Length; i++)
        {
            var s = suppliers[i];
            int r = i + 2;
            wsSupp.Cell(r, 1).Value = s.Id;
            wsSupp.Cell(r, 2).Value = s.Code;
            wsSupp.Cell(r, 3).Value = s.Name;
            wsSupp.Cell(r, 4).Value = s.Contact;
            wsSupp.Cell(r, 5).Value = s.Email;
            wsSupp.Cell(r, 6).Value = s.Phone;
            wsSupp.Cell(r, 7).Value = s.Addr;
            wsSupp.Cell(r, 8).Value = s.Terms;
            wsSupp.Cell(r, 9).Value = s.Status;
            wsSupp.Cell(r, 10).Value = s.Created.ToString("yyyy-MM-dd HH:mm:ss");
        }
        wsSupp.Columns().AdjustToContents();

        // 7. Purchases & 8. PurchaseItems Worksheets
        var wsPurch = workbook.Worksheets.Add("Purchases");
        FormatHeaderRow(wsPurch, new[] { "PurchaseId", "PurchaseNumber", "SupplierId", "SupplierName", "PurchaseDate", "ExpectedDeliveryDate", "TotalAmount", "Status", "Notes", "CreatedBy", "CreatedAt" });
        var wsPurchItems = workbook.Worksheets.Add("PurchaseItems");
        FormatHeaderRow(wsPurchItems, new[] { "PurchaseItemId", "PurchaseId", "ProductId", "ProductName", "SKU", "Quantity", "UnitPrice" });

        var purchases = new[]
        {
            new { Id = 1, Num = "PO-2026-0001", SuppId = 1, SuppName = "TechSource Global Supply", Date = TimeHelper.Now.AddMonths(-2), Delivery = TimeHelper.Now.AddMonths(-2).AddDays(5), Total = 25500.00m, Status = "Received", Notes = "Initial Q1 hardware restock", By = "sarah.ops@erp.com", Created = TimeHelper.Now.AddMonths(-2) },
            new { Id = 2, Num = "PO-2026-0002", SuppId = 2, SuppName = "ErgoComfort Manufacturing", Date = TimeHelper.Now.AddMonths(-1), Delivery = TimeHelper.Now.AddMonths(-1).AddDays(7), Total = 10400.00m, Status = "Received", Notes = "Office furniture inventory order", By = "sarah.ops@erp.com", Created = TimeHelper.Now.AddMonths(-1) },
            new { Id = 3, Num = "PO-2026-0003", SuppId = 3, SuppName = "PrimeLink Network Hardware", Date = TimeHelper.Now.AddDays(-5), Delivery = TimeHelper.Now.AddDays(5), Total = 4400.00m, Status = "Ordered", Notes = "Switches and patch cables", By = "sarah.ops@erp.com", Created = TimeHelper.Now.AddDays(-5) }
        };
        for (int i = 0; i < purchases.Length; i++)
        {
            var po = purchases[i];
            int r = i + 2;
            wsPurch.Cell(r, 1).Value = po.Id;
            wsPurch.Cell(r, 2).Value = po.Num;
            wsPurch.Cell(r, 3).Value = po.SuppId;
            wsPurch.Cell(r, 4).Value = po.SuppName;
            wsPurch.Cell(r, 5).Value = po.Date.ToString("yyyy-MM-dd");
            wsPurch.Cell(r, 6).Value = po.Delivery.ToString("yyyy-MM-dd");
            wsPurch.Cell(r, 7).Value = po.Total;
            wsPurch.Cell(r, 8).Value = po.Status;
            wsPurch.Cell(r, 9).Value = po.Notes;
            wsPurch.Cell(r, 10).Value = po.By;
            wsPurch.Cell(r, 11).Value = po.Created.ToString("yyyy-MM-dd HH:mm:ss");
        }
        wsPurch.Columns().AdjustToContents();

        var poItems = new[]
        {
            new { Id = 1, PoId = 1, ProdId = 1, Name = "Enterprise Pro Laptop 15\"", SKU = "PROD-LAP-001", Qty = 30, Price = 850.00m },
            new { Id = 2, PoId = 2, ProdId = 5, Name = "Adjustable Standing Desk 60\"", SKU = "PROD-DSK-005", Qty = 20, Price = 310.00m },
            new { Id = 3, PoId = 2, ProdId = 6, Name = "Executive Ergonomic Mesh Chair", SKU = "PROD-CHR-006", Qty = 20, Price = 210.00m },
            new { Id = 4, PoId = 3, ProdId = 8, Name = "24-Port Gigabit Managed Switch", SKU = "PROD-SWT-008", Qty = 20, Price = 220.00m }
        };
        for (int i = 0; i < poItems.Length; i++)
        {
            var item = poItems[i];
            int r = i + 2;
            wsPurchItems.Cell(r, 1).Value = item.Id;
            wsPurchItems.Cell(r, 2).Value = item.PoId;
            wsPurchItems.Cell(r, 3).Value = item.ProdId;
            wsPurchItems.Cell(r, 4).Value = item.Name;
            wsPurchItems.Cell(r, 5).Value = item.SKU;
            wsPurchItems.Cell(r, 6).Value = item.Qty;
            wsPurchItems.Cell(r, 7).Value = item.Price;
        }
        wsPurchItems.Columns().AdjustToContents();

        // 9. Sales & 10. SalesItems Worksheets
        var wsSales = workbook.Worksheets.Add("Sales");
        FormatHeaderRow(wsSales, new[] { "SaleId", "SaleOrderNumber", "CustomerId", "CustomerName", "OrderDate", "DeliveryDate", "TotalAmount", "Status", "Notes", "CreatedBy", "CreatedAt" });
        var wsSalesItems = workbook.Worksheets.Add("SalesItems");
        FormatHeaderRow(wsSalesItems, new[] { "SalesItemId", "SaleId", "ProductId", "ProductName", "SKU", "Quantity", "UnitPrice" });

        var sales = new[]
        {
            new { Id = 1, Num = "SO-2026-0001", CustId = 1, CustName = "Acme Global Industries", Date = TimeHelper.Now.AddMonths(-1).AddDays(-10), Delivery = (DateTime?)TimeHelper.Now.AddMonths(-1).AddDays(-5), Total = 6499.95m, Status = "Fulfilled", Notes = "Executive fleet laptops", By = "john.sales@erp.com", Created = TimeHelper.Now.AddMonths(-1).AddDays(-10) },
            new { Id = 2, Num = "SO-2026-0002", CustId = 2, CustName = "Apex Retail Enterprises", Date = TimeHelper.Now.AddDays(-15), Delivery = (DateTime?)TimeHelper.Now.AddDays(-10), Total = 4500.00m, Status = "Fulfilled", Notes = "New branch setup equipment", By = "john.sales@erp.com", Created = TimeHelper.Now.AddDays(-15) },
            new { Id = 3, Num = "SO-2026-0003", CustId = 1, CustName = "Acme Global Industries", Date = TimeHelper.Now.AddDays(-3), Delivery = (DateTime?)null, Total = 1200.00m, Status = "Confirmed", Notes = "Peripherals expansion", By = "acme.buyer@client.com", Created = TimeHelper.Now.AddDays(-3) }
        };
        for (int i = 0; i < sales.Length; i++)
        {
            var so = sales[i];
            int r = i + 2;
            wsSales.Cell(r, 1).Value = so.Id;
            wsSales.Cell(r, 2).Value = so.Num;
            wsSales.Cell(r, 3).Value = so.CustId;
            wsSales.Cell(r, 4).Value = so.CustName;
            wsSales.Cell(r, 5).Value = so.Date.ToString("yyyy-MM-dd");
            if (so.Delivery.HasValue) wsSales.Cell(r, 6).Value = so.Delivery.Value.ToString("yyyy-MM-dd");
            wsSales.Cell(r, 7).Value = so.Total;
            wsSales.Cell(r, 8).Value = so.Status;
            wsSales.Cell(r, 9).Value = so.Notes;
            wsSales.Cell(r, 10).Value = so.By;
            wsSales.Cell(r, 11).Value = so.Created.ToString("yyyy-MM-dd HH:mm:ss");
        }
        wsSales.Columns().AdjustToContents();

        var soItems = new[]
        {
            new { Id = 1, SoId = 1, ProdId = 1, Name = "Enterprise Pro Laptop 15\"", SKU = "PROD-LAP-001", Qty = 5, Price = 1299.99m },
            new { Id = 2, SoId = 2, ProdId = 2, Name = "UltraWide 4K Monitor 34\"", SKU = "PROD-MON-002", Qty = 5, Price = 599.99m },
            new { Id = 3, SoId = 2, ProdId = 5, Name = "Adjustable Standing Desk 60\"", SKU = "PROD-DSK-005", Qty = 2, Price = 549.00m },
            new { Id = 4, SoId = 3, ProdId = 3, Name = "Ergonomic Mechanical Keyboard", SKU = "PROD-KEY-003", Qty = 5, Price = 129.50m },
            new { Id = 5, SoId = 3, ProdId = 4, Name = "Precision Wireless Mouse", SKU = "PROD-MOU-004", Qty = 8, Price = 69.90m }
        };
        for (int i = 0; i < soItems.Length; i++)
        {
            var item = soItems[i];
            int r = i + 2;
            wsSalesItems.Cell(r, 1).Value = item.Id;
            wsSalesItems.Cell(r, 2).Value = item.SoId;
            wsSalesItems.Cell(r, 3).Value = item.ProdId;
            wsSalesItems.Cell(r, 4).Value = item.Name;
            wsSalesItems.Cell(r, 5).Value = item.SKU;
            wsSalesItems.Cell(r, 6).Value = item.Qty;
            wsSalesItems.Cell(r, 7).Value = item.Price;
        }
        wsSalesItems.Columns().AdjustToContents();

        // 11. Invoices Worksheet
        var wsInvHdr = workbook.Worksheets.Add("Invoices");
        FormatHeaderRow(wsInvHdr, new[] { "InvoiceId", "InvoiceNumber", "SaleId", "SaleOrderNumber", "CustomerId", "CustomerName", "IssueDate", "DueDate", "TotalAmount", "PaidAmount", "Status", "Notes", "CreatedAt" });
        var invoices = new[]
        {
            new { Id = 1, Num = "INV-2026-0001", SoId = 1, SoNum = "SO-2026-0001", CustId = 1, CustName = "Acme Global Industries", Issue = TimeHelper.Now.AddMonths(-1).AddDays(-5), Due = TimeHelper.Now.AddDays(25), Total = 6499.95m, Paid = 6499.95m, Status = "Paid", Notes = "Paid via ACH transfer", Created = TimeHelper.Now.AddMonths(-1).AddDays(-5) },
            new { Id = 2, Num = "INV-2026-0002", SoId = 2, SoNum = "SO-2026-0002", CustId = 2, CustName = "Apex Retail Enterprises", Issue = TimeHelper.Now.AddDays(-10), Due = TimeHelper.Now.AddDays(20), Total = 4500.00m, Paid = 0.00m, Status = "Unpaid", Notes = "Net 30 terms", Created = TimeHelper.Now.AddDays(-10) },
            new { Id = 3, Num = "INV-2026-0003", SoId = 3, SoNum = "SO-2026-0003", CustId = 1, CustName = "Acme Global Industries", Issue = TimeHelper.Now.AddDays(-1), Due = TimeHelper.Now.AddDays(29), Total = 1200.00m, Paid = 0.00m, Status = "Unpaid", Notes = "Pending settlement", Created = TimeHelper.Now.AddDays(-1) }
        };
        for (int i = 0; i < invoices.Length; i++)
        {
            var inv = invoices[i];
            int r = i + 2;
            wsInvHdr.Cell(r, 1).Value = inv.Id;
            wsInvHdr.Cell(r, 2).Value = inv.Num;
            wsInvHdr.Cell(r, 3).Value = inv.SoId;
            wsInvHdr.Cell(r, 4).Value = inv.SoNum;
            wsInvHdr.Cell(r, 5).Value = inv.CustId;
            wsInvHdr.Cell(r, 6).Value = inv.CustName;
            wsInvHdr.Cell(r, 7).Value = inv.Issue.ToString("yyyy-MM-dd");
            wsInvHdr.Cell(r, 8).Value = inv.Due.ToString("yyyy-MM-dd");
            wsInvHdr.Cell(r, 9).Value = inv.Total;
            wsInvHdr.Cell(r, 10).Value = inv.Paid;
            wsInvHdr.Cell(r, 11).Value = inv.Status;
            wsInvHdr.Cell(r, 12).Value = inv.Notes;
            wsInvHdr.Cell(r, 13).Value = inv.Created.ToString("yyyy-MM-dd HH:mm:ss");
        }
        wsInvHdr.Columns().AdjustToContents();

        // 12. Transactions Worksheet
        var wsTxn = workbook.Worksheets.Add("Transactions");
        FormatHeaderRow(wsTxn, new[] { "TransactionId", "TransactionNumber", "ReferenceType", "ReferenceNumber", "Type", "Category", "Amount", "TransactionDate", "PaymentMethod", "Status", "Notes", "CreatedBy" });
        var transactions = new[]
        {
            new { Id = 1, Num = "TXN-2026-0001", RefType = "Purchase", RefNum = "PO-2026-0001", Type = "Expense", Cat = "Inventory Purchase", Amt = 25500.00m, Date = TimeHelper.Now.AddMonths(-2), Method = "Bank Transfer", Status = "Completed", Notes = "Supplier settlement PO-2026-0001", By = "sarah.ops@erp.com" },
            new { Id = 2, Num = "TXN-2026-0002", RefType = "Purchase", RefNum = "PO-2026-0002", Type = "Expense", Cat = "Inventory Purchase", Amt = 10400.00m, Date = TimeHelper.Now.AddMonths(-1), Method = "Bank Transfer", Status = "Completed", Notes = "Supplier settlement PO-2026-0002", By = "sarah.ops@erp.com" },
            new { Id = 3, Num = "TXN-2026-0003", RefType = "InvoicePayment", RefNum = "INV-2026-0001", Type = "Income", Cat = "Sales Revenue", Amt = 6499.95m, Date = TimeHelper.Now.AddDays(-20), Method = "Bank Transfer", Status = "Completed", Notes = "Customer payment for INV-2026-0001", By = "john.sales@erp.com" },
            new { Id = 4, Num = "TXN-2026-0004", RefType = "Salary", RefNum = "PAY-2026-01", Type = "Expense", Cat = "Payroll Expense", Amt = 28000.00m, Date = TimeHelper.Now.AddDays(-28), Method = "Direct Deposit", Status = "Completed", Notes = "Monthly employee payroll", By = "admin@erp.com" }
        };
        for (int i = 0; i < transactions.Length; i++)
        {
            var t = transactions[i];
            int r = i + 2;
            wsTxn.Cell(r, 1).Value = t.Id;
            wsTxn.Cell(r, 2).Value = t.Num;
            wsTxn.Cell(r, 3).Value = t.RefType;
            wsTxn.Cell(r, 4).Value = t.RefNum;
            wsTxn.Cell(r, 5).Value = t.Type;
            wsTxn.Cell(r, 6).Value = t.Cat;
            wsTxn.Cell(r, 7).Value = t.Amt;
            wsTxn.Cell(r, 8).Value = t.Date.ToString("yyyy-MM-dd HH:mm:ss");
            wsTxn.Cell(r, 9).Value = t.Method;
            wsTxn.Cell(r, 10).Value = t.Status;
            wsTxn.Cell(r, 11).Value = t.Notes;
            wsTxn.Cell(r, 12).Value = t.By;
        }
        wsTxn.Columns().AdjustToContents();

        // 13. Notifications Worksheet
        var wsNotif = workbook.Worksheets.Add("Notifications");
        FormatHeaderRow(wsNotif, new[] { "NotificationId", "Title", "Message", "Type", "OrderId", "OrderNumber", "CustomerName", "TotalAmount", "OrderStatus", "CreatedAt", "IsRead", "TargetRole" });
        wsNotif.Columns().AdjustToContents();
    }

    /// <summary>
    /// Thread-safe read executor on Excel workbook
    /// </summary>
    public T Read<T>(Func<IXLWorkbook, T> query)
    {
        lock (_fileLock)
        {
            using var workbook = new XLWorkbook(_filePath);
            return query(workbook);
        }
    }

    /// <summary>
    /// Thread-safe write executor on Excel workbook
    /// </summary>
    public T Write<T>(Func<IXLWorkbook, T> command)
    {
        lock (_fileLock)
        {
            using var workbook = new XLWorkbook(_filePath);
            var result = command(workbook);
            workbook.Save();
            return result;
        }
    }

    /// <summary>
    /// Thread-safe write executor without return value
    /// </summary>
    public void Write(Action<IXLWorkbook> command)
    {
        lock (_fileLock)
        {
            using var workbook = new XLWorkbook(_filePath);
            command(workbook);
            workbook.Save();
        }
    }
}


