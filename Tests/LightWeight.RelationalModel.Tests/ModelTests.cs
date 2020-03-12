namespace LightWeight.RelationalModel.Tests
{
    using FizzCode.LightWeight.RelationalModel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ModelTests
    {
        [TestMethod]
        public void Keys()
        {
            var model = new RelationalModel("dbo");
            var secondary = model.AddSchema("secondary");

            var people = model.DefaultSchema.AddTable("people");
            var peopleId = people.AddColumn("id", true);
            people.AddColumn("name", false);
            var peopleFavoritePetId = people.AddColumn("favoritePetId", false);

            var pet = secondary.AddTable("pet");
            var petId = pet.AddColumn("id", true);
            pet.AddColumn("name", false);
            var petOwnerId = pet.AddColumn("ownerPeopleId", false);

            people.AddForeignKeyTo(pet)
                .AddColumnPair(peopleFavoritePetId, petId);

            pet.AddForeignKeyTo(people)
                .AddColumnPair(petOwnerId, peopleId);

            Assert.AreEqual(2, model.Schemas.Count);
            Assert.AreEqual(1, model["dbo"].Tables.Count);
            Assert.AreEqual(1, secondary.Tables.Count);
            Assert.AreEqual(3, model["dbo"]["PEOPLE"].Columns.Count);
            Assert.AreEqual(3, secondary["PET"].Columns.Count);
            Assert.AreEqual(1, model["dbo"]["People"].ForeignKeys.Count);
            Assert.AreEqual(1, secondary["Pet"].ForeignKeys.Count);
            Assert.IsTrue(pet.ForeignKeys[0].TargetTable == people);
            Assert.IsTrue(people.ForeignKeys[0].ColumnPairs[0].SourceColumn == peopleFavoritePetId);
            Assert.IsTrue(people.ForeignKeys[0].ColumnPairs[0].TargetColumn == petId);
            Assert.IsTrue(pet.ForeignKeys[0].ColumnPairs[0].SourceColumn == petOwnerId);
            Assert.IsTrue(pet.ForeignKeys[0].ColumnPairs[0].TargetColumn == peopleId);
            Assert.AreEqual(1, model["dbo"]["PEOPLE"].PrimaryKeyColumns.Count);
            Assert.AreEqual(1, secondary["peT"].PrimaryKeyColumns.Count);
        }

        [TestMethod]
        public void TableAdditionalData()
        {
            var model = new RelationalModel("dbo");

            var people = model.DefaultSchema.AddTable("people");
            people.SetAdditionalData("a", 18);
            people.SetAdditionalData("a", 21);

            Assert.AreEqual(21, (int)people.GetAdditionalData("A"));
        }

        [TestMethod]
        public void ColumnAdditionalData()
        {
            var model = new RelationalModel();
            var dbo = model.AddSchema("dbo");

            var people = dbo.AddTable("people");
            var peopleId = people.AddColumn("id", true);
            peopleId.SetAdditionalData("a", 18);
            peopleId.SetAdditionalData("a", 21);

            Assert.AreEqual(21, (int)people["ID"].GetAdditionalData("A"));
        }

        [TestMethod]
        public void SchemaAndName()
        {
            var model = new RelationalModel("dbo");
            var dbo = model["dbo"];

            var people = dbo.AddTable("people");
            var peopleId = people.AddColumn("id", true);

            Assert.AreEqual("dbo.people", people.SchemaAndName);
            Assert.AreEqual("people.id", peopleId.TableQualifiedName);
        }
    }
}