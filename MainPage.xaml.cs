/*
Copyright © 2012, Shai Raiten
All rights reserved.
http://blogs.microsoft.co.il/blogs/shair
 */
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Phone.Controls;
using System.ComponentModel;
using System.Windows.Threading;

namespace Puzzle15
{
    public partial class MainPage : PhoneApplicationPage, INotifyPropertyChanged
    {
        private readonly int[] _bordersNums = { 0, 4, 8, 12, 3, 7, 11, 15 };
        private readonly Random _rnd;
        private readonly DispatcherTimer _timer;
        private bool _firstLoad = true;

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private int _moves;
        private DateTime _startTime;

        public MainPage()
        {
            InitializeComponent();

            _rnd = new Random();
            _timer = new DispatcherTimer();
            _timer.Tick += new EventHandler(TimerTick);
            _timer.Interval = new TimeSpan(0, 0, 0, 1);
        }

        void TimerTick(object sender, EventArgs e)
        {
            var time = DateTime.Now - _startTime;
            txtTime.Text = string.Format(Const.TimeFormat, time.Hours, time.Minutes, time.Seconds);
        }

        /// <summary>
        /// The Range of all Stackpanels is between 15 and 0, when 15 is the first (top left) and 0 is last (right bottom).
        /// 15 , 14 , 13 , 12
        /// 11 , 10 , 09 , 08
        /// 07 , 06 , 05 , 04
        /// 03 , 02 , 01 , 00
        /// The values are 1 to 16, meaning that 15 equals 1 and 00 equals 16
        /// </summary>
        public void NewGame()
        {
            _moves = 0;
            txtMoves.Text = "0";
            txtTime.Text = Const.DefaultTimeValue;

            Scrambles();
            while (!CheckIfSolvable())
            {
                Scrambles();
            }

            _startTime = DateTime.Now.AddSeconds(1);
            _timer.Start();

            GridScrambling.Visibility = System.Windows.Visibility.Collapsed;
        }

        /// <summary>
        /// Find the parent of image with a specific Tag value
        /// </summary>
        /// <param name="tag">The tag value of image you want to find.</param>
        /// <returns>Return the Stackpanel contains the image.</returns>
        StackPanel FindStackPanelByTagId(int tag)
        {
            if (tag == 16)
            {
                return (from stackPanel in ContentPanel.Children.OfType<StackPanel>()
                        where stackPanel.Children.Count == 0
                        select stackPanel).First();
            }
            else
            {
                return (from stackPanel in ContentPanel.Children.OfType<StackPanel>()
                        from img in stackPanel.Children.OfType<Image>()
                        where Convert.ToInt32(img.Tag) == tag
                        select stackPanel).First();
            }
        }

        /// <summary>
        /// Find the position of stackpanel without children.
        /// </summary>
        /// <returns></returns>
        int FindEmptyItemPosition()
        {
            int index = 15;
            for (int i = 0; i < 15; i++)
            {
                if (((StackPanel)ContentPanel.Children[i]).Children.Count == 0)
                    return index;

                index--;
            }
            return 0;
        }

        /// <summary>
        /// Get the Tag value by StackPanel position.
        /// </summary>
        /// <param name="position">position of StackPanel</param>
        /// <returns>The Image Tag value, if there is no images then returns - 16</returns>
        int FindItemValueByPosition(int position)
        {
            return ((StackPanel)ContentPanel.Children[position]).Children.Count > 0 ?
                Convert.ToInt32(((Image)((StackPanel)ContentPanel.Children[position]).Children[0]).Tag) : 16;
        }

        /// <summary>
        /// Runs n times and generate random numbers from 1 to 16, for each number find the current stackpanel that hold him.(FindStackPanelByTagId)
        /// If First and Second number are smaller then 16 then - swipe the images and tag values.
        /// If One of the values is 16 the swipe  - One Spackpanel will be cleared of Items.
        /// </summary>
        void Scrambles()
        {
            var count = 0;
            while (count < 25)
            {
                var a = _rnd.Next(1, 17);
                var b = _rnd.Next(1, 17);

                if (a == b) continue;

                var stack1 = FindStackPanelByTagId(a);
                var stack2 = FindStackPanelByTagId(b);

                if (a == 16)
                {
                    var image2 = stack2.Children[0];
                    stack2.Children.Clear();
                    stack1.Children.Add(image2);
                }
                else if (b == 16)
                {
                    var image1 = stack1.Children[0];
                    stack1.Children.Clear();
                    stack2.Children.Add(image1);
                }
                else
                {
                    var image1 = stack1.Children[0];
                    var image2 = stack2.Children[0];

                    stack1.Children.Clear();
                    stack2.Children.Clear();

                    stack1.Children.Add(image2);
                    stack2.Children.Add(image1);
                }

                count++;
            }
        }

