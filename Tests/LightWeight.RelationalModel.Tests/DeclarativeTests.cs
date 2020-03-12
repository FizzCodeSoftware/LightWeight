namespace LightWeight.RelationalModel.Tests
{
    using System;
    using FizzCode.LightWeight.RelationalModel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DeclarativeTests
    {
        public class TestModel : RelationalModel
        {
            public PeopleTable People { get; set; }

            [Schema("Secondary")]
            public PetTable PET { get; set; }

            public sealed class PeopleTable : RelationalTable
            {
                [PrimaryKey]
                public RelationalColumn ID { get; set; }

                [Flag("TestFlag", true, false)]
                public new RelationalColumn Name { get; set; }

                [SingleColumnForeignKey(typeof(PetTable), nameof(PET.Id))]
                [Flag("TestFlag", true, false)]
                [Flag("TestFlag", false, false)]
                public RelationalColumn FavoritePetId { get; set; }
            }

            public sealed class PetTable : RelationalTable
            {
                [PrimaryKey]
                public RelationalColumn Id { get; set; }

                [SingleColumnForeignKey(typeof(PeopleTable), nameof(People.ID))]
                public RelationalColumn OwnerPeopleID { get; set; }
            }

            public TestModel(string schemaName = null)
                : base(schemaName)
            {
                BuildFromProperties();

                PET.AddColumn("Name", false, 1);
            }
        }

        public class BrokenExclusiveFlagTestModel : RelationalModel
        {
            public PeopleTable People { get; set; }

            public sealed class PeopleTable : RelationalTable
            {
                [Flag("TestFlag", true, true)]
                public new RelationalColumn Name { get; set; }

                [Flag("TestFlag", true, true)]
                public RelationalColumn FavoritePetId { get; set; }
            }

            public BrokenExclusiveFlagTestModel(string schemaName)
                : base(schemaName)
            {
                BuildFromProperties();
            }
        }

        [TestMethod]
        public void Keys()
        {
            var model = new TestModel("dbo");

            Assert.AreEqual(2, model.Tables.Count);
            Assert.AreEqual(3, model["dbo.PEOPLE"].Columns.Count);
            Assert.AreEqual(3, model["secondary.PET"].Columns.Count);
            Assert.AreEqual(1, model["dbo.People"].ForeignKeys.Count);
            Assert.AreEqual(1, model["secondary.Pet"].ForeignKeys.Count);
            Assert.IsTrue(model.PET.ForeignKeys[0].TargetTable == model.People);
            Assert.IsTrue(model.People.ForeignKeys[0].ColumnPairs[0].SourceColumn == model.People.FavoritePetId);
            Assert.IsTrue(model.People.ForeignKeys[0].ColumnPairs[0].TargetColumn == model.PET.Id);
            Assert.IsTrue(model.PET.ForeignKeys[0].ColumnPairs[0].SourceColumn == model.PET.OwnerPeopleID);
            Assert.IsTrue(model.PET.ForeignKeys[0].ColumnPairs[0].TargetColumn == model.People.ID);
            Assert.AreEqual(1, model.People.PrimaryKeyColumns.Count);
            Assert.AreEqual(1, model.PET.PrimaryKeyColumns.Count);
        }

        [TestMethod]
        public void KeysNoSchema()
        {
            var model = new TestModel();

            Assert.AreEqual(2, model.Tables.Count);
            Assert.AreEqual(3, model["PEOPLE"].Columns.Count);
            Assert.AreEqual(3, model["secondary.PET"].Columns.Count);
            Assert.AreEqual(1, model["People"].ForeignKeys.Count);
            Assert.AreEqual(1, model["secondary.Pet"].ForeignKeys.Count);
            Assert.IsTrue(model.PET.ForeignKeys[0].TargetTable == model.People);
            Assert.IsTrue(model.People.ForeignKeys[0].ColumnPairs[0].SourceColumn == model.People.FavoritePetId);
            Assert.IsTrue(model.People.ForeignKeys[0].ColumnPairs[0].TargetColumn == model.PET.Id);
            Assert.IsTrue(model.PET.ForeignKeys[0].ColumnPairs[0].SourceColumn == model.PET.OwnerPeopleID);
            Assert.IsTrue(model.PET.ForeignKeys[0].ColumnPairs[0].TargetColumn == model.People.ID);
            Assert.AreEqual(1, model.People.PrimaryKeyColumns.Count);
            Assert.AreEqual(1, model.PET.PrimaryKeyColumns.Count);
        }

        [TestMethod]
        public void Flags()
        {
            var model = new TestModel("dbo");

            Assert.IsTrue(model.People.Name.GetFlag("TestFlag"));
            Assert.IsFalse(model.People.FavoritePetId.GetFlag("TestFlag"));

            Assert.AreEqual(1, model.People.GetColumnsWithFlag("TestFlag").Count);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void BrokenExclusiveFlag()
        {
            var _ = new BrokenExclusiveFlagTestModel("dbo");
        }
    }
}