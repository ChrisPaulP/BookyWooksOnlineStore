namespace BookyWooks.SharedKernel.Specification;
public abstract class BaseSpecification<T> : ISpecification<T>
{
    protected BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }
    protected BaseSpecification() { }
    public Expression<Func<T, bool>> Criteria { get; }
    public List<Expression<Func<T, object>>> Includes { get; } = new List<Expression<Func<T, object>>>();
    public List<string> IncludeStrings { get; } = new List<string>();
    public Expression<Func<T, object>> OrderBy { get; private set; }
    public Expression<Func<T, object>> OrderByDescending { get; private set; }
    public Expression<Func<T, object>> GroupBy { get; private set; }

    public int Take { get; private set; }
    public int Skip { get; private set; }
    public bool IsPagingEnabled { get; private set; } = false;
    public string? CacheKey { get; set; }
    public bool CacheEnabled { get; set; }

    protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    protected virtual void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    protected virtual void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    protected virtual void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    protected virtual void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
    {
        OrderByDescending = orderByDescendingExpression;
    }

    protected virtual void ApplyGroupBy(Expression<Func<T, object>> groupByExpression)
    {
        GroupBy = groupByExpression;
    }

    protected virtual void EnableCache(string specificationName, params object[] args)
    {
        CacheKey = $"{specificationName}-{string.Join("-", args)}";
        CacheEnabled = true;
    }
}

//Notes on  Expression Trees: 
//An expression tree is a representation of code in a tree-like data structure, where each node is an expression.Expression trees are primarily used to represent code in a way that can be inspected, modified, or executed at runtime.

//In the example you provided:

//protected virtual void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
//{
//    OrderBy = orderByExpression;
//}

//Expression<Func<T, object>> is an expression tree that represents a function taking a parameter of type T and returning an object.
//Why Use Expression Trees?
//Deferred Execution: They allow for deferred execution, which means the expression is not executed when it's defined. Instead, it's stored as a data structure(the tree) that can be executed later.
//Dynamic Queries: They enable the construction of dynamic queries.For example, when using LINQ (Language-Integrated Query), expression trees can be used to build queries at runtime based on different conditions.

//Inspection and Modification: Expression trees allow you to inspect and modify code.Since the expression is represented as a tree structure, you can traverse and change it, which is useful for creating or optimizing queries dynamically.
//ORMs (Object-Relational Mappers): Tools like Entity Framework use expression trees to convert LINQ queries into SQL queries.They inspect the expression tree to understand which properties and methods are being used, and then generate the corresponding SQL commands.

//Example of an Expression Tree
//Here’s a simple example:

//Expression<Func<int, bool>> expr = num => num > 5;

//Expression<Func<int, bool>> is an expression tree that represents a function taking an int and returning a bool.
//The expression num => num > 5 is stored as a tree:
//The root is the lambda expression.
//It has a parameter num.
//The body of the lambda is a greater-than comparison.