namespace SupportDesk.Application.Common;

// Thrown for business-rule failures; the API maps it to 400 Bad Request.
public class AppValidationException(string message) : Exception(message);
