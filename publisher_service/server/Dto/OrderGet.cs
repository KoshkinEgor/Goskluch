using System;
using System.Collections.Generic;
using Repo;

namespace Dto;

public class OrderGet
{
    public int Id { get; set; }
    public int EpguOrderId { get; set; }
    public DateTime CreatedDate { get; set; }
    
    public string ReceiverIdType { get; set; }
    public string ReceiverId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string Description { get; set; }
    
    public ICollection<Dto.DocumentGet> DocumentsPack { get; set; }

    public OrderGet(Order order)
    {
        Id = order.Id;
        EpguOrderId = order.EpguOrderId;
        CreatedDate = order.CreatedDate;
        ReceiverIdType = order.ReceiverIdType;
        ReceiverId = order.ReceiverId;
        UserId = order.UserId;
        UserName = order.User.Name;
        Description = order.Description;
        DocumentsPack = order.Documents.Select(d => new Dto.DocumentGet(d)).ToList();

    }
}