// Quick tool to generate BCrypt password hash
// Run with: dotnet run --project EngAce.Api GeneratePasswordHash.cs

using System;

class GeneratePasswordHash
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== BCrypt Password Hash Generator ===\n");
        
        // Test passwords
        string[] passwords = { "Hoangtai@2", "Test123!@#", "Admin123!" };
        
        foreach (var password in passwords)
        {
            string hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
            
            Console.WriteLine($"Password: {password}");
            Console.WriteLine($"BCrypt Hash: {hash}");
            Console.WriteLine($"Hash Length: {hash.Length}");
            Console.WriteLine($"\nSQL Update Command:");
            Console.WriteLine($"UPDATE users SET password_hash = '{hash}' WHERE username = 'hoangtai';");
            Console.WriteLine("\n" + new string('-', 80) + "\n");
        }
        
        // Verify test
        Console.WriteLine("=== Verification Test ===");
        string testPassword = "Hoangtai@2";
        string testHash = BCrypt.Net.BCrypt.HashPassword(testPassword, workFactor: 12);
        bool isValid = BCrypt.Net.BCrypt.Verify(testPassword, testHash);
        Console.WriteLine($"Password: {testPassword}");
        Console.WriteLine($"Hash: {testHash}");
        Console.WriteLine($"Verification: {(isValid ? "✓ PASS" : "✗ FAIL")}");
    }
}
