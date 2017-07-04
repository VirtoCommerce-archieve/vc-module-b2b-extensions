//Call this to register our module to main application
var moduleName = "virtoCommerce.b2bExtensionsModule";

if (AppDependencies != undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
.run(['virtoCommerce.customerModule.memberTypesResolverService', function (memberTypesResolverService) {
    memberTypesResolverService.registerType({
        memberType: 'Company',
        description: 'customer.blades.member-add.organization.description',
        fullTypeName: 'VirtoCommerce.B2BExtensionsModule.Web.Model.Company',
        icon: 'fa-university',
        detailBlade: {
            template: 'Modules/$(VirtoCommerce.Customer)/Scripts/blades/organization-detail.tpl.html',
            metaFields: [{
                name: 'businessCategory',
                title: "customer.blades.organization-detail.labels.business-category",
                valueType: "ShortText"
            }]
        },
        knownChildrenTypes: ['CompanyMember']
    });
    memberTypesResolverService.registerType({
        memberType: 'CompanyMember',
        description: 'customer.blades.member-add.employee.description',
        fullTypeName: 'VirtoCommerce.B2BExtensionsModule.Web.Model.CompanyMember',
        icon: ' fa-user',
        detailBlade: {
            template: 'Modules/$(VirtoCommerce.Customer)/Scripts/blades/employee-detail.tpl.html',
            metaFields: [{
                name: 'defaultLanguage',
                title: "customer.blades.employee-detail.labels.defaultLanguage",
                valueType: "ShortText"
            },
            {
                name: 'photoUrl',
                title: "customer.blades.employee-detail.labels.photo-url",
                valueType: "Url"
            }]
        }
    });
}]);