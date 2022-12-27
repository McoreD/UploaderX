﻿#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2022 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using Newtonsoft.Json;
using ShareX.HelpersLib;
// using ShareX.UploadersLib.Properties;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Threading;
// using System.Windows.Forms;

namespace ShareX.UploadersLib.FileUploaders
{
    public class GfycatFileUploaderService : FileUploaderService
    {
        public override FileDestination EnumValue { get; } = FileDestination.Gfycat;

        // public override Image ServiceImage => Resources.Gfycat;

        public override bool CheckConfig(UploadersConfig config)
        {
            return config.GfycatAccountType == AccountType.Anonymous || OAuth2Info.CheckOAuth(config.GfycatOAuth2Info);
        }

        public override GenericUploader CreateUploader(UploadersConfig config, TaskReferenceHelper taskInfo)
        {
            if (config.GfycatOAuth2Info == null)
            {
                config.GfycatOAuth2Info = new OAuth2Info(APIKeys.GfycatClientID, APIKeys.GfycatClientSecret);
            }

            return new GfycatUploader(config.GfycatOAuth2Info)
            {
                UploadMethod = config.GfycatAccountType,
                Private = !config.GfycatIsPublic,
                KeepAudio = config.GfycatKeepAudio,
                Title = config.GfycatTitle
            };
        }

        // public override TabPage GetUploadersConfigTabPage(UploadersConfigForm form) => form.tpGfycat;
    }

    public class GfycatUploader : FileUploader, IOAuth2
    {
        public OAuth2Info AuthInfo { get; set; }
        public AccountType UploadMethod { get; set; }
        public OAuth2Token AnonymousToken { get; set; }
        public bool NoResize { get; set; } = true;
        public bool IgnoreExisting { get; set; } = true;
        public bool Private { get; set; } = true;
        public bool KeepAudio { get; set; } = true;
        public string Title { get; set; }

        private const string URL_AUTHORIZE = "https://gfycat.com/oauth/authorize";
        private const string URL_UPLOAD = "https://filedrop.gfycat.com";
        private const string URL_API = "https://api.gfycat.com/v1";
        private const string URL_API_TOKEN = URL_API + "/oauth/token";
        private const string URL_API_CREATE_GFY = URL_API + "/gfycats";
        private const string URL_API_STATUS = URL_API + "/gfycats/fetch/status/";

        public GfycatUploader(OAuth2Info oauth)
        {
            AuthInfo = oauth;
        }

        public string GetAuthorizationURL()
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("client_id", AuthInfo.Client_ID);
            args.Add("scope", "all");
            args.Add("state", "ShareX");
            args.Add("response_type", "code");
            args.Add("redirect_uri", Links.Callback);

            return URLHelpers.CreateQueryString(URL_AUTHORIZE, args);
        }

        public bool GetAccessToken(string code)
        {
            string request = JsonConvert.SerializeObject(new
            {
                client_id = AuthInfo.Client_ID,
                client_secret = AuthInfo.Client_Secret,
                grant_type = "authorization_code",
                redirect_uri = Links.Callback,
                code = code
            });

            string response = SendRequest(HttpMethod.POST, URL_API_TOKEN, request, RequestHelpers.ContentTypeJSON);

            if (!string.IsNullOrEmpty(response))
            {
                OAuth2Token token = JsonConvert.DeserializeObject<OAuth2Token>(response);

                if (token != null && !string.IsNullOrEmpty(token.access_token))
                {
                    token.UpdateExpireDate();
                    AuthInfo.Token = token;
                    return true;
                }
            }

            return false;
        }

        public bool RefreshAccessToken()
        {
            if (OAuth2Info.CheckOAuth(AuthInfo) && !string.IsNullOrEmpty(AuthInfo.Token.refresh_token))
            {
                string request = JsonConvert.SerializeObject(new
                {
                    refresh_token = AuthInfo.Token.refresh_token,
                    client_id = AuthInfo.Client_ID,
                    client_secret = AuthInfo.Client_Secret,
                    grant_type = "refresh"
                });

                string response = SendRequest(HttpMethod.POST, URL_API_TOKEN, request, RequestHelpers.ContentTypeJSON);

                if (!string.IsNullOrEmpty(response))
                {
                    OAuth2Token token = JsonConvert.DeserializeObject<OAuth2Token>(response);

                    if (token != null && !string.IsNullOrEmpty(token.access_token))
                    {
                        token.UpdateExpireDate();
                        AuthInfo.Token = token;
                        return true;
                    }
                }
            }

            return false;
        }

