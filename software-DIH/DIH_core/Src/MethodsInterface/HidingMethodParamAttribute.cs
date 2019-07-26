using System;

namespace DIH_core.Src.MethodsInterface
{
    [AttributeUsage(AttributeTargets.Property)]
    public class HidingMethodParamAttribute : Attribute {
        public bool Required { get; set; } = false;
        public bool NotInEmbed { get; set; } = false;
        public bool NotInExtract { get; set; } = false;

        public HidingMethodParamAttribute() {}
        public enum ParamType { Set,ConstrValue,Range, IntValue,FilePath }
        public ParamType Type { get; set; }
       
        public string Description { get; set; }

        // Set
        public int[] Set { get; set; }

        //Enum value
        public Type EnumType { get; set; }
        public string[] EnumValues { get; set; }

        //Range Value
        public double MinRange { get; set; }
        public double MaxRange { get; set; }

    }
}
