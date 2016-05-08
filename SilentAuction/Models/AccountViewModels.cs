using Common.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SilentAuction.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
		[DisplayNameLocalized(typeof(SilentAuction.Resources.TextStrings), "Email")]
		public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }

    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
		[DisplayNameLocalized(typeof(SilentAuction.Resources.TextStrings), "Code")]
		public string Code { get; set; }
        public string ReturnUrl { get; set; }

		[DisplayNameLocalized(typeof(SilentAuction.Resources.TextStrings), "RememberThisBrowser")]
		public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
		[DisplayNameLocalized(typeof(SilentAuction.Resources.TextStrings), "Email")]
		public string Email { get; set; }
    }

    public class LoginViewModel
    {
		[Required]
		[StringLengthLocalized(20, 4, typeof(SilentAuction.Resources.TextStrings), "ValidationErrorMinLength")]
		[DataType(DataType.Text)]
		[DisplayNameLocalized(typeof(SilentAuction.Resources.TextStrings), "Username")]
		public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
		[DisplayNameLocalized(typeof(SilentAuction.Resources.TextStrings), "Password")]
		public string Password { get; set; }

		[DisplayNameLocalized(typeof(SilentAuction.Resources.TextStrings), "RememberMe")]
		public bool RememberMe { get; set; }
	}

    public class RegisterViewModel
    {
		[Required]
		[StringLengthLocalized(20, 4, typeof(SilentAuction.Resources.TextStrings), "ValidationErrorMinLength")]
		[DataType(DataType.Text)]
		[DisplayNameLocalized(typeof(SilentAuction.Resources.TextStrings), "Username")]
		public string Username { get; set; }

        [EmailAddress]
		[DisplayNameLocalized(typeof(SilentAuction.Resources.TextStrings), "Email")]
		public string Email { get; set; }

        [Required]
		[StringLengthLocalized(20, 6, typeof(SilentAuction.Resources.TextStrings), "ValidationErrorMinLength")]
		[DataType(DataType.Password)]
		[DisplayNameLocalized(typeof(SilentAuction.Resources.TextStrings), "Password")]
		public string Password { get; set; }

        [DataType(DataType.Password)]
		[DisplayNameLocalized(typeof(SilentAuction.Resources.TextStrings), "ConfirmPassword")]
		[CompareLocalized("Password", typeof(SilentAuction.Resources.TextStrings), "ValidationPasswordsDoNotMatch")]
		public string ConfirmPassword { get; set; }

		[DisplayNameLocalized(typeof(SilentAuction.Resources.TextStrings), "RememberMe")]
		public bool RememberMe { get; set; }

        public virtual ICollection<Bid> Bids { get; set; }

    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
		[DisplayNameLocalized(typeof(SilentAuction.Resources.TextStrings), "Email")]
		public string Email { get; set; }

        [Required]
		[StringLengthLocalized(20, 6, typeof(SilentAuction.Resources.TextStrings), "ValidationErrorMinLength")]
		[DataType(DataType.Password)]
		[DisplayNameLocalized(typeof(SilentAuction.Resources.TextStrings), "Password")]
		public string Password { get; set; }

        [DataType(DataType.Password)]
		[DisplayNameLocalized(typeof(SilentAuction.Resources.TextStrings), "ConfirmPassword")]
		[CompareLocalized("Password", typeof(SilentAuction.Resources.TextStrings), "ValidationPasswordsDoNotMatch")]
		public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
		[DisplayNameLocalized(typeof(SilentAuction.Resources.TextStrings), "Email")]
		public string Email { get; set; }
    }
}
