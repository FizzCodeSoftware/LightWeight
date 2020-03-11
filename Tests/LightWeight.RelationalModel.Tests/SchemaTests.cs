namespace LightWeight.RelationalModel.Tests
{
    using FizzCode.LightWeight.RelationalModel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SchemaTests
    {
        [TestMethod]
        public void Keys()
        {
            var model = new RelationalModel("dbo");

            var people = model.AddTable("people");
            var peopleId = people.AddColumn("id", true);
            people.AddColumn("name", false);
            var peopleFavoritePetId = people.AddColumn("favoritePetId", false);

            var pet = model.AddTable("pet");
            var petId = pet.AddColumn("id", true);
            pet.AddColumn("name", false);
            var petOwnerId = pet.AddColumn("ownerPeopleId", false);

            people.AddForeignKeyTo(pet)
                .AddColumnPair(peopleFavoritePetId, petId);

            pet.AddForeignKeyTo(people)
                .AddColumnPair(petOwnerId, peopleId);

            Assert.AreEqual(2, model.Tables.Count);
            Assert.AreEqual(3, model["PEOPLE"].Columns.Count);
            Assert.AreEqual(3, model["PET"].Columns.Count);
            Assert.AreEqual(1, model["People"].ForeignKeys.Count);
            Assert.AreEqual(1, model["Pet"].ForeignKeys.Count);
            Assert.IsTrue(model[pet.Name].ForeignKeys[0].TargetTable == people);
            Assert.IsTrue(model[people.Name].ForeignKeys[0].ColumnPairs[0].SourceColumn == peopleFavoritePetId);
            Assert.IsTrue(model[people.Name].ForeignKeys[0].ColumnPairs[0].TargetColumn == petId);
            Assert.IsTrue(model[pet.Name].ForeignKeys[0].ColumnPairs[0].SourceColumn == petOwnerId);
            Assert.IsTrue(model[pet.Name].ForeignKeys[0].ColumnPairs[0].TargetColumn == peopleId);
            Assert.AreEqual(1, model["PEOPLE"].PrimaryKeyColumns.Count);
            Assert.AreEqual(1, model["peT"].PrimaryKeyColumns.Count);
        }

        [TestMethod]
        public void TableAdditionalData()
        {
            var model = new RelationalModel("dbo");

            var people = model.AddTable("people");
            people.SetAdditionalData("a", 18);
            people.SetAdditionalData("a", 21);

            Assert.AreEqual(21, (int)people.GetAdditionalData("A"));
        }

        [TestMethod]
        public void ColumnAdditionalData()
        {
            var model = new RelationalModel("dbo");

            var people = model.AddTable("people");
            var peopleId = people.AddColumn("id", true);
            peopleId.SetAdditionalData("a", 18);
            peopleId.SetAdditionalData("a", 21);

            Assert.AreEqual(21, (int)people["ID"].GetAdditionalData("A"));
        }

        [TestMethod]
        public void SchemaAndName()
        {
            var model = new RelationalModel("dbo");
            var people = model.AddTable("people");
            var peopleId = people.AddColumn("id", true);

            Assert.AreEqual("dbo.people", people.SchemaAndName);
            Assert.AreEqual("people.id", peopleId.TableQualifiedName);
        }

        [TestMethod]
        public void SchemaAndNameNull()
        {
            var model = new RelationalModel(null);
            var people = model.AddTable("people");
            var peopleId = people.AddColumn("id", true);

            Assert.AreEqual("people", people.SchemaAndName);
            Assert.AreEqual("people.id", peopleId.TableQualifiedName);
        }
    }
}