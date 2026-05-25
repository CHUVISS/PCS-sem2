using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Data;
using ProductionManagement.Models;

namespace ProductionManagement.Controllers;

[Route("api/materials")]
[ApiController]
public class MaterialsApiController : ControllerBase
{
    private readonly AppDbContext _db;
    public MaterialsApiController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] bool low_stock = false)
    {
        var q = _db.Materials.AsQueryable();
        if (low_stock)
            q = q.Where(m => m.Quantity <= m.MinimalStock);
        var list = await q.Select(m => new {
            m.Id, m.Name, m.Quantity, m.UnitOfMeasure, m.MinimalStock,
            IsLowStock = m.Quantity <= m.MinimalStock
        }).ToListAsync();
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMaterialDto dto)
    {
        var m = new Material { Name = dto.name, Quantity = dto.quantity, UnitOfMeasure = dto.unit, MinimalStock = dto.min_stock };
        _db.Materials.Add(m);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = m.Id }, m);
    }

    [HttpPut("{id}/stock")]
    public async Task<IActionResult> UpdateStock(int id, [FromBody] StockUpdateDto dto)
    {
        var m = await _db.Materials.FindAsync(id);
        if (m == null) return NotFound();
        m.Quantity = dto.amount;
        await _db.SaveChangesAsync();
        return Ok(new { m.Id, m.Name, m.Quantity });
    }

    public record CreateMaterialDto(string name, decimal quantity, string unit, decimal min_stock);
    public record StockUpdateDto(decimal amount);
}

[Route("api/products")]
[ApiController]
public class ProductsApiController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProductsApiController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? category = null)
    {
        var q = _db.Products.AsQueryable();
        if (!string.IsNullOrEmpty(category))
            q = q.Where(p => p.Category == category);
        var list = await q.Select(p => new { p.Id, p.Name, p.Category, p.ProductionTimePerUnit, p.MinimalStock }).ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id}/materials")]
    public async Task<IActionResult> GetMaterials(int id)
    {
        var mats = await _db.ProductMaterials
            .Where(pm => pm.ProductId == id)
            .Include(pm => pm.Material)
            .Select(pm => new { pm.Material!.Name, pm.Material.UnitOfMeasure, pm.QuantityNeeded })
            .ToListAsync();
        return Ok(mats);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var p = new Product { Name = dto.name, ProductionTimePerUnit = dto.prod_time, Category = dto.category };
        _db.Products.Add(p);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = p.Id }, p);
    }

    public record CreateProductDto(string name, int prod_time, string category);
}

[Route("api/lines")]
[ApiController]
public class LinesApiController : ControllerBase
{
    private readonly AppDbContext _db;
    public LinesApiController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] bool available = false)
    {
        var q = _db.ProductionLines.AsQueryable();
        if (available)
            q = q.Where(l => l.Status == "Active" && l.CurrentWorkOrderId == null);
        var list = await q.Select(l => new { l.Id, l.Name, l.Status, l.EfficiencyFactor, l.CurrentWorkOrderId }).ToListAsync();
        return Ok(list);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusDto dto)
    {
        var line = await _db.ProductionLines.FindAsync(id);
        if (line == null) return NotFound();
        line.Status = dto.status;
        await _db.SaveChangesAsync();
        return Ok(new { line.Id, line.Status });
    }

    [HttpGet("{id}/schedule")]
    public async Task<IActionResult> GetSchedule(int id)
    {
        var orders = await _db.WorkOrders
            .Where(w => w.ProductionLineId == id && w.Status != "Cancelled")
            .Include(w => w.Product)
            .OrderBy(w => w.StartDate)
            .Select(w => new { w.Id, ProductName = w.Product!.Name, w.Quantity, w.StartDate, w.EstimatedEndDate, w.Status, w.ProgressPercent })
            .ToListAsync();
        return Ok(orders);
    }

    public record StatusDto(string status);
}

[Route("api/orders")]
[ApiController]
public class OrdersApiController : ControllerBase
{
    private readonly AppDbContext _db;
    public OrdersApiController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? status = null, [FromQuery] string? date = null)
    {
        var q = _db.WorkOrders.Include(w => w.Product).Include(w => w.ProductionLine).AsQueryable();
        if (!string.IsNullOrEmpty(status) && status != "active")
            q = q.Where(w => w.Status == status);
        else if (status == "active")
            q = q.Where(w => w.Status == "InProgress" || w.Status == "Pending");
        if (date == "today")
            q = q.Where(w => w.StartDate.Date == DateTime.Today);
        var list = await q.Select(w => new {
            w.Id, ProductName = w.Product!.Name, LineName = w.ProductionLine != null ? w.ProductionLine.Name : "—",
            w.Quantity, w.Status, w.StartDate, w.EstimatedEndDate, w.ProgressPercent
        }).ToListAsync();
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        var product = await _db.Products.FindAsync(dto.product_id);
        if (product == null) return BadRequest("Продукт не найден");

        ProductionLine? line = null;
        if (dto.line_id.HasValue)
        {
            line = await _db.ProductionLines.FindAsync(dto.line_id.Value);
            if (line == null) return BadRequest("Линия не найдена");
        }

        float efficiency = line?.EfficiencyFactor ?? 1.0f;
        double totalMinutes = (dto.quantity * product.ProductionTimePerUnit) / efficiency;

        var order = new WorkOrder
        {
            ProductId = dto.product_id,
            ProductionLineId = dto.line_id,
            Quantity = dto.quantity,
            StartDate = DateTime.Now,
            EstimatedEndDate = DateTime.Now.AddMinutes(totalMinutes),
            Status = "Pending"
        };
        _db.WorkOrders.Add(order);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
    }

    [HttpPut("{id}/progress")]
    public async Task<IActionResult> UpdateProgress(int id, [FromBody] ProgressDto dto)
    {
        var order = await _db.WorkOrders.FindAsync(id);
        if (order == null) return NotFound();
        order.ProgressPercent = Math.Clamp(dto.percent, 0, 100);
        if (order.ProgressPercent == 100) order.Status = "Completed";
        await _db.SaveChangesAsync();
        return Ok(new { order.Id, order.ProgressPercent, order.Status });
    }

    [HttpGet("{id}/details")]
    public async Task<IActionResult> GetDetails(int id)
    {
        var order = await _db.WorkOrders
            .Include(w => w.Product).ThenInclude(p => p!.ProductMaterials).ThenInclude(pm => pm.Material)
            .Include(w => w.ProductionLine)
            .FirstOrDefaultAsync(w => w.Id == id);
        if (order == null) return NotFound();
        return Ok(order);
    }

    public record CreateOrderDto(int product_id, int quantity, int? line_id);
    public record ProgressDto(int percent);
}

[Route("api/calculate")]
[ApiController]
public class CalculateApiController : ControllerBase
{
    private readonly AppDbContext _db;
    public CalculateApiController(AppDbContext db) => _db = db;

    [HttpPost("production")]
    public async Task<IActionResult> CalcProduction([FromBody] CalcDto dto)
    {
        var product = await _db.Products.FindAsync(dto.product_id);
        if (product == null) return BadRequest("Продукт не найден");
        double totalMinutes = (double)dto.quantity * product.ProductionTimePerUnit;
        return Ok(new
        {
            product_id = dto.product_id,
            quantity = dto.quantity,
            total_minutes = totalMinutes,
            total_hours = Math.Round(totalMinutes / 60, 2),
            days = Math.Round(totalMinutes / 480, 2)
        });
    }

    public record CalcDto(int product_id, int quantity);
}
