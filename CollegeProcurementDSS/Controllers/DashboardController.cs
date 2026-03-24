using Microsoft.AspNetCore.Mvc;
using CollegeProcurementDSS.Data;
using System.Linq;
using Xceed.Words.NET;
using Xceed.Document.NET;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using System.Xml.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CollegeProcurementDSS.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Подключаем нашу базу данных
        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Параметр filter будет меняться при нажатии на твои кнопки
        // ВАЖНО: Добавили async Task<>
        public async Task<IActionResult> Index(string filter = "all")
        {
            ViewBag.CurrentFilter = filter;

            // === БЛОК: ИНТЕГРАЦИЯ С API НАЦБАНКА РК ===
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Стучимся на сервер Нацбанка
                    string xmlData = await client.GetStringAsync("https://nationalbank.kz/rss/rates_all.xml");
                    XDocument xml = XDocument.Parse(xmlData);

                    // Ищем узел с долларом США
                    var usdNode = xml.Descendants("item").FirstOrDefault(x => x.Element("title")?.Value == "USD");
                    if (usdNode != null)
                    {
                        ViewBag.LiveUsdRate = usdNode.Element("description")?.Value;
                        ViewBag.LiveUsdDate = usdNode.Element("pubDate")?.Value;
                    }
                }
            }
            catch
            {
                ViewBag.LiveUsdRate = "Ошибка соединения";
                ViewBag.LiveUsdDate = "Нет данных";
            }
            // ==========================================

            var predictions = _context.PricePredictions.AsQueryable();

            // Логика наших функциональных кнопок
            switch (filter)
            {
                case "critical":
                    // Ищем товары, где ИИ предсказал рост более 10%
                    predictions = predictions.Where(p => p.Real_Price_KZT > 0 && ((p.AI_Predicted_Price_KZT - p.Real_Price_KZT) / p.Real_Price_KZT) > 0.10m);
                    break;
                case "optimistic":
                    // Ищем товары, где цена упадет (можно сэкономить)
                    predictions = predictions.Where(p => p.AI_Predicted_Price_KZT < p.Real_Price_KZT);
                    break;
                default:
                    // Показывать все
                    break;
            }

            // Берем 50 записей, чтобы не перегружать страницу
            var result = predictions.Take(50).ToList();
            return View(result);
        }

        public IActionResult GenerateDoc(int id)
        {
            // 1. Ищем товар в базе по ID
            var item = _context.PricePredictions.FirstOrDefault(p => p.Id == id);
            if (item == null) return NotFound();

            // 2. Создаем Word-документ в оперативной памяти
            using (MemoryStream ms = new MemoryStream())
            {
                using (DocX document = DocX.Create(ms))
                {
                    // 3. Собираем текст служебки
                    document.InsertParagraph("СЛУЖЕБНАЯ ЗАПИСКА")
                            .Bold().FontSize(16).Alignment = Alignment.center;

                    document.InsertParagraph("\nКому: Администрации").Bold();
                    document.InsertParagraph("От: Системы мониторинга закупок (СППР)").Bold();
                    document.InsertParagraph($"Дата формирования: {System.DateTime.Now.ToShortDateString()}");

                    document.InsertParagraph("\nО необходимости внепланового закупа").Bold().FontSize(14);

                    document.InsertParagraph($"\nНа основании анализа предиктивной модели (LSTM), уведомляем о прогнозируемом изменении цены на позицию: {item.Item_Name}.");
                    document.InsertParagraph($"Текущая рыночная цена: {item.Real_Price_KZT:N0} KZT.");
                    document.InsertParagraph($"Прогноз модели на следующий период: {item.AI_Predicted_Price_KZT:N0} KZT.");

                    string trend = item.PriceDiffPercent > 0 ? "Ожидаемый рост" : "Ожидаемый спад";
                    document.InsertParagraph($"{trend}: {item.PriceDiffPercent:F1}%.");

                    document.InsertParagraph("\nРЕКОМЕНДАЦИЯ:").Bold();
                    if (item.PriceDiffPercent > 10)
                    {
                        document.InsertParagraph("Срочно произвести закупку данной позиции для экономии бюджетных средств").Italic();
                    }
                    else
                    {
                        document.InsertParagraph("Закупку можно отложить, ожидается стабилизация или снижение цен.").Italic();
                    }

                    document.Save();
                }

                // 4. Отдаем файл пользователю
                string fileName = $"Приказ_{item.Item_Name.Replace(" ", "_")}.docx";
                return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
            }
        }
    }
}