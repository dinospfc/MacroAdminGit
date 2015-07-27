using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ListBoxTest
{
    public partial class TestMouseMovement : Form
    {
        public TestMouseMovement()
        {
            InitializeComponent();
        }

        private void TestMouseMovement_MouseMove(object sender, MouseEventArgs e)
        {
            this.pictureBox1.Location = new Point(e.X, e.Y);
        }
    }
}
