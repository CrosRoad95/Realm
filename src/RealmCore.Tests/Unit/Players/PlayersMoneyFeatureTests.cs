namespace RealmCore.Tests.Unit.Players;

public class PlayersMoneyFeatureTests : RealmUnitTestingBase
{
    [InlineData(0, 0, 1)]
    [InlineData(1, 1, 1)]
    [InlineData(1.234567890, 1.2345, 1)]
    [InlineData(1.234, 123.4, 100)]
    [Theory]
    public void GiveAndTakeMoneyShouldGiveExpectedAmountOfMoney(decimal moneyGiven, decimal expectedAmount, int times)
    {
        var realmTestingServer = CreateServer();
        var player = CreatePlayer();

        decimal moneyAdded = 0;
        decimal moneyTaken = 0;
        player.Money.Added += (that, amount) =>
        {
            moneyAdded += amount;
        };
        player.Money.Taken += (that, amount) =>
        {
            moneyTaken += amount;
        };

        for (int i = 0; i < times; i++)
            player.Money.GiveMoney(moneyGiven);
        player.Money.Amount.Should().Be(expectedAmount);
        moneyAdded.Should().Be(expectedAmount);

        for (int i = 0; i < times; i++)
            player.Money.TakeMoney(moneyGiven);

        player.Money.Amount.Should().Be(0);
        moneyTaken.Should().Be(expectedAmount);
    }

    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(1.234567890, 1.2345)]
    [InlineData(1.234, 1.234)]
    [Theory]
    public void SettingAndGettingMoneyShouldWork(decimal moneySet, decimal expectedMoney)
    {
        var realmTestingServer = CreateServer();
        var player = CreatePlayer();
        player.Money.SetMoneyInternal(1000000);

        player.Money.Amount = moneySet;
        player.Money.Amount.Should().Be(expectedMoney);
    }

    [Fact]
    public void GiveAndTakeMoneyShouldNotAllowNegativeValues()
    {
        var realmTestingServer = CreateServer();
        var player = CreatePlayer();
        player.Money.SetMoneyInternal(1000000);

        Action actGiveMoney = () => { player.Money.GiveMoney(-1); };
        Action actTakeMoney = () => { player.Money.TakeMoney(-1); };

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
        var realmTestingServer = CreateServer();
        var player = CreatePlayer();

        Action actGiveMoney = () => { player.Money.GiveMoney(amount); };
        Action actTakeMoney = () => { player.Money.TakeMoney(amount); };
        Action actSetMoney = () => { player.Money.Amount = amount; };

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
    public async Task TestIfPlayerIsThreadSafe()
    {
        var realmTestingServer = CreateServer();
        var player = CreatePlayer();

        await ParallelHelpers.Run(() =>
        {
            player.Money.GiveMoney(1);
        });

        player.Money.Amount.Should().Be(800);

        await ParallelHelpers.Run(() =>
        {
            player.Money.TakeMoney(1);
        });

        player.Money.Amount.Should().Be(0);
    }

    [Fact]
    public void YouShouldNotBeAbleToTakeMoneyIfThereIsNotEnoughOfThem()
    {
        var realmTestingServer = CreateServer();
        var player = CreatePlayer();
        player.Money.SetMoneyInternal(1000000);

        player.Money.Amount = 15;
        Action take = () => { player.Money.TakeMoney(10); };

        take.Should().NotThrow<GameplayException>();
        take.Should().Throw<GameplayException>()
            .WithMessage("Unable to take money, not enough money.");
        player.Money.Amount.Should().Be(5);
    }

    [Fact]
    public void YouShouldBeAbleToForceTakeMoneyIfThereIsNotEnoughOfThem()
    {
        var realmTestingServer = CreateServer();
        var player = CreatePlayer();
        player.Money.SetMoneyInternal(1000000);

        player.Money.Amount = 15;
        Action take = () => { player.Money.TakeMoney(10, true); };

        take.Should().NotThrow<GameplayException>();
        take.Should().NotThrow<GameplayException>();
        player.Money.Amount.Should().Be(-5);
    }

    [Fact]
    public void YouShouldBeAbleToTransferMoneyBetweenPlayers()
    {
        var realmTestingServer = CreateServer();
        var player1 = CreatePlayer();
        var player2 = CreatePlayer();

        player1.Money.Amount = 15;
        player1.Money.TransferMoney(player2, 10);

        player1.Money.Amount.Should().Be(5);
        player2.Money.Amount.Should().Be(10);
    }

    [Fact]
    public void YouCannotTransferMoreMoneyThanYouHave()
    {
        var realmTestingServer = CreateServer();
        var player1 = CreatePlayer();
        var player2 = CreatePlayer();
        player1.Money.SetMoneyInternal(1000000);
        player2.Money.SetMoneyInternal(1000000);

        player1.Money.Amount = 15;
        Action transfer = () => { player1.Money.TransferMoney(player2, 20, false); };

        transfer.Should().Throw<GameplayException>().WithMessage("Unable to take money, not enough money.");
    }

    [Fact]
    public async Task TransferMoneyShouldBeThreadSafety()
    {
        var realmTestingServer = CreateServer();
        var player1 = CreatePlayer();
        var player2 = CreatePlayer();
        player1.Money.SetMoneyInternal(1000000);

        player1.Money.Amount = 800;

        await ParallelHelpers.Run(() =>
        {
            player1.Money.TransferMoney(player2, 1);
        });

        player1.Money.Amount.Should().Be(0);
        player2.Money.Amount.Should().Be(800);
    }

    [InlineData(10, 5, false, true)]
    [InlineData(10, 5, true, true)]
    [InlineData(10, 15, false, false)]
    [InlineData(10, 15, true, true)]
    [Theory]
    public void HasMoneyShouldReturnExpectedValue(decimal amount, decimal requiredAmount, bool force, bool expectedResult)
    {
        var realmTestingServer = CreateServer();
        var player = CreatePlayer();
        player.Money.SetMoneyInternal(1000000);

        player.Money.Amount = amount;
        player.Money.HasMoney(requiredAmount, force).Should().Be(expectedResult);
    }

    [InlineData(6, 4)]
    [InlineData(20, 10)]
    [Theory]
    public void TryTakeMoneyShouldWork(decimal takenMoney, decimal expectedMoney)
    {
        var realmTestingServer = CreateServer();
        var player = CreatePlayer();
        player.Money.SetMoneyInternal(1000000);

        player.Money.Amount = 10;
        player.Money.TryTakeMoney(takenMoney);
        player.Money.Amount.Should().Be(expectedMoney);
    }

    [Fact]
    public void TryTakeMoneyWithCallbackShouldSucceed()
    {
        var realmTestingServer = CreateServer();
        var player = CreatePlayer();
        player.Money.SetMoneyInternal(1000000);

        player.Money.Amount = 10;
        player.Money.TryTakeMoney(5, () =>
        {
            return true;
        });
        player.Money.Amount.Should().Be(5);
    }

    [Fact]
    public void TryTakeMoneyWithCallbackShouldFail()
    {
        var realmTestingServer = CreateServer();
        var player = CreatePlayer();
        player.Money.SetMoneyInternal(1000000);

        player.Money.Amount = 10;
        player.Money.TryTakeMoney(5, () =>
        {
            return false;
        });
        player.Money.Amount.Should().Be(10);
    }

    [Fact]
    public void TryTakeMoneyWithCallbackShouldFailOnException()
    {
        var realmTestingServer = CreateServer();
        var player = CreatePlayer();
        player.Money.SetMoneyInternal(1000000);

        player.Money.Amount = 10;
        var act = () =>
        {
            player.Money.TryTakeMoney(5, () =>
            {
                throw new InvalidOperationException();
            });
        };

        act.Should().ThrowExactly<InvalidOperationException>();
        player.Money.Amount.Should().Be(10);
    }

    [Fact]
    public async Task TryTakeMoneyWithCallbackAsyncShouldFailOnException()
    {
        var realmTestingServer = CreateServer();
        var player = CreatePlayer();
        player.Money.SetMoneyInternal(1000000);

        player.Money.Amount = 10;
        var act = async () =>
        {
            await player.Money.TryTakeMoneyAsync(5, () =>
            {
                throw new InvalidOperationException();
            });
        };

        await act.Should().ThrowExactlyAsync<InvalidOperationException>();
        player.Money.Amount.Should().Be(10);
    }

    [Fact]
    public async Task TryTakeMoneyWithCallbackAsyncShouldNotFail()
    {
        var realmTestingServer = CreateServer();
        var player = CreatePlayer();
        player.Money.SetMoneyInternal(1000000);

        player.Money.Amount = 100;
        bool success = false;
        var act = async () =>
        {
            success = await player.Money.TryTakeMoneyAsync(5, () =>
            {
                return Task.FromResult(true);
            });
        };

        for (int i = 0; i < 20; i++)
        {
            await act.Should().NotThrowAsync();
            success.Should().BeTrue();
        }
        player.Money.Amount.Should().Be(0);
    }

    [Fact]
    public void TryTakeMoneyWithCallbackShouldFailIfHasNotEnoughMoney()
    {
        var realmTestingServer = CreateServer();
        var player = CreatePlayer();
        player.Money.SetMoneyInternal(1000000);

        player.Money.Amount = 100;
        bool success = player.Money.TryTakeMoney(101, () =>
        {
            return true;
        });

        success.Should().BeFalse();
        player.Money.Amount.Should().Be(100);
    }

    [Fact]
    public async Task TryTakeMoneyWithCallbackAsyncShouldFailIfHasNotEnoughMoney()
    {
        var realmTestingServer = CreateServer();
        var player = CreatePlayer();
        player.Money.SetMoneyInternal(1000000);

        player.Money.Amount = 100;
        bool success = await player.Money.TryTakeMoneyAsync(101, () =>
        {
            return Task.FromResult(true);
        });

        success.Should().BeFalse();
        player.Money.Amount.Should().Be(100);
    }

    [Fact]
    public void TryTakeMoneyWithCallbackShouldShouldNotTakeMoneyIfCallbackReturnFalse()
    {
        var realmTestingServer = CreateServer();
        var player = CreatePlayer();
        player.Money.SetMoneyInternal(1000000);

        player.Money.Amount = 100;
        bool success = player.Money.TryTakeMoney(50, () =>
        {
            return false;
        });

        success.Should().BeFalse();
        player.Money.Amount.Should().Be(100);
    }

    [Fact]
    public async Task TryTakeMoneyWithCallbackAsyncShouldNotTakeMoneyIfCallbackReturnFalse()
    {
        var realmTestingServer = CreateServer();
        var player = CreatePlayer();
        player.Money.SetMoneyInternal(1000000);

        player.Money.Amount = 100;
        bool success = await player.Money.TryTakeMoneyAsync(50, () =>
        {
            return Task.FromResult(false);
        });

        success.Should().BeFalse();
        player.Money.Amount.Should().Be(100);
    }

    [Fact]
    public void YouShouldNotBeAbleToSetMoneyInPlayerEvents()
    {
        var realmTestingServer = CreateServer();
        var player = CreatePlayer();
        player.Money.SetMoneyInternal(1000000);

        var act = () =>
        {
            player.Money.Set += (that, amount) =>
            {
                that.Amount = 50;
            };
            player.Money.Amount = 100;
        };

        act.Should().Throw<LockRecursionException>();
        player.Money.Amount.Should().Be(100);
    }
}
