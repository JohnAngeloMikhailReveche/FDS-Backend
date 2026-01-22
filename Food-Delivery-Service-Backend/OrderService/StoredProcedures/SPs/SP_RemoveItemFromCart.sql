CREATE PROCEDURE SP_RemoveItemFromCart
	@UserId INT,
	@CartItemId INT,
	@QuantityToRemove INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CartId INT;
	DECLARE @CurrentQuantity INT;
	DECLARE @VariantPrice DECIMAL(10,2);

	/* Get Cart */
	SELECT @CartId = cart_id
	FROM Cart
	WHERE users_id = @UserId;

	IF @CartId IS NULL
		RETURN;

	/* Get the Cart Item for Deletion */
	SELECT
		@CurrentQuantity = quantity,
		@VariantPrice = variant_price
	FROM CartItem
	WHERE cart_item_id = @CartItemId
		AND cart_id = @CartId;

	IF @CurrentQuantity IS NULL
		RETURN;

	/* Decrease quantity */
	SET @CurrentQuantity = @CurrentQuantity - @QuantityToRemove;

	/* Remove/Update the Quantity of an item */
	IF @CurrentQuantity <= 0
	BEGIN
		DELETE FROM CartItem
		WHERE cart_item_id = @CartItemId;
	END
	ELSE
	BEGIN
		UPDATE CartItem
		SET
			quantity = @CurrentQuantity,
			computed_subtotal = @CurrentQuantity * @VariantPrice
		WHERE cart_item_id = @CartItemId;
	END

	/* Cart subtotal recalculation */
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
