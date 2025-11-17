namespace CreateInvoiceSystem.Abstractions.Exceptions;

public class NotFoundException(string message) : Exception(message)
{
}
