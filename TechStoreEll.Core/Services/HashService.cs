using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace TechStoreEll.Core.Services;

public class HashService
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int DefaultIterations = 100000;

    public static string HashPassword(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException(null, nameof(input));

        byte[] salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        byte[] hash = KeyDerivation.Pbkdf2(
            password: input,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: DefaultIterations,
            numBytesRequested: HashSize);

        byte[] hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        return Convert.ToBase64String(hashBytes);
    }

    public static bool VerifyHash(string input, string hashedOutput)
    {
        if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(hashedOutput))
            return false;

        try
        {
            byte[] hashBytes = Convert.FromBase64String(hashedOutput);
            if (hashBytes.Length != SaltSize + HashSize)
                return false;

            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            byte[] storedHash = new byte[HashSize];
            Array.Copy(hashBytes, SaltSize, storedHash, 0, HashSize);

            byte[] computedHash = KeyDerivation.Pbkdf2(
                password: input,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: DefaultIterations,
                numBytesRequested: HashSize);

            return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}