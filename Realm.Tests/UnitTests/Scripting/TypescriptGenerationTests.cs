namespace Realm.Tests.UnitTests.Scripting;


[CollectionDefinition("Non-Parallel Collection", DisableParallelization = true)]
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
        var expectedGeneratedTypes = "export interface NotImplemented { }\r\n\r\nexport interface Object {\r\n  GetType(): Type;\r\n  ToString(): string;\r\n  Equals(obj?: Object): boolean;\r\n  Equals(objA?: Object, objB?: Object): boolean;\r\n  ReferenceEquals(objA?: Object, objB?: Object): boolean;\r\n  GetHashCode(): number;\r\n}\r\n\r\n\r\nexport interface Test extends Object {\r\n  SampleProperty?: string;\r\n  Add(a: number, b?: number | null): number;\r\n}\r\n";
        var expectedGeneratedTypesWithoutWhiteSpaces = Regex.Replace(expectedGeneratedTypes, @"\s+", "");
        var resultWithoutWhiteSpaces = Regex.Replace(result, @"\s+", "");
        resultWithoutWhiteSpaces.Should().Be(expectedGeneratedTypesWithoutWhiteSpaces);
    }
}