using PaymentService2.Data;

namespace PaymentService2.Services;

/// <summary>
/// Handles database migrations by reading SQL scripts from external files.
/// This keeps SQL completely out of C# code for clean separation of concerns.
/// </summary>
public class MigrationService
{
    private readonly SqlHelper _sql;
    private readonly string _migrationsPath;

    public MigrationService(SqlHelper sql, string? migrationsPath = null)
    {
        _sql = sql;
        // Default to Migrations folder in the output directory
        _migrationsPath = migrationsPath ?? Path.Combine(
            AppContext.BaseDirectory, 
            "Migrations"
        );
    }

    /// <summary>
    /// Run all database migrations from SQL files.
    /// </summary>
    public async Task RunMigrationsAsync()
    {
        Console.WriteLine("Running database migrations from SQL files...");
        Console.WriteLine($"Migrations path: {Path.GetFullPath(_migrationsPath)}");
        
        if (!Directory.Exists(_migrationsPath))
        {
            Console.WriteLine($"⚠ Migrations folder not found: {_migrationsPath}");
            Console.WriteLine("  Falling back to embedded migrations...");
            await RunEmbeddedMigrationsAsync();
            return;
        }

        var sqlFiles = Directory.GetFiles(_migrationsPath, "*.sql")
                                .OrderBy(f => f)
                                .ToList();

        if (sqlFiles.Count == 0)
        {
            Console.WriteLine("⚠ No SQL migration files found.");
            return;
        }

        foreach (var sqlFile in sqlFiles)
        {
            var fileName = Path.GetFileName(sqlFile);
            try
            {
                await ExecuteSqlFileAsync(sqlFile);
                Console.WriteLine($"✓ Applied: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ Warning in {fileName}: {ex.Message}");
            }
        }

        Console.WriteLine("Database migrations completed.");
    }

    private async Task ExecuteSqlFileAsync(string filePath)
    {
        var sqlContent = await File.ReadAllTextAsync(filePath);
        
        // Split by GO statements (SQL Server batch separator)
        var batches = SplitSqlBatches(sqlContent);
        
        foreach (var batch in batches)
        {
            var trimmedBatch = batch.Trim();
            if (!string.IsNullOrEmpty(trimmedBatch))
            {
                await _sql.ExecuteRawSqlAsync(trimmedBatch);
            }
        }
    }

