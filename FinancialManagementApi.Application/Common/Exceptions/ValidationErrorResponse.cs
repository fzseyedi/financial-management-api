namespace FinancialManagementApi.Application.Common.Exceptions;

public sealed class ValidationErrorResponse
{
    public string Message { get; init; } = default!;
    public IDictionary<string, string[]> Errors { get; init; } = new Dictionary<string, string[]>();
}