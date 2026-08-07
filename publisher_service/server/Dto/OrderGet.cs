using Repo;
using Smev;

namespace Dto;

public class OrderGet
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public string ReceiverSnils { get; set; }
    public string ReceiverOid { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string Description { get; set; }
    public string ReqDepartment {get;set;}
    public OrderStatusData StatusData { get; set; }
    public ICollection<DocumentGet> DocumentsPack { get; set; }

    public OrderGet(Order order)
    {
        Id = order.Id;
        CreatedDate = order.CreatedDate;
        ReceiverSnils = order?.ReceiverSnils ?? "";
        ReceiverOid = order?.ReceiverOid ?? "";
        UserId = order.UserId;
        UserName = order.User.Name;

        Description = order?.Description ?? "";
        DocumentsPack = order?.Documents?
            .Select(d => new Dto.DocumentGet(d))
            .ToList()
            ?? new List<DocumentGet>();



        string orderStatusId = "";

        if (order?.EpguOrder != null)
        {

            orderStatusId = order.EpguOrder.OrderStatusId;
            ReqDepartment = "epgu";
        }

        else if (order?.SmevOrder != null)
        {
            orderStatusId = order.SmevOrder.OrderStatusId;
            ReqDepartment = "smev";

        }
        


        StatusData = OrderStatusCodesHelper.StatusCodes[orderStatusId];


    }
}