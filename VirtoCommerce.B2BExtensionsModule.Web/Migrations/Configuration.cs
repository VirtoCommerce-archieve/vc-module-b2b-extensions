namespace VirtoCommerce.B2BExtensionsModule.Web.Migrations
{
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<Repositories.CorporateMembersRepository>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            MigrationsDirectory = @"Migrations";
        }

        protected override void Seed(Repositories.CorporateMembersRepository context)
        {
        }
    }
}
