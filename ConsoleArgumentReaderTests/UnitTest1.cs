using System;
using Xunit;
using ZhanTools.ConsoleArgumentReader;

namespace ConsoleArgumentReaderTests
{
    public class UnitTest1
    {
        [Fact]
        public void BaseTest()
        {
            var reader = new CmdLineArgumentReader(typeof(TargetType), true);
            var args = new string[] { "NamelessParameter1", 
                "-StringProperty", "Value1",
                "-StringPropertyWithDescription", "Value2",
                "-IntegerProperty", "3",
                "-NullableIntegerProperty", "4",
                "-NullableIntegerPropertyWithDefault", "5",
                "-NullableIntegerPropertyRequired", "6",
                "-BoolProperty",
                "NamelessParameter2"};

            var actual = reader.ReadArguments<TargetType>(args);
            Assert.Equal( "Value1", actual.StringProperty);
            Assert.Equal( "Value2", actual.StringPropertyWithDescription);
            Assert.Equal( 3, actual.IntegerProperty);
            Assert.Equal( 4, actual.NullableIntegerProperty);
            Assert.Equal( 5, actual.NullableIntegerPropertyWithDefault);
            Assert.Equal( 6, actual.NullableIntegerPropertyRequired);
            Assert.Equal( true, actual.BoolProperty);

            Assert.Equal("NamelessParameter1", reader.ValueArguments[0]);
            Assert.Equal("NamelessParameter2", reader.ValueArguments[1]);
            Assert.Null(reader.Error);

        }
        [Fact]
        public void DefaultValueTest()
        {
            var reader = new CmdLineArgumentReader(typeof(TargetType), true);
            var args = new string[] { "-NullableIntegerPropertyRequired", "6"};

            var actual = reader.ReadArguments<TargetType>(args);
            Assert.Equal(null, actual.StringProperty);
            Assert.Equal(null, actual.StringPropertyWithDescription);
            Assert.Equal(0, actual.IntegerProperty);
            Assert.Equal(null, actual.NullableIntegerProperty);
            Assert.Equal(-1000, actual.NullableIntegerPropertyWithDefault);
            Assert.Equal(6, actual.NullableIntegerPropertyRequired);
            Assert.Equal(false, actual.BoolProperty);
            Assert.Equal(true, actual.BoolPropertyWithDefault);
            Assert.Null(reader.Error);
        }

        [Fact]
        public void RequiredArgExceptionTest()
        {
            var reader = new CmdLineArgumentReader(typeof(TargetType), true);
            var args = new string[] {  };

            var actual = reader.ReadArguments<TargetType>(args);
            Assert.Equal(null, actual);
            Assert.NotNull(reader.Error);
        }
    }

    public class TargetType
    {
        [CmdLineArgument]
        public string StringProperty { get; set; }

        [CmdLineArgument(Default = "This is cool")]
        public string StringPropertyWithDefault { get; set; }

        [CmdLineArgument(Description = "This is also cool")]
        public string StringPropertyWithDescription { get; set; }

        [CmdLineArgument]
        public int IntegerProperty { get; set; }

        [CmdLineArgument]
        public int? NullableIntegerProperty { get; set; }

        [CmdLineArgument(Default = -1000)]
        public int? NullableIntegerPropertyWithDefault { get; set; }

        [CmdLineArgument(IsRequired = true)]
        public int? NullableIntegerPropertyRequired { get; set; }

        [CmdLineArgument]
        public bool BoolProperty { get; set; }

        [CmdLineArgument(Default = true)]
        public bool BoolPropertyWithDefault { get; set; }
    }
}
