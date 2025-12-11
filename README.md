# Notification Service

A microservice responsible for managing and delivering notifications to users through multiple channels (In-App, SMS, and Email).

## Overview

The Notification Service provides API for managing user notifications across food delivery system's services. It handles retrieval, creation, status updates, and deletion of notifications while enforcing strict authentication and authorization controls.

## Table of Contents

- [Getting Started](#getting-started)
- [API Endpoints](#api-endpoints)
- [Authentication](#authentication)
- [Endpoints Documentation](#endpoints-documentation)
  - [Get All Notifications](#get-all-notifications)
  - [Get Specific Notification](#get-specific-notification)
  - [Send Notification](#send-notification)
  - [Mark as Read](#mark-as-read)
  - [Delete All Notifications](#delete-all-notifications)
  - [Delete Specific Notification](#delete-specific-notification)
- [Notification Types](#notification-types)
- [Notification Status](#notification-status)
- [Data Models](#data-models)
- [Database Schema](#database-schema)
- [Error Handling](#error-handling)
- [Best Practices](#best-practices)
- [Support](#support)

## Getting Started

### Prerequisites

- .NET 8.0 or higher

### Installation

1. Clone the repository
2. Restore dependencies: `dotnet restore`
3. Build the project: `dotnet build`
4. Run the application: `dotnet run`

## API Endpoints

| No. | Method | Endpoint | Description |
|-----|--------|----------|-------------|
| 1 | GET | `/api/notifications` | Get all notifications for a specific user |
| 2 | GET | `/api/notifications/{notifId}` | Get a specific notification |
| 3 | POST | `/api/notifications/notify` | Send In-App/SMS/Email notification |
| 4 | PATCH | `/api/notifications/{notifId}` | Mark a notification as read |
| 5 | DELETE | `/api/notifications` | Delete all notifications for a user |
| 6 | DELETE | `/api/notifications/{notifId}` | Delete a specific notification |

## Authentication

All endpoints require authentication via Bearer token in the Authorization header.

**Required Header:**
```
Authorization: Bearer <token>
```

**Access Control:**
- Users can only access their own notifications
- Admin users may have additional permissions for moderation

---

## Endpoints Documentation

### 1. Get All Notifications

Retrieve all notifications belonging to the authenticated user.

#### HTTP Request

```
GET /api/notifications
```

#### Permissions

- Must be authenticated
- Only user owners can access their own notifications

#### Request Headers

| Header | Required | Description |
|--------|----------|-------------|
| Authorization | Yes | Bearer token for authentication |
| Accept | No | Expected response format (application/json) |

#### Request Body

None

#### Response Body (Success)

**Status Code: 200 OK**

```json
{
  "notifications": [
    {
      "id": "notif123",
      "type": "account",
      "title": "Account Created Successfully",
      "message": "Your account has been created successfully.",
      "status": "unread",
      "createdAt": "2025-12-05T08:00:00Z"
    },
    {
      "id": "notif124",
      "type": "cart",
      "title": "Item Added to Cart",
      "message": "You have successfully added 'Laptop' to your cart.",
      "status": "unread",
      "createdAt": "2025-12-05T08:05:00Z"
    }
  ]
}
```

#### Error Responses

**Status Code: 401 Unauthorized**
```json
{
  "error": "UNAUTHORIZED",
  "message": "User is not authenticated"
}
```

**Status Code: 403 Forbidden**
```json
{
  "error": "ACCESS_DENIED",
  "message": "You are not allowed to access notifications for this user"
}
```

**Status Code: 404 Not Found**
```json
{
  "error": "NO_NOTIFICATIONS",
  "message": "No notifications found for this user"
}
```

**Status Code: 500 Internal Server Error**
```json
{
  "error": "SERVER_ERROR",
  "message": "Unable to retrieve notification at this time"
}
```

#### Business Logic

1. Validate access token
2. Verify the user is the owner
3. Fetch notifications from the database for the specified userId
4. Return notifications in descending order of createdAt
5. If no notifications exist, return 404 with an appropriate message

---

### 2. Get Specific Notification

Retrieve a single notification by its ID.

#### HTTP Request

```
GET /api/notifications/{notifId}
```

#### Permissions

- Must be authenticated
- Only user owners can access their notifications

#### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| notifId | string | Yes | Unique ID of the notification to retrieve |

#### Request Headers

| Header | Required | Description |
|--------|----------|-------------|
| Authorization | Yes | Bearer token for authentication |
| Accept | No | Expected response format (application/json) |

#### Request Body

None

#### Response Body (Success)

**Status Code: 200 OK**

```json
{
  "id": "notif123",
  "type": "account",
  "title": "Account Created Successfully",
  "message": "Your account has been created successfully.",
  "status": "unread",
  "createdAt": "2025-12-05T08:00:00Z"
}
```

#### Error Responses

**Status Code: 401 Unauthorized**
```json
{
  "error": "UNAUTHORIZED",
  "message": "User is not authenticated"
}
```

**Status Code: 403 Forbidden**
```json
{
  "error": "ACCESS_DENIED",
  "message": "You are not allowed to access notifications for this user"
}
```

**Status Code: 404 Not Found**
```json
{
  "error": "NO_NOTIFICATIONS",
  "message": "No notifications found for this user"
}
```

**Status Code: 500 Internal Server Error**
```json
{
  "error": "SERVER_ERROR",
  "message": "Unable to retrieve notification at this time"
}
```

#### Business Logic

1. Validate access token
2. Verify the user is the owner
3. Fetch the notification from the database using notifId and userId
4. If found, return the notification details
5. If not found, return 404 Not Found
6. If the user is unauthorized, return 401; if access is forbidden, return 403

---

### 3. Send Notification

Create and send a notification to a user via In-App, SMS, or Email.

#### HTTP Request

```
POST /api/notifications/notify
```

#### Permissions

- Must be authenticated
- Only user owners can send notifications for themselves

#### Request Headers

| Header | Required | Description |
|--------|----------|-------------|
| Authorization | Yes | Bearer token for authentication |
| Accept | No | Expected response format (application/json) |
| Content-Type | Yes | Must be application/json |

#### Request Body

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| type | string | Yes | Type of notification: "in-app", "sms", or "email" |
| title | string | Yes | Short title of the notification |
| message | string | Yes | Detailed notification message |
| extraData | object | No | Additional information (e.g., orderId, trackingNumber) |

**Example Request:**

```json
{
  "type": "in-app",
  "title": "Order Delivered",
  "number/email": "example@gmail.com",
  "message": "Your order #1234 has been delivered.",
  "extraData": {
    "orderId": "1234",
    "trackingNumber": "TRACK12345"
  }
}
```

#### Response Body (Success)

**Status Code: 201 Created**

```json
{
  "id": "notif123",
  "userId": "user456",
  "type": "in-app",
  "title": "Order Delivered",
  "message": "Your order #1234 has been delivered.",
  "status": "unread",
  "createdAt": "2025-12-05T10:00:00Z",
  "extraData": {
    "orderId": "1234",
    "trackingNumber": "TRACK12345"
  }
}
```

#### Error Responses

**Status Code: 400 Bad Request**
```json
{
  "error": "INVALID_REQUEST",
  "message": "Missing required field 'type' or 'message'"
}
```

**Status Code: 401 Unauthorized**
```json
{
  "error": "UNAUTHORIZED",
  "message": "User is not authenticated"
}
```

**Status Code: 403 Forbidden**
```json
{
  "error": "ACCESS_DENIED",
  "message": "You are not allowed to access notifications for this user"
}
```

**Status Code: 404 Not Found**
```json
{
  "error": "NO_NOTIFICATIONS",
  "message": "No notifications found for this user"
}
```

**Status Code: 500 Internal Server Error**
```json
{
  "error": "SERVER_ERROR",
  "message": "Unable to retrieve notification at this time"
}
```

#### Business Logic

1. Validate access token
2. Validate the request body (type, title, message are required)
3. Verify that the sender has permission to notify the target user
4. Save the notification in the database (for in-app) or send it via SMS/Email gateway
5. Return the created notification object with status 201 Created

---

### 4. Mark as Read

Update a notification's status to "read".

#### HTTP Request

```
PATCH /api/notifications/{notifId}
```

#### Permissions

- Must be authenticated
- Only user owners can update their notifications

#### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| notifId | string | Yes | Unique ID of the notification |

#### Request Headers

| Header | Required | Description |
|--------|----------|-------------|
| Authorization | Yes | Bearer token for authentication |
| Accept | No | Expected response format (application/json) |
| Content-Type | Yes | Must be application/json |

#### Request Body

```json
{
  "status": "read"
}
```

#### Response Body (Success)

**Status Code: 200 OK**

```json
{
  "id": "notif123",
  "userId": "user001",
  "type": "in-app",
  "title": "Order Ready for Pickup",
  "message": "Your order #553 is now ready.",
  "status": "read",
  "updatedAt": "2025-12-07T12:45:00Z",
  "createdAt": "2025-12-07T11:30:00Z"
}
```

#### Error Responses

**Status Code: 400 Bad Request**
```json
{
  "error": "INVALID_REQUEST",
  "message": "Invalid status value"
}
```

**Status Code: 401 Unauthorized**
```json
{
  "error": "UNAUTHORIZED",
  "message": "User is not authenticated"
}
```

**Status Code: 403 Forbidden**
```json
{
  "error": "ACCESS_DENIED",
  "message": "You are not allowed to access notifications for this user"
}
```

**Status Code: 404 Not Found**
```json
{
  "error": "NO_NOTIFICATIONS",
  "message": "No notifications found for this user"
}
```

**Status Code: 500 Internal Server Error**
```json
{
  "error": "SERVER_ERROR",
  "message": "Unable to retrieve notification at this time"
}
```

#### Business Logic

1. Validate access token
2. Ensure userId matches authenticated user
3. Find notification by notifId belonging to user
4. Update status to "read"
5. Save timestamp for updatedAt
6. Return updated notification object with status 200 OK

---

### 5. Delete All Notifications

Delete all notifications for the authenticated user.

#### HTTP Request

```
DELETE /api/notifications
```

#### Permissions

- Must be authenticated
- Only user owners can delete their notifications
- Admin may delete for moderation or account reset (optional logic)

#### Request Headers

| Header | Required | Description |
|--------|----------|-------------|
| Authorization | Yes | Bearer token for authentication |
| Accept | No | Expected response format (application/json) |
| Content-Type | Yes | Must be application/json |

#### Request Body

None

#### Response Body (Success)

**Status Code: 204 No Content**

```json
{
  "userId": "user123",
  "message": "All notifications successfully deleted."
}
```

#### Error Responses

**Status Code: 401 Unauthorized**
```json
{
  "error": "UNAUTHORIZED",
  "message": "User is not authenticated"
}
```

**Status Code: 403 Forbidden**
```json
{
  "error": "ACCESS_DENIED",
  "message": "You do not have permission to delete these notifications."
}
```

**Status Code: 404 Not Found**
```json
{
  "error": "NO_NOTIFICATIONS",
  "message": "User not found."
}
```

**Status Code: 500 Internal Server Error**
```json
{
  "error": "SERVER_ERROR",
  "message": "Failed to delete notifications. Please try again later."
}
```

#### Business Logic

1. Validate access token
2. Ensure userId matches authenticated user
3. Query user's notification list
4. Delete all notification records
5. Return 204 No Content with message

---

### 6. Delete Specific Notification

Delete a single notification by its ID.

#### HTTP Request

```
DELETE /api/notifications/{notifId}
```

#### Permissions

- Must be authenticated
- Only user owners can delete their notifications
- Admin may delete for moderation or account reset (optional logic)

#### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| notifId | string | Yes | Unique notification ID |

#### Request Headers

| Header | Required | Description |
|--------|----------|-------------|
| Authorization | Yes | Bearer token for authentication |
| Accept | No | Expected response format (application/json) |
| Content-Type | Yes | Must be application/json |

#### Request Body

None

#### Response Body (Success)

**Status Code: 204 No Content**

```json
{
  "userId": "user123",
  "notifId": "notif456",
  "message": "Notification successfully deleted."
}
```

#### Error Responses

**Status Code: 401 Unauthorized**
```json
{
  "error": "UNAUTHORIZED",
  "message": "User is not authenticated"
}
```

**Status Code: 403 Forbidden**
```json
{
  "error": "ACCESS_DENIED",
  "message": "You do not have permission to delete this notification."
}
```

**Status Code: 404 Not Found**
```json
{
  "error": "NO_NOTIFICATIONS",
  "message": "Notification does not exist."
}
```

**Status Code: 500 Internal Server Error**
```json
{
  "error": "SERVER_ERROR",
  "message": "Failed to delete notification. Please try again later."
}
```

#### Business Logic

1. Validate access token
2. Ensure userId matches authenticated user
3. Look up notification by userId and notifId
4. If not found, return 404
5. Delete the record
6. Return 204 No Content with message

---

## Notification Types

The service supports the following notification delivery channels:

| Type | Description |
|------|-------------|
| `in-app` | Notifications displayed within the application |
| `sms` | Short Message Service (SMS) notifications |
| `email` | Email notifications |

## Notification Status

| Status | Description |
|--------|-------------|
| `unread` | Notification has been created but not yet read by the user |
| `read` | Notification has been marked as read by the user |

## Data Models

### Notification Object

```json
{
  "id": "string (UUID)",
  "userId": "string (UUID)",
  "type": "enum (in-app, sms, email)",
  "title": "string",
  "message": "string",
  "status": "enum (unread, read)",
  "extraData": "object (optional)",
  "createdAt": "ISO 8601 DateTime",
  "updatedAt": "ISO 8601 DateTime (optional)"
}
```

## Database Schema

### Required Fields

The Notification Service requires the following fields in the database:

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Id` | UUID/NVARCHAR(36) | Yes | Unique identifier for the notification |
| `UserId` | UUID/NVARCHAR(36) | Yes | ID of the user who owns the notification (Foreign Key) |
| `Type` | NVARCHAR(20) | Yes | Delivery channel type: "in-app", "sms", or "email" |
| `Title` | NVARCHAR(255) | Yes | Short title of the notification |
| `Message` | NVARCHAR(MAX) | Yes | Detailed notification message |
| `Status` | NVARCHAR(20) | Yes | Current status: "unread" or "read" |
| `CreatedAt` | DATETIME2 | Yes | Timestamp when notification was created (UTC) |
| `UpdatedAt` | DATETIME2 | No | Timestamp when notification was last updated (UTC) |
| `ExtraData` | NVARCHAR(MAX) | No | Additional metadata stored as JSON (e.g., orderId, trackingNumber) |
| `PhoneNumber` | NVARCHAR(20) | Conditional | Phone number for SMS notifications |
| `EmailAddress` | NVARCHAR(255) | Conditional | Email address for email notifications |

### SQL Table Definition

```sql
CREATE TABLE [dbo].[Notifications] (
    [Id] NVARCHAR(36) NOT NULL PRIMARY KEY,
    [UserId] NVARCHAR(36) NOT NULL,
    [Type] NVARCHAR(20) NOT NULL,
    [Title] NVARCHAR(255) NOT NULL,
    [Message] NVARCHAR(MAX) NOT NULL,
    [Status] NVARCHAR(20) NOT NULL,
    [ExtraData] NVARCHAR(MAX) NULL,
    [PhoneNumber] NVARCHAR(20) NULL,
    [EmailAddress] NVARCHAR(255) NULL,
    [CreatedAt] DATETIME2 NOT NULL,
    [UpdatedAt] DATETIME2 NULL,
    
    -- Indexes for performance
    INDEX [IX_UserId] ([UserId]),
    INDEX [IX_CreatedAt] ([CreatedAt]),
    INDEX [IX_Status] ([Status]),
    INDEX [IX_UserId_CreatedAt] ([UserId], [CreatedAt]),
    
    -- Foreign Key constraint
    CONSTRAINT [FK_Notifications_Users] 
        FOREIGN KEY ([UserId]) 
        REFERENCES [dbo].[Users]([Id])
);
```

### Entity Framework Core Model

```csharp
public class Notification
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string Type { get; set; }        // in-app, sms, email
    public string Title { get; set; }
    public string Message { get; set; }
    public string Status { get; set; }      // unread, read
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string ExtraData { get; set; }   // JSON stored as string
    public string PhoneNumber { get; set; }
    public string EmailAddress { get; set; }
    
    // Navigation property
    public virtual User User { get; set; }
}
```

### Database Configuration

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Notification>(entity =>
    {
        entity.HasKey(e => e.Id);
        
        entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
        entity.Property(e => e.UserId).HasMaxLength(36).IsRequired();
        entity.Property(e => e.Type).HasMaxLength(20).IsRequired();
        entity.Property(e => e.Title).HasMaxLength(255).IsRequired();
        entity.Property(e => e.Message).HasColumnType("nvarchar(max)").IsRequired();
        entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
        entity.Property(e => e.CreatedAt).IsRequired();
        entity.Property(e => e.UpdatedAt);
        entity.Property(e => e.ExtraData).HasColumnType("nvarchar(max)");
        entity.Property(e => e.PhoneNumber).HasMaxLength(20);
        entity.Property(e => e.EmailAddress).HasMaxLength(255);
        
        // Indexes
        entity.HasIndex(e => e.UserId).HasName("IX_UserId");
        entity.HasIndex(e => e.CreatedAt).HasName("IX_CreatedAt");
        entity.HasIndex(e => e.Status).HasName("IX_Status");
        entity.HasIndex(e => new { e.UserId, e.CreatedAt }).HasName("IX_UserId_CreatedAt");
        
        // Foreign Key
        entity.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    });
}
```

### Sample Data

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "userId": "660e8400-e29b-41d4-a716-446655440111",
  "type": "in-app",
  "title": "Order Delivered",
  "message": "Your order #1234 has been delivered successfully.",
  "status": "unread",
  "createdAt": "2025-12-05T10:00:00Z",
  "updatedAt": null,
  "extraData": "{\"orderId\": \"1234\", \"trackingNumber\": \"TRACK12345\", \"deliveryTime\": \"10:30 AM\"}",
  "phoneNumber": null,
  "emailAddress": null
}
```

### Database Performance Recommendations

1. **Indexing Strategy:**
   - Create composite index on `(UserId, CreatedAt)` for efficient user notification retrieval
   - Index on `Status` for filtering unread notifications
   - Index on `CreatedAt` for sorting and archival queries

2. **Partitioning (for large scale):**
   - Consider partitioning by `CreatedAt` if table grows beyond millions of records
   - Implement quarterly or monthly partitions for easier maintenance

3. **Archival Policy:**
   - Archive notifications older than 6-12 months to separate historical tables
   - Implement soft delete with `IsDeleted` flag if audit trails are needed

4. **Data Retention:**
   - Implement automatic cleanup jobs to remove notifications older than retention period
   - Define retention policy based on business requirements (typically 3-12 months)

5. **Connection Pooling:**
   - Set appropriate connection pool size in `appsettings.json`
   - Recommended: 20-40 connections for production

### Extra Data Examples

The `ExtraData` field stores JSON data for various notification types:

```json
// Order Notification
{
  "orderId": "ORD12345",
  "restaurantName": "Pizza Palace",
  "totalAmount": 25.99,
  "estimatedDeliveryTime": "30-45 minutes"
}

// Delivery Notification
{
  "deliveryPersonName": "John Doe",
  "deliveryPersonPhone": "+1-555-0100",
  "trackingNumber": "TRACK12345",
  "currentLocation": "5.3521°N 100.0050°E"
}

// Promotional Notification
{
  "campaignId": "CAMP2025",
  "discountPercentage": 20,
  "couponCode": "SAVE20",
  "expiryDate": "2025-12-31T23:59:59Z"
}
```

## Error Handling

The API uses standard HTTP status codes and returns error responses in a consistent format:

```json
{
  "error": "ERROR_CODE",
  "message": "Human-readable error message"
}
```

| Status Code | Meaning |
|-------------|---------|
| 200 | OK - Request successful |
| 201 | Created - Resource created successfully |
| 204 | No Content - Request successful with no content to return |
| 400 | Bad Request - Invalid request parameters |
| 401 | Unauthorized - Authentication required or failed |
| 403 | Forbidden - Authenticated but not authorized |
| 404 | Not Found - Resource not found |
| 500 | Internal Server Error - Server error occurred |

## Best Practices

1. **Always include Authorization header** - All endpoints require a valid Bearer token
2. **Validate response status** - Check HTTP status codes to determine success/failure
3. **Handle errors gracefully** - Implement proper error handling for all error responses
4. **Use pagination** (when available) - For endpoints returning lists of notifications
5. **Mark notifications as read** - Use PATCH endpoint to update notification status
6. **Clean up old notifications** - Regularly delete old notifications to maintain performance

## Support

For issues or questions regarding this service, please contact the development team or create an issue in the repository.

---

**Version:** 1.0  
**Last Updated:** December 10, 2025
