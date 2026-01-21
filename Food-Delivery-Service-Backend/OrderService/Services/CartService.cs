using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models.DTO;
using System.Data;

namespace OrderService.Services
{
    public class CartService
    {
        private readonly OrderDbContext _db;
        private readonly HttpClient _menuClient;

        public CartService(OrderDbContext db, IHttpClientFactory httpFactory)
        {
            _db = db;
            _menuClient = httpFactory.CreateClient("MenuService");
        }


        public async Task<CartDTO?> AddItem(
                int menuId,
                int variantId,
                int userId,
                string specialInstructions
            )
        {

            /* Get Menu Item from MenuService */
            var menuItem = await _menuClient
                .GetFromJsonAsync<MenuDTO>($"/api/menu-items/{menuId}");

            if (menuItem == null)
            {
                throw new Exception("Item does not exist.");
            }

            // Check if Available
            if (menuItem.isAvailable == false)
            {
                throw new Exception("Item is not available. Please refer to the Menu.");
            }
            // Get the Variant request from the frontend and check if the menuClient has it then map it towards the variant name, variant price and variant id.
            var chosenVariant = menuItem.variants
                .FirstOrDefault(v => v.Id == variantId);

            if (chosenVariant == null)
            {
                throw new Exception("Selected variant does not exist for this menu item.");
            }

            await _db.Database.ExecuteSqlRawAsync(
                @"EXEC SP_AddItemToCart
                    @UserId,
                    @MenuItemId,
                    @VariantId,
                    @ItemName,
                    @ItemDescription,
                    @ImgUrl,
                    @VariantName,
                    @VariantPrice,
                    @Quantity,
                    @SpecialInstructions",
                new SqlParameter("@UserId", userId),
                new SqlParameter("@MenuItemId", menuId),
                new SqlParameter("@VariantId", chosenVariant.Id),
                new SqlParameter("@ItemName", menuItem.name),
                new SqlParameter("@ItemDescription", menuItem.description),
                new SqlParameter("@ImgUrl", menuItem.imageUrl),
                new SqlParameter("@VariantName", chosenVariant.name),
                new SqlParameter("@VariantPrice", chosenVariant.price),
                new SqlParameter("@Quantity", 1),
                new SqlParameter("@SpecialInstructions", specialInstructions)
                );

            return await ViewCart(userId);
        }

        public async Task<CartDTO?> ViewCart(int userId)
        {
            CartDTO? cart = null;

            // Get and Open Database Connection
            using var connection = _db.Database.GetDbConnection();
            await connection.OpenAsync();

            // Create a command which is like an SQL command (EXEC SP_ViewCart) and tell it that it is a Stored Procedure to work.
            using var command = connection.CreateCommand();
            command.CommandText = "SP_ViewCart";
            command.CommandType = CommandType.StoredProcedure;

            // Add the parameter that is needed to run the Stored Procedure, once added it will be ready for execution.
            var userIdParam = command.CreateParameter();
            userIdParam.ParameterName = "@UserId";
            userIdParam.Value = userId;
            command.Parameters.Add(userIdParam);

            using var reader = await command.ExecuteReaderAsync(); // Execute the stored procedure in read mode.

            // Loop through the rows returned by the stored procedure, it will move to the next rows and return false if there are no rows left.
            while (await reader.ReadAsync()) // If ever there is no existing userID, the SQL will return none which will make this ReadAsync return false--meaning the loop will never happen.
            {
                // This will run once since we only need to get the existing cart, because if there is an existing cart then reuse it.
                if (cart == null)
                {
                    cart = new CartDTO
                    {
                        cart_id = reader.GetInt32(reader.GetOrdinal("cart_id")),
                        users_id = reader.GetInt32(reader.GetOrdinal("users_id")),
                        subtotal = reader.GetDecimal(reader.GetOrdinal("subtotal")),
                        updated_at = reader.GetDateTime(reader.GetOrdinal("updated_at")),
                        cartItems = new List<CartItemDTO>()
                    };
                }

                // Checks if the column cart_item_id is not null.
                if (!reader.IsDBNull(reader.GetOrdinal("cart_item_id")))
                {
                    cart.cartItems.Add(new CartItemDTO
                    {
                        cart_item_id = reader.GetInt32(reader.GetOrdinal("cart_item_id")),
                        item_name = reader.GetString(reader.GetOrdinal("item_name")),
                        item_description = reader.GetString(reader.GetOrdinal("item_description")),
                        variant_name = reader.GetString(reader.GetOrdinal("variant_name")),
                        variant_price = reader.GetDecimal(reader.GetOrdinal("variant_price")),
                        quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
                        img_url = reader.GetString(reader.GetOrdinal("imgUrl")),
                        specialInstructions = reader.GetString(reader.GetOrdinal("special_instructions"))
                    });
                }
            }

            return cart;
        }

        public async Task<CartDTO?> RemoveItem(int userID, int cartItemID, int quantityToRemove)
        {
            await _db.Database.ExecuteSqlRawAsync(
                @"EXEC SP_RemoveItemFromCart
                    @UserId
                    , @CartItemId
                    , @QuantityToRemove",
                new SqlParameter("@UserId", userID),
                new SqlParameter("@CartItemId", cartItemID),
                new SqlParameter("@QuantityToRemove", quantityToRemove)
                );

            return await ViewCart(userID);
        }


        public async Task<CartDTO?> IncreaseItem(int userID, int cartItemID, int count)
        {
            await _db.Database.ExecuteSqlRawAsync(
                @"EXEC SP_IncreaseItemInCart
                    @UserId,
                    @CartItemId,
                    @Count",
                new SqlParameter("@UserId", userID),
                new SqlParameter("@CartItemId", cartItemID),
                new SqlParameter("@Count", count)
            );

            return await ViewCart(userID);
        }
    }
}

