using System.Globalization;
using Google.Protobuf;

namespace Porticle.Grpc.UnitTests;

[TestClass]
public sealed class Tests
{
    private static readonly Guid Guid1 = Guid.Parse("61A866D3-97F8-425D-BFB2-0E85AB93C236");
    private static readonly Guid Guid2 = Guid.Parse("9BB5C5B2-0D8B-4B51-A36F-DC59D3AB4F9E");
    private static readonly Guid Guid3 = Guid.Parse("BC483FF2-27D7-4CE7-94A1-531E61BBF549");
    private static readonly Guid Guid4 = Guid.Parse("D78E2E14-CC83-48A2-A782-E4A0807D25F4");
    private static readonly Guid Guid5 = Guid.Parse("666383AD-0637-4233-92FE-842072372FF7");

    private static readonly decimal Decimal1 = 12345.6789m;
    private static readonly decimal Decimal2 = -99999.00001m;

    [TestMethod]
    public void TestDecimalWithNull()
    {
        var message = new TestMessageMapped { SingleGuid = Guid4, SingleDecimal = Decimal1, SingleNullableDecimal = null };

        var byteArray = message.ToByteArray();

        var deserializedMessage = TestMessageMapped.Parser.ParseFrom(byteArray);

        Assert.AreEqual(Decimal1, deserializedMessage.SingleDecimal);
        Assert.IsNull(deserializedMessage.SingleNullableDecimal);
    }

    [TestMethod]
    public void TestDecimalWithoutNull()
    {
        var message = new TestMessageMapped { SingleGuid = Guid4, SingleDecimal = Decimal1, SingleNullableDecimal = Decimal2 };

        var byteArray = message.ToByteArray();

        var deserializedMessage = TestMessageMapped.Parser.ParseFrom(byteArray);

        Assert.AreEqual(Decimal1, deserializedMessage.SingleDecimal);
        Assert.AreEqual(Decimal2, deserializedMessage.SingleNullableDecimal);
    }

    [TestMethod]
    public void TestDecimalUnmappedToMapped()
    {
        var message = new TestMessage
        {
            SingleGuid = Guid4.ToString(),
            SingleDecimal = Decimal1.ToString(CultureInfo.InvariantCulture),
            SingleNullableDecimal = Decimal2.ToString(CultureInfo.InvariantCulture)
        };

        var byteArray = message.ToByteArray();

        var deserializedMessage = TestMessageMapped.Parser.ParseFrom(byteArray);

        Assert.AreEqual(Decimal1, deserializedMessage.SingleDecimal);
        Assert.AreEqual(Decimal2, deserializedMessage.SingleNullableDecimal);
    }

    [TestMethod]
    public void TestDecimalMappedToUnmapped()
    {
        var message = new TestMessageMapped { SingleGuid = Guid4, SingleDecimal = Decimal1, SingleNullableDecimal = Decimal2 };

        var byteArray = message.ToByteArray();

        var deserializedMessage = TestMessage.Parser.ParseFrom(byteArray);

        Assert.AreEqual(Decimal1.ToString(CultureInfo.InvariantCulture), deserializedMessage.SingleDecimal);
        Assert.AreEqual(Decimal2.ToString(CultureInfo.InvariantCulture), deserializedMessage.SingleNullableDecimal);
    }

    [TestMethod]
    public void TestWithNull()
    {
        Guid[] guids = [Guid1, Guid2, Guid3];

        var message = new TestMessageMapped { SingleGuid = Guid4, SingleNullableGuid = null, SingleNullableString = null, ListOfGuid = { guids } };

        var byteArray = message.ToByteArray();

        var deserializesMessage = TestMessageMapped.Parser.ParseFrom(byteArray);

        Assert.AreEqual(Guid4, deserializesMessage.SingleGuid);
        Assert.IsNull(deserializesMessage.SingleNullableGuid);
        Assert.IsNull(deserializesMessage.SingleNullableString);
        Assert.AreEqual(3, deserializesMessage.ListOfGuid.Count);
        Assert.AreEqual(Guid1, deserializesMessage.ListOfGuid[0]);
        Assert.AreEqual(Guid2, deserializesMessage.ListOfGuid[1]);
        Assert.AreEqual(Guid3, deserializesMessage.ListOfGuid[2]);
        Assert.IsTrue(message.ListOfGuid.SequenceEqual(deserializesMessage.ListOfGuid));
    }

    [TestMethod]
    public void TestToString()
    {
        Guid[] guids = [Guid1, Guid2, Guid3];

        var message = new TestMessageMapped { SingleGuid = Guid4, SingleNullableGuid = null, SingleNullableString = null, ListOfGuid = { guids } };

        var customDiagnosticMessage = (ICustomDiagnosticMessage)message;

        // should not crash
        Assert.AreEqual(customDiagnosticMessage.ToString(), customDiagnosticMessage.ToDiagnosticString());
    }

