using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.DTOs;
using NotificationService.Interfaces;
using System.Security.Claims;

namespace NotificationService.Controllers;
    
[Route("api/notifications/users")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    private string GetUserId()
    {
        // return User.FindFirstValue(ClaimTypes.NameIdentifier)
        //     ?? User.FindFirstValue("sub")
        //     ?? throw new UnauthorizedAccessException("User ID not found in token.");
        
        return "123";
    }

    /// Get all users
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }

    // Create a new user
    // [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> CreateUser(CreateUserDTO userDto)
    {
        var userId = GetUserId();

        try
        {
            var id = await _userService.CreateAsync(userId, userDto);

            return Ok(new
            {
                message = "User created successfully.",
                userId = id
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// Update user information
    // [Authorize]
    [HttpPut("update")]
    public async Task<IActionResult> UpdateUser(CreateUserDTO userDTO)
    {
        var userId = GetUserId();

        try
        {
            var user = await _userService.UpdateAsync(userId, userDTO);
            if (user == null)
                return NotFound(new
                {
                    message = "User not found."
                });
            
            return Ok(new { user, userId });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Failed to update user." });
        }
    }


    /// Delete user account
    // [Authorize]
    [HttpDelete("remove")]
    public async Task<IActionResult> DeleteUser()
    {
        var userId = GetUserId();

        var success = await _userService.DeleteAsync(userId);
        if (!success)
            return NotFound(new
            {
                message = "User not found."
            });

        return Ok(new
        {
            message = "User deleted successfully.",
            userId
        });
    }
}
