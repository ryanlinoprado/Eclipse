using ClosedXML.Excel;
using Eclipse.Data;
using Eclipse.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure; // Document/Font
using System.Globalization;
using LicenseType = QuestPDF.Infrastructure.LicenseType;

namespace Eclipse.Controllers
{
    public class FeedbacksController : Controller
    {
        private readonly AppDbContext _context;

        public FeedbacksController(AppDbContext context)
        {
            _context = context;
        }


      
        [HttpGet]
        public async Task<IActionResult> ExportExcel()
        {
            var feedbacks = await _context.Feedback
                .AsNoTracking()
                .OrderBy(f => f.FeedbackId)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Feedbacks");

            // Cabeçalho
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Nota";
            worksheet.Cell(1, 3).Value = "Comentário";

            var headerRange = worksheet.Range("A1:C1");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(91, 155, 213);
            headerRange.Style.Font.FontColor = XLColor.White;

            // Linhas
            int row = 2;
            foreach (var f in feedbacks)
            {
                worksheet.Cell(row, 1).Value = f.FeedbackId;
                worksheet.Cell(row, 2).Value = f.Nota;
                worksheet.Cell(row, 3).Value = f.Comentario;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Feedbacks.xlsx");
        }


        
        [HttpGet]
        public async Task<IActionResult> ExportPdf()
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var feedbacks = await _context.Feedback
                .AsNoTracking()
                .OrderBy(f => f.FeedbackId)
                .ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().AlignCenter().Text("Relatório de Feedbacks")
                                .FontSize(18).SemiBold().FontColor(Colors.Purple.Medium);
                            col.Item().AlignCenter().Text($"Gerado em {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(10).FontColor(Colors.Grey.Darken1);
                        });
                    });

                    page.Content().PaddingVertical(15).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(5);
                        });

                        // Cabeçalho
                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Purple.Medium).Padding(5)
                                .Text("ID").FontColor(Colors.White).SemiBold();
                            header.Cell().Background(Colors.Purple.Medium).Padding(5)
                                .Text("Nota").FontColor(Colors.White).SemiBold();
                            header.Cell().Background(Colors.Purple.Medium).Padding(5)
                                .Text("Comentário").FontColor(Colors.White).SemiBold();
                        });

                        bool isEven = false;
                        foreach (var f in feedbacks)
                        {
                            var bg = isEven ? Colors.Grey.Lighten3 : Colors.White;
                            isEven = !isEven;

                            table.Cell().Background(bg).Padding(4).Text(f.FeedbackId.ToString());
                            table.Cell().Background(bg).Padding(4).Text(f.Nota.ToString());
                            table.Cell().Background(bg).Padding(4).Text(f.Comentario ?? "-");
                        }
                    });

                    page.Footer().AlignRight().Text(x =>
                    {
                        x.Span("Página ").Light();
                        x.CurrentPageNumber();
                        x.Span(" de ").Light();
                        x.TotalPages();
                    });
                });
            });

            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", "Feedbacks.pdf");
        }








        // GET: Feedbacks
        public async Task<IActionResult> Index()
        {
            return View(await _context.Feedback.ToListAsync());
        }

        // GET: Feedbacks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedback = await _context.Feedback
                .FirstOrDefaultAsync(m => m.FeedbackId == id);
            if (feedback == null)
            {
                return NotFound();
            }

            return View(feedback);
        }

        // GET: Feedbacks/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Feedbacks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FeedbackId,Nota,Comentario")] Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                _context.Add(feedback);
                await _context.SaveChangesAsync();

                TempData["MensagemSucesso"] = "Feedback enviado com sucesso!";


                return RedirectToAction(nameof(Index));


                
            }
            return View(feedback);
        }

        // GET: Feedbacks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedback = await _context.Feedback.FindAsync(id);
            if (feedback == null)
            {
                return NotFound();
            }
            return View(feedback);
        }

        // POST: Feedbacks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FeedbackId,Nota,Comentario")] Feedback feedback)
        {
            if (id != feedback.FeedbackId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(feedback);
                    await _context.SaveChangesAsync();

                    TempData["MensagemSucesso"] = "Feedback atualizado com sucesso!";

                    
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FeedbackExists(feedback.FeedbackId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(feedback);
        }

        // GET: Feedbacks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedback = await _context.Feedback
                .FirstOrDefaultAsync(m => m.FeedbackId == id);
            if (feedback == null)
            {
                return NotFound();
            }

            return View(feedback);
        }

        // POST: Feedbacks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var feedback = await _context.Feedback.FindAsync(id);
            if (feedback != null)
            {
                _context.Feedback.Remove(feedback);
            }

            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] = "Feedback atualizado com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        private bool FeedbackExists(int id)
        {
            return _context.Feedback.Any(e => e.FeedbackId == id);
        }
    }
}
