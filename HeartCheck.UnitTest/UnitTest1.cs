using System;
using HeartCheck.Data;
using HeartCheck.DTOs.Plans;
using HeartCheck.Models;
using HeartCheck.Services;
using Moq;
using Xunit;

namespace HeartCheck.UnitTest;

public class UnitTest1
{
    [Fact]
    public async Task Test1()
    {

    }
}

public class PlanServiceTests
{
    private readonly Mock<IPlanRepository> _planRepositoryMock;
    private readonly Mock<IUserPlanRepository> _userPlanRepositoryMock;
    private readonly PlanService _planService;

    public PlanServiceTests()
    {
        _planRepositoryMock = new Mock<IPlanRepository>();
        _userPlanRepositoryMock = new Mock<IUserPlanRepository>();
        _planService = new PlanService(
            _planRepositoryMock.Object,
            _userPlanRepositoryMock.Object
        );
    }

    [Fact]
    public async Task GetActivePlansAsync_ReturnsAllActivePlans()
    {
        var plans = new List<Plan>
        {
            new Plan
            {
                Id = ObjectId.GenerateNewId(),
                Name = "Basic",
                Description = "Plan basico",
                Price = 0m,
                MaxDevices = 1,
                MeasurementIntervalMinutes = 30,
                IncludesEmergencyCalls = false,
                Status = "active",
                CreatedAt = DateTime.UtcNow
            },
            new Plan
            {
                Id = ObjectId.GenerateNewId(),
                Name = "Premium",
                Description = "Plan premium",
                Price = 19.99m,
                MaxDevices = 3,
                MeasurementIntervalMinutes = 15,
                IncludesEmergencyCalls = true,
                Status = "active",
                CreatedAt = DateTime.UtcNow
            },
            new Plan
            {
                Id = ObjectId.GenerateNewId(),
                Name = "Gold",
                Description = "Plan gold",
                Price = 49.99m,
                MaxDevices = 5,
                MeasurementIntervalMinutes = 5,
                IncludesEmergencyCalls = true,
                Status = "active",
                CreatedAt = DateTime.UtcNow
            }
        };

        _planRepositoryMock
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(plans);

        var result = await _planService.GetActivePlansAsync();

        result.Should().HaveCount(3);
        result.Should().Contain(p => p.Name == "Basic" && p.Price == 0m);
        result.Should().Contain(p => p.Name == "Premium" && p.Price == 19.99m);
        result.Should().Contain(p => p.Name == "Gold" && p.Price == 49.99m);
    }

