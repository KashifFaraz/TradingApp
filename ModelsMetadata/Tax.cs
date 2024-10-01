using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TradingApp.Models;


[ModelMetadataType(typeof(TaxMetadata))]
public partial class Tax
{

}
public class TaxMetadata
{
    [Display(Name = "Computation Type")]
    public byte ComputationType { get; set; }
    [DisplayFormat(DataFormatString = "{0:N}")]
    public decimal Value { get; set; }
    [Display(Name = "Created By")]
    public int? CreatedBy { get; set; }
    [Display(Name = "Created On")]
    public DateTime? CreatedOn { get; set; }
    [Display(Name = "Edited By")]
    public int? EditedBy { get; set; }
    [Display(Name = "Edited On")]
    public DateTime? EditedOn { get; set; }
}