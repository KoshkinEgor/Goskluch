using System.Xml.Linq;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.WebUtilities;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Пространства имен СМЭВ и вида сведений
XNamespace tns = "urn://gosuslugi/sig-contract-snils-UKEP/1.0.0";
XNamespace smev = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/1.2";
XNamespace soapenv = "http://www.w3.org/2003/05/soap-envelope";

// Эмуляция асинхронной очереди СМЭВ (Хранилище принятых заявок)
var smevQueue = new ConcurrentDictionary<string, (string Snils, string DocId, string ReqId)>();

// ==========================================
// ЭНДПОИНТ 1: Прием запроса (аналог sendRequestRequest)
// ==========================================
app.MapPost("/smev3/sign-ukep", async (HttpRequest request) =>
{
    string? xmlBody = await ExtractXmlFromRequestAsync(request);
    if (xmlBody == null) return Results.BadRequest("Не удалось извлечь XML из MTOM-пакета или тела запроса");

    XDocument reqDoc;
    try { reqDoc = XDocument.Parse(xmlBody); }
    catch { return Results.BadRequest("Некорректный XML"); }

    // Поиск бизнес-запроса внутри SOAP-конверта
    var reqElement = reqDoc.Descendants(tns + "RequestSignUkep").FirstOrDefault();
    if (reqElement == null) return Results.BadRequest("Элемент RequestSignUkep не найден");

    // Извлечение метаданных
    var reqId = reqElement.Attribute("Id")?.Value ?? "unknown-req-id";
    var snils = reqElement.Element(tns + "SNILS")?.Value ?? "000-000-000 00";
    var docId = reqElement.Descendants(tns + "Document").Attributes("docId").FirstOrDefault()?.Value ?? "default-doc-id";

    // Генерация служебного MessageID СМЭВ
    var messageId = Guid.NewGuid().ToString();

    // Сохранение запроса в "очередь" СМЭВ
    smevQueue[messageId] = (snils, docId, reqId);

    // Формирование служебного ответа СМЭВ (sendRequestResponse)
    var responseDoc = new XDocument(
        new XElement(soapenv + "Envelope",
            new XAttribute(XNamespace.Xmlns + "soapenv", soapenv.NamespaceName),
            new XAttribute(XNamespace.Xmlns + "smev", smev.NamespaceName),
            new XElement(soapenv + "Header"),
            new XElement(soapenv + "Body",
                new XElement(smev + "sendRequestResponse",
                    new XElement(smev + "MessageID", messageId)
                )
            )
        )
    );

    return Results.Content(responseDoc.ToString(), "application/soap+xml; charset=utf-8");
});


app.MapPost("/smev3/get-response", async (HttpRequest request) =>
{
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();
    
    XDocument reqDoc;
    try { reqDoc = XDocument.Parse(body); }
    catch { return Results.BadRequest("Некорректный XML"); }

    var messageId = reqDoc.Descendants(smev + "MessageID").FirstOrDefault()?.Value;
    
    if (string.IsNullOrEmpty(messageId) || !smevQueue.TryGetValue(messageId, out var requestData))
    {
        return Results.NotFound("Сообщение с таким MessageID не найдено в очереди СМЭВ");
    }

    var respId = "R-" + Guid.NewGuid().ToString();
    var timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
    
    XElement businessData = requestData.Snils switch
    {
        "111-111-111 11" => new XElement(tns + "Documents",
            new XElement(tns + "Document",
                new XElement(tns + "ID", requestData.DocId),
                new XElement(tns + "SignatureGosKey",
                    new XAttribute("docId", "sig-" + requestData.DocId),
                    new XAttribute("uuid", Guid.NewGuid().ToString()),
                    new XAttribute("mimeType", "application/sig")
                )
            )),
        "222-222-222 22" => new XElement(tns + "SignReject"),
        "333-333-333 33" => new XElement(tns + "Error",
            new XElement(tns + "ErrorCode", 4),
            new XElement(tns + "ErrorMessage", "Истекло время для подписания документов")),
        _ => new XElement(tns + "Error",
            new XElement(tns + "ErrorCode", 1),
            new XElement(tns + "ErrorMessage", "Пользователь не найден"))
    };

    var respDoc = new XDocument(
        new XElement(tns + "ResponseSignUkep",
            new XAttribute("Id", respId),
            new XAttribute("ReqId", requestData.ReqId),
            new XAttribute("timestamp", timestamp),
            new XElement(tns + "SNILS", requestData.Snils),
            businessData
        )
    );

    return Results.Content(respDoc.ToString(), "application/xml; charset=utf-8");
});


async Task<string?> ExtractXmlFromRequestAsync(HttpRequest request)
{
    if (request.ContentType != null && request.ContentType.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase))
    {
       
        string? boundary = null;
        var contentTypeParts = request.ContentType.Split(';');
        
        foreach (var part in contentTypeParts)
        {
            var trimmedPart = part.Trim();
            if (trimmedPart.StartsWith("boundary=", StringComparison.OrdinalIgnoreCase))
            {
                // Извлекаем значение и убираем кавычки, если они есть
                boundary = trimmedPart.Substring("boundary=".Length).Trim('"');
                break;
            }
        }

        if (string.IsNullOrEmpty(boundary)) return null;

        var multipartReader = new MultipartReader(boundary, request.Body);
        MultipartSection section;
        
        // Читаем секции MTOM-пакета
        while ((section = await multipartReader.ReadNextSectionAsync()) != null)
        {
            if (section.ContentType != null && section.ContentType.Contains("xml", StringComparison.OrdinalIgnoreCase))
            {
                using var streamReader = new StreamReader(section.Body, System.Text.Encoding.UTF8);
                return await streamReader.ReadToEndAsync();
            }
        }
        return null;
    }
    else
    {
        // Обработка обычного XML (без MTOM)
        using var reader = new StreamReader(request.Body);
        return await reader.ReadToEndAsync();
    }
}

app.Run();