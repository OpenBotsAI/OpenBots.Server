using OpenBots.Server.Model.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#nullable enable

namespace OpenBots.Server.Model.Identity
{
    /// <summary>
    /// Represents a Person
    /// </summary>
    /// <seealso cref="NamedEntity" />
    public class Person : NamedEntity, ITenanted
    {
        public Person()
        {
            Emails = new List<PersonEmail>();
            Credentials = new List<PersonCredential>();
            EmailVerifications = new List<EmailVerification>();
        }

        /// <summary>
        /// First Name of the Person
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        [Display(Name= "FirstName")]
        public string? FirstName { get; set; }

        [Display(Name= "LastName")]
        public string? LastName { get; set; }

        [DefaultValue("false")]
        [Display(Name = "IsAgent")]
        public bool IsAgent { get; set; }

        [Display(Name= "Company")]
        public string? Company { get; set; }

        [Display(Name= "Department")]
        public string? Department { get; set; }

        [Display(Name= "Emails")]
        public List<PersonEmail> Emails { get; set; }

        [Display(Name= "Credentials")]
        public List<PersonCredential> Credentials { get; set; }

        [Display(Name= "EmailVerifications")]
        public List<EmailVerification> EmailVerifications { get; set; }

        [NotMapped]
        public Guid? OrganizationId { get; set; }

        [NotMapped]
        public string? OrgEmail { get; set; }
    }
}
