// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddressDetail.cs" company="Sven Erik Matzen">
//   Copyright (c) Sven Erik Matzen. GNU Library General Public License (LGPL) Version 2.1.
// </copyright>
// <summary>
//   Enumeration of address formatting styles
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sem.Sync.SyncBase.DetailData
{
    using System;
    using System.Text.RegularExpressions;

    using Sem.GenericHelpers;

    /// <summary>
    /// Enumeration of address formatting styles
    /// </summary>
    public enum AddressFormatting
    {
        /// <summary>
        ///   plain text with all information
        /// </summary>
        Default = 0, 

        /// <summary>
        ///   e.g. "Sesamstreet 56\n65287 SomewhereTown"
        /// </summary>
        StreetAndCity = 1
    }

    /// <summary>
    /// Holds the detail data of an address
    /// </summary>
    [Serializable]
    public class AddressDetail
    {
        #region Constants and Fields

        /// <summary>
        ///   detects that there is some text to get (the country)
        /// </summary>
        private static readonly Regex RegCityPre = new Regex("^[a-zA-Z -.�������]+");

        /// <summary>
        ///   extracts the postal code and the city
        /// </summary>
        private static readonly Regex RegPlzCity = new Regex("(?<plz>^[0-9]+)(?<city>[^0-9]+.+)?");

        /// <summary>
        ///   detects that there are postal code and city
        /// </summary>
        private static readonly Regex RegPlzCityPre = new Regex("^[0-9]+");

        /// <summary>
        ///   detects that there is a street number and may be an extension
        /// </summary>
        private static readonly Regex RegStreetNumberExtensionPre = new Regex("^[a-zA-Z -.�������]+[0-9]+");

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "AddressDetail" /> class.
        /// </summary>
        public AddressDetail()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressDetail"/> class. 
        ///   This constructor also parses the string parameter <paramref name="address"/> by lines to extract the address information.
        /// </summary>
        /// <param name="address">
        /// the string representation of an address
        /// </param>
        public AddressDetail(string address)
        {
            address = address ?? string.Empty;
            var lines = address.Split('\n');
            foreach (var parts in lines)
            {
                var line = parts.Replace("\t", string.Empty).Replace("\n", string.Empty).Trim();

                if (RegPlzCityPre.IsMatch(line))
                {
                    var lineParts = RegPlzCity.Matches(line);
                    this.PostalCode = GetMatchGroupWithDefault(lineParts, "plz", string.Empty);
                    this.CityName = GetMatchGroupWithDefault(lineParts, "city", string.Empty);
                    continue;
                }

                if (RegStreetNumberExtensionPre.IsMatch(line))
                {
                    this.StreetName = line;
                    continue;
                }

                if (RegCityPre.IsMatch(line))
                {
                    this.CountryName += line;
                    continue;
                }

                Tools.DebugWriteLine("??? : " + line);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        ///   Gets or sets the name of the city ("Washington DC", "Fankfurt am Main"...)
        /// </summary>
        public string CityName { get; set; }

        /// <summary>
        ///   Gets or sets the name of the country (USA / Germany / Spain...)
        /// </summary>
        public string CountryName { get; set; }

        /// <summary>
        ///   Gets or sets the phone number that belongs to the address
        /// </summary>
        public PhoneNumber Phone { get; set; }

        /// <summary>
        ///   Gets or sets the postal code or "zip-code"
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        ///   Gets or sets an identifier for the room inside the house/building
        /// </summary>
        public string Room { get; set; }

        /// <summary>
        ///   Gets or sets the name of the state or province (Texas (in USA) / Hessen (in Germany) / ...)
        /// </summary>
        public string StateName { get; set; }

        /// <summary>
        ///   Gets or sets the name of the street
        /// </summary>
        public string StreetName { get; set; }

        #endregion

        #region Operators

        /// <summary>
        ///   Implements the equal operator by comparing the <see cref = "ToString()" /> result.
        /// </summary>
        /// <param name = "left"> The left side object for the comparison. </param>
        /// <param name = "right"> The right side object for the comparison. </param>
        /// <returns> true in case of both instances containing equal data </returns>
        public static bool operator ==(AddressDetail left, AddressDetail right)
        {
            return object.Equals(left, right);
        }

        /// <summary>
        ///   Implements the "not equal" operator by comparing the <see cref = "ToString()" /> result.
        /// </summary>
        /// <param name = "left"> The left side object for the comparison. </param>
        /// <param name = "right"> The right side object for the comparison. </param>
        /// <returns> true in case of both instances containing different data </returns>
        public static bool operator !=(AddressDetail left, AddressDetail right)
        {
            return !object.Equals(left, right);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Compares the content of this address instance to another based on the <see cref="ToString()"/> method.
        /// </summary>
        /// <param name="other">
        /// The other instance. 
        /// </param>
        /// <returns>
        /// true in case of equal content 
        /// </returns>
        public bool Equals(AddressDetail other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other.ToString() == this.ToString();
        }

        /// <summary>
        /// Implements a comparison against an object with the same type
        ///   based on the <see cref="ToString()"/> method.
        /// </summary>
        /// <param name="obj">
        /// The other object. 
        /// </param>
        /// <returns>
        /// true in case of equal content 
        /// </returns>
        public override bool Equals(object obj)
        {
            var other = obj as AddressDetail;

            if (other == null)
            {
                return false;
            }

            return this.ToString().Equals(other.ToString());
        }

        /// <summary>
        /// Overrides the object method <see cref="object.GetHashCode"/> in order to implement comparison
        /// </summary>
        /// <returns>
        /// A value to distribute different instances of this class in a list. This value will not be unique 
        ///   nor will it be a "strong" cryptographic hash
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var result = this.CountryName != null ? this.CountryName.GetHashCode() : 0;
                result = (result * 397) ^ (this.StateName != null ? this.StateName.GetHashCode() : 0);
                result = (result * 397) ^ (this.PostalCode != null ? this.PostalCode.GetHashCode() : 0);
                result = (result * 397) ^ (this.CityName != null ? this.CityName.GetHashCode() : 0);
                result = (result * 397) ^ (this.StreetName != null ? this.StreetName.GetHashCode() : 0);
                result = (result * 397) ^ (this.Room != null ? this.Room.GetHashCode() : 0);
                result = (result * 397) ^ (this.Phone != null ? this.Phone.GetHashCode() : 0);
                return result;
            }
        }

        /// <summary>
        /// Builds up a string representation of the information inside this object.
        /// </summary>
        /// <returns>
        /// a well formatted string representation of the data
        /// </returns>
        public override string ToString()
        {
            return this.ToString(AddressFormatting.Default);
        }

        /// <summary>
        /// Builds up a string representation of the information inside this object.
        /// </summary>
        /// <param name="format">
        /// The format to use for the address formatting (<see cref="AddressFormatting"/>). 
        /// </param>
        /// <returns>
        /// a well formatted string representation of the data 
        /// </returns>
        public string ToString(AddressFormatting format)
        {
            var result = string.Empty;

            switch (format)
            {
                case AddressFormatting.StreetAndCity:
                    result += this.StreetName ?? string.Empty;
                    result += string.IsNullOrEmpty(result) ? string.Empty : "\n";

                    result += string.IsNullOrEmpty(this.PostalCode) ? string.Empty : this.PostalCode + " ";
                    result += string.IsNullOrEmpty(this.CityName) ? string.Empty : this.CityName;
                    break;

                default:
                    result += this.StreetName ?? string.Empty;
                    result += string.IsNullOrEmpty(result) ? string.Empty : " / ";

                    result += string.IsNullOrEmpty(this.PostalCode) ? string.Empty : this.PostalCode + " ";
                    result += string.IsNullOrEmpty(this.CityName) ? string.Empty : this.CityName + " ";
                    result += string.IsNullOrEmpty(this.StateName) ? string.Empty : "(" + this.StateName + ") ";
                    result += string.IsNullOrEmpty(this.CountryName) ? string.Empty : this.CountryName + " ";
                    result += (this.Phone == null) ? string.Empty : (" Phone: " + this.Phone);

                    result = result.Trim().Replace("  ", " ");
                    break;
            }

            return result;
        }

        #endregion

        #region Methods

        /// <summary>
        /// extracts a named match string
        /// </summary>
        /// <param name="lineParts">
        /// The line parts. 
        /// </param>
        /// <param name="groupName">
        /// The group name. 
        /// </param>
        /// <param name="defaultValue">
        /// The default value. 
        /// </param>
        /// <returns>
        /// the default value if there is no such group name match
        /// </returns>
        private static string GetMatchGroupWithDefault(MatchCollection lineParts, string groupName, string defaultValue)
        {
            if (lineParts.Count < 1 || lineParts[0].Groups[groupName] == null)
            {
                return defaultValue;
            }

            return lineParts[0].Groups[groupName].ToString().Trim();
        }

        #endregion
    }
}