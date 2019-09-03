using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace AffineImage
{
    public static class AffineImage
    {

        /// <summary>
        /// アフィン変換行列(mat)に基づいて画像を描画する
        /// </summary>
        /// <param name="g">描画先のGraphicsオブジェクト</param>
        /// <param name="bmp">描画するBitmapオブジェクト</param>
        /// <param name="mat">アフィン変換行列</param>
        public static void DrawImage(this Graphics g, Bitmap bmp, System.Drawing.Drawing2D.Matrix mat)
        {
            if ((g == null) || (bmp == null))return;

            // 画像の画素の外側の領域
            var srcRect = new RectangleF(
                    -0.5f,
                    -0.5f,
                    bmp.Width,
                    bmp.Height
                );

            // 画像の左上、右上、左下の座標
            var points = new PointF[] {
                    new PointF(srcRect.Left, srcRect.Top),  // 左上
                    new PointF(srcRect.Right, srcRect.Top), // 右上
                    new PointF(srcRect.Left, srcRect.Bottom)// 左下
            };

            // アフィン変換で描画先の座標に変換する
            mat.TransformPoints(points);

            // 描画
            g.DrawImage(
                bmp,
                points,
                srcRect,
                GraphicsUnit.Pixel
                );
        }

        /// <summary>
        /// 画像をピクチャボックスに合わせて表示するアフィン変換行列の計算（拡張メソッド）
        /// </summary>
        /// <param name="mat">アフィン変換行列</param>
        /// <param name="pic">描画先のピクチャボックス</param>
        /// <param name="bmp">描画するBitmapオブジェクト</param>
        public static void ZoomFit(this System.Drawing.Drawing2D.Matrix mat, PictureBox pic, Bitmap bmp)
        {
            if (bmp == null) return;

            // アフィン変換行列の初期化（単位行列へ）
            mat.Reset();

            // 0.5画素分移動
            mat.Translate(0.5f, 0.5f, System.Drawing.Drawing2D.MatrixOrder.Append);

            int srcWidth = bmp.Width;
            int srcHeight = bmp.Height;
            int dstWidth = pic.Width;
            int dstHeight = pic.Height;

            float scale;

            // 縦に合わせるか？横に合わせるか？
            if (srcHeight * dstWidth > dstHeight * srcWidth)
            {
                // ピクチャボックスの縦方法に画像表示を合わせる場合
                scale = dstHeight / (float)srcHeight;
                mat.Scale(scale, scale, System.Drawing.Drawing2D.MatrixOrder.Append);
                // 中央へ平行移動
                mat.Translate((dstWidth - srcWidth * scale) / 2f, 0f, System.Drawing.Drawing2D.MatrixOrder.Append);
            }
            else
            {
                // ピクチャボックスの横方法に画像表示を合わせる場合
                scale = dstWidth / (float)srcWidth;
                mat.Scale(scale, scale, System.Drawing.Drawing2D.MatrixOrder.Append);
                // 中央へ平行移動
                mat.Translate(0f, (dstHeight - srcHeight * scale) / 2f, System.Drawing.Drawing2D.MatrixOrder.Append);
            }

        }

        /// <summary>
        /// 指定した点を中心に拡大縮小するアフィン変換行列の計算
        /// </summary>
        /// <param name="mat">アフィン変換行列</param>
        /// <param name="scale">拡大縮小の倍率</param>
        /// <param name="center">拡大縮小の中心座標</param>
        public static void ScaleAt(this System.Drawing.Drawing2D.Matrix mat, float scale, PointF center)
        {
            // 原点へ移動
            mat.Translate(-center.X, -center.Y, System.Drawing.Drawing2D.MatrixOrder.Append);

            // 拡大縮小
            mat.Scale(scale, scale, System.Drawing.Drawing2D.MatrixOrder.Append);

            // 元へ戻す
            mat.Translate(center.X, center.Y, System.Drawing.Drawing2D.MatrixOrder.Append);

        }




    }
}
