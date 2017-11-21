namespace VirtoCommerce.B2BExtensionsModule.Web.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Company",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organization", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.CompanyMember",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Title = c.String(maxLength: 128),
                        IsActive = c.Boolean(nullable: false)
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Contact", t => t.Id)
                .Index(t => t.Id);           

            //Convert  all exist Contact records to CompanyMember
            Sql("INSERT INTO dbo.CompanyMember (Id) SELECT Id FROM dbo.Contact");

            CreateTable(
                "dbo.Department",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ParentId = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Member", t => t.Id)
                .Index(t => t.Id);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Department", "Id", "dbo.Member");
            DropForeignKey("dbo.CompanyMember", "Id", "dbo.Contact");
            DropForeignKey("dbo.Company", "Id", "dbo.Organization");

            DropIndex("dbo.Department", new[] { "Id" });
            DropIndex("dbo.CompanyMember", new[] { "Id" });
            DropIndex("dbo.Company", new[] { "Id" });

            DropTable("dbo.Department");
            DropTable("dbo.CompanyMember");
            DropTable("dbo.Company");
        }
    }
}
