using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using System.Drawing;
using System.Reflection.Metadata;
using System.Text;
using System.Xml.Linq;

namespace Safepot.WebApp.Helpers
{
    public class FileConversion
    {
        public byte[] ConverttoPDF(string viewContent, string reportTitle)
        {
            byte[] bytes;
            try
            {
                var ms = new MemoryStream();
                var doc = new iTextSharp.text.Document(PageSize.A4.Rotate());
                var writer = PdfWriter.GetInstance(doc, ms);
                doc.Open();
                doc.Add(new Chunk(""));
                var example_html = viewContent;
                var srHtml = new StringReader(viewContent);
                XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, srHtml);
                doc.Close();
                bytes = ms.ToArray();
                srHtml.Dispose();
                writer.Dispose();
                doc.Dispose();
                ms.Dispose();

                iTextSharp.text.Font blackFont = FontFactory.GetFont("Arial", 7, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                iTextSharp.text.Font titleFont = FontFactory.GetFont("Arial", 18, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                Font headerFont = new Font(FontFactory.GetFont("Arial", 18, Font.BOLD));
                using (MemoryStream stream = new MemoryStream())
                {
                    PdfReader reader = new PdfReader(bytes);
                    using (PdfStamper stamper = new PdfStamper(reader, stream))
                    {
                        int pages = reader.NumberOfPages;
                        for (int i = 1; i <= pages; i++)
                        {
                            //Header Code
                            PdfPTable tblHeader = new PdfPTable(2);
                            tblHeader = new PdfPTable(2);

                            tblHeader.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                            tblHeader.TotalWidth = PageSize.A4.Height;


                            Paragraph p1 = new Paragraph("IT Manager", headerFont);
                            PdfPCell _cell = new PdfPCell();
                            _cell.AddElement(p1);

                            _cell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                            tblHeader.AddCell(_cell);
                            _cell = new PdfPCell();
                            //iTextSharp.text.Image myImage = iTextSharp.text.Image.GetInstance(DataCache.MapPath + "/NewAdmin/manager-logo.png");
                            //myImage.ScaleAbsolute(120f, 40f);
                            //myImage.Alignment = Image.ALIGN_RIGHT;
                            //_cell.AddElement(myImage);
                            //_cell.Border = Rectangle.NO_BORDER;
                            //tblHeader.AddCell(_cell);

                            // Print the Header
                            ColumnText column = new ColumnText(stamper.GetOverContent(i));
                            iTextSharp.text.Rectangle rectPage1 = new iTextSharp.text.Rectangle(-5, 60, 635, 840);//8, 60, 590, 840
                            rectPage1 = new iTextSharp.text.Rectangle(-35, 10, 880, 600);
                            column.SetSimpleColumn(rectPage1);
                            column.AddElement(tblHeader);
                            column.Go();

                            ColumnText.ShowTextAligned(stamper.GetUnderContent(i),
                            @Element.ALIGN_CENTER, new Phrase(reportTitle, titleFont), 400f, 569f, 0);

                            ColumnText.ShowTextAligned(stamper.GetOverContent(i),
                             @Element.ALIGN_LEFT, new Phrase("IT Manager", blackFont), 63f, 24f, 0);

                            ColumnText.ShowTextAligned(stamper.GetUnderContent(i),
                            @Element.ALIGN_CENTER, new Phrase("Page " + i.ToString() + " of " + pages, blackFont), 300f, 24f, 0);

                            ColumnText.ShowTextAligned(stamper.GetUnderContent(i),
                            @Element.ALIGN_RIGHT, new Phrase("" + DateTime.Now, blackFont), 549f, 24f, 0);
                        }
                    }

                    bytes = stream.ToArray();

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bytes;
        }



        public string SaveReport(string flag, string viewContent, string reportTitle,string rootpath)
        {
            string filePath = string.Empty;
            try
            {
                byte[] bytes;
                if (flag == "PDF")
                {
                    bytes = ConverttoPDF(viewContent, reportTitle);
                    Guid obj = Guid.NewGuid();
                    filePath = Path.Combine(rootpath + "/Attachments/", "Invoice_" + obj.ToString() + ".pdf");
                }
                else
                {
                    bytes = Encoding.ASCII.GetBytes(viewContent);
                    Guid obj = Guid.NewGuid();
                    filePath = Path.Combine(rootpath + "/Attachments/", "Invoice_" + obj.ToString() + ".csv");
                }
                System.IO.File.WriteAllBytes(filePath, bytes);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return filePath;
        }
    }
}
