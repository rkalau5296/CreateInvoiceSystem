namespace CreateInvoiceSystem.Abstractions.Error
{
    public static class ErrorType
    {
        public const string NotFound = "NotFound";
        public const string ValidationError = "ValidationError";
        public const string Unauthorized = "Unauthorized";
        public const string Forbidden = "Forbidden";
        public const string InternalServerError = "InternalServerError";
        public const string NotAuthenticated = "NotAuthenticated";
        public const string UnsupportedMethod = "UnsupportedMethod";
        public const string RequestTooLarge = "RequestTooLarge";
        public const string TooManyRequests = "TooManyRequests";
    }
}
