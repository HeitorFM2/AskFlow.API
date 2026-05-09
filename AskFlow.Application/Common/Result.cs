namespace AskFlow.Application.Common
{
    public class Result
    {
        protected Result(bool isSuccess, string? error, ResultType type)
        {
            IsSuccess = isSuccess;
            Error = error;
            Type = type;
        }

        public bool IsSuccess { get; }
        public string? Error { get; }
        public ResultType Type { get; }

        public static Result Success() => new(true, null, ResultType.Ok);
        public static Result NotFound(string error) => new(false, error, ResultType.NotFound);
        public static Result Unauthorized(string error) => new(false, error, ResultType.Unauthorized);
        public static Result Invalid(string error) => new(false, error, ResultType.Invalid);
        public static Result Failure(string error) => new(false, error, ResultType.Failure);
    }

    public sealed class Result<T> : Result
    {
        private Result(T value) : base(true, null, ResultType.Ok) => Value = value;
        private Result(string error, ResultType type) : base(false, error, type) => Value = default;

        public T? Value { get; }

        public static Result<T> Success(T value) => new(value);
        public new static Result<T> NotFound(string error) => new(error, ResultType.NotFound);
        public new static Result<T> Unauthorized(string error) => new(error, ResultType.Unauthorized);
        public new static Result<T> Invalid(string error) => new(error, ResultType.Invalid);
        public new static Result<T> Failure(string error) => new(error, ResultType.Failure);
    }
}
