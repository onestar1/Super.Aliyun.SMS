using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Notifications;
using OrchardCore.Security.Permissions;
using OrchardCore.Sms;
using OrchardCore.Sms.Activities;
using OrchardCore.Sms.Drivers;
using OrchardCore.Sms.Services;
using OrchardCore.Workflows.Helpers;
using Super.Aliyun.SMS.Activities;
using Super.Aliyun.SMS.DisplayDriver;
using Super.Aliyun.SMS.Services;

namespace Super.Aliyun.SMS
{
    public class Startup : StartupBase
    {

        private readonly IHostEnvironment _hostEnvironment;

        public Startup(IHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }
      
        public override void ConfigureServices(IServiceCollection services)
        {

            services.AddSmsProvider<AliyunSmsProvider>("Aliyun");
            services.AddSmsProviderOptionsConfiguration<AliyunProviderOptionsConfigurations>()
                .AddSiteDisplayDriver<AliyunSettingsDisplayDriver>();
            services.AddNavigationProvider<AdminMenu>();
        }
  
        [Feature("OrchardCore.Notifications.Sms")]
        public sealed class NotificationsStartup : StartupBase
        {
            public override void ConfigureServices(IServiceCollection services)
            {
                services.AddScoped<INotificationMethodProvider, SmsNotificationProvider>();
            }
        }
       // [RequireFeatures("OrchardCore.Workflows")]
        public sealed class WorkflowsAliyunStartup : StartupBase
        {
            public override void ConfigureServices(IServiceCollection services)
            {
                services.AddActivity<AliyunSmsTask, AliyunSmsTaskDisplayDriver>();
            }
        }

    }
}