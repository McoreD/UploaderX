#region License Information (GPL v3)

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
using System.ComponentModel;
using System.Linq;
using System.Text;
// using System.Windows.Forms;

namespace ShareX.UploadersLib
{
    public class CustomUploaderItem
    {
        [DefaultValue("")]
        public string Version { get; set; }

        [DefaultValue("")]
        public string Name { get; set; }

        public bool ShouldSerializeName() => !string.IsNullOrEmpty(Name) && Name != URLHelpers.GetHostName(RequestURL);

        [DefaultValue(CustomUploaderDestinationType.None)]
        public CustomUploaderDestinationType DestinationType { get; set; }

        [DefaultValue(HttpMethod.POST), JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public HttpMethod RequestMethod { get; set; } = HttpMethod.POST;

        // For backward compatibility
        [JsonProperty]
        private HttpMethod RequestType { set => RequestMethod = value; }

        [DefaultValue("")]
        public string RequestURL { get; set; }

        [DefaultValue(null)]
        public Dictionary<string, string> Parameters { get; set; }

        public bool ShouldSerializeParameters() => Parameters != null && Parameters.Count > 0;

        [DefaultValue(null)]
        public Dictionary<string, string> Headers { get; set; }

        public bool ShouldSerializeHeaders() => Headers != null && Headers.Count > 0;

        [DefaultValue(CustomUploaderBody.None)]
        public CustomUploaderBody Body { get; set; }

        [DefaultValue(null)]
        public Dictionary<string, string> Arguments { get; set; }

        public bool ShouldSerializeArguments() => (Body == CustomUploaderBody.MultipartFormData || Body == CustomUploaderBody.FormURLEncoded) &&
            Arguments != null && Arguments.Count > 0;

        [DefaultValue("")]
        public string FileFormName { get; set; }

        public bool ShouldSerializeFileFormName() => Body == CustomUploaderBody.MultipartFormData && !string.IsNullOrEmpty(FileFormName);

        [DefaultValue("")]
        public string Data { get; set; }

        public bool ShouldSerializeData() => (Body == CustomUploaderBody.JSON || Body == CustomUploaderBody.XML) && !string.IsNullOrEmpty(Data);

        // For backward compatibility
        public ResponseType ResponseType { private get; set; }

        [DefaultValue("")]
        public string URL { get; set; }

        [DefaultValue("")]
        public string ThumbnailURL { get; set; }

        [DefaultValue("")]
        public string DeletionURL { get; set; }

        [DefaultValue("")]
        public string ErrorMessage { get; set; }

        private CustomUploaderItem()
        {
        }

        public static CustomUploaderItem Init()
        {
            return new CustomUploaderItem()
            {
                Version = "0.1", //TODO: Application.ProductVersion,
                RequestMethod = HttpMethod.POST,
                Body = CustomUploaderBody.MultipartFormData
            };
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                return Name;
            }

            string name = URLHelpers.GetHostName(RequestURL);

            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }

            return "Name";
        }

        public string GetFileName()
        {
            return ToString() + ".sxcu";
        }

        public string GetRequestURL(CustomUploaderInput input)
        {
            if (string.IsNullOrEmpty(RequestURL))
            {
                throw new Exception("Request must be configured.");
            }

            ShareXCustomUploaderSyntaxParser parser = new ShareXCustomUploaderSyntaxParser(input);
            parser.URLEncode = true;
            string url = parser.Parse(RequestURL);

            url = URLHelpers.FixPrefix(url);

            Dictionary<string, string> parameters = GetParameters(input);
            return URLHelpers.CreateQueryString(url, parameters);
        }

        public Dictionary<string, string> GetParameters(CustomUploaderInput input)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            if (Parameters != null)
            {
                ShareXCustomUploaderSyntaxParser parser = new ShareXCustomUploaderSyntaxParser(input);
                parser.UseNameParser = true;

                foreach (KeyValuePair<string, string> parameter in Parameters)
                {
                    parameters.Add(parameter.Key, parser.Parse(parameter.Value));
                }
            }

