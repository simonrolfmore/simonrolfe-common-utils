using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimonRolfe.Web.Mvc
{
    public class MvcConsts
    {
        public const string Email_Address_Validation_Regex = @"(?:[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*\"")@"
                                           + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
                                           + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
                                           + @"([0-9a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$"; //test cases from http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx, apart from tests 4 and 6, which should not be valid but are, as backslash characters have to be escaped. Really, who would do that?

        public const string Blank_Item_Text = "Please select...";

        public const string PasswordRegex = @"(?=^.{6,128}$)(?=(?:.*?\d){1})(?=.*[a-z])(?=(?:.*?[A-Z]){1})(?=(?:.*?[!@#\\$%*()_+^&}{:;?.£]){1})(?!.*\s)[0-9a-zA-Z!@#\\$%*()_+^&}{:;?.£]*$";
        public const string PasswordValidationMsg = "Passwords must be between 6 and 128 characters long and should contain at least 1 number, 1 uppercase letter, 1 lowercase letter and 1 special character.";
        public const string PasswordPolicy = "<strong>Your password should be comprised of</strong><ul><li>Minimum 6 characters</li><li>Maximum 128 characters</li><li>One Number</li><li>One Uppercase Letter</li><li>One Lowercase Letter</li><li>One Special character</li><li>Special characters allowed are <strong>!@#$%*()_+^ &}{:;?.£</strong></li></ul>";

        public const string NoSpacesRegex = @"[^\s]+";
        public const string NoSpacesValidationMsg = "Usernames cannot include spaces";

    }
}
