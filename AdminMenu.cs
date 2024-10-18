using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Sms;
using Super.Aliyun.SMS.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.Mvc.Core.Utilities;
namespace Super.Aliyun.SMS
{
    public sealed class AdminMenu : AdminNavigationProvider
    {
        private static readonly RouteValueDictionary _routeValues = new()
        {
        { "area", "OrchardCore.Settings" },
        { "groupId", SmsSettings.GroupId },
    };

        internal readonly IStringLocalizer S;

        public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
        {
            S = stringLocalizer;
        }

        protected override ValueTask BuildAsync(NavigationBuilder builder)
        {
            builder
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["Settings"], settings => settings
                        //.Add(S["aliyunsms"], S["aliyunsms"].PrefixPosition(), sms => sms
                        //    .AddClass("sms")
                        //    .Id("sms")
                        //    .Action("Index", "Admin", _routeValues)
                        //    .Permission(SmsPermissions.ManageSmsSettings)
                        //    .LocalNav()
                        //)
                          .Add(S["aliyun SMS Test"], S["aliyun SMS Test"].PrefixPosition(), sms => sms
                        .AddClass("smstest")
                        .Id("smstest")
                        .Action(nameof(AdminController.Testnew), typeof(AdminController).ControllerName(), "Super.Aliyun.SMS")
                        .Permission(SmsPermissions.ManageSmsSettings)
                        .LocalNav()
                    )

                    )
                )


                ;

            return ValueTask.CompletedTask;
        }
    }
}
