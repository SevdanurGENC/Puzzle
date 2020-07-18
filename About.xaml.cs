/*
Copyright © 2012, Shai Raiten
All rights reserved.
http://blogs.microsoft.co.il/blogs/shair
 */
using System.Windows.Input;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace Puzzle15
{
    public partial class About : PhoneApplicationPage
    {
        private const string BlogUri = "http://www.sevdanurgenc.com/";
        public About()
        {
            InitializeComponent();
        }

        private void TextBlock1ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            var task = new WebBrowserTask {URL = BlogUri};
            task.Show();
        }
    }
}