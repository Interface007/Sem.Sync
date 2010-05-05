﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleReport.cs" company="Sven Erik Matzen">
//     Copyright (c) Sven Erik Matzen. GNU Library General Public License (LGPL) Version 2.1.
// </copyright>
// <author>Sven Erik Matzen</author>
// <summary>
//   This client is a write only client that aggregates the information to some statistical information.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sem.Sync.Connector.Statistic
{
    #region usings

    using System.Collections.Generic;
    using System.IO;

    using GenericHelpers;
    using GenericHelpers.Entities;

    using SyncBase;
    using SyncBase.Attributes;

    #endregion usings

    /// <summary>
    /// This client is a write only client that aggregates the information to some statistical information.
    /// </summary>
    [ConnectorDescription(
        CanReadContacts = false, 
        CanWriteContacts = true, 
        CanReadCalendarEntries = false, 
        CanWriteCalendarEntries = true, 
        IsGeneric = true,
        NeedsCredentials = false, 
        DisplayName = "Simple Report")]
    [ClientStoragePathDescription(ReferenceType = ClientPathType.FileSystemPath)]
    public class SimpleReport : StdClient
    {
        /// <summary>
        /// Writes a range of elements to the standard connector.
        /// </summary>
        /// <param name="elements"> The elements. </param>
        /// <param name="clientFolderName"> The client folder name. </param>
        public override void AddRange(List<StdElement> elements, string clientFolderName)
        {
            this.WriteFullList(elements, clientFolderName, true);
        }
        
        /// <summary>
        /// Writing will write a simple XML with some statistical information.
        /// </summary>
        /// <param name="elements">the list of elements that should be written to the target system.</param>
        /// <param name="clientFolderName">the information to where inside the source the elements should be written - 
        /// This does not need to be a real "path", but need to be something that can be expressed as a string</param>
        /// <param name="skipIfExisting">specifies whether existing elements should be updated or simply left as they are</param>
        protected override void WriteFullList(List<StdElement> elements, string clientFolderName, bool skipIfExisting)
        {
            this.LogProcessingEvent("preparing data...");
            elements.ForEach(x => x.NormalizeContent());

            this.LogProcessingEvent("calculating statistics...");

            var statistic = new SimpleStatisticResult
                {
                    NumberOfElements = elements.Count,
                    PropertyUsage = PropertyUsageCounter.GetPropertyUsage(elements),
                    ValueAnalysis = new ValueAnalysisCounter(elements),
                };

            this.LogProcessingEvent("saving statistic file...");
            Tools.SaveToFile(statistic, Path.Combine(clientFolderName, this.FriendlyClientName + ".xml"), typeof(KeyValuePair), typeof(ValueAnalysisCounter));

            this.LogProcessingEvent("writing finished");
        }
    }
}