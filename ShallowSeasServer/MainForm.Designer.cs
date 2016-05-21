namespace ShallowSeasServer
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
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
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.commandBox = new System.Windows.Forms.TextBox();
			this.logBox = new System.Windows.Forms.RichTextBox();
			this.fishMapTableLayout = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 336F));
			this.tableLayoutPanel1.Controls.Add(this.commandBox, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.logBox, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.fishMapTableLayout, 1, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(907, 612);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// commandBox
			// 
			this.commandBox.AcceptsReturn = true;
			this.commandBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.commandBox.Font = new System.Drawing.Font("Lucida Console", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.commandBox.Location = new System.Drawing.Point(2, 589);
			this.commandBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.commandBox.Name = "commandBox";
			this.commandBox.Size = new System.Drawing.Size(567, 21);
			this.commandBox.TabIndex = 0;
			this.commandBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.commandBox_KeyDown);
			// 
			// logBox
			// 
			this.logBox.BackColor = System.Drawing.SystemColors.Window;
			this.logBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.logBox.Font = new System.Drawing.Font("Lucida Console", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.logBox.Location = new System.Drawing.Point(2, 2);
			this.logBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.logBox.Name = "logBox";
			this.logBox.ReadOnly = true;
			this.logBox.Size = new System.Drawing.Size(567, 583);
			this.logBox.TabIndex = 1;
			this.logBox.Text = "";
			this.logBox.WordWrap = false;
			this.logBox.Enter += new System.EventHandler(this.logBox_Enter);
			// 
			// fishMapTableLayout
			// 
			this.fishMapTableLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.fishMapTableLayout.ColumnCount = 1;
			this.fishMapTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.fishMapTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.fishMapTableLayout.Location = new System.Drawing.Point(574, 3);
			this.fishMapTableLayout.Name = "fishMapTableLayout";
			this.fishMapTableLayout.RowCount = 1;
			this.tableLayoutPanel1.SetRowSpan(this.fishMapTableLayout, 2);
			this.fishMapTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.fishMapTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.fishMapTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.fishMapTableLayout.Size = new System.Drawing.Size(330, 606);
			this.fishMapTableLayout.TabIndex = 2;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(907, 612);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.Name = "MainForm";
			this.Text = "Shallow Seas Server";
			this.Shown += new System.EventHandler(this.MainForm_Shown);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.TextBox commandBox;
		private System.Windows.Forms.RichTextBox logBox;
		private System.Windows.Forms.TableLayoutPanel fishMapTableLayout;
	}
}

