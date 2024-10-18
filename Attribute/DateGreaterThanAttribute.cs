namespace TradingApp.Attribute;

using System;
using System.ComponentModel.DataAnnotations;

public class DateGreaterThanAttribute : ValidationAttribute
{
    private readonly string _comparisonProperty;

    public DateGreaterThanAttribute(string comparisonProperty)
    {
        _comparisonProperty = comparisonProperty;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var currentValue = (DateTime?)value;
        var comparisonProperty = validationContext.ObjectType.GetProperty(_comparisonProperty);

        if (comparisonProperty == null)
            throw new ArgumentException($"Property with name {_comparisonProperty} not found");

        var comparisonValue = (DateTime?)comparisonProperty.GetValue(validationContext.ObjectInstance);

        if (currentValue != null && comparisonValue != null && currentValue <= comparisonValue)
        {
            return new ValidationResult($"{validationContext.DisplayName} must be greater than {_comparisonProperty}.");
        }

        return ValidationResult.Success;
    }
}
