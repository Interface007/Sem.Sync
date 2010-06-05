﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ContactClient.cs" company="Sven Erik Matzen">
//   Copyright (c) Sven Erik Matzen. GNU Library General Public License (LGPL) Version 2.1.
// </copyright>
// <summary>
//   Client implementation for reading information from www.wer-kennt-wen.de
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sem.Sync.Connector.WerKenntWen
{
    #region usings

    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;

    using Sem.GenericHelpers;
    using Sem.GenericHelpers.Entities;
    using Sem.Sync.SyncBase;
    using Sem.Sync.SyncBase.Attributes;
    using Sem.Sync.SyncBase.DetailData;
    using Sem.Sync.SyncBase.Interfaces;

    #endregion usings

    /// <summary>
    /// Client implementation for reading information from www.wer-kennt-wen.de
    /// </summary>
    [ClientStoragePathDescription(Irrelevant = true)]
    [ConnectorDescription(CanReadContacts = true, CanWriteContacts = false, NeedsCredentials = true,
        NeedsCredentialsDomain = false, DisplayName = "Wer-Kennt-Wen.de",
        MatchingIdentifier = ProfileIdentifierType.WerKenntWenUrl)]
    public class ContactClient : WebScrapingBaseClient // StdClient, IExtendedReader
    {
        #region Constants and Fields
        
        private const string wkwCaptcha = "wkw/captcha/";

        private WebSideParameters parameters = new WebSideParameters
                    {
                        HttpUrlBaseAddress = "http://www.wer-kennt-wen.de",
                        ContactListUrl = "/people/friends",
                        ExtractorFriendUrls = @".div class=""pl-pic"".*?a href=""(?<id>[a-zA-Z0-9/]+)""",
                        ExtractorProfilePictureUrl = "div id=\"person_picture\".*?img src=\"(.*?)\"",
                        HttpDataLogOnRequest = "loginName={0}&pass={1}&x=0&y=0&logIn=1", 
                        HttpDetectionStringLogOnFailed = "/app/user?op=lostpassword", 
                        HttpDetectionStringLogOnNeeded = new[] { "id=\"loginform\"" }, 
                        HttpUrlContactDownload = "{0}",
                        HttpUrlFriendList = "/people/friends",
                        HttpUrlLogOnRequest = "/start.php",
                        ////ImagePlaceholderUrl = ,
                        PersonIdentifierFromContactsListRegex = "/users/([a-zA-Z0-9 %\\+]*)/([a-zA-Z0-9 %\\+-]*)",
                        ProfileIdentifierType = ProfileIdentifierType.WerKenntWenUrl,
                        ProfileIdFormatter = "{*}",
                        ProfileIdPartExtractor = ".*",
                    };

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "ContactClient" /> class.
        /// </summary>
        public ContactClient()
            : base(new HttpHelper("http://www.wer-kennt-wen.de", true))
        {
        }

        #endregion

        #region Properties

        /// <summary>
        ///   Gets the user readable name of the client implementation. This name should
        ///   be specific enough to let the user know what element store will be accessed.
        /// </summary>
        public override string FriendlyClientName
        {
            get
            {
                return "Wer-Kennt-Wen";
            }
        }

        #endregion

        #region Methods

        protected override StdContact ConvertToStdContact(string contactUrl, string content)
        {
            var dataExtractor = new Regex("/users/([a-zA-Z0-9 %\\+]*)/([a-zA-Z0-9 %\\+-]*)", RegexOptions.Singleline);
            var matches = dataExtractor.Matches(content);

            var contact = new StdContact
            {
                Id = Guid.NewGuid(),
                PersonalAddressPrimary = new AddressDetail(),
                Name = new PersonName(),
            };

            var birthday = string.Empty;
            var birthyear = string.Empty;

            foreach (Match match in matches)
            {
                var key = match.Groups[1].ToString();

                // the encoding needs to be set to get the correct special chars
                var value = HttpUtility.UrlDecode(match.Groups[2].ToString(), Encoding.GetEncoding("iso8859-1"));
                switch (key)
                {
                    case "":
                        break;

                    // The following information is provided by Wer-kennt-wen, but not handled by sem.sync, yet.
                    // It may be handled with one of the next versions.
                    case "hobbies":
                    case "music":
                    case "movies":
                    case "placesToVisit":
                    case "placesVisited":
                    case "books":
                    case "jobclass":
                        break;

                    case "birthName":
                        contact.Name.FormerName = value;
                        break;

                    case "partnership":
                        switch (value)
                        {
                            case "1":
                                contact.RelationshipStatus = RelationshipStatus.InARelationship;
                                break;

                            case "4":
                                contact.RelationshipStatus = RelationshipStatus.Single;
                                break;

                            case "5":
                                contact.RelationshipStatus = RelationshipStatus.Married;
                                break;

                            default:
                                Tools.DebugWriteLine(contact + " : " + key + " = " + value);
                                break;
                        }

                        break;

                    case "firstName":
                        contact.Name.FirstName = value;
                        break;

                    case "lastName":
                        contact.Name.LastName = value;
                        break;

                    case "city":
                        contact.PersonalAddressPrimary.CityName = value;
                        break;

                    case "zipCode":
                        contact.PersonalAddressPrimary.PostalCode = value;
                        break;

                    case "gender":
                        contact.PersonGender = match.Groups[2].ToString() == "1" ? Gender.Female : Gender.Male;
                        break;

                    case "birthyear":
                        birthyear = value;
                        break;

                    case "birthday":
                        birthday = value;
                        break;

                    default:
                        Tools.DebugWriteLine("unknown attribute: " + key + " - " + value);
                        break;
                }
            }

            if (!string.IsNullOrEmpty(birthyear) && !string.IsNullOrEmpty(birthday))
            {
                if (birthday.Length == 2)
                {
                    birthday = birthday + "-01";
                }

                contact.DateOfBirth = new DateTime(
                    int.Parse(birthyear.Substring(0, 4), CultureInfo.InvariantCulture),
                    int.Parse(birthday.Substring(0, 2), CultureInfo.InvariantCulture),
                    int.Parse(birthday.Substring(3, 2), CultureInfo.InvariantCulture));
            }

            contact.ExternalIdentifier.SetProfileId(ProfileIdentifierType.WerKenntWenUrl, contactUrl);

            dataExtractor = new Regex(this.parameters.ExtractorProfilePictureUrl, RegexOptions.Singleline);
            matches = dataExtractor.Matches(content);

            if (matches.Count == 1)
            {
                var pictureName = matches[0].Groups[1].ToString();
                if (!pictureName.Contains("images/dummy"))
                {
                    contact.PictureName = pictureName.Substring(pictureName.LastIndexOf('/') + 1);
                    contact.PictureData = this.HttpRequester.GetContentBinary(pictureName, pictureName);
                }
            }

            this.LogProcessingEvent(contact, "downloaded");
            return contact;
        }

        /// <summary>
        /// The get image from page.
        /// </summary>
        /// <param name="page"> The page content. </param>
        /// <returns> The captcha image from the page. </returns>
        /// <exception cref="NotImplementedException"></exception>
        private static string GetImageUrlFromPage(string page)
        {
            var imageUrl = Regex.Match(page, "<iframe src=\"(http://api.recaptcha.net/noscript[?]k=[a-zA-Z0-9]*)");
            if (imageUrl.Groups.Count == 2)
            {
                return imageUrl.Groups[1].ToString();
            }

            throw new NotImplementedException();
        }
        
        private bool GetLogon()
        {
            if (string.IsNullOrEmpty(this.LogOnPassword))
            {
                this.QueryForLogOnCredentials("Wer kennt wen benötigt die Log-In-Daten.");
            }

            // tell the user that we need to log on
            this.LogProcessingEvent("Wer kennt wen benötigt die Log-In-Daten.", this.LogOnUserId);

            // prepare the post data for log on
            var postData = HttpHelper.PreparePostData(this.parameters.HttpDataLogOnRequest, this.LogOnUserId, this.LogOnPassword);

            // post to get the cookies
            var logInResponse = this.HttpRequester.GetContentPost(this.parameters.HttpUrlLogOnRequest, string.Empty, postData);

            return !logInResponse.Contains(this.parameters.HttpDetectionStringLogOnFailed);
        }

        /// <summary>
        /// The resolve captcha.
        /// </summary>
        private void ResolveCaptcha()
        {
            var request = new CaptchaResolveRequest
            {
                UrlOfWebSite = "http://www.wer-kennt-wen.de/captcha",
            };

            var page = this.HttpRequester.GetContent(request.UrlOfWebSite);
            using (var imageStream = new MemoryStream(this.HttpRequester.GetContentBinary(GetImageUrlFromPage(page))))
            {
                request.CaptchaImage = Image.FromStream(imageStream);
                imageStream.Dispose();
            }

            this.UiDispatcher.ResolveCaptcha(
                "WKW will ein Captcha gelöst haben.",
                "WKW Captcha",
                request);
        }

        #endregion

        protected override WebSideParameters WebSideParameters
        {
            get
            {
                return parameters;
            }
        }

        public StdElement FillContacts(StdElement contactToFill, ICollection<MatchingEntry> baseline)
        {
            var contact = contactToFill as StdContact;
            const ProfileIdentifierType ProfileIdentifierType = ProfileIdentifierType.WerKenntWenUrl;
            const string ProfilePhpId = "/person/";

            if (contact == null || !contact.ExternalIdentifier.ContainsKey(ProfileIdentifierType))
            {
                return contactToFill;
            }

            var profileIdInformation = contact.ExternalIdentifier[ProfileIdentifierType];
            if (profileIdInformation == null || string.IsNullOrWhiteSpace(profileIdInformation.Id))
            {
                return contactToFill;
            }

            var offset = 0;
            var added = 0;
            while (true)
            {
                this.LogProcessingEvent("reading contacts ({0})", offset);
                this.HttpRequester.ContentCredentials = this;

                // get the contact list
                var url = string.Format(
                    CultureInfo.InvariantCulture,
                    "http://www.wer-kennt-wen.de/people/friends/{0}/sort/friends/0/0/{1}",
                    profileIdInformation.Id.Replace(ProfilePhpId, string.Empty),
                    offset);

                string profileContent;
                while (true)
                {
                    profileContent = this.HttpRequester.GetContent(url, string.Format(CultureInfo.InvariantCulture, "Wkw-{0}", offset));
                    if (profileContent.Contains(@"id=""loginform"""))
                    {
                        if (!this.GetLogon())
                        {
                            return contactToFill;
                        }

                        continue;
                    }

                    if (profileContent.Contains(wkwCaptcha))
                    {
                        this.ResolveCaptcha();
                        continue;
                    }

                    break;
                }

                var extracts = Regex.Matches(profileContent, @"\<a href=""/person/(?<profileId>[0-9a-zA-Z]*)""\>", RegexOptions.Singleline);

                // if there is no contact in list, we did reach the end
                if (extracts.Count < 3)
                {
                    break;
                }

                // create a new instance of a list of references if there is none
                contact.Contacts = contact.Contacts ?? new List<ContactReference>(extracts.Count);

                foreach (Match extract in extracts)
                {
                    var profileId = ProfilePhpId + extract.Groups["profileId"];
                    var stdId =
                        (from x in baseline
                         where x.ProfileId.GetProfileId(ProfileIdentifierType) == profileId
                         select x.Id).FirstOrDefault();

                    // we ignore contacts we donn't know
                    if (stdId == default(Guid))
                    {
                        continue;
                    }

                    // lookup an existing entry in this contacts contact-list
                    var contactInList = (from x in contact.Contacts where x.Target == stdId select x).FirstOrDefault();

                    if (contactInList == null)
                    {
                        // add a new one if no existing entry has been found
                        contactInList = new ContactReference { Target = stdId };
                        contact.Contacts.Add(contactInList);
                        added++;
                    }

                    // update the flag that this entry is a private contact
                    // todo: private/business contact
                    contactInList.IsPrivateContact = true;
                }

                Thread.Sleep(new Random().Next(230, 8789));

                offset += 64; // extracts.Count;
            }

            this.LogProcessingEvent(contact, "{0} contacts found, {1} new added", offset, added);

            return contactToFill;
        }
    }
}