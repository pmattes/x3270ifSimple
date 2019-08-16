// <copyright file="PeerFormsTestApp.cs" company="Paul Mattes">
//     Copyright (c) Paul Mattes. All rights reserved.
// </copyright>

namespace PeerFormsTestApp
{
    using System;
    using System.Windows.Forms;

    using X3270is;

    /// <summary>
    /// The main form.
    /// </summary>
    public partial class Form1 : Form
    {
        /// <summary>
        /// The emulator.
        /// </summary>
        private NewEmulator emulator;

        /// <summary>
        /// Initializes a new instance of the <see cref="Form1"/> class.
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Button click.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void Button1_Click(object sender, EventArgs e)
        {
            // Only one at a time.
            if (this.emulator != null)
            {
                return;
            }

            // Start up a new emulator.
            this.emulator = new NewEmulator();
            this.emulator.Start();
            this.label1.Text = $"Emulator started, waiting {this.timer1.Interval}ms...";

            // Start the timer.
            this.timer1.Enabled = true;
        }

        /// <summary>
        /// Timer tick.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void Timer1_Tick(object sender, EventArgs e)
        {
            this.timer1.Stop();

            // Ask the emulator to do something.
            var y = this.emulator.Run("Query", "Cursor");
            this.label1.Text = $"Cursor is {y}";
            var em = this.emulator;
            this.emulator = null;
            em.Dispose();
        }
    }
}
