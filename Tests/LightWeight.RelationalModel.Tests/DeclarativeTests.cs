namespace LightWeight.RelationalModel.Tests;

using FizzCode.LightWeight.RelationalModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class DeclarativeTests
{
    public class TestModel : RelationalModel
    {
        public DboSchema Dbo { get; set; }
        public SecondarySchema Secondary { get; set; }

        public class DboSchema : RelationalSchema
        {
            public PeopleTable People { get; set; }

            public class PeopleTable : RelationalTable
            {
                [PrimaryKey]
                public RelationalColumn ID { get; set; }

                [Flag("TestFlag", true)]
                public new RelationalColumn Name { get; set; }

                [SingleColumnForeignKey(typeof(SecondarySchema.PetTable), nameof(SecondarySchema.PetTable.Id))]
                [Flag("TestFlag", true)]
                [Flag("TestFlag", false)]
                public RelationalColumn FavoritePetId { get; set; }
            }
        }

        public class SecondarySchema : RelationalSchema
        {
            public PetTable PET { get; set; }

            public class PetTable : RelationalTable
            {
                [PrimaryKey]
                public RelationalColumn Id { get; set; }

                [SingleColumnForeignKey(typeof(DboSchema.PeopleTable), nameof(DboSchema.PeopleTable.ID))]
                public RelationalColumn OwnerPeopleID { get; set; }
            }
        }

        public TestModel(string defaultSchemaName = null)
            : base(defaultSchemaName)
        {
            BuildFromProperties();
            Secondary.PET.AddColumn("Name", false, 1);
        }
    }

    [TestMethod]
    public void Keys()
    {
        var model = new TestModel("dbo");

        Assert.AreEqual(2, model.Schemas.Count);
        Assert.AreEqual(1, model.Dbo.Tables.Count);
        Assert.AreEqual(1, model.Secondary.Tables.Count);
        Assert.AreEqual(3, model["dbo"]["PEOPLE"].Columns.Count);
        Assert.AreEqual(3, model["secondary"]["PET"].Columns.Count);
        Assert.AreEqual(1, model["dbo"]["People"].ForeignKeys.Count);
        Assert.AreEqual(1, model["secondary"]["Pet"].ForeignKeys.Count);
        Assert.IsTrue(model.Secondary.PET.ForeignKeys[0].TargetTable == model.Dbo.People);
        Assert.IsTrue(model.Dbo.People.ForeignKeys[0].ColumnPairs[0].SourceColumn == model.Dbo.People.FavoritePetId);
        Assert.IsTrue(model.Dbo.People.ForeignKeys[0].ColumnPairs[0].TargetColumn == model.Secondary.PET.Id);
        Assert.IsTrue(model.Secondary.PET.ForeignKeys[0].ColumnPairs[0].SourceColumn == model.Secondary.PET.OwnerPeopleID);
        Assert.IsTrue(model.Secondary.PET.ForeignKeys[0].ColumnPairs[0].TargetColumn == model.Dbo.People.ID);
        Assert.AreEqual(1, model.Dbo.People.PrimaryKeyColumns.Count);
        Assert.AreEqual(1, model.Secondary.PET.PrimaryKeyColumns.Count);
    }

    [TestMethod]
    public void Flags()
    {
        var model = new TestModel("dbo");

        Assert.IsTrue(model.Dbo.People.Name.GetFlag("TestFlag"));
        Assert.IsFalse(model.Dbo.People.FavoritePetId.GetFlag("TestFlag"));

        Assert.AreEqual(1, model.Dbo.People.GetColumnsWithFlag("TestFlag").Count);
    }
}
