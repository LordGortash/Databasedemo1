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
            // TODO: Write a LINQ query that fetches all customers
            //       and prints their names + emails to the console.
            // HINT: context.Customers
            Console.WriteLine("=== TASK 01: List All Customers ===");

            var customers = await _dbContext.Customers
                // .AsNoTracking() // optional for read-only
                .ToListAsync();

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
            // TODO: Write a query to return all orders,
            //       along with the associated customer name, order status,
            //       and the total quantity of items in that order.

            // HINT: Use Include/ThenInclude or projection with .Select(...).
            //       Summing the quantities: order.OrderItems.Sum(oi => oi.Quantity).

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
            // TODO: Write a query to fetch all products and sort them
            //       by descending price.
            // HINT: context.Products.OrderByDescending(p => p.Price)
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
            // TODO: Write a query to fetch only PENDING orders,
            //       and calculate their total price.
            // HINT: The total can be computed from each OrderItem:
            //       (oi.UnitPrice * oi.Quantity) - oi.Discount
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
                decimal total = 0;
                if (order.OrderItems != null)
                {
                    total = order.OrderItems.Sum(oi =>
                        (oi.UnitPrice ?? 0) * oi.Quantity - (oi.Discount ?? 0));
                }

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
            // TODO: Write a query that groups by Customer,
            //       counting the number of orders each has.

            // HINT: 
            //  1) Join Orders and Customers, or
            //  2) Use the navigation (context.Orders or context.Customers),
            //     then group by customer ID or by the customer entity.
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
            // TODO: Calculate each customer's total order value 
            //       using their Orders -> OrderItems -> (UnitPrice * Quantity - Discount).
            //       Sort descending and take top 3.

            // HINT: You can do this in a single query or multiple steps.
            //       One approach:
            //         1) Summarize each Order's total
            //         2) Summarize for each Customer
            //         3) Order by descending total
            //         4) Take(3)
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
            // TODO: Filter orders to only those with OrderDate >= (DateTime.Now - 30 days).
            //       Output ID, date, and the customer's name.
            Console.WriteLine(" ");
            Console.WriteLine("=== Task 07: Recent Orders ===");

            var cutoffDate = DateTime.Now.AddDays(-100);

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
            // TODO: Group or join OrdersItems by Product.
            //       Summation of quantity.
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
            // TODO: Identify orders with any OrderItem having (Discount > 0).
            //       Display order details, plus the discounted products.
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
        ///     to retrieve a more complex set of data. For example:
        ///     - All orders that contain products in a certain category
        ///       (e.g., "Electronics"), including the store where each product
        ///       is stocked most. (Requires `Stocks`, `Store`, `ProductCategory`, etc.)
        ///     - Or any custom scenario that spans multiple tables.
        /// </summary>
        public async Task Task10_AdvancedQueryExample()
        {
            // TODO: Design your own complex query that demonstrates
            //       multiple joins or navigation paths. For example:
            //       - Orders that contain any product from "Electronics" category.
            //       - Then, find which store has the highest stock of that product.
            //       - Print results.

            // Here's an outline you could explore:
            // 1) Filter products by category name "Electronics"
            // 2) Find any orders that contain these products
            // 3) For each of those products, find the store with the max stock
            //    (requires .MaxBy(...) in .NET 6+ or custom code in older versions)
            // 4) Print a combined result

            // (Implementation is left as an exercise.)
            Console.WriteLine(" ");
            Console.WriteLine("=== Task 10: Advanced Query Example ===");
        }
    }
}
