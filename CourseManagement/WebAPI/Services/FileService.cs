
﻿using Azure.Storage;
using Azure.Storage.Blobs;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using WebAPI.DTOS.request;
using WebAPI.DTOS.response;
using WebAPI.Models;
using WebAPI.Services.Interfaces;
﻿using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Drawing;
using System.Xml.Linq;
using WebAPI.Services.Interfaces;
using static System.Net.Mime.MediaTypeNames;
using PdfSharp.Fonts;

namespace WebAPI.Services
{
    public class FileService : IFileService
    {
        private readonly string _storageAccount = "elearningcourse";
        private readonly string _key = "d7y4wTEjIl0mMyy9Tlu6Ds9qgS0twnsKblHJ+aexF6PPg/0IoCk+cpT3QSMBazoU/0C+e4krt9cr+AStz43GaA==";
        private BlobContainerClient _filesContainer;
        private readonly ECourseContext _context;

        public FileService(ECourseContext context)
        {
            var credential = new StorageSharedKeyCredential(_storageAccount, _key);
            var blobUri = $"https://{_storageAccount}.blob.core.windows.net";
            var blobServiceClient = new BlobServiceClient(new Uri(blobUri), credential);
            _filesContainer = blobServiceClient.GetBlobContainerClient("web");
            _context = context;
        }

        public async Task<List<BlobDto>> ListAsync()
        {
            List<BlobDto> files = new List<BlobDto>();
            await foreach (var file in _filesContainer.GetBlobsAsync())
            {
                string uri = _filesContainer.Uri.ToString();
                var name = file.Name;
                var fullUri = $"{uri}.{name}";
                files.Add(new BlobDto
                {
                    Uri = fullUri,
                    Name = name,
                    ContentType = file.Properties.ContentType
                });
            }
            return files;
        }

