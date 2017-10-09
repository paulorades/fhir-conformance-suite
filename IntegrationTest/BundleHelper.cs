using System;
using Hl7.Fhir.Model;

namespace FHIRTest
{
    internal class BundleHelper
    {
        /// <summary>
        /// Method to convert bundle entry request urls to relative until it's updated in the api. See https://github.com/ewoutkramer/fhir-net-api/issues/400 for context
        /// </summary>
        /// <param name="bundle"></param>
        /// <param name="endpoint"></param>
        //TODO: Remove this method when the issue in the API is resolved 
        internal static void CleanEntryRequestUrls(Bundle bundle, Uri endpoint)
        {
            foreach (var entry in bundle.Entry)
            {
                entry.Request.Url = entry.Request.Url.Replace(endpoint.ToString(), "");
                entry.Request.UrlElement.Value = entry.Request.UrlElement.Value.Replace(endpoint.ToString(), "");
            }
        }
    }
}
