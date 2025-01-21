using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UI_v1
{
    public partial class ConfirmWindow2 : Window
    {
        public string String1 { get; private set; }
        public string String2 { get; private set; }
        public int Number { get; private set; }

        public ConfirmWindow2()
        {
            this.Background = new SolidColorBrush(Colors.LightBlue);

            var grid = new Grid
            {
                Background = new SolidColorBrush(Colors.LightBlue),
                Margin = new Thickness(10)
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var string1Label = new TextBlock { Text = "你想要怎麼樣的聲音", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            var string1Box = new TextBox { Margin = new Thickness(5) };
            Grid.SetRow(string1Label, 0);
            Grid.SetColumn(string1Label, 0);
            Grid.SetRow(string1Box, 0);
            Grid.SetColumn(string1Box, 1);
            grid.Children.Add(string1Label);
            grid.Children.Add(string1Box);

            var numberLabel = new TextBlock { Text = "這段聲音要幾秒", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            var numberBox = new TextBox { Margin = new Thickness(5) };
            Grid.SetRow(numberLabel, 1);
            Grid.SetColumn(numberLabel, 0);
            Grid.SetRow(numberBox, 1);
            Grid.SetColumn(numberBox, 1);
            grid.Children.Add(numberLabel);
            grid.Children.Add(numberBox);

            var string2Label = new TextBlock { Text = "幫這段聲音取名字", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5) };
            var string2Box = new TextBox { Margin = new Thickness(5) };
            Grid.SetRow(string2Label, 2);
            Grid.SetColumn(string2Label, 0);
            Grid.SetRow(string2Box, 2);
            Grid.SetColumn(string2Box, 1);
            grid.Children.Add(string2Label);
            grid.Children.Add(string2Box);

            var okButton = new Button
            {
                Content = "確定",
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetRow(okButton, 3);
            Grid.SetColumnSpan(okButton, 2);
            grid.Children.Add(okButton);

            okButton.Click += (s, e) =>
            {
                String1 = string1Box.Text;
                String2 = string2Box.Text;

                if (int.TryParse(numberBox.Text, out int result))
                {
                    Number = result;
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("請輸入有效的數字！", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            this.Content = grid;
            this.Title = "AI幫幫我";
            this.Width = 400;
            this.Height = 250;
        }
    }
}
