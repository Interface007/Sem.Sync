﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="vCardConverter.cs" company="Sven Erik Matzen">
//   Copyright (c) Sven Erik Matzen. GNU Library General Public License (LGPL) Version 2.1.
// </copyright>
// <summary>
//   Class to convert StdContacts to/from vCards
//   This class does NOT fully implement the vCard 2.1 specification, because it does NOT support:
//   - Property parameters without explicit name
//   - folding of property values
//   - grouping of properties
//   - ONLY Base64-encoding is supported, quoted-printable and 8bit are NOT supported
//   - value locations - ONLY inline and URL are supported
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sem.Sync.SyncBase.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    using Sem.GenericHelpers;
    using Sem.GenericHelpers.Contracts;
    using Sem.GenericHelpers.Interfaces;
    using Sem.Sync.SyncBase.DetailData;

    /// <summary>
    /// Class to convert StdContacts to/from vCards
    ///   This class does NOT fully implement the vCard 2.1 specification, because it does NOT support:
    ///   - Property parameters without explicit name
    ///   - folding of property values
    ///   - grouping of properties
    ///   - ONLY Base64-encoding is supported, quoted-printable and 8bit are NOT supported
    ///   - value locations - ONLY inline and URL are supported
    /// </summary>
    public class VCardConverter
    {
        #region Properties

        /// <summary>
        ///   Gets or sets the HttpRequester to download pictures in case of url-references.
        /// </summary>
        public IHttpHelper HttpRequester { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// converts a StdContact into a vCard (binary content)
        /// </summary>
        /// <param name="contact">
        /// The contact to be converted. 
        /// </param>
        /// <returns>
        /// a binary vCard representation 
        /// </returns>
        public static byte[] StdContactToVCard(StdContact contact)
        {
            Bouncer
                .ForCheckData(() => contact)
                .Assert(x => x.Name != null);

            var vCard = new StringBuilder();
            vCard.AppendLine("BEGIN:VCARD");
            vCard.AppendLine("VERSION:2.1");
            AddAttributeToStringBuilder(
                vCard, 
                "N", 
                contact.Name.LastName, 
                contact.Name.FirstName, 
                contact.Name.MiddleName, 
                contact.Name.AcademicTitle);
            AddAttributeToStringBuilder(vCard, "FN", contact.GetFullName());
            AddAttributeToStringBuilder(vCard, "SORT-STRING", contact.Name.LastName);
            AddAttributeToStringBuilder(vCard, "EMAIL;TYPE=INTERNET;TYPE=WORK;TYPE=PREF", contact.BusinessEmailPrimary);
            AddAttributeToStringBuilder(vCard, "EMAIL;TYPE=INTERNET;TYPE=HOME", contact.PersonalEmailPrimary);
            AddAttributeToStringBuilder(vCard, "URL;TYPE=HOME", contact.PersonalHomepage);

            if (contact.DateOfBirth.Year > 1900 && contact.DateOfBirth.Year < 2200)
            {
                AddAttributeToStringBuilder(
                    vCard, "BDAY", contact.DateOfBirth.ToString("yyyyMMdd", CultureInfo.CurrentCulture));
            }

            if (contact.BusinessAddressPrimary != null)
            {
                AddAttributeToStringBuilder(
                    vCard, 
                    "ADR;TYPE=WORK", 
                    null, 
                    null, 
                    contact.BusinessAddressPrimary.StreetName, 
                    contact.BusinessAddressPrimary.CityName, 
                    contact.BusinessAddressPrimary.StateName, 
                    contact.BusinessAddressPrimary.PostalCode, 
                    contact.BusinessAddressPrimary.CountryName);
                AddAttributeToStringBuilder(vCard, "TEL;TYPE=WORK", contact.BusinessAddressPrimary.Phone);
            }

            if (contact.PersonalAddressPrimary != null)
            {
                AddAttributeToStringBuilder(
                    vCard, 
                    "ADR;TYPE=HOME", 
                    null, 
                    null, 
                    contact.PersonalAddressPrimary.StreetName, 
                    contact.PersonalAddressPrimary.CityName, 
                    contact.PersonalAddressPrimary.StateName, 
                    contact.PersonalAddressPrimary.PostalCode, 
                    contact.PersonalAddressPrimary.CountryName);
                AddAttributeToStringBuilder(vCard, "TEL;TYPE=HOME", contact.PersonalAddressPrimary.Phone);
            }

            AddAttributeToStringBuilder(vCard, "TEL;TYPE=CELL,HOME", contact.PersonalPhoneMobile);
            AddAttributeToStringBuilder(vCard, "TEL;TYPE=CELL,WORK", contact.BusinessPhoneMobile);

            AddAttributeToStringBuilder(vCard, "ORG", contact.BusinessCompanyName);
            AddAttributeToStringBuilder(vCard, "URL;TYPE=WORK", contact.BusinessHomepage);
            AddAttributeToStringBuilder(vCard, "URL;TYPE=HOME", contact.PersonalHomepage);
            AddAttributeToStringBuilder(vCard, "TITLE", contact.BusinessPosition);
            AddAttributeToStringBuilder(vCard, "NOTE", contact.AdditionalTextData);

            AddAttributeToStringBuilder(vCard, "X-MATZEN-STDUID", contact.Id);
            AddAttributeToStringBuilder(vCard, "X-MATZEN-GENERATOR", "generated by Sem.Sync - www.svenerikmatzen.info");
            AddAttributeToStringBuilder(vCard, "PRODID", "-//MATZEN//www.svenerikmatzen.info//Sem.Sync//Version 1.0");
            AddAttributeToStringBuilder(vCard, "PHOTO;TYPE=JPEG", contact.PictureData);
            AddAttributeToStringBuilder(vCard, "UID", contact.Id.ToString("N"));

            vCard.AppendLine("END:VCARD");

            return Encoding.UTF8.GetBytes(vCard.ToString());
        }

        /// <summary>
        /// converts a vCard into a standard contact.
        /// </summary>
        /// <param name="vCard">
        /// The vCard. 
        /// </param>
        /// <returns>
        /// a StdContact representation of the vCard
        /// </returns>
        public StdContact VCardToStdContact(byte[] vCard)
        {
            return this.VCardToStdContact(vCard, ProfileIdentifierType.Default);
        }

        /// <summary>
        /// converts a vCard into a standard contact.
        /// </summary>
        /// <param name="vCard">
        /// The vCard. 
        /// </param>
        /// <param name="useIndentifierAs">
        /// This value determines the meaning of the identifier. 
        /// </param>
        /// <returns>
        /// a StdContact representation of the vCard
        /// </returns>
        public StdContact VCardToStdContact(byte[] vCard, ProfileIdentifierType useIndentifierAs)
        {
            if (vCard == null)
            {
                throw new ArgumentNullException("vCard");
            }

            var contact = new StdContact
                {
                    InternalSyncData = new SyncData { DateOfCreation = DateTime.Now, DateOfLastChange = DateTime.Now }, 
                    Name = new PersonName(), 
                };

            var vCardUtf8 = Encoding.UTF8.GetString(vCard);
            var vCardIso8859 =
                Encoding.UTF8.GetString(Encoding.Convert(Encoding.GetEncoding("iso8859-1"), Encoding.UTF8, vCard));

            contact.Name = new PersonName();

            var linesUtf8 = vCardUtf8.Split('\n');
            var linesIso8859 = vCardIso8859.Split('\n');
            for (var i = 0; i < linesIso8859.Length; i++)
            {
                var line = linesIso8859[i].Replace("\r", string.Empty);

                if (line.Length == 0 || !line.Contains(":"))
                {
                    continue;
                }

                line = GetInformationSegment(
                    ref i, line.ToUpperInvariant().Contains("CHARSET=UTF-8:") ? linesUtf8 : linesIso8859);

                var propertyDescription = line.Substring(0, line.IndexOf(':')).ToUpperInvariant();
                var value = line.Substring(line.IndexOf(':') + 1).Replace("\r", string.Empty);
                var valueParts = value.Split(';');
                var type = PropertyAttribute(propertyDescription, "TYPE", string.Empty);

                var propertyName = propertyDescription;
                if (propertyDescription.Contains(";"))
                {
                    propertyName = propertyDescription.Substring(0, propertyDescription.IndexOf(';'));
                    if (type.Count == 0 || (type.Count == 1 && type[0] == string.Empty))
                    {
                        type.AddRange(propertyDescription.Split(';'));
                    }
                }

                var binaryData = new byte[] { };
                DecodeData(propertyDescription, ref value, ref binaryData);

                switch (propertyName)
                {
                    case "TEL":

                        // skip this if it's fax without voice support
                        if (type.Contains("FAX") && !type.Contains("VOICE"))
                        {
                            break;
                        }

                        if (type.Contains("CELL"))
                        {
                            if (type.Contains("HOME"))
                            {
                                contact.PersonalPhoneMobile = new PhoneNumber(value);
                                break;
                            }

                            if (type.Contains("WORK"))
                            {
                                contact.BusinessPhoneMobile = new PhoneNumber(value);
                                break;
                            }
                        }

                        if (type.Contains("HOME"))
                        {
                            if (contact.PersonalAddressPrimary == null)
                            {
                                contact.PersonalAddressPrimary = new AddressDetail();
                            }

                            contact.PersonalAddressPrimary.Phone = new PhoneNumber(value);
                            break;
                        }

                        if (type.Contains("WORK"))
                        {
                            if (contact.BusinessAddressPrimary == null)
                            {
                                contact.BusinessAddressPrimary = new AddressDetail();
                            }

                            contact.BusinessAddressPrimary.Phone = new PhoneNumber(value);
                        }

                        break;

                    case "N":
                        contact.Name.LastName = GetNthElement(valueParts, 1);
                        contact.Name.FirstName = GetNthElement(valueParts, 2);
                        contact.Name.MiddleName = GetNthElement(valueParts, 3);
                        contact.Name.AcademicTitle = GetNthElement(valueParts, 4);
                        break;

                    case "EMAIL":
                        if (type.Contains("WORK"))
                        {
                            contact.BusinessEmailPrimary = value;
                            break;
                        }

                        if (type.Contains("HOME"))
                        {
                            contact.PersonalEmailPrimary = value;
                            break;
                        }

                        contact.PersonalEmailPrimary = value;
                        Tools.DebugWriteLine("!!Unhandled email address - added as personal!!");
                        break;

                    case "URL":
                        if (type.Contains("HOME"))
                        {
                            contact.PersonalHomepage = value;
                        }
                        else
                        {
                            contact.BusinessHomepage = value;
                        }

                        break;

                    case "ORG":
                        contact.BusinessCompanyName = value;
                        break;

                    case "NOTE":
                        contact.AdditionalTextData = value;
                        break;

                    case "ADR":
                        if (line.EndsWith(";;;;;;\r", StringComparison.Ordinal))
                        {
                            // in this case we do not have an address - it's all empty
                            break;
                        }

                        var address = new AddressDetail
                            {
                                CityName = GetNthElement(valueParts, 4), 
                                StreetName = GetNthElement(valueParts, 3), 
                                StateName = GetNthElement(valueParts, 5), 
                                PostalCode = GetNthElement(valueParts, 6), 
                                CountryName = GetNthElement(valueParts, 7), 
                            };

                        if (type.Contains("WORK"))
                        {
                            contact.BusinessAddressPrimary = address;
                        }
                        else
                        {
                            contact.PersonalAddressPrimary = address;
                        }

                        break;

                    case "TITLE":
                        contact.BusinessPosition = value;
                        break;

                    case "X-WAB-GENDER":
                        contact.PersonGender = value == "2"
                                                   ? Gender.Male
                                                   : value == "1" ? Gender.Female : Gender.Unspecified;
                        break;

                    case "PHOTO":
                        if (binaryData.Length > 0)
                        {
                            contact.PictureData = binaryData;
                        }
                        else
                        {
                            var url = value.Replace("URI:", string.Empty);
                            contact.PictureData = this.HttpRequester.GetContentBinary(url, url);
                        }

                        break;

                    case "BDAY":
                        var dateString = value;
                        if (dateString.IndexOf("-", 0, StringComparison.Ordinal) == -1)
                        {
                            dateString = dateString.Substring(0, 4) + "-" + dateString.Substring(4, 2) + "-" +
                                         dateString.Substring(6, 2);
                        }

                        contact.DateOfBirth = DateTime.Parse(
                            dateString, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal);
                        break;

                    case "UID":
                        contact.ExternalIdentifier.SetProfileId(useIndentifierAs, value);
                        break;

                    case "X-MATZEN-STDUID":
                        contact.Id = new Guid(value);
                        break;

                    case "CATEGORIES":
                        if (contact.SourceSpecificAttributes == null)
                        {
                            contact.SourceSpecificAttributes = new SerializableDictionary<string, string>();
                        }

                        contact.SourceSpecificAttributes.Add(propertyName, value);
                        break;

                    case "LABEL":
                    case "PRODID":
                    case "BEGIN":
                    case "END":
                    case "SORT-STRING":
                    case "CLASS":
                    case "FN":
                    case "":
                    case "VERSION":
                    case "X-MATZEN-GENERATOR":
                    case "X-MS-OL-DESIGN":
                    case "X-MS-OL-DEFAULT-POSTAL-ADDRESS":
                        break;

                    default:
                        Tools.DebugWriteLine("unhandled: " + line.Replace("\r", string.Empty));
                        break;
                }
            }

            if (contact.Id == Guid.Empty)
            {
                contact.Id = Guid.NewGuid();
            }

            return contact;
        }

        #endregion

        #region Methods

        /// <summary>
        /// adds a contact attribute to the vCard. This overload will simply use the ToString method to serialize the object
        /// </summary>
        /// <param name="vCard">
        /// the StringBuilder that is currently writing the vCard
        /// </param>
        /// <param name="attributeSpecification">
        /// the textual attribute specification
        /// </param>
        /// <param name="values">
        /// the value to write to the attribute
        /// </param>
        private static void AddAttributeToStringBuilder(
            StringBuilder vCard, string attributeSpecification, object values)
        {
            if (values != null)
            {
                AddAttributeToStringBuilder(vCard, attributeSpecification, values.ToString());
            }
        }

        /// <summary>
        /// adds a contact attribute to the vCard. This overload will use base64 encoding
        /// </summary>
        /// <param name="vCard">
        /// the StringBuilder that is currently writing the vCard
        /// </param>
        /// <param name="attributeSpecification">
        /// the textual attribute specification
        /// </param>
        /// <param name="values">
        /// the value to write to the attribute
        /// </param>
        private static void AddAttributeToStringBuilder(
            StringBuilder vCard, string attributeSpecification, byte[] values)
        {
            if (values != null && values.Length > 0)
            {
                AddAttributeToStringBuilder(
                    vCard, attributeSpecification + ";ENCODING=B", Convert.ToBase64String(values));
            }
        }

        /// <summary>
        /// adds a contact attribute to the vCard.
        /// </summary>
        /// <param name="vCard"> the StringBuilder that is currently writing the vCard </param>
        /// <param name="attributeSpecification"> the textual attribute specification </param>
        /// <param name="values"> the value to write to the attribute </param>
        private static void AddAttributeToStringBuilder(StringBuilder vCard, string attributeSpecification, params string[] values)
        {
            if (values.Any(s => !string.IsNullOrEmpty(s)))
            {
                vCard.AppendLine(attributeSpecification + ";CHARSET=UTF-8:" + string.Join(";", values));
            }
        }

        /// <summary>
        /// Decodes the data of the <paramref name="value"/> parameter and extracts binary data into the
        ///   <paramref name="binaryData"/> parameter in case of base64-encoding.
        /// </summary>
        /// <param name="propertyDescription">
        /// The property description. 
        /// </param>
        /// <param name="value">
        /// The value to be decoded. 
        /// </param>
        /// <param name="binaryData">
        /// The binary data extracted if the encoding was for binary data. 
        /// </param>
        private static void DecodeData(string propertyDescription, ref string value, ref byte[] binaryData)
        {
            var encoding = string.Concat(PropertyAttribute(propertyDescription, "ENCODING", string.Empty));
            if (string.IsNullOrEmpty(encoding))
            {
                return;
            }

            switch (encoding)
            {
                case "B":
                case "BASE64":
                    binaryData = Convert.FromBase64String(value);
                    break;

                case "QUOTED-PRINTABLE":
                    value = Tools.DecodeFromQuotedPrintable(value);
                    break;

                default:
                    Tools.DebugWriteLine("unhandled encoding: " + encoding);
                    break;
            }
        }

        /// <summary>
        /// Extractes the value from the current data segment. Increments the line index if the 
        ///   data spans multiple lines.
        /// </summary>
        /// <param name="lineIndex">
        /// The current line index. 
        /// </param>
        /// <param name="lines">
        /// The lines. 
        /// </param>
        /// <returns>
        /// a string represening the data for the current segment 
        /// </returns>
        private static string GetInformationSegment(ref int lineIndex, string[] lines)
        {
            var line = lines[lineIndex];

            while (lines[lineIndex + 1].StartsWith(" ", StringComparison.Ordinal))
            {
                line += lines[lineIndex + 1];
                lineIndex++;
            }

            return line;
        }

        /// <summary>
        /// get the n'th element if there is one
        /// </summary>
        /// <param name="inputArray">
        /// the array to get the value from
        /// </param>
        /// <param name="index">
        /// the index of the element to get
        /// </param>
        /// <returns>
        /// the string element with the desired index, null if there is no such entry
        /// </returns>
        private static string GetNthElement(string[] inputArray, int index)
        {
            if (inputArray.Length < index || index < 1)
            {
                return null;
            }

            var returnValue = inputArray[index - 1];
            return returnValue;
        }

        /// <summary>
        /// gets a list of attribute values from a vCard property
        /// </summary>
        /// <param name="property">
        /// the vCard property to get the attribute from
        /// </param>
        /// <param name="attributeName">
        /// the attribute name
        /// </param>
        /// <param name="defaultAttributeValue">
        /// a default value for the attribute, if there is no value inside the property
        /// </param>
        /// <returns>
        /// a list of attribute values
        /// </returns>
        private static List<string> PropertyAttribute(
            string property, string attributeName, string defaultAttributeValue)
        {
            var values = new List<string>();
            foreach (var propertyItem in property.Split(';'))
            {
                if (propertyItem.StartsWith(attributeName + "=", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var s in propertyItem.Substring(propertyItem.IndexOf('=') + 1).Split(','))
                    {
                        values.Add(s);
                    }
                }
            }

            if (values.Count == 0)
            {
                foreach (var s in defaultAttributeValue.Split(','))
                {
                    values.Add(s);
                }
            }

            return values;
        }

        #endregion
    }
}