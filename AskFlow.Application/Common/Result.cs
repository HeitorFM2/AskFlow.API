namespace AskFlow.Application.Common
{
    public class Result
    {
        protected Result(bool isSuccess, string? errorCode, string? error, ResultType type)
        {
            IsSuccess = isSuccess;
            ErrorCode = errorCode;
            Error = error;
            Type = type;
        }

        public bool IsSuccess { get; }
        public string? ErrorCode { get; }
        public string? Error { get; }
        public ResultType Type { get; }

        public static Result Success() => new(true, null, null, ResultType.Ok);
        public static Result NotFound(string code, string error) => new(false, code, error, ResultType.NotFound);
        public static Result Unauthorized(string code, string error) => new(false, code, error, ResultType.Unauthorized);
        public static Result Invalid(string code, string error) => new(false, code, error, ResultType.Invalid);
        public static Result Failure(string code, string error) => new(false, code, error, ResultType.Failure);
    }

    public sealed class Result<T> : Result
    {
        private Result(T value) : base(true, null, null, ResultType.Ok) => Value = value;
        private Result(string code, string error, ResultType type) : base(false, code, error, type) => Value = default;

        public T? Value { get; }

        public static Result<T> Success(T value) => new(value);
        public new static Result<T> NotFound(string code, string error) => new(code, error, ResultType.NotFound);
        public new static Result<T> Unauthorized(string code, string error) => new(code, error, ResultType.Unauthorized);
        public new static Result<T> Invalid(string code, string error) => new(code, error, ResultType.Invalid);
        public new static Result<T> Failure(string code, string error) => new(code, error, ResultType.Failure);
    }
}
