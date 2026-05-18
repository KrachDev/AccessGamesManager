using System;
using System.Security.Cryptography;

namespace AccessGamesManager.Misc
{
    public static class TotpUtility
    {
        public static string GenerateTotp(string base32Secret)
        {
            byte[] key = Base32Decode(base32Secret);
            if (key.Length == 0) throw new ArgumentException("Invalid secret key.");
            
            long unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long counter = unixTimestamp / 30;

            byte[] counterBytes = BitConverter.GetBytes(counter);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(counterBytes);

            using (var hmac = new HMACSHA1(key))
            {
                byte[] hash = hmac.ComputeHash(counterBytes);
                int offset = hash[hash.Length - 1] & 0x0F;
                int binaryCode = ((hash[offset] & 0x7F) << 24)
                               | ((hash[offset + 1] & 0xFF) << 16)
                               | ((hash[offset + 2] & 0xFF) << 8)
                               | (hash[offset + 3] & 0xFF);

                int otp = binaryCode % 1000000;
                return otp.ToString("D6");
            }
        }

        private static byte[] Base32Decode(string base32)
        {
            base32 = base32.Trim().ToUpper().Replace(" ", "").Replace("-", "");
            if (string.IsNullOrEmpty(base32)) return Array.Empty<byte>();

            // Valid base32 padding removal
            base32 = base32.TrimEnd('=');
            
            int byteCount = base32.Length * 5 / 8;
            byte[] returnArray = new byte[byteCount];

            byte curByte = 0, bitsRemaining = 8;
            int arrayIndex = 0;

            foreach (char c in base32)
            {
                int cValue = CharToValue(c);
                if (cValue < 0) continue; // skip invalid chars
                
                if (bitsRemaining > 5)
                {
                    int mask = cValue << (bitsRemaining - 5);
                    curByte = (byte)(curByte | mask);
                    bitsRemaining -= 5;
                }
                else
                {
                    int mask = cValue >> (5 - bitsRemaining);
                    curByte = (byte)(curByte | mask);
                    if (arrayIndex < returnArray.Length)
                        returnArray[arrayIndex++] = curByte;
                    curByte = (byte)(cValue << (3 + bitsRemaining));
                    bitsRemaining += 3;
                }
            }

            return returnArray;
        }

        private static int CharToValue(char c)
        {
            if (c >= 'A' && c <= 'Z') return c - 'A';
            if (c >= '2' && c <= '7') return c - '2' + 26;
            return -1;
        }
    }
}
