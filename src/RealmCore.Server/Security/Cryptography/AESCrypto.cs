using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace RealmCore.Server.Security.Cryptography;

public class AESCrypto
{
    private readonly byte[] _key = new byte[16];
    private readonly byte[] _iv = new byte[16];
    private readonly IBufferedCipher _cipher = CipherUtilities.GetCipher("AES/CTR/NoPadding");
    private const int _chunkSize = 16;

    // Key and IV I get from client.
    public AESCrypto(byte[] key, byte[] iv, bool forEncryption)
    {
        _key = key;
        _iv = iv;
        _cipher.Init(forEncryption, new ParametersWithIV(new KeyParameter(_key), _iv));
    }

    public byte[] PerformAES(byte[] incomingBytes)
    {
        int blockCount = incomingBytes.Length / _chunkSize; // Number of blocks
        int blockRemaining = incomingBytes.Length % _chunkSize; // Remaining bytes of the last block

        byte[] outcomingBytes = new byte[incomingBytes.Length];

        for (var i = 0; i < blockCount; i++)
        {
            // Why do I need to re-init it again?
            //cipher.Init(false, new ParametersWithIV(new KeyParameter(Key), IV));

            byte[] temp = new byte[_chunkSize];
            Array.Copy(incomingBytes, i * _chunkSize, temp, 0, _chunkSize);

            byte[] decryptedChunk = _cipher.ProcessBytes(temp);
            Array.Copy(decryptedChunk, 0, outcomingBytes, i * _chunkSize, _chunkSize);
            //Increase(IV); Why do I need to increse iv by hand?
        }

        if (blockRemaining != 0)
        {
            // Why do I need to re-init it again?
            //cipher.Init(false, new ParametersWithIV(new KeyParameter(Key), IV));

            byte[] temp = new byte[blockRemaining];
            Array.Copy(incomingBytes, incomingBytes.Length - blockRemaining, temp, 0, blockRemaining);

            byte[] decryptedChunk = _cipher.DoFinal(temp);
            Array.Copy(decryptedChunk, 0, outcomingBytes, incomingBytes.Length - blockRemaining, blockRemaining);
            //Increase(IV); Why do I need to increse iv by hand?
        }

        return outcomingBytes;
    }
}
