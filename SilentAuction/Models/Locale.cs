using Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SilentAuction.Models
{
    /// <summary>
    /// Locale data. DisplayName holds the name to show the user eg "English (UK)", "Francais (France)". Input not required: this will be a techie action to add data to tables.
    /// If they can create a new resource file, they can add a row to a database.
    /// May want to change that later, just to be nice, but not right now.
    /// TODO: Make nice for foreign users to add new languages
    /// </summary>
    public class Locale
    {
        [Key]
        [Required]
        [StringLength(10)]
        [DataType(DataType.Text)]
        public string Key { get; set; }

        [Required]
        [StringLength(200)]
        [DataType(DataType.Text)]
        public string FlagPath { get; set; }

        [Required]
        [StringLength(100)]
        [DataType(DataType.Text)]
        public string DisplayName { get; set; }
    }
}