using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace TimHanewich.ImageMetadata
{
    public class ImageMetadata
    {
        public float? Latitude {get; set;}
        public float? Longitude {get; set;}
        public float? AltitudeMeters {get; set;}

        public static ImageMetadata Read(Stream image)
        {
            ImageMetadata ToReturn = new ImageMetadata();

            Image i = new Bitmap(image);

            //Variables we will collect
            float? lat = null;
            float? lon = null;
            byte? LatitudeReference = null;
            byte? LongitudeReference = null;

            //Get the properties
            foreach (PropertyItem pi in i.PropertyItems)
            {
                if (pi.Id == 2) //Latitude
                {
                    lat = ToReturn.GetCoordinateFromExif(pi);
                }
                else if (pi.Id == 4) //Longitude
                {
                    lon = ToReturn.GetCoordinateFromExif(pi);
                }
                else if (pi.Id == 1) //Latitude reference - either 'n' or 's' in ASCII code
                {
                    LatitudeReference = pi.Value[0];
                }
                else if (pi.Id == 3) //Longitude reference - either 'e' or 'w' in ASCII code
                {
                    LongitudeReference = pi.Value[0];
                }
            }

            //Save the data if it exists
            if (lat.HasValue && LatitudeReference.HasValue)
            {
                if (LatitudeReference.Value == 83) //83 is ASCII code to 's'. So we have to flip the latitude to a negative because it is below the equator
                {
                    lat = lat * -1;
                }
                ToReturn.Latitude = lat;
            }
            if (lon.HasValue && LongitudeReference.HasValue)
            {
                if (LongitudeReference.Value == 87)
                {
                    lon = lon * -1;
                }
                ToReturn.Longitude = lon;
            }


            return ToReturn;
        }

        private float GetCoordinateFromExif(PropertyItem pi)
        {             
            uint degreesN = BitConverter.ToUInt32(pi.Value, 0);
            uint degreesD = BitConverter.ToUInt32(pi.Value, 4);
            uint minutesN = BitConverter.ToUInt32(pi.Value, 8);
            uint minutesD = BitConverter.ToUInt32(pi.Value, 12);
            uint secondsN = BitConverter.ToUInt32(pi.Value, 16);
            uint secondsD = BitConverter.ToUInt32(pi.Value, 20);

            float degrees = Convert.ToSingle(degreesN) / Convert.ToSingle(degreesD);
            float minutes = Convert.ToSingle(minutesN) / Convert.ToSingle(minutesD);
            float seconds = Convert.ToSingle(secondsN) / Convert.ToSingle(secondsD);

            float coord = degrees + (minutes / 60f) + (seconds / 3600f);

            return coord;
        }

    }
}