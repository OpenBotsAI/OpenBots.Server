using OpenBots.Server.DataAccess.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenBots.Server.Web;

namespace OpenBots.Server.WebAPI.Controllers
{
    public static class ExceptionExtentions
    {
        public static IActionResult GetActionResult(this Exception ex)
        {
                ProblemDetails problem = ex.GetProblemDetails();
                switch(problem.Status)
                {
                    case 400:
                        return new BadRequestObjectResult(problem);
                    case 403:
                        return new ForbidResult();
                    case 404:
                        return new NotFoundObjectResult(problem);
                    case 409:
                        return new ConflictObjectResult(problem);
                    case 422:
                        return new UnprocessableEntityObjectResult(problem);
                }
            return new BadRequestResult();
        }

        public static ProblemDetails GetProblemDetails(this Exception ex)
        {
            var problem = new ServiceBadRequest();
            problem.Detail = ex.Message;
            problem.Title = string.Concat("Unknown Exception", ex.Message);
            problem.Status = 400;

            if (ex is UnauthorizedOperationException || ex is UnauthorizedAccessException)
            {
                problem.Title = string.Concat("Unauthorized Access.", ex.Message);
                problem.Status = 403;
            }
            if (ex is EntityOperationException)
            {
                problem.Title = string.Concat("Entity Operation Exception.", ex.Message);
                problem.Status = 400;
            }
            if(ex is EntityValidationException)
            {
                problem = GetValidationProblemDetails(ex as EntityValidationException);
                problem.Title = string.Concat("Validation Error.", ex.Message);
                problem.Status = 422;
            }
            if(ex is EntityAlreadyExistsException)
            {
                problem.Title = string.Concat("Entity Already Exist.", ex.Message);
                problem.Status = 409;
            }
            if (ex is EntityConcurrencyException)
            {
                problem.Title = string.Concat("Entity Concurrency Error.", ex.Message);
                problem.Status = 409;
            }
            if(ex is EntityDoesNotExistException)
            {
                problem.Title = string.Concat("Entity Does Not Exist.", ex.Message);
                problem.Status = 400;
                problem.serviceErrors = new string[1] { "Record(s) no longer exists or you do not have authorized access." };
            }
            if (ex is CannotInsertDuplicateConstraintException)
            {
                Dictionary<string, string[]> errors = new Dictionary<string, string[]>();
                CannotInsertDuplicateConstraintException cidce = ex as CannotInsertDuplicateConstraintException;
                List<string> messages = new List<string>();
                messages.Add($"{cidce.EntityName} {cidce.PropertyName} already exists. Cannot add duplicate.");
                errors.Add(cidce.PropertyName, messages.ToArray());
                problem  = new ServiceBadRequest(errors);
                problem.Title = string.Concat("Cannot Insert Duplicate Constraint.", ex.Message);
                problem.Status = 400;
            }
            return problem;
        }

        public static ServiceBadRequest GetValidationProblemDetails(EntityValidationException ex)
        {
            Dictionary<string, string[]> errors = new Dictionary<string, string[]>();
            var results = ex.Validation;

            var members = results.SelectMany(r => r.MemberNames);
            foreach (var member in members)
            {
                List<string> errMessages = new List<string>();
                foreach (var error in results.Where(r => r.MemberNames.Contains(member)))
                {
                    errMessages.Add(error.ErrorMessage);
                }
                errors.Add(member, errMessages.ToArray());

            }
            ServiceBadRequest details = new ServiceBadRequest(errors);

            return details;
        }
    }
}
