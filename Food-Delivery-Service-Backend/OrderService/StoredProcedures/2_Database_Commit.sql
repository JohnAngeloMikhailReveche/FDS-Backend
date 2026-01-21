USE OrderDatabase

ALTER TABLE CartItem
ADD variant_name NVARCHAR(255) NOT NULL;

ALTER TABLE CartItem
ADD variant_price DECIMAL(10,2) NOT NULL;