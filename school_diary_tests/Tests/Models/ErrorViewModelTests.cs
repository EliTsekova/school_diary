namespace school_diary.school_diary_tests.Tests.Models;
using System.ComponentModel.DataAnnotations;
using school_diary.Models;


public class ErrorViewModelTests
{
    private static IList<ValidationResult> ValidateModel(object model)
    {
        var context = new ValidationContext(model);

        var results = new List<ValidationResult>();

        Validator.TryValidateObject(
            model,
            context,
            results,
            true);

        return results;
    }

    [Fact]
    public void ErrorViewModel_IsValid_WhenRequestIdLengthIsWithinLimit()
    {
        var model = new ErrorViewModel
        {
            RequestId = "REQ-123"
        };

        var results = ValidateModel(model);

        Assert.Empty(results);
    }

    [Fact]
    public void ErrorViewModel_IsInvalid_WhenRequestIdIsTooLong()
    {
        var model = new ErrorViewModel
        {
            RequestId = new string('A', 101)
        };

        var results = ValidateModel(model);

        Assert.NotEmpty(results);
    }

    [Fact]
    public void ShowRequestId_ReturnsFalse_WhenRequestIdIsNull()
    {
        var model = new ErrorViewModel
        {
            RequestId = null
        };

        Assert.False(model.ShowRequestId);
    }

    [Fact]
    public void ShowRequestId_ReturnsFalse_WhenRequestIdIsEmpty()
    {
        var model = new ErrorViewModel
        {
            RequestId = ""
        };

        Assert.False(model.ShowRequestId);
    }

    [Fact]
    public void ShowRequestId_ReturnsTrue_WhenRequestIdExists()
    {
        var model = new ErrorViewModel
        {
            RequestId = "REQ-123"
        };

        Assert.True(model.ShowRequestId);
    }
}