using System;
using System.Globalization;
using System.Windows.Data;

namespace GlobalPopup
{
    public class FormFilledMultiConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            for (int i = 0; i < values.Length; i++)
                if (values[i] == null)
                    return false;

            string titel = values[0].ToString();
            string beskrivelse = values[1].ToString();
            string cpr = values[2].ToString();
            int selectedIndex1 = System.Convert.ToInt32(values[3]);
            int selectedIndex2 = System.Convert.ToInt32(values[4]);
            int selectedIndex3 = System.Convert.ToInt32(values[5]);
            int selectedIndex4 = System.Convert.ToInt32(values[6]);

            // Hvis noget tekst er skrevet i "Titel" og "Beskrivelse", samt alle comboboxes har en værdi, returnér true, ellers returnér false
            return (titel.Length > 0 && beskrivelse.Length > 0 && cpr.Length > 0 && selectedIndex1 != -1
                && selectedIndex2 != -1 && selectedIndex3 != -1 && selectedIndex4 != -1) ? true : false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}