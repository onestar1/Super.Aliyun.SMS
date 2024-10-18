using Microsoft.Extensions.Localization;
using OrchardCore.Sms;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using Super.Aliyun.SMS.Models;

namespace Super.Aliyun.SMS.Activities;


/// <summary>
/// 工作流的短信发送 
/// </summary>
public class AliyunSmsTask : TaskActivity<AliyunSmsTask>
{
    private readonly ISmsService _smsService;
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    protected readonly IStringLocalizer S;

    public AliyunSmsTask(
        ISmsService smsService,
        IWorkflowExpressionEvaluator expressionEvaluator,
        IStringLocalizer<AliyunSmsTask> stringLocalizer
    )
    {
        _smsService = smsService;
        _expressionEvaluator = expressionEvaluator;
        S = stringLocalizer;
    }

    public override LocalizedString DisplayText => S["Aliyun SMS Task"];

    public override LocalizedString Category => S["Messaging"];

    public WorkflowExpression<string> PhoneNumber {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }
    public WorkflowExpression<string> TemplateCode {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }
    public WorkflowExpression<string> SignName {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }
  
    public WorkflowExpression<string> Body {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Outcomes(S["Done"], S["Failed"]);
    }
    /// <summary>
    ///  重写
    /// </summary>
    /// <param name="workflowContext"></param>
    /// <param name="activityContext"></param>
    /// <returns></returns>
    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var message = new AliyunSmsMessage {
            To = await _expressionEvaluator.EvaluateAsync(PhoneNumber, workflowContext, null),
            Body = await _expressionEvaluator.EvaluateAsync(Body, workflowContext, null),
            TemplateCode = await _expressionEvaluator.EvaluateAsync(TemplateCode, workflowContext, null),
            SignName = await _expressionEvaluator.EvaluateAsync(SignName, workflowContext, null),
        };

        var result = await _smsService.SendAsync(message);

        workflowContext.LastResult = result;

        if (result.Succeeded) {
            return Outcomes("Done");
        }

        return Outcomes("Failed");
    }
}

