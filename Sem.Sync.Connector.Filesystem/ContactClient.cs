﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ContactClient.cs" company="Sven Erik Matzen">
//   Copyright (c) Sven Erik Matzen. GNU Library General Public License (LGPL) Version 2.1.
// </copyright>
// <summary>
//   This class is the client class for handling contacts persisted to the file system
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sem.Sync.Connector.Filesystem
{
    #region usings

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Xml.Serialization;

    using Sem.GenericHelpers;
    using Sem.Sync.Connector.Filesystem.Properties;
    using Sem.Sync.SyncBase;
    using Sem.Sync.SyncBase.Attributes;
    using Sem.Sync.SyncBase.DetailData;
    using Sem.Sync.SyncBase.Helpers;

    #endregion usings

    /// <summary>
    /// This class is the client class for handling contacts persisted to the file system
    /// </summary>
    [ClientStoragePathDescription(Mandatory = true, Default = "{FS:WorkingFolder}\\Contacts.xml", 
        ReferenceType = ClientPathType.FileSystemFileNameAndPath)]
    [ConnectorDescription(DisplayName = "Filesystem one big Xml file")]
    public class ContactClient : StdClient
    {
        #region Constants and Fields

        /// <summary>
        ///   This is the formatter instance for serializing the list of contacts.
        /// </summary>
        private static readonly XmlSerializer ContactListFormatter = new XmlSerializer(typeof(List<StdContact>));

        #endregion

        #region Properties

        /// <summary>
        ///   Gets the user friendly name of the connector
        /// </summary>
        public override string FriendlyClientName
        {
            get
            {
                return "FileSystem Contact Connector - one file for all contacts";
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// This overrides the event of just before accessing the storage location to 
        ///   ensure the path for saving/loading data does exist.
        /// </summary>
        /// <param name="clientFolderName">
        /// The client folder name for the destination/source of the contact file. 
        /// </param>
        protected override void BeforeStorageAccess(string clientFolderName)
        {
            Tools.EnsurePathExist(Path.GetDirectoryName(clientFolderName.Replace("\n", string.Empty).Trim()));
        }

        /// <summary>
        /// Overrides the method to read the full list of data.
        /// </summary>
        /// <param name="clientFolderName">
        /// the full name including path of the file that does contain the contacts.
        /// </param>
        /// <param name="result">
        /// A list of StdElements that will get the new imported entries.
        /// </param>
        /// <returns>
        /// The list with the added contacts
        /// </returns>
        protected override List<StdElement> ReadFullList(string clientFolderName, List<StdElement> result)
        {
            if (File.Exists(clientFolderName))
            {
                using (var file = new FileStream(clientFolderName, FileMode.Open))
                {
                    if (file.Length > 0)
                    {
                        result = ((List<StdContact>)ContactListFormatter.Deserialize(file)).ToStdElements();
                        CleanUpEntities(result);
                    }

                    this.LogProcessingEvent(
                        string.Format(CultureInfo.CurrentCulture, Resources.uiElementsRead, result.Count));
                }
            }

            return result;
        }

        /// <summary>
        /// Overrides the method to write the full list of data.
        /// </summary>
        /// <param name="elements">
        /// The elements to be exported. 
        /// </param>
        /// <param name="clientFolderName">
        /// the full name including path of the file that will get the contacts while exporting data.
        /// </param>
        /// <param name="skipIfExisting">
        /// this value is not used in this client.
        /// </param>
        protected override void WriteFullList(List<StdElement> elements, string clientFolderName, bool skipIfExisting)
        {
            using (var file = new FileStream(clientFolderName, FileMode.Create))
            {
                try
                {
                    CleanUpEntities(elements);
                    ContactListFormatter.Serialize(file, elements.ToStdContacts());
                }
                catch (Exception ex)
                {
                    this.LogProcessingEvent(ex.Message);
                }
            }
        }

        #endregion
    }
}