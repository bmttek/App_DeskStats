using iTextSharp.text;
using iTextSharp.text.pdf;
using Image = iTextSharp.text.Image;


namespace APP_DeskStats.Helpers
{
    static public class ReportHelper
    {
        static public PdfPCell createImageCell(Image image, int widthBorder = 0, int hAlign = Element.ALIGN_LEFT, int vAlign = Element.ALIGN_TOP)
        {
            PdfPCell pCell = new PdfPCell();
            pCell.AddElement(image);
            pCell.HorizontalAlignment = hAlign;
            pCell.VerticalAlignment = vAlign;
            pCell.BorderWidthBottom = widthBorder;
            pCell.BorderWidthLeft = widthBorder;
            pCell.BorderWidthTop = widthBorder;
            pCell.BorderWidthRight = widthBorder;
            return pCell;
        }
        static public PdfPCell createTextCell(string data, iTextSharp.text.Font font, BaseColor baseColor, float borderTop, float borderRight, float borderBottom, float borderLeft, int rowSpan = 0, int rotation = 0, int hAlign = Element.ALIGN_LEFT, int vAlign = Element.ALIGN_TOP, int columnSpan = 0)
        {
            PdfPCell pCell = new PdfPCell(new Phrase(data, font));
            pCell.HorizontalAlignment = hAlign;
            pCell.VerticalAlignment = vAlign;
            pCell.BorderWidthBottom = borderBottom;
            pCell.BorderWidthLeft = borderLeft;
            pCell.BorderWidthTop = borderTop;
            pCell.BorderWidthRight = borderRight;
            pCell.BackgroundColor = baseColor;
            if (rowSpan > 0)
            {
                pCell.Rowspan = rowSpan;
                pCell.NoWrap = true;
            }
            if (columnSpan > 0)
            {
                pCell.Colspan = columnSpan;
            }
            if (rotation > 0) { pCell.Rotation = rotation; }
            return pCell;
        }
    }
}
