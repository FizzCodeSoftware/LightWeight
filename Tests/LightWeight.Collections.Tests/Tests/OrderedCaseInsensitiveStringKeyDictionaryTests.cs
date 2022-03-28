namespace LightWeight.CollectionTests;

[TestClass]
public class OrderedCaseInsensitiveStringKeyDictionaryTests
{
    [TestMethod]
    public void KeyIsCaseInsensitive()
    {
        var dict = new OrderedCaseInsensitiveStringKeyDictionary<int>
        {
            ["x"] = 17
        };

        Assert.AreEqual(17, dict["x"]);
        Assert.AreEqual(17, dict["X"]);
    }

    [TestMethod]
    public void ValueCanBeOverwritten()
    {
        var dict = new OrderedCaseInsensitiveStringKeyDictionary<int>
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
        var dict = new OrderedCaseInsensitiveStringKeyDictionary<int>
        {
            ["x"] = 17
        };

        Assert.AreEqual(17, dict["x"]);
        Assert.AreEqual(default, dict["y"]);
    }

    [TestMethod]
    public void UnknownKeyReturnsDefaultString()
    {
        var dict = new OrderedCaseInsensitiveStringKeyDictionary<string>
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
        var dict = new OrderedCaseInsensitiveStringKeyDictionary<int>
        {
            ["x"] = 17,
            ["y"] = 21,
            ["z"] = 5,
        };
        dict.Remove("y");

        Assert.AreEqual(17, dict.First());
        Assert.AreEqual(5, dict.Skip(1).First());
        Assert.AreEqual(default, dict["y"]);
        Assert.AreEqual(2, dict.Count);

        dict.Remove("X");
        Assert.AreEqual(default, dict["x"]);
        Assert.AreEqual(default, dict["y"]);
        Assert.AreEqual(5, dict.First());
        Assert.AreEqual(1, dict.Count);
    }

    [TestMethod]
    public void RemoveNonExistingValue()
    {
        var dict = new OrderedCaseInsensitiveStringKeyDictionary<int>
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
        var dict = new OrderedCaseInsensitiveStringKeyDictionary<int>
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
        var dict = new OrderedCaseInsensitiveStringKeyDictionary<int>
        {
            ["x"] = 17,
            ["y"] = 21
        };

        Assert.AreEqual(2, dict.Count);
        Assert.IsTrue(dict.First() == 17);
        Assert.IsTrue(dict.Skip(1).First() == 21);
        Assert.IsTrue(dict.Skip(2).FirstOrDefault() == default);
    }

    [TestMethod]
    public void Ordering()
    {
        var dict = new OrderedCaseInsensitiveStringKeyDictionary<int>
        {
            ["x"] = 17
        };

        dict.Insert("y", 21, 0);

        Assert.AreEqual(2, dict.Count);
        Assert.IsTrue(dict.First() == 21);
        Assert.IsTrue(dict.Skip(1).First() == 17);
        Assert.IsTrue(dict.Skip(2).FirstOrDefault() == default);
    }

    [TestMethod]
    public void IndexCantBeChangedWhenValueIsChanged()
    {
        var dict = new OrderedCaseInsensitiveStringKeyDictionary<int>
        {
            ["x"] = 17
        };

        dict.Insert("y", 21, 0);
        Assert.AreEqual(false, dict.Insert("y", 24, 2)); // index can't be changed this way
        Assert.AreEqual(false, dict.Insert("z", 1, 3));
        dict.Add("y", 90);

        Assert.AreEqual(2, dict.Count);
        Assert.IsTrue(dict.First() == 90);
        Assert.IsTrue(dict.Skip(1).First() == 17);
        Assert.IsTrue(dict.Skip(2).FirstOrDefault() == default);
    }
}
