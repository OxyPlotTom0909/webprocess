using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WebProcess.Helper
{
    public class AESCryptography
    {
        //預設金鑰向量, ASCII -> MyFirstProject
        private static byte[] _key = { 0x02, 0x4D, 0x79, 0x46, 0x69, 0x72, 0x73, 0x74, 0x50, 0x72, 0x6F, 0x6A, 0x65, 0x63, 0x74, 0x03 };


        /// <summary>
        /// AES加密演算法
        /// </summary>
        /// <param name="plainText">明文字串</param>
        /// <param name="strKey">金鑰</param>
        /// <returns>返回加密後的密文位元組陣列</returns>
        public static byte[] AESEncrypt(string plainText, string strKey)
        {
            //分組加密演算法
            SymmetricAlgorithm des = Rijndael.Create();
            byte[] inputByteArray = Encoding.UTF8.GetBytes(plainText);//得到需要加密的位元組陣列
                                                                      //設定金鑰及金鑰向量
            des.Key = Encoding.UTF8.GetBytes(strKey);
            des.IV = _key;
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            byte[] cipherBytes = ms.ToArray();//得到加密後的位元組陣列
            cs.Close();
            ms.Close();
            return cipherBytes;
        }

        /// <summary>
        /// AES加密演算法，使用字串，String in, String out
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="strKey"></param>
        /// <returns>經過16進制編碼過的小寫字串</returns>
        public static string AESEncryptString(string plainText, string strKey)
        {
            string sResult = null;
            StringBuilder sb = new StringBuilder();

            try
            {
                var aesEncrypt = AESEncrypt(plainText, strKey);

                foreach (byte b in aesEncrypt)
                    sb.AppendFormat("{0:x2}", b);

                sResult = sb.ToString();
            }
            catch(Exception ex)
            {
                sResult = $"Encrypt error. {ex.ToString()}";
            }

            return sResult;
        }

        /// <summary>
        /// AES解密16進制的小寫字串
        /// </summary>
        /// <param name="decryptText">待解密字串</param>
        /// <param name="strKey">金鑰</param>
        /// <returns>返回解密後的字串</returns>
        public static string AESDecrypt(string decryptText, string strKey)
        {
            string sResult = null;

            try
            {
                //Recovery Hex to Byte
                byte[] byteContent = new byte[decryptText.Length / 2];
                for (int i = 0; i < (decryptText.Length / 2); i++)
                {
                    int code = Convert.ToInt32(decryptText.Substring(i * 2, 2), 16);
                    byteContent[i] = (byte)code;
                }

                var result = AESDecrypt(byteContent, strKey);

                sResult = Encoding.UTF8.GetString(result).Replace("\0", string.Empty);
            }
            catch (Exception ex)
            {
                sResult = $"Decrypt Error. {ex.ToString()}";
            }

            return sResult;
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="cipherText">密文位元組陣列</param>
        /// <param name="strKey">金鑰</param>
        /// <returns>返回解密後的字串</returns>
        public static byte[] AESDecrypt(byte[] cipherText, string strKey)
        {
            SymmetricAlgorithm des = Rijndael.Create();
            des.Key = Encoding.UTF8.GetBytes(strKey);
            des.IV = _key;
            byte[] decryptBytes = new byte[cipherText.Length];
            MemoryStream ms = new MemoryStream(cipherText);
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read);
            cs.Read(decryptBytes, 0, decryptBytes.Length);
            cs.Close();
            ms.Close();
            return decryptBytes;
        }

        public static string AESCodeToNumberString(byte[] aesCode)
        {
            var strEncrypt = Convert.ToBase64String(aesCode, Base64FormattingOptions.None);

            var numberString = Encoding.UTF8.GetString(aesCode);

            return strEncrypt;
        }

        /// <summary>
        /// 將AES編碼字串重新分段組合
        /// </summary>
        /// <param name="coding">加密後字串</param>
        /// <param name="suffix">加入後綴</param>
        /// <returns>重新分段組合後的字串</returns>
        public static string AESCodingStringReorganization(string coding, string suffix = null, int insertPosition = 10)
        {
            string newStrIdEncrypt;

            var front = coding.Substring(coding.Length / 2, coding.Length / 2);
            var back = coding.Substring(0, coding.Length / 2);
            newStrIdEncrypt =  front + back;

            if (suffix != null)
            {
                var headString = newStrIdEncrypt.Substring(0, newStrIdEncrypt.Length - insertPosition);
                var lastString = newStrIdEncrypt.Substring(newStrIdEncrypt.Length - insertPosition);
                newStrIdEncrypt = headString + suffix + lastString;
            }

            return newStrIdEncrypt;
        }

        /// <summary>
        /// 將AES重編後字串回復原本內容
        /// </summary>
        /// <param name="reorganizationCoding">AES重編後字串</param>
        /// <param name="suffixNumber">後綴數量</param>
        /// <returns>去掉後綴後的重新組合回原本編碼的字串</returns>
        public static string[] AESCodingStringRecovery(string reorganizationCoding, int suffixNumber = 16, int insertPosition = 10)
        {
            var suffix = reorganizationCoding.Substring((reorganizationCoding.Length - (insertPosition + suffixNumber)), suffixNumber);
            var headString = reorganizationCoding.Substring(0, reorganizationCoding.Length - (insertPosition + suffixNumber));
            var lastString = reorganizationCoding.Substring(reorganizationCoding.Length - insertPosition);
            var coding = headString + lastString;
            var front = coding.Substring(0, coding.Length / 2);
            var back = coding.Substring(coding.Length / 2, coding.Length / 2);

            string[] recoveryEncrypt = new string[]
                {
                    back + front,
                    suffix
                };
            
            return recoveryEncrypt;
        }

        /// <summary>
        /// 返回一組隨機key
        /// </summary>
        /// <param name=""></param>
        /// <returns>隨機字元key</returns>
        public static string GetRandomString(int keyCount, bool characterUpper = true)
        {
            var rand = new Random();

            string code = "";

            var max = 2;

            if (characterUpper)
                max = 3;

            for (int i = 0; i < keyCount; ++i)
            {
                switch (rand.Next(0, max))
                {
                    case 0:
                        code += rand.Next(0, 10);
                        break;
                    case 1:
                        code += (char)rand.Next(97, 122);
                        break;
                    case 2:
                        code += (char)rand.Next(65, 91);
                        break;
                }
            }

            return code;
        }
    }
}
