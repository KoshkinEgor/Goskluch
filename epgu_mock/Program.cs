
using Microsoft.AspNetCore.Mvc;



var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapPost("/api/gusmev/order", (Dto.Meta meta) =>
{

    Console.WriteLine(meta.region);
    Console.WriteLine(meta.serviceCode);
    Console.WriteLine(meta.targetCode);

    var orderId = Random.Shared.NextInt64(1000000000L, 10000000000L).ToString();

    return Results.Json(new
    {
        orderId = orderId
    });

});

app.MapPost("/api/gusmev/push", async ([FromForm] Dto.Meta meta, [FromForm] IFormFile file) =>
{
    if (file != null && file.Length > 0)
    {
        Console.WriteLine("Файл получен");

        var directory = Path.Combine(Directory.GetCurrentDirectory(), "Archives");
        Directory.CreateDirectory(directory);
        var safeFileName = Path.GetFileName($"{Guid.NewGuid()} {file.FileName}");
        var destinationPath = Path.Combine(directory, safeFileName);
        using (var stream = new FileStream(destinationPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        Console.WriteLine($"Файл успешно сохранен по пути: {destinationPath}");
    }

    var orderId = Random.Shared.NextInt64(1000000000L, 10000000000L).ToString();

    return Results.Json(new
    {
        orderId = orderId
    });

}).DisableAntiforgery();


app.MapPost("/api/gusmev/order/{id}", (long id, [FromQuery] string? embed) =>
{
    List<(int Code, string Message)> statusCodes = new List<(int, string)>
{
    // (0, "NEW"), 
    (17, "WAIT_RESPONSE"), 
    (3, "DONE"), 
    (4, "SIGN_REJECT"), 
    (4, "EXPIRED"),           
    (5, "REQUEST_ERROR"), 
    (5, "STATE_EDS_NO_ANSWER"), 
    (5, "SNILS_NOT_FOUND")      
};

    var statusIndex = (int)(id % 7);

    var response = new
    {
        code = "OK",
        message = (string?)null,
        order = new
        {
            orderType = "ORDER",
            smevTx = "563fd555-64c1-43f1-a525-6d2b27f0feeb",
            hasEmpowerment2021 = false,
            smevMessageId = statusCodes[statusIndex].Message,
            formVersion = "1",
            ownerId = 1078941035,
            notifySms = "",
            hasTimestamp = false,
            orderStatusName = "Заявление в очереди на отправку",
            portalName = "Портал гос услуг v1",
            paymentRequired = false,
            id = id, //  подстановка ID из запроса
            signCnt = 1,
            childrenSigned = true,
            stateOrgStatusCode = "",
            inviteToEqueueUrl = "",
            statusColorCode = "",
            orderStatusId = statusCodes[statusIndex].Code,
            withCustomResult = false,
            hasNoPaidPayment = false,
            servicePassportId = "600374",
            routingCode = "",
            sourceSystem = "PGU",
            stateOrgStatusName = "",
            serviceUrl = "",
            orgId = 1078941035,
            serviceEpguId = "1",
            edsStatus = "EDS_MANDATORY",
            allowToDelete = false,
            requestDate = "2022-07-21T17:03:17.543+0300",
            complexMode = "",
            stateOrgCode = "minkomsvyaz",
            personType = "LEGAL",
            serviceTargetId = "-10000000374",
            orderPayments = Array.Empty<object>(),
            orderResponseFiles = Array.Empty<object>(),
            hasResult = false,
            userId = 1078941034,
            allowToEdit = false,
            closed = false,
            readyToSign = false,
            currentStatusHistory = new
            {
                date = "2022-07-21T17:03:17.532+0300",
                cancelAllowed = false,
                unreadEvent = true,
                deliveryCancelAllowed = false,
                finalStatus = false,
                orderId = id,
                stateOrgStatusCode = "",
                author = "",
                hasResult = "N",
                stateOrgStatusDescr = "",
                title = "Заявление в очереди на отправку",
                mfcFinalStatus = false,
                sendMessageAllowed = false,
                statusId = 17,
                editAllowed = false,
                sender = "Министерство цифрового развития, связи и массовых коммуникаций Российской Федерации",
                mnemonic = "",
                comment = "",
                id = 2042110473
            },
            infoMessages = Array.Empty<object>(),
            notifyEmail = "",
            paymentCount = 0,
            draftHidden = false,
            stateStructureId = "10000001086",
            hasNewStatus = true,
            stateOrgId = 35,
            redirectUrl = "",
            hasChildren = false,
            stateStructureName = "Минцифры России",
            portalCode = "PGU",
            esepOperationId = "",
            orderAttributeEvents = Array.Empty<object>(),
            notifyPush = "",
            eserviceId = "10000000374",
            currentStatusHistoryId = 2042110473,
            linkToOrderForm = "",
            extOrderUrl = "",
            paymentStatusEvents = Array.Empty<object>(),
            payback = false,
            readyToPush = false,
            admLevelCode = "FEDERAL",
            formPrefilling = false,
            allFileSign = false,
            orgUserName = "",
            noPaidPaymentCount = -1,
            creationMode = "direct",
            notifyTelegram = "",
            steps = Array.Empty<object>(),
            powerMnemonic = "",
            statuses = new[]
            {
                new {
                    date = "2022-07-21T17:03:15.429+0300",
                    cancelAllowed = false,
                    unreadEvent = true,
                    deliveryCancelAllowed = false,
                    finalStatus = false,
                    orderId = id,
                    stateOrgStatusCode = "",
                    author = "",
                    hasResult = "N",
                    stateOrgStatusDescr = "",
                    title = "Черновик заявления",
                    mfcFinalStatus = false,
                    sendMessageAllowed = false,
                    statusId = 0,
                    editAllowed = false,
                    sender = "",
                    mnemonic = "",
                    comment = "",
                    id = 2042110472
                },
                new {
                    date = "2022-07-21T17:03:17.532+0300",
                    cancelAllowed = false,
                    unreadEvent = true,
                    deliveryCancelAllowed = false,
                    finalStatus = false,
                    orderId = id,
                    stateOrgStatusCode = "",
                    author = "",
                    hasResult = "N",
                    stateOrgStatusDescr = "",
                    title = "Заявление в очереди на отправку",
                    mfcFinalStatus = false,
                    sendMessageAllowed = false,
                    statusId = 17,
                    editAllowed = false,
                    sender = "Министерство цифрового развития, связи и массовых коммуникаций Российской Федерации",
                    mnemonic = "",
                    comment = "",
                    id = 2042110473
                }
            },
            orderDate = "2022-07-21T17:03:15.000+0300",
            updated = "2022-07-21T17:03:17.543+0300",
            checkQueue = false,
            withDelivery = false,
            gisdo = false,
            userSelectedRegion = "00000000000",
            description = "",
            eQueueEvents = Array.Empty<object>(),
            hasActiveInviteToEqueue = false,
            multRegion = false,
            extSystem = false,
            useAsTemplate = false,
            qrlink = new
            {
                hasAltMimeType = false,
                fileName = "",
                edsStatus = "",
                fileSize = 0,
                canSentToMFC = false,
                link = "",
                id = "",
                mimeType = "",
                hasDigitalSignature = false,
                additionalName = ""
            },
            hasPreviewPdf = false,
            testUser = false,
            textMessages = Array.Empty<object>(),
            unreadMessageCnt = 0,
            parentOrderStateStructureName = "",
            cr_uin = "",
            infSysCode = "",
            serviceName = "Отправка документов на подпись для организаций",
            deprecatedService = false,
            hubForm = false,
            extOrderId = "",
            orderAttachmentFiles = new[]
            {
                new {
                    fileName = "07_01.pdf",
                    fileSize = 76092,
                    link = $"terrabyte://00/{id}/07_01.pdf/2",
                    id = $"{id}/files/MDdfMDEucGRm",
                    mimeType = "application/pdf",
                    hasDigitalSignature = false,
                    type = "ATTACHMENT"
                },
                new {
                    fileName = "req.xml",
                    fileSize = 1200,
                    link = $"terrabyte://00/{id}/req.xml/2",
                    id = $"{id}/files/cmVxLnhtbA",
                    mimeType = "application/xml",
                    hasDigitalSignature = true,
                    type = "REQUEST"
                }
            },
            online = false,
            location = "77"
        }
    };

    return Results.Json(response);
});

app.Run();