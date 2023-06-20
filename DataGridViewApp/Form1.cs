using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataGridViewApp
{
    public partial class Form1 : Form
    {
        private System.Windows.Forms.Timer timer;
        private int progressValue;
        private bool isPaused;
        private int row_count = 1;
        private Image circularProgressBarImage;
        private bool[] isRowPaused;


        public Form1()
        {
            InitializeComponent();

            InitializeProgressBar();
           
            isPaused = false;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dataGridView1.RowTemplate.Height = 40;
            dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dataGridView1.AutoGenerateColumns = false;

            DataGridViewImageColumn progressColumn = new DataGridViewImageColumn();
            progressColumn.HeaderText = "Progress";
            progressColumn.Name = "colProgress";
            
            progressColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
            progressColumn.Width = 100;
            dataGridView1.Columns.Add(progressColumn);

            dataGridView1.Columns[1].DefaultCellStyle.NullValue = null;
            dataGridView1.Columns[4].DefaultCellStyle.NullValue = "Browse";
            dataGridView1.Columns[5].DefaultCellStyle.NullValue = "Pause";
            dataGridView1.Columns[6].DefaultCellStyle.NullValue = "Continue";
            dataGridView1.Columns[7].DefaultCellStyle.NullValue = "Delete";

            dataGridView1.CellFormatting += dataGridView1_CellFormatting;
        }

        private void InitializeProgressBar()
        {
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Value = 0;
            progressValue = 0;
        }
        private void StartProgressBar()
        {
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 100;
            timer.Tick += Timer_Tick;
            timer.Start();
            if (progressBar.Value == progressBar.Maximum) {
                UpdateStatusRichTextBox(@"Process Completed");
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {

            if (progressValue < progressBar.Maximum)
            {
                progressValue++;
                progressBar.Value = progressValue;
                if (dataGridView1.CurrentRow != null && !isRowPaused[dataGridView1.CurrentRow.Index])
                {
                    dataGridView1.CurrentRow.Cells["colProgress"].Value = GetCircularProgressBarImage(progressValue);
                }
            }
            else
            {
                StopProgressBar();
            }
        }

        private Image GetCircularProgressBarImage(int progress)
        {
            int size = 32;
            int penWidth = 4;
            Color progressColor = Color.Blue;
            Color backgroundColor = Color.LightGray;
            Bitmap image = new Bitmap(size, size);
            Graphics graphics = Graphics.FromImage(image);
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (Pen pen = new Pen(backgroundColor, penWidth))
            {
                graphics.DrawEllipse(pen, penWidth / 2, penWidth / 2, size - penWidth, size - penWidth);
            }
            using (Pen pen = new Pen(progressColor, penWidth))
            {
                graphics.DrawArc(pen, penWidth / 2, penWidth / 2, size - penWidth, size - penWidth, 0, (float)(progress / (double)progressBar.Maximum) * 360);
            }
            graphics.Dispose();
            return image;
        }

        private void PauseProgressBar()
        {
            timer.Stop();
            isPaused = true;
        }
        private void ContinueProgressBar()
        {
            timer.Start();
            isPaused = false;
        }
        private void StopProgressBar()
        {
            timer.Stop();
            progressValue = 0;
            progressBar.Value = progressValue;
        }

          private void PopulateDataGridViewForFile(string[] filePaths)
          {
              dataGridView1.Rows.Clear();
              row_count = 1;

            isRowPaused = new bool[filePaths.Length];
            for (int i = 0; i < filePaths.Length; i++)
            {
                isRowPaused[i] = false;
            }
            foreach (string filePath in filePaths)
              {
                  Image fileIcon = Icon.ExtractAssociatedIcon(filePath).ToBitmap();
                  string fileName = Path.GetFileName(filePath);
                  string fileSize = new FileInfo(filePath).Length.ToString();
                  dataGridView1.Rows.Add(row_count, fileIcon, fileName, fileSize, circularProgressBarImage);
                  row_count++;
              }
        }

          private void PopulateDataGridViewForFolder(string[] filePaths)
          {
              dataGridView1.Rows.Clear();
              row_count = 1;
            isRowPaused = new bool[filePaths.Length];
            for (int i = 0; i < filePaths.Length; i++)
            {
                isRowPaused[i] = false;
            }
            foreach (string folderPath in filePaths)
              {
                  Image folderIcon = Icon.ExtractAssociatedIcon(folderPath).ToBitmap();
                  string folderName = new DirectoryInfo(folderPath).Name;
                  string folderSize = new FileInfo(folderPath).Length.ToString();
                  dataGridView1.Rows.Add(row_count, folderIcon, folderName, folderSize, circularProgressBarImage);
                  row_count++;
              }
          }

        private void UpdateStatusRichTextBox(string status)
        {
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.Invoke(new Action<string>(UpdateStatusRichTextBox), status);
            }
            else
            {
                richTextBox1.AppendText(status + Environment.NewLine);
                richTextBox1.ScrollToCaret();
            }
        }

        private void PauseRowProgress(int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < isRowPaused.Length)
            {
                isRowPaused[rowIndex] = true;
                dataGridView1.Rows[rowIndex].Cells["colPause"].Value = "Paused";
                UpdateStatusRichTextBox(@"Thread Paused...");
            }
        }
        private void ContinueRowProgress(int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < isRowPaused.Length)
            {
                isRowPaused[rowIndex] = false;
                dataGridView1.Rows[rowIndex].Cells["colPause"].Value = "Pause";
                UpdateStatusRichTextBox(@"Thread Continued...");
                if (timer == null || !timer.Enabled)
                {
                    StartProgressBar();
                    System.Threading.Thread.Sleep(1000);

                    // Update the circular progress bar image for the current row
                    dataGridView1.Rows[rowIndex].Cells["colProgress"].Value = circularProgressBarImage;
                }
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnFileBrowse_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select File ";
                openFileDialog.CheckFileExists = false;
                openFileDialog.CheckPathExists = true;
                openFileDialog.Multiselect = true;
                openFileDialog.Filter = "All Files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = openFileDialog.FileName;
                    bool isFile = File.Exists(selectedPath);
                    bool isFolder = Directory.Exists(selectedPath);
                    if (isFile)
                    {
                        string[] filePaths = openFileDialog.FileNames;
                        PopulateDataGridViewForFile(filePaths);
                    }
                }
            }
        }

          private void btnFolderBrowse_Click(object sender, EventArgs e)
          {
              dataGridView1.Rows.Clear();
              var folderBrowserDialog = new FolderBrowserDialog();
              Icon folderIcon = new Icon(@"C:\Users\OpenAI\source\repos\DataGridViewApp\folder_icon.ico");
              if (folderBrowserDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
              {
                  string folderPath = folderBrowserDialog.SelectedPath;
                  string[] filePaths = Directory.GetDirectories(folderPath);
                  DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
                  long folderSize = 0;
                  foreach (FileInfo fileInfo in directoryInfo.GetFiles("*", SearchOption.AllDirectories))
                  {
                      folderSize += fileInfo.Length;
                  }
                  dataGridView1.Rows.Add(row_count, folderIcon.ToBitmap(), folderPath, folderSize);
                  row_count++;
            }
          }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (timer == null || !timer.Enabled)
            {
                StartProgressBar();
                System.Threading.Thread.Sleep(1000);
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if (!isRowPaused[i])
                    {
                        dataGridView1.Rows[i].Cells["colProgress"].Value = circularProgressBarImage;
                    }
                }
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    dataGridView1.Rows[i].Cells["colProgress"].Value = circularProgressBarImage;
                }
                UpdateStatusRichTextBox(@"Threads Started...");
                
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (timer.Enabled)
            {
                StopProgressBar();
                progressBar.Value = 0;
                
                System.Threading.Thread.Sleep(1000);
                UpdateStatusRichTextBox(@"Threads Stoped...");
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            progressBar.Value = 0;
            richTextBox1.Clear();
            UpdateStatusRichTextBox(@"All Threads Restored...");
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

            if (e.ColumnIndex == dataGridView1.Columns["colDelete"].Index && e.RowIndex >= 0)
            {
                dataGridView1.Rows.RemoveAt(e.RowIndex);
                UpdateStatusRichTextBox(@"Threads Deleted...");
                row_count = 1;
            }

             if (e.ColumnIndex == dataGridView1.Columns["colBrowse"].Index && e.RowIndex >= 0)
              {
                  using (var openFileDialog = new OpenFileDialog())
                  {
                      openFileDialog.Title = "Select File ";
                      openFileDialog.CheckFileExists = false;
                      openFileDialog.CheckPathExists = true;
                      openFileDialog.Multiselect = false;
                      openFileDialog.Filter = "All Files (*.*)|*.*";
                      if (openFileDialog.ShowDialog() == DialogResult.OK)
                      {
                          string selectedPath = openFileDialog.FileName;
                          bool isFile = File.Exists(selectedPath);
                          bool isFolder = Directory.Exists(selectedPath);
                          if (isFile)
                          {
                              string[] filePaths = openFileDialog.FileNames;
                              PopulateDataGridViewForFile(filePaths);
                          }
                          else if (isFolder)
                          {
                              string[] folderPaths = Directory.GetDirectories(selectedPath);
                              PopulateDataGridViewForFolder(folderPaths);
                          }
                      }
                  }
              }

            if (e.ColumnIndex == dataGridView1.Columns["colPause"].Index && e.RowIndex >= 0)
            {
                PauseRowProgress(e.RowIndex);
                PauseProgressBar();
            }

            if (e.ColumnIndex == dataGridView1.Columns["colContinue"].Index && e.RowIndex >= 0)
            {
                ContinueRowProgress(e.RowIndex);
                ContinueProgressBar();
            }

        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].Name == "colProgress")
            {
                if (e.Value == null || e.Value == DBNull.Value)
                {
                    e.Value = null;
                }
            }
        }
    }
}