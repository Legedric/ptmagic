using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Specialized;
using System.Configuration;

namespace Core.Helper {
  public class EncryptionHelper {

    #region Properties

    public static string CryptoMainSaltValue {
      get {
        return "b3+Pz.~L<R 8NH-p=Ze<smbpb*]dP,%d9d{P{DC)R$xf]s|6UC-d)X[y_kDR^EsL";
      }
    }

    public static string CryptoSaltValue {
      get {
        return "/-T:_~Z|j~0%@~|?7,L~]:us9-=VO[.0V[nZDYTjnUeHcka#hdQ{U^YHv:0sJlfk";
      }
    }
    public static string CryptoInitVector {
      get {
        return "qWEE:ADg)}6b;V{B";
      }
    }
    public static string CryptoPassPhrase {
      get {
        return "KUBD`o.]*#CCL n9m}tZN4B4~>2EK>((/xnTbWdTo:/5_$hq8ja8yOq% j}M6zTM";
      }
    }

    #endregion

    #region Methoden

    #region Passwortverschlüsselung
    public static string CreateHash(string password, string randomSalt) {
      // Generate a random salt
      byte[] salt = Encoding.UTF8.GetBytes(EncryptionHelper.CryptoMainSaltValue + randomSalt);
      byte[] hash = PBKDF2(password, salt, 64000, 24);

      return Convert.ToBase64String(hash);
    }

    public static bool SlowEquals(string aHash, string bHash) {
      byte[] a = Encoding.UTF8.GetBytes(aHash);
      byte[] b = Encoding.UTF8.GetBytes(bHash);

      uint diff = (uint)a.Length ^ (uint)b.Length;
      for (int i = 0; i < a.Length && i < b.Length; i++) {
        diff |= (uint)(a[i] ^ b[i]);
      }
      return diff == 0;
    }

