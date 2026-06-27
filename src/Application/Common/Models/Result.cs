namespace Application.Common.Models;

public class Result<T>
{
    public bool IsSuccess{get;}
    public T? Value {get;}
    public List<string> Errors{get;} = new();
    public bool IsFailure => !IsSuccess;

    private Result(bool isSuccess, T? value, List<string>? errors)
    {
        IsSuccess = isSuccess;
        Value = value;
        if(errors != null) Errors = errors;
    }

    public static Result<T> Success(T value) => new(true,value,null);
    public static Result<T> Failure(string error) => new(false, default,new List<string>{error});
    public static Result<T> Failure(List<string> errors) => new(false,default, errors);

    public static implicit operator Result<T>(T value) => Success(value);

}