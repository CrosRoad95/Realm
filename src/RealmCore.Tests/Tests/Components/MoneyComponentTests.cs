namespace RealmCore.Tests.Tests.Components;

public class MoneyComponentTests
{
    [InlineData(0, 0, 1)]
    [InlineData(1, 1, 1)]
    [InlineData(1.234567890, 1.2345, 1)]
    [InlineData(1.234, 123.4, 100)]
    [Theory]
    public void GiveAndTakeMoneyShouldGiveExpectedAmountOfMoney(decimal moneyGiven, decimal expectedAmount, int times)
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var moneyComponent = player.AddComponent(new MoneyComponent(1000000, 4));

        decimal moneyAdded = 0;
        decimal moneyTaken = 0;
        moneyComponent.MoneyAdded += (moneyComponent, amount) =>
        {
            moneyAdded += amount;
        };
        moneyComponent.MoneyTaken += (moneyComponent, amount) =>
        {
            moneyTaken += amount;
        };

        for (int i = 0; i < times; i++)
            moneyComponent.GiveMoney(moneyGiven);
        moneyComponent.Money.Should().Be(expectedAmount);
        moneyAdded.Should().Be(expectedAmount);

        for (int i = 0; i < times; i++)
            moneyComponent.TakeMoney(moneyGiven);