            return parameters;
        }

        public string GetContentType()
        {
            switch (Body)
            {
                case CustomUploaderBody.MultipartFormData:
                    return RequestHelpers.ContentTypeMultipartFormData;
                case CustomUploaderBody.FormURLEncoded:
                    return RequestHelpers.ContentTypeURLEncoded;
                case CustomUploaderBody.JSON:
                    return RequestHelpers.ContentTypeJSON;
                case CustomUploaderBody.XML:
                    return RequestHelpers.ContentTypeXML;
                case CustomUploaderBody.Binary:
                    return RequestHelpers.ContentTypeOctetStream;
            }

            return null;
        }

        public string GetData(CustomUploaderInput input)
        {
            NameParser nameParser = new NameParser(NameParserType.Text);
            string result = nameParser.Parse(Data);

            Dictionary<string, string> replace = new Dictionary<string, string>();
            replace.Add("{input}", EncodeBodyData(input.Input));
            replace.Add("{filename}", EncodeBodyData(input.FileName));
            result = result.BatchReplace(replace, StringComparison.OrdinalIgnoreCase);

            return result;
        }

        private string EncodeBodyData(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                if (Body == CustomUploaderBody.JSON)
                {
                    return URLHelpers.JSONEncode(input);
                }
                else if (Body == CustomUploaderBody.XML)
                {
                    return URLHelpers.XMLEncode(input);
                }
            }

            return input;
        }

        public string GetFileFormName()
        {
            if (string.IsNullOrEmpty(FileFormName))
            {
                throw new Exception("File form name must be configured.");
            }

            return FileFormName;
        }

        public Dictionary<string, string> GetArguments(CustomUploaderInput input)
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>();

            if (Arguments != null)
            {
                ShareXCustomUploaderSyntaxParser parser = new ShareXCustomUploaderSyntaxParser(input);
                parser.UseNameParser = true;

                foreach (KeyValuePair<string, string> arg in Arguments)
                {
                    arguments.Add(arg.Key, parser.Parse(arg.Value));
                }
            }

            return arguments;
        }

        public NameValueCollection GetHeaders(CustomUploaderInput input)
        {
            if (Headers != null && Headers.Count > 0)
            {
                NameValueCollection collection = new NameValueCollection();

                ShareXCustomUploaderSyntaxParser parser = new ShareXCustomUploaderSyntaxParser(input);
                parser.UseNameParser = true;

                foreach (KeyValuePair<string, string> header in Headers)
                {
                    collection.Add(header.Key, parser.Parse(header.Value));
                }

                return collection;
            }

            return null;
        }

        public void ParseResponse(UploadResult result, ResponseInfo responseInfo, UploaderErrorManager errors, CustomUploaderInput input, bool isShortenedURL = false)
        {
            if (result != null && responseInfo != null)
            {
                result.ResponseInfo = responseInfo;

                if (responseInfo.ResponseText == null)
                {
                    responseInfo.ResponseText = "";
                }

                ShareXCustomUploaderSyntaxParser parser = new ShareXCustomUploaderSyntaxParser()
                {
                    FileName = input.FileName,
                    ResponseInfo = responseInfo,
                    URLEncode = true
                };

                if (responseInfo.IsSuccess)
                {
                    string url;

                    if (!string.IsNullOrEmpty(URL))
                    {
                        url = parser.Parse(URL);
                    }
                    else
                    {
                        url = parser.ResponseInfo.ResponseText;
                    }

                    if (isShortenedURL)
                    {
                        result.ShortenedURL = url;
                    }
                    else
                    {
                        result.URL = url;
                    }

                    result.ThumbnailURL = parser.Parse(ThumbnailURL);
                    result.DeletionURL = parser.Parse(DeletionURL);
                }
                else
                {
                    if (!string.IsNullOrEmpty(ErrorMessage))
                    {
                        string parsedErrorMessage = parser.Parse(ErrorMessage);

                        if (!string.IsNullOrEmpty(parsedErrorMessage))
                        {
                            errors.AddFirst(parsedErrorMessage);
                        }
                    }
                }
            }
        }

        public void TryParseResponse(UploadResult result, ResponseInfo responseInfo, UploaderErrorManager errors, CustomUploaderInput input, bool isShortenedURL = false)
        {
            try
            {
                ParseResponse(result, responseInfo, errors, input, isShortenedURL);
            }
            catch (JsonReaderException e)
            {
                string hostName = URLHelpers.GetHostName(RequestURL);
                errors.AddFirst($"Invalid response content is returned from host ({hostName}), expected response content is JSON." +
                    Environment.NewLine + Environment.NewLine + e);
            }
            catch (Exception e)
            {
                string hostName = URLHelpers.GetHostName(RequestURL);
                errors.AddFirst($"Unable to parse response content returned from host ({hostName})." +
                    Environment.NewLine + Environment.NewLine + e);
            }
        }

        public void CheckBackwardCompatibility()
        {
            CheckRequestURL();

            if (string.IsNullOrEmpty(Version) || Helpers.CompareVersion(Version, "12.3.1") <= 0)
            {
                if (RequestMethod == HttpMethod.POST)
                {
                    Body = CustomUploaderBody.MultipartFormData;
                }
                else
                {
                    Body = CustomUploaderBody.None;

                    if (Arguments != null)
                    {
                        if (Parameters == null)
                        {
                            Parameters = new Dictionary<string, string>();
                        }

                        foreach (KeyValuePair<string, string> pair in Arguments)
                        {
                            if (!Parameters.ContainsKey(pair.Key))
                            {
                                Parameters.Add(pair.Key, pair.Value);
                            }
                        }

                        Arguments = null;
                    }
                }

                if (ResponseType == ResponseType.RedirectionURL)
                {
                    if (string.IsNullOrEmpty(URL))
                    {
                        URL = "$responseurl$";
                    }

                    URL = URL.Replace("$response$", "$responseurl$");
                    ThumbnailURL = ThumbnailURL?.Replace("$response$", "$responseurl$");
                    DeletionURL = DeletionURL?.Replace("$response$", "$responseurl$");
                }
                else if (ResponseType == ResponseType.Headers)
                {
                    URL = "Response type option is deprecated, please use \\$header:header_name\\$ syntax instead.";
                }
                else if (ResponseType == ResponseType.LocationHeader)
                {
                    if (string.IsNullOrEmpty(URL))
                    {
                        URL = "$header:Location$";
                    }

                    URL = URL.Replace("$response$", "$header:Location$");
                    ThumbnailURL = ThumbnailURL?.Replace("$response$", "$header:Location$");
                    DeletionURL = DeletionURL?.Replace("$response$", "$header:Location$");
                }

                ResponseType = ResponseType.Text;

                Version = "13.7.1";
            }

            if (Helpers.CompareVersion(Version, "13.7.1") <= 0)
            {
                RequestURL = MigrateOldSyntax(RequestURL);

                if (Parameters != null)
                {
                    foreach (string key in Parameters.Keys.ToList())
                    {
                        Parameters[key] = MigrateOldSyntax(Parameters[key]);
                    }
                }

                if (Headers != null)
                {
                    foreach (string key in Headers.Keys.ToList())
                    {
                        Headers[key] = MigrateOldSyntax(Headers[key]);
                    }
                }

                if (Arguments != null)
                {
                    foreach (string key in Arguments.Keys.ToList())
                    {
                        Arguments[key] = MigrateOldSyntax(Arguments[key]);
                    }
                }

                Data = Data.Replace("$input$", "{input}", StringComparison.OrdinalIgnoreCase).
                    Replace("$filename$", "{filename}", StringComparison.OrdinalIgnoreCase);
                URL = MigrateOldSyntax(URL);
                ThumbnailURL = MigrateOldSyntax(ThumbnailURL);
                DeletionURL = MigrateOldSyntax(DeletionURL);
                ErrorMessage = MigrateOldSyntax(ErrorMessage);

                Version = "0.1"; //TODO: Application.ProductVersion;
            }
        }

        private string MigrateOldSyntax(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            StringBuilder sbInput = new StringBuilder();

            bool start = true;

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '$')
                {
                    sbInput.Append(start ? '{' : '}');
                    start = !start;
                    continue;
                }
                else if (input[i] == '\\')
                {
                    i++;
                    continue;
                }
                else if (input[i] == '{' || input[i] == '}')
                {
                    sbInput.Append('\\');
                }

                sbInput.Append(input[i]);
            }

            return sbInput.ToString();
        }

        private void CheckRequestURL()
        {
            if (!string.IsNullOrEmpty(RequestURL))
            {
                NameValueCollection nvc = URLHelpers.ParseQueryString(RequestURL);

                if (nvc != null && nvc.Count > 0)
                {
                    if (Parameters == null)
                    {
                        Parameters = new Dictionary<string, string>();
                    }

                    foreach (string key in nvc)
                    {
                        if (key == null)
                        {
                            foreach (string value in nvc.GetValues(key))
                            {
                                if (!Parameters.ContainsKey(value))
                                {
                                    Parameters.Add(value, "");
                                }
                            }
                        }
                        else if (!Parameters.ContainsKey(key))
                        {
                            string value = nvc[key];
                            Parameters.Add(key, value);
                        }
                    }

                    RequestURL = URLHelpers.RemoveQueryString(RequestURL);
                }
            }
        }
    }
}