        /// <summary>
        /// Each move the user do, perform a loop and checks values from 1 to 16.
        /// if the numbers are not in the correct order than nothing happed.
        /// </summary>
        void CheckBoard()
        {
            var index = 1;
            for (var i = 15; i > 0; i--)
            {
                if (FindItemValueByPosition(i) != index) return;
                index++;
            }

            _timer.Stop();
            WinGrid.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// Check if the current Scramble is solveable.
        /// </summary>
        /// <returns></returns>
        bool CheckIfSolvable()
        {
            var n = 0;
            for (var i = 1; i <= 16; i++)
            {
                if (!(ContentPanel.Children[i] is StackPanel)) continue;

                var num1 = FindItemValueByPosition(i);
                var num2 = FindItemValueByPosition(i - 1);

                if (num1 > num2)
                {
                    n++;
                }
            }

            var emptyPos = FindEmptyItemPosition();
            return n % 2 == (emptyPos + emptyPos / 4) % 2 ? true : false;
        }

        /// <summary>
        /// Click Event on all images,
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            var item = (UIElement)e.OriginalSource;
            if (!(item is Image)) return;

            var to = CanMove(item);

            if (to != null)
            {
                _moves++;
                txtMoves.Text = _moves.ToString();
                MoveItem(item, to);
                CheckBoard();
            }

            e.Handled = true;
            e.Complete();
        }

        /// <summary>
        /// Move Item From One StackPanel to Another.
        /// </summary>
        /// <param name="item">Gets the Image item you want to move</param>
        /// <param name="targetPanel">Destination StackPanel</param>
        void MoveItem(UIElement item, StackPanel targetPanel)
        {
            foreach (var stackPanel in
                ContentPanel.Children.OfType<StackPanel>().Where(stackPanel => stackPanel.Children.Count > 0 && stackPanel.Children.Contains(item)))
            {
                stackPanel.Children.Remove(item);
            }

            targetPanel.Children.Add(item);
        }

        /// <summary>
        /// Bug Fix - if both of the items you want to swipe are in the Board borders do nothing.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        bool IsBorderSwich(int a, int b)
        {
            return _bordersNums.Contains(a) && _bordersNums.Contains(b);
        }

        /// <summary>
        /// Check if the Item Can move, Checking all panels around the specific item with -1 +1 -4 +4, if one of them is empty then he can move.
        /// </summary>
        /// <param name="itemToMove">The Item that has been click by user.</param>
        /// <returns></returns>
        StackPanel CanMove(UIElement itemToMove)
        {
            var count = ContentPanel.Children.Count;
            for (var i = 0; i < count; i++)
            {
                if (!(ContentPanel.Children[i] is StackPanel)) continue;

                var stackPanel = (StackPanel)ContentPanel.Children[i];
                if (!stackPanel.Children.Contains(itemToMove)) continue;

                if (!IsBorderSwich(i, i + 1) && i + 1 <= 15 && ContentPanel.Children[i + 1] != null && ((StackPanel)ContentPanel.Children[i + 1]).Children.Count == 0)
                    return ((StackPanel)ContentPanel.Children[i + 1]);

                if (!IsBorderSwich(i, i - 1) && i - 1 > -1 && ContentPanel.Children[i - 1] != null && ((StackPanel)ContentPanel.Children[i - 1]).Children.Count == 0)
                    return ((StackPanel)ContentPanel.Children[i - 1]);

                if (i + 4 <= 15 && ContentPanel.Children[i + 4] != null && ((StackPanel)ContentPanel.Children[i + 4]).Children.Count == 0)
                    return ((StackPanel)ContentPanel.Children[i + 4]);

                if (i - 4 > -1 && ContentPanel.Children[i - 4] != null && ((StackPanel)ContentPanel.Children[i - 4]).Children.Count == 0)
                    return ((StackPanel)ContentPanel.Children[i - 4]);

            }
            return null;
        }

        private void BtnNoThanksManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            WinGrid.Visibility = System.Windows.Visibility.Collapsed;
            NewGame();
            e.Handled = true;
            e.Complete();
        }

        private void PhoneApplicationPageLoaded(object sender, RoutedEventArgs e)
        {
            if (!_firstLoad) return;

            _firstLoad = false;
            GridScrambling.Visibility = System.Windows.Visibility.Visible;
            NewGame();
        }

        private void BtnHelpClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Help.xaml", UriKind.Relative));
        }

        private void BtnPlayClick(object sender, EventArgs e)
        {
            GridScrambling.Visibility = System.Windows.Visibility.Visible;
            NewGame();
        }

        private void BtnAboutClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }

        private void BtnSettingsClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
        }
    }
}
