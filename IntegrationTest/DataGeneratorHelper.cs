// <copyright file="DataGeneratorHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// MIT License
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

using System;

namespace FHIRTest
{
    public class DataGeneratorHelper
    {
        private const string FhirTestServerUrl = "FHIR_TEST_SERVER_URL";

        /// <summary>
        /// Environment variable <see cref="FhirTestServerUrl"/> is read if set. This will help in
        /// parameterizing the VSTS build definition to run the tests for different FHIR servers when a build is
        /// queued. In case the environment variable is not set then VONK server will be hit
        /// </summary>
        /// <returns>FHIR test server end point</returns>
        public static string GetServerUrl()
        {
            return Environment.GetEnvironmentVariable(FhirTestServerUrl) ?? Constants.HapiFhirEndpoint;
        }
    }
}
