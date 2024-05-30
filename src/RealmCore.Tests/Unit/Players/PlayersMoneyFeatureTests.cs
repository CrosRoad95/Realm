using RealmCore.Server.Modules.Players.Money;

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
        var server = CreateServer();
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
            player.Money.Give(moneyGiven);
        player.Money.Amount.Should().Be(expectedAmount);
        moneyAdded.Should().Be(expectedAmount);

        for (int i = 0; i < times; i++)
            player.Money.Take(moneyGiven);

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
        var player = CreateServerWithOnePlayer();

        var money = player.Money;
        money.SetInternal(1000000);

        money.Amount = moneySet;
        money.Amount.Should().Be(expectedMoney);
    }

    [Fact]
    public void GiveAndTakeMoneyShouldNotAllowNegativeValues()
    {
        var server = CreateServer();
        var player = CreatePlayer();
        player.Money.SetInternal(1000000);

        Action actGiveMoney = () => { player.Money.Give(-1); };
        Action actTakeMoney = () => { player.Money.Take(-1); };

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
        var server = CreateServer();
        var player = CreatePlayer();

        Action actGiveMoney = () => { player.Money.Give(amount); };
        Action actTakeMoney = () => { player.Money.Take(amount); };
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
        var server = CreateServer();
        var player = CreatePlayer();

        await ParallelHelpers.Run(() =>
        {
            player.Money.Give(1);
        });

        player.Money.Amount.Should().Be(800);

        await ParallelHelpers.Run(() =>
        {
            player.Money.Take(1);
        });

        player.Money.Amount.Should().Be(0);
    }

    [Fact]
    public void YouShouldNotBeAbleToTakeMoneyIfThereIsNotEnoughOfThem()
    {
        var server = CreateServer();
        var player = CreatePlayer();
        player.Money.SetInternal(1000000);

        player.Money.Amount = 15;
        Action take = () => { player.Money.Take(10); };

        take.Should().NotThrow<GameplayException>();
        take.Should().Throw<GameplayException>()
            .WithMessage("Unable to take money, not enough money.");
        player.Money.Amount.Should().Be(5);
    }

    [Fact]
    public void YouShouldBeAbleToForceTakeMoneyIfThereIsNotEnoughOfThem()
    {
        var server = CreateServer();
        var player = CreatePlayer();
        player.Money.SetInternal(1000000);

        player.Money.Amount = 15;
        Action take = () => { player.Money.Take(10, true); };

        take.Should().NotThrow<GameplayException>();
        take.Should().NotThrow<GameplayException>();
        player.Money.Amount.Should().Be(-5);
    }

    [Fact]
    public void YouShouldBeAbleToTransferMoneyBetweenPlayers()
    {
        var server = CreateServer();
        var player1 = CreatePlayer();
        var player2 = CreatePlayer();

        player1.Money.Amount = 15;
        player1.Money.Transfer(player2.Money, 10);

        player1.Money.Amount.Should().Be(5);
        player2.Money.Amount.Should().Be(10);
    }

    [Fact]
    public void YouCannotTransferMoreMoneyThanYouHave()
    {
        var server = CreateServer();
        var player1 = CreatePlayer();
        var player2 = CreatePlayer();
        player1.Money.SetInternal(1000000);
        player2.Money.SetInternal(1000000);

        player1.Money.Amount = 15;
        Action transfer = () => { player1.Money.Transfer(player2.Money, 20, false); };

        transfer.Should().Throw<GameplayException>().WithMessage("Unable to take money, not enough money.");
    }

    [Fact]
    public async Task TransferMoneyShouldBeThreadSafety()
    {
        var server = CreateServer();
        var player1 = CreatePlayer();
        var player2 = CreatePlayer();
        player1.Money.SetInternal(1000000);

        player1.Money.Amount = 800;

        await ParallelHelpers.Run(() =>
        {
            player1.Money.Transfer(player2.Money, 1);
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
        var server = CreateServer();
        var player = CreatePlayer();
        player.Money.SetInternal(1000000);

        player.Money.Amount = amount;
        player.Money.Has(requiredAmount, force).Should().Be(expectedResult);
    }

    [InlineData(6, 4)]
    [InlineData(20, 10)]
    [Theory]
    public void TryTakeMoneyShouldWork(decimal takenMoney, decimal expectedMoney)
    {
        var server = CreateServer();
        var player = CreatePlayer();
        player.Money.SetInternal(1000000);

        player.Money.Amount = 10;
        player.Money.TryTake(takenMoney);
        player.Money.Amount.Should().Be(expectedMoney);
    }

    [Fact]
    public void TryTakeMoneyWithCallbackShouldSucceed()
    {
        var server = CreateServer();
        var player = CreatePlayer();
        player.Money.SetInternal(1000000);

        player.Money.Amount = 10;
        player.Money.TryTake(5, () =>
        {
            return true;
        });
        player.Money.Amount.Should().Be(5);
    }

    [Fact]
    public void TryTakeMoneyWithCallbackShouldFail()
    {
        var server = CreateServer();
        var player = CreatePlayer();
        player.Money.SetInternal(1000000);

        player.Money.Amount = 10;
        player.Money.TryTake(5, () =>
        {
            return false;
        });
        player.Money.Amount.Should().Be(10);
    }

    [Fact]
    public void TryTakeMoneyWithCallbackShouldFailOnException()
    {
        var server = CreateServer();
        var player = CreatePlayer();
        server.GetRequiredService<PlayerMoneyHostedService>().StartAsync(CancellationToken.None).Wait();
        player.Money.SetInternal(1000000);

        player.Money.Amount = 10;
        var act = () =>
        {
            player.Money.TryTake(5, () =>
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
        var server = CreateServer();
        var player = CreatePlayer();
        player.Money.SetInternal(1000000);

        player.Money.Amount = 10;
        var act = async () =>
        {
            await player.Money.TryTakeAsync(5, () =>
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
        var server = CreateServer();
        var player = CreatePlayer();
        player.Money.SetInternal(1000000);

        player.Money.Amount = 100;
        bool success = false;
        var act = async () =>
        {
            success = await player.Money.TryTakeAsync(5, () =>
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
        var server = CreateServer();
        var player = CreatePlayer();
        player.Money.SetInternal(1000000);

        player.Money.Amount = 100;
        bool success = player.Money.TryTake(101, () =>
        {
            return true;
        });

        success.Should().BeFalse();
        player.Money.Amount.Should().Be(100);
    }

    [Fact]
    public async Task TryTakeMoneyWithCallbackAsyncShouldFailIfHasNotEnoughMoney()
    {
        var server = CreateServer();
        var player = CreatePlayer();
        player.Money.SetInternal(1000000);

        player.Money.Amount = 100;
        bool success = await player.Money.TryTakeAsync(101, () =>
        {
            return Task.FromResult(true);
        });

        success.Should().BeFalse();
        player.Money.Amount.Should().Be(100);
    }

    [Fact]
    public void TryTakeMoneyWithCallbackShouldShouldNotTakeMoneyIfCallbackReturnFalse()
    {
        var server = CreateServer();
        var player = CreatePlayer();
        player.Money.SetInternal(1000000);

        player.Money.Amount = 100;
        bool success = player.Money.TryTake(50, () =>
        {
            return false;
        });

        success.Should().BeFalse();
        player.Money.Amount.Should().Be(100);
    }

    [Fact]
    public async Task TryTakeMoneyWithCallbackAsyncShouldNotTakeMoneyIfCallbackReturnFalse()
    {
        var server = CreateServer();
        var player = CreatePlayer();
        player.Money.SetInternal(1000000);

        player.Money.Amount = 100;
        bool success = await player.Money.TryTakeAsync(50, () =>
        {
            return Task.FromResult(false);
        });

        success.Should().BeFalse();
        player.Money.Amount.Should().Be(100);
    }

    [Fact]
    public void SettingMoneyInsideEventsShouldWork()
    {
        using var _ = new AssertionScope();

        var server = CreateServer();
        var player = CreatePlayer();
        player.Money.SetInternal(1000000);

        var act = () =>
        {
            player.Money.Set += (that, amount) =>
            {
                that.Amount = 50;
            };
            player.Money.Amount = 100;
        };

        act.Should().NotThrow();
        player.Money.Amount.Should().Be(50);
    }

    [Fact]
    public void UsingMoneyComponentShouldIncreaseVersions()
    {
        using var _ = new AssertionScope();

        var server = CreateServer();
        var player1 = CreatePlayer();
        var player2 = CreatePlayer();
        var money = player1.Money;
        var user1 = player1.User;
        var expectedVersion = 0;

        user1.GetVersion().Should().Be(0);
        money.SetInternal(50);
        user1.GetVersion().Should().Be(expectedVersion, "SetMoneyInternal(50)");

        money.Give(50);
        user1.GetVersion().Should().Be(++expectedVersion, "Give(50)");

        money.Take(5);
        user1.GetVersion().Should().Be(++expectedVersion, "Take(5)");

        money.TryTake(500);
        user1.GetVersion().Should().Be(expectedVersion, "TryTake(500)");

        money.TryTake(5);
        user1.GetVersion().Should().Be(++expectedVersion, "TryTake(5)");

        money.TryTake(5, () => true);
        user1.GetVersion().Should().Be(++expectedVersion, "TryTake(5, () => true)");

        money.TryTake(5, () => false);
        user1.GetVersion().Should().Be(expectedVersion, "TryTake(5, () => false)");

        money.TryTakeAsync(5, () => Task.FromResult(true)).Wait();
        user1.GetVersion().Should().Be(++expectedVersion, "TryTakeAsync(5, () => Task.FromResult(true)).Wait()");

        money.TryTakeAsync(5, () => Task.FromResult(false)).Wait();
        user1.GetVersion().Should().Be(expectedVersion, "TryTakeAsync(5, () => Task.FromResult(false)).Wait()");
    }

    [Fact]
    public void GivingAndTakingZeroMoneyShouldNotRaiseAnyEvent()
    {
        using var _ = new AssertionScope();

        var server = CreateServer();
        var player = CreatePlayer();
        var money = player.Money;

        using var monitor1 = money.Monitor();

        money.Give(0);
        money.Take(0);
        money.Take(0, true);
        money.TryTake(0);
        money.TryTake(0, true);
        money.TryTake(0, () => true);
        money.TryTake(0, () => false);
        money.TryTakeAsync(0, () => Task.FromResult(true)).Wait();
        money.TryTakeAsync(0, () => Task.FromResult(false)).Wait();

        player.User.GetVersion().Should().Be(0);
        monitor1.OccurredEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task AsyncMethodCallsInsideTryTakeAsyncShouldWork()
    {
        using var _ = new AssertionScope();

        var server = CreateServer();
        var player = CreatePlayer();
        var money = player.Money;
        money.Amount = 1000;

        await money.TryTakeAsync(10, async () =>
        {
            await Task.Delay(1);
            return true;
        });

        await money.TryTakeAsync(10, async () =>
        {
            await Task.Delay(1).ConfigureAwait(false);
            return true;
        });
        money.Amount.Should().Be(980);
    }
}
