using Realm.Tools.TypescriptDefinitionGenerator;

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
        var expectedGeneratedTypes = "export interface NotImplemented { }\r\n\r\nexport interface Object {\r\n  getType(): Type;\r\n  toString(): string;\r\n  equals(obj?: Object): boolean;\r\n  equals(objA?: Object, objB?: Object): boolean;\r\n  referenceEquals(objA?: Object, objB?: Object): boolean;\r\n  getHashCode(): number;\r\n}\r\n\r\n\r\nexport interface Test extends Object {\r\n  sampleProperty?: string;\r\n  add(a: number, b?: number | null): number;\r\n}\r\n";
        var expectedGeneratedTypesWithoutWhiteSpaces = Regex.Replace(expectedGeneratedTypes, @"\s+", "");
        var resultWithoutWhiteSpaces = Regex.Replace(result, @"\s+", "");
        resultWithoutWhiteSpaces.Should().Be(expectedGeneratedTypesWithoutWhiteSpaces);
    }
}