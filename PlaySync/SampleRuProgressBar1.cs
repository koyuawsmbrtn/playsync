/*
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RuFramework;  // Insert reference to RuFramework

namespace RuProgressBarTemplate
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void Button1_Click(object sender, EventArgs e)
        {
            Run_RuProgressBar();
        }
        /// summary>
        /// This function run a processing and display the progress with RuProgressBar
        /// </summary>
        public void Run_RuProgressBar()
        {
            try
            {
                // Init RuProgressBar with message text in RuProgressBar
                RuProgressBar ruProgressBar = new RuProgressBar(Text = "RuProgressBar in operation");
                // Just one example of an MyObject handed over (Dummy)
                object MyObject = null;
                // Handed down dummy object, number of passes and step(divisor)
                MyFunctionality myFunctionality = new MyFunctionality(MyObject, 1000000, 10);
                // Run application with RuProgressBar
                // In the example, a whole object containing the function is passed
                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(myFunctionality.MyFunction), ruProgressBar);
                ruProgressBar.ShowDialog();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }
    }
    public class MyFunctionality
    {
        private int max = 0;
        private int divisor = 1;
        public MyFunctionality(object MyObject = null, int Max = 0, int Divisor = 1)
        {
            // Just one example of an MyObject handed over (Dummy)
            object myObject = MyObject;
            // Initiate the RuProgressBar with the maximum possible number of runs
            max = Max;
            // Only every n change should be displayed
            divisor = Divisor;
        }
        /// <summary>
        ///  Function of progress should appear 
        /// </summary>
        /// <param name="status"></param>
        public void MyFunction(object status)
        {
            try
            {
                IProgressCallback callback = status as IProgressCallback;
                // Start the progressbar
                callback.Begin(0, max / divisor);
                // *******************************************
                // Here you enter what your function should do
                // *******************************************
                // Here is the handling for the myObject
                // ...
                // Process for example in a loop
                for (int i = 0; i < max; i++)
                {
                    if (i % divisor == 0 & i > 0)
                    {
                        // Change Progressbar
                        callback.StepTo(i / divisor);
                    }
                }
                // End process
                callback.End();
            }
            catch (System.FormatException)
            {
            }
        }
    }
}
*/