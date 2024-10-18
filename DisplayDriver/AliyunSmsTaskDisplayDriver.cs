using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;
using OrchardCore.Sms.Activities;
using OrchardCore.Sms.Drivers;
using OrchardCore.Sms.ViewModels;
using OrchardCore.Sms;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using OrchardCore.Mvc.ModelBinding;
using Super.Aliyun.SMS.Activities;
using Super.Aliyun.SMS.ViewModels;


namespace Super.Aliyun.SMS.DisplayDriver
{
    /// <summary>
    /// 工作流 -短信， 修改tempcode
    /// </summary>
    public class AliyunSmsTaskDisplayDriver : ActivityDisplayDriver<AliyunSmsTask, AliyunSmsTaskViewModel>
    {
        private readonly IPhoneFormatValidator _phoneFormatValidator;
        private readonly ILiquidTemplateManager _liquidTemplateManager;

        internal readonly IStringLocalizer S;

        public AliyunSmsTaskDisplayDriver(
            IPhoneFormatValidator phoneFormatValidator,
            ILiquidTemplateManager liquidTemplateManager,
            IStringLocalizer<SmsTaskDisplayDriver> stringLocalizer
            )
        {
            _phoneFormatValidator = phoneFormatValidator;
            _liquidTemplateManager = liquidTemplateManager;
            S = stringLocalizer;
        }

        protected override void EditActivity(AliyunSmsTask activity, AliyunSmsTaskViewModel model)
        {
            model.PhoneNumber = activity.PhoneNumber.Expression;
            model.Body = activity.Body.Expression;
            model.TemplateCode = activity.TemplateCode.Expression;
            model.SignName = activity.SignName.Expression;
           
        } 

        //public override IDisplayResult Edit(AliyunSmsTask activity, BuildEditorContext context)
        //{
        //    return Initialize<AliyunSmsTaskViewModel>("AliyunSmsTask_Fields_Edit", viewModel => EditActivityAsync(activity, viewModel))
        //        .Location("Content");
        //}
        public override async Task<IDisplayResult> UpdateAsync(AliyunSmsTask activity, UpdateEditorContext context)
        {
            var viewModel = new AliyunSmsTaskViewModel();

            await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

            if (string.IsNullOrWhiteSpace(viewModel.PhoneNumber)) {
                context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.PhoneNumber), S["Phone number requires a value."]);
            } else if (!_phoneFormatValidator.IsValid(viewModel.PhoneNumber)) {
                context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.PhoneNumber), S["Invalid phone number used."]);
            }

            if (string.IsNullOrWhiteSpace(viewModel.Body)) {
                context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.Body), S["Message Body requires a value."]);
            } else if (!_liquidTemplateManager.Validate(viewModel.Body, out var bodyErrors)) {
                context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.Body), string.Join(' ', bodyErrors));
            }
            if (string.IsNullOrEmpty(viewModel.TemplateCode)) {
                context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.TemplateCode), S["Message TemplateCode requires a value."]);

            } else if (!_liquidTemplateManager.Validate(viewModel.TemplateCode, out var templateCodeErrors)) {
                context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.TemplateCode), string.Join(' ', templateCodeErrors));
            }
            if (string.IsNullOrEmpty(viewModel.SignName)) {
                context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.SignName), S["Message SignName requires a value."]);

            }else if(!_liquidTemplateManager.Validate(viewModel.SignName, out var signNameErrors)){
                context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.SignName), string.Join(' ', signNameErrors));
            }

            activity.PhoneNumber = new WorkflowExpression<string>(viewModel.PhoneNumber);
            
            activity.Body = new WorkflowExpression<string>(viewModel.Body);
            activity.TemplateCode= new WorkflowExpression<string>(viewModel.TemplateCode);
            activity.SignName = new WorkflowExpression<string>(viewModel.SignName);
            return await EditAsync(activity, context);
        }
    }
}