namespace BookyWooks.SharedKernel.Validation;
public abstract record Error(string Message): IError;
public interface IError; // Error Marker class
