using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace ScraperBase
{
    [XmlRoot(ElementName = "item")]
    public class Item
    {
        [XmlElement(ElementName = "date")]
        public string Date { get; set; }
        [XmlElement(ElementName = "day")]
        public string Day { get; set; }
        [XmlElement(ElementName = "time")]
        public string Time { get; set; }
        [XmlElement(ElementName = "predictions_in_ft")]
        public string Predictions_in_ft { get; set; }
        [XmlElement(ElementName = "predictions_in_cm")]
        public string Predictions_in_cm { get; set; }
        [XmlElement(ElementName = "highlow")]
        public string Highlow { get; set; }
    }

    [XmlRoot(ElementName = "data")]
    public class Data
    {
        [XmlElement(ElementName = "item")]
        public List<Item> Item { get; set; }
    }

    [XmlRoot(ElementName = "datainfo")]
    public class Datainfo
    {
        [XmlElement(ElementName = "origin")]
        public string Origin { get; set; }
        [XmlElement(ElementName = "disclaimer")]
        public string Disclaimer { get; set; }
        [XmlElement(ElementName = "producttype")]
        public string Producttype { get; set; }
        [XmlElement(ElementName = "stationname")]
        public string Stationname { get; set; }
        [XmlElement(ElementName = "state")]
        public string State { get; set; }
        [XmlElement(ElementName = "stationid")]
        public string Stationid { get; set; }
        [XmlElement(ElementName = "stationtype")]
        public string Stationtype { get; set; }
        [XmlElement(ElementName = "BeginDate")]
        public string BeginDate { get; set; }
        [XmlElement(ElementName = "EndDate")]
        public string EndDate { get; set; }
        [XmlElement(ElementName = "dataUnits")]
        public string DataUnits { get; set; }
        [XmlElement(ElementName = "Timezone")]
        public string Timezone { get; set; }
        [XmlElement(ElementName = "Datum")]
        public string Datum { get; set; }
        [XmlElement(ElementName = "IntervalType")]
        public string IntervalType { get; set; }
        [XmlElement(ElementName = "data")]
        public Data Data { get; set; }
    }

}
