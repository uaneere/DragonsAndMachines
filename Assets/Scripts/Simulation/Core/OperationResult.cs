namespace DragonsAndMachines.Core
{
    public readonly struct OperationResult
    {
        public bool Success { get; }
        public string ErrorCode { get; }
        public string Message { get; }

        private OperationResult(bool success, string errorCode, string message)
        {
            Success = success;
            ErrorCode = errorCode;
            Message = message;
        }

        public static OperationResult Ok(string message = null) => new OperationResult(true, null, message);

        public static OperationResult Fail(string errorCode, string message) =>
            new OperationResult(false, errorCode, message);

        public override string ToString() => Success ? (Message ?? "OK") : $"{ErrorCode}: {Message}";
    }
}
