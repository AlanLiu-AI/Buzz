using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Runner.Base.Util
{
    public static class HashUtil
    {

        public static string GetMd5Hash_UTF8(string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            return GetMd5Hash(inputBytes);
        }

        public static string GetMd5Hash(string input)
        {
            byte[] inputBytes = Encoding.Default.GetBytes(input);
            return GetMd5Hash(inputBytes);
        }

        public static string GetMd5Hash(string input, string charsetName)
        {
            Encoding customEncoding = null;
            if (charsetName != null && charsetName.Length > 0)
            {
                customEncoding = System.Text.Encoding.GetEncoding(charsetName);
            }
            byte[] inputBytes = customEncoding.GetBytes(input);
            return GetMd5Hash(inputBytes);
        }

        public static string GetMd5Hash(byte[] input)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5 md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(input);

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();

        }

        public static string GetMd5Sum(string fileName)
        {
            using (FileStream fs = File.OpenRead(fileName))
            {
                MD5 m5 = MD5.Create();
                byte[] hash = m5.ComputeHash(fs);
                string base64Hash = Convert.ToBase64String(hash);
                return base64Hash;
            }
            throw new Exception("Checksum using md5 hash failed, input file is: " + fileName);
        }

        public static bool VerifyMd5Sum(string file1, string file2)
        {
            // Hash the op1.
            string hashOfOp1 = GetMd5Sum(file1);

            // Hash the op2.
            string hashOfOp2 = GetMd5Sum(file2);

            // Create a StringComparer an comare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfOp1, hashOfOp2))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Verify a hash against a string.
        public static bool VerifyMd5Hash(string input, string hash)
        {
            // Hash the input.
            string hashOfInput = GetMd5Hash(input);

            // Create a StringComparer an comare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Verify a hash against a string.
        public static bool VerifyMd5Hash(byte[] op1, byte[] op2)
        {
            // Hash the op1.
            string hashOfOp1 = GetMd5Hash(op1);

            // Hash the op2.
            string hashOfOp2 = GetMd5Hash(op2);

            // Create a StringComparer an comare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfOp1, hashOfOp2))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Verify a hash against a string.
        public static bool VerifyMd5Hash_UTF8(string op1, string op2)
        {
            // Hash the op1.
            string hashOfOp1 = GetMd5Hash_UTF8(op1);

            // Hash the op2.
            string hashOfOp2 = GetMd5Hash_UTF8(op2);

            // Create a StringComparer an comare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfOp1, hashOfOp2))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }

}