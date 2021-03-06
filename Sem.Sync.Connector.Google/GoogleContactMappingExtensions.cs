﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GoogleContactMappingExtensions.cs" company="Sven Erik Matzen">
//   Copyright (c) Sven Erik Matzen. GNU Library General Public License (LGPL) Version 2.1.
// </copyright>
// <summary>
//   Mapping class that contains only mapping logic between google contacts and sem.sync contacts
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sem.Sync.Connector.Google
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using global::Google.Contacts;
    using global::Google.GData.Client;
    using global::Google.GData.Extensions;

    using Sem.GenericHelpers;
    using Sem.GenericHelpers.Entities;
    using Sem.GenericHelpers.Interfaces;
    using Sem.Sync.SyncBase.DetailData;

    using PhoneNumber = Sem.Sync.SyncBase.DetailData.PhoneNumber;

    /// <summary>
    /// Mapping class that contains only mapping logic between google contacts and sem.sync contacts
    /// </summary>
    internal static class GoogleContactMappingExtensions
    {
        #region Constants and Fields

        /// <summary>
        ///   The schema prefix for the google data api
        /// </summary>
        private const string GoogleSchemaPrefix2005 = "http://schemas.google.com/g/2005#";

        #endregion

        #region Properties

        /// <summary>
        ///   Gets or sets the generic ui responder.
        /// </summary>
        public static IUiInteraction GenericUiResponder { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a specific <see cref="AddressDetail"/> to the google address list of a google contact
        /// </summary>
        /// <param name="googleContact">The google contact.</param>
        /// <param name="stdAddress">The <see cref="AddressDetail"/> data from the <see cref="StdContact"/>.</param>
        /// <param name="addressType">A text type of address ("home", "work").</param>
        public static void AddAddress(this Contact googleContact, AddressDetail stdAddress, string addressType)
        {
            if (stdAddress == null)
            {
                return;
            }

            var addressText = stdAddress.ToString(AddressFormatting.StreetAndCity);
            if (!string.IsNullOrEmpty(addressText))
            {
                var postalAddress = new StructuredPostalAddress
                                        {
                                            Street = stdAddress.StreetName,
                                            City = stdAddress.CityName,
                                            Rel = GoogleSchemaPrefix2005 + addressType,
                                        };

                if (!IsAddressExisting(googleContact.PostalAddresses, stdAddress))
                {
                    googleContact.PostalAddresses.Add(postalAddress);
                }
            }

            googleContact.AddPhoneNumber(stdAddress.Phone, addressType);
        }

        /// <summary>
        /// adds email information to a google contact
        /// </summary>
        /// <param name="googleContact">The google contact to add the information to.</param>
        /// <param name="email">The email address.</param>
        /// <param name="addressType">The address type (home or work).</param>
        public static void AddEmail(this Contact googleContact, string email, string addressType)
        {
            if (!string.IsNullOrEmpty(email))
            {
                if (googleContact.Emails.All(x => x.Rel != GoogleSchemaPrefix2005 + addressType))
                {
                    googleContact.Emails.Add(new EMail(email, GoogleSchemaPrefix2005 + addressType));
                }
            }
        }

        /// <summary>
        /// adds email information to a google contact
        /// </summary>
        /// <param name="googleContact">The google contact to add the information to.</param>
        /// <param name="instantMessengerAddresses">The instant messenger addresses.</param>
        /// <param name="addressType">The address type (home or work).</param>
        public static void AddImAddress(this Contact googleContact, InstantMessengerAddresses instantMessengerAddresses, string addressType)
        {
            if (instantMessengerAddresses != null && !string.IsNullOrEmpty(instantMessengerAddresses.MsnMessenger))
            {
                if (googleContact.IMs.All(x => x.Address != instantMessengerAddresses.MsnMessenger))
                {
                    googleContact.IMs.Add(new IMAddress(instantMessengerAddresses.MsnMessenger));
                }
            }
        }

        /// <summary>
        /// Adda a business company and position to the contact - the department is currently ignored
        /// </summary>
        /// <param name="googleContact">The google contact.</param>
        /// <param name="stdBusinessCompanyName">The business company name to add.</param>
        /// <param name="stdBusinessDepartment">The business department name to add - this parameter is currently ignored.</param>
        /// <param name="stdBusinessPosition">The business position to add.</param>
        public static void AddOrganization(this Contact googleContact, string stdBusinessCompanyName, string stdBusinessDepartment, string stdBusinessPosition)
        {
            if (string.IsNullOrEmpty(stdBusinessCompanyName))
            {
                return;
            }

            var company = new Organization
                {
                    Name = stdBusinessCompanyName,
                    Title = stdBusinessPosition,
                    Rel = GoogleSchemaPrefix2005 + "work"
                };

            googleContact.Organizations.Add(company);
        }

        /// <summary>
        /// Adds a specific <see cref="SyncBase.DetailData.PhoneNumber"/> to the google address list of a google contact
        /// </summary>
        /// <param name="googleContact">The google contact.</param>
        /// <param name="stdPhoneNumber">The std phone number.</param>
        /// <param name="addressType">The address type.</param>
        public static void AddPhoneNumber(this Contact googleContact, PhoneNumber stdPhoneNumber, string addressType)
        {
            if (stdPhoneNumber == null || string.IsNullOrEmpty(stdPhoneNumber.ToString()))
            {
                return;
            }

            var phone = new global::Google.GData.Extensions.PhoneNumber(stdPhoneNumber.ToString())
                {
                    Rel = GoogleSchemaPrefix2005 + addressType
                };

            googleContact.Phonenumbers.Add(phone);
        }

        /// <summary>
        /// Sets a postal address from the google contact into the std contact.
        /// </summary>
        /// <param name="stdEntry">The std entry to be updated.</param>
        /// <param name="address">The address to set.</param>
        public static void SetAddress(this StdContact stdEntry, StructuredPostalAddress address)
        {
            var stdAddress = new AddressDetail(address.FormattedAddress);
            if (address.Usage == "home")
            {
                if (stdEntry.PersonalAddressPrimary == null)
                {
                    stdEntry.PersonalAddressPrimary = stdAddress;
                }
                else
                {
                    stdEntry.PersonalAddressSecondary = stdAddress;
                }
            }

            if (address.Usage == "Work")
            {
                if (stdEntry.BusinessAddressPrimary == null)
                {
                    stdEntry.BusinessAddressPrimary = stdAddress;
                }
                else
                {
                    stdEntry.BusinessAddressSecondary = stdAddress;
                }
            }
        }

        /// <summary>
        /// Sets the organization info including the business position.
        /// </summary>
        /// <param name="stdEntry">The std entry.</param>
        /// <param name="organization">The organization information from the google contact.</param>
        public static void SetBusiness(this StdContact stdEntry, Organization organization)
        {
            if (!organization.Primary
                && !string.IsNullOrEmpty(stdEntry.BusinessCompanyName))
            {
                return;
            }

            stdEntry.BusinessPosition = organization.Title;
            stdEntry.BusinessCompanyName = organization.Name;
        }

        /// <summary>
        /// sets the correct email address from a google contacts email
        /// </summary>
        /// <param name="stdEntry">The std entry.</param>
        /// <param name="email">The email address.</param>
        public static void SetEmail(this StdContact stdEntry, EMail email)
        {
            if (email.Home)
            {
                stdEntry.PersonalEmailPrimary = email.Address;
            }

            if (email.Work)
            {
                stdEntry.BusinessEmailPrimary = email.Address;
            }

            if (!email.Home && !email.Work && string.IsNullOrEmpty(stdEntry.PersonalEmailPrimary))
            {
                stdEntry.PersonalEmailPrimary = email.Address;
            }
        }

        /// <summary>
        /// sets the correct IM address from a google contacts Ims
        /// </summary>
        /// <param name="stdEntry">The std entry.</param>
        /// <param name="instantMessengerAddress">The IM address.</param>
        public static void SetInstantMessenger(this StdContact stdEntry, IMAddress instantMessengerAddress)
        {
            if (stdEntry.PersonalInstantMessengerAddresses == null)
            {
                stdEntry.PersonalInstantMessengerAddresses = new InstantMessengerAddresses();
            }

            if (instantMessengerAddress.Home)
            {
                switch (instantMessengerAddress.Protocol)
                {
                    case "msn":
                        stdEntry.PersonalInstantMessengerAddresses.MsnMessenger = instantMessengerAddress.Address;
                        break;
                }
            }

            if (instantMessengerAddress.Work)
            {
                switch (instantMessengerAddress.Protocol)
                {
                    case "msn":
                        stdEntry.BusinessInstantMessengerAddresses.MsnMessenger = instantMessengerAddress.Address;
                        break;
                }
            }
        }

        /// <summary>
        /// sets the correct phone number from a google contacts email
        /// </summary>
        /// <param name="stdEntry">The std entry.</param>
        /// <param name="phoneNumber">The phone number to set into the std entry.</param>
        public static void SetPhone(this StdContact stdEntry, global::Google.GData.Extensions.PhoneNumber phoneNumber)
        {
            var stdPhoneNumber = new PhoneNumber(phoneNumber.Value);
            if (phoneNumber.Home)
            {
                stdEntry.PersonalAddressPrimary = stdEntry.PersonalAddressPrimary ?? new AddressDetail();
                stdEntry.PersonalAddressPrimary.Phone = stdPhoneNumber;
            }

            if (phoneNumber.Work)
            {
                stdEntry.BusinessAddressPrimary = stdEntry.BusinessAddressPrimary ?? new AddressDetail();
                stdEntry.BusinessAddressPrimary.Phone = stdPhoneNumber;
            }

            if (phoneNumber.Rel == GoogleSchemaPrefix2005 + "mobile")
            {
                if (stdEntry.PersonalPhoneMobile == null)
                {
                    stdEntry.PersonalPhoneMobile = stdPhoneNumber;
                }
                else
                {
                    stdEntry.BusinessPhoneMobile = stdPhoneNumber;
                }
            }
        }

        /// <summary>
        /// doenloads the image for a contact
        /// </summary>
        /// <param name="stdEntry">The std entry.</param>
        /// <param name="googleContact">The google contact.</param>
        /// <param name="requester">The requester.</param>
        public static void SetPicture(this StdContact stdEntry, Contact googleContact, ContactsRequest requester)
        {
            if (googleContact == null || googleContact.PhotoUri == null)
            {
                return;
            }

            try
            {
                using (var stream = requester.GetPhoto(googleContact))
                {
                    if (stream != null)
                    {
                        var value = new StreamReader(stream).ReadToEnd();
                        stdEntry.PictureData = Encoding.ASCII.GetBytes(value);
                    }
                }
            }
            catch (GDataNotModifiedException)
            {
                var helper = HttpHelper.DefaultInstance;
                helper.ContentCredentials.LogOnDomain = "[GOOGLE]";
                helper.ContentCredentials.LogOnPassword =
                    ((GDataGAuthRequestFactory)requester.Service.RequestFactory).GAuthToken;
                stdEntry.PictureData = helper.GetContentBinary(googleContact.PhotoUri.AbsoluteUri, string.Empty, string.Empty);
            }
        }

        /// <summary>
        /// Sets the sem.sync guid into the contact
        /// </summary>
        /// <param name="googleContact">The google contact to add the information to.</param>
        /// <param name="identifier">The identifier.</param>
        public static void SetSyncIdentifier(this ContactBase googleContact, Guid identifier)
        {
            if (googleContact.ExtendedProperties == null)
            {
                return;
            }

            ExtendedProperty semSyncId = null;
            if (googleContact.ExtendedProperties.Count > 0)
            {
                semSyncId =
                    (from x in googleContact.ExtendedProperties where x.Name == "SemSyncId" select x).FirstOrDefault();
            }

            if (semSyncId == null)
            {
                semSyncId = new ExtendedProperty { Name = identifier.ToString(), Value = "SemSyncId" };
                googleContact.ExtendedProperties.Add(semSyncId);
            }
        }

        /// <summary>
        /// Creates or updates photo information for a google contact with the information found in the std-contact.
        /// </summary>
        /// <param name="googleContact">The google contact.</param>
        /// <param name="contact">The contact containing the image information.</param>
        /// <param name="credentials">The credentials providing object.</param>
        public static void UpdatePhoto(this Contact googleContact, StdContact contact, ContactClient credentials)
        {
            if (contact.PictureData == null || contact.PictureData.Length <= 0 || string.IsNullOrEmpty(googleContact.Id))
            {
                return;
            }

            var rs = new RequestSettings("Sem.Sync.Connector.Google", credentials.LogOnUserId, credentials.LogOnPassword) { AutoPaging = true };
            var cr = new ContactsRequest(rs);

            using (var photoStream = new MemoryStream(contact.PictureData))
            {
                for (var i = 0; i < 3; i++)
                {
                    try
                    {
                        cr.SetPhoto(googleContact, photoStream);
                        break;
                    }
                    catch (GDataRequestException ex)
                    {
                        credentials.LogError(ex);
                        if (string.IsNullOrEmpty(ex.ResponseString))
                        {
                            break;
                        }
                    }
                    catch (CaptchaRequiredException)
                    {
                        UnlockCaptch();
                    }
                    catch (Exception ex)
                    {
                        credentials.LogError(ex);
                        break;
                    }
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Searches for a matching address inside the google address collection.
        /// </summary>
        /// <param name="googleAddresses">The google address collection.</param>
        /// <param name="stdAddress">The <see cref="AddressDetail"/> data from the <see cref="StdContact"/>.</param>
        /// <returns>true if the address is already part of the collection</returns>
        private static bool IsAddressExisting(IEnumerable<StructuredPostalAddress> googleAddresses, AddressDetail stdAddress)
        {
            return stdAddress != null 
                && googleAddresses.Any(address => address.City == stdAddress.CityName 
                                               && address.Street == stdAddress.StateName);
        }

        /// <summary>
        /// unlocks a locked account using a captcha
        /// </summary>
        /// <exception cref="NotImplementedException">This still needs to be implemented - we should implement that inside a UI assembly</exception>
        private static void UnlockCaptch()
        {
            if (GenericUiResponder != null)
            {
                GenericUiResponder.ResolveCaptcha(
                    "Google requests you to resolve a captach to reactivate your account. Please resolve the captcha on the web page and press ok after that.",
                    "Google Captcha Request",
                    new CaptchaResolveRequest { UrlOfWebSite = @"https://www.google.com/accounts/DisplayUnlockCaptcha" });
            }
            else
            {
                throw new InvalidOperationException("Google requests a captcha to be resolved in order to reactivate the account, but the UI handler to resolve the captcha has not been setup yet. Make sure the GenericUiResponder property of the static GoogleContactMappingExtensions class has been setup properly.");
            }
        }

        #endregion
    }
}