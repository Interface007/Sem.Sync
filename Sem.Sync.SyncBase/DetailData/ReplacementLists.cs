﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReplacementLists.cs" company="Sven Erik Matzen">
//   Copyright (c) Sven Erik Matzen. GNU Library General Public License (LGPL) Version 2.1.
// </copyright>
// <summary>
//   A replacements list does contain different lists of key value pairs that
//   define replacement configuration to use a standardized string for one or
//   multiple input strings. E.g. you  can write the company name "SDX AG" in
//   many ways like "Sdx Ag" or "SDX-AG". To have a normalized set or strings
//   inside the internal data, these lists provide translations.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sem.Sync.SyncBase.DetailData
{
    using System;
    using System.Collections.Generic;

    using Sem.GenericHelpers.Entities;

    /// <summary>
    /// A replacements list does contain different lists of key value pairs that 
    ///   define replacement configuration to use a standardized string for one or
    ///   multiple input strings. E.g. you  can write the company name "SDX AG" in
    ///   many ways like "Sdx Ag" or "SDX-AG". To have a normalized set or strings
    ///   inside the internal data, these lists provide translations.
    /// </summary>
    [Serializable]
    public class ReplacementLists
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "ReplacementLists" /> class.
        /// </summary>
        public ReplacementLists()
        {
            this.BusinessCompanyName = new List<KeyValuePair>();
            this.BusinessHomepage = new List<KeyValuePair>();
            this.City = new List<KeyValuePair>();
        }

        #endregion

        #region Properties

        /// <summary>
        ///   Gets or sets the list of permutations of company names with replacement strings.
        /// </summary>
        public List<KeyValuePair> BusinessCompanyName { get; set; }

        /// <summary>
        ///   Gets or sets the list of permutations of business URLs with replacement strings.
        /// </summary>
        public List<KeyValuePair> BusinessHomepage { get; set; }

        /// <summary>
        ///   Gets or sets the list of permutations of city names with replacement strings.
        /// </summary>
        public List<KeyValuePair> City { get; set; }

        #endregion
    }
}