        public bool CheckAuthorization()
        {
            if (OAuth2Info.CheckOAuth(AuthInfo))
            {
                if (AuthInfo.Token.IsExpired && !RefreshAccessToken())
                {
                    Errors.Add("Refresh access token failed.");
                    return false;
                }
            }
            else
            {
                Errors.Add("Gfycat login is required.");
                return false;
            }

            return true;
        }

        public override UploadResult Upload(Stream stream, string fileName)
        {
            AllowReportProgress = false;

            OAuth2Token token = GetOrCreateToken();
            if (token == null)
            {
                return null;
            }

            NameValueCollection headers = new NameValueCollection();
            headers.Add("Authorization", "Bearer " + token.access_token);

            GfycatCreateResponse gfy = CreateGfycat(headers);
            if (gfy == null)
            {
                return null;
            }

            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("key", gfy.GfyName);

            AllowReportProgress = true;

            UploadResult result = SendRequestFile(URL_UPLOAD, stream, fileName, "file", args);
            if (!result.IsError)
            {
                WaitForTranscode(gfy.GfyName, result);
            }

            return result;
        }

        private void WaitForTranscode(string name, UploadResult result)
        {
            ProgressManager progress = new ProgressManager(10000);
            progress.CustomProgressText = "Gfycat encoding...";
            OnProgressChanged(progress);

            int iterations = 0;

            while (!StopUploadRequested)
            {
                string statusJson = SendRequest(HttpMethod.GET, URL_API_STATUS + name);
                GfycatStatusResponse response = JsonConvert.DeserializeObject<GfycatStatusResponse>(statusJson);

                if (response.Error != null)
                {
                    Errors.Add(response.Error);
                    result.IsSuccess = false;
                    break;
                }
                else if (response.Task.Equals("error", StringComparison.OrdinalIgnoreCase))
                {
                    Errors.Add(response.Description);
                    result.IsSuccess = false;
                    break;
                }
                else if (response.GfyName != null)
                {
                    result.IsSuccess = true;
                    result.URL = "https://gfycat.com/" + response.GfyName;
                    break;
                }
                else if ((response.Task.Equals("NotFoundo", StringComparison.OrdinalIgnoreCase) ||
                    response.Task.Equals("NotFound", StringComparison.OrdinalIgnoreCase)) && iterations > 5)
                {
                    Errors.Add("Gfy not found");
                    result.IsSuccess = false;
                    break;
                }

                if (progress.UpdateProgress((progress.Length - progress.Position) / response.Time))
                {
                    OnProgressChanged(progress);
                }

                iterations++;
                Thread.Sleep(500);
            }

            progress.CustomProgressText = "";
            OnProgressChanged(progress);
        }

        private GfycatCreateResponse CreateGfycat(NameValueCollection headers)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("private", Private);
            args.Add("noResize", NoResize);
            args.Add("noMd5", IgnoreExisting);
            args.Add("keepAudio", KeepAudio);
            if (!string.IsNullOrEmpty(Title)) args.Add("title", Title);

            string json = JsonConvert.SerializeObject(args);

            string response = SendRequest(HttpMethod.POST, URL_API_CREATE_GFY, json, RequestHelpers.ContentTypeJSON, null, headers);

            if (!string.IsNullOrEmpty(response))
            {
                return JsonConvert.DeserializeObject<GfycatCreateResponse>(response);
            }

            return null;
        }

        private OAuth2Token GetOrCreateToken()
        {
            if (UploadMethod == AccountType.User)
            {
                if (!CheckAuthorization())
                {
                    return null;
                }

                return AuthInfo.Token;
            }
            else
            {
                if (AnonymousToken == null || AnonymousToken.IsExpired)
                {
                    string request = JsonConvert.SerializeObject(new
                    {
                        client_id = AuthInfo.Client_ID,
                        client_secret = AuthInfo.Client_Secret,
                        grant_type = "client_credentials",
                    });

                    string response = SendRequest(HttpMethod.POST, URL_API_TOKEN, request, RequestHelpers.ContentTypeJSON);

                    if (!string.IsNullOrEmpty(response))
                    {
                        AnonymousToken = JsonConvert.DeserializeObject<OAuth2Token>(response);

                        if (AnonymousToken != null && !string.IsNullOrEmpty(AnonymousToken.access_token))
                        {
                            AnonymousToken.UpdateExpireDate();
                        }
                    }
                }

                return AnonymousToken;
            }
        }
    }

    public class GfycatCreateResponse
    {
        public string GfyName { get; set; }
        public string Secret { get; set; }
    }

    public class GfycatStatusResponse
    {
        public string Task { get; set; }
        public int Time { get; set; }
        public string GfyName { get; set; }
        public string Error { get; set; }
        public string Description { get; set; }
    }
}