namespace Realm.Tests.UnitTests.Scripting;

public class TypescriptGenerationTest
{
    private readonly TypescriptTypesGenerator _typescriptTypesGenerator;

    class Test
    {
        public int Add(int a, int? b) => a + b ?? 0;
        public string? SampleProperty { get; set; }
    }

    public TypescriptGenerationTest()
    {
        _typescriptTypesGenerator = new TypescriptTypesGenerator();
    }

    [Fact]
    public void TestTypesGeneration()
    {
        _typescriptTypesGenerator.AddType(typeof(Test));
        var result = _typescriptTypesGenerator.Build();
        result.Should().Be(@"export interface NotImplemented { }

export interface Object {
  getType(): Type;
  toString(): string;
  equals(obj?: Object): boolean;
  equals(objA?: Object, objB?: Object): boolean;
  referenceEquals(objA?: Object, objB?: Object): boolean;
  getHashCode(): number;
}


export interface Test extends Object {
  sampleProperty?: string;
  add(a: number, b?: number | null): number;
}
");
    }
}