    private static byte[] PBKDF2(string password, byte[] salt, int iterations, int outputBytes) {
      using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt)) {
        pbkdf2.IterationCount = iterations;
        return pbkdf2.GetBytes(outputBytes);
      }
    }


    #endregion

    #region Standardverschlüsselung
    public static string Encrypt(string plainText) {
      return Encrypt(plainText, EncryptionHelper.CryptoPassPhrase, EncryptionHelper.CryptoSaltValue, "SHA512", 2, EncryptionHelper.CryptoInitVector, 256);
    }

    public static string Decrypt(string cipherText) {
      return Decrypt(cipherText, EncryptionHelper.CryptoPassPhrase, EncryptionHelper.CryptoSaltValue, "SHA512", 2, EncryptionHelper.CryptoInitVector, 256, true);
    }

    public static string Encrypt(string plainText, string passPhrase) {
      return Encrypt(plainText, passPhrase, EncryptionHelper.CryptoSaltValue, "SHA512", 2, EncryptionHelper.CryptoInitVector, 256);
    }

    public static string Decrypt(string cipherText, string passPhrase) {
      return Decrypt(cipherText, passPhrase, EncryptionHelper.CryptoSaltValue, "SHA512", 2, EncryptionHelper.CryptoInitVector, 256, true);
    }


    /// <summary>
    /// Encrypts specified plaintext using Rijndael symmetric key algorithm
    /// and returns a base64-encoded result.
    /// </summary>
    /// <param name="plainText">
    /// Plaintext value to be encrypted.
    /// </param>
    /// <param name="passPhrase">
    /// Passphrase from which a pseudo-random password will be derived. The
    /// derived password will be used to generate the encryption key.
    /// Passphrase can be any string. In this example we assume that this
    /// passphrase is an ASCII string.
    /// </param>
    /// <param name="saltValue">
    /// Salt value used along with passphrase to generate password. Salt can
    /// be any string. In this example we assume that salt is an ASCII string.
    /// </param>
    /// <param name="hashAlgorithm">
    /// Hash algorithm used to generate password. Allowed values are: "MD5" and
    /// "SHA1". SHA1 hashes are a bit slower, but more secure than MD5 hashes.
    /// </param>
    /// <param name="passwordIterations">
    /// Number of iterations used to generate password. One or two iterations
    /// should be enough.
    /// </param>
    /// <param name="initVector">
    /// Initialization vector (or IV). This value is required to encrypt the
    /// first block of plaintext data. For RijndaelManaged class IV must be 
    /// exactly 16 ASCII characters long.
    /// </param>
    /// <param name="keySize">
    /// Size of encryption key in bits. Allowed values are: 128, 192, and 256. 
    /// Longer keys are more secure than shorter keys.
    /// </param>
    /// <returns>
    /// Encrypted value formatted as a base64-encoded string.
    /// </returns>
    public static string Encrypt(string plainText,
                                 string passPhrase,
                                 string saltValue,
                                 string hashAlgorithm,
                                 int passwordIterations,
                                 string initVector,
                                 int keySize) {
      // Convert strings into byte arrays.
      byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
      byte[] saltValueBytes = Encoding.UTF8.GetBytes(saltValue);

      // Convert our plaintext into a byte array.
      // Let us assume that plaintext contains UTF8-encoded characters.
      byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

      // First, we must create a password, from which the key will be derived.
      // This password will be generated from the specified passphrase and 
      // salt value. The password will be created using the specified hash 
      // algorithm. Password creation can be done in several iterations.
      PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, saltValueBytes, hashAlgorithm, passwordIterations);

      // Use the password to generate pseudo-random bytes for the encryption
      // key. Specify the size of the key in bytes (instead of bits).
      byte[] keyBytes = password.GetBytes(keySize / 8);

      // Create uninitialized Rijndael encryption object.
      RijndaelManaged symmetricKey = new RijndaelManaged();

      // It is reasonable to set encryption mode to Cipher Block Chaining
      // (CBC). Use default options for other symmetric key parameters.
      symmetricKey.Mode = CipherMode.CBC;

      // Generate encryptor from the existing key bytes and initialization 
      // vector. Key size will be defined based on the number of the key 
      // bytes.
      ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);

      // Define memory stream which will be used to hold encrypted data.
      MemoryStream memoryStream = new MemoryStream();

      // Define cryptographic stream (always use Write mode for encryption).
      CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
      // Start encrypting.
      cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);

      // Finish encrypting.
      cryptoStream.FlushFinalBlock();

      // Convert our encrypted data from a memory stream into a byte array.
      byte[] cipherTextBytes = memoryStream.ToArray();

      // Close both streams.
      memoryStream.Close();
      cryptoStream.Close();

      // Convert encrypted data into a base64-encoded string.
      string cipherText = Convert.ToBase64String(cipherTextBytes);

      // Return encrypted string.
      return cipherText;
    }

    /// <summary>
    /// Decrypts specified ciphertext using Rijndael symmetric key algorithm.
    /// </summary>
    /// <param name="cipherText">
    /// Base64-formatted ciphertext value.
    /// </param>
    /// <param name="passPhrase">
    /// Passphrase from which a pseudo-random password will be derived. The
    /// derived password will be used to generate the encryption key.
    /// Passphrase can be any string. In this example we assume that this
    /// passphrase is an ASCII string.
    /// </param>
    /// <param name="saltValue">
    /// Salt value used along with passphrase to generate password. Salt can
    /// be any string. In this example we assume that salt is an ASCII string.
    /// </param>
    /// <param name="hashAlgorithm">
    /// Hash algorithm used to generate password. Allowed values are: "MD5" and
    /// "SHA1". SHA1 hashes are a bit slower, but more secure than MD5 hashes.
    /// </param>
    /// <param name="passwordIterations">
    /// Number of iterations used to generate password. One or two iterations
    /// should be enough.
    /// </param>
    /// <param name="initVector">
    /// Initialization vector (or IV). This value is required to encrypt the
    /// first block of plaintext data. For RijndaelManaged class IV must be
    /// exactly 16 ASCII characters long.
    /// </param>
    /// <param name="keySize">
    /// Size of encryption key in bits. Allowed values are: 128, 192, and 256.
    /// Longer keys are more secure than shorter keys.
    /// </param>
    /// <returns>
    /// Decrypted string value.
    /// </returns>
    /// <remarks>
    /// Most of the logic in this function is similar to the Encrypt
    /// logic. In order for decryption to work, all parameters of this function
    /// - except cipherText value - must match the corresponding parameters of
    /// the Encrypt function which was called to generate the
    /// ciphertext.
    /// </remarks>
    public static string Decrypt(string cipherText,
                                 string passPhrase,
                                 string saltValue,
                                 string hashAlgorithm,
                                 int passwordIterations,
                                 string initVector,
                                 int keySize,
                                 bool doDecrypt) {
      if (doDecrypt) {
        // Convert strings defining encryption key characteristics into byte
        // arrays. 
        byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
        byte[] saltValueBytes = Encoding.UTF8.GetBytes(saltValue);

        // Convert our ciphertext into a byte array.
        byte[] cipherTextBytes = Convert.FromBase64String(cipherText);

        // First, we must create a password, from which the key will be 
        // derived. This password will be generated from the specified 
        // passphrase and salt value. The password will be created using
        // the specified hash algorithm. Password creation can be done in
        // several iterations.
        PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, saltValueBytes, hashAlgorithm, passwordIterations);

        // Use the password to generate pseudo-random bytes for the encryption
        // key. Specify the size of the key in bytes (instead of bits).
        byte[] keyBytes = password.GetBytes(keySize / 8);

        // Create uninitialized Rijndael encryption object.
        RijndaelManaged symmetricKey = new RijndaelManaged();

        // It is reasonable to set encryption mode to Cipher Block Chaining
        // (CBC). Use default options for other symmetric key parameters.
        symmetricKey.Mode = CipherMode.CBC;

        // Generate decryptor from the existing key bytes and initialization 
        // vector. Key size will be defined based on the number of the key 
        // bytes.
        ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);

        // Define memory stream which will be used to hold encrypted data.
        MemoryStream memoryStream = new MemoryStream(cipherTextBytes);

        // Define cryptographic stream (always use Read mode for encryption).
        CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

        // Since at this point we don't know what the size of decrypted data
        // will be, allocate the buffer long enough to hold ciphertext;
        // plaintext is never longer than ciphertext.
        byte[] plainTextBytes = new byte[cipherTextBytes.Length];

        // Start decrypting.
        int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

        // Close both streams.
        memoryStream.Close();
        cryptoStream.Close();

        // Convert decrypted data into a string. 
        // Let us assume that the original plaintext string was UTF8-encoded.
        string plainText = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);

        // Return decrypted string.   
        return plainText;
      } else {
        return "";
      }
    }
    #endregion
    #endregion

  }
}