        public async Task<BlobResponseDto> UploadAsync(IFormFile blob)
        {
            BlobResponseDto response = new();
            BlobClient client = _filesContainer.GetBlobClient(blob.FileName);
            await using (Stream data = blob.OpenReadStream())
            {
                await client.UploadAsync(data, overwrite: true);
            }
            response.Status = $"File {blob.FileName} upload successfully";
            response.Error = false;
            response.Blob.Uri = client.Uri.AbsoluteUri;
            response.Blob.Name = client.Name;

            return response;
        }
        public byte[] GenerateCertificatePdf(string userName, string courseName, DateTime issueDate)
        {

            using (MemoryStream stream = new MemoryStream())
            {
                // Create a new PDF document
                PdfDocument document = new PdfDocument();
                document.Info.Title = "Certificate of Completion";

                // Add a page to the document
                PdfPage page = document.AddPage();
                page.Size = PdfSharp.PageSize.A4;

                // Create an XGraphics object for drawing
                XGraphics gfx = XGraphics.FromPdfPage(page);

                // Set up fonts
                XFont titleFont = new XFont("Verdana", 36, XFontStyleEx.Bold);
                XFont nameFont = new XFont("Verdana", 28, XFontStyleEx.Bold);
                XFont courseFont = new XFont("Verdana", 20, XFontStyleEx.BoldItalic);
                XFont contentFont = new XFont("Verdana", 16, XFontStyleEx.Regular);
                XFont footerFont = new XFont("Verdana", 12, XFontStyleEx.Italic);

                // Background color with gradient for a premium look
                XLinearGradientBrush backgroundBrush = new XLinearGradientBrush(
                    new XRect(0, 0, page.Width, page.Height),
                    XColor.FromArgb(255, 240, 248, 255),
                    XColor.FromArgb(255, 176, 224, 230),
                    XLinearGradientMode.Vertical);
                gfx.DrawRectangle(backgroundBrush, 0, 0, page.Width, page.Height);

                // Add a decorative border
                XPen borderPen = new XPen(XColors.Gold, 4);
                gfx.DrawRectangle(borderPen, 20, 20, page.Width - 40, page.Height - 40);

                // Draw a thin inner border for additional decoration
                XPen innerBorderPen = new XPen(XColors.Gray, 2);
                gfx.DrawRectangle(innerBorderPen, 30, 30, page.Width - 60, page.Height - 60);

                // Draw the title text with shadow for depth
                gfx.DrawString("CERTIFICATE OF COMPLETION", titleFont, XBrushes.DarkBlue,
                    new XRect(0, 150, page.Width, 40), XStringFormats.Center);

                // Draw recipient's name with a prominent font
                gfx.DrawString(userName, nameFont, XBrushes.Black,
                    new XRect(0, 220, page.Width, 40), XStringFormats.Center);

                // Add course completion text with line breaks for spacing
                gfx.DrawString("has successfully completed the online course:", contentFont, XBrushes.Black,
                    new XRect(0, 270, page.Width, 20), XStringFormats.Center);

                // Draw course name in a stylish font with bold and italics
                gfx.DrawString(courseName, courseFont, XBrushes.DarkBlue,
                    new XRect(0, 310, page.Width, 30), XStringFormats.Center);

                // Add decorative text for commitment statement
                gfx.DrawString("This achievement demonstrates dedication and commitment to personal growth.",
                    contentFont, XBrushes.DarkGray, new XRect(0, 360, page.Width, 20), XStringFormats.Center);

                // Issue date and signature line
                gfx.DrawString($"Date Issued: {issueDate:dd MMMM yyyy}", contentFont, XBrushes.Black,
                    new XRect(40, page.Height - 120, page.Width - 80, 20), XStringFormats.TopLeft);

                // Add an authorized signature
                gfx.DrawString("Signature: Phạm Thanh Bình", footerFont, XBrushes.Black,
                    new XRect(40, page.Height - 80, page.Width - 80, 20), XStringFormats.TopLeft);

                // Draw an organization footer with a small logo (optional)
                gfx.DrawString("Authorized by: Your Organization", footerFont, XBrushes.Black,
                    new XRect(40, page.Height - 50, page.Width - 80, 20), XStringFormats.TopLeft);

                // Add a watermark in the center of the certificate
                XFont watermarkFont = new XFont("Verdana", 72, XFontStyleEx.BoldItalic);
                gfx.DrawString("Achieved", watermarkFont, new XSolidBrush(XColor.FromArgb(50, 192, 192, 192)),
                    new XRect(0, page.Height / 2 - 50, page.Width, 100), XStringFormats.Center);

                // Save the document to the stream
                document.Save(stream, false);
                return stream.ToArray();
            }


        }

        public async Task<Certificate> GenerateAndUploadCertificateAsync(CertificateRequest cer)
        {
            // Step 1: Generate the Certificate PDF
            string certificateName = $"Certificate_{cer.EnrollmentID}_{DateTime.UtcNow.Ticks}.pdf";
            byte[] pdfBytes = GenerateCertificatePdf(cer.UserName, cer.CourseName, DateTime.UtcNow);
            var enrollment = _context.Enrollments.FirstOrDefault(x => x.Id == cer.EnrollmentID);

            enrollment.Status = 3;
            await _context.SaveChangesAsync();
            // Step 2: Upload the PDF to Azure Blob Storage
            BlobClient client = _filesContainer.GetBlobClient(certificateName);
            await using (MemoryStream ms = new MemoryStream(pdfBytes))
            {
                await client.UploadAsync(ms, overwrite: true);
            }

            // Step 3: Save the Certificate record to the database
            Certificate certificate = new Certificate
            {
                EnrollmentId = cer.EnrollmentID,
                IssueDate = DateTime.UtcNow,
                CertificateNumber = Guid.NewGuid().ToString(), // Unique number for the certificate
                CertificateUrl = client.Uri.AbsoluteUri
            };

            await _context.Certificates.AddAsync(certificate);
            await _context.SaveChangesAsync();

            return certificate;
        }

    }
}