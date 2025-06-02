namespace Auto.Services
{
    public static class Utility
    {
        // hier überprüfe ich das Format des Bildes um den richtigen base 64 String zu erzeugen
        public static string findformat(string image)
        {
            string imageFormat = null;
            if (!string.IsNullOrEmpty(image))
            {
                if (image.StartsWith("PHN2"))
                {
                    imageFormat = "data:image/svg+xml;base64,";
                }
                else if (image.StartsWith("iVBOR"))
                {
                    imageFormat = "data:image/png+xml;base64,";
                }
                else if (image.StartsWith("/9j/"))
                {
                    imageFormat = "data:image/jpeg;base64,";
                }
                else if (image.StartsWith("R0lG"))
                {
                    imageFormat = "data:image/gif;base64,";
                }
                else if (image.StartsWith("UklGR"))
                {
                    imageFormat = "data:image/webp;base64,";
                }
                else if (image.StartsWith("Qk"))
                {
                    imageFormat = "data:image/bmp;base64,";
                }
                else if (image.StartsWith("SUkq") || image.StartsWith("II2C"))
                {
                    imageFormat = "data:image/tiff;base64,";
                }
                else if (image.StartsWith("AAAAGGZ0eXB"))
                {
                    imageFormat = "data:image/heif;base64,";
                }
                else if (image.StartsWith("AAAAIGZ0eXB"))
                {
                    imageFormat = "data:image/avif;base64,";
                }


                else return null;
            }


            return imageFormat + image;
        }

    }
}
