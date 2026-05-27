using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Data;
using ProductionManagement.Models;

namespace ProductionManagement.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;
    public HomeController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalOrders = await _db.WorkOrders.CountAsync();
        ViewBag.ActiveOrders = await _db.WorkOrders.CountAsync(w => w.Status == "InProgress");
        ViewBag.PendingOrders = await _db.WorkOrders.CountAsync(w => w.Status == "Pending");
        ViewBag.ActiveLines = await _db.ProductionLines.CountAsync(l => l.Status == "Active");
        ViewBag.LowStockCount = await _db.Materials.CountAsync(m => m.Quantity <= m.MinimalStock);
        return View();
    }
}

public class MaterialsController : Controller
{
    private readonly AppDbContext _db;
    public MaterialsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var materials = await _db.Materials.ToListAsync();
        return View(materials);
    }

    public IActionResult Create() => View(new Material());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Material m)
    {
        if (!ModelState.IsValid) return View(m);
        _db.Materials.Add(m);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var m = await _db.Materials.FindAsync(id);
        if (m == null) return NotFound();
        return View(m);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Material m)
    {
        if (id != m.Id) return BadRequest();
        if (!ModelState.IsValid) return View(m);
        _db.Materials.Update(m);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Replenish(int id, decimal amount)
    {
        var m = await _db.Materials.FindAsync(id);
        if (m == null) return NotFound();
        m.Quantity += amount;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var m = await _db.Materials.FindAsync(id);
        if (m != null) { _db.Materials.Remove(m); await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }
}

public class ProductsController : Controller
{
    private readonly AppDbContext _db;
    public ProductsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? category, string? search)
    {
        var q = _db.Products.AsQueryable();
        if (!string.IsNullOrEmpty(category)) q = q.Where(p => p.Category == category);
        if (!string.IsNullOrEmpty(search)) q = q.Where(p => p.Name.Contains(search));
        ViewBag.Categories = await _db.Products.Select(p => p.Category).Distinct().ToListAsync();
        ViewBag.SelectedCategory = category;
        ViewBag.Search = search;
        return View(await q.ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Materials = await _db.Materials.ToListAsync();
        return View(new Product());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product, int[]? materialIds, decimal[]? quantities)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Materials = await _db.Materials.ToListAsync();
            return View(product);
        }
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        if (materialIds != null && quantities != null)
        {
            for (int i = 0; i < materialIds.Length; i++)
            {
                if (materialIds[i] > 0 && i < quantities.Length && quantities[i] > 0)
                {
                    _db.ProductMaterials.Add(new ProductMaterial
                    {
                        ProductId = product.Id,
                        MaterialId = materialIds[i],
                        QuantityNeeded = quantities[i]
                    });
                }
            }
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var p = await _db.Products.Include(p => p.ProductMaterials).ThenInclude(pm => pm.Material).FirstOrDefaultAsync(p => p.Id == id);
        if (p == null) return NotFound();
        ViewBag.Materials = await _db.Materials.ToListAsync();
        return View(p);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product, int[]? materialIds, decimal[]? quantities)
    {
        if (id != product.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            ViewBag.Materials = await _db.Materials.ToListAsync();
            return View(product);
        }
        _db.Products.Update(product);
        var existing = _db.ProductMaterials.Where(pm => pm.ProductId == id);
        _db.ProductMaterials.RemoveRange(existing);
        if (materialIds != null && quantities != null)
        {
            for (int i = 0; i < materialIds.Length; i++)
            {
                if (materialIds[i] > 0 && i < quantities.Length && quantities[i] > 0)
                    _db.ProductMaterials.Add(new ProductMaterial { ProductId = id, MaterialId = materialIds[i], QuantityNeeded = quantities[i] });
            }
        }
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _db.Products.FindAsync(id);
        if (p != null) { _db.Products.Remove(p); await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var p = await _db.Products.Include(p => p.ProductMaterials).ThenInclude(pm => pm.Material).FirstOrDefaultAsync(p => p.Id == id);
        if (p == null) return NotFound();
        return View(p);
    }
}

public class OrdersController : Controller
{
    private readonly AppDbContext _db;
    public OrdersController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? status)
    {
        var q = _db.WorkOrders.Include(w => w.Product).Include(w => w.ProductionLine).AsQueryable();
        if (!string.IsNullOrEmpty(status)) q = q.Where(w => w.Status == status);
        ViewBag.StatusFilter = status;
        return View(await q.OrderByDescending(w => w.Id).ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Products = await _db.Products.ToListAsync();
        ViewBag.Lines = await _db.ProductionLines.Where(l => l.Status == "Active").ToListAsync();
        return View(new WorkOrder { StartDate = DateTime.Today, EstimatedEndDate = DateTime.Today.AddDays(1) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WorkOrder order)
    {
        var product = await _db.Products
            .Include(p => p.ProductMaterials)
            .ThenInclude(pm => pm.Material)
            .FirstOrDefaultAsync(p => p.Id == order.ProductId);

        if (product == null)
        {
            ModelState.AddModelError("", "Продукт не найден");
        }
        else
        {
            // Проверяем наличие материалов
            var missingLines = new List<string>();
            foreach (var pm in product.ProductMaterials)
            {
                decimal needed = pm.QuantityNeeded * order.Quantity;
                if (pm.Material!.Quantity < needed)
                    missingLines.Add(
                        $"{pm.Material.Name}: нужно {needed} {pm.Material.UnitOfMeasure}, " +
                        $"доступно {pm.Material.Quantity}");
            }

            if (missingLines.Count > 0)
            {
                ModelState.AddModelError("", "Не хватает материалов:\n" + string.Join("\n", missingLines));
            }
            else
            {
                // Списываем материалы
                foreach (var pm in product.ProductMaterials)
                    pm.Material!.Quantity -= pm.QuantityNeeded * order.Quantity;

                float efficiency = 1.0f;
                if (order.ProductionLineId.HasValue)
                {
                    var line = await _db.ProductionLines.FindAsync(order.ProductionLineId.Value);
                    efficiency = line?.EfficiencyFactor ?? 1.0f;
                }
                double mins = (order.Quantity * product.ProductionTimePerUnit) / efficiency;
                order.EstimatedEndDate = order.StartDate.AddMinutes(mins);
                order.Status = "Pending";
                _db.WorkOrders.Add(order);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        }

        ViewBag.Products = await _db.Products.ToListAsync();
        ViewBag.Lines = await _db.ProductionLines.Where(l => l.Status == "Active").ToListAsync();
        return View(order);
    }

    [HttpPost]
    public async Task<IActionResult> Start(int id)
    {
        var order = await _db.WorkOrders.Include(w => w.ProductionLine).FirstOrDefaultAsync(w => w.Id == id);
        if (order == null) return NotFound();
        order.Status = "InProgress";
        if (order.ProductionLineId.HasValue)
        {
            var line = await _db.ProductionLines.FindAsync(order.ProductionLineId.Value);
            if (line != null) { line.CurrentWorkOrderId = id; line.Status = "Active"; }
        }
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(int id)
    {
        var order = await _db.WorkOrders.FindAsync(id);
        if (order == null) return NotFound();
        order.Status = "Cancelled";
        if (order.ProductionLineId.HasValue)
        {
            var line = await _db.ProductionLines.FindAsync(order.ProductionLineId.Value);
            if (line != null && line.CurrentWorkOrderId == id) line.CurrentWorkOrderId = null;
        }
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProgress(int id, int percent)
    {
        var order = await _db.WorkOrders.FindAsync(id);
        if (order == null) return NotFound();
        order.ProgressPercent = Math.Clamp(percent, 0, 100);
        if (order.ProgressPercent == 100) order.Status = "Completed";
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _db.WorkOrders
            .Include(w => w.Product).ThenInclude(p => p!.ProductMaterials).ThenInclude(pm => pm.Material)
            .Include(w => w.ProductionLine)
            .FirstOrDefaultAsync(w => w.Id == id);
        if (order == null) return NotFound();
        return View(order);
    }
}

public class LinesController : Controller
{
    private readonly AppDbContext _db;
    public LinesController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var lines = await _db.ProductionLines
            .Include(l => l.CurrentWorkOrder).ThenInclude(w => w!.Product)
            .Include(l => l.WorkOrders.Where(w => w.Status != "Cancelled" && w.Status != "Completed"))
            .ToListAsync();
        return View(lines);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var line = await _db.ProductionLines.FindAsync(id);
        if (line == null) return NotFound();
        line.Status = line.Status == "Active" ? "Stopped" : "Active";
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateEfficiency(int id, float efficiency)
    {
        var line = await _db.ProductionLines.FindAsync(id);
        if (line == null) return NotFound();
        line.EfficiencyFactor = Math.Clamp(efficiency, 0.5f, 2.0f);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Create()
    {
        return View(new ProductionLine());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductionLine line)
    {
        if (!ModelState.IsValid) return View(line);
        _db.ProductionLines.Add(line);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
