using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace RealmCore.Server.Security.Cryptography;

public class AESCrypto
{
    private byte[] Key = new byte[16];
    private byte[] IV = new byte[16];
    private const int CHUNK_SIZE = 16;
    private IBufferedCipher cipher = CipherUtilities.GetCipher("AES/CTR/NoPadding");

    // Key and IV I get from client.
    public AESCrypto(byte[] key, byte[] iv, bool forEncryption)
    {
        Key = key;
        IV = iv;
        cipher.Init(forEncryption, new ParametersWithIV(new KeyParameter(Key), IV));
    }

    public byte[] PerformAES(byte[] incomingBytes)
    {
        int blockCount = incomingBytes.Length / CHUNK_SIZE; // Number of blocks
        int blockRemaining = incomingBytes.Length % CHUNK_SIZE; // Remaining bytes of the last block

        byte[] outcomingBytes = new byte[incomingBytes.Length];

        for (var i = 0; i < blockCount; i++)
        {
            // Why do I need to re-init it again?
            //cipher.Init(false, new ParametersWithIV(new KeyParameter(Key), IV));

            byte[] temp = new byte[CHUNK_SIZE];
            Array.Copy(incomingBytes, i * CHUNK_SIZE, temp, 0, CHUNK_SIZE);

            byte[] decryptedChunk = cipher.ProcessBytes(temp);
            Array.Copy(decryptedChunk, 0, outcomingBytes, i * CHUNK_SIZE, CHUNK_SIZE);
            //Increase(IV); Why do I need to increse iv by hand?
        }

        if (blockRemaining != 0)
        {
            // Why do I need to re-init it again?
            //cipher.Init(false, new ParametersWithIV(new KeyParameter(Key), IV));

            byte[] temp = new byte[blockRemaining];
            Array.Copy(incomingBytes, incomingBytes.Length - blockRemaining, temp, 0, blockRemaining);

            byte[] decryptedChunk = cipher.DoFinal(temp);
            Array.Copy(decryptedChunk, 0, outcomingBytes, incomingBytes.Length - blockRemaining, blockRemaining);
            //Increase(IV); Why do I need to increse iv by hand?
        }

        return outcomingBytes;
    }

    private void Increase(byte[] iv)
    {
        for (var i = 0; i < iv.Length; i++)
        {
            iv[i]++;
            if (iv[i] != 0)
            {
                break;
            }
        }
    }
}
