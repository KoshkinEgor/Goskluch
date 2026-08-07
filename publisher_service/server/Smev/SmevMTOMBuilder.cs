using System.Net.Http.Headers;
using System.Text;

namespace Smev;

public static class SmevMtomBuilder
{
    public static MultipartContent Build(
        string requestXml,
        IReadOnlyList<SmevDocumentInfo> documents)
    {
        string boundary = $"----=_Part_{Guid.NewGuid():N}";
        string rootContentId = $"rootpart.{Guid.NewGuid()}@smev.message";

        string soapEnvelope = SmevSoapBuilder.BuildEnvelope(requestXml, documents);

        var multipart = new MultipartContent("related", boundary);
        
        // ИСПРАВЛЕНИЕ: Убираем ручные кавычки "\"". 
        // .NET HttpClient сам добавит кавычки при сериализации, если они требуются по RFC.
        multipart.Headers.ContentType!.Parameters.Add(
            new NameValueHeaderValue("type", "application/xop+xml"));
            
        multipart.Headers.ContentType.Parameters.Add(
            new NameValueHeaderValue("start", $"<{rootContentId}>")); 
            
        multipart.Headers.ContentType.Parameters.Add(
            new NameValueHeaderValue("start-info", "application/soap+xml"));

        // 1. Root part — SOAP-конверт (application/xop+xml)
        var xmlContent = new ByteArrayContent(Encoding.UTF8.GetBytes(soapEnvelope));
        xmlContent.Headers.ContentType = new MediaTypeHeaderValue("application/xop+xml");
        
        // Убираем ручные кавычки
        xmlContent.Headers.ContentType.Parameters.Add(
            new NameValueHeaderValue("type", "application/soap+xml"));
        xmlContent.Headers.ContentType.Parameters.Add(
            new NameValueHeaderValue("charset", "utf-8"));
            
        xmlContent.Headers.Add("Content-ID", $"<{rootContentId}>");
        xmlContent.Headers.Add("Content-Transfer-Encoding", "8bit");
        multipart.Add(xmlContent);

        // 2. Бинарные вложения — документы и подписи
        foreach (var doc in documents)
        {
            var docContent = new ByteArrayContent(doc.Content);
            docContent.Headers.ContentType = new MediaTypeHeaderValue(doc.MimeType);
            docContent.Headers.Add("Content-ID", $"<{doc.DocumentUuid}>");
            docContent.Headers.Add("Content-Transfer-Encoding", "binary");
            multipart.Add(docContent);

            var sigContent = new ByteArrayContent(doc.SignatureContent);
            sigContent.Headers.ContentType = new MediaTypeHeaderValue("application/sig");
            sigContent.Headers.Add("Content-ID", $"<{doc.SignatureUuid}>");
            sigContent.Headers.Add("Content-Transfer-Encoding", "binary");
            multipart.Add(sigContent);
        }

        return multipart;
    }
}