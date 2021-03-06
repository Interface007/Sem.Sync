﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GenericToolsTest.cs" company="Sven Erik Matzen">
//   Copyright (c) Sven Erik Matzen. GNU Library General Public License (LGPL) Version 2.1.
// </copyright>
// <summary>
//   The recursive test class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sem.Sync.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Sem.GenericHelpers;
    using Sem.GenericHelpers.Entities;
    using Sem.GenericHelpers.Exceptions;
    using Sem.Sync.SyncBase.DetailData;
    using Sem.Sync.SyncBase.Helpers;
    using Sem.Sync.Test.DataGenerator;

    /// <summary>
    /// The recursive test class.
    /// </summary>
    public class RecursiveTestClass
    {
        #region Properties

        /// <summary>
        /// Gets or sets s.
        /// </summary>
        public string[] s { get; set; }

        /// <summary>
        /// Gets or sets t.
        /// </summary>
        public RecursiveTestClass[] t { get; set; }

        #endregion
    }

    /// <summary>
    /// The complex test class.
    /// </summary>
    public class ComplexTestClass
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexTestClass"/> class.
        /// </summary>
        public ComplexTestClass()
        {
            this.myProp1 = new NetworkCredential("sven", "geheim1", "domain");

            this.myProp2 = null;

            this.myProp3 = new[]
                {
                   new NetworkCredential("sven", "geheim2", "domain"), new NetworkCredential("sven", "geheim3", "domain")
                };

            this.myProp4 = new List<NetworkCredential>
                {
                   new NetworkCredential("sven", "geheim4", "domain"), 
                  new NetworkCredential("sven", "geheim5", "domain")
                };

            this.myProp5 = new Dictionary<string, NetworkCredential>
                {
                    { "key1", new NetworkCredential("sven", "geheim6", "domain") }, 
                    { "key2", new NetworkCredential("sven", "geheim7", "domain") }
                };

            this.myProp6 = new DateTime(2009, 7, 8);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets myProp1.
        /// </summary>
        public NetworkCredential myProp1 { get; set; }

        /// <summary>
        /// Gets or sets myProp10.
        /// </summary>
        public ComplexTestClass myProp10 { get; set; }

        /// <summary>
        /// Gets or sets myProp2.
        /// </summary>
        public NetworkCredential myProp2 { get; set; }

        /// <summary>
        /// Gets or sets myProp3.
        /// </summary>
        public NetworkCredential[] myProp3 { get; set; }

        /// <summary>
        /// Gets or sets myProp4.
        /// </summary>
        public List<NetworkCredential> myProp4 { get; set; }

        /// <summary>
        /// Gets or sets myProp5.
        /// </summary>
        public Dictionary<string, NetworkCredential> myProp5 { get; set; }

        /// <summary>
        /// Gets or sets myProp6.
        /// </summary>
        public DateTime? myProp6 { get; set; }

        /// <summary>
        /// Gets or sets myProp7.
        /// </summary>
        public DateTime? myProp7 { get; set; }

        /// <summary>
        /// Gets or sets myProp8.
        /// </summary>
        public int? myProp8 { get; set; }

        /// <summary>
        /// Gets or sets myProp9.
        /// </summary>
        public DateTime myProp9 { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// The as string.
        /// </summary>
        /// <returns>
        /// </returns>
        public DateTime AsString()
        {
            return this.myProp6 ?? new DateTime(1900, 1, 1);
        }

        /// <summary>
        /// The as string.
        /// </summary>
        /// <param name="defaultValue">
        /// The default value.
        /// </param>
        /// <returns>
        /// </returns>
        public DateTime AsString(string defaultValue)
        {
            return this.myProp6 ?? DateTime.Parse(defaultValue);
        }

        #endregion
    }

    /// <summary>
    /// Tests the functionality of the generic (non-project-specific) library
    /// </summary>
    [TestClass]
    public class GenericToolsTest
    {
        /// <summary>
        ///   Gets or sets the test context which provides
        ///   information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// The check cache path generation.
        /// </summary>
        [TestMethod]
        public void CheckCachePathGeneration()
        {
            Assert.AreEqual("da39a3ee5e6b4b0d3255bfef95601890afd80709", Tools.GetSha1Hash(string.Empty));
            Assert.AreEqual("da39a3ee5e6b4b0d3255bfef95601890afd80709", Tools.GetSha1Hash(null));
            Assert.AreEqual("48af14dbae9dd04377e61d02ac07126e7e75e65e", Tools.GetSha1Hash("a little test"));
        }

        /// <summary>
        /// The check quoted printable test.
        /// </summary>
        [TestMethod]
        public void CheckQuotedPrintableTest()
        {
            Assert.AreEqual(string.Empty, Tools.EncodeToQuotedPrintable(string.Empty));
            Assert.AreEqual(string.Empty, Tools.EncodeToQuotedPrintable(null));
            Assert.AreEqual("a little test", Tools.EncodeToQuotedPrintable("a little test"));
            Assert.AreEqual("g&#=dc=e4=d6(?%&j8?=e4=f6=fc", Tools.EncodeToQuotedPrintable("g&#ÜäÖ(?%&j8?äöü"));

            Assert.AreEqual("g&#ÜäÖ(?%&j8?äöü", Tools.DecodeFromQuotedPrintable("g&#=dc=e4=d6(?%&j8?=e4=f6=fc"));
            Assert.AreEqual("a little test", Tools.DecodeFromQuotedPrintable("a little test"));
            Assert.AreEqual(string.Empty, Tools.DecodeFromQuotedPrintable(null));
            Assert.AreEqual(string.Empty, Tools.DecodeFromQuotedPrintable(string.Empty));
        }

        /// <summary>
        /// The compressed serialization.
        /// </summary>
        [TestMethod]
        public void CompressedSerialization()
        {
            var testData = Contacts.GetStandardContactList(true);
            var serialized = Tools.SaveToString(testData, true);
            var deserilizd = Tools.LoadFromString<List<StdContact>>(serialized);

            foreach (var stdContact in testData)
            {
                foreach (var propertyName in Tools.GetPropertyList(string.Empty, typeof(StdContact)))
                {
                    Assert.AreEqual(
                        Tools.GetPropertyValueString(stdContact, propertyName), 
                        Tools.GetPropertyValueString(deserilizd.GetElementById<StdContact>(stdContact.Id), propertyName));
                }
            }
        }

        /// <summary>
        /// The compression test.
        /// </summary>
        [TestMethod]
        public void CompressionTest()
        {
            var probe = string.Empty;
            for (var i = 0; i < 19999; i++)
            {
                Assert.AreEqual(probe, Tools.Decompress(Tools.Compress(probe)));
                probe += "-";
            }

            probe = string.Empty;
            for (byte i = 0; i < 255; i++)
            {
                Assert.AreEqual(probe, Tools.Decompress(Tools.Compress(probe)));
                probe += Encoding.UTF8.GetString(new[] { i });
            }
        }

        /// <summary>
        /// The crypto test.
        /// </summary>
        [TestMethod]
        public void CryptoTest()
        {
            var input = "11234567890kjhgfdsaqwertzuiomnbvcxywertzuio8ztrfdxy34r5tzhuj!%&WDFCVZUJKOIJHNk098u7ztrfdä#ölkjh";
            var key = "<RSAKeyValue><Modulus>jApiPKqlogJQ+z1OOiFqFfxpwK54o8wqhNKQ6rZ9BuUPIwYcVyaVON98M7i8kcaecLfnnQGgzoXy/PNUEH+BeKevz6qa5y3nRWkKDaBJH+2QFcmKhxJ4RBAkGVbWMzRyQb4IvAy6W5btUJWs/9lNskrxINAP3Q2/dvofaido+9KEFc9ubToMQaAxA/SXEFy0fDHTp+uEhFPESef0EFvCtmIAxUVqep3loo2XqKOhF+aqZgpY8y2QLp0koe8AJuE+BH194fJZc1kgMnbxdAEEZP0smHJ0VhfFMCBpzyvgGQWT8OiSOsmH1RUW+YzoWiVtAbtK5eMw27/uUxSDLiSpgQ==</Modulus><Exponent>AQAB</Exponent><P>wSJjw5aGPQqq22BR+oyGNoWHTS+FelF1hVggPauMIQBBQnMxkiNukPzxXfTXI6U1DHV8yohRddn3RfvMnDXyMDSUmiYhvEKPv0+Ko91pDEW7NBC0lF2ivRLRSVNN7ymqGxuHLZl9L8cGZbAd3kcOved2kwIpCTjIBZPF9FvO6W0=</P><Q>uZ/D4uFjN/TNGigBEMDnLC/QKvaKk6k5okvD5nucrZ4E4uZciTSCNVovGPUcVHaF8yVGyr2/Q/HJFqhYiVtJW0dgrgW2MGNH1zkuGdAIOWfXIJJ98OYjbSalmfdfH+Kvm2EMeobRzO2sgiXyk5FcXjVE9qv6lHsnO7aPo+vPZ+U=</Q><DP>pEohvW5dMK3TW5wbAFvri5OY3fLPw5Zptw2ZF3zHTgdOfD1LbLoRQuq4U6mEHUFa2AdVKWA+k1bf/rtMeZF2PMVtp5dKWT/x0dbS48PjqVnj/k07n33rgpTwTUS85fZ2SmrnWcXYdP0DlxryvXOKucac2j8bM0oG9J+Y4935LB0=</DP><DQ>LI5H48a8HStarAOcNdxH4Rhc/GMPYmBFYEqVbFaRhi8e9yPaZGjBNHNASNpVAYUto+53rqSbK4D3BsRD9DyAQDPC5iKi17yM+wFTqoh/4N+nCL+BEXj8We/j4jA5mhq8kixaZXLFG06VkOvw7TEAHfDla9xeGpPxF+k7U1p//20=</DQ><InverseQ>AM+xNS08+xznvloPJSwrOTPXbRYgJwM0pedDT3xWxQ1PhuO9uy0XtoVbsOnsmZ34liUamkXbmUMkmtgc4545lxIGkhvsh+pmbTShBtnn+nKYDRE5/pgmOjGVG7jILb4xpqh7jylS+1wqlxmOIDK2nzVW1e8DJ2XX6gtZw4SJifw=</InverseQ><D>PSUfhYug7F8Eit5WtKz4TAc9CYNka2huvDXQTptFdeg3trwpTagsCXwTF+Y2d5P4hBDYUUZvtxznXqjD8LwrhMn6yrqcDgqN61GUsSRmKUmp2sl38cgPuPAvQt0Wg58HsErQN4N7Lxh0H/ZqZf+0m/96zy/pbURecA1///KbNTAtWVhtx1/b22gDqNR9WKzz0NZwk/+F/01flGEfu5ui1o/7O0n7hJGAk4dff8Mc8hYC6eTI9WIc/WeW7unexe5mSJTols1s6kIAIJxKsQz8YcuF5bB3q1D0T3TvphRVJKo59FxLJyqQaqBUIrNqbvQx8/rzm2rLTtwfYS0zFVpdUQ==</D></RSAKeyValue>";
            var publicOnly = SimpleCrypto.ExtractPublic(key);

            Assert.AreEqual(input, SimpleCrypto.DecryptString(SimpleCrypto.EncryptString(input, key), key));
            Assert.AreEqual(input, SimpleCrypto.DecryptString(SimpleCrypto.EncryptString(input, publicOnly), key));

            key = SimpleCrypto.GenerateNewKey(2048);
            publicOnly = SimpleCrypto.ExtractPublic(key);
            Assert.AreEqual(input, SimpleCrypto.DecryptString(SimpleCrypto.EncryptString(input, key), key));
            Assert.AreEqual(input, SimpleCrypto.DecryptString(SimpleCrypto.EncryptString(input, publicOnly), key));

            for (var i = 0; i < 5; i++)
            {
                input += input;
            }

            Assert.AreEqual(input, SimpleCrypto.DecryptString(SimpleCrypto.EncryptString(input, key), key));
            Assert.AreEqual(input, SimpleCrypto.DecryptString(SimpleCrypto.EncryptString(input, publicOnly), key));

            input = "1234567890";
            Assert.AreEqual(input, SimpleCrypto.DecryptString(SimpleCrypto.EncryptString(input, publicOnly), key));

            ////key = SimpleCrypto.GenerateNewKey(4096);
            ////publicOnly = SimpleCrypto.ExtractPublic(key);
            ////Assert.AreEqual(input, SimpleCrypto.DecryptString(SimpleCrypto.EncryptString(input, key), key));
            ////Assert.AreEqual(input, SimpleCrypto.DecryptString(SimpleCrypto.EncryptString(input, key), publicOnly));
        }

        /// <summary>
        /// The get property nulls.
        /// </summary>
        [TestMethod]
        public void GetPropertyNulls()
        {
            var x = new ComplexTestClass();
            Assert.AreEqual("default", x.NewIfNull().myProp2.NewIfNull().Domain.DefaultIfNullOrEmpty("default"));
        }

        /// <summary>
        /// Tests the functionality to read object paths from an object
        /// </summary>
        [TestMethod]
        public void GetPropertyValueTest()
        {
            var testData = Contacts.GetStandardContactList(true);

            Assert.AreEqual(testData[2].Name.FirstName, Tools.GetPropertyValueString(testData[2], "Name.FirstName"));
            Assert.AreEqual(
                testData[2].ExternalIdentifier.GetProfileId(ProfileIdentifierType.Default).ToString(), 
                Tools.GetPropertyValueString(testData, "[2].ExternalIdentifier.[Default]"));

            var testClass =
                new
                    {
                        arr = new[] { "str1", "str2", "str3" }, 
                        arrayOfArrays = new[] { new[] { "str11", "str12", "str13" }, new[] { "str21", "str22", "str23" } }, 
                        arrayOfClasses =
                            new[]
                                {
                                    new { a = "str31", b = "str32", c = "str33" }, 
                                    new { a = "str41", b = "str42", c = "str43" }, 
                                }
                    };

            Assert.AreEqual(testClass.arr[1], Tools.GetPropertyValueString(testClass, "arr[1]"));

            //// Assert.AreEqual(testClass.arrayOfArrays[1][2], Tools.GetPropertyValueString(testClass, "arrayOfArrays[1][2]"));
            Assert.AreEqual(
                testClass.arrayOfClasses[1].a, Tools.GetPropertyValueString(testClass, "arrayOfClasses[1].a"));
        }

        /// <summary>
        /// The get property value test co ba 1.
        /// </summary>
        [TestMethod]
        public void GetPropertyValueTestCoBa1()
        {
            var t = new RecursiveTestClass { s = new[] { "hallo", "123" }, t = new RecursiveTestClass[5] };

            t.t[0] = new RecursiveTestClass { s = new[] { "Ichbineintest", "nocheintest" } };

            var s = Tools.GetPropertyValue(t, ".t[0].s[1]") as string;
            Assert.AreEqual("nocheintest", s);

            s = Tools.GetPropertyValue(t, ".s[0]") as string;
            Assert.AreEqual("hallo", s);
        }

        /// <summary>
        /// The get property value test co ba 2.
        /// </summary>
        [TestMethod]
        public void GetPropertyValueTestCoBa2()
        {
            var x = new ComplexTestClass();

            Assert.AreEqual("geheim1", Tools.GetPropertyValue(x, "myProp1.Password"));
            Assert.IsNull(Tools.GetPropertyValue(x, "myProp2.Password"));
            Assert.AreEqual("geheim3", Tools.GetPropertyValue(x, "myProp3[1].Password"));
            Assert.AreEqual("geheim4", Tools.GetPropertyValue(x, "myProp4[0].Password"));
            Assert.AreEqual("geheim7", Tools.GetPropertyValue(x, "myProp5[key2].Password"));

            Assert.AreEqual(new DateTime(2009, 7, 8), Tools.GetPropertyValue(x, "myProp6"));
            Assert.AreEqual("08.07.2009 00:00:00", Tools.GetPropertyValueString(x, "myProp6"));
            Assert.AreEqual("08.07.2009 00:00:00", Tools.GetPropertyValueString(x, "AsString()"));
            Assert.AreEqual("08.07.2009 00:00:00", Tools.GetPropertyValueString(x, "AsString(----)"));

            Assert.IsNull(Tools.GetPropertyValue(x, "myProp7"));
            Assert.AreEqual(string.Empty, Tools.GetPropertyValueString(x, "myProp7"));

            Assert.IsNull(Tools.GetPropertyValue(x, "myProp4[20?].Password"));
            Assert.IsNull(Tools.GetPropertyValue(x, "myProp5[nonexistingkey?].Password"));
        }

        /// <summary>
        /// The simple exception test.
        /// </summary>
        [TestMethod]
        public void SimpleExceptionTest()
        {
            try
            {
                File.WriteAllText("invalid Path:\\\\\\\\::?!", @"test");
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex);
            }
        }

        /// <summary>
        /// The technical exception test.
        /// </summary>
        /// <exception cref="TechnicalException">
        /// </exception>
        [TestMethod]
        public void TechnicalExceptionTest()
        {
            var exceptionText = string.Empty;

            try
            {
                try
                {
                    File.WriteAllText("invalid Path:\\\\\\\\::?!", @"test");
                }
                catch (Exception ex)
                {
                    throw new TechnicalException(
                        "bad file name", 
                        ex, 
                        new KeyValuePair<string, object>("myString", "hello world"), 
                        new KeyValuePair<string, object>("myInt", 49));
                }
            }
            catch (Exception ex)
            {
                exceptionText = ExceptionHandler.HandleException(ex);
            }

            Assert.IsTrue(exceptionText.Contains("<string name=\"myString\">hello world</string>"));
            Assert.IsTrue(exceptionText.Contains("<int name=\"myInt\">49</int>"));
            Assert.IsTrue(exceptionText.Contains("QTAgent32.exe</ExecutingMainModule"));
            Assert.IsTrue(exceptionText.Contains("<SpecificInformation>Sem.GenericHelpers.Exceptions.TechnicalException: bad file name"));
            Assert.IsTrue(
                exceptionText.Contains("<SpecificInformation>System.ArgumentException: Illegales Zeichen im Pfad.") ||
                exceptionText.Contains("<SpecificInformation>System.ArgumentException: Illegal characters in path."));
        }

        /// <summary>
        /// The test crypted credentials.
        /// </summary>
        [TestMethod]
        public void TestCryptedCredentials()
        {
            var credentials = new Credentials
                {
                   LogOnDomain = "domain", LogOnPassword = "password", LogOnUserId = "hello" 
                };

            Assert.AreEqual("domain", credentials.LogOnDomain);
            Assert.AreEqual("password", credentials.LogOnPassword);
            Assert.AreEqual("hello", credentials.LogOnUserId);

            var xmlSerialized = Tools.SaveToString(credentials);
            Assert.IsFalse(xmlSerialized.Contains("password"));

            var binSerializer = new BinaryFormatter();
            using (var contentStream = new MemoryStream())
            {
                binSerializer.Serialize(contentStream, credentials);
                var data = contentStream.ToArray();
                var dataString = Encoding.ASCII.GetString(data);

                Assert.IsFalse(dataString.Contains("password"));
            }
        }

        /// <summary>
        /// The test gender by text.
        /// </summary>
        [TestMethod]
        public void TestGenderByText()
        {
            Assert.AreEqual(Gender.Female, SyncTools.GenderByText("Mrs."));
            Assert.AreEqual(Gender.Female, SyncTools.GenderByText("Frau"));
            Assert.AreEqual(Gender.Male, SyncTools.GenderByText("Mr."));
            Assert.AreEqual(Gender.Male, SyncTools.GenderByText("Herr"));
            Assert.AreEqual(Gender.Unspecified, SyncTools.GenderByText("something"));
            Assert.AreEqual(Gender.Unspecified, SyncTools.GenderByText(null));
            Assert.AreEqual(Gender.Unspecified, SyncTools.GenderByText(string.Empty));
        }

        /// <summary>
        /// The test sync tools normalize file name.
        /// </summary>
        [TestMethod]
        public void TestSyncToolsNormalizeFileName()
        {
            Assert.AreEqual(@"hello.txt", SyncTools.NormalizeFileName(@"hello.txt"));
            Assert.AreEqual(@"hello1.txt", SyncTools.NormalizeFileName(@"hello1.txt"));
            Assert.AreEqual(@"hello..txt", SyncTools.NormalizeFileName(@"hello..txt"));
            Assert.AreEqual(@"hello_.txt", SyncTools.NormalizeFileName(@"hello?.txt"));
            Assert.AreEqual(@"hello_.txt", SyncTools.NormalizeFileName(@"hello:.txt"));
            Assert.AreEqual(@"hello_.txt", SyncTools.NormalizeFileName(@"hello\.txt"));
            Assert.AreEqual(@"hello_.txt", SyncTools.NormalizeFileName(@"hello/.txt"));
        }
    }
}