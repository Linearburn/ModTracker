using System;
using System.IO;
using System.Windows.Forms;

namespace ModTracker
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            Load += Form2_Load;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "readme.md");
                if (File.Exists(filePath))
                {
                    richTextBox1.Text = File.ReadAllText(filePath);
                }
                else
                {
                    MessageBox.Show("The file readme.md could not be found.", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while reading the file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            // Handle any text changed event if necessary
        }
    }
}