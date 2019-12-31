using System;
using System.Collections.Generic;
using Domain.SeedWork;
using MongoDB.Bson;

namespace Domain.Aggregates.ColorPalettes
{
    public class ColorPalette: IAggregateRoot
    {
        public ObjectId UserId { get; set; }
        public List<ColorBaseValue> ColorBaseValues { get; set; }
    }

    public class ColorBaseValue
    {
        public string Key { get; set; }
        public string Color { get; set; }
    }
}
