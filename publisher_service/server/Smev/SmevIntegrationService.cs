
using System.Text;
using System.Xml.Linq;

namespace Smev;

public class SmevIntegrationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Repo.ApplicationContext _context;
    private readonly IConfiguration _configuration;
    private readonly string _storagePath;
    private readonly ILogger<SmevIntegrationService> _logger;

    private const string SmevBasicNs = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.2";
    private const string SoapEnvNs = "http://www.w3.org/2003/05/soap-envelope";
    private const string SmevServiceNs = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/1.2";
    private const string BusinessNs = "urn://gosuslugi/sig-contract-snils-UKEP/1.0.0";

    public SmevIntegrationService(
    IHttpClientFactory httpClientFactory,
    Repo.ApplicationContext context,
    ILogger<SmevIntegrationService> logger,
    IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _context = context;
        _configuration = configuration;
        _storagePath = _configuration["Storage:StoragePath"] ?? "DocumentsStorage";
        _logger = logger;
    }


    public async Task<string?> SendOrderAsync(Repo.Order order)
    {
        if (string.IsNullOrEmpty(order.ReceiverSnils))
            throw new ArgumentException("СМЭВ поддерживает отправку только по СНИЛС");

        var reqBuilder = new SmevRequestBuilder();
        reqBuilder.SetRequest(
            snils: order.ReceiverSnils,
            desc: order.Description,
            backlink: null,
            signExp: null);

        foreach (var doc in order.Documents)
        {
            string localPath = Path.Combine(_storagePath, doc.LocalName);
            byte[] fileBytes = await File.ReadAllBytesAsync(localPath);

            // Формирование отсоединённой подписи отправителя (УКЭП)
            byte[] signatureBytes = FileSignatureHelper.CreateDetachedSignatureBytes(fileBytes);

            var docInfo = new SmevDocumentInfo
            {
                DocumentId = doc.DocumentEpguCode,
                MimeType = FileNameHelper.GetMimeType(doc.Name),
                Description = order.Description,
                FileName = doc.Name,
                Content = fileBytes,
                SignatureContent = signatureBytes
            };

            reqBuilder.AddDocument(docInfo);
        }

        // Формирование XML бизнес-запроса
        string requestXml = reqBuilder.Build();
        var documents = reqBuilder.GetDocuments();

        // Формирование MTOM-пакета
        HttpContent mtomContent = SmevMtomBuilder.Build(requestXml, documents);

        try
        {
            // Отправка запроса в СМЭВ
            using var client = _httpClientFactory.CreateClient("SmevApiClient");
            var response = await client.PostAsync("smev3/sign-ukep", mtomContent);
            response.EnsureSuccessStatusCode();

            // Сохранение RequestId для последующего запроса статуса (getStatusRequest)
            var messageId = await ExtractMessageIdAsync(response);

            order.SmevOrder.SmevMessageId = messageId;

            _context.SaveChanges();
            return messageId;
        }
        catch (System.Exception)
        {
            return null;
        }

    }

    private async Task<string> ExtractMessageIdAsync(HttpResponseMessage response)
    {
        var responseContent = await response.Content.ReadAsStringAsync();
        var xdoc = XDocument.Parse(responseContent);
        XNamespace smev = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/1.2";
        return xdoc.Descendants(smev + "MessageID").FirstOrDefault()?.Value ?? string.Empty;
    }

    public async Task<List<(string DocumentId, string SignatureUuid)>?> GetOrderResultAsync(Repo.Order order)
    {
        if (order.SmevOrder == null || string.IsNullOrEmpty(order.SmevOrder.SmevMessageId))
        {
            _logger.LogWarning("Невозможно получить результат: отсутствует SmevMessageId для заказа {OrderId}.", order.Id);
            return null;
        }

        string messageId = order.SmevOrder.SmevMessageId;
        XNamespace soapenv = "http://www.w3.org/2003/05/soap-envelope";
        XNamespace smev = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/1.2";
        XNamespace tns = "urn://gosuslugi/sig-contract-snils-UKEP/1.0.0";

        var requestXml = new XElement(soapenv + "Envelope",
            new XAttribute(XNamespace.Xmlns + "soapenv", soapenv.NamespaceName),
            new XAttribute(XNamespace.Xmlns + "smev", smev.NamespaceName),
            new XElement(soapenv + "Header"),
            new XElement(soapenv + "Body",
                new XElement(smev + "getResponseRequest",
                    new XElement(smev + "MessageID", messageId)
                )
            )
        );

        var xmlString = requestXml.ToString(SaveOptions.DisableFormatting);
        var content = new StringContent(xmlString, Encoding.UTF8, "application/soap+xml");

        using var client = _httpClientFactory.CreateClient("SmevApiClient");

        try
        {
            var response = await client.PostAsync("smev3/get-response", content);
            response.EnsureSuccessStatusCode();

            string rawResponse = await response.Content.ReadAsStringAsync();
            string xmlPart = ExtractXmlFromMtom(rawResponse);

            var xdoc = XDocument.Parse(xmlPart);
            var responseSignUkep = xdoc.Descendants(tns + "ResponseSignUkep").FirstOrDefault();

            if (responseSignUkep == null)
            {
                return null;
            }

            var documentsNode = responseSignUkep.Element(tns + "Documents");

            if (documentsNode != null)
            {
                var signedDocuments = new List<(string DocumentId, string SignatureUuid)>();

                foreach (var doc in documentsNode.Elements(tns + "Document"))
                {
                    var idElement = doc.Element(tns + "ID");
                    var sigElement = doc.Element(tns + "SignatureGosKey");

                    var docId = idElement?.Value;
                    var sigUuid = sigElement?.Attribute("uuid")?.Value;

                    if (!string.IsNullOrWhiteSpace(docId) && !string.IsNullOrWhiteSpace(sigUuid))
                    {
                        signedDocuments.Add((
                            DocumentId: docId,
                            SignatureUuid: sigUuid
                        ));
                    }
                }
                return signedDocuments;
            }

            // Во всех остальных случаях (SignReject, Error или неизвестный формат) возвращаем null
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Исключение при получении результата СМЭВ для MessageID {MessageId}", messageId);
            return null;
        }
    }

    public async Task<string?> GetOrderStatusAsync(Repo.Order order)
    {
        if (order.SmevOrder == null || string.IsNullOrEmpty(order.SmevOrder.SmevMessageId))
        {
            _logger.LogWarning("Невозможно получить результат: отсутствует SmevMessageId для заказа {OrderId}.", order.Id);
            return null;
        }

        string messageId = order.SmevOrder.SmevMessageId;

        // 1. Формирование SOAP-запроса getResponseRequest (СМЭВ 3, типы 1.2)
        XNamespace soapenv = "http://www.w3.org/2003/05/soap-envelope";
        XNamespace smev = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/1.2";
        XNamespace tns = "urn://gosuslugi/sig-contract-snils-UKEP/1.0.0";

        var requestXml = new XElement(soapenv + "Envelope",
            new XAttribute(XNamespace.Xmlns + "soapenv", soapenv.NamespaceName),
            new XAttribute(XNamespace.Xmlns + "smev", smev.NamespaceName),
            new XElement(soapenv + "Header"),
            new XElement(soapenv + "Body",
                new XElement(smev + "getResponseRequest",
                    new XElement(smev + "MessageID", messageId)
                )
            )
        );

        var xmlString = requestXml.ToString(SaveOptions.DisableFormatting);
        var content = new StringContent(xmlString, Encoding.UTF8, "application/soap+xml");

        // 2. Отправка запроса на сервер
        using var client = _httpClientFactory.CreateClient("SmevApiClient");
        var response = await client.PostAsync("smev3/get-response", content);
        response.EnsureSuccessStatusCode();

        // 3. Парсинг ответа СМЭВ
        string rawResponse = await response.Content.ReadAsStringAsync();
        string xmlPart = ExtractXmlFromMtom(rawResponse);

        XDocument xdoc;
        try
        {
            xdoc = XDocument.Parse(xmlPart);
        }
        catch (Exception ex)
        {
            return null;
        }

        // Поиск бизнес-ответа ResponseSignUkep внутри служебного конверта СМЭВ
        var responseSignUkep = xdoc.Descendants(tns + "ResponseSignUkep").FirstOrDefault();
        if (responseSignUkep == null)
        {
            return null;
        }

        // 4. Анализ результата и маршрутизация по сценариям (п. 2.2 РП)
        string orderStatusId = "WAIT_RESPONSE"; // Статус по умолчанию

        var documentsNode = responseSignUkep.Element(tns + "Documents");
        var signRejectNode = responseSignUkep.Element(tns + "SignReject");
        var errorNode = responseSignUkep.Element(tns + "Error");

        if (documentsNode != null)
        {
            orderStatusId = "DONE";
        }
        else if (signRejectNode != null)
        {
            orderStatusId = "SIGN_REJECT";
        }
        else if (errorNode != null)
        {
            var errorCode = errorNode.Element(tns + "ErrorCode")?.Value;
            var errorMessage = errorNode.Element(tns + "ErrorMessage")?.Value;

            orderStatusId = errorCode switch
            {
                "1" => "SNILS_NOT_FOUND",
                "4" => "EXPIRED",
                "10" => "STATE_EDS_NO_ANSWER",
                "11" => "REQUEST_ERROR",
                "12" => "STATE_EDS_NO_ANSWER",
                "13" => "STATE_EDS_NO_ANSWER",
                "14" => "STATE_EDS_NO_ANSWER",
                _ => ""
            };
        }

        order.SmevOrder.OrderStatusId = orderStatusId;
        await _context.SaveChangesAsync();
        return orderStatusId;
    }


    private string ExtractXmlFromMtom(string rawMtomResponse)
    {
        int xmlStart = rawMtomResponse.IndexOf("<?xml", StringComparison.OrdinalIgnoreCase);
        if (xmlStart == -1) xmlStart = rawMtomResponse.IndexOf("<soapenv:Envelope", StringComparison.OrdinalIgnoreCase);
        if (xmlStart == -1) xmlStart = rawMtomResponse.IndexOf("<Envelope", StringComparison.OrdinalIgnoreCase);

        if (xmlStart >= 0)
        {
            int xmlEnd = rawMtomResponse.LastIndexOf("</soapenv:Envelope>", StringComparison.OrdinalIgnoreCase);
            if (xmlEnd == -1) xmlEnd = rawMtomResponse.LastIndexOf("</Envelope>", StringComparison.OrdinalIgnoreCase);

            if (xmlEnd > xmlStart)
            {
                int length = xmlEnd - xmlStart + "</soapenv:Envelope>".Length;
                return rawMtomResponse.Substring(xmlStart, length);
            }
        }

        return rawMtomResponse;
    }


}