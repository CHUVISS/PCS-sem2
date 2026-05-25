using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionManagement.Models;

public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Название обязательно")]
    [Display(Name = "Название")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Описание")]
    public string? Description { get; set; }

    [Display(Name = "Характеристики (JSON)")]
    public string? Specifications { get; set; }

    [Display(Name = "Категория")]
    public string? Category { get; set; }

    [Display(Name = "Мин. запас")]
    public int MinimalStock { get; set; }

    [Display(Name = "Время производства (мин/шт)")]
    public int ProductionTimePerUnit { get; set; }

    public ICollection<ProductMaterial> ProductMaterials { get; set; } = new List<ProductMaterial>();
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}

public class ProductionLine
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Название обязательно")]
    [Display(Name = "Название линии")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Статус")]
    public string Status { get; set; } = "Stopped";

    [Display(Name = "Коэффициент эффективности")]
    [Range(0.5, 2.0)]
    public float EfficiencyFactor { get; set; } = 1.0f;

    [Display(Name = "Текущий заказ")]
    public int? CurrentWorkOrderId { get; set; }

    public WorkOrder? CurrentWorkOrder { get; set; }
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}

public class Material
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Название обязательно")]
    [Display(Name = "Название")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Количество")]
    public decimal Quantity { get; set; }

    [Display(Name = "Единица измерения")]
    public string UnitOfMeasure { get; set; } = "шт";

    [Display(Name = "Мин. запас")]
    public decimal MinimalStock { get; set; }

    [NotMapped]
    public bool IsLowStock => Quantity <= MinimalStock;

    public ICollection<ProductMaterial> ProductMaterials { get; set; } = new List<ProductMaterial>();
}

public class ProductMaterial
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int MaterialId { get; set; }
    public Material? Material { get; set; }

    [Display(Name = "Количество на единицу")]
    public decimal QuantityNeeded { get; set; }
}

public class WorkOrder
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Продукт")]
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [Display(Name = "Производственная линия")]
    public int? ProductionLineId { get; set; }
    public ProductionLine? ProductionLine { get; set; }

    [Display(Name = "Количество")]
    public int Quantity { get; set; }

    [Display(Name = "Дата начала")]
    public DateTime StartDate { get; set; }

    [Display(Name = "Планируемая дата завершения")]
    public DateTime EstimatedEndDate { get; set; }

    [Display(Name = "Статус")]
    public string Status { get; set; } = "Pending";

    [Display(Name = "Прогресс (%)")]
    public int ProgressPercent { get; set; } = 0;
}
