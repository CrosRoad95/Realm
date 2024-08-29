using FluentAssertions.Equivalency;

namespace RealmCore.Tests.Integration;

internal static class RealmTestsHelpers
{
    public static EquivalencyAssertionOptions<T> DateTimeCloseTo<T>(EquivalencyAssertionOptions<T> options) => options
            .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1.Seconds()))
            .WhenTypeIs<DateTime>();
}