-- Create database only if it does not exist
IF DB_ID('OrderDatabase') IS NULL
BEGIN
    CREATE DATABASE OrderDatabase;
END
GO

-- Switch to the database
USE OrderDatabase;
GO

-- Define Table : Cart
CREATE TABLE Cart (
    cart_id INT IDENTITY(1,1) PRIMARY KEY,
    users_id INT NOT NULL,
    subtotal DECIMAL(10,2) NOT NULL DEFAULT 0,
    updated_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    vat DECIMAL(10,2) NOT NULL DEFAULT 0
);

-- Define Table : CartItem
CREATE TABLE CartItem (
    cart_item_id INT IDENTITY(1,1) PRIMARY KEY,
    cart_id INT NOT NULL,
    menu_item_id INT NOT NULL,
    variant_id INT NOT NULL,
    item_name NVARCHAR(255) NOT NULL,
    item_description NVARCHAR(255) NOT NULL,
    imgUrl NVARCHAR(255) NOT NULL,
    quantity INT NOT NULL CHECK (quantity > 0),
    computed_subtotal DECIMAL(10,2) NOT NULL,
    special_instructions NVARCHAR(255),
    added_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    delivery_fee DECIMAL(10,2) NOT NULL DEFAULT 0,
    FOREIGN KEY (cart_id) REFERENCES Cart(cart_id)
);

-- Define Table : Order
CREATE TABLE Orders (
    orders_id INT IDENTITY(1,1) PRIMARY KEY,
    users_id INT NOT NULL,
    payment_id INT,
    status TINYINT NOT NULL DEFAULT 0,
    subtotal DECIMAL(10,2) NOT NULL DEFAULT 0,
    total_cost DECIMAL(10,2) NOT NULL DEFAULT 0,
    placed_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    fulfilled_at DATETIME2,
    cancellation_status TINYINT NOT NULL DEFAULT 0,
    cancellation_reason NVARCHAR(255),
    refund_status TINYINT NOT NULL DEFAULT 0,
    delivery_fee DECIMAL(10,2) NOT NULL DEFAULT 0,
    payment_method NVARCHAR(50) NOT NULL,
    item_count INT NOT NULL DEFAULT 0
);

-- Define Table : OrderItem
CREATE TABLE OrderItem (
    order_item_id INT IDENTITY(1,1) PRIMARY KEY,
    orders_id INT NOT NULL,
    menu_item_id INT NOT NULL,
    item_variant_id INT NOT NULL,
    item_name NVARCHAR(255) NOT NULL,
    item_description NVARCHAR(255),
    img_url NVARCHAR(500),
    variant_name NVARCHAR(50),
    variant_price DECIMAL(10,2) NOT NULL,
    quantity INT NOT NULL CHECK (quantity > 0),
    special_instructions NVARCHAR(255),
    line_subtotal DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (orders_id) REFERENCES Orders(orders_id)
);

