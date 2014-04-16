using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PosphorumUniversal
{
    class CustomGridView : GridView
    {
        protected override void PrepareContainerForItemOverride(Windows.UI.Xaml.DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            if ((item as MealData).HighestCalories)
            {  
                VariableSizedWrapGrid.SetRowSpan(element as UIElement, 2);
                VariableSizedWrapGrid.SetColumnSpan(element as UIElement, 1);
            }
        }
    }
}
