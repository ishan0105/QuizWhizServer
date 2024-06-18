using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace QuizWhiz.Domain.Helpers
{
    public class HashingHelper
    {
        
        public string HashPassword(string password)
        {
           
            byte[] salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            
            byte[] subkey = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 20);
           
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(subkey, 0, hashBytes, 16, 20);
        
            return Convert.ToBase64String(hashBytes);
        }

        public bool VerifyPassword(string inputPassword, string hashedPassword)
        {
            
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);
           
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            
            byte[] storedSubkey = new byte[20];
            Array.Copy(hashBytes, 16, storedSubkey, 0, 20);
           
            byte[] subkey = KeyDerivation.Pbkdf2(
                password: inputPassword,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 20);
           
            return storedSubkey.SequenceEqual(subkey);
        }
    }
}
