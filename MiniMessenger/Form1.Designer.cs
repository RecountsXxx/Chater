namespace MiniMessenger
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.startBtn = new System.Windows.Forms.Button();
            this.ipAdressTextBox = new System.Windows.Forms.TextBox();
            this.portNumeric = new System.Windows.Forms.NumericUpDown();
            this.consoleListBox = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.closeBtn = new System.Windows.Forms.Button();
            this.pathDBbutton = new System.Windows.Forms.Button();
            this.createDBBtn = new System.Windows.Forms.Button();
            this.nameDbTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.portNumeric)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // startBtn
            // 
            this.startBtn.Location = new System.Drawing.Point(12, 12);
            this.startBtn.Name = "startBtn";
            this.startBtn.Size = new System.Drawing.Size(94, 29);
            this.startBtn.TabIndex = 0;
            this.startBtn.Text = "Start server";
            this.startBtn.UseVisualStyleBackColor = true;
            this.startBtn.Click += new System.EventHandler(this.startBtn_Click);
            // 
            // ipAdressTextBox
            // 
            this.ipAdressTextBox.Location = new System.Drawing.Point(123, 32);
            this.ipAdressTextBox.Name = "ipAdressTextBox";
            this.ipAdressTextBox.Size = new System.Drawing.Size(125, 27);
            this.ipAdressTextBox.TabIndex = 1;
            this.ipAdressTextBox.Text = "127.0.0.1";
            // 
            // portNumeric
            // 
            this.portNumeric.Location = new System.Drawing.Point(268, 32);
            this.portNumeric.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.portNumeric.Name = "portNumeric";
            this.portNumeric.Size = new System.Drawing.Size(113, 27);
            this.portNumeric.TabIndex = 2;
            this.portNumeric.Value = new decimal(new int[] {
            8000,
            0,
            0,
            0});
            // 
            // consoleListBox
            // 
            this.consoleListBox.FormattingEnabled = true;
            this.consoleListBox.ItemHeight = 20;
            this.consoleListBox.Location = new System.Drawing.Point(12, 82);
            this.consoleListBox.Name = "consoleListBox";
            this.consoleListBox.Size = new System.Drawing.Size(773, 364);
            this.consoleListBox.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(123, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 20);
            this.label1.TabIndex = 4;
            this.label1.Text = "IPAdress";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(268, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 20);
            this.label2.TabIndex = 5;
            this.label2.Text = "Port";
            // 
            // closeBtn
            // 
            this.closeBtn.Enabled = false;
            this.closeBtn.Location = new System.Drawing.Point(12, 47);
            this.closeBtn.Name = "closeBtn";
            this.closeBtn.Size = new System.Drawing.Size(94, 29);
            this.closeBtn.TabIndex = 6;
            this.closeBtn.Text = "Close server";
            this.closeBtn.UseVisualStyleBackColor = true;
            this.closeBtn.Click += new System.EventHandler(this.closeBtn_Click);
            // 
            // pathDBbutton
            // 
            this.pathDBbutton.Location = new System.Drawing.Point(6, 36);
            this.pathDBbutton.Name = "pathDBbutton";
            this.pathDBbutton.Size = new System.Drawing.Size(94, 29);
            this.pathDBbutton.TabIndex = 7;
            this.pathDBbutton.Text = "Path DB";
            this.pathDBbutton.UseVisualStyleBackColor = true;
            this.pathDBbutton.Click += new System.EventHandler(this.pathDBbutton_Click);
            // 
            // createDBBtn
            // 
            this.createDBBtn.Location = new System.Drawing.Point(106, 35);
            this.createDBBtn.Name = "createDBBtn";
            this.createDBBtn.Size = new System.Drawing.Size(94, 29);
            this.createDBBtn.TabIndex = 8;
            this.createDBBtn.Text = "Create DB";
            this.createDBBtn.UseVisualStyleBackColor = true;
            this.createDBBtn.Click += new System.EventHandler(this.createDBBtn_Click);
            // 
            // nameDbTextBox
            // 
            this.nameDbTextBox.Location = new System.Drawing.Point(206, 37);
            this.nameDbTextBox.Name = "nameDbTextBox";
            this.nameDbTextBox.Size = new System.Drawing.Size(125, 27);
            this.nameDbTextBox.TabIndex = 9;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(206, 14);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 20);
            this.label3.TabIndex = 10;
            this.label3.Text = "Name DB";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.pathDBbutton);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.createDBBtn);
            this.groupBox1.Controls.Add(this.nameDbTextBox);
            this.groupBox1.Location = new System.Drawing.Point(432, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(343, 70);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Creating DB";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(797, 450);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.closeBtn);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.consoleListBox);
            this.Controls.Add(this.portNumeric);
            this.Controls.Add(this.ipAdressTextBox);
            this.Controls.Add(this.startBtn);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.portNumeric)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button startBtn;
        private TextBox ipAdressTextBox;
        private NumericUpDown portNumeric;
        private ListBox consoleListBox;
        private Label label1;
        private Label label2;
        private Button closeBtn;
        private Button pathDBbutton;
        private Button createDBBtn;
        private TextBox nameDbTextBox;
        private Label label3;
        private GroupBox groupBox1;
    }
}