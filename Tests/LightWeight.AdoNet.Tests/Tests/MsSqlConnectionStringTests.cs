namespace FizzCode.LightWeight.AdoNetTests;

[TestClass]
public class MsSqlConnectionStringTests
{
    [TestMethod]
    public void ChangeIdentifier0()
    {
        var formatter = new MsSqlConnectionString("", "");
        var result = formatter.ChangeObjectIdentifier("person", "__temp");
        const string expected = "__temp";

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void ChangeIdentifier1()
    {
        var formatter = new MsSqlConnectionString("", "");
        var result = formatter.ChangeObjectIdentifier("[dbo].[person]", "__temp");
        const string expected = "[dbo].[__temp]";

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void ChangeIdentifier2()
    {
        var formatter = new MsSqlConnectionString("", "");
        var result = formatter.ChangeObjectIdentifier("[mydb.test].[dbo].[person]", "__temp");
        const string expected = "[mydb.test].[dbo].[__temp]";

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void ChangeIdentifier3()
    {
        var formatter = new MsSqlConnectionString("", "");
        var result = formatter.ChangeObjectIdentifier("dbo.person", "__temp");
        const string expected = "dbo.__temp";

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void ChangeIdentifier4()
    {
        var formatter = new MsSqlConnectionString("", "");
        var result = formatter.ChangeObjectIdentifier("mydb.dbo.person", "__temp");
        const string expected = "mydb.dbo.__temp";

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void ChangeIdentifier5()
    {
        var formatter = new MsSqlConnectionString("", "");
        var result = formatter.ChangeObjectIdentifier("mydb.dbo.[person]", "__temp");
        const string expected = "mydb.dbo.[__temp]";

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void ChangeIdentifier6()
    {
        var formatter = new MsSqlConnectionString("", "");
        var result = formatter.ChangeObjectIdentifier("mydb.[dbo.hello].person", "__temp");
        const string expected = "mydb.[dbo.hello].[__temp]";

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void ChangeIdentifier7()
    {
        var formatter = new MsSqlConnectionString("", "");
        var result = formatter.ChangeObjectIdentifier("mydb.[dbo.hello].[person.today]", "__temp");
        const string expected = "mydb.[dbo.hello].[__temp]";

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void ChangeIdentifier8()
    {
        var formatter = new MsSqlConnectionString("", "");
        var result = formatter.ChangeObjectIdentifier("[mydb backup].[dbo.hello].[person.today]", "__temp");
        const string expected = "[mydb backup].[dbo.hello].[__temp]";

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void SetInitialCatalog1()
    {
        var cs = new MsSqlConnectionString("test", "Server=tcp:test.sql.azuresynapse.net,1433;Initial Catalog=ds;Persist Security Info=False;User ID=x;Password=y;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;");
        var cs2 = cs.CloneWithInitialCatalog("newcatalog");
        const string expected = "Server=tcp:test.sql.azuresynapse.net,1433;Initial Catalog=newcatalog;Persist Security Info=False;User ID=x;Password=y;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;";
        Assert.AreEqual(expected, cs2.ConnectionString);
    }

    [TestMethod]
    public void SetInitialCatalog2()
    {
        var cs = new MsSqlConnectionString("test", "Server=tcp:test.sql.azuresynapse.net,1433;Persist Security Info=False;User ID=x;Password=y;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;Initial Catalog=ds");
        var cs2 = cs.CloneWithInitialCatalog("newcatalog");
        const string expected = "Server=tcp:test.sql.azuresynapse.net,1433;Persist Security Info=False;User ID=x;Password=y;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;Initial Catalog=newcatalog";
        Assert.AreEqual(expected, cs2.ConnectionString);
    }

    [TestMethod]
    public void SetInitialCatalog3()
    {
        var cs = new MsSqlConnectionString("test", "Server=tcp:test.sql.azuresynapse.net,1433;Persist Security Info=False;User ID=x;Password=y;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Initial Catalog=ds;Connection Timeout=30");
        var cs2 = cs.CloneWithInitialCatalog("newcatalog");
        const string expected = "Server=tcp:test.sql.azuresynapse.net,1433;Persist Security Info=False;User ID=x;Password=y;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Initial Catalog=newcatalog;Connection Timeout=30";
        Assert.AreEqual(expected, cs2.ConnectionString);
    }
}