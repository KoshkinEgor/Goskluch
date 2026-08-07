
using System.Security.Cryptography;
using System.Text;

public static class FileSignatureHelper
{
    public static byte[] CreateDetachedSignatureBytes(byte[] data)
    {
        if (data == null || data.Length == 0)
            throw new ArgumentException("Данные для подписания не могут быть пустыми.", nameof(data));

        // 1. Маркер, указывающий на то, что это заглушка (удобно для отладки)
        string stubMarker = "STUB_DETACHED_SIGNATURE_PKCS7_GOST_MOCK";
        byte[] markerBytes = Encoding.UTF8.GetBytes(stubMarker);

        // 2. Вычисляем SHA-256 хеш от исходных данных для имитации привязки подписи к содержимому
        using var sha256 = SHA256.Create();
        byte[] dataHash = sha256.ComputeHash(data);

        // 3. Формируем итоговый массив байт (имитация бинарного файла .sig)
        byte[] signatureBytes = new byte[markerBytes.Length + dataHash.Length];
        Buffer.BlockCopy(markerBytes, 0, signatureBytes, 0, markerBytes.Length);
        Buffer.BlockCopy(dataHash, 0, signatureBytes, markerBytes.Length, dataHash.Length);

        return signatureBytes;
    }
}