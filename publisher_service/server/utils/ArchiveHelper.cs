

using System.IO.Compression;

public static class ArchiveHelper
{

    public static void AddFileToArchive(ZipArchive archive, string entryName, byte[] fileBytes)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        using var entryStream = entry.Open();
        entryStream.Write(fileBytes, 0, fileBytes.Length);
    }

    public static async Task<byte[]> buildArchiveAsync()
    {
        using var memoryStream = new MemoryStream();
        var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true);
        byte[] zipArchiveBytes = memoryStream.ToArray();
        return zipArchiveBytes;
    }

    public static async Task<byte[]> buildArchiveAsync(Dictionary<string, byte[]> filesToZip)
    {
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var kvp in filesToZip)
            {
                var entry = archive.CreateEntry(kvp.Key, CompressionLevel.Optimal);
                using var entryStream = entry.Open();
                await entryStream.WriteAsync(kvp.Value, 0, kvp.Value.Length);
            }
        }
        byte[] zipArchiveBytes = memoryStream.ToArray();
        return zipArchiveBytes;
    }

}