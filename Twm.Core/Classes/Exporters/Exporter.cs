using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Twm.Core.DataCalc.Optimization;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Pdf;

namespace Twm.Core.Classes.Exporters
{
    public class Exporter<T> where T : ExportMechanism
    {
        public static bool BeginExport(DataObject data, string file, Action<double> progress)
        {
            var exporter = (T)Activator.CreateInstance(typeof(T), progress);
            return exporter.Export(file, data);
        }

        public static Task<bool> BeginExportAsync(DataObject data, string file, Action<double> progress, CancellationTokenSource cts)
        {
            return cts.IsCancellationRequested ? Task.Run(() => false) : Task.Run(() => BeginExport(data, file, progress), cts.Token);
        }
    }

    public sealed class OptimizationPDFExporter : PDFExporter
    {
        public static string TestName = "tm";

        public static string Strategy = "strategy";
        public static string Symbol = "sym";
        public static string Timeframe = "timefr";
        public static string OptimizationFitness = "of";
        public static string Optimizer = "op";
        public static string QuantityPeriods = "qp";
        public static string TotalIterations = "ti";
        public static string IndividualIterations = "ii";

        public OptimizationPDFExporter(Action<double> progress) : base(progress)
        {
        }


        public override bool Export(string file, DataObject data)
        {
            base.Export(file, data);
            var document = new Document();
            // PageSetup pageSetup = document.DefaultPageSetup.Clone();
            // pageSetup.Orientation = Orientation.Landscape;

            var section = document.AddSection();
            section.PageSetup = document.DefaultPageSetup.Clone();
            section.PageSetup.PageFormat = PageFormat.A4;//стандартный размер страницы
            section.PageSetup.Orientation = Orientation.Landscape;//ориентация
            section.PageSetup.BottomMargin = 20;//нижний отступ
            section.PageSetup.TopMargin = 20;//верхний отсту
            section.PageSetup.LeftMargin = 20;
            section.PageSetup.RightMargin = 10;
            //

            // var section = pageSetup.Section;
            Progress.Invoke(10);



            var header = section.AddParagraph();
            header.Format.Font.Size = 17;

            header.AddFormattedText("Strategy In-Sample vs Out of Sample Research", TextFormat.Bold).AddLineBreak();
            section.AddParagraph().AddFormattedText(DateTime.Now.ToString("dd.MM.yyyy")).AddLineBreak();

            var text = section.AddParagraph();
            text.Format.Alignment = ParagraphAlignment.Left;
            text.AddLineBreak();
            text.AddFormattedText(
            @"This document demostrates test results for a strategy. The in sample part of the test is where the strategy has been optimized using the optimization fitness mentioned. The out of sample part is where the strategy has been simulated with the parameters found during the in sample optimization process. Each out of sample proceeds into the next out of sample with no time interruption.
You can see a list of in sample - out of sample periods, combined out of sample equity and draw down plots  with summary and individual period plots with summaries for both in sample and out of sample parts. Please note a vertical lines on all graphs provided. For the individual plots they represent a break between the in sample and out of sample parts of the test and for the combined out of sample results they represent individual out of sample parts.
Please note futures and forex trading contains substantial risk and is not for every investor. An investor could potentially lose all or more than the initial investment. Risk capital is money that can be lost without jeopardizing ones’ financial security or life style. Only risk capital should be used for trading and only those with sufficient risk capital should consider trading. Past performance is not necessarily indicative of future results.
");
            text.AddLineBreak();
            var paragraph = section.AddParagraph();
            paragraph.AddFormattedText("Strategy: ", TextFormat.Bold);
            paragraph.AddText(data.Texts[Strategy].Invoke());
            paragraph.AddLineBreak();

            paragraph.AddFormattedText("Symbol: ", TextFormat.Bold);
            paragraph.AddText(data.Texts[Symbol].Invoke());
            paragraph.AddLineBreak();

            paragraph.AddFormattedText("Timeframe: ", TextFormat.Bold);
            paragraph.AddText(data.Texts[Timeframe].Invoke());
            paragraph.AddLineBreak();
            Progress.Invoke(20);

            paragraph.AddFormattedText("Optimization Fitness: ", TextFormat.Bold);
            paragraph.AddText(data.Texts[OptimizationFitness].Invoke());
            paragraph.AddLineBreak();

            paragraph.AddFormattedText("Optimizer: ", TextFormat.Bold);
            paragraph.AddText(data.Texts[Optimizer].Invoke());
            paragraph.AddLineBreak();

            // paragraph.AddFormattedText("Quantity: ", TextFormat.Bold);
            // paragraph.AddText(data.Texts[Strategy].Invoke());
            // paragraph.AddLineBreak();

            paragraph.AddFormattedText("Total Iterations: ", TextFormat.Bold);
            paragraph.AddText(data.Texts[TotalIterations].Invoke());
            paragraph.AddLineBreak();

            paragraph.AddFormattedText("Individual iterations: ", TextFormat.Bold);
            paragraph.AddText(data.Texts[IndividualIterations].Invoke());
            paragraph.AddLineBreak();
            Progress.Invoke(50);

            var paragrah2 = section.AddParagraph();
            paragrah2.AddLineBreak();
            paragrah2.AddFormattedText("IS/OS List", TextFormat.Bold);

            DrawTable(document, data.Tables["main"], 0.75, 20);
            document.LastSection.LastParagraph.AddLineBreak();

            float sectionWidth = section.PageSetup.PageHeight - section.PageSetup.LeftMargin - section.PageSetup.RightMargin;

            var mainSummaryKey = data.Tables.Keys.FirstOrDefault(key => key.StartsWith(data.Texts[TestName].Invoke()));
            var mainSummar = data.Tables[mainSummaryKey];

            var mainImages = data.Images.Where((pair, i) =>
            {
                return pair.Key.StartsWith(data.Texts[TestName].Invoke());
            });
            data.Tables.Remove(mainSummaryKey);
            {
                section = AddSection(document);
                var imageParagraph = document.LastSection.AddParagraph();
                imageParagraph.Format.Alignment = ParagraphAlignment.Center;
                imageParagraph.AddFormattedText(data.Texts[TestName].Invoke(), TextFormat.Bold);

                var imageTable = new MigraDoc.DocumentObjectModel.Tables.Table();
                imageTable.Format.Alignment = ParagraphAlignment.Center;

                imageTable.Borders.Width = 0;
                for (int i = 0; i < mainImages.Count(); i++)
                {
                    var secondColumn = imageTable.AddColumn();
                    secondColumn.Format.Alignment = ParagraphAlignment.Center;
                    secondColumn.Width = sectionWidth / mainImages.Count();
                }

                var imageRow = imageTable.AddRow();
                imageRow.VerticalAlignment = VerticalAlignment.Center;


                for (int i = 0; i < mainImages.Count(); i++)
                {
                    var path = Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid()}.png");
                    File.WriteAllBytes(path, mainImages.ToList()[i].Value.Invoke());
                    var im = imageRow.Cells[i].AddImage(path);
                    im.LockAspectRatio = true;
                    im.Width = (sectionWidth / 2) * 0.9;
                }

                document.LastSection.Add(imageTable);
                DrawTable(document, mainSummar, 0.75, 20);
            }




            var images = data.Images.Except(mainImages).GroupBy(pair => pair.Key);
            foreach (var image in images)
            {
                section = AddSection(document);
                var imageParagraph = document.LastSection.AddParagraph();
                imageParagraph.Format.Alignment = ParagraphAlignment.Center;
                imageParagraph.AddFormattedText(image.Key, TextFormat.Bold);

                var imageTable = new MigraDoc.DocumentObjectModel.Tables.Table();
                imageTable.Format.Alignment = ParagraphAlignment.Center;

                imageTable.Borders.Width = 0;
                for (int i = 0; i < image.Count(); i++)
                {
                    var secondColumn = imageTable.AddColumn();
                    secondColumn.Format.Alignment = ParagraphAlignment.Center;
                    secondColumn.Width = sectionWidth / image.Count();
                }

                var imageRow = imageTable.AddRow();
                imageRow.VerticalAlignment = VerticalAlignment.Center;


                for (int i = 0; i < image.Count(); i++)
                {
                    var path = Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid()}.png");
                    File.WriteAllBytes(path, image.ToList()[i].Value.Invoke());
                    var im = imageRow.Cells[i].AddImage(path);
                    im.LockAspectRatio = true;
                    im.Width = (sectionWidth / 2) * 0.9;
                }

                document.LastSection.Add(imageTable);
                var isPar = document.LastSection.AddParagraph(" ");
                isPar.Format.Alignment = ParagraphAlignment.Center;
                isPar.AddLineBreak();
                isPar.AddFormattedText("In Sample Summary", TextFormat.Bold).AddLineBreak();
                DrawTable(document, data.Tables.FirstOrDefault(s => s.Key.StartsWith(image.Key) && s.Key.EndsWith("IS")).Value, 0.75, 20);

                var osData = data.Tables.FirstOrDefault(s => s.Key.StartsWith(image.Key) && s.Key.EndsWith("OS")).Value;

                if (osData.Data != null)
                {
                    var outPar = document.LastSection.AddParagraph(" ");
                    outPar.Format.Alignment = ParagraphAlignment.Center;
                    outPar.AddLineBreak();
                    outPar.AddFormattedText("Out of Sample Summary", TextFormat.Bold).AddLineBreak();
                    DrawTable(document, osData, 0.75, 20);
                }
            }

