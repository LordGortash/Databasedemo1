UPDATE orders
SET "CarrierId" = 1,                     -- DHL
    "TrackingNumber" = 'DH123456789',
    "ShippedDate" = NOW(),
    order_status = 'Shipped'
WHERE order_id = 1;


UPDATE orders
SET "DeliveredDate" = NOW(),
    order_status = 'Delivered'
WHERE order_id = 1;