using System.Windows;

namespace AssignmentTracker.Views
{
    public partial class InputDialog : Window
    {
        public string Input { get; private set; }

        public InputDialog(string title, string prompt, string[] comboBoxItems = null)
        {
            InitializeComponent();
            this.Title = title;
            this.PromptTextBlock.Text = prompt;

            if (comboBoxItems != null)
            {
                this.InputComboBox.ItemsSource = comboBoxItems;
                this.InputComboBox.Visibility = Visibility.Visible;
                this.InputTextBox.Visibility = Visibility.Collapsed;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.InputComboBox.Visibility == Visibility.Visible)
            {
                this.Input = this.InputComboBox.SelectedItem as string;
            }
            else
            {
                this.Input = this.InputTextBox.Text;
            }
            this.DialogResult = true;
            this.Close();
        }
    }
}
