## **Assignment: Add Discount Codes & Update an Existing Table with an Enum**

### **Overview**

We want to support **discount codes** (or coupon codes) in our e‐store, where each code can be **percentage‐based** or **flat** (a fixed amount). Instead of storing “percentage” or “flat” as strings, we’ll use a **C# enum** called `DiscountType`. This requires a **new table** (`DiscountCode`), a **foreign key** in `Order`, and an **existing column** change if desired. You’ll demonstrate EF Core **migrations**, including how to store an **enum** in your database.

---

## **Part A: Create an Enum for Discount Types\*\***

1. **Define an Enum**  
   Create a file `DiscountType.cs` (or place it in `DiscountCode.cs` for brevity). For example:

   ```csharp
    public enum DiscountType
    {
        Percentage = 0,
        Flat = 1
    }
   ```

   This indicates:

   - `Percentage` (e.g. “10% off”)
   - `Flat` (e.g. “$5 off”)

2. **Decide How to Store the Enum in the DB**  
   EF Core can store enums as **int** (the numeric value) or as a **string**. Storing as a string can be more readable in queries, while storing as an int is more compact.

---

## **Part B: Define the `DiscountCode` Entity with an Enum**

Create `DiscountCode.cs` inside **Entities** folder:

```csharp
using System;
using System.Collections.Generic;

namespace MyECommerce.Console.Entities
{
    public class DiscountCode
    {
        public int DiscountCodeId { get; set; }

        // e.g. "SUMMER2025"
        public string Code { get; set; } = null!;

        // e.g. "10% off entire order" or "$5 discount for new customers"
        public string? Description { get; set; }

        // Instead of string, use our enum
        public DiscountType DiscountType { get; set; }

        // e.g. 10 => 10% if "Percentage", or $10 if "Flat"
        public decimal DiscountValue { get; set; }

        // Code invalid after this date
        public DateTime? ExpirationDate { get; set; }

        // If single-use, set to 1, else null or larger number
        public int? MaxUsage { get; set; }

        // Times code used so far
        public int TimesUsed { get; set; }

        // Navigation property to Orders using this code
        public ICollection<Order>? Orders { get; set; }
    }
}
```

And in `Order.cs`, add the foreign key:

```csharp
public int? DiscountCodeId { get; set; }
public DiscountCode? DiscountCode { get; set; }
```

---

## **Part C: Configure the Enum in `DbContext`**

Open your `DbContext` (e.g., `WebStoreContext`) and **use** `HasPostgresEnum<>()` in `OnModelCreating` to tell EF Core to **create** a native Postgres enum type for `DiscountType`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // 1) Register the DiscountType enum as a native Postgres enum
    //    This method is provided by the Npgsql.EntityFrameworkCore.PostgreSQL extension.
    //    By default, it will create an enum type in your DB with labels "Percentage" and "Flat".
    modelBuilder.HasPostgresEnum<DiscountType>(
        schema: "public",       // or whatever schema you want
        name: "discount_type"   // the name to use in PostgreSQL
    );

    // 2) Configure the DiscountCode entity to use the new enum type
    modelBuilder.Entity<DiscountCode>(entity =>
    {
        entity.ToTable("discount_codes");

        entity.HasKey(dc => dc.DiscountCodeId);

        // By default, EF will map .NET enum to the new Postgres enum type
        // if we specify the column type as "discount_type"
        entity.Property(dc => dc.DiscountType)
              .HasColumnType("discount_type");  // must match the name above

        // Other columns
        entity.Property(dc => dc.Code)
              .HasMaxLength(50)
              .IsRequired();
    });

    // 3) Relationship to Order
    // Update the existing order configuration by adding the DiscountCode relation
    modelBuilder.Entity<Order>(entity =>
    {

        entity.HasOne(o => o.DiscountCode)
              .WithMany(dc => dc.Orders)
              .HasForeignKey(o => o.DiscountCodeId)
              .OnDelete(DeleteBehavior.SetNull);
    });
}
```

**How This Works**:

- `modelBuilder.HasPostgresEnum<DiscountType>("public", "discount_type")`  
   Tells EF + Npgsql to **create** a Postgres enum type named `"discount_type"` in the `"public"` schema, with labels `"Percentage"` and `"Flat"` (derived from your enum names).
- For the `DiscountCode` entity’s `DiscountType` property, we specify `.HasColumnType("discount_type")`, which matches the newly registered Postgres enum.
- At migration time, EF will **generate** something like:
  ```sql
  CREATE TYPE public.discount_type AS ENUM ('Percentage', 'Flat');
  CREATE TABLE discount_codes (
      discount_code_id int4 NOT NULL,
      discount_type public.discount_type NOT NULL,
      ...
  );
  ```

---

## **Part D: Generate & Apply Migrations**

1. **Baseline Migration** (if needed, look Assignment4.md instructions for example)  
   If you originally scaffolded from a DB with no migrations, do:

   ```bash
   dotnet ef migrations add InitialBaseline
   ```

   and **empty** the `Up()` method to record the existing schema. Then:

   ```bash
   dotnet ef database update
   ```

2. **Add the New Migration**

   ```bash
   dotnet ef migrations add AddDiscountCode
   ```

   EF will pick up the new entity/table, the new column in `orders` for the FK, the new Postgres enum creation script, etc.

3. **Apply the Migration**

   ```bash
   dotnet ef database update
   ```

   - This will create the **`discount_type`** Postgres enum type, create a **`discount_codes`** table, add a **`discount_code_id`** column to `orders`.

---

## **Part E: Testing & Verification**

1. **Manual SQL Insertion**

   - Because the column is a **Postgres enum**, you must insert valid enum labels: `"Percentage"` or `"Flat"`. For example:
     ```sql
     INSERT INTO discount_codes (
         code,
         description,
         discount_type,
         discount_value,
         expiration_date,
         max_usage,
         times_used
     )
     VALUES (
         'SUMMER2025',
         '10% off entire order',
         'Percentage',    -- matches your enum label
         10,
         '2025-09-01',
         100,
         0
     );
     ```
   - If your code uses the enum values, e.g. `DiscountType.Percentage`, EF will convert that to `'Percentage'` when saving.

2. **Update an Order** to Use the Discount

   ```sql
   UPDATE orders
   SET discount_code_id = 1  -- referencing discount_codes.discount_code_id
   WHERE order_id = 42;
   ```

3. **Query** the Column

   ```sql
   SELECT discount_code_id,
          discount_type,
          discount_value
   FROM discount_codes;
   ```

   You’ll see `discount_type` = `"Percentage"` or `"Flat"` (the Postgres enum label).

## **Submission Requirements and evaluation**

1. **Updated Entity Classes (max 2 points)**:
   - `DiscountCodes.cs` file and updated `Order.cs`.
2. **Migrations (max 2 points)**:
   - Include your **baseline** migration (empty) if needed, plus the new **AddDiscountCode** migration in source control.
3. **Manual SQL Scripts (max 1 point)**:
   - Provide new **INSERT** and **UPDATE** statements as `.sql` files.

---
