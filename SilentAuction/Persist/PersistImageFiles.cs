using SilentAuction.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SilentAuction.Persist
{
	public class PersistImageFiles
	{
        /// <summary>
        /// Saves a list of ImageFile objects
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="imageFiles"></param>
        public void ImagesSave(ApplicationDbContext dbContext, IEnumerable<ImageFile> imageFiles)
        {
            // Create new row for each image file
            foreach (ImageFile file in imageFiles)
            {
                dbContext.ImageFiles.Add(file);
            }
            dbContext.SaveChanges();			// generate keys etc
        }

        /// <summary>
        /// Save images and associate them with the given Lot
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="imageFiles"></param>
        /// <param name="lot"></param>
        public void ImagesSaveForLot(ApplicationDbContext dbContext, IEnumerable<ImageFile> imageFiles, Lot lot, bool createConnectionToLot)
		{
            // save images to database
            ImagesSave(dbContext, imageFiles);

            if (createConnectionToLot)
            {
                // Create links for images for the lot
                int n = 0;
                foreach (ImageFile file in imageFiles)
                {
                    LotImageFile connection = new LotImageFile();
                    connection.ImageFile = file;
                    connection.ImageFileId = file.ImageFileId;
                    connection.Lot = lot;
                    connection.LotId = lot.LotId;
                    connection.Order = n;
                    n++;

                    dbContext.LotImageFiles.Add(connection);
                }
                dbContext.SaveChanges();
            }
		}


		/// <summary>
		/// Save images uploaded in the request
		/// </summary>
		/// <param name="Request"></param>
		/// <param name="serverMapPath"></param>
		/// <returns></returns>
		public List<ImageFile> SaveUploadedFiles(HttpRequestBase Request, string serverMapPath)
		{
			const string imageSavePath = "Images\\Uploads";
			List<ImageFile> files = new List<ImageFile>();
			string fName = "";
			try
			{
				foreach (string fileName in Request.Files)
				{
					// Get file from request object
					HttpPostedFileBase file = Request.Files[fileName];

					fName = file.FileName;
					if (file != null && file.ContentLength > 0)
					{
						// Get directory to save into
						DirectoryInfo saveDirectory = new DirectoryInfo(string.Format("{0}{1}", serverMapPath, imageSavePath));

						// Create folder if necessary
						string pathString = saveDirectory.FullName;
						bool exists = System.IO.Directory.Exists(pathString);
						if (!exists)
						{
							System.IO.Directory.CreateDirectory(pathString);
						}

						// Generate unique filename and save binary file
						string uniuqeFileName = string.Format("{0}_{1}", Path.GetFileNameWithoutExtension(file.FileName), DateTime.UtcNow.ToString("yyyy-MM-dd HH-mm-ss-fffffff"));
						uniuqeFileName += Path.GetExtension(file.FileName);
						string savePath = string.Format("{0}\\{1}", pathString, uniuqeFileName);
						file.SaveAs(savePath);

						// Database entry uses relative path
						string relativePath = string.Format("{0}\\{1}", imageSavePath, uniuqeFileName); 
						ImageFile imageFile = new ImageFile(relativePath);
						files.Add(imageFile);
					}
				}
			}
			catch
			{
			}

			return files;
		}

	}
}