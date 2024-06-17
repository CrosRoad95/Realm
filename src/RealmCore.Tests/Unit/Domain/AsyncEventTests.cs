namespace RealmCore.Tests.Unit.Domain;

public class SampleEventArgs : EventArgs
{
    public int A { get; }
    public int B { get; }

    public SampleEventArgs(int a, int b)
    {
        A = a;
        B = b;
    }
}

public class AsyncEventTests
{
    [Fact]
    public async Task ShouldWork()
    {
        AsyncEvent<SampleEventArgs> asyncEvent = new();

        bool called1 = false;
        bool called2 = false;
        Task handle1(object sender, SampleEventArgs e)
        {
            called1 = true;
            return Task.CompletedTask;
        }

        async Task handle2(object sender, SampleEventArgs e)
        {
            await Task.Delay(20);
            called2 = true;
        }

        asyncEvent += handle1;
        asyncEvent += handle2;

        await asyncEvent.InvokeAsync(this, new SampleEventArgs(1, 2));
        called1.Should().BeTrue();
        called2.Should().BeTrue();
    }

    [Fact]
    public async Task UnsubscribingShouldWork()
    {
        AsyncEvent<SampleEventArgs> asyncEvent = new();

        bool called1 = false;
        bool called2 = false;
        Task handle1(object sender, SampleEventArgs e)
        {
            called1 = true;
            return Task.CompletedTask;
        }

        Task handle2(object sender, SampleEventArgs e)
        {
            called2 = true;
            return Task.CompletedTask;
        }

        asyncEvent += handle1;
        asyncEvent += handle2;
        asyncEvent -= handle2;

        await asyncEvent.InvokeAsync(this, new SampleEventArgs(1, 2));
        called1.Should().BeTrue();
        called2.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldAggregateExceptions()
    {
        AsyncEvent<SampleEventArgs> asyncEvent = new();

        bool called = false;
        Task handle1(object sender, SampleEventArgs e)
        {
            throw new Exception("a");
        }

        Task handle2(object sender, SampleEventArgs e)
        {
            called = true;
            return Task.CompletedTask;
        }

        Task handle3(object sender, SampleEventArgs e)
        {
            throw new Exception("b");
        }

        asyncEvent += handle1;
        asyncEvent += handle2;
        asyncEvent += handle3;

        var act = async () => await asyncEvent.InvokeAsync(this, new SampleEventArgs(1, 2));

        var result = await act.Should().ThrowAsync<AggregateException>();
        result.Which.InnerExceptions.Should().HaveCount(2);
        called.Should().BeTrue();
    }
}