            var pdfRenderer = new PdfDocumentRenderer(true, PdfFontEmbedding.Always);
            pdfRenderer.Document = document;
            pdfRenderer.RenderDocument();
            pdfRenderer.PdfDocument.Save(file);
            Progress.Invoke(100);
            System.Windows.MessageBox.Show("Export successfully completed!");
            return true;
        }

        private void DrawTable(Document document, Table dataTable, Unit borderWidth, Unit rowHeight)
        {
            var table = new MigraDoc.DocumentObjectModel.Tables.Table();
            table.Format.Alignment = ParagraphAlignment.Justify;

            table.Borders.Width = borderWidth;
            table.Rows.Height = rowHeight;


            var section = document.LastSection;
            float sectionWidth = section.PageSetup.PageHeight - section.PageSetup.LeftMargin - section.PageSetup.RightMargin;
            float columnWidth = sectionWidth / dataTable.Width;

            for (var i = 0; i < dataTable.Width; i++)
            {
                var column = table.AddColumn();
                column.Format.Alignment = ParagraphAlignment.Center;
                column.Width = columnWidth;
            }

            var row = table.AddRow();
            row.VerticalAlignment = VerticalAlignment.Center;

            for (var i = 0; i < dataTable.Width; i++)
            {
                row.Cells[i].AddParagraph(dataTable.Headers[i]);
            }
            for (var i = 0; i < dataTable.Height; i++)
            {
                row = table.AddRow();
                row.VerticalAlignment = VerticalAlignment.Center;

                for (var j = 0; j < dataTable.Width; j++)
                {
                    row.Cells[j].AddParagraph(dataTable[i, j]);

                }
            }

            document.LastSection.Add(table);
        }

