﻿namespace Sem.Sync.Test.Contracts
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Sem.GenericHelpers.Contracts;
    using Sem.GenericHelpers.Contracts.Exceptions;

    /// <summary>
    ///This is a test class for BouncerTest and is intended
    ///to contain all BouncerTest Unit Tests
    ///</summary>
    [TestClass]
    public class BouncerIsOneOfTest
    {
        [TestMethod]
        public void CheckParameterIsOneOfMustFail1()
        {
            Assert.IsFalse(Rules.IsOneOf<string>().CheckExpression("1", new[] { "2", "3" }));
        }
        [TestMethod]
        public void CheckParameterIsOneOfMustFail2()
        {
            Assert.IsFalse(Rules.IsOneOf<string>().CheckExpression(null, new[] { "2", "3" }));
        }

        [TestMethod]
        public void CheckParameterIsOneOfMustPass1()
        {
            Assert.IsTrue(Rules.IsOneOf<string>().CheckExpression("1", new[] { "2", "1" }));
        }

        [TestMethod]
        public void CheckParameterIsOneOfMustPass2()
        {
            Assert.IsTrue(Rules.IsOneOf<string>().CheckExpression("1", new[] { "1", "2" }));
        }
    }
}