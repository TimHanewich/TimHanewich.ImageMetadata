using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace TimHanewich.ImageMetadata
{
    public class ImageMetadata
    {
        public DateTime? TakenAt {get; set;}
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
                else if (pi.Id == 6) //Altitude
                {
                    ToReturn.AltitudeMeters = ToReturn.GetAltitudeFromExif(pi);
                }
                else if (pi.Id == 36867)
                {
                    string val = System.Text.Encoding.ASCII.GetString(pi.Value);
                    List<string> Splitter = new List<string>();
                    Splitter.Add(":");
                    Splitter.Add(" ");
                    string[] parts = val.Split(Splitter.ToArray(), StringSplitOptions.None);
                    int year = Convert.ToInt32(parts[0]);
                    int month = Convert.ToInt32(parts[1]);
                    int day = Convert.ToInt32(parts[2]);
                    int hour = Convert.ToInt32(parts[3]);
                    int minute = Convert.ToInt32(parts[4]);
                    int second = Convert.ToInt32(parts[5]);
                    ToReturn.TakenAt = new DateTime(year, month, day, hour, minute, second);
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

        private float GetAltitudeFromExif(PropertyItem pi)
        {
            //Byte lenght of value should be 8 (two uint's divided by each other)
            uint val1 = BitConverter.ToUInt32(pi.Value, 0);
            uint val2 = BitConverter.ToUInt32(pi.Value, 4);
            float val = Convert.ToSingle(val1) / Convert.ToSingle(val2);
            return val;
        }

    }
}