using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace Othello
{
    class Board
    {
        public const int WIDTH = 40;    // chiều rộng 1 ô
        public const int BLACK = 1;
        public const int WHITE = -1;
        public const int EMPTY = 0;

        private int whitecount = 0;  // số quân trắng
        private int blackcount = 0;  // số quân đen
        private int piecescount = 0;    // số quân trên bàn cờ
        private int enablesteps;

        private int[,] board;   // ma trận bàn cờ

        private int x_pre = -1;
        private int y_pre = -1;

        public int WhiteCount
        {
            get { return whitecount; }
        }

        public int BlackCount
        {
            get { return blackcount; }
        }

        public int EnableSteps
        {
            get { return enablesteps; }
        }

        public int X_Pre
        {
            get { return x_pre; }
            set { x_pre = value; }
        }

        public int Y_Pre
        {
            get { return y_pre; }
            set { y_pre = value; }
        }

        public Board(bool resetflag)
        {
            if (!resetflag)
            {
                InitMat();
            }
            else
            {
                ResetMat();
            }
        }

        /// <summary>
        /// Copy ma trận bàn cờ
        /// </summary>
        /// <returns>Bàn cờ đã copy</returns>
        public Board Copy()
        {
            Board b = new Board(false);
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    b.board[i, j] = board[i, j];
                }
            }
            b.whitecount = this.WhiteCount;
            b.blackcount = this.BlackCount;
            return b;
        }

        /// <summary>
        /// tính quân trên bàn cờ của mỗi bên
        /// </summary>
        private void CountPieces()
        {
            whitecount = 0;
            blackcount = 0;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (board[i, j] == WHITE)
                        whitecount++;
                    else
                        if (board[i, j] == BLACK)
                            blackcount++;
                }
            }
        }
                
        public bool IsEnd
        {
            get { return (PiecesCount == 64); }
        }

        /// <summary>
        /// Đếm số quân trên bàn cờ
        /// </summary>
        public int PiecesCount
        {
            get { return piecescount; }
        }

        /// <summary>
        /// Khởi tạo bàn cờ
        /// </summary>
        public void InitMat()
        {
            whitecount = 0;
            blackcount = 0;
            piecescount = 0;
            
            board = new int[8, 8];
            
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    board[i, j] = EMPTY;
                }
            }

            // 4 quân cờ đầu tiên khi tạo 1 ván mới
            
            board[3, 3] = WHITE;
            board[4, 3] = BLACK;
            board[3, 4] = BLACK;
            board[4, 4] = WHITE;

            CountPieces();
        }

        /// <summary>
        /// Reset bàn cờ
        /// </summary>
        public void ResetMat()
        {
            whitecount = 0;
            blackcount = 0;
            piecescount = 0;

            board = new int[8, 8];

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    board[i, j] = EMPTY;
                }
            }
        }

        /// <summary>
        /// vẽ vào panel
        /// </summary>
        /// <param name="p">Panel</param>
        /// <param name="IsDrawHelp">vẽ các nước đi hợp lệ</param>
        /// <param name="step">bước</param>
        public void Draw(Panel p, bool IsDrawHelp, int step)
        {
            Graphics gr = Graphics.FromHwnd(p.Handle);
            Draw(gr, IsDrawHelp, step);
        }

        /// <summary>
        /// lấy giá trị hiện tại ở ô mat[i, j]
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public int GetPiece(int i, int j)
        {
            return board[i, j]; 
        }

        /// <summary>
        /// vẽ bàn cờ
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="IsDrawHelp"></param>
        /// <param name="step"></param>
        public void Draw(System.Drawing.Graphics graphics, bool IsDrawHelp, int step)
        {
            Pen pen = new Pen(Color.White, (float)0.5);
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int p = board[i, j];
                    Color c = Color.Green;
                    
                    graphics.FillRectangle(new SolidBrush(c), i * WIDTH, j * WIDTH, WIDTH, WIDTH);                    

                    graphics.DrawRectangle(pen, i * WIDTH, j * WIDTH, WIDTH, WIDTH);

                    switch (p)
                    {
                        case BLACK:
                            graphics.FillEllipse(new SolidBrush(Color.Black), i * WIDTH + 3, j * WIDTH + 3, WIDTH - 6, WIDTH - 6);
                            break;
                        case WHITE:
                            graphics.FillEllipse(new SolidBrush(Color.White), i * WIDTH + 3, j * WIDTH + 3, WIDTH - 6, WIDTH - 6);
                            break;
                    };
                }
            }
            if (IsDrawHelp)
                DrawEnableSteps(step, graphics);

            if (x_pre >= 0 && y_pre >= 0)
            {
                graphics.DrawEllipse(new Pen(Color.Red, 2), x_pre * WIDTH + 3, y_pre * WIDTH + 3, WIDTH - 6, WIDTH - 6);
            }
            x_pre = -1;
            y_pre = -1;
        }

        
        /// <summary>
        /// vẽ các nước đi hợp lệ
        /// </summary>
        /// <param name="step"></param>
        /// <param name="graphics"></param>
        public void DrawEnableSteps(int step, Graphics graphics)
        {
            Pen pen = new Pen(Color.White, (float)0.5);
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (Move(i, j, step, false) > 0)
                    {
                        Color c = Color.FromArgb(1, 255, 0);
                        graphics.FillRectangle(new SolidBrush(c), i * WIDTH, j * WIDTH, WIDTH, WIDTH);
                        graphics.DrawRectangle(pen, i * WIDTH, j * WIDTH, WIDTH, WIDTH);
                    }
                }
            }
        }

        /// <summary>
        /// Tính toán giá trị bàn cờ
        /// Nếu đủ bàn cờ: quân của máy nhiều hơn -> dương vô cùng ngược lại (đen nhiều hơn) là âm vô cùng
        /// Tính tổng 4 góc bàn cờ
        /// </summary>
        /// <returns></returns>
        public int GetValue()
        {
            // nếu đủ bàn cờ
            if (WhiteCount + BlackCount == 64)
                return (WhiteCount > BlackCount) ? Int32.MaxValue : -Int32.MaxValue;

            int edge_value = 0; // tổng các biên
            int corner_value = 0;   // tính tổng 4 góc
            int differentce_pieces = WhiteCount - BlackCount; // hiệu số quân
            int same_color = 0;

            corner_value = board[0, 0] + board[0, 7] + board[7, 7] + board[7, 0];

            // tính tổng các biên của bàn cờ
            for (int i = 0; i < 8; i++)
                edge_value += (board[i, 0] + board[i, 7] + board[0, i] + board[7, i]);

            int first = board[0, 0];// góc trên bên trái
            if (first != EMPTY)
            {
                for (int i = 1; i < 8 && board[0, i] == first; i++)
                    same_color += first;
                for (int i = 1; i < 8 && board[i, 0] == first; i++)
                    same_color += first;
            }

            first = board[0, 7];// góc trên bên phải
            if (first != EMPTY)
            {
                for (int i = 6; i >= 0 && board[0, i] == first; i--)
                    same_color += first;
                for (int i = 1; i < 8 && board[i, 7] == first; i++)
                    same_color += first;
            }

            first = board[7, 0];// góc dưới bên trái
            if (first != EMPTY)
            {
                for (int i = 1; i < 8 && board[7, i] == first; i++)
                    same_color += first;
                for (int i = 6; i >= 0 && board[i, 7] == first; i--)
                    same_color += first;
            }

            first = board[7, 7];// góc dưới bên phải
            if (first != EMPTY)
            {
                for (int i = 6; i >= 0 && board[i, 7] == first; i--)
                    same_color += first;
                for (int i = 6; i >= 0 && board[7, i] == first; i--)
                    same_color += first;
            }

            int boardvalue = ((100 + WhiteCount + BlackCount) * differentce_pieces)
                             + (-200 * corner_value)
                             + (-150 * edge_value)
                             + (-250 * same_color);

            return boardvalue;
        }


        /*
         *	Kiểm tra các hướng
         */
        #region SideCheck
        private int UpCheck(int x, int y, int p, bool f)
        {
            bool found = false;
            int i = 0;
            int c = 0;
            for (i = y; i >= 0; i--)
            {
                if (board[x, i] == EMPTY) return 0;

                if (board[x, i] == -p) c++;
                if (board[x, i] == p)
                {
                    found = c > 0;
                    if (c > 0 && f)
                        for (int j = y; j != i; j--)
                            board[x, j] = p;
                    break;
                }
            }
            return found ? c : 0;
        }

        private int DownCheck(int x, int y, int p, bool f)
        {
            bool found = false;
            int i = 0;
            int c = 0;
            for (i = y; i < 8; i++)
            {
                if (board[x, i] == EMPTY) return 0;

                if (board[x, i] == -p) c++;
                if (board[x, i] == p)
                {
                    found = c > 0;
                    if (c > 0 && f)
                        for (int j = y; j != i; j++)
                            board[x, j] = p;
                    break;
                }
            }
            return found ? c : 0;
        }

        private int LeftCheck(int x, int y, int p, bool f)
        {
            bool found = false;
            int i = 0;
            int c = 0;
            for (i = x; i >= 0; i--)
            {
                if (board[i, y] == EMPTY) return 0;

                if (board[i, y] == -p) c++;
                if (board[i, y] == p)
                {
                    found = c > 0;
                    if (c > 0 && f)
                        for (int j = x; j != i; j--)
                            board[j, y] = p;
                    break;
                }
            }
            return found ? c : 0;
        }

        private int RightCheck(int x, int y, int p, bool f)
        {
            bool found = false;
            int i = 0;
            int c = 0;
            for (i = x; i < 8; i++)
            {
                if (board[i, y] == EMPTY) return 0;

                if (board[i, y] == -p) c++;
                if (board[i, y] == p)
                {
                    found = c > 0;
                    if (c > 0 && f)
                        for (int j = x; j != i; j++)
                            board[j, y] = p;
                    break;
                }
            }
            return found ? c : 0;
        }
        #endregion SideCheck

        /*
         *	Kiểm tra các hướng chéo
         */
        #region Diagonal
        private int UpLeftCheck(int x, int y, int p, bool f)
        {
            bool found = false;
            int i = 0, j = 0;
            int a, b;
            int c = 0;
            for (i = x, j = y; i >= 0 && j >= 0; i--, j--)
            {
                if (board[i, j] == EMPTY) return 0;

                if (board[i, j] == -p) c++;
                if (board[i, j] == p)
                {
                    found = c > 0;
                    if (c > 0 && f)
                        for (a = x, b = y; a != i && b != j; a--, b--)
                            board[a, b] = p;
                    break;
                }
            }
            return found ? c : 0;
        }

        private int UpRightCheck(int x, int y, int p, bool f)
        {
            bool found = false;
            int i = 0, j = 0;
            int a, b;
            int c = 0;
            for (i = x, j = y; i < 8 && j >= 0; i++, j--)
            {
                if (board[i, j] == EMPTY) return 0;

                if (board[i, j] == -p) c++;
                if (board[i, j] == p)
                {
                    found = c > 0;
                    if (c > 0 && f)
                        for (a = x, b = y; a != i && b != j; a++, b--)
                            board[a, b] = p;
                    break;
                }
            }
            return found ? c : 0;
        }

        private int DownRightCheck(int x, int y, int p, bool f)
        {
            bool found = false;
            int i = 0, j = 0;
            int a, b;
            int c = 0;
            for (i = x, j = y; i < 8 && j < 8; i++, j++)
            {
                if (board[i, j] == EMPTY) return 0;

                if (board[i, j] == -p) c++;
                if (board[i, j] == p)
                {
                    found = c > 0;
                    if (c > 0 && f)
                        for (a = x, b = y; a != i && b != j; a++, b++)
                            board[a, b] = p;
                    break;
                }
            }
            return found ? c : 0;
        }

        private int DownLeftCheck(int x, int y, int p, bool f)
        {
            bool found = false;
            int i = 0, j = 0;
            int a, b;
            int c = 0;
            for (i = x, j = y; i >= 0 && j < 8; i--, j++)
            {
                if (board[i, j] == EMPTY) return 0;

                if (board[i, j] == -p) c++;
                if (board[i, j] == p)
                {
                    found = c > 0;
                    if (c > 0 && f)
                        for (a = x, b = y; a != i && b != j; a--, b++)
                            board[a, b] = p;
                    break;
                }
            }
            return found ? c : 0;
        }
        #endregion Diagonal


        /// <summary>
        /// Đặt quân cờ vào bàn, kiểm tra có hợp lệ không
        /// 
        /// </summary>
        /// <param name="x">hàng</param>
        /// <param name="y">cột</param>
        /// <param name="p"></param>
        /// <param name="IsAdd"></param>
        /// <returns>Giá trị trả về > 0 là hợp lệ, ngược lại = 0 là không hợp lệ</returns>
        internal int Move(int x, int y, int p, bool IsAdd)
        {
            int res = 0;

            if (board[x, y] == EMPTY)
            {
                res = UpCheck(x, y - 1, p, IsAdd) +
                        DownCheck(x, y + 1, p, IsAdd) +
                        LeftCheck(x - 1, y, p, IsAdd) +
                        RightCheck(x + 1, y, p, IsAdd) +
                        UpLeftCheck(x - 1, y - 1, p, IsAdd) +
                        UpRightCheck(x + 1, y - 1, p, IsAdd) +
                        DownRightCheck(x + 1, y + 1, p, IsAdd) +
                        DownLeftCheck(x - 1, y + 1, p, IsAdd);

                if (res > 0)
                {
                    if (IsAdd)
                    {
                        whitecount++;
                        board[x, y] = p;
                        CountPieces();
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Tính toán nước đi hợp lệ
        /// </summary>
        /// <param name="color">màu quân cờ</param>
        /// <returns>List hợp lệ</returns>
        public List<int[]> GetEnableSteps(int color)
        {
            List<int[]> EnableStepsList = new List<int[]>();

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int res = Move(j, i, color, false);
                    if (res > 0)
                    {
                        EnableStepsList.Add(new int[] { i, j, res });
                    }
                }
            }
            enablesteps = EnableStepsList.Count;

            return EnableStepsList;
        }

        /// <summary>
        /// Tính toán Max - Min lần lượt cho quân đen và tráng
        /// 
        /// Nếu độ sâu % 2 = 1 (đen) -> tính Max; ngược lại -> trắng
        /// </summary>
        /// <param name="cur_value"></param>
        /// <param name="bestvalue"></param>
        /// <param name="depth">Độ sâu</param>
        /// <returns></returns>
        private static int MinMaxValue(int cur_value, int bestvalue, int depth)
        {            
            if (bestvalue == -Int32.MaxValue)
                return cur_value;
            
            return (depth % 2 == 1) ? Math.Max(cur_value, bestvalue) : Math.Min(cur_value, bestvalue);
        }

        /// <summary>
        /// Cắt tỉa alpha beta
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        private static bool AlphaBetaPruning(int alpha, int beta, int depth)
        {
            return (
                    (alpha != -Int32.MaxValue && beta != Int32.MaxValue) &&
                    (
                        (alpha >= beta && depth % 2 == 0) || (alpha <= beta && depth % 2 == 1)
                    )                        
                    );
        }

        /// <summary>
        /// Tìm nước đi tốt nhất (MiniMax)
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="alpha"></param>
        /// <param name="depth">Độ sâu</param>
        /// <param name="board"></param>
        /// <param name="panel"></param>
        /// <returns></returns>
        public static int GetBestStep(int piece, int alpha, int depth, int difficulty, Board board, Panel panel)
        {

            // Giới hạn độ sâu
            if (depth > difficulty || board.WhiteCount + board.BlackCount == 64)
            {
                int k = board.GetValue();
                return k;
            }


            // List các nước đi có thể xảy ra
            List<int[]> EnableStepsList = board.GetEnableSteps(piece);

            if (EnableStepsList.Count == 0 && piece == WHITE)
                return -Int32.MaxValue;

            int bestvalue = Int32.MaxValue;

            foreach (int[] s in EnableStepsList)
            {
                if (AlphaBetaPruning(alpha, bestvalue, depth))
                {
                    return alpha;
                }
                
                Board boardcopy = board.Copy();
                boardcopy.Move(s[1], s[0], piece, true);

                // đệ quy tiếp, tăng độ sâu lên 1
                int cur_value = GetBestStep(-piece, alpha, depth + 1, difficulty, boardcopy, panel);

                bestvalue = MinMaxValue(cur_value, -bestvalue, depth);
            }

            return bestvalue;
        }
    }
}