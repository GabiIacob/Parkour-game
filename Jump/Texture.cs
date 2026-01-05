using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

// Evităm conflictul de nume între PixelFormat din OpenTK și System.Drawing
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace Jump
{
    public class Texture
    {
        // ID-ul texturii în OpenGL
        public int Handle;

        // Încarcă o textură din fișier
        public static Texture LoadFromFile(string path)
        {
            // Verificăm dacă fișierul există
            if (!File.Exists(path))
                return null;

            // 1. Creăm textura și obținem ID-ul ei
            int handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, handle);

            // 2. Încărcăm imaginea din fișier
            using (Bitmap image = new Bitmap(path))
            {
                // OpenGL are axa Y inversată față de imagini
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);

                // Blocăm pixelii imaginii în memorie
                BitmapData data = image.LockBits(
                    new Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb
                );

                // 3. Trimitem pixelii către OpenGL
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

                // Deblocăm imaginea
                image.UnlockBits(data);
            }

            // 4. Setăm filtrele texturii (cum se scalează)
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);

            // 5. Returnăm textura creată
            return new Texture
            {
                Handle = handle
            };
        }

        // Activează textura curentă în OpenGL
        public void Use()
        {
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }
    }
}
