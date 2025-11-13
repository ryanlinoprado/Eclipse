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

//novos


namespace Eclipse.Controllers
{
    public class ChamadoController : Controller
    {
        private readonly AppDbContext _context;

        public ChamadoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Midias/ExportExcel
        [HttpGet]
        public async Task<IActionResult> ExportExcel()
        {
            var dados = await _context.Chamado
                .AsNoTracking()
                .OrderBy(m => m.Titulo)
                .ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Chamado");

            // Cabeçalho
            ws.Cell(1, 1).Value = "ID";
            ws.Cell(1, 2).Value = "Título";
            ws.Cell(1, 3).Value = "Descricao";
            ws.Cell(1, 4).Value = "Data Criação";
            ws.Cell(1, 5).Value = "Data Atualização"; 

            // Estilo do cabeçalho (opcional)
            var header = ws.Range(1, 1, 1, 8);
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.LightGray;

            // Linhas
            var row = 2;
            foreach (var m in dados)
            {
                ws.Cell(row, 1).Value = m.ChamadoId;
                ws.Cell(row, 2).Value = m.Titulo;
                ws.Cell(row, 3).Value = m.Descricao;
                ws.Cell(row, 4).Value = m.DataCriacaoString;
                ws.Cell(row, 5).Value = m.DataAtualizacaoString;
                row++;
            }

            ws.Columns().AdjustToContents(); // ajustar largura

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            var bytes = ms.ToArray();

            const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(bytes, contentType, "chamado.xlsx");
        }

        // GET: /Midias/ExportPdf
        [HttpGet]
        public async Task<IActionResult> ExportPdf()
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var dados = await _context.Chamado
                    .AsNoTracking()
                    .OrderBy(m => m.Titulo)
                    .ToListAsync();

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Header().Text("Relatório de Chamados").SemiBold().FontSize(16).AlignCenter();
                    page.Content().Table(table =>
                    {
                        // colunas
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(1);  // ID
                            cols.RelativeColumn(3);  // Título
                            cols.RelativeColumn(3);  // Descrição
                            cols.RelativeColumn(2);  // Data Atualização
                            cols.RelativeColumn(2);  // Data Criação
                            
                        });

                        // cabeçalho
                        table.Header(header =>
                        {
                            header.Cell().Element(CellHeader).Text("ID");
                            header.Cell().Element(CellHeader).Text("Título");
                            header.Cell().Element(CellHeader).Text("Descricao");
                            header.Cell().Element(CellHeader).Text("Data Atualização");
                            header.Cell().Element(CellHeader).Text("Data Criação");
                           
                        });

                        // linhas
                        foreach (var m in dados)
                        {
                            table.Cell().Element(CellBody).Text(m.ChamadoId.ToString());
                            table.Cell().Element(CellBody).Text(m.Titulo ?? "");
                            table.Cell().Element(CellBody).Text(m.Descricao ?? "");
                            table.Cell().Element(CellBody).Text(m.DataAtualizacaoString?? "");
                            table.Cell().Element(CellBody).Text(m.DataCriacaoString?? "");
                         
                           
                        }

                        // helpers de célula
                        IContainer CellHeader(IContainer c) => c
                            .Background(Colors.Purple.Medium)
                            .Padding(6)
                            .DefaultTextStyle(x => x.SemiBold().FontColor(Colors.White))
                            .Border(0.5f)
                            .BorderColor(Colors.Grey.Darken2);

                        IContainer CellBody(IContainer c) => c.Padding(5)
                        .BorderBottom(0.5f)
                        .BorderColor(Colors.Grey.Lighten1);

                    });

                    page.Footer().AlignRight().Text(txt =>
                    {
                        txt.Span("Gerado em ").Light();
                        txt.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).SemiBold();
                        txt.Span(" | Página ").Light();
                        txt.CurrentPageNumber();
                        txt.Span(" de ").Light();
                        txt.TotalPages();
                    });
                });
            });

            var pdfBytes = doc.GeneratePdf();
            return File(pdfBytes, "application/pdf", "chamado.pdf");
        }




        // GET: Chamado
        public async Task<IActionResult> Index()
        {
            return View(await _context.Chamado.ToListAsync());
        }

        // GET: Chamado/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chamado = await _context.Chamado
                .FirstOrDefaultAsync(m => m.ChamadoId == id);
            if (chamado == null)
            {
                return NotFound();
            }

            return View(chamado);
        }

        // GET: Chamado/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Chamado/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ChamadoId,Titulo,Descricao,Status,DataCriacaoString,DataAtualizacaoString")] Chamado chamado)
        {
            if (ModelState.IsValid)
            {
                chamado.DataCriacaoString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                chamado.DataAtualizacaoString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                _context.Add(chamado);
                await _context.SaveChangesAsync();

                TempData["MensagemSucesso"] = "Chamado criado com sucesso!";

                return RedirectToAction(nameof(Index));
            }
            return View(chamado);
        }


        // GET: Chamado/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chamado = await _context.Chamado.FindAsync(id);
            if (chamado == null)
            {
                return NotFound();
            }
            return View(chamado);
        }

        // POST: Chamado/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ChamadoId,Titulo,Descricao,Status,DataCriacaoString,DataAtualizacaoString")] Chamado chamado)
        {
            if (id != chamado.ChamadoId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    chamado.DataAtualizacaoString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    _context.Update(chamado);
                    await _context.SaveChangesAsync();

                    TempData["MensagemSucesso"] = "Chamado atualizado com sucesso!";

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChamadoExists(chamado.ChamadoId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(chamado);
        }

        // GET: Chamado/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chamado = await _context.Chamado
                .FirstOrDefaultAsync(m => m.ChamadoId == id);
            if (chamado == null)
            {
                return NotFound();
            }

            return View(chamado);
        }

        // POST: Chamado/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chamado = await _context.Chamado.FindAsync(id);
            if (chamado != null)
            {
                _context.Chamado.Remove(chamado);
            }

            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] = "Chamado excluído com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        private bool ChamadoExists(int id)
        {
            return _context.Chamado.Any(e => e.ChamadoId == id);
        }
    }




}
