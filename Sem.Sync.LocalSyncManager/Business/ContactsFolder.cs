﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ContactsFolder.cs" company="Sven Erik Matzen">
//   Copyright (c) Sven Erik Matzen. GNU Library General Public License (LGPL) Version 2.1.
// </copyright>
// <summary>
//   Represents a contacts folder for binding UI elements
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sem.Sync.LocalSyncManager.Business
{
    using System.Collections.Generic;
    using System.ComponentModel;

    using Sem.Sync.SyncBase;
    using Sem.Sync.SyncBase.DetailData;
    using Sem.Sync.SyncBase.Interfaces;

    /// <summary>
    /// Represents a contacts folder for binding UI elements
    /// </summary>
    public class ContactsFolder : INotifyPropertyChanged
    {
        #region Constants and Fields

        /// <summary>
        ///   A list of contacts from the folder
        /// </summary>
        private IEnumerable<StdContact> contacts;

        /// <summary>
        ///   the currectly selected contact
        /// </summary>
        private StdContact currentContact;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactsFolder"/> class.
        /// </summary>
        /// <param name="connector">
        /// The connector to read from. 
        /// </param>
        /// <param name="clientPath">
        /// The client path. 
        /// </param>
        public ContactsFolder(IClientBase connector, string clientPath)
        {
            if (connector == null)
            {
                return;
            }

            this.Contacts = (IEnumerable<StdContact>)connector.GetAll(clientPath);
        }

        #endregion

        #region Events

        /// <summary>
        ///   Implements the needed event handler for the interface <see cref = "INotifyPropertyChanged" />
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        /// <summary>
        ///   Gets or sets the list of Contacts.
        /// </summary>
        public IEnumerable<StdContact> Contacts
        {
            get
            {
                return this.contacts;
            }

            set
            {
                this.contacts = value;
                this.RaisePropertyChanged("Contacts");
            }
        }

        /// <summary>
        ///   Gets or sets the currently selected contact.
        /// </summary>
        public StdContact CurrentContact
        {
            get
            {
                return this.currentContact;
            }

            set
            {
                this.currentContact = value;
                this.RaisePropertyChanged("CurrentContact");
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Calls the event to inform other classes about an internal change of this objects 
        ///   state - this will cause the GUI to read the data from this object.
        /// </summary>
        /// <param name="propertyName">
        /// The property name that has been changed. 
        /// </param>
        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}