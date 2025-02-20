﻿//
// Copyright 2020 Google LLC
//
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
//

using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.Apis.Services;
using Google.Solutions.Common.Net;
using Google.Solutions.IapDesktop.Application.Services.Authorization;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

namespace Google.Solutions.IapDesktop.Application.Services.Adapters
{
    public static class ClientServiceFactory
    {
        public static BaseClientService.Initializer ForMtlsEndpoint(
            ICredential credential,
            IDeviceEnrollment enrollment,
            string baseUri)
        {
            var initializer = new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = Globals.UserAgent.ToApplicationName()
            };

            if (enrollment?.Certificate != null &&
                HttpClientHandlerExtensions.IsClientCertificateSupported)
            {
                ApplicationTraceSources.Default.TraceInformation(
                    "Enabling MTLS for {0}",
                    baseUri);

                // Switch to mTLS endpoint.
                initializer.BaseUri = baseUri;

                // Add client certificate.
                initializer.HttpClientFactory = new MtlsHttpClientFactory(enrollment.Certificate);
            }

            return initializer;
        }
    }

    internal class MtlsHttpClientFactory : HttpClientFactory
    {
        private readonly X509Certificate2 clientCertificate;

        public MtlsHttpClientFactory(X509Certificate2 clientCertificate)
        {
            this.clientCertificate = clientCertificate;
        }

        protected override HttpClientHandler CreateClientHandler()
        {
            var handler = base.CreateClientHandler();
            var added = handler.TryAddClientCertificate(this.clientCertificate);
            Debug.Assert(added);
            return handler;
        }
    }

    public static class ClientServiceExtensions
    {
        public static bool IsMtlsEnabled(this IClientService service)
            => service.BaseUri.Contains(".mtls.googleapis.com");

        public static bool IsClientCertificateProvided(this IClientService service)
            => IsClientCertificateProvided(service.HttpClient.MessageHandler);

        private static bool IsClientCertificateProvided(HttpMessageHandler handler)
        {
            if (handler is DelegatingHandler delegatingHandler)
            {
                return IsClientCertificateProvided(delegatingHandler.InnerHandler);
            }
            else if (handler is HttpClientHandler httpHandler)
            {
                return httpHandler.GetClientCertificates().Any();
            }
            else
            {
                return false;
            }
        }
    }
}
