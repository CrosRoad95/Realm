namespace RealmCore.Tests.Unit.Players;

public class PlayersMoneyFeatureTests : IClassFixture<RealmTestingServerHostingFixtureWithPlayer>
{
    private readonly RealmTestingServerHostingFixtureWithPlayer _fixture;
    private readonly RealmTestingPlayer _player;

    public PlayersMoneyFeatureTests(RealmTestingServerHostingFixtureWithPlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
    }

    [InlineData(0, 0, 1)]
    [InlineData(1, 1, 1)]
    [InlineData(1.234567890, 1.2345, 1)]
    [InlineData(1.234, 123.4, 100)]
    [Theory]
    public void GiveAndTakeMoneyShouldGiveExpectedAmountOfMoney(decimal moneyGiven, decimal expectedAmount, int times)
    {
        _fixture.CleanPlayer(_player);

        decimal moneyAdded = 0;
        decimal moneyTaken = 0;
        _player.Money.Added += (that, amount) =>
        {
            moneyAdded += amount;
        };
        _player.Money.Taken += (that, amount) =>
        {
            moneyTaken += amount;
        };

        for (int i = 0; i < times; i++)
            _player.Money.Give(moneyGiven);

        using var _ = new AssertionScope();

        _player.Money.Amount.Should().Be(expectedAmount);
        moneyAdded.Should().Be(expectedAmount);

        for (int i = 0; i < times; i++)
            _player.Money.Take(moneyGiven);

        _player.Money.Amount.Should().Be(0);
        moneyTaken.Should().Be(expectedAmount);
    }

    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(1.234567890, 1.2345)]
    [InlineData(1.234, 1.234)]
    [Theory]
    public void SettingAndGettingMoneyShouldWork(decimal moneySet, decimal expectedMoney)
    {
        _fixture.CleanPlayer(_player);

        var money = _player.Money;

        money.Amount = moneySet;
        using var _ = new AssertionScope();
        money.Amount.Should().Be(expectedMoney);
    }

    [Fact]
    public void GiveAndTakeMoneyShouldNotAllowNegativeValues()
    {
        _fixture.CleanPlayer(_player);

        _player.Money.SetInternal(1000000);

        Action actGiveMoney = () => { _player.Money.Give(-1); };
        Action actTakeMoney = () => { _player.Money.Take(-1); };

        using var _ = new AssertionScope();
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
        _fixture.CleanPlayer(_player);

        Action actGiveMoney = () => { _player.Money.Give(amount); };
        Action actTakeMoney = () => { _player.Money.Take(amount); };
        Action actSetMoney = () => { _player.Money.Amount = amount; };

        using var _ = new AssertionScope();
        if (amount > 0)
        {
            actGiveMoney.Should().Throw<GameplayException>()
                .WithMessage("Unable to give money beyond limit.");
            actTakeMoney.Should().Throw<GameplayException>()
                .WithMessage("Unable to take money beyond limit.");
        }
        actSetMoney.Should().Throw<GameplayException>()
            .WithMessage("Unable to set money beyond limit (1000000).");
    }

    [Fact]
    public async Task TestIfPlayerIsThreadSafe()
    {
        _fixture.CleanPlayer(_player);

        await ParallelHelpers.Run(() =>
        {
            _player.Money.Give(1);
        });

        _player.Money.Amount.Should().Be(800);

        await ParallelHelpers.Run(() =>
        {
            _player.Money.Take(1);
        });

        using var _ = new AssertionScope();
        _player.Money.Amount.Should().Be(0);
    }

    [Fact]
    public void YouShouldNotBeAbleToTakeMoneyIfThereIsNotEnoughOfThem()
    {
        _fixture.CleanPlayer(_player);

        _player.Money.Amount = 15;
        Action take = () => { _player.Money.Take(10); };

        using var _ = new AssertionScope();
        take.Should().NotThrow<GameplayException>();
        take.Should().Throw<GameplayException>()
            .WithMessage("Unable to take money, not enough money.");
        _player.Money.Amount.Should().Be(5);
    }

    [Fact]
    public void YouShouldBeAbleToForceTakeMoneyIfThereIsNotEnoughOfThem()
    {
        _fixture.CleanPlayer(_player);

        _player.Money.Amount = 15;
        Action take = () => { _player.Money.Take(10, true); };

        using var _ = new AssertionScope();
        take.Should().NotThrow<GameplayException>();
        take.Should().NotThrow<GameplayException>();
        _player.Money.Amount.Should().Be(-5);
    }

    [Fact]
    public async Task YouShouldBeAbleToTransferMoneyBetweenPlayers()
    {
        _fixture.CleanPlayer(_player);

        _player.Money.Amount.Should().Be(0);

        var player2 = await _fixture.Hosting.CreatePlayer();

        _player.Money.Amount.Should().Be(0);
        _player.Money.Amount = 15;
        _player.Money.Transfer(player2.Money, 10);

        using var _ = new AssertionScope();
        _player.Money.Amount.Should().Be(5);
        player2.Money.Amount.Should().Be(10);
    }

    [Fact]
    public async Task YouCannotTransferMoreMoneyThanYouHave()
    {
        _fixture.CleanPlayer(_player);

        var player2 = await _fixture.Hosting.CreatePlayer();

        _player.Money.SetInternal(100000);
        player2.Money.SetInternal(100000);

        _player.Money.Amount = 15;
        Action transfer = () => { _player.Money.Transfer(player2.Money, 20, false); };

        using var _ = new AssertionScope();
        transfer.Should().Throw<GameplayException>().WithMessage("Unable to take money, not enough money.");
    }

    [Fact]
    public async Task TransferMoneyShouldBeThreadSafety()
    {
        _fixture.CleanPlayer(_player);

        var player2 = await _fixture.Hosting.CreatePlayer();

        _player.Money.Amount = 800;

        await ParallelHelpers.Run(() =>
        {
            _player.Money.Transfer(player2.Money, 1);
        });

        using var _ = new AssertionScope();
        _player.Money.Amount.Should().Be(0);
        player2.Money.Amount.Should().Be(800);
    }

    [InlineData(10, 5, false, true)]
    [InlineData(10, 5, true, true)]
    [InlineData(10, 15, false, false)]
    [InlineData(10, 15, true, true)]
    [Theory]
    public void HasMoneyShouldReturnExpectedValue(decimal amount, decimal requiredAmount, bool force, bool expectedResult)
    {
        _fixture.CleanPlayer(_player);

        _player.Money.Amount = amount;

        using var _ = new AssertionScope();
        _player.Money.Has(requiredAmount, force).Should().Be(expectedResult);
    }

    [InlineData(6, 4)]
    [InlineData(20, 10)]
    [Theory]
    public void TryTakeMoneyShouldWork(decimal takenMoney, decimal expectedMoney)
    {
        _fixture.CleanPlayer(_player);

        _player.Money.Amount = 10;
        _player.Money.TryTake(takenMoney);

        using var _ = new AssertionScope();
        _player.Money.Amount.Should().Be(expectedMoney);
    }

    [Theory]
    [InlineData(true, 5)]
    [InlineData(false, 10)]
    public void TryTakeMoneyWithCallbackShouldFailOrSucceed(bool returns, decimal expectedAmount)
    {
        _fixture.CleanPlayer(_player);

        _player.Money.Amount = 10;
        _player.Money.TryTake(5, () =>
        {
            return returns;
        });

        using var _ = new AssertionScope();
        _player.Money.Amount.Should().Be(expectedAmount);
    }

    [Fact]
    public void TryTakeMoneyWithCallbackShouldFailOnException()
    {
        _fixture.CleanPlayer(_player);

        _player.Money.Amount = 10;
        var act = () =>
        {
            _player.Money.TryTake(5, () =>
            {
                throw new InvalidOperationException();
            });
        };

        using var _ = new AssertionScope();
        act.Should().ThrowExactly<InvalidOperationException>();
        _player.Money.Amount.Should().Be(10);
    }

    [Fact]
    public async Task TryTakeMoneyWithCallbackAsyncShouldFailOnException()
    {
        _fixture.CleanPlayer(_player);

        _player.Money.Amount = 10;
        var act = async () =>
        {
            await _player.Money.TryTakeAsync(5, () =>
            {
                throw new InvalidOperationException();
            });
        };

        using var _ = new AssertionScope();
        await act.Should().ThrowExactlyAsync<InvalidOperationException>();
        _player.Money.Amount.Should().Be(10);
    }

    [Fact]
    public async Task TryTakeMoneyWithCallbackAsyncShouldNotFail()
    {
        _fixture.CleanPlayer(_player);

        _player.Money.Amount = 100;
        bool success = false;
        var act = async () =>
        {
            success = await _player.Money.TryTakeAsync(5, () =>
            {
                return Task.FromResult(true);
            });
        };

        using var _ = new AssertionScope();
        for (int i = 0; i < 20; i++)
        {
            await act.Should().NotThrowAsync();
            success.Should().BeTrue();
        }
        _player.Money.Amount.Should().Be(0);
    }

    [Fact]
    public void TryTakeMoneyWithCallbackShouldFailIfHasNotEnoughMoney()
    {
        _fixture.CleanPlayer(_player);

        _player.Money.Amount = 100;
        bool success = _player.Money.TryTake(101, () =>
        {
            return true;
        });

        using var _ = new AssertionScope();
        success.Should().BeFalse();
        _player.Money.Amount.Should().Be(100);
    }

    [Fact]
    public async Task TryTakeMoneyWithCallbackAsyncShouldFailIfHasNotEnoughMoney()
    {
        _fixture.CleanPlayer(_player);

        _player.Money.Amount = 100;
        bool success = await _player.Money.TryTakeAsync(101, () =>
        {
            return Task.FromResult(true);
        });

        using var _ = new AssertionScope();
        success.Should().BeFalse();
        _player.Money.Amount.Should().Be(100);
    }

    [Fact]
    public void TryTakeMoneyWithCallbackShouldShouldNotTakeMoneyIfCallbackReturnFalse()
    {
        _fixture.CleanPlayer(_player);

        _player.Money.Amount = 100;
        bool success = _player.Money.TryTake(50, () =>
        {
            return false;
        });

        using var _ = new AssertionScope();
        success.Should().BeFalse();
        _player.Money.Amount.Should().Be(100);
    }

    [Fact]
    public async Task TryTakeMoneyWithCallbackAsyncShouldNotTakeMoneyIfCallbackReturnFalse()
    {
        _fixture.CleanPlayer(_player);

        _player.Money.Amount = 100;
        bool success = await _player.Money.TryTakeAsync(50, () =>
        {
            return Task.FromResult(false);
        });

        using var _ = new AssertionScope();
        success.Should().BeFalse();
        _player.Money.Amount.Should().Be(100);
    }

    //[Fact]
    //public void SettingMoneyInsideEventsShouldWork()
    //{
    //    _fixture.CleanPlayer(_player);

    //    _player.Money.SetInternal(1000000);

    //    var act = () =>
    //    {
    //        _player.Money.Set += (that, amount) =>
    //        {
    //            that.Amount = 50;
    //        };
    //        _player.Money.Amount = 100;
    //    };

    //    using var _ = new AssertionScope();
    //    act.Should().NotThrow();
    //    _player.Money.Amount.Should().Be(50);
    //}

    //[Fact]
    //public async Task UsingMoneyComponentShouldIncreaseVersions()
    //{
    //    _fixture.CleanPlayer(_player);

    //    var player2 = await _fixture.Hosting.CreatePlayer();

    //    var money = _player.Money;
    //    var user1 = _player.User;
    //    var expectedVersion = 0;

    //    using var _ = new AssertionScope();
    //    user1.GetVersion().Should().Be(0);
    //    money.SetInternal(50);
    //    user1.GetVersion().Should().Be(expectedVersion, "SetMoneyInternal(50)");

    //    money.Give(50);
    //    user1.GetVersion().Should().Be(++expectedVersion, "Give(50)");

    //    money.Take(5);
    //    user1.GetVersion().Should().Be(++expectedVersion, "Take(5)");

    //    money.TryTake(500);
    //    user1.GetVersion().Should().Be(expectedVersion, "TryTake(500)");

    //    money.TryTake(5);
    //    user1.GetVersion().Should().Be(++expectedVersion, "TryTake(5)");

    //    money.TryTake(5, () => true);
    //    user1.GetVersion().Should().Be(++expectedVersion, "TryTake(5, () => true)");

    //    money.TryTake(5, () => false);
    //    user1.GetVersion().Should().Be(expectedVersion, "TryTake(5, () => false)");

    //    await money.TryTakeAsync(5, () => Task.FromResult(true));
    //    user1.GetVersion().Should().Be(++expectedVersion, "TryTakeAsync(5, () => Task.FromResult(true)).Wait()");

    //    await money.TryTakeAsync(5, () => Task.FromResult(false));
    //    user1.GetVersion().Should().Be(expectedVersion, "TryTakeAsync(5, () => Task.FromResult(false)).Wait()");
    //}

    //[Fact]
    //public async Task GivingAndTakingZeroMoneyShouldNotRaiseAnyEvent()
    //{

    //    _fixture.CleanPlayer(_player);

    //    var money = _player.Money;

    //    using var monitor = money.Monitor();

    //    money.Give(0);
    //    money.Take(0);
    //    money.Take(0, true);
    //    money.TryTake(0);
    //    money.TryTake(0, true);
    //    money.TryTake(0, () => true);
    //    money.TryTake(0, () => false);
    //    await money.TryTakeAsync(0, () => Task.FromResult(true));
    //    await money.TryTakeAsync(0, () => Task.FromResult(false));

    //    using var _ = new AssertionScope();
    //    monitor.OccurredEvents.Should().BeEmpty();
    //    _player.User.GetVersion().Should().Be(0);
    //}

    [Fact]
    public async Task AsyncMethodCallsInsideTryTakeAsyncShouldWork()
    {
        _fixture.CleanPlayer(_player);

        var money = _player.Money;
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

        using var _ = new AssertionScope();
        money.Amount.Should().Be(980);
    }
}