    private static List<string> SplitSqlBatches(string sql)
    {
        // Split on GO statements (case-insensitive, must be on its own line)
        var batches = new List<string>();
        var lines = sql.Split('\n');
        var currentBatch = new List<string>();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.Equals("GO", StringComparison.OrdinalIgnoreCase))
            {
                if (currentBatch.Count > 0)
                {
                    batches.Add(string.Join('\n', currentBatch));
                    currentBatch.Clear();
                }
            }
            else
            {
                currentBatch.Add(line);
            }
        }

        // Don't forget the last batch if no trailing GO
        if (currentBatch.Count > 0)
        {
            batches.Add(string.Join('\n', currentBatch));
        }

        return batches;
    }

    /// <summary>
    /// Fallback embedded migrations if SQL files are not found.
    /// This ensures the app still works even without external SQL files.
    /// </summary>
    private async Task RunEmbeddedMigrationsAsync()
    {
        // Minimal embedded migrations as fallback
        await EnsureTablesExistAsync();
        Console.WriteLine("✓ Embedded fallback migrations applied");
    }

    private async Task EnsureTablesExistAsync()
    {
        // Just ensure critical tables exist - full SPs should come from files
        await _sql.ExecuteRawSqlAsync(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Wallets')
            CREATE TABLE Wallets (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                UserId NVARCHAR(100) NOT NULL UNIQUE,
                Balance DECIMAL(18,2) NOT NULL DEFAULT 0,
                Coins INT NOT NULL DEFAULT 0,
                LastUpdated DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
            );
        ");

        await _sql.ExecuteRawSqlAsync(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Transactions')
            CREATE TABLE Transactions (
                Id NVARCHAR(50) PRIMARY KEY,
                UserId NVARCHAR(100) NOT NULL,
                Type NVARCHAR(50) NOT NULL,
                Amount DECIMAL(18,2) NOT NULL,
                Description NVARCHAR(500),
                ReferenceId NVARCHAR(100),
                CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
            );
        ");

        await _sql.ExecuteRawSqlAsync(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Orders')
            CREATE TABLE Orders (
                Id NVARCHAR(50) PRIMARY KEY,
                UserId NVARCHAR(100) NOT NULL,
                Amount DECIMAL(18,2) NOT NULL,
                Status NVARCHAR(50) NOT NULL DEFAULT 'pending',
                PaymentMethod NVARCHAR(50),
                PaymentStatus NVARCHAR(50) DEFAULT 'pending',
                VoucherCode NVARCHAR(50),
                VoucherDiscount DECIMAL(18,2) DEFAULT 0,
                CoinsUsed INT DEFAULT 0,
                CoinsDiscount DECIMAL(18,2) DEFAULT 0,
                FinalAmount DECIMAL(18,2) NOT NULL,
                Branch NVARCHAR(200),
                PaymentUrl NVARCHAR(MAX),
                PaymentLinkId NVARCHAR(100),
                CheckoutSessionId NVARCHAR(100),
                CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
                CompletedAt DATETIME2
            );
        ");

        await _sql.ExecuteRawSqlAsync(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrderItems')
            CREATE TABLE OrderItems (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                OrderId NVARCHAR(50) NOT NULL,
                Name NVARCHAR(200) NOT NULL,
                Quantity INT NOT NULL,
                Price DECIMAL(18,2) NOT NULL
            );
        ");

        await _sql.ExecuteRawSqlAsync(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TopUps')
            CREATE TABLE TopUps (
                Id NVARCHAR(50) PRIMARY KEY,
                UserId NVARCHAR(100) NOT NULL,
                Amount DECIMAL(18,2) NOT NULL,
                Status NVARCHAR(50) NOT NULL DEFAULT 'pending',
                PaymentMethod NVARCHAR(50),
                PaymentUrl NVARCHAR(MAX),
                CheckoutSessionId NVARCHAR(100),
                CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
                CompletedAt DATETIME2
            );
        ");

        await _sql.ExecuteRawSqlAsync(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Vouchers')
            CREATE TABLE Vouchers (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                Code NVARCHAR(50) NOT NULL UNIQUE,
                Description NVARCHAR(500),
                DiscountType NVARCHAR(20) NOT NULL,
                DiscountValue DECIMAL(18,2) NOT NULL,
                MinOrderAmount DECIMAL(18,2) DEFAULT 0,
                MaxDiscount DECIMAL(18,2),
                UsageLimit INT,
                UsedCount INT DEFAULT 0,
                ExpiresAt DATETIME2,
                IsActive BIT NOT NULL DEFAULT 1,
                CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
            );
        ");

        await _sql.ExecuteRawSqlAsync(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Refunds')
            CREATE TABLE Refunds (
                Id NVARCHAR(50) PRIMARY KEY,
                UserId NVARCHAR(100) NOT NULL,
                OrderId NVARCHAR(50),
                Amount DECIMAL(18,2) NOT NULL,
                Reason NVARCHAR(1000),
                Category NVARCHAR(100),
                Status NVARCHAR(50) NOT NULL DEFAULT 'pending',
                CustomerName NVARCHAR(200),
                CustomerEmail NVARCHAR(200),
                CustomerPhone NVARCHAR(50),
                AdminNotes NVARCHAR(1000),
                RejectionReason NVARCHAR(500),
                ReviewedBy NVARCHAR(100),
                WalletCredited BIT DEFAULT 0,
                CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
                ReviewedAt DATETIME2
            );
        ");
    }
}
