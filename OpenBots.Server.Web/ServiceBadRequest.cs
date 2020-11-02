using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.Json.Serialization;

namespace OpenBots.Server.Web
{
    public class ServiceBadRequest : ValidationProblemDetails
    {
        //Gets the validation errors associated with this instance of Microsoft.AspNetCore.Mvc.ValidationProblemDetails.
        [JsonPropertyName("serviceErrors")]
        public string[] serviceErrors { get; set; }
        private StringCollection sericeErrorMessages = new StringCollection();

        public ServiceBadRequest():base()
        {
        }

        public ServiceBadRequest(ModelStateDictionary modelState)  : base (modelState) {

            ConstructErrorMessages();
        }

        public ServiceBadRequest(IDictionary<string, string[]> errors) : base(errors) {

            ConstructErrorMessages();
        }

        public ServiceBadRequest(ActionContext context, ModelStateDictionary modelState) : base()
        {
            
            Title = "Invalid arguments to the API";
            Detail = "The inputs supplied to the API are invalid";
            Status = 400;
            ConstructErrorMessages(context);
            Type = context.HttpContext.TraceIdentifier;
        }

        private void ConstructErrorMessages(ActionContext context)
        {
            //Build Error collection
            foreach (var keyModelStatePair in context.ModelState)
            {
                var key = keyModelStatePair.Key;
                var errors = keyModelStatePair.Value.Errors;
                if (errors != null && errors.Count > 0)
                {
                    if (errors.Count == 1)
                    {
                        var errorMessage = GetErrorMessage(errors[0]);
                        Errors.Add(key, new[] { errorMessage });
                    }
                    else
                    {
                        var errorMessages = new string[errors.Count];
                        for (var i = 0; i < errors.Count; i++)
                        {
                            errorMessages[i] = GetErrorMessage(errors[i]);
                        }
                        Errors.Add(key, errorMessages);
                    }
                }
            }
            //Build Error Message Collection
            ConstructErrorMessages();
        }

        private void ConstructErrorMessages()
        {
            //Build Error Message Collection
            foreach (KeyValuePair<string, string[]> entry in Errors)
            {
                foreach (string err in entry.Value)
                {
                    sericeErrorMessages.Add(err);
                }
            }
            if (sericeErrorMessages.Count > 0)
                serviceErrors = sericeErrorMessages.Cast<string>().ToArray<string>();
        }

        private string GetErrorMessage(ModelError error)
        {
            return string.IsNullOrEmpty(error.ErrorMessage) ?
                "The input was not valid." :
            error.ErrorMessage;
        }
    }
}
