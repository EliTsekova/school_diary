namespace school_diary.school_diary_tests.Tests.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using school_diary.Controllers;
using school_diary.Dtos;
using school_diary.Services;

public class StatisticsControllerTests
{
    [Fact]
    public async Task Overview_ReturnsView_WithGlobalStatistics_WhenSchoolIdIsNull()
    {
        var statisticsService = new Mock<IAdminStatisticsService>();

        var globalStats = new List<SubjectAverageDto>
        {
            new SubjectAverageDto("Math", 5.50),
            new SubjectAverageDto("History", 4.75)
        };

        statisticsService
            .Setup(x => x.GetGlobalSubjectAveragesAsync())
            .ReturnsAsync(globalStats);

        var controller = new StatisticsController(statisticsService.Object);

        var result = await controller.Overview(null);

        Assert.IsType<ViewResult>(result);

        var stats = Assert.IsType<List<SubjectAverageDto>>(
            (object)controller.ViewBag.SubjectAverages);

        Assert.Equal(2, stats.Count);
        Assert.Equal("Math", stats[0].SubjectName);

        statisticsService.Verify(
            x => x.GetGlobalSubjectAveragesAsync(),
            Times.Once);

        statisticsService.Verify(
            x => x.GetSubjectAveragesBySchoolAsync(It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task Overview_ReturnsView_WithSchoolStatistics_WhenSchoolIdIsProvided()
    {
        var statisticsService = new Mock<IAdminStatisticsService>();

        var schoolStats = new List<SubjectAverageDto>
        {
            new SubjectAverageDto("Biology", 5.80)
        };

        statisticsService
            .Setup(x => x.GetSubjectAveragesBySchoolAsync(1))
            .ReturnsAsync(schoolStats);

        var controller = new StatisticsController(statisticsService.Object);

        var result = await controller.Overview(1);

        Assert.IsType<ViewResult>(result);

        var stats = Assert.IsType<List<SubjectAverageDto>>(
            (object)controller.ViewBag.SubjectAverages);

        Assert.Single(stats);
        Assert.Equal("Biology", stats[0].SubjectName);

        statisticsService.Verify(
            x => x.GetSubjectAveragesBySchoolAsync(1),
            Times.Once);

        statisticsService.Verify(
            x => x.GetGlobalSubjectAveragesAsync(),
            Times.Never);
    }
}