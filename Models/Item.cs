using System;
using System.Collections.Generic;

namespace TradingApp.Models;

public partial class Item : BaseEntity
{

    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Code { get; set; }
    public string? Barcode { get; set; }
    public string? QRCode { get; set; }
    public string? Color { get; set; }
    public string? Material { get; set; }
    public string? Size { get; set; }
    public decimal? Price { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public decimal? Weight { get; set; }
    public int? SaleUnit { get; set; }
    public int? ProductCategoryId { get; set; }
    public int? ProductBrandId { get; set; }
    public virtual ProductBrand? ProductBrand { get; set; }
    public virtual ProductCategory? ProductCategory { get; set; }
    public virtual MeasureUnit? SaleUnitNavigation { get; set; }
    // Store image and video paths in this field
    public string? ProductMediaAssets { get; set; }  // Comma-separated paths for images and videos
    public string? Thumbnail { get; set; }
}
