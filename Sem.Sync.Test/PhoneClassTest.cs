﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PhoneClassTest.cs" company="Sven Erik Matzen">
//   Copyright (c) Sven Erik Matzen. GNU Library General Public License (LGPL) Version 2.1.
// </copyright>
// <summary>
//   Summary description for PersonNameClassTest
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sem.Sync.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Sem.Sync.SyncBase.DetailData;

    /// <summary>
    /// Summary description for PersonNameClassTest
    /// </summary>
    [TestClass]
    public class PhoneClassTest
    {
        #region Public Methods

        /// <summary>
        /// The test comparison.
        /// </summary>
        [TestMethod]
        public void TestComparison()
        {
            var phone1 = new PhoneNumber("+49(1234)5678-9");
            var phone2 = new PhoneNumber("+49(1234)5678-9");

            Assert.AreEqual(phone1, phone2);
            Assert.IsTrue(phone1 == phone2);
            Assert.IsFalse(phone1 != phone2);

            phone2 = new PhoneNumber("+41(1234)5678-9");
            Assert.AreNotEqual(phone1, phone2);
            Assert.IsFalse(phone1 == phone2);
            Assert.IsTrue(phone1 != phone2);

            phone2 = new PhoneNumber("+49(1231)5678-9");
            Assert.AreNotEqual(phone1, phone2);
            Assert.IsFalse(phone1 == phone2);
            Assert.IsTrue(phone1 != phone2);

            phone2 = new PhoneNumber("+49(1234)5671-9");
            Assert.AreNotEqual(phone1, phone2);
            Assert.IsFalse(phone1 == phone2);
            Assert.IsTrue(phone1 != phone2);

            phone2 = new PhoneNumber("1234");
            Assert.AreNotEqual(phone1, phone2);
            Assert.IsFalse(phone1 == phone2);
            Assert.IsTrue(phone1 != phone2);

            phone1 = new PhoneNumber("1234");
            Assert.AreEqual(phone1, phone2);
            Assert.IsTrue(phone1 == phone2);
            Assert.IsFalse(phone1 != phone2);

            phone1 = new PhoneNumber("2234");
            Assert.AreNotEqual(phone1, phone2);
            Assert.IsFalse(phone1 == phone2);
            Assert.IsTrue(phone1 != phone2);
        }

        #endregion
    }
}