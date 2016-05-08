using SilentAuction.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace SilentAuction.Models
{
	// An image file. Just an ID and a path.
	// This data will never be displayed, but used to display the images in question.
	public class ImageFile
	{
		public int ImageFileId { get; set; }

		[Required]
		public string LocalRelativePath { get; set; }

		public virtual ICollection<LotImageFile> ImageFileLots { get; set; }


		public ImageFile() { }

        public string WebPath
        {
            get
            {
                // convert local windows file path to one suitable for the web
                string localrelativepath = "/" + LocalRelativePath;
                string webPath = localrelativepath.Replace("\\", "/");
                return webPath;
            }
        }

		public ImageFile(string path)
		{
            LocalRelativePath = path;
		}

        public string GetThumbnail(int height)
        {
            return Utils.GetThumbnail(LocalRelativePath, height);
        }

	}
}