﻿using System;
using System.Collections.Generic;

namespace TradingApp.Models;

public partial class MeasureUnit
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual Item? Item { get; set; }
}
