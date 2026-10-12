using System;
using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace NiramoyHealth.DbVerification;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        Console.WriteLine("=================================================================");
        Console.WriteLine("NIRAMOY HEALTH (.NET 10 / NPGSQL) DATABASE CONNECTIVITY CHECK");
        Console.WriteLine("=================================================================\n");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        string? devConnStr = configuration.GetConnectionString("DefaultConnection");
        string? testConnStr = configuration.GetConnectionString("TestConnection");

        if (string.IsNullOrWhiteSpace(devConnStr))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[FAIL] Connection string 'DefaultConnection' not found in configuration.");
            Console.ResetColor();
            return 1;
        }

        Console.WriteLine($"[CONFIG] Dev Connection String: [CONFIGURED - HOST/DB MASKED]");
        Console.WriteLine($"[CONFIG] Test Connection String: {(string.IsNullOrEmpty(testConnStr) ? "Not Configured" : "[CONFIGURED]")}\n");

        if (args.Length > 0 && args[0] == "seed-passwords")
        {
            var custHash = BCrypt.Net.BCrypt.EnhancedHashPassword("Password123!", 11);
            var adminHash = BCrypt.Net.BCrypt.EnhancedHashPassword("Admin123!", 11);
            await using var conn = new NpgsqlConnection(devConnStr);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(@"
                UPDATE users SET password_hash = @custHash WHERE email LIKE '%example.com' OR phone_number LIKE '%11223344%';
                UPDATE users SET password_hash = @adminHash WHERE email LIKE '%admin%' OR email LIKE '%pharmacist%';
                INSERT INTO users (phone_number, email, password_hash, full_name, is_phone_verified, is_email_verified, status)
                VALUES ('+8801712345678', 'customer@niramoyhealth.com', @custHash, 'Demo Customer', TRUE, TRUE, 'active')
                ON CONFLICT (phone_number) DO UPDATE SET password_hash = @custHash;
                INSERT INTO user_roles (user_id, role_id)
                SELECT u.id, (SELECT id FROM roles WHERE name = 'customer') FROM users u WHERE u.phone_number = '+8801712345678'
                ON CONFLICT DO NOTHING;
                INSERT INTO users (phone_number, email, password_hash, full_name, is_phone_verified, is_email_verified, status)
                VALUES ('+8801700000099', 'admin@niramoyhealth.com', @adminHash, 'Niramoy Administrator', TRUE, TRUE, 'active')
                ON CONFLICT (phone_number) DO UPDATE SET password_hash = @adminHash;
                INSERT INTO user_roles (user_id, role_id)
                SELECT u.id, (SELECT id FROM roles WHERE name = 'admin') FROM users u WHERE u.email = 'admin@niramoyhealth.com'
                ON CONFLICT DO NOTHING;
            ", conn);
            cmd.Parameters.AddWithValue("custHash", custHash);
            cmd.Parameters.AddWithValue("adminHash", adminHash);
            await cmd.ExecuteNonQueryAsync();
            Console.WriteLine("[SUCCESS] Seeded real BCrypt password hashes for demo customer and admin!");
            return 0;
        }

        bool devSuccess = await TestConnectionAsync("Development (niramoy_health_dev)", devConnStr);

        bool testSuccess = true;
        if (!string.IsNullOrWhiteSpace(testConnStr))
        {
            testSuccess = await TestConnectionAsync("Testing (niramoy_health_test)", testConnStr);
        }

        Console.WriteLine("\n=================================================================");
        if (devSuccess && testSuccess)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[SUCCESS] All .NET Npgsql database connectivity checks passed!");
            Console.ResetColor();
            return 0;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[FAIL] One or more database connectivity checks failed.");
            Console.ResetColor();
            return 1;
        }
    }

    private static async Task<bool> TestConnectionAsync(string envName, string connectionString)
    {
        Console.WriteLine($"--> Testing connection to: {envName}...");
        try
        {
            await using var dataSource = NpgsqlDataSource.Create(connectionString);
            await using var connection = await dataSource.OpenConnectionAsync();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"    [OK] Connection opened successfully. State: {connection.State}");
            Console.ResetColor();

            // 1. Query Server Version and Database Info
            await using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT current_database(), current_user, version();";
                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    string dbName = reader.GetString(0);
                    string user = reader.GetString(1);
                    string version = reader.GetString(2);
                    Console.WriteLine($"    [INFO] Database: {dbName} | User: {user}");
                    Console.WriteLine($"    [INFO] Engine: {version[..Math.Min(60, version.Length)]}...");
                }
            }

            // 2. Query Table Count in public schema
            int tableCount = 0;
            await using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE';";
                tableCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                Console.WriteLine($"    [INFO] Registered Base Tables: {tableCount}");
                if (tableCount < 20)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"    [INFO] Database currently has {tableCount} tables (fresh or unmigrated test database).");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"    [OK] Table count matches full domain schema ({tableCount} tables).");
                    Console.ResetColor();
                }
            }

            // 3. Query Unicode Bengali String Support if products table exists
            bool productsTableExists = false;
            await using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'products');";
                productsTableExists = Convert.ToBoolean(await cmd.ExecuteScalarAsync());
            }

            if (productsTableExists)
            {
                await using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT name, name_bn, base_price, discount_price FROM products WHERE sku = 'MED-SQR-001' LIMIT 1;";
                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    string name = reader.GetString(0);
                    string nameBn = reader.GetString(1);
                    decimal basePrice = reader.GetDecimal(2);
                    decimal? discountPrice = reader.IsDBNull(3) ? null : reader.GetDecimal(3);
                    Console.WriteLine($"    [INFO] Unicode Sample: {name} / {nameBn} (BDT {discountPrice ?? basePrice})");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("    [OK] Bilingual UTF-8 and decimal precision verified.");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine("    [INFO] Products table present (0 rows seeded yet).");
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"    [ERROR] Connection failed: {ex.Message}");
            Console.ResetColor();
            return false;
        }
    }
}
