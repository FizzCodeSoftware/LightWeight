namespace LightWeight.AdoNet.Tests
{
    using FizzCode.LightWeight.AdoNet;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SqlServerSemanticFormatterTests
    {
        [TestMethod]
        public void ChangeIdentifier0()
        {
            var formatter = new SqlServerSemanticFormatter();
            var result = formatter.ChangeIdentifier("person", "__temp");
            var expected = "__temp";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ChangeIdentifier1()
        {
            var formatter = new SqlServerSemanticFormatter();
            var result = formatter.ChangeIdentifier("[dbo].[person]", "__temp");
            var expected = "[dbo].[__temp]";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ChangeIdentifier2()
        {
            var formatter = new SqlServerSemanticFormatter();
            var result = formatter.ChangeIdentifier("[mydb.test].[dbo].[person]", "__temp");
            var expected = "[mydb.test].[dbo].[__temp]";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ChangeIdentifier3()
        {
            var formatter = new SqlServerSemanticFormatter();
            var result = formatter.ChangeIdentifier("dbo.person", "__temp");
            var expected = "dbo.__temp";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ChangeIdentifier4()
        {
            var formatter = new SqlServerSemanticFormatter();
            var result = formatter.ChangeIdentifier("mydb.dbo.person", "__temp");
            var expected = "mydb.dbo.__temp";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ChangeIdentifier5()
        {
            var formatter = new SqlServerSemanticFormatter();
            var result = formatter.ChangeIdentifier("mydb.dbo.[person]", "__temp");
            var expected = "mydb.dbo.[__temp]";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ChangeIdentifier6()
        {
            var formatter = new SqlServerSemanticFormatter();
            var result = formatter.ChangeIdentifier("mydb.[dbo.hello].person", "__temp");
            var expected = "mydb.[dbo.hello].[__temp]";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ChangeIdentifier7()
        {
            var formatter = new SqlServerSemanticFormatter();
            var result = formatter.ChangeIdentifier("mydb.[dbo.hello].[person.today]", "__temp");
            var expected = "mydb.[dbo.hello].[__temp]";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ChangeIdentifier8()
        {
            var formatter = new SqlServerSemanticFormatter();
            var result = formatter.ChangeIdentifier("[mydb backup].[dbo.hello].[person.today]", "__temp");
            var expected = "[mydb backup].[dbo.hello].[__temp]";

            Assert.AreEqual(expected, result);
        }
    }
}