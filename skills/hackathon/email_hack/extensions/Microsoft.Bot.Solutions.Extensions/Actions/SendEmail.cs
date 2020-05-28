﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using EmailSkill.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Graph;
using Newtonsoft.Json;

namespace Microsoft.Bot.Solutions.Extensions.Actions
{
    public class SendEmail : Dialog
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.Graph.Email.SendEmail";

        [JsonConstructor]
        public SendEmail([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        [JsonProperty("resultProperty")]
        public string ResultProperty { get; set; }

        [JsonProperty("token")]
        public StringExpression Token { get; set; }

        [JsonProperty("emailSubjectProperty")]
        public StringExpression EmailSubjectProperty { get; set; }

        [JsonProperty("emailContentProperty")]
        public StringExpression EmailContentProperty { get; set; }

        [JsonProperty("emailAddressProperty")]
        public StringExpression EmailAddressProperty { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var dcState = dc.State;
            var token = this.Token.GetValue(dcState);
            var emailSubjectProperty = this.EmailSubjectProperty.GetValue(dcState);
            var emailContentProperty = this.EmailContentProperty.GetValue(dcState);
            var emailAddressProperty = this.EmailAddressProperty.GetValue(dcState);
            var recipient = new Recipient() { EmailAddress = new EmailAddress() { Address = emailAddressProperty } };

            var service = new ServiceManager().InitMailService(token);
            // send user message.
            await service.SendMessageAsync(emailContentProperty, emailSubjectProperty, new List<Recipient>() { recipient });

            // Write Trace Activity for the http request and response values
            await dc.Context.TraceActivityAsync(nameof(SendEmail), null, valueType: DeclarativeType, label: this.Id).ConfigureAwait(false);

            if (this.ResultProperty != null)
            {
                dcState.SetValue(ResultProperty, null);
            }

            // return the actionResult as the result of this operation
            return await dc.EndDialogAsync(result: null, cancellationToken: cancellationToken);
        }
    }
}