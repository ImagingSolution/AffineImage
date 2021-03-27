using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AffineImage
{
    public partial class MainForm : Form
    {
        private Bitmap _image;
        private Graphics _gPic;
        private System.Drawing.Drawing2D.Matrix _matAffine;

        private bool _mouseDownFlg = false;
        private PointF _oldPoint;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Graphicsオブジェクトの確保
            CreateGraphics(picImage, ref _gPic);

            _matAffine = new System.Drawing.Drawing2D.Matrix();

            // ホイールイベントの追加
            this.picImage.MouseWheel
                += new System.Windows.Forms.MouseEventHandler(this.picImage_MouseWheel);

            // 画像ファイルのDropを有効にする
            picImage.AllowDrop = true;

        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            // Graphicsオブジェクトの再確保
            CreateGraphics(picImage, ref _gPic);

            RedrawImage();
        }

        private void mnuFileOpen_Click(object sender, EventArgs e)
        {
            //ファイルを開くダイアログボックスの作成  
            var ofd = new OpenFileDialog();
            //ファイルフィルタ  
            ofd.Filter = "Image File(*.bmp,*.jpg,*.png,*.tif)|*.bmp;*.jpg;*.png;*.tif|Bitmap(*.bmp)|*.bmp|Jpeg(*.jpg)|*.jpg|PNG(*.png)|*.png";
            //ダイアログの表示 （Cancelボタンがクリックされた場合は何もしない）
            if (ofd.ShowDialog() == DialogResult.Cancel) return;

            // 画像ファイルを開く
            OpenImageFile(ofd.FileName);

        }

        private void mnuFileExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void picImage_MouseDown(object sender, MouseEventArgs e)
        {
            picImage.Focus();
            _oldPoint.X = e.X;
            _oldPoint.Y = e.Y;

            _mouseDownFlg = true;

        }

        /// <summary>
        /// 画像の移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
private void picImage_MouseMove(object sender, MouseEventArgs e)
{
    // マウスをクリックしながら移動中のとき、画像の移動
    if (_mouseDownFlg == true)
    {
        // 画像の移動
        _matAffine.Translate(e.X - _oldPoint.X, e.Y - _oldPoint.Y,
            System.Drawing.Drawing2D.MatrixOrder.Append);
        // 画像の描画
        RedrawImage();

        // ポインタ位置の保持
        _oldPoint.X = e.X;
        _oldPoint.Y = e.Y;
    }

    ////////////////////////////
    // マウスポインタの座標と画像の座標を表示する

    // マウスポインタの座標
    lblMousePointer.Text = $"Mouse {e.Location}";

    // アフィン変換行列（画像座標→ピクチャボックス座標）の逆行列(ピクチャボックス座標→画像座標)を求める
    // Invertで元の行列が上書きされるため、Cloneを取ってから逆行列
    var invert = _matAffine.Clone();
    invert.Invert();

    var pf = new PointF[] { e.Location };

    // ピクチャボックス座標を画像座標に変換する
    invert.TransformPoints(pf);

    // 画像の座標
    lblImagePointer.Text = $"Image {pf[0]}";

}

        private void picImage_MouseUp(object sender, MouseEventArgs e)
        {
            _mouseDownFlg = false;

        }

        /// <summary>
        /// マウスホイールイベント(画像の拡大縮小)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void picImage_MouseWheel(object sender, MouseEventArgs e)
        {
            bool shiftKeyFlg = false;
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                shiftKeyFlg = true;
            }


            if (e.Delta > 0)
            {
                if (shiftKeyFlg)
                {
                    // ポインタの位置周りに回転
                    _matAffine.RotateAt(5f, e.Location, System.Drawing.Drawing2D.MatrixOrder.Append);
                }
                else
                {
                    // 拡大
                    // ポインタの位置周りに拡大
                    _matAffine.ScaleAt(1.5f, e.Location);
                }
            }
            else
            {
                if (shiftKeyFlg)
                {
                    // ポインタの位置周りに回転
                    _matAffine.RotateAt(-5f, e.Location, System.Drawing.Drawing2D.MatrixOrder.Append);
                }
                else
                {
                    // 縮小
                    // ポインタの位置周りに縮小
                    _matAffine.ScaleAt(1.0f / 1.5f, e.Location);
                }
            }
            // 画像の描画
            RedrawImage();
        }

        /// <summary>
        /// ダブルクリック時、画像をピクチャボックスに合わせて表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void picImage_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // 画像をピクチャボックスに合わせる
            _matAffine.ZoomFit(picImage, _image);

            // 画像の描画
            RedrawImage();
        }


        private void picImage_DragEnter(object sender, DragEventArgs e)
        {
            //コントロール内にドラッグされたとき実行される
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                //ドラッグされたデータ形式を調べ、ファイルのときはコピーとする
                e.Effect = DragDropEffects.Copy;
            else
                //ファイル以外は受け付けない
                e.Effect = DragDropEffects.None;
        }

        private void picImage_DragDrop(object sender, DragEventArgs e)
        {
            //コントロール内にドロップされたとき実行される
            //ドロップされたすべてのファイル名を取得する
            var fileName =
                (string[])e.Data.GetData(DataFormats.FileDrop, false);

            // ドロップされたファイルから画像ファイルを残す
            var imageFiles = GetImageFiles(fileName);

            if (imageFiles.Length == 0) return;

            // 配列の最初の画像ファイルを開く
            OpenImageFile(imageFiles[0]);
        }


        /// <summary>
        /// Graphicsオブジェクトの作成
        /// </summary>
        /// <param name="pic">描画先のPictureBox</param>
        /// <param name="g">Graphicsオブジェクト</param>
        private static void CreateGraphics(PictureBox pic, ref Graphics g)
        {
            if (g != null)
            {
                g.Dispose();
                g = null;
            }
            if (pic.Image != null)
            {
                pic.Image.Dispose();
                pic.Image = null;
            }

            if ((pic.Width == 0) || (pic.Height == 0))
            {
                return;
            }

            pic.Image = new Bitmap(pic.Width, pic.Height);

            g = Graphics.FromImage(pic.Image);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

        }

        /// <summary>
        /// 画像ファイルを開く
        /// </summary>
        /// <param name="filename">画像ファイル名</param>
        private void OpenImageFile(string filename)
        {
            if (_image != null) _image.Dispose();

            _image = new Bitmap(filename);

            // 画像をピクチャボックスに合わせて表示するアフィン変換行列の計算
            _matAffine.ZoomFit(picImage, _image);

            // 画像の描画
            RedrawImage();
        }

        /// <summary>
        /// 画像の描画
        /// </summary>
        private void RedrawImage()
        {
            // ピクチャボックスの背景で画像を削除
            _gPic.Clear(picImage.BackColor);
            // アフィン変換行列に基づいて画像の描画
            _gPic.DrawImage(_image, _matAffine);
            // 更新
            picImage.Refresh();
        }


        /// <summary>
        /// ファイル名の配列から画像ファイルのみを残して配列で返す
        /// </summary>
        /// <param name="filenames">ファイル名の配列</param>
        /// <returns>画像ファイルのみを残して配列</returns>
        private string[] GetImageFiles(string[] filenames)
        {
            List<string> imageFiles = new List<string>();

            string[] extensions = new string[] {
                ".bmp",
                ".gif",
                ".png",
                ".jpg",
                ".jpeg"
            };

            foreach (var name in filenames)
            {
                if (Array.IndexOf(extensions, System.IO.Path.GetExtension(name).ToLower()) != -1)
                {
                    imageFiles.Add(name);
                }
            }

            return imageFiles.ToArray();
        }


    }
}
