using System.Collections.Generic;
using System.Net;
using Xunit;

namespace FHIRTest
{
    internal class AssertHelper
    {
        internal static void CheckStatusCode(HttpStatusCode expectedStatus, string actualStatus)
        {
            Assert.StartsWith(((int)expectedStatus).ToString(), actualStatus);
        }

        internal static void CheckDeleteStatusCode(string actualStatus)
        {
            var acceptedValues = new HashSet<string>
            {
                ((int)HttpStatusCode.OK).ToString(),
                ((int)HttpStatusCode.NoContent).ToString(),
                "200 Ok",
                "204 No Content"
            };

            CheckSubsetStatusCode(acceptedValues, actualStatus);
        }
        
        internal static void CheckSubsetStatusCode(HashSet<string> allowedStatuses, string actualStatus)
        {
            Assert.Subset(allowedStatuses, new HashSet<string> { actualStatus });
        }
    }
}
