using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntuneDriveMapping.Models
{
    public class DriveLetter
    {
        public static IEnumerable<SelectListItem> GetDriveLetters()
        {
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            char[] chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

            foreach (Char letter in chars)
            {
                selectListItems.Add(new SelectListItem
                {
                    Text = new string(letter + ":"),
                    Value = letter.ToString()
                });
            }

            return selectListItems;
        }
    }
}