        moneyComponent.Money.Should().Be(0);
        moneyTaken.Should().Be(expectedAmount);
    }

    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(1.234567890, 1.2345)]
    [InlineData(1.234, 1.234)]
    [Theory]
    public void SettingAndGettingMoneyShouldWork(decimal moneySet, decimal expectedMoney)
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var moneyComponent = player.AddComponent(new MoneyComponent(1000000, 4));

        moneyComponent.Money = moneySet;
        moneyComponent.Money.Should().Be(expectedMoney);
    }

    [Fact]
    public void GiveAndTakeMoneyShouldNotAllowNegativeValues()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var moneyComponent = player.AddComponent(new MoneyComponent(1000000, 4));

        Action actGiveMoney = () => { moneyComponent.GiveMoney(-1); };
        Action actTakeMoney = () => { moneyComponent.TakeMoney(-1); };

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
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var moneyComponent = player.AddComponent(new MoneyComponent(1000000, 4));

        Action actGiveMoney = () => { moneyComponent.GiveMoney(amount); };
        Action actTakeMoney = () => { moneyComponent.TakeMoney(amount); };
        Action actSetMoney = () => { moneyComponent.Money = amount; };

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
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var moneyComponent = player.AddComponent(new MoneyComponent(1000000, 4));

        await ParallelHelpers.Run(() =>
        {
            moneyComponent.GiveMoney(1);
        });

        moneyComponent.Money.Should().Be(800);

        await ParallelHelpers.Run(() =>
        {
            moneyComponent.TakeMoney(1);
        });

        moneyComponent.Money.Should().Be(0);
    }

    [Fact]
    public void YouShouldNotBeAbleToTakeMoneyIfThereIsNotEnoughOfThem()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var moneyComponent = player.AddComponent(new MoneyComponent(1000000, 4));

        moneyComponent.Money = 15;
        Action take = () => { moneyComponent.TakeMoney(10); };

        take.Should().NotThrow<GameplayException>();
        take.Should().Throw<GameplayException>()
            .WithMessage("Unable to take money, not enough money.");
        moneyComponent.Money.Should().Be(5);
    }

    [Fact]
    public void YouShouldBeAbleToForceTakeMoneyIfThereIsNotEnoughOfThem()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var moneyComponent = player.AddComponent(new MoneyComponent(1000000, 4));

        moneyComponent.Money = 15;
        Action take = () => { moneyComponent.TakeMoney(10, true); };

        take.Should().NotThrow<GameplayException>();
        take.Should().NotThrow<GameplayException>();
        moneyComponent.Money.Should().Be(-5);
    }

    [Fact]
    public void YouShouldBeAbleToTransferMoneyBetweenMoneyComponents()
    {
        var realmTestingServer = new RealmTestingServer();
        var player1 = realmTestingServer.CreatePlayer();
        var player2 = realmTestingServer.CreatePlayer();
        var moneyComponent = player1.AddComponent(new MoneyComponent(1000000, 2));
        var targetMoneyComponent = player2.AddComponent(new MoneyComponent(1000000, 2));

        moneyComponent.Money = 15;
        moneyComponent.TransferMoney(targetMoneyComponent, 10);

        moneyComponent.Money.Should().Be(5);
        targetMoneyComponent.Money.Should().Be(10);
    }

    [Fact]
    public void YouCannotTransferMoreMoneyThanYouHave()
    {
        var realmTestingServer = new RealmTestingServer();
        var player1 = realmTestingServer.CreatePlayer();
        var player2 = realmTestingServer.CreatePlayer();
        var moneyComponent = player1.AddComponent(new MoneyComponent(1000000, 2));
        var targetMoneyComponent = player2.AddComponent(new MoneyComponent(1000000, 2));

        moneyComponent.Money = 15;
        Action transfer = () => { moneyComponent.TransferMoney(targetMoneyComponent, 20, false); };

        transfer.Should().Throw<GameplayException>().WithMessage("Unable to take money, not enough money.");
    }

    [Fact]
    public async Task TransferMoneyShouldBeThreadSafety()
    {
        var realmTestingServer = new RealmTestingServer();
        var player1 = realmTestingServer.CreatePlayer();
        var player2 = realmTestingServer.CreatePlayer();
        var moneyComponent = player1.AddComponent(new MoneyComponent(1000000, 2));
        var targetMoneyComponent = player2.AddComponent(new MoneyComponent(1000000, 2));

        moneyComponent.Money = 800;

        await ParallelHelpers.Run(() =>
        {
            moneyComponent.TransferMoney(targetMoneyComponent, 1);
        });

        moneyComponent.Money.Should().Be(0);
        targetMoneyComponent.Money.Should().Be(800);
    }

    [InlineData(10, 5, false, true)]
    [InlineData(10, 5, true, true)]
    [InlineData(10, 15, false, false)]
    [InlineData(10, 15, true, true)]
    [Theory]
    public void HasMoneyShouldReturnExpectedValue(decimal amount, decimal requiredAmount, bool force, bool expectedResult)
    {
        var realmTestingServer = new RealmTestingServer();
        var player1 = realmTestingServer.CreatePlayer();
        var moneyComponent = player1.AddComponent(new MoneyComponent(1000000, 2));

        moneyComponent.Money = amount;
        moneyComponent.HasMoney(requiredAmount, force).Should().Be(expectedResult);
    }

    [InlineData(6, 4)]
    [InlineData(20, 10)]
    [Theory]
    public void TryTakeMoneyShouldWork(decimal takenMoney, decimal expectedMoney)
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var moneyComponent = player.AddComponent(new MoneyComponent(1000000, 4));

        moneyComponent.Money = 10;
        moneyComponent.TryTakeMoney(takenMoney);
        moneyComponent.Money.Should().Be(expectedMoney);
    }

    [Fact]
    public void TryTakeMoneyWithCallbackShouldSucceed()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var moneyComponent = player.AddComponent(new MoneyComponent(1000000, 4));
        
        moneyComponent.Money = 10;
        moneyComponent.TryTakeMoneyWithCallback(5, () =>
        {
            return true;
        });
        moneyComponent.Money.Should().Be(5);
    }

    [Fact]
    public void TryTakeMoneyWithCallbackShouldFail()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var moneyComponent = player.AddComponent(new MoneyComponent(1000000, 2));

        moneyComponent.Money = 10;
        moneyComponent.TryTakeMoneyWithCallback(5, () =>
        {
            return false;
        });
        moneyComponent.Money.Should().Be(10);
    }

    [Fact]
    public void TryTakeMoneyWithCallbackShouldFailOnException()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var moneyComponent = player.AddComponent(new MoneyComponent(1000000, 2));

        moneyComponent.Money = 10;
        var act = () =>
        {
            moneyComponent.TryTakeMoneyWithCallback(5, () =>
            {
                throw new InvalidOperationException();
            });
        };

        act.Should().ThrowExactly<InvalidOperationException>();
        moneyComponent.Money.Should().Be(10);
    }

    [Fact]
    public async Task TryTakeMoneyWithCallbackAsyncShouldFailOnException()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var moneyComponent = player.AddComponent(new MoneyComponent(1000000, 4));

        moneyComponent.Money = 10;
        var act = async () =>
        {
            await moneyComponent.TryTakeMoneyWithCallbackAsync(5, () =>
            {
                throw new InvalidOperationException();
            });
        };

        await act.Should().ThrowExactlyAsync<InvalidOperationException>();
        moneyComponent.Money.Should().Be(10);
    }

    [Fact]
    public async Task TryTakeMoneyWithCallbackAsyncShouldNotFail()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var moneyComponent = player.AddComponent(new MoneyComponent(1000000, 4));

        moneyComponent.Money = 100;
        bool success = false;
        var act = async () =>
        {
            success = await moneyComponent.TryTakeMoneyWithCallbackAsync(5, () =>
            {
                return Task.FromResult(true);
            });
        };

        for (int i = 0; i < 20; i++)
        {
            await act.Should().NotThrowAsync();
            success.Should().BeTrue();
        }
        moneyComponent.Money.Should().Be(0);
    }

    [Fact]
    public void TryTakeMoneyWithCallbackShouldFailIfHasNotEnoughMoney()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var moneyComponent = player.AddComponent(new MoneyComponent(1000000, 4));

        moneyComponent.Money = 100;
        bool success = moneyComponent.TryTakeMoneyWithCallback(101, () =>
        {
            return true;
        });

        success.Should().BeFalse();
        moneyComponent.Money.Should().Be(100);
    }

    [Fact]
    public async Task TryTakeMoneyWithCallbackAsyncShouldFailIfHasNotEnoughMoney()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var moneyComponent = player.AddComponent(new MoneyComponent(1000000, 4));

        moneyComponent.Money = 100;
        bool success = await moneyComponent.TryTakeMoneyWithCallbackAsync(101, () =>
        {
            return Task.FromResult(true);
        });

        success.Should().BeFalse();
        moneyComponent.Money.Should().Be(100);
    }

    [Fact]
    public void TryTakeMoneyWithCallbackShouldShouldNotTakeMoneyIfCallbackReturnFalse()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var moneyComponent = player.AddComponent(new MoneyComponent(1000000, 4));

        moneyComponent.Money = 100;
        bool success = moneyComponent.TryTakeMoneyWithCallback(50, () =>
        {
            return false;
        });

        success.Should().BeFalse();
        moneyComponent.Money.Should().Be(100);
    }

    [Fact]
    public async Task TryTakeMoneyWithCallbackAsyncShouldNotTakeMoneyIfCallbackReturnFalse()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var moneyComponent = player.AddComponent(new MoneyComponent(1000000, 4));

        moneyComponent.Money = 100;
        bool success = await moneyComponent.TryTakeMoneyWithCallbackAsync(50, () =>
        {
            return Task.FromResult(false);
        });

        success.Should().BeFalse();
        moneyComponent.Money.Should().Be(100);
    }

    [Fact]
    public void YouShouldNotBeAbleToSetMoneyInMoneyComponentEvents()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var moneyComponent = player.AddComponent(new MoneyComponent(1000000, 2));

        var act = () =>
        {
            moneyComponent.MoneySet += (that, amount) =>
            {
                that.Money = 50;
            };
            moneyComponent.Money = 100;
        };

        act.Should().Throw<LockRecursionException>();
        moneyComponent.Money.Should().Be(100);
    }
}
