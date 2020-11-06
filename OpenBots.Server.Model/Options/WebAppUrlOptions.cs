﻿using System;
using System.Collections.Generic;
using System.Text;

namespace OpenBots.Server.Model.Options
{
    /// <summary>
    /// Options for WebAppUrlOptions Configuration
    /// </summary>
    /// <remarks>
    /// Use these options to set application base paths
    /// </remarks>
    public class WebAppUrlOptions
    {
        /// <summary>
        /// Configuration Name in App Settings For WebAppUrl
        /// </summary>
        public const string WebAppUrl = "WebAppUrl";

        /// <summary>
        /// Sets the base URL of the application
        /// </summary>
        public string Url {get; set;}

        /// <summary>
        /// Relative path to be used when loging in
        /// </summary>
        public string login { get; set; }

        /// <summary>
        /// Relative path to be used when resetting password
        /// </summary>
        public string forgotpassword { get; set; }

        /// <summary>
        /// Redirection path for when a token error occurs
        /// </summary>
        public string tokenerror { get; set; }

        /// <summary>
        /// Redirection path for when no user exists
        /// </summary>
        public string NoUserExists { get; set; }

        /// <summary>
        /// Redirection path for when email has been confirmed
        /// </summary>
        public string emailaddressconfirmed { get; set; }
    }
}
