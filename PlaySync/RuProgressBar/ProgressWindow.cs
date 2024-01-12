using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace RuFramework
{
    /// <summary>
    /// RuProgressBar simplifies the use of progressbar version 3.2
    /// </summary>
    public class RuProgressBar : System.Windows.Forms.Form, IProgressCallback
    {
        private System.Windows.Forms.ProgressBar progressBar;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public delegate void SetTextInvoker(String text);
        public delegate void IncrementInvoker(int val);
        public delegate void StepToInvoker(int val);
        public delegate void RangeInvoker(int minimum, int maximum);

        private String titleRoot = "";
        private System.Threading.ManualResetEvent initEvent = new System.Threading.ManualResetEvent(false);
        private System.Threading.ManualResetEvent abortEvent = new System.Threading.ManualResetEvent(false);
        private bool requiresClose = true;

        public RuProgressBar(string Text = null)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            this.Text = Text;

        }

        #region Implementation of IProgressCallback
        /// <summary>
        /// Call this method from the worker thread to initialize
        /// the progress meter.
        /// </summary>
        /// <param name="minimum">The minimum value in the progress range (e.g. 0)</param>
        /// <param name="maximum">The maximum value in the progress range (e.g. 100)</param>
        public void Begin(int minimum, int maximum)
        {
            initEvent.WaitOne();
            Invoke(new RangeInvoker(DoBegin), new object[] { minimum, maximum });
        }

        /// <summary>
        /// Call this method from the worker thread to initialize
        /// the progress callback, without setting the range
        /// </summary>
        public void Begin()
        {
            initEvent.WaitOne();
            Invoke(new MethodInvoker(DoBegin));
        }

        /// <summary>
        /// Call this method from the worker thread to reset the range in the progress callback
        /// </summary>
        /// <param name="minimum">The minimum value in the progress range (e.g. 0)</param>
        /// <param name="maximum">The maximum value in the progress range (e.g. 100)</param>
        /// <remarks>You must have called one of the Begin() methods prior to this call.</remarks>
        public void SetRange(int minimum, int maximum)
        {
            initEvent.WaitOne();
            Invoke(new RangeInvoker(DoSetRange), new object[] { minimum, maximum });
        }

        /// <summary>
        /// Call this method from the worker thread to update the progress text.
        /// </summary>
        /// <param name="text">The progress text to display</param>
        public void SetText(String text)
        {
            Invoke(new SetTextInvoker(DoSetText), new object[] { text });
        }

        /// <summary>
        /// Call this method from the worker thread to increase the progress counter by a specified value.
        /// </summary>
        /// <param name="val">The amount by which to increment the progress indicator</param>
        public void Increment(int val)
        {
            Invoke(new IncrementInvoker(DoIncrement), new object[] { val });
        }

        /// <summary>
        /// Call this method from the worker thread to step the progress meter to a particular value.
        /// </summary>
        /// <param name="val"></param>
        public void StepTo(int val)
        {
            Invoke(new StepToInvoker(DoStepTo), new object[] { val });
        }


        /// <summary>
        /// If this property is true, then you should abort work
        /// </summary>
        public bool IsAborting
        {
            get
            {
                return abortEvent.WaitOne(0, false);
            }
        }

        /// <summary>
        /// Call this method from the worker thread to finalize the progress meter
        /// </summary>
        public void End()
        {
            if (requiresClose)
            {
                Invoke(new MethodInvoker(DoEnd));
            }
        }
        #endregion

        #region Implementation members invoked on the owner thread
        private void DoSetText(String text)
        {
            //label.Text = text;
        }

        private void DoIncrement(int val)
        {
            progressBar.Increment(val);
            UpdateStatusText();
        }

        private void DoStepTo(int val)
        {
            progressBar.Value = val;
            UpdateStatusText();
        }

        private void DoBegin(int minimum, int maximum)
        {
            DoBegin();
            DoSetRange(minimum, maximum);
        }

        private void DoBegin()
        {
            ControlBox = true;
        }

        private void DoSetRange(int minimum, int maximum)
        {
            progressBar.Minimum = minimum;
            progressBar.Maximum = maximum;
            progressBar.Value = minimum;
            titleRoot = Text;
        }

        private void DoEnd()
        {
            Close();
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Handles the form load, and sets an event to ensure that
        /// intialization is synchronized with the appearance of the form.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);
            ControlBox = false;
            initEvent.Set();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Handler for 'Close' clicking
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            requiresClose = false;
            AbortWork();
            base.OnClosing(e);
        }
        #endregion

        #region Implementation Utilities
        /// <summary>
        /// Utility function that formats and updates the title bar text
        /// </summary>
        private void UpdateStatusText()
        {
            try {
                Text = titleRoot + String.Format(" - {0}% complete", (progressBar.Value * 100) / (progressBar.Maximum - progressBar.Minimum));
            } catch { }
        }

        /// <summary>
        /// Utility function to terminate the thread
        /// </summary>
        private void AbortWork()
        {
            abortEvent.Set();
        }
        #endregion

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // progressBar
            // 
            this.progressBar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.progressBar.Location = new System.Drawing.Point(-3, 1);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(295, 27);
            this.progressBar.TabIndex = 1;
            // 
            // RuProgressBar
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(290, 30);
            this.ControlBox = false;
            this.Controls.Add(this.progressBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RuProgressBar";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "RuProgressBar";
            this.ResumeLayout(false);
        }
        #endregion


    }
}
