using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace APP_DeskStats.Functions
{
    class pdfDocument
    {
        public void createDocument(string docName)
        {
            FileStream fs = new FileStream(docName, FileMode.Create, FileAccess.Write, FileShare.None);
            Document doc = new Document();
            PdfWriter writer = PdfWriter.GetInstance(doc, fs);
            doc.Open();
            doc.NewPage();
            iTextSharp.text.Image image;
            image.o
            doc.Add(new Paragraph("Hello World"));






            doc.Close();
        }
    }
}
