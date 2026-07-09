using HotelStay.Domain.Enums;

namespace HotelStay.Domain.ValueObjects;

public class Document
{
    public string HolderName { get; init; }
    public DocumentType Type { get; init; }
    public string Number { get; init; }

    private Document()
    {
        HolderName = string.Empty;
        Number = string.Empty;
    }

    public static Document Create(string holderName, DocumentType type, string number)
    {
        return new Document
        {
            HolderName = holderName,
            Type = type,
            Number = number
        };
    }
}
