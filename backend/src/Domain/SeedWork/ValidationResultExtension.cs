using FluentValidation.Results;
using Tg4.Infrastructure.Functional;

namespace Domain.SeedWork
{
    public static class ValidationResultExtension
    {
        public static Result<T> ToResult<T>(this ValidationResult validation, T data)
        {
            return !validation.IsValid ? 
                Result.Fail<T>(string.Join(',', validation.Errors)) : 
                Result.Ok(data);
        }
    }
}