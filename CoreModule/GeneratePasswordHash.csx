#!/usr/bin/env dotnet script

#r "nuget: Microsoft.AspNetCore.Identity, 2.2.0"

using System;
using Microsoft.AspNetCore.Identity;

// Create password hasher
var hasher = new PasswordHasher<object>();

// Generate hash for Admin@123
var password = "Admin@123";
var hash = hasher.HashPassword(null, password);

Console.WriteLine("=".PadRight(80, '='));
Console.WriteLine("PASSWORD HASH GENERATOR");
Console.WriteLine("=".PadRight(80, '='));
Console.WriteLine();
Console.WriteLine($"Password: {password}");
Console.WriteLine();
Console.WriteLine($"Generated Hash:");
Console.WriteLine(hash);
Console.WriteLine();
Console.WriteLine($"Hash Length: {hash.Length} characters");
Console.WriteLine();

// Verify it works
var result = hasher.VerifyHashedPassword(null, hash, password);
Console.WriteLine($"Verification Test: {result}");
Console.WriteLine();

if (result == PasswordVerificationResult.Success)
{
    Console.WriteLine("✅ Hash is VALID and ready to use!");
}
else
{
    Console.WriteLine("❌ Hash verification failed!");
}

Console.WriteLine();
Console.WriteLine("=".PadRight(80, '='));
Console.WriteLine("COPY THE SQL BELOW AND RUN IT IN YOUR DATABASE:");
Console.WriteLine("=".PadRight(80, '='));
Console.WriteLine();
Console.WriteLine("UPDATE core.users");
Console.WriteLine("SET hashed_password = '" + hash + "',");
Console.WriteLine("    updated_at_utc = NOW()");
Console.WriteLine("WHERE email = 'admin@encryptz.com';");
Console.WriteLine();
Console.WriteLine("=".PadRight(80, '='));

