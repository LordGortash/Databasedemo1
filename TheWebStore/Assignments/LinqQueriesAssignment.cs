using Microsoft.EntityFrameworkCore;
using WebStore.Entities;

namespace WebStore.Assignments
{
    /// Additional tutorial materials https://dotnettutorials.net/lesson/linq-to-entities-in-entity-framework-core/

    /// <summary>
    /// This class demonstrates various LINQ query tasks 
    /// to practice querying an EF Core database.
    /// 
    /// ASSIGNMENT INSTRUCTIONS:
    ///   1. For each method labeled "TODO", write the necessary
    ///      LINQ query to return or display the required data.
    ///      
    ///   2. Print meaningful output to the console (or return
    ///      collections, as needed).
    ///      
    ///   3. Test each method by calling it from your Program.cs
    ///      or test harness.
    /// </summary>
    public class LinqQueriesAssignment
    {

        private readonly WebStoreContext _dbContext;

        public LinqQueriesAssignment(WebStoreContext context)
        {
            _dbContext = context;
        }


        /// <summary>
        /// 1. List all customers in the database:
        ///    - Print each customer's full name (First + Last) and Email.
        /// </summary>
        public async Task Task01_ListAllCustomers()
        {
            Console.WriteLine("=== TASK 01: List All Customers ===");

            var customers = await _dbContext.Customers.ToListAsync();

            foreach (var c in customers)
            {
                Console.WriteLine($"{c.FirstName} {c.LastName} - {c.Email}");
            }
        }

        /// <summary>
        /// 2. Fetch all orders along with:
        ///    - Customer Name
        ///    - Order ID
        ///    - Order Status
        ///    - Number of items in each order (the sum of OrderItems.Quantity)
        /// </summary>
        public async Task Task02_ListOrdersWithItemCount()
        {
            Console.WriteLine(" ");
            Console.WriteLine("=== TASK 02: List Orders With Item Count ===");

            var orders = await _dbContext.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ToListAsync();

            foreach (var order in orders)
            {
                var itemCount = order.OrderItems?.Sum(oi => oi.Quantity) ?? 0;
                Console.WriteLine($"Order #{order.OrderId} by {order.Customer?.FirstName} {order.Customer?.LastName} " +
                                         $"| Status: {order.OrderStatus} | Items Count: {itemCount}");
            }
        }

        /// <summary>
        /// 3. List all products (ProductName, Price),
        ///    sorted by price descending (highest first).
        /// </summary>
        public async Task Task03_ListProductsByDescendingPrice()
        {
            Console.WriteLine(" ");
            Console.WriteLine("=== Task 03: List Products By Descending Price ===");

            var products = await _dbContext.Products
                .OrderByDescending(p => p.Price)
                .ToListAsync();

            foreach (var p in products)
            {
                Console.WriteLine($"{p.ProductName} - {p.Price:c}");
            }
        }

        /// <summary>
        /// 4. Find all "Pending" orders (order status = "Pending")
        ///    and display:
        ///      - Customer Name
        ///      - Order ID
        ///      - Order Date
        ///      - Total price (sum of unit_price * quantity - discount) for each order
        /// </summary>
        public async Task Task04_ListPendingOrdersWithTotalPrice()
        {
            Console.WriteLine(" ");
            Console.WriteLine("=== Task 04: List Pending Orders With Total Price ===");

            var pendingOrders = await _dbContext.Orders
                .Where(o => o.OrderStatus == "Pending")
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToListAsync();

            foreach (var order in pendingOrders)
            {
                decimal total = order.OrderItems?.Sum(oi =>
                    (oi.UnitPrice ?? 0) * oi.Quantity - (oi.Discount ?? 0)) ?? 0;

                Console.WriteLine(
                    $"Order #{order.OrderId} - {order.Customer?.FirstName} {order.Customer?.LastName} " +
                    $"| OrderDate: {order.OrderDate} | Total: {total:c}");
            }
        }

        /// <summary>
        /// 5. List the total number of orders each customer has placed.
        ///    Output should show:
        ///      - Customer Full Name
        ///      - Number of Orders
        /// </summary>
        public async Task Task05_OrderCountPerCustomer()
        {
            Console.WriteLine(" ");
            Console.WriteLine("=== Task 05: Order Count Per Customer ===");

            var results = await _dbContext.Customers
                .Select(c => new
                {
                    c.FirstName,
                    c.LastName,
                    OrderCount = c.Orders.Count
                })
                .ToListAsync();

            foreach (var item in results)
            {
                Console.WriteLine($"{item.FirstName} {item.LastName}: Orders = {item.OrderCount}");
            }
        }

