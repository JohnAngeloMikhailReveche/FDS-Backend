CREATE PROCEDURE SP_ViewCart
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		c.cart_id
		, c.users_id
		, c.subtotal
		, c.updated_at
		, ci.cart_item_id
		, ci.item_name
		, ci.variant_name
		, ci.variant_price
		, ci.item_description
		, ci.special_instructions
		, ci.delivery_fee
		, ci.added_at
		, ci.quantity
		, ci.imgUrl
	FROM Cart c
	LEFT JOIN CartItem ci
		ON c.cart_id = ci.cart_id
	WHERE c.users_id = @UserId;
END
GO