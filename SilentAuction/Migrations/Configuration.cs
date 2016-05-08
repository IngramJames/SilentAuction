namespace SilentAuction.Migrations
{
	using Microsoft.AspNet.Identity;
	using Microsoft.AspNet.Identity.EntityFramework;
    using SilentAuction.Consts;
    using SilentAuction.Models;
    using SilentAuction.Persist;
    using System;
	using System.Collections.Generic;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using System.IO;
	using System.Linq;
	using System.Web.Hosting;

    internal sealed class Configuration : DbMigrationsConfiguration<SilentAuction.Models.ApplicationDbContext>
    {
        private const string _LogSection = "----------{0}-----------";
        private const string _sectionComplete = "Section complete";
        private const string ImagesPath = @"Images\Uploads\";

        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "SilentAuction.Models.ApplicationDbContext";
        }

        protected override void Seed(SilentAuction.Models.ApplicationDbContext context)
        {
            string logDirectory = AppDomain.CurrentDomain.BaseDirectory + @"\logs";
            if(!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            using (StreamWriter sw = new System.IO.StreamWriter(logDirectory + "\\Migrationlog.txt", true))
            {
                sw.WriteLine("-------------------------------------------------------------------------------------------");
                sw.WriteLine(string.Format("{0} Run begins (all datetimes are UTC)", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")));
                sw.WriteLine("-------------------------------------------------------------------------------------------");

                sw.Write("Connection: " + context.Database.Connection.ConnectionString);

                // Delete all stored procs
                string dropFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Sql\StoredProcs\drops");
                RunSqlInFolder(context, dropFolder, sw);

                // Create all stored procs
                string procFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Sql\StoredProcs");
                RunSqlInFolder(context, procFolder, sw);

                sw.WriteLine(string.Format(_LogSection, "Admin user"));
                var userStore = new UserStore<ApplicationUser>(context);
                var userManager = new UserManager<ApplicationUser>(userStore);

                // TODO: get from external config file and throw exception if not there
                string username = "admin";
                string password = "passw0rd";

                if (!context.Users.Any(t => t.UserName == username))
                {
                    sw.WriteLine("Creating Admin user");
                    // set up admin account
                    ApplicationUser user = new ApplicationUser { UserName = username };
                    userManager.Create(user, password);

                    // add admin role
                    context.Roles.AddOrUpdate(r => r.Name, new IdentityRole { Name = "Admin" });
                    context.SaveChanges();

                    // add user to role
                    userManager.AddToRole(user.Id, "Admin");
                }
                else
                {
                    sw.WriteLine("Admin user already exists. Not creating.");
                }
                sw.WriteLine(_sectionComplete);

                //  This method will be called after migrating to the latest version.

                //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
                //  to avoid creating duplicate seed data. E.g.
                //
                //    context.People.AddOrUpdate(
                //      p => p.FullName,
                //      new Person { FullName = "Andrew Peters" },
                //      new Person { FullName = "Brice Lambson" },
                //      new Person { FullName = "Rowan Miller" }
                //    );
                //

                
                /////////////////////////////////////////////////////////////
                // Locales
                /////////////////////////////////////////////////////////////
                sw.WriteLine(string.Format(_LogSection, "Locales"));
                AddOrUpdateLocale(sw, context, "en-GB", "/Images/Flags/en-GB.jpg", "English");
                AddOrUpdateLocale(sw, context, "fr-FR", "/Images/Flags/fr-FR.jpg", "Francais");
                AddOrUpdateLocale(sw, context, "de-DE", "/Images/Flags/de-DE.jpg", "Deustch");
                sw.WriteLine(_sectionComplete);




                /////////////////////////////////////////////////////////////
                // Registration and user account Parameters
                /////////////////////////////////////////////////////////////
                sw.WriteLine(string.Format(_LogSection, "Registration And User Configuration Parameters"));

                // Allow registration (at all - if FALSE then the admn must create users)
                AddOrUpdateParameter(sw, context, Consts.ConfigurationParameterConsts.ParamName_AllowRegistration, true, true, false, ParameterType.Boolean, ParameterCategory.Registration);

                // allow email address for registration
                AddOrUpdateParameter(sw, context, Consts.ConfigurationParameterConsts.ParamName_AllowEmail, true, true, false, ParameterType.Boolean, ParameterCategory.Registration);

                // Allow users to enter any auction
                AddOrUpdateParameter(sw, context, Consts.ConfigurationParameterConsts.ParamName_UsersAllowedAnyAuction, true, true, false, ParameterType.Boolean, ParameterCategory.Registration);
                sw.WriteLine(_sectionComplete);


                ////////////////////////////////////////
                //Auctions
                ////////////////////////////////////////
                sw.WriteLine(string.Format(_LogSection, "Auction Configuration Parameters"));
                // Default minimum bid
                AddOrUpdateParameter(sw, context, Consts.ConfigurationParameterConsts.ParamName_DefaultMinimumBid, 1.01m, true, true, ParameterType.Currency, ParameterCategory.Bid);
                sw.WriteLine(_sectionComplete);

                ////////////////////////////////////////
                //Currency
                ////////////////////////////////////////
                sw.WriteLine(string.Format(_LogSection, "Currency Configuration Parameters"));
                // Default currency details
                AddOrUpdateParameter(sw, context, Consts.ConfigurationParameterConsts.ParamName_CurrencyText, "£", true, false, ParameterType.Text, ParameterCategory.Currency);
                AddOrUpdateParameter(sw, context, Consts.ConfigurationParameterConsts.ParamName_CurrencyPosition, CurrencyTextPosition.Left, true, false, ParameterType.Enumeration, ParameterCategory.Currency);
                AddOrUpdateParameter(sw, context, Consts.ConfigurationParameterConsts.ParamName_CurrencyAddSpace, false, true, false, ParameterType.Boolean, ParameterCategory.Currency);
                AddOrUpdateParameter(sw, context, Consts.ConfigurationParameterConsts.ParamName_CurrencyDecimalPlaces, true, true, false, ParameterType.Boolean, ParameterCategory.Currency);
                sw.WriteLine(_sectionComplete);
                
                ////////////////////////////////////////
                //Fonts
                ////////////////////////////////////////
                sw.WriteLine(string.Format(_LogSection, "Font Configuration Parameters"));
                AddOrUpdateParameter(sw, context, Consts.ConfigurationParameterConsts.ParamName_DefaultFontTitle, Consts.ConfigurationParameterConsts.ParamDefault_DefaultFontTitle, true, false, ParameterType.Text, ParameterCategory.Hidden);
                AddOrUpdateParameter(sw, context, Consts.ConfigurationParameterConsts.ParamName_DefaultFontBody, Consts.ConfigurationParameterConsts.ParamDefault_DefaultFontBody, true, true, ParameterType.Text, ParameterCategory.Hidden);
                sw.WriteLine(_sectionComplete);

                CreateTestData(sw, context);
                sw.WriteLine("-------------------------------------------------------------------------------------------");
                sw.WriteLine(string.Format("{0} Run ends (all datetimes are UTC)", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")));
                sw.WriteLine("-------------------------------------------------------------------------------------------");
                sw.Close();

            }
        }

        private void AddOrUpdateParameter(StreamWriter sw, ApplicationDbContext context, string key, object setting, bool adminSetting, bool userOverridable, ParameterType type, ParameterCategory category)
        {
            sw.Write(string.Format("Adding or updating param \"{0}\"...", key));
            ConfigurationParameter newParam = new ConfigurationParameter
            {
                Key = key,
                AdminSetting = adminSetting,
                UserOverridable = userOverridable,
                Type = type,
                Category = category
            };

            switch(type)
            {
                case ParameterType.Boolean:
                    newParam.SettingAsBool = (bool)setting;
                    break;
                case ParameterType.Currency:
                    newParam.SettingAsDecimal = (decimal)setting;
                    break;
                case ParameterType.Double:
                    newParam.SettingAsDouble = (double)setting;
                    break;
                case ParameterType.Integer:
                case ParameterType.Enumeration:
                    newParam.SettingAsInt = (int)setting;
                    break;
                case ParameterType.Text:
                    newParam.SettingAsString = (string)setting;
                    break;
                default:
                    throw new Exception("Type not found");
            }

            context.SystemParameters.AddOrUpdate(
                s => s.Key,
                newParam
                );
            sw.WriteLine("done.");
        }

        private void AddOrUpdateLocale(StreamWriter sw, ApplicationDbContext context, string key, string flagPath, string displayName)
        {
            sw.Write(string.Format("Adding or updating key=\"{0}\", flagPath=\"{1}\", displayName=\"{2}\"...", key, flagPath, displayName));
            context.Locales.AddOrUpdate(
                s => s.Key,
                new Locale
                {
                    Key = key,
                    FlagPath = flagPath,
                    DisplayName = displayName
                }
            );

            sw.WriteLine("done.");

        }


		private void CreateTestData(StreamWriter sw, SilentAuction.Models.ApplicationDbContext context)
		{
            sw.WriteLine(string.Format(_LogSection, "Creating test data"));
            PersistLot persistLot = new PersistLot();
			PersistImageFiles persistImageFiles = new PersistImageFiles();

			if(!CopyTestImages(sw))
            {
                // Test images already exist: assume test data does as well
                sw.WriteLine("Test image data already exists. Aborting.");
                return;
            }

			List<ImageFile> images = new List<ImageFile>();
			AddImageFile(images, "porsche.jpg");
			AddImageFile(images, "porsche2.jpg");
			AddImageFile(images, "porsche3.jpg");
			AddImageFile(images, "porsche4.jpg");

			Lot lot = new Lot("Porsche 911", "A fast car. A very fast car. In fact, if you've got a moment, it's a car as fast as a very fast thing which has no reason to be slow.", 9999.99m);
            sw.WriteLine("Creating lot \"{0}\"", lot.Name);
            persistLot.Create(context, lot, images);
            sw.WriteLine("done");

            images = new List<ImageFile>();
			AddImageFile(images, "teacup.jpg");
			lot = new Lot("Tea cup (white)", "A lovely tea cup, just for you.", 0.50m);
            sw.WriteLine("Creating lot \"{0}\"", lot.Name);
            persistLot.Create(context, lot, images);
            sw.WriteLine("done");

            images = new List<ImageFile>();
			AddImageFile(images, "teacup2.jpg");
			lot = new Lot("Tea cup (deluxe)", "Revel in the luxury of a tea-cup painted with awful taste!", 0.75m);
            sw.WriteLine("Creating lot \"{0}\"", lot.Name);
            persistLot.Create(context, lot, images);
            sw.WriteLine("done");

            images = new List<ImageFile>();
			AddImageFile(images, "teacupwithtea.jpg");
			lot = new Lot("Tea cup (pre-filled)", "Why mess about waiting? Have your tea now, with this splendid pre-filled tea cup", 1.00m);
            sw.WriteLine("Creating lot \"{0}\"", lot.Name);
            persistLot.Create(context, lot, images);
            sw.WriteLine("done");

            images = new List<ImageFile>();
			AddImageFile(images, "yfronts.jpg");
			lot = new Lot("Men's lingerie", "Sexy it up, and bring spice back into your marriage!", 5.00m);
            sw.WriteLine("Creating lot \"{0}\"", lot.Name);
            persistLot.Create(context, lot, images);
            sw.WriteLine("done");

            sw.WriteLine(_sectionComplete);
        }


        private void AddImageFile(List<ImageFile> images, string imageName)
		{
			ImageFile imageFile = new ImageFile(ImagesPath + imageName);
			images.Add(imageFile);
		}

		private string GetRootDirectory()
		{
			string binPath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).AbsolutePath;
			string binDir = Path.GetDirectoryName(binPath);
			DirectoryInfo tmp = new DirectoryInfo(binDir + "\\..");
			string hostPath = tmp.FullName + "\\";
			return hostPath;			
		}

		private bool CopyTestImages(StreamWriter sw)
		{
            sw.WriteLine("Copying test images to /Images/Uploads");
            bool copied = false;
			string hostPath = GetRootDirectory();
			DirectoryInfo testDataDirectory = new DirectoryInfo(hostPath + "TestData");

            sw.WriteLine(string.Format("Test image path is \"{0}\"", testDataDirectory.FullName));

            // ensure that destination directory exists
            string destDirectoryName = hostPath + ImagesPath;
            sw.WriteLine("Destination directory is \"{0}\"", destDirectoryName);
            if(!Directory.Exists(destDirectoryName))
            {
                sw.WriteLine("Destination directory does not exist. Creating...");
                Directory.CreateDirectory(destDirectoryName);
                sw.WriteLine("Done.");
            }

            foreach (FileInfo fi in testDataDirectory.EnumerateFiles("*.jpg"))
			{
                sw.WriteLine(string.Format("Copying {0}...", fi.FullName));
                string copyTo = hostPath + ImagesPath + fi.Name;
                sw.WriteLine(string.Format("To......{0}", copyTo));
                if (!File.Exists(copyTo))
				{
                    fi.CopyTo(copyTo);
                    copied = true;
                    sw.WriteLine("Done.");
				}
                else
                {
                    sw.WriteLine("File exsists. Aborting.");
                }
			}

            return copied;
		}

        private void RunSqlInFolder(ApplicationDbContext dbContext, string sqlFolder, StreamWriter sw)
        {
            string sectionTitle = string.Format("Running scripts in folder \"{0}\"", sqlFolder);
            sw.WriteLine(string.Format(_LogSection, sectionTitle));

            foreach (string file in Directory.GetFiles(sqlFolder, "*.sql"))
            {
                sw.WriteLine(string.Format("{0} Executing {1}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), file));
                dbContext.Database.ExecuteSqlCommand(File.ReadAllText(file), new object[0]);
                sw.WriteLine(string.Format("{0} Execution complete", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")));
            }
            sw.WriteLine(_sectionComplete);
        }
    }



}