        /// <summary>
        /// 6. Show the top 3 customers who have placed the highest total order value overall.
        ///    - For each customer, calculate SUM of (OrderItems * Price).
        ///      Then pick the top 3.
        /// </summary>
        public async Task Task06_Top3CustomersByOrderValue()
        {
            Console.WriteLine(" ");
            Console.WriteLine("=== Task 06: Top 3 Customers By Order Value ===");

            var query = await _dbContext.Customers
                .Select(c => new
                {
                    FullName = c.FirstName + " " + c.LastName,
                    TotalValue = c.Orders
                        .SelectMany(o => o.OrderItems)
                        .Sum(oi => (oi.UnitPrice ?? 0) * oi.Quantity - (oi.Discount ?? 0))
                })
                .OrderByDescending(x => x.TotalValue)
                .Take(3)
                .ToListAsync();

            foreach (var item in query)
            {
                Console.WriteLine($"{item.FullName} - TotalValue: {item.TotalValue:c}");
            }
        }

        /// <summary>
        /// 7. Show all orders placed in the last 30 days (relative to now).
        ///    - Display order ID, date, and customer name.
        /// </summary>
        public async Task Task07_RecentOrders()
        {
            Console.WriteLine(" ");
            Console.WriteLine("=== Task 07: Recent Orders ===");

            var cutoffDate = DateTime.Now.AddDays(-30);

            var recentOrders = await _dbContext.Orders
                .Where(o => o.OrderDate >= cutoffDate)
                .Include(o => o.Customer)
                .ToListAsync();

            foreach (var order in recentOrders)
            {
                Console.WriteLine($"Order #{order.OrderId} - {order.OrderDate} " +
                                         $"- Customer: {order.Customer?.FirstName} {order.Customer?.LastName}");
            }
        }

        /// <summary>
        /// 8. For each product, display how many total items have been sold
        ///    across all orders.
        ///    - Product name, total sold quantity.
        ///    - Sort by total sold descending.
        /// </summary>
        public async Task Task08_TotalSoldPerProduct()
        {
            Console.WriteLine(" ");
            Console.WriteLine("=== Task 08: Total Sold Per Product ===");

            var productSales = await _dbContext.Products
                .Select(p => new
                {
                    p.ProductId,
                    p.ProductName,
                    TotalSold = p.OrderItems.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .ToListAsync();

            foreach (var item in productSales)
            {
                Console.WriteLine($"{item.ProductName} - Sold {item.TotalSold} items");
            }
        }

        /// <summary>
        /// 9. List any orders that have at least one OrderItem with a Discount > 0.
        ///    - Show Order ID, Customer name, and which products were discounted.
        /// </summary>
        public async Task Task09_DiscountedOrders()
        {
            Console.WriteLine(" ");
            Console.WriteLine("=== Task 09: Discounted Orders ===");

            var discountedOrders = await _dbContext.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.OrderItems.Any(oi => (oi.Discount ?? 0) > 0))
                .ToListAsync();

            foreach (var order in discountedOrders)
            {
                var discountedItems = order.OrderItems
                    .Where(oi => (oi.Discount ?? 0) > 0)
                    .Select(oi => oi.Product?.ProductName)
                    .ToList();

                Console.WriteLine(
                    $"Order #{order.OrderId} (Customer: {order.Customer?.FirstName} {order.Customer?.LastName}) " +
                    $"Discounted Products: {string.Join(", ", discountedItems)}"
                );
            }
        }

        /// <summary>
        /// 10. (Open-ended) Combine multiple joins or navigation properties
        ///     to retrieve a more complex set of data.
        /// </summary>
        public async Task Task10_AdvancedQueryExample()
        {
            Console.WriteLine(" ");
            Console.WriteLine("=== Task 10: Advanced Query Example ===");

            var electronicsOrders = await _dbContext.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Categories)
                .Where(o => o.OrderItems
                    .Any(oi => oi.Product.Categories
                        .Any(c => c.CategoryName == "Electronics")))
                .ToListAsync();

            foreach (var order in electronicsOrders)
            {
                var electronicsProducts = order.OrderItems
                    .Where(oi => oi.Product.Categories
                        .Any(c => c.CategoryName == "Electronics"))
                    .Select(oi => oi.Product.ProductName)
                    .Distinct();

                Console.WriteLine(
                    $"Order #{order.OrderId} by {order.Customer?.FirstName} {order.Customer?.LastName} " +
                    $"- Electronics Products: {string.Join(", ", electronicsProducts)}"
                );
            }
        }
    }
}