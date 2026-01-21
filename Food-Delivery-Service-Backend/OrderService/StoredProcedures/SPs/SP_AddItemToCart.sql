CREATE PROCEDURE SP_AddItemToCart
	@UserId INT,
	@MenuItemId INT,
	@VariantId INT,
	@ItemName NVARCHAR(150),
	@ItemDescription NVARCHAR(500),
	@ImgUrl NVARCHAR(255),
	@VariantName NVARCHAR(100),
	@VariantPrice DECIMAL(10,2),
	@Quantity INT,
	@SpecialInstructions NVARCHAR(500)

AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CartId INT;

	/* Get Cart if it is existing */
	SELECT @CartId = cart_id
	FROM Cart
	WHERE users_id = @UserId;

	/* If it is null then create one */
	IF @CartId IS NULL
	BEGIN

		INSERT INTO Cart(
		users_id
		, subtotal
		, updated_at
		)
		VALUES (
		@UserId
		,0
		,GETUTCDATE()
		);

		/* Get the latest Cart ID that this Insert did */
		SET @CartId = SCOPE_IDENTITY();
	END

	/* Check if the item exists in cart */
	IF EXISTS (
		SELECT 1
		FROM CartItem
		WHERE cart_id = @CartId
			AND menu_item_id = @MenuItemId
			AND variant_id = @VariantId
	)
	BEGIN
		/* Increment Quantity */
		UPDATE CartItem
		SET quantity = quantity + @Quantity
		, computed_subtotal = (quantity + @Quantity) * variant_price
		WHERE cart_id = @CartId
			AND menu_item_id = @MenuItemId
			AND variant_id = @VariantId;
	END
	ELSE
	BEGIN
		/* If it is not existing then create it */
		INSERT INTO CartItem
		(
			cart_id
			, menu_item_id
			, variant_id
			, item_name
			, item_description
			, imgUrl
			, variant_name
			, variant_price
			, quantity
			, computed_subtotal
			, special_instructions
			, added_at
		)
		VALUES
		(
			@CartId
			, @MenuItemId
			, @VariantId
			, @ItemName
			, @ItemDescription
			, @ImgUrl
			, @VariantName
			, @VariantPrice
			, @Quantity
			, @VariantPrice * @Quantity
			, @SpecialInstructions
			, GETDATE()
		);
	END

	/* Recompute the cart subtotal */
	UPDATE Cart
	SET subtotal = (
		SELECT SUM(quantity * variant_price)
		FROM CartItem
		WHERE cart_id = @CartId
	),
	updated_at = GETUTCDATE()
	WHERE cart_id = @CartId;

END
GO