    [Fact]
    public async Task AssignPlanToUserAsync_NewPlan_CreatesSubscription()
    {
        var userId = ObjectId.GenerateNewId();
        var planId = ObjectId.GenerateNewId();
        var plan = new Plan
        {
            Id = planId,
            Name = "Premium",
            Description = "Plan premium",
            Price = 19.99m,
            MaxDevices = 3,
            MeasurementIntervalMinutes = 15,
            IncludesEmergencyCalls = true,
            Status = "active",
            CreatedAt = DateTime.UtcNow
        };

        _planRepositoryMock
            .Setup(x => x.GetByIdAsync(planId))
            .ReturnsAsync(plan);

        _userPlanRepositoryMock
            .Setup(x => x.GetActiveByUserIdAsync(userId))
            .ReturnsAsync((UserPlan?)null);

        UserPlan createdPlan = null!;
        _userPlanRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<UserPlan>()))
            .Callback<UserPlan>(p => createdPlan = p)
            .Returns(Task.CompletedTask);

        var request = new AssignUserPlanRequest { PlanId = planId.ToString() };
        var result = await _planService.AssignPlanToUserAsync(userId, request);

        result.UserId.Should().Be(userId.ToString());
        result.PlanId.Should().Be(planId.ToString());
        result.PlanName.Should().Be("Premium");
        result.Status.Should().Be("active");
        result.StartDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        _userPlanRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<UserPlan>()), Times.Once);
        _userPlanRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<UserPlan>()), Times.Never);
    }

    [Fact]
    public async Task AssignPlanToUserAsync_CancelsPreviousActivePlan()
    {
        var userId = ObjectId.GenerateNewId();
        var oldPlanId = ObjectId.GenerateNewId();
        var newPlanId = ObjectId.GenerateNewId();

        var oldPlan = new Plan
        {
            Id = oldPlanId,
            Name = "Basic",
            Description = "Plan basico",
            Price = 0m,
            MaxDevices = 1,
            MeasurementIntervalMinutes = 30,
            IncludesEmergencyCalls = false,
            Status = "active",
            CreatedAt = DateTime.UtcNow
        };

        var newPlan = new Plan
        {
            Id = newPlanId,
            Name = "Gold",
            Description = "Plan gold",
            Price = 49.99m,
            MaxDevices = 5,
            MeasurementIntervalMinutes = 5,
            IncludesEmergencyCalls = true,
            Status = "active",
            CreatedAt = DateTime.UtcNow
        };

        var existingActive = new UserPlan
        {
            Id = ObjectId.GenerateNewId(),
            UserId = userId,
            PlanId = oldPlanId,
            StartDate = DateTime.UtcNow.AddDays(-30),
            Status = "active",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        _planRepositoryMock
            .Setup(x => x.GetByIdAsync(newPlanId))
            .ReturnsAsync(newPlan);

        _userPlanRepositoryMock
            .Setup(x => x.GetActiveByUserIdAsync(userId))
            .ReturnsAsync(existingActive);

        UserPlan updatedPlan = null!;
        _userPlanRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<UserPlan>()))
            .Callback<UserPlan>(p => updatedPlan = p)
            .Returns(Task.CompletedTask);

        UserPlan createdPlan = null!;
        _userPlanRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<UserPlan>()))
            .Callback<UserPlan>(p => createdPlan = p)
            .Returns(Task.CompletedTask);

        var request = new AssignUserPlanRequest { PlanId = newPlanId.ToString() };
        var result = await _planService.AssignPlanToUserAsync(userId, request);

        result.PlanName.Should().Be("Gold");
        result.Status.Should().Be("active");

        existingActive.Status.Should().Be("cancelled");
        existingActive.EndDate.Should().NotBeNull();

        _userPlanRepositoryMock.Verify(x => x.UpdateAsync(It.Is<UserPlan>(p => p.Status == "cancelled")), Times.Once);
        _userPlanRepositoryMock.Verify(x => x.CreateAsync(It.Is<UserPlan>(p => p.Status == "active")), Times.Once);
    }

    [Fact]
    public async Task AssignPlanToUserAsync_PlanNotFound_ReturnsNotFound()
    {
        var userId = ObjectId.GenerateNewId();
        var invalidPlanId = ObjectId.GenerateNewId();

        _planRepositoryMock
            .Setup(x => x.GetByIdAsync(invalidPlanId))
            .ReturnsAsync((Plan?)null);

        var request = new AssignUserPlanRequest { PlanId = invalidPlanId.ToString() };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _planService.AssignPlanToUserAsync(userId, request)
        );

        _userPlanRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<UserPlan>()), Times.Never);
    }

    [Fact]
    public async Task GetUserActivePlanAsync_ReturnsActivePlan()
    {
        var userId = ObjectId.GenerateNewId();
        var planId = ObjectId.GenerateNewId();

        var plan = new Plan
        {
            Id = planId,
            Name = "Premium",
            Description = "Plan premium",
            Price = 19.99m,
            MaxDevices = 3,
            MeasurementIntervalMinutes = 15,
            IncludesEmergencyCalls = true,
            Status = "active",
            CreatedAt = DateTime.UtcNow
        };

        var userPlan = new UserPlan
        {
            Id = ObjectId.GenerateNewId(),
            UserId = userId,
            PlanId = planId,
            StartDate = DateTime.UtcNow,
            Status = "active",
            CreatedAt = DateTime.UtcNow
        };

        _userPlanRepositoryMock
            .Setup(x => x.GetActiveByUserIdAsync(userId))
            .ReturnsAsync(userPlan);

        _planRepositoryMock
            .Setup(x => x.GetByIdAsync(planId))
            .ReturnsAsync(plan);

        var result = await _planService.GetUserActivePlanAsync(userId);

        result.Should().NotBeNull();
        result!.PlanName.Should().Be("Premium");
        result.Status.Should().Be("active");
        result.UserId.Should().Be(userId.ToString());
    }

    [Fact]
    public async Task GetUserActivePlanAsync_NoActivePlan_ReturnsNull()
    {
        var userId = ObjectId.GenerateNewId();

        _userPlanRepositoryMock
            .Setup(x => x.GetActiveByUserIdAsync(userId))
            .ReturnsAsync((UserPlan?)null);

        var result = await _planService.GetUserActivePlanAsync(userId);

        result.Should().BeNull();
    }
}