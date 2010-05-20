﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ContactService.svc.cs" company="Sven Erik Matzen">
//   Copyright (c) Sven Erik Matzen. GNU Library General Public License (LGPL) Version 2.1.
// </copyright>
// <summary>
//   Service implementation for the <see cref="IContactService" /> interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sem.Sync.OnlineStorage2
{
    using System.Collections.Generic;
    using System.Linq;

    using Sem.GenericHelpers;
    using Sem.Sync.Connector.Filesystem;
    using Sem.Sync.SyncBase;
    using Sem.Sync.SyncBase.Helpers;

    /// <summary>
    /// Service implementation for the <see cref="IContactService"/> interface.
    /// </summary>
    public class ContactService : IContactService
    {
        #region Constants and Fields

        /// <summary>
        ///   The file system path to store the information.
        /// </summary>
        private readonly string storagePath = "C:\\ContactsServerData\\Contacts.xml";

        #endregion

        #region Implemented Interfaces

        #region IContactService

        /// <summary>
        /// Reads the contacts from a contact store specified in the parameter <paramref name="clientFolderName"/>.
        /// </summary>
        /// <param name="clientFolderName">
        /// The client folder name. 
        /// </param>
        /// <param name="startElementIndex">
        /// the 0 based first element index for the result set
        /// </param>
        /// <param name="countOfElements">
        /// the number of elements to read
        /// </param>
        /// <returns>
        /// A contact list container with the contacts from the folder. 
        /// </returns>
        public ContactListContainer GetAll(string clientFolderName, int startElementIndex, int countOfElements)
        {
            var contactList = new ContactClient().GetAll(this.storagePath).ToStdContacts();

            var stdContacts = new ContactListContainer
                {
                    ContactList = Tools.SaveToString((from x in contactList select x).Take(countOfElements).ToList()), 
                    FirstElementIndex = startElementIndex, 
                    TotalElements = contactList.Count
                };

            return stdContacts;
        }

        /// <summary>
        /// Writes contacts to a contact store specified in the parameter <paramref name="clientFolderName"/>.
        /// </summary>
        /// <param name="elements">
        /// The elements to be written. 
        /// </param>
        /// <param name="clientFolderName">
        /// The client folder name.  
        /// </param>
        /// <param name="skipIfExisting">
        /// Ignored in this implementation. 
        /// </param>
        /// <returns>
        /// A value indicating whether the operation was successfull. 
        /// </returns>
        public bool WriteFullList(ContactListContainer elements, string clientFolderName, bool skipIfExisting)
        {
            new ContactClient().WriteRange(
                Tools.LoadFromString<List<StdContact>>(elements.ContactList).ToStdElements(), this.storagePath);
            return true;
        }

        #endregion

        #endregion
    }
}