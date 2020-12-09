using System.Collections.Generic;

namespace Doitsu.Ecommerce.Presentation.Server.Helpers
{
    public static class StaticDataExtensions
    {
        public static Dictionary<string, (string className, string iconName)> GetExternalButtonDictionary()
            => new Dictionary<string, (string className, string iconName)>()
            {
                { "Facebook", (className:"btn btn-block btn-primary", iconName: "fab fa-facebook mr-2" )},
                { "Google", (className:"btn btn-block btn-danger", iconName: "fab fa-google-plus mr-2" )},
            };
    }
}