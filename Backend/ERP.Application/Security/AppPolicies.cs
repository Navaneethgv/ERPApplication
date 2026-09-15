namespace ERP.Application.Security;

/// <summary>
/// Defines standard policy constants mapped to Menu Master routes and action codes.
/// Supports dynamic authorization checks formatted as "{MenuRoute}:{OptionCode}".
/// </summary>
public static class AppPolicies
{
    public const string PolicyPrefix = "Permissions:";

    public static string Format(string menuRoute, string optionCode) =>
        $"{menuRoute.Trim().ToLowerInvariant()}:{optionCode.Trim().ToUpperInvariant()}";

    public static class Employees
    {
        public const string View = "employees:VIEW";
        public const string Add = "employees:ADD";
        public const string Edit = "employees:EDIT";
        public const string Delete = "employees:DELETE";
    }

    public static class Customers
    {
        public const string View = "customers:VIEW";
        public const string Add = "customers:ADD";
        public const string Edit = "customers:EDIT";
        public const string Delete = "customers:DELETE";
    }

    public static class Products
    {
        public const string View = "products:VIEW";
        public const string Add = "products:ADD";
        public const string Edit = "products:EDIT";
        public const string Delete = "products:DELETE";
    }

    public static class Inventory
    {
        public const string View = "inventory:VIEW";
        public const string Add = "inventory:ADD";
        public const string Edit = "inventory:EDIT";
        public const string Delete = "inventory:DELETE";
    }

    public static class Suppliers
    {
        public const string View = "suppliers:VIEW";
        public const string Add = "suppliers:ADD";
        public const string Edit = "suppliers:EDIT";
        public const string Delete = "suppliers:DELETE";
    }

    public static class Purchases
    {
        public const string View = "purchases:VIEW";
        public const string Add = "purchases:ADD";
        public const string Edit = "purchases:EDIT";
        public const string Delete = "purchases:DELETE";
    }

    public static class Sales
    {
        public const string View = "sales:VIEW";
        public const string Add = "sales:ADD";
        public const string Edit = "sales:EDIT";
        public const string Delete = "sales:DELETE";
    }

    public static class Invoices
    {
        public const string View = "invoices:VIEW";
        public const string Add = "invoices:ADD";
        public const string Edit = "invoices:EDIT";
        public const string Delete = "invoices:DELETE";
    }

    public static class Transactions
    {
        public const string View = "transactions:VIEW";
        public const string Add = "transactions:ADD";
        public const string Edit = "transactions:EDIT";
        public const string Delete = "transactions:DELETE";
    }

    public static class Reports
    {
        public const string View = "reports:VIEW";
    }

    public static class Security
    {
        public const string View = "security:VIEW";
        public const string Edit = "security:EDIT";
    }

    public static class Dashboard
    {
        public const string View = "dashboard:VIEW";
    }

    public static class Profile
    {
        public const string View = "profile:VIEW";
        public const string Edit = "profile:EDIT";
    }
}
