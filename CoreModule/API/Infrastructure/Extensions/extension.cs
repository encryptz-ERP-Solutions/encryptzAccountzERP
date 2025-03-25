using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;

namespace Infrastructure.Extensions
{
    public static class extension
    {
        // Generic method to convert one object to another
        public static TTarget ConvertToClassObject<TSource, TTarget>(this TSource source)
            where TSource : class
            where TTarget : class, new()
        {
            if (source == null)
            {
                return null;
            }

            var target = new TTarget();

            // For simplicity, using reflection to copy properties with the same name and type
            foreach (var sourceProp in typeof(TSource).GetProperties())
            {
                var targetProp = typeof(TTarget).GetProperty(sourceProp.Name);
                if (targetProp != null && targetProp.CanWrite && targetProp.PropertyType == sourceProp.PropertyType)
                {
                    targetProp.SetValue(target, sourceProp.GetValue(source));
                }
            }

            return target;
        }

        /// Performs encryption or decryption using a crypto transform.        
        private static byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
        {
            using (var ms = new MemoryStream())
            using (var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
            {
                cryptoStream.Write(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();
                return ms.ToArray();
            }
        }


        private static readonly string EncryptionKey = "223encryptzSecretCodeForDreamProject445"; // 16, 24, or 32 characters for AES key        
        /// Encrypts a string using AES encryption.        
        public static string Encrypt(this string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            byte[] keyBytes = Encoding.UTF8.GetBytes(EncryptionKey);
            byte[] ivBytes = new byte[16]; // AES requires a 16-byte IV, initialized as zeros

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = ivBytes;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] encryptedBytes = PerformCryptography(plainBytes, encryptor);
                    return Convert.ToBase64String(encryptedBytes);
                }
            }
        }

        
        /// Decrypts an AES-encrypted string.        
        public static string Decrypt(this string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;

            byte[] keyBytes = Encoding.UTF8.GetBytes(EncryptionKey);
            byte[] ivBytes = new byte[16]; // AES requires a 16-byte IV, initialized as zeros

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = ivBytes;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                    byte[] decryptedBytes = PerformCryptography(encryptedBytes, decryptor);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }

        /// Validates whether a given string is in a proper email format.        
        public static bool IsValidEmail(this string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern, RegexOptions.IgnoreCase);
        }

        /// Converts a DataRow to an object of a specified class.
        public static T ToObjectFromDR<T>(this DataRow row) where T : new()
        {
            if (row == null)
                throw new ArgumentNullException(nameof(row));

            T obj = new T();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                if (row.Table.Columns.Contains(prop.Name) && row[prop.Name] != DBNull.Value)
                {
                    try
                    {
                        prop.SetValue(obj, Convert.ChangeType(row[prop.Name], prop.PropertyType));
                    }
                    catch
                    {
                        throw;
                        // Handle type conversion errors if needed
                    }
                }
            }

            return obj;
        }

        /// Converts a DataTable to a list of objects of a specified class.
        public static List<T> ToList<T>(this DataTable table) where T : new()
        {
            List<T> list = new List<T>();
            if (table == null || table.Rows.Count == 0)
                return list;

            // Get all properties of the target class
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (DataRow row in table.Rows)
            {
                T obj = new T();
                foreach (var prop in properties)
                {
                    if (table.Columns.Contains(prop.Name) && row[prop.Name] != DBNull.Value)
                    {
                        try
                        {
                            prop.SetValue(obj, Convert.ChangeType(row[prop.Name], prop.PropertyType));
                        }
                        catch
                        {
                            throw;
                            // Handle type conversion errors if needed
                        }
                    }
                }
                list.Add(obj);
            }
            return list;
        }
    }

}
