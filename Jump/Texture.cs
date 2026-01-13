using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace Jump
{
    public class Texture
    {
        public int Handle;

        public static Texture LoadFromFile(string path)
        {
            if (!File.Exists(path))
                return null;

            int handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, handle);

            using (Bitmap image = new Bitmap(path))
            {
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);

                BitmapData data = image.LockBits(
                    new Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb
                );

                GL.TexImage2D(
                    TextureTarget.Texture2D,
                    level: 0,
                    internalformat: PixelInternalFormat.Rgba,
                    width: image.Width,
                    height: image.Height,
                    border: 0,
                    format: PixelFormat.Bgra,
                    type: PixelType.UnsignedByte,
                    pixels: data.Scan0
                );

                image.UnlockBits(data);
            }

            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);

            return new Texture
            {
                Handle = handle
            };
        }

        public void Use()
        {
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }
    }
}
