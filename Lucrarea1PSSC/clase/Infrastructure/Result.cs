using System;

namespace Lucrarea1PSSC.clase.Infrastructure
{
    /// <summary>
    /// Railway-oriented programming result type
    /// Represents either success with a value or failure with errors
    /// Eliminates the need for tuples like (bool, T, string)
    /// </summary>
    public abstract record Result<TSuccess, TFailure>
    {
        private Result() { } // Sealed hierarchy

        public sealed record Success(TSuccess Value) : Result<TSuccess, TFailure>;
        public sealed record Failure(TFailure Error) : Result<TSuccess, TFailure>;

        public bool IsSuccess => this is Success;
        public bool IsFailure => this is Failure;

        public TSuccess GetValueOrThrow() => this switch
        {
            Success(var value) => value,
            Failure(var error) => throw new InvalidOperationException($"Cannot get value from failure: {error}"),
            _ => throw new InvalidOperationException("Unknown result state")
        };

        public TFailure GetErrorOrThrow() => this is Failure(var error) 
            ? error 
            : throw new InvalidOperationException("Cannot get error from success");

        /// <summary>
        /// Map transforms the success value without unwrapping
        /// Failures pass through unchanged
        /// </summary>
        public Result<TNewSuccess, TFailure> Map<TNewSuccess>(Func<TSuccess, TNewSuccess> mapper) => this switch
        {
            Success(var value) => new Result<TNewSuccess, TFailure>.Success(mapper(value)),
            Failure(var error) => new Result<TNewSuccess, TFailure>.Failure(error),
            _ => throw new InvalidOperationException("Unknown result state")
        };

        /// <summary>
        /// Bind chains operations that return Result
        /// Short-circuits on first failure (railway switching)
        /// </summary>
        public Result<TNewSuccess, TFailure> Bind<TNewSuccess>(
            Func<TSuccess, Result<TNewSuccess, TFailure>> binder) => this switch
        {
            Success(var value) => binder(value),
            Failure(var error) => new Result<TNewSuccess, TFailure>.Failure(error),
            _ => throw new InvalidOperationException("Unknown result state")
        };

        public static implicit operator Result<TSuccess, TFailure>(TSuccess success) => 
            new Success(success);
        
        public static implicit operator Result<TSuccess, TFailure>(TFailure failure) => 
            new Failure(failure);
    }

    /// <summary>
    /// Simplified version for domain operations with DomainError
    /// </summary>
    public abstract record Result<T>
    {
        private Result() { }

        public sealed record Success(T Value) : Result<T>;
        public sealed record Failure(DomainError Error) : Result<T>;

        public bool IsSuccess => this is Success;
        public bool IsFailure => this is Failure;

        public T GetValueOrThrow() => this switch
        {
            Success(var value) => value,
            Failure(var error) => throw new InvalidOperationException($"Cannot get value from failure: {error.Message}"),
            _ => throw new InvalidOperationException("Unknown result state")
        };

        public DomainError GetErrorOrThrow() => this is Failure(var error)
            ? error
            : throw new InvalidOperationException("Cannot get error from success");

        public Result<TNew> Map<TNew>(Func<T, TNew> mapper) => this switch
        {
            Success(var value) => new Result<TNew>.Success(mapper(value)),
            Failure(var error) => new Result<TNew>.Failure(error),
            _ => throw new InvalidOperationException("Unknown result state")
        };

        public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> binder) => this switch
        {
            Success(var value) => binder(value),
            Failure(var error) => new Result<TNew>.Failure(error),
            _ => throw new InvalidOperationException("Unknown result state")
        };

        public static implicit operator Result<T>(T success) => new Success(success);
        public static implicit operator Result<T>(DomainError failure) => new Failure(failure);
    }

    /// <summary>
    /// Domain-specific error with code and message
    /// Replaces primitive string error messages
    /// </summary>
    public sealed record DomainError(string Code, string Message, string? Details = null)
    {
        public static DomainError Create(string code, string message, string? details = null) =>
            new(code, message, details);

        public static DomainError ValidationFailed(string message, string? details = null) =>
            new("VALIDATION_FAILED", message, details);

        public static DomainError NotFound(string entityType, string id) =>
            new("NOT_FOUND", $"{entityType} with ID '{id}' was not found");

        public static DomainError InvalidState(string currentState, string operation) =>
            new("INVALID_STATE", $"Cannot perform '{operation}' in state '{currentState}'");

        public static DomainError BusinessRuleViolation(string rule, string? details = null) =>
            new("BUSINESS_RULE_VIOLATION", rule, details);

        public static DomainError InvariantViolation(string invariant, string? details = null) =>
            new("INVARIANT_VIOLATION", invariant, details);

        public override string ToString() => Details != null 
            ? $"[{Code}] {Message} - {Details}" 
            : $"[{Code}] {Message}";
    }

    /// <summary>
    /// Unit type for operations that don't return a value
    /// Replaces void in Result context
    /// </summary>
    public readonly struct Unit
    {
        public static readonly Unit Default = new();
        
        public static implicit operator Unit(ValueTuple _) => Default;
    }
}
