using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Order2VPos.Core.VPosClient
{
    public class Receipt
    {
        public Receipt()
        {
            Plus = new List<Plu>();
            Discounts = new List<Discount>();
        }

        public int Gc { get; set; }
        public int Operator { get; set; }

        public int OperatorCode { get; set; }
        public int MediaNo { get; set; }
        public List<Plu> Plus { get; set; }
        public List<Discount> Discounts { get; set; }
        public string GcText { get; set; }

        public string ToJson()
        {
            StringWriter output = new StringWriter();
            using (JsonTextWriter writer = new JsonTextWriter(output))
            {
                writer.WriteStartObject();

                writer.WritePropertyName("Gc");
                writer.WriteValue(Gc);
                writer.WritePropertyName("GcText");
                writer.WriteValue(GcText);
                writer.WritePropertyName("Operator");
                writer.WriteValue(Operator);
                writer.WritePropertyName("OperatorCode");
                writer.WriteValue(OperatorCode);
                writer.WritePropertyName("FinanceWay");
                writer.WriteValue(MediaNo);

                writer.WritePropertyName("Discounts");
                writer.WriteStartArray();
                foreach (var discount in Discounts)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("Number");
                    writer.WriteValue(discount.Number);
                    writer.WritePropertyName("Value");
                    writer.WriteValue(discount.Value);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();

                writer.WritePropertyName("Plus");
                writePluArray(writer, Plus);

                writer.WriteEndObject();
                writer.Flush();

                return output.ToString();
            }

            void writePluArray(JsonTextWriter writer, IEnumerable<Plu> plus)
            {
                writer.WriteStartArray();
                foreach (var plu in plus)
                {
                    writer.WriteStartObject();

                    if (plu.AdditionalPlus != null)
                    {
                        writer.WritePropertyName("AdditionalPlus");
                        writePluArray(writer, plu.AdditionalPlus);
                    }

                    writer.WritePropertyName("Number");
                    writer.WriteValue(plu.Number);
                    writer.WritePropertyName("Quantity");
                    writer.WriteValue(plu.Quantity);
                    writer.WritePropertyName("ModifyPriceAbsoluteValue");
                    writer.WriteValue(plu.ModifyPriceAbsoluteValue);
                    writer.WritePropertyName("ModifyPriceAbsoluteFactor");
                    writer.WriteValue(plu.ModifyPriceAbsoluteFactor);
                    writer.WritePropertyName("ModifyPricePercentValue");
                    writer.WriteValue(plu.ModifyPricePercentValue);
                    writer.WritePropertyName("ModifyPricePercentFactor");
                    writer.WriteValue(plu.ModifyPricePercentFactor);
                    writer.WritePropertyName("OverridePriceValue");
                    writer.WriteValue(plu.OverridePriceValue);
                    writer.WritePropertyName("OverridePriceFactor");
                    writer.WriteValue(plu.OverridePriceFactor);

                    if (plu.Modifier != null)
                    {
                        writer.WritePropertyName("Modifier");
                        writer.WriteStartArray();
                        foreach (var modifier in plu.Modifier)
                        {
                            writer.WriteValue(modifier);
                        }
                        writer.WriteEndArray();
                    }

                    writer.WriteEndObject();
                }
                writer.WriteEndArray();

            }
        }
    }
}
