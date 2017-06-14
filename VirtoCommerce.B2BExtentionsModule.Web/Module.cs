using System;
using System.Web.Http;
using Microsoft.Practices.Unity;
using VirtoCommerce.Platform.Core.Modularity;

namespace VirtoCommerce.B2BExtentionsModule.Web
{
    public class Module : ModuleBase
    {
        private const string _connectionStringName = "VirtoCommerce";
        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        #region IModule Members

        public override void SetupDatabase()
        {
        }

        public override void Initialize()
        {
        }

        public override void PostInitialize()
        {
        }

        #endregion
    }
}