using System;

namespace HotelStay.Domain.Exceptions;

public sealed class DocumentMismatchException : Exception
{
    public DocumentMismatchException(string message) : base(message) { }

    public DocumentMismatchException(string message, Exception innerException)
        : base(message, innerException) { }
}
