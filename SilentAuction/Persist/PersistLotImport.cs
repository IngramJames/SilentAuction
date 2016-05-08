using SilentAuction.Models;
using SilentAuction.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SilentAuction.Persist
{
    public class PersistLotImport
    {
        private const string _fieldName = "name";
        private const string _fieldDescription = "description";
        private const string _fieldReserve = "reserve";

        List<ImportError> _errors;

        public Dictionary<string, ImportFile> ImportFromRequest(HttpRequestBase request)
        {
            _errors = new List<ImportError>();
            Dictionary<string, ImportFile> files = GetSortedFilenamesFromRequest(request);
            foreach (KeyValuePair<string, ImportFile> kvp in files)
            {
                ImportFile importFile = kvp.Value;
                // read from http request
                HttpPostedFileBase file = importFile.HttpFile;

                // Read input stream
                StreamReader sr = new StreamReader(file.InputStream);
                int dataRows;
                List<Lot> fileLots = ImportFile(sr, file.FileName, out dataRows);
                importFile.GoodLots = fileLots;
                importFile.DataRows = dataRows;
            }
            return files;
        }

        public List<Lot> ImportFile(StreamReader sr, string fileName, out int dataRows)
        {
            dataRows = 0;
            List<Lot> newLots = new List<Lot>();
            Dictionary<string, int> headerLookup;
            if (ReadHeadings(sr, fileName, out headerLookup))
            {
                // get indexes of the fields we want to pull out
                int nameIndex = headerLookup[_fieldName];
                int descriptionIndex = headerLookup[_fieldDescription];
                int reserveIndex = -1;
                if (headerLookup.ContainsKey(_fieldReserve))
                {
                    reserveIndex = headerLookup[_fieldReserve];
                }

                // Read all data from file
                dataRows = ReadDataFromFile(sr, fileName, nameIndex, descriptionIndex, reserveIndex, headerLookup.Count, newLots);
            }
            return newLots;

        }


        private int ReadDataFromFile(StreamReader sr, string fileName, int nameIndex, int descriptionIndex, int reserveIndex, int noOfCols, List<Lot> newLots)
        {
            int dataRows = 0;
            int lineNo = 1;     // data starts on the 2nd row - after headings. Will be incremented immediately
            while (!sr.EndOfStream)
            {
                dataRows++;
                lineNo++;
                string dataLine = sr.ReadLine();
                string[] fields = dataLine.Split(new char[] { ',' }, StringSplitOptions.None);
                bool valid = true;      // optimism

                // catch wrong no of columns
                if (fields.Length != noOfCols)
                {
                    string error = string.Format(Resources.Errors.ImportLineWrongNoOfCols, noOfCols, fields.Length);
                    AddError(fileName, lineNo, error);
                    continue;
                }

                // create lot from data
                Lot newLot = new Lot();

                newLot.Name = fields[nameIndex];
                newLot.Description = fields[descriptionIndex];

                // check for blank name
                if (newLot.Name.Trim().Length == 0)
                {
                    AddErrorMissingField(fileName, lineNo, _fieldName);
                    valid = false;
                }

                // check for blank description
                if (newLot.Description.Trim().Length == 0)
                {
                    AddErrorMissingField(fileName, lineNo, _fieldDescription);
                    valid = false;
                }

                // validate and add reserve (if present)
                if (reserveIndex > -1)
                {
                    string reserveText = fields[reserveIndex];
                    decimal reserve;
                    if (!decimal.TryParse(reserveText, out reserve))
                    {
                        AddError(fileName, lineNo, Resources.Errors.ReserveMustBePositiveNumber);
                        valid = false;
                    }
                    newLot.Reserve = reserve;
                }

                if (valid)
                {
                    newLots.Add(newLot);
                }
            }
            return dataRows;
        }

        private Dictionary<string, ImportFile> GetSortedFilenamesFromRequest(HttpRequestBase request)
        {
            // get all the file names and file objects into a dictionary
            Dictionary<string, ImportFile> files = new Dictionary<string, ImportFile>();
            foreach (string requestFilename in request.Files)
            {
                // Get file from request object
                HttpPostedFileBase file = request.Files[requestFilename];

                if (file != null)
                {
                    string fileName = file.FileName;

                    ImportFile importFile = new ImportFile();
                    importFile.Filename = fileName;
                    importFile.HttpFile = file;
                    importFile.IsHttp = true;
                    files.Add(fileName, importFile);
                }
            }

            // sort the keys
            List<string> list = files.Keys.ToList();
            list.Sort();

            // rearrange so that the dictionary is in alphabetical order
            Dictionary<string, ImportFile> sorted = new Dictionary<string, ImportFile>();
            foreach (string fileKey in list)
            {
                sorted.Add(fileKey, files[fileKey]);
            }
            return sorted;
        }

        private bool ReadHeadings(StreamReader sr, string fileName, out Dictionary<string, int> headerLookup)
        {
            headerLookup = new Dictionary<string, int>();
            string headerLine = sr.ReadLine();

            // check that there is data in the first row
            if (headerLine == null || headerLine.Trim().Length == 0)
            {
                AddError(fileName, Resources.Errors.ImportFileNoHeader);
                return false;
            }

            // extract headers
            string[] headers = headerLine.Split(new char[] { ',' }, StringSplitOptions.None);

            // hack into a dictionary for fast easy lookup validation
            for (int n = 0; n < headers.Length; n++)
            {
                headerLookup.Add(headers[n].ToLower(), n);
            }

            // verify that mandatory data is present
            bool validHeader = VerifyHeader(_fieldName, headerLookup, fileName);
            validHeader = validHeader & VerifyHeader(_fieldDescription, headerLookup, fileName);

            if (!validHeader)
            {
                return false;
            }
            return true;

        }

        private void AddError(string filename, string errorText)
        {
            ImportError err = new ImportError(filename, errorText);
            _errors.Add(err);
        }

        private void AddError(string filename, int lineNo, string errorText)
        {
            ImportError err = new ImportError(filename, lineNo, errorText);
            _errors.Add(err);
        }

        private void AddErrorMissingField(string filename, int lineNo, string field)
        {
            string fieldError = string.Format(Resources.Errors.ImportFieldCannotBeBlank, field);
            AddError(filename, lineNo, fieldError);

        }

        private void AddErrorMissingHeader(string filename, string headerName)
        {
            AddError(filename, string.Format(Resources.Errors.ImportHeaderMissing, headerName));
        }

        private bool VerifyHeader(string headerName, Dictionary<string, int> headers, string fileName)
        {
            if (headers.ContainsKey(headerName))
            {
                return true;
            }

            // missing header.
            AddErrorMissingHeader(fileName, headerName);
            return false;
        }

        public string[,] GetAllErrors()
        {
            string[,] errorArray = new string[_errors.Count, 3];

            // prepare the errors for display
            int n = 0;
            foreach (ImportError error in _errors)
            {
                errorArray[n, 0] = error.Filename;
                errorArray[n, 1] = error.LineNo.ToString();
                errorArray[n, 2] = error.Error;
                n++;
            }

            return errorArray;
        }

        public string[,] GetGoodLotsSummary(Dictionary<string, ImportFile> importFiles)
        {
            string[,] summary = new string[importFiles.Count, 3];
            int n = 0;
            foreach (KeyValuePair<string, ImportFile> kvp in importFiles)
            {
                ImportFile importFile = kvp.Value;
                summary[n, 0] = importFile.Filename;
                summary[n, 1] = importFile.DataRows.ToString();             // the total number of data rows
                summary[n, 2] = importFile.GoodLots.Count.ToString();       // the lots which were good
                n++;
            }

            return summary;
        }


        public List<Lot> GetLotsForSaving(Dictionary<string, ImportFile> importFiles)
        {
            List<Lot> lots = new List<Lot>();
            foreach(KeyValuePair<string, ImportFile> kvp in importFiles)
            {
                ImportFile importFile = kvp.Value;
                foreach(Lot lot in importFile.GoodLots)
                {
                    lots.Add(lot);
                }
            }
            return lots;
        }

        public int ErrorCount {  get { return _errors.Count; } }
    }


}