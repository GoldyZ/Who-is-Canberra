using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Engine
{
    public static class Constants
    {
        public const string DefaultConnectionStringName = "goldilocks";
        public const int QuestionTimeout = 10;

        public static string ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings[DefaultConnectionStringName].ConnectionString;
            }
        }
    }
}