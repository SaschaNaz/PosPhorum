using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Globalization.DateTimeFormatting;
using Windows.Storage;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Posroid
{
    public sealed partial class DietDay : UserControl
    {
        public DietDay()
        {
            this.InitializeComponent();
        }
    }


    public class DateConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (value == null)
                throw new ArgumentNullException("value", "Value cannot be null.");

            if (!typeof(DateTime).Equals(value.GetType()))
                throw new ArgumentException("Value must be of type DateTime.", "value");

            DateTime dt = (DateTime)value;

            if (parameter == null)
            {
                // Date "7/27/2011 9:30:59 AM" returns "7/27/2011"
                return DateTimeFormatter.ShortDate.Format(dt);
            }
            else if ((string)parameter == "day")
            {
                // Date "7/27/2011 9:30:59 AM" returns "27"
                DateTimeFormatter dateFormatter = new DateTimeFormatter("{day.integer(2)}");
                return dateFormatter.Format(dt);
            }
            else if ((string)parameter == "month")
            {
                // Date "7/27/2011 9:30:59 AM" returns "JUL"
                DateTimeFormatter dateFormatter = new DateTimeFormatter("{month.integer(2)}");
                return dateFormatter.Format(dt);
            }
            else if ((string)parameter == "MM/DD")
            {
                // Date "7/27/2011 9:30:59 AM" returns "JUL"
                DateTimeFormatter dateFormatter1 = new DateTimeFormatter("{month.integer(2)}");
                DateTimeFormatter dateFormatter2 = new DateTimeFormatter("{day.integer(2)}");
                return String.Format("{0}/{1}",dateFormatter1.Format(dt), dateFormatter2.Format(dt));
            }
            else if ((string)parameter == "MM/DD-dayofweek")
            {

                DateTimeFormatter dateFormatter1 = new DateTimeFormatter("{month.integer(2)}");
                DateTimeFormatter dateFormatter2 = new DateTimeFormatter("{day.integer(2)}");
                DateTimeFormatter dateFormatter3 = new DateTimeFormatter("{dayofweek.full}");
                return String.Format("{0}/{1}\n{2}", dateFormatter1.Format(dt), dateFormatter2.Format(dt), dateFormatter3.Format(dt));
            }
            else if ((string)parameter == "MM/DD/dayofweek")
            {

                DateTimeFormatter dateFormatter1 = new DateTimeFormatter("{month.integer(2)}");
                DateTimeFormatter dateFormatter2 = new DateTimeFormatter("{day.integer(2)}");
                DateTimeFormatter dateFormatter3 = new DateTimeFormatter("{dayofweek.full}");
                return String.Format("{0}/{1}/{2}", dateFormatter1.Format(dt), dateFormatter2.Format(dt), dateFormatter3.Format(dt));
            }
            else if ((string)parameter == "dayofweek")
            {
                DateTimeFormatter dateFormatter = new DateTimeFormatter("{dayofweek.full}");
                return dateFormatter.Format(dt);
            }
            //else if ((string)parameter == "year")
            //{
            //    // Date "7/27/2011 9:30:59 AM" returns "2011"
            //    DateTimeFormatter dateFormatter = new DateTimeFormatter("{year.full}");
            //    return dateFormatter.Format(dt);
            //}
            else
            {
                // Requested format is unknown. Return in the original format.
                return dt.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            string strValue = value as string;
            DateTime resultDateTime;
            if (DateTime.TryParse(strValue, out resultDateTime))
            {
                return resultDateTime;
            }
            return Windows.UI.Xaml.DependencyProperty.UnsetValue;
        }
    }

    public class LanguageConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (value == null)
                throw new ArgumentNullException("value", "Value cannot be null.");

            if (!typeof(LangString).Equals(value.GetType()))
                throw new ArgumentException("Value must be of type LangString.", "value");

            LangString ls = (LangString)value;

            String returner;
            if (!(Boolean)ApplicationData.Current.LocalSettings.Values["ForceKorean"])
            {
                //returns the name for OS default language for default value.
                returner = ls.NameByLanguage(
                    new Windows.Globalization.Language(Windows.Globalization.ApplicationLanguages.Languages[0]));
            }
            else
            {
                returner = ls.NameByLanguage(new Windows.Globalization.Language("ko"));
            }

            if (parameter != null)
                return parameter + returner;
            else
                return returner;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DayBrushConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (value == null)
                throw new ArgumentNullException("value", "Value cannot be null.");

            if (!typeof(DateTime).Equals(value.GetType()))
                throw new ArgumentException("Value must be of type DateTime.", "value");

            DayOfWeek dayofweek = ((DateTime)value).DayOfWeek;

            if (dayofweek == DayOfWeek.Monday)
            {
                return new SolidColorBrush(new Windows.UI.Color() { A = 0xFF, R = 0xFF, G = 0x66, B = 0x66 });
            }
            else if (dayofweek == DayOfWeek.Tuesday)
            {
                return new SolidColorBrush(new Windows.UI.Color() { A = 0xFF, R = 0xFF, G = 0xC1, B = 0x66 });
            }
            else if (dayofweek == DayOfWeek.Wednesday)
            {
                return new SolidColorBrush(new Windows.UI.Color() { A = 0xFF, R = 0xFF, G = 0xFF, B = 0x66 });
            }
            else if (dayofweek == DayOfWeek.Thursday)
            {
                return new SolidColorBrush(new Windows.UI.Color() { A = 0xFF, R = 0x66, G = 0xFF, B = 0x66 });
            }
            else if (dayofweek == DayOfWeek.Friday)
            {
                return new SolidColorBrush(new Windows.UI.Color() { A = 0xFF, R = 0x66, G = 0x66, B = 0xFF });
            }
            else if (dayofweek == DayOfWeek.Saturday)
            {
                return new SolidColorBrush(new Windows.UI.Color() { A = 0xFF, R = 0xC1, G = 0x66, B = 0xFF });
            }
            else if (dayofweek == DayOfWeek.Sunday)
            {
                return new SolidColorBrush(new Windows.UI.Color() { A = 0xFF, R = 0xFF, G = 0x66, B = 0xFF });
            }
            else
            {
                return new SolidColorBrush(Windows.UI.Colors.Black);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    //public class MealInfoConverter : Windows.UI.Xaml.Data.IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, string culture)
    //    {
    //        if (value == null)
    //            throw new ArgumentNullException("value", "Value cannot be null.");

    //        IMealBlock block = value as IMealBlock;

    //        if (block == null)
    //        {
    //            throw new ArgumentException("Value must be of type IMealBlock.", "value");
    //        }

    //        return ((IMealBlock)value).InternalData;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, string culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class MealtimeStringConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (value == null)
                throw new ArgumentNullException("value", "Value cannot be null.");

            if (!typeof(When).Equals(value.GetType()))
                throw new ArgumentException("Value must be of type When.", "value");

            When mealtime = (When)value;
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();

            switch (mealtime)
            {
                case When.Breakfast:
                    return loader.GetString("Breakfast");
                case When.Lunch:
                    return loader.GetString("Lunch");
                case When.Dinner:
                    return loader.GetString("Dinner");
                default:
                    return loader.GetString("(Error)");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MealtimeBrushConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (value == null)
                throw new ArgumentNullException("value", "Value cannot be null.");

            if (!typeof(When).Equals(value.GetType()))
                throw new ArgumentException("Value must be of type When.", "value");

            When mealtime = (When)value;

            switch (mealtime)
            {
                case When.Breakfast:
                    return new SolidColorBrush(new Windows.UI.Color() { A = 0xFF, R = 0x0E, G = 0x9C, B = 0x00 });
                case When.Lunch:
                    return new SolidColorBrush(new Windows.UI.Color() { A = 0xFF, R = 0x00, G = 0x63, B = 0x9C });
                case When.Dinner:
                    return new SolidColorBrush(new Windows.UI.Color() { A = 0xFF, R = 0xE0, G = 0x00, B = 0x85 });
                default:
                    return new SolidColorBrush(Windows.UI.Colors.Transparent);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IntCaloriesConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (value == null)
                throw new ArgumentNullException("value", "Value cannot be null.");

            if (!typeof(Int32).Equals(value.GetType()))
                throw new ArgumentException("Value must be of type Int32.", "value");

            Int32 cal = (Int32)value;
            if (cal != -1)
                return String.Format("{0}kcal", cal);
            else
                return "(Unknown)";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    //public class MealFoodConverter : Windows.UI.Xaml.Data.IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, string culture)
    //    {
    //        if (value == null)
    //            throw new ArgumentNullException("value", "Value cannot be null.");

    //        if (!typeof(Time[]).Equals(value.GetType()))
    //            throw new ArgumentException("Value must be of type Time array.", "value");

    //        Time[] times = (Time[])value;

    //        if (times == null)
    //        {
    //            return new Time[] { };
    //        }
    //        else
    //        {
    //            List<MealBlockShort> infolist = new List<MealBlockShort>();
    //            foreach (Time time in times)
    //            {
    //                List<MealBlockShort> smalllist = new List<MealBlockShort>();
    //                Int32 i = 0;
    //                Int32 lastNumber = 0;
    //                Int32 lastCalories = 0;
    //                foreach (FoodsInfo info in time.WhatFoods)
    //                {
    //                    smalllist.Add(new MealBlockShort(info.Type, time.Mealtime, info.Kilocalories) { Width = 120, Height = 120 });
    //                    if (lastCalories < info.Kilocalories)
    //                    {
    //                        lastCalories = info.Kilocalories;
    //                        lastNumber = i;
    //                    }
    //                    i++;
    //                }
    //                if (i != 0)
    //                {
    //                    Double d = smalllist[lastNumber].Width;
    //                    smalllist[lastNumber].Width = (d + 10) * 2 - 10;
    //                }
    //                infolist.AddRange(smalllist);
    //                //가로만 확장하면 영 어색하므로 그에 맞게 최적화
    //                //저 랩그리드 무한히 안 가도록 새로운 패널 만들기 ㅎㅎ 그냥 아이템즈컨트롤에 ArrangeOverride 같은 걸 씌우나?
    //            }
    //            return infolist.ToArray();
    //        }
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, string culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    //public class HeightConverter : Windows.UI.Xaml.Data.IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, string culture)
    //    {
    //        if (value == null)
    //            throw new ArgumentNullException("value", "Value cannot be null.");

    //        if (!typeof(Double).Equals(value.GetType()))
    //            throw new ArgumentException("Value must be of type double.", "value");

    //        Double times = (Double)value;

    //        if (times < 190)
    //        {
    //            return 0;
    //        }
    //        else
    //        {
    //            return times - 190;
    //        }
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, string culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
