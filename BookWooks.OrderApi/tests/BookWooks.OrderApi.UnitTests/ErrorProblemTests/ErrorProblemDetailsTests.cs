using System.Collections.Generic;
using BookWooks.OrderApi.Web.Shared;
using BookyWooks.SharedKernel.Validation;
using Xunit;

public class ErrorProblemDetailsTests
{
  [Fact]
  public void ToProblemDetails_MapsPropertiesAndExtensions()
  {
    var extensions = new Dictionary<string, IError?>
        {
            { "errorType", null }
        };

    var error = new TestErrorProblemDetails(
        "Test Title",
        "Test Detail",
        418,
        "https://httpstatuses.com/418",
        extensions
    );

    // Act
    var pd = error;

    // Assert
    Assert.Equal("Test Title", pd.Title);
    Assert.Equal("Test Detail", pd.Detail);
    Assert.Equal(418, pd.Status);
    Assert.Equal("https://httpstatuses.com/418", pd.Type);
    Assert.True(pd.Extensions?.ContainsKey("errorType"));
    Assert.Null(pd.Extensions?["errorType"]);
  }

  // Minimal concrete implementation for testing the abstract record
  private record TestErrorProblemDetails(
      string Title,
      string Detail,
      int Status,
      string Type,
      IDictionary<string, IError?>? Extensions = null
  ) : ToHttpResult.ErrorDefinition(Title, Status, "");
}
