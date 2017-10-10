// <copyright file="BundleHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// MIT License
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

using System;
using Hl7.Fhir.Model;

namespace FHIRTest
{
    internal class BundleHelper
    {
        /// <summary>
        /// Method to convert bundle entry request urls to relative until it's updated in the api. See https://github.com/ewoutkramer/fhir-net-api/issues/400 for context
        /// </summary>
        /// <param name="bundle">The bundle to clean up</param>
        /// <param name="endpoint">Intended endpoint for the request</param>
        // TODO: Remove this method when the issue in the API is resolved
        internal static void CleanEntryRequestUrls(Bundle bundle, Uri endpoint)
        {
            foreach (var entry in bundle.Entry)
            {
                entry.Request.Url = entry.Request.Url.Replace(endpoint.ToString(), string.Empty);
                entry.Request.UrlElement.Value = entry.Request.UrlElement.Value.Replace(endpoint.ToString(), string.Empty);
            }
        }
    }
}
