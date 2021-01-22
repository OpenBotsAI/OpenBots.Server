using Mustache;
using OpenBots.Server.ViewModel.Email;
using System.IO;

namespace OpenBots.Server.Web.Extensions
{
    public static class EmailTextFormatter
    {
        public static string Format(EmailTemplateData templateObj)
        {
            string emailBody = "";
            using (StreamReader reader = new StreamReader(Path.Combine(templateObj.Url, templateObj.FileName)))
            {
                emailBody = reader.ReadToEnd();
                var compiler = new FormatCompiler();
                var generator = compiler.Compile(emailBody);
                return generator.Render(templateObj);
            }
            return "";
        }
    }
}
