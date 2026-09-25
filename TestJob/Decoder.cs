using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace TestJob
{
    public static class Decoder
    {
        public static bool TryDecodeBytes(string? base64, [NotNullWhen(true)] out byte[]? result)
        {
            result = null;

            if (string.IsNullOrEmpty(base64))
            {
                return false;
            }

            byte[]? byteArrayBuffer = null;
            Span<byte> buffer = 1024 >= base64.Length ? stackalloc byte[base64.Length] : default;

            if (base64.Length > 1024)
            {
                byteArrayBuffer = ArrayPool<byte>.Shared.Rent(base64.Length);
                buffer = byteArrayBuffer;
            }

            if (Convert.TryFromBase64String(base64, buffer, out int bytesWritten))
            {
                result = buffer[..bytesWritten].ToArray();
            }
            if (byteArrayBuffer != null)
            {
                ArrayPool<byte>.Shared.Return(byteArrayBuffer);
            }

            return result != null;
        }
        public static bool TryDecodeText(byte[] encodedBytes, byte[] key, [NotNullWhen(true)] out string? result)
        {
            result = null;
            using Aes aes = Aes.Create();
            aes.KeySize = 256;
            aes.Key = key;
            aes.Mode = CipherMode.ECB;

            byte[]? byteArrayBuffer = null;
            Span<byte> buffer = 1024 >= encodedBytes.Length ? stackalloc byte[encodedBytes.Length] : default;

            if (encodedBytes.Length > 1024)
            {
                byteArrayBuffer = ArrayPool<byte>.Shared.Rent(encodedBytes.Length);
                buffer = byteArrayBuffer;
            }

            if (aes.TryDecryptEcb(encodedBytes, buffer, PaddingMode.None, out int bytesWritten))
            {
                result = Encoding.UTF8.GetString(buffer[0..bytesWritten]);
            }
            if (byteArrayBuffer != null)
            {
                ArrayPool<byte>.Shared.Return(byteArrayBuffer);
            }

            return result != null;
        }
    }
}
