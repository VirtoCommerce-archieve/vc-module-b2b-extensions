namespace VirtoCommerce.B2BExtensionsModule.Web.Migrations
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class UniqueName : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Member", new[] { "Name" });
            AlterColumn("dbo.Member", "Name", c => c.String(maxLength: 128,
                annotations: new Dictionary<string, AnnotationValues>
                {
                    { 
                        "Name",
                        new AnnotationValues(oldValue: null, newValue: "IndexAnnotation: { IsUnique: True }")
                    },
                }));
            CreateIndex("dbo.Member", "Name");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Member", new[] { "Name" });
            AlterColumn("dbo.Member", "Name", c => c.String(maxLength: 128,
                annotations: new Dictionary<string, AnnotationValues>
                {
                    { 
                        "Name",
                        new AnnotationValues(oldValue: "IndexAnnotation: { IsUnique: True }", newValue: null)
                    },
                }));
            CreateIndex("dbo.Member", "Name");
        }
    }
}
