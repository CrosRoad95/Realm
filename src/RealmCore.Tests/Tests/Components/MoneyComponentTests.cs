namespace RealmCore.Tests.Tests.Components;

public class MoneyComponentTests
{
    private readonly Entity _entity;
    private readonly Entity _entityB;
    private readonly MoneyComponent _moneyComponent;

    public MoneyComponentTests()
    {
        _entity = new("test");
        _entityB = new("test2");
        _moneyComponent = new(1000000, 4);
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
        _moneyComponent.MoneyAdded += (moneyComponent, amount) =>
        {
            moneyAdded += amount;
        };
        _moneyComponent.MoneyTaken += (moneyComponent, amount) =>
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

        if (amount > 0)
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

    [Fact]
    public void YouShouldNotBeAbleToTakeMoneyIfThereIsNotEnoughOfThem()
    {
        _moneyComponent.Money = 15;
        Action take = () => { _moneyComponent.TakeMoney(10); };

        take.Should().NotThrow<GameplayException>();
        take.Should().Throw<GameplayException>()
            .WithMessage("Unable to take money, not enough money.");
        _moneyComponent.Money.Should().Be(5);
    }

    [Fact]
    public void YouShouldBeAbleToForceTakeMoneyIfThereIsNotEnoughOfThem()
    {
        _moneyComponent.Money = 15;
        Action take = () => { _moneyComponent.TakeMoney(10, true); };

        take.Should().NotThrow<GameplayException>();
        take.Should().NotThrow<GameplayException>();
        _moneyComponent.Money.Should().Be(-5);
    }

    [Fact]
    public void YouShouldBeAbleToTransferMoneyBetweenMoneyComponents()
    {
        var targetMoneyComponent = new MoneyComponent(1000000, 2);
        _entityB.AddComponent(targetMoneyComponent);

        _moneyComponent.Money = 15;
        _moneyComponent.TransferMoney(targetMoneyComponent, 10);

        _moneyComponent.Money.Should().Be(5);
        targetMoneyComponent.Money.Should().Be(10);
    }

    [Fact]
    public void YouCannotTransferMoreMoneyThanYouHave()
    {
        var targetMoneyComponent = new MoneyComponent(1000000, 2);
        _entityB.AddComponent(targetMoneyComponent);

        _moneyComponent.Money = 15;
        Action transfer = () => { _moneyComponent.TransferMoney(targetMoneyComponent, 20, false); };

        transfer.Should().Throw<GameplayException>().WithMessage("Unable to take money, not enough money.");
    }

    [Fact]
    public async Task TransferMoneyShouldBeThreadSafety()
    {
        var targetMoneyComponent = new MoneyComponent(1000000, 2);
        _entityB.AddComponent(targetMoneyComponent);

        _moneyComponent.Money = 800;

        await ParallelHelpers.Run(() =>
        {
            _moneyComponent.TransferMoney(targetMoneyComponent, 1);
        });

        _moneyComponent.Money.Should().Be(0);
        targetMoneyComponent.Money.Should().Be(800);
    }

    [InlineData(10, 5, false, true)]
    [InlineData(10, 5, true, true)]
    [InlineData(10, 15, false, false)]
    [InlineData(10, 15, true, true)]
    [Theory]
    public void HasMoneyShouldReturnExpectedValue(decimal amount, decimal requiredAmount, bool force, bool expectedResult)
    {
        var moneyComponent = new MoneyComponent(1000000, 2);
        _entityB.AddComponent(moneyComponent);

        _moneyComponent.Money = amount;
        _moneyComponent.HasMoney(requiredAmount, force).Should().Be(expectedResult);
    }

    [InlineData(6, 4)]
    [InlineData(20, 10)]
    [Theory]
    public void TryTakeMoneyShouldWork(decimal takenMoney, decimal expectedMoney)
    {
        var moneyComponent = new MoneyComponent(1000000, 2);
        _entityB.AddComponent(moneyComponent);

        _moneyComponent.Money = 10;
        _moneyComponent.TryTakeMoney(takenMoney);
        _moneyComponent.Money.Should().Be(expectedMoney);
    }

    [Fact]
    public void TryTakeMoneyWithCallbackShouldSucceed()
    {
        var moneyComponent = new MoneyComponent(1000000, 2);
        _entityB.AddComponent(moneyComponent);

        _moneyComponent.Money = 10;
        _moneyComponent.TryTakeMoneyWithCallback(5, () =>
        {
            return true;
        });
        _moneyComponent.Money.Should().Be(5);
    }

    [Fact]
    public void TryTakeMoneyWithCallbackShouldFail()
    {
        var moneyComponent = new MoneyComponent(1000000, 2);
        _entityB.AddComponent(moneyComponent);

        _moneyComponent.Money = 10;
        _moneyComponent.TryTakeMoneyWithCallback(5, () =>
        {
            return false;
        });
        _moneyComponent.Money.Should().Be(10);
    }

    [Fact]
    public void TryTakeMoneyWithCallbackShouldFailOnException()
    {
        var moneyComponent = new MoneyComponent(1000000, 2);
        _entityB.AddComponent(moneyComponent);

        _moneyComponent.Money = 10;
        var act = () =>
        {
            _moneyComponent.TryTakeMoneyWithCallback(5, () =>
            {
                throw new InvalidOperationException();
            });
        };

        act.Should().ThrowExactly<InvalidOperationException>();
        _moneyComponent.Money.Should().Be(10);
    }

    [Fact]
    public async Task TryTakeMoneyWithCallbackAsyncShouldFailOnException()
    {
        var moneyComponent = new MoneyComponent(1000000, 2);
        _entityB.AddComponent(moneyComponent);

        _moneyComponent.Money = 10;
        var act = async () =>
        {
            await _moneyComponent.TryTakeMoneyWithCallbackAsync(5, () =>
            {
                throw new InvalidOperationException();
            });
        };

        await act.Should().ThrowExactlyAsync<InvalidOperationException>();
        _moneyComponent.Money.Should().Be(10);
    }

    [Fact]
    public async Task TryTakeMoneyWithCallbackAsyncShouldNotFail()
    {
        var moneyComponent = new MoneyComponent(1000000, 2);
        _entityB.AddComponent(moneyComponent);

        _moneyComponent.Money = 100;
        bool success = false;
        var act = async () =>
        {
            success = await _moneyComponent.TryTakeMoneyWithCallbackAsync(5, () =>
            {
                return Task.FromResult(true);
            });
        };

        for (int i = 0; i < 20; i++)
        {
            await act.Should().NotThrowAsync();
            success.Should().BeTrue();
        }
        _moneyComponent.Money.Should().Be(0);
    }

    [Fact]
    public void TryTakeMoneyWithCallbackShouldFailIfHasNotEnoughMoney()
    {
        var moneyComponent = new MoneyComponent(1000000, 2);
        _entityB.AddComponent(moneyComponent);

        _moneyComponent.Money = 100;
        bool success = _moneyComponent.TryTakeMoneyWithCallback(101, () =>
        {
            return true;
        });

        success.Should().BeFalse();
        _moneyComponent.Money.Should().Be(100);
    }

    [Fact]
    public async Task TryTakeMoneyWithCallbackAsyncShouldFailIfHasNotEnoughMoney()
    {
        var moneyComponent = new MoneyComponent(1000000, 2);
        _entityB.AddComponent(moneyComponent);

        _moneyComponent.Money = 100;
        bool success = await _moneyComponent.TryTakeMoneyWithCallbackAsync(101, () =>
        {
            return Task.FromResult(true);
        });

        success.Should().BeFalse();
        _moneyComponent.Money.Should().Be(100);
    }

    [Fact]
    public void TryTakeMoneyWithCallbackShouldShouldNotTakeMoneyIfCallbackReturnFalse()
    {
        var moneyComponent = new MoneyComponent(1000000, 2);
        _entityB.AddComponent(moneyComponent);

        _moneyComponent.Money = 100;
        bool success = _moneyComponent.TryTakeMoneyWithCallback(50, () =>
        {
            return false;
        });

        success.Should().BeFalse();
        _moneyComponent.Money.Should().Be(100);
    }

    [Fact]
    public async Task TryTakeMoneyWithCallbackAsyncShouldNotTakeMoneyIfCallbackReturnFalse()
    {
        var moneyComponent = new MoneyComponent(1000000, 2);
        _entityB.AddComponent(moneyComponent);

        _moneyComponent.Money = 100;
        bool success = await _moneyComponent.TryTakeMoneyWithCallbackAsync(50, () =>
        {
            return Task.FromResult(false);
        });

        success.Should().BeFalse();
        _moneyComponent.Money.Should().Be(100);
    }

    [Fact]
    public void YouShouldNotBeAbleToSetMoneyInMoneyComponentEvents()
    {
        var moneyComponent = new MoneyComponent(1000000, 2);
        _entityB.AddComponent(moneyComponent);

        var act = () =>
        {
            _moneyComponent.MoneySet += (that, amount) =>
            {
                that.Money = 50;
            };
            _moneyComponent.Money = 100;
        };

        act.Should().Throw<LockRecursionException>();
        _moneyComponent.Money.Should().Be(100);
    }
}
