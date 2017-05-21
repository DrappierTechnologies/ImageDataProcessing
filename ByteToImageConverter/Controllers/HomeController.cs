using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ByteToImageConverter.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult ConvertToJPEG()
        {
            string[] imageNames = Directory.GetFiles(@"D:\RIFAR Tire Binary\all", "*", SearchOption.AllDirectories);

            for (int i = 0; i < imageNames.Length; i++)
            {
                // Load the image.
                Image image = Image.FromFile(imageNames[i]);

                // Save the image in JPEG format.
                image.Save(@"D:\RIFAR Tire Binary\all_jpeg\" + imageNames[i].Split('\\')[imageNames[i].Split('\\').Length - 1], System.Drawing.Imaging.ImageFormat.Jpeg);

                image.Dispose();
            }

            ViewBag.Message = "Operation Completed";

            return View();
        }

        public ActionResult SQLImagesToPNG(int width, int height)
        {
            //for (int i = 1; i < 6; i++)
            //{
                // declare the SqlDataReader, which is used in
                // both the try block and the finally block
                SqlDataReader reader = null;

                // create a connection object
                SqlConnection sqlConnection = new SqlConnection("Data Source=(local);Initial Catalog=TF_RCRC_IMGS;Integrated Security=SSPI");

                //string query = "SELECT TOP 50 [ID], [Image], [ReasonID] FROM [TF_RCRC_IMGS].[dbo].[ImageUploads] WHERE [Image] IS NOT NULL AND [ReasonID] = " + i.ToString() + " ORDER BY NEWID()";
                string query = "SELECT [ID], [Image], [ReasonID] FROM [TF_RCRC_IMGS].[dbo].[ImageUploads] WHERE [Image] IS NOT NULL AND [ImageTypeID] = 1 AND [ImageWidth] > 32 AND [ImageHeight] > 32 AND [ID] > 162651 AND [ID] < 187651";

                // create a command object
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlCommand.CommandTimeout = 21600;

                try
                {
                    // open the connection
                    sqlConnection.Open();

                    FileStream fileStream;                          // Writes the BLOB to a file (*.bmp).
                    BinaryWriter binaryWriter;                        // Streams the BLOB to the FileStream object.

                    // 1. get an instance of the SqlDataReader
                    reader = sqlCommand.ExecuteReader();

                    // 2. print necessary columns of each record
                    while (reader.Read())
                    {
                        // get the results of each column
                        string name = reader["ID"].ToString();

                        // Create a file to hold the output.
                        //fileStream = new FileStream("D:\\RIFAR 9\\" + reader["ReasonID"].ToString() + "\\" + name + ".jpg", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                        fileStream = new FileStream("D:\\RIFAR Tire Binary\\" + "all" + "\\" + name + ".jpg", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                        binaryWriter = new BinaryWriter(fileStream);

                        // Reset the starting byte for the new BLOB.
                        //startIndex = 0;

                        // Read the bytes into outbyte[] and retain the number of bytes returned.
                        //NOTE: Framework 4.5 (+) we can use rdr.GetStream() to instead of returning bytes
                        var imageBytes = GetBytes(reader, 1);

                        var bitmap32Squared = ResizeImageSquared(imageBytes, width, height);

                        ImageConverter imgConverter = new ImageConverter();
                        byte[] img32 = (byte[])imgConverter.ConvertTo(bitmap32Squared, typeof(byte[]));

                        binaryWriter.Write(img32);

                        binaryWriter.Flush();

                        // Close the output file.
                        binaryWriter.Close();
                        fileStream.Close();
                    }
                }
                finally
                {
                    // 3. close the reader
                    if (reader != null)
                    {
                        reader.Close();
                    }

                    // close the connection
                    if (sqlConnection != null)
                    {
                        sqlConnection.Close();
                    }
                }
            //}

            ViewBag.Message = "Operation Completed";

            return View();
        }

        private byte[] GetBytes(SqlDataReader reader, int ordinal)
        {
            byte[] result = null;

            if (!reader.IsDBNull(ordinal))
            {
                long size = reader.GetBytes(ordinal, 0, null, 0, 0); //get the length of data 
                result = new byte[size];
                int bufferSize = 1024;
                long bytesRead = 0;
                int curPos = 0;
                while (bytesRead < size)
                {
                    bytesRead += reader.GetBytes(ordinal, curPos, result, curPos, bufferSize);
                    curPos += bufferSize;
                }
            }

            return result;
        }


        private Bitmap ResizeImageSquared(byte[] imgBytes, int width, int height)
        {
            Image image;
            using (MemoryStream memoryStream = new MemoryStream(imgBytes))
            {
                image = Image.FromStream(memoryStream);
            }
            //a holder for the result
            Bitmap result = new Bitmap(width, height);
            //use a graphics object to draw the resized image into the bitmap
            using (Graphics graphics = Graphics.FromImage(result))
            {
                //set the resize quality modes to high quality
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                //draw the image into the target bitmap
                graphics.DrawImage(image, 0, 0, result.Width, result.Height);
            }

            //return the resulting bitmap
            return result;
        }
    }
}