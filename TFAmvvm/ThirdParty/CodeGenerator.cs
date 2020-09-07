using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace TFAmvvm.ThirdParty
{
    public class CodeGenerator
    {
        #region PrivateVars

        private static string ValidChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        private int CodeModulo;
        private MacAlgorithmProvider crypt = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha1);


        #endregion

        #region PublicProperties

        public static int CodeLength { get; set; }
        public static int intervalLength { get; set; }

        #endregion

        #region Constructors

        public CodeGenerator() : this(6, 30)
        {
        }

        public CodeGenerator(int codeLength, int intervalLength)
        {
            CodeLength = codeLength;
            CodeGenerator.intervalLength = intervalLength;
            CodeModulo = (int)Math.Pow(10, CodeLength);
        }

        #endregion

        private int TrailingZeros(int num)
        {
            string binValue = Convert.ToString(num, 2);
            if (Convert.ToInt32(binValue) == 0)
                return 1;
            int i = 0;
            while (Convert.ToInt32(binValue) % 10 == 0)
            {
                int tempNum = Convert.ToInt32(binValue);
                tempNum = tempNum / 10;
                binValue = tempNum.ToString();
                i++;
            }
            return i;
        }

        private byte[] FromBase32String(string encoded)
        {
            // Remove whitespace, separators and capitalize all characters
            encoded = encoded.Trim().Replace(" ", "").Replace("-", "").ToUpper();
            char[] DIGITS = ValidChars.ToCharArray();
            int MASK = DIGITS.Length - 1;
            int SHIFT = TrailingZeros(DIGITS.Length);

            if (encoded.Length == 0)
            {
                return new byte[0];
            }
            int encodedLength = encoded.Length;
            int outLength = encodedLength * SHIFT / 8;
            byte[] result = new byte[outLength];
            int buffer = 0;
            int next = 0;
            int bitsLeft = 0;
            Dictionary<char, int> CHAR_MAP = new Dictionary<char, int>();
            for (int i = 0; i < DIGITS.Length; i++)
            {
                CHAR_MAP.Add(DIGITS[i], i);
            }

            foreach (char c in encoded.ToCharArray())
            {

                buffer <<= SHIFT;
                buffer |= CHAR_MAP[c] & MASK;
                bitsLeft += SHIFT;
                if (bitsLeft >= 8)
                {
                    result[next++] = (byte)(buffer >> (bitsLeft - 8));
                    bitsLeft -= 8;
                }
            }
            return result;
        }

        private String PadOutput(int value)
        {
            String result = value.ToString();
            for (int i = result.Length; i < CodeLength; i++)
            {
                result = "0" + result;
            }
            return result;
        }

        private async Task<long> GetCurrentInterval()
        {
            DateTime NtpUtc = await GetUtc();
            TimeSpan TS = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long currentTimeSeconds = (long)Math.Floor(TS.TotalSeconds);
            long currentInterval = currentTimeSeconds / intervalLength;
            return currentInterval;
        }

        public string ComputeCode(string secretKey)
        {
            return Task.Run(async () => await ComputeCodeAsync(secretKey)).Result;
        }

        public async Task<string> ComputeCodeAsync(string secretKey)
        {
            int CodeValue = 0;
            try
            {
                byte[] keyBytes = FromBase32String(secretKey);
                IBuffer buffKey = CryptographicBuffer.CreateFromByteArray(keyBytes);
                CryptographicKey key = crypt.CreateKey(buffKey);

                long counter = await GetCurrentInterval();
                byte[] counterBytes = BitConverter.GetBytes(counter);
                Array.Reverse(counterBytes);
                IBuffer buffCounter = CryptographicBuffer.CreateFromByteArray(counterBytes);

                IBuffer buffHash = CryptographicEngine.Sign(key, buffCounter);
                byte[] hash = new byte[buffHash.Length];
                CryptographicBuffer.CopyToByteArray(buffHash, out hash);
                int offset = hash[hash.Length - 1] & 0xF;

                int binary = ((hash[offset] & 0x7f) << 24)
                    | ((hash[offset + 1] & 0xff) << 16)
                    | ((hash[offset + 2] & 0xff) << 8)
                    | (hash[offset + 3] & 0xff);

                CodeValue = binary % CodeModulo;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return "Invalid Key";
            }

            return PadOutput(CodeValue);
        }

        public async Task<int> NumberSecondsLeft()
        {
            DateTime NtpUtcTime = await GetUtc();
            TimeSpan TS = NtpUtcTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long currentTimeSeconds = (long)Math.Floor(TS.TotalSeconds);
            int secondsElapsed = (int)currentTimeSeconds % intervalLength;
            int secondsLeft = intervalLength - secondsElapsed;
            return secondsLeft;
        }

        private async Task<DateTime> GetUtc()
        {
            return await new Ntp().GetNetworkTimeAsync();
        }
    }
}
