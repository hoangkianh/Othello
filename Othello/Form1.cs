using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Othello
{
    public partial class Form1 : Form
    {
        delegate void InvokeDraw(Panel p, bool b, int s);
        delegate void CompStepDelegate();
        delegate void InvokeShowRes();
        delegate void InvokeShowGrade(string s);
        delegate void InvokeAdd(List<int[]> list, int j);
        delegate void SetStatusTextDelegate(ToolStripStatusLabel label, string text);
        delegate void ShowMessageDelegate(string message, string title, MessageBoxButtons btn, MessageBoxIcon icon);

        private CompStepDelegate EvCompStep;
        private Board board;

        private bool flag = false;// cờ đánh dấu chuyển lượt đi
        private bool started = false; // biến đánh dấu, ván cờ đã bắt đầu
        private bool IsDrawHelp = false;

        private DateTime da1;// đếm giờ
        private DateTime da2;

        public Form1()
        {
            InitializeComponent();
            EvCompStep = new CompStepDelegate(CompStep);
            ResetBoard();
            toolStripStatusLabel1.Text = "";
            toolStripStatusLabel2.Text = "";
        }

        private void InitBoard()
        {
            label5.Text = "";
            board = new Board(false);
            panel1.Width = 8 * Board.WIDTH;
            panel1.Height = 8 * Board.WIDTH;
        }

        private void ResetBoard()
        {
            label5.Text = "";
            board = new Board(true);
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            board.Draw(e.Graphics, IsDrawHelp, Board.EMPTY);
        }

        /// <summary>
        /// Tính quân mỗi bên
        /// </summary>
        private void ShowPoints()
        {
            label1.Text = board.BlackCount.ToString();
            label2.Text = board.WhiteCount.ToString();
        }

        /// <summary>
        /// Giá trị của trạng thái vừa đi
        /// </summary>
        /// <param name="s"></param>
        private void ShowGrade(string s)
        {
            label5.Text = s;
        }

        delegate void SetCheckBoxDelegate(bool check);

        private void SetCheckBox(bool check)
        {
            checkBox1.Enabled = check;
        }

        /// <summary>
        /// Máy tính đi nước đi tốt nhất
        /// </summary>
        private void DoBestStep()
        {
            List<int[]> list = board.GetEnableSteps(Board.WHITE);

            int j = 0;
            int max = -Int32.MaxValue;

            int difficulty = Convert.ToInt32(numericUpDown1.Value);


            for (int i = 0; i < list.Count; i++)
            {
                Board copyboard = board.Copy();
                copyboard.Move(list[i][1], list[i][0], Board.WHITE, true);

                int res = Board.GetBestStep(Board.BLACK, -Int32.MaxValue, 0, difficulty, copyboard, panel1);

                if (max < res)
                {
                    j = i;
                    max = res;
                }
            }

            if (list.Count > j)
            {
                board.Move(list[j][1], list[j][0], Board.WHITE, true);
            }

            Invoke(new InvokeShowGrade(ShowGrade), max.ToString());

            try
            {
                Invoke(new InvokeAdd(InsertToTable), list, j);
            }
            catch
            {
                Invoke(new ShowMessageDelegate(ShowMessage), "Máy tính mất lượt", "Mất lượt"
                                                            , MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Chèn DL vào bảng từ nước cờ do máy đánh
        /// </summary>
        /// <param name="list"></param>
        /// <param name="j"></param>
        private void InsertToTable(List<int[]> list, int j)
        {
            int m = listView1.Items.Count + 1;
            string[] stritems = { m.ToString(), "Trắng", Convert.ToChar(list[j][1] + 65) + (list[j][0] + 1).ToString() };

            ListViewItem newitem = new ListViewItem(stritems);
            listView1.Items.Add(newitem);
            listView1.EnsureVisible(m - 1);
            listView1.Items[m - 1].Selected = true;

            for (int i = 0; i < m - 1; i++)
            {
                listView1.Items[i].Selected = false;
            }

            board.X_Pre = list[j][1];
            board.Y_Pre = list[j][0];
        }

        /// <summary>
        /// Thực hiện nc đi của máy
        /// </summary>
        private void CompStep()
        {
            Invoke(new EnableTimerDelegate(EnableTimer), true);
            Invoke(new SetCheckBoxDelegate(SetCheckBox), false);
            do
            {
                DoBestStep();
            }
            while (board.GetEnableSteps(Board.BLACK).Count == 0 && board.GetEnableSteps(Board.WHITE).Count > 0);
        }

        private void SetStatusText(ToolStripStatusLabel label, string text)
        {
            label.Text = text;
            if (text.Equals(""))
            {
                toolStripProgressBar1.Style = ProgressBarStyle.Blocks; 
            }
            if (text.Equals("Tới lượt bạn"))
            {
                panelColor.BackColor = Color.Black;
            }
        }

        private void ShowMessage(string message, string title, MessageBoxButtons btn, MessageBoxIcon icon)
        {
            MessageBox.Show(this, message, title, btn, icon);
        }

        delegate void EnableTimerDelegate(bool enabled);

        private void EnableTimer(bool enabled)
        {
            timer1.Enabled = enabled;
            if (enabled)
            {
                da1 = DateTime.Now;
                timer1.Start();
                toolStripStatusLabel2.Text = "0:0:0:0";
                timer2.Stop();
            }
            else
            {
                da2 = DateTime.Now;
                timer1.Stop();
                toolStripStatusLabel2.Text = "0:0:0:0";
                timer2.Start();
            }
        }

        /// <summary>
        /// Kết thúc bước đi của máy
        /// </summary>
        /// <param name="ob"></param>
        private void EndCompStep(object ob)
        {
            if (panel1.InvokeRequired)
                Invoke(new InvokeDraw(board.Draw), panel1, IsDrawHelp, 1);
            else
                board.Draw(panel1, IsDrawHelp, Board.BLACK);

            Invoke(new SetStatusTextDelegate(SetStatusText), toolStripStatusLabel1, "Tới lượt bạn");
            Invoke(new EnableTimerDelegate(EnableTimer), false);

            int HumanSteps = board.GetEnableSteps(1).Count;    // số nước đi tiếp theo của người

            if (HumanSteps == 0)
            {
                if (board.WhiteCount > board.BlackCount)
                {
                    Invoke(new ShowMessageDelegate(ShowMessage), "Máy tính đã thắng", "Thông báo"
                                                    , MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    if (board.WhiteCount < board.BlackCount)
                        Invoke(new ShowMessageDelegate(ShowMessage), "Bạn đã thắng", "Thông báo"
                                                    , MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                    {
                        if (board.WhiteCount == board.BlackCount)
                        {
                            Invoke(new ShowMessageDelegate(ShowMessage), "Hòa cờ", "Thông báo"
                                                       , MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            Invoke(new ShowMessageDelegate(ShowMessage), "Bạn bị mất lượt", "Thông báo"
                                                       , MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                Invoke(new InvokeShowGrade(ShowGrade), "");
                Invoke(new SetStatusTextDelegate(SetStatusText), toolStripStatusLabel1, "");
                Invoke(new SetStatusTextDelegate(SetStatusText), toolStripStatusLabel2, "");
                timer1.Stop();
                timer2.Stop();
                started = false;
            }
            Invoke(new SetCheckBoxDelegate(SetCheckBox), true);
            
            flag = false;

            if (InvokeRequired)
                Invoke(new InvokeShowRes(ShowPoints));
            else
                ShowPoints();
        }

        /// <summary>
        /// Thực hiện nước đi của người
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            if (started)
            {
                Invoke(new EnableTimerDelegate(EnableTimer), false);

                numericUpDown1.Enabled = false;

                if (flag)
                    return;

                int x = e.X / Board.WIDTH;
                int y = e.Y / Board.WIDTH;

                if (board.Move(x, y, Board.BLACK, true) > 0)
                {
                    flag = true;
                    Invoke(new EnableTimerDelegate(EnableTimer), true);

                    board.X_Pre = x;
                    board.Y_Pre = y;

                    board.Draw(panel1, IsDrawHelp, Board.WHITE);
                    EvCompStep.BeginInvoke(new AsyncCallback(EndCompStep), null);
                    ShowPoints();

                    toolStripStatusLabel1.Text = "Tới lượt máy";
                    panelColor.BackColor = Color.White;
                    toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
                    toolStripProgressBar1.MarqueeAnimationSpeed = (int)numericUpDown1.Value * 10;
                }
                else
                {
                    if (IsDrawHelp)
                    {
                        Invoke(new ShowMessageDelegate(ShowMessage), "Đi sai nước.\nBạn chỉ có thể đi vào ô được tô màu sáng", "Đi sai nước"
                                            , MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        Invoke(new ShowMessageDelegate(ShowMessage), "Đi sai nước.\nBạn có thể chọn chế độ xem nước đi hợp lệ", "Đi sai nước"
                                            , MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    return;
                }

                int m = listView1.Items.Count + 1;
                string[] stritems = { m.ToString(), "Đen", Convert.ToChar(x + 65) + (y + 1).ToString() };

                ListViewItem newitem = new ListViewItem(stritems);
                listView1.Items.Add(newitem);
                listView1.EnsureVisible(m - 1);// scroll list view xuống dưới            
                listView1.Items[m - 1].Selected = true;
                for (int i = 0; i < m - 1; i++)
                {
                    listView1.Items[i].Selected = false;
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                IsDrawHelp = true;
            }
            else
            {
                IsDrawHelp = false;
            }
            board.Draw(panel1, IsDrawHelp, Board.BLACK);
        }

        private void ResetGame()
        {
            ResetBoard();
            board.Draw(panel1, IsDrawHelp, Board.BLACK);
            listView1.Items.Clear();
            label1.Text = "0";
            label2.Text = "0";
            numericUpDown1.Enabled = true;
            button1.Enabled = true;
            toolStripStatusLabel1.Text = "";
            toolStripStatusLabel2.Text = "";
            toolStripProgressBar1.Style = ProgressBarStyle.Blocks;
            started = false;
            timer1.Enabled = false;
            timer2.Enabled = false;
        }

        // game mới
        private void button2_Click(object sender, EventArgs e)
        {
            if (started)
            {
                DialogResult res = MessageBox.Show("Bạn đang chơi dở ván cờ.\nBạn muốn hủy bỏ và chơi ván cờ mới ?", "Tạo ván cờ mới ?"
                                                    , MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (res == DialogResult.Yes)
                {
                    ResetGame();
                }
            }
            else
            {
                ResetGame();
            }
        }

        // bắt đầu ván cờ
        private void button1_Click(object sender, EventArgs e)
        {
            InitBoard();
            checkBox1.Enabled = true;
            checkBox1.Checked = true;
            board.Draw(panel1, IsDrawHelp, Board.BLACK);
            listView1.Items.Clear();
            label1.Text = "2";
            label2.Text = "2";
            numericUpDown1.Enabled = false;
            started = true;
            button1.Enabled = false;

            toolStripStatusLabel1.Text = "Tới lượt bạn";
            toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
            panelColor.BackColor = Color.Black;
        }
        /// <summary>
        /// Đếm giờ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            TimeSpan span = DateTime.Now.Subtract(da1);
            toolStripStatusLabel2.Text = span.Hours.ToString() + ":" 
                                        + span.Minutes.ToString() + ":"
                                        + span.Seconds.ToString() + ":"
                                        + span.Milliseconds.ToString();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            TimeSpan span = DateTime.Now.Subtract(da2);
            toolStripStatusLabel2.Text = span.Hours.ToString() + ":"
                                        + span.Minutes.ToString() + ":"
                                        + span.Seconds.ToString() + ":"
                                        + span.Milliseconds.ToString();
        }
    }
}