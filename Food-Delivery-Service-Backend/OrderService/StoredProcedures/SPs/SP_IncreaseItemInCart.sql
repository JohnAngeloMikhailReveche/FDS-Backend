CREATE PROCEDURE SP_IncreaseItemInCart
	@UserId INT,
	@CartItemId INT,
	@Count INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CartId INT;
	DECLARE @CurrentQuantity INT;
	DECLARE @VariantPrice DECIMAL(10, 2);

	/* Get Cart */
	SELECT @CartId = cart_id
	FROM Cart
	WHERE users_id = @UserId;

	IF @CartId IS NULL
		RETURN;

	/* Get Cart Item */
	SELECT
		@CurrentQuantity = quantity,
		@VariantPrice = variant_price
	FROM CartItem
	WHERE cart_item_id = @CartItemId
		AND cart_id = @CartId;

	IF @CurrentQuantity IS NULL
		RETURN;

	/* Increase quantity */
	SET @CurrentQuantity = @CurrentQuantity + @Count;

	/* Update cart item subtotal */
	UPDATE CartItem
	SET
		quantity = @CurrentQuantity,
		computed_subtotal = @CurrentQuantity * @VariantPrice
	WHERE cart_item_id = @CartItemId;

	/* Recalculate cart subtotal */
	UPDATE Cart
	SET
		subtotal = ISNULL((
			SELECT SUM(quantity * variant_price)
			FROM CartItem
			WHERE cart_id = @CartId
		), 0),
		updated_at = GETUTCDATE()
	WHERE cart_id = @CartId;
END
GO