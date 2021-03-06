﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StdElement.cs" company="Sven Erik Matzen">
//   Copyright (c) Sven Erik Matzen. GNU Library General Public License (LGPL) Version 2.1.
// </copyright>
// <summary>
//   This is the base class for contacts and calendar entries. It should contain everything that
//   is needed to successfully sync all kind of the entities.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sem.Sync.SyncBase.DetailData
{
    using System;
    using System.Xml.Serialization;

    using Sem.GenericHelpers.Attributes;

    /// <summary>
    /// This is the base class for contacts and calendar entries. It should contain everything that 
    ///   is needed to successfully sync all kind of the entities.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", 
        Justification = "overriding IComparable is just for sorting!")]
    [Serializable]
    [XmlInclude(typeof(StdContact))]
    [XmlInclude(typeof(StdCalendarItem))]
    public abstract class StdElement : IComparable<StdElement>
    {
        #region Constants and Fields

        /// <summary>
        /// The external identifier.
        /// </summary>
        private ProfileIdentifierDictionary externalIdentifier;

        /// <summary>
        ///   The backinmg field for the <seealso cref = "Id" /> property
        /// </summary>
        private Guid id;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "StdElement" /> class and sets a new GUID for the <see cref = "Id" />
        ///   - this will also initialize the <see cref = "ExternalIdentifier" /> property and set the <see cref = "ProfileIdentifierType.Default" />
        ///   id in this collection.
        /// </summary>
        protected StdElement()
        {
            this.Id = Guid.NewGuid();
            this.InternalSyncData = new SyncData();
        }

        #endregion

        #region Properties

        /// <summary>
        ///   Gets or sets the list of ExternalIdentifier to match one calendar entry to multiple external systems.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", 
            "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "serialization")]
        public ProfileIdentifierDictionary ExternalIdentifier
        {
            get
            {
                return this.externalIdentifier;
            }

            set
            {
                this.externalIdentifier = value;
                this.externalIdentifier.SetProfileId(ProfileIdentifierType.Default, this.Id.ToString("B"));
            }
        }

        /// <summary>
        ///   Gets or sets the unique identifier of the entity. Optimally you will sync
        ///   entities in a way that one physical entity (person / event in time and 
        ///   space / whatever) will have only one ID.
        /// </summary>
        [XmlAttribute]
        public Guid Id
        {
            get
            {
                return this.id;
            }

            set
            {
                this.id = value;

                if (this.ExternalIdentifier == null)
                {
                    this.ExternalIdentifier = new ProfileIdentifierDictionary();
                }

                this.ExternalIdentifier.SetProfileId(ProfileIdentifierType.Default, value.ToString("B"));
            }
        }

        /// <summary>
        ///   Gets or sets some internal synchronization data that does not need to (but 
        ///   might) match to any real property of the entity.
        /// </summary>
        [ComparisonModifier(SkipCompare = true, SkipMerge = true)]
        public SyncData InternalSyncData { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// This method must be overridden and should be implemented in a way that it 
        ///   performs "clean up" operation to the properties of the entity. E.g. by
        ///   removing leading and tailing whitespace or pretty formatting other data.
        /// </summary>
        public abstract void NormalizeContent();

        /// <summary>
        /// Implements a overridable sortable string representation of the entity. This 
        ///   is NOT intended to be shown in any UI and should strictly be used only for 
        ///   sorting entities.
        /// </summary>
        /// <returns>
        /// a string that provides a "weight"/"rank" of the entity
        /// </returns>
        public virtual string ToSortSimple()
        {
            return this.ToString();
        }

        /// <summary>
        /// Implements an overridable SIMPLE string representation.
        /// </summary>
        /// <returns>
        /// a dense and simple string representation of the entity
        /// </returns>
        public virtual string ToStringSimple()
        {
            return this.ToString();
        }

        #endregion

        #region Implemented Interfaces

        #region IComparable<StdElement>

        /// <summary>
        /// compares two entities
        /// </summary>
        /// <param name="other">
        /// The other instance to compare to. 
        /// </param>
        /// <returns>
        /// a value indicating whether the other is "greater", "euqal" or "less" than this entity 
        /// </returns>
        public virtual int CompareTo(StdElement other)
        {
            return string.Compare(this.ToSortSimple(), other.ToSortSimple(), StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #endregion
    }
}