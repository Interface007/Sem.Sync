﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IExtendedReader.cs" company="Sven Erik Matzen">
//   Copyright (c) Sven Erik Matzen. GNU Library General Public License (LGPL) Version 2.1.
// </copyright>
// <summary>
//   Provides access to additional data collection methods
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sem.Sync.SyncBase.Interfaces
{
    using System.Collections.Generic;

    using Sem.Sync.SyncBase.DetailData;

    /// <summary>
    /// Provides access to additional data collection methods
    /// </summary>
    public interface IExtendedReader
    {
        #region Public Methods

        /// <summary>
        /// Reads additional contact relations with known contacts.
        /// </summary>
        /// <param name="contactToFill"> The contact to fill.  </param>
        /// <param name="baseline"> The baseline to lookup the contact id.  </param>
        /// <returns> the "enriched" element  </returns>
        StdElement FillContacts(StdElement contactToFill, ICollection<MatchingEntry> baseline);

        /// <summary>
        /// Reads all contact relations for all the specified contacts in the <paramref name="contactToFill"/> collection.
        /// Use this to implement connectors that are capable to read multiple contact relation lists in one operation.
        /// </summary>
        /// <param name="contactToFill"></param>
        /// <param name="baseline"></param>
        void FillAllContacts(ICollection<StdElement> contactToFill, ICollection<MatchingEntry> baseline);

        #endregion
    }
}