        public override Task<bool> ExportAsync(string file, DataObject data)
        {
            return Task.Run(() => Export(file, data));
        }

        public Section AddSection(Document document)
        {
            var section = document.AddSection();
            section.PageSetup = document.DefaultPageSetup.Clone();
            section.PageSetup.PageFormat = PageFormat.A4;//стандартный размер страницы
            section.PageSetup.Orientation = Orientation.Landscape;//ориентация
            section.PageSetup.BottomMargin = 20;//нижний отступ
            section.PageSetup.TopMargin = 20;//верхний отсту
            section.PageSetup.LeftMargin = 20;
            section.PageSetup.RightMargin = 10;

            return section;
        }
        public static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize = 2)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }
    }



    public class PDFExporter : ExportMechanism
    {
        public override bool Export(string file, DataObject data)
        {
            Progress.Invoke(0);
            return true;
        }

        public override Task<bool> ExportAsync(string file, DataObject data)
        {
            return Task.Run(() => Export(file, data));
        }

        public PDFExporter(Action<double> progress) : base(progress)
        {
        }
    }

    public abstract class ExportMechanism
    {
        public Action<double> Progress { get; private set; }

        public ExportMechanism(Action<double> progress)
        {
            this.Progress = progress;
        }

        public abstract bool Export(string file, DataObject data);
        public abstract Task<bool> ExportAsync(string file, DataObject data);
    }

    public readonly struct DataObject
    {
        public Dictionary<string, Func<string>> Texts { get; }
        public Dictionary<string, Table> Tables { get; }
        public IEnumerable<KeyValuePair<string, Func<byte[]>>> Images { get; }

        public DataObject(Dictionary<string, Func<string>> texts, Dictionary<string, Table> tables, IEnumerable<KeyValuePair<string, Func<byte[]>>> images)
        {
            Tables = tables;
            Images = images;
            Texts = texts;
        }
    }

    public readonly struct Table
    {
        public List<string> Headers { get; }
        public List<List<string>> Data { get; }

        public int Height => Data.Count;
        public int Width => Height == 0 ? 0 : Data[0].Count;

        public string this[int i, int y]
        {
            get
            {
                try
                {
                    return Data[i][y];
                }
                catch
                {
                    return "";
                }
            }
        }

        public Table(List<string> headers, List<List<string>> data)
        {
            Headers = headers;
            Data = data;
        }

    }
}
