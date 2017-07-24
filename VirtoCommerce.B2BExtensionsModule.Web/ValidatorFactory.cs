using System;
using FluentValidation;
using Microsoft.Practices.Unity;

namespace VirtoCommerce.B2BExtensionsModule.Web
{
    [CLSCompliant(false)]
    public class ValidatorFactory : ValidatorFactoryBase
    {
        private readonly IUnityContainer _container;

        public ValidatorFactory(IUnityContainer container)
        {
            _container = container;
        }

        public override IValidator CreateInstance(Type validatorType)
        {
            return _container.Resolve(validatorType) as IValidator;
        }
    }
}