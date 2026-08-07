

using System.IO.Compression;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Repo;

namespace Epgu;

public class EpguIntegrationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Repo.ApplicationContext _context;
    private readonly IConfiguration _configuration;
    private readonly string _storagePath;
    private readonly ILogger<EpguIntegrationService> _logger;

    public EpguIntegrationService(
        IHttpClientFactory httpClientFactory,
        Repo.ApplicationContext context,
        ILogger<EpguIntegrationService> logger,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _context = context;
        _configuration = configuration;
        _storagePath = _configuration["Storage:StoragePath"] ?? "DocumentsStorage";
        _logger = logger;
    }

    public async Task<(int Code, string OrderStatusId)?> GetOrderStatusAsync(Repo.Order order)
    {

        var config = _context.ConfigSettings.FirstOrDefault();
        var client = _httpClientFactory.CreateClient("EpguApiClient");

        var meta = new Dto.Meta
        {
            region = config.Region,
            serviceCode = config.ServiceCode,
            targetCode = config.TargetCode
        };

        var json = JsonSerializer.Serialize(meta);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"order/{order.EpguOrder.EpguOrderCode}", content);
        Console.WriteLine(response.StatusCode);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(jsonResponse);

            Console.WriteLine(jsonResponse);

            int statusCode = -1;
            string smevMessageId = "";

            if (jsonDoc.RootElement.TryGetProperty("order", out var orderElement))
            {

                if (orderElement.TryGetProperty("orderStatusId", out var statusIdElement))
                {
                    statusCode = statusIdElement.GetInt32();
                }

                if (orderElement.TryGetProperty("smevMessageId", out var smevMessageIdElement))
                {
                    smevMessageId = smevMessageIdElement.GetString() ?? "";
                }

            }



            Console.WriteLine(statusCode);
            Console.WriteLine(smevMessageId);

            return (statusCode, smevMessageId);

        }

        return null;
    }

    public async Task<string?> FetchOrderIdAsync()
    {
        var config = _context.ConfigSettings.FirstOrDefault();
        var client = _httpClientFactory.CreateClient("EpguApiClient");

        var meta = new Dto.Meta
        {
            region = config.Region,
            serviceCode = config.ServiceCode,
            targetCode = config.TargetCode
        };

        string orderId = "0";

        var json = JsonSerializer.Serialize(meta);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync($"order/", content: content);


            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(jsonResponse);

                if (jsonDoc.RootElement.TryGetProperty("orderId", out JsonElement orderIdElement))
                {
                    orderId = orderIdElement.GetString();
                    return orderId;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        catch (System.Exception)
        {
            return null;
        }

    }


    public async Task<string?> SendOrderAsync(Repo.Order order)
    {

        var config = _context.ConfigSettings.FirstOrDefault();
        var filesToZip = new Dictionary<string, byte[]>();

        // 1. Сформировать req.xml
        var reqXmlBuilder = new EpguRequestBuilder()
        .SetConfig(
            mnemonics: config.Mnemonics,
            serviceName: config.ServiceName,
            serviceCode: config.ServiceCode,
            orgName: config.OrgName

        )
        .SetRecipient(
            oid: order.ReceiverOid,
            snils: order.ReceiverSnils,
            signType: order.EpguOrder.SignatureType
        );

        foreach (var doc in order.Documents)
        {

            var docInfo = new EpguDocumentInfo
            {
                DocumentId = doc.DocumentEpguCode,
                MimeType = FileNameHelper.GetMimeType(doc.Name),
                Description = order.Description,
                SignExpiration = DateTime.UtcNow.AddHours(25)
            };

            reqXmlBuilder.AddDocument(docInfo);
        }
        ;

        var reqXmlBytes = reqXmlBuilder.BuildBytes();


        // 2. Подписать req.xml
        var reqSignatureBytes = FileSignatureHelper.CreateDetachedSignatureBytes(reqXmlBytes);


        // 3. Добавить req.xml и .sig в архив
        filesToZip.Add("req.xml", reqXmlBytes);
        filesToZip.Add("req.xml.sig", reqSignatureBytes);

        // Добавить документы в архив
        foreach (var doc in order.Documents)
        {
            string localPath = Path.Combine(_storagePath, doc.LocalName);
            byte[] fileBytes = await File.ReadAllBytesAsync(localPath);

            // Имя файла в архиве должно быть строго ZipFileName
            filesToZip.Add(doc.ZipFileName, fileBytes);
            filesToZip.Add(doc.ZipFileName + ".sig", FileSignatureHelper.CreateDetachedSignatureBytes(fileBytes));
        }

        // 4. Сформировать архив
        var archiveBytes = await ArchiveHelper.buildArchiveAsync(filesToZip);



        // 5. Отправить архив в ЕПГУ и дождаться ответа
        var meta = new Dto.Meta
        {
            region = config.Region,
            serviceCode = config.ServiceCode,
            targetCode = config.TargetCode
        };


        using var multipartContent = new MultipartFormDataContent();

        string metaJson = JsonSerializer.Serialize(meta);
        var metaContent = new StringContent(metaJson, Encoding.UTF8, "application/json");
        multipartContent.Add(metaContent, "meta");

        var fileContent = new ByteArrayContent(archiveBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
        multipartContent.Add(fileContent, "file", "archive.zip");

        try
        {
            using var client = _httpClientFactory.CreateClient("EpguApiClient");
            var response = await client.PostAsync("push", multipartContent);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(jsonResponse);

                if (jsonDoc.RootElement.TryGetProperty("orderId", out JsonElement orderIdElement))
                {
                    return orderIdElement.GetString();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;                
            }
        }
        catch (System.Exception)
        {
            return null;
        }


    }


}