    [TestMethod]
    public void TestToStringError()
    {
        var message = new TestMessageMapped();

        // Should crash when deserializing, because "SingleGuid" backend field ist an empty string and cannot be converted to a Guid
        var equatable = message.ToString();

        // Expect Exception
        Assert.AreEqual(equatable.Split('-').First().Trim(), "Message Invalid");
    }

    [TestMethod]
    public void TestWithoutNull()
    {
        var message = new TestMessageMapped { SingleGuid = Guid4, SingleNullableGuid = Guid5, SingleNullableString = "Hello", ListOfGuid = { Guid1, Guid2, Guid3 } };

        var byteArray = message.ToByteArray();

        var deserializesMessage = TestMessageMapped.Parser.ParseFrom(byteArray);

        Assert.AreEqual(Guid4, deserializesMessage.SingleGuid);
        Assert.AreEqual(Guid5, deserializesMessage.SingleNullableGuid);
        Assert.AreEqual("Hello", deserializesMessage.SingleNullableString);
        Assert.AreEqual(3, deserializesMessage.ListOfGuid.Count);
        Assert.AreEqual(Guid1, deserializesMessage.ListOfGuid[0]);
        Assert.AreEqual(Guid2, deserializesMessage.ListOfGuid[1]);
        Assert.AreEqual(Guid3, deserializesMessage.ListOfGuid[2]);
        Assert.IsTrue(message.ListOfGuid.SequenceEqual(deserializesMessage.ListOfGuid));
    }

    [TestMethod]
    public void TestUnmappedToMappedWithNull()
    {
        var message = new TestMessage
        {
            SingleGuid = Guid4.ToString(), SingleNullableGuid = null, SingleNullableString = null, ListOfGuid = { Guid1.ToString(), Guid2.ToString(), Guid3.ToString() }
        };

        var byteArray = message.ToByteArray();

        var deserializesMessage = TestMessageMapped.Parser.ParseFrom(byteArray);

        Assert.AreEqual(Guid4, deserializesMessage.SingleGuid);
        Assert.IsNull(deserializesMessage.SingleNullableGuid);
        Assert.IsNull(deserializesMessage.SingleNullableString);
        Assert.AreEqual(3, deserializesMessage.ListOfGuid.Count);
        Assert.AreEqual(Guid1, deserializesMessage.ListOfGuid[0]);
        Assert.AreEqual(Guid2, deserializesMessage.ListOfGuid[1]);
        Assert.AreEqual(Guid3, deserializesMessage.ListOfGuid[2]);
        Assert.IsTrue(message.ListOfGuid.Select(Guid.Parse).SequenceEqual(deserializesMessage.ListOfGuid));
    }

    [TestMethod]
    public void TestUnmappedToMappedWithoutNull()
    {
        var message = new TestMessage
        {
            SingleGuid = Guid4.ToString(),
            SingleNullableGuid = Guid5.ToString(),
            SingleNullableString = "Hello",
            ListOfGuid = { Guid1.ToString(), Guid2.ToString(), Guid3.ToString() }
        };

        var byteArray = message.ToByteArray();

        var deserializesMessage = TestMessageMapped.Parser.ParseFrom(byteArray);

        Assert.AreEqual(Guid4, deserializesMessage.SingleGuid);
        Assert.AreEqual(Guid5, deserializesMessage.SingleNullableGuid);
        Assert.AreEqual("Hello", deserializesMessage.SingleNullableString);
        Assert.AreEqual(3, deserializesMessage.ListOfGuid.Count);
        Assert.AreEqual(Guid1, deserializesMessage.ListOfGuid[0]);
        Assert.AreEqual(Guid2, deserializesMessage.ListOfGuid[1]);
        Assert.AreEqual(Guid3, deserializesMessage.ListOfGuid[2]);
        Assert.IsTrue(message.ListOfGuid.Select(Guid.Parse).SequenceEqual(deserializesMessage.ListOfGuid));
    }

    [TestMethod]
    public void TestMappedToUnmappedWithNull()
    {
        var message = new TestMessageMapped { SingleGuid = Guid4, SingleNullableGuid = null, SingleNullableString = null, ListOfGuid = { Guid1, Guid2, Guid3 } };

        var byteArray = message.ToByteArray();

        var deserializesMessage = TestMessage.Parser.ParseFrom(byteArray);

        Assert.AreEqual(Guid4.ToString(), deserializesMessage.SingleGuid);
        Assert.IsNull(deserializesMessage.SingleNullableGuid);
        Assert.IsNull(deserializesMessage.SingleNullableString);
        Assert.AreEqual(3, deserializesMessage.ListOfGuid.Count);
        Assert.AreEqual(Guid1.ToString(), deserializesMessage.ListOfGuid[0]);
        Assert.AreEqual(Guid2.ToString(), deserializesMessage.ListOfGuid[1]);
        Assert.AreEqual(Guid3.ToString(), deserializesMessage.ListOfGuid[2]);
        Assert.IsTrue(message.ListOfGuid.Select(s => s.ToString()).SequenceEqual(deserializesMessage.ListOfGuid));
    }

