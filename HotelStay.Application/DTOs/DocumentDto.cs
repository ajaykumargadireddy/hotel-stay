using HotelStay.Domain.Enums;

namespace HotelStay.Application.DTOs;

public class DocumentDto
{
    public string HolderName { get; set; } = string.Empty;
    public DocumentType Type { get; set; } 
    public string Number { get; set; } = string.Empty;
}
