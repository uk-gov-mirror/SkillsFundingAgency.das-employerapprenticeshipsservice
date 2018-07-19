﻿using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema.Generation;
using NUnit.Framework;
using SFA.DAS.EAS.Support.Infrastructure.Settings;
using SFA.DAS.EAS.Support.Web.Configuration;
using SFA.DAS.Support.Shared.Authentication;
using SFA.DAS.Support.Shared.Challenge;
using SFA.DAS.Support.Shared.SiteConnection;

namespace SFA.DAS.EAS.Support.Web.Tests.Configuration
{
    [TestFixture]
    public class WebConfigurationTesting
    {
        [SetUp]
        public void Setup()
        {
            _unit = new WebConfiguration
            {
                AccountApi = new AccountApiConfiguration
                {
                    ApiBaseUrl = "--- configuration value goes here ---",
                    ClientId = "12312312-140e-4f9f-807b-112312312375",
                    ClientSecret = "--- configuration value goes here ---",
                    IdentifierUri = "--- configuration value goes here ---",
                    Tenant = "--- configuration value goes here ---"
                },
                Challenge = new ChallengeSettings
                {
                    ChallengeExpiryMinutes = 10,
                    ChallengeMaxRetries = 3
                },
                SiteValidator = new SiteValidatorSettings
                {
                    Audience = "--- configuration value goes here ---",
                    Scope = "--- configuration value goes here ---",
                    Tenant = "--- configuration value goes here ---"
                },
                Crypto = new CryptoSettings()
                {
                    Salt = "SaltySweedishSeaDog",
                    Secret = "ShhhhSecretSquirrel"
                },
                LevySubmission = new LevySubmissionsSettings
                {
                    HmrcApiBaseUrlSetting = new HmrcApiBaseUrlConfig
                    {
                        HmrcApiBaseUrl = "--- configuration value goes here ---"
                    },
                    LevySubmissionsApiConfig = new LevySubmissionsApiConfiguration
                    {
                        ApiBaseUrl = "",
                        ClientId = "",
                        ClientSecret = "",
                        IdentifierUri = "",
                        Tenant = "",
                        LevyTokenCertificatethumprint = ""
                    }
                },
                SiteConnector = new SiteConnectorSettings()
                {
                    ClientId = "--- configuration value goes here ---",
                    ClientSecret = "--- configuration value goes here ---",
                    IdentifierUri = "--- configuration value goes here ---",
                    Tenant = "--- configuration value goes here ---"
                },
                HashingService = new HashingServiceConfig
                {
                    AllowedCharacters = "",
                    Hashstring = ""
                },
                Site = new SiteSettings()
                {
                    BaseUrls = "--- configuration|value goes here ---"
                }
            };
        }

        private const string SiteConfigFileName = "SFA.DAS.Support.EAS";

        private WebConfiguration _unit;

        [Test]
        public void ItShouldDeserialiseFaithfuly()
        {
            var json = JsonConvert.SerializeObject(_unit);
            Assert.IsNotNull(json);
            var actual = JsonConvert.DeserializeObject<WebConfiguration>(json);
            Assert.AreEqual(json, JsonConvert.SerializeObject(actual));
        }

        [Test]
        public void ItShouldDeserialize()
        {
            var json = JsonConvert.SerializeObject(_unit);
            Assert.IsNotNull(json);
            var actual = JsonConvert.DeserializeObject<WebConfiguration>(json);
            Assert.IsNotNull(actual);
        }


        [Test]
        public void ItShouldGenerateASchema()
        {
            var provider = new FormatSchemaProvider();
            var jSchemaGenerator = new JSchemaGenerator();
            jSchemaGenerator.GenerationProviders.Clear();
            jSchemaGenerator.GenerationProviders.Add(provider);
            var actual = jSchemaGenerator.Generate(typeof(WebConfiguration));


            Assert.IsNotNull(actual);
            // hack to leverage format as 'environmentVariable'
            var schemaString = actual.ToString().Replace($"\"format\":", "\"environmentVariable\":");
            Assert.IsNotNull(schemaString);
            File.WriteAllText($@"{AppDomain.CurrentDomain.BaseDirectory}\{SiteConfigFileName}.schema.json",
                schemaString);
        }

        [Test]
        public void ItShouldSerialize()
        {
            var json = JsonConvert.SerializeObject(_unit);
            Assert.IsFalse(string.IsNullOrWhiteSpace(json));


            File.WriteAllText($@"{AppDomain.CurrentDomain.BaseDirectory}\{SiteConfigFileName}.json", json);
        }
    }
}