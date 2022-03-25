namespace LightWeight.Collections.Tests;

using System.Linq;
using FizzCode.LightWeight.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class CaseInsensitiveStringKeyDictionaryTests
{
    [TestMethod]
    public void KeyIsCaseInsensitive()
    {
        var dict = new CaseInsensitiveStringKeyDictionary<int>
        {
            ["x"] = 17
        };

        Assert.AreEqual(17, dict["x"]);
        Assert.AreEqual(17, dict["X"]);
    }

    [TestMethod]
    public void ValueCanBeOverwritten()
    {
        var dict = new CaseInsensitiveStringKeyDictionary<int>
        {
            ["x"] = 17,
            ["X"] = 21
        };

        Assert.AreEqual(21, dict["x"]);
        Assert.AreEqual(21, dict["X"]);
    }

    [TestMethod]
    public void UnknownKeyReturnsDefaultInt()
    {
        var dict = new CaseInsensitiveStringKeyDictionary<int>
        {
            ["x"] = 17
        };

        Assert.AreEqual(17, dict["x"]);
        Assert.AreEqual(default, dict["y"]);
    }

    [TestMethod]
    public void UnknownKeyReturnsDefaultString()
    {
        var dict = new CaseInsensitiveStringKeyDictionary<string>
        {
            ["x"] = "y"
        };

        Assert.AreEqual("y", dict["x"]);
        Assert.AreEqual(null, dict["y"]);
        Assert.AreEqual(default, dict["y"]);
    }

    [TestMethod]
    public void RemoveExistingValue()
    {
        var dict = new CaseInsensitiveStringKeyDictionary<int>
        {
            ["x"] = 17,
            ["y"] = 21
        };
        dict.Remove("y");

        Assert.AreEqual(17, dict["x"]);
        Assert.AreEqual(default, dict["y"]);
        Assert.AreEqual(1, dict.Count);

        dict.Remove("X");
        Assert.AreEqual(default, dict["x"]);
        Assert.AreEqual(0, dict.Count);
    }

    [TestMethod]
    public void RemoveNonExistingValue()
    {
        var dict = new CaseInsensitiveStringKeyDictionary<int>
        {
            ["x"] = 17
        };
        dict.Remove("y");

        Assert.AreEqual(17, dict["x"]);
        Assert.AreEqual(1, dict.Count);
    }

    [TestMethod]
    public void Clear()
    {
        var dict = new CaseInsensitiveStringKeyDictionary<int>
        {
            ["x"] = 17
        };
        dict.Clear();

        Assert.AreEqual(0, dict.Count);
        Assert.AreEqual(default, dict["x"]);
    }

    [TestMethod]
    public void Enumerable()
    {
        var dict = new CaseInsensitiveStringKeyDictionary<int>
        {
            ["x"] = 17,
            ["y"] = 21
        };

        Assert.AreEqual(2, dict.Count);
#pragma warning disable CA1829 // Use Length/Count property instead of Count() when available
        Assert.AreEqual(2, dict.Count());
#pragma warning restore CA1829 // Use Length/Count property instead of Count() when available
        Assert.IsTrue(dict.ContainsValue(17));
        Assert.IsTrue(dict.ContainsValue(21));
    }
}
