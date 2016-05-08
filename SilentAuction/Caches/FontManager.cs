using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;

namespace SilentAuction.Classes
{
    public class FontCacheManager
    {
        private const string _fontsRootPath = "fonts";
        private const string _fontFamily = "font-family:";
        private SortedDictionary<string, Font> _fonts;

        public FontCacheManager()
        {
            _fonts = new SortedDictionary<string, Font>();
        }

        public void RefreshFonts()
        {
            string systemRoot = HostingEnvironment.ApplicationPhysicalPath;
            string fontsPath = systemRoot + _fontsRootPath;
            if (Directory.Exists(fontsPath))
            {
                DirectoryInfo dir = new DirectoryInfo(fontsPath);

                AddFontsFromPath(dir, systemRoot);
            }

            HttpRuntime.Cache.Insert(
                Consts.CacheConsts.FontCache,
                _fonts,
                null,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable,
                null);
        }

        private void AddFontsFromPath(DirectoryInfo dir, string systemRoot)
        {
            foreach(FileInfo file in dir.EnumerateFiles("*.css"))
            {
                AddFontsFromFile(file, systemRoot);
            }

            // iterate sub directories
            foreach (DirectoryInfo subDirectory in dir.EnumerateDirectories())
            {
                AddFontsFromPath(subDirectory, systemRoot);
            }
        }

        private void AddFontsFromFile(FileInfo file, string systemRoot)
        {
            StreamReader sr = new StreamReader(file.FullName);
            while(!sr.EndOfStream)
            {
                string line = sr.ReadLine();

                // TODO: make this a sensible CSS parsing. As it stands, the text could be commented out 
                // and it assumes that the value is on the same line.
                // A CSS parser would be required to do this properly. Doesn't have to recognise all syntax
                // but should be able to pull out properties, pairs, classes etc.
                int index = line.IndexOf(_fontFamily);
                if(index>-1)
                {
                    // this is a font family line.
                    int openQuote = line.IndexOf("'", index);
                    int closeQuote = line.IndexOf("'", openQuote + 1);

                    // is the name valid?
                    if(openQuote >-1 & closeQuote > -1)
                    {
                        Font font = new Font();
                        font.FontName = line.Substring(openQuote+1, closeQuote - openQuote -1);
                        font.CssPhysicalPath = file.FullName;

                        string relativePath = file.FullName.Replace(systemRoot, "");
                        relativePath = relativePath.Replace(@"\", "/");
                        if(!relativePath.StartsWith("/"))
                        {
                            relativePath = "/" + relativePath;
                        }
                        font.CssRelativePath = relativePath;

                        string key = font.FontName.ToLower();
                        if (!_fonts.ContainsKey(key))
                        {
                            _fonts.Add(key, font);
                        }
                    }
                }
                
            }
            
        }

    }
}