namespace VirtoCommerce.B2BExtensionsModule.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CompanyMemberTitle : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CompanyMember", "Title", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.CompanyMember", "Title");
        }
    }
}
