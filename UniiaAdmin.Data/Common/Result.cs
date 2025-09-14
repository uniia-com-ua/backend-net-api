namespace UniiaAdmin.Data.Common
{
    public class Result<T>
    {
        public T? Value { get; }
        public Exception? Error { get; }
        public bool IsSuccess => Error is null;

        private Result(T? value, Exception? error)
        {
            Value = value;
            Error = error;
        }

        public static Result<T> Success(T value) => new(value, null);
        public static Result<T> Failure(Exception error) => new(default, error);
        public static Result<T> SuccessNoContent() => new(default, null);
    }
}
