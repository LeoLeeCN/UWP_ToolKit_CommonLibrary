using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace CommonLibrary
{
    public class SecurityHelper
    {
        public static string MD5(string input)
        {
            IBuffer bufInput = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);
            HashAlgorithmProvider algo = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            IBuffer bufOutput = algo.HashData(bufInput);
            if (bufOutput.Length != algo.HashLength)
            {
                return string.Empty;
            }
            string output = CryptographicBuffer.EncodeToHexString(bufOutput).ToUpper();
            return output;
        }

        public static string UrlEncoder(string input)
        {
            return System.Net.WebUtility.UrlEncode(input);
        }

        public static string UrlDecode(string input)
        {
            return System.Net.WebUtility.UrlDecode(input);
        }

        public static string AESEncrypt(string key, string value)
        {
            try
            {
                IBuffer bufMsg = CryptographicBuffer.ConvertStringToBinary(value, BinaryStringEncoding.Utf8);
                IBuffer bufKey = CryptographicBuffer.ConvertStringToBinary(key, BinaryStringEncoding.Utf8);
                string strAlgName = Windows.Security.Cryptography.Core.SymmetricAlgorithmNames.AesCbcPkcs7;
                SymmetricKeyAlgorithmProvider alg = SymmetricKeyAlgorithmProvider.OpenAlgorithm(strAlgName);
                CryptographicKey encryptKey = alg.CreateSymmetricKey(bufKey);
                IBuffer bufOutput = CryptographicEngine.Encrypt(encryptKey, bufMsg, bufKey);
                string output = CryptographicBuffer.EncodeToBase64String(bufOutput);
                return output;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string AESDecrypt(string key, string value)
        {
            try
            {
                IBuffer bufMsg = CryptographicBuffer.DecodeFromBase64String(value);
                IBuffer bufKey = CryptographicBuffer.ConvertStringToBinary(key, BinaryStringEncoding.Utf8);
                string strAlgName = Windows.Security.Cryptography.Core.SymmetricAlgorithmNames.AesCbcPkcs7;
                SymmetricKeyAlgorithmProvider alg = SymmetricKeyAlgorithmProvider.OpenAlgorithm(strAlgName);
                CryptographicKey encryptKey = alg.CreateSymmetricKey(bufKey);
                IBuffer bufOutput = CryptographicEngine.Decrypt(encryptKey, bufMsg, bufKey);
                string output = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, bufOutput);
                return output;
            }
            catch
            {
                return string.Empty;
            }
        }

    }

}
