﻿using FluentAssertions;
using Realm.Common.Exceptions;
using Realm.Configuration;
using Realm.Domain;
using Realm.Domain.Components.Players;
using Realm.Tests.Helpers;

namespace Realm.Tests.Tests;

public class MoneyComponentTests
{
    private readonly Entity _entity;
    private readonly MoneyComponent _moneyComponent;

    public MoneyComponentTests()
    {
        var services = new ServiceCollection();
        services.AddSingleton<RealmConfigurationProvider>(new TestConfigurationProvider());
        _entity = new(services.BuildServiceProvider(), "test", "test");
        _moneyComponent = new();
        _entity.AddComponent(_moneyComponent);
    }

    [InlineData(0, 0, 1)]
    [InlineData(1, 1, 1)]
    [InlineData(1.234567890, 1.2345, 1)]
    [InlineData(1.234, 123.4, 100)]
    [Theory]
    public void GiveAndTakeMoneyShouldGiveExpectedAmountOfMoney(decimal moneyGiven, decimal expectedAmount, int times)
    {
        decimal moneyAdded = 0;
        decimal moneyTaken = 0;
        _moneyComponent.MoneyAdded += amount =>
        {
            moneyAdded += amount;
        };
        _moneyComponent.MoneyTaken += amount =>
        {
            moneyTaken += amount;
        };

        for (int i = 0; i < times; i++)
            _moneyComponent.GiveMoney(moneyGiven);
        _moneyComponent.Money.Should().Be(expectedAmount);
        moneyAdded.Should().Be(expectedAmount);

        for (int i = 0; i < times; i++)
            _moneyComponent.TakeMoney(moneyGiven);

        _moneyComponent.Money.Should().Be(0);
        moneyTaken.Should().Be(expectedAmount);
    }

    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(1.234567890, 1.2345)]
    [InlineData(1.234, 1.234)]
    [Theory]
    public void SettingAndGettingMoneyShouldWork(decimal moneySet, decimal expectedMoney)
    {
        _moneyComponent.Money = moneySet;
        _moneyComponent.Money.Should().Be(expectedMoney);
    }

    [Fact]
    public void GiveAndTakeMoneyShouldNotAllowNegativeValues()
    {
        Action actGiveMoney = () => { _moneyComponent.GiveMoney(-1); };
        Action actTakeMoney = () => { _moneyComponent.TakeMoney(-1); };

        actGiveMoney.Should().Throw<GameplayException>()
            .WithMessage("Unable to give money, amount can not get negative.");
        actTakeMoney.Should().Throw<GameplayException>()
            .WithMessage("Unable to take money, amount can not get negative.");
    }

    [InlineData(10000000)]
    [InlineData(-10000000)]
    [Theory]
    public void YouCanNotGiveTakeOrSetMoneyBeyondLimit(decimal amount)
    {
        Action actGiveMoney = () => { _moneyComponent.GiveMoney(amount); };
        Action actTakeMoney = () => { _moneyComponent.TakeMoney(amount); };
        Action actSetMoney = () => { _moneyComponent.Money = amount; };

        if(amount > 0)
        {
            actGiveMoney.Should().Throw<GameplayException>()
                .WithMessage("Unable to give money beyond limit.");
            actTakeMoney.Should().Throw<GameplayException>()
                .WithMessage("Unable to take money beyond limit.");
        }
        actSetMoney.Should().Throw<GameplayException>()
            .WithMessage("Unable to set money beyond limit.");
    }

    [Fact]
    public async Task TestIfMoneyComponentIsThreadSafe()
    {
        await ParallelHelpers.Run(() =>
        {
            _moneyComponent.GiveMoney(1);
        });

        _moneyComponent.Money.Should().Be(800);

        await ParallelHelpers.Run(() =>
        {
            _moneyComponent.TakeMoney(1);
        });

        _moneyComponent.Money.Should().Be(0);
    }
}