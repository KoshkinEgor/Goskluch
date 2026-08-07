
using Repo;

public class OrderStatusData
{
    public int OrderStatusId { get; set; }
    public string MessageId { get; set; }
    public string MessageAnnotation { get; set; }


}

public static class OrderStatusCodesHelper
{
    public static readonly Dictionary<string, OrderStatusData> StatusCodes =
        new Dictionary<string, OrderStatusData>
        {
            {
                "NEW",
                new OrderStatusData
                {
                    OrderStatusId = 0,
                    MessageId = "Черновик заявления",
                    MessageAnnotation = "Запрос отправлен, но документы еще не прикреплены"
                }
            },
            {
                "WAIT_RESPONSE",
                new OrderStatusData
                {
                    OrderStatusId = 17,
                    MessageId = "Заявление в очереди на отправку",
                    MessageAnnotation = "Заявление ожидает или уже передано в МП «Госключ»"
                }
            },
            {
                "DONE",
                new OrderStatusData
                {
                    OrderStatusId = 3,
                    MessageId = "Документы подписаны",
                    MessageAnnotation = "Документы подписаны в МП «Госключ» и готовы к скачиванию"
                }
            },
            {
                "SIGN_REJECT",
                new OrderStatusData
                {
                    OrderStatusId = 4,
                    MessageId = "Отказано в предоставлении услуги",
                    MessageAnnotation = "Отказ пользователя от подписания документов в МП «Госключ»"
                }
            },
            {
                "EXPIRED",
                new OrderStatusData
                {
                    OrderStatusId = 4,
                    MessageId = "Отказано в предоставлении услуги",
                    MessageAnnotation = "Истекло время подписания документов в МП «Госключ»"
                }
            },
            {
                "REQUEST_ERROR",
                new OrderStatusData
                {
                    OrderStatusId = 5,
                    MessageId = "Ошибка отправки в ведомство",
                    MessageAnnotation = "Внутренняя ошибка системы"
                }
            },
            {
                "STATE_EDS_NO_ANSWER",
                new OrderStatusData
                {
                    OrderStatusId = 5,
                    MessageId = "УКЭП отправителя не прошла проверку",
                    MessageAnnotation = "Непрохождение проверки УКЭП отправителя по истечении 8 часов"
                }
            },
            {
                "SNILS_NOT_FOUND",
                new OrderStatusData
                {
                    OrderStatusId = 5,
                    MessageId = "Ошибка отправки в ведомство",
                    MessageAnnotation = "Учетная запись физического лица не найдена"
                }

            },

                {"",
                new OrderStatusData
                {
                    OrderStatusId = -1,
                    MessageId = "Статус не указан",
                    MessageAnnotation = ""
                }
            }
        };
}