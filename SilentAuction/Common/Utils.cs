using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace SilentAuction.Common
{
    public static class Utils
    {
        public static string StringToJSFormat(string text)
        {
            text = text.Replace(Environment.NewLine, "\\n");
            text = text.Replace("\"", "\\\"");      // quote with escaped quote
            return text;
        }

        public static string StringToHTML(string text)
        {
            string[] lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            StringBuilder sb = new StringBuilder(text.Length * 2);

            foreach (string line in lines)
            {
                string fixedLine = HttpUtility.HtmlEncode(line);

                // preserve spaces at the start of a line.
                if (fixedLine.StartsWith(" "))
                {
                    // get the number of spaces
                    int n = 0;
                    bool foundAll = false;
                    string fixPadding = string.Empty;
                    while (!foundAll && n < fixedLine.Length)
                    {
                        if (fixedLine.Substring(n, 1) == " ")
                        {
                            n++;
                            fixPadding += "&nbsp;";
                        }
                        else
                        {
                            foundAll = true;
                        }
                    }

                    // replace only spaces at the start.
                    fixedLine = fixPadding + fixedLine.Substring(n);
                }

                sb.AppendFormat("<p>{0}</p>", fixedLine);
            }


            return sb.ToString();
        }

        /// <summary>
        /// Returns the path to the thumbnail file.
        /// Creates the thumbnail file if it does not already exist.
        /// </summary>
        /// <returns></returns>
        public static string GetThumbnail(string path, int height)
        {
            const string errorImage = "/Images/Error.jpg";
            const string noImage = "/Images/NoImage.png";
            string errorImageFullPath = HttpUtility.UrlPathEncode(HostingEnvironment.ApplicationVirtualPath + errorImage);

            try
            {
                path = path.Replace("/", @"\");

                // Get full physical path
                string bigPath = HostingEnvironment.ApplicationPhysicalPath + path;
                if (!System.IO.File.Exists(bigPath))
                {
                    // original file does not exist
                    return noImage;
                }

                string originalPath = System.IO.Path.GetDirectoryName(path);
                string physicalDirectory = System.IO.Path.GetDirectoryName(bigPath);
                string thumbnailSuffix = string.Format("_{0}", height);
                string thumbnailFilenameNoExt = System.IO.Path.GetFileNameWithoutExtension(bigPath) + thumbnailSuffix;
                string thumbsPath = physicalDirectory + "\\thumbs\\";    
                string thumbnailPhysicalPath = thumbsPath + thumbnailFilenameNoExt + ".png";

                if (!System.IO.Directory.Exists(thumbsPath))
                {
                    System.IO.Directory.CreateDirectory(thumbsPath);
                }

                if (!System.IO.File.Exists(thumbnailPhysicalPath))
                {
                    // create thumbnail file
                    using (Image bigImage = new Bitmap(bigPath))
                    {
                        // Algorithm simplified for purpose of example.
                        int newHeight = height;
                        double resizeRatio = bigImage.Height / newHeight;
                        int newWidth = (int)(bigImage.Width / resizeRatio);

                        // Now create a thumbnail
                        using (Image smallImage = new Bitmap(newWidth, newHeight))
                        {
                            using (var graphicsHandle = Graphics.FromImage(smallImage))
                            {
                                graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                graphicsHandle.SmoothingMode = SmoothingMode.HighQuality;
                                graphicsHandle.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                graphicsHandle.DrawImage(bigImage, 0, 0, newWidth, newHeight);
                            }
                            smallImage.Save(thumbnailPhysicalPath, System.Drawing.Imaging.ImageFormat.Png);
                        }

                    }
                }

                // now generate the URL of the thumbnail
                string thumbnailRelativePath = originalPath + "/thumbs/" + thumbnailFilenameNoExt + ".png";
                thumbnailRelativePath = thumbnailRelativePath.Replace(@"\", "/");

                // ensure images always go from the root folder, because we have locale directories at the start of URLs
                // eg: /en-GB/Images... but we want /Images
                if (!thumbnailRelativePath.StartsWith(@"/"))
                {
                    thumbnailRelativePath = @"/" + thumbnailRelativePath;
                }
                return HttpUtility.UrlPathEncode(thumbnailRelativePath);
            }
            catch
            {
                return errorImageFullPath;
            }
        }

        /// <summary>
        /// Return TRUE/FALSE from a checkbox element on a form
        /// </summary>
        /// <param name="formValue"></param>
        /// <returns></returns>
        public static bool GetCheckboxValue(string formValue)
        {
            // HTML doesn't allow just "false" to be returned. So the standard CheckboxFor contains a hidden control with a duplicate name
            // and the value "false". If the checkbox is not checked, the actual checkbox control is not submitted, so the hidden value of "false"
            // gets sent. If the checkbox IS checked, the BOTH values are submitted.
            // Old, old HTML reasons which nobody knows about now.
            if (formValue == "true,false")
            {
                return true;
            }

            return false;

        }
    }
}