    [TestMethod]
    public void TestMappedToUnmappedWithoutNull()
    {
        var message = new TestMessageMapped { SingleGuid = Guid4, SingleNullableGuid = Guid5, SingleNullableString = "Hello", ListOfGuid = { Guid1, Guid2, Guid3 } };

        var byteArray = message.ToByteArray();

        var deserializesMessage = TestMessage.Parser.ParseFrom(byteArray);

        Assert.AreEqual(Guid4.ToString(), deserializesMessage.SingleGuid);
        Assert.AreEqual(Guid5.ToString(), deserializesMessage.SingleNullableGuid);
        Assert.AreEqual("Hello", deserializesMessage.SingleNullableString);
        Assert.AreEqual(3, deserializesMessage.ListOfGuid.Count);
        Assert.AreEqual(Guid1.ToString(), deserializesMessage.ListOfGuid[0]);
        Assert.AreEqual(Guid2.ToString(), deserializesMessage.ListOfGuid[1]);
        Assert.AreEqual(Guid3.ToString(), deserializesMessage.ListOfGuid[2]);
        Assert.IsTrue(message.ListOfGuid.Select(s => s.ToString()).SequenceEqual(deserializesMessage.ListOfGuid));
    }

    [TestMethod]
    public void TestOptionalEnum()
    {
        var message = new TestMessageMapped { EnumOptional = TestEnum.Foo };

        Assert.AreEqual(message.EnumOptional, TestEnum.Foo);

        message.EnumOptional = TestEnum.Bar;
        Assert.AreEqual(message.EnumOptional, TestEnum.Bar);

        message.EnumOptional = null;
        Assert.AreEqual(message.EnumOptional, null);

        message.EnumOptional = TestEnum.Bar;
        message.ClearEnumOptional();
        Assert.AreEqual(message.EnumOptional, null);

        message.EnumOptional = TestEnum.Foo;
        message.ClearEnumOptional();
        Assert.AreEqual(message.EnumOptional, null);
    }

    [TestMethod]
    public void TestOptionalEnumSerialize1()
    {
        var message = new FooBarMessage { EnumOptional = FooBar.Foo };
        TestOptionalEnum(message, FooBar.Foo, true);
    }

    [TestMethod]
    public void TestOptionalEnumSerialize2()
    {
        var message = new FooBarMessage { EnumOptional = FooBar.Bar };
        TestOptionalEnum(message, FooBar.Bar, true);
    }

    [TestMethod]
    public void TestOptionalEnumSerialize3()
    {
        var message = new FooBarMessage { EnumOptional = FooBar.Bar };
        message.ClearEnumOptional();
        message.EnumOptional = FooBar.Foo;
        TestOptionalEnum(message, FooBar.Foo, true);
    }

    [TestMethod]
    public void TestOptionalEnumSerialize4()
    {
        var message = new FooBarMessage { EnumOptional = FooBar.Bar };
        message.ClearEnumOptional();
        TestOptionalEnum(message, null, false);
    }

    [TestMethod]
    public void TestOptionalEnumSerialize5()
    {
        var message = new FooBarMessage { EnumOptional = null };
        TestOptionalEnum(message, null, false);
    }

    [TestMethod]
    public void TestOptionalEnumSerialize6()
    {
        var message = new FooBarMessage();
        TestOptionalEnum(message, null, false);
    }

    [TestMethod]
    public void TestOptionalEnumSerialize7()
    {
        var message = new FooBarMessage();
        message.EnumOptional = FooBar.Bar;
        message.ClearEnumOptional();
        TestOptionalEnum(message, null, false);
    }

    private static void TestOptionalEnum(FooBarMessage messageIn, FooBar? result, bool hasEnum)
    {
        var messageOut = FooBarMessage.Parser.ParseFrom(messageIn.ToByteArray());

        Assert.AreEqual(messageIn.EnumOptional, result);
        Assert.AreEqual(messageOut.EnumOptional, result);
        Assert.AreEqual(messageIn.HasEnumOptional, hasEnum);
        Assert.AreEqual(messageOut.HasEnumOptional, hasEnum);
        Assert.AreEqual(messageIn.ToString(), messageOut.ToString());
        Assert.AreEqual(BitConverter.ToString(messageIn.ToByteArray()), BitConverter.ToString(messageOut.ToByteArray()));
    }
}